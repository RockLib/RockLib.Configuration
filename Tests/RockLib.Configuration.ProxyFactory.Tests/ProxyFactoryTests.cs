using Microsoft.Extensions.Configuration;
using RockLib.Configuration.ProxyFactory;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Tests
{
    public class ProxyFactoryTests
    {
        [Fact]
        public void NonGenericCanCreateProxyForReadonlyProperties()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");
            var fooObject = fooSection.CreateProxy(typeof(IReadonlyProperties));

            Assert.IsAssignableFrom<IReadonlyProperties>(fooObject);

            var foo = (IReadonlyProperties)fooObject!;

            Assert.Equal("abcdefg", foo.Bar);
            Assert.Equal(123, foo.Baz);
        }

        [Fact]
        public void NonGenericCanCreateProxyForReadWriteProperties()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");
            var fooObject = fooSection.CreateProxy(typeof(IReadWriteProperties));

            Assert.IsAssignableFrom<IReadWriteProperties>(fooObject);

            var foo = (IReadWriteProperties)fooObject!;

            Assert.Equal("abcdefg", foo.Bar);
            Assert.Equal(123, foo.Baz);

            // Make sure getters and setters work as expected.
            foo.Bar = "wxyz";
            foo.Baz = 789;

            Assert.Equal("wxyz", foo.Bar);
            Assert.Equal(789, foo.Baz);
        }

        [Fact]
        public void GenericCanCreateProxyForReadonlyProperties()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.CreateProxy<IReadonlyProperties>();

            Assert.Equal("abcdefg", foo!.Bar);
            Assert.Equal(123, foo.Baz);
        }

        [Fact]
        public void GenericCanCreateProxyForReadWriteProperties()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");
            var foo = fooSection.CreateProxy<IReadWriteProperties>();

            Assert.Equal("abcdefg", foo!.Bar);
            Assert.Equal(123, foo.Baz);

            // Make sure getters and setters work as expected.
            foo.Bar = "wxyz";
            foo.Baz = 789;

            Assert.Equal("wxyz", foo.Bar);
            Assert.Equal(789, foo.Baz);
        }

        [Fact]
        public void NonGenericGivenNullConfigurationThrowsArgumentNullException()
        {
            IConfiguration fooSection = null!;

            Assert.Throws<ArgumentNullException>(() => fooSection.CreateProxy(typeof(IReadonlyProperties)));
        }

        [Fact]
        public void NonGenericGivenNullTypeThrowsArgumentNullException()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");

            Assert.Throws<ArgumentNullException>(() => fooSection.CreateProxy(null!));
        }

        [Fact]
        public void GenericGivenNullConfigurationThrowsArgumentNullException()
        {
            IConfiguration fooSection = null!;

            Assert.Throws<ArgumentNullException>(() => fooSection.CreateProxy<IReadonlyProperties>());
        }

        [Fact]
        public void GivenTypeIsNotAnInterfaceThrowsArgumentException()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");

            var ex = Assert.Throws<ArgumentException>(() => fooSection.CreateProxy<NotAnInterface>());

#if DEBUG
            var expected = Exceptions.CannotCreateProxyOfNonInterfaceType(typeof(NotAnInterface));
            Assert.Equal(expected.Message, ex.Message);
#endif
        }

        [Fact]
        public void GivenInterfaceTypeDefinesAMethodThrowsArgumentException()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");

            var ex = Assert.Throws<ArgumentException>(() => fooSection.CreateProxy<IHasMethod>());

#if DEBUG
            var expected = Exceptions.TargetInterfaceCannotHaveAnyMethods(typeof(IHasMethod), typeof(IHasMethod).GetTypeInfo().GetMethod("Foo"));
            Assert.Equal(expected.Message, ex.Message);
#endif
        }

        [Fact]
        public void GivenInterfaceTypeDefinesAnEventThrowsArgumentException()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");

            var ex = Assert.Throws<ArgumentException>(() => fooSection.CreateProxy<IHasEvent>());

#if DEBUG
            var expected = Exceptions.TargetInterfaceCannotHaveAnyEvents(typeof(IHasEvent), typeof(IHasEvent).GetTypeInfo().GetEvent("Foo"));
            Assert.Equal(expected.Message, ex.Message);
#endif
        }

        [Fact]
        public void GivenInterfaceTypeDefinesAnIndexerPropertyThrowsArgumentException()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");

            var ex = Assert.Throws<ArgumentException>(() => fooSection.CreateProxy<IHasIndexerProperty>());

#if DEBUG
            var expected = Exceptions.TargetInterfaceCannotHaveAnyIndexerProperties(typeof(IHasIndexerProperty), typeof(IHasIndexerProperty).GetTypeInfo().GetProperties()[0]);
            Assert.Equal(expected.Message, ex.Message);
#endif
        }

        [Fact]
        public void GivenInterfaceTypeDefinesAWriteOnlyPropertyThrowsArgumentException()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");

            var ex = Assert.Throws<ArgumentException>(() => fooSection.CreateProxy<IHasWriteOnlyProperty>());

#if DEBUG
            var expected = Exceptions.TargetInterfaceCannotHaveAnyWriteOnlyProperties(typeof(IHasWriteOnlyProperty), typeof(IHasWriteOnlyProperty).GetTypeInfo().GetProperty("Foo"));
            Assert.Equal(expected.Message, ex.Message);
#endif
        }

        private interface IReadonlyProperties
        {
            string Bar { get; }
            int Baz { get; }
        }

        private interface IReadWriteProperties
        {
            string Bar { get; set; }
            int Baz { get; set; }
        }

        private class NotAnInterface { }

        private interface IHasMethod
        {
            void Foo();
        }

        private interface IHasEvent
        {
            event EventHandler Foo;
        }

        private interface IHasIndexerProperty
        {
            int this[string foo] { get; set; }
        }

        private interface IHasWriteOnlyProperty
        {
            int Foo { set; }
        }
    }
}
