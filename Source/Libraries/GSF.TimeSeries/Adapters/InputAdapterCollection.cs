//******************************************************************************************************
//  InputAdapterCollection.cs - Gbtc
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
using System.Text;
using GSF.Diagnostics;

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
        /// a finite amount of data, e.g., reading a historical range of data during temporal processing.
        /// </remarks>
        public event EventHandler ProcessingComplete;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="InputAdapterCollection"/>.
        /// </summary>
        public InputAdapterCollection()
        {
            base.Name = "Input Adapter Collection";
            base.DataMember = "InputAdapters";
            base.MonitorTimerEnabled = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if readonly collections should be converted
        /// to writable so published measurement sets can be augmented by filter adapters.
        /// </summary>
        /// <remarks>
        /// When filter adapters are detected in the <see cref="IaonSession"/>, collections that
        /// output new measurements should convert all readonly measurement collections into
        /// collections that can be modified so that measurements can be removed if needed.
        /// </remarks>
        public bool ConvertReadonlyCollectionsToWritable { get; set; }

        /// <summary>
        /// Gets the descriptive status of this <see cref="ActionAdapterCollection"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new();

                status.Append(base.Status);
                status.AppendLine($"      Readonly Collections: {(ConvertReadonlyCollectionsToWritable ? "Converting to Writable" : "Unmodified")}");

                return status.ToString();
            }
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
                if (ConvertReadonlyCollectionsToWritable && measurements.IsReadOnly)
                    NewMeasurements?.Invoke(this, new EventArgs<ICollection<IMeasurement>>(new List<IMeasurement>(measurements)));

                NewMeasurements?.Invoke(this, new EventArgs<ICollection<IMeasurement>>(measurements));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for {nameof(NewMeasurements)} event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Raises the <see cref="ProcessingComplete"/> event.
        /// </summary>
        protected virtual void OnProcessingComplete()
        {
            try
            {
                ProcessingComplete?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for {nameof(ProcessingComplete)} event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IInputAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IInputAdapter"/> implementation.</param>
        protected override void InitializeItem(IInputAdapter item)
        {
            if (item is null)
                return;

            // Wire up new measurement event
            item.NewMeasurements += item_NewMeasurements;
            item.ProcessingComplete += item_ProcessingComplete;
            base.InitializeItem(item);
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IInputAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IInputAdapter"/> to dispose.</param>
        protected override void DisposeItem(IInputAdapter item)
        {
            if (item is null)
                return;

            // Un-wire new measurements event
            item.NewMeasurements -= item_NewMeasurements;
            item.ProcessingComplete -= item_ProcessingComplete;
            base.DisposeItem(item);
        }

        // Raise new measurements event on behalf of each item in collection
        private void item_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e) => NewMeasurements?.Invoke(sender, e);

        // Raise processing complete event on behalf of each item in collection
        private void item_ProcessingComplete(object sender, EventArgs e) =>
            ProcessingComplete?.Invoke(sender, e);

        #endregion
    }
}