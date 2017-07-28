using Microsoft.Extensions.Configuration;
using RockLib.Immutable;
using System;
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
            set => GetField(ref _type, _typeLocker).Value = GetType(value);
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
        public T CreateInstance() => (T)Value.Get(_type.Value);

        private static Type GetType(string assemblyQualifiedName)
        {
            var type = System.Type.GetType(assemblyQualifiedName, true, true);
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(type))
                throw new InvalidOperationException();
            return type;
        }

        private static Semimutable<TField> GetField<TField>(ref Semimutable<TField> field, object locker)
        {
            if (field == null) lock (locker) if (field == null) field = new Semimutable<TField>();
            return field;
        }
    }
}
