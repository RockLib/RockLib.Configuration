using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;

namespace RockLib.Configuration
{
    /// <summary>
    /// Provides a mechanism for converting an instance of <see cref="IConfigurationSection"/> to
    /// a type specified via a cast.
    /// </summary>
    internal class ConvertibleConfigurationSection : DynamicObject, IConfigurationSection
    {
        private ConcurrentDictionary<Type, object> _conversionCache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertibleConfigurationSection"/> class.
        /// </summary>
        /// <param name="section">The backing <see cref="IConfigurationSection"/>.</param>
        public ConvertibleConfigurationSection(IConfigurationSection section)
        {
            Section = section;
        }

        /// <summary>
        /// Gets the backing <see cref="IConfigurationSection"/> for this instance.
        /// </summary>
        public IConfigurationSection Section { get; }

        /// <summary>
        /// Attempts to convert this instance to the type specified by <see cref="ConvertBinder.Type"/>.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns>true if the operation is successful; otherwise, false.</returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = _conversionCache.GetOrAdd(binder.Type, type =>
            {
                try { return Section.Get(type); }
                catch { return null; }
            });
            return result != null;
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
