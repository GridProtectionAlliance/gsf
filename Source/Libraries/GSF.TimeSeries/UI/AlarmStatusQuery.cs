//******************************************************************************************************
//  AlarmStatusQuery.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  05/16/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using GSF.Console;
using GSF.ServiceProcess;

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
        public const int DefaultResponseTimeout = 5000; // Defaulting to waiting for five seconds, could be lots of alarms

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
        private AutoResetEvent m_requestComplete;
        private AutoResetEvent m_responseComplete;
        private readonly object m_queuedQueryPending;
        private int m_responseTimeout;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AlarmStatusQuery"/>.
        /// </summary>
        public AlarmStatusQuery()
        {
            m_serviceClient = CommonFunctions.GetWindowsServiceClient();
            m_serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;

            m_responseComplete = new AutoResetEvent(false);
            m_requestComplete = new AutoResetEvent(true);
            m_queuedQueryPending = new object();
            m_responseTimeout = DefaultResponseTimeout;
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
        public int ResponseTimeout
        {
            get
            {
                return m_responseTimeout;
            }
            set
            {
                m_responseTimeout = value;
            }
        }

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

                        if ((object)m_requestComplete != null)
                        {
                            // Release any waiting threads before disposing wait handle
                            m_requestComplete.Set();
                            m_requestComplete.Dispose();
                        }

                        m_requestComplete = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Requests the current state of highest severity alarms.
        /// </summary>
        public void RequestRaisedAlarmStates()
        {
            // Create single item operation queue so that multiple requests will be handled one at a time
            ThreadPool.QueueUserWorkItem(QueueAlarmStateQuery);
        }

        // Queue one alarm state query at a time
        private void QueueAlarmStateQuery(object state)
        {
            // Queue up a query unless another thread has already requested one
            if (Monitor.TryEnter(m_queuedQueryPending))
            {
                try
                {
                    // Queue new query after waiting for any prior query request to complete
                    if (m_requestComplete.WaitOne())
                        ThreadPool.QueueUserWorkItem(ExecuteAlarmStateQuery, state);
                }
                finally
                {
                    Monitor.Exit(m_queuedQueryPending);
                }
            }
        }

        // Execute alarm state query
        private void ExecuteAlarmStateQuery(object state)
        {
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
                    if (!m_responseComplete.WaitOne(m_responseTimeout))
                        OnProcessException(new TimeoutException(string.Format("Timed-out after {0} seconds waiting for service response.", (m_responseTimeout / 1000.0D).ToString("0.00"))));
                }

                // If alarms were returned, expose them to consumer
                if ((object)m_raisedAlarms != null)
                    OnRaisedAlarmStates(m_raisedAlarms);
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Alarm state query error: " + ex.Message, ex));
            }
            finally
            {
                if ((object)m_requestComplete != null)
                    m_requestComplete.Set();
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
            if (ProcessException != null)
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
                    string sourceCommand;
                    bool responseSuccess;

                    if (ClientHelper.TryParseActionableResponse(response, out sourceCommand, out responseSuccess) && responseSuccess)
                    {
                        if (!string.IsNullOrWhiteSpace(sourceCommand) && string.Compare(sourceCommand.Trim(), "INVOKE", true) == 0)
                        {
                            List<object> attachments = response.Attachments;

                            // A GetHighestSeverityAlarms INVOKE will have two attachments: an alarm array, item 0, and the original command arguments, item 1
                            if ((object)attachments != null && attachments.Count > 1)
                            {
                                Arguments arguments = attachments[1] as Arguments;

                                // Check the method that was invoked - the second argument after the adapter ID
                                if ((object)arguments != null && string.Compare(arguments["OrderedArg2"], "GetRaisedAlarms", true) == 0)
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
