﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.TopicWasAffected.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        /// <summary>
        /// Topic was affected.
        /// </summary>
        public class TopicWasAffected : Event<Shipment>, IUsePayload<PayloadTopicWasAffected>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <summary>
            /// Gets or sets the de-normalized parcel id.
            /// </summary>
            public Guid ParcelId { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                /* no-op */
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.TopicWasAffected"/>.
    /// </summary>
    public class PayloadTopicWasAffected : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadTopicWasAffected"/> class.
        /// </summary>
        public PayloadTopicWasAffected()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadTopicWasAffected"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the envelope.</param>
        /// <param name="topic">Topic being that was affected.</param>
        /// <param name="envelope">Envelope that finished the topic being affected.</param>
        public PayloadTopicWasAffected(TrackingCode trackingCode, AffectedTopic topic, Envelope envelope)
        {
            this.TrackingCode = trackingCode;
            this.Topic = topic;
            this.Envelope = envelope;
        }

        /// <summary>
        /// Gets or sets the tracking code of the envelope.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the topic being that was affected.
        /// </summary>
        public AffectedTopic Topic { get; set; }

        /// <summary>
        /// Gets or sets the envelope that finished the topic being affected.
        /// </summary>
        public Envelope Envelope { get; set; }
    }
}