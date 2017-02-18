﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactoryJobActivator.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;

    using global::Hangfire;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.MessageBus.Hangfire.Sender;

    using OBeautifulCode.TypeRepresentation;

    /// <summary>
    /// Hangfire job activator that will lookup the correct implementation of the Hangfire job via SimpleInjector DI container.
    /// </summary>
    public class DispatcherFactoryJobActivator : JobActivator
    {
        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether handlers are matched in strict mode...
        private readonly TypeComparer typeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly DispatcherFactory dispatcherFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherFactoryJobActivator"/> class.
        /// </summary>
        /// <param name="dispatcherFactory">Dispatcher manager to .</param>
        public DispatcherFactoryJobActivator(DispatcherFactory dispatcherFactory)
        {
            if (dispatcherFactory == null)
            {
                throw new ArgumentNullException("dispatcherFactory");
            }

            this.dispatcherFactory = dispatcherFactory;
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            if (this.typeComparer.Equals(jobType, typeof(HangfireDispatcher)))
            {
                var realDispatcher = this.dispatcherFactory.Create();
                return new HangfireDispatcher(realDispatcher);
            }

            throw new DispatchException(
                "Attempted to load type other than IDispatchMessages, type: " + jobType.FullName);
        }
    }
}