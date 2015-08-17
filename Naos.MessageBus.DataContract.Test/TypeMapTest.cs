﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeMapTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract.Test
{
    using System.Collections.Generic;
    using System.Linq;

    using Xunit;

    public class TypeMapTest
    {
        [Fact]
        public static void GetTypeMapsOfImplementersOfGenericType_MatchingTypesProvidedICollectionString_MatchingReturned()
        {
            var expectedMap = new TypeMap()
                                  {
                                      InterfaceType = typeof(ICollection<string>),
                                      ConcreteType = typeof(List<string>)
                                  };

            InternalRunTest(expectedMap);
        }

        [Fact]
        public static void GetTypeMapsOfImplementersOfGenericType_MatchingTypesProvidedICollectionInt_MatchingReturned()
        {
            var expectedMap = new TypeMap()
                                  {
                                      InterfaceType = typeof(ICollection<int>),
                                      ConcreteType = typeof(List<int>)
                                  };

            InternalRunTest(expectedMap);
        }

        private static void InternalRunTest(TypeMap expectedMap)
        {
            var typesToCheck = new[] { expectedMap.ConcreteType };
            var genericTypeToFilter = typeof(ICollection<>); // I'm specifically testing that this will work for any implementer...                
            var maps = typesToCheck.GetTypeMapsOfImplementersOfGenericType(genericTypeToFilter);

            Assert.NotNull(maps);
            Assert.Equal(1, maps.Count);

            var actualMap = maps.Single();
            Assert.Equal(expectedMap.InterfaceType, actualMap.InterfaceType);
            Assert.Equal(expectedMap.ConcreteType, actualMap.ConcreteType);
        }
    }
}