using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// A base class for reloading proxy classes.
    /// </summary>
    public abstract class ConfigReloadingProxyBase : IDisposable
    {
        private readonly Type _interfaceType;
        private readonly IConfiguration _section;
        private readonly DefaultTypes _defaultTypes;
        private readonly ValueConverters _valueConverters;
        private readonly Type _declaringType;
        private readonly string _memberName;

        /// <summary>
        /// The backing field that holds the underlying object.
        /// </summary>
        protected internal object _object;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigReloadingProxyBase"/> class.
        /// </summary>
        /// <param name="interfaceType">The type of the interface that this proxy class will implement.</param>
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
        protected ConfigReloadingProxyBase(Type interfaceType, IConfiguration section, DefaultTypes defaultTypes, ValueConverters valueConverters, Type declaringType, string memberName)
        {
            _interfaceType = interfaceType;
            _section = section;
            _defaultTypes = defaultTypes;
            _valueConverters = valueConverters;
            _declaringType = declaringType;
            _memberName = memberName;
            _object = CreateObject();
            ChangeToken.OnChange(section.GetReloadToken, ReloadObject);
        }

        /// <summary>
        /// Occurs immediately before the underlying object is reloaded.
        /// </summary>
        public event EventHandler Reloading;

        /// <summary>
        /// Occurs immediately after the underlying object is reloaded.
        /// </summary>
        public event EventHandler Reloaded;

        /// <summary>
        /// Reload the underlying object.
        /// </summary>
        protected internal abstract void ReloadObject();

        /// <summary>
        /// Create the underlying object using the current section.
        /// </summary>
        /// <returns>The underlying object.</returns>
        protected internal object CreateObject()
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

                if (!_interfaceType.GetTypeInfo().IsAssignableFrom(concreteType))
                    throw Exceptions.ConfigurationSpecifiedTypeIsNotAssignableToTargetType(_interfaceType, concreteType);

                valueSection = _section.GetSection(ConfigurationObjectFactory.ValueKey);
            }

            // If there is a registered default type, use it.
            else if (ConfigurationObjectFactory.TryGetDefaultType(_defaultTypes, _interfaceType, _declaringType, _memberName, out concreteType))
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
            return valueSection.Create(concreteType, _defaultTypes, _valueConverters);
        }

        /// <summary>
        /// Gets a value indicating whether the ReloadOnChange flag has been explicitly set to false.
        /// </summary>
        protected internal bool IsReloadOnChangeExplicitlyTurnedOff =>
            string.Equals(_section[ConfigurationObjectFactory.ReloadOnChangeKey]?.ToLowerInvariant(), "false");

        /// <summary>
        /// Fires the <see cref="Reloading"/> event.
        /// </summary>
        protected internal void OnReloading() => Reloading?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Fires the <see cref="Reloaded"/> event.
        /// </summary>
        protected internal void OnReloaded() => Reloaded?.Invoke(this, EventArgs.Empty);

        void IDisposable.Dispose() => (_object as IDisposable)?.Dispose();

        // This class doesn't seem necessary, does it? Without it, the "partial interface implementation
        // in an abstract class" doesn't seem to work. Gluing the base class and the interface together
        // in the library makes everything work. Without it, the Reloading/Reloaded events aren't properly
        // implemented. So leave this class here for now, ok?
        private class MagicGlue : ConfigReloadingProxyBase, IConfigReloadingProxy<int>
        {
            public MagicGlue() : base(null, null, null, null, null, null) => throw new NotImplementedException();
            public int Object => throw new NotImplementedException();
            protected internal override void ReloadObject() => throw new NotImplementedException();
        }
    }
}
