//******************************************************************************************************
//  FrameParser.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  05/19/2011 - Ritchie
//       Added DST file support.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Security.Permissions;
using System.Text;
using GSF.IO;
using GSF.Parsing;

namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents a frame parser for a BPA PDCstream binary data stream and returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")] // Required by file watcher...
    public class FrameParser : FrameParserBase<FrameType>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a BPA PDCstream <see cref="ConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a BPA PDCstream <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

        // Fields
        private ConfigurationFrame m_configurationFrame;
        private string m_configurationFileName;
        private bool m_refreshConfigurationFileOnChange;
        private SafeFileWatcher m_configurationFileWatcher;
        private readonly object m_syncLock;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParser"/>.
        /// </summary>
        /// <param name="checkSumValidationFrameTypes">Frame types that should perform check-sum validation; default to <see cref="GSF.PhasorProtocols.CheckSumValidationFrameTypes.AllFrames"/></param>
        /// <param name="trustHeaderLength">Determines if header lengths should be trusted over parsed byte count.</param>
        /// <param name="configurationFileName">The required external BPA PDCstream INI based configuration file.</param>
        public FrameParser(CheckSumValidationFrameTypes checkSumValidationFrameTypes = CheckSumValidationFrameTypes.AllFrames, bool trustHeaderLength = true, string configurationFileName = null)
            : base(checkSumValidationFrameTypes, trustHeaderLength)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new[] { PhasorProtocols.Common.SyncByte };
            m_syncLock = new object();
            m_configurationFileName = configurationFileName;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets current <see cref="IConfigurationFrame"/> used for parsing <see cref="IDataFrame"/>'s encountered in the data stream from a device.
        /// </summary>
        /// <remarks>
        /// If a <see cref="IConfigurationFrame"/> has been parsed, this will return a reference to the parsed frame.  Consumer can manually assign a
        /// <see cref="IConfigurationFrame"/> to start parsing data if one has not been encountered in the stream.
        /// </remarks>
        public override IConfigurationFrame ConfigurationFrame
        {
            get => m_configurationFrame;
            set => m_configurationFrame = CastToDerivedConfigurationFrame(value, m_configurationFileName);
        }

        /// <summary>
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes => !UsePhasorDataFileFormat;

        /// <summary>
        /// Gets or sets required external BPA PDCstream INI based configuration file.
        /// </summary>
        public string ConfigurationFileName
        {
            get => m_configurationFrame?.ConfigurationFileName ?? m_configurationFileName;
            set
            {
                m_configurationFileName = value;
                ResetFileWatcher();
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if configuration file is automatically reloaded when it has changed on disk.
        /// </summary>
        public bool RefreshConfigurationFileOnChange
        {
            get => m_refreshConfigurationFileOnChange;
            set
            {
                m_refreshConfigurationFileOnChange = value;
                ResetFileWatcher();
            }
        }

        /// <summary>
        /// Gets or sets flag that interprets word count in packet header from a byte instead of a word.
        /// </summary>
        /// <remarks>
        /// Set to <c>true</c> to interpret word count in packet header from a byte instead of a word - if the sync byte
        /// (0xAA) is at position one, then the word count would be interpreted from byte four.  Some older BPA PDC
        /// stream implementations have a 0x01 in byte three where there should be a 0x00 and this throws off the
        /// frame length, setting this property to <c>true</c> will correctly interpret the word count.
        /// </remarks>
        public bool ParseWordCountFromByte { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if source data is in the Phasor Data File Format (i.e., a DST file).
        /// </summary>
        public bool UsePhasorDataFileFormat { get; set; }

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParser"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("    INI configuration file: ");
                status.Append(FilePath.TrimFileName(m_configurationFileName, 51));
                status.AppendLine();
                status.Append("  Using phasor file format: ");
                status.Append(UsePhasorDataFileFormat);
                status.AppendLine();

                if (m_configurationFrame is not null)
                {
                    status.Append("       BPA PDC stream type: ");
                    status.Append(m_configurationFrame.StreamType);
                    status.AppendLine();
                    status.Append("   BPA PDC revision number: ");
                    status.Append(m_configurationFrame.RevisionNumber);
                    status.AppendLine();
                }

                status.Append(" Auto-reload configuration: ");
                status.Append(m_refreshConfigurationFileOnChange);
                status.AppendLine();
                status.Append("Parse word count from byte: ");
                status.Append(ParseWordCountFromByte);
                status.AppendLine();
                
                status.Append(base.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets or sets any connection specific <see cref="IConnectionParameters"/> that may be needed for parsing.
        /// </summary>
        public override IConnectionParameters ConnectionParameters
        {
            get => base.ConnectionParameters;
            set
            {
                if (value is not ConnectionParameters parameters)
                    return;

                base.ConnectionParameters = parameters;

                // Assign new incoming connection parameter values
                m_configurationFileName = parameters.ConfigurationFileName;
                ParseWordCountFromByte = parameters.ParseWordCountFromByte;
                m_refreshConfigurationFileOnChange = parameters.RefreshConfigurationFileOnChange;
                UsePhasorDataFileFormat = parameters.UsePhasorDataFileFormat;
                ResetFileWatcher();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FrameParser"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (m_configurationFileWatcher is not null)
                {
                    m_configurationFileWatcher.Changed -= m_configurationFileWatcher_Changed;
                    m_configurationFileWatcher.Dispose();
                }

                m_configurationFileWatcher = null;
            }
            finally
            {
                m_disposed = true;       // Prevent duplicate dispose.
                base.Dispose(disposing); // Call base class Dispose().
            }
        }

        /// <summary>
        /// Start the data parser.
        /// </summary>
        public override void Start()
        {
            // We narrow down parsing types to just those needed...
            base.Start(new[] { typeof(DataFrame), typeof(ConfigurationFrame) });
        }

        /// <summary>
        /// Parses a common header instance that implements <see cref="ICommonHeader{TTypeIdentifier}"/> for the output type represented
        /// in the binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <returns>The <see cref="ICommonHeader{TTypeIdentifier}"/> which includes a type ID for the <see cref="Type"/> to be parsed.</returns>
        /// <remarks>
        /// <para>
        /// Derived classes need to provide a common header instance (i.e., class that implements <see cref="ICommonHeader{TTypeIdentifier}"/>)
        /// for the output types; this will primarily include an ID of the <see cref="Type"/> that the data image represents.  This parsing is
        /// only for common header information, actual parsing will be handled by output type via its <see cref="ISupportBinaryImage.ParseBinaryImage"/>
        /// method. This header image should also be used to add needed complex state information about the output type being parsed if needed.
        /// </para>
        /// <para>
        /// If there is not enough buffer available to parse common header (as determined by <paramref name="length"/>), return null.  Also, if
        /// the protocol allows frame length to be determined at the time common header is being parsed and there is not enough buffer to parse
        /// the entire frame, it will be optimal to prevent further parsing by returning null.
        /// </para>
        /// </remarks>
        protected override ICommonHeader<FrameType> ParseCommonHeader(byte[] buffer, int offset, int length)
        {
            // See if there is enough data in the buffer to parse the common frame header.
            if (length < CommonFrameHeader.FixedLength)
                return null;

            // Parse common frame header
            CommonFrameHeader parsedFrameHeader = new CommonFrameHeader(ParseWordCountFromByte, UsePhasorDataFileFormat, m_configurationFrame, buffer, offset, length);

            // As an optimization, we also make sure entire frame buffer image is available to be parsed - by doing this
            // we eliminate the need to validate length on all subsequent data elements that comprise the frame
            if (length < parsedFrameHeader.FrameLength)
                return null;

            // Expose the frame buffer image in case client needs this data for any reason
            OnReceivedFrameBufferImage(parsedFrameHeader.FrameType, buffer, offset, parsedFrameHeader.FrameLength);

            // Handle special parsing states
            switch (parsedFrameHeader.TypeID)
            {
                case FrameType.DataFrame:
                    // Assign data frame parsing state
                    parsedFrameHeader.State = new DataFrameParsingState(parsedFrameHeader.FrameLength, m_configurationFrame, DataCell.CreateNewCell, TrustHeaderLength, ValidateDataFrameCheckSum);
                    break;
                case FrameType.ConfigurationFrame:
                    // Assign configuration frame parsing state
                    parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, m_configurationFileName, ConfigurationCell.CreateNewCell, TrustHeaderLength, ValidateConfigurationFrameCheckSum);
                    break;
            }

            return parsedFrameHeader;
        }

        // Handler for file watcher - we notify consumer when changes have occured to configuration file
        private void m_configurationFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // We synchronize change actions - don't want more than one refresh happening at a time...
            lock (m_syncLock)
            {
                // Notify consumer of change in configuration
                OnConfigurationChanged();

                // Reload configuration...
                m_configurationFrame?.Refresh();
            }
        }

        // Reset file watcher
        private void ResetFileWatcher()
        {
            if (m_configurationFileWatcher is not null)
            {
                m_configurationFileWatcher.Changed -= m_configurationFileWatcher_Changed;
                m_configurationFileWatcher.Dispose();
            }

            m_configurationFileWatcher = null;

            string configurationFile = ConfigurationFileName;

            if (!m_refreshConfigurationFileOnChange || string.IsNullOrEmpty(configurationFile) || !File.Exists(configurationFile))
                return;

            try
            {
                // Create a new file watcher for configuration file - we'll automatically refresh configuration file when this file gets updated...
                m_configurationFileWatcher = new SafeFileWatcher(FilePath.GetDirectoryName(configurationFile), FilePath.GetFileName(configurationFile));
                m_configurationFileWatcher.Changed += m_configurationFileWatcher_Changed;
                m_configurationFileWatcher.EnableRaisingEvents = true;
                m_configurationFileWatcher.IncludeSubdirectories = false;
                m_configurationFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            }
            catch (Exception ex)
            {
                OnParsingException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IConfigurationFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.</param>
        protected override void OnReceivedConfigurationFrame(IConfigurationFrame frame)
        {
            // We override this method so we can cache configuration frame when it's received
            base.OnReceivedConfigurationFrame(frame);

            // Cache new configuration frame for parsing subsequent data frames...
            if (frame is ConfigurationFrame configurationFrame)
                m_configurationFrame = configurationFrame;
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/> or <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise BPA PDCstream specific channel frame events, if any have been subscribed
            if (frame is null || ReceivedDataFrame is null && ReceivedConfigurationFrame is null)
                return;

            switch (frame)
            {
                case DataFrame dataFrame:
                {
                    ReceivedDataFrame?.Invoke(this, new EventArgs<DataFrame>(dataFrame));
                    break;
                }
                case ConfigurationFrame configFrame:
                {
                    ReceivedConfigurationFrame?.Invoke(this, new EventArgs<ConfigurationFrame>(configFrame));
                    break;
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Attempts to cast given frame into a BPA PDCstream configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame, string configurationFileName)
        {
            // See if frame is already a BPA PDCstream configuration frame (if so, we don't need to do any work)
            if (sourceFrame is ConfigurationFrame derivedFrame)
                return derivedFrame;

            // Create a new BPA PDCstream configuration frame converted from equivalent configuration information
            derivedFrame = new ConfigurationFrame(sourceFrame.Timestamp, configurationFileName, 1, RevisionNumber.Revision2, StreamType.Compact);

            foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
            {
                // Create new derived configuration cell
                ConfigurationCell derivedCell = new ConfigurationCell(derivedFrame, sourceCell.IDCode, sourceCell.NominalFrequency);

                // Create equivalent derived phasor definitions
                foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                    derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.Offset, sourcePhasor.PhasorType, null));

                // Create equivalent derived frequency definition
                IFrequencyDefinition sourceFrequency = sourceCell.FrequencyDefinition;

                if (sourceFrequency is not null)
                    derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency.Label);

                // Create equivalent derived analog definitions (assuming analog type = SinglePointOnWave)
                foreach (IAnalogDefinition sourceAnalog in sourceCell.AnalogDefinitions)
                    derivedCell.AnalogDefinitions.Add(new AnalogDefinition(derivedCell, sourceAnalog.Label, sourceAnalog.ScalingValue, sourceAnalog.Offset, sourceAnalog.AnalogType));

                // Create equivalent derived digital definitions
                foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                    derivedCell.DigitalDefinitions.Add(new DigitalDefinition(derivedCell, sourceDigital.Label));

                // Add cell to frame
                derivedFrame.Cells.Add(derivedCell);
            }

            return derivedFrame;
        }

        #endregion
    }
}