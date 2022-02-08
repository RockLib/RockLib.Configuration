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
        public void NonGeneric_CanCreateProxyForReadonlyProperties()
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
        public void NonGeneric_CanCreateProxyForReadWriteProperties()
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
        public void Generic_CanCreateProxyForReadonlyProperties()
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
        public void Generic_CanCreateProxyForReadWriteProperties()
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
        public void NonGeneric_GivenNullConfiguration_ThrowsArgumentNullException()
        {
            IConfiguration fooSection = null!;

            Assert.Throws<ArgumentNullException>(() => fooSection.CreateProxy(typeof(IReadonlyProperties)));
        }

        [Fact]
        public void NonGeneric_GivenNullType_ThrowsArgumentNullException()
        {
            var config = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string>
               {
                    { "foo:bar", "abcdefg" },
                    { "foo:baz", "123" },
               }).Build();

            var fooSection = config.GetSection("foo");

            Assert.Throws<ArgumentNullException>(() => fooSection.CreateProxy(null));
        }

        [Fact]
        public void Generic_GivenNullConfiguration_ThrowsArgumentNullException()
        {
            IConfiguration fooSection = null!;

            Assert.Throws<ArgumentNullException>(() => fooSection.CreateProxy<IReadonlyProperties>());
        }

        [Fact]
        public void GivenTypeIsNotAnInterface_ThrowsArgumentException()
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
        public void GivenInterfaceTypeDefinesAMethod_ThrowsArgumentException()
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
        public void GivenInterfaceTypeDefinesAnEvent_ThrowsArgumentException()
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
        public void GivenInterfaceTypeDefinesAnIndexerProperty_ThrowsArgumentException()
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
        public void GivenInterfaceTypeDefinesAWriteOnlyProperty_ThrowsArgumentException()
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

        public interface IReadonlyProperties
        {
            string Bar { get; }
            int Baz { get; }
        }

        public interface IReadWriteProperties
        {
            string Bar { get; set; }
            int Baz { get; set; }
        }

        public class NotAnInterface { }

        public interface IHasMethod
        {
            void Foo();
        }

        public interface IHasEvent
        {
            event EventHandler Foo;
        }

        public interface IHasIndexerProperty
        {
            int this[string foo] { get; set; }
        }

        public interface IHasWriteOnlyProperty
        {
            int Foo { set; }
        }
    }
}
