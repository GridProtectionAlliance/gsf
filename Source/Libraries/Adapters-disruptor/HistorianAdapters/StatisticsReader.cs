//******************************************************************************************************
//  StatisticsReader.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  12/14/2010 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using GSF.Historian;
using GSF.Historian.Files;

namespace HistorianAdapters
{
    /// <summary>
    /// Reads certain device and input stream statistics from the statistics archive.
    /// The statistics that are read are the total frame count, the missing frame count,
    /// connectivity, average latency, the actual data rate, data quality errors, and
    /// time quality errors.
    /// </summary>
    public class StatisticsReader
    {
        #region [ Members ]

        // Fields
        private ArchiveFile m_archive;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_totalFrames;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_missingFrames;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_connectedStats;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_averageLatency;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_actualDataRate;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_dataQualityErrors;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_timeQualityErrors;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="StatisticsReader"/> class.
        /// All parameters--<see cref="ArchiveFilePath"/>, <see cref="StateFilePath"/>,
        /// <see cref="IntercomFilePath"/>, <see cref="MetadataFilePath"/>,
        /// <see cref="StartTime"/>, and <see cref="EndTime"/>--must be set manually
        /// before attempting to read statistics from the archive.
        /// </summary>
        public StatisticsReader()
        {
            m_totalFrames = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_missingFrames = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_connectedStats = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_averageLatency = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_actualDataRate = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_dataQualityErrors = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_timeQualityErrors = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StatisticsReader"/> class.
        /// The configuration file specified in the parameter must contain the statistics
        /// historian records in order to automatically locate the archive files.
        /// Otherwise, the other constructor should be used. The <see cref="StartTime"/> and
        /// <see cref="EndTime"/> parameters are not set when using this constructor and must
        /// be set manually before attempting to read statistics from the archive.
        /// </summary>
        /// <param name="openPdcConfigPath">Path to the openPDC configuration file.</param>
        public StatisticsReader(string openPdcConfigPath)
            : this()
        {
            XmlDocument doc;
            XmlNode archiveFileNode;
            XmlNode stateFileNode;
            XmlNode intercomFileNode;
            XmlNode metadataFileNode;

            if (!File.Exists(openPdcConfigPath))
                throw new FileNotFoundException(openPdcConfigPath);

            doc = new XmlDocument();
            doc.Load(openPdcConfigPath);

            archiveFileNode = doc.SelectSingleNode("configuration/categorizedSettings/statArchiveFile");
            stateFileNode = doc.SelectSingleNode("configuration/categorizedSettings/statStateFile");
            intercomFileNode = doc.SelectSingleNode("configuration/categorizedSettings/statIntercomFile");
            metadataFileNode = doc.SelectSingleNode("configuration/categorizedSettings/statMetadataFile");

            if ((object)archiveFileNode != null)
                ArchiveFilePath = archiveFileNode.ChildNodes.Cast<XmlNode>().Single(node => node.Name == "FileName").Value;

            if ((object)stateFileNode != null)
                StateFilePath = stateFileNode.ChildNodes.Cast<XmlNode>().Single(node => node.Name == "FileName").Value;

            if ((object)intercomFileNode != null)
                IntercomFilePath = intercomFileNode.ChildNodes.Cast<XmlNode>().Single(node => node.Name == "FileName").Value;

            if ((object)metadataFileNode != null)
                MetadataFilePath = metadataFileNode.ChildNodes.Cast<XmlNode>().Single(node => node.Name == "FileName").Value;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Path to the archive file (*_archive.d).
        /// </summary>
        public string ArchiveFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the state file (*_startup.dat).
        /// </summary>
        public string StateFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the intercom file (scratch.dat).
        /// </summary>
        public string IntercomFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the metadata file (*_dbase.dat).
        /// </summary>
        public string MetadataFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// The timestamp at which to start gathering statistics from the archive.
        /// </summary>
        public DateTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// The timestamp at which to stop gathering statistics from the archive.
        /// </summary>
        public DateTime EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on the total number of frames.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> TotalFrames
        {
            get
            {
                return m_totalFrames;
            }
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on missing frames.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> MissingFrames
        {
            get
            {
                return m_missingFrames;
            }
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on input stream connectivity.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> ConnectedStats
        {
            get
            {
                return m_connectedStats;
            }
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on the average latency.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> AverageLatency
        {
            get
            {
                return m_averageLatency;
            }
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on the actual data rate.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> ActualDataRate
        {
            get
            {
                return m_actualDataRate;
            }
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on data quality errors.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> DataQualityErrors
        {
            get
            {
                return m_dataQualityErrors;
            }
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on time quality errors.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> TimeQualityErrors
        {
            get
            {
                return m_timeQualityErrors;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Causes the <see cref="StatisticsReader"/> to open the archive file and retrieve the statistics.
        /// </summary>
        public void Open()
        {
            IEnumerable<MetadataRecord> records;
            IEnumerable<MetadataRecord> totalFrameRecords;
            IEnumerable<MetadataRecord> missingFrameRecords;
            IEnumerable<MetadataRecord> connectedStatRecords;
            IEnumerable<MetadataRecord> averageLatencyRecords;
            IEnumerable<MetadataRecord> actualDataRateRecords;
            IEnumerable<MetadataRecord> dataQualityErrorRecords;
            IEnumerable<MetadataRecord> timeQualityErrorRecords;

            if (m_archive != null)
                throw new InvalidOperationException("The StatisticsReader must be closed before reopening.");

            m_archive = OpenArchiveFile();
            records = m_archive.MetadataFile.Read();

            totalFrameRecords = records.Where(record => record.Name.EndsWith("!IS:ST1"));
            missingFrameRecords = records.Where(record => record.Name.EndsWith("!IS:ST3"));
            connectedStatRecords = records.Where(record => record.Name.EndsWith("!IS:ST8"));
            averageLatencyRecords = records.Where(record => record.Name.EndsWith("!IS:ST14"));
            actualDataRateRecords = records.Where(record => record.Name.EndsWith("!IS:ST17"));
            dataQualityErrorRecords = records.Where(IsDataQualityError);
            timeQualityErrorRecords = records.Where(IsTimeQualityError);

            foreach (MetadataRecord totalFrameRecord in totalFrameRecords)
            {
                m_totalFrames.Add(totalFrameRecord, m_archive.ReadData(totalFrameRecord.HistorianID, StartTime, EndTime));
            }

            foreach (MetadataRecord missingFrameRecord in missingFrameRecords)
            {
                m_missingFrames.Add(missingFrameRecord, m_archive.ReadData(missingFrameRecord.HistorianID, StartTime, EndTime));
            }

            foreach (MetadataRecord connectedStatRecord in connectedStatRecords)
            {
                m_connectedStats.Add(connectedStatRecord, m_archive.ReadData(connectedStatRecord.HistorianID, StartTime, EndTime));
            }

            foreach (MetadataRecord averageLatencyRecord in averageLatencyRecords)
            {
                m_averageLatency.Add(averageLatencyRecord, m_archive.ReadData(averageLatencyRecord.HistorianID, StartTime, EndTime));
            }

            foreach (MetadataRecord actualDataRateRecord in actualDataRateRecords)
            {
                m_actualDataRate.Add(actualDataRateRecord, m_archive.ReadData(actualDataRateRecord.HistorianID, StartTime, EndTime));
            }

            foreach (MetadataRecord dataQualityErrorRecord in dataQualityErrorRecords)
            {
                m_dataQualityErrors.Add(dataQualityErrorRecord, m_archive.ReadData(dataQualityErrorRecord.HistorianID, StartTime, EndTime));
            }

            foreach (MetadataRecord timeQualityErrorRecord in timeQualityErrorRecords)
            {
                m_timeQualityErrors.Add(timeQualityErrorRecord, m_archive.ReadData(timeQualityErrorRecord.HistorianID, StartTime, EndTime));
            }
        }

        /// <summary>
        /// Closes the archive file once the statistics are no longer needed.
        /// </summary>
        public void Close()
        {
            if ((object)m_archive == null)
                return;

            m_archive.Close();
            m_archive = null;
        }

        // Opens the archive file in order to retrieve the statistics.
        private ArchiveFile OpenArchiveFile()
        {
            ArchiveFile file = new ArchiveFile
            {
                FileName = ArchiveFilePath,
                FileAccessMode = FileAccess.Read,

                StateFile = new StateFile
                {
                    FileAccessMode = FileAccess.Read,
                    FileName = StateFilePath
                },

                IntercomFile = new IntercomFile
                {
                    FileAccessMode = FileAccess.Read,
                    FileName = IntercomFilePath
                },

                MetadataFile = new MetadataFile
                {
                    FileAccessMode = FileAccess.Read,
                    FileName = MetadataFilePath,
                    LoadOnOpen = true
                }
            };

            file.Open();

            return file;
        }

        // Determines whether the given MetadataRecord describes the data quality error statistic.
        private static bool IsDataQualityError(MetadataRecord record)
        {
            return record.Name.EndsWith(":ST1") && !record.Name.EndsWith("!IS:ST1") && !record.Name.EndsWith("!OS:ST1");
        }

        // Determines whether the given MetadataRecord describes the time quality error statistic.
        private static bool IsTimeQualityError(MetadataRecord record)
        {
            return record.Name.EndsWith(":ST2") && !record.Name.EndsWith("!IS:ST2") && !record.Name.EndsWith("!OS:ST2");
        }

        #endregion
    }
}
