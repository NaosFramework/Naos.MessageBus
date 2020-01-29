﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryEqualityComparer.cs" company="OBeautifulCode">
//   Copyright (c) OBeautifulCode 2018. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in OBeautifulCode.Equality.Recipes source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace OBeautifulCode.Equality.Recipes
{
    using System.Collections.Generic;

    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> for any <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of dictionary values.</typeparam>
#if !OBeautifulCodeEqualityRecipesProject
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("OBeautifulCode.Equality.Recipes", "See package version number")]
    internal
#else
    public
#endif
        class DictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<IDictionary<TKey, TValue>>
    {
        /// <inheritdoc />
        public bool Equals(
            IDictionary<TKey, TValue> x,
            IDictionary<TKey, TValue> y)
        {
            var result = x.IsDictionaryEqualTo(y);

            return result;
        }

        /// <inheritdoc />
        public int GetHashCode(
            IDictionary<TKey, TValue> obj)
        {
            var result = HashCodeHelper.Initialize().Hash(obj).Value;

            return result;
        }
    }
}
