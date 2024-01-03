using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent each of the values in the source series divided by N.
/// N is a floating point value representing a divisive factor to be applied to each value the source series.
/// N can either be constant value or a named target available from the expression.
/// </summary>
/// <remarks>
/// Signature: <c>Divide(N, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Divide(1.732, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
/// Variants: Divide<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Divide<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Divide";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series divided with N.";

    /// <inheritdoc />
    public override GroupOperations PublishedGroupOperations => GroupOperations.Standard | GroupOperations.Set;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "N",
            Default = 0,
            Description = "A floating point value representing an divisive offset to be applied to each value the source series.",
            Required = true
        },
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Divide<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            double valueN = parameters.Value<double>(0);

            // Transpose computed value
            DataSourceValue transposeCompute(DataSourceValue dataValue) => dataValue with
            {
                Value = dataValue.Value / valueN
            };

            // Return deferred enumeration of computed values
            foreach (DataSourceValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Divide<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            double valueN = parameters.Value<double>(0);

            // Transpose computed value
            PhasorValue transposeCompute(PhasorValue dataValue) => dataValue with
            {
                Magnitude = dataValue.Magnitude / valueN,
                Angle = dataValue.Angle / valueN
            };

            // Return deferred enumeration of computed values
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters).Select(transposeCompute))
                yield return dataValue;
        }
    }
}