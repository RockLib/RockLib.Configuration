using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using static RockLib.Configuration.ObjectFactory.ConfigurationObjectFactory.ObjectBuilder;

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
        public void MatchedOrDefaultParametersRatioIsCorrect(Type type, double expectedMatchedParametersRatio, params string[] resolvableMemberNames)
        {
            var constructor = type.GetTypeInfo().GetConstructors()[0];
            var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null);

            var orderInfo = new ConstructorOrderInfo(constructor, members);

            Assert.Same(constructor, orderInfo.Constructor);
            Assert.Equal(expectedMatchedParametersRatio, orderInfo.MatchedOrDefaultParametersRatio);
        }

        private class DefaultConstructor {}
        private class OneParameter { public OneParameter(int bar) {} }
        private class TwoParameters { public TwoParameters(int bar, int baz) {} }
        private class OneParameterOneOptionalParameter { public OneParameterOneOptionalParameter(int bar, int baz = -1) {} }
    }
}
