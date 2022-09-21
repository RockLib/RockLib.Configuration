using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace RockLib.Configuration.Conditional.Tests;

public class ConditionalConfigurationSectionTests
{
    [Fact]
    public void KeyPropertyHasSameValueAsBaseSectionKeyProperty()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "foo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var switchingConfigurationSection = configuration.GetSection("main").SwitchingOn("switcher");

        // Act
        var actual = switchingConfigurationSection.Key;

        // Assert
        actual.Should().Be("main");
    }

    [Fact]
    public void GetIndexReturnsValueFromBaseSectionWhenOnlyBaseSectionContainsRequestedKey()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" },
            { "main:foo", "MainFoo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var switchingConfigurationSection = configuration.GetSection("main").SwitchingOn("switcher");

        // Act
        var actual = switchingConfigurationSection["foo"];

        // Assert
        actual.Should().Be("MainFoo");
    }

    [Fact]
    public void GetIndexReturnsValueFromOverrideSectionWhenOnlyOverrideSectionContainsRequestedKey()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" },
            { "main:left:foo", "LeftFoo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var switchingConfigurationSection = configuration.GetSection("main").SwitchingOn("switcher");

        // Act
        var actual = switchingConfigurationSection["foo"];

        // Assert
        actual.Should().Be("LeftFoo");
    }

    [Fact]
    public void GetIndexReturnsValueFromOverrideSectionWhenBothSectionsContainRequestedKey()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" },
            { "main:foo", "MainFoo" },
            { "main:left:foo", "LeftFoo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var switchingConfigurationSection = configuration.GetSection("main").SwitchingOn("switcher");

        // Act
        var actual = switchingConfigurationSection["foo"];

        // Assert
        actual.Should().Be("LeftFoo");
    }

    [Fact]
    public void GetIndexReturnsNullWhenNeitherSectionContainsRequestedKey()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var switchingConfigurationSection = configuration.GetSection("main").SwitchingOn("switcher");

        // Act
        var actual = switchingConfigurationSection["foo"];

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public void SetIndexSetsValueOnBaseSectionWhenOnlyBaseSectionContainsRequestedKey()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" },
            { "main:foo", "MainFoo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var baseSection = configuration.GetSection("main");
        var switchingConfigurationSection = baseSection.SwitchingOn("switcher");

        // Act
        switchingConfigurationSection["foo"] = "NewFoo";

        // Assert
        baseSection["foo"].Should().Be("NewFoo");
        baseSection["left:foo"].Should().BeNull();
    }

    [Fact]
    public void SetIndexSetsValueOnOverrideSectionWhenOnlyOverrideSectionContainsRequestedKey()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" },
            { "main:left:foo", "LeftFoo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var baseSection = configuration.GetSection("main");
        var switchingConfigurationSection = baseSection.SwitchingOn("switcher");

        // Act
        switchingConfigurationSection["foo"] = "NewFoo";

        // Assert
        baseSection["foo"].Should().BeNull();
        baseSection["left:foo"].Should().Be("NewFoo");
    }

    [Fact]
    public void SetIndexSetsValueOnOverrideSectionWhenBothSectionsContainRequestedKey()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" },
            { "main:foo", "MainFoo" },
            { "main:left:foo", "LeftFoo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var baseSection = configuration.GetSection("main");
        var switchingConfigurationSection = baseSection.SwitchingOn("switcher");

        // Act
        switchingConfigurationSection["foo"] = "NewFoo";

        // Assert
        baseSection["foo"].Should().Be("MainFoo");
        baseSection["left:foo"].Should().Be("NewFoo");
    }

    [Fact]
    public void SetIndexSetsValueOnBaseSectionWhenNeitherSectionContainsRequestedKey()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var baseSection = configuration.GetSection("main");
        var switchingConfigurationSection = baseSection.SwitchingOn("switcher");

        // Act
        switchingConfigurationSection["foo"] = "NewFoo";

        // Assert
        baseSection["foo"].Should().Be("NewFoo");
        baseSection["left:foo"].Should().BeNull();
    }

    [Fact]
    public void SetValueSetsValueOnBaseSection()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
        var baseSection = configuration.GetSection("main");
        var switchingConfigurationSection = baseSection.SwitchingOn("switcher");

        // Act
        switchingConfigurationSection.GetSection("foo").Value = "NewFoo";

        // Assert
        baseSection["foo"].Should().Be("NewFoo");
        baseSection["left:foo"].Should().BeNull();
    }

    [Fact]
    public void ConditionalConfigurationSectionMergesOverrideSectionIntoBaseSectionWhenExplicitlyBoundToConfigType()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" },
            { "main:foo", "MainFoo" },
            { "main:bar:baz", "MainBarBaz" },
            { "main:left:bar:baz", "LeftBarBaz" },
            { "main:left:bar:bingo", "LeftBarBingo" },
            { "main:right:bar:baz", "RightBarBaz" },
            { "main:right:bar:bingo", "RightBarBingo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();

        // Act
        var switchingConfigurationSection = configuration.GetSection("main").SwitchingOn("switcher");
        var myConfig = switchingConfigurationSection.Get<MyConfig>();

        // Assert
        myConfig.Foo.Should().Be("MainFoo");
        myConfig.Bar.Baz.Should().Be("LeftBarBaz");
        myConfig.Bar.Bingo.Should().Be("LeftBarBingo");
    }

    [Fact]
    public void ConditionalConfigurationSectionMergesOverrideSectionIntoBaseSectionWhenAccessThroughServiceProvider()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            { "main:switcher", "left" },
            { "main:foo", "MainFoo" },
            { "main:bar:baz", "MainBarBaz" },
            { "main:left:bar:baz", "LeftBarBaz" },
            { "main:left:bar:bingo", "LeftBarBingo" },
            { "main:right:bar:baz", "RightBarBaz" },
            { "main:right:bar:bingo", "RightBarBingo" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();

        // Act
        var services = new ServiceCollection();
        services.Configure<MyConfig>(configuration.GetSection("main").SwitchingOn("switcher"));
        var serviceProvider = services.BuildServiceProvider();
        var myConfig = serviceProvider.GetRequiredService<IOptionsMonitor<MyConfig>>();

        // Assert
        myConfig.CurrentValue.Foo.Should().Be("MainFoo");
        myConfig.CurrentValue.Bar.Baz.Should().Be("LeftBarBaz");
        myConfig.CurrentValue.Bar.Bingo.Should().Be("LeftBarBingo");
    }

    [Fact]
    public void ConditionalConfigurationSectionReplacesMergedSectionAfterConditionalPropertyHasChanged()
    {
        // Arrange
        var source = new ModifiableConfigurationSource(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "main:switcher", "left" },
            { "main:left:bar:baz", "LeftBarBaz" },
            { "main:right:bar:baz", "RightBarBaz" },
        });
        var provider = source.Provider;
        var configuration = new ConfigurationBuilder()
            .Add(source)
            .Build();

        // Act 1
        var services = new ServiceCollection();
        services.Configure<MyConfig>(configuration.GetSection("main").SwitchingOn("switcher"));
        var serviceProvider = services.BuildServiceProvider();
        var myConfig = serviceProvider.GetRequiredService<IOptionsMonitor<MyConfig>>();

        // Assert 1
        myConfig.CurrentValue.Bar.Baz.Should().Be("LeftBarBaz");

        // Act 2
        provider.Modify("main:switcher", "right");

        // Assert 2
        myConfig.CurrentValue.Bar.Baz.Should().Be("RightBarBaz");
    }

    [Fact]
    public void ConditionalConfigurationSectionReturnsNullForThatKeyWhenNewSectionIsMissingKey()
    {
        // Arrange
        var source = new ModifiableConfigurationSource(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "main:switcher", "left" },
            { "main:left:bar:baz", "LeftBarBaz" },
        });
        var provider = source.Provider;
        var configuration = new ConfigurationBuilder()
            .Add(source)
            .Build();

        // Act 1
        var services = new ServiceCollection();
        services.Configure<MyConfig>(configuration.GetSection("main").SwitchingOn("switcher"));
        var serviceProvider = services.BuildServiceProvider();
        var myConfig = serviceProvider.GetRequiredService<IOptionsMonitor<MyConfig>>();

        // Assert 1
        myConfig.CurrentValue.Bar.Baz.Should().Be("LeftBarBaz");

        // Act 2
        provider.Modify("main:switcher", "right");

        // Assert 2
        myConfig.CurrentValue.Bar?.Baz.Should().BeNull();
    }
}

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
#pragma warning disable CS8618 // Non-nullable field is uninitialized
internal class MyConfig
{
    public string Foo { get; set; }
    public MyConfigBar Bar { get; set; }
}

internal class MyConfigBar
{
    public string Baz { get; set; }
    public string Bingo { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field is uninitialized
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
