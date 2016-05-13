﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParcelTrackingSystem.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for tracking parcels in the bus.
    /// </summary>
    public interface IParcelTrackingSystem : IGetTrackingReports
    {
        /// <summary>
        /// Begins tracking a parcel.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="parcel">Parcel that was sent.</param>
        /// <param name="metadata">Metadata about the sending or the parcel.</param>
        void Sent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata);

        /// <summary>
        /// Parcel was addressed.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="assignedChannel">Channel the parcel is being sent to.</param>
        void Addressed(TrackingCode trackingCode, Channel assignedChannel);

        /// <summary>
        /// Delivery is attempted on a handler, handler details provided.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="harnessDetails">Details about the harness it is being delivered to.</param>
        void Attempting(TrackingCode trackingCode, HarnessDetails harnessDetails);

        /// <summary>
        /// Delivery was rejected by the harness.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="exception">Exception that occurred.</param>
        void Rejected(TrackingCode trackingCode, Exception exception);

        /// <summary>
        /// Delivery was accepted by the harness.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        void Delivered(TrackingCode trackingCode);
    }

    /// <summary>
    /// Null implementation of <see cref="IParcelTrackingSystem"/>.
    /// </summary>
    public class NullParcelTrackingSystem : IParcelTrackingSystem
    {
        /// <inheritdoc />
        public void Attempting(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public void Delivered(TrackingCode trackingCode)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public void Sent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public void Addressed(TrackingCode trackingCode, Channel assignedChannel)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public void Rejected(TrackingCode trackingCode, Exception exception)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ParcelTrackingReport> GetTrackingReport(IReadOnlyCollection<TrackingCode> trackingCodes)
        {
            return new List<ParcelTrackingReport>();
        }

        /// <inheritdoc />
        public CertifiedNotice GetLatestCertifiedNotice(string topic)
        {
            return null;
        }
    }
}