using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.FunctionParsing;

namespace GrafanaAdapters.Functions;

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
    public override ParameterDefinitions Parameters => new List<IParameter>
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
        public override IEnumerable<DataSourceValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //bool useSample = (parameters[1] as IParameter<bool>).Value;

            //// Compute
            //DataSourceValue lastElement = dataSourceValues.Source.Last();
            //lastElement.Value = dataSourceValues.Source.Select(dataValue => { return dataValue.Value; }).StandardDeviation(useSample);

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{useSample})";
            //dataSourceValues.Source = Enumerable.Repeat(lastElement, 1);

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : StandardDeviation<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(List<IParameter> parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //bool useSample = (parameters[1] as IParameter<bool>).Value;

            //// Compute
            //PhasorValue lastElement = phasorValues.Source.Last();
            //lastElement.Magnitude = phasorValues.Source.Select(dataValue => { return dataValue.Magnitude; }).StandardDeviation(useSample);

            //lastElement.Angle = phasorValues.Source.Select(dataValue => { return dataValue.Angle; }).StandardDeviation(useSample);

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]},{useSample});{Name}({labels[1]},{useSample})";
            //phasorValues.Source = Enumerable.Repeat(lastElement, 1);

            //return phasorValues;
            return null;
        }
    }
}