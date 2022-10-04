using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RockLib.Configuration.Remote.Tests;

public class JsonConfigurationParserTests
{
    [Fact]
    public void ParseReturnsDictionaryWhenRawIsValidJson()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            word = "hello",
            number = 123,
            list = new object[]
            {
                new
                {
                    flag = true,
                    value = "x"
                },
                new
                {
                    flag = false,
                    value = "y"
                }
            }
        });

        // Act
        var parser = new JsonConfigurationParser("Section:Prefix");
        var actual = parser.Parse(json);

        // Assert
        actual.Should().Contain(
            KeyValue("Section:Prefix:word", "hello"),
            KeyValue("Section:Prefix:number", "123"),
            KeyValue("Section:Prefix:list:0:flag", "true"),
            KeyValue("Section:Prefix:list:0:value", "x"),
            KeyValue("Section:Prefix:list:1:flag", "false"),
            KeyValue("Section:Prefix:list:1:value", "y"));
    }

    private static KeyValuePair<string, string> KeyValue(string key, string value)
    {
        return new KeyValuePair<string, string>(key, value);
    }
}
