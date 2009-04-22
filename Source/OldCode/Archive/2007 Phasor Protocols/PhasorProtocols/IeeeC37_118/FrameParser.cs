//*******************************************************************************************************
//  FrameParser.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using PCS.Parsing;

namespace PCS.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents a frame parser for an IEEE C37.118 binary data stream and returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<FrameType>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="ConfigurationFrame"/> type 1 has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ConfigurationFrame1>> ReceivedConfigurationFrame1;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="ConfigurationFrame"/> type 2 has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ConfigurationFrame1>> ReceivedConfigurationFrame2;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="HeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="HeaderFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<HeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an IEEE C37.118 <see cref="CommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="CommandFrame"/> that was received.
        /// <para>
        /// Command frames are normally sent, not received, but there is nothing that prevents this.
        /// </para>
        /// </remarks>
        public new event EventHandler<EventArgs<CommandFrame>> ReceivedCommandFrame;

        // Fields
        private DraftRevision m_draftRevision;
        private ConfigurationFrame2 m_configurationFrame2;
        private bool m_configurationChangeHandled;

        #endregion

        #region [ Constructors ]

        public FrameParser(DraftRevision draftRevision)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new byte[] { PhasorProtocols.Common.SyncByte };

            m_draftRevision = draftRevision;
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
                return m_configurationFrame2;
            }
            set
            {
                m_configurationFrame2 = CastToDerivedConfigurationFrame(value, m_draftRevision);
            }
        }

        /// <summary>
        /// Gets the <see cref="IeeeC37_118.DraftRevision"/> of this <see cref="FrameParser"/>.
        /// </summary>
        public DraftRevision DraftRevision
        {
            get
            {
                return m_draftRevision;
            }
            set
            {
                m_draftRevision = value;
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 resolution of fractional timestamps of the current <see cref="ConfigurationFrame"/>, if one has been parsed.
        /// </summary>
        public uint Timebase
        {
            get
            {
                if (m_configurationFrame2 == null)
                    return 0;
                else
                    return m_configurationFrame2.Timebase;
            }
        }

        /// <summary>
        /// Gets flag that determines if this protocol parsing implementation uses synchronization bytes.
        /// </summary>
        public override bool ProtocolUsesSyncBytes
        {
            get
            {
                return true;
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

                status.Append("IEEEC37.118 draft revision: ");
                status.Append(m_draftRevision);
                status.AppendLine();
                status.Append("         Current time base: ");
                status.Append(Timebase);
                status.AppendLine();
                status.Append(base.Status);

                return status.ToString();
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

            // We narrow down parsing types to just those needed...
            if (m_draftRevision == DraftRevision.Draft7)
                base.Start(new Type[] { typeof(DataFrame), typeof(ConfigurationFrame1), typeof(ConfigurationFrame2), typeof(HeaderFrame) });
            else
                base.Start(new Type[] { typeof(DataFrame), typeof(ConfigurationFrame1Draft6), typeof(ConfigurationFrame2Draft6), typeof(HeaderFrame) });
        }

        /// <summary>
        /// Parses a common header instance that implements <see cref="ICommonHeader{TTypeIdentifier}"/> for the output type represented
        /// in the binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing data to parse.</param>
        /// <param name="offset">Offset index into buffer that represents where to start parsing.</param>
        /// <param name="length">Maximum length of valid data from offset.</param>
        /// <param name="commonHeader">The <see cref="ICommonHeader{TTypeIdentifier}"/> which includes a type ID for the <see cref="Type"/> to be parsed.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// <para>
        /// Derived classes need to provide a common header instance (i.e., class that implements <see cref="ICommonHeader{TTypeIdentifier}"/>) for
        /// the output types via the <paramref name="commonHeader"/> parameter; this will primarily include an ID of the <see cref="Type"/> that the
        /// data image represents.  This parsing is only for common header information, actual parsing will be handled by output type via its
        /// <see cref="ISupportBinaryImage.Initialize"/> method. This header image should also be used to add needed complex state information
        /// about the output type being parsed if needed.
        /// </para>
        /// <para>
        /// This function should return total number of bytes that were parsed from the buffer. Consumers can choose to return "zero" if the output
        /// type <see cref="ISupportBinaryImage.Initialize"/> implementation expects the entire buffer image, however it will be optimal if
        /// the ParseCommonHeader method parses the header, and the Initialize method only parses the body of the image.
        /// </para>
        /// <para>
        /// If there is not enough buffer available to parse common header (as determined by <paramref name="length"/>), set <paramref name="commonHeader"/>
        /// to null and return 0.  If the protocol allows frame length to be determined at the time common header is being parsed and there is not enough
        /// buffer to parse the entire frame, it will be optimal to prevent further parsing by executing the same action, that is set
        /// <paramref name="commonHeader"/> = null and return 0.
        /// </para>
        /// </remarks>
        protected override int ParseCommonHeader(byte[] buffer, int offset, int length, out ICommonHeader<FrameType> commonHeader)
        {
            // See if there is enough data in the buffer to parse the common frame header.
            if (length >= CommonFrameHeader.FixedLength)
            {
                // Parse common frame header
                CommonFrameHeader parsedFrameHeader = new CommonFrameHeader(m_configurationFrame2, buffer, offset);

                // As an optimization, we also make sure entire frame buffer image is available to be parsed - by doing this
                // we eliminate the need to validate length on all subsequent data elements that comprise the frame
                if (length >= parsedFrameHeader.FrameLength)
                {
                    // Expose the frame buffer image in case client needs this data for any reason
                    OnReceivedFrameBufferImage(parsedFrameHeader.FrameType, buffer, offset, parsedFrameHeader.FrameLength);
                    commonHeader = parsedFrameHeader as ICommonHeader<FrameType>;

                    // Handle special parsing states
                    switch (parsedFrameHeader.TypeID)
                    {
                        case FrameType.DataFrame:
                            // Assign data frame parsing state
                            parsedFrameHeader.State = new DataFrameParsingState(parsedFrameHeader.FrameLength, m_configurationFrame2, DataCell.CreateNewCell);
                            break;
                        case FrameType.ConfigurationFrame2:
                            // Assign configuration frame parsing state
                            parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell.CreateNewCell);
                            break;
                        case FrameType.ConfigurationFrame1:
                            // Assign configuration frame parsing state
                            parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell.CreateNewCell);
                            break;
                        case FrameType.HeaderFrame:
                            // Assign header frame parsing state
                            parsedFrameHeader.State = new HeaderFrameParsingState(parsedFrameHeader.FrameLength, parsedFrameHeader.DataLength);
                            break;
                    }

                    return CommonFrameHeader.FixedLength;
                }
            }

            commonHeader = null;
            return 0;
        }

        /// <summary>
        /// Raises the <see cref="ReceivedDataFrame"/> event.
        /// </summary>
        /// <param name="frame"><see cref="IDataFrame"/> to send to <see cref="ReceivedDataFrame"/> event.</param>
        protected override void OnReceivedDataFrame(IDataFrame frame)
        {
            // We override this method so we can detect and respond to a configuration change notification
 	        base.OnReceivedDataFrame(frame);

            bool configurationChangeDetected = false;

            DataCellCollection dataCells = frame.Cells as DataCellCollection;

            if (dataCells != null)
            {
                // Check for a configuration change notification from any data cell
                for (int x = 0; x < dataCells.Count; x++)
                {
                    if (dataCells[x].ConfigurationChangeDetected)
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
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/>, <see cref="ConfigurationFrame"/>, <see cref="CommandFrame"/> or <see cref="HeaderFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise IEEE C37.118 specific channel frame events, if any have been subscribed
            if (frame != null && (ReceivedDataFrame != null || ReceivedConfigurationFrame2 != null || ReceivedConfigurationFrame1 != null || ReceivedHeaderFrame != null || ReceivedCommandFrame != null))
            {
                DataFrame dataFrame = frame as DataFrame;

                if (dataFrame != null)
                {
                    if (ReceivedDataFrame != null)
                        ReceivedDataFrame(this, new EventArgs<DataFrame>(dataFrame));
                }
                else
                {
                    ConfigurationFrame1 configFrame = frame as ConfigurationFrame1;

                    if (configFrame != null)
                    {
                        // Distinguish config frame type 1 from type 2
                        switch (configFrame.TypeID)
                        {
                            case FrameType.ConfigurationFrame2:
                                if (ReceivedConfigurationFrame2 != null)
                                    ReceivedConfigurationFrame2(this, new EventArgs<ConfigurationFrame1>(configFrame));
                                break;
                            case FrameType.ConfigurationFrame1:
                                if (ReceivedConfigurationFrame1 != null)
                                    ReceivedConfigurationFrame1(this, new EventArgs<ConfigurationFrame1>(configFrame));
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        HeaderFrame headerFrame = frame as HeaderFrame;

                        if (headerFrame != null)
                        {
                            if (ReceivedHeaderFrame != null)
                                ReceivedHeaderFrame(this, new EventArgs<HeaderFrame>(headerFrame));
                        }
                        else
                        {
                            CommandFrame commandFrame = frame as CommandFrame;

                            if (commandFrame != null)
                            {
                                if (ReceivedCommandFrame != null)
                                    ReceivedCommandFrame(this, new EventArgs<CommandFrame>(commandFrame));
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Attempts to cast given frame into an IEEE C37.118 configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame2 CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame, DraftRevision draftRevision)
        {
            // See if frame is already an IEEE C37.118 configuration frame, type 2 (if so, we don't need to do any work)
            ConfigurationFrame2 derivedFrame = sourceFrame as ConfigurationFrame2;

            if (derivedFrame == null)
            {
                // Create a new IEEE C37.118 configuration frame converted from equivalent configuration information
                ConfigurationCell derivedCell;
                IFrequencyDefinition sourceFrequency;
                
                // Assuming configuration frame 2 and timebase = 100000
                if (draftRevision == DraftRevision.Draft7)
                    derivedFrame = new ConfigurationFrame2(100000, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);
                else
                    derivedFrame = new ConfigurationFrame2Draft6(100000, sourceFrame.IDCode, sourceFrame.Timestamp, sourceFrame.FrameRate);

                foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
                {
                    // Create new derived configuration cell
                    derivedCell = new ConfigurationCell(derivedFrame, sourceCell.IDCode, sourceCell.NominalFrequency);

                    // Create equivalent derived phasor definitions
                    foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                    {
                        derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.Offset, sourcePhasor.Type, null));
                    }

                    // Create equivalent dervied frequency definition
                    sourceFrequency = sourceCell.FrequencyDefinition;

                    if (sourceFrequency != null)
                        derivedCell.FrequencyDefinition = new FrequencyDefinition(derivedCell, sourceFrequency.Label);

                    // Create equivalent dervied analog definitions (assuming analog type = SinglePointOnWave)
                    foreach (IAnalogDefinition sourceAnalog in sourceCell.AnalogDefinitions)
                    {
                        derivedCell.AnalogDefinitions.Add(new AnalogDefinition(derivedCell, sourceAnalog.Label, sourceAnalog.ScalingValue, sourceAnalog.Offset, AnalogType.SinglePointOnWave));
                    }

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