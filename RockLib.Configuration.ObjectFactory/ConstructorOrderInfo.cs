using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
   internal sealed class ConstructorOrderInfo : IComparable<ConstructorOrderInfo>
   {
      public ConstructorOrderInfo(ConstructorInfo constructor, Dictionary<string, IConfigurationSection> availableMembers, IResolver? resolver)
      {
         Constructor = constructor;
         var parameters = constructor.GetParameters();
         TotalParameters = parameters.Length;
         if (TotalParameters == 0)
         {
            IsInvokableWithoutDefaultParameters = true;
            IsInvokableWithDefaultParameters = true;
            MissingParameterNames = new List<string>();
            MatchedParameters = 0;
            ParameterTypes = new List<Type>();
         }
         else
         {
            bool HasAvailableValue(ParameterInfo p) =>
                p.GetNames().Any(name => availableMembers.ContainsKey(name)) || (resolver?.CanResolve(p) ?? false);
            bool HasAvailableNamedValue(ParameterInfo p) =>
                p.GetNames().Any(name => availableMembers.ContainsKey(name));

            IsInvokableWithoutDefaultParameters = parameters.Count(HasAvailableValue) == TotalParameters;
            IsInvokableWithDefaultParameters = parameters.Count(p => HasAvailableValue(p) || p.HasDefaultValue) == TotalParameters;
            MissingParameterNames = parameters.Where(p => !HasAvailableValue(p) && !p.HasDefaultValue).Select(p => p.Name!).ToList();
            MatchedParameters = parameters.Count(HasAvailableValue);
            MatchedNamedParameters = parameters.Count(HasAvailableNamedValue);
            ParameterTypes = parameters.Select(p => p.ParameterType).ToList();
         }
      }

      public ConstructorInfo Constructor { get; }
      public bool IsInvokableWithoutDefaultParameters { get; }
      public bool IsInvokableWithDefaultParameters { get; }
      public int MatchedParameters { get; }
      public int MatchedNamedParameters { get; }
      public List<Type> ParameterTypes { get; }
      public int TotalParameters { get; }
      public List<string> MissingParameterNames { get; }

      public int CompareTo(ConstructorOrderInfo? other)
      {
         if (other is null) return 1;
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
         // Finds parameter types in ParameterTypes that are not in other.ParameterTypes
         if (ParameterTypes.Except(other.ParameterTypes).Any()) return -1;
         // Finds parameter types in other.ParameterTypes that are not in ParameterTypes
         if (other.ParameterTypes.Except(ParameterTypes).Any()) return 1;
         return 0;
      }
   }
}
