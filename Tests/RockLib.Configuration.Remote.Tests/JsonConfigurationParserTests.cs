using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace RockLib.Configuration.Remote.Tests;

public class JsonConfigurationParserTests
{
    [Fact]
    public void ParseReturnsDictionaryWhenRawIsValidJson()
    {
        // Arrange
        var json = JsonConvert.SerializeObject(new
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
            KeyValue("Section:Prefix:list:0:flag", "True"),
            KeyValue("Section:Prefix:list:0:value", "x"),
            KeyValue("Section:Prefix:list:1:flag", "False"),
            KeyValue("Section:Prefix:list:1:value", "y"));
    }

    private static KeyValuePair<string, string> KeyValue(string key, string value)
    {
        return new KeyValuePair<string, string>(key, value);
    }
}
