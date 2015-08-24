//*******************************************************************************************************
//  FrameParser.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/30/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using TVA.Parsing;

namespace TVA.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents a frame parser for a Macrodyne binary data stream that returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<int>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a Macrodyne <see cref="ConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a Macrodyne <see cref="DataFrame"/> has been received.
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
        public FrameParser()
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new byte[] { PhasorProtocols.Common.SyncByte };
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
                m_configurationFrame = CastToDerivedConfigurationFrame(value);
            }
        }

        /// <summary>
        /// Gets flag that determines if Macrodyne protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes
        {
	        get
            {
                return true;
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
            if (length >= CommonFrameHeader.FixedLength)
            {
                // Parse common frame header
                CommonFrameHeader parsedFrameHeader = new CommonFrameHeader(buffer, offset);

                // TODO: Make sure there is enough buffer available to parse entire data frame - otherwise return null...

                //// Create configuration frame if it doesn't exist or frame size has changed
                //if (m_configurationFrame == null)
                //{
                //    // Create virtual configuration frame
                //    m_configurationFrame = new ConfigurationFrame(OnlineDataFormatFlags.parsedFrameHeader.FrameSize, m_messagePeriod, parsedFrameHeader.IDCode);

                //    // Notify clients of new configuration frame
                //    OnReceivedChannelFrame(m_configurationFrame);
                //}

                if (m_configurationFrame != null)
                {
                    // Assign common header and data frame parsing state
                    parsedFrameHeader.State = new DataFrameParsingState(0, m_configurationFrame, DataCell.CreateNewCell);

                    // Expose the frame buffer image in case client needs this data for any reason
                    OnReceivedFrameBufferImage(FundamentalFrameType.DataFrame, buffer, offset, length);

                    return parsedFrameHeader;
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

            // Raise Macrodyne specific channel frame events, if any have been subscribed
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

        // Attempts to cast given frame into a Macrodyne configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame)
        {
            // See if frame is already a Macrodyne frame (if so, we don't need to do any work)
            ConfigurationFrame derivedFrame = sourceFrame as ConfigurationFrame;

            if (derivedFrame == null)
            {
                // Create a new Macrodyne configuration frame converted from equivalent configuration information; Macrodyne only supports one device
                if (sourceFrame.Cells.Count > 0)
                {
                    IConfigurationCell sourceCell = sourceFrame.Cells[0];
                    string stationName = sourceCell.StationName.TruncateLeft(8);

                    if (string.IsNullOrEmpty(stationName))
                        stationName = "Unit " + sourceCell.IDCode.ToString();

                    switch (sourceFrame.Cells[0].PhasorDefinitions.Count)
                    {
                        case 10:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor10Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        case 9:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor9Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        case 8:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor8Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        case 7:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor7Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        case 6:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor6Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        case 5:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor5Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        case 4:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor4Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        case 3:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor3Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        case 2:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.Phasor2Enabled | OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                        default:
                            derivedFrame = new ConfigurationFrame(OnlineDataFormatFlags.TimestampEnabled, stationName);
                            break;
                    }

                    derivedFrame.IDCode = sourceFrame.IDCode;
                    
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

                    // Create equivalent dervied digital definitions
                    foreach (IDigitalDefinition sourceDigital in sourceCell.DigitalDefinitions)
                    {
                        derivedCell.DigitalDefinitions.Add(new DigitalDefinition(derivedCell, sourceDigital.Label));
                    }

                    // Add cell to frame
                    derivedFrame.Cells.Add(derivedCell);
                }
            }

            return derivedFrame;
        }

        #endregion
    }
}