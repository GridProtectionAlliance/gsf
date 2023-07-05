using GSF.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Description;
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
        public T Value { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public Type ParameterType => typeof(T);
        public string ParameterTypeName { get; set; }

        public void SetValue(GrafanaDataSourceBase dataSourceBase, object value, string target)
        {
            // No value specified
            if (value == null)
            {
                // Required -> error
                if (this.Required)
                {
                    throw new ArgumentException($"Required parameter '{this.GetType().ToString()}' is missing.");
                }
                // Not required -> default
                else
                {
                    Value = this.Default;
                }
            }

            // Data
            if(typeof(T) == typeof(DataSourceValueGroup))
            {
                Value = (T)value;
                return;
            }

            string valueString = value.ToString();

            // String
            if (typeof(T) == typeof(string))
            {
                Value = (T)(object)valueString;
                return;
            }
            
            // Attempt to convert
            try
            {
                Value = (T)Convert.ChangeType(valueString, typeof(T));
            }

            // Not proper type, check metadata
            catch (Exception)
            {
                // Attempt to get metadata
                DataRow[] rows = dataSourceBase?.Metadata.Tables["ActiveMeasurements"].Select($"PointTag = '{target}'") ?? new DataRow[0];
                string metaValue = string.Empty;
                if (rows.Length > 0 && rows[0].Table.Columns.Contains(valueString))
                {
                    metaValue = rows[0][valueString].ToString();
                }

                // Did not find
                if(metaValue == string.Empty)
                {
                    //Value = this.Default;
                    //return;
                    throw new ArgumentException($"Did not locate valid metadata for '{target}'.");
                }

                // Found, attempt to convert
                try
                {
                    Value = (T)Convert.ChangeType(metaValue, typeof(T));
                }
                catch (Exception ex)
                {
                    //Value = this.Default;
                    throw new Exception("Error converting " + valueString + " to " + typeof(T) + " with found metadata of " + metaValue + ".", ex);
                }
            }
        }
        //public void GetValue()
        //{

        //}
    }



    internal class QueryDataHolder
    {
        public Target SourceTarget { get; }
        public DateTime StartTime { get; }
        public DateTime StopTime { get; }
        public string Interval { get; }
        public bool IncludePeaks { get; }
        public bool DropEmptySeries { get; }
        public Dictionary<string, List<string>> MetadataSelection { get; }

        public CancellationToken CancellationToken { get; }

        public QueryDataHolder(Target sourceTarget, DateTime startTime, DateTime stopTime, string interval, bool includePeaks, 
            bool dropEmptySeries, Dictionary<string, List<string>> metadataSelection, CancellationToken cancellationToken)
        {
            SourceTarget = sourceTarget;
            StartTime = startTime;
            StopTime = stopTime;
            Interval = interval;
            IncludePeaks = includePeaks;
            DropEmptySeries = dropEmptySeries;
            MetadataSelection = metadataSelection;
            CancellationToken = cancellationToken;
        }
    }
}
