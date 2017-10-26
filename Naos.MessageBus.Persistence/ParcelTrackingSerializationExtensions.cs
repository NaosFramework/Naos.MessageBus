﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingSerializationExtensions.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Naos.MessageBus.Domain;
    using Naos.Serialization.Domain;
    using Naos.Serialization.Domain.Extensions;
    using Naos.Serialization.Factory;
    using Naos.Serialization.Factory.Extensions;

    using OBeautifulCode.TypeRepresentation;

    /// <summary>
    /// Serialization extension methods for serializing items for transport through ParcelTracking.
    /// </summary>
    public static class ParcelTrackingSerializationExtensions
    {
        private static readonly SerializationDescription ParcelTrackingSerializationDescription = new SerializationDescription(SerializationFormat.Json, SerializationRepresentation.String);

        /// <summary>
        /// Deserializes the message in an envelope.
        /// </summary>
        /// <typeparam name="T">Type to deserialize into.</typeparam>
        /// <param name="envelope">An <see cref="Envelope" />.</param>
        /// <returns>Deserialized object.</returns>
        public static T DeserializeMessage<T>(this Envelope envelope)
        {
            return envelope.SerializedMessage.DeserializePayload<T>(TypeMatchStrategy.NamespaceAndName, MultipleMatchStrategy.NewestVersion);
        }

        /// <summary>
        /// Deserializes a string used to pass information through ParcelTracking.
        /// </summary>
        /// <typeparam name="T">Type to deserialize into.</typeparam>
        /// <param name="serializedString">String to deserialize.</param>
        /// <returns>Deserialized object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string", Justification = "Spelling/name is correct.")]
        public static T FromParcelTrackingSerializedString<T>(this string serializedString)
        {
            var serializer = SerializerFactory.Instance.BuildSerializer(ParcelTrackingSerializationDescription);
            return serializer.Deserialize<T>(serializedString);
        }

        /// <summary>
        /// Serializes an object in order to pass through ParcelTracking.
        /// </summary>
        /// <param name="objectToSerialize">Object to serialize.</param>
        /// <returns>Serialized string representation of object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object", Justification = "Spelling/name is correct.")]
        public static string ToParcelTrackingSerializedString(this object objectToSerialize)
        {
            var serializer = SerializerFactory.Instance.BuildSerializer(ParcelTrackingSerializationDescription);
            return serializer.SerializeToString(objectToSerialize);
        }
    }
}
