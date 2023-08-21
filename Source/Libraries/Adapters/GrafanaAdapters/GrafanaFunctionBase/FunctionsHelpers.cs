using GrafanaAdapters.GrafanaFunctionBase;
using GSF.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using GSF.Units;


namespace GrafanaAdapters.GrafanaFunctions
{
    internal static class FunctionsModelHelper
    {
        public const string ExpressionFormat = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";
    }

    internal class Parameter<T> : IParameter<T>
    {
        public T Default { get; set; }
        public T Value { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public Type ParameterType => typeof(T);
        public string ParameterTypeName { get; set; }

        private (T Value, bool Success) LookupMetadata(GrafanaDataSourceBase dataSourceBase, Dictionary<string, string> metadata, string value, string target, bool isPhasor)
        {
            // Attempt to find in dictionary
            if (metadata.Count != 0 && metadata.TryGetValue(value, out string metaValue))
            {
                // Found, attempt to convert
                try
                {
                    return ((T)Convert.ChangeType(metaValue, typeof(T)), true);
                }
                catch 
                {
                    return (default(T), false);
                }
            }
            // Not found, check ActiveMeasurements
            else
            {
                DataRow[] rows;
                if (isPhasor)
                    rows = dataSourceBase?.Metadata.Tables["Phasor"].Select($"Label = '{target}'") ?? new DataRow[0];
                else
                    rows = dataSourceBase?.Metadata.Tables["ActiveMeasurements"].Select($"PointTag = '{target}'") ?? new DataRow[0];

                //Not valid
                if (!(rows.Length > 0 && rows[0].Table.Columns.Contains(value)))
                {
                    return (default(T), false);
                }

                // Found, attempt to convert
                string foundValue = rows[0][value].ToString();
                try
                {
                    return ((T)Convert.ChangeType(foundValue, typeof(T)), true);
                }
                catch 
                {
                    return (default(T), false);
                }
            }
        }

        /*
         * This function is used to convert the value to the proper type
         * If the type of value provided and expected match, then it directly converts
         * If the types do not match, then it first searches through the provided metadata.
         * If nothing is found, it looks through ActiveMeasurements for it.
         * Finally, if none of the above work it throws an error.
         */
        public void SetValue(GrafanaDataSourceBase dataSourceBase, object value, string target, 
            Dictionary<string, string> metadata, bool isPhasor)
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
                    return;
                }
            }

            // Data
            if(typeof(T) == typeof(IDataSourceValueGroup))
            {
                Value = (T)value;
                return;
            }

            // Check if requested metadata
            string valueString = value.ToString();
            if (valueString.StartsWith("{") && valueString.EndsWith("}"))
            {
                valueString = valueString.Substring(1, valueString.Length - 2);
                var result = LookupMetadata(dataSourceBase, metadata, valueString, target, isPhasor);
                if (result.Success)
                    valueString = result.Value.ToString();
            }

            // Time Unit
            if (typeof(T) == typeof(TargetTimeUnit))
            {
                if (!TargetTimeUnit.TryParse(valueString, out TargetTimeUnit timeUnit))
                    Value = (T)(object)timeUnit;
                else
                    Value = this.Default;

                return;
            }

            // Angle Unit
            if (typeof(T) == typeof(AngleUnit))
            {
                if (Enum.TryParse(valueString, out AngleUnit angleUnit))
                    Value = (T)(object)angleUnit;
                else
                    Value = this.Default;

                return;
            }

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
                var result = LookupMetadata(dataSourceBase, metadata, valueString, target, isPhasor);
                if (result.Success)
                    Value = result.Value;
                else
                    throw new Exception($"Unable convert or find corresponding metadata for {valueString}");
            }
        }
    }



    internal class QueryDataHolder
    {
        public Target SourceTarget { get; }
        public DateTime StartTime { get; }
        public DateTime StopTime { get; }
        public string Interval { get; }
        public bool IncludePeaks { get; }
        public bool DropEmptySeries { get; }
        public bool IsPhasor { get; }
        public Dictionary<string, List<string>> MetadataSelection { get; }

        public CancellationToken CancellationToken { get; }

        public QueryDataHolder(Target sourceTarget, DateTime startTime, DateTime stopTime, string interval, bool includePeaks, 
            bool dropEmptySeries, bool isPhasor, Dictionary<string, List<string>> metadataSelection, CancellationToken cancellationToken)
        {
            SourceTarget = sourceTarget;
            StartTime = startTime;
            StopTime = stopTime;
            Interval = interval;
            IncludePeaks = includePeaks;
            DropEmptySeries = dropEmptySeries;
            IsPhasor = isPhasor;
            MetadataSelection = metadataSelection;
            CancellationToken = cancellationToken;
        }
    }
}
