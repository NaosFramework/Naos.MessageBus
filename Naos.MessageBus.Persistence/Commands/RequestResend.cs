// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestResend.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    using static System.FormattableString;

    /// <summary>
    /// Send command for a <see cref="Shipment"/>.
    /// </summary>
    public class RequestResend : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator => new ValidationPlan<Shipment>
                                                                   {
                                                                       ValidationRules.IsAbortedOrRejectedOrDelivered(this.TrackingCode)
                                                                   };

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var trackingCodeSet = Validate.That<Send>(cmd => cmd.TrackingCode != null).WithErrorMessage(Invariant($"{nameof(this.TrackingCode)} must be specified."));

                return new ValidationPlan<Send> { trackingCodeSet };
            }
        }

        /// <summary>
        /// Gets or sets the tracking code of the shipment.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }
    }
}