//******************************************************************************************************
//  PublisherStatistics.cs - Gbtc
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
//  02/22/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.TimeSeries.Statistics;
using GSF.TimeSeries.Transport;

namespace GatewayProtocolAdapters
{
    public static class PublisherStatistics
    {

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

            //if ((object)publisher != null)
            //    statistic = publisher.ClientConnections.Count;

            return statistic;
        }

        private static double GetPublisherStatistic_ProcessedMeasurements(object source, string arguments)
        {
            double statistic = 0.0D;
            DataPublisher publisher = source as DataPublisher;

            if ((object)publisher != null)
                statistic = s_statisticValueCache.GetDifference(publisher, publisher.ProcessedEntities, "ProcessedMeasurements");

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

        private static readonly StatisticValueStateCache s_statisticValueCache = new StatisticValueStateCache();
    }
}
