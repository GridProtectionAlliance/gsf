//******************************************************************************************************
//  IDataPointScanner.cs - Gbtc
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
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.Historian.Files
{
    /// <summary>
    /// Represents a scanner that reads data points from an archive file for a given time range.
    /// </summary>
    public interface IArchiveFileScanner
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the file allocation table of
        /// the file that this scanner is reading from.
        /// </summary>
        ArchiveFileAllocationTable FileAllocationTable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of
        /// point IDs to be scanned from the file.
        /// </summary>
        IEnumerable<int> HistorianIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum value of the timestamps
        /// of the data points returned by the scanner.
        /// </summary>
        TimeTag StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum value of the timestamps
        /// of the data points returned by the scanner.
        /// </summary>
        TimeTag EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data point from which to resume
        /// the scan if it was interrupted by a rollover.
        /// </summary>
        IDataPoint ResumeFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the handler used to handle
        /// errors that occur while scanning the file.
        /// </summary>
        EventHandler<EventArgs<Exception>> DataReadExceptionHandler
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all <see cref="IDataPoint"/>s for the specified historian IDs.
        /// </summary>
        /// <returns>Each <see cref="IDataPoint"/> for the specified historian IDs.</returns>
        IEnumerable<IDataPoint> Read();

        #endregion
    }
}
