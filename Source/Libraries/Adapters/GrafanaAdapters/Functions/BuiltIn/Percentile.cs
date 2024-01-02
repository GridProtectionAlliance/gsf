using System.Collections.Generic;
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

    //// Converts value or percent to the number of points selected
    //private static double ConvertToValue(string rawValue)
    //{
    //    try
    //    {
    //        //Percent
    //        if (rawValue.EndsWith("%"))
    //            return Convert.ToDouble(rawValue.TrimEnd('%'));

    //        //Number
    //        return Convert.ToDouble(rawValue);
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception($"Error converting {rawValue} to {typeof(double)}.", ex);
    //    }
    //}

    /// <inheritdoc />
    public class ComputeDataSourceValue : Percentile<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //string rawValue = (parameters[0] as IParameter<string>).Value;
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            //double percent = ConvertToValue(rawValue);

            //// Compute
            //DataSourceValue selectedElement = dataSourceValues.Source.Last();

            //if (percent <= 0)
            //    selectedElement = dataSourceValues.Source.First();
            //else
            //{
            //    List<DataSourceValue> values = dataSourceValues.Source.ToList();
            //    double n = (dataSourceValues.Source.Count() - 1) * (percent / 100.0D) + 1.0D;
            //    int k = (int)n;
            //    if (k >= values.Count)
            //        k = (values.Count - 1);
            //    DataSourceValue kData = values[k];
            //    double d = n - k;
            //    double k0 = values[k - 1].Value;
            //    double k1 = kData.Value;

            //    selectedElement.Value = k0 + d * (k1 - k0);
            //    selectedElement.Time = kData.Time;
            //}

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target})";
            //dataSourceValues.Source = Enumerable.Repeat(selectedElement, 1);

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Percentile<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //string rawValue = (parameters[0] as IParameter<string>).Value;
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;
            //double percent = ConvertToValue(rawValue);

            //// Compute
            //PhasorValue selectedElement = phasorValues.Source.Last();
            //if (percent <= 0)
            //    selectedElement = phasorValues.Source.First();
            //else
            //{
            //    List<PhasorValue> values = phasorValues.Source.ToList();
            //    double n = (phasorValues.Source.Count() - 1) * (percent / 100.0D) + 1.0D;
            //    int k = (int)n;
            //    PhasorValue kData = values[k];
            //    double d = n - k;
            //    double k0 = values[k - 1].Magnitude;
            //    double k1 = kData.Magnitude;

            //    selectedElement.Magnitude = k0 + d * (k1 - k0);
            //    selectedElement.Angle = k0 + d * (k1 - k0);
            //    selectedElement.Time = kData.Time;
            //}

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]});{Name}({labels[1]})";
            //phasorValues.Source = Enumerable.Repeat(selectedElement, 1);

            //return phasorValues;
            return null;
        }
    }
}