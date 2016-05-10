﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestItsConfigMessageBusHandlerSettings.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using Its.Configuration;

    using Naos.MessageBus.Domain;

    using Xunit;

    public class TestItsConfigMessageBusHandlerSettings
    {
        [Fact]
        public static void ItsConfigGetSettings_MessageBusHarnessSettingsHost_ComeOutCorrectly()
        {
            var settings = SetupItsConfigAndGetSettingsByPrecedence("Host");

            Assert.NotNull(settings);
            Assert.Equal("server=localhost1", settings.ConnectionConfiguration.CourierConnectionString);
            Assert.Equal("server=localhost2", settings.ConnectionConfiguration.PostmasterEventsConnectionString);
            Assert.Equal("server=localhost3", settings.ConnectionConfiguration.PostmasterReadModelConnectionString);
            var hostSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();
            Assert.NotNull(hostSettings);
            Assert.Equal(true, hostSettings.RunDashboard);
        }

        [Fact]
        public static void ItsConfigGetSettings_MessageBusHarnessSettingsExecutor_ComeOutCorrectly()
        {
            var settings = SetupItsConfigAndGetSettingsByPrecedence("Executor");

            Assert.NotNull(settings);
            Assert.Equal("server=localhost1", settings.ConnectionConfiguration.CourierConnectionString);
            Assert.Equal("server=localhost2", settings.ConnectionConfiguration.PostmasterEventsConnectionString);
            Assert.Equal("server=localhost3", settings.ConnectionConfiguration.PostmasterReadModelConnectionString);
            var hostSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();
            Assert.Null(hostSettings);
            var executorSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>().SingleOrDefault();
            Assert.NotNull(executorSettings);
            Assert.Equal("monkeys", executorSettings.ChannelsToMonitor.First().Name);
            Assert.Equal("pandas", executorSettings.ChannelsToMonitor.Skip(1).First().Name);
            Assert.Equal(4, executorSettings.WorkerCount);
            Assert.Equal("I:\\Gets\\My\\Dlls\\Here", executorSettings.HandlerAssemblyPath);
            Assert.Equal(TimeSpan.FromMinutes(1), executorSettings.PollingTimeSpan);
            Assert.Equal(TimeSpan.FromSeconds(1), executorSettings.MessageDispatcherWaitThreadSleepTime);
            Assert.Equal(TimeSpan.FromMinutes(10), executorSettings.HarnessProcessTimeToLive);
        }

        private static MessageBusHarnessSettings SetupItsConfigAndGetSettingsByPrecedence(string precedence)
        {
            Settings.Reset();
            Settings.SettingsDirectory = Settings.SettingsDirectory.Replace("\\bin\\Debug", string.Empty);
            Settings.Precedence = new[] { precedence };
            Settings.Deserialize = Serializer.Deserialize;
            return Settings.Get<MessageBusHarnessSettings>();
        }
    }
}
