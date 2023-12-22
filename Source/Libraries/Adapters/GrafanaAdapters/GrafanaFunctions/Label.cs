using System.Collections.Generic;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters.GrafanaFunctions;

/// <summary>
/// Renames a series with the specified label value. If multiple series are targeted, labels will be indexed starting at one, e.g., if there are three
/// series in the target expression with a label value of "Max", series would be labeled as "Max 1", "Max 2" and "Max 3". Group operations on this
/// function will be ignored. The label parameter also supports substitutions when root target metadata can be resolved. For series values that directly
/// map to a point tag, metadata value substitutions for the tag can be used in the label value - for example: {ID}, {SignalID}, {PointTag}, {AlternateTag},
/// {SignalReference}, {Device}, {FramesPerSecond}, {Protocol}, {ProtocolType}, {SignalType}, {EngineeringUnits}, {PhasorType}, {Company}, {Description} -
/// where applicable, these substitutions can be used in any combination.
/// </summary>
/// <remarks>
/// Signature: <c>Label(value, expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>Label('AvgFreq', SetAvg(FILTER TOP 20 ActiveMeasurements WHERE SignalType='FREQ'))</c><br/>
/// Variants: Label, Name<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class Label<T> : GrafanaFunctionBase<T> where T : IDataSourceValue
{
    /// <inheritdoc />
    public override string Name => "Label";

    /// <inheritdoc />
    public override string Description => "Renames a series with the specified label value.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Name" };

    /// <inheritdoc />
    public override List<IParameter> Parameters => new()
    {
        new Parameter<string>
        {
            Default = "",
            Description = "Specifies label to be renamed to.",
            Required = true
        },

        InputDataPointValues
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : Label<DataSourceValue>
    {
        /// <inheritdoc />
        public override DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            string label = (parameters[0] as IParameter<string>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Set Values
            dataSourceValues.Target = $"{label}";

            return dataSourceValues;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Label<PhasorValue>
    {
        /// <inheritdoc />
        public override DataSourceValueGroup<PhasorValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            string label = (parameters[0] as IParameter<string>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Set Values
            phasorValues.Target = $"{label}";

            return phasorValues;
        }
    }
}