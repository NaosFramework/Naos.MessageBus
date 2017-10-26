﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStuffAndOpenEnvelopes.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using Naos.Compression.Domain;
    using Naos.Serialization.Domain;
    using Naos.Serialization.Domain.Extensions;

    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;

    /// <summary>
    /// Interface for stuffing and opening envelopes.
    /// </summary>
    public interface IStuffAndOpenEnvelopes
    {
        /// <summary>
        /// Open an envelope.
        /// </summary>
        /// <param name="envelope">Envelope to open.</param>
        /// <returns>Message within the envelope.</returns>
        IMessage OpenEnvelope(Envelope envelope);

        /// <summary>
        /// Open an envelope with a specific message type.
        /// </summary>
        /// <typeparam name="T">Type of message.</typeparam>
        /// <param name="envelope">Envelope to open.</param>
        /// <returns>Message within the envelope as specified type.</returns>
        T OpenEnvelope<T>(Envelope envelope)
            where T : IMessage;

        /// <summary>
        /// Pack an envelope from an <see cref="AddressedMessage" />.
        /// </summary>
        /// <param name="addressedMessage">Addressed message to pack.</param>
        /// <param name="id">Optional specific ID; DEFAULT is a new <see cref="Guid" />.</param>
        /// <returns>Packed <see cref="Envelope" />.</returns>
        Envelope StuffEnvelope(AddressedMessage addressedMessage, string id = null);
    }

    /// <summary>
    /// Implementation of <see cref="IStuffAndOpenEnvelopes" />.
    /// </summary>
    public class EnvelopeMachine : IStuffAndOpenEnvelopes
    {
        private readonly SerializationDescription messageSerializationDescription;

        private readonly ISerializerFactory serializerFactory;

        private readonly ICompressorFactory compressorFactory;

        private readonly TypeMatchStrategy typeMatchStrategyForMessageResolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvelopeMachine"/> class.
        /// </summary>
        /// <param name="messageSerializationDescription">Serialization description to use.</param>
        /// <param name="serializerFactory">Serializer factory to use.</param>
        /// <param name="compressorFactory">Compressor factory to use.</param>
        /// <param name="typeMatchStrategyForMessageResolution">Type match strategy to use.</param>
        public EnvelopeMachine(SerializationDescription messageSerializationDescription, ISerializerFactory serializerFactory, ICompressorFactory compressorFactory, TypeMatchStrategy typeMatchStrategyForMessageResolution)
        {
            new { messageSerializationDescription, serializerFactory, compressorFactory }.Must().NotBeNull().OrThrowFirstFailure();

            this.messageSerializationDescription = messageSerializationDescription;
            this.serializerFactory = serializerFactory;
            this.compressorFactory = compressorFactory;
            this.typeMatchStrategyForMessageResolution = typeMatchStrategyForMessageResolution;
        }

        /// <inheritdoc />
        public IMessage OpenEnvelope(Envelope envelope)
        {
            return this.OpenEnvelope<IMessage>(envelope);
        }

        /// <inheritdoc />
        public T OpenEnvelope<T>(Envelope envelope)
            where T : IMessage
        {
            new { envelope }.Must().NotBeNull().OrThrowFirstFailure();

            var ret = envelope.SerializedMessage.DeserializePayloadUsingSpecificFactory<T>(
                this.serializerFactory,
                this.compressorFactory,
                this.typeMatchStrategyForMessageResolution,
                MultipleMatchStrategy.NewestVersion);

            return ret;
        }

        /// <inheritdoc />
        public Envelope StuffEnvelope(AddressedMessage addressedMessage, string id = null)
        {
            new { addressedMessage }.Must().NotBeNull().OrThrowFirstFailure();

            var localId = id ?? Guid.NewGuid().ToString().ToUpperInvariant();
            var serializedMessage = addressedMessage.Message.ToDescribedSerializationUsingSpecificFactory(
                this.messageSerializationDescription,
                this.serializerFactory,
                this.compressorFactory,
                this.typeMatchStrategyForMessageResolution,
                MultipleMatchStrategy.NewestVersion);

            var ret = new Envelope(localId, addressedMessage.Message.Description, addressedMessage.Address, serializedMessage);

            return ret;
        }
    }
}