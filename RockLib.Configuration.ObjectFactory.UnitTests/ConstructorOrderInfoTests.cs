#if DEBUG
using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests
{
    public class ConstructorOrderInfoTests
    {
        [Theory]
        [InlineData(typeof(DefaultConstructor), 0)]
        [InlineData(typeof(OneParameter), 1)]
        [InlineData(typeof(TwoParameters), 2)]
        [InlineData(typeof(OneParameterOneOptionalParameter), 2)]
        public void TotalParametersIsCorrect(Type type, int expectedTotalParameters)
        {
            var constructor = type.GetTypeInfo().GetConstructors()[0];
            var members = new Dictionary<string, IConfigurationSection>();

            var orderInfo = new ConstructorOrderInfo(constructor, members);

            Assert.Same(constructor, orderInfo.Constructor);
            Assert.Equal(expectedTotalParameters, orderInfo.TotalParameters);
        }

        [Theory]
        [InlineData(typeof(DefaultConstructor), 1)]
        [InlineData(typeof(OneParameter), 1, "bar")]
        [InlineData(typeof(OneParameter), 0)]
        [InlineData(typeof(TwoParameters), 1, "bar", "baz")]
        [InlineData(typeof(TwoParameters), 0.5, "bar")]
        [InlineData(typeof(TwoParameters), 0)]
        [InlineData(typeof(OneParameterOneOptionalParameter), 1, "bar", "baz")]
        [InlineData(typeof(OneParameterOneOptionalParameter), 0.5, "bar")]
        [InlineData(typeof(OneParameterOneOptionalParameter), 0)]
        public void MatchedParametersRatioIsCorrect(Type type, double expectedMatchedParametersRatio, params string[] resolvableMemberNames)
        {
            var constructor = type.GetTypeInfo().GetConstructors()[0];
            var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null);

            var orderInfo = new ConstructorOrderInfo(constructor, members);

            Assert.Same(constructor, orderInfo.Constructor);
            Assert.Equal(expectedMatchedParametersRatio, orderInfo.MatchedParametersRatio);
        }

        [Theory]
        [InlineData(typeof(DefaultConstructor), 1)]
        [InlineData(typeof(OneParameter), 1, "bar")]
        [InlineData(typeof(OneParameter), 0)]
        [InlineData(typeof(TwoParameters), 1, "bar", "baz")]
        [InlineData(typeof(TwoParameters), 0.5, "bar")]
        [InlineData(typeof(TwoParameters), 0)]
        [InlineData(typeof(OneParameterOneOptionalParameter), 1, "bar", "baz")]
        [InlineData(typeof(OneParameterOneOptionalParameter), 1, "bar")]
        [InlineData(typeof(OneParameterOneOptionalParameter), 0.5)]
        public void MatchedOrDefaultParametersRatioIsCorrect(Type type, double expectedMatchedOrDefaultParametersRatio, params string[] resolvableMemberNames)
        {
            var constructor = type.GetTypeInfo().GetConstructors()[0];
            var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null);

            var orderInfo = new ConstructorOrderInfo(constructor, members);

            Assert.Same(constructor, orderInfo.Constructor);
            Assert.Equal(expectedMatchedOrDefaultParametersRatio, orderInfo.MatchedOrDefaultParametersRatio);
        }

        [Theory]
        [InlineData(typeof(OneParameter), typeof(TwoParameters), -1, "bar")]
        [InlineData(typeof(TwoParameters), typeof(OneParameter), 1, "bar")]
        [InlineData(typeof(TwoParameters), typeof(OneParameterOneOptionalParameter), 1, "bar")]
        [InlineData(typeof(OneParameterOneOptionalParameter), typeof(TwoParameters), -1, "bar")]
        [InlineData(typeof(OneParameter), typeof(TwoParameters), 1, "bar", "baz")]
        [InlineData(typeof(TwoParameters), typeof(OneParameter), -1, "bar", "baz")]
        [InlineData(typeof(TwoParameters), typeof(OneParameterOneOptionalParameter), 0, "bar", "baz")]
        [InlineData(typeof(OneParameterOneOptionalParameter), typeof(TwoParameters), 0, "bar", "baz")]
        public void CompareToReturnsTheCorrectValue(Type lhsConstructorType, Type rhsConstructorType, int expectedComparisonValue, params string[] resolvableMemberNames)
        {
            var lhsConstructor = lhsConstructorType.GetTypeInfo().GetConstructors()[0];
            var rhsConstructor = rhsConstructorType.GetTypeInfo().GetConstructors()[0];

            var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null);

            var lhs = new ConstructorOrderInfo(lhsConstructor, members);
            var rhs = new ConstructorOrderInfo(rhsConstructor, members);

            var actual = lhs.CompareTo(rhs);

            Assert.Equal(expectedComparisonValue, actual);
        }

        private class DefaultConstructor { }
        private class OneParameter { public OneParameter(int bar) { } }
        private class TwoParameters { public TwoParameters(int bar, int baz) { } }
        private class OneParameterOneOptionalParameter { public OneParameterOneOptionalParameter(int bar, int baz = -1) { } }
    }
}
#endif
