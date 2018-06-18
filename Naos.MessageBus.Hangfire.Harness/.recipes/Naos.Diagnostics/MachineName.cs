﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MachineName.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Diagnostics.Recipes
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.NetworkInformation;

    using static System.FormattableString;

    /// <summary>
    /// Uses various methods to get the name of a machine.
    /// </summary>
#if NaosDiagnosticsRecipes
    public
#else
    [System.CodeDom.Compiler.GeneratedCode("Naos.Diagnostics", "See package version number")]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal
#endif
    static class MachineName
    {
        /// <summary>
        /// Gets the name of this machine, using various methods to come
        /// up with a genrally "good" name for use in a broad set of scenarios.
        /// </summary>
        /// <returns>
        /// The name of this machine.
        /// </returns>
        public static string GetMachineName()
        {
            var result = GetMachineName(GetResolvedLocalhostName, GetFullyQualifiedDomainName, GetNetBiosName);
            return result;
        }

        /// <summary>
        /// Gets the name of this machine, using provided name map to come
        /// up with a genrally "good" name for use in a broad set of scenarios.
        /// </summary>
        /// <param name="machineNameKindToValueMap">Map of available names.</param>
        /// <returns>
        /// The name of this machine.
        /// </returns>
        public static string GetMachineName(this IReadOnlyDictionary<MachineNameKind, string> machineNameKindToValueMap)
        {
            if (machineNameKindToValueMap == null)
            {
                throw new ArgumentNullException(nameof(machineNameKindToValueMap));
            }

            if (!machineNameKindToValueMap.ContainsKey(MachineNameKind.ResolvedLocalhostName))
            {
                throw new ArgumentException(Invariant($"Parameter {machineNameKindToValueMap} must contain entry for {nameof(MachineNameKind)}.{MachineNameKind.ResolvedLocalhostName}."));
            }

            if (!machineNameKindToValueMap.ContainsKey(MachineNameKind.FullyQualifiedDomainName))
            {
                throw new ArgumentException(Invariant($"Parameter {machineNameKindToValueMap} must contain entry for {nameof(MachineNameKind)}.{MachineNameKind.FullyQualifiedDomainName}."));
            }

            if (!machineNameKindToValueMap.ContainsKey(MachineNameKind.NetBiosName))
            {
                throw new ArgumentException(Invariant($"Parameter {machineNameKindToValueMap} must contain entry for {nameof(MachineNameKind)}.{MachineNameKind.NetBiosName}."));
            }

            var result = GetMachineName(
                () => machineNameKindToValueMap[MachineNameKind.ResolvedLocalhostName],
                () => machineNameKindToValueMap[MachineNameKind.FullyQualifiedDomainName],
                () => machineNameKindToValueMap[MachineNameKind.NetBiosName]);

            return result;
        }

        /// <summary>
        /// Gets the name of this machine, using provided name functions to come
        /// up with a genrally "good" name for use in a broad set of scenarios.
        /// </summary>
        /// <param name="resolvedLocalhostFunc">Function to get the value of <see cref="GetResolvedLocalhostName" />.</param>
        /// <param name="fullyQualifiedDomainNameFunc">Function to get the value of <see cref="GetFullyQualifiedDomainName" />.</param>
        /// <param name="netBiosNameFunc">Function to get the value of <see cref="GetNetBiosName" />.</param>
        /// <returns>
        /// The name of this machine.
        /// </returns>
        public static string GetMachineName(Func<string> resolvedLocalhostFunc, Func<string> fullyQualifiedDomainNameFunc, Func<string> netBiosNameFunc)
        {
            if (resolvedLocalhostFunc == null)
            {
                throw new ArgumentNullException(nameof(resolvedLocalhostFunc));
            }

            if (fullyQualifiedDomainNameFunc == null)
            {
                throw new ArgumentNullException(nameof(fullyQualifiedDomainNameFunc));
            }

            if (netBiosNameFunc == null)
            {
                throw new ArgumentNullException(nameof(netBiosNameFunc));
            }

            var result = resolvedLocalhostFunc();
            if (result == "localhost")
            {
                result = fullyQualifiedDomainNameFunc();
                if (string.IsNullOrEmpty(result))
                {
                    result = netBiosNameFunc();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the name of this machine using various methods of determining the name.
        /// </summary>
        /// <returns>
        /// A map of the kind of machine name to the machine name.
        /// </returns>
        public static IReadOnlyDictionary<MachineNameKind, string> GetMachineNames()
        {
            var result = new Dictionary<MachineNameKind, string>
            {
                { MachineNameKind.NetBiosName, GetNetBiosName() },
                { MachineNameKind.FullyQualifiedDomainName, GetFullyQualifiedDomainName() },
                { MachineNameKind.ResolvedLocalhostName, GetResolvedLocalhostName() },
            };

            return result;
        }

        /// <summary>
        /// Gets the NetBIOS name of this local computer.
        /// </summary>
        /// <returns>
        /// The NetBIOS name of this local computer.
        /// </returns>
        public static string GetNetBiosName()
        {
            var result = Environment.MachineName;
            return result;
        }

        /// <summary>
        /// Gets the fully qualified domain name.
        /// </summary>
        /// <returns>
        /// The fully qualified domain name.
        /// </returns>
        /// <remarks>
        /// Adapted from <a href="https://stackoverflow.com/a/804719/356790" />
        /// </remarks>
        public static string GetFullyQualifiedDomainName()
        {
            var result = Dns.GetHostName();

            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;

            if (!string.IsNullOrEmpty(domainName))
            {
                domainName = "." + domainName;

                // if hostname does not already include domain name
                if (!result.EndsWith(domainName, StringComparison.Ordinal))
                {
                    // add the domain name part
                    result += domainName;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the resolved localhost name.
        /// </summary>
        /// <returns>
        /// Gets the resolved
        /// </returns>
        public static string GetResolvedLocalhostName()
        {
            var result = Dns.GetHostEntry("localhost").HostName;
            return result;
        }
    }
}
