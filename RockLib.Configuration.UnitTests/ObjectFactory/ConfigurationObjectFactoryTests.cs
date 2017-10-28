using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Tests
{
    public class ConfigurationObjectFactoryTests
    {
        [Fact]
        public void CanSpecifyDefaultTypesWithDefaultTypeAttribute()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred", "123.45" },
                    { "foo:baz:waldo", "-456.78" },
                    { "foo:qux:thud", "987.65" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithMembersDecoratedWithDefaultTypeAttribute>();

            Assert.IsType<DefultBarWithDefaultType>(foo.Bar);
            Assert.Equal(123.45, ((DefultBarWithDefaultType)foo.Bar).Fred);

            Assert.IsType<DefaultBazWithoutDefaultType>(foo.Baz);
            Assert.Equal(-456.78, ((DefaultBazWithoutDefaultType)foo.Baz).Waldo);

            Assert.IsType<DefaultQuxWithoutDefaultType>(foo.Qux);
            Assert.Equal(987.65, ((DefaultQuxWithoutDefaultType)foo.Qux).Thud);
        }

        [Fact]
        public void CanSpecifyDefaultTypesWithLocallyDefinedDefaultTypeAttribute()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred", "123.45" },
                    { "foo:baz:waldo", "-456.78" },
                    { "foo:qux:thud", "987.65" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithMembersDecoratedWithLocallyDefinedDefaultTypeAttribute>();

            Assert.IsType<DefultBarWithLocallyDefinedDefaultType>(foo.Bar);
            Assert.Equal(123.45, ((DefultBarWithLocallyDefinedDefaultType)foo.Bar).Fred);

            Assert.IsType<DefaultBazWithoutDefaultType>(foo.Baz);
            Assert.Equal(-456.78, ((DefaultBazWithoutDefaultType)foo.Baz).Waldo);

            Assert.IsType<DefaultQuxWithoutDefaultType>(foo.Qux);
            Assert.Equal(987.65, ((DefaultQuxWithoutDefaultType)foo.Qux).Thud);
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
            var defaultTypes = DefaultTypes.New(typeof(FooWithReadWriteConcreteProperties), "bar", typeof(InheritedBarWithReadWriteProperties));
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>(defaultTypes: defaultTypes);

            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedBarWithReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedBarWithReadWriteProperties)foo.Bar;
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
            var defaultTypes = DefaultTypes.New(typeof(BarWithReadWriteProperties), typeof(InheritedBarWithReadWriteProperties));
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>(defaultTypes: defaultTypes);

            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedBarWithReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedBarWithReadWriteProperties)foo.Bar;
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
                    { "foo:bar:type", typeof(BarWithReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = DefaultTypes.New(typeof(FooWithReadWriteConcreteProperties), "bar", typeof(InheritedBarWithReadWriteProperties));
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>(defaultTypes: defaultTypes);

            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<BarWithReadWriteProperties>(foo.Bar);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void PassingDefaultTypesDoesNotOverrideTypeSpecifiedMembersOfTheTargetType()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(BarWithReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var defaultTypes = DefaultTypes.New(typeof(BarWithReadWriteProperties), typeof(InheritedBarWithReadWriteProperties));
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>(defaultTypes: defaultTypes);

            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<BarWithReadWriteProperties>(foo.Bar);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void PassingConvertFuncOverridesDefaultConversion()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "(123.45, -456.78)" },
               })
               .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithCoordinate>((value, targetType, declaringType, memberName) =>
            {
                // Make sure we're getting all the right stuff.
                Assert.Equal("(123.45, -456.78)", value);
                Assert.Equal(typeof(Coordinate), targetType);
                Assert.Equal(typeof(FooWithCoordinate), declaringType);
                Assert.Equal("Bar", memberName);
                return new Coordinate { Latitude = 111.11, Longitude = -222.22 };
            });

            Assert.Equal(111.11, foo.Bar.Latitude);
            Assert.Equal(-222.22, foo.Bar.Longitude);
        }

        [Fact]
        public void PassingConvertFuncDoesNotOverrideDefaultConversionWhenItReturnsNull()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "123.45" },
               })
               .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithDouble>((value, targetType, declaringType, memberName) =>
            {
                // Make sure we're getting all the right stuff.
                Assert.Equal("123.45", value);
                Assert.Equal(typeof(double), targetType);
                Assert.Equal(typeof(FooWithDouble), declaringType);
                Assert.Equal("Bar", memberName);
                return null;
            });

            // The default convertion works.
            Assert.Equal(123.45, foo.Bar);
        }

        [Fact]
        public void CanBindToReadWriteSimpleProperties()
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
            var foo = fooSection.Create<FooWithReadWriteSimpleProperties>();

            Assert.Equal(123, foo.Bar);
            Assert.Equal(now, foo.Baz);
            Assert.Equal(true, foo.Qux);
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
            var foo = fooSection.Create<FooWithSimpleConstructorParameters>();

            Assert.Equal(123, foo.Bar);
            Assert.Equal(now, foo.Baz);
            Assert.Equal(true, foo.Qux);
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
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>();

            Assert.Equal(true, foo.Bar.Qux);
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
            var foo = fooSection.Create<FooWithConcreteConstructorParameters>();

            Assert.Equal(true, foo.Bar.Qux);
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
                    { "foo:bar:type", typeof(InheritedBarWithReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:bar:value:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>();

            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedBarWithReadWriteProperties>(foo.Bar);
            var inheritedBar = (InheritedBarWithReadWriteProperties)foo.Bar;
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
                    { "foo:bar:type", typeof(InheritedBarWithConstructorParameters).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:bar:value:spam", "But I don't LIKE Spam!" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithConcreteConstructorParameters>();

            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<InheritedBarWithConstructorParameters>(foo.Bar);
            var inheritedBar = (InheritedBarWithConstructorParameters)foo.Bar;
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
                    { "foo:bar:type", typeof(BarWithReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:type", typeof(BazWithReadWriteProperties).AssemblyQualifiedName },
                    { "foo:baz:value:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithReadWriteInterfaceProperties>();

            Assert.IsType<BarWithReadWriteProperties>(foo.Bar);
            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<BazWithReadWriteProperties>(foo.Baz);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToInterfaceConstructorParameters()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(BarWithConstructorParameters).AssemblyQualifiedName },
                    { "foo:bar:value:qux", "true" },
                    { "foo:bar:value:garply", "123.45" },
                    { "foo:baz:type", typeof(BazWithConstructorParameters).AssemblyQualifiedName },
                    { "foo:baz:value:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithInterfaceConstructorParameters>();

            Assert.IsType<BarWithConstructorParameters>(foo.Bar);
            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(123.45, foo.Bar.Garply);
            Assert.IsType<BazWithConstructorParameters>(foo.Baz);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWritePropertyWithoutSpecifyingTheValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedBarWithReadWriteProperties).AssemblyQualifiedName },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>();

            Assert.Equal(false, foo.Bar.Qux);
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
                    { "foo:bar:type", typeof(InheritedBarWithDefaultConstructor).AssemblyQualifiedName },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithConcreteConstructorParameters>();

            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(543.21, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWritePropertyANullValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedBarWithReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value", null },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>();

            Assert.Equal(false, foo.Bar.Qux);
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
                    { "foo:bar:type", typeof(InheritedBarWithDefaultConstructor).AssemblyQualifiedName },
                    { "foo:bar:value", null },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithConcreteConstructorParameters>();

            Assert.Equal(true, foo.Bar.Qux);
            Assert.Equal(543.21, foo.Bar.Garply);
            Assert.Equal(guid, foo.Baz.Grault);
        }

        [Fact]
        public void CanBindToReadWritePropertyAnEmptyValue()
        {
            var guid = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(InheritedBarWithReadWriteProperties).AssemblyQualifiedName },
                    { "foo:bar:value", "" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithReadWriteConcreteProperties>();

            Assert.Equal(false, foo.Bar.Qux);
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
                    { "foo:bar:type", typeof(InheritedBarWithDefaultConstructor).AssemblyQualifiedName },
                    { "foo:bar:value", "" },
                    { "foo:baz:grault", guid.ToString() },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithConcreteConstructorParameters>();

            Assert.Equal(true, foo.Bar.Qux);
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
            var foo = fooSection.Create<FooWithSimpleReadWriteCollectionProperties>();

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
            var foo = fooSection.Create<FooWithSimpleReadonlyCollectionProperties>();

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
            var foo = fooSection.Create<FooWithSimpleCollectionConstructorParameters>();

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
            var foo = fooSection.Create<FooWithConcreteReadWriteCollectionProperties>();

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
            var foo = fooSection.Create<FooWithConcreteReadonlyCollectionProperties>();

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
            var foo = fooSection.Create<FooWithConcreteCollectionConstructorParameters>();

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
        public void CanBindToReadWriteInterfaceCollectionProperties()
        {
            TimeSpan quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:0:value:baz", "utf-8" },
                    { "foo:bar:0:value:qux", quxValue.ToString() },
                    { "foo:bar:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:1:value:baz", "ascii" },

                    { "foo:baz:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:garply:1:value:baz", "ascii" },

                    { "foo:grault:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:grault:0:value:baz", "utf-8" },
                    { "foo:grault:0:value:qux", quxValue.ToString() },
                    { "foo:grault:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:grault:1:value:baz", "ascii" },

                    { "foo:fred:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:fred:0:value:baz", "utf-8" },
                    { "foo:fred:0:value:qux", quxValue.ToString() },
                    { "foo:fred:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:fred:1:value:baz", "ascii" },

                    { "foo:waldo:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:waldo:0:value:baz", "utf-8" },
                    { "foo:waldo:0:value:qux", quxValue.ToString() },
                    { "foo:waldo:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:waldo:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithInterfaceReadWriteCollectionProperties>();

            Assert.Equal(2, foo.Bar.Length);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Bar[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Bar[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Bar[1]);

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Baz[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Baz[1]);

            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Qux[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Qux[1]);

            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Garply.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
            Assert.IsType<BarWithSomething>(foo.Garply.Skip(1).First());

            Assert.Equal(2, foo.Grault.Count());
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Grault.First());
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Grault.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Grault.Skip(1).First().Baz);
            Assert.IsType<BarWithSomething>(foo.Grault.Skip(1).First());

            Assert.Equal(2, foo.Fred.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Fred.First());
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Fred.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Fred.Skip(1).First().Baz);
            Assert.IsType<BarWithSomething>(foo.Fred.Skip(1).First());

            Assert.Equal(2, foo.Waldo.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Waldo[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Waldo[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Waldo[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Waldo[1]);
        }

        [Fact]
        public void CanBindToReadonlyInterfaceCollectionProperties()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:baz:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:garply:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithInterfaceReadonlyCollectionProperties>();

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Baz[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Baz[1]);

            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Qux[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Qux[1]);

            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Garply.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
            Assert.IsType<BarWithSomething>(foo.Garply.Skip(1).First());
        }

        [Fact]
        public void CanBindToInterfaceCollectionConstructorParameters()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:0:value:baz", "utf-8" },
                    { "foo:bar:0:value:qux", quxValue.ToString() },
                    { "foo:bar:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:1:value:baz", "ascii" },

                    { "foo:baz:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:0:value:baz", "utf-8" },
                    { "foo:baz:0:value:qux", quxValue.ToString() },
                    { "foo:baz:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:1:value:baz", "ascii" },

                    { "foo:qux:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:qux:0:value:baz", "utf-8" },
                    { "foo:qux:0:value:qux", quxValue.ToString() },
                    { "foo:qux:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:qux:1:value:baz", "ascii" },

                    { "foo:garply:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:garply:0:value:baz", "utf-8" },
                    { "foo:garply:0:value:qux", quxValue.ToString() },
                    { "foo:garply:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:garply:1:value:baz", "ascii" },

                    { "foo:grault:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:grault:0:value:baz", "utf-8" },
                    { "foo:grault:0:value:qux", quxValue.ToString() },
                    { "foo:grault:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:grault:1:value:baz", "ascii" },

                    { "foo:fred:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:fred:0:value:baz", "utf-8" },
                    { "foo:fred:0:value:qux", quxValue.ToString() },
                    { "foo:fred:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:fred:1:value:baz", "ascii" },

                    { "foo:waldo:0:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:waldo:0:value:baz", "utf-8" },
                    { "foo:waldo:0:value:qux", quxValue.ToString() },
                    { "foo:waldo:1:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:waldo:1:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithInterfaceCollectionConstructorParameters>();

            Assert.Equal(2, foo.Bar.Length);
            Assert.Equal(Encoding.UTF8, foo.Bar[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Bar[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Bar[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Bar[1]);

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Baz[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Baz[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Baz[1]);

            Assert.Equal(2, foo.Qux.Count);
            Assert.Equal(Encoding.UTF8, foo.Qux[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Qux[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Qux[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Qux[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Qux[1]);

            Assert.Equal(2, foo.Garply.Count);
            Assert.Equal(Encoding.UTF8, foo.Garply.First().Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Garply.First());
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Garply.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Garply.Skip(1).First().Baz);
            Assert.IsType<BarWithSomething>(foo.Garply.Skip(1).First());

            Assert.Equal(2, foo.Grault.Count());
            Assert.Equal(Encoding.UTF8, foo.Grault.First().Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Grault.First());
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Grault.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Grault.Skip(1).First().Baz);
            Assert.IsType<BarWithSomething>(foo.Grault.Skip(1).First());

            Assert.Equal(2, foo.Fred.Count);
            Assert.Equal(Encoding.UTF8, foo.Fred.First().Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Fred.First());
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Fred.First()).Qux);
            Assert.Equal(Encoding.ASCII, foo.Fred.Skip(1).First().Baz);
            Assert.IsType<BarWithSomething>(foo.Fred.Skip(1).First());

            Assert.Equal(2, foo.Waldo.Count);
            Assert.Equal(Encoding.UTF8, foo.Waldo[0].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Waldo[0]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Waldo[0]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Waldo[1].Baz);
            Assert.IsType<BarWithSomething>(foo.Waldo[1]);
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
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithSimpleReadWriteDictionaryProperties>();

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(123, foo.Bar["fred"]);
            Assert.Equal(456, foo.Bar["waldo"]);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(123, foo.Baz["fred"]);
            Assert.Equal(456, foo.Baz["waldo"]);
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
            var foo = fooSection.Create<FooWithSimpleReadonlyDictionaryProperties>();

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
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithSimpleDictionaryConstructorParameters>();

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(123, foo.Bar["fred"]);
            Assert.Equal(456, foo.Bar["waldo"]);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(123, foo.Baz["fred"]);
            Assert.Equal(456, foo.Baz["waldo"]);
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
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithConcreteReadWriteDictionaryProperties>();

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
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
            var foo = fooSection.Create<FooWithConcreteReadonlyDictionaryProperties>();

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
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithConcreteDictionaryParameters>();

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
        }

        [Fact]
        public void CanBindToReadWriteInterfaceDictionaryProperties()
        {
            TimeSpan quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithInterfaceReadWriteDictionaryProperties>();

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Bar["fred"]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Bar["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.IsType<BarWithSomething>(foo.Bar["waldo"]);

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Baz["fred"]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Baz["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
            Assert.IsType<BarWithSomething>(foo.Baz["waldo"]);
        }

        [Fact]
        public void CanBindToReadonlyInterfaceDictionaryProperties()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithInterfaceReadonlyDictionaryProperties>();

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Bar["fred"]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Bar["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.IsType<BarWithSomething>(foo.Bar["waldo"]);

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Baz["fred"]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Baz["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
            Assert.IsType<BarWithSomething>(foo.Baz["waldo"]);
        }

        [Fact]
        public void CanBindToInterfaceDictionaryConstructorParameters()
        {
            var quxValue = TimeSpan.FromMilliseconds(123);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:fred:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:fred:value:baz", "utf-8" },
                    { "foo:bar:fred:value:qux", quxValue.ToString() },
                    { "foo:bar:waldo:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:bar:waldo:value:baz", "ascii" },

                    { "foo:baz:fred:type", typeof(DerivedBarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:fred:value:baz", "utf-8" },
                    { "foo:baz:fred:value:qux", quxValue.ToString() },
                    { "foo:baz:waldo:type", typeof(BarWithSomething).AssemblyQualifiedName },
                    { "foo:baz:waldo:value:baz", "ascii" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Create<FooWithInterfaceDictionaryParameters>();

            Assert.Equal(2, foo.Bar.Count);
            Assert.Equal(Encoding.UTF8, foo.Bar["fred"].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Bar["fred"]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Bar["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Bar["waldo"].Baz);
            Assert.IsType<BarWithSomething>(foo.Bar["waldo"]);

            Assert.Equal(2, foo.Baz.Count);
            Assert.Equal(Encoding.UTF8, foo.Baz["fred"].Baz);
            Assert.IsType<DerivedBarWithSomething>(foo.Baz["fred"]);
            Assert.Equal(quxValue, ((DerivedBarWithSomething)foo.Baz["fred"]).Qux);
            Assert.Equal(Encoding.ASCII, foo.Baz["waldo"].Baz);
            Assert.IsType<BarWithSomething>(foo.Baz["waldo"]);
        }
    }

    public class FooWithSimpleReadWriteDictionaryProperties
    {
        public Dictionary<string, int> Bar { get; set; }
        public IDictionary<string, int> Baz { get; set; }
    }

    public class FooWithSimpleReadonlyDictionaryProperties
    {
        public Dictionary<string, int> Bar { get; } = new Dictionary<string, int>();
        public IDictionary<string, int> Baz { get; } = new Dictionary<string, int>();
    }

    public class FooWithSimpleDictionaryConstructorParameters
    {
        public FooWithSimpleDictionaryConstructorParameters(Dictionary<string, int> bar, IDictionary<string, int> baz)
        {
            Bar = bar;
            Baz = baz;
        }

        public Dictionary<string, int> Bar { get; }
        public IDictionary<string, int> Baz { get; }
    }

    public class FooWithConcreteReadWriteDictionaryProperties
    {
        public Dictionary<string, BarWithSomething> Bar { get; set; }
        public IDictionary<string, BarWithSomething> Baz { get; set; }
    }

    public class FooWithConcreteReadonlyDictionaryProperties
    {
        public Dictionary<string, BarWithSomething> Bar { get; } = new Dictionary<string, BarWithSomething>();
        public IDictionary<string, BarWithSomething> Baz { get; } = new Dictionary<string, BarWithSomething>();
    }

    public class FooWithConcreteDictionaryParameters
    {
        public FooWithConcreteDictionaryParameters(Dictionary<string, BarWithSomething> bar, IDictionary<string, BarWithSomething> baz)
        {
            Bar = bar;
            Baz = baz;
        }

        public Dictionary<string, BarWithSomething> Bar { get; }
        public IDictionary<string, BarWithSomething> Baz { get; }
    }

    public class FooWithInterfaceReadWriteDictionaryProperties
    {
        public Dictionary<string, IBarWithSomething> Bar { get; set; }
        public IDictionary<string, IBarWithSomething> Baz { get; set; }
    }

    public class FooWithInterfaceReadonlyDictionaryProperties
    {
        public Dictionary<string, IBarWithSomething> Bar { get; } = new Dictionary<string, IBarWithSomething>();
        public IDictionary<string, IBarWithSomething> Baz { get; } = new Dictionary<string, IBarWithSomething>();
    }

    public class FooWithInterfaceDictionaryParameters
    {
        public FooWithInterfaceDictionaryParameters(Dictionary<string, IBarWithSomething> bar, IDictionary<string, IBarWithSomething> baz)
        {
            Bar = bar;
            Baz = baz;
        }

        public Dictionary<string, IBarWithSomething> Bar { get; }
        public IDictionary<string, IBarWithSomething> Baz { get; }
    }

    public class FooWithSimpleReadWriteCollectionProperties
    {
        public int[] Bar { get; set; }
        public List<int> Baz { get; set; }
        public IList<int> Qux { get; set; }
        public ICollection<int> Garply { get; set; }
        public IEnumerable<int> Grault { get; set; }
        public IReadOnlyCollection<int> Fred { get; set; }
        public IReadOnlyList<int> Waldo { get; set; }
    }

    public class FooWithSimpleReadonlyCollectionProperties
    {
        public List<int> Baz { get; } = new List<int>();
        public IList<int> Qux { get; } = new List<int>();
        public ICollection<int> Garply { get; } = new List<int>();
    }

    public class FooWithSimpleCollectionConstructorParameters
    {
        public FooWithSimpleCollectionConstructorParameters(
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

    public class FooWithConcreteReadWriteCollectionProperties
    {
        public BarWithSomething[] Bar { get; set; }
        public List<BarWithSomething> Baz { get; set; }
        public IList<BarWithSomething> Qux { get; set; }
        public ICollection<BarWithSomething> Garply { get; set; }
        public IEnumerable<BarWithSomething> Grault { get; set; }
        public IReadOnlyCollection<BarWithSomething> Fred { get; set; }
        public IReadOnlyList<BarWithSomething> Waldo { get; set; }
    }

    public class FooWithConcreteReadonlyCollectionProperties
    {
        public List<BarWithSomething> Baz { get; } = new List<BarWithSomething>();
        public IList<BarWithSomething> Qux { get; } = new List<BarWithSomething>();
        public ICollection<BarWithSomething> Garply { get; } = new List<BarWithSomething>();
    }

    public class FooWithConcreteCollectionConstructorParameters
    {
        public FooWithConcreteCollectionConstructorParameters(
            BarWithSomething[] bar,
            List<BarWithSomething> baz,
            IList<BarWithSomething> qux,
            ICollection<BarWithSomething> garply,
            IEnumerable<BarWithSomething> grault,
            IReadOnlyCollection<BarWithSomething> fred,
            IReadOnlyList<BarWithSomething> waldo)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
            Garply = garply;
            Grault = grault;
            Fred = fred;
            Waldo = waldo;
        }
        public BarWithSomething[] Bar { get; }
        public List<BarWithSomething> Baz { get; }
        public IList<BarWithSomething> Qux { get; }
        public ICollection<BarWithSomething> Garply { get; }
        public IEnumerable<BarWithSomething> Grault { get; }
        public IReadOnlyCollection<BarWithSomething> Fred { get; }
        public IReadOnlyList<BarWithSomething> Waldo { get; }
    }

    public class FooWithInterfaceReadWriteCollectionProperties
    {
        public IBarWithSomething[] Bar { get; set; }
        public List<IBarWithSomething> Baz { get; set; }
        public IList<IBarWithSomething> Qux { get; set; }
        public ICollection<IBarWithSomething> Garply { get; set; }
        public IEnumerable<IBarWithSomething> Grault { get; set; }
        public IReadOnlyCollection<IBarWithSomething> Fred { get; set; }
        public IReadOnlyList<IBarWithSomething> Waldo { get; set; }
    }

    public class FooWithInterfaceReadonlyCollectionProperties
    {
        public List<IBarWithSomething> Baz { get; } = new List<IBarWithSomething>();
        public IList<IBarWithSomething> Qux { get; } = new List<IBarWithSomething>();
        public ICollection<IBarWithSomething> Garply { get; } = new List<IBarWithSomething>();
    }

    public class FooWithInterfaceCollectionConstructorParameters
    {
        public FooWithInterfaceCollectionConstructorParameters(
            IBarWithSomething[] bar,
            List<IBarWithSomething> baz,
            IList<IBarWithSomething> qux,
            ICollection<IBarWithSomething> garply,
            IEnumerable<IBarWithSomething> grault,
            IReadOnlyCollection<IBarWithSomething> fred,
            IReadOnlyList<IBarWithSomething> waldo)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
            Garply = garply;
            Grault = grault;
            Fred = fred;
            Waldo = waldo;
        }
        public IBarWithSomething[] Bar { get; }
        public List<IBarWithSomething> Baz { get; }
        public IList<IBarWithSomething> Qux { get; }
        public ICollection<IBarWithSomething> Garply { get; }
        public IEnumerable<IBarWithSomething> Grault { get; }
        public IReadOnlyCollection<IBarWithSomething> Fred { get; }
        public IReadOnlyList<IBarWithSomething> Waldo { get; }
    }

    public interface IBarWithSomething
    {
        Encoding Baz { get; }
    }

    public class BarWithSomething : IBarWithSomething
    {
        public Encoding Baz { get; set; }
    }

    public class DerivedBarWithSomething : BarWithSomething
    {
        public TimeSpan Qux { get; set; }
    }

    public class FooWithReadWriteSimpleProperties
    {
        public int Bar { get; set; }
        public DateTime Baz { get; set; }
        public bool Qux { get; set; }
    }

    public class FooWithSimpleConstructorParameters
    {
        public FooWithSimpleConstructorParameters(int bar, DateTime baz, bool qux)
        {
            Bar = bar;
            Baz = baz;
            Qux = qux;
        }

        public int Bar { get; }
        public DateTime Baz { get; }
        public bool Qux { get; }
    }

    public class FooWithReadWriteConcreteProperties
    {
        public BarWithReadWriteProperties Bar { get; set; }
        public BazWithReadWriteProperties Baz { get; set; }
    }

    public class FooWithReadWriteInterfaceProperties
    {
        public IBarWithReadWriteProperties Bar { get; set; }
        public IBazWithReadWriteProperties Baz { get; set; }
    }

    public interface IBarWithReadWriteProperties
    {
        bool Qux { get; set; }
        double Garply { get; set; }
    }

    public interface IBazWithReadWriteProperties
    {
        Guid Grault { get; set; }
    }

    public class BarWithReadWriteProperties : IBarWithReadWriteProperties
    {
        public bool Qux { get; set; }
        public double Garply { get; set; }
    }

    public class InheritedBarWithReadWriteProperties : BarWithReadWriteProperties
    {
        public string Spam { get; set; }
    }

    public class BazWithReadWriteProperties : IBazWithReadWriteProperties
    {
        public Guid Grault { get; set; }
    }

    public class FooWithConcreteConstructorParameters
    {
        public FooWithConcreteConstructorParameters(BarWithConstructorParameters bar, BazWithConstructorParameters baz)
        {
            Bar = bar;
            Baz = baz;
        }
        public BarWithConstructorParameters Bar { get; }
        public BazWithConstructorParameters Baz { get; }
    }

    public class FooWithInterfaceConstructorParameters
    {
        public FooWithInterfaceConstructorParameters(IBarWithConstructorParameters bar, IBazWithConstructorParameters baz)
        {
            Bar = bar;
            Baz = baz;
        }
        public IBarWithConstructorParameters Bar { get; }
        public IBazWithConstructorParameters Baz { get; }
    }

    public interface IBarWithConstructorParameters
    {
        bool Qux { get; }
        double Garply { get; }
    }

    public interface IBazWithConstructorParameters
    {
        Guid Grault { get; }
    }

    public class BarWithConstructorParameters : IBarWithConstructorParameters
    {
        public BarWithConstructorParameters(bool qux, double garply)
        {
            Qux = qux;
            Garply = garply;
        }
        public bool Qux { get; }
        public double Garply { get; }
    }

    public class InheritedBarWithConstructorParameters : BarWithConstructorParameters
    {
        public InheritedBarWithConstructorParameters(bool qux, double garply, string spam)
            : base(qux, garply)
        {
            Spam = spam;
        }
        public string Spam { get; }
    }

    public class InheritedBarWithDefaultConstructor : BarWithConstructorParameters
    {
        public InheritedBarWithDefaultConstructor()
            : base(true, 543.21)
        {
        }
    }

    public class BazWithConstructorParameters : IBazWithConstructorParameters
    {
        public BazWithConstructorParameters(Guid grault)
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

    public class FooWithCoordinate
    {
        public Coordinate Bar { get; set; }
    }

    public class FooWithDouble
    {
        public double Bar { get; set; }
    }

    public class FooWithMembersDecoratedWithDefaultTypeAttribute
    {
        public FooWithMembersDecoratedWithDefaultTypeAttribute(
            [DefaultType(typeof(DefaultQuxWithoutDefaultType))] IQuxWithoutDefaultType qux)
        {
            Qux = qux;
        }

        public IBarWithDefaultType Bar { get; set; }

        [DefaultType(typeof(DefaultBazWithoutDefaultType))]
        public IBazWithoutDefaultType Baz { get; set; }

        public IQuxWithoutDefaultType Qux { get; }
    }

    [DefaultType(typeof(DefultBarWithDefaultType))]
    public interface IBarWithDefaultType
    {
    }

    public class DefultBarWithDefaultType : IBarWithDefaultType
    {
        public double Fred { get; set; }
    }

    public interface IBazWithoutDefaultType
    {
    }

    public class DefaultBazWithoutDefaultType : IBazWithoutDefaultType
    {
        public double Waldo { get; set; }
    }

    public interface IQuxWithoutDefaultType
    {
    }

    public class DefaultQuxWithoutDefaultType : IQuxWithoutDefaultType
    {
        public double Thud { get; set; }
    }

    public class FooWithMembersDecoratedWithLocallyDefinedDefaultTypeAttribute
    {
        public FooWithMembersDecoratedWithLocallyDefinedDefaultTypeAttribute(
            [LocallyDefined.DefaultType(typeof(DefaultQuxWithoutDefaultType))] IQuxWithoutDefaultType qux)
        {
            Qux = qux;
        }

        public IBarWithLocallyDefinedDefaultType Bar { get; set; }

        [LocallyDefined.DefaultType(typeof(DefaultBazWithoutDefaultType))]
        public IBazWithoutDefaultType Baz { get; set; }

        public IQuxWithoutDefaultType Qux { get; }
    }

    [LocallyDefined.DefaultType(typeof(DefultBarWithLocallyDefinedDefaultType))]
    public interface IBarWithLocallyDefinedDefaultType
    {
    }

    public class DefultBarWithLocallyDefinedDefaultType : IBarWithLocallyDefinedDefaultType
    {
        public double Fred { get; set; }
    }
}

namespace LocallyDefined
{
    internal class DefaultTypeAttribute : Attribute
    {
        public DefaultTypeAttribute(Type value) => Value = value;
        public Type Value { get; }
    }
}
