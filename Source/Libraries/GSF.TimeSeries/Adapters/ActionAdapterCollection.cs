//******************************************************************************************************
//  ActionAdapterCollection.cs - Gbtc
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
        /// This event is raised every five seconds allowing consumer to track current number of unpublished seconds of data in the queue.
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

        /// <summary>
        /// Event is raised when temporal support is requested.
        /// </summary>
        public event EventHandler RequestTemporalSupport;

        // Fields

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

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag indicating if action adapter should respect auto-start requests based on input demands.
        /// </summary>
        /// <remarks>
        /// Action adapters are in the curious position of being able to both consume and produce points, as such the user needs to be able to control how their
        /// adapter will behave concerning routing demands when the adapter is setup to connect on demand. In the case of respecting auto-start input demands,
        /// as an example, this would be <c>false</c> for an action adapter that calculated measurement, but <c>true</c> for an action adapter used to archive inputs.
        /// </remarks>
        public virtual bool RespectInputDemands { get; set; }

        /// <summary>
        /// Gets or sets flag indicating if action adapter should respect auto-start requests based on output demands.
        /// </summary>
        /// <remarks>
        /// Action adapters are in the curious position of being able to both consume and produce points, as such the user needs to be able to control how their
        /// adapter will behave concerning routing demands when the adapter is setup to connect on demand. In the case of respecting auto-start output demands,
        /// as an example, this would be <c>true</c> for an action adapter that calculated measurement, but <c>false</c> for an action adapter used to archive inputs.
        /// </remarks>
        public virtual bool RespectOutputDemands { get; set; }

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
                status.AppendLine($"  Respecting Input Demands: {RespectInputDemands}");
                status.AppendLine($" Respecting Output Demands: {RespectOutputDemands}");
                status.AppendLine($"      Readonly Collections: {(ConvertReadonlyCollectionsToWritable ? "Converting to Writable" : "Unmodified")}");

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ActionAdapterCollection"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            RespectInputDemands = settings.TryGetValue(nameof(RespectInputDemands), out string setting) && setting.ParseBoolean();
            RespectOutputDemands = !settings.TryGetValue(nameof(RespectOutputDemands), out setting) || setting.ParseBoolean();
        }

        /// <summary>
        /// Queues a collection of measurements for processing to each <see cref="IActionAdapter"/> implementation in
        /// this <see cref="ActionAdapterCollection"/>.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            try
            {
                lock (this)
                {
                    foreach (IActionAdapter item in this)
                    {
                        // ReSharper disable once PossibleMultipleEnumeration
                        if (item.Enabled)
                            item.QueueMeasurementsForProcessing(measurements);
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException("Failed to queue measurements to action adapters: " + ex.Message, ex));
            }
        }

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
        /// Raises the <see cref="UnpublishedSamples"/> event.
        /// </summary>
        /// <param name="unpublishedSamples">Total number of unpublished seconds of data in the queue.</param>
        protected virtual void OnUnpublishedSamples(int unpublishedSamples)
        {
            try
            {
                UnpublishedSamples?.Invoke(this, new EventArgs<int>(unpublishedSamples));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for {nameof(UnpublishedSamples)} event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Raises the <see cref="DiscardingMeasurements"/> event.
        /// </summary>
        /// <param name="measurements">Enumeration of <see cref="IMeasurement"/> values being discarded.</param>
        protected virtual void OnDiscardingMeasurements(IEnumerable<IMeasurement> measurements)
        {
            try
            {
                DiscardingMeasurements?.Invoke(this, new EventArgs<IEnumerable<IMeasurement>>(measurements));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for {nameof(DiscardingMeasurements)} event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Raises <see cref="RequestTemporalSupport"/> event.
        /// </summary>
        protected virtual void OnRequestTemporalSupport()
        {
            try
            {
                RequestTemporalSupport?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for {nameof(RequestTemporalSupport)} event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IActionAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IActionAdapter"/> implementation.</param>
        protected override void InitializeItem(IActionAdapter item)
        {
            if (item is null)
                return;

            // Wire up events
            item.NewMeasurements += item_NewMeasurements;
            item.UnpublishedSamples += item_UnpublishedSamples;
            item.DiscardingMeasurements += item_DiscardingMeasurements;

            // Attach to collection-specific temporal support event
            if (item is ActionAdapterCollection collection)
                collection.RequestTemporalSupport += item_RequestTemporalSupport;

            base.InitializeItem(item);
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IActionAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IActionAdapter"/> to dispose.</param>
        protected override void DisposeItem(IActionAdapter item)
        {
            if (item is null)
                return;

            // Un-wire events
            item.NewMeasurements -= item_NewMeasurements;
            item.UnpublishedSamples -= item_UnpublishedSamples;
            item.DiscardingMeasurements -= item_DiscardingMeasurements;

            // Detach from collection-specific temporal support event
            if (item is ActionAdapterCollection collection)
                collection.RequestTemporalSupport -= item_RequestTemporalSupport;

            base.DisposeItem(item);
        }

        // Raise new measurements event on behalf of each item in collection
        private void item_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e) => 
            NewMeasurements?.Invoke(sender, e);

        // Raise unpublished samples event on behalf of each item in collection
        private void item_UnpublishedSamples(object sender, EventArgs<int> e) => 
            UnpublishedSamples?.Invoke(sender, e);

        // Raise discarding measurements event on behalf of each item in collection
        private void item_DiscardingMeasurements(object sender, EventArgs<IEnumerable<IMeasurement>> e) => 
            DiscardingMeasurements?.Invoke(sender, e);

        // Raise request temporal support event on behalf of each item in collection
        private void item_RequestTemporalSupport(object sender, EventArgs e) => 
            RequestTemporalSupport?.Invoke(sender, e);

        #endregion
    }
}