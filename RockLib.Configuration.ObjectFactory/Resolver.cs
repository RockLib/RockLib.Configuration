using System;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    public class Resolver : IResolver
    {
        public Resolver(Func<Type, object> resolve)
        {
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            CanResolve = p =>
            {
                try { return resolve(p.ParameterType) != null; }
                catch { return false; }
            };

            Resolve = p =>
            {
                try { return resolve(p.ParameterType); }
                catch { return null; }
            };
        }

        public Resolver(Func<Type, bool> canResolve, Func<Type, object> resolve)
        {
            if (canResolve == null) throw new ArgumentNullException(nameof(canResolve));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            CanResolve = p =>
            {
                try { return canResolve(p.ParameterType); }
                catch { return false; }
            };

            Resolve = p =>
            {
                try { return resolve(p.ParameterType); }
                catch { return null; }
            };
        }

        public Resolver(Func<Type, object> resolve, Func<Type, string, object> resolveNamed)
        {
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));
            if (resolveNamed == null) throw new ArgumentNullException(nameof(resolveNamed));

            CanResolve = p =>
            {
                try
                {
                    if (resolveNamed(p.ParameterType, p.Name) != null)
                        return true;
                }
                catch {}

                try { return resolve(p.ParameterType) != null; }
                catch { return false; }
            };

            Resolve = p =>
            {
                try
                {
                    var obj = resolveNamed(p.ParameterType, p.Name);
                    if (obj != null)
                        return obj;
                }
                catch {}

                try { return resolve(p.ParameterType); }
                catch { return null; }
            };
        }

        public Resolver(Func<Type, bool> canResolve, Func<Type, string, bool> canResolveNamed,
            Func<Type, object> resolve, Func<Type, string, object> resolveNamed)
        {
            if (canResolve == null) throw new ArgumentNullException(nameof(canResolve));
            if (canResolveNamed == null) throw new ArgumentNullException(nameof(canResolveNamed));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));
            if (resolveNamed == null) throw new ArgumentNullException(nameof(resolveNamed));

            CanResolve = p =>
            {
                try
                {
                    if (canResolveNamed(p.ParameterType, p.Name))
                        return true;
                }
                catch {}

                try { return canResolve(p.ParameterType); }
                catch { return false; }
            };

            Resolve = p =>
            {
                try
                {
                    var obj = resolveNamed(p.ParameterType, p.Name);
                    if (obj != null)
                        return obj;
                }
                catch {}

                try { return resolve(p.ParameterType); }
                catch { return null; }
            };
        }

        public static Resolver Empty { get; } = new Resolver(t => false, t => null);

        public Func<ParameterInfo, bool> CanResolve { get; }

        public Func<ParameterInfo, object> Resolve { get; }

        bool IResolver.CanResolve(ParameterInfo parameter) => CanResolve(parameter);

        public bool TryResolve(ParameterInfo parameter, out object value)
        {
            if (CanResolve(parameter))
            {
                value = Resolve(parameter);
                return value != null;
            }

            value = null;
            return false;
        }
    }
}
