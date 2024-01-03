using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.
/// N, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.
/// </summary>
/// <remarks>
/// Signature: <c>Round([N = 0], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Round(3, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Round<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Round<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Round";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.";

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<int>
        {
            Name = "N",
            Default = 0,
            Description = "A positive integer value representing the number of decimal places in the return value - defaults to 0.",
            Required = false
        }
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Round<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            int valueN = parameters.ParsedCount == 0 ? 0 : parameters.Value<int>(0);

            // Transpose computed value
            DataSourceValue transposeCompute(DataSourceValue dataValue) => dataValue with
            {
                Value = Math.Round(dataValue.Value, valueN)
            };

            // Return deferred enumeration of computed values
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Round<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            int valueN = parameters.ParsedCount == 0 ? 0 : parameters.Value<int>(0);

            // Transpose computed value
            PhasorValue transposeCompute(PhasorValue dataValue) => dataValue with
            {
                Magnitude = Math.Round(dataValue.Magnitude, valueN),
                Angle = Math.Round(dataValue.Angle, valueN)
            };

            // Return deferred enumeration of computed values
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }
}