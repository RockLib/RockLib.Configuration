using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Dynamic;

namespace RockLib.Configuration
{
    internal class ConvertibleConfigurationSection : DynamicObject, IConfigurationSection
    {
        public ConvertibleConfigurationSection(IConfigurationSection section)
        {
            Section = section;
        }

        public IConfigurationSection Section { get; }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            try
            {
                result = Section.Get(binder.Type);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        IEnumerable<IConfigurationSection> IConfiguration.GetChildren() => Section.GetChildren();
        IChangeToken IConfiguration.GetReloadToken() => Section.GetReloadToken();
        IConfigurationSection IConfiguration.GetSection(string key) => Section.GetSection(key);
        string IConfiguration.this[string key] { get => Section[key]; set => Section[key] = value; }
        string IConfigurationSection.Key => Section.Key;
        string IConfigurationSection.Path => Section.Path;
        string IConfigurationSection.Value { get => Section.Value; set => Section.Value = value; }
    }
}
