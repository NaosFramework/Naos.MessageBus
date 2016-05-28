﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryRejected.cs" company="Naos">
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
        /// A shipment has been rejected.
        /// </summary>
        public class EnvelopeDeliveryRejected : Event<Shipment>, IUsePayload<PayloadEnvelopeDeliveryRejected>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.ExtractPayload().TrackingCode].Exception = this.ExtractPayload().Exception;
                aggregate.Tracking[this.ExtractPayload().TrackingCode].Status = this.ExtractPayload().NewStatus;
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.EnvelopeDeliveryRejected"/>.
    /// </summary>
    public class PayloadEnvelopeDeliveryRejected : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDeliveryRejected"/> class.
        /// </summary>
        public PayloadEnvelopeDeliveryRejected()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDeliveryRejected"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the envelope that was rejected.</param>
        /// <param name="newStatus">New status of the envelope.</param>
        /// <param name="exception">Exception of the delivery.</param>
        public PayloadEnvelopeDeliveryRejected(TrackingCode trackingCode, ParcelStatus newStatus, Exception exception)
        {
            this.TrackingCode = trackingCode;
            this.Exception = exception;
            this.NewStatus = newStatus;
        }

        /// <summary>
        /// Gets or sets the tracking code of the envelope that was rejected.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; set; }

        /// <summary>
        /// Gets or sets the exception of the delivery.
        /// </summary>
        public Exception Exception { get; set; }
    }
}