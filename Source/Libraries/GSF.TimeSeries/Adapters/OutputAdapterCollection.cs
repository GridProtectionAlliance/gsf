//******************************************************************************************************
//  OutputAdapterCollection.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using GSF.Diagnostics;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents a collection of <see cref="IOutputAdapter"/> implementations.
    /// </summary>
    public class OutputAdapterCollection : AdapterCollectionBase<IOutputAdapter>, IOutputAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event is raised every five seconds allowing host to track total number of unprocessed measurements.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each <see cref="IOutputAdapter"/> implementation reports its current queue size of unprocessed
        /// measurements so that if queue size reaches an unhealthy threshold, host can take evasive action.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is total number of unprocessed measurements.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnprocessedMeasurements;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="OutputAdapterCollection"/>.
        /// </summary>
        public OutputAdapterCollection()
        {
            base.Name = "Output Adapter Collection";
            base.DataMember = "OutputAdapters";
            base.MonitorTimerEnabled = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the total number of measurements processed and destined for archive thus far by each
        /// <see cref="IOutputAdapter"/> implementation in the <see cref="OutputAdapterCollection"/>.
        /// </summary>
        public override long ProcessedMeasurements
        {
            get
            {
                long processedMeasurements = 0;

                // Calculate new total for all archive destined output adapters
                lock (this)
                {
                    foreach (IOutputAdapter item in this)
                    {
                        if (item.OutputIsForArchive)
                            processedMeasurements += item.ProcessedMeasurements;
                    }
                }

                return processedMeasurements;
            }
        }

        /// <summary>
        /// Returns a flag that determines if all measurements sent to this <see cref="OutputAdapterCollection"/> are
        /// destined for archival.
        /// </summary>
        public virtual bool OutputIsForArchive
        {
            get
            {
                lock (this)
                {
                    foreach (IOutputAdapter item in this)
                    {
                        if (item.OutputIsForArchive)
                            return true;
                    }
                }

                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Queues a collection of measurements for processing to each <see cref="IOutputAdapter"/> implementation in
        /// this <see cref="OutputAdapterCollection"/>.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            try
            {
                lock (this)
                {
                    foreach (IOutputAdapter item in this)
                    {
                        item.QueueMeasurementsForProcessing(measurements);
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to queue measurements to output adapters: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// This function removes a range of measurements from the internal measurement queues. Note that the requested
        /// <paramref name="total"/> will be removed from each <see cref="IOutputAdapter"/> implementation in this
        /// <see cref="OutputAdapterCollection"/>.
        /// </summary>
        /// <param name="total">Total measurements to remove from the each <see cref="IOutputAdapter"/> queue.</param>
        /// <remarks>
        /// This method is typically only used to curtail size of measurement queue if it's getting too large.  If more points are
        /// requested than there are points available - all points in the queue will be removed.
        /// </remarks>
        public virtual void RemoveMeasurements(int total)
        {
            lock (this)
            {
                foreach (IOutputAdapter item in this)
                {
                    if (item.Enabled)
                        item.RemoveMeasurements(total);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="UnprocessedMeasurements"/> event.
        /// </summary>
        /// <param name="unprocessedMeasurements">Total measurements in the queue that have not been processed.</param>
        protected virtual void OnUnprocessedMeasurements(int unprocessedMeasurements)
        {
            try
            {
                UnprocessedMeasurements?.Invoke(this, new EventArgs<int>(unprocessedMeasurements));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for UnprocessedMeasurements event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IOutputAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IOutputAdapter"/> implementation.</param>
        protected override void InitializeItem(IOutputAdapter item)
        {
            if ((object)item != null)
            {
                // Wire up unprocessed measurements event
                item.UnprocessedMeasurements += item_UnprocessedMeasurements;
                base.InitializeItem(item);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IOutputAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IOutputAdapter"/> to dispose.</param>
        protected override void DisposeItem(IOutputAdapter item)
        {
            if ((object)item != null)
            {
                // Un-wire unprocessed measurements event
                item.UnprocessedMeasurements -= item_UnprocessedMeasurements;
                base.DisposeItem(item);
            }
        }

        // Raise unprocessed measurements event on behalf of each item in collection
        private void item_UnprocessedMeasurements(object sender, EventArgs<int> e) => UnprocessedMeasurements?.Invoke(sender, e);

        #endregion
    }
}