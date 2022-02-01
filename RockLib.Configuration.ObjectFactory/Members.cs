using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    internal static class Members
    {
        public static IEnumerable<Member> Find(Type declaringType, string memberName)
        {
            if (declaringType == null || memberName == null) return Enumerable.Empty<Member>();
            var constructorParameters = FindConstructorParameters(declaringType, memberName).ToList();
            return FindProperties(declaringType, memberName, constructorParameters).Concat(constructorParameters);
        }

        private static IEnumerable<Member> FindConstructorParameters(Type declaringType, string memberName) =>
            declaringType.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(c => c.GetParameters())
                .Where(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, memberName))
                .Select(p => new Member(p.Name!, p.ParameterType, MemberType.ConstructorParameter));

        private static IEnumerable<Member> FindProperties(Type declaringType, string memberName, List<Member> constructorParameters) =>
            declaringType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, memberName)
                    && (p.CanWrite
                        || ((p.IsReadonlyList() || p.IsReadonlyDictionary())
                            && !constructorParameters.Any(c => StringComparer.OrdinalIgnoreCase.Equals(c.Name, memberName)))))
                .Select(p => new Member(p.Name, p.PropertyType, MemberType.Property));

        public static bool IsReadonlyList(this PropertyInfo p) =>
            p.CanRead
            && !p.CanWrite
            && ((p.PropertyType.GetTypeInfo().IsGenericType
                    && (p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                        || p.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)
                        || p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
                || p.PropertyType.IsNonGenericList());

        public static bool IsReadonlyDictionary(this PropertyInfo p) =>
            p.CanRead
            && !p.CanWrite
            && p.PropertyType.GetTypeInfo().IsGenericType
            && (p.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                || p.PropertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    internal class Member
    {
        public Member(string name, Type type, MemberType memberType)
        {
            Name = name;
            Type = type;
            MemberType = memberType;
        }

        public string Name { get; }
        public Type Type { get; }
        public MemberType MemberType { get; }

        public override string ToString() =>
            $"{(MemberType == MemberType.Property ? "Property" : "Constructor parameter")}: {Type} {Name}";
    }

    internal enum MemberType
    {
        Property,
        ConstructorParameter
    }
}
