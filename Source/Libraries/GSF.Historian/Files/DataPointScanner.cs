//******************************************************************************************************
//  DataPointScanner.cs - Gbtc
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
//  09/22/2014 - Stephen C. Wills
//       Extracted from the TimeSortedDataPointScanner (now the TimeSortedArchiveFileScanner).
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.Historian.Files
{
    /// <summary>
    /// Scans an archive file for a given signal's data points over a given time range.
    /// </summary>
    internal class DataPointScanner
    {
        #region [ Members ]

        // Fields
        private readonly List<ArchiveDataBlock> m_dataBlocks;
        private readonly TimeTag m_startTime;
        private readonly TimeTag m_endTime;
        private readonly int m_historianID;
        private readonly bool m_includeStartTime;
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
        /// <param name="includeStartTime">True to include data points equal to the start time; false to exclude.</param>
        /// <param name="dataReadExceptionHandler">Read exception handler.</param>
        public DataPointScanner(ArchiveFileAllocationTable dataBlockAllocationTable, int historianID, TimeTag startTime, TimeTag endTime, bool includeStartTime, EventHandler<EventArgs<Exception>> dataReadExceptionHandler)
        {
            // Find all data blocks for desired point over given time range
            m_dataBlocks = dataBlockAllocationTable.FindDataBlocks(historianID, startTime, endTime, false);
            m_startTime = startTime;
            m_endTime = endTime;
            m_historianID = historianID;
            m_includeStartTime = includeStartTime;
            m_dataReadExceptionHandler = dataReadExceptionHandler;
        }

        #endregion

        #region [ Properties]

        /// <summary>
        /// Gets the historian ID associated with this <see cref="DataPointScanner"/>.
        /// </summary>
        public int HistorianID
        {
            get
            {
                return m_historianID;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all <see cref="IDataPoint"/>s from the <see cref="ArchiveDataBlock"/>s.
        /// </summary>
        /// <returns>Each <see cref="IDataPoint"/> read from the <see cref="ArchiveDataBlock"/>s.</returns>
        public IEnumerable<IDataPoint> Read()
        {
            int count = m_dataBlocks.Count;
            int index = 0;

            Func<IDataPoint, bool> includeDataPoint;

            if (m_includeStartTime)
                includeDataPoint = dataPoint => (dataPoint.Time.CompareTo(m_startTime) >= 0 && dataPoint.Time.CompareTo(m_endTime) <= 0);
            else
                includeDataPoint = dataPoint => (dataPoint.Time.CompareTo(m_startTime) > 0 && dataPoint.Time.CompareTo(m_endTime) <= 0);

            // Loop through each data block
            foreach (ArchiveDataBlock dataBlock in m_dataBlocks)
            {
                // Attach to data read exception event for the data block
                dataBlock.DataReadException += m_dataReadExceptionHandler;

                // Pre-read all data points in this block
                List<IDataPoint> dataPoints = new List<IDataPoint>();

                if (index == 0 || index == count - 1)
                {
                    // Read data through first and last data blocks validating time range
                    foreach (IDataPoint dataPoint in dataBlock.Read())
                    {
                        if (includeDataPoint(dataPoint))
                            dataPoints.Add(dataPoint);
                    }
                }
                else
                {
                    // Read all of the data from the rest of the data blocks
                    foreach (IDataPoint dataPoint in dataBlock.Read())
                    {
                        dataPoints.Add(dataPoint);
                    }
                }

                // Detach from data read exception event for the data block
                dataBlock.DataReadException -= m_dataReadExceptionHandler;

                foreach (IDataPoint dataPoint in dataPoints)
                {
                    yield return dataPoint;
                }

                index++;
            }
        }

        #endregion
    }
}
