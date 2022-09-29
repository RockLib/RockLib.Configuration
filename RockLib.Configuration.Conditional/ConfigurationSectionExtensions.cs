using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration.Conditional;

/// <summary>
/// Extension methods for creating <see cref="ConditionalConfigurationSection" />
/// from <see cref="IConfigurationSection" /> instances.
/// </summary>
public static class ConfigurationSectionExtensions
{
    /// <summary>
    /// Create a <see cref="ConditionalConfigurationSection" /> using a
    /// switching property to select an override section.
    /// </summary>
    /// <param name="config">The base <see cref="IConfigurationSection" /></param>
    /// <param name="switchingProperty">
    ///   A property name whose value determines which child section to use as
    ///   the override section
    /// </param>
    /// <returns>
    /// A <see cref="ConditionalConfigurationSection" />
    /// </returns>
    public static IConfigurationSection SwitchingOn(this IConfigurationSection config, string switchingProperty)
    {
        return new ConditionalConfigurationSection(config, switchingProperty);
    }
}
