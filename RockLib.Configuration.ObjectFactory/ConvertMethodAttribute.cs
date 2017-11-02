using System;

namespace RockLib.Configuration.ObjectFactory
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class ConvertMethodAttribute : Attribute
    {
        public ConvertMethodAttribute(string convertMethodName) => ConvertMethodName = convertMethodName ?? throw new ArgumentNullException(nameof(convertMethodName));
        public string ConvertMethodName { get; }
    }
}
