using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Configuration
{
    internal class CompositeConfigurationSection : IConfigurationSection
    {
        private readonly Lazy<IReadOnlyCollection<IConfigurationSection>> _allSections;
        private readonly Lazy<IConfigurationSection> _primarySection;
        private readonly Lazy<IEnumerable<IConfigurationSection>> _children;

        public CompositeConfigurationSection(IEnumerable<IConfigurationSection> sections)
        {
            _allSections = new Lazy<IReadOnlyCollection<IConfigurationSection>>(() => sections.ToList());

            _primarySection = new Lazy<IConfigurationSection>(() => _allSections.Value.FirstOrDefault(section => section.Value != null)
                ?? _allSections.Value.First());

            _children = new Lazy<IEnumerable<IConfigurationSection>>(() =>
                _allSections.Value.SelectMany(section => section.GetChildren())
                    .GroupBy(s => s.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(s => new CompositeConfigurationSection(s))
                    .OrderBy(s => s.Key, CompositeSectionKeyComparer.Instance)
                    .ToList());
        }

        public IConfigurationSection GetSection(string key) =>
            new CompositeConfigurationSection(_allSections.Value.Select(s => s.GetSection(key)));

        public IEnumerable<IConfigurationSection> GetChildren() => _children.Value;

        public IChangeToken GetReloadToken() => _primarySection.Value.GetReloadToken();

        public string this[string key]
        {
            get => _allSections.Value.FirstOrDefault(s => s[key] != null)?[key];
            set
            {
                foreach (var section in _allSections.Value)
                {
                    if (section[key] == null)
                        continue;
                    section[key] = value;
                    return;
                }
                _primarySection.Value[key] = value;
            }
        }

        public string Key => _primarySection.Value.Key;

        public string Path => _primarySection.Value.Path;

        public string Value
        {
            get => _primarySection.Value.Value;
            set => _primarySection.Value.Value = value;
        }

        private class CompositeSectionKeyComparer : IComparer<string>
        {
            public static readonly IComparer<string> Instance = new CompositeSectionKeyComparer();

            private CompositeSectionKeyComparer() {}

            int IComparer<string>.Compare(string x, string y)
            {
                var xIsInt = uint.TryParse(x, out var xValue);
                var yIsInt = uint.TryParse(y, out var yValue);

                // Do not change order when neither key is numeric.
                if (!xIsInt && !yIsInt)
                    return 0;

                // Put numeric keys in ascending order.
                if (xIsInt && yIsInt)
                    return xValue.CompareTo(yValue);

                // Put numeric keys after non-numeric keys.
                if (xIsInt) return 1;
                /*if (yIsInt)*/ return -1;
            }
        }
    }
}
