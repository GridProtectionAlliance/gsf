//******************************************************************************************************
//  SystemPerformanceMonitor.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
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
//  04/29/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents a system performance monitor for system level performance statistics, e.g.,
    /// CPU utilization and available memory.
    /// </summary>
    public class SystemPerformanceMonitor : PerformanceMonitorBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SystemPerformanceMonitor"/>.
        /// </summary>
        /// <param name="samplingInterval">
        /// Interval, in milliseconds, at which the <see cref="PerformanceMonitorBase.Counters"/>
        /// are to be sampled.
        /// </param>
        /// <remarks>
        /// It is recommended to use the <see cref="Default"/> instance of this class
        /// instead of creating a new instance where applicable.
        /// </remarks>
        public SystemPerformanceMonitor(double samplingInterval = DefaultSamplingInterval) 
            : base(samplingInterval)
        {
            AddCounter("Processor", "% Processor Time", "_Total", "CPU Utilization", "Average %", 1.0F);
            AddCounter("Memory", "Available MBytes", "", "Available Memory", "GB", 1024.0F);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the processor utilization of the system.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter CPUUsage =>
            FindCounter("% Processor Time");

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the remaining available memory of the system.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter AvailableMemory =>
            FindCounter("Available MBytes");

        #endregion

        #region [ Static ]

        // Static Fields
        private static SystemPerformanceMonitor s_systemPerformanceMonitor;

        // Static Properties

        /// <summary>
        /// Gets default reference of <see cref="SystemPerformanceMonitor"/>.
        /// </summary>
        public static SystemPerformanceMonitor Default =>
            s_systemPerformanceMonitor ??= new SystemPerformanceMonitor();

        #endregion
    }
}
