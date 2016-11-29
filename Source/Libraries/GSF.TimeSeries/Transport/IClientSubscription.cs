//******************************************************************************************************
//  IClientSubscription.cs - Gbtc
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
//  06/24/2011 - Ritchie
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GSF.Diagnostics;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents a common set of interfaces for a client adapter subscription to the <see cref="DataPublisher"/>.
    /// </summary>
    public interface IClientSubscription : IActionAdapter
    {
        /// <summary>
        /// Indicates that a buffer block needed to be retransmitted because
        /// it was previously sent, but no confirmation was received.
        /// </summary>
        event EventHandler BufferBlockRetransmission;

        /// <summary>
        /// Indicates to the host that processing for an input adapter (via temporal session) has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal processing.
        /// </remarks>
        event EventHandler<EventArgs<IClientSubscription, EventArgs>> ProcessingComplete;

        /// <summary>
        /// Gets the <see cref="Guid"/> client TCP connection identifier of this <see cref="IClientSubscription"/>.
        /// </summary>
        Guid ClientID
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> based subscriber ID of this <see cref="IClientSubscription"/>.
        /// </summary>
        Guid SubscriberID
        {
            get;
        }

        /// <summary>
        /// Gets the current signal index cache of this <see cref="IClientSubscription"/>.
        /// </summary>
        SignalIndexCache SignalIndexCache
        {
            get;
        }

        /// <summary>
        /// Gets the input filter requested by the subscriber when establishing this <see cref="IClientSubscription"/>.
        /// </summary>
        string RequestedInputFilter
        {
            get;
        }

        /// <summary>
        /// Gets or sets flag that determines if payload compression should be enabled in data packets of this <see cref="IClientSubscription"/>.
        /// </summary>
        bool UsePayloadCompression
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the compression strength value to use when <see cref="UsePayloadCompression"/> is <c>true</c> for this <see cref="IClientSubscription"/>.
        /// </summary>
        int CompressionStrength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if the compact measurement format should be used in data packets of this <see cref="IClientSubscription"/>.
        /// </summary>
        bool UseCompactMeasurementFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Gets size of timestamp in bytes.
        /// </summary>
        int TimestampSize
        {
            get;
        }

        /// <summary>
        /// Gets or sets host name used to identify connection source of client subscription.
        /// </summary>
        string HostName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the status of the active temporal session, if any.
        /// </summary>
        string TemporalSessionStatus
        {
            get;
        }
        /// <summary>
        /// Gets or sets the measurement reporting interval.
        /// </summary>
        int MeasurementReportingInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Handles the confirmation message received from the
        /// subscriber to indicate that a buffer block was received.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the buffer block.</param>
        /// <returns>A list of buffer block sequence numbers for blocks that need to be retransmitted.</returns>
        void ConfirmBufferBlock(uint sequenceNumber);

        /// <summary>
        /// Explicitly raises the <see cref="IAdapter.StatusMessage"/> event.
        /// </summary>
        /// <param name="level">The <see cref="MessageLevel"/> to assign to this message</param>
        /// <param name="status">New status message.</param>
        /// <param name="eventName">A fixed string to classify this event; defaults to <see cref="AdapterBase.DefaultEventName"/>.</param>
        /// <param name="flags"><see cref="MessageFlags"/> to use, if any; defaults to <see cref="MessageFlags.None"/>.</param>
        void OnStatusMessage(MessageLevel level, string status, string eventName = null, MessageFlags flags = MessageFlags.None);

        /// <summary>
        /// Explicitly raises the <see cref="IAdapter.ProcessException"/> event.
        /// </summary>
        /// <param name="level">The <see cref="MessageLevel"/> to assign to this message</param>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        /// <param name="eventName">A fixed string to classify this event; defaults to <see cref="AdapterBase.DefaultEventName"/>.</param>
        /// <param name="flags"><see cref="MessageFlags"/> to use, if any; defaults to <see cref="MessageFlags.None"/>.</param>
        void OnProcessException(MessageLevel level, Exception ex, string eventName = null, MessageFlags flags = MessageFlags.None);

        /// <summary>
        /// Explicitly raises the <see cref="IInputAdapter.ProcessingComplete"/> event.
        /// </summary>
        /// <param name="sender"><see cref="IInputAdapter"/> raising the notification.</param>
        /// <param name="e">Event arguments for notification, if any.</param>
        void OnProcessingCompleted(object sender, EventArgs e);
    }

    /// <summary>
    /// Defines static extension functions for <see cref="IClientSubscription"/> implementations.
    /// </summary>
    public static class IClientSubscriptionExtensions
    {
        // Define cache of dynamically defined event handlers associated with each client subscription
        private static readonly ConcurrentDictionary<IClientSubscription, EventHandler<EventArgs<string, UpdateType>>> s_statusMessageHandlers = new ConcurrentDictionary<IClientSubscription, EventHandler<EventArgs<string, UpdateType>>>();
        private static readonly ConcurrentDictionary<IClientSubscription, EventHandler<EventArgs<Exception>>> s_processExceptionHandlers = new ConcurrentDictionary<IClientSubscription, EventHandler<EventArgs<Exception>>>();
        private static readonly ConcurrentDictionary<IClientSubscription, EventHandler> s_processingCompletedHandlers = new ConcurrentDictionary<IClientSubscription, EventHandler>();

        /// <summary>
        /// Returns a new temporal <see cref="IaonSession"/> for a <see cref="IClientSubscription"/>.
        /// </summary>
        /// <param name="clientSubscription"><see cref="IClientSubscription"/> instance to create temporal <see cref="IaonSession"/> for.</param>
        /// <returns>New temporal <see cref="IaonSession"/> for a <see cref="IClientSubscription"/>.</returns>
        public static IaonSession CreateTemporalSession(this IClientSubscription clientSubscription)
        {
            IaonSession session;

            // Cache the specified input measurement keys requested by the remote subscription
            // internally since these will only be needed in the private Iaon session
            MeasurementKey[] inputMeasurementKeys = clientSubscription.InputMeasurementKeys;
            IMeasurement[] outputMeasurements = clientSubscription.OutputMeasurements;

            // Since historical data is requested, we "turn off" interaction with the outside real-time world
            // by removing the client subscription adapter from external routes. To accomplish this we expose
            // I/O demands for an undefined measurement as assigning to null would mean "broadcast" is desired.
            clientSubscription.InputMeasurementKeys = new[] { MeasurementKey.Undefined };
            clientSubscription.OutputMeasurements = new IMeasurement[] { Measurement.Undefined };

            // Create a new Iaon session
            session = new IaonSession();
            session.Name = "<" + clientSubscription.HostName.ToNonNullString("unavailable") + ">@" + clientSubscription.StartTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss");

            // Assign requested input measurement keys as a routing restriction
            session.InputMeasurementKeysRestriction = inputMeasurementKeys;

            // Setup default bubbling event handlers associated with the client session adapter
            EventHandler<EventArgs<string, UpdateType>> statusMessageHandler = (sender, e) =>
            {
                if (e.Argument2 == UpdateType.Information)
                    clientSubscription.OnStatusMessage(MessageLevel.Info, e.Argument1);
                else
                    clientSubscription.OnStatusMessage(MessageLevel.Warning, "0x" + (int)e.Argument2 + e.Argument1);
            };

            EventHandler<EventArgs<Exception>> processExceptionHandler = (sender, e) => clientSubscription.OnProcessException(MessageLevel.Warning, e.Argument);
            EventHandler processingCompletedHandler = clientSubscription.OnProcessingCompleted;

            // Cache dynamic event handlers so they can be detached later
            s_statusMessageHandlers[clientSubscription] = statusMessageHandler;
            s_processExceptionHandlers[clientSubscription] = processExceptionHandler;
            s_processingCompletedHandlers[clientSubscription] = processingCompletedHandler;

            // Attach handlers to new session - this will proxy all temporal session messages through the client session adapter
            session.StatusMessage += statusMessageHandler;
            session.ProcessException += processExceptionHandler;
            session.ProcessingComplete += processingCompletedHandler;

            // Send the first message indicating a new temporal session is being established
            statusMessageHandler(null, new EventArgs<string, UpdateType>(
                // ReSharper disable once UseStringInterpolation
                string.Format("Initializing temporal session for host \"{0}\" spanning {1} to {2} processing data {3}...",
                    clientSubscription.HostName.ToNonNullString("unknown"),
                    clientSubscription.StartTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    clientSubscription.StopTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    clientSubscription.ProcessingInterval == 0 ? "as fast as possible" :
                    clientSubscription.ProcessingInterval == -1 ? "at the default rate" : "at " + clientSubscription.ProcessingInterval + "ms intervals"),
                UpdateType.Information));

            // Duplicate current real-time session configuration for adapters that report temporal support
            session.DataSource = IaonSession.ExtractTemporalConfiguration(clientSubscription.DataSource);

            // Initialize temporal session adapters without starting them
            session.Initialize(false);

            // Define an in-situ action adapter for the temporal Iaon session used to proxy data back to the client subscription. Note
            // to enable adapters that are connect-on-demand in the temporal session, we must make sure the proxy adapter is setup to
            // respect input demands. The proxy adapter produces no points into the temporal session - all received points are simply
            // internally proxied out to the parent client subscription outside the purview of the Iaon session; from the perspective
            // of the Iaon session, points seem to dead-end in the proxy adapter. The proxy adapter is an action adapter and action
            // adapters typically produce measurements, as such, actions default to respecting output demands not input demands. Using
            // the default settings of not respecting input demands and the proxy adapter not producing any points, the Iaon session
            // would ignore the adapter's input needs. In this case we want Iaon session to recognize the inputs of the proxy adapter
            // as important to the connect-on-demand dependency chain, so we request respect for the input demands.
            TemporalClientSubscriptionProxy proxyAdapter = new TemporalClientSubscriptionProxy
            {
                // Assign critical adapter properties
                ID = 0,
                Name = "PROXY!SERVICES",
                ConnectionString = "",
                DataSource = session.DataSource,
                RespectInputDemands = true,
                InputMeasurementKeys = inputMeasurementKeys,
                OutputMeasurements = outputMeasurements,
                Parent = clientSubscription,
                Initialized = true
            };

            // Add new proxy adapter to temporal session action adapter collection - this will start adapter
            session.ActionAdapters.Add(proxyAdapter);

            // Load current temporal constraint parameters
            Dictionary<string, string> settings = clientSubscription.Settings;
            string startTime, stopTime, parameters;

            settings.TryGetValue("startTimeConstraint", out startTime);
            settings.TryGetValue("stopTimeConstraint", out stopTime);
            settings.TryGetValue("timeConstraintParameters", out parameters);

            // Assign requested temporal constraints to all private session adapters
            session.AllAdapters.SetTemporalConstraint(startTime, stopTime, parameters);
            session.AllAdapters.ProcessingInterval = clientSubscription.ProcessingInterval;

            // Start temporal session adapters
            session.AllAdapters.Start();

            // Recalculate routing tables to accommodate addition of proxy adapter and handle
            // input measurement keys restriction
            session.RecalculateRoutingTables();

            return session;
        }

        /// <summary>
        /// Disposes a temporal <see cref="IaonSession"/> created using <see cref="CreateTemporalSession"/>.
        /// </summary>
        /// <param name="adapter"><see cref="IClientSubscription"/> source instance.</param>
        /// <param name="session"><see cref="IaonSession"/> instance to dispose.</param>
        public static void DisposeTemporalSession(this IClientSubscription adapter, ref IaonSession session)
        {
            if ((object)session != null)
            {
                EventHandler<EventArgs<string, UpdateType>> statusMessageFunction;
                EventHandler<EventArgs<Exception>> processExceptionFunction;
                EventHandler processingCompletedFunction;

                // Remove and detach from event handlers
                if (s_statusMessageHandlers.TryRemove(adapter, out statusMessageFunction))
                    session.StatusMessage -= statusMessageFunction;

                if (s_processExceptionHandlers.TryRemove(adapter, out processExceptionFunction))
                    session.ProcessException -= processExceptionFunction;

                if (s_processingCompletedHandlers.TryRemove(adapter, out processingCompletedFunction))
                    session.ProcessingComplete -= processingCompletedFunction;

                session.Dispose();
            }

            session = null;
        }
    }
}
