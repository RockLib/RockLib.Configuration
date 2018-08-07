#if NET451 || NET462
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RockLib.Configuration
{
    /// <summary>
    /// Defines a configuration section that transforms its child xml elements
    /// into Microsoft.Extensions.Configuration key/value pairs upon
    /// deserialization.
    /// </summary>
    public sealed class RockLibConfigurationSection : ConfigurationSection
    {
        private static readonly IEqualityComparer<string> _ignoreCase = StringComparer.InvariantCultureIgnoreCase;

        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>(_ignoreCase);
        private List<XElement> _unrecognizedElements = new List<XElement>();

        /// <summary>
        /// Gets the Microsoft.Extensions.Configuration key/value pairs that
        /// represent the child xml elements of this configuration section.
        /// </summary>
        public IReadOnlyDictionary<string, string> Settings => _settings;

        /// <inheritdoc/>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            var element = XElement.Load(reader.ReadSubtree());
            _unrecognizedElements.Add(element);
            return true;
        }

        /// <inheritdoc/>
        protected override void PostDeserialize()
        {
            if (_unrecognizedElements.Count == 1)
                ProcessElement(_unrecognizedElements[0], SectionInformation.Name);
            else
            {
                int i = 0;
                foreach (var element in _unrecognizedElements)
                    ProcessElement(element, $"{SectionInformation.Name}:{i++}");
            }
            _unrecognizedElements = null;
        }

        private void ProcessElement(XElement element, string path)
        {
            foreach (var attribute in element.Attributes())
                _settings[$"{path}:{attribute.Name.LocalName}"] = attribute.Value.Trim();

            if (element.HasElements)
            {
                foreach (var childrenByName in element.Elements().GroupBy(x => x.Name.LocalName, _ignoreCase))
                {
                    int i = 0;
                    XElement onlyChild = null;
                    foreach (var child in childrenByName)
                    {
                        if (i == 0) // The first child is an only child...
                            onlyChild = child;
                        else
                        {
                            if (i == 1) // ...until the second one comes along.
                            {
                                ProcessElement(onlyChild, $"{path}:{onlyChild.Name.LocalName}:0");
                                onlyChild = null;
                            }
                            ProcessElement(child, $"{path}:{child.Name.LocalName}:{i}");
                        }
                        i++;
                    }

                    if (onlyChild != null)
                        ProcessElement(onlyChild, $"{path}:{onlyChild.Name.LocalName}");
                }
            }
            else if (!string.IsNullOrWhiteSpace(element.Value) && !_settings.ContainsKey(path))
                _settings[path] = element.Value.Trim();
        }
    }
}
#endif
