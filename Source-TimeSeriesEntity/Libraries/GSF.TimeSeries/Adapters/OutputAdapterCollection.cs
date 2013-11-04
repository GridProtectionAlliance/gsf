//******************************************************************************************************
//  OutputAdapterCollection.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  11/04/2013 - Stephen C. Wills
//       Updated to process time-series entities.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents a collection of <see cref="IOutputAdapter"/> implementations.
    /// </summary>
    public class OutputAdapterCollection : AdapterCollectionBase<IOutputAdapter>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event is raised every five seconds allowing host to track total number of unprocessed time-series entities.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each <see cref="IOutputAdapter"/> implementation reports its current queue size of unprocessed
        /// entities so that if queue size reaches an unhealthy threshold, host can take evasive action.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is total number of unprocessed entities.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnprocessedEntities;

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
        /// Gets the total number of time-series entities processed and destined for archive thus far by each
        /// <see cref="IOutputAdapter"/> implementation in the <see cref="OutputAdapterCollection"/>.
        /// </summary>
        public override long ProcessedEntities
        {
            get
            {
                long processedEntities = 0;

                // Calculate new total for all archive destined output adapters
                lock (this)
                {
                    foreach (IOutputAdapter item in this)
                    {
                        if (item.OutputIsForArchive)
                            processedEntities += item.ProcessedEntities;
                    }
                }

                return processedEntities;
            }
        }

        /// <summary>
        /// Returns a flag that determines if all time-series entities sent to this
        /// <see cref="OutputAdapterCollection"/> are destined for archival.
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
        /// Raises the <see cref="UnprocessedEntities"/> event.
        /// </summary>
        /// <param name="unprocessedEntities">Total entities in the queue that have not been processed.</param>
        protected virtual void OnUnprocessedEntities(int unprocessedEntities)
        {
            try
            {
                if ((object)UnprocessedEntities != null)
                    UnprocessedEntities(this, new EventArgs<int>(unprocessedEntities));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for UnprocessedEntities event: {0}", ex.Message), ex));
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
                // Wire up unprocessed entities event
                item.UnprocessedEntities += item_UnprocessedEntities;
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
                // Un-wire unprocessed entities event
                item.UnprocessedEntities -= item_UnprocessedEntities;
                base.DisposeItem(item);
            }
        }

        // Raise unprocessed entities event on behalf of each item in collection
        private void item_UnprocessedEntities(object sender, EventArgs<int> e)
        {
            if ((object)UnprocessedEntities != null)
                UnprocessedEntities(sender, e);
        }

        #endregion
    }
}