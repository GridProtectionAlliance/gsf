﻿using System;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GrafanaAdapters.Functions.BuiltIn;

/// <summary>
/// Returns a phasor refferenced to the first series of a slice of values.
/// The <c>sliceTolerance</c> parameter is a floating-point value that must be greater than or equal to 0.001 that represents the
/// desired time tolerance, in seconds, for the time slice.
/// </summary>
/// <remarks>
/// Signature: <c>Reference(sliceTolerance, expression)</c><br/>
/// Returns: Single value.<br/>
/// Example: <c>Reference(ReferencePhasor; FILTER PhasorValues WHERE SignalType='IPHM')</c><br/>
/// Variants: Ref<br/>
/// Execution: Immediate enumeration.
/// </remarks>
public abstract class Reference<T> : GrafanaFunctionBase<T> where T : struct, IDataSourceValue<T>
{
    /// <inheritdoc />
    public override string Name => nameof(Reference<T>);

    /// <inheritdoc />
    public override string Description => "Returns a set of series referenced to the first Phasor.";

    /// <inheritdoc />
    public override string[] Aliases => new[] { "Ref" };

    /// <inheritdoc />
    public override ReturnType ReturnType => ReturnType.Series;

    /// <inheritdoc />
    // Function only operates on slices. Other group operations are ignored, see CheckAllowedGroupOperation.
    public override GroupOperations AllowedGroupOperations => GroupOperations.Slice;

    /// <inheritdoc />
    // Only non-group operation "Reference" is published instead of requiring "SliceReference".
    public override GroupOperations PublishedGroupOperations => GroupOperations.None;

    // No parameters other than slice tolerance are required for this function. Note that
    // required slice tolerance parameter added automatically by Grafana function handling.

    /// <inheritdoc />
    public override GroupOperations CheckAllowedGroupOperation(GroupOperations requestedOperation)
    {
        // Force group operation to be Slice as Reference only supports slice operations. This ignores
        // any requested group operation instead of throwing an exception based on what is allowed.
        return GroupOperations.Slice;
    }

   
    /// <inheritdoc />
    public class ComputePhasorValue : Reference<PhasorValue>
    {
        /// <inheritdoc />
        public override async IAsyncEnumerable<PhasorValue> ComputeAsync(Parameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using IAsyncEnumerator<PhasorValue> enumerator = GetDataSourceValues(parameters).GetAsyncEnumerator(cancellationToken);

            // Get reference from first series
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                yield break;

            double reference = enumerator.Current.Angle;

            // Return First Series
            PhasorValue current = enumerator.Current;
            current.Angle = current.Angle - reference;

            yield return current;

            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                current = enumerator.Current;
                current.Angle = current.Angle - reference;

                yield return current;
            }        
        }

    }
}