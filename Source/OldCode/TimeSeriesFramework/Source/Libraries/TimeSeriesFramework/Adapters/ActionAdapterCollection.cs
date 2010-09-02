//******************************************************************************************************
//  ActionAdapterCollection.cs - Gbtc
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
    /// Represents a collection of <see cref="IActionAdapter"/> implementations.
    /// </summary>
    public class ActionAdapterCollection : AdapterCollectionBase<IActionAdapter>, IActionAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// This event will be raised when there are new measurements available from the action adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        /// <summary>
        /// This event is raised every second allowing consumer to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnpublishedSamples;

        /// <summary>
        /// This event is raised if there are any measurements being discarded during the sorting process.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the enumeration of <see cref="IMeasurement"/> values that are being discarded during the sorting process.
        /// </remarks>
        public event EventHandler<EventArgs<IEnumerable<IMeasurement>>> DiscardingMeasurements;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="ActionAdapterCollection"/>.
        /// </summary>
        public ActionAdapterCollection()
        {
            base.Name = "Action Adapter Collection";
            base.DataMember = "ActionAdapters";
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Queues a collection of measurements for processing to each <see cref="IActionAdapter"/> implementation in
        /// this <see cref="ActionAdapterCollection"/>.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            foreach (IActionAdapter item in this)
            {
                item.QueueMeasurementsForProcessing(measurements);
            }
        }

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        /// <param name="measurements">New measurements.</param>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            if (NewMeasurements != null)
                NewMeasurements(this, new EventArgs<ICollection<IMeasurement>>(measurements));
        }

        /// <summary>
        /// Raises the <see cref="UnpublishedSamples"/> event.
        /// </summary>
        /// <param name="unpublishedSamples">Total number of unpublished seconds of data in the queue.</param>
        protected virtual void OnUnpublishedSamples(int unpublishedSamples)
        {
            if (UnpublishedSamples != null)
                UnpublishedSamples(this, new EventArgs<int>(unpublishedSamples));
        }

        /// <summary>
        /// Raises the <see cref="DiscardingMeasurements"/> event.
        /// </summary>
        /// <param name="measurements">Enumeration of <see cref="IMeasurement"/> values being discarded.</param>
        protected virtual void OnDiscardingMeasurements(IEnumerable<IMeasurement> measurements)
        {
            if (DiscardingMeasurements != null)
                DiscardingMeasurements(this, new EventArgs<IEnumerable<IMeasurement>>(measurements));
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IActionAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IActionAdapter"/> implementation.</param>
        protected override void InitializeItem(IActionAdapter item)
        {
            if (item != null)
            {
                // Wire up events
                item.NewMeasurements += NewMeasurements;
                item.UnpublishedSamples += UnpublishedSamples;
                item.DiscardingMeasurements += DiscardingMeasurements;
                base.InitializeItem(item);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IActionAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IActionAdapter"/> to dispose.</param>
        protected override void DisposeItem(IActionAdapter item)
        {
            if (item != null)
            {
                // Un-wire events
                item.NewMeasurements -= NewMeasurements;
                item.UnpublishedSamples -= UnpublishedSamples;
                item.DiscardingMeasurements -= DiscardingMeasurements;
                base.DisposeItem(item);
            }
        }

        #endregion
    }
}