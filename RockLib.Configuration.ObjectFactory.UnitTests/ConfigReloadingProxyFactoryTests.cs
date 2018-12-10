using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests
{
    public class ConfigReloadingProxyFactoryTests
    {
        [Fact]
        public void NullConfigurationThrowsArgumentNullException()
        {
            IConfiguration configuration = null;
            Type interfaceType = typeof(IFoo);

            Assert.Throws<ArgumentNullException>(() => configuration.CreateReloadingProxy(interfaceType));
        }

        [Fact]
        public void NullInterfaceTypeThrowsArgumentNullException()
        {
            IConfiguration configuration = GetConfig();
            Type interfaceType = null;

            Assert.Throws<ArgumentNullException>(() => configuration.CreateReloadingProxy(interfaceType));
        }

        [Fact]
        public void NonInterfaceInterfaceTypeThrowsArgumentException()
        {
            IConfiguration configuration = GetConfig();
            Type interfaceType = typeof(int);

            Assert.Throws<ArgumentException>(() => configuration.CreateReloadingProxy(interfaceType));
        }

        [Fact]
        public void EnumerableInterfaceTypeThrowsArgumentException()
        {
            IConfiguration configuration = GetConfig();
            Type interfaceType = typeof(IEnumerable<int>);

            Assert.Throws<ArgumentException>(() => configuration.CreateReloadingProxy(interfaceType));
        }

        [Fact]
        public void NullConfigurationThrowsArgumentNullExceptionGeneric()
        {
            IConfiguration configuration = null;

            Assert.Throws<ArgumentNullException>(() => configuration.CreateReloadingProxy<IFoo>());
        }

        [Fact]
        public void NonInterfaceInterfaceTypeThrowsArgumentExceptionGeneric()
        {
            IConfiguration configuration = GetConfig();

            Assert.Throws<ArgumentException>(() => configuration.CreateReloadingProxy<int>());
        }

        [Fact]
        public void EnumerableInterfaceTypeThrowsArgumentExceptionGeneric()
        {
            IConfiguration configuration = GetConfig();

            Assert.Throws<ArgumentException>(() => configuration.CreateReloadingProxy<IEnumerable<int>>());
        }

        [Fact]
        public void PropertiesWork()
        {
            IConfigurationRoot configuration = GetConfig();

            IFoo foo = configuration.GetSection("foo").CreateReloadingProxy<IFoo>();

            Assert.Equal(123, foo.Bar);

            ChangeConfig(configuration);

            Assert.Equal(456, foo.Bar);
        }

        [Fact]
        public void MethodsWork()
        {
            IConfigurationRoot configuration = GetConfig();

            IFoo foo = configuration.GetSection("foo").CreateReloadingProxy<IFoo>();

            Assert.Equal(123 * 2, foo.Baz());

            ChangeConfig(configuration);

            Assert.Equal(456 * 2, foo.Baz());
        }

        [Fact]
        public void EventsWork()
        {
            IConfigurationRoot configuration = GetConfig();

            IBar bar = configuration.GetSection("bar").CreateReloadingProxy<IBar>();

            int qux = -1;

            bar.Baz += (s, e) =>
            {
                qux = bar.Qux;
            };

            ((Bar)((ConfigReloadingProxy<IBar>)bar).Object).OnBaz();

            Assert.Equal(5, qux);
            
            ChangeConfig(configuration);

            qux = -1;

            ((Bar)((ConfigReloadingProxy<IBar>)bar).Object).OnBaz();

            Assert.Equal(10, qux);
        }

        [Fact]
        public void DisposableImplementationsAreDisposed()
        {
            IConfigurationRoot configuration = GetConfig();

            IBaz baz = configuration.GetSection("baz").CreateReloadingProxy<IBaz>();

            Baz initialBaz = (Baz)((ConfigReloadingProxy<IBaz>)baz).Object;

            Assert.False(initialBaz.IsDisposed);

            ChangeConfig(configuration);

            Baz changedBaz = (Baz)((ConfigReloadingProxy<IBaz>)baz).Object;

            Assert.False(changedBaz.IsDisposed);
            Assert.True(initialBaz.IsDisposed);

            ((IDisposable)baz).Dispose();

            Assert.True(changedBaz.IsDisposed);
        }

        [Fact]
        public void ReadWriteReferenceTypePropertiesCopyTheOldValueWhenTheValueIsNotSpecifiedInChangedConfig()
        {
            IConfigurationRoot configuration = GetConfig();

            IFoo foo = configuration.GetSection("foo").CreateReloadingProxy<IFoo>();
            foo.Qux = "abc";

            ChangeConfig(configuration);

            Assert.Equal("abc", foo.Qux);
        }

        [Fact]
        public void ReadWriteReferenceTypePropertiesDoNotCopyTheOldValueWhenTheValueIsSpecifiedInChangedConfig()
        {
            IConfigurationRoot configuration = GetConfig();

            IFoo foo = configuration.GetSection("foo").CreateReloadingProxy<IFoo>();
            foo.Qux = "abc";

            ChangeConfig(configuration, new KeyValuePair<string, string>("foo:value:qux", "xyz"));

            Assert.Equal("xyz", foo.Qux);
        }

        [Fact]
        public void SettingReloadOnChangeToFalseCausesTheProxyToStopReloading()
        {
            IConfigurationRoot configuration = GetConfig();

            IFoo foo = configuration.GetSection("foo").CreateReloadingProxy<IFoo>();

            ChangeConfig(configuration, new KeyValuePair<string, string>("foo:reloadOnChange", "false"));

            Assert.Equal(123, foo.Bar);
        }

        [Fact]
        public void ReloadMethodForcesTheUnderlyingObjectToReload()
        {
            IConfigurationRoot configuration = GetConfig();

            var foo = (ConfigReloadingProxy<IFoo>)configuration.GetSection("foo").CreateReloadingProxy<IFoo>();

            var initialObject = foo.Object;

            foo.Reload();

            var reloadedObject = foo.Object;

            Assert.NotSame(initialObject, reloadedObject);
        }

        [Fact]
        public void AnInitialReloadOnChangeOfFalseDoesNotCreateProxy()
        {
            IConfigurationRoot configuration = GetConfig(new KeyValuePair<string, string>("foo:reloadOnChange", "false"));

            var foo = configuration.GetSection("foo").CreateReloadingProxy<IFoo>();

            Assert.IsType<Foo>(foo);
        }

        [Fact]
        public void UnrelatedConfigChangeDoesNotCauseReload()
        {
            IConfigurationRoot configuration = GetConfig(new KeyValuePair<string, string>("garply", "abc"));

            var foo = (ConfigReloadingProxy<IFoo>)configuration.GetSection("foo").CreateReloadingProxy<IFoo>();

            var initialObject = foo.Object;

            ChangeConfig(configuration, settings: new Dictionary<string, string> { ["garply"] = "xyz" });

            var reloadedObject = foo.Object;

            Assert.Same(initialObject, reloadedObject);
        }

        [Fact]
        public void ReturnedObjectsImplementIConfigReloadingProxyInterface()
        {
            IConfigurationRoot configuration = GetConfig();

            IFoo foo = configuration.GetSection("foo").CreateReloadingProxy<IFoo>();

            ConfigReloadingProxy<IFoo> proxyFoo = (ConfigReloadingProxy<IFoo>)foo;

            var initialObject = proxyFoo.Object;
            Assert.IsType<Foo>(initialObject);

            IFoo reloadingFoo = null;
            IFoo reloadedFoo = null;

            proxyFoo.Reloading += (s, e) => { reloadingFoo = proxyFoo.Object; };
            proxyFoo.Reloaded += (s, e) => { reloadedFoo = proxyFoo.Object; };

            ChangeConfig(configuration, new KeyValuePair<string, string>("foo:value:qux", "xyz"));

            var changedObject = proxyFoo.Object;

            Assert.NotSame(initialObject, changedObject);
            Assert.Same(initialObject, reloadingFoo);
            Assert.Same(changedObject, reloadedFoo);
        }

        [Fact]
        public void SpecifyingReloadOnChangeInConfigCausesCreateExtensionToReturnReloadingProxy()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["foo:type"] = typeof(Foo).AssemblyQualifiedName,
                ["foo:value:bar"] = "123",
                ["foo:reloadOnChange"] = "true",
            }).Build();

            var foo = config.GetSection("foo").Create<IFoo>();

            Assert.IsAssignableFrom<ConfigReloadingProxy<IFoo>>(foo);
        }

        [Fact]
        public void SpecifyingReloadOnChangeInConfigCausesCreateExtensionToReturnReloadingProxy2()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["foo:value:bar"] = "123",
                ["foo:reloadOnChange"] = "true",
            }).Build();

            var foo = config.GetSection("foo").Create<IFoo>(new DefaultTypes().Add(typeof(IFoo), typeof(Foo)));

            Assert.IsAssignableFrom<ConfigReloadingProxy<IFoo>>(foo);
        }

        private static IConfigurationRoot GetConfig(params KeyValuePair<string, string>[] additionalSettings)
        {
            var initialData = new Dictionary<string, string>
            {
                ["foo:type"] = typeof(Foo).AssemblyQualifiedName,
                ["foo:value:bar"] = "123",
                ["bar:type"] = typeof(Bar).AssemblyQualifiedName,
                ["bar:value:qux"] = "5",
                ["baz:type"] = typeof(Baz).AssemblyQualifiedName,
                ["baz:value:qux"] = "7",
            };
            foreach (var setting in additionalSettings)
                initialData.Add(setting.Key, setting.Value);
            return new ConfigurationBuilder().AddInMemoryCollection(initialData).Build();
        }

        private static void ChangeConfig(IConfigurationRoot root, params KeyValuePair<string, string>[] additionalSettings)
        {
            ChangeConfig(root, new Dictionary<string, string> { { "foo:value:bar", "456" }, { "bar:value:qux", "10" }, { "baz:value:foo", "11" } }.Concat(additionalSettings));
        }

        private static void ChangeConfig(IConfigurationRoot root, IEnumerable<KeyValuePair<string, string>> settings)
        {
            var provider = (MemoryConfigurationProvider)root.Providers.First();
            foreach (var setting in settings)
                provider.Set(setting.Key, setting.Value);
            var _onReloadMethod = typeof(ConfigurationProvider).GetMethod("OnReload", BindingFlags.Instance | BindingFlags.NonPublic);
            _onReloadMethod.Invoke(provider, null);
        }

        public interface IFooBase
        {
            int Bar { get; }
        }

        public interface IFoo : IFooBase
        {
            int Baz();
            string Qux { get; set; }
        }

        public class Foo : IFoo
        {
            public Foo(int bar)
            {
                Bar = bar;
            }

            public int Bar { get; }
            public int Baz() => Bar * 2;
            public string Qux { get; set; }
        }

        public interface IBar
        {
            int Qux { get; }
            event EventHandler Baz;
        }

        public class Bar : IBar
        {
            public Bar(int qux)
            {
                Qux = qux;
            }

            public int Qux { get; }

            public event EventHandler Baz;

            public void OnBaz()
            {
                Baz?.Invoke(this, EventArgs.Empty);
            }
        }

        public interface IBaz
        {
            int Foo { get; set; }
            bool IsDisposed { get; }
        }

        public class Baz : IBaz, IDisposable
        {
            public int Foo { get; set; }
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
