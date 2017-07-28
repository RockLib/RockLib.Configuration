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

    public class Garply
    {
        public string Grault { get; set; }
        public bool Thud { get; set; }
    }
}
