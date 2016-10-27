//******************************************************************************************************
//  OptimizationOptions.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  10/27/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using GSF.Configuration;
using GSF.Diagnostics;

namespace GSF
{
    /// <summary>
    /// This class will contain various optimizations that can be enabled in certain circumstances 
    /// through the SystemSettings. Since this framework is used in many settings, for stability
    /// reasons, tradeoffs are made. This gives the users opportunities to enable/disable certain
    /// optimizations if for some reason they cause adverse effects on their system.
    /// </summary>
    public static class OptimizationOptions
    {
        private readonly static LogPublisher Log = Logger.CreatePublisher(typeof(OptimizationOptions), MessageClass.Framework);

        public readonly static bool DisableAsyncQueueInProtocolParsing = false;

        static OptimizationOptions()
        {
            string setting = string.Empty;
            try
            {
                ConfigurationFile configFile = ConfigurationFile.Current;
                CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
                systemSettings.Add("OptimizationsConnectionString", "", "Specifies which optimizations to enable for the system.");
                setting = systemSettings["OptimizationsConnectionString"].ValueAsString("");
                Dictionary<string, string> optimizations = setting.ParseKeyValuePairs();

                if (optimizations.ContainsKey("DisableAsyncQueueInProtocolParsing"))
                {
                    Log.Publish(MessageLevel.Info, "Enable Optimization", "DisableAsyncQueueInProtocolParsing");
                    DisableAsyncQueueInProtocolParsing = true;
                }
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Warning, "Could not parse Optimization Settings", setting, null, ex);
            }

        }
    }
}
