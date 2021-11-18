using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    internal class ConstructorOrderInfo : IComparable<ConstructorOrderInfo>
    {
        public ConstructorOrderInfo(ConstructorInfo constructor, Dictionary<string, IConfigurationSection> availableMembers, IResolver resolver)
        {
            Constructor = constructor;
            var parameters = constructor.GetParameters();
            TotalParameters = parameters.Length;
            if (TotalParameters == 0)
            {
                IsInvokableWithoutDefaultParameters = true;
                IsInvokableWithDefaultParameters = true;
                MissingParameterNames = Enumerable.Empty<string>();
                MatchedParameters = 0;
            }
            else
            {
                bool HasAvailableValue(ParameterInfo p) =>
                    p.GetNames().Any(name => availableMembers.ContainsKey(name)) || resolver.CanResolve(p);
                bool HasAvailableNamedValue(ParameterInfo p) =>
                    p.GetNames().Any(name => availableMembers.ContainsKey(name));

                IsInvokableWithoutDefaultParameters = parameters.Count(HasAvailableValue) == TotalParameters;
                IsInvokableWithDefaultParameters = parameters.Count(p => HasAvailableValue(p) || p.HasDefaultValue) == TotalParameters;
                MissingParameterNames = parameters.Where(p => !HasAvailableValue(p) && !p.HasDefaultValue).Select(p => p.Name);
                MatchedParameters = parameters.Count(HasAvailableValue);
                MatchedNamedParameters = parameters.Count(HasAvailableNamedValue);
            }
        }

        public ConstructorInfo Constructor { get; }
        public bool IsInvokableWithoutDefaultParameters { get;  }
        public bool IsInvokableWithDefaultParameters { get; }
        public int MatchedParameters { get; }
        public int MatchedNamedParameters { get; }
        public int TotalParameters { get; }
        public IEnumerable<string> MissingParameterNames { get; }

        public int CompareTo(ConstructorOrderInfo other)
        {
            if (IsInvokableWithoutDefaultParameters && !other.IsInvokableWithoutDefaultParameters) return -1;
            if (!IsInvokableWithoutDefaultParameters && other.IsInvokableWithoutDefaultParameters) return 1;
            if (IsInvokableWithDefaultParameters && !other.IsInvokableWithDefaultParameters) return -1;
            if (!IsInvokableWithDefaultParameters && other.IsInvokableWithDefaultParameters) return 1;
            if (MatchedParameters > other.MatchedParameters) return -1;
            if (MatchedParameters < other.MatchedParameters) return 1;
            if (TotalParameters > other.TotalParameters) return -1;
            if (TotalParameters < other.TotalParameters) return 1;
            if (MatchedNamedParameters > other.MatchedNamedParameters) return -1;
            if (MatchedNamedParameters < other.MatchedNamedParameters) return 1;
            return 0;
        }
    }
}
