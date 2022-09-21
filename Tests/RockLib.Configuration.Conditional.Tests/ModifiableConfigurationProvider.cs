using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace RockLib.Configuration.Conditional.Tests;

internal class ModifiableConfigurationSource : IConfigurationSource
{
    public ModifiableConfigurationSource(IDictionary<string, string> data)
    {
        Provider = new ModifiableConfigurationProvider(data);
    }

    public ModifiableConfigurationProvider Provider { get; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return Provider;
    }
}

internal class ModifiableConfigurationProvider : ConfigurationProvider
{
    public ModifiableConfigurationProvider(IDictionary<string, string> data)
    {
        Data = data;
    }

    public void Modify(string key, string value)
    {
        Data[key] = value;
        OnReload();
    }
}
