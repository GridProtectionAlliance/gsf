//******************************************************************************************************
//  MetadataExportAdapter.cs - Gbtc
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
//  02/23/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace MetadataAdapters
{
    /// <summary>
    /// Defines an adapter that automatically dumps metadata to a file
    /// </summary>
    [Description("Metadata Export: Automatically exports metadata to a file")]
    public class MetadataExportAdapter : FacileActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="MetadataTables"/>.
        /// </summary>
        public const string DefaultMetadataTables =
            "SELECT NodeID, UniqueID, OriginalSource, IsConcentrator, Acronym, Name, AccessID, ParentAcronym, ProtocolName, FramesPerSecond, CompanyAcronym, VendorAcronym, VendorDeviceName, Longitude, Latitude, InterconnectionName, ContactList, Enabled, UpdatedOn FROM DeviceDetail WHERE IsConcentrator = 0;" +
            "SELECT DeviceAcronym, ID, SignalID, PointTag, SignalReference, SignalAcronym, PhasorSourceIndex, Description, Internal, Enabled, UpdatedOn FROM MeasurementDetail WHERE SignalAcronym <> 'STAT';" +
            "SELECT ID, DeviceAcronym, Label, Type, Phase, DestinationPhasorID, SourceIndex, UpdatedOn FROM PhasorDetail;" +
            "SELECT VersionNumber FROM SchemaVersion";

        // Fields
        private LongSynchronizedOperation m_dumpOperation;
        private DateTime m_lastDump;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MetadataExportAdapter"/> class.
        /// </summary>
        public MetadataExportAdapter()
        {
            m_dumpOperation = new LongSynchronizedOperation(DumpMetadata, HandleException);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets semi-colon separated list of SQL select statements used to create data for metadata exchange.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultMetadataTables)]
        [Description("Semi-colon separated list of SQL select statements used to create data for metadata exchange.")]
        public string MetadataTables { get; set; }

        /// <summary>
        /// The file path used when exporting metadata.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue("")]
        [Description("Defines the file path used when exporting metadata.")]
        public string ExportFilePath { get; set; }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="AdapterBase"/>.
        /// </summary>
        public override DataSet DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                base.DataSource = value;
                ExportMetadata();
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Gets or sets primary keys of input measurements the <see cref="MetadataExportAdapter"/> expects, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return base.InputMeasurementKeys;
            }
            set
            {
                base.InputMeasurementKeys = value;
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the <see cref="MetadataExportAdapter"/> will produce, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new IMeasurement[] OutputMeasurements
        {
            get
            {
                return base.OutputMeasurements;
            }
            set
            {
                base.OutputMeasurements = value;
            }
        }

        /// <summary>
        /// Gets or sets the frames per second to be used by the <see cref="MetadataExportAdapter"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new int FramesPerSecond
        {
            get
            {
                return base.FramesPerSecond;
            }
            set
            {
                base.FramesPerSecond = value;
            }
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double LagTime
        {
            get
            {
                return base.LagTime;
            }
            set
            {
                base.LagTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new double LeadTime
        {
            get
            {
                return base.LeadTime;
            }
            set
            {
                base.LeadTime = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="MetadataExportAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            ConnectionStringParser<ConnectionStringParameterAttribute> parser = new ConnectionStringParser<ConnectionStringParameterAttribute>();
            parser.ParseConnectionString(ConnectionString, this);

            base.Initialize();

            if (string.IsNullOrWhiteSpace(ExportFilePath))
                ExportFilePath = Path.Combine("MetadataExports", $"{Name}.bin");
        }

        /// <summary>
        /// Starts the <see cref="MetadataExportAdapter"/> or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            base.Start();
            ExportMetadata();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="MetadataExportAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="MetadataExportAdapter"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            string shortStatus = "Dumping metadata...";

            if (!m_dumpOperation.IsRunning)
            {
                string timestampFormat = "HH:mm:ss";

                if (DateTime.Now.Subtract(m_lastDump).TotalDays > 1.0D)
                    timestampFormat = "yyyy-MM-dd HH:mm";

                string lastDump = m_lastDump.ToString(timestampFormat);

                shortStatus = $"Last dump: {lastDump}";
            }

            return shortStatus.CenterText(maxLength);
        }

        /// <summary>
        /// Initiates the operation to export metadata.
        /// </summary>
        [AdapterCommand("Initiates the operation to export metadata.")]
        public void ExportMetadata()
        {
            if (Enabled)
                m_dumpOperation.RunOnceAsync();
        }

        // Executes the metadata export.
        private void DumpMetadata()
        {
            const int MaxFailedAttempts = 5;
            const int DelayAfterFailure = 2000;
            int failedAttempts = 0;

            while (true)
            {
                try
                {
                    string directory = Path.GetDirectoryName(ExportFilePath);

                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    // Open the file to be exported
                    using (FileStream stream = File.Create(ExportFilePath))
                    {
                        // Get metadata and serialize it to the file
                        DataSet metadata = AcquireMetadata();
                        metadata.SerializeToStream(stream);
                        m_lastDump = DateTime.Now;
                        break;
                    }
                }
                catch (IOException)
                {
                    // Only throw an IOException
                    // after five failed attempts
                    failedAttempts++;

                    if (failedAttempts >= MaxFailedAttempts)
                        throw;

                    Thread.Sleep(DelayAfterFailure);
                }
            }
        }

        // Gets the metadata to be exported to a file.
        private DataSet AcquireMetadata()
        {
            using (AdoDataConnection adoDatabase = new AdoDataConnection("systemSettings"))
            {
                IDbConnection dbConnection = adoDatabase.Connection;
                DataSet metadata = new DataSet();

                // Initialize active node ID
                Guid nodeID = Guid.Parse(dbConnection.ExecuteScalar($"SELECT NodeID FROM IaonActionAdapter WHERE ID = {ID}").ToString());

                // Copy key metadata tables
                foreach (string tableExpression in MetadataTables.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(tableExpression))
                        continue;

                    // Query the table or view information from the database
                    DataTable table = dbConnection.RetrieveData(adoDatabase.AdapterType, tableExpression);

                    // Remove any expression from table name
                    Match regexMatch = Regex.Match(tableExpression, @"FROM \w+");
                    table.TableName = regexMatch.Value.Split(' ')[1];

                    // Add a copy of the results to the dataset for metadata exchange
                    metadata.Tables.Add(table.Copy());
                }

                return metadata;
            }
        }

        // Handles exceptions encountered by the dump operation.
        private void HandleException(Exception ex)
        {
            OnProcessException(MessageLevel.Error, ex);
        }

        #endregion
    }
}
