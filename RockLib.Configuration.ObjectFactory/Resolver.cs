using System;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    public class Resolver : IResolver
    {
        public Resolver(Func<ParameterInfo, bool> canResolve, Func<ParameterInfo, object> resolve)
        {
            CanResolve = canResolve ?? throw new ArgumentNullException(nameof(canResolve));
            Resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
        }

        public static Resolver FromDelegates(Func<Type, bool> canResolve, Func<Type, object> resolve)
        {
            if (canResolve == null) throw new ArgumentNullException(nameof(canResolve));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));
            return new Resolver(p => canResolve(p.ParameterType), p => resolve(p.ParameterType));
        }

        public static Resolver FromDelegates(Func<Type, string, bool> canResolve, Func<Type, string, object> resolve)
        {
            if (canResolve == null) throw new ArgumentNullException(nameof(canResolve));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));
            return new Resolver(p => canResolve(p.ParameterType, p.Name), p => resolve(p.ParameterType, p.Name));
        }

        public static Resolver Empty { get; } = new Resolver(p => false, p => null);

        public Func<ParameterInfo, bool> CanResolve { get; }

        public Func<ParameterInfo, object> Resolve { get; }

        bool IResolver.CanResolve(ParameterInfo parameter) => CanResolve(parameter);

        public bool TryResolve(ParameterInfo parameter, out object value)
        {
            if (CanResolve(parameter))
            {
                try
                {
                    value = Resolve(parameter);
                    return value != null;
                }
                catch
                {
                }
            }

            value = null;
            return false;
        }
    }
}
