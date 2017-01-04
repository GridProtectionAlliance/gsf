//******************************************************************************************************
//  DeviceStatistics.cs - Gbtc
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
//  02/13/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.TimeSeries.Statistics
{
    class DeviceStatistics
    {
        /// <summary>
        /// Calculates number of data quality errors reported by device during last reporting interval.
        /// </summary>
        /// <param name="source">Source Device.</param>
        /// <param name="arguments">Any needed arguments for statistic calculation.</param>
        /// <returns>Data Quality Errors Statistic.</returns>
        private static double GetDeviceStatistic_DataQualityErrors(object source, string arguments)
        {
            double statistic = 0.0D;
            IDevice device = source as IDevice;

            if ((object)device != null)
                statistic = s_statisticValueCache.GetDifference(device, device.DataQualityErrors, nameof(device.DataQualityErrors));

            return statistic;
        }

        /// <summary>
        /// Calculates number of time quality errors reported by device during last reporting interval.
        /// </summary>
        /// <param name="source">Source Device.</param>
        /// <param name="arguments">Any needed arguments for statistic calculation.</param>
        /// <returns>Time Quality Errors Statistic.</returns>
        private static double GetDeviceStatistic_TimeQualityErrors(object source, string arguments)
        {
            double statistic = 0.0D;
            IDevice device = source as IDevice;

            if ((object)device != null)
                statistic = s_statisticValueCache.GetDifference(device, device.TimeQualityErrors, nameof(device.TimeQualityErrors));

            return statistic;
        }

        /// <summary>
        /// Calculates number of device errors reported by device during last reporting interval.
        /// </summary>
        /// <param name="source">Source Device.</param>
        /// <param name="arguments">Any needed arguments for statistic calculation.</param>
        /// <returns>Device Errors Statistic.</returns>
        private static double GetDeviceStatistic_DeviceErrors(object source, string arguments)
        {
            double statistic = 0.0D;
            IDevice device = source as IDevice;

            if ((object)device != null)
                statistic = s_statisticValueCache.GetDifference(device, device.DeviceErrors, nameof(device.DeviceErrors));

            return statistic;
        }

        /// <summary>
        /// Calculates number of measurements received from device during last reporting interval.
        /// </summary>
        /// <param name="source">Source Device.</param>
        /// <param name="arguments">Any needed arguments for statistic calculation.</param>
        /// <returns>Measurements Received Statistic.</returns>
        private static double GetDeviceStatistic_MeasurementsReceived(object source, string arguments)
        {
            double statistic = 0.0D;
            IDevice device = source as IDevice;

            if ((object)device != null)
                statistic = s_statisticValueCache.GetDifference(device, device.MeasurementsReceived, nameof(device.MeasurementsReceived));

            return statistic;
        }

        /// <summary>
        /// Calculates expected number of measurements received from device during last reporting interval.
        /// </summary>
        /// <param name="source">Source Device.</param>
        /// <param name="arguments">Any needed arguments for statistic calculation.</param>
        /// <returns>Measurements Expected Statistic.</returns>
        private static double GetDeviceStatistic_MeasurementsExpected(object source, string arguments)
        {
            double statistic = 0.0D;
            IDevice device = source as IDevice;

            if ((object)device != null)
                statistic = s_statisticValueCache.GetDifference(device, device.MeasurementsExpected, nameof(device.MeasurementsExpected));

            return statistic;
        }

        /// <summary>
        /// Calculates expected number of measurements received while device was reporting errors during last reporting interval.
        /// </summary>
        /// <param name="source">Source Device.</param>
        /// <param name="arguments">Any needed arguments for statistic calculation.</param>
        /// <returns>Measurements Expected Statistic.</returns>
        private static double GetDeviceStatistic_MeasurementsWithError(object source, string arguments)
        {
            double statistic = 0.0D;
            IDevice device = source as IDevice;

            if ((object)device != null)
                statistic = s_statisticValueCache.GetDifference(device, device.MeasurementsWithError, nameof(device.MeasurementsWithError));

            return statistic;
        }

        /// <summary>
        /// Calculates number of defined measurements from device during last reporting interval.
        /// </summary>
        /// <param name="source">Source Device.</param>
        /// <param name="arguments">Any needed arguments for statistic calculation.</param>
        /// <returns>Defined Measurements Statistic.</returns>
        private static double GetDeviceStatistic_MeasurementsDefined(object source, string arguments)
        {
            double statistic = 0.0D;
            IDevice device = source as IDevice;

            if ((object)device != null)
                statistic = device.MeasurementsDefined;

            return statistic;
        }

        private static readonly StatisticValueStateCache s_statisticValueCache = new StatisticValueStateCache();
    }
}
