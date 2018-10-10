using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    public abstract class ConfigReloadingProxyBase : IDisposable
    {
        private readonly Type _interfaceType;
        private readonly IConfiguration _section;
        private readonly DefaultTypes _defaultTypes;
        private readonly ValueConverters _valueConverters;
        private readonly Type _declaringType;
        private readonly string _memberName;
        protected internal object _object;

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

        public event EventHandler Reloading;
        public event EventHandler Reloaded;

        protected internal abstract void ReloadObject();

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
                if (IsReloadOnChangeExplicitlyTurnedOn)
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

        protected internal bool IsReloadOnChangeExplicitlyTurnedOn =>
            string.Equals(_section[ConfigurationObjectFactory.ReloadOnChangeKey]?.ToLowerInvariant(), "true");

        protected internal bool IsReloadOnChangeExplicitlyTurnedOff =>
            string.Equals(_section[ConfigurationObjectFactory.ReloadOnChangeKey]?.ToLowerInvariant(), "false");

        protected internal void OnReloading() => Reloading?.Invoke(this, EventArgs.Empty);
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
