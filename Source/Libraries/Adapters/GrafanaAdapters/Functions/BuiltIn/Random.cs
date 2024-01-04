using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF;
using GSF.Collections;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of N, or N% of total, values that are a random sample of the values in the source series.
/// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
/// suffixed with '%' representing a percentage, that must range from greater than 0 to less than or equal to 100.
/// Third parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
/// N can either be constant value or a named target available from the expression. Any target values that fall between 0
/// and 1 will be treated as a percentage.
/// </summary>
/// <remarks>
/// Signature: <c>Random(N|N%, [normalizeTime = true], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Random(25%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: Random, Rand, Sample<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Random<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Random";

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values that are a random sample of the values in the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Rand", "Sample" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "N",
            Default = "1",
            Description = "A integer value or percent representing number or % of elements to take.",
            Required = true
        },
        new ParameterDefinition<bool>
        {
            Name = "normalizeTime",
            Default = true,
            Description = "A boolean flag which representing if time in dataset should be normalized.",
            Required = false
        },
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Random<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Immediately load values in-memory only enumerating data source once
            DataSourceValue[] values = GetDataSourceValues(parameters).ToArray();

            if (values.Length == 0)
                yield break;

            int count = ParseCount(parameters.Value<string>(0), values.Length);

            if (count > values.Length)
                count = values.Length;

            bool normalizeTime = parameters.ParsedCount == 1 || parameters.Value<bool>(1);
            double baseTime = values[0].Time;
            double timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
            List<int> indexes = new(Enumerable.Range(0, values.Length));
            indexes.Scramble();

            DataSourceValue transposeOrder(DataSourceValue dataValue, int index) => new()
            {
                Value = values[index].Value,
                Time = normalizeTime ? baseTime + index * timeStep : values[index].Time,
                Target = values[index].Target,
                Flags = values[index].Flags
            };

            // Return immediate enumeration of computed values
            foreach (DataSourceValue dataValue in values.Take(count).Select(transposeOrder))
                yield return dataValue;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Random<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Immediately load values in-memory only enumerating data source once
            PhasorValue[] values = GetDataSourceValues(parameters).ToArray();

            if (values.Length == 0)
                yield break;

            int count = ParseCount(parameters.Value<string>(0), values.Length);

            if (count > values.Length)
                count = values.Length;

            bool normalizeTime = parameters.ParsedCount == 1 || parameters.Value<bool>(1);
            double baseTime = values[0].Time;
            double timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
            List<int> indexes = new(Enumerable.Range(0, values.Length));
            indexes.Scramble();

            PhasorValue transposeOrder(PhasorValue dataValue, int index) => new()
            {
                Magnitude = values[index].Magnitude,
                Angle = values[index].Angle,
                Time = normalizeTime ? baseTime + index * timeStep : values[index].Time,
                MagnitudeTarget = values[index].MagnitudeTarget,
                AngleTarget = values[index].AngleTarget,
                Flags = values[index].Flags
            };

            // Return immediate enumeration of computed values
            foreach (PhasorValue dataValue in values.Take(count).Select(transposeOrder))
                yield return dataValue;
        }
    }
}