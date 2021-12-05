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
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text;
using GSF.Parsing;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.FNET
{
    /// <summary>
    /// Represents a frame parser for a F-NET text based data stream that returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<int>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a F-NET <see cref="ConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a F-NET <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

        // Fields
        private ConfigurationFrame m_configurationFrame;

    #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParser"/>.
        /// </summary>
        /// <param name="checkSumValidationFrameTypes">Frame types that should perform check-sum validation; default to <see cref="GSF.PhasorProtocols.CheckSumValidationFrameTypes.AllFrames"/></param>
        /// <param name="trustHeaderLength">Determines if header lengths should be trusted over parsed byte count.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="timeOffset">The time offset of F-NET device in <see cref="Ticks"/>.</param>
        /// <param name="stationName">The station name of the F-NET device.</param>
        public FrameParser(CheckSumValidationFrameTypes checkSumValidationFrameTypes = CheckSumValidationFrameTypes.AllFrames, bool trustHeaderLength = true, ushort frameRate = Common.DefaultFrameRate, LineFrequency nominalFrequency = Common.DefaultNominalFrequency, long timeOffset = Common.DefaultTimeOffset, string stationName = Common.DefaultStationName)
            : base(checkSumValidationFrameTypes, trustHeaderLength)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new[] { Common.StartByte };

            FrameRate = frameRate;
            NominalFrequency = nominalFrequency;
            TimeOffset = timeOffset;
            StationName = stationName;
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
        /// Gets flag that determines if F-NET protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes => true;

        /// <summary>
        /// Gets or sets time offset of F-NET device in <see cref="Ticks"/>.
        /// </summary>
        /// <remarks>
        /// F-NET devices normally report time in 11 seconds past real-time, this property defines the offset for this artificial delay.
        /// Note that the parameter value is in ticks to allow a very high-resolution offset;  1 second = 10000000 ticks.
        /// </remarks>
        public Ticks TimeOffset { get; set; }

        /// <summary>
        /// Gets or sets the configured frame rate for the F-NET device.
        /// </summary>
        /// <remarks>
        /// This is typically set to 10 frames per second.
        /// </remarks>
        public ushort FrameRate { get; set; }

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of the F-NET device.
        /// </summary>
        public LineFrequency NominalFrequency { get; set; }

        /// <summary>
        /// Gets or sets the station name for the F-NET device.
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParser"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                if (m_configurationFrame is null)
                    return base.Status;

                StringBuilder status = new();

                status.Append("        Reported longitude: ");
                status.Append(m_configurationFrame.Longitude);
                status.Append('°');
                status.AppendLine();
                status.Append("         Reported latitude: ");
                status.Append(m_configurationFrame.Latitude);
                status.Append('°');
                status.AppendLine();
                status.Append("      Number of satellites: ");
                status.Append(m_configurationFrame.NumberOfSatellites);
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
                TimeOffset = parameters.TimeOffset;
                FrameRate = parameters.FrameRate;
                NominalFrequency = parameters.NominalFrequency;
                StationName = parameters.StationName;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Start the data parser.
        /// </summary>
        public override void Start()
        {
            // We narrow down parsing types to just those needed...
            base.Start(new[] { typeof(DataFrame) });
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
        protected override ICommonHeader<int> ParseCommonHeader(byte[] buffer, int offset, int length)
        {
            // Calculate a maximum reasonable scan size for the buffer
            int scanLength = length > Common.MaximumPracticalFrameSize ? Common.MaximumPracticalFrameSize : length;

            // See if there is enough data in the buffer to parse the common frame header by scanning for the F-NET termination byte
            if (scanLength > 0 && Array.IndexOf(buffer, Common.EndByte, offset, scanLength) >= 0)
            {
                // Pre-parse F-NET data row...
                CommonFrameHeader parsedFrameHeader = new(buffer, offset, length);

                int parsedLength = parsedFrameHeader.ParsedLength;

                if (parsedLength > 0)
                {
                    // Create configuration frame if it doesn't exist
                    if (m_configurationFrame is null)
                    {
                        string[] data = parsedFrameHeader.DataElements;

                        // Create virtual configuration frame
                        m_configurationFrame = new ConfigurationFrame(ushort.Parse(data[Element.UnitID]), DateTime.UtcNow.Ticks, FrameRate, NominalFrequency, TimeOffset, StationName);

                        // Notify clients of new configuration frame
                        OnReceivedChannelFrame(m_configurationFrame);
                    }

                    if (m_configurationFrame is null)
                        return null;

                    // Assign common header and data frame parsing state
                    parsedFrameHeader.State = new DataFrameParsingState(parsedLength, m_configurationFrame, DataCell.CreateNewCell, TrustHeaderLength, ValidateDataFrameCheckSum);

                    // Expose the frame buffer image in case client needs this data for any reason
                    OnReceivedFrameBufferImage(FundamentalFrameType.DataFrame, buffer, offset, parsedLength);

                    return parsedFrameHeader;
                }
            }
            else if (scanLength == Common.MaximumPracticalFrameSize)
            {
                throw new InvalidOperationException($"Possible bad F-NET data stream, scanned {Common.MaximumPracticalFrameSize:N0} bytes without finding an expected termination byte 0x0");
            }

            return null;
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/> or <see cref="ConfigurationFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise F-NET specific channel frame events, if any have been subscribed
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

        // Attempts to cast given frame into a F-NET configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame)
        {
            // See if frame is already a F-NET frame (if so, we don't need to do any work)
            if (sourceFrame is ConfigurationFrame derivedFrame)
                return derivedFrame;

            // Create a new F-NET configuration frame converted from equivalent configuration information; F-NET only supports one device
            derivedFrame = sourceFrame.Cells.Count > 0 ? 
                new ConfigurationFrame(sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate, sourceFrame.Cells[0].NominalFrequency, Common.DefaultTimeOffset, Common.DefaultStationName) : 
                new ConfigurationFrame(sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate, LineFrequency.Hz60, Common.DefaultTimeOffset, Common.DefaultStationName);

            return derivedFrame;
        }

        #endregion
    }
}