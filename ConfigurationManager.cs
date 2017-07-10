#if !NET45
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace RockLib.Configuration
{
    /// <summary>
    /// Provides access to configuration files for client applications. This class cannot be inherited.
    /// 
    /// The intent of our copy of ConfigurationManager is to mimic that of the existing/legacy configuration manager in .net 45+
    ///     The idea is that someone consuming our library should be able to drop this in and have it 'just work'
    /// 
    /// Api for legacy system
    /// https://msdn.microsoft.com/en-us/library/system.configuration.configurationmanager(v=vs.110).aspx
    /// </summary>
    public static class ConfigurationManager
    {
        private static readonly IConfigurationRoot _configuration;

        static ConfigurationManager()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("rocklib.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            AppSettings = new AppSettings(_configuration);
        }

        public static AppSettings AppSettings { get; }

        public static object GetSection(string sectionName)
        {
            var section = _configuration.GetSection(sectionName);

            if (section == null)
            {
                return null;
            }

            return GetLooselyTypedObject(section);
        }

        private static object GetLooselyTypedObject(IConfigurationSection section)
        {
            if (section.Value != null)
            {
                bool b;
                if (bool.TryParse(section.Value, out b))
                {
                    return b;
                }

                int i;
                if (int.TryParse(section.Value, out i))
                {
                    return i;
                }

                return section.Value;
            }

            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var child in section.GetChildren())
            {
                expando[child.Key] = GetLooselyTypedObject(child);
            }

            int dummy;
            if (expando.Keys.Any() && expando.Keys.All(k => int.TryParse(k, out dummy)))
            {
                return expando.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            }

            return expando;
        }
    }

}
#endif