using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace RockLib.Configuration.Conditional;

/// <summary>
/// A <see cref="IConfigurationSection" /> that can use an alternate
/// <see cref="IConfigurationSection" /> to override some values.
/// </summary>
public sealed class ConditionalConfigurationSection : IConfigurationSection
{
    private readonly IConfigurationSection _baseSection;
    private readonly Func<IConfigurationSection> _getOverrideSection;

    private ConditionalConfigurationSection(IConfigurationSection baseSection, Func<IConfigurationSection> getOverrideSection)
    {
        _baseSection = baseSection;
        _getOverrideSection = getOverrideSection;
    }

    /// <summary>
    /// Create a <see cref="ConditionalConfigurationSection" /> instance with
    /// overrides from a child section determined by a switching property.
    /// </summary>
    /// <param name="baseSection">
    ///   The base <see cref="IConfigurationSection" /> to use for this
    ///   configuration
    /// </param>
    /// <param name="switchingProperty">
    ///   A property name whose value determines which child section to use as
    ///   the override section
    /// </param>
    public ConditionalConfigurationSection(IConfigurationSection baseSection, string switchingProperty)
        : this(baseSection, () => baseSection.GetSection(baseSection[switchingProperty])) { }

    /// <InheritDoc />
    public string this[string key]
    {
        get => GetSectionForKey(key)[key];
        set => GetSectionForKey(key)[key] = value;
    }

    /// <summary>
    /// Gets the key this section's base occupies in its parent.
    /// </summary>
    public string Key => _baseSection.Key;

    /// <summary>
    /// Gets the full path to this section's base within the <see cref="IConfiguration" />.
    /// </summary>
    public string Path => _baseSection.Path;

    /// <summary>
    /// Gets the value from the override section if it exists, otherwise from the base section.
    /// Sets the value of the base section.
    /// </summary>
    public string Value
    {
        get => _getOverrideSection.Invoke().Value ?? _baseSection.Value;
        set => _baseSection.Value = value;
    }

    /// <summary>
    /// Gets the immediate descendant configuration sub-sections of this section
    /// and the override section.
    /// </summary>
    /// <returns>
    /// The configuration sub-sections with overrides applied.
    /// </returns>
    public IEnumerable<IConfigurationSection> GetChildren()
    {
        return GetAllSections().SelectMany(section => section.GetChildren())
            .Select(section => section.Key)
            .Distinct()
            .Select(key => GetSection(key));
    }

    /// <summary>
    /// Returns a <see cref="IChangeToken" /> that can be used to observe when
    /// this configuration's base is reloaded.
    /// </summary>
    /// <returns>
    /// A <see cref="IChangeToken" />.
    /// </returns>
    public IChangeToken GetReloadToken()
    {
        return _baseSection.GetReloadToken();
    }

    /// <summary>
    /// Gets a configuration sub-section with the specified key.
    ///
    /// The returned section will be a <see cref="ConditionalConfigurationSection" />
    /// that merges any override configuration into the base configuration.
    ///
    /// This method will never return null. If no matching sub-section is found
    /// with the specified key, an empty <see cref="IConfigurationSection" />
    /// will be returned.
    /// </summary>
    /// <returns>
    /// The IConfigurationSection.
    /// </returns>
    public IConfigurationSection GetSection(string key)
    {
        return new ConditionalConfigurationSection(_baseSection.GetSection(key), () => _getOverrideSection.Invoke().GetSection(key));
    }

    private IEnumerable<IConfigurationSection> GetAllSections()
    {
        yield return _getOverrideSection.Invoke();
        yield return _baseSection;
    }

    private IConfigurationSection GetSectionForKey(string key)
    {
        return GetAllSections().FirstOrDefault(section => section[key] != null) ?? _baseSection;
    }
}
