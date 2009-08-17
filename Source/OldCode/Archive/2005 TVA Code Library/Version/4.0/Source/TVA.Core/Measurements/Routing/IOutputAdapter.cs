//*******************************************************************************************************
//  IOutputAdapter.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/01/2006 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA;
using TVA.Measurements;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents that abstract interface for any outgoing adapter.
    /// </summary>
    [CLSCompliant(false)]
    public interface IOutputAdapter : IAdapter
	{
        /// <summary>
        /// Event is raised every second allowing host to track total number of unprocessed measurements.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Implementations of this interface are expected to report current queue size of unprocessed
        /// measurements so that if queue size reaches an unhealthy threshold, host can take evasive action.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is total number of unprocessed measurements.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<int>> UnprocessedMeasurements;

        /// <summary>
        /// Gets the total number of measurements processed thus far by the <see cref="IOutputAdapter"/>.
        /// </summary>
        long ProcessedMeasurements { get; }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this <see cref="IOutputAdapter"/> are
        /// destined for archival.
        /// </summary>
        /// <remarks>
        /// This property allows the <see cref="OutputAdapterCollection"/> to calculate statistics on how
        /// many measurements have been archived per minute. Historians would normally set this property
        /// to <c>true</c>; other custom exports would set this property to <c>false</c>.
        /// </remarks>
        bool OutputIsForArchive { get; }
				
        /// <summary>
        /// Queues measurements for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
		void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements);

        /// <summary>
        /// Implementations of this function should remove a range of measurements from the internal measurement queue.
        /// </summary>
        /// <remarks>
        /// This method is typically only used to curtail size of measurement queue if it's getting too large.  If more
        /// points are requested than there are points available - all points in the queue should be removed.
        /// </remarks>
        void RemoveMeasurements(int total);
	}
}