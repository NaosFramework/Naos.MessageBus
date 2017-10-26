﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicWasAffectedMessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class TopicWasAffectedMessageHandler : MessageHandlerBase<TopicWasAffectedMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(TopicWasAffectedMessage message)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}