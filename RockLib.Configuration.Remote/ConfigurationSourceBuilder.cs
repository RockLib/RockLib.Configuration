using System;
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration.Remote;

/// <summary>
/// A configuration source builder with access to configuration sources added
/// before the source being added now.
/// </summary>
public class ConfigurationSourceBuilder<TConfigurationSource> where TConfigurationSource : IConfigurationSource
{
    private readonly Lazy<IConfigurationRoot> _configuration;

    /// <summary>
    /// Create a ConfigurationSourceBuilder instance.
    /// </summary>
    /// <param name="builder">The IConfigurationBuilder that the new source will be added to</param>
    /// <param name="source">The new IConfigurationSource to add and configure</param>
    public ConfigurationSourceBuilder(IConfigurationBuilder builder, TConfigurationSource source)
    {
        _configuration = new Lazy<IConfigurationRoot>(() => builder.Build());
        Options = source;
    }

    /// <summary>
    /// All of the configuration that has been added until now.
    /// <br/>
    /// WARNING: Using this will build each configuration source previously
    /// added. This may cause unintended side-effects.
    /// </summary>
    public IConfigurationRoot PriorConfiguration => _configuration.Value;

    /// <summary>
    /// The IConfigurationSource being added and configured. This can be used to
    /// configure additional properties.
    /// </summary>
    public TConfigurationSource Options { get; private set; }
}
