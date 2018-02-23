//******************************************************************************************************
//  CsvImportAdapter.cs - Gbtc
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
//  02/21/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using GSF;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Scheduling;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace CsvAdapters
{
    /// <summary>
    /// Represents an input adapter that imports measurements in CSV format from a given directory.
    /// </summary>
    [Description("CSV Import: Reads data from a CSV file archive")]
    public class CsvImportAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="ImportSchedule"/> property.
        /// </summary>
        private const string DefaultImportSchedule = "*/5 * * * *";

        private const string ScheduleName = nameof(CsvImportAdapter);

        // Fields
        private LongSynchronizedOperation m_importOperation;
        private ScheduleManager m_scheduleManager;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CsvImportAdapter"/> class.
        /// </summary>
        public CsvImportAdapter()
        {
            m_importOperation = new LongSynchronizedOperation(Import);
            m_scheduleManager = new ScheduleManager();
            m_scheduleManager.ScheduleDue += ScheduleManager_ScheduleDue;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path in which to search for new CSV files.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the path in which to search for new CSV files")]
        public string ImportPath { get; set; }

        /// <summary>
        /// Gets or sets the schedule, defined by cron syntax, to search for new CSV files.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultImportSchedule)]
        [Description("Defines the schedule, defined by cron syntax, to search for new CSV files")]
        public string ImportSchedule { get; set; }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        protected override bool UseAsyncConnect => false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="AdapterBase"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            ConnectionStringParser<ConnectionStringParameterAttribute> parser = new ConnectionStringParser<ConnectionStringParameterAttribute>();
            parser.ParseConnectionString(ConnectionString, this);
            m_scheduleManager.AddSchedule(ScheduleName, ImportSchedule, true);
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        public override string GetShortStatus(int maxLength)
        {
            return $"{ProcessedMeasurements} measurements imported so far...".CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to connect to data input source.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_scheduleManager.Start();
        }

        /// <summary>
        /// Attempts to disconnect from data input source.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_scheduleManager.Stop();
        }

        // Searches the import path for new CSV files and imports the measurements contained therein.
        private void Import()
        {
            foreach (string filePath in FilePath.EnumerateFiles(ImportPath, "*.csv", exceptionHandler: HandleException))
            {
                using (TextReader reader = File.OpenText(filePath))
                {
                    // Skip header
                    reader.ReadLine();

                    List<IMeasurement> measurements = new List<IMeasurement>();
                    string line = reader.ReadLine();

                    while (!string.IsNullOrEmpty(line))
                    {
                        measurements.Add(FromCSV(line));
                        line = reader.ReadLine();
                    }

                    OnNewMeasurements(measurements);
                }

                File.Delete(filePath);
            }
        }

        // Converts the given row of CSV data to a single measurement.
        private IMeasurement FromCSV(string csv)
        {
            string[] split = csv.Split(',');
            DateTime timestamp = DateTime.Parse(split[0]);
            Guid signalID = Guid.Parse(split[1]);
            double value = double.Parse(split[2]);

            return new Measurement()
            {
                Metadata = MeasurementKey.LookUpBySignalID(signalID).Metadata,
                Timestamp = timestamp,
                Value = value
            };
        }

        // Executes the import process for measurement data in CSV files.
        private void ScheduleManager_ScheduleDue(object sender, EventArgs<Schedule> e)
        {
            m_importOperation.TryRun();
        }

        // Handles exceptions thrown while searching for new files to import.
        private void HandleException(Exception ex)
        {
            OnProcessException(MessageLevel.Error, ex);
        }

        #endregion
    }
}
