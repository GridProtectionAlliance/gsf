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
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.Historian.Files
{
    /// <summary>
    /// Scans an archive file for data points in a given time
    /// range and returns them in the order that they are scanned.
    /// </summary>
    public class ArchiveFileScanner : IArchiveFileScanner
    {
        #region [ Members ]

        // Fields
        private ArchiveFileAllocationTable m_fileAllocationTable;
        private IEnumerable<int> m_historianIDs;
        private TimeTag m_startTime;
        private TimeTag m_endTime;
        private IDataPoint m_resumeFrom;
        private EventHandler<EventArgs<Exception>> m_dataReadExceptionHandler;

        #endregion

        #region [ Constructors ]

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the file allocation table of
        /// the file that this scanner is reading from.
        /// </summary>
        public ArchiveFileAllocationTable FileAllocationTable
        {
            get
            {
                return m_fileAllocationTable;
            }
            set
            {
                m_fileAllocationTable = value;
            }
        }

        /// <summary>
        /// Gets or sets the collection of
        /// point IDs to be scanned from the file.
        /// </summary>
        public IEnumerable<int> HistorianIDs
        {
            get
            {
                return m_historianIDs;
            }
            set
            {
                m_historianIDs = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of the timestamps
        /// of the data points returned by the scanner.
        /// </summary>
        public TimeTag StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                m_startTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the timestamps
        /// of the data points returned by the scanner.
        /// </summary>
        public TimeTag EndTime
        {
            get
            {
                return m_endTime;
            }
            set
            {
                m_endTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the data point from which to resume
        /// the scan if it was interrupted by a rollover.
        /// </summary>
        public IDataPoint ResumeFrom
        {
            get
            {
                return m_resumeFrom;
            }
            set
            {
                m_resumeFrom = value;
            }
        }

        /// <summary>
        /// Gets or sets the handler used to handle
        /// errors that occur while scanning the file.
        /// </summary>
        public EventHandler<EventArgs<Exception>> DataReadExceptionHandler
        {
            get
            {
                return m_dataReadExceptionHandler;
            }
            set
            {
                m_dataReadExceptionHandler = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all <see cref="IDataPoint"/>s for the specified historian IDs.
        /// </summary>
        /// <returns>Each <see cref="IDataPoint"/> for the specified historian IDs.</returns>
        public IEnumerable<IDataPoint> Read()
        {
            foreach (DataPointScanner scanner in GetScanners())
            {
                foreach (IDataPoint dataPoint in scanner.Read())
                    yield return dataPoint;
            }
        }

        // Gets the list of data point scanners used to
        // scan the file for data belonging to this query.
        private List<DataPointScanner> GetScanners()
        {
            List<DataPointScanner> dataPointScanners = new List<DataPointScanner>();
            bool alreadyScanned = ((object)m_resumeFrom != null);

            foreach (int historianID in m_historianIDs)
            {
                if (!alreadyScanned)
                {
                    dataPointScanners.Add(new DataPointScanner(m_fileAllocationTable, historianID, m_startTime, m_endTime, true, m_dataReadExceptionHandler));
                }
                else if (historianID == m_resumeFrom.HistorianID)
                {
                    dataPointScanners.Add(new DataPointScanner(m_fileAllocationTable, historianID, m_resumeFrom.Time, m_endTime, false, m_dataReadExceptionHandler));
                    alreadyScanned = false;
                }
            }

            return dataPointScanners;
        }

        #endregion
    }
}
