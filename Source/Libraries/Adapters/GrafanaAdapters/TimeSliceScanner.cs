//******************************************************************************************************
//  TimeSliceScanner.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  05/12/2017 - J/. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GrafanaAdapters
{
    /// <summary>
    /// Reads series of <see cref="DataSourceValue"/> instances for the same time interval.
    /// </summary>
    public class TimeSliceScanner
    {
        #region [ Members ]

        // Fields
        private readonly List<IEnumerator<DataSourceValue>> m_enumerators;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TimeSliceScanner"/>.
        /// </summary>
        /// <param name="dataset">Source <see cref="DataSourceValue"/> series to scan.</param>
        /// <param name="tolerance">Time tolerance for data slices in Unix epoch milliseconds.</param>
        public TimeSliceScanner(IEnumerable<DataSourceValueGroup> dataset, double tolerance = 0.0D)
        {
            m_enumerators = new List<IEnumerator<DataSourceValue>>();
            Tolerance = tolerance;

            foreach (DataSourceValueGroup group in dataset)
            {
                IEnumerator<DataSourceValue> enumerator = group.Source.GetEnumerator();

                // Add enumerator to the list if it has at least one value
                if (enumerator.MoveNext())
                    m_enumerators.Add(enumerator);
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a flag that determines if data read has been completed.
        /// </summary>
        public bool DataReadComplete => m_enumerators.Count == 0;

        /// <summary>
        /// Gets time tolerance for data slices in Unix epoch milliseconds.
        /// </summary>
        public double Tolerance { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads next time slice from the series set.
        /// </summary>
        /// <param name="lastValue">
        /// Set to <c>true</c> to only publish last value per-series in time-slice for specified <see cref="Tolerance"/>;
        /// otherwise, <c>false</c> to publish all series values since last slice.
        /// </param>
        /// <returns>Next time slice.</returns>
        public IEnumerable<DataSourceValue> ReadNextTimeSlice(bool lastValue = true)
        {
            if (lastValue)
            {
                Dictionary<string, DataSourceValue> nextSlice = new Dictionary<string, DataSourceValue>(StringComparer.OrdinalIgnoreCase);
                ReadNextTimeSlice(value => nextSlice[value.Target] = value);
                return nextSlice.Values;
            }
            else
            {
                List<DataSourceValue> nextSlice = new List<DataSourceValue>();
                ReadNextTimeSlice(value => nextSlice.Add(value));
                return nextSlice;
            }
        }

        private void ReadNextTimeSlice(Action<DataSourceValue> addValue)
        {
            DataSourceValue dataPoint;
            double publishTime = double.MaxValue;

            // Find minimum publication time for current values
            foreach (IEnumerator<DataSourceValue> enumerator in m_enumerators)
            {
                dataPoint = enumerator.Current;

                if (dataPoint.Time < publishTime)
                    publishTime = dataPoint.Time;
            }

            publishTime += Tolerance;

            List<int> completed = new List<int>();
            int index = 0;

            // Publish all values at the current time
            foreach (IEnumerator<DataSourceValue> enumerator in m_enumerators)
            {
                bool enumerationComplete = false;
                dataPoint = enumerator.Current;

                if (dataPoint.Time <= publishTime)
                {
                    // Attempt to advance to next data point, tracking completed enumerators
                    if (!enumerator.MoveNext())
                    {
                        enumerationComplete = true;
                        completed.Add(index);
                    }

                    addValue(dataPoint);

                    // Make sure any point IDs with duplicated times directly follow
                    if (!enumerationComplete)
                    {
                        while (enumerator.Current.Time <= publishTime)
                        {
                            addValue(enumerator.Current);

                            if (!enumerator.MoveNext())
                            {
                                completed.Add(index);
                                break;
                            }
                        }
                    }
                }

                index++;
            }

            // Remove completed enumerators
            if (completed.Count > 0)
            {
                completed.Sort();

                // Remove highest numeric indexes first to retain source index integrity
                for (int i = completed.Count - 1; i >= 0; i--)
                    m_enumerators.RemoveAt(completed[i]);

                completed.Clear();
            }
        }

        #endregion
    }
}
