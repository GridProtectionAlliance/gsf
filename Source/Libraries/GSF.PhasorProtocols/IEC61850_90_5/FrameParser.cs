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
//  04/19/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text;
using GSF.Parsing;

namespace GSF.PhasorProtocols.IEC61850_90_5
{
    /// <summary>
    /// Represents a frame parser for an IEC 61850-90-5 binary data stream and returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<FrameType>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when an IEC 61850-90-5 <see cref="ConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when an IEC 61850-90-5 <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when an IEC 61850-90-5 <see cref="CommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="CommandFrame"/> that was received.
        /// <para>
        /// Command frames are normally sent, not received, but there is nothing that prevents this.
        /// </para>
        /// </remarks>
        public new event EventHandler<EventArgs<CommandFrame>> ReceivedCommandFrame;

        // Fields
        private ConfigurationFrame m_configurationFrame;
        private bool m_configurationChangeHandled;
        private long m_unexpectedCommandFrames;
        private bool m_useETRConfiguration;
        private bool m_guessConfiguration;
        private bool m_parseRedundantASDUs;
        private bool m_ignoreSignatureValidationFailures;
        private bool m_ignoreSampleSizeValidationFailures;
        private AngleFormat m_phasorAngleFormat;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParser"/>.
        /// </summary>
        /// <param name="checkSumValidationFrameTypes">Frame types that should perform check-sum validation; default to <see cref="CheckSumValidationFrameTypes.AllFrames"/></param>
        /// <param name="trustHeaderLength">Determines if header lengths should be trusted over parsed byte count.</param>
        public FrameParser(CheckSumValidationFrameTypes checkSumValidationFrameTypes = CheckSumValidationFrameTypes.AllFrames, bool trustHeaderLength = true)
            : base(checkSumValidationFrameTypes, trustHeaderLength)
        {
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
            set => m_configurationFrame = CastToDerivedConfigurationFrame(value);
        }

        /// <summary>
        /// Gets the IEC 61850-90-5 resolution of fractional timestamps of the current <see cref="ConfigurationFrame"/>, if one has been parsed.
        /// </summary>
        public uint Timebase => 
            m_configurationFrame?.Timebase ?? (uint)0;

        /// <summary>
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        // Since this implementation can parse both IEEE C37.118 configuration frames and IEC 61850-90-5 data frames, there is no common sync byte
        public override bool ProtocolUsesSyncBytes => false;

        /// <summary>
        /// Gets the number of redundant frames in each packet.
        /// </summary>
        /// <remarks>
        /// This value is used when calculating statistics. It is assumed that for each
        /// frame that is received, that frame will be included in the next <c>n</c>
        /// packets, where <c>n</c> is the number of redundant frames per packet.
        /// </remarks>
        public override int RedundantFramesPerPacket
        {
            get
            {
                if (m_configurationFrame is null || !m_configurationFrame.CommonHeader.ParseRedundantASDUs)
                    return base.RedundantFramesPerPacket;

                return m_configurationFrame.CommonHeader.AsduCount - 1;
            }
        }

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParser"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new();

                status.Append("         Current time base: ");
                status.Append(Timebase);
                status.AppendLine();
                status.Append("     Use ETR configuration: ");
                status.Append(m_useETRConfiguration);
                status.AppendLine();
                status.Append("       Guess configuration: ");
                status.Append(m_guessConfiguration);
                status.AppendLine();
                status.Append("     Parse redundant ASDUs: ");
                status.Append(m_parseRedundantASDUs);
                status.AppendLine();
                status.Append("Bypass checksum validation: ");
                status.Append(m_ignoreSignatureValidationFailures);
                status.AppendLine();
                status.Append("Bypass data len validation: ");
                status.Append(m_ignoreSampleSizeValidationFailures);
                status.AppendLine();
                status.Append(" Unexpected command frames: ");
                status.Append(m_unexpectedCommandFrames);
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
                if (value is ConnectionParameters parameters)
                {
                    base.ConnectionParameters = parameters;

                    // Assign new incoming connection parameter values
                    m_useETRConfiguration = parameters.UseETRConfiguration;
                    m_guessConfiguration = parameters.GuessConfiguration;
                    m_parseRedundantASDUs = parameters.ParseRedundantASDUs;
                    m_ignoreSignatureValidationFailures = parameters.IgnoreSignatureValidationFailures;
                    m_ignoreSampleSizeValidationFailures = parameters.IgnoreSampleSizeValidationFailures;
                    m_phasorAngleFormat = parameters.PhasorAngleFormat;
                }
                else
                {
                    m_useETRConfiguration = IEC61850_90_5.ConnectionParameters.DefaultUseETRConfiguration;
                    m_guessConfiguration = IEC61850_90_5.ConnectionParameters.DefaultGuessConfiguration;
                    m_parseRedundantASDUs = IEC61850_90_5.ConnectionParameters.DefaultParseRedundantASDUs;
                    m_ignoreSignatureValidationFailures = IEC61850_90_5.ConnectionParameters.DefaultIgnoreSignatureValidationFailures;
                    m_ignoreSampleSizeValidationFailures = IEC61850_90_5.ConnectionParameters.DefaultIgnoreSampleSizeValidationFailures;
                    m_phasorAngleFormat = (AngleFormat)Enum.Parse(typeof(AngleFormat), IEC61850_90_5.ConnectionParameters.DefaultPhasorAngleFormat, true);
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Start the data parser.
        /// </summary>
        public override void Start()
        {
            m_configurationChangeHandled = false;
            m_unexpectedCommandFrames = 0;

            //// We narrow down parsing types to just those needed...
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
            CommonFrameHeader parsedFrameHeader = new(m_configurationFrame, m_useETRConfiguration, m_guessConfiguration, m_parseRedundantASDUs, m_ignoreSignatureValidationFailures, m_ignoreSampleSizeValidationFailures, m_phasorAngleFormat, buffer, offset, length)
            {
                PublishFrame = OnReceivedChannelFrame
            };

            // As an optimization, we also make sure entire frame buffer image is available to be parsed - by doing this
            // we eliminate the need to validate length on all subsequent data elements that comprise the frame
            if (length >= parsedFrameHeader.FrameLength)
            {
                // Expose the frame buffer image in case client needs this data for any reason
                OnReceivedFrameBufferImage(parsedFrameHeader.FrameType, buffer, offset, parsedFrameHeader.FrameLength);

                // Handle special parsing states
                switch (parsedFrameHeader.TypeID)
                {
                    case FrameType.DataFrame:
                        // Assign data frame parsing state                            
                        DataFrameParsingState parsingState = new(parsedFrameHeader.FrameLength, m_configurationFrame, DataCell.CreateNewCell, TrustHeaderLength, ValidateDataFrameCheckSum);

                        // Assume one device if no configuration frame is available
                        if (m_configurationFrame is null)
                            parsingState.CellCount = 1;

                        parsedFrameHeader.State = parsingState;
                        break;
                    case FrameType.ConfigurationFrame:
                        // Assign configuration frame parsing state
                        parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell.CreateNewCell, TrustHeaderLength, ValidateConfigurationFrameCheckSum);
                        break;
                }

                return parsedFrameHeader;
            }

            return null;
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
        /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedDataFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IDataFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedDataFrame"/> event.</param>
        protected override void OnReceivedDataFrame(IDataFrame frame)
        {
            // We override this method so we can detect and respond to a configuration change notification
            base.OnReceivedDataFrame(frame);

            bool configurationChangeDetected = false;

            if (frame.Cells is DataCellCollection dataCells)
            {
                // Check for a configuration change notification from any data cell
                for (int x = 0; x < dataCells.Count; x++)
                {
                    // Configuration change is only relevant if raw status flags <> FF (status undefined)
                    if (dataCells[x].ConfigurationChangeDetected && !dataCells[x].DeviceError && ((DataCellBase)dataCells[x]).StatusFlags != ushort.MaxValue)
                    {
                        configurationChangeDetected = true;

                        // Configuration change detection flag should terminate after one minute, but
                        // we only want to send a single notification
                        if (!m_configurationChangeHandled)
                        {
                            m_configurationChangeHandled = true;
                            base.OnConfigurationChanged();
                        }

                        break;
                    }
                }
            }

            if (!configurationChangeDetected)
                m_configurationChangeHandled = false;
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/>, <see cref="ConfigurationFrame"/> or <see cref="CommandFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise IEC 61850-90-5 specific channel frame events, if any have been subscribed
            if (frame is null || ReceivedDataFrame is null && ReceivedConfigurationFrame is null && ReceivedCommandFrame is null)
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
                case CommandFrame commandFrame:
                {
                    ReceivedCommandFrame?.Invoke(this, new EventArgs<CommandFrame>(commandFrame));
                    break;
                }
            }
        }

        /// <summary>
        /// Handles unknown frame types.
        /// </summary>
        /// <param name="frameType">Unknown frame ID.</param>
        protected override void OnUnknownFrameTypeEncountered(FrameType frameType)
        {
            // It is unusual, but not all that uncommon, for a device to send a command frame to its host. Normally the host sends to commands
            // to the device. However, since some devices seem to do this frequently we suppress reporting that the frame is "undefined".
            // Technically the frame is defined, but it is not part of the valid set of frames intended for reception.
            if (frameType != FrameType.CommandFrame)
                base.OnUnknownFrameTypeEncountered(frameType);
            else
                m_unexpectedCommandFrames++;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Attempts to cast given frame into an IEC 61850-90-5 configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame)
        {
            // See if frame is already an IEC 61850-90-5 configuration frame (if so, we don't need to do any work)
            if (sourceFrame is ConfigurationFrame derivedFrame)
                return derivedFrame;

            // Create a new IEC 61850-90-5 configuration frame converted from equivalent configuration information
            // Assuming timebase = 16777216
            derivedFrame = new ConfigurationFrame(Common.Timebase, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);

            foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
            {
                // Create new derived configuration cell
                ConfigurationCell derivedCell = new(derivedFrame, sourceCell.IDCode, sourceCell.NominalFrequency);

                string stationName = sourceCell.StationName;
                string idLabel = sourceCell.IDLabel;

                if (!string.IsNullOrWhiteSpace(stationName))
                    derivedCell.StationName = stationName.TruncateLeft(derivedCell.MaximumStationNameLength);

                if (!string.IsNullOrWhiteSpace(idLabel))
                    derivedCell.IDLabel = idLabel.TruncateLeft(derivedCell.IDLabelLength);

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
                    derivedCell.DigitalDefinitions.Add(new DigitalDefinition(derivedCell, sourceDigital.Label, 0, 0));

                // Add cell to frame
                derivedFrame.Cells.Add(derivedCell);
            }

            return derivedFrame;
        }

        #endregion
    }
}