using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace RockLib.Configuration.UnitTests
{
    public class LateBoundConfigurationSectionTests
    {
        [Fact]
        public void LateBindingWorksWithConfigurationSectionAsExpected()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo1:bar:type", "RockLib.Configuration.UnitTests.DefaultBar, RockLib.Configuration.UnitTests" },
                    { "foo1:bar:value:baz", "123" },
                    { "foo1:bar:value:qux", "543.21" },
                    { "foo2:bar:type", "RockLib.Configuration.UnitTests.AnotherBar, RockLib.Configuration.UnitTests" },
                    { "foo2:bar:value:garplies:0:grault", "abc" },
                    { "foo2:bar:value:garplies:0:thud", "true" },
                    { "foo2:bar:value:garplies:1:grault", "xyz" },
                    { "foo2:bar:value:garplies:1:thud", "false" },
                    { "foo3:bar:type", "RockLib.Configuration.UnitTests.EmptyBar, RockLib.Configuration.UnitTests" },
                    { "foo4:bar:type", "RockLib.Configuration.UnitTests.EmptyBar, RockLib.Configuration.UnitTests" },
                    { "foo4:bar:value", null },
                    { "foo5:bar:type", "RockLib.Configuration.UnitTests.EmptyBar, RockLib.Configuration.UnitTests" },
                    { "foo5:bar:value", "" },
                })
                .Build();

            var foo1 = config.GetSection("foo1").Get<Foo>();
            var bar1 = (DefaultBar)foo1.Bar.CreateInstance();

            Assert.Equal(123, bar1.Baz);
            Assert.Equal(543.21, bar1.Qux);

            var foo2 = config.GetSection("foo2").Get<Foo>();
            var bar2 = (AnotherBar)foo2.Bar.CreateInstance();

            Assert.NotNull(bar2.Garplies);
            Assert.Equal(2, bar2.Garplies.Count);
            Assert.Equal("abc", bar2.Garplies[0].Grault);
            Assert.Equal(true, bar2.Garplies[0].Thud);
            Assert.Equal("xyz", bar2.Garplies[1].Grault);
            Assert.Equal(false, bar2.Garplies[1].Thud);

            var foo3 = config.GetSection("foo3").Get<Foo>();
            var bar3 = foo3.Bar.CreateInstance();
            Assert.IsType<EmptyBar>(bar3);

            var foo4 = config.GetSection("foo4").Get<Foo>();
            var bar4 = foo4.Bar.CreateInstance();
            Assert.IsType<EmptyBar>(bar4);

            var foo5 = config.GetSection("foo5").Get<Foo>();
            var bar5 = foo5.Bar.CreateInstance();
            Assert.IsType<EmptyBar>(bar5);
        }

        [Fact]
        public void MissingTypeThrowsInvalidOperationException()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:value", null },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Get<Foo>();

            var ex = Assert.Throws<InvalidOperationException>(() => foo.Bar.CreateInstance());
            Assert.Contains("The Type property has not been set", ex.Message);
        }

        [Fact]
        public void InvalidTypeThrowsInvalidOperationException()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", "This.Type.Does.Not.Exist, Neither.Does.This.Assembly" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Get<Foo>();

            var ex = Assert.Throws<InvalidOperationException>(() => foo.Bar.CreateInstance());
            Assert.Contains("The type specified by the assembly-qualified name, 'This.Type.Does.Not.Exist, Neither.Does.This.Assembly', could not be found", ex.Message);
        }

        [Fact]
        public void MissingValueThrowsInvalidOperationException()
        {
            // This situation shouldn't happen with a real configuration, but can happen when programatically instantiated.
            var foo = new Foo { Bar = new LateBoundConfigurationSection<IBar> { Type = typeof(EmptyBar).AssemblyQualifiedName } };

            var ex = Assert.Throws<InvalidOperationException>(() => foo.Bar.CreateInstance());
            Assert.Contains("The Value property has not been set", ex.Message);
        }

        [Fact]
        public void TypeWithoutDefaultConstructorThrowsInvalidOperationException()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(BarWithoutDefaultConstructor).AssemblyQualifiedName },
                    { "foo:bar:value:baz", "123" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Get<Foo>();

            var ex = Assert.Throws<InvalidOperationException>(() => foo.Bar.CreateInstance());
            Assert.Contains("The binding `Get(this IConfiguration, Type)` extension method threw an exception", ex.Message);
            Assert.Contains("Attempting to invoke the default constructor of the 'RockLib.Configuration.UnitTests.BarWithoutDefaultConstructor' type with `Activator.CreateInstance(Type)` threw an exception", ex.Message);
        }

        [Fact]
        public void TypeWithoutDefaultConstructorAndNoValueElementsThrowsInvalidOperationException()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(BarWithoutDefaultConstructor).AssemblyQualifiedName },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Get<Foo>();

            var ex = Assert.Throws<InvalidOperationException>(() => foo.Bar.CreateInstance());
            Assert.Contains("The binding `Get(this IConfiguration, Type)` extension method returned null", ex.Message);
            Assert.Contains("Attempting to invoke the default constructor of the 'RockLib.Configuration.UnitTests.BarWithoutDefaultConstructor' type with `Activator.CreateInstance(Type)` threw an exception", ex.Message);
        }

        [Fact]
        public void TypeWithoutDefaultConstructorAndNullValueElementThrowsInvalidOperationException()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(BarWithoutDefaultConstructor).AssemblyQualifiedName },
                    { "foo:bar:value", null },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Get<Foo>();

            var ex = Assert.Throws<InvalidOperationException>(() => foo.Bar.CreateInstance());
            Assert.Contains("The binding `Get(this IConfiguration, Type)` extension method returned null", ex.Message);
            Assert.Contains("Attempting to invoke the default constructor of the 'RockLib.Configuration.UnitTests.BarWithoutDefaultConstructor' type with `Activator.CreateInstance(Type)` threw an exception", ex.Message);
        }

        [Fact]
        public void TypeWithoutDefaultConstructorAndEmptyValueElementThrowsInvalidOperationException()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo:bar:type", typeof(BarWithoutDefaultConstructor).AssemblyQualifiedName },
                    { "foo:bar:value", "" },
                })
                .Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.Get<Foo>();

            var ex = Assert.Throws<InvalidOperationException>(() => foo.Bar.CreateInstance());
            Assert.Contains("The binding `Get(this IConfiguration, Type)` extension method threw an exception", ex.Message);
            Assert.Contains("Attempting to invoke the default constructor of the 'RockLib.Configuration.UnitTests.BarWithoutDefaultConstructor' type with `Activator.CreateInstance(Type)` threw an exception", ex.Message);
        }
    }

    public class Foo
    {
        public LateBoundConfigurationSection<IBar> Bar { get; set; }
    }

    public interface IBar
    {
    }

    public class DefaultBar : IBar
    {
        public int Baz { get; set; }
        public double Qux { get; set; }
    }

    public class AnotherBar : IBar
    {
        public List<Garply> Garplies { get; set; }
    }

    public class EmptyBar : IBar
    {
    }

    public class BarWithoutDefaultConstructor : IBar
    {
        public BarWithoutDefaultConstructor(int baz)
        {
        }
    }

    public class Garply
    {
        public string Grault { get; set; }
        public bool Thud { get; set; }
    }
}
