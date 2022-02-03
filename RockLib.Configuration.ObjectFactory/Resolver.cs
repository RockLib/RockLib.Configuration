using System;
using System.Diagnostics.CodeAnalysis;
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
         if (resolve is null) throw new ArgumentNullException(nameof(resolve));

         CanResolve = p =>
         {
            return resolve(p.ParameterType) is not null;
         };

         Resolve = p =>
         {
            return resolve(p.ParameterType);
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
         if (resolve is null) throw new ArgumentNullException(nameof(resolve));
         if (canResolve is null) throw new ArgumentNullException(nameof(canResolve));

         CanResolve = p =>
         {
            return canResolve(p.ParameterType);
         };

         Resolve = p =>
         {
            return resolve(p.ParameterType);
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
         if (resolve is null) throw new ArgumentNullException(nameof(resolve));
         if (resolveNamed is null) throw new ArgumentNullException(nameof(resolveNamed));

         CanResolve = p =>
         {
            if (resolveNamed(p.ParameterType, p.Name!) is not null)
               return true;

            return resolve(p.ParameterType) is not null;
         };

         Resolve = p =>
         {
            var obj = resolveNamed(p.ParameterType, p.Name!);
            if (obj is not null)
               return obj;

            return resolve(p.ParameterType);
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
         if (resolve is null) throw new ArgumentNullException(nameof(resolve));
         if (resolveNamed is null) throw new ArgumentNullException(nameof(resolveNamed));
         if (canResolve is null) throw new ArgumentNullException(nameof(canResolve));
         if (canResolveNamed is null) throw new ArgumentNullException(nameof(canResolveNamed));

         CanResolve = p =>
         {
            if (canResolveNamed(p.ParameterType, p.Name!))
               return true;

            return canResolve(p.ParameterType);
         };

         Resolve = p =>
         {
            var obj = resolveNamed(p.ParameterType, p.Name!);
            if (obj is not null)
               return obj;

            return resolve(p.ParameterType);
         };
      }

      /// <summary>
      /// Gets a <see cref="Resolver"/> that cannot resolve for any constructor parameters.
      /// </summary>
      public static Resolver Empty { get; } = new Resolver(t => new object(), t => false);

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

      // Since there's already a CanResolve property on the class definition
      // we can't resolve these CA issues.
#pragma warning disable CA1033 // Interface methods should be callable by child types
      bool IResolver.CanResolve(ParameterInfo parameter) => CanResolve(parameter);
#pragma warning restore CA1033 // Interface methods should be callable by child types

      /// <inheritdoc/>
      public bool TryResolve(ParameterInfo parameter, [MaybeNullWhen(false)] out object value)
      {
         if (CanResolve(parameter))
         {
            value = Resolve(parameter);
            return value is not null;
         }

         value = null;
         return false;
      }
   }
}
