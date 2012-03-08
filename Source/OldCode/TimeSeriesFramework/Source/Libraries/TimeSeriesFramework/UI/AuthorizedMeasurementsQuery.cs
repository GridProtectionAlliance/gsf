//******************************************************************************************************
//  AuthorizedMeasurementsQuery.cs - Gbtc
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
//  03/06/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using TVA;
using TVA.Collections;
using TVA.Data;
using TVA.ServiceProcess;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents an object that will query DataSubscriber instances to determine if measurements are authorized.
    /// </summary>
    public class AuthorizedMeasurementsQuery : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="ResponseTimeout"/>.
        /// </summary>
        public const int DefaultResponseTimeout = 250;

        // Events

        /// <summary>
        /// Provides full list of authorized measurement signal IDs.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is an array of the <see cref="Guid"/> based signal IDs of the authorized measurements.
        /// </remarks>
        public event EventHandler<EventArgs<Guid[]>> AuthorizedMeasurements;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private WindowsServiceClient m_serviceClient;
        private List<Guid> m_authorizedSignalIDs;
        private AutoResetEvent m_requestComplete;
        private AutoResetEvent m_responseComplete;
        private object m_queuedQueryPending;
        private int m_requests;
        private int m_responses;
        private int m_responseTimeout;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AuthorizedMeasurementsQuery"/>.
        /// </summary>
        public AuthorizedMeasurementsQuery()
        {
            m_serviceClient = CommonFunctions.GetWindowsServiceClient();
            m_serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;

            m_authorizedSignalIDs = new List<Guid>();
            m_responseComplete = new AutoResetEvent(false);
            m_requestComplete = new AutoResetEvent(true);
            m_queuedQueryPending = new object();
            m_responseTimeout = DefaultResponseTimeout;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AuthorizedMeasurementsQuery"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AuthorizedMeasurementsQuery()
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
        /// Releases all the resources used by the <see cref="AuthorizedMeasurementsQuery"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AuthorizedMeasurementsQuery"/> object and optionally releases the managed resources.
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
        /// Requests the authorization state of the specified <paramref name="sourceMeasurements"/>.
        /// </summary>
        /// <param name="sourceMeasurements">Measurement signal IDs to request authorization state for.</param>
        public void RequestAuthorizationStatus(IEnumerable<Guid> sourceMeasurements)
        {
            if ((object)sourceMeasurements != null && sourceMeasurements.Count() > 0)
            {
                // Create single item operation queue so that multiple requests will be handled one at a time
                ThreadPool.QueueUserWorkItem(QueueAuthorizationQuery, sourceMeasurements.Distinct());
            }
        }

        // Queue one authorization query at a time
        private void QueueAuthorizationQuery(object state)
        {
            // Queue up an authorization query unless another thread has already requested one
            if (Monitor.TryEnter(m_queuedQueryPending))
            {
                try
                {
                    // Queue new authorization query after waiting for any prior query request to complete
                    if (m_requestComplete.WaitOne())
                        ThreadPool.QueueUserWorkItem(ExecuteAuthorizationQuery, state);
                }
                finally
                {
                    Monitor.Exit(m_queuedQueryPending);
                }
            }
        }

        // Execute authorization query
        private void ExecuteAuthorizationQuery(object state)
        {
            try
            {
                IEnumerable<Guid> sourceMeasurements = state as IEnumerable<Guid>;

                if ((object)sourceMeasurements != null)
                {
                    List<int> deviceIDs = new List<int>();
                    AdoDataConnection database = null;

                    // Query associated device ID list for given measurements
                    try
                    {
                        database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                        string guidPrefix = database.DatabaseType == DatabaseType.Access ? "{" : "'";
                        string guidSuffix = database.DatabaseType == DatabaseType.Access ? "}" : "'";
                        string query = string.Format("SELECT DISTINCT DeviceID FROM ActiveMeasurement WHERE ProtocolType = 'Measurement' AND SignalID IN ({0})", sourceMeasurements.Select(signalID => guidPrefix + signalID.ToString() + guidSuffix).ToDelimitedString(", "));
                        DataTable measurementDevices = database.Connection.RetrieveData(database.AdapterType, query);

                        foreach (DataRow row in measurementDevices.Rows)
                        {
                            int? deviceID = row.ConvertNullableField<int>("DeviceID");

                            if (deviceID.HasValue)
                            {
                                // Validate that device ID is unique (not trusting all databases will handle DISTINCT properly)
                                if (deviceIDs.BinarySearch(deviceID.Value) < 0)
                                {
                                    deviceIDs.Add(deviceID.Value);
                                    deviceIDs.Sort();
                                }
                            }
                        }
                    }
                    finally
                    {
                        if ((object)database != null)
                            database.Dispose();
                    }

                    // Clear existing destination signal ID lists
                    lock (m_authorizedSignalIDs)
                    {
                        m_authorizedSignalIDs.Clear();
                    }

                    // Reset request and response counts to zero
                    m_requests = 0;
                    m_responses = 0;

                    // Send service commands to DataSubscribers to determine signal authorizations
                    foreach (int deviceID in deviceIDs)
                    {
                        // Send command for authorized signals for this device
                        m_serviceClient.Helper.SendRequest(string.Format("INVOKE {0} GetAuthorizedSignalIDs", deviceID));
                        m_requests++;
                    }

                    if (m_requests > 0)
                    {
                        // Wait for command responses allowing processing time for each
                        if ((object)m_responseComplete != null)
                            m_responseComplete.WaitOne(m_requests * m_responseTimeout);

                        // Create a sorted list of the source measurements to use as a filter to authorized measurements
                        List<Guid> sourceFilter = new List<Guid>(sourceMeasurements);
                        sourceFilter.Sort();

                        Guid[] authorizedSignalIDs = null;

                        // Provide user with a distinct list of query results - if there are any
                        lock (m_authorizedSignalIDs)
                        {
                            if (m_authorizedSignalIDs.Count > 0)
                                authorizedSignalIDs = m_authorizedSignalIDs.Distinct().Where(signalID => sourceFilter.BinarySearch(signalID) >= 0).ToArray();
                        }

                        if (authorizedSignalIDs != null && authorizedSignalIDs.Length > 0)
                            OnAuthorizedMeasurements(authorizedSignalIDs);
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Authorized measurements query error: " + ex.Message, ex));
            }
            finally
            {
                if ((object)m_requestComplete != null)
                    m_requestComplete.Set();
            }
        }

        /// <summary>
        /// Raises <see cref="AuthorizedMeasurements"/> event.
        /// </summary>
        /// <param name="authorizedMeasurements">Authorized measurements.</param>
        protected virtual void OnAuthorizedMeasurements(Guid[] authorizedMeasurements)
        {
            if (AuthorizedMeasurements != null)
                AuthorizedMeasurements(this, new EventArgs<Guid[]>(authorizedMeasurements));
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

                            if ((object)attachments != null && attachments.Count > 0)
                            {
                                Guid[] signalIDs = attachments[0] as Guid[];

                                if ((object)signalIDs != null)
                                {
                                    lock (m_authorizedSignalIDs)
                                    {
                                        m_authorizedSignalIDs.AddRange(signalIDs);
                                    }

                                    m_responses++;
                                }
                            }
                        }
                    }

                    // Release waiting thread once all responses have been received
                    if (m_responses >= m_requests && (object)m_responseComplete != null)
                        m_responseComplete.Set();
                }
            }
        }

        #endregion
    }
}
