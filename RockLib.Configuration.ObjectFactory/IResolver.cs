using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    /// <summary>
    /// Represents a dependency injection container, such as Ninject, Unity,
    /// Autofac, or StructureMap.
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Returns a value indicating whether a dependency can be retrieved
        /// that is suitable for the specified parameter.
        /// </summary>
        /// <param name="parameter">
        /// A constructor parameter that needs a value.
        /// </param>
        /// <returns>
        /// true if a dependency can be retrieved for the specified parameter;
        /// otherwise, false.
        /// </returns>
        bool CanResolve(ParameterInfo parameter);

        /// <summary>
        /// Retrieves a dependency suitable for the specified parameter.
        /// </summary>
        /// <param name="parameter">
        /// A constructor parameter that needs a value.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the object that was resolved
        /// for the specified parameter, if it is resolvable; otherwise, null.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if a dependency was successfully retrieved; otherwise, false.
        /// </returns>
        bool TryResolve(ParameterInfo parameter, out object value);
    }
}
