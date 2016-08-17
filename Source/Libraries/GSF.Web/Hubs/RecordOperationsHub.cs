//******************************************************************************************************
//  RecordOperationsHub.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/17/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading.Tasks;
using GSF.Web.Model;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Configuration;
using Microsoft.AspNet.SignalR.Hubs;

// ReSharper disable StaticMemberInGenericType
namespace GSF.Web.Hubs
{
    /// <summary>
    /// Defines a base class for implementing SignalR hubs that need record operations support.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IRecordOperationsHub"/>, typically inheritor.</typeparam>
    public abstract class RecordOperationsHub<T> : Hub, IRecordOperationsHub where T : class, IHub, IRecordOperationsHub
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private readonly Action<string, UpdateType> m_logStatusMessageFunction;
        private readonly Action<Exception> m_logExceptionFunction;
        private dynamic m_hubClient;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="RecordOperationsHub{T}"/>.
        /// </summary>
        protected RecordOperationsHub() :
            this(null, null, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="RecordOperationsHub{T}"/> with the specified logging functions.
        /// </summary>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        protected RecordOperationsHub(Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction) :
            this(null, logStatusMessageFunction, logExceptionFunction)
        {
        }

        /// <summary>
        /// Creates a new <see cref="RecordOperationsHub{T}"/> with the specified <see cref="DataContext"/> and logging functions.
        /// </summary>
        /// <param name="dataContext">Data context to use for this hub; set to <c>null</c> for default data context.</param>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        protected RecordOperationsHub(DataContext dataContext, Action<string, UpdateType> logStatusMessageFunction, Action<Exception> logExceptionFunction)
        {
            m_dataContext = dataContext ?? new DataContext(exceptionHandler: ex => LogException(ex));
            m_logStatusMessageFunction = logStatusMessageFunction;
            m_logExceptionFunction = logExceptionFunction;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="IRecordOperationsHub.RecordOperationsCache"/> for SignalR hub.
        /// </summary>
        public RecordOperationsCache RecordOperationsCache => s_recordOperationsCache;

        /// <summary>
        /// Gets reference to SignalR hub client instance.
        /// </summary>
        /// <remarks>
        /// This property can be used to call registered Javascript hub functions.
        /// </remarks>
        public dynamic HubClient => m_hubClient ?? (m_hubClient = Clients?.Client(Context?.ConnectionId));

        /// <summary>
        /// Gets <see cref="Model.DataContext"/> created for this <see cref="RecordOperationsHub{T}"/> instance.
        /// </summary>
        public DataContext DataContext => m_dataContext;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="RecordOperationsHub{T}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        m_dataContext?.Dispose();
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public override Task OnConnected()
        {
            s_connectCount++;

            return base.OnConnected();
        }

        /// <summary>
        /// Called when a connection disconnects from this hub gracefully or due to a timeout.
        /// </summary>
        /// <param name="stopCalled">
        /// <c>true</c> if stop was called on the client closing the connection gracefully;
        /// otherwise, <c>false</c>, if the connection has been lost for longer than the
        /// <see cref="IConfigurationManager.DisconnectTimeout" />. Timeouts can be caused
        /// by clients reconnecting to another SignalR server in scale-out.
        /// </param>
        /// <returns>A <see cref="Task" /></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
                s_connectCount--;

            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// Logs a status message to hub client and any provided log status message delegate.
        /// </summary>
        /// <param name="message">Message content to log.</param>
        /// <param name="type">Type of message to log.</param>
        /// <param name="logToClient">Determines if message should be logged to hub client.</param>
        /// <remarks>
        /// When <paramref name="logToClient"/> is <c>true</c>, function will
        /// call "sendInfoMessage" Javascript hub client function if defined.
        /// </remarks>
        protected void LogStatusMessage(string message, UpdateType type = UpdateType.Information, bool logToClient = true)
        {
            // Send status message to hub client
            if (logToClient)
                HubClient?.sendInfoMessage(message, type == UpdateType.Information ? 2000 : -1);

            // Send status message to program host
            m_logStatusMessageFunction?.Invoke(message, type);

        }

        /// <summary>
        /// Logs an exception to hub client and any provided log exception delegate.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        /// <param name="logToClient">Determines if message should be logged to hub client.</param>
        /// <remarks>
        /// When <paramref name="logToClient"/> is <c>true</c>, function will
        /// call "sendErrorMessage" Javascript hub client function if defined.
        /// </remarks>
        protected void LogException(Exception ex, bool logToClient = true)
        {
            // Send exception to hub client
            if (logToClient)
                HubClient?.sendErrorMessage(ex.Message, -1);

            // Send exception to program host
            m_logExceptionFunction?.Invoke(ex);
        }

        #endregion

        #region [ Operators ]

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly RecordOperationsCache<T> s_recordOperationsCache;
        private static volatile int s_connectCount;

        // Static Constructor
        static RecordOperationsHub()
        {
            s_recordOperationsCache = new RecordOperationsCache<T>();
        }

        // Static Properties

        /// <summary>
        /// Gets the hub connection count.
        /// </summary>
        public static int ConnectionCount => s_connectCount;

        // Static Methods

        /// <summary>
        /// Gets statically cached instance of <see cref="RecordOperationsCache"/> for <see cref="RecordOperationsHub{T}"/> instances.
        /// </summary>
        /// <returns>Statically cached instance of <see cref="RecordOperationsCache"/> for <see cref="RecordOperationsHub{T}"/> instances.</returns>
        public static RecordOperationsCache GetRecordOperationsCache() => s_recordOperationsCache;

        #endregion
    }
}
