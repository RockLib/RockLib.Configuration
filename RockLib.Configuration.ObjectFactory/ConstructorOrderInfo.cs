using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RockLib.Configuration.ObjectFactory
{
    internal class ConstructorOrderInfo : IComparable<ConstructorOrderInfo>
    {
        public ConstructorOrderInfo(ConstructorInfo constructor, Dictionary<string, IConfigurationSection> availableMembers)
        {
            Constructor = constructor;
            var parameters = constructor.GetParameters();
            TotalParameters = parameters.Length;
            if (TotalParameters == 0)
            {
                MatchedParametersRatio = 1;
                MatchedOrDefaultParametersRatio = 1;
            }
            else
            {
                MatchedParametersRatio = parameters.Count(p => availableMembers.ContainsKey(p.Name)) / (double)TotalParameters;
                MatchedOrDefaultParametersRatio = parameters.Count(p => availableMembers.ContainsKey(p.Name) || p.HasDefaultValue) / (double)TotalParameters;
            }
        }

        public ConstructorInfo Constructor { get; }
        public double MatchedParametersRatio { get; }
        public double MatchedOrDefaultParametersRatio { get; }
        public int TotalParameters { get; }

        public int CompareTo(ConstructorOrderInfo other)
        {
            if (MatchedParametersRatio > other.MatchedParametersRatio) return -1;
            if (MatchedParametersRatio < other.MatchedParametersRatio) return 1;
            if (MatchedOrDefaultParametersRatio > other.MatchedOrDefaultParametersRatio) return -1;
            if (MatchedOrDefaultParametersRatio < other.MatchedOrDefaultParametersRatio) return 1;
            if (TotalParameters > other.TotalParameters) return -1;
            if (TotalParameters < other.TotalParameters) return 1;
            return 0;
        }
    }
}
