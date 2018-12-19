using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    public interface IResolver
    {
        bool CanResolve(ParameterInfo parameter);

        bool TryResolve(ParameterInfo parameter, out object value);
    }
}
