#if REFERENCE_MODEL
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using RockLib.Configuration.ObjectFactory;
using RockLib.Configuration.ObjectFactory.ReferenceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests
{
    public class ConfigReloadingProxyReferenceModelTests
    {
        [Fact]
        public void TypeSpecifiedExplicitly()
        {
            var config = GetConfig();

            var garplies = 0;
            var reloading = 0;
            var reloaded = 0;

            IFoo foo = new ProxyFoo(config.GetSection("foo"), new DefaultTypes(), new ValueConverters(), null, null);

            IConfigReloadingProxy<IFoo> proxyFoo = (IConfigReloadingProxy<IFoo>)foo;
            Foo initialFoo = (Foo)proxyFoo.Object;

            foo.Qux = "xyz";
            foo.Garply += (s, e) => { garplies++; };

            proxyFoo.Reloading += (s, e) => { reloading++; };
            proxyFoo.Reloaded += (s, e) => { reloaded++; };

            initialFoo.OnGarply();

            Assert.Equal(123, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(1, garplies);

            Assert.False(initialFoo.IsDisposed);
            Assert.Equal(0, reloading);
            Assert.Equal(0, reloaded);

            ChangeConfig(config);

            Assert.True(initialFoo.IsDisposed);
            Assert.Equal(1, reloading);
            Assert.Equal(1, reloaded);

            Foo changedFoo = (Foo)proxyFoo.Object;

            changedFoo.OnGarply();

            Assert.Equal(456, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(2, garplies);

            Assert.False(changedFoo.IsDisposed);

            ((IDisposable)foo).Dispose();

            Assert.True(changedFoo.IsDisposed);
        }

        [Fact]
        public void TypeSpecifiedByDefaultType()
        {
            var config = GetNonTypeSpecifiedConfig();

            var garplies = 0;
            var reloading = 0;
            var reloaded = 0;

            var defaultTypes = new DefaultTypes().Add(typeof(IFoo), typeof(Foo));

            IFoo foo = new ProxyFoo(config.GetSection("foo"), defaultTypes, new ValueConverters(), null, null);

            IConfigReloadingProxy<IFoo> proxyFoo = (IConfigReloadingProxy<IFoo>)foo;
            Foo initialFoo = (Foo)proxyFoo.Object;

            foo.Qux = "xyz";
            foo.Garply += (s, e) => { garplies++; };

            proxyFoo.Reloading += (s, e) => { reloading++; };
            proxyFoo.Reloaded += (s, e) => { reloaded++; };

            initialFoo.OnGarply();

            Assert.Equal(123, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(1, garplies);

            Assert.False(initialFoo.IsDisposed);
            Assert.Equal(0, reloading);
            Assert.Equal(0, reloaded);

            ChangeNonTypeSpecifiedConfig(config);

            Assert.True(initialFoo.IsDisposed);
            Assert.Equal(1, reloading);
            Assert.Equal(1, reloaded);

            Foo changedFoo = (Foo)proxyFoo.Object;

            changedFoo.OnGarply();

            Assert.Equal(456, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(2, garplies);

            Assert.False(changedFoo.IsDisposed);

            ((IDisposable)foo).Dispose();

            Assert.True(changedFoo.IsDisposed);
        }

        [Fact]
        public void ReloadOnChangeSetAndTypeSpecifiedExplicitly()
        {
            var config = GetReloadingConfig(typeof(Foo));

            var garplies = 0;
            var reloading = 0;
            var reloaded = 0;

            IFoo foo = new ProxyFoo(config.GetSection("foo"), new DefaultTypes(), new ValueConverters(), null, null);

            IConfigReloadingProxy<IFoo> proxyFoo = (IConfigReloadingProxy<IFoo>)foo;
            Foo initialFoo = (Foo)proxyFoo.Object;

            foo.Qux = "xyz";
            foo.Garply += (s, e) => { garplies++; };

            proxyFoo.Reloading += (s, e) => { reloading++; };
            proxyFoo.Reloaded += (s, e) => { reloaded++; };

            initialFoo.OnGarply();

            Assert.Equal(123, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(1, garplies);

            Assert.False(initialFoo.IsDisposed);
            Assert.Equal(0, reloading);
            Assert.Equal(0, reloaded);

            ChangeConfig(config);

            Assert.True(initialFoo.IsDisposed);
            Assert.Equal(1, reloading);
            Assert.Equal(1, reloaded);

            Foo changedFoo = (Foo)proxyFoo.Object;

            changedFoo.OnGarply();

            Assert.Equal(456, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(2, garplies);

            Assert.False(changedFoo.IsDisposed);

            ((IDisposable)foo).Dispose();

            Assert.True(changedFoo.IsDisposed);
        }

        [Fact]
        public void ReloadOnChangeSetAndTypeSpecifiedByDefaultType()
        {
            var config = GetReloadingConfig();

            var garplies = 0;
            var reloading = 0;
            var reloaded = 0;

            var defaultTypes = new DefaultTypes().Add(typeof(IFoo), typeof(Foo));

            IFoo foo = new ProxyFoo(config.GetSection("foo"), defaultTypes, new ValueConverters(), null, null);

            IConfigReloadingProxy<IFoo> proxyFoo = (IConfigReloadingProxy<IFoo>)foo;
            Foo initialFoo = (Foo)proxyFoo.Object;

            foo.Qux = "xyz";
            foo.Garply += (s, e) => { garplies++; };

            proxyFoo.Reloading += (s, e) => { reloading++; };
            proxyFoo.Reloaded += (s, e) => { reloaded++; };

            initialFoo.OnGarply();

            Assert.Equal(123, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(1, garplies);

            Assert.False(initialFoo.IsDisposed);
            Assert.Equal(0, reloading);
            Assert.Equal(0, reloaded);

            ChangeConfig(config);

            Assert.True(initialFoo.IsDisposed);
            Assert.Equal(1, reloading);
            Assert.Equal(1, reloaded);

            Foo changedFoo = (Foo)proxyFoo.Object;

            changedFoo.OnGarply();

            Assert.Equal(456, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(2, garplies);

            Assert.False(changedFoo.IsDisposed);

            ((IDisposable)foo).Dispose();

            Assert.True(changedFoo.IsDisposed);
        }

        [Fact]
        public void WhenReloadOnChangeIsChangedToFalseObjectIsNotReloaded()
        {
            var config = GetConfig();

            var garplies = 0;
            var reloading = 0;
            var reloaded = 0;

            IFoo foo = new ProxyFoo(config.GetSection("foo"), new DefaultTypes(), new ValueConverters(), null, null);

            IConfigReloadingProxy<IFoo> proxyFoo = (IConfigReloadingProxy<IFoo>)foo;
            Foo initialFoo = (Foo)proxyFoo.Object;

            foo.Qux = "xyz";
            foo.Garply += (s, e) => { garplies++; };

            proxyFoo.Reloading += (s, e) => { reloading++; };
            proxyFoo.Reloaded += (s, e) => { reloaded++; };

            initialFoo.OnGarply();

            Assert.Equal(123, foo.Bar);
            Assert.Equal("xyz", foo.Qux);
            Assert.Equal(1, garplies);

            Assert.False(initialFoo.IsDisposed);
            Assert.Equal(0, reloading);
            Assert.Equal(0, reloaded);

            ChangeConfig(config, new KeyValuePair<string, string>("foo:reloadOnChange", "false"));

            Assert.False(initialFoo.IsDisposed);
            Assert.Equal(0, reloading);
            Assert.Equal(0, reloaded);

            Foo changedFoo = (Foo)proxyFoo.Object;

            Assert.Same(initialFoo, changedFoo);
        }

        [Fact]
        public void MissingTypeThrowsInvalidOperationException()
        {
            var config = GetNonTypeSpecifiedConfig();

            Assert.Throws<InvalidOperationException>(() => new ProxyFoo(config.GetSection("foo"), new DefaultTypes(), new ValueConverters(), null, null));
        }

        [Fact]
        public void InvalidTypeThrowsTypeLoadException()
        {
            var config = GetConfig("invalid type");

            Assert.Throws<TypeLoadException>(() => new ProxyFoo(config.GetSection("foo"), new DefaultTypes(), new ValueConverters(), null, null));
        }

        private static IConfigurationRoot GetNonTypeSpecifiedConfig()
        {
            return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["foo:bar"] = "123",
            }).Build();
        }

        private static IConfigurationRoot GetReloadingConfig(Type type = null)
        {
            var initialData = new Dictionary<string, string>
            {
                ["foo:reloadOnChange"] = "true",
                ["foo:value:bar"] = "123",
            };

            if (type != null)
                initialData.Add("foo:type", type.AssemblyQualifiedName);

            return new ConfigurationBuilder().AddInMemoryCollection(initialData).Build();
        }

        private static IConfigurationRoot GetConfig(string type = null)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["foo:type"] = type ?? typeof(Foo).AssemblyQualifiedName,
                ["foo:value:bar"] = "123",
            }).Build();
        }

        private static void ChangeConfig(IConfigurationRoot root, params KeyValuePair<string, string>[] additionalSettings)
        {
            MemoryConfigurationProvider provider = (MemoryConfigurationProvider)root.Providers.First();
            provider.Set("foo:value:bar", "456");
            foreach (var setting in additionalSettings)
                provider.Set(setting.Key, setting.Value);
            var _onReloadMethod = typeof(ConfigurationProvider).GetMethod("OnReload", BindingFlags.Instance | BindingFlags.NonPublic);
            _onReloadMethod.Invoke(provider, null);
        }

        private static void ChangeNonTypeSpecifiedConfig(IConfigurationRoot root, params KeyValuePair<string, string>[] additionalSettings)
        {
            MemoryConfigurationProvider provider = (MemoryConfigurationProvider)root.Providers.First();
            provider.Set("foo:bar", "456");
            foreach (var setting in additionalSettings)
                provider.Set(setting.Key, setting.Value);
            var _onReloadMethod = typeof(ConfigurationProvider).GetMethod("OnReload", BindingFlags.Instance | BindingFlags.NonPublic);
            _onReloadMethod.Invoke(provider, null);
        }

        private class Foo : IFoo, IDisposable
        {
            public Foo(int bar) => Bar = bar;
            public int Bar { get; }
            public string Qux { get; set; }
            public event EventHandler Garply;
            public int Baz(int factor) => Bar * factor;
            public void OnGarply() => Garply?.Invoke(this, EventArgs.Empty);
            public bool IsDisposed { get; private set; }
            public void Dispose() => IsDisposed = true;
        }
    }
}
#endif
