//*******************************************************************************************************
//  FrameParser.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/27/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using TVA;
using TVA.Parsing;

namespace PhasorProtocols.SelFastMessage
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
        private MessagePeriod m_messagePeriod;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParser"/>.
        /// </summary>
        public FrameParser()
            : this(MessagePeriod.DefaultRate)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrameParser"/> from specified parameters.
        /// </summary>
        /// <param name="messagePeriod">The desired <see cref="SelFastMessage.MessagePeriod"/> for SEL device connection.</param>
        public FrameParser(MessagePeriod messagePeriod)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new byte[] { Common.HeaderByte1, Common.HeaderByte2 };

            m_messagePeriod = messagePeriod;
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
            get
            {
                return m_configurationFrame;
            }
            set
            {
                m_configurationFrame = CastToDerivedConfigurationFrame(value, m_messagePeriod);
            }
        }

        /// <summary>
        /// Gets flag that determines if SEL Fast Message protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes
        {
	        get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the desired message period for the SEL device.
        /// </summary>
        public MessagePeriod MessagePeriod
        {
            get
            {
                return m_messagePeriod;
            }
            set
            {
                m_messagePeriod = value;
            }
        }

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParser"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("    Defined message period: ");
                status.Append(m_messagePeriod.ToString());
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
            get
            {
                return base.ConnectionParameters;
            }
            set
            {
                SelFastMessage.ConnectionParameters parameters = value as SelFastMessage.ConnectionParameters;

                if (parameters != null)
                {
                    base.ConnectionParameters = parameters;

                    // Assign new incoming connection parameter values
                    m_messagePeriod = parameters.MessagePeriod;
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
            base.Start(new Type[] { typeof(DataFrame) });
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
        /// only for common header information, actual parsing will be handled by output type via its <see cref="ISupportBinaryImage.Initialize"/>
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
                
                // We also make sure entire frame buffer image is available to be parsed
                if (length >= (int)parsedFrameHeader.FrameSize)
                {
                    // Create configuration frame if it doesn't exist or frame size has changed
                    if (m_configurationFrame == null || m_configurationFrame.FrameSize != parsedFrameHeader.FrameSize)
                    {
                        // Create virtual configuration frame
                        m_configurationFrame = new ConfigurationFrame(parsedFrameHeader.FrameSize, m_messagePeriod, parsedFrameHeader.IDCode);

                        // Notify clients of new configuration frame
                        OnReceivedChannelFrame(m_configurationFrame);
                    }

                    if (m_configurationFrame != null)
                    {
                        int parsedLength = (int)parsedFrameHeader.FrameSize;

                        // Assign common header and data frame parsing state
                        parsedFrameHeader.State = new DataFrameParsingState(parsedLength, m_configurationFrame, DataCell.CreateNewCell);

                        // Expose the frame buffer image in case client needs this data for any reason
                        OnReceivedFrameBufferImage(FundamentalFrameType.DataFrame, buffer, offset, parsedLength);

                        return parsedFrameHeader;
                    }
                }
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

            // Raise SEL Fast Message specific channel frame events, if any have been subscribed
            if (frame != null && (ReceivedDataFrame != null || ReceivedConfigurationFrame != null))
            {
                DataFrame dataFrame = frame as DataFrame;

                if (dataFrame != null)
                {
                    if (ReceivedDataFrame != null)
                        ReceivedDataFrame(this, new EventArgs<DataFrame>(dataFrame));
                }
                else
                {
                    ConfigurationFrame configFrame = frame as ConfigurationFrame;

                    if (configFrame != null)
                    {
                        if (ReceivedConfigurationFrame != null)
                            ReceivedConfigurationFrame(this, new EventArgs<ConfigurationFrame>(configFrame));
                    }
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

            if (derivedFrame == null)
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

                    // Create equivalent dervied frequency definition
                    sourceFrequency = sourceCell.FrequencyDefinition;

                    if (sourceFrequency != null)
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