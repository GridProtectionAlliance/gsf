//******************************************************************************************************
//  TimeSortedValueScanner.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  01/07/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;

namespace PIAdapters
{
    /// <summary>
    /// Reads a series of record values from a <see cref="PIPointList"/> in time sorted order.
    /// </summary>
    public class TimeSortedValueScanner
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the source points that this scanner is reading from.
        /// </summary>
        public PIPointList Points
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum value of the timestamps of the data points returned by the scanner.
        /// </summary>
        public AFTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum value of the timestamps of the data points returned by the scanner.
        /// </summary>
        public AFTime EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the handler used to manage any exceptions that may occur while reading data.
        /// </summary>
        public Action<Exception> DataReadExceptionHandler
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all <see cref="AFValue"/> instances in time sorted order as a yielding enumerable.
        /// </summary>
        /// <param name="pageFactor">Defines a paging factor used to load more data into a page.</param>
        /// <returns>Each recorded <see cref="AFValue"/> in time-sorted order for the specified <see cref="Points"/> and time-range.</returns>
        public IEnumerable<AFValue> Read(int pageFactor = 1)
        {
            PIPagingConfiguration config = new PIPagingConfiguration(PIPageType.TagCount, Points.Count * pageFactor);
            List<IEnumerator<AFValue>> enumerators = new List<IEnumerator<AFValue>>();

            try
            {
                // Setup enumerators for each set of points that have data
                foreach (AFValues scanner in Points.RecordedValues(new AFTimeRange(StartTime, EndTime), AFBoundaryType.Inside, null, false, config))
                {
                    IEnumerator<AFValue> enumerator = scanner.GetEnumerator();

                    // Add enumerator to the list if it has at least one value
                    if (enumerator.MoveNext())
                        enumerators.Add(enumerator);
                }
            }
            catch (OperationCanceledException)
            {
                // Errors that occur during bulk calls get trapped here, actual error is stored on the PIPagingConfiguration object
                DataReadExceptionHandler(config.Error);
            }
            catch (Exception ex)
            {
                DataReadExceptionHandler(ex);
            }

            if (enumerators.Count == 0)
                yield break;

            List<int> completed = new List<int>();
            AFValue dataPoint;

            // Start publishing data points in time-sorted order
            do
            {
                AFTime publishTime = AFTime.MaxValue;

                // Find minimum publication time for current values
                foreach (IEnumerator<AFValue> enumerator in enumerators)
                {
                    dataPoint = enumerator.Current;

                    if (dataPoint.Timestamp < publishTime)
                        publishTime = dataPoint.Timestamp;
                }

                int index = 0;

                // Publish all values at the current time
                foreach (IEnumerator<AFValue> enumerator in enumerators)
                {
                    bool enumerationComplete = false;
                    dataPoint = enumerator.Current;

                    if (dataPoint.Timestamp <= publishTime)
                    {
                        // Attempt to advance to next data point, tracking completed enumerators
                        if (!enumerator.MoveNext())
                        {
                            enumerationComplete = true;
                            completed.Add(index);
                        }

                        yield return dataPoint;

                        // Make sure any point IDs with duplicated times directly follow
                        if (!enumerationComplete)
                        {
                            while (enumerator.Current.Timestamp <= publishTime)
                            {
                                yield return enumerator.Current;

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

                    for (int i = completed.Count - 1; i >= 0; i--)
                        enumerators.RemoveAt(completed[i]);

                    completed.Clear();
                }
            }
            while (enumerators.Count > 0);
        }

        #endregion
    }
}
