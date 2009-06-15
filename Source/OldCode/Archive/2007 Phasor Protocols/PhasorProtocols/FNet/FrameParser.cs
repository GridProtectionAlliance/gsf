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
//  02/08/2007 - James R Carroll & Jian (Ryan) Zuo
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using TVA;
using TVA.Parsing;

namespace PhasorProtocols.FNet
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
        private Ticks m_timeOffset;
        private ushort m_frameRate;
        private LineFrequency m_nominalFrequency;
        private string m_stationName;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParser"/>.
        /// </summary>
        public FrameParser()
            : this(Common.DefaultFrameRate, Common.DefaultNominalFrequency, Common.DefaultTimeOffset, Common.DefaultStationName)
        {
            // FNet devices default to 10 frames per second, 60Hz and 11 second time offset
        }

        /// <summary>
        /// Creates a new <see cref="FrameParser"/> from specified parameters.
        /// </summary>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="timeOffset">The time offset of F-NET device in <see cref="Ticks"/>.</param>
        /// <param name="stationName">The station name of the F-NET device.</param>
        public FrameParser(ushort frameRate, LineFrequency nominalFrequency, Ticks timeOffset, string stationName)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new byte[] { Common.StartByte };

            m_frameRate = frameRate;
            m_nominalFrequency = nominalFrequency;
            m_timeOffset = timeOffset;
            m_stationName = stationName;
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
        /// Gets flag that determines if F-NET protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes
        {
	        get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets time offset of F-NET device in <see cref="Ticks"/>.
        /// </summary>
        /// <remarks>
        /// F-NET devices normally report time in 11 seconds past real-time, this property defines the offset for this this artificial delay.
        /// Note that the parameter value is in ticks to allow a very high-resolution offset;  1 second = 10000000 ticks.
        /// </remarks>
        public Ticks TimeOffset
        {
            get
            {
                return m_timeOffset;
            }
            set
            {
                m_timeOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the configured frame rate for the F-NET device.
        /// </summary>
        /// <remarks>
        /// This is typically set to 10 frames per second.
        /// </remarks>
        public ushort FrameRate
        {
            get
            {
                return m_frameRate;
            }
            set
            {
                m_frameRate = value;
            }
        }

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of the F-NET device.
        /// </summary>
        public LineFrequency NominalFrequency
        {
            get
            {
                return m_nominalFrequency;
            }
            set
            {
                m_nominalFrequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the station name for the F-NET device.
        /// </summary>
        public string StationName
        {
            get
            {
                return m_stationName;
            }
            set
            {
                m_stationName = value;
            }
        }

        /// <summary>
        /// Gets current descriptive status of the <see cref="FrameParser"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                if (m_configurationFrame == null)
                {
                    return base.Status;
                }
                else
                {
                    StringBuilder status = new StringBuilder();

                    status.Append("        Reported longitude: ");
                    status.Append(m_configurationFrame.Longitude);
                    status.Append('°');
                    status.AppendLine();
                    status.Append("         Reported latitude: ");
                    status.Append(m_configurationFrame.Latitude);
                    status.Append('°');
                    status.AppendLine();
                    status.Append("      Number of satellites:");
                    status.Append(m_configurationFrame.NumberOfSatellites);
                    status.AppendLine();
                    status.Append(base.Status);

                    return status.ToString();
                }
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
                FNet.ConnectionParameters parameters = value as FNet.ConnectionParameters;

                if (parameters != null)
                {
                    base.ConnectionParameters = parameters;

                    // Assign new incoming connection parameter values
                    m_timeOffset = parameters.TimeOffset;
                    m_frameRate = parameters.FrameRate;
                    m_nominalFrequency = parameters.NominalFrequency;
                    m_stationName = parameters.StationName;
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
            if (length > 0)
            {
                // Pre-parse F-NET data row...
                CommonFrameHeader parsedFrameHeader = new CommonFrameHeader(buffer, offset, length);

                // Create configuration frame if it doesn't exist
                if (m_configurationFrame == null)
                {
                    string[] data = parsedFrameHeader.DataElements;

                    // Create virtual configuration frame
                    m_configurationFrame = new ConfigurationFrame(ushort.Parse(data[Element.UnitID]), DateTime.Now.Ticks, m_frameRate, m_nominalFrequency, m_timeOffset, m_stationName);

                    // Notify clients of new configuration frame
                    OnReceivedChannelFrame(m_configurationFrame);
                }

                if (m_configurationFrame != null)
                {
                    int parsedLength = parsedFrameHeader.ParsedLength;

                    // Assign common header and data frame parsing state
                    parsedFrameHeader.State = new DataFrameParsingState(parsedLength, m_configurationFrame, DataCell.CreateNewCell);

                    // Expose the frame buffer image in case client needs this data for any reason
                    OnReceivedFrameBufferImage(FundamentalFrameType.DataFrame, buffer, offset, parsedLength);

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

            // Raise F-NET specific channel frame events, if any have been subscribed
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

        // Attempts to cast given frame into a F-NET configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame)
        {
            // See if frame is already a F-NET frame (if so, we don't need to do any work)
            ConfigurationFrame derivedFrame = sourceFrame as ConfigurationFrame;

            if (derivedFrame == null)
            {
                // Create a new F-NET configuration frame converted from equivalent configuration information; F-NET only supports one device
                if (sourceFrame.Cells.Count > 0)
                    derivedFrame = new ConfigurationFrame(sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate, sourceFrame.Cells[0].NominalFrequency, Common.DefaultTimeOffset, Common.DefaultStationName);
                else
                    derivedFrame = new ConfigurationFrame(sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate, LineFrequency.Hz60, Common.DefaultTimeOffset, Common.DefaultStationName);
            }

            return derivedFrame;
        }

        #endregion
    }
}