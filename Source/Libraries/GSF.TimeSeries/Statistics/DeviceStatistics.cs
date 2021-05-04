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

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
namespace GSF.TimeSeries.Statistics
{
    internal class DeviceStatistics
    {
        // Calculates number of data quality errors reported by device during last reporting interval.
        private static double GetDeviceStatistic_DataQualityErrors(object source, string _) => 
            source is IDevice device ? s_statisticValueCache.GetDifference(device, device.DataQualityErrors, nameof(device.DataQualityErrors)) : 0.0D;

        // Calculates number of time quality errors reported by device during last reporting interval.
        private static double GetDeviceStatistic_TimeQualityErrors(object source, string _) => 
            source is IDevice device ? s_statisticValueCache.GetDifference(device, device.TimeQualityErrors, nameof(device.TimeQualityErrors)) : 0.0D;

        // Calculates number of device errors reported by device during last reporting interval.
        private static double GetDeviceStatistic_DeviceErrors(object source, string _) => 
            source is IDevice device ? s_statisticValueCache.GetDifference(device, device.DeviceErrors, nameof(device.DeviceErrors)) : 0.0D;

        // Calculates number of measurements received from device during last reporting interval.
        private static double GetDeviceStatistic_MeasurementsReceived(object source, string _) => 
            source is IDevice device ? s_statisticValueCache.GetDifference(device, device.MeasurementsReceived, nameof(device.MeasurementsReceived)) : 0.0D;

        // Calculates expected number of measurements received from device during last reporting interval.
        private static double GetDeviceStatistic_MeasurementsExpected(object source, string _) => 
            source is IDevice device ? s_statisticValueCache.GetDifference(device, device.MeasurementsExpected, nameof(device.MeasurementsExpected)) : 0.0D;

        // Calculates expected number of measurements received while device was reporting errors during last reporting interval.
        private static double GetDeviceStatistic_MeasurementsWithError(object source, string _) => 
            source is IDevice device ? s_statisticValueCache.GetDifference(device, device.MeasurementsWithError, nameof(device.MeasurementsWithError)) : 0.0D;

        // Calculates number of defined measurements from device during last reporting interval.
        private static double GetDeviceStatistic_MeasurementsDefined(object source, string _) => 
            source is IDevice device ? device.MeasurementsDefined : 0.0D;

        // Calculates the difference between a device time and the average of all devices
        private static double GetDeviceStatistic_DeviceTimeDeviationFromAverage(object source, string _)
        {
            if (source is IDevice device)
            {
                long deviation = GlobalDeviceStatistics.GetDeviceTimeDeviationFromAverage(device);
                return deviation > long.MinValue ? new Ticks(deviation).ToSeconds() : double.NaN;
            }

            return double.NaN;
        }

        private static readonly StatisticValueStateCache s_statisticValueCache = new StatisticValueStateCache();
    }
}
