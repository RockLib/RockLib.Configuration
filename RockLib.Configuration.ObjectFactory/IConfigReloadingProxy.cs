using System;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Represents a proxy object that reloads its underlying value.
    /// </summary>
    /// <typeparam name="TInterface">The interface type that this proxy class wraps.</typeparam>
    public interface IConfigReloadingProxy<TInterface>
    {
        /// <summary>
        /// Gets the underlying object.
        /// </summary>
        TInterface Object { get; }

        /// <summary>
        /// Occurs immediately before the underlying object is reloaded.
        /// </summary>
        event EventHandler Reloading;

        /// <summary>
        /// Occurs immediately after the underlying object is reloaded.
        /// </summary>
        event EventHandler Reloaded;
    }
}
