﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Returns a series of values that represent the difference between consecutive values in the source series.
    /// </summary>
    /// <remarks>
    /// Signature: <c>Difference(expression)</c><br/>
    /// Returns: Series of values.<br/>
    /// Example: <c>Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
    /// Variants: Difference, Diff<br/>
    /// Execution: Deferred enumeration.
    /// </remarks>
    public class Difference : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Difference";

        /// <inheritdoc />
        public string Description { get; } = "Returns a series of values that represent the difference between consecutive values in the source series.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Difference);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Difference|Diff)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true,
                    ParameterTypeName = "data"
                }
            };

        /// <summary>
        /// Computes based on type DataSourceValue
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            // Compute
            double previousTime = dataSourceValues.Source.First().Time;
            IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source
                .Skip(1)
                .Select(pair =>
                {
                    DataSourceValue transformedValue = pair.Value;
                    var previousValue = dataSourceValues.Source.ElementAt(pair.Index - 1).Value;

                    transformedValue.Time = pair.Value.Time;
                    transformedValue.Value = pair.Value.Value - previousValue;

                    previousTime = dataSourceValue.Time;
                    return transformedValue;
                });

            // Set Values
            dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            dataSourceValues.Source = transformedDataSourceValues;

            return dataSourceValues;
        }

        /// <summary>
        /// Computes based on type PhasorValue
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
        {
            // Get Values
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;

            // Compute
            double previousTime = dataSourceValues.Source.First().Time;
            IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source
                .Select((dataSourceValue, index) => new { Value = dataSourceValue, Index = index })
                .Skip(1)
                .Select(pair =>
                {
                    PhasorValue transformedValue = pair.Value;
                    var previousValue = phasorValues.Source.ElementAt(pair.Index - 1);

                    transformedValue.Time = pair.Value.Time;
                    transformedValue.Magnitude = pair.Value.Magnitude - previousValue.Magnitude;
                    transformedValue.Angle = pair.Value.Angle - previousValue.Angle;

                    previousTime = dataSourceValue.Time;
                    return transformedValue;
                });

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            phasorValues.Source = transformedPhasorValues;

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Difference"/> class.
        /// </summary>
        public Difference() { }
    }
}