using System;

namespace RockLib.Configuration.ObjectFactory
{
    public interface IConfigReloadingProxy<T>
    {
        T Object { get; }
        event EventHandler Reloading;
        event EventHandler Reloaded;
    }
}
