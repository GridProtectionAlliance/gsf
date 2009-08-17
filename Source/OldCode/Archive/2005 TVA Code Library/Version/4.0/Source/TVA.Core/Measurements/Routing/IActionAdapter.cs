//*******************************************************************************************************
//  IActionAdapter.cs
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
//  05/08/2009 - James R. Carroll
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
    /// Represents thet abstract interface for any action adapter.
    /// </summary>
    [CLSCompliant(false)]
    public interface IActionAdapter : IAdapter
	{
        /// <summary>
        /// Provides new measurements from action adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of new measurements for host to process.
        /// </remarks>
        event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        /// <summary>
        /// This event is raised every second allowing consumer to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        event EventHandler<EventArgs<int>> UnpublishedSamples;

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        int FramesPerSecond { get; set; }

        /// <summary>
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
		IMeasurement[] OutputMeasurements { get; set; }

        /// <summary>
        /// Gets or sets primary keys of input measurements the action adapter expects.
        /// </summary>
        MeasurementKey[] InputMeasurementKeys { get; set; }

		/// <summary>
        /// Gets or sets minimum number of input measurements required for calcualtion.  Set to -1 to require all.
		/// </summary>
		int MinimumMeasurementsToUse { get; set; }

        /// <summary>
        /// Queues measurements for calculation processing.  Measurements are automatically filters to the defined
        /// <see cref="InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for calculation processing.</param>
        void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements);
    }
}