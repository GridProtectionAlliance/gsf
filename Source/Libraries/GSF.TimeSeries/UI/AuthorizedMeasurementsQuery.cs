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
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using GSF.Collections;
using GSF.Console;
using GSF.Data;
using GSF.ServiceProcess;
using GSF.Threading;

namespace GSF.TimeSeries.UI
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
        public const int DefaultResponseTimeout = 1000; // Defaulting to waiting for one second for each round-trip request

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
        private readonly List<Guid> m_authorizedSignalIDs;
        private LongSynchronizedOperation m_authorizationQueryOperation;
        private ISet<Guid> m_authorizationQueryIDs;
        private object m_authorizationQueryLock;
        private AutoResetEvent m_responseComplete;
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
            m_authorizationQueryOperation = new LongSynchronizedOperation(ExecuteAuthorizationQuery) { IsBackground = true };
            m_authorizationQueryIDs = new HashSet<Guid>();
            m_authorizationQueryLock = new object();
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
            if ((object)sourceMeasurements != null)
            {
                lock (m_authorizationQueryLock)
                {
                    m_authorizationQueryIDs.UnionWith(sourceMeasurements);

                    if (m_authorizationQueryIDs.Count > 0)
                        m_authorizationQueryOperation.RunOnceAsync();
                }
            }
        }

        // Execute authorization query
        private void ExecuteAuthorizationQuery()
        {
            try
            {
                List<Guid> sourceMeasurements;

                lock (m_authorizationQueryLock)
                {
                    sourceMeasurements = m_authorizationQueryIDs.ToList();
                    m_authorizationQueryIDs.Clear();
                }

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

                // Reset state of response complete event (in case a prior event set never completed)
                if ((object)m_responseComplete != null)
                    m_responseComplete.Reset();

                // Send service commands to DataSubscribers to determine signal authorizations
                foreach (int deviceID in deviceIDs)
                {
                    // Send command for authorized signals for this device
                    CommonFunctions.SendCommandToService(string.Format("INVOKE {0} GetAuthorizedSignalIDs", deviceID));
                    m_requests++;
                }

                if (m_requests > 0)
                {
                    // Wait for command responses allowing processing time for each
                    if ((object)m_responseComplete != null)
                    {
                        if (!m_responseComplete.WaitOne(m_requests * m_responseTimeout))
                            OnProcessException(new TimeoutException(string.Format("Timed-out after {0} seconds waiting for {1} service response{2}.", (m_requests * m_responseTimeout / 1000.0D).ToString("0.00"), m_requests, m_requests == 1 ? "" : "s")));
                    }

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
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Authorized measurements query error: " + ex.Message, ex));
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

                            // An INVOKE for GetAuthorizedSignalIDs will have two attachments: a guid array, item 0, and the original command arguments, item 1
                            if ((object)attachments != null && attachments.Count > 1)
                            {
                                Arguments arguments = attachments[1] as Arguments;

                                // Check the method that was invoked - the second argument after the adapter ID
                                if ((object)arguments != null && string.Compare(arguments["OrderedArg2"], "GetAuthorizedSignalIDs", true) == 0)
                                {
                                    Guid[] signalIDs = attachments[0] as Guid[];

                                    if ((object)signalIDs != null)
                                    {
                                        lock (m_authorizedSignalIDs)
                                        {
                                            m_authorizedSignalIDs.AddRange(signalIDs);
                                        }
                                    }

                                    // A response for GetAuthorizedSignalIDs counts whether or not a guid array was returned
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
