using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

#if NET
using System;
#endif

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent each of the values in the source series clamped to the inclusive range of <c>min</c> and <c>max</c>.
/// <c>min</c> is lower bound of the result and <c>max</c> is the upper bound of the result.
/// </summary>
/// <remarks>
/// Signature: <c>Clap(min, max, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Clamp(49.95, 50.05, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: Clamp, Limit<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Clamp<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Clamp<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent each of the values in the source series clamped to the inclusive range of min and max.";

    /// <inheritdoc />
    public override string[] Aliases => ["Limit"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "min",
            Default = 1.0D,
            Description = "A floating point value representing the lower bound of the of each value in the series.",
            Required = true
        },
        new ParameterDefinition<double>
        {
            Name = "max",
            Default = 1.0D,
            Description = "A floating point value representing the upper bound of the of each value in the series.",
            Required = true
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        double min = parameters.Value<double>(0);
        double max = parameters.Value<double>(1);

        if (min > max)
            throw new SyntaxErrorException($"For the '{nameof(Clamp<T>)}' function, the min value of the range should be less than the max value of the range: min = {min}, max = {max}");

    #if NET
        return GetDataSourceValues(parameters).Select(dataValue => dataValue with
        {
            Value = Math.Clamp(dataValue.Value, min, max)
        });
    #else
        return GetDataSourceValues(parameters).Select(dataValue => dataValue with
        {
            Value = Math_Clamp(dataValue.Value, min, max)
        });
    #endif
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : Clamp<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Clamp<PhasorValue>
    {
        // Operating on magnitude only
    }

#if !NET
    // No 'Math.Clamp' function exists in .NET Framework, this is its proxy implementation:
    private static double Math_Clamp(double value, double min, double max)
    {
        return value < min ? min : value > max ? max : value;
    }
#endif
}
