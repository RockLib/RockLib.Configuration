using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace Tests
{
#pragma warning disable CA1034 // Nested types should not be visible
    public class ConfigurationObjectFactoryTests
    {
        [Fact]
        public void SameConstructorWithDifferentParameterType()
        {
            var config = new ConfigurationBuilder()
             .AddInMemoryCollection(new Dictionary<string, string?>
             {
                    { "MatchingConstructorsWithDifferenthParamType:name", "test" },
                    { "MatchingConstructorsWithDifferenthParamType:url", "https://www.google.com" }
             })
             .Build();

            var fooSection = config.GetSection("MatchingConstructorsWithDifferenthParamType");
            var foo = fooSection.Create<MatchingConstructorsWithDifferenthParamType>()!;
            Assert.Equal("test", foo.Name);
            Assert.Equal(new Uri("https://www.google.com"), foo.Url);
            Assert.Equal("POST", foo.Method);
            Assert.Null(foo.DefaultHeaders);
        }

        public class MatchingConstructorsWithDifferenthParamType
        {
            public string Name { get; }
            public Uri Url { get; }
            public string Method { get; set; }
            public IReadOnlyDictionary<string, string>? DefaultHeaders { get; set; }
            public MatchingConstructorsWithDifferenthParamType(string name, Uri url, string method = "POST", IReadOnlyDictionary<string, string>? defaultHeaders = null)
            {
                Name = name;
                Url = url;
                Method = method;
                DefaultHeaders = defaultHeaders;
            }
            public MatchingConstructorsWithDifferenthParamType(string name, string url, string method = "POST", IReadOnlyDictionary<string, string>? defaultHeaders = null)
            {
                Name = name;
                Url = new Uri(url);
                Method = method;
                DefaultHeaders = defaultHeaders;
            }
        }

        [Fact]
        public void SupportsCustomTypeImplementingIEnumerable()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:baz", "123.45" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<CustomEnumerablePropertyClass>()!;
            Assert.Equal(123.45M, foo.Bar!.Baz);
        }

        [Fact]
        public void SupportsMembersOfTypeFuncOfT()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
                { "baz:foo", "123"},
                { "baz:bar:type", typeof(AnotherBar).AssemblyQualifiedName! },
                { "baz:bar:value:garply", "abc" } })
                .Build();

            var baz = config.GetSection("baz").Create<Baz>()!;
            Assert.Equal(123, baz.GetFoo());
            var bar = baz.GetBar();
            Assert.IsType<AnotherBar>(bar);
            var anotherBar = (AnotherBar)bar;
            Assert.Equal("abc", anotherBar.Garply);
        }

        public class Baz
        {
            private readonly Func<int> _foo;
            private readonly Func<IBar> _bar;

            public Baz(Func<int> foo, Func<IBar> bar)
            {
                _foo = foo;
                _bar = bar;
            }

            public int GetFoo() => _foo();
            public IBar GetBar() => _bar();
        }

        [Fact]
        public void MissingConstructorParametersAreSuppliedByTheResolver()
        {
            var config = new ConfigurationBuilder().Build(); // empty config!

            var bar = new Bar();
            var resolver = new Resolver(t => bar, t => t == typeof(IBar));

            var foo = config.Create<Foo>(resolver: resolver)!;

            Assert.Same(bar, foo.Bar);
        }

        public class Foo
        {
            public Foo(IBar bar) => Bar = bar;
            public IBar Bar { get; }
        }

#pragma warning disable CA1040 // Avoid empty interfaces
        public interface IBar { }
#pragma warning restore CA1040 // Avoid empty interfaces

        public class Bar : IBar { }

        public class AnotherBar : IBar
        {
            public AnotherBar(string garply) => Garply = garply;
            public string Garply { get; }
        }

        public class PascalCase
        {
            public string? ThingOne { get; set; }
            public string? ThingTwo { get; set; }
            public string? ThingThree { get; set; }
            public string? ThingFour { get; set; }
            public string? ThingFive { get; set; }
        }

        public class camelCase
        {
            public string? thingOne { get; set; }
            public string? thingTwo { get; set; }
            public string? thingThree { get; set; }
            public string? thingFour { get; set; }
            public string? thingFive { get; set; }
        }

#pragma warning disable CA1707 // Identifiers should not contain underscores
        public class Snake_Case
        {
            public string? Thing_One { get; set; }
            public string? Thing_Two { get; set; }
            public string? Thing_Three { get; set; }
            public string? Thing_Four { get; set; }
            public string? Thing_Five { get; set; }
        }
#pragma warning restore CA1707 // Identifiers should not contain underscores

#if NET8_0_OR_GREATER
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#endif
        public class nocase
#if NET8_0_OR_GREATER
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#endif
        {
            public string? thingone { get; set; }
            public string? thingtwo { get; set; }
            public string? thingthree { get; set; }
            public string? thingfour { get; set; }
            public string? thingfive { get; set; }
        }

        public class UPPERCaseWORDS
        {
            public string? THINGOne { get; set; }
            public string? ThingTWO { get; set; }
#pragma warning disable CA1707 // Identifiers should not contain underscores
            public string? Thing_Three { get; set; }
            public string? Thing_Four { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores
        }

        [Fact]
        public void CanMixAndMatchIdentifierCasing()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:ThingOne", "a" },    // Pascal
                    { "foo:thingTwo", "b" },    // Camel
                    { "foo:thing-three", "c" }, // Kebab
                    { "foo:thing_four", "d" },  // Snake
                    { "foo:thingfive", "e" },   // None
                })
                .Build();

            var fooSection = config.GetSection("foo");

            var pascal = fooSection.Create<PascalCase>()!;

            Assert.Equal("a", pascal.ThingOne);
            Assert.Equal("b", pascal.ThingTwo);
            Assert.Equal("c", pascal.ThingThree);
            Assert.Equal("d", pascal.ThingFour);
            Assert.Equal("e", pascal.ThingFive);

            var camel = fooSection.Create<camelCase>()!;

            Assert.Equal("a", camel.thingOne);
            Assert.Equal("b", camel.thingTwo);
            Assert.Equal("c", camel.thingThree);
            Assert.Equal("d", camel.thingFour);
            Assert.Equal("e", camel.thingFive);

            var snake = fooSection.Create<Snake_Case>()!;

            Assert.Equal("a", snake.Thing_One);
            Assert.Equal("b", snake.Thing_Two);
            Assert.Equal("c", snake.Thing_Three);
            Assert.Equal("d", snake.Thing_Four);
            Assert.Equal("e", snake.Thing_Five);

            var none = fooSection.Create<nocase>()!;

            Assert.Equal("a", none.thingone);
            Assert.Equal("b", none.thingtwo);
            Assert.Equal("c", none.thingthree);
            Assert.Equal("d", none.thingfour);
            Assert.Equal("e", none.thingfive);
        }

        [Fact]
        public void CanHandleUPPERCasedWORDS()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:thing-one", "a" },
                    { "foo:thing-two", "b" },
                    { "foo:thingTHREE", "c" },
                    { "foo:THINGFour", "d" },
                })
                .Build();

            var fooSection = config.GetSection("foo");

            var pascal = fooSection.Create<UPPERCaseWORDS>()!;

            Assert.Equal("a", pascal.THINGOne);
            Assert.Equal("b", pascal.ThingTWO);
            Assert.Equal("c", pascal.Thing_Three);
            Assert.Equal("d", pascal.Thing_Four);
        }

        [Fact]
        public void CanSpecifyConvertMethodWithConvertMethodAttribute()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "123.45" },
                    { "foo:baz:0", "234.56" },
                    { "foo:baz:1", "345.67" },
                    { "foo:qux:fred", "456.78" },
                    { "foo:qux:waldo", "567.89" },
                }).Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasMembersDecoratedWithValueConverterAttribute>()!;

            Assert.Equal(123.45 * 2, foo.Bar);

            Assert.Equal(2, foo.Baz!.Count());
            Assert.Equal(234.56 * 3, foo.Baz!.First());
            Assert.Equal(345.67 * 3, foo.Baz!.Skip(1).First());

            Assert.Equal(2, foo.Qux.Count);
            Assert.True(foo.Qux.ContainsKey("fred"));
            Assert.True(foo.Qux.ContainsKey("waldo"));
            Assert.Equal(456.78 * 5, foo.Qux["fred"].Value);
            Assert.Equal(567.89 * 5, foo.Qux["waldo"].Value);
        }

        [Fact]
        public void CanSpecifyConvertMethodWithLocallyDefinedConvertMethodAttribute()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "123.45" },
                    { "foo:baz:0", "234.56" },
                    { "foo:baz:1", "345.67" },
                    { "foo:qux:fred", "456.78" },
                    { "foo:qux:waldo", "567.89" },
                }).Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasMembersDecoratedWithLocallyDefinedValueConverterAttribute>()!;

            Assert.Equal(123.45 * 7, foo.Bar);

            Assert.Equal(2, foo.Baz!.Count());
            Assert.Equal(234.56 * 11, foo.Baz!.First());
            Assert.Equal(345.67 * 11, foo.Baz!.Skip(1).First());

            Assert.Equal(2, foo.Qux.Count);
            Assert.True(foo.Qux.ContainsKey("fred"));
            Assert.True(foo.Qux.ContainsKey("waldo"));
            Assert.Equal(456.78 * 13, foo.Qux["fred"].Value);
            Assert.Equal(567.89 * 13, foo.Qux["waldo"].Value);
        }

        [Fact]
        public void CanSpecifyDefaultTypesWithDefaultTypeAttribute()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:garplyBar:fred", "123.45" },
                    { "foo:garplyBarArray:0:fred", "123.45" },
                    { "foo:garplyBarList:0:fred", "123.45" },
                    { "foo:garplyBarDictionary:spam:fred", "123.45" },
                    { "foo:graultBar:fred", "123.45" },
                    { "foo:graultBarArray:0:fred", "123.45" },
                    { "foo:graultBarList:0:fred", "123.45" },
                    { "foo:graultBarDictionary:spam:fred", "123.45" },
                    { "foo:barReadonlyList:0:fred", "123.45" },
                    { "foo:barReadonlyDictionary:spam:fred", "123.45" },
                    { "foo:baz:waldo", "-456.78" },
                    { "foo:bazArray:0:waldo", "-456.78" },
                    { "foo:bazList:0:waldo", "-456.78" },
                    { "foo:bazDictionary:spam:waldo", "-456.78" },
                    { "foo:bazReadonlyList:0:waldo", "-456.78" },
                    { "foo:bazReadonlyDictionary:spam:waldo", "-456.78" },
                    { "foo:qux:thud", "987.65" },
                    { "foo:quxArray:0:thud", "987.65" },
                    { "foo:quxList:0:thud", "987.65" },
                    { "foo:quxDictionary:spam:thud", "987.65" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasMembersDecoratedWithDefaultTypeAttribute>()!;

            Assert.IsType<DefaultHasDefaultType>(foo.GarplyBar);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GarplyBar!).Fred);
            Assert.Single(foo.GarplyBarArray!);
            Assert.IsType<DefaultHasDefaultType>(foo.GarplyBarArray![0]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GarplyBarArray[0]).Fred);
            Assert.Single(foo.GarplyBarList!);
            Assert.IsType<DefaultHasDefaultType>(foo.GarplyBarList![0]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GarplyBarList[0]).Fred);
            Assert.Single(foo.GarplyBarDictionary!);
            Assert.IsType<DefaultHasDefaultType>(foo.GarplyBarDictionary!["spam"]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GarplyBarDictionary["spam"]).Fred);

            Assert.IsType<DefaultHasDefaultType>(foo.GraultBar);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GraultBar).Fred);
            Assert.Single(foo.GraultBarArray);
            Assert.IsType<DefaultHasDefaultType>(foo.GraultBarArray[0]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GraultBarArray[0]).Fred);
            Assert.Single(foo.GraultBarList);
            Assert.IsType<DefaultHasDefaultType>(foo.GraultBarList[0]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GraultBarList[0]).Fred);
            Assert.Single(foo.GraultBarDictionary);
            Assert.IsType<DefaultHasDefaultType>(foo.GraultBarDictionary["spam"]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GraultBarDictionary["spam"]).Fred);

            Assert.Single(foo.BarReadonlyList);
            Assert.IsType<DefaultHasDefaultType>(foo.BarReadonlyList[0]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.BarReadonlyList[0]).Fred);
            Assert.Single(foo.BarReadonlyDictionary);
            Assert.IsType<DefaultHasDefaultType>(foo.BarReadonlyDictionary["spam"]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.BarReadonlyDictionary["spam"]).Fred);

            Assert.IsType<DefaultHasNoDefaultType>(foo.Baz);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.Baz!).Waldo);
            Assert.Single(foo.BazArray!);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazArray![0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazArray[0]).Waldo);
            Assert.Single(foo.BazList!);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazList![0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazList[0]).Waldo);
            Assert.Single(foo.BazDictionary!);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazDictionary!["spam"]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazDictionary["spam"]).Waldo);

            Assert.Single(foo.BazReadonlyList!);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazReadonlyList![0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazReadonlyList[0]).Waldo);
            Assert.Single(foo.BazReadonlyDictionary);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazReadonlyDictionary["spam"]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazReadonlyDictionary["spam"]).Waldo);

            Assert.IsType<DefaultIAlsoHasNoDefaultType>(foo.Qux);
            Assert.Equal(987.65, ((DefaultIAlsoHasNoDefaultType)foo.Qux).Thud);
            Assert.Single(foo.QuxArray);
            Assert.IsType<DefaultIAlsoHasNoDefaultType>(foo.QuxArray[0]);
            Assert.Equal(987.65, ((DefaultIAlsoHasNoDefaultType)foo.QuxArray[0]).Thud);
            Assert.Single(foo.QuxList);
            Assert.IsType<DefaultIAlsoHasNoDefaultType>(foo.QuxList[0]);
            Assert.Equal(987.65, ((DefaultIAlsoHasNoDefaultType)foo.QuxList[0]).Thud);
            Assert.Single(foo.QuxDictionary);
            Assert.IsType<DefaultIAlsoHasNoDefaultType>(foo.QuxDictionary["spam"]);
            Assert.Equal(987.65, ((DefaultIAlsoHasNoDefaultType)foo.QuxDictionary["spam"]).Thud);
        }

        [Fact]
        public void CanSpecifyDefaultTypesWithLocallyDefinedDefaultTypeAttribute()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:garplyBar:fred", "123.45" },
                    { "foo:garplyBarArray:0:fred", "123.45" },
                    { "foo:garplyBarList:0:fred", "123.45" },
                    { "foo:garplyBarDictionary:spam:fred", "123.45" },
                    { "foo:graultBar:fred", "123.45" },
                    { "foo:graultBarArray:0:fred", "123.45" },
                    { "foo:graultBarList:0:fred", "123.45" },
                    { "foo:graultBarDictionary:spam:fred", "123.45" },
                    { "foo:barReadonlyList:0:fred", "123.45" },
                    { "foo:barReadonlyDictionary:spam:fred", "123.45" },
                    { "foo:baz:waldo", "-456.78" },
                    { "foo:bazArray:0:waldo", "-456.78" },
                    { "foo:bazList:0:waldo", "-456.78" },
                    { "foo:bazDictionary:spam:waldo", "-456.78" },
                    { "foo:bazReadonlyList:0:waldo", "-456.78" },
                    { "foo:bazReadonlyDictionary:spam:waldo", "-456.78" },
                    { "foo:qux:thud", "987.65" },
                    { "foo:quxArray:0:thud", "987.65" },
                    { "foo:quxList:0:thud", "987.65" },
                    { "foo:quxDictionary:spam:thud", "987.65" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<MembersDecoratedWithLocallyDefinedDefaultTypeAttribute>()!;

            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GarplyBar);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GarplyBar!).Fred);
            Assert.Single(foo.GarplyBarArray!);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GarplyBarArray![0]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GarplyBarArray[0]).Fred);
            Assert.Single(foo.GarplyBarList!);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GarplyBarList![0]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GarplyBarList[0]).Fred);
            Assert.Single(foo.GarplyBarDictionary!);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GarplyBarDictionary!["spam"]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GarplyBarDictionary["spam"]).Fred);

            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GraultBar);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GraultBar).Fred);
            Assert.Single(foo.GraultBarArray);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GraultBarArray[0]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GraultBarArray[0]).Fred);
            Assert.Single(foo.GraultBarList);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GraultBarList[0]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GraultBarList[0]).Fred);
            Assert.Single(foo.GraultBarDictionary);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GraultBarDictionary["spam"]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GraultBarDictionary["spam"]).Fred);

            Assert.Single(foo.BarReadonlyList);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.BarReadonlyList[0]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.BarReadonlyList[0]).Fred);
            Assert.Single(foo.BarReadonlyDictionary);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.BarReadonlyDictionary["spam"]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.BarReadonlyDictionary["spam"]).Fred);

            Assert.IsType<DefaultHasNoDefaultType>(foo.Baz);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.Baz!).Waldo);
            Assert.Single(foo.BazArray!);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazArray![0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazArray[0]).Waldo);
            Assert.Single(foo.BazList!);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazList![0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazList[0]).Waldo);
            Assert.Single(foo.BazDictionary!);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazDictionary!["spam"]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazDictionary["spam"]).Waldo);

            Assert.Single(foo.BazReadonlyList);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazReadonlyList[0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazReadonlyList[0]).Waldo);
            Assert.Single(foo.BazReadonlyDictionary);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazReadonlyDictionary["spam"]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazReadonlyDictionary["spam"]).Waldo);

            Assert.IsType<DefaultIAlsoHasNoDefaultType>(foo.Qux);
            Assert.Equal(987.65, ((DefaultIAlsoHasNoDefaultType)foo.Qux).Thud);
            Assert.Single(foo.QuxArray);
            Assert.IsType<DefaultIAlsoHasNoDefaultType>(foo.QuxArray[0]);
            Assert.Equal(987.65, ((DefaultIAlsoHasNoDefaultType)foo.QuxArray[0]).Thud);
            Assert.Single(foo.QuxList);
            Assert.IsType<DefaultIAlsoHasNoDefaultType>(foo.QuxList[0]);
            Assert.Equal(987.65, ((DefaultIAlsoHasNoDefaultType)foo.QuxList[0]).Thud);
            Assert.Single(foo.QuxDictionary);
            Assert.IsType<DefaultIAlsoHasNoDefaultType>(foo.QuxDictionary["spam"]);
            Assert.Equal(987.65, ((DefaultIAlsoHasNoDefaultType)foo.QuxDictionary["spam"]).Thud);
        }

        [Fact]
        public void PassingDefaultTypesOverridesNonTypeSpecifiedMember()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:qux", "true" },
                    { "foo:bar:garply", "123.45" },
                    { "foo:bar:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = new DefaultTypes { { typeof(HasReadWriteConcreteProperties), "bar", typeof(InheritedHasReadWriteProperties) } };
            var foo = fooSection.Create<HasReadWriteConcreteProperties>(defaultTypes: defaultTypes)!;

            Assert.True(foo.Bar!.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedHasReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedHasReadWriteProperties)foo.Bar;
            Assert.Equal("But I don't LIKE Spam!", inheritedBar.Spam);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void PassingDefaultTypesOverridesNonTypeSpecifiedMembersOfTheTargetType()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:qux", "true" },
                    { "foo:bar:garply", "123.45" },
                    { "foo:bar:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = new DefaultTypes { { typeof(HasReadWriteProperties), typeof(InheritedHasReadWriteProperties) } };
            var foo = fooSection.Create<HasReadWriteConcreteProperties>(defaultTypes: defaultTypes)!;

            Assert.True(foo.Bar!.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedHasReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedHasReadWriteProperties)foo.Bar;
            Assert.Equal("But I don't LIKE Spam!", inheritedBar.Spam);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void PassingDefaultTypesDoesNotOverrideTypeSpecifiedMember()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(HasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = new DefaultTypes { { typeof(HasReadWriteConcreteProperties), "bar", typeof(InheritedHasReadWriteProperties) } };
            var foo = fooSection.Create<HasReadWriteConcreteProperties>(defaultTypes: defaultTypes)!;

            Assert.True(foo.Bar!.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<HasReadWriteProperties>(foo.Bar);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void PassingDefaultTypesDoesNotOverrideTypeSpecifiedMembersOfTheTargetType()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(HasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = new DefaultTypes { { typeof(HasReadWriteProperties), typeof(InheritedHasReadWriteProperties) } };
            var foo = fooSection.Create<HasReadWriteConcreteProperties>(defaultTypes: defaultTypes)!;

            Assert.True(foo.Bar!.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<HasReadWriteProperties>(foo.Bar);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void PassingValueConvertersOverridesDefaultConversionWhenThereIsAMatch()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string?>
               {
                    { "foo:bar", "123.45" },
               })
               .Build();

            var fooSection = config.GetSection("foo");

            var valueConverters = new ValueConverters()
                .Add(typeof(double), value => double.Parse(value, CultureInfo.InvariantCulture) * 2);

            var foo = fooSection.Create<DoubleContainer>(valueConverters: valueConverters)!;

            Assert.Equal(123.45 * 2, foo.Bar); // Doubled by the custom converter
        }

        [Fact]
        public void TheValueConverterRegisteredByDeclaringTypeAndMemberNameHasPriorityOverTheValueConverterRegisteredByTargetType()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string?>
               {
                    { "foo:bar", "123.45" },
               })
               .Build();

            var fooSection = config.GetSection("foo");

            var valueConverters = new ValueConverters()
                .Add(typeof(double), value => double.Parse(value, CultureInfo.InvariantCulture) * 2)
                .Add(typeof(DoubleContainer), "bar", value => double.Parse(value, CultureInfo.InvariantCulture) * 3);

            var foo = fooSection.Create<DoubleContainer>(valueConverters: valueConverters)!;

            // Tripled by the custom converter registered to the declaring type and member name
            Assert.Equal(123.45 * 3, foo.Bar);

            var anotherFoo = fooSection.Create<AnotherDoubleContainer>(valueConverters: valueConverters)!;

            // Still matches by target type for other doubles
            Assert.Equal(123.45 * 2, anotherFoo.Bar);
        }

        [Fact]
        public void PassingValueConvertersDoesNotOverrideDefaultConversionWhenThereIsNotAMatch()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string?>
               {
                    { "foo:bar", "123.45" },
               })
               .Build();

            var fooSection = config.GetSection("foo");

            var valueConverters = new ValueConverters()
                .Add(typeof(AnotherDoubleContainer), "bar", value => double.Parse(value, CultureInfo.InvariantCulture) * 2);

            var foo = fooSection.Create<DoubleContainer>(valueConverters: valueConverters)!;

            // Not doubled by the custom converter because the declaring types were different.
            Assert.Equal(123.45, foo.Bar);
        }

        [Fact]
        public void TypeSpecificationIsCaseInsensitive()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(HasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:Type", typeof(AlsoHasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:baz:Value:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteInterfaceProperties>()!;

            Assert.IsType<HasReadWriteProperties>(foo.Bar);
            Assert.True(foo.Bar!.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<AlsoHasReadWriteProperties>(foo.Baz);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void CanBindToReadWriteSimpleProperties()
        {
            var now = DateTime.Now;
            var quxType = GetType();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "123" },
                    { "foo:baz", now.ToString("O") },
                    { "foo:qux", quxType.AssemblyQualifiedName! },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteSimpleProperties>()!;

            Assert.Equal(123, foo.Bar);
            Assert.Equal(now, foo.Baz);
            Assert.Equal(quxType, foo.Qux);
        }

        [Fact]
        public void CanBindToSimpleConstructorParameters()
        {
            var now = DateTime.Now;
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "123" },
                    { "foo:baz", now.ToString("O") },
                    { "foo:qux", "true" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleConstructorParameters>()!;

            Assert.Equal(123, foo.Bar);
            Assert.Equal(now, foo.Baz);
            Assert.True(foo.Qux);
        }

        [Fact]
        public void CanBindToOptionalNullableConstructorParameters()
        {
            var config = new ConfigurationBuilder().Build();

            var foo = config.Create<HasOptionalNullableConstructorParameters>()!;

            Assert.Equal(5, foo.Bar);
            Assert.Equal(10.2m, foo.Baz);
            Assert.Equal(ExampleValues.Grault, foo.Flag);
            Assert.Null(foo.Flag2);
        }

        [Fact]
        public void CanBindToReadWriteConcreteProperties()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:qux", "true" },
                    { "foo:bar:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>()!;

            Assert.True(foo.Bar!.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void CanBindToConcreteConstructorParameters()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:qux", "true" },
                    { "foo:bar:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>()!;

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToInheritorOfTypeWithConcreteReadWriteProperties()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(InheritedHasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:bar:value:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>()!;

            Assert.True(foo.Bar!.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedHasReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedHasReadWriteProperties)foo.Bar;
            Assert.Equal("But I don't LIKE Spam!", inheritedBar.Spam);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void CanBindToInheritorOfTypeWithConcreteConstructorParameters()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(InheritedHasConstructorParameters).AssemblyQualifiedName! },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:bar:value:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>()!;

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedHasConstructorParameters>(foo.Bar);
            var inheritedBar = (InheritedHasConstructorParameters)foo.Bar;
            Assert.Equal("But I don't LIKE Spam!", inheritedBar.Spam);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWriteInterfaceProperties()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(HasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:type", typeof(AlsoHasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:baz:value:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteInterfaceProperties>()!;

            Assert.IsType<HasReadWriteProperties>(foo.Bar);
            Assert.True(foo.Bar!.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<AlsoHasReadWriteProperties>(foo.Baz);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void CanBindToInterfaceConstructorParameters()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(HasConstructorParameters).AssemblyQualifiedName! },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:type", typeof(AlsoHasConstructorParameters).AssemblyQualifiedName! },
                    { "foo:baz:value:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceConstructorParameters>()!;

            Assert.IsType<HasConstructorParameters>(foo.Bar);
            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<AlsoHasConstructorParameters>(foo.Baz);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWritePropertyWithoutSpecifyingTheValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(InheritedHasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>()!;

            Assert.False(foo.Bar!.Qux);
            Assert.Equal(0D, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void CanBindToConstructorParameterWithoutSpecifyingTheValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(InheritedHasDefaultConstructor).AssemblyQualifiedName! },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>()!;

            Assert.True(foo.Bar.Qux);
            Assert.Equal(543.21, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWritePropertyWithANullValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(InheritedHasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:bar:value", null! },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>()!;

            Assert.False(foo.Bar!.Qux);
            Assert.Equal(0D, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void CanBindToConstructorParameterWithANullValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(InheritedHasDefaultConstructor).AssemblyQualifiedName! },
                    { "foo:bar:value", null! },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>()!;

            Assert.True(foo.Bar.Qux);
            Assert.Equal(543.21, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWritePropertyWithAnEmptyValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(InheritedHasReadWriteProperties).AssemblyQualifiedName! },
                    { "foo:bar:value", "" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>()!;

            Assert.False(foo.Bar!.Qux);
            Assert.Equal(0D, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz!.Grault);
        }

        [Fact]
        public void CanBindToConstructorParameterWithAnEmptyValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(InheritedHasDefaultConstructor).AssemblyQualifiedName! },
                    { "foo:bar:value", "" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>()!;

            Assert.True(foo.Bar.Qux);
            Assert.Equal(543.21, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWriteSimpleCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:0", "123" },
                    { "foo:bar:1", "456" },
                    { "foo:baz:0", "123" },
                    { "foo:baz:1", "456" },
                    { "foo:qux:0", "123" },
                    { "foo:qux:1", "456" },
                    { "foo:garply:0", "123" },
                    { "foo:garply:1", "456" },
                    { "foo:grault:0", "123" },
                    { "foo:grault:1", "456" },
                    { "foo:fred:0", "123" },
                    { "foo:fred:1", "456" },
                    { "foo:waldo:0", "123" },
                    { "foo:waldo:1", "456" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleReadWriteCollectionProperties>()!;

            Assert.Equal(2, foo.Bar!.Length);
            Assert.Equal(123, foo.Bar[0]);
            Assert.Equal(456, foo.Bar[1]);
            Assert.Equal(2, foo.Baz!.Count);
            Assert.Equal(123, foo.Baz[0]);
            Assert.Equal(456, foo.Baz[1]);
            Assert.Equal(2, foo.Qux!.Count);
            Assert.Equal(123, foo.Qux[0]);
            Assert.Equal(456, foo.Qux[1]);
            Assert.Equal(2, foo.Garply!.Count);
            Assert.Equal(123, foo.Garply.First());
            Assert.Equal(456, foo.Garply.Skip(1).First());
            Assert.Equal(2, foo.Grault!.Count());
            Assert.Equal(123, foo.Grault!.First());
            Assert.Equal(456, foo.Grault!.Skip(1).First());
            Assert.Equal(2, foo.Fred!.Count);
            Assert.Equal(123, foo.Fred.First());
            Assert.Equal(456, foo.Fred.Skip(1).First());
            Assert.Equal(2, foo.Waldo!.Count);
            Assert.Equal(123, foo.Waldo[0]);
            Assert.Equal(456, foo.Waldo[1]);
        }

        [Fact]
        public void CanBindToReadWriteSimpleCollectionPropertiesWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "123" },
                    { "foo:baz", "123" },
                    { "foo:qux", "123" },
                    { "foo:garply", "123" },
                    { "foo:grault", "123" },
                    { "foo:fred", "123" },
                    { "foo:waldo", "123" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleReadWriteCollectionProperties>()!;

            Assert.Single(foo.Bar!);
            Assert.Equal(123, foo.Bar![0]);
            Assert.Single(foo.Baz!);
            Assert.Equal(123, foo.Baz![0]);
            Assert.Single(foo.Qux!);
            Assert.Equal(123, foo.Qux![0]);
            Assert.Single(foo.Garply!);
            Assert.Equal(123, foo.Garply!.First());
            Assert.Single(foo.Grault!);
            Assert.Equal(123, foo.Grault!.First());
            Assert.Single(foo.Fred!);
            Assert.Equal(123, foo.Fred!.First());
            Assert.Single(foo.Waldo!);
            Assert.Equal(123, foo.Waldo![0]);
        }

        [Fact]
        public void CanBindToReadonlySimpleCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:baz:0", "123" },
                    { "foo:baz:1", "456" },
                    { "foo:qux:0", "123" },
                    { "foo:qux:1", "456" },
                    { "foo:garply:0", "123" },
                    { "foo:garply:1", "456" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleReadonlyCollectionProperties>()!;

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(123, foo.Baz[0]);
            Assert.Equal(456, foo.Baz[1]);
            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(123, foo.Qux[0]);
            Assert.Equal(456, foo.Qux[1]);
            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(123, foo.Garply.First());
            Assert.Equal(456, foo.Garply.Skip(1).First());
        }

        [Fact]
        public void CanBindToReadonlySimpleCollectionPropertiesWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:baz", "123" },
                    { "foo:qux", "123" },
                    { "foo:garply", "123" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleReadonlyCollectionProperties>()!;

            Assert.Single(foo.Baz);
            Assert.Equal(123, foo.Baz[0]);
            Assert.Single(foo.Qux);
            Assert.Equal(123, foo.Qux[0]);
            Assert.Single(foo.Garply);
            Assert.Equal(123, foo.Garply.First());
        }

        [Fact]
        public void CanBindToSimpleCollectionConstructorParameters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:0", "123" },
                    { "foo:bar:1", "456" },
                    { "foo:baz:0", "123" },
                    { "foo:baz:1", "456" },
                    { "foo:qux:0", "123" },
                    { "foo:qux:1", "456" },
                    { "foo:garply:0", "123" },
                    { "foo:garply:1", "456" },
                    { "foo:grault:0", "123" },
                    { "foo:grault:1", "456" },
                    { "foo:fred:0", "123" },
                    { "foo:fred:1", "456" },
                    { "foo:waldo:0", "123" },
                    { "foo:waldo:1", "456" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleCollectionConstructorParameters>()!;

            Assert.Equal(2, foo.Bar.Length);
            Assert.Equal(123, foo.Bar[0]);
            Assert.Equal(456, foo.Bar[1]);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(123, foo.Baz[0]);
            Assert.Equal(456, foo.Baz[1]);
            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(123, foo.Qux[0]);
            Assert.Equal(456, foo.Qux[1]);
            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(123, foo.Garply.First());
            Assert.Equal(456, foo.Garply.Skip(1).First());
            Assert.Equal(2, foo.Grault.Count());
            Assert.Equal(123, foo.Grault.First());
            Assert.Equal(456, foo.Grault.Skip(1).First());
            Assert.Equal(2, foo.Fred.Count);
            Assert.Equal(123, foo.Fred.First());
            Assert.Equal(456, foo.Fred.Skip(1).First());
            Assert.Equal(2, foo.Waldo.Count);
            Assert.Equal(123, foo.Waldo[0]);
            Assert.Equal(456, foo.Waldo[1]);
        }

        [Fact]
        public void CanBindToByteCollectionConstructorParameters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "QmFy" },
                    { "foo:baz", "QmF6" },
                    { "foo:qux", "UXV6" },
                    { "foo:garply", "R2FycGx5" },
                    { "foo:grault", "R3JhdWx0" },
                    { "foo:fred", "RnJlZA==" },
                    { "foo:waldo", "V2FsZG8=" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasByteCollectionConstructorParameters>()!;

            Assert.Equal("QmFy", foo.Bar);
            Assert.Equal("QmF6", foo.Baz);
            Assert.Equal("UXV6", foo.Qux);
            Assert.Equal("R2FycGx5", foo.Garply);
            Assert.Equal("R3JhdWx0", foo.Grault);
            Assert.Equal("RnJlZA==", foo.Fred);
            Assert.Equal("V2FsZG8=", foo.Waldo);
        }

        [Fact]
        public void CanBindToSimpleCollectionConstructorParametersWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "123" },
                    { "foo:baz", "123" },
                    { "foo:qux", "123" },
                    { "foo:garply", "123" },
                    { "foo:grault", "123" },
                    { "foo:fred", "123" },
                    { "foo:waldo", "123" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleCollectionConstructorParameters>()!;

            Assert.Single(foo.Bar);
            Assert.Equal(123, foo.Bar[0]);
            Assert.Single(foo.Baz);
            Assert.Equal(123, foo.Baz[0]);
            Assert.Single(foo.Qux);
            Assert.Equal(123, foo.Qux[0]);
            Assert.Single(foo.Garply);
            Assert.Equal(123, foo.Garply.First());
            Assert.Single(foo.Grault);
            Assert.Equal(123, foo.Grault.First());
            Assert.Single(foo.Fred);
            Assert.Equal(123, foo.Fred.First());
            Assert.Single(foo.Waldo);
            Assert.Equal(123, foo.Waldo[0]);
        }

        [Fact]
        public void CanBindToReadWriteConcreteCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:0:baz", "utf-8" },
                    { "foo:bar:1:baz", "ascii" },
                    { "foo:baz:0:baz", "utf-8" },
                    { "foo:baz:1:baz", "ascii" },
                    { "foo:qux:0:baz", "utf-8" },
                    { "foo:qux:1:baz", "ascii" },
                    { "foo:garply:0:baz", "utf-8" },
                    { "foo:garply:1:baz", "ascii" },
                    { "foo:grault:0:baz", "utf-8" },
                    { "foo:grault:1:baz", "ascii" },
                    { "foo:fred:0:baz", "utf-8" },
                    { "foo:fred:1:baz", "ascii" },
                    { "foo:waldo:0:baz", "utf-8" },
                    { "foo:waldo:1:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteReadWriteCollectionProperties>()!;

            Assert.Equal(2, foo.Bar!.Length);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Bar[1].Baz);
            Assert.Equal(2, foo.Baz!.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.Equal(2, foo.Qux!.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.Equal(2, foo.Garply!.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
            Assert.Equal(2, foo.Grault!.Count());
            Assert.Equal(Encoding.UTF8, foo.Grault!.First().Baz);
            Assert.Equal(Encoding.ASCII, foo.Grault!.Skip(1).First().Baz);
            Assert.Equal(2, foo.Fred!.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.Equal(Encoding.ASCII, foo.Fred.Skip(1).First().Baz);
            Assert.Equal(2, foo.Waldo!.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Waldo[1].Baz);
        }

        [Fact]
        public void CanBindToByteCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "QmFy" },
                    { "foo:baz", "QmF6" },
                    { "foo:qux", "UXV6" },
                    { "foo:garply", "R2FycGx5" },
                    { "foo:grault", "R3JhdWx0" },
                    { "foo:fred", "RnJlZA==" },
                    { "foo:waldo", "V2FsZG8=" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasByteCollectionProperties>()!;

            Assert.Equal("QmFy", Convert.ToBase64String(foo.Bar!));
            Assert.Equal("QmF6", Convert.ToBase64String(foo.Baz!.ToArray()));
            Assert.Equal("UXV6", Convert.ToBase64String(foo.Qux!.ToArray()));
            Assert.Equal("R2FycGx5", Convert.ToBase64String(foo.Garply!.ToArray()));
            Assert.Equal("R3JhdWx0", Convert.ToBase64String(foo.Grault!.ToArray()));
            Assert.Equal("RnJlZA==", Convert.ToBase64String(foo.Fred!.ToArray()));
            Assert.Equal("V2FsZG8=", Convert.ToBase64String(foo.Waldo!.ToArray()));
        }

        [Fact]
        public void CanBindToReadWriteConcreteCollectionPropertiesWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:baz", "utf-8" },
                    { "foo:baz:baz", "utf-8" },
                    { "foo:qux:baz", "utf-8" },
                    { "foo:garply:baz", "utf-8" },
                    { "foo:grault:baz", "utf-8" },
                    { "foo:fred:baz", "utf-8" },
                    { "foo:waldo:baz", "utf-8" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteReadWriteCollectionProperties>()!;

            Assert.Single(foo.Bar!);
            Assert.Equal(Encoding.UTF8, foo.Bar![0].Baz);
            Assert.Single(foo.Baz!);
            Assert.Equal(Encoding.UTF8, foo.Baz![0].Baz);
            Assert.Single(foo.Qux!);
            Assert.Equal(Encoding.UTF8, foo.Qux![0].Baz);
            Assert.Single(foo.Garply!);
            Assert.Equal(Encoding.UTF8, foo.Garply!.First().Baz);
            Assert.Single(foo.Grault!);
            Assert.Equal(Encoding.UTF8, foo.Grault!.First().Baz);
            Assert.Single(foo.Fred!);
            Assert.Equal(Encoding.UTF8, foo.Fred!.First().Baz);
            Assert.Single(foo.Waldo!);
            Assert.Equal(Encoding.UTF8, foo.Waldo![0].Baz);
        }

        [Fact]
        public void CanBindToReadonlyConcreteCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:baz:0:baz", "utf-8" },
                    { "foo:baz:1:baz", "ascii" },
                    { "foo:qux:0:baz", "utf-8" },
                    { "foo:qux:1:baz", "ascii" },
                    { "foo:garply:0:baz", "utf-8" },
                    { "foo:garply:1:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteReadonlyCollectionProperties>()!;

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
        }

        [Fact]
        public void CanBindToNonGenericListProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:0:baz", "utf-8" },
                    { "foo:bar:1:baz", "ascii" },
                    { "foo:baz:0:baz", "utf-8" },
                    { "foo:baz:1:baz", "ascii" },
                    { "foo:qux:0:baz", "utf-8" },
                    { "foo:qux:1:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasNonGenericCollectionProperties>()!;

            Assert.Equal(2, foo.Bar!.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Bar[1].Baz);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
        }

        [Fact]
        public void ReadonlyListPropertiesAreClearedBeforeAddingToThem()
        {
            // Verify that the object starts out with initial items.
            var defaultFoo = new HasReadonlyListPropertiesWithInitialItems();

            Assert.Single(defaultFoo.Bar);
            Assert.Equal(Encoding.ASCII, defaultFoo.Bar[0].Baz);
            Assert.Single(defaultFoo.Baz);
            Assert.Equal(Encoding.ASCII, defaultFoo.Baz[0].Baz);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:0:baz", "utf-8" },
                    { "foo:bar:1:baz", "utf-32" },
                    { "foo:baz:0:baz", "utf-8" },
                    { "foo:baz:1:baz", "utf-32" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadonlyListPropertiesWithInitialItems>()!;

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.Equal(Encoding.UTF32, foo.Bar[1].Baz);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Equal(Encoding.UTF32, foo.Baz[1].Baz);
        }

        [Fact]
        public void CanMapReadOnlyPropertiesOfTypeNonGenericIListImplementationWithoutDefaultConstructor()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "name", "source1" },
                    { "listeners:0:name", "listener1" },
                    { "listeners:1:name", "listener2" },
                })
                .Build();

            // The TraceSource.Listeners property is readonly, and the TraceListenerCollection class
            // does not have a default constructor, so this is a good example class to use for testing.

            var defaultTypes = new DefaultTypes { { typeof(TraceListener), typeof(DefaultTraceListener) } };

            var traceSource = config.Create<TraceSource>(defaultTypes)!;

            Assert.Equal("source1", traceSource.Name);
            Assert.Equal(2, traceSource.Listeners.Count);
            Assert.Equal("listener1", traceSource.Listeners[0].Name);
            Assert.Equal("listener2", traceSource.Listeners[1].Name);
        }

        [Fact]
        public void FlagsEnumsSupportCSharpAndVisualBasicEnumDelimiters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar", "Garply, Grault, Corge" },
                    { "foo:baz", "Garply | Grault | Corge" },
                    { "foo:qux", "Garply Or Grault Or Corge" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasFlagsEnumProperties>()!;

            Assert.Equal(ExampleValues.Garply | ExampleValues.Grault | ExampleValues.Corge, foo.Bar);
            Assert.Equal(ExampleValues.Garply | ExampleValues.Grault | ExampleValues.Corge, foo.Baz);
            Assert.Equal(ExampleValues.Garply | ExampleValues.Grault | ExampleValues.Corge, foo.Qux);
        }

        [Fact]
        public void CanBindToReadonlyConcreteCollectionPropertiesWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:baz:baz", "utf-8" },
                    { "foo:qux:baz", "utf-8" },
                    { "foo:garply:baz", "utf-8" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteReadonlyCollectionProperties>()!;

            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Single(foo.Qux);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Single(foo.Garply);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
        }

        [Fact]
        public void CanBindToConcreteCollectionConstructorParameters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:0:baz", "utf-8" },
                    { "foo:bar:1:baz", "ascii" },
                    { "foo:baz:0:baz", "utf-8" },
                    { "foo:baz:1:baz", "ascii" },
                    { "foo:qux:0:baz", "utf-8" },
                    { "foo:qux:1:baz", "ascii" },
                    { "foo:garply:0:baz", "utf-8" },
                    { "foo:garply:1:baz", "ascii" },
                    { "foo:grault:0:baz", "utf-8" },
                    { "foo:grault:1:baz", "ascii" },
                    { "foo:fred:0:baz", "utf-8" },
                    { "foo:fred:1:baz", "ascii" },
                    { "foo:waldo:0:baz", "utf-8" },
                    { "foo:waldo:1:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteCollectionConstructorParameters>()!;

            Assert.Equal(2, foo.Bar.Length);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Bar[1].Baz);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
            Assert.Equal(2, foo.Grault.Count());
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.Equal(Encoding.ASCII, foo.Grault.Skip(1).First().Baz);
            Assert.Equal(2, foo.Fred.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.Equal(Encoding.ASCII, foo.Fred.Skip(1).First().Baz);
            Assert.Equal(2, foo.Waldo.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.Equal(Encoding.ASCII, foo.Waldo[1].Baz);
        }

        [Fact]
        public void CanBindToConcreteCollectionConstructorParametersWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:baz", "utf-8" },
                    { "foo:baz:baz", "utf-8" },
                    { "foo:qux:baz", "utf-8" },
                    { "foo:garply:baz", "utf-8" },
                    { "foo:grault:baz", "utf-8" },
                    { "foo:fred:baz", "utf-8" },
                    { "foo:waldo:baz", "utf-8" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteCollectionConstructorParameters>()!;

            Assert.Single(foo.Bar);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Single(foo.Qux);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Single(foo.Garply);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.Single(foo.Grault);
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.Single(foo.Fred);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.Single(foo.Waldo);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
        }

        [Fact]
        public void CanBindToReadWriteInterfaceCollectionProperties()
        {
            TimeSpan quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:bar:0:value:baz", "utf-8" },
                    { "foo:bar:0:value:qux", quxValue.ToString() },
                    { "foo:bar:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:bar:1:value:baz", "ascii" },

                    { "foo:baz:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:garply:1:value:baz", "ascii" },

                    { "foo:grault:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:grault:0:value:baz", "utf-8" },
                    { "foo:grault:0:value:qux", quxValue.ToString() },
                    { "foo:grault:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:grault:1:value:baz", "ascii" },

                    { "foo:fred:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:fred:0:value:baz", "utf-8" },
                    { "foo:fred:0:value:qux", quxValue.ToString() },
                    { "foo:fred:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:fred:1:value:baz", "ascii" },

                    { "foo:waldo:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:waldo:0:value:baz", "utf-8" },
                    { "foo:waldo:0:value:qux", quxValue.ToString() },
                    { "foo:waldo:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:waldo:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadWriteCollectionProperties>()!;

            Assert.Equal(2, foo.Bar!.Length);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar[1].Baz);
            Assert.IsType<HasSomething>(foo.Bar[1]);

            Assert.Equal(2, foo.Baz!.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.IsType<HasSomething>(foo.Baz[1]);

            Assert.Equal(2, foo.Qux!.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.IsType<HasSomething>(foo.Qux[1]);

            Assert.Equal(2, foo.Garply!.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
            Assert.IsType<HasSomething>(foo.Garply.Skip(1).First());

            Assert.Equal(2, foo.Grault!.Count());
            Assert.Equal(Encoding.UTF8, foo.Grault!.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Grault!.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Grault!.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Grault!.Skip(1).First().Baz);
            Assert.IsType<HasSomething>(foo.Grault!.Skip(1).First());

            Assert.Equal(2, foo.Fred!.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Fred.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Fred.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Fred.Skip(1).First().Baz);
            Assert.IsType<HasSomething>(foo.Fred.Skip(1).First());

            Assert.Equal(2, foo.Waldo!.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Waldo[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Waldo[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Waldo[1].Baz);
            Assert.IsType<HasSomething>(foo.Waldo[1]);
        }

        [Fact]
        public void CanBindToReadWriteInterfaceCollectionPropertiesWithSingleNonListItem()
        {
            TimeSpan quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:bar:value:baz", "utf-8" },
                    { "foo:bar:value:qux", quxValue.ToString() },

                    { "foo:baz:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:value:baz", "utf-8" },
                    { "foo:baz:value:qux", quxValue.ToString() },

                    { "foo:qux:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:qux:value:baz", "utf-8" },
                    { "foo:qux:value:qux", quxValue.ToString() },

                    { "foo:garply:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:garply:value:baz", "utf-8" },
                    { "foo:garply:value:qux", quxValue.ToString() },

                    { "foo:grault:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:grault:value:baz", "utf-8" },
                    { "foo:grault:value:qux", quxValue.ToString() },

                    { "foo:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:fred:value:baz", "utf-8" },
                    { "foo:fred:value:qux", quxValue.ToString() },

                    { "foo:waldo:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:waldo:value:baz", "utf-8" },
                    { "foo:waldo:value:qux", quxValue.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadWriteCollectionProperties>()!;

            Assert.Single(foo.Bar!);
            Assert.Equal(Encoding.UTF8, foo.Bar![0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar[0]).Qux);

            Assert.Single(foo.Baz!);
            Assert.Equal(Encoding.UTF8, foo.Baz![0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);

            Assert.Single(foo.Qux!);
            Assert.Equal(Encoding.UTF8, foo.Qux![0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);

            Assert.Single(foo.Garply!);
            Assert.Equal(Encoding.UTF8, foo.Garply!.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply!.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply!.First()).Qux);

            Assert.Single(foo.Grault!);
            Assert.Equal(Encoding.UTF8, foo.Grault!.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Grault!.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Grault!.First()).Qux);

            Assert.Single(foo.Fred!);
            Assert.Equal(Encoding.UTF8, foo.Fred!.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Fred!.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Fred!.First()).Qux);

            Assert.Single(foo.Waldo!);
            Assert.Equal(Encoding.UTF8, foo.Waldo![0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Waldo[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Waldo[0]).Qux);
        }

        [Fact]
        public void CanBindToReadonlyInterfaceCollectionProperties()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:baz:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:garply:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadonlyCollectionProperties>()!;

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.IsType<HasSomething>(foo.Baz[1]);

            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.IsType<HasSomething>(foo.Qux[1]);

            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
            Assert.IsType<HasSomething>(foo.Garply.Skip(1).First());
        }

        [Fact]
        public void CanBindToReadonlyInterfaceCollectionPropertiesWithSingleNonListItem()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:baz:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:value:baz", "utf-8" },
                    { "foo:baz:value:qux", quxValue.ToString() },

                    { "foo:qux:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:qux:value:baz", "utf-8" },
                    { "foo:qux:value:qux", quxValue.ToString() },

                    { "foo:garply:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:garply:value:baz", "utf-8" },
                    { "foo:garply:value:qux", quxValue.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadonlyCollectionProperties>()!;

            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);

            Assert.Single(foo.Qux);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);

            Assert.Single(foo.Garply);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply.First()).Qux);
        }

        [Fact]
        public void CanBindToInterfaceCollectionConstructorParameters()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:bar:0:value:baz", "utf-8" },
                    { "foo:bar:0:value:qux", quxValue.ToString() },
                    { "foo:bar:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:bar:1:value:baz", "ascii" },

                    { "foo:baz:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:garply:1:value:baz", "ascii" },

                    { "foo:grault:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:grault:0:value:baz", "utf-8" },
                    { "foo:grault:0:value:qux", quxValue.ToString() },
                    { "foo:grault:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:grault:1:value:baz", "ascii" },

                    { "foo:fred:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:fred:0:value:baz", "utf-8" },
                    { "foo:fred:0:value:qux", quxValue.ToString() },
                    { "foo:fred:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:fred:1:value:baz", "ascii" },

                    { "foo:waldo:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:waldo:0:value:baz", "utf-8" },
                    { "foo:waldo:0:value:qux", quxValue.ToString() },
                    { "foo:waldo:1:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:waldo:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceCollectionConstructorParameters>()!;

            Assert.Equal(2, foo.Bar.Length);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar[1].Baz);
            Assert.IsType<HasSomething>(foo.Bar[1]);

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.IsType<HasSomething>(foo.Baz[1]);

            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.IsType<HasSomething>(foo.Qux[1]);

            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
            Assert.IsType<HasSomething>(foo.Garply.Skip(1).First());

            Assert.Equal(2, foo.Grault.Count());
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Grault.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Grault.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Grault.Skip(1).First().Baz);
            Assert.IsType<HasSomething>(foo.Grault.Skip(1).First());

            Assert.Equal(2, foo.Fred.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Fred.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Fred.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Fred.Skip(1).First().Baz);
            Assert.IsType<HasSomething>(foo.Fred.Skip(1).First());

            Assert.Equal(2, foo.Waldo.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Waldo[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Waldo[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Waldo[1].Baz);
            Assert.IsType<HasSomething>(foo.Waldo[1]);
        }

        [Fact]
        public void CanBindToInterfaceCollectionConstructorParametersWithSingleNonListItem()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:bar:value:baz", "utf-8" },
                    { "foo:bar:value:qux", quxValue.ToString() },

                    { "foo:baz:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:value:baz", "utf-8" },
                    { "foo:baz:value:qux", quxValue.ToString() },

                    { "foo:qux:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:qux:value:baz", "utf-8" },
                    { "foo:qux:value:qux", quxValue.ToString() },

                    { "foo:garply:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:garply:value:baz", "utf-8" },
                    { "foo:garply:value:qux", quxValue.ToString() },

                    { "foo:grault:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:grault:value:baz", "utf-8" },
                    { "foo:grault:value:qux", quxValue.ToString() },

                    { "foo:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:fred:value:baz", "utf-8" },
                    { "foo:fred:value:qux", quxValue.ToString() },

                    { "foo:waldo:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:waldo:value:baz", "utf-8" },
                    { "foo:waldo:value:qux", quxValue.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceCollectionConstructorParameters>()!;

            Assert.Single(foo.Bar);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar[0]).Qux);

            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);

            Assert.Single(foo.Qux);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);

            Assert.Single(foo.Garply);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply.First()).Qux);

            Assert.Single(foo.Grault);
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Grault.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Grault.First()).Qux);

            Assert.Single(foo.Fred);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Fred.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Fred.First()).Qux);

            Assert.Single(foo.Waldo);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Waldo[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Waldo[0]).Qux);
        }

        [Fact]
        public void CanBindToReadWriteSimpleDictionaryProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred", "123" },
                    { "foo:bar:waldo", "456" },
                    { "foo:baz:fred", "123" },
                    { "foo:baz:waldo", "456" },
                    { "foo:qux:fred", "123" },
                    { "foo:qux:waldo", "456" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleReadWriteDictionaryProperties>()!;

            Assert.Equal(2, foo.Bar!.Count);
            Assert.Equal(123, foo.Bar["fred"]);
            Assert.Equal(456, foo.Bar["waldo"]);
            Assert.Equal(2, foo.Baz!.Count);
            Assert.Equal(123, foo.Baz["fred"]);
            Assert.Equal(456, foo.Baz["waldo"]);
            Assert.Equal(2, foo.Qux!.Count);
            Assert.Equal(123, foo.Qux["fred"]);
            Assert.Equal(456, foo.Qux["waldo"]);
        }

        [Fact]
        public void CanBindToReadonlySimpleDictionaryProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred", "123" },
                    { "foo:bar:waldo", "456" },
                    { "foo:baz:fred", "123" },
                    { "foo:baz:waldo", "456" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleReadonlyDictionaryProperties>()!;

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(123, foo.Bar["fred"]);
            Assert.Equal(456, foo.Bar["waldo"]);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(123, foo.Baz["fred"]);
            Assert.Equal(456, foo.Baz["waldo"]);
        }

        [Fact]
        public void CanBindToSimpleDictionaryConstructorParameters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred", "123" },
                    { "foo:bar:waldo", "456" },
                    { "foo:baz:fred", "123" },
                    { "foo:baz:waldo", "456" },
                    { "foo:qux:fred", "123" },
                    { "foo:qux:waldo", "456" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleDictionaryConstructorParameters>()!;

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(123, foo.Bar["fred"]);
            Assert.Equal(456, foo.Bar["waldo"]);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(123, foo.Baz["fred"]);
            Assert.Equal(456, foo.Baz["waldo"]);
            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(123, foo.Qux["fred"]);
            Assert.Equal(456, foo.Qux["waldo"]);
        }

        [Fact]
        public void CanBindToReadWriteConcreteDictionaryProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred:baz", "utf-8" },
                    { "foo:bar:waldo:baz", "ascii" },
                    { "foo:baz:fred:baz", "utf-8" },
                    { "foo:baz:waldo:baz", "ascii" },
                    { "foo:qux:fred:baz", "utf-8" },
                    { "foo:qux:waldo:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteReadWriteDictionaryProperties>()!;

            Assert.Equal(2, foo.Bar!.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.Equal(2, foo.Baz!.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
            Assert.Equal(2, foo.Qux!.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Qux["waldo"].Baz);
        }

        [Fact]
        public void CanBindToReadonlyConcreteDictionaryProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred:baz", "utf-8" },
                    { "foo:bar:waldo:baz", "ascii" },
                    { "foo:baz:fred:baz", "utf-8" },
                    { "foo:baz:waldo:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteReadonlyDictionaryProperties>()!;

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
        }

        [Fact]
        public void CanBindToConcreteDictionaryConstructorParameters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred:baz", "utf-8" },
                    { "foo:bar:waldo:baz", "ascii" },
                    { "foo:baz:fred:baz", "utf-8" },
                    { "foo:baz:waldo:baz", "ascii" },
                    { "foo:qux:fred:baz", "utf-8" },
                    { "foo:qux:waldo:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteDictionaryParameters>()!;

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Qux["waldo"].Baz);
        }

        [Fact]
        public void CanBindToReadWriteInterfaceDictionaryProperties()
        {
            TimeSpan quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:baz:waldo:value:baz", "ascii" },

                    { "foo:qux:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:qux:fred:value:baz", "utf-8" },
                    { "foo:qux:fred:value:qux", quxValue.ToString() },
                    { "foo:qux:waldo:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:qux:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadWriteDictionaryProperties>()!;

            Assert.Equal(2, foo.Bar!.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar["fred"]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.IsType<HasSomething>(foo.Bar["waldo"]);

            Assert.Equal(2, foo.Baz!.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz["fred"]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
            Assert.IsType<HasSomething>(foo.Baz["waldo"]);

            Assert.Equal(2, foo.Qux!.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux["fred"].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux["fred"]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Qux["waldo"].Baz);
            Assert.IsType<HasSomething>(foo.Qux["waldo"]);
        }

        [Fact]
        public void CanBindToReadonlyInterfaceDictionaryProperties()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:baz:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadonlyDictionaryProperties>()!;

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar["fred"]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.IsType<HasSomething>(foo.Bar["waldo"]);

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz["fred"]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
            Assert.IsType<HasSomething>(foo.Baz["waldo"]);
        }

        [Fact]
        public void CanBindToInterfaceDictionaryConstructorParameters()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "foo:bar:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:baz:waldo:value:baz", "ascii" },

                    { "foo:qux:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName! },
                    { "foo:qux:fred:value:baz", "utf-8" },
                    { "foo:qux:fred:value:qux", quxValue.ToString() },
                    { "foo:qux:waldo:type", typeof(HasSomething).AssemblyQualifiedName! },
                    { "foo:qux:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceDictionaryParameters>()!;

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar["fred"]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.IsType<HasSomething>(foo.Bar["waldo"]);

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz["fred"]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
            Assert.IsType<HasSomething>(foo.Baz["waldo"]);

            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux["fred"].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux["fred"]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Qux["waldo"].Baz);
            Assert.IsType<HasSomething>(foo.Qux["waldo"]);
        }

        [Fact]
        public void UsesDefaultTypeAttributeForTopLevelType()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "fred", "123.45" },
                })
                .Build();

            var instance = config.Create<IHasDefaultType>()!;

            Assert.IsType<DefaultHasDefaultType>(instance);
            Assert.Equal(123.45, ((DefaultHasDefaultType)instance).Fred);
        }

        [Fact]
        public void UsesDefaultTypesObjectForTopLevelType()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "waldo", "123.45" },
                })
                .Build();

            var defaults = new DefaultTypes().Add(typeof(IHasNoDefaultType), typeof(DefaultHasNoDefaultType));

            var instance = config.Create<IHasNoDefaultType>(defaults)!;

            Assert.IsType<DefaultHasNoDefaultType>(instance);
            Assert.Equal(123.45, ((DefaultHasNoDefaultType)instance).Waldo);
        }

        [Fact]
        public void UsesConvertMethodAttributeForTopLevelType()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "fred", "123.45" },
                })
                .Build();

            var instance = config.GetSection("fred").Create<HasConvertMethod>()!;

            Assert.Equal(123.45, instance.Fred);
        }

        [Fact]
        public void UsesValueConvertersObjectForTopLevelType()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "waldo", "123.45" },
                })
                .Build();

            var valueConverters = new ValueConverters().Add(typeof(HasNoConvertMethod), value => new HasNoConvertMethod { Waldo = double.Parse(value, CultureInfo.InvariantCulture) });

            var instance = config.GetSection("waldo").Create<HasNoConvertMethod>(valueConverters: valueConverters)!;

            Assert.Equal(123.45, instance.Waldo);
        }

        [Fact]
        public void ObjectFieldWithDefaultTypeOfStringDictionaryIsSupported()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["foo:bar:a"] = "abc",
                ["foo:bar:b"] = "xyz",
                ["foo:baz:c"] = "123",
                ["foo:baz:d"] = "456"
            }).Build();

            var defaultTypes = new DefaultTypes()
                .Add(typeof(HasObjectMembersWithDefaultTypeOfStringDictionary),
                    nameof(HasObjectMembersWithDefaultTypeOfStringDictionary.Baz),
                    typeof(Dictionary<string, int>));

            var foo = config.GetSection("foo").Create<HasObjectMembersWithDefaultTypeOfStringDictionary>(defaultTypes)!;

            var bar = (Dictionary<string, string>)foo.Bar;
            var baz = (Dictionary<string, int>)foo.Baz;

            Assert.Equal("abc", bar["a"]);
            Assert.Equal("xyz", bar["b"]);
            Assert.Equal(123, baz["c"]);
            Assert.Equal(456, baz["d"]);
        }

        [Fact]
        public void ObjectFieldsWithDirectConfigurationValuesAreSupported()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["foo:bar"] = "abc",
                ["foo:baz:a"] = "123",
                ["foo:baz:b"] = "xyz"
            }).Build();

            var foo = config.GetSection("foo").Create<HasObjectMembers>()!;

            Assert.Equal("abc", foo.Bar);
            Assert.Equal("123", foo.Baz["a"]);
            Assert.Equal("xyz", foo.Baz["b"]);
        }

        [Theory]
        [InlineData(typeof(HasMembersDecoratedWithSingleAlternateNameAttribute), "foo")]
        [InlineData(typeof(HasMembersDecoratedWithSingleAlternateNameAttribute), "foo1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "foo")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "foo1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "foo2")]
        [InlineData(typeof(HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute), "foo")]
        [InlineData(typeof(HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute), "foo1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "foo")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "foo1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "foo2")]
        public void ConstructorsUseAlternateNameAttribute(Type type, string configurationKey)
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                [configurationKey] = "abc"
            }).Build();

            var item = (IHasMembers)config!.Create(type)!;

            Assert.Equal("abc", item.Foo);
        }

        [Theory]
        [InlineData(typeof(HasMembersDecoratedWithSingleAlternateNameAttribute), "bar")]
        [InlineData(typeof(HasMembersDecoratedWithSingleAlternateNameAttribute), "bar1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "bar")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "bar1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "bar2")]
        [InlineData(typeof(HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute), "bar")]
        [InlineData(typeof(HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute), "bar1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "bar")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "bar1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "bar2")]
        public void ReadWritePropertiesUseAlternateNameAttribute(Type type, string configurationKey)
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                [configurationKey] = "abc"
            }).Build();

            var item = (IHasMembers)config!.Create(type)!;

            Assert.Equal("abc", item.Bar);
        }

        [Theory]
        [InlineData(typeof(HasMembersDecoratedWithSingleAlternateNameAttribute), "baz")]
        [InlineData(typeof(HasMembersDecoratedWithSingleAlternateNameAttribute), "baz1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "baz")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "baz1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "baz2")]
        [InlineData(typeof(HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute), "baz")]
        [InlineData(typeof(HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute), "baz1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "baz")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "baz1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "baz2")]
        public void ReadonlyListPropertiesUseAlternateNameAttribute(Type type, string configurationKey)
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                [configurationKey] = "abc"
            }).Build();

            var item = (IHasMembers)config!.Create(type)!;

            Assert.Single(item.Baz);
            Assert.Equal("abc", item.Baz[0]);
        }

        [Theory]
        [InlineData(typeof(HasMembersDecoratedWithSingleAlternateNameAttribute), "qux")]
        [InlineData(typeof(HasMembersDecoratedWithSingleAlternateNameAttribute), "qux1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "qux")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "qux1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleAlternateNameAttributes), "qux2")]
        [InlineData(typeof(HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute), "qux")]
        [InlineData(typeof(HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute), "qux1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "qux")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "qux1")]
        [InlineData(typeof(HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes), "qux2")]
        public void ReadonlyDictionaryPropertiesUseAlternateNameAttribute(Type type, string configurationKey)
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{configurationKey}:garply"] = "abc"
            }).Build();

            var item = (IHasMembers)config!.Create(type)!;

            Assert.Single(item.Qux);
            var dictionaryItem = item.Qux.Single();
            Assert.Equal("garply", dictionaryItem.Key);
            Assert.Equal("abc", dictionaryItem.Value);
        }
    }

    public class HasSimpleReadWriteDictionaryProperties
    {
        public Dictionary<string, int>? Bar { get; set; }
        public IDictionary<string, int>? Baz { get; set; }
        public IReadOnlyDictionary<string, int>? Qux { get; set; }
    }

    public class HasSimpleReadonlyDictionaryProperties
    {
        public Dictionary<string, int> Bar { get; } = new Dictionary<string, int>();
        public IDictionary<string, int> Baz { get; } = new Dictionary<string, int>();
    }

    public class HasSimpleDictionaryConstructorParameters
    {
        public HasSimpleDictionaryConstructorParameters(Dictionary<string, int> bar, IDictionary<string, int> baz, IReadOnlyDictionary<string, int> qux)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
        }

        public Dictionary<string, int> Bar { get; }
        public IDictionary<string, int> Baz { get; }
        public IReadOnlyDictionary<string, int> Qux { get; }
    }

    public class HasConcreteReadWriteDictionaryProperties
    {
        public Dictionary<string, HasSomething>? Bar { get; set; }
        public IDictionary<string, HasSomething>? Baz { get; set; }
        public IReadOnlyDictionary<string, HasSomething>? Qux { get; set; }
    }

    public class HasConcreteReadonlyDictionaryProperties
    {
        public Dictionary<string, HasSomething> Bar { get; } = new Dictionary<string, HasSomething>();
        public IDictionary<string, HasSomething> Baz { get; } = new Dictionary<string, HasSomething>();
    }

    public class HasConcreteDictionaryParameters
    {
        public HasConcreteDictionaryParameters(Dictionary<string, HasSomething> bar, IDictionary<string, HasSomething> baz, IReadOnlyDictionary<string, HasSomething> qux)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
        }

        public Dictionary<string, HasSomething> Bar { get; }
        public IDictionary<string, HasSomething> Baz { get; }
        public IReadOnlyDictionary<string, HasSomething> Qux { get; }
    }

    public class HasInterfaceReadWriteDictionaryProperties
    {
        public Dictionary<string, IHasSomething>? Bar { get; set; }
        public IDictionary<string, IHasSomething>? Baz { get; set; }
        public IReadOnlyDictionary<string, IHasSomething>? Qux { get; set; }
    }

    public class HasInterfaceReadonlyDictionaryProperties
    {
        public Dictionary<string, IHasSomething> Bar { get; } = new Dictionary<string, IHasSomething>();
        public IDictionary<string, IHasSomething> Baz { get; } = new Dictionary<string, IHasSomething>();
    }

    public class HasInterfaceDictionaryParameters
    {
        public HasInterfaceDictionaryParameters(Dictionary<string, IHasSomething> bar, IDictionary<string, IHasSomething> baz, IReadOnlyDictionary<string, IHasSomething> qux)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
        }

        public Dictionary<string, IHasSomething> Bar { get; }
        public IDictionary<string, IHasSomething> Baz { get; }
        public IReadOnlyDictionary<string, IHasSomething> Qux { get; }
    }

    public class HasSimpleReadWriteCollectionProperties
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public int[]? Bar { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only
        public List<int>? Baz { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<int>? Qux { get; set; }
        public ICollection<int>? Garply { get; set; }
        public IEnumerable<int>? Grault { get; set; }
        public IReadOnlyCollection<int>? Fred { get; set; }
        public IReadOnlyList<int>? Waldo { get; set; }
    }

    public class HasSimpleReadonlyCollectionProperties
    {
#pragma warning disable CA1002 // Do not expose generic lists
        public List<int> Baz { get; } = new List<int>();
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<int> Qux { get; } = new List<int>();
        public ICollection<int> Garply { get; } = new List<int>();
    }

    public class HasSimpleCollectionConstructorParameters
    {
        public HasSimpleCollectionConstructorParameters(
            int[] bar,
#pragma warning disable CA1002 // Do not expose generic lists
            List<int> baz,
#pragma warning restore CA1002 // Do not expose generic lists
            IList<int> qux,
            ICollection<int> garply,
            IEnumerable<int> grault,
            IReadOnlyCollection<int> fred,
            IReadOnlyList<int> waldo)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
            Garply = garply;
            Grault = grault;
            Fred = fred;
            Waldo = waldo;
        }
#pragma warning disable CA1819 // Properties should not return arrays
        public int[] Bar { get; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning disable CA1002 // Do not expose generic lists
        public List<int> Baz { get; }
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<int> Qux { get; }
        public ICollection<int> Garply { get; }
        public IEnumerable<int> Grault { get; }
        public IReadOnlyCollection<int> Fred { get; }
        public IReadOnlyList<int> Waldo { get; }
    }

#pragma warning disable CA1812
    internal sealed class HasByteCollectionConstructorParameters
#pragma warning restore CA1812
    {
        public HasByteCollectionConstructorParameters(
            byte[] bar,
#pragma warning disable CA1002 // Do not expose generic lists
            List<byte> baz,
#pragma warning restore CA1002 // Do not expose generic lists
            IList<byte> qux,
            ICollection<byte> garply,
            IEnumerable<byte> grault,
            IReadOnlyCollection<byte> fred,
            IReadOnlyList<byte> waldo)
        {
            Bar = Convert.ToBase64String(bar);
            Baz = Convert.ToBase64String(baz.ToArray());
            Qux = Convert.ToBase64String(qux.ToArray());
            Garply = Convert.ToBase64String(garply.ToArray());
            Grault = Convert.ToBase64String(grault.ToArray());
            Fred = Convert.ToBase64String(fred.ToArray());
            Waldo = Convert.ToBase64String(waldo.ToArray());
        }
        public string Bar { get; }
        public string Baz { get; }
        public string Qux { get; }
        public string Garply { get; }
        public string Grault { get; }
        public string Fred { get; }
        public string Waldo { get; }
    }

    public class HasConcreteReadWriteCollectionProperties
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public HasSomething[]? Bar { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only
        public List<HasSomething>? Baz { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<HasSomething>? Qux { get; set; }
        public ICollection<HasSomething>? Garply { get; set; }
        public IEnumerable<HasSomething>? Grault { get; set; }
        public IReadOnlyCollection<HasSomething>? Fred { get; set; }
        public IReadOnlyList<HasSomething>? Waldo { get; set; }
    }

    public class HasByteCollectionProperties
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[]? Bar { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only
        public List<byte>? Baz { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<byte>? Qux { get; set; }
        public ICollection<byte>? Garply { get; set; }
        public IEnumerable<byte>? Grault { get; set; }
        public IReadOnlyCollection<byte>? Fred { get; set; }
        public IReadOnlyList<byte>? Waldo { get; set; }
    }

    public class HasConcreteReadonlyCollectionProperties
    {
#pragma warning disable CA1002 // Do not expose generic lists
        public List<HasSomething> Baz { get; } = new List<HasSomething>();
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<HasSomething> Qux { get; } = new List<HasSomething>();
        public ICollection<HasSomething> Garply { get; } = new List<HasSomething>();
    }

    public class HasNonGenericCollectionProperties
    {
        public HasNonGenericCollectionProperties(HasSomethingCollection baz)
        {
            Baz = baz;
        }

        public HasSomethingCollection? Bar { get; set; }
        public HasSomethingCollection Baz { get; }
        public HasSomethingCollection Qux { get; } = new HasSomethingCollection();
    }

    public class HasReadonlyListPropertiesWithInitialItems
    {
#pragma warning disable CA1002 // Do not expose generic lists
        public List<HasSomething> Bar { get; } = new List<HasSomething>() { new HasSomething { Baz = Encoding.ASCII } };
#pragma warning restore CA1002 // Do not expose generic lists
        public HasSomethingCollection Baz { get; } = new HasSomethingCollection() { new HasSomething { Baz = Encoding.ASCII } };
    }

    public class HasFlagsEnumProperties
    {
        public ExampleValues Bar { get; set; }
        public ExampleValues Baz { get; set; }
        public ExampleValues Qux { get; set; }
    }

    [Flags]
    public enum ExampleValues
    {
        Garply = 1,
        Grault = 2,
        Corge = 4
    }

#pragma warning disable CA1010 // Generic interface should also be implemented
#pragma warning disable CA1033 // Interface methods should be callable by child types
    public class HasSomethingCollection : IList
#pragma warning restore CA1010 // Generic interface should also be implemented
    {
        private readonly List<HasSomething> _list = new List<HasSomething>();

        object? IList.this[int index] { get => this[index]; set => this[index] = (HasSomething)value!; }

        public HasSomething this[int index] { get => _list[index]; set => _list[index] = value; }

        bool IList.IsFixedSize => ((IList)_list).IsFixedSize;

        bool IList.IsReadOnly => ((IList)_list).IsReadOnly;

        public int Count => _list.Count;

        bool ICollection.IsSynchronized => ((IList)_list).IsSynchronized;

        object ICollection.SyncRoot => ((IList)_list).SyncRoot;

        int IList.Add(object? value)
        {
            return Add((HasSomething)value!);
        }

        public int Add(HasSomething value)
        {
            return ((IList)_list).Add(value);
        }

        public void Clear()
        {
            _list.Clear();
        }

        bool IList.Contains(object? value)
        {
            return Contains((HasSomething)value!);
        }

        public bool Contains(HasSomething value)
        {
            return _list.Contains(value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IList)_list).CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList)_list).GetEnumerator();
        }

        int IList.IndexOf(object? value)
        {
            return IndexOf((HasSomething)value!);
        }

        public int IndexOf(HasSomething value)
        {
            return _list.IndexOf(value);
        }

        void IList.Insert(int index, object? value)
        {
            Insert(index, (HasSomething)value!);
        }

        public void Insert(int index, HasSomething value)
        {
            _list.Insert(index, value);
        }

        void IList.Remove(object? value)
        {
            Remove((HasSomething)value!);
        }

        public void Remove(HasSomething value)
        {
            _list.Remove(value);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
#pragma warning restore CA1033 // Interface methods should be callable by child types
    }

    public class HasConcreteCollectionConstructorParameters
    {
        public HasConcreteCollectionConstructorParameters(
            HasSomething[] bar,
#pragma warning disable CA1002 // Do not expose generic lists
            List<HasSomething> baz,
#pragma warning restore CA1002 // Do not expose generic lists
            IList<HasSomething> qux,
            ICollection<HasSomething> garply,
            IEnumerable<HasSomething> grault,
            IReadOnlyCollection<HasSomething> fred,
            IReadOnlyList<HasSomething> waldo)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
            Garply = garply;
            Grault = grault;
            Fred = fred;
            Waldo = waldo;
        }
#pragma warning disable CA1819 // Properties should not return arrays
        public HasSomething[] Bar { get; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning disable CA1002 // Do not expose generic lists
        public List<HasSomething> Baz { get; }
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<HasSomething> Qux { get; }
        public ICollection<HasSomething> Garply { get; }
        public IEnumerable<HasSomething> Grault { get; }
        public IReadOnlyCollection<HasSomething> Fred { get; }
        public IReadOnlyList<HasSomething> Waldo { get; }
    }

    public class HasInterfaceReadWriteCollectionProperties
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public IHasSomething[]? Bar { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only
        public List<IHasSomething>? Baz { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<IHasSomething>? Qux { get; set; }
        public ICollection<IHasSomething>? Garply { get; set; }
        public IEnumerable<IHasSomething>? Grault { get; set; }
        public IReadOnlyCollection<IHasSomething>? Fred { get; set; }
        public IReadOnlyList<IHasSomething>? Waldo { get; set; }
    }

    public class HasInterfaceReadonlyCollectionProperties
    {
#pragma warning disable CA1002 // Do not expose generic lists
        public List<IHasSomething> Baz { get; } = new List<IHasSomething>();
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<IHasSomething> Qux { get; } = new List<IHasSomething>();
        public ICollection<IHasSomething> Garply { get; } = new List<IHasSomething>();
    }

    public class HasInterfaceCollectionConstructorParameters
    {
        public HasInterfaceCollectionConstructorParameters(
            IHasSomething[] bar,
#pragma warning disable CA1002 // Do not expose generic lists
            List<IHasSomething> baz,
#pragma warning restore CA1002 // Do not expose generic lists
            IList<IHasSomething> qux,
            ICollection<IHasSomething> garply,
            IEnumerable<IHasSomething> grault,
            IReadOnlyCollection<IHasSomething> fred,
            IReadOnlyList<IHasSomething> waldo)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
            Garply = garply;
            Grault = grault;
            Fred = fred;
            Waldo = waldo;
        }
#pragma warning disable CA1819 // Properties should not return arrays
        public IHasSomething[] Bar { get; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning disable CA1002 // Do not expose generic lists
        public List<IHasSomething> Baz { get; }
#pragma warning restore CA1002 // Do not expose generic lists
        public IList<IHasSomething> Qux { get; }
        public ICollection<IHasSomething> Garply { get; }
        public IEnumerable<IHasSomething> Grault { get; }
        public IReadOnlyCollection<IHasSomething> Fred { get; }
        public IReadOnlyList<IHasSomething> Waldo { get; }
    }

    public interface IHasSomething
    {
        Encoding? Baz { get; }
    }

    public class HasSomething : IHasSomething
    {
        public Encoding? Baz { get; set; }
    }

    public class DerivedHasSomething : HasSomething
    {
        public TimeSpan Qux { get; set; }
    }

    public class HasReadWriteSimpleProperties
    {
        public int Bar { get; set; }
        public DateTime Baz { get; set; }
        public Type? Qux { get; set; }
    }

    public class HasSimpleConstructorParameters
    {
        public HasSimpleConstructorParameters(int bar, DateTime baz, bool qux)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
        }

        public int Bar { get; }
        public DateTime Baz { get; }
        public bool Qux { get; }
    }

    public class HasReadWriteConcreteProperties
    {
        public HasReadWriteProperties? Bar { get; set; }
        public AlsoHasReadWriteProperties? Baz { get; set; }
    }

    public class HasReadWriteInterfaceProperties
    {
        public IHasSimpleProperties? Bar { get; set; }
        public IAlsoHasSimpleProperties? Baz { get; set; }
    }

    public interface IHasSimpleProperties
    {
        bool Qux { get; }
        double Garply { get; }
    }

    public interface IAlsoHasSimpleProperties
    {
        Guid Grault { get; }
    }

    public class HasReadWriteProperties : IHasSimpleProperties
    {
        public bool Qux { get; set; }
        public double Garply { get; set; }
    }

    public class InheritedHasReadWriteProperties : HasReadWriteProperties
    {
        public string? Spam { get; set; }
    }

    public class AlsoHasReadWriteProperties : IAlsoHasSimpleProperties
    {
        public Guid Grault { get; set; }
    }

    public class HasConcreteConstructorParameters
    {
        public HasConcreteConstructorParameters(HasConstructorParameters bar, AlsoHasConstructorParameters baz)
        {
            Bar = bar;
            Baz = baz;
        }
        public HasConstructorParameters Bar { get; }
        public AlsoHasConstructorParameters Baz { get; }
    }

    public class HasInterfaceConstructorParameters
    {
        public HasInterfaceConstructorParameters(IHasSimpleProperties bar, IAlsoHasSimpleProperties baz)
        {
            Bar = bar;
            Baz = baz;
        }
        public IHasSimpleProperties Bar { get; }
        public IAlsoHasSimpleProperties Baz { get; }
    }

    public class HasConstructorParameters : IHasSimpleProperties
    {
        public HasConstructorParameters(bool qux, double garply)
        {
            Qux = qux;
            Garply = garply;
        }
        public bool Qux { get; }
        public double Garply { get; }
    }

    public class HasOptionalNullableConstructorParameters
    {
        public HasOptionalNullableConstructorParameters(int? bar = 5, decimal? baz = 10.2m, ExampleValues? flag = ExampleValues.Grault, ExampleValues? flag2 = null)
        {
            Bar = bar;
            Baz = baz;
            Flag = flag;
            Flag2 = flag2;
        }

        public int? Bar { get; }
        public decimal? Baz { get; }
        public ExampleValues? Flag { get; }
        public ExampleValues? Flag2 { get; }
    }

    public class InheritedHasConstructorParameters : HasConstructorParameters
    {
        public InheritedHasConstructorParameters(bool qux, double garply, string spam)
            : base(qux, garply)
        {
            Spam = spam;
        }
        public string Spam { get; }
    }

    public class InheritedHasDefaultConstructor : HasConstructorParameters
    {
        public InheritedHasDefaultConstructor()
            : base(true, 543.21)
        {
        }
    }

    public class AlsoHasConstructorParameters : IAlsoHasSimpleProperties
    {
        public AlsoHasConstructorParameters(Guid grault)
        {
            Grault = grault;
        }
        public Guid Grault { get; }
    }

    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class CoordinateContainer
    {
        public Coordinate? Bar { get; set; }
    }

    public class DoubleContainer
    {
        public double Bar { get; set; }
    }

    public class AnotherDoubleContainer
    {
        public double Bar { get; set; }
    }

    public class StringContainer
    {
        public string? Bar { get; set; }
    }

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class HasMembersDecoratedWithDefaultTypeAttribute
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public HasMembersDecoratedWithDefaultTypeAttribute(
            [DefaultType(typeof(DefaultHasDefaultType))] IHasDefaultType graultBar,
            [DefaultType(typeof(DefaultHasDefaultType))] IHasDefaultType[] graultBarArray,
#pragma warning disable CA1002 // Do not expose generic lists
            [DefaultType(typeof(DefaultHasDefaultType))] List<IHasDefaultType> graultBarList,
#pragma warning restore CA1002 // Do not expose generic lists
            [DefaultType(typeof(DefaultHasDefaultType))] Dictionary<string, IHasDefaultType> graultBarDictionary,

            [DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] IAlsoHasNoDefaultType qux,
            [DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] IAlsoHasNoDefaultType[] quxArray,
#pragma warning disable CA1002 // Do not expose generic lists
            [DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] List<IAlsoHasNoDefaultType> quxList,
#pragma warning restore CA1002 // Do not expose generic lists
            [DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] Dictionary<string, IAlsoHasNoDefaultType> quxDictionary)
        {
            GraultBar = graultBar;
            GraultBarArray = graultBarArray;
            GraultBarList = graultBarList;
            GraultBarDictionary = graultBarDictionary;

            Qux = qux;
            QuxArray = quxArray;
            QuxList = quxList;
            QuxDictionary = quxDictionary;
        }

        public IHasDefaultType? GarplyBar { get; set; }
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA1819 // Properties should not return arrays
        public IHasDefaultType[]? GarplyBarArray { get; set; }
        public List<IHasDefaultType>? GarplyBarList { get; set; }
        public Dictionary<string, IHasDefaultType>? GarplyBarDictionary { get; set; }

        public IHasDefaultType GraultBar { get; }
        public IHasDefaultType[] GraultBarArray { get; }
        public List<IHasDefaultType> GraultBarList { get; }
        public Dictionary<string, IHasDefaultType> GraultBarDictionary { get; }

        public List<IHasDefaultType> BarReadonlyList { get; } = new List<IHasDefaultType>();
        public Dictionary<string, IHasDefaultType> BarReadonlyDictionary { get; } = new Dictionary<string, IHasDefaultType>();

        [DefaultType(typeof(DefaultHasNoDefaultType))] public IHasNoDefaultType? Baz { get; set; }
        [DefaultType(typeof(DefaultHasNoDefaultType))] public IHasNoDefaultType[]? BazArray { get; set; }
        [DefaultType(typeof(DefaultHasNoDefaultType))] public List<IHasNoDefaultType>? BazList { get; set; }
        [DefaultType(typeof(DefaultHasNoDefaultType))] public Dictionary<string, IHasNoDefaultType>? BazDictionary { get; set; }

        [DefaultType(typeof(DefaultHasNoDefaultType))] public List<IHasNoDefaultType>? BazReadonlyList { get; } = new List<IHasNoDefaultType>();
        [DefaultType(typeof(DefaultHasNoDefaultType))] public Dictionary<string, IHasNoDefaultType> BazReadonlyDictionary { get; } = new Dictionary<string, IHasNoDefaultType>();

        public IAlsoHasNoDefaultType Qux { get; }
        public IAlsoHasNoDefaultType[] QuxArray { get; }
        public List<IAlsoHasNoDefaultType> QuxList { get; }
        public Dictionary<string, IAlsoHasNoDefaultType> QuxDictionary { get; }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA1002 // Do not expose generic lists
    }

    [DefaultType(typeof(DefaultHasDefaultType))]
#pragma warning disable CA1040 // Avoid empty interfaces
    public interface IHasDefaultType
    {
    }

    public class DefaultHasDefaultType : IHasDefaultType
    {
        public double Fred { get; set; }
    }

    public interface IHasNoDefaultType
    {
    }

    public class DefaultHasNoDefaultType : IHasNoDefaultType
    {
        public double Waldo { get; set; }
    }

    [ConvertMethod(nameof(Convert))]
    public class HasConvertMethod
    {
        public double Fred { get; set; }

        private static HasConvertMethod Convert(string value)
        {
            return new HasConvertMethod { Fred = double.Parse(value, CultureInfo.InvariantCulture) };
        }
    }

    public class HasNoConvertMethod
    {
        public double Waldo { get; set; }
    }

    public interface IAlsoHasNoDefaultType
    {
    }

    public class DefaultIAlsoHasNoDefaultType : IAlsoHasNoDefaultType
    {
        public double Thud { get; set; }
    }

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA1819 // Properties should not return arrays
    public class MembersDecoratedWithLocallyDefinedDefaultTypeAttribute
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public MembersDecoratedWithLocallyDefinedDefaultTypeAttribute(
            [LocallyDefined.DefaultType(typeof(DefaultHasLocallyDefinedDefaultType))] IHasLocallyDefinedDefaultType graultBar,
            [LocallyDefined.DefaultType(typeof(DefaultHasLocallyDefinedDefaultType))] IHasLocallyDefinedDefaultType[] graultBarArray,
            [LocallyDefined.DefaultType(typeof(DefaultHasLocallyDefinedDefaultType))] List<IHasLocallyDefinedDefaultType> graultBarList,
            [LocallyDefined.DefaultType(typeof(DefaultHasLocallyDefinedDefaultType))] Dictionary<string, IHasLocallyDefinedDefaultType> graultBarDictionary,

            [LocallyDefined.DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] IAlsoHasNoDefaultType qux,
            [LocallyDefined.DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] IAlsoHasNoDefaultType[] quxArray,
            [LocallyDefined.DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] List<IAlsoHasNoDefaultType> quxList,
            [LocallyDefined.DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] Dictionary<string, IAlsoHasNoDefaultType> quxDictionary)
        {
            GraultBar = graultBar;
            GraultBarArray = graultBarArray;
            GraultBarList = graultBarList;
            GraultBarDictionary = graultBarDictionary;

            Qux = qux;
            QuxArray = quxArray;
            QuxList = quxList;
            QuxDictionary = quxDictionary;
        }

        public IHasLocallyDefinedDefaultType? GarplyBar { get; set; }
        public IHasLocallyDefinedDefaultType[]? GarplyBarArray { get; set; }
        public List<IHasLocallyDefinedDefaultType>? GarplyBarList { get; set; }
        public Dictionary<string, IHasLocallyDefinedDefaultType>? GarplyBarDictionary { get; set; }

        public IHasLocallyDefinedDefaultType GraultBar { get; }
        public IHasLocallyDefinedDefaultType[] GraultBarArray { get; }
        public List<IHasLocallyDefinedDefaultType> GraultBarList { get; }
        public Dictionary<string, IHasLocallyDefinedDefaultType> GraultBarDictionary { get; }

        public List<IHasLocallyDefinedDefaultType> BarReadonlyList { get; } = new List<IHasLocallyDefinedDefaultType>();
        public Dictionary<string, IHasLocallyDefinedDefaultType> BarReadonlyDictionary { get; } = new Dictionary<string, IHasLocallyDefinedDefaultType>();

        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public IHasNoDefaultType? Baz { get; set; }
        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public IHasNoDefaultType[]? BazArray { get; set; }
        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public List<IHasNoDefaultType>? BazList { get; set; }
        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public Dictionary<string, IHasNoDefaultType>? BazDictionary { get; set; }

        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public List<IHasNoDefaultType> BazReadonlyList { get; } = new List<IHasNoDefaultType>();
        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public Dictionary<string, IHasNoDefaultType> BazReadonlyDictionary { get; } = new Dictionary<string, IHasNoDefaultType>();

        public IAlsoHasNoDefaultType Qux { get; }
        public IAlsoHasNoDefaultType[] QuxArray { get; }
        public List<IAlsoHasNoDefaultType> QuxList { get; }
        public Dictionary<string, IAlsoHasNoDefaultType> QuxDictionary { get; }
    }
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA1002 // Do not expose generic lists

    [LocallyDefined.DefaultType(typeof(DefaultHasLocallyDefinedDefaultType))]
    public interface IHasLocallyDefinedDefaultType
    {
    }

    public class DefaultHasLocallyDefinedDefaultType : IHasLocallyDefinedDefaultType
    {
        public double Fred { get; set; }
    }

    [ConvertMethod(nameof(Convert))]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class IsDecoratedWithValueConverterAttribute
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        private IsDecoratedWithValueConverterAttribute(double value) => Value = value;
        public double Value { get; }
        private static IsDecoratedWithValueConverterAttribute Convert(string value) =>
            new IsDecoratedWithValueConverterAttribute(double.Parse(value, CultureInfo.InvariantCulture) * 5);
    }

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class HasMembersDecoratedWithValueConverterAttribute
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public HasMembersDecoratedWithValueConverterAttribute(
            [ConvertMethod(nameof(ConvertBar))] double bar) => Bar = bar;

        public double Bar { get; }
        [ConvertMethod(nameof(ConvertBaz))] public IEnumerable<double>? Baz { get; set; }
        public Dictionary<string, IsDecoratedWithValueConverterAttribute> Qux { get; } = new Dictionary<string, IsDecoratedWithValueConverterAttribute>();

        private static double ConvertBar(string value) => double.Parse(value, CultureInfo.InvariantCulture) * 2;
        private static double ConvertBaz(string value) => double.Parse(value, CultureInfo.InvariantCulture) * 3;
    }

    [LocallyDefined.ConvertMethod(nameof(Convert))]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class IsDecoratedWithLocallyDefinedValueConverterAttribute
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        private IsDecoratedWithLocallyDefinedValueConverterAttribute(double value) => Value = value;
        public double Value { get; }
        private static IsDecoratedWithLocallyDefinedValueConverterAttribute Convert(string value) =>
            new IsDecoratedWithLocallyDefinedValueConverterAttribute(double.Parse(value, CultureInfo.InvariantCulture) * 13);
    }

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class HasMembersDecoratedWithLocallyDefinedValueConverterAttribute
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public HasMembersDecoratedWithLocallyDefinedValueConverterAttribute(
            [LocallyDefined.ConvertMethod(nameof(ConvertBar))] double bar) => Bar = bar;

        public double Bar { get; }
        [LocallyDefined.ConvertMethod(nameof(ConvertBaz))] public IEnumerable<double>? Baz { get; set; }
        public Dictionary<string, IsDecoratedWithLocallyDefinedValueConverterAttribute> Qux { get; } = new Dictionary<string, IsDecoratedWithLocallyDefinedValueConverterAttribute>();

        private static double ConvertBar(string value) => double.Parse(value, CultureInfo.InvariantCulture) * 7;
        private static double ConvertBaz(string value) => double.Parse(value, CultureInfo.InvariantCulture) * 11;
    }

    public interface IHasMembers
    {
        string? Foo { get; }
        string? Bar { get; }
#pragma warning disable CA1002 // Do not expose generic lists
        List<string> Baz { get; }
#pragma warning restore CA1002 // Do not expose generic lists
        Dictionary<string, string> Qux { get; }
    }

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class HasMembersDecoratedWithSingleAlternateNameAttribute : IHasMembers
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public HasMembersDecoratedWithSingleAlternateNameAttribute([AlternateName("foo1")] string? foo = null) => Foo = foo;
        public string? Foo { get; }
        [AlternateName("bar1")] public string? Bar { get; set; }
        [AlternateName("baz1")] public List<string> Baz { get; } = new List<string>();
        [AlternateName("qux1")] public Dictionary<string, string> Qux { get; } = new Dictionary<string, string>();
    }

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute : IHasMembers
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public HasMembersDecoratedWithSingleLocallyDefinedAlternateNameAttribute([LocallyDefined.AlternateName("foo1")] string? foo = null) => Foo = foo;
        public string? Foo { get; }
        [LocallyDefined.AlternateName("bar1")] public string? Bar { get; set; }
        [LocallyDefined.AlternateName("baz1")] public List<string> Baz { get; } = new List<string>();
        [LocallyDefined.AlternateName("qux1")] public Dictionary<string, string> Qux { get; } = new Dictionary<string, string>();
    }

    public class HasMembersDecoratedWithMultipleAlternateNameAttributes : IHasMembers
    {
        public HasMembersDecoratedWithMultipleAlternateNameAttributes([AlternateName("foo1"), AlternateName("foo2")] string? foo = null) => Foo = foo;
        public string? Foo { get; }
        [AlternateName("bar1"), AlternateName("bar2")] public string? Bar { get; set; }
        [AlternateName("baz1"), AlternateName("baz2")] public List<string> Baz { get; } = new List<string>();
        [AlternateName("qux1"), AlternateName("qux2")] public Dictionary<string, string> Qux { get; } = new Dictionary<string, string>();
    }

    public class HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes : IHasMembers
    {
        public HasMembersDecoratedWithMultipleLocallyDefinedAlternateNameAttributes([LocallyDefined.AlternateName("foo1"), LocallyDefined.AlternateName("foo2")] string? foo = null) => Foo = foo;
        public string? Foo { get; }
        [LocallyDefined.AlternateName("bar1"), LocallyDefined.AlternateName("bar2")] public string? Bar { get; set; }
        [LocallyDefined.AlternateName("baz1"), LocallyDefined.AlternateName("baz2")] public List<string> Baz { get; } = new List<string>();
        [LocallyDefined.AlternateName("qux1"), LocallyDefined.AlternateName("qux2")] public Dictionary<string, string> Qux { get; } = new Dictionary<string, string>();
    }

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class HasObjectMembersWithDefaultTypeOfStringDictionary
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public HasObjectMembersWithDefaultTypeOfStringDictionary([DefaultType(typeof(Dictionary<string, string>))] object bar,
            object baz)
        {
            Bar = bar;
            Baz = baz;
        }

        public object Bar { get; }
        public object Baz { get; }
    }

    public class HasObjectMembers
    {
        public HasObjectMembers(object bar, Dictionary<string, object> baz)
        {
            Bar = bar;
            Baz = baz;
        }

        public object Bar { get; }
        public Dictionary<string, object> Baz { get; }
    }

    public class CustomEnumerablePropertyClass
    {
        public CustomEnumerable? Bar { get; set; }
    }

    public class CustomEnumerable : IEnumerable<char>
    {
        public decimal Baz { get; set; }

        public IEnumerator<char> GetEnumerator() => Baz.ToString(CultureInfo.InvariantCulture).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Baz.ToString(CultureInfo.InvariantCulture).GetEnumerator();
    }
#pragma warning restore CA1040 // Avoid empty interfaces
#pragma warning restore CA1034 // Nested types should not be visible
}

namespace LocallyDefined
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class DefaultTypeAttribute : Attribute
    {
        public DefaultTypeAttribute(Type value) => Value = value;
        public Type Value { get; }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class ConvertMethodAttribute : Attribute
    {
        public ConvertMethodAttribute(string convertMethodName) => ConvertMethodName = convertMethodName;
        public string ConvertMethodName { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true)]
    internal sealed class AlternateNameAttribute : Attribute
    {
        public AlternateNameAttribute(string name) => Name = name;
        public string Name { get; }
    }
}
