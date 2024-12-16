using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using static GrafanaAdapters.Functions.Common;
// ReSharper disable InconsistentNaming

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent the core data around the mean, excluding outliers, using Z-score. The
/// <c>confidence</c> parameter, optional, is a floating-point value, suffixed with '%' representing a percentage,
/// that must be greater than zero and less than one-hundred, that specifies the amount of data to retain around the
/// mean, representing the central portion of the dataset - defaults to 95%. Second parameter, <c>invertFilter</c>,
/// optional, is a boolean flag that determines if outliers should be rejected or retained - default is false, i.e.,
/// keep core data rejecting outliers; otherwise, true excludes core data and retains only outliers. Third parameter,
/// <c>minSamples</c>, optional, is an integer value that specifies the minimum number of samples required for outlier
/// detection - defaults to 20 for single series temporal analysis and 3 for multi-series slice analysis; if fewer
/// samples are provided, the function returns the entire dataset unfiltered to ensure improved statistical validity
/// of the Z-score calculations, or no data when <c>invertFilter</c> is true. If all values are considered identical,
/// i.e., the standard deviation is zero, function will return the entire dataset unfiltered, or no data when
/// <c>invertFilter</c> is true, since this represents a uniform distribution. The <c>confidence</c> parameter value
/// can either be a constant value or a named target available from the expression. Any target values that fall
/// between 0 and 1 will be treated as a percentage. 
/// </summary>
/// <remarks>
/// Signature: <c>FilterOutliers([confidence = 95%], [invertFilter = false], [minSamples = 20 or 3 for slice], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example 1: <c>FilterOutliers(85%, BROWNS_FERRY:FREQ)</c><br/>
/// Example 2: <c>SliceFilterOutliers(0.033, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: FilterOutliers, ZScoreFilter, GaussianFilter<br/>
/// Execution: Immediate in-memory array load.<br/>
/// Group Operations: Slice, Set
/// </remarks>
public abstract class FilterOutliers<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    // Separate default constants for different analysis modes
    private const int MinimumTimeSeriesSamples = 20;    // For single series temporal analysis
    private const int MinimumSliceSamples = 3;          // For multi-series slice analysis

    /// <inheritdoc />
    public override string Name => nameof(FilterOutliers<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent the core data around the mean, excluding outliers, using Z-score.";

    /// <inheritdoc />
    public override string[] Aliases => ["ZScoreFilter", "GaussianFilter", ];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "confidence",
            Default = "95%",
            Description = "Specifies the percentage of data to retain around the mean, representing the central portion of the dataset. Must be greater than 0% and less than 100%. " +
                          "Examples: \"95%\" (default) retains data within \u00b11.96σ and \"99%\" retains data within \u00b12.58σ.",
            Required = false
        },
        new ParameterDefinition<bool>
        {
            Name = "invertFilter",
            Default = false,
            Description = "A boolean flag that determines if outliers should be retained or rejected. Default is false, i.e., keep core data; otherwise, true excludes core data and retains only outliers.",
            Required = false
        },
        new ParameterDefinition<int>
        {
            Name = "minSamples",
            Default = MinimumTimeSeriesSamples, // Defaults to MinimumSliceSamples for slice analysis
            Description = "Minimum number of samples required for outlier detection. If fewer samples are available, all data is returned unfiltered.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override bool IsSliceSeriesEquivalent => false;

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // ParsePercentage validates percentage is greater than zero and less than 100
        double confidence = ParsePercentage(nameof(confidence), parameters.Value<string>(0), false, false) / 100.0D;
        bool invertFilter = parameters.Value<bool>(1);
        int minSamples = parameters.Value<int>(2);

        // Immediately load values in-memory only enumerating data source once
        T[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);

        // Return all values if fewer samples than minimum required for outlier detection
        if (values.Length < minSamples)
        {
            // Have to assume no outliers for insufficient samples
            if (!invertFilter)
            {
                foreach (T value in values)
                    yield return value;
            }

            yield break;
        }

        // Convert confidence percentage to Z-score (inverse CDF)
        double tailProbability = (1.0D - confidence) / 2.0D; // For two tails
        double zThreshold = InverseNormalCDF(tailProbability);

        // Calculate mean and standard deviation
        double mean = values.Average(item => item.Value);
        double totalVariance = values.Select(item => item.Value - mean).Select(deviation => deviation * deviation).Sum();
        double stdDev = Math.Sqrt(totalVariance / values.Length);

        // Handle zero standard deviation edge case
        if (Math.Abs(stdDev) < double.Epsilon)
        {
            // No outliers in a uniform distribution
            if (!invertFilter)
            {
                foreach (T dataValue in values)
                    yield return dataValue;
            }

            yield break;
        }

        // Filter data based on Z-score threshold
        bool filterByZScore(T value)
        {
            // Keep core data (|Z| <= threshold) or outliers (|Z| > threshold)
            double zScore = Math.Abs((value.Value - mean) / stdDev);
            return invertFilter ? zScore > zThreshold : zScore <= zThreshold;
        }

        foreach (T dataValue in values.Where(filterByZScore))
            yield return dataValue;
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeSliceAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        // Replace default minimum samples parameter with minimum samples for slice operations
        if (parameters.ParsedCount < 3)
            parameters[2].Value = MinimumSliceSamples;

        return ComputeAsync(parameters, cancellationToken);
    }

    private static double InverseNormalCDF(double p)
    {
        // Probability range is validated by caller, just a sanity check here
        Debug.Assert(p is > 0.0D and < 1.0D, "Probability must be between 0 and 1, exclusive.");

        // Using the Abramowitz and Stegun Z-score coefficients for the approximation, see
        // Handbook of Mathematical Functions with Formulas, Graphs, and Mathematical Tables, 1964
        double[] c = [2.515517D, 0.802853D, 0.010328D];
        double[] d = [1.432788D, 0.189269D, 0.001308D];

        // Reflection for upper-tail probabilities
        bool isUpper = p > 0.5D;

        if (isUpper)
            p = 1.0D - p;

        // Approximation formula
        double t = Math.Sqrt(-2.0D * Math.Log(p));
        double numerator = c[0] + c[1] * t + c[2] * t * t;
        double denominator = 1 + d[0] * t + d[1] * t * t + d[2] * t * t * t;
        double z = t - numerator / denominator;

        // Reflect result for upper tail
        return isUpper ? -z : z;
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : FilterOutliers<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : FilterOutliers<PhasorValue>
    {
        // Operating on magnitude only
    }
}
