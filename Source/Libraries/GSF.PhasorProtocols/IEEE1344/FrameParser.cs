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
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Parsing;

namespace GSF.PhasorProtocols.IEEE1344
{
    /// <summary>
    /// Represents a frame parser for an IEEE 1344 binary data stream that returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<FrameType>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when an IEEE 1344 <see cref="ConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when an IEEE 1344 <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when an IEEE 1344 <see cref="HeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="HeaderFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<HeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an IEEE 1344 <see cref="CommandFrame"/> has been received.
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
        private FrameImageCollector m_configurationFrameImages;
        private FrameImageCollector m_headerFrameImages;

        private ushort m_lastSampleCount;
        private ushort m_samplesPerFrame;

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
            m_samplesPerFrame = ushort.MaxValue;
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
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        /// <remarks>
        /// The IEEE 1344 protocol does not use synchronization bytes, as a result this property returns <c>false</c>.
        /// </remarks>
        public override bool ProtocolUsesSyncBytes => false; // IEEE 1344 doesn't use synchronization bytes

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Start the data parser.
        /// </summary>
        public override void Start()
        {
            // We narrow down parsing types to just those needed...
            base.Start(new[] { typeof(DataFrame), typeof(ConfigurationFrame), typeof(HeaderFrame) });
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
            // Note that in order to get status flags (which contain frame length), we need at least two more bytes
            if (length < CommonFrameHeader.FixedLength + 2)
                return null;

            // Parse common frame header
            CommonFrameHeader parsedFrameHeader = new(buffer, offset);

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
                    // Assign configuration frame parsing state (note that IEEE 1344 only supports a single device, hence 1 cell)
                    parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell.CreateNewCell, TrustHeaderLength, ValidateConfigurationFrameCheckSum, 1);

                    // Cumulate configuration frame images...
                    CumulateFrameImage(parsedFrameHeader, buffer, offset, ref m_configurationFrameImages);
                    break;
                case FrameType.HeaderFrame:
                    // Assign header frame parsing state
                    parsedFrameHeader.State = new HeaderFrameParsingState(parsedFrameHeader.FrameLength, parsedFrameHeader.DataLength, TrustHeaderLength, ValidateHeaderFrameCheckSum);

                    // Cumulate header frame images...
                    CumulateFrameImage(parsedFrameHeader, buffer, offset, ref m_headerFrameImages);
                    break;
            }

            return parsedFrameHeader;
        }

        /// <summary>
        /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IConfigurationFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedConfigurationFrame"/> event.</param>
        protected override void OnReceivedConfigurationFrame(IConfigurationFrame frame)
        {
            // IEEE 1344 configuration frames can span multiple frame images, so we don't allow base class to raise this event until all frames have been assembled...
            if (frame is not ISupportFrameImage<FrameType> frameImage)
                return;

            if (frameImage.CommonHeader is not CommonFrameHeader commonHeader || !commonHeader.IsLastFrame)
                return;

            base.OnReceivedConfigurationFrame(frame);

            // Cache new configuration frame for parsing subsequent data frames...
            if (frame is ConfigurationFrame configurationFrame)
                m_configurationFrame = configurationFrame;
        }

        /// <summary>
        /// Raises the <see cref="FrameParserBase{TypeIndentifier}.ReceivedHeaderFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IHeaderFrame"/> to send to <see cref="FrameParserBase{TypeIndentifier}.ReceivedHeaderFrame"/> event.</param>
        protected override void OnReceivedHeaderFrame(IHeaderFrame frame)
        {
            // IEEE 1344 header frames can span multiple frame images, so we don't allow base class to raise this event until all frames have been assembled...
            if (frame is not ISupportFrameImage<FrameType> frameImage)
                return;

            if (frameImage.CommonHeader is CommonFrameHeader { IsLastFrame: true })
                base.OnReceivedHeaderFrame(frame);
        }

        /// <summary>
        /// Raises the <see cref="FrameParserBase{TFrameIdentifier}.ReceivedDataFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IDataFrame"/> to send to <see cref="FrameParserBase{TFrameIdentifier}.ReceivedDataFrame"/> event.</param>
        protected override void OnReceivedDataFrame(IDataFrame frame)
        {
            if (frame is DataFrame dataFrame)
            {
                // Keep track of the minimum number of samples between
                // data frames as the number of samples per frame
                ushort sampleCount = dataFrame.CommonHeader.SampleCount;
                ushort diff = (ushort)(sampleCount - m_lastSampleCount);

                if (diff != 0 && diff < m_samplesPerFrame)
                    m_samplesPerFrame = diff;

                m_lastSampleCount = sampleCount;

                // Calculate subsecond time information based on sample count,
                // then add this fraction of time to current seconds value
                if (dataFrame.ConfigurationFrame is not null)
                {
                    int frameIndex = sampleCount / m_samplesPerFrame;
                    long subsecondTicks = frameIndex * Ticks.PerSecond / dataFrame.ConfigurationFrame.FrameRate;
                    dataFrame.CommonHeader.Timestamp += subsecondTicks;
                }
            }

            base.OnReceivedDataFrame(frame);
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/>, <see cref="ConfigurationFrame"/>, <see cref="CommandFrame"/> or <see cref="HeaderFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise IEEE 1344 specific channel frame events, if any have been subscribed
            if (frame is null || ReceivedDataFrame is null && ReceivedConfigurationFrame is null && ReceivedHeaderFrame is null && ReceivedCommandFrame is null)
                return;

            switch (frame)
            {
                case DataFrame dataFrame:
                {
                    ReceivedDataFrame?.Invoke(this, new EventArgs<DataFrame>(dataFrame));
                    break;
                }
                case ConfigurationFrame configFrame when configFrame.CommonHeader.IsLastFrame:
                {
                    ReceivedConfigurationFrame?.Invoke(this, new EventArgs<ConfigurationFrame>(configFrame));
                    break;
                }
                case HeaderFrame headerFrame when headerFrame.CommonHeader.IsLastFrame:
                {
                    ReceivedHeaderFrame?.Invoke(this, new EventArgs<HeaderFrame>(headerFrame));
                    break;
                }
                case CommandFrame commandFrame:
                {
                    ReceivedCommandFrame?.Invoke(this, new EventArgs<CommandFrame>(commandFrame));
                    break;
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Cumulates frame images
        internal static void CumulateFrameImage(CommonFrameHeader parsedFrameHeader, byte[] buffer, int offset, ref FrameImageCollector frameImages)
        {
            // If this is the first frame, cumulate all partial frames together as one complete frame
            if (parsedFrameHeader.IsFirstFrame)
                frameImages = new FrameImageCollector();

            try
            {
                // Append next frame image
                frameImages.AppendFrameImage(buffer, offset, parsedFrameHeader.FrameLength);
            }
            catch
            {
                // Stop accumulation if CRC check fails
                frameImages = null;
                throw;
            }

            // Store a reference to frame image collection in common header state so configuration frame
            // can be parsed from entire combined binary image collection when last frame is received
            parsedFrameHeader.FrameImages = frameImages;

            // Clear local reference to frame image collection if this is the last frame
            if (parsedFrameHeader.IsLastFrame)
                frameImages = null;
        }

        // Attempts to cast given frame into an IEEE 1344 configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame)
        {
            // See if frame is already an IEEE 1344 configuration frame (if so, we don't need to do any work)
            if (sourceFrame is ConfigurationFrame derivedFrame)
                return derivedFrame;

            // Create a new IEEE 1344 configuration frame converted from equivalent configuration information
            derivedFrame = new ConfigurationFrame(sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);

            foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
            {
                // Create new derived configuration cell
                ConfigurationCell derivedCell = new(derivedFrame, sourceCell.IDCode, sourceCell.NominalFrequency);

                // Create equivalent derived phasor definitions
                foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                    derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.Offset, sourcePhasor.PhasorType, null));

                // Create equivalent derived frequency definition
                IFrequencyDefinition sourceFrequency = sourceCell.FrequencyDefinition;

                if (sourceFrequency is not null)
                    derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency.Label);

                // IEEE 1344 does not define analog values...

                // Create equivalent derived digital definitions
                foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                    derivedCell.DigitalDefinitions.Add(new DigitalDefinition(derivedCell, sourceDigital.Label));

                // Add cell to frame
                derivedFrame.Cells.Add(derivedCell);

                // IEEE-1344 only supports one cell
                break;
            }

            return derivedFrame;
        }

        #endregion
    }
}