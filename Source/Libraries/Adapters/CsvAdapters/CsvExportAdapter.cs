//******************************************************************************************************
//  CsvExportAdapter.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/20/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.IO;
using GSF;
using GSF.Configuration;
using GSF.Scheduling;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace CsvAdapters
{
    /// <summary>
    /// Represents an output adapter that exports measurements in to a given directory in CSV format.
    /// </summary>
    [Description("CSV Export: Exports data to a rolling CSV archive")]
    public class CsvExportAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="RolloverSchedule"/> property.
        /// </summary>
        public const string DefaultRollverSchedule = "*/5 * * * *";

        /// <summary>
        /// Default value for the <see cref="TimestampFormat"/> property.
        /// </summary>
        public const string DefaultTimestampFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

        private const string ScheduleName = nameof(CsvExportAdapter);

        // Fields
        private string m_activeFileName;
        private object m_activeFileLock;
        private ScheduleManager m_scheduleManager;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CsvExportAdapter"/> class.
        /// </summary>
        public CsvExportAdapter()
        {
            m_activeFileLock = new object();
            m_scheduleManager = new ScheduleManager();
            m_scheduleManager.ScheduleDue += ScheduleManager_ScheduleDue;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the directory where CSV exports are written.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue("")]
        [Description("Defines the path to the directory where CSV exports are written")]
        public string ExportPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the directory where
        /// CSV exports are moved after a rollover.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue("")]
        [Description("Defines the path to the directory to which CSV exports are moved on rollover")]
        public string OffloadPath { get; set; }

        /// <summary>
        /// Gets or sets the schedule, defined by cron syntax,
        /// to determine how often to roll over CSV files.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultRollverSchedule)]
        [Description("Defines the schedule, defined by cron syntax, to determine how often to roll over CSV files")]
        public string RolloverSchedule { get; set; }

        /// <summary>
        /// Gets or sets the format of timestamps in the CSV exports.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultTimestampFormat)]
        [Description("Defines the format of timestamps in the CSV exports")]
        public string TimestampFormat { get; set; }

        /// <summary>
        /// Gets the flag that determines if measurements sent to this <see cref="CsvExportAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive => true;

        /// <summary>
        /// Gets flag that determines if the data output stream connects asynchronously.
        /// </summary>
        protected override bool UseAsyncConnect => false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="CsvExportAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            ConnectionStringParser<ConnectionStringParameterAttribute> parser = new ConnectionStringParser<ConnectionStringParameterAttribute>();
            parser.ParseConnectionString(ConnectionString, this);

            if (string.IsNullOrWhiteSpace(ExportPath))
                ExportPath = Path.Combine("CSVExports", Name);

            Directory.CreateDirectory(ExportPath);

            m_scheduleManager.AddSchedule(ScheduleName, RolloverSchedule, true);
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="CsvExportAdapter"/>.
        /// </summary>
        public override string GetShortStatus(int maxLength)
        {
            return $"{ProcessedMeasurements} measurements exported so far...".CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to connect to data output stream.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_scheduleManager.Start();
        }

        /// <summary>
        /// Attempts to disconnect from data output stream.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_scheduleManager.Stop();
        }

        /// <summary>
        /// Serializes measurements to data output stream.
        /// </summary>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            lock (m_activeFileLock)
            {
                if (string.IsNullOrWhiteSpace(m_activeFileName))
                    m_activeFileName = GenerateActiveFileName();

                string activeFilePath = Path.Combine(ExportPath, m_activeFileName);
                bool writeHeader = !File.Exists(activeFilePath);

                using (TextWriter writer = AppendToFile(activeFilePath))
                {
                    if (writeHeader)
                        writer.WriteLine("Timestamp,ID,Value");

                    foreach (IMeasurement measurement in measurements)
                        writer.WriteLine(ToCSV(measurement));
                }
            }
        }

        // Converts the measurement to a row in CSV format.
        private string ToCSV(IMeasurement measurement)
        {
            string timestamp = measurement.Timestamp.ToString(TimestampFormat);
            string id = measurement.Key.SignalID.ToString();
            string value = measurement.Value.ToString();
            return string.Join(",", timestamp, id, value);
        }

        // Generates the name of the next active file.
        private string GenerateActiveFileName()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm");
            return $"{timestamp}.csv";
        }

        // Executes the rollover process to offload the
        // currently active file and create a new active file.
        private void ScheduleManager_ScheduleDue(object sender, EventArgs<Schedule> e)
        {
            lock (m_activeFileLock)
            {
                if (string.IsNullOrWhiteSpace(m_activeFileName))
                    return;

                string activeFilePath = Path.Combine(ExportPath, m_activeFileName);

                if (!string.IsNullOrWhiteSpace(OffloadPath) && File.Exists(activeFilePath))
                {
                    Directory.CreateDirectory(OffloadPath);
                    string offloadFilePath = Path.Combine(OffloadPath, m_activeFileName);
                    File.Move(activeFilePath, offloadFilePath);
                }

                m_activeFileName = null;
            }
        }

        private StreamWriter AppendToFile(string filePath)
        {
            // The "Path traversal" security warning is not relevant
            // because the path comes from software configuration
            // performed by an administrator, not user input
#pragma warning disable SG0018 // Path traversal
            return File.AppendText(filePath);
#pragma warning restore SG0018 // Path traversal
        }

        #endregion
    }
}
