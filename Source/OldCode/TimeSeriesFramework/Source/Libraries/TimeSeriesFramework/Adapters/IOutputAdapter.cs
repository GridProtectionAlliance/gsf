//******************************************************************************************************
//  IOutputAdapter.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA;

namespace TimeSeriesFramework.Adapters
{
    /// <summary>
    /// Represents that abstract interface for any outgoing adapter.
    /// </summary>
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