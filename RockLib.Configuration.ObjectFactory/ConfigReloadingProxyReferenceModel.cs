#if REFERENCE_MODEL
using Microsoft.Extensions.Configuration;
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

    public class ProxyFoo : ConfigReloadingProxyBase, IFoo, IConfigReloadingProxy<IFoo>
    {
        public ProxyFoo(IConfiguration section, DefaultTypes defaultTypes, ValueConverters valueConverters, Type declaringType, string memberName)
            : base(typeof(IFoo), section, defaultTypes, valueConverters, declaringType, memberName)
        {
        }

        // Properties are strictly pass-through.
        int IFoo.Bar => Object.Bar;

        // Methods are strictly pass-through.
        int IFoo.Baz(int qux) => Object.Baz(qux);

        // Properties are strictly pass-through.
        string IFoo.Qux { get => Object.Qux; set => Object.Qux = value; }

        // Events are pass-through, but also need to capture handlers in a private field.
        private EventHandler _garply;
        event EventHandler IFoo.Garply
        {
            add
            {
                Object.Garply += value;
                _garply += value;
            }
            remove
            {
                Object.Garply -= value;
                _garply -= value;
            }
        }

        // Implicitly implement the Object property so that tooling (e.g. LINQPad) will more
        // easily find this property for display purposes.
        public IFoo Object => (IFoo)_object;

        protected internal override sealed void ReloadObject()
        {
            // If reloadOnChange is explicitly turned off, don't reload the object - just return.
            if (IsReloadOnChangeExplicitlyTurnedOff)
                return;

            // Before doing anything, invoke Reloading.
            OnReloading();

            // Capture the old object and instantiate the new one (but don't set the field).
            IFoo oldObject = Object;
            IFoo newObject = (IFoo)CreateObject();

            // Event handlers from the old object need to be copied to the new one.
            newObject.Garply += _garply;

            // Special case for when the interface has a read/write property: if the new
            // property value is null and the old property value is not null, then copy
            // the value from old to new.
            if (oldObject.Qux != null && newObject.Qux == null)
                newObject.Qux = oldObject.Qux;

            // After the new object has been fully initialized, set the backing field.
            _object = newObject;

            // If the old object is disposable, dispose it after the backing field has been set.
            (oldObject as IDisposable)?.Dispose();

            // After doing everything, invoke Reloaded.
            OnReloaded();
        }
    }
}
#endif
