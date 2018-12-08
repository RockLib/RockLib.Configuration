using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// The base class for reloading proxy classes.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Object) + ",nq}")]
    public abstract class ConfigReloadingProxy<TInterface> : IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly HashAlgorithm _hashAlgorithm = MD5.Create();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly IConfiguration _section;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly DefaultTypes _defaultTypes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly ValueConverters _valueConverters;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Type _declaringType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly string _memberName;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _hash;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigReloadingProxy{TInterface}"/> class.
        /// </summary>
        /// <param name="section">The configuration section that defines the object that this class creates.</param>
        /// <param name="defaultTypes">
        /// An object that defines the default types to be used when a type is not explicitly specified by a
        /// configuration section.
        /// </param>
        /// <param name="valueConverters">
        /// An object that defines custom converter functions that are used to convert string configuration
        /// values to a target type.
        /// </param>
        /// <param name="declaringType">If present the declaring type of the member that this instance is a value of.</param>
        /// <param name="memberName">If present, the name of the member that this instance is the value of.</param>
        protected ConfigReloadingProxy(IConfiguration section, DefaultTypes defaultTypes, ValueConverters valueConverters, Type declaringType, string memberName)
        {
            if (typeof(TInterface) == typeof(IEnumerable))
                throw new InvalidOperationException("The IEnumerable interface is not supported.");
            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeof(TInterface)))
                throw new InvalidOperationException($"Interfaces that inherit from IEnumerable are not suported: '{typeof(TInterface).FullName}'");

            _section = section ?? throw new ArgumentNullException(nameof(section));
            _defaultTypes = defaultTypes ?? ConfigurationObjectFactory.EmptyDefaultTypes;
            _valueConverters = valueConverters ?? ConfigurationObjectFactory.EmptyValueConverters;
            _declaringType = declaringType; // Null is a valid value
            _memberName = memberName; // Null is a valid value
            _hash = GetHash();
            Object = CreateObject();
            ChangeToken.OnChange(section.GetReloadToken, () => ReloadObject(false));
        }

        /// <summary>
        /// Force the underlying object to reload from the current configuration.
        /// </summary>
        public void Reload() => ReloadObject(true);

        /// <summary>
        /// Gets the underlying object.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TInterface Object { get; private set; }

        /// <summary>
        /// Occurs immediately before the underlying object is reloaded.
        /// </summary>
        public event EventHandler Reloading;

        /// <summary>
        /// Occurs immediately after the underlying object is reloaded.
        /// </summary>
        public event EventHandler Reloaded;

        /// <summary>
        /// Dispose the underlying object if it implements <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose() => (Object as IDisposable)?.Dispose();

        private TInterface CreateObject()
        {
            // In order to create the object (and avoid infinite recursion), we need to figure out
            // the concrete type to create and the config section that defines the value to create.
            Type concreteType;
            IConfiguration valueSection;

            // If _section contains a type-specified value, extract the type and use the value sub-section.
            string typeValue = _section[ConfigurationObjectFactory.TypeKey];
            if (typeValue != null)
            {
                // Throw if the value does not represent a valid Type.
                concreteType = Type.GetType(typeValue, true);

                if (!typeof(TInterface).GetTypeInfo().IsAssignableFrom(concreteType))
                    throw Exceptions.ConfigurationSpecifiedTypeIsNotAssignableToTargetType(typeof(TInterface), concreteType);

                valueSection = _section.GetSection(ConfigurationObjectFactory.ValueKey);
            }

            // If there is a registered default type, use it.
            else if (ConfigurationObjectFactory.TryGetDefaultType(_defaultTypes, typeof(TInterface), _declaringType, _memberName, out concreteType))
            {
                // The value section depends on whether the 'ReloadOnChange' flag is set to true.
                if (string.Equals(_section[ConfigurationObjectFactory.ReloadOnChangeKey]?.ToLowerInvariant(), "true"))
                    valueSection = _section.GetSection(ConfigurationObjectFactory.ValueKey);
                else
                    valueSection = _section;
            }

            // Throw if no type can be located.
            else
            {
                throw Exceptions.TypeNotSpecifiedForReloadingProxy;
            }

            // Put everything together.
            return (TInterface)valueSection.Create(concreteType, _defaultTypes, _valueConverters);
        }

        private void ReloadObject(bool force)
        {
            lock (this)
            {
                string newHash = GetHash();

                // If reloadOnChange is explicitly turned off, don't reload the object - just return.
                // Also return if _section's hash hasn't changed.
                if (!force
                    && (string.Equals(_section[ConfigurationObjectFactory.ReloadOnChangeKey], "false", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(newHash, _hash, StringComparison.Ordinal)))
                    return;

                _hash = newHash;

                // Before doing anything, invoke Reloading.
                Reloading?.Invoke(this, EventArgs.Empty);

                // Capture the old object and instantiate the new one (but don't set the field).
                TInterface oldObject = Object;
                TInterface newObject = CreateObject();

                TransferState(oldObject, newObject);

                // After the new object has been fully initialized, set the backing field.
                Object = newObject;

                // If the old object is disposable, dispose it after the backing field has been set.
                (oldObject as IDisposable)?.Dispose();

                // After doing everything, invoke Reloaded.
                Reloaded?.Invoke(this, EventArgs.Empty);
            }
        }

        private string GetHash()
        {
            var settingsDump = GetSettingsDump(_section);
            var buffer = Encoding.UTF8.GetBytes(settingsDump);
            var hash = _hashAlgorithm.ComputeHash(buffer);
            return Convert.ToBase64String(hash);

            string GetSettingsDump(IConfiguration config)
            {
                var sb = new StringBuilder();
                AddSettingsDump(config, sb);
                return sb.ToString();
            }

            void AddSettingsDump(IConfiguration config, StringBuilder sb)
            {
                if (config is IConfigurationSection section && section.Value != null)
                    sb.Append(section.Path).Append(section.Value);
                foreach (var child in config.GetChildren())
                    AddSettingsDump(child, sb);
            }
        }

        /// <summary>
        /// Transfer state from the old object to the new object, specifically event handlers
        /// and the values of reference-type read/write properties where the new object has a
        /// null value and the old object has a non-null value.
        /// </summary>
        /// <param name="oldObject">
        /// The object that is the current value of the <see cref="Object"/> property which is about
        /// to be replaced by <paramref name="newObject"/>.
        /// </param>
        /// <param name="newObject">
        /// The object (not yet in use) that is about to replace <paramref name="oldObject"/> as the
        /// value of the <see cref="Object"/> property.
        /// </param>
        protected abstract void TransferState(TInterface oldObject, TInterface newObject);
    }
}
