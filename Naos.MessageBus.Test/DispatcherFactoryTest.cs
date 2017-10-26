﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactoryTest.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using FluentAssertions;

    using Naos.MessageBus.Core;

    using OBeautifulCode.TypeRepresentation;

    using Xunit;

    public static class DispatcherFactoryTest
    {
        [Fact(Skip = "Used for debugging specific assembly sets that have issues with reflection loading.")]
        public static void IsolateReflectionIssue()
        {
            // arrange
            var directory = @"D:\Temp\FailedToReflect";

            // act
            var handlerBuilder = new ReflectionHandlerFactory(directory, TypeMatchStrategy.NamespaceAndName);

            // assert
            handlerBuilder.Should().NotBeNull();
        }
    }
}
