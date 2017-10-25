using Microsoft.Extensions.Configuration;
using RockLib.Immutable;
using System;
using System.Linq;
using System.Reflection;

namespace RockLib.Configuration
{
    /// <summary>
    /// An object that is bindable to an instance of <see cref="IConfigurationSection"/> of a specific
    /// format: one containing a <see cref="Type"/> property of type <see cref="string"/> and a
    /// <see cref="Value"/> property of type <see cref="IConfigurationSection"/>. The values of these
    /// two properties are used by the <see cref="CreateInstance"/> method.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object to be returned by the <see cref="CreateInstance"/> method.
    /// </typeparam>
    public sealed class LateBoundConfigurationSection<T> where T : class
    {
        private readonly object _typeLocker = new object();
        private readonly object _valueLocker = new object();

        private Semimutable<Type> _type;
        private Semimutable<IConfigurationSection> _value;

        /// <summary>
        /// Gets or sets the assembly qualified name of the concrete class that either: a) is the same
        /// as class T; b) inherits from class T; or c) implements interface T.
        /// </summary>
        public string Type
        {
            get => _type?.Value.AssemblyQualifiedName;
            set => GetField(ref _type, _typeLocker).SetValue(() => GetType(value));
        }

        /// <summary>
        /// Gets or sets the <see cref="IConfigurationSection"/> that represents the raw value for this
        /// instance of <see cref="LateBoundConfigurationSection{T}"/>.
        /// </summary>
        public IConfigurationSection Value
        {
            get => _value?.Value;
            set => GetField(ref _value, _valueLocker).Value = value;
        }

        /// <summary>
        /// Creates and returns an object that is assignable to type <typeparamref name="T"/>. This object
        /// has a concrete type represented by the <see cref="Type"/> property and its properties are
        /// mapped from the <see cref="Value"/> property.
        /// </summary>
        /// <returns>An instance of type <typeparamref name="T"/>.</returns>
        public T CreateInstance() => (T)CreateObject();

        private object CreateObject()
        {
            if (_type == null) throw new InvalidOperationException("Unable to create object:\n- The Type property has not been set.");
            var type = _type.Value;
            if (_value == null) throw new InvalidOperationException($"Unable to create object of type '{type}':\n- The Value property has not been set.");

            Exception bindingException;
            string bindingErrorMessage;

            try
            {
                var obj = Value.Get(type);
                if (obj != null) return obj;
                bindingException = null;
                bindingErrorMessage = "The binding `Get(this IConfiguration, Type)` extension method returned null.";
            }
            catch (Exception ex)
            {
                bindingException = ex;
                bindingErrorMessage = "The binding `Get(this IConfiguration, Type)` extension method threw an exception.";
            }

            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception activatorException)
            {
                var activatorMessage = $"Attempting to invoke the default constructor of the '{type}' type with `Activator.CreateInstance(Type)` threw an exception.";
                var message = $"Unable to create object of type '{type}':\n- {bindingErrorMessage}\n- {activatorMessage}";
                if (bindingException == null) throw new InvalidOperationException(message, activatorException);
                throw new InvalidOperationException(message, new AggregateException(bindingException, activatorException));
            }
        }

        private static Type GetType(string assemblyQualifiedName)
        {
            Type type;
            try
            {
                type = System.Type.GetType(assemblyQualifiedName, throwOnError: true, ignoreCase: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to set the Type property. The type specified by the assembly-qualified name, "
                    + $"'{assemblyQualifiedName}', could not be found.", ex);
            }
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(type))
                throw new InvalidOperationException($"Unable to set the Type property. The specified value, '{type}', is not assignable to type '{typeof(T)}'.");
            return type;
        }

        private static Semimutable<TField> GetField<TField>(ref Semimutable<TField> field, object locker)
        {
            if (field == null) lock (locker) if (field == null) field = new Semimutable<TField>();
            return field;
        }
    }
}
