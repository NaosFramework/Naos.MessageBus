﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingSystemTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Persistence;

    using OBeautifulCode.TypeRepresentation;

    using Xunit;

    public class TrackingSystemTests
    {
        [Fact(Skip = "This is for testing against a database.")]
        public async Task Do()
        {
            var messages = new List<LogEntry>();
            Log.EntryPosted += (sender, args) => messages.Add(args.LogEntry);

            var eventConnectionConfiguration = new EventPersistenceConnectionConfiguration
                {
                    Server = "(local)\\SQLExpress",
                    Database = "ParcelTrackingEvents",
                };

            var readModelConnectionConfiguration = new ReadModelPersistenceConnectionConfiguration
                {
                    Server = "(local)\\SQLExpress",
                    Database = "ParcelTrackingReadModel",
                };

            var trackingSends = new List<Crate>();
            var courier = Factory.GetInMemoryCourier(trackingSends);
            var parcelTrackingSystem = new ParcelTrackingSystem(courier(), eventConnectionConfiguration, readModelConnectionConfiguration);
            var postOffice = new PostOffice(parcelTrackingSystem, new ChannelRouter(new NullChannel()));

            var topic = new AffectedTopic(Guid.NewGuid().ToString().ToUpperInvariant());

            var tracking = postOffice.Send(
                new NullMessage(),
                new SimpleChannel("channel"),
                "name",
                topic,
                null,
                TopicCheckStrategy.Any,
                SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning);

            Parcel parcel = null;

            var timeout = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));
            while (parcel == null)
            {
                if (DateTime.UtcNow > timeout)
                {
                    throw new ApplicationException("Events never propaggated.");
                }

                parcel = trackingSends.SingleOrDefault()?.Parcel;
            }

            var expectedTopicStatus = TopicStatus.Unknown;
            var seenRejection = false; // after this the parcel will always be in rejected state until aborted or delivered

            foreach (var envelope in parcel.Envelopes)
            {
                var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = envelope.Id };
                if (envelope.Id != parcel.Envelopes.First().Id)
                {
                    // should already be sent by original send...
                    await parcelTrackingSystem.UpdateSentAsync(trackingCode, parcel, envelope.Address, null);
                }

                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(seenRejection ? ParcelStatus.Rejected : ParcelStatus.Unknown);
                await ConfirmNoticeState(parcelTrackingSystem, topic, expectedTopicStatus);

                await parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(seenRejection ? ParcelStatus.Rejected : ParcelStatus.Unknown);
                await ConfirmNoticeState(parcelTrackingSystem, topic, expectedTopicStatus);

                await parcelTrackingSystem.UpdateAbortedAsync(trackingCode, "Try another day");
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Aborted);
                await ConfirmNoticeState(parcelTrackingSystem, topic, expectedTopicStatus);

                await parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Aborted);
                await ConfirmNoticeState(parcelTrackingSystem, topic, expectedTopicStatus);

                await parcelTrackingSystem.UpdateRejectedAsync(trackingCode, new NotImplementedException("Not here yet"));
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);

                // if we've already written a being affected notice then it will be marked as failed...
                expectedTopicStatus = expectedTopicStatus == TopicStatus.BeingAffected ? TopicStatus.Failed : expectedTopicStatus;
                await ConfirmNoticeState(parcelTrackingSystem, topic, expectedTopicStatus);
                seenRejection = true;

                await parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);
                await ConfirmNoticeState(parcelTrackingSystem, topic, expectedTopicStatus);

                await parcelTrackingSystem.UpdateDeliveredAsync(trackingCode, envelope);
                if (envelope.MessageType == typeof(TopicBeingAffectedMessage).ToTypeDescription())
                {
                    expectedTopicStatus = TopicStatus.BeingAffected;
                }

                // should be last message and will assert differently
                if (envelope.MessageType != typeof(TopicWasAffectedMessage).ToTypeDescription())
                {
                    (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);
                    await ConfirmNoticeState(parcelTrackingSystem, topic, expectedTopicStatus);
                }
            }

            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { new TrackingCode { ParcelId = parcel.Id } })).Single()
                .Status.Should()
                .Be(ParcelStatus.Delivered);

            (await parcelTrackingSystem.GetLatestTopicStatusReportAsync(topic, TopicStatus.BeingAffected)).Should().BeNull();
            (await parcelTrackingSystem.GetLatestTopicStatusReportAsync(topic, TopicStatus.WasAffected)).Should().NotBeNull();
            (await parcelTrackingSystem.GetLatestTopicStatusReportAsync(topic)).Status.Should().Be(TopicStatus.WasAffected);

            messages.Count.Should().Be(0);
        }

        private static async Task ConfirmNoticeState(
            ParcelTrackingSystem parcelTrackingSystem,
            AffectedTopic affectedTopic,
            TopicStatus expectedTopicStatus)
        {
            (await parcelTrackingSystem.GetLatestTopicStatusReportAsync(affectedTopic)).Status.Should().Be(expectedTopicStatus);

            var queryFilterResultIsNullMap = new Dictionary<TopicStatus, bool>();

            switch (expectedTopicStatus)
            {
                case TopicStatus.Unknown:
                    queryFilterResultIsNullMap.Add(TopicStatus.BeingAffected, true);
                    queryFilterResultIsNullMap.Add(TopicStatus.WasAffected, true);
                    queryFilterResultIsNullMap.Add(TopicStatus.Failed, true);
                    break;
                case TopicStatus.BeingAffected:
                    queryFilterResultIsNullMap.Add(TopicStatus.BeingAffected, false);
                    queryFilterResultIsNullMap.Add(TopicStatus.WasAffected, true);
                    queryFilterResultIsNullMap.Add(TopicStatus.Failed, true);
                    break;
                case TopicStatus.WasAffected:
                    queryFilterResultIsNullMap.Add(TopicStatus.BeingAffected, true);
                    queryFilterResultIsNullMap.Add(TopicStatus.WasAffected, false);
                    queryFilterResultIsNullMap.Add(TopicStatus.Failed, true);
                    break;
                case TopicStatus.Failed:
                    queryFilterResultIsNullMap.Add(TopicStatus.BeingAffected, true);
                    queryFilterResultIsNullMap.Add(TopicStatus.WasAffected, true);
                    queryFilterResultIsNullMap.Add(TopicStatus.Failed, false);
                    break;
                default:
                    throw new NotSupportedException("Unsupported expectedTopicStatus: " + expectedTopicStatus);
            }

            foreach (var queryFilterResultIsNull in queryFilterResultIsNullMap)
            {
                if (queryFilterResultIsNull.Value)
                {
                    (await parcelTrackingSystem.GetLatestTopicStatusReportAsync(affectedTopic, queryFilterResultIsNull.Key)).Should().BeNull();
                }
                else
                {
                    (await parcelTrackingSystem.GetLatestTopicStatusReportAsync(affectedTopic, queryFilterResultIsNull.Key)).Should().NotBeNull();
                }
            }
        }
    }
}
