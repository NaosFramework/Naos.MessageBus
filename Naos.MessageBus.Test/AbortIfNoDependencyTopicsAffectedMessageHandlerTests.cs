﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoDependencyTopicsAffectedMessageHandlerTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FakeItEasy;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.AutoFakeItEasy;

    using Xunit;

    public class AbortIfNoDependencyTopicsAffectedMessageHandlerTests
    {
        [Fact]
        public void MissingCurrentNotice_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name) })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        Status = TopicStatus.WasAffected,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc =
                                                            DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    }
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void CurrentNoticeUnknown_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.Unknown })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        Status = TopicStatus.WasAffected,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    }
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void CurrentNoticeDateLessThanPreviousNoticeDateButAlwaysCheckStrategy_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                        })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    }
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.None,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();

            // act
            handler.HandleAsync(message).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void CurrentNoticeDateLessThanPreviousNoticeDate_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                        })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        Status = TopicStatus.WasAffected,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    }
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void MissingPreviousNotice_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                            {
                                Topic = new AffectedTopic(_.Name),
                                Status = TopicStatus.WasAffected,
                                AffectsCompletedDateTimeUtc = DateTime.UtcNow
                            })
                    .Union(new[] { new TopicStatusReport { Topic = impactingTopic, Status = TopicStatus.WasAffected } })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();

            // act
            handler.HandleAsync(message).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void NoNewWithAnyCheck_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var topicStatusReports =
                topics.Cast<ITopic>().Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected }).ToArray();

            var reports =
                topicStatusReports.Union(
                    new[]
                        {
                            new TopicStatusReport
                                {
                                    Topic = impactingTopic,
                                    Status = TopicStatus.WasAffected,
                                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                    DependencyTopicNoticesAtStart = topicStatusReports
                                }
                        }).ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void SomeNewWithAnyCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var notCertified = new AffectedTopic("other");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow
                        })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        Status = TopicStatus.WasAffected,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc =
                                                            DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    },
                                new TopicStatusReport { Topic = notCertified, Status = TopicStatus.BeingAffected }
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();

            // act
            handler.HandleAsync(message).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public async Task AllNewWithAllCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow
                        })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc =
                                                            DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    }
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();

            // act
            await handler.HandleAsync(message);

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void SomeNewWithAllCheck_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var notCertified = new AffectedTopic("other");
            var topics = Some.Dummies<DependencyTopic>(50).ToList();

            var number = 1;
            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = number++ % 2 == 0 ? TopicStatus.BeingAffected : TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow
                        })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        Status = TopicStatus.WasAffected,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc =
                                                            DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    },
                                new TopicStatusReport { Topic = notCertified, Status = TopicStatus.BeingAffected }
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }
    }
}
