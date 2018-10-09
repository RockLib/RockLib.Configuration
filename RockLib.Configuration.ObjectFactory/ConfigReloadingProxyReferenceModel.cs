#if REFERENCE_MODEL
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;

namespace RockLib.Configuration.ObjectFactory.ReferenceModel
{
    public interface IFoo
    {
        int Bar { get; }
        int Baz(int factor);
        string Qux { get; set; } // Special case - read/write reference-type property
        event EventHandler Garply;
    }

    public class ProxyFoo : IFoo, IDisposable, IConfigReloadingProxy<IFoo>
    {
        private readonly IConfiguration _section;
        private readonly DefaultTypes _defaultTypes;
        private readonly ValueConverters _valueConverters;
        private readonly Type _declaringType;
        private readonly string _memberName;
        private IFoo _object;

        public ProxyFoo(IConfiguration section, DefaultTypes defaultTypes, ValueConverters valueConverters, Type declaringType, string memberName)
        {
            _section = section;
            _defaultTypes = defaultTypes;
            _valueConverters = valueConverters;
            _declaringType = declaringType;
            _memberName = memberName;
            _object = CreateObject();
            ChangeToken.OnChange(_section.GetReloadToken, ReloadObject);
        }

        // Properties are strictly pass-through.
        int IFoo.Bar => _object.Bar;

        // Methods are strictly pass-through.
        int IFoo.Baz(int qux) => _object.Baz(qux);

        // Properties are strictly pass-through.
        string IFoo.Qux { get => _object.Qux; set => _object.Qux = value; }

        // Events are pass-through, but also need to capture handlers in a private field.
        private EventHandler _garply;
        event EventHandler IFoo.Garply
        {
            add
            {
                _object.Garply += value;
                _garply += value;
            }
            remove
            {
                _object.Garply -= value;
                _garply -= value;
            }
        }

        // Dispose is implemented for all types.
        void IDisposable.Dispose() => (_object as IDisposable)?.Dispose();

        // Implicitly implement the Object property so that tooling (e.g. LINQPad) will more
        // easily find this property for display purposes.
        public IFoo Object => _object;

        // Explicitly implement the Reloading event in the usual manner.
        private EventHandler _reloading;
        event EventHandler IConfigReloadingProxy<IFoo>.Reloading
        {
            add => _reloading += value;
            remove => _reloading -= value;
        }

        // Explicitly implement the Reloaded event in the usual manner.
        private EventHandler _reloaded;
        event EventHandler IConfigReloadingProxy<IFoo>.Reloaded
        {
            add => _reloaded += value;
            remove => _reloaded -= value;
        }

        private void ReloadObject()
        {
            // If there has been a change to explicitly turn off reloadOnChange, don't reload the object.
            if (_section[ConfigurationObjectFactory._reloadOnChangeKey]?.ToLowerInvariant() == "false")
                return;

            // Before doing anything, invoke Reloading.
            _reloading?.Invoke(this, EventArgs.Empty);

            // Capture the old object and create the new one.
            IFoo oldObject = _object;
            IFoo newObject = CreateObject();

            if (oldObject != null)
            {
                // Event handlers from the old object need to be copied to the new one.
                newObject.Garply += _garply;

                // Special case for when the interface has a read/write property: if the new
                // property value is null and the old property value is not null, then copy
                // the value from old to new.
                if (oldObject.Qux != null && newObject.Qux == null)
                    newObject.Qux = oldObject.Qux;
            }

            // After the new object has been fully initialized, set the backing field.
            _object = newObject;

            // If the old object is disposable, dispose it after the backing field has been set.
            (oldObject as IDisposable)?.Dispose();

            // After doing everything, invoke Reloaded.
            _reloaded?.Invoke(this, EventArgs.Empty);
        }

        private IFoo CreateObject()
        {
            // In order to create the object (and avoid infinite recursion), we need to figure out
            // the concrete type to create and the config section that defines the value to create.
            Type concreteType;
            IConfiguration valueSection;

            // If _section contains a type-specified value, extract the type and use the value sub-section.
            var typeValue = _section[ConfigurationObjectFactory._typeKey];
            if (typeValue != null)
            {
                // Throw if the value does not represent a valid Type.
                concreteType = Type.GetType(typeValue, true);
                valueSection = _section.GetSection(ConfigurationObjectFactory._valueKey);
            }

            // If there is a registered default type, use it, and assume _section is the value section.
            else if (ConfigurationObjectFactory.TryGetDefaultType(_defaultTypes, typeof(IFoo), _declaringType, _memberName, out concreteType))
            {
                valueSection = _section;
            }

            // Throw if no type can be located.
            else
            {
                throw new InvalidOperationException("Type not specified.");
            }

            // Put everything together.
            return (IFoo)valueSection.Create(concreteType, _defaultTypes, _valueConverters);
        }
    }
}
#endif
