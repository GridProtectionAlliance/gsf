using GrafanaAdapters.DataSourceValueTypes;
using GrafanaAdapters.DataSourceValueTypes.BuiltIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a single value selected using the first series of a slice of values as the zero-based index from the remaining series.
/// The <c>sliceTolerance</c> parameter is a floating-point value that must be greater than or equal to 0.001 that represents the
/// desired time tolerance, in seconds, for the time slice.
/// </summary>
/// <remarks>
/// Signature: <c>Switch(sliceTolerance, expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Switch(IndexSeriesTag; FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
/// Variants: Switch, Select<br/>
/// Execution: Immediate enumeration.<br/>
/// Group Operations: Slice
/// </remarks>
public abstract class Switch<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValueType<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Switch<T>);

    /// <inheritdoc />
    public override string Description => "Returns a single value selected using the first series of a slice of values as the zero-based index from the remaining series.";

    /// <inheritdoc />
    public override string[] Aliases => ["Select"];

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Scalar;

    /// <inheritdoc />
    // Function only operates on slices. Other group operations are ignored, see CheckAllowedGroupOperation.
    public override GroupOperations AllowedGroupOperations => GroupOperations.Slice;

    /// <inheritdoc />
    // Only non-group operation "Switch" is published instead of requiring "SliceSwitch".
    public override GroupOperations PublishedGroupOperations => GroupOperations.None;

    // No parameters other than slice tolerance are required for this function. Note that
    // required slice tolerance parameter added automatically by Grafana function handling.

    /// <inheritdoc />
    public override GroupOperations CheckAllowedGroupOperation(GroupOperations requestedOperation)
    {
        // Force group operation to be Slice as Switch only supports slice operations. This ignores
        // any requested group operation instead of throwing an exception based on what is allowed.
        return GroupOperations.Slice;
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<T> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Immediately enumerate to get desired values
        await using IAsyncEnumerator<T> enumerator = GetDataSourceValues(parameters).GetAsyncEnumerator(cancellationToken);

        // Get index from first series
        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
            yield break;

        int index;

        try
        {
            index = (int)enumerator.Current.Value;
        }
        catch (OverflowException ex)
        {
            throw new SyntaxErrorException($"Series \"{enumerator.Current.Target ?? "undefined"}\" value \"{enumerator.Current.Value}\" cannot be interpreted as an integer index: {ex.Message}", ex);
        }

        // Skip to desired index
        for (int i = 0; i < index; i++)
        {
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                yield break;
        }

        yield return enumerator.Current;
    }


    /// <inheritdoc />
    public class ComputeMeasurementValue : Switch<MeasurementValue>
    {
    }

    /// <inheritdoc />
    public class ComputePhasorValue : Switch<PhasorValue>
    {
        // Index from first series comes from magnitude only (IDataSourceValueType.Value defaults to Magnitude for PhasorValue)
    }
}