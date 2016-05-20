﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.CertifiedNoticeDelivered.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        /// <summary>
        /// Certified envelope was delivered.
        /// </summary>
        public class CertifiedNoticeDelivered : Event<Shipment>
        {
            /// <summary>
            /// Gets or sets the tracking code of the envelope.
            /// </summary>
            public TrackingCode TrackingCode { get; set; }

            /// <summary>
            /// Gets or sets the topic of the certified notice.
            /// </summary>
            public ImpactingTopic Topic { get; set; }

            /// <summary>
            /// Gets or sets the envelope that was certified.
            /// </summary>
            public Envelope Envelope { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                /* no-op */
            }
        }
    }
}