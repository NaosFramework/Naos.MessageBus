// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerFactoryConfiguration.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;

    /// <summary>
    /// Configuration for buildering a <see cref="IHandlerFactory" />.
    /// </summary>
    public class HandlerFactoryConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerFactoryConfiguration"/> class.
        /// </summary>
        /// <param name="typeMatchStrategyForMessageResolution">Strategy to match message types on for finding a handler.</param>
        /// <param name="handlerAssemblyPath">Optional directory path to load assemblies from; DEFAULT is null and will only use currently loaded assemblies.</param>
        public HandlerFactoryConfiguration(TypeMatchStrategy typeMatchStrategyForMessageResolution, string handlerAssemblyPath = null)
        {
            this.TypeMatchStrategyForMessageResolution = typeMatchStrategyForMessageResolution;
            this.HandlerAssemblyPath = handlerAssemblyPath;
        }

        /// <summary>
        /// Gets the strategy to match message types on for finding a handler.
        /// </summary>
        public TypeMatchStrategy TypeMatchStrategyForMessageResolution { get; private set; }

        /// <summary>
        /// Gets the optional directory path to load assemblies from; DEFAULT is null and will only use currently loaded assemblies.
        /// </summary>
        public string HandlerAssemblyPath { get; private set; }
    }
}
