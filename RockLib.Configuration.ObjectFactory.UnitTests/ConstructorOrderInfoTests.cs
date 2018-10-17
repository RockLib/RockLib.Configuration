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
        [InlineData(typeof(DefaultConstructor), true)]
        [InlineData(typeof(OneParameter), true, "bar")]
        [InlineData(typeof(OneParameter), false)]
        [InlineData(typeof(OneOptionalParameter), true, "bar")]
        [InlineData(typeof(OneOptionalParameter), false)]
        [InlineData(typeof(TwoParameters), true, "bar", "baz")]
        [InlineData(typeof(TwoParameters), false, "bar")]
        [InlineData(typeof(TwoParameters), false, "baz")]
        [InlineData(typeof(TwoParameters), false)]
        [InlineData(typeof(OneParameterOneOptionalParameter), true, "bar", "baz")]
        [InlineData(typeof(OneParameterOneOptionalParameter), false, "bar")]
        [InlineData(typeof(OneParameterOneOptionalParameter), false, "baz")]
        [InlineData(typeof(OneParameterOneOptionalParameter), false)]
        [InlineData(typeof(TwoOptionalParameters), true, "bar", "baz")]
        [InlineData(typeof(TwoOptionalParameters), false, "bar")]
        [InlineData(typeof(TwoOptionalParameters), false, "baz")]
        [InlineData(typeof(TwoOptionalParameters), false)]
        public void IsInvokableWithoutDefaultParametersIsCorrect(Type type, bool expectedIsInvokableWithoutDefaultParameters, params string[] resolvableMemberNames)
        {
            var constructor = type.GetTypeInfo().GetConstructors()[0];
            var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null);

            var orderInfo = new ConstructorOrderInfo(constructor, members);

            Assert.Same(constructor, orderInfo.Constructor);
            Assert.Equal(expectedIsInvokableWithoutDefaultParameters, orderInfo.IsInvokableWithoutDefaultParameters);
        }

        [Theory]
        [InlineData(typeof(DefaultConstructor), true)]
        [InlineData(typeof(OneParameter), true, "bar")]
        [InlineData(typeof(OneParameter), false)]
        [InlineData(typeof(OneOptionalParameter), true, "bar")]
        [InlineData(typeof(OneOptionalParameter), true)]
        [InlineData(typeof(TwoParameters), true, "bar", "baz")]
        [InlineData(typeof(TwoParameters), false, "bar")]
        [InlineData(typeof(TwoParameters), false, "baz")]
        [InlineData(typeof(TwoParameters), false)]
        [InlineData(typeof(OneParameterOneOptionalParameter), true, "bar", "baz")]
        [InlineData(typeof(OneParameterOneOptionalParameter), true, "bar")]
        [InlineData(typeof(OneParameterOneOptionalParameter), false, "baz")]
        [InlineData(typeof(OneParameterOneOptionalParameter), false)]
        [InlineData(typeof(TwoOptionalParameters), true, "bar", "baz")]
        [InlineData(typeof(TwoOptionalParameters), true, "bar")]
        [InlineData(typeof(TwoOptionalParameters), true, "baz")]
        [InlineData(typeof(TwoOptionalParameters), true)]
        public void IsInvokableWithDefaultParametersIsCorrect(Type type, bool expectedIsInvokableWithDefaultParameters, params string[] resolvableMemberNames)
        {
            var constructor = type.GetTypeInfo().GetConstructors()[0];
            var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null);

            var orderInfo = new ConstructorOrderInfo(constructor, members);

            Assert.Same(constructor, orderInfo.Constructor);
            Assert.Equal(expectedIsInvokableWithDefaultParameters, orderInfo.IsInvokableWithDefaultParameters);
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
        [InlineData(typeof(TwoParameters), typeof(ThreeOptionalParameters), 1, "bar")]
        [InlineData(typeof(ThreeOptionalParameters), typeof(TwoParameters), -1, "bar")]

        [InlineData(typeof(ThreeOptionalParameters), typeof(ThreeParametersOneRequired), 1, "foo")]
        [InlineData(typeof(ThreeParametersOneRequired), typeof(ThreeOptionalParameters), -1, "foo")]
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
        private class OneOptionalParameter { public OneOptionalParameter(int bar = -1) { } }
        private class TwoParameters { public TwoParameters(int bar, int baz) { } }
        private class OneParameterOneOptionalParameter { public OneParameterOneOptionalParameter(int bar, int baz = -1) { } }
        private class TwoOptionalParameters { public TwoOptionalParameters(int bar = -1, int baz = -1) { } }
        private class ThreeOptionalParameters { public ThreeOptionalParameters(int bar = -1, int baz = -1, int qux = -1) { } }
        private class ThreeParametersOneRequired { public ThreeParametersOneRequired(int foo, int baz = -1, int qux = -1) { } }
    }
}
#endif
