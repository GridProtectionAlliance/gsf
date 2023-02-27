//******************************************************************************************************
//  StatisticValueStateCache.cs - Gbtc
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
//  02/22/2012 - Stephen C. Wills
//       Migrated from GSF.PhasorProtocols.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;

namespace GSF.TimeSeries.Statistics
{
    /// <summary>
    /// Caches statistic values to provide the difference between the
    /// current and previous values of any statistic given that
    /// statistic's source, name, and current value.
    /// </summary>
    public class StatisticValueStateCache
    {
        #region [ Members ]

        // Fields
        private readonly Dictionary<object, Dictionary<string, StatisticValueState>> m_statisticValueStates;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="StatisticValueStateCache"/>.
        /// </summary>
        public StatisticValueStateCache() => 
            m_statisticValueStates = new Dictionary<object, Dictionary<string, StatisticValueState>>();

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the statistical difference between current and previous statistic value.
        /// </summary>
        /// <param name="source">Source Device.</param>
        /// <param name="statistic">Current statistic value.</param>
        /// <param name="name">Name of statistic calculation.</param>
        /// <returns>Difference from last cached statistic value.</returns>
        public double GetDifference(object source, double statistic, string name)
        {
            lock (m_statisticValueStates)
            {
                StatisticValueState valueState;

                if (m_statisticValueStates.TryGetValue(source, out Dictionary<string, StatisticValueState> valueStates))
                {
                    if (valueStates.TryGetValue(name, out valueState))
                    {
                        valueState.CurrentState = statistic;
                        statistic = valueState.GetDifference();
                    }
                    else
                    {
                        valueState = new StatisticValueState(name) { PreviousState = statistic };
                        valueStates.Add(name, valueState);
                    }
                }
                else
                {
                    valueStates = new Dictionary<string, StatisticValueState>();
                    valueState = new StatisticValueState(name) { PreviousState = statistic };
                    valueStates.Add(name, valueState);

                    m_statisticValueStates.Add(source, valueStates);

                    // Attach to Disposed event of source, if defined
                    EventInfo disposedEvent = source.GetType().GetEvent("Disposed");

                    disposedEvent?.GetAddMethod().Invoke(source, new object[] { new EventHandler(StatisticSourceDisposed) });
                }
            }

            return statistic;
        }

        // Remove value states cache when statistic source is disposed
        private void StatisticSourceDisposed(object sender, EventArgs e)
        {
            lock (m_statisticValueStates)
                m_statisticValueStates.Remove(sender);
        }

        #endregion
    }
}
