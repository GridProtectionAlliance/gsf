using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
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
    public class Label : IGrafanaFunction
    {
        /// <inheritdoc />
        public string Name { get; } = "Label";

        /// <inheritdoc />
        public string Description { get; } = "Renames a series with the specified label value.";

        /// <inheritdoc />
        public Type Type { get; } = typeof(Label);

        /// <inheritdoc />
        public Regex Regex { get; } = new Regex(string.Format(FunctionsModelHelper.ExpressionFormat, "(Label|Name)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public List<IParameter> Parameters { get; } =
            new List<IParameter>
            {
                new Parameter<string>
                {
                    Default = "",
                    Description = "Specifies label to be renamed to.",
                    Required = true
                },
                new Parameter<IDataSourceValueGroup>
                {
                    Default = new DataSourceValueGroup<DataSourceValue>(),
                    Description = "Data Points",
                    Required = true
                }
            };

        /// <summary>
        /// Computes based on type DataSourceValue
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters)
        {
            // Get Values
            string label = (parameters[0] as IParameter<string>).Value;
            DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Set Values
            dataSourceValues.Target = $"{label}";

            return dataSourceValues;
        }

        /// <summary>
        /// Computes based on type PhasorValue
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters)
        {
            // Get Values
            string label = (parameters[0] as IParameter<string>).Value;
            DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[1] as IParameter<IDataSourceValueGroup>).Value;

            // Set Values
            string[] labels = phasorValues.Target.Split(';');
            phasorValues.Target = $"{label}.MAG;{label}.ANG";

            return phasorValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        public Label() { }
    }

}
