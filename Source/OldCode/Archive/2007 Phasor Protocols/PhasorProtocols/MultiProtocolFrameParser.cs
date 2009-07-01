//*******************************************************************************************************
//  MultiProtocolFrameParser.cs
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
//  03/16/2006 - J. Ritchie Carroll
//       Initial version of source generated
//  06/26/2006 - Pinal C. Patel
//       Changed out the socket code with TcpClient and UdpClient components from TVA.Communication
//  01/31/2007 - J. Ritchie Carroll
//       Added TCP "server" support to allow listening connections from devices that act as data
//       clients, e.g., F-NET devices
//  05/23/2007 - Pinal C. Patel
//       Added member variable 'm_clientConnectionAttempts' to track the number of attempts made for
//       connecting to the server since this information is no longer provided by the event raised by
//       any of the Communication Client components
//  07/05/2007 - J. Ritchie Carroll
//       Wrapped all event raising for frame parsing in Try/Catch so that any exceptions thrown in
//       consumer event handlers won't have a negative effect on continuous data parsing - exceptions
//       in consumer event handlers are duly noted and raised through the ParsingException event
//  09/28/2007 - J. Ritchie Carroll
//       Implemented new disconnect overload on communications client that allows timeout on socket
//       close to fix an issue related non-responsive threads that "lock-up" after sending connection
//       commands that attempt to close the socket for remotely connected devices
//  12/14/2007 - J. Ritchie Carroll
//       Implemented simulated timestamp injection for published frames to allow for real-time
//       data simulations from archived sample data
//  10/28/2008 - James R Carroll
//       Added support for SEL's UDP_T and UDP_U protocol implementations (UDP_S was already supported),
//       implementation handled by allowing definition of a "CommandChannel" in the connection string
//  04/27/2009 - James R Carroll
//       Added support for SEL Fast Message protocol
//
//*******************************************************************************************************

// Define this constant to enable a raw data export for debugging - do not leave this on for deployed builds
#undef RawDataCapture

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using TVA;
using TVA.Communication;
using TVA.IO;
using TVA.Units;

namespace PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Phasor data protocols enumeration.
    /// </summary>
    [Serializable()]
    public enum PhasorProtocol
    {
        /// <summary>
        /// IEEE C37.118-2005 protocol.
        /// </summary>
        IeeeC37_118V1,
        /// <summary>
        /// IEEE C37.118, draft 6 protocol.
        /// </summary>
        IeeeC37_118D6,
        /// <summary>
        /// IEEE 1344-1995 protocol.
        /// </summary>
        Ieee1344,
        /// <summary>
        /// BPA PDCstream protocol.
        /// </summary>
        BpaPdcStream,
        /// <summary>
        /// Virgina Tech F-NET protocol.
        /// </summary>
        FNet,
        /// <summary>
        /// SEL Fast Message protocol.
        /// </summary>
        SelFastMessage,
        /// <summary>
        /// Macrodyne protocol.
        /// </summary>
        Macrodyne
    }

    #endregion

    /// <summary>
    /// Protocol independent frame parser.
    /// </summary>
    /// <remarks>
    /// This class takes all protocol frame parsing implementations and reduces them to a single simple-to-use class exposing all
    /// data through abstract interfaces (e.g., IConfigurationFrame, IDataFrame, etc.) - this way new protocol implementations can
    /// be added without adversely affecting consuming code. Additionally, this class implements a variety of transport options
    /// (e.g., TCP, UDP, Serial, etc.) and hides the complexities of this connectivity and internally pushes all data received from
    /// the selected transport protocol to the selected phasor parsing protocol.
    /// </remarks>
    public class MultiProtocolFrameParser : IFrameParser
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="BufferSize"/> property.
        /// </summary>
        public const int DefaultBufferSize = 262144; // 256K

        /// <summary>
        /// Specifies the default value for the <see cref="DefinedFrameRate"/> property.
        /// </summary>
        public const double DefaultDefinedFrameRate = 1.0D / 30.0D;

        /// <summary>
        /// Specifies the default value for the <see cref="MaximumConnectionAttempts"/> property.
        /// </summary>
        public const int DefaultMaximumConnectionAttempts = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="AutoStartDataParsingSequence"/> property.
        /// </summary>
        public const bool DefaultAutoStartDataParsingSequence = true;

        // Events

        /// <summary>
        /// Occurs when a <see cref="ICommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ICommandFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ICommandFrame>> ReceivedCommandFrame;

        /// <summary>
        /// Occurs when a <see cref="IConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IConfigurationFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a <see cref="IDataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IDataFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IDataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when a <see cref="IHeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IHeaderFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IHeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an undetermined <see cref="IChannelFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the undetermined <see cref="IChannelFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IChannelFrame>> ReceivedUndeterminedFrame;

        /// <summary>
        /// Occurs when a frame buffer image has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument1"/> is the <see cref="FundamentalFrameType"/> of the frame buffer image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument2"/> is the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument3"/> is the offset into the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument4"/> is the length of data in the buffer that contains the frame image that was received.
        /// </remarks>
        public event EventHandler<EventArgs<FundamentalFrameType, byte[], int, int>> ReceivedFrameBufferImage;

        /// <summary>
        /// Occurs when a device sends a notification that its configuration has changed.
        /// </summary>
        public event EventHandler ConfigurationChanged;
        
        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while parsing the data stream.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing the data stream.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ParsingException;
        
        /// <summary>
        /// Occurs when a <see cref="ICommandFrame"/> is sent to a device.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a reference to the <see cref="ICommandFrame"/> that was sent to the device.
        /// </remarks>
        public event EventHandler<EventArgs<ICommandFrame>> SentCommandFrame;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during connection attempt to a device.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the exception that occured during the connection attempt.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of connections attempted so far.
        /// </remarks>
        public event EventHandler<EventArgs<Exception, int>> ConnectionException;

        /// <summary>
        /// Occurs when <see cref="MultiProtocolFrameParser"/> is attempting connection to a device.
        /// </summary>
        public event EventHandler ConnectionAttempt;

        /// <summary>
        /// Occurs when <see cref="MultiProtocolFrameParser"/> has established a connection to a device.
        /// </summary>
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Occurs when device connection has been terminated.
        /// </summary>
        public event EventHandler ConnectionTerminated;
        
        /// <summary>
        /// Occurs when the <see cref="MultiProtocolFrameParser"/> is setup as a listening connection and server connection has been started.
        /// </summary>
        public event EventHandler ServerStarted;

        /// <summary>
        /// Occurs when the <see cref="MultiProtocolFrameParser"/> is setup as a listening connection and server connection has been stopped.
        /// </summary>
        public event EventHandler ServerStopped;

        // Fields
        private PhasorProtocol m_phasorProtocol;
        private TransportProtocol m_transportProtocol;
        private string m_connectionString;
        private int m_maximumConnectionAttempts;
        private ushort m_deviceID;
        private int m_bufferSize;
        private IFrameParser m_frameParser;
        private IClient m_dataChannel;
        private IServer m_serverBasedDataChannel;
        private IClient m_commandChannel;
        private System.Timers.Timer m_rateCalcTimer;
        private IConfigurationFrame m_configurationFrame;
        private long m_dataStreamStartTime;
        private bool m_executeParseOnSeparateThread;
        private bool m_autoRepeatCapturedPlayback;
        private bool m_injectSimulatedTimestamp;
        private long m_totalFramesReceived;
        private int m_frameRateTotal;
        private int m_byteRateTotal;
        private long m_totalBytesReceived;
        private double m_frameRate;
        private double m_byteRate;
        private string m_sourceName;
        private double m_definedFrameRate;
        private long m_lastFrameReceivedTime;
        private bool m_autoStartDataParsingSequence;
        private bool m_initiatingDataStream;
        private long m_initialBytesReceived;
        private bool m_deviceSupportsCommands;
        private bool m_enabled;
        private IConnectionParameters m_connectionParameters;
        private int m_connectionAttempts;
        private bool m_disposed;

        #if RawDataCapture
        FileStream m_rawDataCapture;
        #endif

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="MultiProtocolFrameParser"/> using the default settings.
        /// </summary>
        public MultiProtocolFrameParser()
        {
            m_connectionString = "server=127.0.0.1:4712";
            m_deviceID = 1;
            m_bufferSize = DefaultBufferSize;
            m_definedFrameRate = DefaultDefinedFrameRate;
            m_maximumConnectionAttempts = DefaultMaximumConnectionAttempts;
            m_autoStartDataParsingSequence = DefaultAutoStartDataParsingSequence;
            m_rateCalcTimer = new System.Timers.Timer();

            m_phasorProtocol = PhasorProtocol.IeeeC37_118V1;
            m_transportProtocol = TransportProtocol.Tcp;

            m_rateCalcTimer.Elapsed += m_rateCalcTimer_Elapsed;
            m_rateCalcTimer.Interval = 5000;
            m_rateCalcTimer.AutoReset = true;
            m_rateCalcTimer.Enabled = false;
        }

        /// <summary>
        /// Creates a new <see cref="MultiProtocolFrameParser"/> using the specified <see cref="PhasorProtocol"/> and <see cref="TransportProtocol"/>.
        /// </summary>
        /// <param name="phasorProtocol">Phasor protocol to use for this frame parser.</param>
        /// <param name="transportProtocol">Transport protocol to use for this frame parser.</param>
        public MultiProtocolFrameParser(PhasorProtocol phasorProtocol, TransportProtocol transportProtocol)
            : this()
        {
            m_phasorProtocol = phasorProtocol;
            m_transportProtocol = transportProtocol;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="MultiProtocolFrameParser"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~MultiProtocolFrameParser()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="PhasorProtocols.PhasorProtocol"/> to use with this <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public virtual PhasorProtocol PhasorProtocol
        {
            get
            {
                return m_phasorProtocol;
            }
            set
            {
                m_phasorProtocol = value;
                m_deviceSupportsCommands = DeriveCommandSupport();

                // Setup protocol specific connection parameters...
                switch (value)
                {
                    case PhasorProtocols.PhasorProtocol.BpaPdcStream:
                        m_connectionParameters = new BpaPdcStream.ConnectionParameters();
                        break;
                    case PhasorProtocols.PhasorProtocol.FNet:
                        m_connectionParameters = new FNet.ConnectionParameters();
                        break;
                    case PhasorProtocols.PhasorProtocol.SelFastMessage:
                        m_connectionParameters = new SelFastMessage.ConnectionParameters();
                        break;
                    default:
                        m_connectionParameters = null;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets <see cref="TransportProtocol"/> to use with this <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public virtual TransportProtocol TransportProtocol
        {
            get
            {
                return m_transportProtocol;
            }
            set
            {
                m_transportProtocol = value;
                m_deviceSupportsCommands = DeriveCommandSupport();

                if (m_transportProtocol == TransportProtocol.File && m_autoRepeatCapturedPlayback)
                    ExecuteParseOnSeparateThread = false;
            }
        }

        /// <summary>
        /// Gets or sets the key/value pair based connection information required by the <see cref="MultiProtocolFrameParser"/> to connect to a device.
        /// </summary>
        public virtual string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;

                // Parse connection string to see if a phasor or transport protocol was assigned
                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
                string setting;

                if (settings.TryGetValue("phasorProtocol", out setting))
                    PhasorProtocol = (PhasorProtocol)Enum.Parse(typeof(PhasorProtocol), setting);

                if (settings.TryGetValue("transportProtocol", out setting) || settings.TryGetValue("protocol", out setting))
                    TransportProtocol = (TransportProtocol)Enum.Parse(typeof(TransportProtocol), setting);

                m_deviceSupportsCommands = DeriveCommandSupport();
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if a device supports commands.
        /// </summary>
        /// <remarks>
        /// This property is automatically derived based on the selected <see cref="PhasorProtocol"/>, <see cref="TransportProtocol"/>
        /// and <see cref="ConnectionString"/>, but can be overriden if the consumer already knows that a device supports commands.
        /// </remarks>
        public bool DeviceSupportsCommands
        {
            get
            {
                return m_deviceSupportsCommands;
            }
            set
            {
                // Consumers can choose to override command support if needed
                m_deviceSupportsCommands = value;
            }
        }

        /// <summary>
        /// Gets or sets the device identification code often needed to establish a connection.
        /// </summary>
        /// <remarks>
        /// Most devices validate this ID when sending commands, so it must be correct in order to start parsing sequence.
        /// </remarks>
        public ushort DeviceID
        {
            get
            {
                return m_deviceID;
            }
            set
            {
                m_deviceID = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffer used by the <see cref="MultiProtocolFrameParser"/> for sending and receiving data from a device.
        /// </summary>
        /// <exception cref="ArgumentException">The value specified is either zero or negative.</exception>
        public int BufferSize
        {
            get
            {
                return m_bufferSize;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative.");

                m_bufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of times the <see cref="MultiProtocolFrameParser"/> will attempt to connect to a device.
        /// </summary>
        /// <remarks>Set to -1 for infinite connection attempts.</remarks>
        public int MaximumConnectionAttempts
        {
            get
            {
                return m_maximumConnectionAttempts;
            }
            set
            {
                m_maximumConnectionAttempts = value;
            }
        }

        /// <summary>
        /// Gets or sets flag to automatically send the ConfigFrame2 and EnableRealTimeData command frames used to start a typical data parsing sequence.
        /// </summary>
        /// <remarks>
        /// For devices that support IEEE commands, setting this property to true will automatically start the data parsing sequence.
        /// </remarks>
        public bool AutoStartDataParsingSequence
        {
            get
            {
                return m_autoStartDataParsingSequence;
            }
            set
            {
                m_autoStartDataParsingSequence = value;
            }
        }

        /// <summary>
        /// Gets or sets a descriptive name for a device connection.
        /// </summary>
        public string SourceName
        {
            get
            {
                return m_sourceName;
            }
            set
            {
                m_sourceName = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag that allows frame parsing to be executed on a separate thread (i.e., other than communications thread).
        /// </summary>
        /// <remarks>
        /// This is typically only needed when data frames are very large. This change will happen dynamically, even if a connection is active.
        /// </remarks>
        public bool ExecuteParseOnSeparateThread
        {
            get
            {
                return m_executeParseOnSeparateThread;
            }
            set
            {
                // If using file based source and auto-repeat is enabled, we don't allow execution on a separate thread
                // since file based streaming data source will continue to queue data as quickly as possible and add data
                // data to processing queue much faster than it will be processed thereby consuming all available memory
                if (m_transportProtocol == TransportProtocol.File && m_autoRepeatCapturedPlayback)
                    m_executeParseOnSeparateThread = false;
                else
                    m_executeParseOnSeparateThread = value;

                // Since frame parsers support dynamic changes in this value, we'll pass this value along to the
                // the frame parser if one has been established...
                if (m_frameParser != null)
                    m_frameParser.ExecuteParseOnSeparateThread = m_executeParseOnSeparateThread;
            }
        }

        /// <summary>
        /// Gets or sets desired frame rate to use for maintaining captured frame replay timing.
        /// </summary>
        /// <remarks>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public double DefinedFrameRate
        {
            get
            {
                return m_definedFrameRate;
            }
            set
            {
                m_definedFrameRate = value;
            }
        }

        /// <summary>
        /// Gets or sets flag indicating whether or not to inject local system time into parsed data frames.
        /// </summary>
        /// <remarks>
        /// When connection is made to a file for replay purposes or consumer doesn't trust remote clock source, this flag
        /// can be set to true replace all frame timestamps with a UTC timestamp dervied from the local system clock.
        /// </remarks>
        public bool InjectSimulatedTimestamp
        {
            get
            {
                return m_injectSimulatedTimestamp;
            }
            set
            {
                m_injectSimulatedTimestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines if a file used for replaying data should be restarted at the beginning once it has been completed.
        /// </summary>
        /// <remarks>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public bool AutoRepeatCapturedPlayback
        {
            get
            {
                return m_autoRepeatCapturedPlayback;
            }
            set
            {
                m_autoRepeatCapturedPlayback = value;

                if (m_transportProtocol == TransportProtocol.File && m_autoRepeatCapturedPlayback)
                    ExecuteParseOnSeparateThread = false;
            }
        }

        /// <summary>
        /// Gets or sets current <see cref="IConfigurationFrame"/> used for parsing <see cref="IDataFrame"/>'s encountered in the data stream from a device.
        /// </summary>
        /// <remarks>
        /// If a <see cref="IConfigurationFrame"/> has been parsed, this will return a reference to the parsed frame.  Consumer can manually assign a
        /// <see cref="IConfigurationFrame"/> to start parsing data if one has not been encountered in the stream.
        /// </remarks>
        public IConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_configurationFrame;
            }
            set
            {
                m_configurationFrame = value;

                // Pass new config frame onto appropriate parser, casting into appropriate protocol if needed...
                if (m_frameParser != null)
                    m_frameParser.ConfigurationFrame = value;
            }
        }

        /// <summary>
        /// Gets a flag that determines if the currently selected <see cref="PhasorProtocol"/> is an IEEE standard protocol.
        /// </summary>
        public virtual bool IsIEEEProtocol
        {
            get
            {
                return (m_phasorProtocol == PhasorProtocols.PhasorProtocol.IeeeC37_118V1 || 
                        m_phasorProtocol == PhasorProtocols.PhasorProtocol.IeeeC37_118D6 || 
                        m_phasorProtocol == PhasorProtocols.PhasorProtocol.Ieee1344);
            }
        }

        /// <summary>
        /// Gets a flag that determines if the currently selected <see cref="TransportProtocol"/> is connected.
        /// </summary>
        public virtual bool IsConnected
        {
            get
            {
                if (m_commandChannel != null)
                    return (m_commandChannel.CurrentState == ClientState.Connected);
                else if (m_dataChannel != null)
                    return (m_dataChannel.CurrentState == ClientState.Connected);
                else if (m_serverBasedDataChannel != null)
                    return (m_serverBasedDataChannel.ClientIDs.Length > 0);

                return false;
            }
        }

        /// <summary>
        /// Gets total time connection has been active.
        /// </summary>
        public virtual Time ConnectionTime
        {
            get
            {
                if (m_commandChannel != null)
                    return m_commandChannel.ConnectionTime;
                else if (m_dataChannel != null)
                    return m_dataChannel.ConnectionTime;
                else if (m_serverBasedDataChannel != null)
                    return m_serverBasedDataChannel.RunTime;

                return 0;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="MultiProtocolFrameParser"/> is currently enabled.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Enabled"/> to true will start the <see cref="MultiProtocolFrameParser"/> if it is not started,
        /// setting to false will stop the <see cref="MultiProtocolFrameParser"/> if it is started.
        /// </remarks>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (value && !m_enabled)
                    Start();
                else if (!value && m_enabled)
                    Stop();
            }
        }

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        public int QueuedBuffers
        {
            get
            {
                if (m_frameParser != null)
                    return m_frameParser.QueuedBuffers;

                return 0;
            }
        }

        /// <summary>
        /// Gets a boolean value that determines if data channel is defined as a server based connection.
        /// </summary>
        public virtual bool DataChannelIsServerBased
        {
            get
            {
                if (m_dataChannel != null)
                    return false;

                if (m_serverBasedDataChannel == null)
                {
                    // No connection is currently active, see if connection string defines a server based connection
                    if (!string.IsNullOrEmpty(m_connectionString))
                    {
                        Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
                        string setting;

                        if (settings.TryGetValue("islistener", out setting))
                            return setting.ParseBoolean();

                        return false;
                    }

                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets total number of frames that have been received from a device so far.
        /// </summary>
        public long TotalFramesReceived
        {
            get
            {
                return m_totalFramesReceived;
            }
        }

        /// <summary>
        /// Gets total number of bytes that have been received from a device so far.
        /// </summary>
        public long TotalBytesReceived
        {
            get
            {
                return m_totalBytesReceived;
            }
        }

        /// <summary>
        /// Gets the calculated frame rate (i.e., frames per second) based on data received from device connection.
        /// </summary>
        public double FrameRate
        {
            get
            {
                return m_frameRate;
            }
        }

        /// <summary>
        /// Gets the calculated byte rate (i.e., bytes per second) based on data received from device connection.
        /// </summary>
        public double ByteRate
        {
            get
            {
                return m_byteRate;
            }
        }

        /// <summary>
        /// Gets the calculated bit rate (i.e., bits per second (bps)) based on data received from device connection.
        /// </summary>
        public double BitRate
        {
            get
            {
                return m_byteRate * 8.0D;
            }
        }

        /// <summary>
        /// Gets the calculated megabits per second (Mbps) rate based on data received from device connection.
        /// </summary>
        public double MegaBitRate
        {
            get
            {
                return BitRate / SI2.Mega;
            }
        }

        /// <summary>
        /// Gets or sets a reference to the active <see cref="IFrameParser"/>.
        /// </summary>
        protected IFrameParser FrameParser
        {
            get
            {
                return m_frameParser;
            }
            set
            {
                m_frameParser = value;
            }
        }

        /// <summary>
        /// Gets or sets a reference to the <see cref="IClient"/> data channel.
        /// </summary>
        protected IClient DataChannel
        {
            get
            {
                return m_dataChannel;
            }
            set
            {
                m_dataChannel = value;
            }
        }

        /// <summary>
        /// Gets or sets a reference to the <see cref="IServer"/> server based data channel.
        /// </summary>
        protected IServer ServerBasedDataChannel
        {
            get
            {
                return m_serverBasedDataChannel;
            }
            set
            {
                m_serverBasedDataChannel = value;
            }
        }

        /// <summary>
        /// Gets or sets a reference to the <see cref="IClient"/> command channel.
        /// </summary>
        protected IClient CommandChannel
        {
            get
            {
                return m_commandChannel;
            }
            set
            {
                m_commandChannel = value;
            }
        }

        /// <summary>
        /// Gets a descriptive name for a device connection that includes <see cref="SourceName"/>, if provided.
        /// </summary>
        public string Name
        {
            get
            {

                if (string.IsNullOrEmpty(m_sourceName))
                    return "ID " + m_deviceID + " using " + m_phasorProtocol.GetFormattedProtocolName() + " over " + m_transportProtocol;
                else
                    return m_sourceName + " (" + m_deviceID + ")";
            }
        }

        /// <summary>
        /// Gets current descriptive status of the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("      Device Connection ID: {0}", m_deviceID);
                status.AppendLine();
                status.AppendFormat("           Phasor protocol: {0}", m_phasorProtocol.GetFormattedProtocolName());
                status.AppendLine();
                status.AppendFormat("               Buffer size: {0}", m_bufferSize);
                status.AppendLine();
                status.AppendFormat("     Total frames received: {0}", m_totalFramesReceived);
                status.AppendLine();
                status.AppendFormat("     Calculated frame rate: {0}", m_frameRate);
                status.AppendLine();
                status.AppendFormat("      Calculated data rate: {0} bytes/sec, {1} Mbps", m_byteRate.ToString("0.0"), MegaBitRate.ToString("0.0000"));
                status.AppendLine();

                if (m_frameParser != null)
                    status.Append(m_frameParser.Status);

                if (m_dataChannel != null)
                    status.Append(m_dataChannel.Status);

                if (m_serverBasedDataChannel != null)
                    status.Append(m_serverBasedDataChannel.Status);

                if (m_commandChannel != null)
                    status.Append(m_commandChannel.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets or sets any connection specific <see cref="IConnectionParameters"/> that may be applicable for the current <see cref="PhasorProtocol"/>.
        /// </summary>
        public IConnectionParameters ConnectionParameters
        {
            get
            {
                return m_connectionParameters;
            }
            set
            {
                m_connectionParameters = value;

                // Pass new connection parameters along to derived frame parser if instantiated
                if (m_frameParser != null)
                    m_frameParser.ConnectionParameters = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="MultiProtocolFrameParser"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MultiProtocolFrameParser"/> object and optionally releases the managed resources.
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
                        Stop();

                        if (m_rateCalcTimer != null)
                        {
                            m_rateCalcTimer.Elapsed -= m_rateCalcTimer_Elapsed;
                            m_rateCalcTimer.Dispose();
                        }
                        m_rateCalcTimer = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public virtual void Start()
        {
            // Stop parser if is already running - thus calling start after already started will have the effect
            // of "restarting" the parsing engine...
            Stop();

            //Reset statistics...
            m_totalFramesReceived = 0;
            m_frameRateTotal = 0;
            m_byteRateTotal = 0;
            m_totalBytesReceived = 0;
            m_frameRate = 0.0D;
            m_byteRate = 0.0D;

            try
            {
                // Parse connection string to check for special parameters
                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
                string setting;

                // Establish protocol specific frame parser
                InitializeFrameParser(settings);

                // Establish command channel connection, if defined...
                if (settings.TryGetValue("commandChannel", out setting))
                    InitializeCommandChannel(setting);

                // Establish data channel connection - must be defined.
                InitializeDataChannel(settings);

                // Start parsing engine
                m_frameParser.Start();

                m_rateCalcTimer.Enabled = true;
                m_enabled = true;
            }
            catch
            {
                Stop();
                throw;
            }
        }

        /// <summary>
        /// Initialize frame parser.
        /// </summary>
        /// <param name="settings">Key/value pairs dictionary parsed from connection string.</param>
        protected virtual void InitializeFrameParser(Dictionary<string, string> settings)
        {
            string setting;

            // Instantiate protocol specific frame parser
            switch (m_phasorProtocol)
            {
                case PhasorProtocol.IeeeC37_118V1:
                    m_frameParser = new IeeeC37_118.FrameParser(IeeeC37_118.DraftRevision.Draft7);
                    break;
                case PhasorProtocol.IeeeC37_118D6:
                    m_frameParser = new IeeeC37_118.FrameParser(IeeeC37_118.DraftRevision.Draft6);
                    break;
                case PhasorProtocol.Ieee1344:
                    m_frameParser = new Ieee1344.FrameParser();
                    break;
                case PhasorProtocol.BpaPdcStream:
                    m_frameParser = new BpaPdcStream.FrameParser();

                    // Check for BPA PDCstream protocol specific parameters in connection string
                    BpaPdcStream.ConnectionParameters bpaPdcParameters = m_connectionParameters as BpaPdcStream.ConnectionParameters;

                    if (bpaPdcParameters != null)
                    {
                        // INI file name setting is required
                        if (settings.TryGetValue("iniFileName", out setting))
                            bpaPdcParameters.ConfigurationFileName = FilePath.GetAbsolutePath(setting);
                        else if (string.IsNullOrEmpty(bpaPdcParameters.ConfigurationFileName))
                            throw new ArgumentException("BPA PDCstream INI filename setting (e.g., \"inifilename=DEVICE_PDC.ini\") was not found. This setting is required for BPA PDCstream protocol connections - frame parser initialization terminated.");

                        if (settings.TryGetValue("refreshConfigFileOnChange", out setting))
                            bpaPdcParameters.RefreshConfigurationFileOnChange = setting.ParseBoolean();

                        if (settings.TryGetValue("parseWordCountFromByte", out setting))
                            bpaPdcParameters.ParseWordCountFromByte = setting.ParseBoolean();
                    }
                    break;
                case PhasorProtocol.FNet:
                    m_frameParser = new FNet.FrameParser();

                    // Check for F-NET protocol specific parameters in connection string
                    FNet.ConnectionParameters fnetParameters = m_connectionParameters as FNet.ConnectionParameters;

                    if (fnetParameters != null)
                    {
                        if (settings.TryGetValue("timeOffset", out setting))
                            fnetParameters.TimeOffset = long.Parse(setting);

                        if (settings.TryGetValue("stationName", out setting))
                            fnetParameters.StationName = setting;

                        if (settings.TryGetValue("frameRate", out setting))
                            fnetParameters.FrameRate = ushort.Parse(setting);

                        if (settings.TryGetValue("nominalFrequency", out setting))
                            fnetParameters.NominalFrequency = (LineFrequency)Enum.Parse(typeof(LineFrequency), setting);
                    }
                    break;
                case PhasorProtocol.SelFastMessage:
                    m_frameParser = new SelFastMessage.FrameParser();

                    // Check for SEL Fast Message protocol specific parameters in connection string
                    SelFastMessage.ConnectionParameters selParameters = m_connectionParameters as SelFastMessage.ConnectionParameters;

                    if (selParameters != null)
                    {
                        if (settings.TryGetValue("messageperiod", out setting))
                            selParameters.MessagePeriod = (SelFastMessage.MessagePeriod)Enum.Parse(typeof(SelFastMessage.MessagePeriod), setting);
                    }
                    break;
                case PhasorProtocol.Macrodyne:
                    m_frameParser = new Macrodyne.FrameParser();
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Phasor protocol \"{0}\" is not recognized, failed to initialize frame parser", m_phasorProtocol));
            }

            // Assign frame parser properties
            m_frameParser.ConnectionParameters = m_connectionParameters;
            m_frameParser.ExecuteParseOnSeparateThread = m_executeParseOnSeparateThread;

            // Setup event handlers
            m_frameParser.ReceivedCommandFrame += m_frameParser_ReceivedCommandFrame;
            m_frameParser.ReceivedConfigurationFrame += m_frameParser_ReceivedConfigurationFrame;
            m_frameParser.ReceivedDataFrame += m_frameParser_ReceivedDataFrame;
            m_frameParser.ReceivedHeaderFrame += m_frameParser_ReceivedHeaderFrame;
            m_frameParser.ReceivedUndeterminedFrame += m_frameParser_ReceivedUndeterminedFrame;
            m_frameParser.ReceivedFrameBufferImage += m_frameParser_ReceivedFrameBufferImage;
            m_frameParser.ConfigurationChanged += m_frameParser_ConfigurationChanged;
            m_frameParser.ParsingException += m_frameParser_ParsingException;
        }

        /// <summary>
        /// Initialize command channel.
        /// </summary>
        /// <param name="connectionString">Command channel connection string.</param>
        protected virtual void InitializeCommandChannel(string connectionString)
        {
            // Parse command channel connection settings
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

            // Verify user did not attempt to setup command channel as a TCP server
            if (settings.ContainsKey("islistener") && settings["islistener"].ParseBoolean())
                throw new ArgumentException("Command channel cannot be setup as a TCP server.");

            // Validate command channel transport protocol selection
            TransportProtocol transportProtocol = (TransportProtocol)Enum.Parse(typeof(TransportProtocol), settings["protocol"], true);

            if (transportProtocol != TransportProtocol.Tcp && transportProtocol != TransportProtocol.Serial && transportProtocol != TransportProtocol.File)
                throw new ArgumentException("Command channel transport protocol can only be defined as TCP, Serial or File");

            // Instantiate command channel based on defined transport layer
            m_commandChannel = ClientBase.Create(connectionString);

            // Setup event handlers
            m_commandChannel.ConnectionEstablished += m_commandChannel_ConnectionEstablished;
            m_commandChannel.ConnectionAttempt += m_commandChannel_ConnectionAttempt;
            m_commandChannel.ConnectionException += m_commandChannel_ConnectionException;
            m_commandChannel.ConnectionTerminated += m_commandChannel_ConnectionTerminated;

            // Attempt connection to device over command channel
            m_commandChannel.ReceiveDataHandler = Write;
            m_commandChannel.ReceiveBufferSize = m_bufferSize;
            m_commandChannel.MaxConnectionAttempts = m_maximumConnectionAttempts;
            m_commandChannel.Handshake = false;
            m_commandChannel.Connect();
            m_connectionAttempts = 0;
        }

        /// <summary>
        /// Initialize data channel.
        /// </summary>
        /// <param name="settings">Key/value pairs dictionary parsed from connection string.</param>
        protected virtual void InitializeDataChannel(Dictionary<string, string> settings)
        {
            string setting;

            // Instantiate selected transport layer
            switch (m_transportProtocol)
            {
                case TransportProtocol.Tcp:
                    // The TCP transport may be set up as a server or as a client, we distinguish
                    // this simply by deriving the value of an added key/value pair in the
                    // connection string called "IsListener"
                    if (settings.TryGetValue("islistener", out setting))
                    {
                        if (setting.ParseBoolean())
                            m_serverBasedDataChannel = new TcpServer();
                        else
                            m_dataChannel = new TcpClient();
                    }
                    else
                    {
                        // If the key doesn't exist, we assume it's a client connection
                        m_dataChannel = new TcpClient();
                    }
                    break;
                case TransportProtocol.Udp:
                    m_dataChannel = new UdpClient();
                    break;
                case TransportProtocol.Serial:
                    m_dataChannel = new SerialClient();
                    break;
                case TransportProtocol.File:
                    // For file based playback, we allow the option of auto-repeat
                    FileClient fileClient = new FileClient();

                    fileClient.FileAccessMode = FileAccess.Read;
                    fileClient.FileShareMode = FileShare.Read;
                    fileClient.AutoRepeat = m_autoRepeatCapturedPlayback;
                    m_dataChannel = fileClient;
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Transport protocol \"{0}\" is not recognized, failed to initialize data channel", m_transportProtocol));
            }

            // Handle primary data connection, this *must* be defined...
            if (m_dataChannel != null)
            {
                // Setup event handlers
                m_dataChannel.ConnectionEstablished += m_dataChannel_ConnectionEstablished;
                m_dataChannel.ConnectionAttempt += m_dataChannel_ConnectionAttempt;
                m_dataChannel.ConnectionException += m_dataChannel_ConnectionException;
                m_dataChannel.ConnectionTerminated += m_dataChannel_ConnectionTerminated;

                // Attempt connection to device
                m_dataChannel.ReceiveDataHandler = Write;
                m_dataChannel.ReceiveBufferSize = m_bufferSize;
                m_dataChannel.ConnectionString = m_connectionString;
                m_dataChannel.MaxConnectionAttempts = m_maximumConnectionAttempts;
                m_dataChannel.Handshake = false;
                m_dataChannel.Connect();
                m_connectionAttempts = 0;
            }
            else if (m_serverBasedDataChannel != null)
            {
                // Setup event handlers
                m_serverBasedDataChannel.ClientConnected += m_serverBasedDataChannel_ClientConnected;
                m_serverBasedDataChannel.ClientDisconnected += m_serverBasedDataChannel_ClientDisconnected;
                m_serverBasedDataChannel.ServerStarted += m_serverBasedDataChannel_ServerStarted;
                m_serverBasedDataChannel.ServerStopped += m_serverBasedDataChannel_ServerStopped;

                // Listen for device connection
                m_serverBasedDataChannel.ReceiveClientDataHandler = Write;
                m_serverBasedDataChannel.ReceiveBufferSize = m_bufferSize;
                m_serverBasedDataChannel.ConfigurationString = m_connectionString;
                m_serverBasedDataChannel.MaxClientConnections = 1;
                m_serverBasedDataChannel.Handshake = false;
                m_serverBasedDataChannel.Start();
            }
            else
                throw new InvalidOperationException("No data channel was initialized, cannot start frame parser");
        }

        /// <summary>
        /// Stops the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;
            m_rateCalcTimer.Enabled = false;
            m_lastFrameReceivedTime = 0;
            m_configurationFrame = null;

            // Make sure data stream is disabled
            SendDeviceCommand(DeviceCommand.DisableRealTimeData);

            if (m_dataChannel != null)
            {
                m_dataChannel.Disconnect();
                m_dataChannel.ReceiveDataHandler = null;
                m_dataChannel.ConnectionEstablished -= m_dataChannel_ConnectionEstablished;
                m_dataChannel.ConnectionAttempt -= m_dataChannel_ConnectionAttempt;
                m_dataChannel.ConnectionException -= m_dataChannel_ConnectionException;
                m_dataChannel.ConnectionTerminated -= m_dataChannel_ConnectionTerminated;
                m_dataChannel.Dispose();
            }
            m_dataChannel = null;

            if (m_serverBasedDataChannel != null)
            {
                m_serverBasedDataChannel.DisconnectAll();
                m_serverBasedDataChannel.ReceiveClientDataHandler = null;
                m_serverBasedDataChannel.ClientConnected -= m_serverBasedDataChannel_ClientConnected;
                m_serverBasedDataChannel.ClientDisconnected -= m_serverBasedDataChannel_ClientDisconnected;
                m_serverBasedDataChannel.ServerStarted -= m_serverBasedDataChannel_ServerStarted;
                m_serverBasedDataChannel.ServerStopped -= m_serverBasedDataChannel_ServerStopped;
                m_serverBasedDataChannel.Dispose();
            }
            m_serverBasedDataChannel = null;

            if (m_commandChannel != null)
            {
                m_commandChannel.Disconnect();
                m_commandChannel.ReceiveDataHandler = null;
                m_commandChannel.ConnectionEstablished -= m_commandChannel_ConnectionEstablished;
                m_commandChannel.ConnectionAttempt -= m_commandChannel_ConnectionAttempt;
                m_commandChannel.ConnectionException -= m_commandChannel_ConnectionException;
                m_commandChannel.ConnectionTerminated -= m_commandChannel_ConnectionTerminated;
                m_commandChannel.Dispose();
            }
            m_commandChannel = null;

            if (m_frameParser != null)
            {
                m_frameParser.Stop();
                m_frameParser.ReceivedCommandFrame -= m_frameParser_ReceivedCommandFrame;
                m_frameParser.ReceivedConfigurationFrame -= m_frameParser_ReceivedConfigurationFrame;
                m_frameParser.ReceivedDataFrame -= m_frameParser_ReceivedDataFrame;
                m_frameParser.ReceivedHeaderFrame -= m_frameParser_ReceivedHeaderFrame;
                m_frameParser.ReceivedUndeterminedFrame -= m_frameParser_ReceivedUndeterminedFrame;
                m_frameParser.ReceivedFrameBufferImage -= m_frameParser_ReceivedFrameBufferImage;
                m_frameParser.ConfigurationChanged -= m_frameParser_ConfigurationChanged;
                m_frameParser.ParsingException -= m_frameParser_ParsingException;
                m_frameParser.Dispose();
            }
            m_frameParser = null;

            #if RawDataCapture
            if (m_rawDataCapture != null)
                m_rawDataCapture.Close();
            m_rawDataCapture = null;
            #endif
        }

        /// <summary>
        /// Sends the specified <see cref="DeviceCommand"/> to the remote device.
        /// </summary>
        /// <param name="command"><see cref="DeviceCommand"/> to send to the remote device.</param>
        /// <remarks>
        /// Command will only be sent if <see cref="DeviceSupportsCommands"/> is <c>true</c> and <see cref="MultiProtocolFrameParser"/>.
        /// </remarks>
        public virtual void SendDeviceCommand(DeviceCommand command)
        {
            if (m_deviceSupportsCommands && (m_dataChannel != null || m_serverBasedDataChannel != null || m_commandChannel != null))
            {
                ICommandFrame commandFrame;

                // Only the IEEE and SEL Fast Message protocols support commands
                switch (m_phasorProtocol)
                {
                    case PhasorProtocols.PhasorProtocol.IeeeC37_118V1:
                    case PhasorProtocols.PhasorProtocol.IeeeC37_118D6:
                        commandFrame = new IeeeC37_118.CommandFrame(m_deviceID, command, 1);
                        break;
                    case PhasorProtocols.PhasorProtocol.Ieee1344:
                        commandFrame = new Ieee1344.CommandFrame(m_deviceID, command);
                        break;
                    case PhasorProtocols.PhasorProtocol.SelFastMessage:
                        // Get defined message period
                        SelFastMessage.MessagePeriod messagePeriod = SelFastMessage.MessagePeriod.DefaultRate;
                        SelFastMessage.ConnectionParameters connectionParameters = m_connectionParameters as SelFastMessage.ConnectionParameters;

                        if (connectionParameters != null)
                            messagePeriod = connectionParameters.MessagePeriod;

                        commandFrame = new SelFastMessage.CommandFrame(command, messagePeriod);
                        break;
                    default:
                        commandFrame = null;
                        break;
                }

                if (commandFrame != null)
                {
                    byte[] buffer = commandFrame.BinaryImage;

                    // Send command over appropriate communications channel - command channel, if defined,
                    // will take precedence over other communications channels for command traffic...
                    if (m_commandChannel != null)
                        m_commandChannel.SendAsync(buffer, 0, buffer.Length);
                    else if (m_dataChannel != null)
                        m_dataChannel.SendAsync(buffer, 0, buffer.Length);
                    else
                        m_serverBasedDataChannel.MulticastAsync(buffer, 0, buffer.Length);

                    if (SentCommandFrame != null)
                        SentCommandFrame(this, new EventArgs<ICommandFrame>(commandFrame));
                }
            }
        }

        /// <summary>
        /// Writes data directly to the frame parsing engine buffer.
        /// </summary>
        /// <remarks>
        /// This method allows consumer to "manually send extra data" to the parsing engine to be parsed, if desired.
        /// </remarks>
        /// <param name="buffer">Buffer containing data to be parsed.</param>
        /// <param name="offset">Offset into buffer where data begins.</param>
        /// <param name="count">Length of data in buffer to be parsed.</param>
        public virtual void Write(byte[] buffer, int offset, int count)
        {
            // This is the delegate implementation used by the communication source for reception
            // of data directly from the socket (i.e., ReceiveDataHandler) that is used for a
            // speed boost in communications processing...

            #if RawDataCapture
            if (m_rawDataCapture == null)
                m_rawDataCapture = new FileStream(FilePath.GetAbsolutePath("RawData.Capture"), FileMode.Create);
            m_rawDataCapture.Write(buffer, offset, count);
            #endif

            // Pass data from communications client into protocol specific frame parser
            m_frameParser.Write(buffer, offset, count);
            
            m_byteRateTotal += count;

            if (m_initiatingDataStream)
                m_initialBytesReceived += count;
        }

        // Data received from a server will include a client ID - since in our case
        // the server will only host a single device, we ignore this ID
        private void Write(Guid clientID, byte[] buffer, int offset, int count)
        {
            Write(buffer, offset, count);
        }

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ParsingException"/> event.</param>
        protected virtual void OnParsingException(Exception ex)
        {
            if (ParsingException != null && !(ex is ThreadAbortException))
                ParsingException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="innerException">Actual exception to send as inner exception to <see cref="ParsingException"/> event.</param>
        /// <param name="message">Message of new exception to send to <see cref="ParsingException"/> event.</param>
        /// <param name="args">Arguments of message of new exception to send to <see cref="ParsingException"/> event.</param>
        protected virtual void OnParsingException(Exception innerException, string message, params object[] args)
        {
            if (!(innerException is ThreadAbortException))
                OnParsingException(new Exception(string.Format(message, args), innerException));
        }

        /// <summary>
        /// Derives a flag based on settings that determines if the current connection supports device commands.
        /// </summary>
        /// <returns>Derived flag that determines if the current connection supports device commands.</returns>
        protected virtual bool DeriveCommandSupport()
        {
            // Command support is based on phasor protocol, transport protocol and connection style
            if (IsIEEEProtocol || m_phasorProtocol == PhasorProtocol.SelFastMessage)
            {
                // IEEE protocols using TCP or Serial connection support device commands
                if (m_transportProtocol == TransportProtocol.Tcp || m_transportProtocol == TransportProtocol.Serial)
                    return true;

                if (!string.IsNullOrEmpty(m_connectionString))
                {
                    Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();

                    // A defined command channel inherently means commands are supported
                    if (settings.ContainsKey("commandchannel"))
                    {
                        return true;
                    }
                    else if (m_transportProtocol == TransportProtocol.Udp)
                    {
                        // IEEE protocols "can" use UDP connection to support devices commands, but only
                        // when remote device acts as a UDP listener (i.e., a "server" connection)
                        return settings.ContainsKey("server");
                    }
                }
            }

            return false;
        }

        // Starts data parsing sequence.
        private void StartDataParsingSequence(object state)
        {
            // This thread pool delegate is used to start streaming data on a remote device.
            int attempts = 0;

            // Some devices will only send a config frame once data streaming has been disabled, so
            // we use this code to disable real-time data and wait for data to stop streaming...
            try
            {
                // Make sure data stream is disabled
                SendDeviceCommand(DeviceCommand.DisableRealTimeData);
                Thread.Sleep(300);

                // Wait for real-time data stream to cease for up to two seconds
                while (m_initialBytesReceived > 0)
                {
                    m_initialBytesReceived = 0;
                    Thread.Sleep(100);

                    attempts++;
                    if (attempts >= 20)
                        break;
                }
            }
            finally
            {
                m_initiatingDataStream = false;
            }

            // Request configuration frame once real-time data has been disabled. Note that SEL Fast Message
            // doesn't define a binary configuration frame so skip requesting one...
            if (m_phasorProtocol != PhasorProtocol.SelFastMessage)
                SendDeviceCommand(DeviceCommand.SendConfigurationFrame2);
            else
                SendDeviceCommand(DeviceCommand.EnableRealTimeData);
        }

        // Calculate frame and data rates
        private void m_rateCalcTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            double time = Ticks.ToSeconds(DateTime.Now.Ticks - m_dataStreamStartTime);

            m_frameRate = (double)m_frameRateTotal / time;
            m_byteRate = (double)m_byteRateTotal / time;

            m_totalFramesReceived += m_frameRateTotal;
            m_totalBytesReceived += m_byteRateTotal;

            m_frameRateTotal = 0;
            m_byteRateTotal = 0;
            m_dataStreamStartTime = DateTime.Now.Ticks;
        }

        // Handles needed start-up actions once a client is connected
        private void ClientConnectedHandler()
        {
            if (ConnectionEstablished != null)
                ConnectionEstablished(this, EventArgs.Empty);

            // Begin data parsing sequence to handle reception of configuration frame
            if (m_deviceSupportsCommands && m_autoStartDataParsingSequence)
            {
                m_initialBytesReceived = 0;
                m_initiatingDataStream = true;
                ThreadPool.QueueUserWorkItem(StartDataParsingSequence, null);
            }
        }

        private void MaintainCapturedFrameReplayTiming()
        {
            if (m_lastFrameReceivedTime > 0)
            {
                // To maintain timing on "frames per second", we wait for defined frame rate interval
                double sleepTime = m_definedFrameRate - ((double)(DateTime.Now.Ticks - m_lastFrameReceivedTime) / (double)Ticks.PerSecond);

                // Thread sleep time is a minimum suggested sleep time depending on system activity, so we target 9/10 of a second
                // to make this a little more accurate. Since this is just used for replay, getting close is good enough - no need
                // to incur the overhead of using a PrecisionTimer here...
                if (sleepTime > 0)
                    Thread.Sleep((int)(sleepTime * 900.0D));
            }

            m_lastFrameReceivedTime = DateTime.Now.Ticks;
        }

        #region [ Communications Client Event Handlers ]

        private void m_dataChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            ClientConnectedHandler();
        }

        private void m_dataChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            m_connectionAttempts++;

            if (ConnectionAttempt != null)
                ConnectionAttempt(this, EventArgs.Empty);
        }

        private void m_dataChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            if (ConnectionException != null && !(e.Argument is ThreadAbortException))
                ConnectionException(this, new EventArgs<Exception,int>(e.Argument, m_connectionAttempts));
        }

        private void m_dataChannel_ConnectionTerminated(object sender, EventArgs e)
        {
            if (ConnectionTerminated != null)
                ConnectionTerminated(this, EventArgs.Empty);
        }

        #endregion

        #region [ Communications Server Event Handlers ]

        private void m_serverBasedDataChannel_ClientConnected(object sender, EventArgs<Guid> e)
        {
            ClientConnectedHandler();
        }

        private void m_serverBasedDataChannel_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            if (ConnectionTerminated != null)
                ConnectionTerminated(this, EventArgs.Empty);
        }

        private void m_serverBasedDataChannel_ServerStarted(object sender, EventArgs e)
        {
            if (ServerStarted != null)
                ServerStarted(this, EventArgs.Empty);
        }

        private void m_serverBasedDataChannel_ServerStopped(object sender, EventArgs e)
        {
            if (ServerStopped != null)
                ServerStopped(this, EventArgs.Empty);
        }

        #endregion

        #region [ Command Channel Event Handlers ]

        private void m_commandChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            ClientConnectedHandler();
        }

        private void m_commandChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            m_connectionAttempts++;

            if (ConnectionAttempt != null)
                ConnectionAttempt(this, EventArgs.Empty);
        }

        private void m_commandChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            if (ConnectionException != null && !(e.Argument is ThreadAbortException))
                ConnectionException(this, new EventArgs<Exception,int>(e.Argument, m_connectionAttempts));
        }

        private void m_commandChannel_ConnectionTerminated(object sender, EventArgs e)
        {
            if (ConnectionTerminated != null)
                ConnectionTerminated(this, EventArgs.Empty);
        }

        #endregion

        #region [ Frame Parser Event Handlers ]

        private void m_frameParser_ReceivedCommandFrame(object sender, EventArgs<ICommandFrame> e)
        {
            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                if (ReceivedCommandFrame != null)
                    ReceivedCommandFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedCommandFrame\" consumer event handler exception: {0}", ex.Message);
            }

            if (m_transportProtocol == TransportProtocol.File)
                MaintainCapturedFrameReplayTiming();
        }

        private void m_frameParser_ReceivedConfigurationFrame(object sender, EventArgs<IConfigurationFrame> e)
        {
            // We automatically request enabling of real-time data upon reception of config frame if requested. Note that SEL Fast Message will
            // have already been enabled at this point so we don't duplicate request for enabling real-time data stream
            if (m_configurationFrame == null && m_deviceSupportsCommands && m_autoStartDataParsingSequence && m_phasorProtocol != PhasorProtocol.SelFastMessage)
                SendDeviceCommand(DeviceCommand.EnableRealTimeData);

            m_frameRateTotal++;
            m_configurationFrame = e.Argument;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                if (ReceivedConfigurationFrame != null)
                    ReceivedConfigurationFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedConfigurationFrame\" consumer event handler exception: {0}", ex.Message);
            }

            if (m_transportProtocol == TransportProtocol.File)
                MaintainCapturedFrameReplayTiming();
        }

        private void m_frameParser_ReceivedDataFrame(object sender, EventArgs<IDataFrame> e)
        {
            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                if (ReceivedDataFrame != null)
                    ReceivedDataFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedDataFrame\" consumer event handler exception: {0}", ex.Message);
            }

            if (m_transportProtocol == TransportProtocol.File)
                MaintainCapturedFrameReplayTiming();
        }

        private void m_frameParser_ReceivedHeaderFrame(object sender, EventArgs<IHeaderFrame> e)
        {
            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                if (ReceivedHeaderFrame != null)
                    ReceivedHeaderFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedHeaderFrame\" consumer event handler exception: {0}", ex.Message);
            }

            if (m_transportProtocol == TransportProtocol.File)
                MaintainCapturedFrameReplayTiming();
        }

        private void m_frameParser_ReceivedUndeterminedFrame(object sender, EventArgs<IChannelFrame> e)
        {
            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                if (ReceivedUndeterminedFrame != null)
                    ReceivedUndeterminedFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedUndeterminedFrame\" consumer event handler exception: {0}", ex.Message);
            }

            if (m_transportProtocol == TransportProtocol.File)
                MaintainCapturedFrameReplayTiming();
        }

        private void m_frameParser_ReceivedFrameBufferImage(object sender, EventArgs<FundamentalFrameType, byte[], int, int> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (ReceivedFrameBufferImage != null)
                    ReceivedFrameBufferImage(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedFrameBufferImage\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ConfigurationChanged(object sender, EventArgs e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (ConfigurationChanged != null)
                    ConfigurationChanged(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ConfigurationChanged\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ParsingException(object sender, EventArgs<Exception> e)
        {
            OnParsingException(e.Argument);
        }

        #endregion

        #endregion
    }
}