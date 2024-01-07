using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.DataSources;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent an adjusted set of angles that are wrapped, per specified angle units, so that angle values are consistently
/// between -180 and +180 degrees. The units parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians,
/// Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.
/// </summary>
/// <remarks>
/// Signature: <c>WrapAngle([units = Degrees], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>WrapAngle(Radians, FILTER TOP 5 ActiveMeasurements WHERE SignalType LIKE '%PHA')</c><br/>
/// Variants: WrapAngle, Wrap<br/>
/// Execution: Deferred enumeration.
/// </remarks>
public abstract class WrapAngle<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(WrapAngle<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent an adjusted set of angles that are wrapped.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Wrap" };

    /// <inheritdoc />
    public override ParameterDefinitions ParameterDefinitions => new List<IParameter>
    {
        new ParameterDefinition<AngleUnit>
        {
            Name = "units",
            Default = AngleUnit.Degrees,
            Description = "Specifies the type of angle units and must be one of the following: Degrees, Radians, Grads, ArcMinutes, ArcSeconds or AngularMil",
            Required = false
        }
    };

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        AngleUnit units = parameters.Value<AngleUnit>(0);

        // Transpose computed value
        T transposeCompute(T dataValue) => dataValue with
        {
            Value = Angle.ConvertFrom(dataValue.Value, units).ToRange(-Math.PI, false).ConvertTo(units)
        };

        // Return deferred enumeration of computed values
        await foreach (T dataValue in GetDataSourceValues(parameters).Select(transposeCompute).WithCancellation(cancellationToken))
            yield return dataValue;
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : WrapAngle<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : WrapAngle<PhasorValue>
    {
        /// <inheritdoc />
        protected override IAsyncEnumerable<PhasorValue> GetDataSourceValues(Parameters parameters)
        {
            // Update data source values to operate on angle components of phasor values
            return base.GetDataSourceValues(parameters).Select(PhasorValue.AngleAsTarget);
        }
    }
}