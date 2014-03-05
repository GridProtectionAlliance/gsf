//******************************************************************************************************
//  StatisticsReader.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
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
        private ArchiveFile m_archive;

        private string m_archiveFilePath;
        private string m_stateFilePath;
        private string m_intercomFilePath;
        private string m_metadataFilePath;
        private DateTime m_startTime;
        private DateTime m_endTime;

        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_dataQualityErrors;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_timeQualityErrors;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_measurementsReceived;
        private readonly Dictionary<MetadataRecord, IEnumerable<IDataPoint>> m_measurementsExpected;

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
            m_dataQualityErrors = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_timeQualityErrors = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_measurementsReceived = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
            m_measurementsExpected = new Dictionary<MetadataRecord, IEnumerable<IDataPoint>>();
        }

        #endregion

        #region [ Properties ]

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
        /// Path to the state file (*_startup.dat).
        /// </summary>
        public string StateFilePath
        {
            get
            {
                return m_stateFilePath;
            }
            set
            {
                m_stateFilePath = value;
            }
        }

        /// <summary>
        /// Path to the intercom file (scratch.dat).
        /// </summary>
        public string IntercomFilePath
        {
            get
            {
                return m_intercomFilePath;
            }
            set
            {
                m_intercomFilePath = value;
            }
        }

        /// <summary>
        /// Path to the metadata file (*_dbase.dat).
        /// </summary>
        public string MetadataFilePath
        {
            get
            {
                return m_metadataFilePath;
            }
            set
            {
                m_metadataFilePath = value;
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

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on the number of data quality errors.
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
        /// this will contain the statistics on the number of time quality errors.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> TimeQualityErrors
        {
            get
            {
                return m_timeQualityErrors;
            }
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on the number of measurements received.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> MeasurementsReceived
        {
            get
            {
                return m_measurementsReceived;
            }
        }

        /// <summary>
        /// After the <see cref="StatisticsReader"/> has read statistics from the archive file,
        /// this will contain the statistics on the number of measurements expected.
        /// </summary>
        public Dictionary<MetadataRecord, IEnumerable<IDataPoint>> MeasurementsExpected
        {
            get
            {
                return m_measurementsExpected;
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

            m_archive = OpenArchiveFile();
            records = m_archive.MetadataFile.Read();

            foreach (MetadataRecord record in records)
            {
                if (record.Synonym1.EndsWith("!PMU-ST2"))
                    m_dataQualityErrors.Add(record, m_archive.ReadData(record.HistorianID, StartTime, EndTime));
                else if (record.Synonym1.EndsWith("!PMU-ST3"))
                    m_timeQualityErrors.Add(record, m_archive.ReadData(record.HistorianID, StartTime, EndTime));
                else if (record.Synonym1.EndsWith("!PMU-ST4"))
                    m_measurementsReceived.Add(record, m_archive.ReadData(record.HistorianID, StartTime, EndTime));
                else if (record.Synonym1.EndsWith("!PMU-ST5"))
                    m_measurementsExpected.Add(record, m_archive.ReadData(record.HistorianID, StartTime, EndTime));
            }
        }

        /// <summary>
        /// Closes the archive file once the statistics are no longer needed.
        /// </summary>
        public void Close()
        {
            if ((object)m_archive == null)
                return;

            m_dataQualityErrors.Clear();
            m_timeQualityErrors.Clear();
            m_measurementsReceived.Clear();
            m_measurementsExpected.Clear();

            m_archive.Close();
            m_archive = null;
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="StatisticsReader"/> object.
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
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

        #endregion
    }
}
