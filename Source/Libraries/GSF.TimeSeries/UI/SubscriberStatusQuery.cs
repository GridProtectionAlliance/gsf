//******************************************************************************************************
//  SubscriberStatusQuery.cs - Gbtc
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
//  03/21/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GSF.Console;
using GSF.ServiceProcess;
using GSF.Threading;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents an object that will query the external DataPublisher for real-time subscriber status.
    /// </summary>
    public class SubscriberStatusQuery : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="ResponseTimeout"/>.
        /// </summary>
        public const int DefaultResponseTimeout = 1000; // Defaulting to waiting for one second for each round-trip request

        // Events

        /// <summary>
        /// Provides a dictionary of current subscriber statuses.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a dictionary keyed off the <see cref="Guid"/> based subscriber IDs containing the current connected state and status information.
        /// </remarks>
        public event EventHandler<EventArgs<Dictionary<Guid, Tuple<bool, string>>>> SubscriberStatuses;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private WindowsServiceClient m_serviceClient;
        private readonly List<Tuple<Guid, bool, string>> m_subscriberStatuses;
        private LongSynchronizedOperation m_statusQueryOperation;
        private ISet<Guid> m_statusQueryIDs;
        private object m_statusQueryLock;
        private AutoResetEvent m_responseComplete;
        private int m_requests;
        private int m_responses;
        private int m_responseTimeout;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SubscriberStatusQuery"/>.
        /// </summary>
        public SubscriberStatusQuery()
        {
            m_serviceClient = CommonFunctions.GetWindowsServiceClient();
            m_serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;

            m_subscriberStatuses = new List<Tuple<Guid, bool, string>>();
            m_responseComplete = new AutoResetEvent(false);
            m_statusQueryOperation = new LongSynchronizedOperation(ExecuteStatusQuery) { IsBackground = true };
            m_statusQueryIDs = new HashSet<Guid>();
            m_statusQueryLock = new object();
            m_responseTimeout = DefaultResponseTimeout;

            m_statusQueryOperation.IsBackground = true;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="SubscriberStatusQuery"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~SubscriberStatusQuery()
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
        /// Releases all the resources used by the <see cref="SubscriberStatusQuery"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SubscriberStatusQuery"/> object and optionally releases the managed resources.
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
        /// Requests the status of the specified <paramref name="subscriberIDs"/>.
        /// </summary>
        /// <param name="subscriberIDs">Subscriber IDs to request status for.</param>
        public void RequestSubscriberStatus(IEnumerable<Guid> subscriberIDs)
        {
            if ((object)subscriberIDs != null)
            {
                lock (m_statusQueryLock)
                {
                    m_statusQueryIDs.UnionWith(subscriberIDs);

                    if (m_statusQueryIDs.Count > 0)
                        m_statusQueryOperation.RunOnceAsync();
                }
            }
        }

        // Execute status query
        private void ExecuteStatusQuery()
        {
            try
            {
                List<Guid> subscriberIDs;

                lock (m_statusQueryLock)
                {
                    subscriberIDs = m_statusQueryIDs.ToList();
                    m_statusQueryIDs.Clear();
                }

                // Clear existing subscriber statuses
                lock (m_subscriberStatuses)
                {
                    m_subscriberStatuses.Clear();
                }

                // Reset request and response counts to zero
                m_requests = 0;
                m_responses = 0;

                // Reset state of response complete event (in case a prior event set never completed)
                if ((object)m_responseComplete != null)
                    m_responseComplete.Reset();

                // Send service commands to external data publisher to determine subscriber states
                foreach (Guid subscriberID in subscriberIDs)
                {
                    // Send command for authorized signals for this device
                    CommonFunctions.SendCommandToService(string.Format("INVOKE EXTERNAL!DATAPUBLISHER GetSubscriberStatus {0}", subscriberID));
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

                    // Create a sorted list of the subscriberIDs to use as a filter in case we get old responses from other queries
                    List<Guid> sourceFilter = new List<Guid>(subscriberIDs);
                    sourceFilter.Sort();

                    Dictionary<Guid, Tuple<bool, string>> subscriberStatuses = null;

                    // Provide user with a dictionary of query results - if there are any
                    lock (m_subscriberStatuses)
                    {
                        if (m_subscriberStatuses.Count > 0)
                            subscriberStatuses = m_subscriberStatuses.Where(tuple => sourceFilter.BinarySearch(tuple.Item1) >= 0).ToDictionary(key => key.Item1, value => new Tuple<bool, string>(value.Item2, value.Item3));
                    }

                    if (subscriberStatuses != null && subscriberStatuses.Count > 0)
                        OnSubscriberStatuses(subscriberStatuses);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Subscriber status query error: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Raises <see cref="SubscriberStatuses"/> event.
        /// </summary>
        /// <param name="subscriberStatuses">Dictionary of subscriber statuses.</param>
        protected virtual void OnSubscriberStatuses(Dictionary<Guid, Tuple<bool, string>> subscriberStatuses)
        {
            if (SubscriberStatuses != null)
                SubscriberStatuses(this, new EventArgs<Dictionary<Guid, Tuple<bool, string>>>(subscriberStatuses));
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

                            // An INVOKE for GetSubscriberStatus will have two attachments: a tuple of the values, item 0, and the original command arguments, item 1
                            if ((object)attachments != null && attachments.Count > 1)
                            {
                                Arguments arguments = attachments[1] as Arguments;

                                // Check the method that was invoked - the second argument after the adapter ID
                                if ((object)arguments != null && string.Compare(arguments["OrderedArg2"].ToNonNullString(), "GetSubscriberStatus", true) == 0)
                                {
                                    Tuple<Guid, bool, string> subscriberStatus = attachments[0] as Tuple<Guid, bool, string>;

                                    if ((object)subscriberStatus != null)
                                    {
                                        lock (m_subscriberStatuses)
                                        {
                                            m_subscriberStatuses.Add(subscriberStatus);
                                        }
                                    }

                                    // A response for GetSubscriberStatus counts whether or not a status was returned
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
