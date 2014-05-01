//******************************************************************************************************
//  IServiceMonitor.cs - Gbtc
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
//  07/08/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GSF.Adapters;

namespace GSF.ServiceProcess
{
    /// <summary>
    /// Interface for an adapter that monitors the health of a service.
    /// </summary>
    public interface IServiceMonitor : IAdapter
    {
        /// <summary>
        /// Handles notifications from the service that occur
        /// on an interval to indicate that the service is
        /// still running.
        /// </summary>
        void HandleServiceHeartbeat();

        /// <summary>
        /// Handles messages received by the service
        /// whenever the service encounters an error.
        /// </summary>
        /// <param name="ex">The exception received from the service.</param>
        void HandleServiceError(Exception ex);

        /// <summary>
        /// Handles messages sent by a client.
        /// </summary>
        /// <param name="args">Arguments provided by the client.</param>
        void HandleClientMessage(string[] args);
    }
}
