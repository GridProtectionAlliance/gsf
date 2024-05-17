using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using GSF.NumericalAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that are passed though a Kalman filter which predicts the next state based on the current estimate useful
/// for filtering out noise or reducing variance from a series of values. Optional parameters include <c>processNoise</c> which represents
/// how much the system state is expected to change between measurements, <c>measurementNoise</c> which represents the confidence in the
/// measurements, and <c>estimatedError</c> which represents the initial guess about the error in the state estimate.
/// </summary>
/// <remarks>
/// Signature: <c>KalmanFilter([processNoise = 1e-5], [measurementNoise = 1e-3], [estimatedError = 1], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>LQE(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
/// Variants: KalmanFilter, LQE, LinearQuadraticEstimate<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class KalmanFilter<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(KalmanFilter<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that are passed though a Kalman filter which predicts the next state based on the current estimate useful for filtering out noise or reducing variance from a series of values.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "LQE", " LinearQuadraticEstimate" };

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<double>
        {
            Name = "processNoise",
            Default = KalmanFilter.DefaultProcessNoise,
            Description = "A floating point value representing how much the system state is expected to change between measurements.",
            Required = false
        },
        new ParameterDefinition<double>
        {
            Name = "measurementNoise",
            Default = KalmanFilter.DefaultMeasurementNoise,
            Description = "A floating point value representing the confidence in the measurements. A lower value gives more weight to the measurements.",
            Required = false
        },
        new ParameterDefinition<double>
        {
            Name = "estimatedError",
            Default = KalmanFilter.DefaultEstimateError,
            Description = "A floating point value representing the initial guess about the error in the state estimate.",
            Required = false
        }
    };

    /// <inheritdoc />
    public override IAsyncEnumerable<T> ComputeAsync(Parameters parameters, CancellationToken cancellationToken)
    {
        double processNoise = parameters.Value<double>(0);
        double measurementNoise = parameters.Value<double>(1);
        double estimatedError = parameters.Value<double>(2);

        KalmanFilter filter = new(processNoise, measurementNoise, estimatedError);

        return GetDataSourceValues(parameters).Select(dataValue => dataValue with
        {
            Value = filter.Update(dataValue.Value)
        });
    }

    /// <inheritdoc />
    public class ComputeMeasurementValue : KalmanFilter<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : KalmanFilter<PhasorValue>
    {
        // Operating on magnitude only
    }
}
