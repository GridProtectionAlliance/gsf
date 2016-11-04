//******************************************************************************************************
//  ServiceMonitorBase.cs - Gbtc
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
//  07/08/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using GSF.Adapters;
using GSF.Configuration;

namespace GSF.ServiceProcess
{
    /// <summary>
    /// Base class for service monitors.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public abstract class ServiceMonitorBase : Adapter, IServiceMonitor
    {
        /// <summary>
        /// Handles notifications from the service that occur
        /// on an interval to indicate that the service is
        /// still running.
        /// </summary>
        public virtual void HandleServiceHeartbeat()
        {
        }

        /// <summary>
        /// Handles messages received by the service
        /// whenever the service encounters an error.
        /// </summary>
        /// <param name="ex">The error received from the service.</param>
        public virtual void HandleServiceError(Exception ex)
        {
        }

        /// <summary>
        /// Handles messages sent by a client.
        /// </summary>
        /// <param name="args">Arguments provided by the client.</param>
        public virtual void HandleClientMessage(string[] args)
        {
        }

        /// <summary>
        /// Loads saved <see cref="ServiceMonitorBase"/> settings from the config file if the <see cref="P:GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException"><see cref="P:GSF.Adapters.Adapter.SettingsCategory"/> has a value of null or empty string.</exception>
        public override void LoadSettings()
        {
            base.LoadSettings();

            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("Enabled", "False", "Determines if service monitor should be enabled at startup.");
                Enabled = settings["Enabled"].ValueAsBoolean(false);
            }
        }

        /// <summary>
        /// Saves <see cref="ServiceMonitorBase"/> settings to the config file if the <see cref="P:GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException"><see cref="P:GSF.Adapters.Adapter.SettingsCategory"/> has a value of null or empty string.</exception>
        public override void SaveSettings()
        {
            base.SaveSettings();

            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["Enabled", true].Update(Enabled);
                config.Save();
            }
        }
    }
}
