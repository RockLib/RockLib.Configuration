using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;

namespace RockLib.Configuration.Remote;

/// <summary>
/// A parse for JSON configuration.
/// </summary>
public class JsonConfigurationParser : IConfigurationParser
{
    private const string SectionSeparator = ":";

    private readonly string _section;

    /// <summary>
    /// Create a JsonConfigurationParser instance with keys rooted at a given
    /// section path.
    /// </summary>
    /// <param name="section">The section path to append parsed configuration into</param>
    public JsonConfigurationParser(string section)
    {
        _section = section;
    }

    /// <summary>
    /// Parse configuration from a raw JSON string.
    /// </summary>
    /// <param name="raw">The raw JSON string</param>
    /// <returns>A Dictionary of configuration, rooted at the given section</returns>
    public IDictionary<string, string> Parse(string raw)
    {
        if (raw is null)
        {
            return ImmutableDictionary<string, string>.Empty;
        }

        return ToKeyValuePairs(_section, JsonNode.Parse(raw))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(string path, JsonNode? data)
    {
        var elementPrefix = !string.IsNullOrEmpty(path) ? $"{path}{SectionSeparator}" : "";
        if (data is JsonArray items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                foreach (var keyValuePair in ToKeyValuePairs($"{elementPrefix}{i}", items[i]))
                {
                    yield return keyValuePair;
                }
            }
        }
        else if (data is JsonObject properties)
        {
            foreach (var property in properties)
            {
                foreach (var keyValuePair in ToKeyValuePairs($"{elementPrefix}{property.Key}", property.Value))
                {
                    yield return keyValuePair;
                }
            }
        }
        else if (data is JsonValue value)
        {
            yield return new KeyValuePair<string, string>($"{path}", value.ToString());
        }
    }
}
