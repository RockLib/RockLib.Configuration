using System.Collections.Generic;

namespace RockLib.Configuration.Remote;

/// <summary>
/// An interface capable of parsing configuration from a string.
/// </summary>
public interface IConfigurationParser
{
    /// <summary>
    /// Parse configuration from a raw string.
    /// </summary>
    /// <returns>A Dictionary of configuration</returns>
    IDictionary<string, string> Parse(string raw);
}
