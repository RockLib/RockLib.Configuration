using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Configuration
{
    internal class CompositeConfigurationSection : IConfigurationSection
    {
        private readonly IReadOnlyCollection<IConfigurationSection> _allSections;
        private readonly IConfigurationSection _primarySection;

        public CompositeConfigurationSection(IEnumerable<IConfigurationSection> sections)
        {
            _allSections = sections.ToList();
            _primarySection = _allSections.FirstOrDefault(section => section.Value != null)
                ?? _allSections.First();
        }

        public IConfigurationSection GetSection(string key) =>
            new CompositeConfigurationSection(_allSections.Select(s => s.GetSection(key)));

        public IEnumerable<IConfigurationSection> GetChildren() =>
            _allSections.SelectMany(section => section.GetChildren())
                .GroupBy(section => section.Key, StringComparer.OrdinalIgnoreCase)
                .Select(sections => new CompositeConfigurationSection(sections));

        public IChangeToken GetReloadToken() => _primarySection.GetReloadToken();

        public string this[string key]
        {
            get => _allSections.FirstOrDefault(s => s[key] != null)?[key];
            set
            {
                foreach (var section in _allSections)
                {
                    if (section[key] == null)
                        continue;
                    section[key] = value;
                    return;
                }
                _primarySection[key] = value;
            }
        }

        public string Key => _primarySection.Key;

        public string Path => _primarySection.Path;

        public string Value
        {
            get => _primarySection.Value;
            set => _primarySection.Value = value;
        }
    }
}
