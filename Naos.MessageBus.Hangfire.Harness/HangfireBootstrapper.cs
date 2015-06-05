﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireBootstrapper.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Hosting;

    using global::Hangfire;
    using global::Hangfire.Server;

    using Its.Configuration;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;

    using SimpleInjector;

    /// <inheritdoc />
    public class HangfireBootstrapper : IRegisteredObject
    {
        /// <summary>
        /// Instance variable of the singleton
        /// </summary>
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        private readonly object lockObject = new object();
        private bool started;

        private BackgroundJobServer backgroundJobServer;

        private Container simpleInjectorContainer;

        private HangfireBootstrapper()
        {
            /* to prevent instantiation (otherwise a default public will be created) */
        }

        /// <summary>
        /// Perform a start.
        /// </summary>
        /// <param name="persistenceConnectionString">Connection string to hangfire persistence.</param>
        /// <param name="executorRoleSettings">Executor role settings.</param>
        public void Start(
            string persistenceConnectionString,
            MessageBusHarnessRoleSettingsExecutor executorRoleSettings)
        {
            lock (this.lockObject)
            {
                if (this.started)
                {
                    return;
                }

                this.started = true;

                HostingEnvironment.RegisterObject(this);

                this.simpleInjectorContainer = new Container();

                var files = Directory.GetFiles(
                    executorRoleSettings.HandlerAssemblyPath,
                    "*.dll",
                    SearchOption.AllDirectories);

                // add an unknown assembly resolver to go try to find the dll in one of the files we have discovered...
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        var dllName = args.Name.Split(',')[0] + ".dll";
                        var fullDllPath = files.SingleOrDefault(_ => _.EndsWith(dllName));
                        if (fullDllPath == null)
                        {
                            throw new TypeInitializationException(args.Name, null);
                        }

                        return Assembly.LoadFile(fullDllPath);
                    };

                var handlerTypeMap = new List<TypeMap>();
                foreach (var filePathToPotentialHandlerAssembly in files)
                {
                    var assembly = Assembly.LoadFile(filePathToPotentialHandlerAssembly);
                    var typesInFile = assembly.GetTypes();
                    var mapsInFile = typesInFile.GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));
                    handlerTypeMap.AddRange(mapsInFile);
                }

                foreach (var handlerTypeMapEntry in handlerTypeMap)
                {
                    this.simpleInjectorContainer.Register(
                        handlerTypeMapEntry.InterfaceType,
                        handlerTypeMapEntry.ConcreteType);
                }

                this.simpleInjectorContainer.Register<IDispatchMessages>(
                    () => new MessageDispatcher(this.simpleInjectorContainer));
                GlobalConfiguration.Configuration.UseActivator(
                    new SimpleInjectorJobActivator(this.simpleInjectorContainer));

                var options = new BackgroundJobServerOptions
                                  {
                                      Queues = executorRoleSettings.ChannelsToMonitor.ToArray(),
                                      ServerName = Environment.MachineName,
                                      SchedulePollingInterval =
                                          executorRoleSettings.PollingTimeSpan,
                                      WorkerCount = executorRoleSettings.WorkerCount,
                                  };

                GlobalConfiguration.Configuration.UseSqlServerStorage(persistenceConnectionString);
                this.backgroundJobServer = new BackgroundJobServer(options);
            }
        }

        /// <summary>
        /// Perform a stop.
        /// </summary>
        public void Stop()
        {
            lock (this.lockObject)
            {
                // this will have been set befor the object is registered or the background server is initialized.
                if (this.started)
                {
                    if (this.backgroundJobServer != null)
                    {
                        this.backgroundJobServer.Dispose();
                    }

                    HostingEnvironment.UnregisterObject(this);
                }
            }
        }

        /// <inheritdoc />
        void IRegisteredObject.Stop(bool immediate)
        {
            this.Stop();
        }
    }
}