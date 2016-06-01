﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactory.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Its.Log.Instrumentation;

    using Naos.Diagnostics.Domain;
    using Naos.MessageBus.Domain;

    using SimpleInjector;

    /// <summary>
    /// Class to manage creating a functional instance of IDispatchMessages.
    /// </summary>
    public class DispatcherFactory : IDispatcherFactory
    {
        // this is declared here to persist, it's filled exclusively in the MessageDispatcher...
        private readonly ConcurrentDictionary<Type, object> sharedStateMap = new ConcurrentDictionary<Type, object>();

        private readonly ICollection<IChannel> servicedChannels;

        private readonly Container simpleInjectorContainer = new Container();

        private readonly TypeMatchStrategy typeMatchStrategy;

        private readonly TimeSpan messageDispatcherWaitThreadSleepTime;

        private readonly HarnessStaticDetails harnessStaticDetails;

        private readonly IParcelTrackingSystem parcelTrackingSystem;

        private readonly ITrackActiveMessages activeMessageTracker;

        private readonly IPostOffice postOffice;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherFactory"/> class.
        /// </summary>
        /// <param name="handlerAssemblyPath">Path to the assemblies being searched through to be loaded as message handlers.</param>
        /// <param name="servicedChannels">Channels being monitored.</param>
        /// <param name="typeMatchStrategy">Strategy on how to match types.</param>
        /// <param name="messageDispatcherWaitThreadSleepTime">Amount of time to sleep while waiting on messages to be handled.</param>
        /// <param name="parcelTrackingSystem">Interface for managing life of the parcels.</param>
        /// <param name="activeMessageTracker">Interface to track active messages to know if handler harness can shutdown.</param>
        /// <param name="postOffice">Interface to send parcels.</param>
        public DispatcherFactory(string handlerAssemblyPath, ICollection<IChannel> servicedChannels, TypeMatchStrategy typeMatchStrategy, TimeSpan messageDispatcherWaitThreadSleepTime, IParcelTrackingSystem parcelTrackingSystem, ITrackActiveMessages activeMessageTracker, IPostOffice postOffice)
        {
            if (parcelTrackingSystem == null)
            {
                throw new ArgumentException("Parcel tracking system can't be null");
            }

            if (activeMessageTracker == null)
            {
                throw new ArgumentException("Active message tracker can't be null");
            }

            if (postOffice == null)
            {
                throw new ArgumentException("Post Office can't be null");
            }

            this.servicedChannels = servicedChannels;
            this.typeMatchStrategy = typeMatchStrategy;
            this.messageDispatcherWaitThreadSleepTime = messageDispatcherWaitThreadSleepTime;
            this.parcelTrackingSystem = parcelTrackingSystem;
            this.activeMessageTracker = activeMessageTracker;
            this.postOffice = postOffice;

            var currentlyLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();

            var handlerTypeMap = new List<TypeMap>();
            LoadHandlerTypeMapFromAssemblies(handlerTypeMap, currentlyLoadedAssemblies);

            // find all assemblies files to search for handlers.
            var filesRaw = Directory.GetFiles(handlerAssemblyPath, "*.dll", SearchOption.AllDirectories);

            // initialize the details about this handler.
            var assemblies = filesRaw.Select(_ => AssemblyDetails.CreateFromFile(_)).ToList();
            var machineDetails = MachineDetails.Create();
            this.harnessStaticDetails = new HarnessStaticDetails
                                      {
                                          MachineDetails = machineDetails,
                                          ExecutingUser = Environment.UserDomainName + "\\" + Environment.UserName,
                                          Assemblies = assemblies
                                      };

            // prune out the Microsoft.Bcl nonsense (if present)
            var filesUnfiltered = filesRaw.Where(_ => !_.Contains("Microsoft.Bcl")).ToList();

            // filter out assemblies that are currently loaded and might create overlap problems...
            var alreadyLoadedFileNames = currentlyLoadedAssemblies.Select(_ => _.CodeBase.ToLowerInvariant()).ToList();
            var files = filesUnfiltered.Where(_ => !alreadyLoadedFileNames.Contains(new Uri(_).ToString().ToLowerInvariant())).ToList();
            var pdbFiles = Directory.GetFiles(handlerAssemblyPath, "*.pdb", SearchOption.AllDirectories);

            // add an unknown assembly resolver to go try to find the dll in one of the files we have discovered...
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    var dllNameWithoutExtension = args.Name.Split(',')[0];
                    var dllName = dllNameWithoutExtension + ".dll";
                    var fullDllPath = files.FirstOrDefault(_ => _.EndsWith(dllName));
                    if (fullDllPath == null)
                    {
                        throw new TypeInitializationException(args.Name, null);
                    }

                    // Can't use Assembly.Load() here because it fails when you have different versions of N-level dependencies...I have no idea why Assembly.LoadFrom works.
                    Log.Write(() => "Loaded Assembly (in AppDomain.CurrentDomain.AssemblyResolve): " + dllNameWithoutExtension + " From: " + fullDllPath);
                    return Assembly.LoadFrom(fullDllPath);
                };

            var assembliesFromFiles = files.Select(
                filePathToPotentialHandlerAssembly =>
                    {
                        try
                        {
                            var fullDllPath = filePathToPotentialHandlerAssembly;
                            var dllNameWithoutExtension = (Path.GetFileName(filePathToPotentialHandlerAssembly) ?? string.Empty).Replace(".dll", string.Empty);

                            // Can't use Assembly.LoadFrom() here because it fails for some reason.
                            var assembly = LoadAssemblyFromDisk(dllNameWithoutExtension, pdbFiles, fullDllPath);
                            return assembly;
                        }
                        catch (ReflectionTypeLoadException reflectionTypeLoadException)
                        {
                            throw new ApplicationException(
                                "Failed to load assembly: " + filePathToPotentialHandlerAssembly + ". "
                                + string.Join(",", reflectionTypeLoadException.LoaderExceptions.Select(_ => _.ToString())),
                                reflectionTypeLoadException);
                        }
                    });

            LoadHandlerTypeMapFromAssemblies(handlerTypeMap, assembliesFromFiles);
            this.LoadContainerFromHandlerTypeMap(handlerTypeMap);
        }

        private static void LoadHandlerTypeMapFromAssemblies(List<TypeMap> handlerTypeMap, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var typesInFile = assembly.GetTypes();
                var mapsInFile = typesInFile.GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));
                handlerTypeMap.AddRange(mapsInFile);
            }
        }

        private void LoadContainerFromHandlerTypeMap(ICollection<TypeMap> handlerTypeMap)
        {
            foreach (var handlerTypeMapEntry in handlerTypeMap)
            {
                this.simpleInjectorContainer.Register(handlerTypeMapEntry.InterfaceType, handlerTypeMapEntry.ConcreteType);
            }

            // register the dispatcher so that hangfire can use it when a message is getting processed
            // if we weren't in hangfire we'd just persist the dispatcher and keep these two fields inside of it...
            this.simpleInjectorContainer.Register<IDispatchMessages>(
                () =>
                new MessageDispatcher(
                    this.simpleInjectorContainer,
                    this.sharedStateMap,
                    this.servicedChannels,
                    this.typeMatchStrategy,
                    this.messageDispatcherWaitThreadSleepTime,
                    this.harnessStaticDetails,
                    this.parcelTrackingSystem,
                    this.activeMessageTracker,
                    this.postOffice));

            foreach (var registration in this.simpleInjectorContainer.GetCurrentRegistrations())
            {
                var localScopeRegistration = registration;
                Log.Write(
                    () =>
                    $"Registered Type in SimpleInjector: {localScopeRegistration.ServiceType.FullName} -> {localScopeRegistration.Registration.ImplementationType.FullName}");
            }
        }

        private static Assembly LoadAssemblyFromDisk(string dllNameWithoutExtension, string[] pdbFiles, string fullDllPath)
        {
            var pdbName = dllNameWithoutExtension + ".pdb";
            var fullPdbPath = pdbFiles.FirstOrDefault(_ => _.EndsWith(pdbName));

            if (fullPdbPath == null)
            {
                var dllBytes = File.ReadAllBytes(fullDllPath);
                Log.Write(() => "Loaded Assembly (in GetAssembly): " + dllNameWithoutExtension + " From: " + fullDllPath + " Without Symbols.");
                return Assembly.Load(dllBytes);
            }
            else
            {
                var dllBytes = File.ReadAllBytes(fullDllPath);
                var pdbBytes = File.ReadAllBytes(fullPdbPath);
                Log.Write(() => "Loaded Assembly (in GetAssembly): " + dllNameWithoutExtension + " From: " + fullDllPath + " With Symbols: " + fullPdbPath);
                return Assembly.Load(dllBytes, pdbBytes);
            }
        }

        /// <inheritdoc />
        public IDispatchMessages Create()
        {
            var ret = this.simpleInjectorContainer.GetInstance<IDispatchMessages>();
            return ret;
        }
    }
}
