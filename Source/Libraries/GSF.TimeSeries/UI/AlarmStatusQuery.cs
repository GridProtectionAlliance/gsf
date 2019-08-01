//******************************************************************************************************
//  AlarmStatusQuery.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  05/16/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using GSF.Communication;
using GSF.Console;
using GSF.ServiceProcess;
using GSF.Threading;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents as object that will query current alarm state.
    /// </summary>
    public class AlarmStatusQuery : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="ResponseTimeout"/>.
        /// </summary>
        public const int DefaultResponseTimeout = 20000; // Default to waiting twenty seconds, could be lots of alarms

        // Events

        /// <summary>
        /// Provides collection containing raised alarms with the highest severity for each signal in the system.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of the current high-severity <see cref="Alarm"/> states.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<Alarm>>> RaisedAlarmStates;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private WindowsServiceClient m_serviceClient;
        private ICollection<Alarm> m_raisedAlarms;
        private readonly LongSynchronizedOperation m_alarmStateQueryOperation;
        private AutoResetEvent m_responseComplete;
        private bool m_connectedToService;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AlarmStatusQuery"/>.
        /// </summary>
        public AlarmStatusQuery()
        {
            // Attach to service connected events
            CommonFunctions.ServiceConnectionRefreshed += CommonFunctions_ServiceConnectionRefreshed;

            // Determine initial state of connectivity
            UpdateServiceConnectivity();

            m_alarmStateQueryOperation = new LongSynchronizedOperation(ExecuteAlarmStateQuery)
            {
                IsBackground = true
            };
            m_responseComplete = new AutoResetEvent(false);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AlarmStatusQuery"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AlarmStatusQuery()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets allowed timeout, in milliseconds, for a command request response.
        /// </summary>
        public int ResponseTimeout { get; set; } = DefaultResponseTimeout;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AlarmStatusQuery"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AlarmStatusQuery"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_serviceClient != null && (object)m_serviceClient.Helper != null)
                            m_serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;

                        m_serviceClient = null;

                        if ((object)m_responseComplete != null)
                        {
                            // Release any waiting threads before disposing wait handle
                            m_responseComplete.Set();
                            m_responseComplete.Dispose();
                        }

                        m_responseComplete = null;

                        // Detach from service connected events
                        CommonFunctions.ServiceConnectionRefreshed -= CommonFunctions_ServiceConnectionRefreshed;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        private void UpdateServiceConnectivity(bool refreshRaisedAlarms = false)
        {
            // Detach from original service response if refreshing service connection
            if ((object)m_serviceClient != null && (object)m_serviceClient.Helper != null)
                m_serviceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;

            // Get new service client
            m_serviceClient = CommonFunctions.GetWindowsServiceClient();
            ClientHelper clientHelper = ((object)m_serviceClient != null) ? m_serviceClient.Helper : null;
            IClient remotingClient = ((object)clientHelper != null) ? clientHelper.RemotingClient : null;

            if ((object)clientHelper != null)
                clientHelper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;

            if ((object)remotingClient == null || remotingClient.CurrentState != ClientState.Connected)
            {
                // Remoting client is not connected to service
                m_connectedToService = false;

                // Release any waiting threads if service has been disconnected
                if ((object)m_responseComplete != null)
                    m_responseComplete.Set();
            }
            else
            {
                // If remoting client is connected, refresh raised alarms if requested and attach to connection terminated event
                m_connectedToService = true;

                if (refreshRaisedAlarms)
                    RequestRaisedAlarmStates();

                remotingClient.ConnectionTerminated += RemotingClient_ConnectionTerminated;
            }
        }

        private void CommonFunctions_ServiceConnectionRefreshed(object sender, EventArgs eventArgs)
        {
            // Determine new state of connectivity - since we are reconnecting and this could be to a
            // new node an/or a restarted system, re-request raised alarm states
            UpdateServiceConnectivity(true);
        }

        private void RemotingClient_ConnectionTerminated(object sender, EventArgs eventArgs)
        {
            IClient remotingClient = sender as IClient;

            // Attempt to detach from the event that just occurred
            if ((object)remotingClient != null)
                remotingClient.ConnectionTerminated -= RemotingClient_ConnectionTerminated;

            // Determine new state of connectivity
            UpdateServiceConnectivity();
        }

        /// <summary>
        /// Requests the current state of highest severity alarms.
        /// </summary>
        public void RequestRaisedAlarmStates()
        {
            // Create single item operation queue so that
            // multiple requests will be handled one at a time
            m_alarmStateQueryOperation.RunOnceAsync();
        }

        // Execute alarm state query
        private void ExecuteAlarmStateQuery()
        {
            // Nothing to do if not connected to service
            if (!m_connectedToService)
                return;

            try
            {
                // Clear any existing raised alarms
                m_raisedAlarms = null;

                // Reset state of response complete event (in case a prior event set never completed)
                if ((object)m_responseComplete != null)
                    m_responseComplete.Reset();

                // Send command to alarm services to get highest severity alarms
                CommonFunctions.SendCommandToService("INVOKE ALARM!SERVICES GetRaisedAlarms");

                // Wait for command response allowing for processing time
                if ((object)m_responseComplete != null)
                {
                    if (!m_responseComplete.WaitOne(ResponseTimeout))
                        OnProcessException(new TimeoutException($"Timed-out after {(ResponseTimeout / 1000.0D):0.00} seconds waiting for service response."));
                }

                // If alarms were returned, expose them to consumer
                if ((object)m_raisedAlarms != null)
                    OnRaisedAlarmStates(m_raisedAlarms);
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Alarm state query error: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Raises <see cref="RaisedAlarmStates"/> event.
        /// </summary>
        /// <param name="raisedAlarms">Alarm states.</param>
        protected virtual void OnRaisedAlarmStates(ICollection<Alarm> raisedAlarms)
        {
            if ((object)RaisedAlarmStates != null)
                RaisedAlarmStates(this, new EventArgs<ICollection<Alarm>>(raisedAlarms));
        }

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Handles ReceivedServiceResponse event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            if ((object)e != null)
            {
                ServiceResponse response = e.Argument;

                if ((object)response != null)
                {
                    if (ClientHelper.TryParseActionableResponse(response, out string sourceCommand, out bool responseSuccess) && responseSuccess)
                    {
                        if (!string.IsNullOrWhiteSpace(sourceCommand) && String.Compare(sourceCommand.Trim(), "INVOKE", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            List<object> attachments = response.Attachments;

                            // A GetHighestSeverityAlarms INVOKE will have two attachments: an alarm array, item 0, and the original command arguments, item 1
                            if ((object)attachments != null && attachments.Count > 1)
                            {
                                Arguments arguments = attachments[1] as Arguments;

                                // Check the method that was invoked - the second argument after the adapter ID
                                if ((object)arguments != null && String.Compare(arguments["OrderedArg2"], "GetRaisedAlarms", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    m_raisedAlarms = attachments[0] as ICollection<Alarm>;

                                    // Release waiting thread once desired response has been received
                                    if ((object)m_responseComplete != null)
                                        m_responseComplete.Set();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
