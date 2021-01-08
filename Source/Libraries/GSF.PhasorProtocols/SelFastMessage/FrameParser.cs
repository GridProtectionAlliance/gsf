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
//  04/27/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text;
using GSF.IO;
using GSF.Parsing;

namespace GSF.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents a frame parser for a SEL Fast Message binary data stream that returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<int>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a SEL Fast Message <see cref="ConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a SEL Fast Message <see cref="DataFrame"/> has been received.
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
        /// Creates a new <see cref="FrameParser"/> from specified parameters.
        /// </summary>
        /// <param name="checkSumValidationFrameTypes">Frame types that should perform check-sum validation; default to <see cref="GSF.PhasorProtocols.CheckSumValidationFrameTypes.AllFrames"/></param>
        /// <param name="trustHeaderLength">Determines if header lengths should be trusted over parsed byte count.</param>
        /// <param name="messagePeriod">The desired <see cref="SelFastMessage.MessagePeriod"/> for SEL device connection.</param>
        public FrameParser(CheckSumValidationFrameTypes checkSumValidationFrameTypes = CheckSumValidationFrameTypes.AllFrames, bool trustHeaderLength = true, MessagePeriod messagePeriod = MessagePeriod.DefaultRate)
            : base(checkSumValidationFrameTypes, trustHeaderLength)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new[] { Common.HeaderByte1, Common.HeaderByte2 };

            MessagePeriod = messagePeriod;
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
            set => m_configurationFrame = CastToDerivedConfigurationFrame(value, MessagePeriod);
        }

        /// <summary>
        /// Gets flag that determines if SEL Fast Message protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes => true;

        /// <summary>
        /// Gets or sets the desired message period for the SEL device.
        /// </summary>
        public MessagePeriod MessagePeriod { get; set; }

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParser"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("    Defined message period: ");
                status.Append(MessagePeriod);
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
                    MessagePeriod = parameters.MessagePeriod;
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
            // See if there is enough data in the buffer to parse the common frame header.
            if (length > CommonFrameHeader.FixedLength)
            {
                // Parse common frame header
                CommonFrameHeader parsedFrameHeader = new CommonFrameHeader(buffer, offset);

                // Derive frame length from common frame header
                int frameLength = (int)parsedFrameHeader.FrameSize;

                // We also make sure entire frame buffer image is available to be parsed
                if (length >= frameLength)
                {
                    // Create configuration frame if it doesn't exist or frame size has changed
                    if (m_configurationFrame is null || m_configurationFrame.FrameSize != parsedFrameHeader.FrameSize)
                    {
                        // Create virtual configuration frame
                        m_configurationFrame = new ConfigurationFrame(parsedFrameHeader.FrameSize, MessagePeriod, parsedFrameHeader.IDCode);

                        // Notify clients of new configuration frame
                        OnReceivedChannelFrame(m_configurationFrame);
                    }

                    if (m_configurationFrame is null)
                        return null;

                    // Assign common header and data frame parsing state
                    parsedFrameHeader.State = new DataFrameParsingState(frameLength, m_configurationFrame, DataCell.CreateNewCell, TrustHeaderLength, ValidateDataFrameCheckSum);

                    // Expose the frame buffer image in case client needs this data for any reason
                    OnReceivedFrameBufferImage(FundamentalFrameType.DataFrame, buffer, offset, frameLength);

                    return parsedFrameHeader;
                }
            }

            return null;
        }

        /// <summary>
        /// Queues a sequence of bytes, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">Identifier of the data source.</param>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the queue.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Parse(SourceChannel source, byte[] buffer, int offset, int count)
        {
            // When SEL Fast Message is transmitted over Ethernet it is embedded in a Telnet stream. As a result
            // any 0xFF will be encoded for Telnet compliance as a duplicate, i.e., 0xFF 0xFF. We remove these
            // duplications when encountered to make sure check-sums and parsing work as expected.
            int doubleFFPosition = buffer.IndexOfSequence(new byte[] { 0xFF, 0xFF }, offset, count);

            while (doubleFFPosition > -1)
            {
                using (BlockAllocatedMemoryStream newBuffer = new BlockAllocatedMemoryStream())
                {
                    // Write buffer before repeated byte
                    newBuffer.Write(buffer, offset, doubleFFPosition - offset + 1);

                    int nextByte = doubleFFPosition + 2;

                    // Write buffer after repeated byte, if any
                    if (nextByte < offset + count)
                        newBuffer.Write(buffer, nextByte, offset + count - nextByte);

                    buffer = newBuffer.ToArray();
                }

                offset = 0;
                count = buffer.Length;

                // Find next 0xFF 0xFF sequence
                doubleFFPosition = buffer.IndexOfSequence(new byte[] { 0xFF, 0xFF }, offset, count);
            }

            base.Parse(source, buffer, offset, count);
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/> or <see cref="ConfigurationFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise SEL Fast Message specific channel frame events, if any have been subscribed
            if (frame is null || (ReceivedDataFrame is null && ReceivedConfigurationFrame is null))
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

        // Attempts to cast given frame into a SEL Fast Message configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame, MessagePeriod messagePeriod)
        {
            // See if frame is already a SEL Fast Message frame (if so, we don't need to do any work)
            ConfigurationFrame derivedFrame = sourceFrame as ConfigurationFrame;

            if (derivedFrame is null)
            {
                // Create a new SEL Fast Message configuration frame converted from equivalent configuration information; SEL Fast Message only supports one device
                if (sourceFrame.Cells.Count > 0)
                {
                    IConfigurationCell sourceCell = sourceFrame.Cells[0];

                    switch (sourceCell.PhasorDefinitions.Count)
                    {
                        case 8:
                            derivedFrame = new ConfigurationFrame(FrameSize.A, messagePeriod, sourceFrame.IDCode);
                            break;
                        case 4:
                            derivedFrame = new ConfigurationFrame(FrameSize.V, messagePeriod, sourceFrame.IDCode);
                            break;
                        default:
                            derivedFrame = new ConfigurationFrame(FrameSize.V1, messagePeriod, sourceFrame.IDCode);
                            break;
                    }

                    // Create new derived configuration cell
                    ConfigurationCell derivedCell = new ConfigurationCell(derivedFrame);
                    IFrequencyDefinition sourceFrequency;

                    // Create equivalent derived phasor definitions
                    foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                    {
                        derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.PhasorType, null));
                    }

                    // Create equivalent derived frequency definition
                    sourceFrequency = sourceCell.FrequencyDefinition;

                    if (!(sourceFrequency is null))
                        derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency.Label);

                    // Add cell to frame
                    derivedFrame.Cells.Add(derivedCell);
                }
            }

            return derivedFrame;
        }

        #endregion
    }
}