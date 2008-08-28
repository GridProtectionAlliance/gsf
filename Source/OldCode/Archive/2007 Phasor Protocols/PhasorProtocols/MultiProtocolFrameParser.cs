//*******************************************************************************************************
//  MultiProtocolFrameParser.vb - Protocol independent frame parser
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
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
//       clients, e.g., FNET devices
//  05/23/2007 - Pinal C. Patel
//       Added member variable 'm_clientConnectionAttempts' to track the number of attempts made for
//       connecting to the server since this information is no longer provided by the event raised by
//       any of the Communication Client components
//  07/05/2007 - J. Ritchie Carroll
//       Wrapped all event raising for frame parsing in Try/Catch so that any exceptions thrown in
//       consumer event handlers won't have a negative effect on continuous data parsing - exceptions
//       in consumer event handlers are duly noted and raised through the DataStreamException event
//  09/28/2007 - J. Ritchie Carroll
//       Implemented new disconnect overload on communications client that allows timeout on socket
//       close to fix an issue related non-responsive threads that "lock-up" after sending connection
//       commands that attempt to close the socket for remotely connected devices
//  12/14/2007 - J. Ritchie Carroll
//       Implemented simulated timestamp injection for published frames to allow for real-time
//       data simulations from archived sample data
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;
using System.ComponentModel;
using TVA;
using TVA.Collections;
using TVA.Communication;
using PhasorProtocols;

namespace PhasorProtocols
{
    /// <summary>Protocol independent frame parser</summary>
    [CLSCompliant(false)]
    public class MultiProtocolFrameParser : IFrameParser
    {
        #region " Public Member Declarations "

        public event ReceivedConfigurationFrameEventHandler ReceivedConfigurationFrame;
        public event ReceivedDataFrameEventHandler ReceivedDataFrame;
        public event ReceivedHeaderFrameEventHandler ReceivedHeaderFrame;
        public event ReceivedCommandFrameEventHandler ReceivedCommandFrame;
        public event ReceivedUndeterminedFrameEventHandler ReceivedUndeterminedFrame;
        public event ReceivedFrameBufferImageEventHandler ReceivedFrameBufferImage;
        public event ConfigurationChangedEventHandler ConfigurationChanged;
        public event DataStreamExceptionEventHandler DataStreamException;

        public delegate void SentCommandFrameEventHandler(ICommandFrame frame);
        public event SentCommandFrameEventHandler SentCommandFrame;

        public delegate void ConnectionExceptionEventHandler(Exception ex, int connectionAttempts);
        public event ConnectionExceptionEventHandler ConnectionException;

        public delegate void AttemptingConnectionEventHandler();
        public event AttemptingConnectionEventHandler AttemptingConnection;

        public delegate void ConnectedEventHandler();
        public event ConnectedEventHandler Connected;

        public delegate void DisconnectedEventHandler();
        public event DisconnectedEventHandler Disconnected;

        public delegate void ServerStartedEventHandler();
        public event ServerStartedEventHandler ServerStarted;

        public delegate void ServerStoppedEventHandler();
        public event ServerStoppedEventHandler ServerStopped;

        public const int DefaultBufferSize = 262144; // 256K
        public const double DefaultFrameRate = 1.0D / 30.0D;

        #endregion

        #region " Private Member Declarations "

        // Connection properties
        private PhasorProtocol m_phasorProtocol;
        private TransportProtocol m_transportProtocol;
        private string m_connectionString;
        private int m_maximumConnectionAttempts;
        private ushort m_deviceID;
        private int m_bufferSize;

        // We internalize protocol specific processing to simplfy end user consumption
        private IFrameParser m_frameParser;
        private ICommunicationClient m_communicationClient;
        private ICommunicationServer m_communicationServer;
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
        private int m_clientConnectionAttempts;
        private bool m_disposed;

        #endregion

        #region " Construction Functions "

        public MultiProtocolFrameParser()
        {

            m_connectionString = "server=127.0.0.1; port=4712";
            m_deviceID = 1;
            m_bufferSize = DefaultBufferSize;
            m_definedFrameRate = DefaultFrameRate;
            m_rateCalcTimer = new System.Timers.Timer();
            m_maximumConnectionAttempts = -1;
            m_autoStartDataParsingSequence = true;

            m_phasorProtocol = PhasorProtocol.IeeeC37_118V1;
            m_transportProtocol = TransportProtocol.Tcp;

            m_rateCalcTimer.Elapsed += m_rateCalcTimer_Elapsed;
            m_rateCalcTimer.Interval = 5000;
            m_rateCalcTimer.AutoReset = true;
            m_rateCalcTimer.Enabled = false;

        }

        public MultiProtocolFrameParser(PhasorProtocol phasorProtocol, TransportProtocol transportProtocol)
            : this()
        {

            m_phasorProtocol = phasorProtocol;
            m_transportProtocol = transportProtocol;

        }

        ~MultiProtocolFrameParser()
        {

            Dispose(true);

        }

        protected virtual void Dispose(bool disposing)
        {

            if (!m_disposed)
            {
                if (disposing)
                {
                    Stop();
                    if (m_rateCalcTimer != null)
                    {
                        m_rateCalcTimer.Dispose();
                        m_rateCalcTimer.Elapsed -= m_rateCalcTimer_Elapsed;
                    }
                    m_rateCalcTimer = null;
                }
            }

            m_disposed = true;

        }

        public void Dispose()
        {

            Dispose(true);
            GC.SuppressFinalize(this);

        }

        #endregion

        #region " Public Methods Implementation "

        public PhasorProtocol PhasorProtocol
        {
            get
            {
                return m_phasorProtocol;
            }
            set
            {
                m_phasorProtocol = value;
                m_deviceSupportsCommands = GetDerivedCommandSupport();

                // Setup protocol specific connection parameters...
                switch (value)
                {
                    case PhasorProtocols.PhasorProtocol.BpaPdcStream:
                        m_connectionParameters = new BpaPdcStream.ConnectionParameters();
                        break;
                    case PhasorProtocols.PhasorProtocol.FNet:
                        m_connectionParameters = new FNet.ConnectionParameters();
                        break;
                    default:
                        m_connectionParameters = null;
                        break;
                }
            }
        }

        public TransportProtocol TransportProtocol
        {
            get
            {
                return m_transportProtocol;
            }
            set
            {
                // UDP transport has special requirements on buffer size
                if (value == TVA.Communication.TransportProtocol.Udp)
                {
                    if (m_bufferSize < UdpClient.MinimumUdpBufferSize)
                    {
                        m_bufferSize = UdpClient.MinimumUdpBufferSize;
                    }
                    if (m_bufferSize > UdpClient.MaximumUdpDatagramSize)
                    {
                        m_bufferSize = UdpClient.MaximumUdpDatagramSize;
                    }
                }
                else
                {
                    m_bufferSize = DefaultBufferSize;
                }

                m_transportProtocol = value;
                m_deviceSupportsCommands = GetDerivedCommandSupport();
            }
        }

        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
                m_deviceSupportsCommands = GetDerivedCommandSupport();
            }
        }

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

        public int BufferSize
        {
            get
            {
                return m_bufferSize;
            }
            set
            {
                m_bufferSize = value;

                // UDP has special restrictions on overall buffer size
                if (m_transportProtocol == TVA.Communication.TransportProtocol.Udp)
                {
                    if (m_bufferSize < UdpClient.MinimumUdpBufferSize)
                    {
                        m_bufferSize = UdpClient.MinimumUdpBufferSize;
                    }
                    if (m_bufferSize > UdpClient.MaximumUdpDatagramSize)
                    {
                        m_bufferSize = UdpClient.MaximumUdpDatagramSize;
                    }
                }
            }
        }

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

        public string ConnectionName
        {
            get
            {
                if (m_sourceName == null)
                {
                    return m_deviceID + " (" + m_connectionString + ")";
                }
                else
                {
                    return m_sourceName + ", ID " + m_deviceID + " (" + m_connectionString + ")";
                }
            }
        }

        public void Start()
        {

            Stop();
            m_totalFramesReceived = 0;
            m_frameRateTotal = 0;
            m_byteRateTotal = 0;
            m_totalBytesReceived = 0;
            m_frameRate = 0.0D;
            m_byteRate = 0.0D;

            try
            {
                // Instantiate protocol specific frame parser
                switch (m_phasorProtocol)
                {
                    case PhasorProtocols.PhasorProtocol.IeeeC37_118V1:
                        m_frameParser = new IeeeC37_118.FrameParser(IeeeC37_118.DraftRevision.Draft7);
                        break;
                    case PhasorProtocols.PhasorProtocol.IeeeC37_118D6:
                        m_frameParser = new IeeeC37_118.FrameParser(IeeeC37_118.DraftRevision.Draft6);
                        break;
                    case PhasorProtocols.PhasorProtocol.Ieee1344:
                        m_frameParser = new Ieee1344.FrameParser();
                        break;
                    case PhasorProtocols.PhasorProtocol.BpaPdcStream:
                        m_frameParser = new BpaPdcStream.FrameParser();
                        break;
                    case PhasorProtocols.PhasorProtocol.FNet:
                        m_frameParser = new FNet.FrameParser();
                        break;
                }

                // Setup event handlers
                m_frameParser.ReceivedCommandFrame += m_frameParser_ReceivedCommandFrame;
                m_frameParser.ReceivedConfigurationFrame += m_frameParser_ReceivedConfigurationFrame;
                m_frameParser.ReceivedDataFrame += m_frameParser_ReceivedDataFrame;
                m_frameParser.ReceivedHeaderFrame += m_frameParser_ReceivedHeaderFrame;
                m_frameParser.ReceivedUndeterminedFrame += m_frameParser_ReceivedUndeterminedFrame;
                m_frameParser.ReceivedFrameBufferImage += m_frameParser_ReceivedFrameBufferImage;
                m_frameParser.ConfigurationChanged += m_frameParser_ConfigurationChanged;
                m_frameParser.DataStreamException += m_frameParser_DataStreamException;

                m_frameParser.ConnectionParameters = m_connectionParameters;
                m_frameParser.ExecuteParseOnSeparateThread = m_executeParseOnSeparateThread;
                m_frameParser.Start();

                // Instantiate selected transport layer
                if (m_transportProtocol == TransportProtocol.Tcp)
                {
                    // The TCP transport may be set up as a server or as a client, we distinguish
                    // this simply by deriving the value of an added key/value pair in the
                    // connection string called "IsListener"
                    Dictionary<string, string> settings = TVA.Text.Common.ParseKeyValuePairs(m_connectionString);

                    if (settings.ContainsKey("islistener"))
                    {
                        if (TVA.Text.Common.ParseBoolean(settings["islistener"]))
                        {
                            m_communicationServer = new TcpServer();
                        }
                        else
                        {
                            m_communicationClient = new TcpClient();
                        }
                    }
                    else
                    {
                        // If the key doesn't exist, we assume it's a client connection
                        // (this way old connections strings are still backwards compatible)
                        m_communicationClient = new TcpClient();
                    }
                }
                else if (m_transportProtocol == TransportProtocol.Udp)
                {
                    m_communicationClient = new UdpClient();
                }
                else if (m_transportProtocol == TransportProtocol.Serial)
                {
                    m_communicationClient = new SerialClient();
                }
                else if (m_transportProtocol == TransportProtocol.File)
                {
                    // For file based playback, we allow the option of auto-repeat
                    FileClient fileClient = new FileClient();
                    fileClient.AutoRepeat = m_autoRepeatCapturedPlayback;
                    m_communicationClient = fileClient;
                }

                if (m_communicationClient != null)
                {
                    // Setup event handlers
                    m_communicationClient.Connected += m_communicationClient_Connected;
                    m_communicationClient.Connecting += m_communicationClient_Connecting;
                    m_communicationClient.ConnectingException += m_communicationClient_ConnectingException;
                    m_communicationClient.Disconnected += m_communicationClient_Disconnected;

                    // Attempting connection to device
                    m_communicationClient.ReceiveRawDataFunction = IFrameParserWrite;
                    m_communicationClient.ReceiveBufferSize = m_bufferSize;
                    m_communicationClient.ConnectionString = m_connectionString;
                    m_communicationClient.MaximumConnectionAttempts = m_maximumConnectionAttempts;
                    m_communicationClient.Handshake = false;
                    m_communicationClient.Connect();
                    m_clientConnectionAttempts = 0;
                }
                else if (m_communicationServer != null)
                {
                    // Setup event handlers
                    m_communicationServer.ClientConnected += m_communicationServer_ClientConnected;
                    m_communicationServer.ClientDisconnected += m_communicationServer_ClientDisconnected;
                    m_communicationServer.ServerStarted += m_communicationServer_ServerStarted;
                    m_communicationServer.ServerStopped += m_communicationServer_ServerStopped;
                    m_communicationServer.ServerStartupException += m_communicationServer_ServerStartupException;

                    // Listening for device connection
                    m_communicationServer.ReceiveRawDataFunction = IFrameParserWrite;
                    m_communicationServer.ReceiveBufferSize = m_bufferSize;
                    m_communicationServer.ConfigurationString = m_connectionString;
                    m_communicationServer.MaximumClients = 1;
                    m_communicationServer.Handshake = false;
                    m_communicationServer.Start();
                }
                else
                {
                    throw (new InvalidOperationException("No communications layer was initialized, cannot start parser"));
                }

                m_rateCalcTimer.Enabled = true;
                m_enabled = true;
            }
            catch
            {
                Stop();
                throw;
            }

        }

        public void Stop()
        {

            m_enabled = false;
            m_rateCalcTimer.Enabled = false;

            if (m_communicationClient != null)
            {
                m_communicationClient.Dispose();
            }
            if (m_communicationServer != null)
            {
                m_communicationServer.Dispose();
            }
            if (m_frameParser != null)
            {
                m_frameParser.Dispose();
            }

            m_lastFrameReceivedTime = 0;
            m_configurationFrame = null;

            if (m_frameParser != null)
            {
                m_frameParser.ReceivedCommandFrame -= m_frameParser_ReceivedCommandFrame;
                m_frameParser.ReceivedConfigurationFrame -= m_frameParser_ReceivedConfigurationFrame;
                m_frameParser.ReceivedDataFrame -= m_frameParser_ReceivedDataFrame;
                m_frameParser.ReceivedHeaderFrame -= m_frameParser_ReceivedHeaderFrame;
                m_frameParser.ReceivedUndeterminedFrame -= m_frameParser_ReceivedUndeterminedFrame;
                m_frameParser.ReceivedFrameBufferImage -= m_frameParser_ReceivedFrameBufferImage;
                m_frameParser.ConfigurationChanged -= m_frameParser_ConfigurationChanged;
                m_frameParser.DataStreamException -= m_frameParser_DataStreamException;
            }
            m_frameParser = null;

            if (m_communicationClient != null)
            {
                m_communicationClient.Connected -= m_communicationClient_Connected;
                m_communicationClient.Connecting -= m_communicationClient_Connecting;
                m_communicationClient.ConnectingException -= m_communicationClient_ConnectingException;
                m_communicationClient.Disconnected -= m_communicationClient_Disconnected;
            }
            m_communicationClient = null;

            if (m_communicationServer != null)
            {
                m_communicationServer.ClientConnected -= m_communicationServer_ClientConnected;
                m_communicationServer.ClientDisconnected -= m_communicationServer_ClientDisconnected;
                m_communicationServer.ServerStarted -= m_communicationServer_ServerStarted;
                m_communicationServer.ServerStopped -= m_communicationServer_ServerStopped;
                m_communicationServer.ServerStartupException -= m_communicationServer_ServerStartupException;
            }
            m_communicationServer = null;

        }

        public bool ExecuteParseOnSeparateThread
        {
            get
            {
                return m_executeParseOnSeparateThread;
            }
            set
            {
                m_executeParseOnSeparateThread = value;

                // Since frame parsers support dynamic changes in this value, we'll pass this value along to the
                // the frame parser if one has been established...
                if (m_frameParser != null)
                {
                    m_frameParser.ExecuteParseOnSeparateThread = value;
                }
            }
        }

        public bool AutoRepeatCapturedPlayback
        {
            get
            {
                return m_autoRepeatCapturedPlayback;
            }
            set
            {
                m_autoRepeatCapturedPlayback = value;
            }
        }

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
                {
                    m_frameParser.ConfigurationFrame = value;
                }
            }
        }

        public bool IsIEEEProtocol
        {
            get
            {
                return m_phasorProtocol == PhasorProtocols.PhasorProtocol.IeeeC37_118V1 || m_phasorProtocol == PhasorProtocols.PhasorProtocol.IeeeC37_118D6 || m_phasorProtocol == PhasorProtocols.PhasorProtocol.Ieee1344;
            }
        }

        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
        }

        public int QueuedBuffers
        {
            get
            {
                if (m_frameParser != null)
                {
                    return m_frameParser.QueuedBuffers;
                }

                return 0;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IFrameParser InternalFrameParser
        {
            get
            {
                return m_frameParser;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommunicationClient InternalCommunicationClient
        {
            get
            {
                return m_communicationClient;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommunicationServer InternalCommunicationServer
        {
            get
            {
                return m_communicationServer;
            }
        }

        public long TotalFramesReceived
        {
            get
            {
                return m_totalFramesReceived;
            }
        }

        public long TotalBytesReceived
        {
            get
            {
                return m_totalBytesReceived;
            }
        }

        public double FrameRate
        {
            get
            {
                return m_frameRate;
            }
        }

        public double ByteRate
        {
            get
            {
                return m_byteRate;
            }
        }

        public double BitRate
        {
            get
            {
                return m_byteRate * 8.0D;
            }
        }

        public double KiloBitRate
        {
            get
            {
                return m_byteRate * 8.0D / 1024.0D;
            }
        }

        public double MegaBitRate
        {
            get
            {
                return m_byteRate * 8.0D / 1048576.0D;
            }
        }

        public void SendDeviceCommand(DeviceCommand command)
        {

            if (m_deviceSupportsCommands && ((m_communicationClient != null) || (m_communicationServer != null)))
            {
                PhasorProtocols.ICommandFrame commandFrame;

                // Only the IEEE protocols support commands
                switch (m_phasorProtocol)
                {
                    case PhasorProtocols.PhasorProtocol.IeeeC37_118V1:
                    case PhasorProtocols.PhasorProtocol.IeeeC37_118D6:
                        commandFrame = new IeeeC37_118.CommandFrame(m_deviceID, command, 1);
                        break;
                    case PhasorProtocols.PhasorProtocol.Ieee1344:
                        commandFrame = new Ieee1344.CommandFrame(m_deviceID, command);
                        break;
                    default:
                        commandFrame = null;
                        break;
                }

                if (commandFrame != null)
                {
                    if (m_communicationClient != null)
                    {
                        m_communicationClient.Send(commandFrame.BinaryImage);
                    }
                    else
                    {
                        m_communicationServer.Multicast(commandFrame.BinaryImage);
                    }

                    //if (SentCommandFrameEvent != null)
                    //    SentCommandFrameEvent(commandFrame);
                    if (SentCommandFrame != null)
                        SentCommandFrame(commandFrame);
                }
            }

        }

        public string Status
        {
            get
            {
                System.Text.StringBuilder statusText = new StringBuilder();

                statusText.Append("      Device Connection ID: ");
                statusText.Append(m_deviceID);
                statusText.AppendLine();
                statusText.Append("         Connection string: ");
                statusText.Append(m_connectionString);
                statusText.AppendLine();
                statusText.Append("           Phasor protocol: ");
                statusText.Append(Common.GetFormattedProtocolName(PhasorProtocol));
                statusText.AppendLine();
                statusText.Append("               Buffer size: ");
                statusText.Append(m_bufferSize);
                statusText.AppendLine();
                statusText.Append("     Total frames received: ");
                statusText.Append(m_totalFramesReceived);
                statusText.AppendLine();
                statusText.Append("     Calculated frame rate: ");
                statusText.Append(m_frameRate);
                statusText.AppendLine();
                statusText.Append("      Calculated byte rate: ");
                statusText.Append(m_byteRate);
                statusText.AppendLine();
                statusText.Append("   Calculated MegaBit rate: ");
                statusText.Append(MegaBitRate.ToString("0.0000") + " mbps");
                statusText.AppendLine();

                if (m_frameParser != null)
                {
                    statusText.Append(m_frameParser.Status);
                }
                if (m_communicationClient != null)
                {
                    statusText.Append(m_communicationClient.Status);
                }
                if (m_communicationServer != null)
                {
                    statusText.Append(m_communicationServer.Status);
                }

                return statusText.ToString();
            }
        }

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
                {
                    m_frameParser.ConnectionParameters = value;
                }
            }
        }

        #endregion

        #region " Private Methods Implementation "

        private void m_rateCalcTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            double time = TVA.DateTime.Common.get_TicksToSeconds(DateTime.Now.Ticks - m_dataStreamStartTime);

            m_frameRate = (double)m_frameRateTotal / time;
            m_byteRate = (double)m_byteRateTotal / time;

            m_totalFramesReceived += m_frameRateTotal;
            m_totalBytesReceived += m_byteRateTotal;

            m_frameRateTotal = 0;
            m_byteRateTotal = 0;
            m_dataStreamStartTime = DateTime.Now.Ticks;

        }

        // We access the "raw data function" of the ICommunicationClient and ICommunicationServer for a speed boost in communications processing...
        public void Write(byte[] buffer, int offset, int count)
        {
            this.IFrameParserWrite(buffer, offset, count);
        }

        public void IFrameParserWrite(byte[] buffer, int offset, int count)
        {

            // Pass data from communications client into protocol specific frame parser
            m_frameParser.Write(buffer, offset, count);
            m_byteRateTotal += count;
            if (m_initiatingDataStream)
            {
                m_initialBytesReceived += count;
            }

        }

        private void ClientConnected()
        {

            if (Connected != null)
                Connected();

            // Begin data parsing sequence to handle reception of configuration frame
            if (m_deviceSupportsCommands && m_autoStartDataParsingSequence)
            {
                m_initialBytesReceived = 0;
                m_initiatingDataStream = true;

#if ThreadTracking
				TVA.Threading.ManagedThread thread = TVA.Threading.ManagedThreadPool.QueueUserWorkItem(StartDataParsingSequence);
				thread.Name = "PhasorProtocols.MultiProtocolFrameParser.StartDataParsingSequence()";
#else
                ThreadPool.UnsafeQueueUserWorkItem(StartDataParsingSequence, null);
#endif
            }

        }

        private void StartDataParsingSequence(object state)
        {

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
                    {
                        break;
                    }
                }
            }
            finally
            {
                m_initiatingDataStream = false;
            }

            // Request configuration frame once real-time data has been disabled
            SendDeviceCommand(DeviceCommand.SendConfigurationFrame2);

        }

        private bool GetDerivedCommandSupport()
        {

            // Command support is based on phasor protocol, transport protocol and connection style
            if (IsIEEEProtocol)
            {
                // IEEE protocols using TCP or Serial connection support device commands
                if (m_transportProtocol == TransportProtocol.Tcp || m_transportProtocol == TransportProtocol.Serial)
                {
                    return true;
                }

                // IEEE protocols "can" use UDP connection to support devices commands, but only
                // when remote device acts as a UDP listener (i.e., a "server" connection)
                if (m_transportProtocol == TransportProtocol.Udp)
                {
                    if (!string.IsNullOrEmpty(m_connectionString))
                    {
                        return TVA.Text.Common.ParseKeyValuePairs(m_connectionString).ContainsKey("server");
                    }
                }
            }

            return false;

        }

        private void MaintainCapturedFrameReplayTiming()
        {

            if (m_lastFrameReceivedTime > 0)
            {
                // To keep precise timing on "frames per second", we wait for defined frame rate interval
                double sleepTime = m_definedFrameRate - ((double)(DateTime.Now.Ticks - m_lastFrameReceivedTime) / (double)TVA.DateTime.Common.TicksPerSecond);
                //double sleepTime = m_definedFrameRate - ((double)(TVA.DateTime.PrecisionTimer.UtcNow.Ticks - m_lastFrameReceivedTime) / (double)TVA.DateTime.Common.TicksPerSecond);

                if (sleepTime > 0)
                {
                    Thread.Sleep((int)(sleepTime * 900.0D));
                }
            }

            m_lastFrameReceivedTime = DateTime.Now.Ticks;
            //m_lastFrameReceivedTime = TVA.DateTime.PrecisionTimer.UtcNow.Ticks;

        }

        #region " Communications Client Event Handlers "

        private void m_communicationClient_Connected(System.Object sender, System.EventArgs e)
        {

            ClientConnected();

        }

        private void m_communicationClient_Connecting(object sender, System.EventArgs e)
        {

            m_clientConnectionAttempts++;
            if (AttemptingConnection != null)
                AttemptingConnection();

        }

        private void m_communicationClient_ConnectingException(object sender, GenericEventArgs<System.Exception> e)
        {

            if (ConnectionException != null)
                ConnectionException(e.Argument, m_clientConnectionAttempts);

        }

        private void m_communicationClient_Disconnected(object sender, System.EventArgs e)
        {

            if (Disconnected != null)
                Disconnected();

            if (m_communicationClient != null)
            {
                m_communicationClient.Connected -= m_communicationClient_Connected;
                m_communicationClient.Connecting -= m_communicationClient_Connecting;
                m_communicationClient.ConnectingException -= m_communicationClient_ConnectingException;
                m_communicationClient.Disconnected -= m_communicationClient_Disconnected;
            }
            m_communicationClient = null;

        }

        #endregion

        #region " Communications Server Event Handlers "

        private void m_communicationServer_ClientConnected(object sender, GenericEventArgs<System.Guid> e)
        {

            ClientConnected();

        }

        private void m_communicationServer_ClientDisconnected(object sender, GenericEventArgs<System.Guid> e)
        {

            if (Disconnected != null)
                Disconnected();

        }

        private void m_communicationServer_ServerStarted(object sender, System.EventArgs e)
        {

            if (ServerStarted != null)
                ServerStarted();

        }

        private void m_communicationServer_ServerStopped(object sender, System.EventArgs e)
        {

            if (ServerStopped != null)
                ServerStopped();

        }

        private void m_communicationServer_ServerStartupException(object sender, GenericEventArgs<System.Exception> e)
        {

            if (ConnectionException != null)
                ConnectionException(e.Argument, 1);

        }

        #endregion

        #region " Frame Parser Event Handlers "

        private void m_frameParser_ReceivedCommandFrame(ICommandFrame frame)
        {

            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                {
                    frame.Ticks = DateTime.UtcNow.Ticks;
                    //frame.Ticks = TVA.DateTime.PrecisionTimer.UtcNow.Ticks;
                }
                if (ReceivedCommandFrame != null)
                    ReceivedCommandFrame(frame);
            }
            catch (Exception ex)
            {
                if (DataStreamException != null)
                    DataStreamException(new Exception(string.Format("MultiProtocolFrameParser Consumer \"ReceivedCommandFrame\" Event Handler Exception: {0}", ex.Message), ex));
            }

            if (m_transportProtocol == TransportProtocol.File)
            {
                MaintainCapturedFrameReplayTiming();
            }

        }

        private void m_frameParser_ReceivedConfigurationFrame(IConfigurationFrame frame)
        {

            // We automatically request enabling of real-time data upon reception of config frame if requested
            if (m_configurationFrame == null && m_deviceSupportsCommands && m_autoStartDataParsingSequence)
            {
                SendDeviceCommand(DeviceCommand.EnableRealTimeData);
            }

            m_frameRateTotal++;
            m_configurationFrame = frame;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                {
                    frame.Ticks = DateTime.UtcNow.Ticks;
                    //frame.Ticks = TVA.DateTime.PrecisionTimer.UtcNow.Ticks;
                }
                if (ReceivedConfigurationFrame != null)
                    ReceivedConfigurationFrame(frame);
            }
            catch (Exception ex)
            {
                if (DataStreamException != null)
                    DataStreamException(new Exception(string.Format("MultiProtocolFrameParser Consumer \"ReceivedConfigurationFrame\" Event Handler Exception: {0}", ex.Message), ex));
            }

            if (m_transportProtocol == TransportProtocol.File)
            {
                MaintainCapturedFrameReplayTiming();
            }

        }

        private void m_frameParser_ReceivedDataFrame(IDataFrame frame)
        {

            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                {
                    frame.Ticks = DateTime.UtcNow.Ticks;
                    //frame.Ticks = TVA.DateTime.PrecisionTimer.UtcNow.Ticks;
                }
                if (ReceivedDataFrame != null)
                    ReceivedDataFrame(frame);
            }
            catch (Exception ex)
            {
                if (DataStreamException != null)
                    DataStreamException(new Exception(string.Format("MultiProtocolFrameParser Consumer \"ReceivedDataFrame\" Event Handler Exception: {0}", ex.Message), ex));
            }

            if (m_transportProtocol == TransportProtocol.File)
            {
                MaintainCapturedFrameReplayTiming();
            }

        }

        private void m_frameParser_ReceivedHeaderFrame(IHeaderFrame frame)
        {

            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                {
                    frame.Ticks = DateTime.UtcNow.Ticks;
                    //frame.Ticks = TVA.DateTime.PrecisionTimer.UtcNow.Ticks;
                }
                if (ReceivedHeaderFrame != null)
                    ReceivedHeaderFrame(frame);
            }
            catch (Exception ex)
            {
                if (DataStreamException != null)
                    DataStreamException(new Exception(string.Format("MultiProtocolFrameParser Consumer \"ReceivedHeaderFrame\" Event Handler Exception: {0}", ex.Message), ex));
            }

            if (m_transportProtocol == TransportProtocol.File)
            {
                MaintainCapturedFrameReplayTiming();
            }

        }

        private void m_frameParser_ReceivedUndeterminedFrame(IChannelFrame frame)
        {

            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                {
                    frame.Ticks = DateTime.UtcNow.Ticks;
                    //frame.Ticks = TVA.DateTime.PrecisionTimer.UtcNow.Ticks;
                }
                if (ReceivedUndeterminedFrame != null)
                    ReceivedUndeterminedFrame(frame);
            }
            catch (Exception ex)
            {
                if (DataStreamException != null)
                    DataStreamException(new Exception(string.Format("MultiProtocolFrameParser Consumer \"ReceivedUndeterminedFrame\" Event Handler Exception: {0}", ex.Message), ex));
            }

            if (m_transportProtocol == TransportProtocol.File)
            {
                MaintainCapturedFrameReplayTiming();
            }

        }

        private void m_frameParser_ReceivedFrameBufferImage(FundamentalFrameType frameType, byte[] binaryImage, int offset, int length)
        {

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (ReceivedFrameBufferImage != null)
                    ReceivedFrameBufferImage(frameType, binaryImage, offset, length);
            }
            catch (Exception ex)
            {
                if (DataStreamException != null)
                    DataStreamException(new Exception(string.Format("MultiProtocolFrameParser Consumer \"ReceivedFrameBufferImage\" Event Handler Exception: {0}", ex.Message), ex));
            }

        }

        private void m_frameParser_ConfigurationChanged()
        {

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (ConfigurationChanged != null)
                    ConfigurationChanged();
            }
            catch (Exception ex)
            {
                if (DataStreamException != null)
                    DataStreamException(new Exception(string.Format("MultiProtocolFrameParser Consumer \"ConfigurationChanged\" Event Handler Exception: {0}", ex.Message), ex));
            }

        }

        private void m_frameParser_DataStreamException(System.Exception ex)
        {

            if (DataStreamException != null)
                DataStreamException(ex);

        }

        #endregion

        #region " Old Socket Code "

        //Private m_socketThread As Thread
        //Private m_tcpSocket As Sockets.TcpClient
        //Private m_udpSocket As Socket
        //Private m_receptionPoint As EndPoint
        //Private m_clientStream As NetworkStream

        //' Validate minimal connection parameters required for TCP connection
        //If String.IsNullOrEmpty(m_hostIP) Then Throw New InvalidOperationException("Cannot start TCP stream listener without specifing a host IP")
        //If m_port = 0 Then Throw New InvalidOperationException("Cannot start TCP stream listener without specifing a port")

        //' Connect to PDC/PMU using TCP
        //m_tcpSocket = New Sockets.TcpClient
        //m_tcpSocket.ReceiveBufferSize = m_bufferSize
        //m_tcpSocket.Connect(m_hostIP, m_port)
        //m_clientStream = m_tcpSocket.GetStream()

        //' Start listening to TCP data stream
        //m_socketThread = New Thread(AddressOf ProcessTcpStream)
        //m_socketThread.Start()

        //' Validate minimal connection parameters required for UDP connection
        //If m_port = 0 Then Throw New InvalidOperationException("Cannot start UDP stream listener without specifing a valid port")

        //' Connect to PDC/PMU using UDP (just listening to incoming stream on specified port)
        //m_udpSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        //m_receptionPoint = CType(New IPEndPoint(IPAddress.Any, m_port), System.Net.EndPoint)
        //m_udpSocket.ReceiveBufferSize = m_bufferSize
        //m_udpSocket.Bind(m_receptionPoint)

        //' Start listening to UDP data stream
        //m_socketThread = New Thread(AddressOf ProcessUdpStream)
        //m_socketThread.Start()

        //If m_socketThread IsNot Nothing Then m_socketThread.Abort()
        //m_socketThread = Nothing

        //If m_tcpSocket IsNot Nothing Then m_tcpSocket.Close()
        //m_tcpSocket = Nothing

        //If m_udpSocket IsNot Nothing Then m_udpSocket.Close()
        //m_udpSocket = Nothing

        //m_clientStream = Nothing
        //m_receptionPoint = Nothing

        //Private Sub ProcessUdpStream()

        //Dim buffer As Byte() = CreateArray(Of Byte)(m_bufferSize)
        //Dim received As int

        //' Enter the data read loop
        //Do While True
        //    Try
        //        ' Block thread until we've received some data...
        //        received = m_udpSocket.ReceiveFrom(buffer, m_receptionPoint)

        //        ' Provide received buffer to protocol specific frame parser
        //        If received > 0 Then Write(buffer, 0, received)
        //    Catch ex As ThreadAbortException
        //        ' If we received an abort exception, we'll egress gracefully
        //        Exit Do
        //    Catch ex As IOException
        //        ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
        //        ' in this case we'll bow out gracefully as well...
        //        Exit Do
        //    Catch ex As Exception
        //        RaiseEvent DataStreamException(ex)
        //        Exit Do
        //    End Try
        //Loop

        //End Sub

        //Private Sub ProcessTcpStream()

        //Dim buffer As Byte() = CreateArray(Of Byte)(m_bufferSize)
        //Dim received, attempts As Integer

        //' Handle reception of configuration frame - in case of device that only responds to commands when not sending real-time data,
        //' such as the SEL 421, we disable real-time data stream first...
        //Try
        //    ' Make sure data stream is disabled
        //    SendPmuCommand(DeviceCommand.DisableRealTimeData)

        //    ' Wait for real-time data stream to cease
        //    Do While m_clientStream.DataAvailable
        //        ' Remove all existing data from stream
        //        Do While m_clientStream.DataAvailable
        //            received = m_clientStream.Read(buffer, 0, buffer.Length)
        //        Loop

        //        Thread.Sleep(100)

        //        attempts += 1
        //        If attempts >= 50 Then Exit Do
        //    Loop

        //    ' Request configuration frame 2 (we'll try a few times)
        //    attempts = 0
        //    m_configurationFrame = Nothing

        //    For x As Integer = 1 To 4
        //        SendPmuCommand(DeviceCommand.SendConfigurationFrame2)

        //        Do While m_configurationFrame Is Nothing
        //            ' So long as we are receiving data, we'll push it to the frame parser
        //            Do While m_clientStream.DataAvailable
        //                ' Block thread until we've read some data...
        //                received = m_clientStream.Read(buffer, 0, buffer.Length)

        //                ' Send received data to frame parser
        //                If received > 0 Then Write(buffer, 0, received)
        //            Loop

        //            ' Hang out for a little while so config frame can be parsed
        //            Thread.Sleep(100)

        //            attempts += 1
        //            If attempts >= 50 Then Exit Do
        //        Loop

        //        If m_configurationFrame IsNot Nothing Then Exit For
        //    Next

        //    ' Enable data stream
        //    SendPmuCommand(DeviceCommand.EnableRealTimeData)
        //Catch ex As ThreadAbortException
        //    ' If we received an abort exception, we'll egress gracefully
        //    Exit Sub
        //Catch ex As IOException
        //    ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
        //    ' in this case we'll bow out gracefully as well...
        //    Exit Sub
        //Catch ex As Exception
        //    RaiseEvent DataStreamException(ex)
        //    Exit Sub
        //End Try

        //' Enter the data read loop
        //Do While True
        //    Try
        //        ' Block thread until we've received some data...
        //        received = m_clientStream.Read(buffer, 0, buffer.Length)

        //        ' Provide received buffer to protocol specific frame parser
        //        If received > 0 Then Write(buffer, 0, received)
        //    Catch ex As ThreadAbortException
        //        ' If we received an abort exception, we'll egress gracefully
        //        Exit Do
        //    Catch ex As IOException
        //        ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
        //        ' in this case we'll bow out gracefully as well...
        //        Exit Do
        //    Catch ex As Exception
        //        RaiseEvent DataStreamException(ex)
        //        Exit Do
        //    End Try
        //Loop

        //End Sub

        //Private Sub m_communicationClient_ReceivedData(ByVal sender As Object, ByVal e As DataEventArgs) Handles m_communicationClient.ReceivedData

        //    With e
        //        ' Pass data from communications client into protocol specific frame parser
        //        m_frameParser.Write(.Data, 0, .Data.Length)
        //        m_byteRateTotal += .Data.Length
        //        If m_initiatingDataStream Then m_initialBytesReceived += .Data.Length
        //    End With

        //End Sub

        #endregion

        #endregion

    }
}
