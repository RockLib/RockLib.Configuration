using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace Tests
{
    public class ConfigurationObjectFactoryTests
    {
        [Fact]
        public void CanSpecifyConvertMethodWithConvertMethodAttribute()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar", "123.45" },
                    { "foo:baz:0", "234.56" },
                    { "foo:baz:1", "345.67" },
                    { "foo:qux:fred", "456.78" },
                    { "foo:qux:waldo", "567.89" },
                }).Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasMembersDecoratedWithValueConverterAttribute>();

            Assert.Equal(123.45 * 2, foo.Bar);

            Assert.Equal(2, foo.Baz.Count());
            Assert.Equal(234.56 * 3, foo.Baz.First());
            Assert.Equal(345.67 * 3, foo.Baz.Skip(1).First());

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar", "123.45" },
                    { "foo:baz:0", "234.56" },
                    { "foo:baz:1", "345.67" },
                    { "foo:qux:fred", "456.78" },
                    { "foo:qux:waldo", "567.89" },
                }).Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasMembersDecoratedWithLocallyDefinedValueConverterAttribute>();

            Assert.Equal(123.45 * 7, foo.Bar);

            Assert.Equal(2, foo.Baz.Count());
            Assert.Equal(234.56 * 11, foo.Baz.First());
            Assert.Equal(345.67 * 11, foo.Baz.Skip(1).First());

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
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasMembersDecoratedWithDefaultTypeAttribute>();

            Assert.IsType<DefaultHasDefaultType>(foo.GarplyBar);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GarplyBar).Fred);
            Assert.Single(foo.GarplyBarArray);
            Assert.IsType<DefaultHasDefaultType>(foo.GarplyBarArray[0]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GarplyBarArray[0]).Fred);
            Assert.Single(foo.GarplyBarList);
            Assert.IsType<DefaultHasDefaultType>(foo.GarplyBarList[0]);
            Assert.Equal(123.45, ((DefaultHasDefaultType)foo.GarplyBarList[0]).Fred);
            Assert.Single(foo.GarplyBarDictionary);
            Assert.IsType<DefaultHasDefaultType>(foo.GarplyBarDictionary["spam"]);
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
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.Baz).Waldo);
            Assert.Single(foo.BazArray);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazArray[0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazArray[0]).Waldo);
            Assert.Single(foo.BazList);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazList[0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazList[0]).Waldo);
            Assert.Single(foo.BazDictionary);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazDictionary["spam"]);
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
        public void CanSpecifyDefaultTypesWithLocallyDefinedDefaultTypeAttribute()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<MembersDecoratedWithLocallyDefinedDefaultTypeAttribute>();

            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GarplyBar);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GarplyBar).Fred);
            Assert.Single(foo.GarplyBarArray);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GarplyBarArray[0]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GarplyBarArray[0]).Fred);
            Assert.Single(foo.GarplyBarList);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GarplyBarList[0]);
            Assert.Equal(123.45, ((DefaultHasLocallyDefinedDefaultType)foo.GarplyBarList[0]).Fred);
            Assert.Single(foo.GarplyBarDictionary);
            Assert.IsType<DefaultHasLocallyDefinedDefaultType>(foo.GarplyBarDictionary["spam"]);
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
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.Baz).Waldo);
            Assert.Single(foo.BazArray);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazArray[0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazArray[0]).Waldo);
            Assert.Single(foo.BazList);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazList[0]);
            Assert.Equal(-456.78, ((DefaultHasNoDefaultType)foo.BazList[0]).Waldo);
            Assert.Single(foo.BazDictionary);
            Assert.IsType<DefaultHasNoDefaultType>(foo.BazDictionary["spam"]);
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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:qux", "true" },
                    { "foo:bar:garply", "123.45" },
                    { "foo:bar:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = new DefaultTypes { { typeof(HasReadWriteConcreteProperties), "bar", typeof(InheritedHasReadWriteProperties) } };
            var foo = fooSection.Create<HasReadWriteConcreteProperties>(defaultTypes: defaultTypes);

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedHasReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedHasReadWriteProperties)foo.Bar;
            Assert.Equal("But I don't LIKE Spam!", inheritedBar.Spam);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void PassingDefaultTypesOverridesNonTypeSpecifiedMembersOfTheTargetType()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:qux", "true" },
                    { "foo:bar:garply", "123.45" },
                    { "foo:bar:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = new DefaultTypes { { typeof(HasReadWriteProperties), typeof(InheritedHasReadWriteProperties) } };
            var foo = fooSection.Create<HasReadWriteConcreteProperties>(defaultTypes: defaultTypes);

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedHasReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedHasReadWriteProperties)foo.Bar;
            Assert.Equal("But I don't LIKE Spam!", inheritedBar.Spam);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void PassingDefaultTypesDoesNotOverrideTypeSpecifiedMember()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(HasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = new DefaultTypes { { typeof(HasReadWriteConcreteProperties), "bar", typeof(InheritedHasReadWriteProperties) } };
            var foo = fooSection.Create<HasReadWriteConcreteProperties>(defaultTypes: defaultTypes);

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<HasReadWriteProperties>(foo.Bar);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void PassingDefaultTypesDoesNotOverrideTypeSpecifiedMembersOfTheTargetType()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(HasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = new DefaultTypes { { typeof(HasReadWriteProperties), typeof(InheritedHasReadWriteProperties) } };
            var foo = fooSection.Create<HasReadWriteConcreteProperties>(defaultTypes: defaultTypes);

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<HasReadWriteProperties>(foo.Bar);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void PassingValueConvertersOverridesDefaultConversionWhenThereIsAMatch()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "123.45" },
               })
               .Build();

            var fooSection = config.GetSection("foo");

            var valueConverters = new ValueConverters()
                .Add(typeof(double), value => double.Parse(value) * 2);

            var foo = fooSection.Create<DoubleContainer>(valueConverters: valueConverters);

            Assert.Equal(123.45 * 2, foo.Bar); // Doubled by the custom converter
        }

        [Fact]
        public void TheValueConverterRegisteredByDeclaringTypeAndMemberNameHasPriorityOverTheValueConverterRegisteredByTargetType()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "123.45" },
               })
               .Build();

            var fooSection = config.GetSection("foo");

            var valueConverters = new ValueConverters()
                .Add(typeof(double), value => double.Parse(value) * 2)
                .Add(typeof(DoubleContainer), "bar", value => double.Parse(value) * 3);

            var foo = fooSection.Create<DoubleContainer>(valueConverters: valueConverters);

            // Tripled by the custom converter registered to the declaring type and member name
            Assert.Equal(123.45 * 3, foo.Bar);

            var anotherFoo = fooSection.Create<AnotherDoubleContainer>(valueConverters: valueConverters);

            // Still matches by target type for other doubles
            Assert.Equal(123.45 * 2, anotherFoo.Bar);
        }

        [Fact]
        public void PassingValueConvertersDoesNotOverrideDefaultConversionWhenThereIsNotAMatch()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "123.45" },
               })
               .Build();

            var fooSection = config.GetSection("foo");

            var valueConverters = new ValueConverters()
                .Add(typeof(AnotherDoubleContainer), "bar", value => double.Parse(value) * 2);

            var foo = fooSection.Create<DoubleContainer>(valueConverters: valueConverters);

            // Not doubled by the custom converter because the declaring types were different.
            Assert.Equal(123.45, foo.Bar);
        }

        [Fact]
        public void TypeSpecificationIsCaseInsensitive()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(HasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:Type", typeof(AlsoHasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:baz:Value:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteInterfaceProperties>();

            Assert.IsType<HasReadWriteProperties>(foo.Bar);
            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<AlsoHasReadWriteProperties>(foo.Baz);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWriteSimpleProperties()
        {
            var now = DateTime.Now;
            var quxType = GetType();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar", "123" },
                    { "foo:baz", now.ToString("O") },
                    { "foo:qux", quxType.AssemblyQualifiedName },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteSimpleProperties>();

            Assert.Equal(123, foo.Bar);
            Assert.Equal(now, foo.Baz);
            Assert.Equal(quxType, foo.Qux);
        }

        [Fact]
        public void CanBindToSimpleConstructorParameters()
        {
            var now = DateTime.Now;
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar", "123" },
                    { "foo:baz", now.ToString("O") },
                    { "foo:qux", "true" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleConstructorParameters>();

            Assert.Equal(123, foo.Bar);
            Assert.Equal(now, foo.Baz);
            Assert.True(foo.Qux);
        }

        [Fact]
        public void CanBindToReadWriteConcreteProperties()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:qux", "true" },
                    { "foo:bar:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>();

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToConcreteConstructorParameters()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:qux", "true" },
                    { "foo:bar:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>();

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToInheritorOfTypeWithConcreteReadWriteProperties()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedHasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:bar:value:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>();

            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedHasReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedHasReadWriteProperties)foo.Bar;
            Assert.Equal("But I don't LIKE Spam!", inheritedBar.Spam);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToInheritorOfTypeWithConcreteConstructorParameters()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedHasConstructorParameters).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:bar:value:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(HasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:type", typeof(AlsoHasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:baz:value:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteInterfaceProperties>();

            Assert.IsType<HasReadWriteProperties>(foo.Bar);
            Assert.True(foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<AlsoHasReadWriteProperties>(foo.Baz);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToInterfaceConstructorParameters()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(HasConstructorParameters).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:type", typeof(AlsoHasConstructorParameters).AssemblyQualifiedName },
                    { "foo:baz:value:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceConstructorParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedHasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>();

            Assert.False(foo.Bar.Qux);
            Assert.Equal(0D, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToConstructorParameterWithoutSpecifyingTheValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedHasDefaultConstructor).AssemblyQualifiedName },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>();

            Assert.True(foo.Bar.Qux);
            Assert.Equal(543.21, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWritePropertyWithANullValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedHasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value", null },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>();

            Assert.False(foo.Bar.Qux);
            Assert.Equal(0D, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToConstructorParameterWithANullValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedHasDefaultConstructor).AssemblyQualifiedName },
                    { "foo:bar:value", null },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>();

            Assert.True(foo.Bar.Qux);
            Assert.Equal(543.21, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWritePropertyWithAnEmptyValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedHasReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value", "" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadWriteConcreteProperties>();

            Assert.False(foo.Bar.Qux);
            Assert.Equal(0D, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToConstructorParameterWithAnEmptyValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedHasDefaultConstructor).AssemblyQualifiedName },
                    { "foo:bar:value", "" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteConstructorParameters>();

            Assert.True(foo.Bar.Qux);
            Assert.Equal(543.21, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWriteSimpleCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasSimpleReadWriteCollectionProperties>();

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
        public void CanBindToReadWriteSimpleCollectionPropertiesWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasSimpleReadWriteCollectionProperties>();

            Assert.Single(foo.Bar);
            Assert.Equal(123, foo.Bar[0]);
            Assert.Single(foo.Baz);
            Assert.Equal(123, foo.Baz[0]);
            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(123, foo.Qux[0]);
            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(123, foo.Garply.First());
            Assert.Single(foo.Grault);
            Assert.Equal(123, foo.Grault.First());
            Assert.Equal(1, foo.Fred.Count);
            Assert.Equal(123, foo.Fred.First());
            Assert.Equal(1, foo.Waldo.Count);
            Assert.Equal(123, foo.Waldo[0]);
        }

        [Fact]
        public void CanBindToReadonlySimpleCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasSimpleReadonlyCollectionProperties>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:baz", "123" },
                    { "foo:qux", "123" },
                    { "foo:garply", "123" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleReadonlyCollectionProperties>();

            Assert.Single(foo.Baz);
            Assert.Equal(123, foo.Baz[0]);
            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(123, foo.Qux[0]);
            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(123, foo.Garply.First());
        }

        [Fact]
        public void CanBindToSimpleCollectionConstructorParameters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasSimpleCollectionConstructorParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasByteCollectionConstructorParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasSimpleCollectionConstructorParameters>();

            Assert.Single(foo.Bar);
            Assert.Equal(123, foo.Bar[0]);
            Assert.Single(foo.Baz);
            Assert.Equal(123, foo.Baz[0]);
            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(123, foo.Qux[0]);
            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(123, foo.Garply.First());
            Assert.Single(foo.Grault);
            Assert.Equal(123, foo.Grault.First());
            Assert.Equal(1, foo.Fred.Count);
            Assert.Equal(123, foo.Fred.First());
            Assert.Equal(1, foo.Waldo.Count);
            Assert.Equal(123, foo.Waldo[0]);
        }

        [Fact]
        public void CanBindToReadWriteConcreteCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasConcreteReadWriteCollectionProperties>();

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
        public void CanBindToByteCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasByteCollectionProperties>();

            Assert.Equal("QmFy", Convert.ToBase64String(foo.Bar));
            Assert.Equal("QmF6", Convert.ToBase64String(foo.Baz.ToArray()));
            Assert.Equal("UXV6", Convert.ToBase64String(foo.Qux.ToArray()));
            Assert.Equal("R2FycGx5", Convert.ToBase64String(foo.Garply.ToArray()));
            Assert.Equal("R3JhdWx0", Convert.ToBase64String(foo.Grault.ToArray()));
            Assert.Equal("RnJlZA==", Convert.ToBase64String(foo.Fred.ToArray()));
            Assert.Equal("V2FsZG8=", Convert.ToBase64String(foo.Waldo.ToArray()));
        }

        [Fact]
        public void CanBindToReadWriteConcreteCollectionPropertiesWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasConcreteReadWriteCollectionProperties>();

            Assert.Single(foo.Bar);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.Single(foo.Grault);
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.Equal(1, foo.Fred.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.Equal(1, foo.Waldo.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
        }

        [Fact]
        public void CanBindToReadonlyConcreteCollectionProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasConcreteReadonlyCollectionProperties>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasNonGenericCollectionProperties>();

            Assert.Equal(2, foo.Bar.Count);
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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:0:baz", "utf-8" },
                    { "foo:bar:1:baz", "utf-32" },
                    { "foo:baz:0:baz", "utf-8" },
                    { "foo:baz:1:baz", "utf-32" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasReadonlyListPropertiesWithInitialItems>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "name", "source1" },
                    { "listeners:0:name", "listener1" },
                    { "listeners:1:name", "listener2" },
                })
                .Build();

            // The TraceSource.Listeners property is readonly, and the TraceListenerCollection class
            // does not have a default constructor, so this is a good example class to use for testing.

            var defaultTypes = new DefaultTypes { { typeof(TraceListener), typeof(DefaultTraceListener) } };

            var traceSource = config.Create<TraceSource>(defaultTypes);

            Assert.Equal("source1", traceSource.Name);
            Assert.Equal(2, traceSource.Listeners.Count);
            Assert.Equal("listener1", traceSource.Listeners[0].Name);
            Assert.Equal("listener2", traceSource.Listeners[1].Name);
        }

        [Fact]
        public void FlagsEnumsSupportCSharpAndVisualBasicEnumDelimiters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar", "Garply, Grault, Corge" },
                    { "foo:baz", "Garply | Grault | Corge" },
                    { "foo:qux", "Garply Or Grault Or Corge" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasFlagsEnumProperties>();

            Assert.Equal(Flags.Garply | Flags.Grault | Flags.Corge, foo.Bar);
            Assert.Equal(Flags.Garply | Flags.Grault | Flags.Corge, foo.Baz);
            Assert.Equal(Flags.Garply | Flags.Grault | Flags.Corge, foo.Qux);
        }

        [Fact]
        public void CanBindToReadonlyConcreteCollectionPropertiesWithSingleNonListItem()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:baz:baz", "utf-8" },
                    { "foo:qux:baz", "utf-8" },
                    { "foo:garply:baz", "utf-8" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteReadonlyCollectionProperties>();

            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
        }

        [Fact]
        public void CanBindToConcreteCollectionConstructorParameters()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasConcreteCollectionConstructorParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasConcreteCollectionConstructorParameters>();

            Assert.Single(foo.Bar);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.Single(foo.Grault);
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.Equal(1, foo.Fred.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.Equal(1, foo.Waldo.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
        }

        [Fact]
        public void CanBindToReadWriteInterfaceCollectionProperties()
        {
            TimeSpan quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:bar:0:value:baz", "utf-8" },
                    { "foo:bar:0:value:qux", quxValue.ToString() },
                    { "foo:bar:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:bar:1:value:baz", "ascii" },

                    { "foo:baz:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:garply:1:value:baz", "ascii" },

                    { "foo:grault:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:grault:0:value:baz", "utf-8" },
                    { "foo:grault:0:value:qux", quxValue.ToString() },
                    { "foo:grault:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:grault:1:value:baz", "ascii" },

                    { "foo:fred:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:fred:0:value:baz", "utf-8" },
                    { "foo:fred:0:value:qux", quxValue.ToString() },
                    { "foo:fred:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:fred:1:value:baz", "ascii" },

                    { "foo:waldo:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:waldo:0:value:baz", "utf-8" },
                    { "foo:waldo:0:value:qux", quxValue.ToString() },
                    { "foo:waldo:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:waldo:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadWriteCollectionProperties>();

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
        public void CanBindToReadWriteInterfaceCollectionPropertiesWithSingleNonListItem()
        {
            TimeSpan quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:bar:value:baz", "utf-8" },
                    { "foo:bar:value:qux", quxValue.ToString() },

                    { "foo:baz:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:value:baz", "utf-8" },
                    { "foo:baz:value:qux", quxValue.ToString() },

                    { "foo:qux:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:qux:value:baz", "utf-8" },
                    { "foo:qux:value:qux", quxValue.ToString() },

                    { "foo:garply:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:garply:value:baz", "utf-8" },
                    { "foo:garply:value:qux", quxValue.ToString() },

                    { "foo:grault:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:grault:value:baz", "utf-8" },
                    { "foo:grault:value:qux", quxValue.ToString() },

                    { "foo:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:fred:value:baz", "utf-8" },
                    { "foo:fred:value:qux", quxValue.ToString() },

                    { "foo:waldo:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:waldo:value:baz", "utf-8" },
                    { "foo:waldo:value:qux", quxValue.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadWriteCollectionProperties>();

            Assert.Single(foo.Bar);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar[0]).Qux);

            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);

            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);

            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply.First()).Qux);

            Assert.Single(foo.Grault);
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Grault.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Grault.First()).Qux);

            Assert.Equal(1, foo.Fred.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Fred.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Fred.First()).Qux);

            Assert.Equal(1, foo.Waldo.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Waldo[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Waldo[0]).Qux);
        }

        [Fact]
        public void CanBindToReadonlyInterfaceCollectionProperties()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:baz:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:garply:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadonlyCollectionProperties>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:baz:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:value:baz", "utf-8" },
                    { "foo:baz:value:qux", quxValue.ToString() },

                    { "foo:qux:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:qux:value:baz", "utf-8" },
                    { "foo:qux:value:qux", quxValue.ToString() },

                    { "foo:garply:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:garply:value:baz", "utf-8" },
                    { "foo:garply:value:qux", quxValue.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadonlyCollectionProperties>();

            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);

            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);

            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply.First()).Qux);
        }

        [Fact]
        public void CanBindToInterfaceCollectionConstructorParameters()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:bar:0:value:baz", "utf-8" },
                    { "foo:bar:0:value:qux", quxValue.ToString() },
                    { "foo:bar:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:bar:1:value:baz", "ascii" },

                    { "foo:baz:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:garply:1:value:baz", "ascii" },

                    { "foo:grault:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:grault:0:value:baz", "utf-8" },
                    { "foo:grault:0:value:qux", quxValue.ToString() },
                    { "foo:grault:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:grault:1:value:baz", "ascii" },

                    { "foo:fred:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:fred:0:value:baz", "utf-8" },
                    { "foo:fred:0:value:qux", quxValue.ToString() },
                    { "foo:fred:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:fred:1:value:baz", "ascii" },

                    { "foo:waldo:0:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:waldo:0:value:baz", "utf-8" },
                    { "foo:waldo:0:value:qux", quxValue.ToString() },
                    { "foo:waldo:1:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:waldo:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceCollectionConstructorParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:bar:value:baz", "utf-8" },
                    { "foo:bar:value:qux", quxValue.ToString() },

                    { "foo:baz:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:value:baz", "utf-8" },
                    { "foo:baz:value:qux", quxValue.ToString() },

                    { "foo:qux:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:qux:value:baz", "utf-8" },
                    { "foo:qux:value:qux", quxValue.ToString() },

                    { "foo:garply:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:garply:value:baz", "utf-8" },
                    { "foo:garply:value:qux", quxValue.ToString() },

                    { "foo:grault:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:grault:value:baz", "utf-8" },
                    { "foo:grault:value:qux", quxValue.ToString() },

                    { "foo:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:fred:value:baz", "utf-8" },
                    { "foo:fred:value:qux", quxValue.ToString() },

                    { "foo:waldo:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:waldo:value:baz", "utf-8" },
                    { "foo:waldo:value:qux", quxValue.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceCollectionConstructorParameters>();

            Assert.Single(foo.Bar);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Bar[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Bar[0]).Qux);

            Assert.Single(foo.Baz);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Baz[0]).Qux);

            Assert.Equal(1, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Qux[0]).Qux);

            Assert.Equal(1, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Garply.First()).Qux);

            Assert.Single(foo.Grault);
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Grault.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Grault.First()).Qux);

            Assert.Equal(1, foo.Fred.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.IsType<DerivedHasSomething>(foo.Fred.First());
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Fred.First()).Qux);

            Assert.Equal(1, foo.Waldo.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.IsType<DerivedHasSomething>(foo.Waldo[0]);
            Assert.Equal(quxValue, ((DerivedHasSomething)foo.Waldo[0]).Qux);
        }

        [Fact]
        public void CanBindToReadWriteSimpleDictionaryProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasSimpleReadWriteDictionaryProperties>();

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
        public void CanBindToReadonlySimpleDictionaryProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred", "123" },
                    { "foo:bar:waldo", "456" },
                    { "foo:baz:fred", "123" },
                    { "foo:baz:waldo", "456" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasSimpleReadonlyDictionaryProperties>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasSimpleDictionaryConstructorParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasConcreteReadWriteDictionaryProperties>();

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
        public void CanBindToReadonlyConcreteDictionaryProperties()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred:baz", "utf-8" },
                    { "foo:bar:waldo:baz", "ascii" },
                    { "foo:baz:fred:baz", "utf-8" },
                    { "foo:baz:waldo:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasConcreteReadonlyDictionaryProperties>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
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
            var foo = fooSection.Create<HasConcreteDictionaryParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:baz:waldo:value:baz", "ascii" },

                    { "foo:qux:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:qux:fred:value:baz", "utf-8" },
                    { "foo:qux:fred:value:qux", quxValue.ToString() },
                    { "foo:qux:waldo:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:qux:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadWriteDictionaryProperties>();

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
        public void CanBindToReadonlyInterfaceDictionaryProperties()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:baz:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceReadonlyDictionaryProperties>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:baz:waldo:value:baz", "ascii" },

                    { "foo:qux:fred:type", typeof(DerivedHasSomething).AssemblyQualifiedName },
                    { "foo:qux:fred:value:baz", "utf-8" },
                    { "foo:qux:fred:value:qux", quxValue.ToString() },
                    { "foo:qux:waldo:type", typeof(HasSomething).AssemblyQualifiedName },
                    { "foo:qux:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<HasInterfaceDictionaryParameters>();

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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "fred", "123.45" },
                })
                .Build();

            var instance = config.Create<IHasDefaultType>();

            Assert.IsType<DefaultHasDefaultType>(instance);
            Assert.Equal(123.45, ((DefaultHasDefaultType)instance).Fred);
        }

        [Fact]
        public void UsesDefaultTypesObjectForTopLevelType()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "waldo", "123.45" },
                })
                .Build();

            var defaults = new DefaultTypes().Add(typeof(IHasNoDefaultType), typeof(DefaultHasNoDefaultType));

            var instance = config.Create<IHasNoDefaultType>(defaults);

            Assert.IsType<DefaultHasNoDefaultType>(instance);
            Assert.Equal(123.45, ((DefaultHasNoDefaultType)instance).Waldo);
        }

        [Fact]
        public void UsesConvertMethodAttributeForTopLevelType()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "fred", "123.45" },
                })
                .Build();

            var instance = config.GetSection("fred").Create<HasConvertMethod>();

            Assert.Equal(123.45, instance.Fred);
        }

        [Fact]
        public void UsesValueConvertersObjectForTopLevelType()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "waldo", "123.45" },
                })
                .Build();

            var valueConverters = new ValueConverters().Add(typeof(HasNoConvertMethod), value => new HasNoConvertMethod { Waldo = double.Parse(value) });

            var instance = config.GetSection("waldo").Create<HasNoConvertMethod>(valueConverters: valueConverters);

            Assert.Equal(123.45, instance.Waldo);
        }
    }

    public class HasSimpleReadWriteDictionaryProperties
    {
        public Dictionary<string, int> Bar { get; set; }
        public IDictionary<string, int> Baz { get; set; }
        public IReadOnlyDictionary<string, int> Qux { get; set; }
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
        public Dictionary<string, HasSomething> Bar { get; set; }
        public IDictionary<string, HasSomething> Baz { get; set; }
        public IReadOnlyDictionary<string, HasSomething> Qux { get; set; }
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
        public Dictionary<string, IHasSomething> Bar { get; set; }
        public IDictionary<string, IHasSomething> Baz { get; set; }
        public IReadOnlyDictionary<string, IHasSomething> Qux { get; set; }
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
        public int[] Bar { get; set; }
        public List<int> Baz { get; set; }
        public IList<int> Qux { get; set; }
        public ICollection<int> Garply { get; set; }
        public IEnumerable<int> Grault { get; set; }
        public IReadOnlyCollection<int> Fred { get; set; }
        public IReadOnlyList<int> Waldo { get; set; }
    }

    public class HasSimpleReadonlyCollectionProperties
    {
        public List<int> Baz { get; } = new List<int>();
        public IList<int> Qux { get; } = new List<int>();
        public ICollection<int> Garply { get; } = new List<int>();
    }

    public class HasSimpleCollectionConstructorParameters
    {
        public HasSimpleCollectionConstructorParameters(
            int[] bar,
            List<int> baz,
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
        public int[] Bar { get; }
        public List<int> Baz { get; }
        public IList<int> Qux { get; }
        public ICollection<int> Garply { get; }
        public IEnumerable<int> Grault { get; }
        public IReadOnlyCollection<int> Fred { get; }
        public IReadOnlyList<int> Waldo { get; }
    }

    public class HasByteCollectionConstructorParameters
    {
        public HasByteCollectionConstructorParameters(
            byte[] bar,
            List<byte> baz,
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
        public HasSomething[] Bar { get; set; }
        public List<HasSomething> Baz { get; set; }
        public IList<HasSomething> Qux { get; set; }
        public ICollection<HasSomething> Garply { get; set; }
        public IEnumerable<HasSomething> Grault { get; set; }
        public IReadOnlyCollection<HasSomething> Fred { get; set; }
        public IReadOnlyList<HasSomething> Waldo { get; set; }
    }

    public class HasByteCollectionProperties
    {
        public byte[] Bar { get; set; }
        public List<byte> Baz { get; set; }
        public IList<byte> Qux { get; set; }
        public ICollection<byte> Garply { get; set; }
        public IEnumerable<byte> Grault { get; set; }
        public IReadOnlyCollection<byte> Fred { get; set; }
        public IReadOnlyList<byte> Waldo { get; set; }
    }

    public class HasConcreteReadonlyCollectionProperties
    {
        public List<HasSomething> Baz { get; } = new List<HasSomething>();
        public IList<HasSomething> Qux { get; } = new List<HasSomething>();
        public ICollection<HasSomething> Garply { get; } = new List<HasSomething>();
    }

    public class HasNonGenericCollectionProperties
    {
        public HasNonGenericCollectionProperties(HasSomethingCollection baz)
        {
            Baz = baz;
        }

        public HasSomethingCollection Bar { get; set; }
        public HasSomethingCollection Baz { get; }
        public HasSomethingCollection Qux { get; } = new HasSomethingCollection();
    }

    public class HasReadonlyListPropertiesWithInitialItems
    {
        public List<HasSomething> Bar { get; } = new List<HasSomething>() { new HasSomething { Baz = Encoding.ASCII } };
        public HasSomethingCollection Baz { get; } = new HasSomethingCollection() { new HasSomething { Baz = Encoding.ASCII } };
    }

    public class HasFlagsEnumProperties
    {
        public Flags Bar { get; set; }
        public Flags Baz { get; set; }
        public Flags Qux { get; set; }
    }

    [Flags]
    public enum Flags
    {
        Garply = 1,
        Grault = 2,
        Corge = 4
    }

    public class HasSomethingCollection : IList
    {
        private readonly List<HasSomething> _list = new List<HasSomething>();

        object IList.this[int index] { get => this[index]; set => this[index] = (HasSomething)value; }

        public HasSomething this[int index] { get => _list[index]; set => _list[index] = value; }

        bool IList.IsFixedSize => ((IList)_list).IsFixedSize;

        bool IList.IsReadOnly => ((IList)_list).IsReadOnly;

        public int Count => _list.Count;

        bool ICollection.IsSynchronized => ((IList)_list).IsSynchronized;

        object ICollection.SyncRoot => ((IList)_list).SyncRoot;

        int IList.Add(object value)
        {
            return Add((HasSomething)value);
        }

        public int Add(HasSomething value)
        {
            return ((IList)_list).Add(value);
        }

        public void Clear()
        {
            _list.Clear();
        }

        bool IList.Contains(object value)
        {
            return Contains((HasSomething)value);
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

        int IList.IndexOf(object value)
        {
            return IndexOf((HasSomething)value);
        }

        public int IndexOf(HasSomething value)
        {
            return _list.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (HasSomething)value);
        }

        public void Insert(int index, HasSomething value)
        {
            _list.Insert(index, value);
        }

        void IList.Remove(object value)
        {
            Remove((HasSomething)value);
        }

        public void Remove(HasSomething value)
        {
            _list.Remove(value);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
    }

    public class HasConcreteCollectionConstructorParameters
    {
        public HasConcreteCollectionConstructorParameters(
            HasSomething[] bar,
            List<HasSomething> baz,
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
        public HasSomething[] Bar { get; }
        public List<HasSomething> Baz { get; }
        public IList<HasSomething> Qux { get; }
        public ICollection<HasSomething> Garply { get; }
        public IEnumerable<HasSomething> Grault { get; }
        public IReadOnlyCollection<HasSomething> Fred { get; }
        public IReadOnlyList<HasSomething> Waldo { get; }
    }

    public class HasInterfaceReadWriteCollectionProperties
    {
        public IHasSomething[] Bar { get; set; }
        public List<IHasSomething> Baz { get; set; }
        public IList<IHasSomething> Qux { get; set; }
        public ICollection<IHasSomething> Garply { get; set; }
        public IEnumerable<IHasSomething> Grault { get; set; }
        public IReadOnlyCollection<IHasSomething> Fred { get; set; }
        public IReadOnlyList<IHasSomething> Waldo { get; set; }
    }

    public class HasInterfaceReadonlyCollectionProperties
    {
        public List<IHasSomething> Baz { get; } = new List<IHasSomething>();
        public IList<IHasSomething> Qux { get; } = new List<IHasSomething>();
        public ICollection<IHasSomething> Garply { get; } = new List<IHasSomething>();
    }

    public class HasInterfaceCollectionConstructorParameters
    {
        public HasInterfaceCollectionConstructorParameters(
            IHasSomething[] bar,
            List<IHasSomething> baz,
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
        public IHasSomething[] Bar { get; }
        public List<IHasSomething> Baz { get; }
        public IList<IHasSomething> Qux { get; }
        public ICollection<IHasSomething> Garply { get; }
        public IEnumerable<IHasSomething> Grault { get; }
        public IReadOnlyCollection<IHasSomething> Fred { get; }
        public IReadOnlyList<IHasSomething> Waldo { get; }
    }

    public interface IHasSomething
    {
        Encoding Baz { get; }
    }

    public class HasSomething : IHasSomething
    {
        public Encoding Baz { get; set; }
    }

    public class DerivedHasSomething : HasSomething
    {
        public TimeSpan Qux { get; set; }
    }

    public class HasReadWriteSimpleProperties
    {
        public int Bar { get; set; }
        public DateTime Baz { get; set; }
        public Type Qux { get; set; }
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
        public HasReadWriteProperties Bar { get; set; }
        public AlsoHasReadWriteProperties Baz { get; set; }
    }

    public class HasReadWriteInterfaceProperties
    {
        public IHasSimpleProperties Bar { get; set; }
        public IAlsoHasSimpleProperties Baz { get; set; }
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
        public string Spam { get; set; }
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
        public Coordinate Bar { get; set; }
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
        public string Bar { get; set; }
    }

    public class HasMembersDecoratedWithDefaultTypeAttribute
    {
        public HasMembersDecoratedWithDefaultTypeAttribute(
            [DefaultType(typeof(DefaultHasDefaultType))] IHasDefaultType graultBar,
            [DefaultType(typeof(DefaultHasDefaultType))] IHasDefaultType[] graultBarArray,
            [DefaultType(typeof(DefaultHasDefaultType))] List<IHasDefaultType> graultBarList,
            [DefaultType(typeof(DefaultHasDefaultType))] Dictionary<string, IHasDefaultType> graultBarDictionary,

            [DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] IAlsoHasNoDefaultType qux,
            [DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] IAlsoHasNoDefaultType[] quxArray,
            [DefaultType(typeof(DefaultIAlsoHasNoDefaultType))] List<IAlsoHasNoDefaultType> quxList,
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

        public IHasDefaultType GarplyBar { get; set; }
        public IHasDefaultType[] GarplyBarArray { get; set; }
        public List<IHasDefaultType> GarplyBarList { get; set; }
        public Dictionary<string, IHasDefaultType> GarplyBarDictionary { get; set; }

        public IHasDefaultType GraultBar { get; }
        public IHasDefaultType[] GraultBarArray { get; }
        public List<IHasDefaultType> GraultBarList { get; }
        public Dictionary<string, IHasDefaultType> GraultBarDictionary { get; }

        public List<IHasDefaultType> BarReadonlyList { get; } = new List<IHasDefaultType>();
        public Dictionary<string, IHasDefaultType> BarReadonlyDictionary { get; } = new Dictionary<string, IHasDefaultType>();

        [DefaultType(typeof(DefaultHasNoDefaultType))] public IHasNoDefaultType Baz { get; set; }
        [DefaultType(typeof(DefaultHasNoDefaultType))] public IHasNoDefaultType[] BazArray { get; set; }
        [DefaultType(typeof(DefaultHasNoDefaultType))] public List<IHasNoDefaultType> BazList { get; set; }
        [DefaultType(typeof(DefaultHasNoDefaultType))] public Dictionary<string, IHasNoDefaultType> BazDictionary { get; set; }

        [DefaultType(typeof(DefaultHasNoDefaultType))] public List<IHasNoDefaultType> BazReadonlyList { get; } = new List<IHasNoDefaultType>();
        [DefaultType(typeof(DefaultHasNoDefaultType))] public Dictionary<string, IHasNoDefaultType> BazReadonlyDictionary { get; } = new Dictionary<string, IHasNoDefaultType>();

        public IAlsoHasNoDefaultType Qux { get; }
        public IAlsoHasNoDefaultType[] QuxArray { get; }
        public List<IAlsoHasNoDefaultType> QuxList { get; }
        public Dictionary<string, IAlsoHasNoDefaultType> QuxDictionary { get; }
    }

    [DefaultType(typeof(DefaultHasDefaultType))]
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
            return new HasConvertMethod { Fred = double.Parse(value) };
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

    public class MembersDecoratedWithLocallyDefinedDefaultTypeAttribute
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

        public IHasLocallyDefinedDefaultType GarplyBar { get; set; }
        public IHasLocallyDefinedDefaultType[] GarplyBarArray { get; set; }
        public List<IHasLocallyDefinedDefaultType> GarplyBarList { get; set; }
        public Dictionary<string, IHasLocallyDefinedDefaultType> GarplyBarDictionary { get; set; }

        public IHasLocallyDefinedDefaultType GraultBar { get; }
        public IHasLocallyDefinedDefaultType[] GraultBarArray { get; }
        public List<IHasLocallyDefinedDefaultType> GraultBarList { get; }
        public Dictionary<string, IHasLocallyDefinedDefaultType> GraultBarDictionary { get; }

        public List<IHasLocallyDefinedDefaultType> BarReadonlyList { get; } = new List<IHasLocallyDefinedDefaultType>();
        public Dictionary<string, IHasLocallyDefinedDefaultType> BarReadonlyDictionary { get; } = new Dictionary<string, IHasLocallyDefinedDefaultType>();

        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public IHasNoDefaultType Baz { get; set; }
        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public IHasNoDefaultType[] BazArray { get; set; }
        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public List<IHasNoDefaultType> BazList { get; set; }
        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public Dictionary<string, IHasNoDefaultType> BazDictionary { get; set; }

        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public List<IHasNoDefaultType> BazReadonlyList { get; } = new List<IHasNoDefaultType>();
        [LocallyDefined.DefaultType(typeof(DefaultHasNoDefaultType))] public Dictionary<string, IHasNoDefaultType> BazReadonlyDictionary { get; } = new Dictionary<string, IHasNoDefaultType>();

        public IAlsoHasNoDefaultType Qux { get; }
        public IAlsoHasNoDefaultType[] QuxArray { get; }
        public List<IAlsoHasNoDefaultType> QuxList { get; }
        public Dictionary<string, IAlsoHasNoDefaultType> QuxDictionary { get; }
    }

    [LocallyDefined.DefaultType(typeof(DefaultHasLocallyDefinedDefaultType))]
    public interface IHasLocallyDefinedDefaultType
    {
    }

    public class DefaultHasLocallyDefinedDefaultType : IHasLocallyDefinedDefaultType
    {
        public double Fred { get; set; }
    }

    [ConvertMethod(nameof(Convert))]
    public class IsDecoratedWithValueConverterAttribute
    {
        private IsDecoratedWithValueConverterAttribute(double value) => Value = value;
        public double Value { get; }
        private static IsDecoratedWithValueConverterAttribute Convert(string value) =>
            new IsDecoratedWithValueConverterAttribute(double.Parse(value) * 5);
    }

    public class HasMembersDecoratedWithValueConverterAttribute
    {
        public HasMembersDecoratedWithValueConverterAttribute(
            [ConvertMethod(nameof(ConvertBar))] double bar) => Bar = bar;

        public double Bar { get; }
        [ConvertMethod(nameof(ConvertBaz))] public IEnumerable<double> Baz { get; set; }
        public Dictionary<string, IsDecoratedWithValueConverterAttribute> Qux { get; } = new Dictionary<string, IsDecoratedWithValueConverterAttribute>();

        private static double ConvertBar(string value) => double.Parse(value) * 2;
        private static double ConvertBaz(string value) => double.Parse(value) * 3;
    }

    [LocallyDefined.ConvertMethod(nameof(Convert))]
    public class IsDecoratedWithLocallyDefinedValueConverterAttribute
    {
        private IsDecoratedWithLocallyDefinedValueConverterAttribute(double value) => Value = value;
        public double Value { get; }
        private static IsDecoratedWithLocallyDefinedValueConverterAttribute Convert(string value) =>
            new IsDecoratedWithLocallyDefinedValueConverterAttribute(double.Parse(value) * 13);
    }

    public class HasMembersDecoratedWithLocallyDefinedValueConverterAttribute
    {
        public HasMembersDecoratedWithLocallyDefinedValueConverterAttribute(
            [LocallyDefined.ConvertMethod(nameof(ConvertBar))] double bar) => Bar = bar;

        public double Bar { get; }
        [LocallyDefined.ConvertMethod(nameof(ConvertBaz))] public IEnumerable<double> Baz { get; set; }
        public Dictionary<string, IsDecoratedWithLocallyDefinedValueConverterAttribute> Qux { get; } = new Dictionary<string, IsDecoratedWithLocallyDefinedValueConverterAttribute>();

        private static double ConvertBar(string value) => double.Parse(value) * 7;
        private static double ConvertBaz(string value) => double.Parse(value) * 11;
    }
}

namespace LocallyDefined
{
    internal class DefaultTypeAttribute : Attribute
    {
        public DefaultTypeAttribute(Type value) => Value = value;
        public Type Value { get; }
    }

    internal class ConvertMethodAttribute : Attribute
    {
        public ConvertMethodAttribute(string convertMethodName) => ConvertMethodName = convertMethodName;
        public string ConvertMethodName { get; }
    }
}
