// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertifiedNoticeForDatabase.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Model class to hold info in the read model persistence.
    /// </summary>
    public class CertifiedNoticeForDatabase
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public string GroupKey { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC that the notice was delivered.
        /// </summary>
        public DateTime DeliveredDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the envelope.
        /// </summary>
        public virtual Envelope Envelope { get; set; }
    }
}