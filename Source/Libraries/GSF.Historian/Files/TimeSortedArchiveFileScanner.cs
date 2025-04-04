﻿//******************************************************************************************************
//  TimeSortedDataPointScanner.cs - Gbtc
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
//  11/08/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  11/30/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//      Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.Historian.Files;

/// <summary>
/// Reads a series of data points from an <see cref="ArchiveFile"/> in sorted order.
/// </summary>
/// <param name="reverseQuery">True to read data points in reverse order (i.e., from end time to start time); false to read in normal order.</param>
public class TimeSortedArchiveFileScanner(bool reverseQuery) : IArchiveFileScanner
{
    #region [ Properties ]

    /// <summary>
    /// Gets or sets the file allocation table of
    /// the file that this scanner is reading from.
    /// </summary>
    public ArchiveFileAllocationTable FileAllocationTable { get; set; }

    /// <summary>
    /// Gets or sets the collection of
    /// point IDs to be scanned from the file.
    /// </summary>
    public IEnumerable<int> HistorianIDs { get; set; }

    /// <summary>
    /// Gets or sets the minimum value of the timestamps
    /// of the data points returned by the scanner.
    /// </summary>
    public TimeTag StartTime { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the timestamps
    /// of the data points returned by the scanner.
    /// </summary>
    public TimeTag EndTime { get; set; }

    /// <summary>
    /// Gets flag that determines if data will be queried in reverse order.
    /// </summary>
    public bool ReverseQuery => reverseQuery;

    /// <summary>
    /// Gets or sets the data point from which to resume
    /// the scan if it was interrupted by a rollover.
    /// </summary>
    public IDataPoint ResumeFrom { get; set; }

    /// <summary>
    /// Gets or sets the handler used to handle
    /// errors that occur while scanning the file.
    /// </summary>
    public EventHandler<EventArgs<Exception>> DataReadExceptionHandler { get; set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Reads all <see cref="IDataPoint"/>s in time sorted order for the specified historian IDs.
    /// </summary>
    /// <returns>Each <see cref="IDataPoint"/> for the specified historian IDs.</returns>
    public IEnumerable<IDataPoint> Read()
    {
        List<IEnumerator<IDataPoint>> enumerators = [];

        // Setup enumerators for scanners that have data
        foreach (DataPointScanner scanner in GetScanners())
        {
            IEnumerator<IDataPoint> enumerator = scanner.Read().GetEnumerator();

            // Add enumerator to the list if it has at least one value
            if (enumerator.MoveNext())
                enumerators.Add(enumerator);
            else
                enumerator.Dispose();
        }

        if (enumerators.Count <= 0)
            yield break;
        
        List<int> completed = [];

        // Start publishing data points in time-sorted order
        do
        {
            TimeTag publishTime = reverseQuery ? TimeTag.MinValue : TimeTag.MaxValue;

            // Find minimum publication time for current values
            IDataPoint dataPoint;

            foreach (IEnumerator<IDataPoint> enumerator in enumerators)
            {
                dataPoint = enumerator.Current;
                int result = dataPoint?.Time.CompareTo(publishTime) ?? 0;

                if (reverseQuery ? result > 0 : result < 0)
                    publishTime = dataPoint!.Time;
            }

            int index = 0;

            Func<TimeTag, TimeTag, bool> compareTime = reverseQuery ?
                (srcTime, dstTime) => srcTime.CompareTo(dstTime) >= 0 :
                (srcTime, dstTime) => srcTime.CompareTo(dstTime) <= 0;

            // Publish all values at the current time
            foreach (IEnumerator<IDataPoint> enumerator in enumerators)
            {
                bool enumerationComplete = false;
                dataPoint = enumerator.Current;

                if (compareTime(dataPoint?.Time, publishTime))
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
                        while (enumerator.Current?.Time.CompareTo(publishTime) <= 0)
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

            if (completed.Count == 0)
                continue;

            // Remove completed enumerators
            completed.Sort();

            for (int i = completed.Count - 1; i >= 0; i--)
            {
                enumerators[completed[i]].Dispose();
                enumerators.RemoveAt(completed[i]);
            }

            completed.Clear();
        }
        while (enumerators.Count > 0);
    }

    // Gets the list of data point scanners used to
    // scan the file for data belonging to this query.
    private List<DataPointScanner> GetScanners()
    {
        if (HistorianIDs is null || !HistorianIDs.Any())
            return [];

        List<DataPointScanner> dataPointScanners = [];

        TimeTag startTime = StartTime ?? TimeTag.MinValue;
        TimeTag endTime = EndTime ?? TimeTag.MaxValue;
        int resumeFromHistorianID = 0;
        bool includeStartTime = true;

        // Set up parameters needed to properly resume a query after rollover
        if (ResumeFrom is not null)
        {
            resumeFromHistorianID = ResumeFrom.HistorianID;
            startTime = ResumeFrom.Time ?? startTime;
            includeStartTime = false;
        }

        // Create data point scanners for each historian ID
        List<int> historianIDs = HistorianIDs.ToList();

        historianIDs.Sort();

        foreach (int historianID in historianIDs)
        {
            dataPointScanners.Add(new DataPointScanner(FileAllocationTable, historianID, startTime, endTime, includeStartTime, reverseQuery, DataReadExceptionHandler));

            if (historianID == resumeFromHistorianID)
                includeStartTime = true;
        }

        return dataPointScanners;
    }

    #endregion
}