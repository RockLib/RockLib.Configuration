using System;
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration.Remote;

/// <summary>
/// Extension methods for adding a remote configuration source.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Add a remote configuration source to this IConfigurationBuilder.
    /// </summary>
    /// <param name="builder">This IConfigurationBuilder instance</param>
    /// <param name="source">The source to add</param>
    /// <param name="action">A configure action to be applied to the remote configuration source</param>
    /// <returns>The IConfigurationBuilder with remote configuration added</returns>
    public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, RemoteConfigurationSource source, Action<ConfigurationSourceBuilder<RemoteConfigurationSource>>? action = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        action?.Invoke(new ConfigurationSourceBuilder<RemoteConfigurationSource>(builder, source));
        return builder.Add(source);
    }

    /// <summary>
    /// Add a remote configuration source to this IConfigurationBuilder.
    /// </summary>
    /// <param name="builder">This IConfigurationBuilder instance</param>
    /// <param name="action">A configure action to be applied to a new remote configuration source</param>
    /// <returns>The IConfigurationBuilder with remote configuration added</returns>
    public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, Action<ConfigurationSourceBuilder<RemoteConfigurationSource>>? action = null)
    {
        var source = new RemoteConfigurationSource();
        return builder.AddRemote(source, action);
    }

    /// <summary>
    /// Add a remote configuration source to this IConfigurationBuilder.
    /// </summary>
    /// <param name="builder">This IConfigurationBuilder instance</param>
    /// <param name="configSection">The name of the section where configuration for the remote configuration lives</param>
    /// <param name="action">A configure action to be applied to a new remote configuration source</param>
    /// <returns>The IConfigurationBuilder with remote configuration added</returns>
    public static IConfigurationBuilder AddRemote(this IConfigurationBuilder builder, string configSection, Action<ConfigurationSourceBuilder<RemoteConfigurationSource>>? action = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var source = builder.Build().GetSection(configSection).Get<RemoteConfigurationSource>();
        return builder.AddRemote(source, action);
    }
}
