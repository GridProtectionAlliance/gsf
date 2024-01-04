using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value that represents the Nth order percentile for the sorted values in the source series.
/// N is a floating point value, representing a percentage, that must range from 0 to 100.
/// </summary>
/// <remarks>
/// Signature: <c>Percentile(N[%], expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: Percentile, Pctl<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class Percentile<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "Percentile";

    /// <inheritdoc />
    public override string Description => "Returns a series of N, or N% of total, values from the start of the source series.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Pctl" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<string>
        {
            Name = "N",
            Default = "100",
            Description = "A floating point value, representing a percentage, that must range from 0 to 100.",
            Required = true
        },
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Percentile<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Immediately load values in-memory only enumerating data source once
            DataSourceValue[] values = GetDataSourceValues(parameters).ToArray();

            if (values.Length == 0)
                yield break;

            Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : a.Value > b.Value ? 1 : 0);
            int count = values.Length;

            double percent = ParsePercentage(parameters.Value<string>(0));

            switch (percent)
            {
                case 0.0D:
                    yield return values.First();
                    break;
                case 100.0D:
                    yield return values.Last();
                    break;
                default:
                    double n = (count - 1) * (percent / 100.0D) + 1.0D;
                    int k = (int)n;
                    DataSourceValue kData = values[k];
                    double d = n - k;
                    double k0 = values[k - 1].Value;
                    double k1 = kData.Value;

                    yield return kData with
                    {
                        Value = k0 + d * (k1 - k0)
                    };
                    break;
            }
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Percentile<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            // Immediately load values in-memory only enumerating data source once
            PhasorValue[] values = GetDataSourceValues(parameters).ToArray();

            if (values.Length == 0)
                yield break;

            Array.Sort(values, (a, b) => a.Magnitude < b.Magnitude ? -1 : a.Magnitude > b.Magnitude ? 1 : 0);
            int count = values.Length;

            double percent = ParsePercentage(parameters.Value<string>(0));

            switch (percent)
            {
                case 0.0D:
                    yield return values.First();
                    break;
                case 100.0D:
                    yield return values.Last();
                    break;
                default:
                    double n = (count - 1) * (percent / 100.0D) + 1.0D;
                    int k = (int)n;
                    PhasorValue kData = values[k];
                    double d = n - k;
                    double k0 = values[k - 1].Magnitude;
                    double k1 = kData.Magnitude;

                    yield return kData with
                    {
                        Magnitude = k0 + d * (k1 - k0)
                    };
                    break;
            }
        }
    }
}