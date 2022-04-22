using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit;

namespace Tests
{
   public class ConfigurationObjectFactorySadPathTests
   {
      [Fact]
      public void GivenNullConfigurationPassedToGenericMethodThrowsArgumentNullException()
      {
         IConfiguration config = null!;

         Assert.Throws<ArgumentNullException>(() => config.Create<EmptyClass>());
      }

      [Fact]
      public void GivenNullConfigurationPassedToNonGenericMethodThrowsArgumentNullException()
      {
         IConfiguration config = null!;

         Assert.Throws<ArgumentNullException>(() => config.Create(typeof(EmptyClass)));
      }

      [Fact]
      public void GivenNullTypePassedToNonGenericMethodThrowsArgumentNullException()
      {
         var config = new ConfigurationBuilder().Build();

         Type type = null!;

         Assert.Throws<ArgumentNullException>(() => config.Create(type));
      }

      [Fact]
      public void GivenValueConverterThatReturnsNullThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                    { "foo:bar", "abcdefg" },
            })
            .Build();

         var fooSection = config.GetSection("foo");

         var valueConverters = new ValueConverters()
             .Add(typeof(StringContainer), "bar", value => (string)null!);

         var ex = Assert.Throws<InvalidOperationException>(() => fooSection.Create<StringContainer>(valueConverters: valueConverters));

#if DEBUG
            var expectedInner = Exceptions.ResultCannotBeNull(typeof(string), typeof(StringContainer), "Bar");
            var expected = Exceptions.CannotConvertSectionValueToTargetType(fooSection.GetSection("bar"), typeof(string), expectedInner);

            Assert.Equal(expected.Message, ex.Message);
            Assert.Equal(expectedInner.Message, ex.InnerException.Message);
#endif
      }

      [Fact]
      public void GivenConfigurationValueNotAssignableToTargetTypeThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar", "wtf is this even supposed to be?" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<EmptyClassPropertyClass>());

#if DEBUG
            var expected = Exceptions.CannotConvertSectionValueToTargetType(fooSection.GetSection("bar"), typeof(EmptyClass));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenTypeSpecifiedObjectWithTypeNotAssignableToTargetTypeThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    // SimplePropertyClass does not inherit EmptyClass
                    { "foo:bar:type", typeof(SimplePropertyClass).AssemblyQualifiedName! },
                    { "foo:bar:value", "123.45" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<EmptyClassPropertyClass>());

#if DEBUG
            var expected = Exceptions.ConfigurationSpecifiedTypeIsNotAssignableToTargetType(typeof(EmptyClass), typeof(SimplePropertyClass));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenAnArrayTargetTypeWithARankGreaterThanOneThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:0", "123.45" },
                    { "foo:bar:1", "456.78" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<MultiDimensionalArrayPropertyClass>());

#if DEBUG
            var expected = Exceptions.ArrayRankGreaterThanOneIsNotSupported(typeof(double[,]));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenAnAbstractTargetTypeThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:baz", "wtf is this even supposed to be?" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<InterfacePropertyClass>());

#if DEBUG
            var expected = Exceptions.CannotCreateAbstractType(fooSection.GetSection("bar"), typeof(IBar));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenObjectTargetTypeThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:baz", "wtf is this even supposed to be?" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<ObjectPropertyClass>());

#if DEBUG
            var expected = Exceptions.CannotCreateObjectType;
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenUnsupportedCollectionTargetTypeThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:baz", "wtf is this even supposed to be?" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<ArrayListPropertyClass>());

#if DEBUG
            var expected = Exceptions.UnsupportedCollectionType(typeof(ArrayList));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenNonCollectionTargetTypeWhenConfigurationRepresentsListThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:0", "123.45" },
                    { "foo:bar:1", "123.45" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<NestedSimplePropertyClass>());

#if DEBUG
            var expected = Exceptions.ConfigurationIsAList(fooSection.GetSection("bar"), typeof(SimplePropertyClass));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenAMemberDecoratedWithDefaultTypeAttributeWithAValueNotAssignableToTheMemberTypeThrowsArgumentException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:bar", "123.45" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<ArgumentException>(() => fooSection.Create<InvalidDefaultTypeForPropertyClass>());

#if DEBUG
            var expected = Exceptions.DefaultTypeIsNotAssignableToTargetType(typeof(SimplePropertyClass), typeof(EmptyClass));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenATypeDecoratedWithDefaultTypeAttributeWithAValueNotAssignableToTheDecoratedTypeThrowsArgumentException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:bar", "123.45" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<ArgumentException>(() => fooSection.Create<InvalidDefaultTypeForPropertyTypeClass>());

#if DEBUG
            var expected = Exceptions.DefaultTypeIsNotAssignableToTargetType(typeof(InvalidDefaultType), typeof(EmptyClass));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenAMemberDecoratedWithDefaultTypeAttributeWithAnAbstractValueThrowsArgumentException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:bar", "123.45" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<ArgumentException>(() => fooSection.Create<AbstractDefaultTypeForPropertyClass>());

#if DEBUG
            var expected = Exceptions.DefaultTypeFromAttributeCannotBeAbstract(typeof(AbstractClass));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenATypeDecoratedWithDefaultTypeAttributeWithAnAbstractValueThrowsArgumentException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:bar", "123.45" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<ArgumentException>(() => fooSection.Create<AbstractDefaultTypeForPropertyTypeClass>());

#if DEBUG
            var expected = Exceptions.DefaultTypeFromAttributeCannotBeAbstract(typeof(AbstractClassImplementingInterface));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenATypeWithMultipleMembersWithTheSameNameAndDifferentDefaultTypeAttributeValuesThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:baz", "123.45" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<InconsistentDefaultTypesClass>());

#if DEBUG
            var expected = Exceptions.InconsistentDefaultTypeAttributesForMultipleMembers("bar");
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenATargetTypeWithNoPublicConstructorsThrowsInvalidOperationException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar", "123.45" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<NoPublicConstructorsClass>());

#if DEBUG
            var expected = Exceptions.NoPublicConstructorsFound;
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Theory]
      [InlineData(typeof(double))]
      [InlineData(typeof(Corge))]
      [InlineData(typeof(string))]
      [InlineData(typeof(decimal))]
      [InlineData(typeof(Guid))]
      [InlineData(typeof(DateTime))]
      [InlineData(typeof(DateTimeOffset))]
      [InlineData(typeof(TimeSpan))]
      [InlineData(typeof(Uri))]
      [InlineData(typeof(Encoding))]
      [InlineData(typeof(Type))]
      [InlineData(typeof(IsDecoratedWithConvertMethod))]
      public void GivenABranchNodeWhenTheMemberTypeRequiresALeafNodeThrowsInvalidOperationException(Type type)
      {
         var classType = typeof(GenericPropertyClass<>).MakeGenericType(type);

         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar:baz", "123.45" },
                    { "foo:bar:qux", "456" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create(classType));

#if DEBUG
            var expected = Exceptions.TargetTypeRequiresConfigurationValue(fooSection.GetSection("bar"), type, classType, "Bar");
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenTheReturnTypeOfTheMethodSpecifiedByAConvertMethodAttributeIsNotAssignableToTheTargetTypeThrowsArgumentException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:qux", "123" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<ArgumentException>(() => fooSection.Create<HasInvalidConvertMethodAttribute>());

#if DEBUG
            var expected = Exceptions.ReturnTypeOfMethodFromAttributeIsNotAssignableToTargetType(typeof(SomeStruct), typeof(AnotherStruct), nameof(HasInvalidConvertMethodAttribute.IllegalConvertMethod));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenNoSuitableMethodFoundForMethodSpecifiedByConvertMethodAttributeThrowsArgumentException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:qux", "123" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<ArgumentException>(() => fooSection.Create<HasNoMethodMatchingTheArgumentFromAConvertMethodAttribute>());

#if DEBUG
            var expected = Exceptions.NoMethodFound(typeof(HasNoMethodMatchingTheArgumentFromAConvertMethodAttribute), "ThisMethodDoesNotExist");
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenTheBestMatchingConstructorHasParametersNotMappedToAConfigurationChildThrowsArgumentException()
      {
         var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string>
             {
                    { "foo:bar", "123" },
             })
             .Build();

         var fooSection = config.GetSection("foo");
         var actual = Assert.Throws<InvalidOperationException>(() => fooSection.Create<TwoArgConstructor>());

#if DEBUG
            var constructorOrderInfo = new ConstructorOrderInfo(
                typeof(TwoArgConstructor).GetTypeInfo().GetConstructors()[0],
                new Dictionary<string, IConfigurationSection> { { "bar", null } },
                Resolver.Empty);
            var expected = Exceptions.MissingRequiredConstructorParameters(fooSection, constructorOrderInfo);
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

#pragma warning disable CA1812
      private class EmptyClass { }

      private class SimplePropertyClass
      {
         public double Bar { get; set; }
      }

      private class GenericPropertyClass<T>
      {
         public T? Bar { get; set; }
      }

      [ConvertMethod(nameof(Convert))]
      private class IsDecoratedWithConvertMethod
      {
         public IsDecoratedWithConvertMethod(double baz) => Baz = baz;
         public double Baz { get; }
         private static IsDecoratedWithConvertMethod Convert(string value) => new IsDecoratedWithConvertMethod(double.Parse(value, CultureInfo.InvariantCulture));
      }

      private enum Corge { Garply, Grault }

      private class EmptyClassPropertyClass
      {
         public EmptyClass? Bar { get; set; }
      }

      private class ArrayPropertyClass
      {
         public double[]? Bar { get; set; }
      }

      private class MultiDimensionalArrayPropertyClass
      {
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
         public double[,]? Bar { get; set; }
#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional
      }

      private class ListPropertyClass
      {
         public List<double>? Bar { get; set; }
      }

      private class InterfacePropertyClass
      {
         public IBar? Bar { get; set; }
      }

      private interface IBar { }

      private class ObjectPropertyClass
      {
         public object? Bar { get; set; }
      }

      private class ArrayListPropertyClass
      {
         public ArrayList? Bar { get; set; }
      }

      private class NestedSimplePropertyClass
      {
         public SimplePropertyClass? Bar { get; set; }
      }

      private class NoPublicConstructorsClass
      {
         private NoPublicConstructorsClass() { }
         public double Bar { get; set; }
      }

      private class AmbiguousConstructorsClass
      {
         public AmbiguousConstructorsClass(double bar) => Bar = bar;
         public AmbiguousConstructorsClass(int baz) => Baz = baz;
         public double Bar { get; set; }
         public int Baz { get; set; }
      }

      private class InvalidDefaultTypeForPropertyClass
      {
         [DefaultType(typeof(EmptyClass))]
         public SimplePropertyClass? Bar { get; set; }
      }

      private class InvalidDefaultTypeForPropertyTypeClass
      {
         public InvalidDefaultType? Bar { get; set; }
      }

      [DefaultType(typeof(EmptyClass))]
      private class InvalidDefaultType
      {
         public double Bar { get; set; }
      }

      private class InconsistentDefaultTypesClass
      {
         public InconsistentDefaultTypesClass([DefaultType(typeof(Bar1))] IBar bar)
         {
            Bar = bar;
         }

         [DefaultType(typeof(Bar2))]
         public IBar Bar { get; set; }
      }

      private class Bar1 : IBar
      {
         public string? Baz { get; set; }
      }

      private class Bar2 : IBar
      {
         public string? Baz { get; set; }
      }

      private struct SomeStruct
      {
         public SomeStruct(int bar) => Bar = bar;
         public int Bar { get; }
      }

      private struct AnotherStruct
      {
         public AnotherStruct(int baz) => Baz = baz;
         public int Baz { get; }
      }

      private class HasInvalidConvertMethodAttribute
      {
         public HasInvalidConvertMethodAttribute([ConvertMethod(nameof(IllegalConvertMethod))] SomeStruct qux) => Qux = qux;
         public SomeStruct Qux { get; }
         internal static AnotherStruct IllegalConvertMethod(string value) => new AnotherStruct(int.Parse(value, CultureInfo.InvariantCulture));
      }

      private class HasNoMethodMatchingTheArgumentFromAConvertMethodAttribute
      {
         [ConvertMethod("ThisMethodDoesNotExist")] public SomeStruct Qux { get; set; }
      }

      private interface IInterface { }
      private abstract class AbstractClass : IInterface { }

      [DefaultType(typeof(AbstractClassImplementingInterface))]
      private interface IInterfaceDecoratedWithAbstractDefaultType { }

      private abstract class AbstractClassImplementingInterface : IInterfaceDecoratedWithAbstractDefaultType { }

      private class AbstractDefaultTypeForPropertyClass
      {
         [DefaultType(typeof(AbstractClass))]
         public IInterface? Bar { get; set; }
      }

      private class AbstractDefaultTypeForPropertyTypeClass
      {
         public IInterfaceDecoratedWithAbstractDefaultType? Bar { get; set; }
      }

      private class TwoArgConstructor
      {
         public TwoArgConstructor(int bar, int baz) { }
      }
#pragma warning restore CA1812
   }
}
