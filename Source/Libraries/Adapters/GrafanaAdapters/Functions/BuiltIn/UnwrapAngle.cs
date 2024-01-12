using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GrafanaAdapters.DataSources;
using GSF.Units;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a series of values that represent an adjusted set of angles that are unwrapped, per specified angle units, so that a comparable mathematical
/// operation can be executed. For example, for angles that wrap between -180 and +180 degrees, this algorithm unwraps the values to make the values
/// mathematically comparable. The <c>units</c>parameter, optional, specifies the type of angle units and must be one of the following: Degrees, Radians,
/// Grads, ArcMinutes, ArcSeconds or AngularMil - defaults to Degrees.
/// </summary>
/// <remarks>
/// Signature: <c>UnwrapAngle([units = Degrees], expression)</c><br/>
/// Returns: Series of values.<br/>
/// Example: <c>UnwrapAngle(FSX_PMU2-PA1:VH; REA_PMU3-PA2:VH)</c><br/>
/// Variants: UnwrapAngle, Unwrap<br/>
/// Execution: Immediate in-memory array load.
/// </remarks>
public abstract class UnwrapAngle<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(UnwrapAngle<T>);

    /// <inheritdoc />
    public override string Description => "Returns a series of values that represent an adjusted set of angles that are unwrapped.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Unwrap" };

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

        T[] values = await GetDataSourceValues(parameters).ToArrayAsync(cancellationToken).ConfigureAwait(false);

        Angle anglesFromValues(T dataValue) =>
            Angle.ConvertFrom(dataValue.Value, units);

        T valuesFromAngles(Angle angle, int index) => values[index] with
        {
            Value = angle.ConvertTo(units)
        };

        foreach (T dataValue in Angle.Unwrap(values.Select(anglesFromValues)).Select(valuesFromAngles))
            yield return dataValue;
    }

    /// <inheritdoc />
    public class ComputeDataSourceValue : UnwrapAngle<DataSourceValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : UnwrapAngle<PhasorValue>
    {
        /// <inheritdoc />
        protected override IAsyncEnumerable<PhasorValue> GetDataSourceValues(Parameters parameters)
        {
            // Update data source values to operate on angle components of phasor values
            return base.GetDataSourceValues(parameters).Select(PhasorValue.AngleAsTarget);
        }
    }
}