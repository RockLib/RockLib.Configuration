using System;
using System.Reflection;

namespace RockLib.Configuration.ProxyFactory
{
    internal static class Exceptions
    {
        internal static ArgumentException CannotCreateProxyOfNonInterfaceType(Type type) =>
            new ArgumentException($"Cannot create proxy instance of non-interface type {type}.", nameof(type));

        internal static ArgumentException TargetInterfaceCannotHaveAnyMethods(Type type, MethodInfo m) =>
            new ArgumentException($"Cannot create proxy {type} implementation: target interface cannot contain any methods. `{m}`", nameof(type));

        internal static ArgumentException TargetInterfaceCannotHaveAnyEvents(Type type, EventInfo e) =>
            new ArgumentException($"Cannot create proxy {type} implementation: target interface cannot contain any events. `{e}`", nameof(type));

        internal static ArgumentException TargetInterfaceCannotHaveAnyIndexerProperties(Type type, PropertyInfo p) =>
            new ArgumentException($"Cannot create proxy {type} implementation: target interface cannot contain any indexer properties. `{p.PropertyType.Name} this[{string.Join(", ", p.GetIndexParameters().Select(i => i.ParameterType.Name))}] {{ {(p.CanRead ? "get; " : "")} {(p.CanWrite ? "set; " : "")}}}`", nameof(type));

        internal static ArgumentException TargetInterfaceCannotHaveAnyWriteOnlyProperties(Type type, PropertyInfo p) =>
            new ArgumentException($"Cannot create proxy {type} implementation: target interface cannot contain write-only methods. `{p} {{ set; }}`", nameof(type));
    }
}
