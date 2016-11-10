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
using System.Diagnostics;
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
        /// <summary>
        /// The routing method to use.
        /// </summary>
        public enum RoutingMethod
        {
            /// <summary>
            /// The default method of routing
            /// </summary>
            Default,

            /// <summary>
            /// A custom implementation that sacrifices overall latency for lower CPU utilization.
            /// </summary>
            HighLatencyLowCpu,

        }

        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(OptimizationOptions), MessageClass.Framework);

        /// <summary>
        /// Eliminates certain async queues in the phasor protocol parsing. 
        /// </summary>
        public static bool DisableAsyncQueueInProtocolParsing { get; private set; } = false;

        /// <summary>
        /// Uses dedicated threads instead of LongSynchronizedOperations in certain cases.
        /// </summary>
        public static bool PreferDedicatedThreads { get; private set; } = false;

        /// <summary>
        /// Specifies the desired routing method.
        /// </summary>
        public static RoutingMethod DefaultRoutingMethod { get; private set; } = RoutingMethod.Default;

        /// <summary>
        /// Specifies a routing latency if the routing method recgonizes this.
        /// </summary>
        public static int RoutingLatency { get; private set; } = 50;

        /// <summary>
        /// Specifies that threadpool monitoring will be enabled.
        /// </summary>
        public static bool EnableThreadPoolMonitoring { get; private set; } = false;

        /// <summary>
        /// Specifies that the threadpool monitor should also dump the stack trace of all thread.
        /// </summary>
        public static bool EnableThreadStackDumping { get; private set; } = false;

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

                LoadThreadPoolMonitoring(optimizations);
                LoadPreferDedicatedThreads(optimizations);
                LoadAsyncQueueInProtocolParsing(optimizations);
                LoadProcessorAffinity(optimizations);
                LoadRoutingTable(optimizations);
            }
            catch (Exception ex)
            {
                Log.Publish(MessageLevel.Warning, "Could not parse Optimization Settings", setting, null, ex);
            }
            ThreadPoolMonitor.Initialize();
        }

        private static void LoadThreadPoolMonitoring(Dictionary<string, string> optimizations)
        {
            if (optimizations.ContainsKey("EnableThreadPoolMonitoring"))
            {
                Log.Publish(MessageLevel.Info, "Enable Optimization", "EnableThreadPoolMonitoring");
                EnableThreadPoolMonitoring = true;
            }
            if (optimizations.ContainsKey("EnableThreadStackDumping"))
            {
                Log.Publish(MessageLevel.Info, "Enable Optimization", "EnableThreadStackDumping");
                EnableThreadStackDumping = true;
            }
        }

        private static void LoadAsyncQueueInProtocolParsing(Dictionary<string, string> optimizations)
        {
            if (optimizations.ContainsKey("DisableAsyncQueueInProtocolParsing"))
            {
                Log.Publish(MessageLevel.Info, "Enable Optimization", "DisableAsyncQueueInProtocolParsing");
                DisableAsyncQueueInProtocolParsing = true;
            }
        }

        private static void LoadPreferDedicatedThreads(Dictionary<string, string> optimizations)
        {
            if (optimizations.ContainsKey("PreferDedicatedThreads"))
            {
                Log.Publish(MessageLevel.Info, "Enable Optimization", "PreferDedicatedThreads");
                PreferDedicatedThreads = true;
            }
        }

        private static void LoadProcessorAffinity(Dictionary<string, string> optimizations)
        {
            if (optimizations.ContainsKey("ProcessorAffinity"))
            {
                ulong value;
                if (ulong.TryParse(optimizations["ProcessorAffinity"], out value))
                {
                    if (value > 0)
                    {
                        Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(long)value;
                        Log.Publish(MessageLevel.Info, "Enable Optimization", "Processor Affinity set to " + Process.GetCurrentProcess().ProcessorAffinity.ToInt64().ToString("X"));
                    }
                    else
                    {
                        Log.Publish(MessageLevel.Warning, "Parsing Error", "Processor Affinity cannot be zero");
                    }
                }
                else
                {
                    Log.Publish(MessageLevel.Warning, "Parsing Error", "Unrecognized option for ProcessAffinity: " + optimizations["ProcessorAffinity"]);
                }
            }
        }

        private static void LoadRoutingTable(Dictionary<string, string> optimizations)
        {
            if (optimizations.ContainsKey("RoutingMethod"))
            {
                string method = optimizations["RoutingMethod"];
                if (method.Equals("RouteMappingHighLatencyLowCpu", StringComparison.CurrentCultureIgnoreCase))
                {
                    int latency = -1;

                    if (optimizations.ContainsKey("RoutingLatencyMS"))
                    {
                        string latencyString = optimizations["RoutingLatencyMS"];
                        if (int.TryParse(latencyString, out latency))
                        {
                            if (latency < 1 || latency > 500)
                            {
                                Log.Publish(MessageLevel.Info, "Routing Table", "Invalid range of routing latency. Defaulting to 10 ms. (Range: 1ms to 500ms)", "Value: " + latencyString);
                                latency = 10;
                            }
                        }
                        else
                        {
                            Log.Publish(MessageLevel.Info, "Routing Table", "Could not parse latency value. Defaulting to 10 ms.", "Value: " + latencyString);
                            latency = 10;
                        }
                    }
                    else
                    {
                        latency = 10;
                    }

                    Log.Publish(MessageLevel.Info, "Routing Table", "Using RouteMappingHighLatencyLowCpu.", "Latency: " + latency.ToString());

                    DefaultRoutingMethod = RoutingMethod.HighLatencyLowCpu;
                    RoutingLatency = latency;
                }
                else if (method.Equals("Default", StringComparison.CurrentCultureIgnoreCase))
                {
                    Log.Publish(MessageLevel.Info, "Routing Table", "Using the default routing table.");
                }
                else
                {
                    Log.Publish(MessageLevel.Warning, "Routing Table", "Specified routing method is not recognized. The default will be used.", "Value: " + method);
                }
            }
            else
            {
                Log.Publish(MessageLevel.Info, "Routing Table", "Method not specified, using the default routing table.");
            }
        }
    }
}
