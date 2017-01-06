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

using GSF.TimeSeries.Transport;

// ReSharper disable UnusedParameter.Local
namespace GSF.TimeSeries.Statistics
{
    internal static class GatewayStatistics
    {
        #region [ Subscriber Statistics ]

        private static double GetSubscriberStatistic_Connected(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.IsConnected ? 1.0D : 0.0D;

            return statistic;
        }

        private static double GetSubscriberStatistic_Authenticated(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.Authenticated ? 1.0D : 0.0D;

            return statistic;
        }

        private static double GetSubscriberStatistic_ProcessedMeasurements(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = s_statisticValueCache.GetDifference(subscriber, subscriber.ProcessedMeasurements, "ProcessedMeasurements");

            return statistic;
        }

        private static double GetSubscriberStatistic_TotalBytesReceived(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
            {
                statistic = s_statisticValueCache.GetDifference(subscriber, subscriber.TotalBytesReceived, "TotalBytesReceived");

                if (statistic < 0.0D)
                    statistic = subscriber.TotalBytesReceived;
            }

            return statistic;
        }

        private static double GetSubscriberStatistic_AuthorizedCount(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.GetAuthorizedSignalIDs().Length;

            return statistic;
        }

        private static double GetSubscriberStatistic_UnauthorizedCount(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.GetUnauthorizedSignalIDs().Length;

            return statistic;
        }

        private static double GetSubscriberStatistic_LifetimeMeasurements(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.LifetimeMeasurements;

            return statistic;
        }

        private static double GetSubscriberStatistic_MinimumMeasurementsPerSecond(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.MinimumMeasurementsPerSecond;

            return statistic;
        }

        private static double GetSubscriberStatistic_MaximumMeasurementsPerSecond(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.MaximumMeasurementsPerSecond;

            return statistic;
        }

        private static double GetSubscriberStatistic_AverageMeasurementsPerSecond(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.AverageMeasurementsPerSecond;

            return statistic;
        }

        private static double GetSubscriberStatistic_LifetimeBytesReceived(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.TotalBytesReceived;

            return statistic;
        }

        private static double GetSubscriberStatistic_LifetimeMinimumLatency(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.LifetimeMinimumLatency;

            return statistic;
        }

        private static double GetSubscriberStatistic_LifetimeMaximumLatency(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.LifetimeMaximumLatency;

            return statistic;
        }

        private static double GetSubscriberStatistic_LifetimeAverageLatency(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.LifetimeAverageLatency;

            return statistic;
        }

        private static double GetSubscriberStatistic_UpTime(object source, string arguments)
        {
            double statistic = 0.0D;
            DataSubscriber subscriber = source as DataSubscriber;

            if ((object)subscriber != null)
                statistic = subscriber.RunTime;

            return statistic;
        }

        #endregion

        #region [ Publisher Statistics ]

        private static double GetPublisherStatistic_Connected(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.IsConnected ? 1.0D : 0.0D;

            return statistic;
        }

        private static double GetPublisherStatistic_ConnectedClientCount(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.Count;

            return statistic;
        }

        private static double GetPublisherStatistic_ProcessedMeasurements(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = s_statisticValueCache.GetDifference(publisher, publisher.ProcessedMeasurements, "ProcessedMeasurements");

            return statistic;
        }

        private static double GetPublisherStatistic_TotalBytesSent(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
            {
                statistic = s_statisticValueCache.GetDifference(publisher, publisher.TotalBytesSent, "TotalBytesSent");

                if (statistic < 0.0D)
                    statistic = publisher.TotalBytesSent;
            }

            return statistic;
        }

        private static double GetPublisherStatistic_LifetimeMeasurements(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.LifetimeMeasurements;

            return statistic;
        }

        private static double GetPublisherStatistic_MinimumMeasurementsPerSecond(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.MinimumMeasurementsPerSecond;

            return statistic;
        }

        private static double GetPublisherStatistic_MaximumMeasurementsPerSecond(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.MaximumMeasurementsPerSecond;

            return statistic;
        }

        private static double GetPublisherStatistic_AverageMeasurementsPerSecond(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.AverageMeasurementsPerSecond;

            return statistic;
        }

        private static double GetPublisherStatistic_LifetimeBytesSent(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.TotalBytesSent;

            return statistic;
        }

        private static double GetPublisherStatistic_LifetimeMinimumLatency(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.LifetimeMinimumLatency;

            return statistic;
        }

        private static double GetPublisherStatistic_LifetimeMaximumLatency(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.LifetimeMaximumLatency;

            return statistic;
        }

        private static double GetPublisherStatistic_LifetimeAverageLatency(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.LifetimeAverageLatency;

            return statistic;
        }

        private static double GetPublisherStatistic_BufferBlockRetransmissions(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = s_statisticValueCache.GetDifference(publisher, publisher.BufferBlockRetransmissions, "BufferBlockRetransmissions");

            return statistic;
        }

        private static double GetPublisherStatistic_UpTime(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = publisher.RunTime;

            return statistic;
        }

        #endregion

        private static readonly StatisticValueStateCache s_statisticValueCache = new StatisticValueStateCache();
    }
}
