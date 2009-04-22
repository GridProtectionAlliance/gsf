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
using System.IO;
using System.Text;
using PCS.Parsing;
using PCS.IO;

namespace PCS.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents a frame parser for an BPA PDCstream binary data stream and returns parsed data via events.
    /// </summary>
    /// <remarks>
    /// Frame parser is implemented as a write-only stream - this way data can come from any source.
    /// </remarks>
    public class FrameParser : FrameParserBase<FrameType>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when an BPA PDCstream <see cref="ConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ConfigurationFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when an BPA PDCstream <see cref="DataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="DataFrame"/> that was received.
        /// </remarks>
        public new event EventHandler<EventArgs<DataFrame>> ReceivedDataFrame;

        // Fields
        private ConfigurationFrame m_configurationFrame;
        private string m_configurationFileName;
        private bool m_refreshConfigurationFileOnChange;
        private bool m_parseWordCountFromByte;
        private long m_lastUpdateNotification;
        private FileSystemWatcher m_configurationFileWatcher;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameParser"/> from specified parameters.
        /// </summary>
        /// <param name="configurationFileName"></param>
        public FrameParser(string configurationFileName)
        {
            // Initialize protocol synchronization bytes for this frame parser
            base.ProtocolSyncBytes = new byte[] { PhasorProtocols.Common.SyncByte };

            m_configurationFileName = configurationFileName;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="FrameParser"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~FrameParser()
        {
            Dispose(false);
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
                m_configurationFrame = CastToDerivedConfigurationFrame(value, m_configurationFileName);
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
        /// Gets or sets required external BPA PDCstream INI based configuration file.
        /// </summary>
        public string ConfigurationFileName
        {
            get
            {
                if (m_configurationFrame == null)
                    return m_configurationFileName;
                else
                    return m_configurationFrame.ConfigurationFileName;
            }
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
            get
            {
                return m_refreshConfigurationFileOnChange;
            }
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
        public bool ParseWordCountFromByte
        {
            get
            {
                return m_parseWordCountFromByte;
            }
            set
            {
                m_parseWordCountFromByte = value;
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

                status.Append("    INI configuration file: ");
                status.Append(m_configurationFileName);
                status.AppendLine();
                if (m_configurationFrame != null)
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
                status.Append(m_parseWordCountFromByte);
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
                BpaPdcStream.ConnectionParameters parameters = value as BpaPdcStream.ConnectionParameters;

                if (parameters != null)
                {
                    base.ConnectionParameters = parameters;

                    // Assign new incoming connection parameter values
                    m_configurationFileName = parameters.ConfigurationFileName;
                    m_parseWordCountFromByte = parameters.ParseWordCountFromByte;
                    m_refreshConfigurationFileOnChange = parameters.RefreshConfigurationFileOnChange;
                    ResetFileWatcher();
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="FrameParser"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FrameParser"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_configurationFileWatcher != null)
                        {
                            m_configurationFileWatcher.Changed -= m_configurationFileWatcher_Changed;
                            m_configurationFileWatcher.Dispose();
                        }
                        m_configurationFileWatcher = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Start the data parser.
        /// </summary>
        public override void Start()
        {
            // We narrow down parsing types to just those needed...
            base.Start(new Type[] { typeof(DataFrame), typeof(ConfigurationFrame) });
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
        protected override ICommonHeader<FrameType> ParseCommonHeader(byte[] buffer, int offset, int length)
        {
            // See if there is enough data in the buffer to parse the common frame header.
            if (length >= CommonFrameHeader.FixedLength)
            {
                // Parse common frame header
                CommonFrameHeader parsedFrameHeader = new CommonFrameHeader(m_configurationFrame, buffer, offset);

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
                            parsedFrameHeader.State = new DataFrameParsingState(parsedFrameHeader.FrameLength, m_configurationFrame, DataCell.CreateNewCell);
                            break;
                        case FrameType.ConfigurationFrame:
                            // Assign configuration frame parsing state
                            parsedFrameHeader.State = new ConfigurationFrameParsingState(parsedFrameHeader.FrameLength, ConfigurationCell.CreateNewCell);
                            break;
                    }

                    return parsedFrameHeader;
                }
            }

            return null;
        }

        // Handler for file watcher - we notify consumer when changes have occured to configuration file
        private void m_configurationFileWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            // File watcher sends several notifications for file change - we only want to report one,
            // so we ignore repeated file change notifications that occur within 1/2 a second
            if (m_lastUpdateNotification == 0 || Ticks.ToSeconds(DateTime.Now.Ticks - m_lastUpdateNotification) > 0.5)
            {
                if (m_configurationFrame != null)
                    m_configurationFrame.Refresh();

                OnConfigurationChanged();
            }

            m_lastUpdateNotification = DateTime.Now.Ticks;
        }

        // Reset file watcher
        private void ResetFileWatcher()
        {
            if (m_configurationFileWatcher != null)
            {
                m_configurationFileWatcher.Changed -= m_configurationFileWatcher_Changed;
                m_configurationFileWatcher.Dispose();
            }
            m_configurationFileWatcher = null;

            if (m_refreshConfigurationFileOnChange && !string.IsNullOrEmpty(m_configurationFileName) && File.Exists(m_configurationFileName))
            {
                try
                {
                    // Create a new file watcher for configuration file - we'll automatically refresh configuration file
                    // when this file gets updated...
                    m_configurationFileWatcher = new FileSystemWatcher(Path.GetFullPath(m_configurationFileName), FilePath.GetFileName(m_configurationFileName));
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
            ConfigurationFrame configurationFrame = frame as ConfigurationFrame;

            if (configurationFrame != null)
                m_configurationFrame = configurationFrame;
        }

        /// <summary>
        /// Casts the parsed <see cref="IChannelFrame"/> to its specific implementation (i.e., <see cref="DataFrame"/>, <see cref="ConfigurationFrame"/>, <see cref="CommandFrame"/> or <see cref="HeaderFrame"/>).
        /// </summary>
        /// <param name="frame"><see cref="IChannelFrame"/> that was parsed by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> that implements protocol specific common frame header interface.</param>
        protected override void OnReceivedChannelFrame(IChannelFrame frame)
        {
            // Raise abstract channel frame events as a priority (i.e., IDataFrame, IConfigurationFrame, etc.)
            base.OnReceivedChannelFrame(frame);

            // Raise BPA PDCstream specific channel frame events, if any have been subscribed
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

        // Static Methods

        // Attempts to cast given frame into an BPA PDCstream configuration frame - theoretically this will
        // allow the same configuration frame to be used for any protocol implementation
        internal static ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame sourceFrame, string configurationFileName)
        {
            // See if frame is already an BPA PDCstream configuration frame (if so, we don't need to do any work)
            ConfigurationFrame derivedFrame = sourceFrame as ConfigurationFrame;

            if (derivedFrame == null)
            {
                // Create a new BPA PDCstream configuration frame converted from equivalent configuration information
                ConfigurationCell derivedCell;
                IFrequencyDefinition sourceFrequency;

                derivedFrame = new ConfigurationFrame(configurationFileName);

                foreach (IConfigurationCell sourceCell in sourceFrame.Cells)
                {
                    // Create new derived configuration cell
                    derivedCell = new ConfigurationCell(derivedFrame, sourceCell.IDCode, sourceCell.NominalFrequency);

                    // Create equivalent derived phasor definitions
                    foreach (IPhasorDefinition sourcePhasor in sourceCell.PhasorDefinitions)
                    {
                        derivedCell.PhasorDefinitions.Add(new PhasorDefinition(derivedCell, sourcePhasor.Label, sourcePhasor.ScalingValue, sourcePhasor.Offset, sourcePhasor.PhasorType, null));
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

// TODO: Delete old code...

////*******************************************************************************************************
////  FrameParser.vb - BPA PDCstream Frame Parser
////  Copyright © 2008 - TVA, all rights reserved - Gbtc
////
////  Build Environment: VB.NET, Visual Studio 2008
////  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
////      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
////       Phone: 423/751-2827
////       Email: jrcarrol@tva.gov
////
////  Code Modification History:
////  -----------------------------------------------------------------------------------------------------
////  01/14/2005 - J. Ritchie Carroll
////       Initial version of source generated
////
////*******************************************************************************************************

//using System;
//using System.IO;
//using System.Text;
//using System.Collections.Generic;
//using System.ComponentModel;
//using PCS.IO;

//namespace PCS.PhasorProtocols
//{
//    namespace BpaPdcStream
//    {
//        /// <summary>This class parses a BPA PDC binary data stream and returns parsed data via events</summary>
//        /// <remarks>Frame parser is implemented as a write-only stream - this way data can come from any source</remarks>
//        [CLSCompliant(false)]
//        public class FrameParser : FrameParserBase
//        {
//            #region " Public Member Declarations "

//            // We shadow base class events with their BPA PDCstream specific derived versions for convinience in case users consume this class directly
//            public delegate void ReceivedCommonFrameHeaderEventHandler(ICommonFrameHeader frame);
//            public event ReceivedCommonFrameHeaderEventHandler ReceivedCommonFrameHeader;

//            public delegate void ReceivedConfigurationFrameEventHandler(ConfigurationFrame frame);
//            public new event ReceivedConfigurationFrameEventHandler ReceivedConfigurationFrame;

//            public delegate void ReceivedDataFrameEventHandler(DataFrame frame);
//            public new event ReceivedDataFrameEventHandler ReceivedDataFrame;

//            #endregion

//            #region " Private Member Declarations "

//            private ConfigurationFrame m_configurationFrame;
//            private string m_configurationFileName;
//            private bool m_refreshConfigurationFileOnChange;
//            private bool m_parseWordCountFromByte;
//            private long m_lastUpdateNotification;
//            private FileSystemWatcher m_configurationFileWatcher;
//            private bool m_disposed;

//            #endregion

//            #region " Construction Functions "

//            public FrameParser()
//            {
//            }

//            public FrameParser(string configurationFileName)
//            {

//                m_configurationFileName = configurationFileName;
//                m_refreshConfigurationFileOnChange = true;
//                ResetFileWatcher();

//            }

//            public FrameParser(IConfigurationFrame configurationFrame)
//            {

//                m_configurationFrame = CastToDerivedConfigurationFrame(configurationFrame);
//                m_configurationFileName = m_configurationFrame.ConfigurationFileName;
//                m_refreshConfigurationFileOnChange = true;
//                ResetFileWatcher();

//            }

//            public FrameParser(ConfigurationFrame configurationFrame)
//            {

//                m_configurationFrame = configurationFrame;
//                m_configurationFileName = m_configurationFrame.ConfigurationFileName;
//                m_refreshConfigurationFileOnChange = true;
//                ResetFileWatcher();

//            }

//            #endregion

//            #region " Public Methods Implementation "

//            public override bool ProtocolUsesSyncByte
//            {
//                get
//                {
//                    return true;
//                }
//            }

//            public override IConfigurationFrame ConfigurationFrame
//            {
//                get
//                {
//                    return m_configurationFrame;
//                }
//                set
//                {
//                    m_configurationFrame = CastToDerivedConfigurationFrame(value);
//                }
//            }

//            public string ConfigurationFileName
//            {
//                get
//                {
//                    if (m_configurationFrame == null)
//                    {
//                        return m_configurationFrame.ConfigurationFileName;
//                    }
//                    else
//                    {
//                        return m_configurationFileName;
//                    }
//                }
//                set
//                {
//                    m_configurationFileName = value;
//                    ResetFileWatcher();
//                }
//            }

//            /// <summary>Set to True to automatically reload configuration file when it has changed on disk</summary>
//            public bool RefreshConfigurationFileOnChange
//            {
//                get
//                {
//                    return m_refreshConfigurationFileOnChange;
//                }
//                set
//                {
//                    m_refreshConfigurationFileOnChange = value;
//                    ResetFileWatcher();
//                }
//            }

//            /// <summary>
//            /// Set to True to interpret word count in packet header from a byte instead of a word - if the sync byte
//            /// (0xAA) is at position one, then the word count would be interpreted from byte four.  Some older BPA PDC
//            /// stream implementations have a 0x01 in byte three where there should be a 0x00 and this throws off the
//            /// frame length, setting this property to True will correctly interpret the word count.
//            /// </summary>
//            public bool ParseWordCountFromByte
//            {
//                get
//                {
//                    return m_parseWordCountFromByte;
//                }
//                set
//                {
//                    m_parseWordCountFromByte = value;
//                }
//            }

//            public override string Status
//            {
//                get
//                {
//                    System.Text.StringBuilder status = new StringBuilder();

//                    status.Append("    INI configuration file: ");
//                    status.Append(m_configurationFileName);
//                    status.AppendLine();
//                    if (m_configurationFrame != null)
//                    {
//                        status.Append("       BPA PDC stream type: ");
//                        status.Append(m_configurationFrame.StreamType);
//                        status.AppendLine();
//                        status.Append("   BPA PDC revision number: ");
//                        status.Append(m_configurationFrame.RevisionNumber);
//                        status.AppendLine();
//                    }
//                    status.Append(base.Status);

//                    return status.ToString();
//                }
//            }

//            public override IConnectionParameters ConnectionParameters
//            {
//                get
//                {
//                    return base.ConnectionParameters;
//                }
//                set
//                {
//                    BpaPdcStream.ConnectionParameters parameters = value as BpaPdcStream.ConnectionParameters;

//                    if (parameters != null)
//                    {
//                        base.ConnectionParameters = parameters;

//                        // Assign new incoming connection parameter values
//                        m_configurationFileName = parameters.ConfigurationFileName;
//                        m_parseWordCountFromByte = parameters.ParseWordCountFromByte;
//                        m_refreshConfigurationFileOnChange = parameters.RefreshConfigurationFileOnChange;
//                        ResetFileWatcher();
//                    }
//                }
//            }

//            #endregion

//            #region " Protected Methods Implementation "

//            protected override void Dispose(bool disposing)
//            {

//                if (!m_disposed)
//                {
//                    base.Dispose(disposing);

//                    if (disposing)
//                    {
//                        if (m_configurationFileWatcher != null)
//                        {
//                            m_configurationFileWatcher.Changed -= m_configurationFileWatcher_Changed;
//                            m_configurationFileWatcher.Dispose();
//                        }
//                        m_configurationFileWatcher = null;
//                    }
//                }

//                m_disposed = true;

//            }

//            protected override void ParseFrame(byte[] buffer, int offset, int length, ref int parsedFrameLength)
//            {

//                // See if there is enough data in the buffer to parse the common frame header.
//                // Note that in order to get time tag for data frames, we'll need at least six more bytes
//                if (length >= CommonFrameHeader.BinaryLength + 6)
//                {
//                    // Parse frame header
//                    ICommonFrameHeader parsedFrameHeader = CommonFrameHeader.ParseBinaryImage(m_configurationFrame, m_parseWordCountFromByte, buffer, offset);

//                    // See if there is enough data in the buffer to parse the entire frame
//                    if (length >= parsedFrameHeader.FrameLength)
//                    {
//                        // Expose the frame buffer image in case client needs this data for any reason
//                        RaiseReceivedFrameBufferImage(parsedFrameHeader.FundamentalFrameType, buffer, offset, parsedFrameHeader.FrameLength);

//                        // Entire frame is available, so we go ahead and parse it
//                        switch (parsedFrameHeader.FrameType)
//                        {
//                            case FrameType.DataFrame:
//                                if (m_configurationFrame == null)
//                                {
//                                    // Until we receive configuration frame, we at least expose the part of the frame we have parsed
//                                    RaiseReceivedCommonFrameHeader(parsedFrameHeader);
//                                }
//                                else
//                                {
//                                    // We can only start parsing data frames once we have successfully received a configuration frame...
//                                    RaiseReceivedDataFrame(new DataFrame(parsedFrameHeader, m_configurationFrame, buffer, offset));
//                                }
//                                break;
//                            case FrameType.ConfigurationFrame:
//                                // Parse new configuration frame
//                                ConfigurationFrame configFrame = new ConfigurationFrame(parsedFrameHeader, m_configurationFileName, buffer, offset);
//                                m_configurationFrame = configFrame;
//                                RaiseReceivedConfigurationFrame(configFrame);
//                                break;
//                        }

//                        parsedFrameLength = parsedFrameHeader.FrameLength;
//                    }
//                }

//            }

//            // We override the base class event raisers to allow derived class to also expose the frame in native (i.e., non-interfaced) format
//            protected override void RaiseReceivedDataFrame(IDataFrame frame)
//            {

//                base.RaiseReceivedDataFrame(frame);
//                if (ReceivedDataFrame != null)
//                    ReceivedDataFrame((DataFrame)frame);

//            }

//            protected override void RaiseReceivedConfigurationFrame(IConfigurationFrame frame)
//            {

//                base.RaiseReceivedConfigurationFrame(frame);
//                if (ReceivedConfigurationFrame != null)
//                    ReceivedConfigurationFrame((ConfigurationFrame)frame);

//            }

//            #endregion

//            #region " Private Methods Implementation "

//            private void RaiseReceivedCommonFrameHeader(ICommonFrameHeader frame)
//            {
//                base.RaiseReceivedUndeterminedFrame(frame);
//                if (ReceivedCommonFrameHeader != null)
//                    ReceivedCommonFrameHeader(frame);
//            }
            
//            private ConfigurationFrame CastToDerivedConfigurationFrame(IConfigurationFrame configurationFrame)
//            {
//                ConfigurationFrame derivedFrame = configurationFrame as ConfigurationFrame;

//                if (derivedFrame == null)
//                    return new ConfigurationFrame(configurationFrame);
//                else
//                    return derivedFrame;
//            }


//            #endregion
//        }
//    }
//}
