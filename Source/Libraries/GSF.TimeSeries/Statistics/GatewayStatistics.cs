//******************************************************************************************************
//  GatewayStatistics.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  03/09/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Reflection;
using GSF.TimeSeries.Transport;
using GSF.Units;

// ReSharper disable UnusedParameter.Local
namespace GSF.TimeSeries.Statistics
{
    internal static class GatewayStatistics
    {
        private static T GetPropertyValue<T>(object source, string name)
        {
            PropertyInfo property = source.GetType().GetProperty(name);
            return property != null ? (T)property.GetValue(source) : default(T);
        }

        private static T GetFunctionValue<T>(object source, string name, params object[] parameters)
        {
            MethodInfo method = source.GetType().GetMethod(name);
            return method != null ? (T)method.Invoke(source, parameters) : default(T);
        }

        #region [ Subscriber Statistics ]

        private static double GetSubscriberStatistic_Connected(object source, string arguments)
        {
            return (source is DataSubscriber subscriber ? subscriber.IsConnected : GetPropertyValue<bool>(source, "IsConnected")) ? 1.0D : 0.0D;
        }

        private static double GetSubscriberStatistic_Authenticated(object source, string arguments)
        {
            return (source is DataSubscriber subscriber ? subscriber.Authenticated : GetPropertyValue<bool>(source, "Authenticated")) ? 1.0D : 0.0D;
        }

        private static double GetSubscriberStatistic_ProcessedMeasurements(object source, string arguments)
        {
            double statistic = source is DataSubscriber subscriber ? subscriber.ProcessedMeasurements : GetPropertyValue<long>(source, "ProcessedMeasurements");
            return s_statisticValueCache.GetDifference(source, statistic, "ProcessedMeasurements");
        }

        private static double GetSubscriberStatistic_TotalBytesReceived(object source, string arguments)
        {
            double statistic = source is DataSubscriber subscriber ? subscriber.TotalBytesReceived : GetPropertyValue<long>(source, "TotalBytesReceived");
            double difference = s_statisticValueCache.GetDifference(source, statistic, "TotalBytesReceived");
            return difference < 0.0D ? statistic : difference;
        }

        private static double GetSubscriberStatistic_AuthorizedCount(object source, string arguments)
        {
            Guid[] authorizedSignalIDs = source is DataSubscriber subscriber ? subscriber.GetAuthorizedSignalIDs() : GetFunctionValue<Guid[]>(source, "GetAuthorizedSignalIDs");
            return authorizedSignalIDs?.Length ?? 0;
        }

        private static double GetSubscriberStatistic_UnauthorizedCount(object source, string arguments)
        {
            Guid[] unauthorizedSignalIDs = source is DataSubscriber subscriber ? subscriber.GetUnauthorizedSignalIDs() : GetFunctionValue<Guid[]>(source, "GetUnauthorizedSignalIDs");
            return unauthorizedSignalIDs?.Length ?? 0;
        }

        private static double GetSubscriberStatistic_LifetimeMeasurements(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.LifetimeMeasurements : GetPropertyValue<long>(source, "LifetimeMeasurements");
        }

        private static double GetSubscriberStatistic_MinimumMeasurementsPerSecond(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.MinimumMeasurementsPerSecond : GetPropertyValue<long>(source, "MinimumMeasurementsPerSecond");
        }

        private static double GetSubscriberStatistic_MaximumMeasurementsPerSecond(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.MaximumMeasurementsPerSecond : GetPropertyValue<long>(source, "MaximumMeasurementsPerSecond");
        }

        private static double GetSubscriberStatistic_AverageMeasurementsPerSecond(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.AverageMeasurementsPerSecond : GetPropertyValue<long>(source, "AverageMeasurementsPerSecond");
        }

        private static double GetSubscriberStatistic_LifetimeBytesReceived(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.TotalBytesReceived : GetPropertyValue<long>(source, "TotalBytesReceived");
        }

        private static double GetSubscriberStatistic_LifetimeMinimumLatency(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.LifetimeMinimumLatency : GetPropertyValue<int>(source, "LifetimeMinimumLatency");
        }

        private static double GetSubscriberStatistic_LifetimeMaximumLatency(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.LifetimeMaximumLatency : GetPropertyValue<int>(source, "LifetimeMaximumLatency");
        }

        private static double GetSubscriberStatistic_LifetimeAverageLatency(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.LifetimeAverageLatency : GetPropertyValue<int>(source, "LifetimeAverageLatency");
        }

        private static double GetSubscriberStatistic_UpTime(object source, string arguments)
        {
            return source is DataSubscriber subscriber ? subscriber.RunTime : GetPropertyValue<Time>(source, "RunTime");
        }

        #endregion

        #region [ Publisher Statistics ]

        private static double GetPublisherStatistic_Connected(object source, string arguments)
        {
            return (source is DataPublisher publisher ? publisher.IsConnected : GetPropertyValue<bool>(source, "IsConnected")) ? 1.0D : 0.0D;
        }

        private static double GetPublisherStatistic_ConnectedClientCount(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.Count : GetPropertyValue<int>(source, "Count");
        }

        private static double GetPublisherStatistic_ProcessedMeasurements(object source, string arguments)
        {
            double statistic = source is DataPublisher publisher ? publisher.ProcessedMeasurements : GetPropertyValue<long>(source, "ProcessedMeasurements");
            return s_statisticValueCache.GetDifference(source, statistic, "ProcessedMeasurements");
        }

        private static double GetPublisherStatistic_TotalBytesSent(object source, string arguments)
        {
            double statistic = source is DataPublisher publisher ? publisher.TotalBytesSent : GetPropertyValue<long>(source, "TotalBytesSent");        
            double difference = s_statisticValueCache.GetDifference(source, statistic, "TotalBytesSent");
            return difference < 0.0D ? statistic : difference;
        }

        private static double GetPublisherStatistic_LifetimeMeasurements(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.LifetimeMeasurements : GetPropertyValue<long>(source, "LifetimeMeasurements");
        }

        private static double GetPublisherStatistic_MinimumMeasurementsPerSecond(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.MinimumMeasurementsPerSecond : GetPropertyValue<long>(source, "MinimumMeasurementsPerSecond");
        }

        private static double GetPublisherStatistic_MaximumMeasurementsPerSecond(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.MaximumMeasurementsPerSecond : GetPropertyValue<long>(source, "MaximumMeasurementsPerSecond");
        }

        private static double GetPublisherStatistic_AverageMeasurementsPerSecond(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.AverageMeasurementsPerSecond : GetPropertyValue<long>(source, "AverageMeasurementsPerSecond");
        }

        private static double GetPublisherStatistic_LifetimeBytesSent(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.TotalBytesSent : GetPropertyValue<long>(source, "TotalBytesSent");
        }

        private static double GetPublisherStatistic_LifetimeMinimumLatency(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.LifetimeMinimumLatency : GetPropertyValue<int>(source, "LifetimeMinimumLatency");
        }

        private static double GetPublisherStatistic_LifetimeMaximumLatency(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.LifetimeMaximumLatency : GetPropertyValue<int>(source, "LifetimeMaximumLatency");
        }

        private static double GetPublisherStatistic_LifetimeAverageLatency(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.LifetimeAverageLatency : GetPropertyValue<int>(source, "LifetimeAverageLatency");
        }

        private static double GetPublisherStatistic_BufferBlockRetransmissions(object source, string arguments)
        {
            double statistic = source is DataPublisher publisher ? publisher.BufferBlockRetransmissions : GetPropertyValue<long>(source, "BufferBlockRetransmissions");
            return s_statisticValueCache.GetDifference(source, statistic, "BufferBlockRetransmissions");
        }

        private static double GetPublisherStatistic_UpTime(object source, string arguments)
        {
            return source is DataPublisher publisher ? publisher.RunTime : GetPropertyValue<Time>(source, "RunTime");
        }

        private static double GetPublisherStatistic_TLSSecuredChannel(object source, string arguments)
        {
            return string.Equals(source is DataPublisher publisher ? publisher.SecurityMode.ToString() : GetPropertyValue<object>(source, "SecurityMode").ToString(), "TLS", StringComparison.OrdinalIgnoreCase) ? 1.0D : 0.0D;
        }

        #endregion

        private static readonly StatisticValueStateCache s_statisticValueCache = new StatisticValueStateCache();
    }
}
