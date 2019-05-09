using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace RockLib.Configuration.AspNetCore.Tests
{
    public class AspNetExtensionsTests
    {
        [Fact]
        public void SetConfigRootSetsConfigRoot()
        {
            var configRoot = new ConfigurationBuilder().Build();

            var builder = new TestWebHostBuilder(configRoot);

            builder.SetConfigRoot();

            Config.Root.Should().BeSameAs(configRoot);
        }

        private class TestWebHostBuilder : IWebHostBuilder
        {
            private readonly IConfiguration _configRoot;

            public TestWebHostBuilder(IConfiguration configRoot)
            {
                _configRoot = configRoot;
            }

            public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
            {
                var context = new WebHostBuilderContext { Configuration = _configRoot };
                configureServices(context, null);
                return this;
            }

            public IWebHost Build() => throw new NotImplementedException();
            public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate) => throw new NotImplementedException();
            public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices) => throw new NotImplementedException();
            public string GetSetting(string key) => throw new NotImplementedException();
            public IWebHostBuilder UseSetting(string key, string value) => throw new NotImplementedException();
        }
    }
}
