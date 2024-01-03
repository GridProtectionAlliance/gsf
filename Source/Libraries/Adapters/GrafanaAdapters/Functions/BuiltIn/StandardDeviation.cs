using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.NumericalAnalysis;
using GSF.TimeSeries;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the standard deviation of the values in the source series. Second parameter,
/// optional, is a boolean flag representing if the sample based calculation should be used - defaults to false, which
/// means the population based calculation should be used.
/// </summary>
/// <remarks>
/// Signature: <c>StandardDeviation([useSampleCalc = false], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: StandardDeviation, StdDev<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class StandardDeviation<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "StandardDeviation";

    /// <inheritdoc />
    public override string Description => "Returns a single value that represents the standard deviation of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "StdDev" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<bool>
        {
            Name = "useSampleCalc",
            Default = false,
            Description = "A boolean flag representing if the sample based calculation should be used.",
            Required = false
        }
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : StandardDeviation<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            bool useSampleCalc = parameters.ParsedCount > 0 && parameters.Value<bool>(0);

            double lastTime = 0.0D;
            string lastTarget = null;
            MeasurementStateFlags lastFlags = 0;

            IEnumerable<double> trackedValues = GetDataSourceValues(parameters).Select(dataValue =>
            {
                lastTime = dataValue.Time;
                lastTarget = dataValue.Target;
                lastFlags = dataValue.Flags;
                return dataValue.Value;
            });

            // Return immediate enumeration of computed values
            yield return new DataSourceValue()
            {
                // StandardDeviation uses immediate in-memory array load
                Value = trackedValues.StandardDeviation(useSampleCalc),
                Time = lastTime,
                Target = lastTarget,
                Flags = lastFlags
            };
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : StandardDeviation<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            bool useSampleCalc = parameters.ParsedCount > 0 && parameters.Value<bool>(0);

            List<double> magnitudes = new();
            List<double> angles = new();

            double lastTime = 0.0D;
            string lastMagnitudeTarget = null;
            string lastAngleTarget = null;
            MeasurementStateFlags lastFlags = 0;

            // Immediately load values in-memory only enumerating data source once
            foreach (PhasorValue dataValue in GetDataSourceValues(parameters))
            {
                magnitudes.Add(dataValue.Magnitude);
                angles.Add(dataValue.Angle);

                lastTime = dataValue.Time;
                lastMagnitudeTarget = dataValue.MagnitudeTarget;
                lastAngleTarget = dataValue.AngleTarget;
                lastFlags = dataValue.Flags;
            }

            // Return immediate enumeration of computed values
            yield return new PhasorValue()
            {
                Magnitude = magnitudes.StandardDeviation(useSampleCalc),
                Angle = angles.StandardDeviation(useSampleCalc),
                Time = lastTime,
                MagnitudeTarget = lastMagnitudeTarget,
                AngleTarget = lastAngleTarget,
                Flags = lastFlags
            };
        }
    }
}