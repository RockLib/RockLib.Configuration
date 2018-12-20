using System;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// An adapter implementation of <see cref="IResolver"/> that can
    /// support most dependency injection containers.
    /// </summary>
    public class Resolver : IResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Resolver"/> class.
        /// <para>
        /// Use this constructor when your dependency injection container does not
        /// have a "CanResolve" method and support for named parameters is not
        /// required or supported.
        /// </para>
        /// </summary>
        /// <param name="resolve">
        /// A delegate that returns a dependency for the specified type.
        /// </param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Resolver"/> class.
        /// <para>
        /// Use this constructor when support for named parameters is not
        /// required or supported.
        /// </para>
        /// </summary>
        /// <param name="resolve">
        /// A delegate that returns a dependency for the specified type.
        /// </param>
        /// <param name="canResolve">
        /// A delegate that returns a value indicating whether a dependency can
        /// be resolved for the specified type.
        /// </param>
        public Resolver(Func<Type, object> resolve, Func<Type, bool> canResolve)
        {
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));
            if (canResolve == null) throw new ArgumentNullException(nameof(canResolve));

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Resolver"/> class.
        /// <para>
        /// Use this constructor when your dependency injection container does not
        /// have a "CanResolve" method and support for named parameters is required.
        /// </para>
        /// </summary>
        /// <param name="resolve">
        /// A delegate that returns a dependency for the specified type.
        /// </param>
        /// <param name="resolveNamed">
        /// A delegate that returns a dependency for the specified type and parameter name.
        /// </param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Resolver"/> class.
        /// <para>
        /// Use this constructor when support for named parameters is required.
        /// </para>
        /// </summary>
        /// <param name="resolve">
        /// A delegate that returns a dependency for the specified type.
        /// </param>
        /// <param name="resolveNamed">
        /// A delegate that returns a dependency for the specified type and parameter name.
        /// </param>
        /// <param name="canResolve">
        /// A delegate that returns a value indicating whether a dependency can
        /// be resolved for the specified type.
        /// </param>
        /// <param name="canResolveNamed">
        /// A delegate that returns a value indicating whether a dependency can
        /// be resolved for the specified type and parameter name.
        /// </param>
        public Resolver(Func<Type, object> resolve, Func<Type, string, object> resolveNamed,
            Func<Type, bool> canResolve, Func<Type, string, bool> canResolveNamed)
        {
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));
            if (resolveNamed == null) throw new ArgumentNullException(nameof(resolveNamed));
            if (canResolve == null) throw new ArgumentNullException(nameof(canResolve));
            if (canResolveNamed == null) throw new ArgumentNullException(nameof(canResolveNamed));

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

        /// <summary>
        /// Gets a <see cref="Resolver"/> that cannot resolve for any constructor parameters.
        /// </summary>
        public static Resolver Empty { get; } = new Resolver(t => null, t => false);

        /// <summary>
        /// Gets the function that determines whether a dependency can be resolved
        /// for a given constructor parameter.
        /// </summary>
        public Func<ParameterInfo, bool> CanResolve { get; }

        /// <summary>
        /// Gets the function that retrieves a dependency for a given constructor
        /// parameter.
        /// </summary>
        public Func<ParameterInfo, object> Resolve { get; }

        bool IResolver.CanResolve(ParameterInfo parameter) => CanResolve(parameter);

        /// <inheritdoc/>
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
