//******************************************************************************************************
//  DataPointScanner.cs - Gbtc
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
//  09/22/2014 - Stephen C. Wills
//       Extracted from the TimeSortedDataPointScanner (now the TimeSortedArchiveFileScanner).
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.Historian.Files;

/// <summary>
/// Scans an archive file for a given signal's data points over a given time range.
/// </summary>
internal class DataPointScanner
{
    #region [ Members ]

    // Fields
    private readonly ArchiveFileAllocationTable m_dataBlockAllocationTable;
    private readonly EventHandler<EventArgs<Exception>> m_dataReadExceptionHandler;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="DataPointScanner"/> instance.
    /// </summary>
    /// <param name="dataBlockAllocationTable"><see cref="ArchiveFileAllocationTable"/> for the file to be scanned.</param>
    /// <param name="historianID">Historian ID to scan for.</param>
    /// <param name="startTime">Desired start time.</param>
    /// <param name="endTime">Desired end time.</param>
    /// <param name="includeEdgeTimes">True to include data points equal to the start or end time; false to exclude.</param>
    /// <param name="dataReadExceptionHandler">Read exception handler.</param>
    public DataPointScanner(ArchiveFileAllocationTable dataBlockAllocationTable, int historianID, TimeTag startTime, TimeTag endTime, bool includeEdgeTimes, EventHandler<EventArgs<Exception>> dataReadExceptionHandler)
    {
        m_dataBlockAllocationTable = dataBlockAllocationTable;
        HistorianID = historianID;
        StartTime = startTime;
        EndTime = endTime;
        IncludeEdgeTimes = includeEdgeTimes;
        m_dataReadExceptionHandler = dataReadExceptionHandler;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the historian ID associated with this <see cref="DataPointScanner"/>.
    /// </summary>
    public int HistorianID { get; }

    /// <summary>
    /// Gets flag that indicates whether the start or end time is inclusive when reading data points from the <see cref="ArchiveDataBlock"/>s.
    /// </summary>
    public bool IncludeEdgeTimes { get; }

    /// <summary>
    /// Gets the start time associated with this <see cref="DataPointScanner"/>. The start time is inclusive if <see cref="IncludeEdgeTimes"/> is true.
    /// </summary>
    public TimeTag StartTime { get; }

    /// <summary>
    /// Gets the end time associated with this <see cref="DataPointScanner"/>. The end time is inclusive if <see cref="IncludeEdgeTimes"/> is true.
    /// </summary>
    public TimeTag EndTime { get; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Reads all <see cref="IDataPoint"/>s from the <see cref="ArchiveDataBlock"/>s.
    /// </summary>
    /// <returns>Each <see cref="IDataPoint"/> read from the <see cref="ArchiveDataBlock"/>s.</returns>
    public IEnumerable<IDataPoint> Read()
    {
        Func<IDataPoint, bool> includeDataPoint = IncludeEdgeTimes ? 
            dataPoint => dataPoint.Time.CompareTo(StartTime) >= 0 && dataPoint.Time.CompareTo(EndTime) <= 0 : 
            dataPoint => dataPoint.Time.CompareTo(StartTime) > 0 && dataPoint.Time.CompareTo(EndTime) <= 0;

        bool isFirst = true;

        // Loop through each data block
        foreach ((ArchiveDataBlock dataBlock, bool isLast) in m_dataBlockAllocationTable.FindDataBlocks(HistorianID, StartTime, EndTime, false))
        {
            try
            {
                // Attach to data read exception event for the data block
                dataBlock.DataReadException += m_dataReadExceptionHandler;

                if (isFirst || isLast)
                {
                    isFirst = false;

                    // Read data through first or last data blocks
                    foreach (IDataPoint dataPoint in dataBlock.Read())
                    {
                        // Validate time range on edges
                        if (includeDataPoint(dataPoint))
                            yield return dataPoint;
                    }
                }
                else
                {
                    // Read all the data from remainder of data blocks
                    foreach (IDataPoint dataPoint in dataBlock.Read())
                        yield return dataPoint;
                }
            }
            finally
            {
                // Detach from data read exception event for the data block
                dataBlock.DataReadException -= m_dataReadExceptionHandler;
            }
        }
    }

    #endregion
}