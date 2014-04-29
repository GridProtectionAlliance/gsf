//******************************************************************************************************
//  ServiceMonitors.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  04/29/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Adapters;

namespace GSF.ServiceProcess
{
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
            foreach (IServiceMonitor serviceMonitor in Adapters)
            {
                if (serviceMonitor.Enabled)
                    serviceMonitor.HandleServiceHeartbeat();
            }
        }

        /// <summary>
        /// Handles messages received by the service
        /// whenever the service encounters an error.
        /// </summary>
        /// <param name="message">The error message received from the service.</param>
        public void HandleServiceErrorMessage(string message)
        {
            foreach (IServiceMonitor serviceMonitor in Adapters)
            {
                if (serviceMonitor.Enabled)
                    serviceMonitor.HandleServiceErrorMessage(message);
            }
        }

        /// <summary>
        /// Handles messages sent by a client.
        /// </summary>
        /// <param name="args">Arguments provided by the client.</param>
        public void HandleClientMessage(string[] args)
        {
            foreach (IServiceMonitor serviceMonitor in Adapters)
            {
                if (serviceMonitor.Enabled)
                    serviceMonitor.HandleClientMessage(args);
            }
        }
    }
}
