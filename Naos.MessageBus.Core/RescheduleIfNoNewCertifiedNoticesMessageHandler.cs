﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RescheduleIfNoNewCertifiedNoticesMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class RescheduleIfNoNewCertifiedNoticesMessageHandler : IHandleMessages<RescheduleIfNoNewCertifiedNoticesMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(RescheduleIfNoNewCertifiedNoticesMessage message)
        {
            var tracker = HandlerToolShed.GetParcelTracker();

            await this.HandleAsync(message, tracker);
        }

        /// <summary>
        /// Handle <see cref="RescheduleIfNoNewCertifiedNoticesMessage"/> message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="tracker">Tracker to get certified notices.</param>
        /// <returns>Task for async.</returns>
        public async Task HandleAsync(RescheduleIfNoNewCertifiedNoticesMessage message, IGetTrackingReports tracker)
        {
            var topicRecentResults = message.TopicChecks.ToDictionary(
                key => key.Topic,
                val =>
                    {
                        var latest = tracker.GetLatestCertifiedNotice(val.Topic);
                        var isRecent = DateTime.UtcNow.Subtract(latest.DeliveredDateUtc) <= val.RecentnessThreshold;
                        return isRecent;
                    });

            bool dataIsRecent;

            switch (message.CheckStrategy)
            {
                case TopicCheckStrategy.All:
                    dataIsRecent = topicRecentResults.Values.All(_ => _);
                    break;
                case TopicCheckStrategy.Any:
                    dataIsRecent = topicRecentResults.Values.Any(_ => _);
                    break;
                default:
                    throw new NotSupportedException("Not supported TopicCheckStrategy: " + message.CheckStrategy);
            }

            if (!dataIsRecent)
            {
                Thread.Sleep(message.WaitTimeBeforeRescheduling);

                throw new AbortAndRescheduleParcelException("Planned reschedule due to missing notice.");
            }

            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}