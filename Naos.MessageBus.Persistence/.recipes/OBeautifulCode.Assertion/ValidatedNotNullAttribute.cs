﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidatedNotNullAttribute.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in OBeautifulCode.Assertion.Recipes source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Assertion.Recipes
{
    using System;

    /// <summary>
    /// Attribute that avoids false positives of Code Analysis rule CA1062.
    /// </summary>
    /// <remarks>
    /// See <a href="http://esmithy.net/2011/03/15/suppressing-ca1062/" />.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
#if !OBeautifulCodeAssertionRecipesProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("OBeautifulCode.Assertion.Recipes", "See package version number")]
    internal
#else
    public
#endif
        sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}
