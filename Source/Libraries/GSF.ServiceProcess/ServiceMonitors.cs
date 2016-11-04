//******************************************************************************************************
//  ServiceMonitors.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  04/29/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.Adapters;

namespace GSF.ServiceProcess
{
    /// <summary>
    /// A special exception thrown by error handlers.
    /// </summary>
    [Serializable]
    public class ErrorHandlerException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ErrorHandlerException"/> class.
        /// </summary>
        public ErrorHandlerException()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ErrorHandlerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ErrorHandlerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ErrorHandlerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ErrorHandlerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandlerException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination. </param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
        /// <exception cref="SerializationException">The class name is null or <see cref="Exception.HResult" /> is zero (0). </exception>
        protected ErrorHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Adapter loader that loads implementations of <see cref="IServiceMonitor"/>
    /// and delegates messages to the enabled monitors.
    /// </summary>
    public class ServiceMonitors : AdapterLoader<IServiceMonitor>
    {
        /// <summary>
        /// Handles notifications from the service that occur
        /// on an interval to indicate that the service is
        /// still running.
        /// </summary>
        public void HandleServiceHeartbeat()
        {
            OperationQueue.Enqueue(new Action<IServiceMonitor>(serviceMonitor => serviceMonitor.HandleServiceHeartbeat()));
        }

        /// <summary>
        /// Handles messages received by the service
        /// whenever the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception received from the service.</param>
        public void HandleServiceError(Exception ex)
        {
            OperationQueue.Enqueue(new Action<IServiceMonitor>(serviceMonitor =>
            {
                try
                {
                    serviceMonitor.HandleServiceError(ex);
                }
                catch (Exception errorHandlerException)
                {
                    throw new ErrorHandlerException(errorHandlerException.Message, errorHandlerException);
                }
            }));
        }

        /// <summary>
        /// Handles messages sent by a client.
        /// </summary>
        /// <param name="args">Arguments provided by the client.</param>
        public void HandleClientMessage(string[] args)
        {
            OperationQueue.Enqueue(new Action<IServiceMonitor>(serviceMonitor => serviceMonitor.HandleClientMessage(args)));
        }

        /// <summary>
        /// Executes an operation on the <paramref name="adapter"/> with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="adapter">Adapter on which an operation is to be executed.</param>
        /// <param name="data">The operation to be executed.</param>
        protected override void ExecuteAdapterOperation(IServiceMonitor adapter, object data)
        {
            Action<IServiceMonitor> operation = data as Action<IServiceMonitor>;

            if ((object)operation != null)
                operation(adapter);
        }
    }
}
