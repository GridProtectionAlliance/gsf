//******************************************************************************************************
//  StatisticsReader.cs - Gbtc
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
//  12/14/2010 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using GSF.Historian;
using GSF.Historian.Files;

namespace StatHistorianReportGenerator
{
    /// <summary>
    /// Reads certain device and input stream statistics from the statistics archive.
    /// The statistics that are read are the total frame count, the missing frame count,
    /// connectivity, average latency, the actual data rate, data quality errors, and
    /// time quality errors.
    /// </summary>
    public class StatisticsReader : IDisposable
    {
        #region [ Members ]

        // Fields
        private ArchiveReader m_archiveReader;
        private List<MetadataRecord> m_metadataRecords;

        private string m_archiveFilePath;
        private DateTime m_startTime;
        private DateTime m_endTime;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Metadata records scanned from the metadata file.
        /// </summary>
        public IReadOnlyCollection<MetadataRecord> MetadataRecords
        {
            get
            {
                return m_metadataRecords.AsReadOnly();
            }
        }

        /// <summary>
        /// Path to the archive file (*_archive.d).
        /// </summary>
        public string ArchiveFilePath
        {
            get
            {
                return m_archiveFilePath;
            }
            set
            {
                m_archiveFilePath = value;
            }
        }

        /// <summary>
        /// The timestamp at which to start gathering statistics from the archive.
        /// </summary>
        public DateTime StartTime
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
        /// The timestamp at which to stop gathering statistics from the archive.
        /// </summary>
        public DateTime EndTime
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Causes the <see cref="StatisticsReader"/> to open the archive file and retrieve the statistics.
        /// </summary>
        public void Open()
        {
            m_archiveReader = new ArchiveReader();
            m_archiveReader.Open(ArchiveFilePath);

            m_metadataRecords = m_archiveReader.MetadataFile.Read()
                .Where(record => !string.IsNullOrEmpty(record.Name))
                .ToList();
        }

        /// <summary>
        /// Reads data for a specified statistic.
        /// </summary>
        /// <param name="statisticSuffix">The suffix that defines the category of the statistic.</param>
        /// <param name="signalIndex">The index that defines which statistic in the given category is to be retrieved.</param>
        /// <returns>A lookup table containing a collection of data from the historian for each metadata record found with the given statistic defined.</returns>
        /// <remarks>
        /// All parameters--<see cref="ArchiveFilePath"/>, <see cref="StartTime"/>, and <see cref="EndTime"/>--must
        /// be set manually before attempting to read statistics from the archive.
        /// </remarks>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> Read(string statisticSuffix, int signalIndex)
        {
            string signalReferenceEnding = $"!{statisticSuffix}-ST{signalIndex}";

            return m_metadataRecords
                .Where(record => record.Synonym1.EndsWith(signalReferenceEnding))
                .ToDictionary(record => record, record => m_archiveReader.ReadData(record.HistorianID, StartTime, EndTime, false));
        }

        /// <summary>
        /// Reads data for a group of historian IDs.
        /// </summary>
        /// <param name="historianIDs">The historian IDs.</param>
        /// <returns>Collection of data points read from the archive.</returns>
        /// <remarks>
        /// All parameters--<see cref="ArchiveFilePath"/>, <see cref="StartTime"/>, and <see cref="EndTime"/>--must
        /// be set manually before attempting to read statistics from the archive.
        /// </remarks>
        public IEnumerable<IDataPoint> Read(IEnumerable<int> historianIDs)
        {
            return m_archiveReader.ReadData(historianIDs, StartTime, EndTime, false);
        }

        /// <summary>
        /// Closes the archive file once the statistics are no longer needed.
        /// </summary>
        public void Close()
        {
            if ((object)m_archiveReader == null)
                return;

            m_archiveReader.Dispose();
            m_archiveReader = null;
            m_metadataRecords = null;
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="StatisticsReader"/> object.
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
