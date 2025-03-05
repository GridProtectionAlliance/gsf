//******************************************************************************************************
//  CsvFrameCollectorAdapter.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  01/02/2025 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace CsvAdapters
{
    /// <summary>
    /// Represents an output adapter that writes frames to a CSV file.
    /// </summary>
    [Description("CSV: Archives frames to a CSV file")]
    public class CsvFrameCollectorAdapter : ActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="TimestampFormat"/> property.
        /// </summary>
        public const string DefaultTimestampFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

        /// <summary>
        /// Default value for the <see cref="FileNameTemplate"/> property.
        /// </summary>
        public const string DefaultFileNameTemplate = "{0:yyyy-MM-dd HH.mm}.csv";

        /// <summary>
        /// Default value for the <see cref="SignalNameTemplate"/> property.
        /// </summary>
        public const string DefaultSignalNameTemplate = "{MeasurementID}";

        /// <summary>
        /// Default value for the <see cref="DownsampleInterval"/> property.
        /// </summary>
        public const double DefaultDownsampleInterval = 0.0D;

        /// <summary>
        /// Default value for the <see cref="RolloverInterval"/> property.
        /// </summary>
        public const double DefaultRolloverInterval = 60.0D;

        // Fields
        private readonly object m_activeFileLock;
        private string m_headerText;
        private long m_lastTimestamp;
        private long m_lastRolloverIndex;
        private long m_downsampleFrameCount;
        private long m_rolloverFrameCount;
        private long m_totalExports;
        private bool m_needsOffload;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CsvFrameCollectorAdapter"/> class.
        /// </summary>
        public CsvFrameCollectorAdapter()
        {
            m_activeFileLock = new object();
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
        /// Gets or sets the interval, in seconds,
        /// to determine how often to roll over CSV files.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultRolloverInterval)]
        [Description("Defines the rollover interval, in seconds")]
        public double RolloverInterval { get; set; }

        /// <summary>
        /// Gets or sets the format of timestamps in the CSV exports.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultTimestampFormat)]
        [Description("Defines the format of timestamps in the CSV exports")]
        public string TimestampFormat { get; set; }

        /// <summary>
        /// Gets or sets the CSV export filename template. String format parameter 0 is current UTC time.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultFileNameTemplate)]
        [Description("Defines the CSV export filename template - string format parameter 0 is current UTC time")]
        public string FileNameTemplate { get; set; }

        /// <summary>
        /// Gets or sets the CSV export signalname template - accepts \"{MeasurementID}\", \"{PointTag}\" and \"{SignalReference}\".
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultSignalNameTemplate)]
        [Description("Defines the CSV export signalname template - accepts \"{MeasurementID}\" and \"{PointTag}\"")]
        public string SignalNameTemplate { get; set; }

        /// <summary>
        /// Gets or sets the downsampling interval, in seconds, set to zero for no downsampling.
        /// </summary>
        [ConnectionStringParameter]
        [DefaultValue(DefaultDownsampleInterval)]
        [Description("Defines the downsampling interval, in seconds, set to 0.0 for no downsampling")]
        public double DownsampleInterval { get; set; }

        /// <summary>
        /// Gets the status of this <see cref="CsvFrameCollectorAdapter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                string activeFileName;
                lock (m_activeFileLock)
                    activeFileName = GenerateActiveFileName(m_lastRolloverIndex * m_rolloverFrameCount);

                StringBuilder status = new StringBuilder(base.Status);

                status.AppendLine($"               Export Path: {FilePath.TrimFileName(ExportPath, 51)}");
                status.AppendLine($"              Offload Path: {FilePath.TrimFileName(OffloadPath, 51)}");
                status.AppendLine($"         Rollover Interval: {RolloverInterval:N4} seconds");
                status.AppendLine($"          Timestamp Format: {TimestampFormat}");
                status.AppendLine($"    CSV File Name Template: {FileNameTemplate.TrimWithEllipsisEnd(51)}");
                status.AppendLine($"     Downsampling Interval: {(DownsampleInterval > 0.0D ? $"{DownsampleInterval:N4} seconds)" : "Disabled - Full Resolution Export")}");

                if (DownsampleInterval > 0.0D)
                    status.AppendLine($"         Frames Per Second: {FramesPerSecond:N0}");

                status.AppendLine($"          Allowed Lag Time: {LagTime:N4} seconds");
                status.AppendLine($"         Allowed Lead Time: {LeadTime:N4} seconds");
                status.AppendLine($"    Active CSV Export File: {FilePath.TrimFileName(activeFileName, 51)}");
                status.AppendLine($"         Total CSV Exports: {m_totalExports:N0}");

                return status.ToString();
            }
        }

        /// <inheritdoc/>
        public override bool SupportsTemporalProcessing => false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="CsvFrameCollectorAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            ConnectionStringParser<ConnectionStringParameterAttribute> parser = new ConnectionStringParser<ConnectionStringParameterAttribute>();
            parser.ParseConnectionString(ConnectionString, this);

            Ticks rolloverIntervalTicks = Ticks.FromSeconds(RolloverInterval);
            long computedFrameCount = ToFrameCount(rolloverIntervalTicks);
            m_rolloverFrameCount = Math.Max(computedFrameCount, 1L);

            if (DownsampleInterval > 0.0D)
            {
                // For framerate-aligned downsampling, compute the exact number of frames represented
                // by the interval to accurately determine which frames need to be exported
                Ticks downsampleIntervalTicks = Ticks.FromSeconds(DownsampleInterval);
                computedFrameCount = ToFrameCount(downsampleIntervalTicks);
                m_downsampleFrameCount = Math.Max(computedFrameCount, 1L);
            }

            base.Initialize();

            if (string.IsNullOrWhiteSpace(ExportPath))
                ExportPath = Path.Combine("CSVExports", Name);

            InputMeasurementKeys = InputMeasurementKeys
                .OrderBy(key => key.ID)
                .ToArray();

            m_headerText = "Timestamp," + string.Join(",", InputMeasurementKeys.Select(ToHeader));
        }

        /// <summary>
        /// Offloads lingering files which were not
        /// offloaded due to errors or system failures.
        /// </summary>
        [AdapterCommand("Offloads lingering files which were not offloaded due to errors or system failures")]
        public void OffloadLingeringFiles()
        {
            if (string.IsNullOrWhiteSpace(OffloadPath))
                return;

            string targetFileExtension = Path.GetExtension(GenerateActiveFileName(DateTime.UtcNow));

            if (string.IsNullOrWhiteSpace(targetFileExtension))
                targetFileExtension = ".csv";

            lock (m_activeFileLock)
            {
                string activeFileName = GenerateActiveFileName(m_lastRolloverIndex * m_rolloverFrameCount);

                foreach (string filePath in FilePath.EnumerateFiles(ExportPath, $"*{targetFileExtension}", SearchOption.TopDirectoryOnly, HandleException))
                {
                    string fileName = Path.GetFileName(filePath);

                    // Ignore the active file, if it exists
                    if (fileName == activeFileName)
                        continue;

                    string offloadFilePath = Path.Combine(OffloadPath, fileName);
                    Directory.CreateDirectory(OffloadPath);
                    File.Move(filePath, offloadFilePath);
                }
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="CsvFrameCollectorAdapter"/>.
        /// </summary>
        public override string GetShortStatus(int maxLength) =>
            $"{ProcessedMeasurements:N0} measurements exported so far...".CenterText(maxLength);

        /// <inheritdoc/>
        public override void Start()
        {
            m_needsOffload = true;
            base.Start();
        }

        /// <summary>
        /// Serializes frame to data output stream.
        /// </summary>
        private void ProcessFrame(string activeFileName, IFrame frame)
        {
            lock (m_activeFileLock)
            {
                string activeFilePath = Path.Combine(ExportPath, activeFileName);
                bool writeHeader = !File.Exists(activeFilePath);
                Directory.CreateDirectory(ExportPath);

                string timestamp = frame.Timestamp.ToString(TimestampFormat);

                using (TextWriter writer = AppendToFile(activeFilePath))
                {
                    if (writeHeader)
                        writer.WriteLine(m_headerText);

                    IEnumerable<string> values = InputMeasurementKeys
                        .Select(frame.Measurements.GetOrDefault)
                        .Select(measurement => measurement?.AdjustedValue.ToString() ?? string.Empty);

                    writer.WriteLine($"{timestamp},{string.Join(",", values)}");
                }
            }
        }

        /// <inheritdoc/>
        protected override void PublishFrame(IFrame frame, int index)
        {
            if (DownsampleInterval > 0.0D)
            {
                // If last measurement timestamp is within the downsampling interval, then we have
                // a measurement that is too close to the last measurement and should be ignored
                if ((frame.Timestamp - m_lastTimestamp).ToSeconds() < DownsampleInterval)
                    return;

                // For framerate-aligned downsampling, only export measurements that fall on the downsampling interval
                if (ToFrameCount(frame.Timestamp) % m_downsampleFrameCount != 0)
                    return;

                m_lastTimestamp = frame.Timestamp.Value;
            }

            long rolloverIndex = ToFrameCount(frame.Timestamp) / m_rolloverFrameCount;

            if (rolloverIndex > m_lastRolloverIndex)
            {
                string previousFileName = GenerateActiveFileName(m_lastRolloverIndex * m_rolloverFrameCount);
                RollOver(previousFileName);
            }

            lock (m_activeFileLock)
            {
                m_lastRolloverIndex = rolloverIndex;
                string activeFileName = GenerateActiveFileName(rolloverIndex * m_rolloverFrameCount);
                ProcessFrame(activeFileName, frame);
            }

            if (m_needsOffload)
            {
                OffloadLingeringFiles();
                m_needsOffload = false;
            }
        }

        // Computes the number of frames represented by the given time interval.
        private long ToFrameCount(Ticks ticks)
        {
            // Prevent overflow by splitting the computation into second and subsecond components
            long baseFrameCount = ticks / Ticks.PerSecond * FramesPerSecond;
            long subsecondTicks = ticks % Ticks.PerSecond;

            // Perform rounding by adding half the denominator to the numerator
            long subsecondFrameCount = (subsecondTicks * FramesPerSecond + Ticks.PerSecond / 2L) / Ticks.PerSecond;
            return baseFrameCount + subsecondFrameCount;
        }

        // Computes the number of ticks since epoch represented by the given frame count.
        private Ticks FromFrameCount(long frameCount)
        {
            // Prevent overflow using the same strategy as ToFrameCount()
            long baseTicks = frameCount / FramesPerSecond * Ticks.PerSecond;
            long subFramerateFrameCount = frameCount % FramesPerSecond;
            long subFramerateTicks = (subFramerateFrameCount * Ticks.PerSecond + FramesPerSecond / 2L) / FramesPerSecond;
            return baseTicks + subFramerateTicks;
        }

        // Converts the measurement to an entry in CSV header format.
        private string ToHeader(MeasurementKey key)
        {
            string measurementName = SignalNameTemplate;
            measurementName = measurementName.Replace("{MeasurementID}", key.SignalID.ToString());
            measurementName = measurementName.Replace("{PointTag}", key.Metadata.TagName);
            return measurementName;
        }

        // Generates the name of the next active file.
        private string GenerateActiveFileName(long frameCount) =>
            GenerateActiveFileName((DateTime)FromFrameCount(frameCount));

        // Generates the name of the next active file.
        private string GenerateActiveFileName(DateTime timestamp) =>
            string.Format(FileNameTemplate, timestamp);

        // Rolls over the active file by moving it to the offload directory
        // and unsetting the active file name so that a new active file
        // can be generated the next time this adapter needs to update it.
        private void RollOver(string activeFileName)
        {
            string activeFilePath = Path.Combine(ExportPath, activeFileName);

            if (string.IsNullOrWhiteSpace(OffloadPath) || !File.Exists(activeFilePath))
                return;

            m_totalExports++;
            Directory.CreateDirectory(OffloadPath);
            string offloadFilePath = Path.Combine(OffloadPath, activeFileName);
            File.Move(activeFilePath, offloadFilePath);
        }

        // Handles the given exception.
        private void HandleException(Exception ex) =>
            OnProcessException(MessageLevel.Error, ex);

        private static StreamWriter AppendToFile(string filePath)
        {
            // The "Path traversal" security warning is not relevant
            // because the path comes from software configuration
            // performed by an administrator, not user input
            #pragma warning disable SG0018 // Path traversal
            return File.AppendText(filePath);
            #pragma warning restore SG0018
        }

        #endregion
    }
}
