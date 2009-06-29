//*******************************************************************************************************
//  SerialClient.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/24/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed serial port data access
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;

namespace TVA.Communication
{
    /// <summary>
    /// Represents a communication client based on <see cref="SerialPort"/>.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="SerialClient"/> for communicating with <see cref="SerialPort"/>:
    /// <code>
    /// using System;
    /// using TVA;
    /// using TVA.Communication;
    /// 
    /// class Program
    /// {
    ///     static SerialClient m_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         m_client = new SerialClient("Port=COM1; BaudRate=9600; Parity=None; StopBits=One; DataBits=8; DtrEnable=False; RtsEnable=False");
    ///         m_client.Initialize();
    ///         // Register event handlers.
    ///         m_client.ConnectionAttempt += m_client_ConnectionAttempt;
    ///         m_client.ConnectionEstablished += m_client_ConnectionEstablished;
    ///         m_client.ConnectionTerminated += m_client_ConnectionTerminated;
    ///         m_client.SendDataComplete += m_client_SendDataComplete;
    ///         m_client.ReceiveDataComplete += m_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         m_client.Connect();
    /// 
    ///         // Write user input to the serial port.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_client.Send(input);
    ///         }
    /// 
    ///         // Disconnect the client on shutdown.
    ///         m_client.Disconnect();
    ///     }
    /// 
    ///     static void m_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to serial port.");
    ///     }
    /// 
    ///     static void m_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to serial port.");
    ///     }
    /// 
    ///     static void m_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from serial port.");
    ///     }
    /// 
    ///     static void m_client_SendDataComplete(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine(string.Format("Sent data - {0}", m_client.TextEncoding.GetString(m_client.Client.SendBuffer)));
    ///     }
    /// 
    ///     static void m_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}", m_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class SerialClient : ClientBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Port=COM1; BaudRate=9600; Parity=None; StopBits=One; DataBits=8; DtrEnable=False; RtsEnable=False";

        // Fields
        private TransportProvider<SerialPort> m_serialClient;
        private Dictionary<string, string> m_connectData;
        private int m_receivedBytesThreshold;
#if ThreadTracking
        private ManagedThread m_connectionThread;
#else
        private Thread m_connectionThread;
#endif

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialClient"/> class.
        /// </summary>
        public SerialClient()
            : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="SerialClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public SerialClient(string connectString)
            : base(TransportProtocol.Serial, connectString)
        {
            m_serialClient = new TransportProvider<SerialPort>();
            m_receivedBytesThreshold = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="SerialClient"/>.</param>
        public SerialClient(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="TransportProvider{SerialPort}"/> object for the <see cref="SerialClient"/>.
        /// </summary>
        [Browsable(false)]
        public TransportProvider<SerialPort> Client
        {
            get
            {
                return m_serialClient;
            }
        }

        /// <summary>
        /// Gets the server URI of the <see cref="SerialClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri
        {
            get
            {
                return string.Format("{0}://{1}", TransportProtocol, m_connectData["port"]).ToLower();
            }
        }

        /// <summary>
        /// Gets or sets the needed number of bytes in the internal input buffer before a <see cref="ClientBase.OnReceiveDataComplete"/> event occurs.
        /// </summary>
        public int ReceivedBytesThreshold
        {
            get
            {
                return m_receivedBytesThreshold;
            }
            set
            {
                m_receivedBytesThreshold = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Disconnects the <see cref="SerialClient"/> from the <see cref="SerialPort"/>.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
                m_serialClient.Provider.DataReceived -= SerialPort_DataReceived;
                m_serialClient.Provider.ErrorReceived -= SerialPort_ErrorReceived;
                m_serialClient.Reset();

                if (m_connectionThread != null)
                    m_connectionThread.Abort();

                OnConnectionTerminated();
            }
        }

        /// <summary>
        /// Connects the <see cref="SerialClient"/> to the <see cref="SerialPort"/> asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="SerialClient"/> when it is connected.</exception>
        public override void ConnectAsync()
        {
            if (CurrentState == ClientState.Disconnected)
            {
                // Initialize if unitialized.
                Initialize();

                m_serialClient.ID = this.ClientID;
                m_serialClient.Passphrase = this.HandshakePassphrase;
                m_serialClient.ReceiveBuffer = new byte[ReceiveBufferSize];
                m_serialClient.Provider = new SerialPort();
                m_serialClient.Provider.ReceivedBytesThreshold = m_receivedBytesThreshold;
                m_serialClient.Provider.DataReceived += SerialPort_DataReceived;
                m_serialClient.Provider.ErrorReceived += SerialPort_ErrorReceived;
                m_serialClient.Provider.PortName = m_connectData["port"];
                m_serialClient.Provider.BaudRate = int.Parse(m_connectData["baudrate"]);
                m_serialClient.Provider.DataBits = int.Parse(m_connectData["databits"]);
                m_serialClient.Provider.Parity = (Parity)(Enum.Parse(typeof(Parity), m_connectData["parity"]));
                m_serialClient.Provider.StopBits = (StopBits)(Enum.Parse(typeof(StopBits), m_connectData["stopbits"]));

                if (m_connectData.ContainsKey("dtrenable"))
                    m_serialClient.Provider.DtrEnable = m_connectData["dtrenable"].ParseBoolean();
                if (m_connectData.ContainsKey("rtsenable"))
                    m_serialClient.Provider.RtsEnable = m_connectData["rtsenable"].ParseBoolean();

#if ThreadTracking
                m_connectionThread = new ManagedThread(OpenPort);
                m_connectionThread.Name = "TVA.Communication.SerialClient.OpenPort()";
#else
                m_connectionThread = new Thread(OpenPort);
#endif
                m_connectionThread.Start();
            }
            else
            {
                throw new InvalidOperationException("Client is currently not disconnected.");
            }
        }

        /// <summary>
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentException">BaudRate property is missing.</exception>
        /// <exception cref="ArgumentException">Parity property is missing.</exception>
        /// <exception cref="ArgumentException">StopBits property is missing.</exception>
        /// <exception cref="ArgumentException">DataBits property is missing.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            if (!m_connectData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port property is missing. Example: {0}.", DefaultConnectionString));

            if (!m_connectData.ContainsKey("baudrate"))
                throw new ArgumentException(string.Format("BaudRate property is missing. Example: {0}.", DefaultConnectionString));

            if (!m_connectData.ContainsKey("parity"))
                throw new ArgumentException(string.Format("Parity property is missing. Example: {0}.", DefaultConnectionString));

            if (!m_connectData.ContainsKey("stopbits"))
                throw new ArgumentException(string.Format("StopBits property is missing. Example: {0}.", DefaultConnectionString));

            if (!m_connectData.ContainsKey("databits"))
                throw new ArgumentException(string.Format("DataBits property is missing. Example: {0}.", DefaultConnectionString));
        }

        /// <summary>
        /// Gets the passphrase to be used for ciphering client data.
        /// </summary>
        /// <returns>Cipher passphrase.</returns>
        protected override string GetSessionPassphrase()
        {
            return m_serialClient.Passphrase;
        }

        /// <summary>
        /// Sends (writes) data to the <see cref="SerialPort"/> asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent (written).</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending (writing) data.</param>
        /// <param name="length">The number of bytes to be sent (written) from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            WaitHandle handle;

            // Send data to the file asynchronously.
            handle = m_serialClient.Provider.BaseStream.BeginWrite(data, offset, length, SendDataAsyncCallback, null).AsyncWaitHandle;

            // Notify that the send operation has started.
            m_serialClient.SendBuffer = data;
            m_serialClient.SendBufferOffset = offset;
            m_serialClient.SendBufferLength = length;
            m_serialClient.Statistics.UpdateBytesSent(m_serialClient.SendBufferLength);
            OnSendDataStart();

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle;
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void SendDataAsyncCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Send operation is complete.
                m_serialClient.Provider.BaseStream.EndWrite(asyncResult);
                OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
        }

        /// <summary>
        /// Connects to the <see cref="SerialPort"/>.
        /// </summary>
        private void OpenPort()
        {
            int connectionAttempts = 0;
            while (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts)
            {
                try
                {
                    OnConnectionAttempt();
                    m_serialClient.Provider.Open();
                    OnConnectionEstablished();

                    break;
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    connectionAttempts++;
                    OnConnectionException(ex);
                }
            }
        }

        /// <summary>
        /// Receive (read) data from the <see cref="SerialPort"/> (.NET serial port class raises this event when data is available).
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesRead = 0;

                while (bytesRead < m_serialClient.Provider.BytesToRead)
                {
                    // Retrieve data from the port.
                    bytesRead += m_serialClient.Provider.Read(m_serialClient.ReceiveBuffer, bytesRead, m_serialClient.ReceiveBuffer.Length - bytesRead);
                }

                m_serialClient.ReceiveBufferLength = bytesRead;
                m_serialClient.Statistics.UpdateBytesReceived(bytesRead);

                // Notify of the retrieved data.
                OnReceiveDataComplete(m_serialClient.ReceiveBuffer, bytesRead);
            }
            catch (Exception ex)
            {
                OnReceiveDataException(ex);
            }
        }

        /// <summary>
        /// Receive (read) error data from the <see cref="SerialPort"/> (.NET serial port class raises this event when error occurs).
        /// </summary>
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            switch (e.EventType)
            {
                case SerialError.Frame:
                    OnReceiveDataException(new ApplicationException("The hardware detected a framing error."));
                    break;
                case SerialError.Overrun:
                    OnReceiveDataException(new ApplicationException("A character-buffer overrun has occurred. The next character is lost."));
                    break;
                case SerialError.RXOver:
                    OnReceiveDataException(new ApplicationException("An input buffer overflow has occurred. There is either no room in the input buffer, or a character was received after the end-of-file (EOF) character."));
                    break;
                case SerialError.RXParity:
                    OnReceiveDataException(new ApplicationException("The hardware detected a parity error."));
                    break;
                case SerialError.TXFull:
                    OnReceiveDataException(new ApplicationException("The application tried to transmit a character, but the output buffer was full."));
                    break;
            }
        }

        #endregion
    }
}