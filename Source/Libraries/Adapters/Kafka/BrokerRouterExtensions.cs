//******************************************************************************************************
//  BrokerRouterExtensions.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  10/27/2015 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Misakai.Kafka;

namespace KafkaAdapters
{
    /// <summary>
    /// Defines extensions methods for <see cref="BrokerRouter"/> instances.
    /// </summary>
    public static class BrokerRouterExtensions
    {
        /// <summary>
        /// Fetches a single result for the specified <paramref name="topic"/> and <paramref name="partitionID"/>.
        /// </summary>
        /// <param name="router">Router used to fetch result.</param>
        /// <param name="topic">Topic to fetch result from.</param>
        /// <param name="partitionID">Partition ID to fetch result from.</param>
        /// <param name="offset">Offset of <paramref name="topic"/> to begin fetching result.</param>
        /// <param name="maxBytes">Defines maximum size of request that can be adjusted for large serializations.</param>
        /// <returns>A <see cref="FetchResponse"/> instance, if available; otherwise, <c>null</c>.</returns>
        public static FetchResponse Fetch(this BrokerRouter router, string topic, int partitionID = 0, long offset = 0, int maxBytes = 32768)
        {
            if ((object)router == null)
                throw new ArgumentNullException(nameof(router));

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentNullException(nameof(topic));

            // Create a fetch request with a single item to request
            FetchRequest request = new FetchRequest()
            {
                Fetches = new List<Fetch>(new[]
                {
                    new Fetch
                    {
                        Topic = topic,
                        PartitionId = partitionID,
                        Offset = offset,
                        MaxBytes = maxBytes
                    }
                })
            };

            return router.SelectBrokerRoute(topic, partitionID).Connection.SendAsync(request).Result.FirstOrDefault();
        }

        /// <summary>
        /// Gets the latest offset for a Topic partition.
        /// </summary>
        /// <param name="router">Router used to derive latest offset.</param>
        /// <param name="topic">Topic to determine latest offset for.</param>
        /// <param name="partitionID">Topic partition to determine latest offset for.</param>
        /// <returns>The latest offset for a partition in a topic.</returns>
        public static long LatestOffset(this BrokerRouter router, string topic, int partitionID = 0)
        {
            long offset = 0;

            FetchResponse response = router.Fetch(topic, partitionID);

            if ((object)response != null)
                offset = response.HighWaterMark;

            return offset;
        }
    }
}
