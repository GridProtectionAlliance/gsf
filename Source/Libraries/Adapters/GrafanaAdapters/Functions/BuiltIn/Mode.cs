using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.DataSources;
using GSF.Collections;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the mode of the values in the source series.
/// </summary>
/// <remarks>
/// Signature: <c>Mode(expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')</c><br/>
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

    // TODO: JRC - consider adding a "number of bins" parameter to better estimate mode for a set double values

    /*
        Since a set of double values here are unlikely to repeat, finding mode by its classic definition, the most
        frequently occurring value, might not be useful in this scenario. Finding a value (or values) around which
        data is most densely clustered may be a better approach. One method is to use binning: you divide the the
        range of data into intervals (bins) and then find the bin with the most values:

            double[] data = { /* your array of doubles * / };
            int numberOfBins = 10; // Choose a number that suits your data, perhaps parameterize this

            double min = data.Min();
            double max = data.Max();
            double range = max - min;
            double binSize = range / numberOfBins;

            int[] bins = new int[numberOfBins];

            foreach (double value in data)
            {
                int binIndex = (int)((value - min) / binSize);

                if (binIndex == numberOfBins)
                    binIndex--; // To handle the max value

                bins[binIndex]++;
            }

            int maxBinCount = bins.Max();
            int maxBinIndex = Array.IndexOf(bins, maxBinCount);
            double modalRangeStart = min + maxBinIndex * binSize;
            double modalRangeEnd = modalRangeStart + binSize;

            Console.WriteLine($"Modal Range: [{modalRangeStart}, {modalRangeEnd})");

        Since we want a single answer, we could take the midpoint of the modal range as our mode.

        Would want this to be optional since source data could represent integer values, in which case
        the existing algorithm would be fine. Perhaps default "numberOfBins" to 0 which would mean to
        not use binning, i.e., instead use the existing algorithm.
    */

    /// <inheritdoc />
    public class ComputeDataSourceValue : Mode<DataSourceValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<DataSourceValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // Immediately load values in-memory only enumerating data source once
            DataSourceValue[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken);
            yield return values.MajorityBy(values.Last(), dataValue => dataValue.Value, false);
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Mode<PhasorValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<PhasorValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            List<double> magnitudes = new();
            List<double> angles = new();
            PhasorValue lastValue = default;

            // Immediately load values in-memory only enumerating data source once
            await foreach (PhasorValue dataValue in GetDataSourceValues(parameters).WithCancellation(cancellationToken))
            {
                lastValue = dataValue;
                magnitudes.Add(dataValue.Magnitude);
                angles.Add(dataValue.Angle);
            }

            if (magnitudes.Count == 0)
                yield break;

            double magnitudeMode = magnitudes.Majority(false);
            double angleMode = angles.Majority(false);

            // Return computed results
            if (lastValue.Time > 0.0D)
            {
                yield return lastValue with
                {
                    Magnitude = magnitudeMode,
                    Angle = angleMode
                };
            }
        }
    }
}