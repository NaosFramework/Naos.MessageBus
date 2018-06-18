﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformanceCounterLibrary.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.Diagnostics.Recipes
{
    using System.Collections.Generic;

    /// <summary>
    /// Various common <see cref="System.Diagnostics.PerformanceCounter" />'s sometimes with standard ranges.
    /// </summary>
#if NaosDiagnosticsRecipes
    public
#else
    [System.CodeDom.Compiler.GeneratedCode("Naos.Diagnostics", "See package version number")]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal
#endif
    static class PerformanceCounterLibrary
    {
        /// <summary>
        /// Gets disk idle time.
        /// </summary>
        public static RecipePerformanceCounterDescription DiskPercentageIdleTime => new RecipePerformanceCounterDescription(
            Category.PhysicalDisk,
            "%idle time",
            instanceName: null,
            expectedMinValue: 60,
            expectedMaxValue: 100);

        /*
         PhysicalDisk/%idle time - should not be less than ~%60

PhysicalDisk/Avg. Disk sec/Read should not be higher than ~20ms

PhysicalDisk/Avg. Disk sec/Write should not be higher than ~20ms

PhysicalDisk/Current disk queue length. should not be higher than 2

Memory\Available Mbytes, minimum 10% of memory should be free and available

\Memory\Pages/sec should not be higher than 1000

Network Interface(*)\Bytes Total/sec

Less than 40% of the interface consumed = Healthy

41%-64% of the interface consumed = Monitor or Caution

65-100% of the interface consumed = Critical, performance will be adversely affected

Network Interface(*)\Output Queue Length

0 = Healthy

1-2 = Monitor or Caution

Greater than 2 = Critical, performance will be adversely affected

Hyper-V Hypervisor Logical Processor(_Total)\% Total Run Time

Less than 60% consumed = Healthy

60% - 89% consumed = Monitor or Caution

90% - 100% consumed = Critical, performance will be adversely affected

Paging File\%Usage should not be higher than %10
         */

        /// <summary>
        /// Common categories.
        /// </summary>
        public static class Category
        {
            /// <summary>
            /// Physical disk.
            /// </summary>
            public const string PhysicalDisk = "PhysicalDisk";
        }

        /// <summary>
        /// Gets common counters.
        /// </summary>
        public static IReadOnlyCollection<RecipePerformanceCounterDescription> CommonCounters => new[]
                                                                                           {
                                                                                               DiskPercentageIdleTime,
                                                                                           };
    }
}
