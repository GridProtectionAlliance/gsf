using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GrafanaAdapters.GrafanaFunctions
{
    internal static class FunctionsModelHelper
    {
        public const string ExpressionFormat = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";
        public static void CheckValuesIntegrity(IGrafanaFunction functionsModel, object[] values)
        {
            if (values.Length < functionsModel.Parameters.Count(p => p.Required) || values.Length > functionsModel.Parameters.Count)
            {
                throw new ArgumentException("Incorrect number of values.");
            }

            for (int i = 0; i < functionsModel.Parameters.Count; i++)
            {
                IParameter parameter = functionsModel.Parameters[i];

                if (i < values.Length)
                {
                    object value = values[i];

                    if (value == null && parameter.Required)
                    {
                        throw new ArgumentNullException($"Required parameter '{parameter.Description}' is null.");
                    }

                    if (value != null && !IsValueCompatibleWithType(value, parameter.GetType()))
                    {
                        throw new ArgumentException($"Value type does not match parameter type for '{parameter.Description}'.");
                    }
                }
                else if (parameter.Required)
                {
                    throw new ArgumentException($"Missing value for required parameter '{parameter.Description}'.");
                }
            }
        }


        private static bool IsValueCompatibleWithType(object value, Type parameterType)
        {
            // Invalid parameter type
            if (!parameterType.IsGenericType || parameterType.GetGenericTypeDefinition() != typeof(Parameter<>))
            {
                return false;
            }

            Type parameterValueType = parameterType.GetGenericArguments()[0];

            return value == null || parameterValueType.IsAssignableFrom(value.GetType());
        }
    }

    internal class Parameter<T> : IParameter<T>
    {
        public T Default { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
    }


    internal class QueryDataHolder
    {
        public Target SourceTarget { get; }
        public DateTime StartTime { get; }
        public DateTime StopTime { get; }
        public string Interval { get; }
        public bool IncludePeaks { get; }
        public bool DropEmptySeries { get; }

        public CancellationToken CancellationToken { get; }

        public QueryDataHolder(Target sourceTarget, DateTime startTime, DateTime stopTime, string interval, bool includePeaks, bool dropEmptySeries, CancellationToken cancellationToken)
        {
            SourceTarget = sourceTarget;
            StartTime = startTime;
            StopTime = stopTime;
            Interval = interval;
            IncludePeaks = includePeaks;
            DropEmptySeries = dropEmptySeries;
            CancellationToken = cancellationToken;
        }
    }
}
