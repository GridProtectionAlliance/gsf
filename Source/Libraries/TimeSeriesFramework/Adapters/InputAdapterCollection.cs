//******************************************************************************************************
//  InputAdapterCollection.cs - Gbtc
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents a collection of <see cref="IInputAdapter"/> implementations.
    /// </summary>
    public class InputAdapterCollection : AdapterCollectionBase<IInputAdapter>, IInputAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// This event will be raised when there are new measurements available from the input data source.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        /// <summary>
        /// Indicates to the host that processing for one of the input adapters has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal procesing.
        /// </remarks>
        public event EventHandler ProcessingComplete;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="InputAdapterCollection"/>.
        /// </summary>
        public InputAdapterCollection()
            : this(null)
        {
            // When collection is spawned as an adapter, it needs a parameterless constructor
        }

        /// <summary>
        /// Creates a new <see cref="InputAdapterCollection"/>.
        /// </summary>
        /// <param name="waitHandles">Wait handle dictionary.</param>
        public InputAdapterCollection(ConcurrentDictionary<string, AutoResetEvent> waitHandles)
            : base(waitHandles)
        {
            base.Name = "Input Adapter Collection";
            base.DataMember = "InputAdapters";
            base.MonitorTimerEnabled = true;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        /// <param name="measurements">New measurements.</param>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            try
            {
                if (NewMeasurements != null)
                    NewMeasurements(this, new EventArgs<ICollection<IMeasurement>>(measurements));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for NewMeasurements event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ProcessingComplete"/> event.
        /// </summary>
        protected virtual void OnProcessingComplete()
        {
            try
            {
                if (ProcessingComplete != null)
                    ProcessingComplete(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ProcessingComplete event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IInputAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IInputAdapter"/> implementation.</param>
        protected override void InitializeItem(IInputAdapter item)
        {
            if (item != null)
            {
                // Wire up new measurement event
                item.NewMeasurements += item_NewMeasurements;
                item.ProcessingComplete += item_ProcessingComplete;
                base.InitializeItem(item);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IInputAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IInputAdapter"/> to dispose.</param>
        protected override void DisposeItem(IInputAdapter item)
        {
            if (item != null)
            {
                // Un-wire new meaurements event
                item.NewMeasurements -= item_NewMeasurements;
                item.ProcessingComplete -= item_ProcessingComplete;
                base.DisposeItem(item);
            }
        }

        // Raise new measurements event on behalf of each item in collection
        private void item_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (NewMeasurements != null)
                NewMeasurements(sender, e);
        }

        // Raise processing complete event on behalf of each item in collection
        private void item_ProcessingComplete(object sender, EventArgs e)
        {
            if (ProcessingComplete != null)
                ProcessingComplete(sender, e);
        }

        #endregion
    }
}