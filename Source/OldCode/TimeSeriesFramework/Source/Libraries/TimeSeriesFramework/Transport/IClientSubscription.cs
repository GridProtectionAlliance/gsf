//******************************************************************************************************
//  IClientSubscription.cs - Gbtc
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
//  06/24/2011 - Ritchie
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using TimeSeriesFramework.Adapters;
using TVA;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Represents a common set of interfaces for a client adapter subscription to the <see cref="DataPublisher"/>.
    /// </summary>
    public interface IClientSubscription : IActionAdapter
    {
        /// <summary>
        /// Indicates to the host that processing for an input adapter (via temporal session) has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal procesing.
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
        /// Gets or sets flag that determines if the compact measurement format should be used in data packets of this <see cref="IClientSubscription"/>.
        /// </summary>
        bool UseCompactMeasurementFormat
        {
            get;
            set;
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
        /// Explictly raises the <see cref="IAdapter.StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        void OnStatusMessage(string status);

        /// <summary>
        /// Explictly raises the <see cref="IAdapter.ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        void OnProcessException(Exception ex);

        /// <summary>
        /// Explictly raises the <see cref="IInputAdapter.ProcessingComplete"/> event.
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
        // Define cache of dyanmically defined event handlers associated with each client subscription
        private static Dictionary<IClientSubscription, EventHandler<EventArgs<string, UpdateType>>> s_statusMessageHandlers = new Dictionary<IClientSubscription, EventHandler<EventArgs<string, UpdateType>>>();
        private static Dictionary<IClientSubscription, EventHandler<EventArgs<Exception>>> s_processExceptionHandlers = new Dictionary<IClientSubscription, EventHandler<EventArgs<Exception>>>();
        private static Dictionary<IClientSubscription, EventHandler> s_processingCompletedHandlers = new Dictionary<IClientSubscription, EventHandler>();

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
            // by removing this adapter from external routes. To accomplish this we expose I/O demands for an
            // undefined measurement. Note: assigning to null would mean "broadcast" of all data is desired.
            clientSubscription.InputMeasurementKeys = new MeasurementKey[] { MeasurementKey.Undefined };
            clientSubscription.OutputMeasurements = new Measurement[] { Measurement.Undefined };

            // Create a new Iaon session
            session = new IaonSession();
            session.Name = "<" + clientSubscription.HostName.ToNonNullString("unavailable") + ">@" + clientSubscription.StartTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss");

            // Setup default bubbling event handlers associated with the client session adapter
            EventHandler<EventArgs<string, UpdateType>> statusMessageHandler = (sender, e) =>
            {
                if (e.Argument2 == UpdateType.Information)
                    clientSubscription.OnStatusMessage(e.Argument1);
                else
                    clientSubscription.OnStatusMessage("0x" + (int)e.Argument2 + e.Argument1);
            };
            EventHandler<EventArgs<Exception>> processExceptionHandler = (sender, e) => clientSubscription.OnProcessException(e.Argument);
            EventHandler processingCompletedHandler = (sender, e) => clientSubscription.OnProcessingCompleted(sender, e);

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
                string.Format("Initializing temporal session for host \"{0}\" spanning {1} to {2} processing data {3}...",
                    clientSubscription.HostName.ToNonNullString("unknown"),
                    clientSubscription.StartTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    clientSubscription.StopTimeConstraint.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    clientSubscription.ProcessingInterval == 0 ? "as fast as possible" :
                    clientSubscription.ProcessingInterval == -1 ? "at the default rate" : "at " + clientSubscription.ProcessingInterval + "ms intervals"),
                UpdateType.Information));

            // Duplicate current run-time session configuration that has temporal support
            session.DataSource = IaonSession.ExtractTemporalConfiguration(clientSubscription.DataSource);

            // Initialize temporal session adapters without starting them
            session.Initialize(false);

            // Create in-situ action adapter for temporal Iaon session used to proxy data to the client subscription
            TemporalClientSubscriptionProxy proxy = new TemporalClientSubscriptionProxy(clientSubscription);

            // Assign critical adapter properties
            proxy.Name = "PROXY!SERVICES";
            proxy.ID = 0;
            proxy.InitializationTimeout = session.ActionAdapters.InitializationTimeout;
            proxy.DataSource = session.DataSource;
            proxy.ConnectionString = null;

            // Provide proxy with the original measurement keys requested by the remote subscriber, this will
            // establish the needed session level measurement key routing demands
            proxy.InputMeasurementKeys = inputMeasurementKeys;
            proxy.OutputMeasurements = outputMeasurements;

            // Add proxy to temporal session action adapters collection, this will auto-initialize the adapter
            session.ActionAdapters.Add(proxy);

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

            // Recalculate routing tables to accomodate addtion of proxy adapter
            // and possible changes due to assignment of temporal constraints
            session.RecalculateRoutingTables(inputMeasurementKeys);

            return session;
        }

        /// <summary>
        /// Disposes a temporal <see cref="IaonSession"/> created using <see cref="CreateTemporalSession"/>.
        /// </summary>
        /// <param name="adapter"><see cref="IClientSubscription"/> source instance.</param>
        /// <param name="session"><see cref="IaonSession"/> instance to dispose.</param>
        public static void DisposeTemporalSession(this IClientSubscription adapter, ref IaonSession session)
        {
            if (session != null)
            {
                EventHandler<EventArgs<string, UpdateType>> statusMessageFunction;
                EventHandler<EventArgs<Exception>> processExceptionFunction;
                EventHandler processingCompletedFunction;

                // Lookup event handlers, detach and remove
                if (s_statusMessageHandlers.TryGetValue(adapter, out statusMessageFunction))
                {
                    session.StatusMessage -= statusMessageFunction;
                    s_statusMessageHandlers.Remove(adapter);
                }

                if (s_processExceptionHandlers.TryGetValue(adapter, out processExceptionFunction))
                {
                    session.ProcessException -= processExceptionFunction;
                    s_processExceptionHandlers.Remove(adapter);
                }

                if (s_processingCompletedHandlers.TryGetValue(adapter, out processingCompletedFunction))
                {
                    session.ProcessingComplete -= processingCompletedFunction;
                    s_processingCompletedHandlers.Remove(adapter);
                }

                session.Dispose();
            }

            session = null;
        }
    }
}
