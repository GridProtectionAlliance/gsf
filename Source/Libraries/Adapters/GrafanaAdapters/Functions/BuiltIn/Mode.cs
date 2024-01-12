using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.Collections;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the mode of the values in the source series. The <c>numberOfBins</c>
/// parameter is used to define how many bins to use when computing the mode for float-point values. A value of
/// zero means use a majority-value algorithm which treats all inputs as integer-based values. When using a value
/// of zero for the number of bins, user should consider using an integer function like <see cref="Round{T}"/>
/// with zero digits, <see cref="Ceiling{T}"/>, <see cref="Floor{T}"/> or <see cref="Truncate{T}"/> as an input
/// to this function to ensure the conversion of values to integer-based values is handled as expected.
/// </summary>
/// <remarks>
/// Signature: <c>Mode([numberOfBins = 0], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example 1: <c>Mode(FILTER TOP 50 ActiveMeasurements WHERE SignalType='DIGI')</c><br/>
/// Example 2: <c>Mode(20, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Example 3: <c>Mode(Round(FILTER ActiveMeasurements WHERE SignalType='FREQ'))</c><br/>
/// Example 4: <c>Divide(100, Mode(0, Floor(Multiply(100, FILTER TOP 20 ActiveMeasurements WHERE SignalType='FREQ'))))</c><br/>
/// Variants: Mode<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Mode<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Mode<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the mode of the values in the source series.";

    /// <inheritdoc />
    public override bool ResultIsSetTargetSeries => true;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<int>
        {
            Name = "numberOfBins",
            Default = 0,
            Description = "An integer value representing the number of bins to use when finding the mode of the values in the source series. Use zero if series values are integer-based.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Immediately load values in-memory only enumerating data source once
        T[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);

        if (values.Length == 0)
            yield break;

        int numberOfBins = parameters.Value<int>(0);

        if (numberOfBins == 0)
        {
            // Use majority-value algorithm executing over integer values
            yield return values.MajorityBy(values.Last(), dataValue => (int)dataValue.Value, false);
        }
        else
        {
            double min = values.Min(dataValue => dataValue.Value);
            double max = values.Max(dataValue => dataValue.Value);
            double range = max - min;
            double binSize = range / numberOfBins;

            int[] bins = new int[numberOfBins];

            foreach (T dataValue in values)
            {
                int binIndex = (int)((dataValue.Value - min) / binSize);

                if (binIndex == numberOfBins)
                    binIndex--; // To handle the max value

                bins[binIndex]++;
            }

            int maxBinCount = bins.Max();
            int maxBinIndex = Array.IndexOf(bins, maxBinCount);
            double modalRangeStart = min + maxBinIndex * binSize;
            double modalRangeEnd = modalRangeStart + binSize;
            double mode = (modalRangeStart + modalRangeEnd) / 2.0D;

            // Find data value closest to computed mode
            T closestDataValue = values[0];
            double closestDistance = Math.Abs(closestDataValue.Value - mode);

            foreach (T dataValue in values)
            {
                double currentDistance = Math.Abs(dataValue.Value - mode);

                if (currentDistance >= closestDistance)
                    continue;

                closestDataValue = dataValue;
                closestDistance = currentDistance;
            }

            // Return computed results
            yield return closestDataValue with { Value = mode };
        }
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : Mode<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Mode<PhasorValue>
    {
        // Operating on magnitude only
    }
}