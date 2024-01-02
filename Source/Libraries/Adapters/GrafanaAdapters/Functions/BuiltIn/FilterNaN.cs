using System.Collections.Generic;
using System.Threading;
using GrafanaAdapters.DataSources;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.
/// First parameter, optional, is a boolean flag that determines if infinite values should also be excluded - defaults to true.
/// </summary>
/// <remarks>
/// Signature: <c>FilterNaN([alsoFilterInfinity = true], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>FilterNaN(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
/// Variants: FilterNaN<br/>
/// Execution: Deferred enumeration.
/// </remarks> 
public abstract class FilterNaN<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => "FilterNaN";

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent a filtered set of the values in the source series where each value is a real number, i.e., value is not NaN.";

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<bool>
        {
            Name = "alsoFilterInfinity",
            Default = true,
            Description = "A boolean flag that determines if infinite values should also be excluded - defaults to true",
            Required = false
        },
    };

    /// <inheritdoc />
    public class ComputeDataSourceValue : FilterNaN<DataSourceValue>
    {
        /// <inheritdoc />
        public override IEnumerable<DataSourceValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<DataSourceValue> dataSourceValues = (DataSourceValueGroup<DataSourceValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //bool filterInfinity = (parameters[1] as IParameter<bool>).Value;

            //// Compute
            //IEnumerable<DataSourceValue> transformedDataSourceValues = dataSourceValues.Source.Where(dataValue => !(double.IsNaN(dataValue.Value) || (filterInfinity && double.IsInfinity(dataValue.Value))));

            //// Set Values
            //dataSourceValues.Target = $"{Name}({dataSourceValues.Target},{filterInfinity})";
            //dataSourceValues.Source = transformedDataSourceValues;

            //return dataSourceValues;
            return null;
        }
    }

    /// <inheritdoc />
    public class ComputePhasorValue : FilterNaN<PhasorValue>
    {
        /// <inheritdoc />
        public override IEnumerable<PhasorValue> Compute(Parameters parameters, CancellationToken cancellationToken)
        {
            //// Get Values
            //DataSourceValueGroup<PhasorValue> phasorValues = (DataSourceValueGroup<PhasorValue>)(parameters[0] as IParameter<IDataSourceValueGroup>).Value;
            //bool filterInfinity = (parameters[1] as IParameter<bool>).Value;

            //// Compute
            //IEnumerable<PhasorValue> transformedPhasorValues = phasorValues.Source.Where(phasorValue => !(double.IsNaN(phasorValue.Magnitude) || double.IsNaN(phasorValue.Angle) || (filterInfinity && (double.IsInfinity(phasorValue.Magnitude) || double.IsInfinity(phasorValue.Angle)))));

            //// Set Values
            //string[] labels = phasorValues.Target.Split(';');
            //phasorValues.Target = $"{Name}({labels[0]},{filterInfinity});{Name}({labels[1]},{filterInfinity})";
            //phasorValues.Source = transformedPhasorValues;

            //return phasorValues;
            return null;
        }
    }
}