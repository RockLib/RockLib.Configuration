using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ObjectFactory;
using RockLib.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests
{
   public class ConstructorOrderInfoTests
   {
      private static Type _constructorOrderInfoType =
         Type.GetType("RockLib.Configuration.ObjectFactory.ConstructorOrderInfo, RockLib.Configuration.ObjectFactory")!;

      [Theory]
      [InlineData(typeof(DefaultConstructor), 0)]
      [InlineData(typeof(OneParameter), 1)]
      [InlineData(typeof(TwoParameters), 2)]
      [InlineData(typeof(OneParameterOneOptionalParameter), 2)]
      public void TotalParametersIsCorrect(Type type, int expectedTotalParameters)
      {
#pragma warning disable CA1062 // Validate arguments of public methods
         var constructor = type.GetConstructors()[0];
#pragma warning restore CA1062 // Validate arguments of public methods
         var members = new Dictionary<string, IConfigurationSection>();

         var orderInfo = _constructorOrderInfoType.New(constructor, members, Resolver.Empty);

         Assert.Same(constructor, orderInfo.Constructor.Object);
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
#pragma warning disable CA1062 // Validate arguments of public methods
         var constructor = type.GetConstructors()[0];
#pragma warning restore CA1062 // Validate arguments of public methods
         var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null!);

         var orderInfo = _constructorOrderInfoType.New(constructor, members, Resolver.Empty);

         Assert.Same(constructor, orderInfo.Constructor.Object);
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
#pragma warning disable CA1062 // Validate arguments of public methods
         var constructor = type.GetConstructors()[0];
#pragma warning restore CA1062 // Validate arguments of public methods
         var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null!);

         var orderInfo = _constructorOrderInfoType.New(constructor, members, Resolver.Empty);

         Assert.Same(constructor, orderInfo.Constructor.Object);
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
#pragma warning disable CA1062 // Validate arguments of public methods
         var lhsConstructor = lhsConstructorType.GetConstructors()[0];
         var rhsConstructor = rhsConstructorType.GetConstructors()[0];
#pragma warning restore CA1062 // Validate arguments of public methods

         var members = resolvableMemberNames.ToDictionary(x => x, x => (IConfigurationSection)null!);

         var lhs = _constructorOrderInfoType.New(lhsConstructor, members, Resolver.Empty);
         var rhs = _constructorOrderInfoType.New(rhsConstructor, members, Resolver.Empty);

         var actual = lhs.CompareTo(rhs);

         Assert.Equal(expectedComparisonValue, actual);
      }

      [Theory]
      [InlineData(typeof(OneAlternateName), "foo")]
      [InlineData(typeof(OneAlternateName), "bar")]
      [InlineData(typeof(MultipleAlternateNames), "foo")]
      [InlineData(typeof(MultipleAlternateNames), "bar")]
      [InlineData(typeof(MultipleAlternateNames), "baz")]
      public void AlternateNameIsUsed(Type type, string configurationMemberName)
      {
#pragma warning disable CA1062 // Validate arguments of public methods
         var constructor = type.GetConstructors()[0];
#pragma warning restore CA1062 // Validate arguments of public methods
         var members = new Dictionary<string, IConfigurationSection>() { { configurationMemberName, null! } };

         var orderInfo = _constructorOrderInfoType.New(constructor, members, Resolver.Empty);

         Assert.True(orderInfo.IsInvokableWithoutDefaultParameters);
         Assert.True(orderInfo.IsInvokableWithDefaultParameters);
         Assert.Equal(1, orderInfo.MatchedParameters);
         Assert.Empty(orderInfo.MissingParameterNames.Object);
      }

      [Fact]
      public void MatchedNamesAreHigherPriorityThanResolvedTypes()
      {
         var constructors = typeof(ConstructorsWithMatchingParamCount).GetConstructors();
         var members = new Dictionary<string, IConfigurationSection>
            {
                { "one", null! },
                { "two", null! }
            };
         var resolver = new Resolver(t => t, t => t == typeof(string) ? true : false);

         var orderInfo1 = _constructorOrderInfoType.New(constructors[0], members, resolver);
         var orderInfo2 = _constructorOrderInfoType.New(constructors[1], members, resolver);

         Assert.Equal(2, orderInfo1.MatchedParameters);
         Assert.Equal(2, orderInfo2.MatchedParameters);
         Assert.Equal(4, orderInfo1.TotalParameters);
         Assert.Equal(4, orderInfo2.TotalParameters);
         Assert.Equal(2, orderInfo1.MatchedNamedParameters);
         Assert.Equal(1, orderInfo2.MatchedNamedParameters);

         Assert.Equal(-1, orderInfo1.CompareTo(orderInfo2));
      }


      [Theory]
      [InlineData(2, 3, -1)]
      [InlineData(3, 2, 1)]
      [InlineData(2, 2, 0)]
      public void CompareParameterTypes(int constructorOneIndex, int constructorTwoIndex, int expectedComparisonValue)
      {
         var constructors = typeof(ConstructorsWithMatchingParamCount).GetConstructors();
         var members = new Dictionary<string, IConfigurationSection>
            {
                { "name", null! },
                { "url", null! }
            };
         var resolver = new Resolver(t => t, t => t == typeof(string) ? true : false);

         var orderInfo1 = _constructorOrderInfoType.New(constructors[constructorOneIndex], members, resolver);
         var orderInfo2 = _constructorOrderInfoType.New(constructors[constructorTwoIndex], members, resolver);

         Assert.Equal(expectedComparisonValue, orderInfo1.CompareTo(orderInfo2));
      }

#pragma warning disable CA1812
      private class DefaultConstructor { }
      private class OneParameter { public OneParameter(int bar) { } }
      private class OneOptionalParameter { public OneOptionalParameter(int bar = -1) { } }
      private class TwoParameters { public TwoParameters(int bar, int baz) { } }
      private class OneParameterOneOptionalParameter { public OneParameterOneOptionalParameter(int bar, int baz = -1) { } }
      private class TwoOptionalParameters { public TwoOptionalParameters(int bar = -1, int baz = -1) { } }
      private class ThreeOptionalParameters { public ThreeOptionalParameters(int bar = -1, int baz = -1, int qux = -1) { } }
      private class ThreeParametersOneRequired { public ThreeParametersOneRequired(int foo, int baz = -1, int qux = -1) { } }

      private class OneAlternateName
      {
         public OneAlternateName([AlternateName("bar")] int foo) => Foo = foo;
         public int Foo { get; }
      }

      private class MultipleAlternateNames
      {
         public MultipleAlternateNames([AlternateName("bar"), AlternateName("baz")] int foo) => Foo = foo;
         public int Foo { get; }
      }

      private class ConstructorsWithMatchingParamCount
      {
         public ConstructorsWithMatchingParamCount(int one = 1, double two = 2, decimal three = 3, object? four = null) { }
         public ConstructorsWithMatchingParamCount(string notOne, double two = 2, decimal three = 3, object? four = null) { }
         public ConstructorsWithMatchingParamCount(string name, Uri url, string method = "POST", IReadOnlyDictionary<string, string>? defaultHeaders = null) { }
         public ConstructorsWithMatchingParamCount(string name, string url, string method = "POST", IReadOnlyDictionary<string, string>? defaultHeaders = null) { }
      }
#pragma warning restore CA1812
   }
}
