//*******************************************************************************************************
//  SerialServer.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
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
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using PCS.Configuration;
using PCS.Threading;

namespace PCS.Communication
{
    /// <summary>
    /// Represents a communication client based on <see cref="SerialPort"/>.
    /// </summary>
    public class SerialClient : ClientBase
    {

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Port=COM1; BaudRate=9600; Parity=None; StopBits=One; DataBits=8; DtrEnable=False; RtsEnable=False";

        #region [ Members ]

        // Fields
        private TransportProvider<SerialPort> m_serialClient;
        private Dictionary<string, string> m_connectData;
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
        public SerialClient(string connectionString)
            : base(TransportProtocol.Serial, connectionString)
        {
            m_serialClient = new TransportProvider<SerialPort>();
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Disconnects the <see cref="SerialClient"/> from the <see cref="SerialPort"/>.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
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
                m_serialClient.ID = this.ClientID;
                m_serialClient.Passphrase = this.HandshakePassphrase;
                m_serialClient.ReceiveBuffer = new byte[ReceiveBufferSize];
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
                m_connectionThread.Name = "PCS.Communication.SerialClient.OpenPort()";
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

        ///// <summary>
        ///// Sends prepared data to the server.
        ///// </summary>
        ///// <param name="data">The prepared data that is to be sent to the server.</param>
        //protected override void SendPreparedData(byte[] data)
        //{
        //    if (base.Enabled && base.IsConnected)
        //    {
        //        OnSendDataBegin(new IdentifiableItem<Guid, byte[]>(ClientID, data));
        //        m_serialClient.Write(data, 0, data.Length);
        //        OnSendDataComplete(new IdentifiableItem<Guid, byte[]>(ClientID, data));
        //    }
        //}

        /// <summary>
        /// Sends (writes) data to the file asynchronously.
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
        /// Connects to the serial port.
        /// </summary>
        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
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
        /// Receive data from the serial port (.NET serial port class raises this event when data is available)
        /// </summary>
        private void m_serialClient_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Retrieve data from the port.
            m_serialClient.ReceiveBufferLength = m_serialClient.Provider.Read(m_serialClient.ReceiveBuffer,  m_serialClient.ReceiveBufferOffset, m_serialClient.ReceiveBuffer.Length);
            m_serialClient.Statistics.UpdateBytesReceived(m_serialClient.ReceiveBufferLength);

            // Notify of the retrieved data.
            OnReceiveDataComplete(m_serialClient.ReceiveBuffer, m_serialClient.ReceiveBufferLength);

            //int received;
            //// JRC: modified this code to make sure all available data on the serial port buffer is read, regardless of size of communication buffer
            //for (int x = 1; x <= (int)(System.Math.Ceiling((double)m_serialClient.BytesToRead / m_buffer.Length)); x++)
            //{
            //    // Retrieve data from the serial port
            //    received = m_serialClient.Read(m_buffer, 0, m_buffer.Length);

            //    // Post raw data to real-time function delegate if defined - this bypasses all other activity
            //    if (m_receiveRawDataFunction != null)
            //    {
            //        m_receiveRawDataFunction(m_buffer, 0, received);
            //        m_totalBytesReceived += received;
            //    }
            //    else
            //    {
            //        // Unpack data and make available via event
            //        OnReceivedData(new IdentifiableItem<Guid, byte[]>(ServerID, m_buffer.CopyBuffer(0, received)));
            //    }
            //}
        }

        #endregion
    }
}

















//using System.Threading;
//using System.ComponentModel;
//using System.Collections.Generic;
//using PCS.Configuration;
//using PCS.Threading;

//namespace PCS.Communication
//{
//    /// <summary>
//    /// Represents a communication client based on <see cref="SerialPort"/>.
//    /// </summary>
//    public class SerialClient : ClientBase
//    {

//        #region [ Members ]

//        // Fields
//#if ThreadTracking
//        private ManagedThread m_connectionThread;
//#else
//        private Thread m_connectionThread;
//#endif
//        private Dictionary<string, string> m_connectionData;
//        private SerialPort m_serialClient;
//        private bool m_disposed;

//        #endregion

//        #region [ Constructors ]

//        public SerialClient()
//        {
//            m_serialClient = new SerialPort();
//            m_serialClient.DataReceived += m_serialClient_DataReceived;

//            base.ConnectionString = "Port=COM1; BaudRate=9600; Parity=None; StopBits=One; DataBits=8; DtrEnable=False; RtsEnable=False";
//            base.Protocol = TransportProtocol.Serial;

//        }

//        /// <summary>
//        /// Initializes a instance of PCS.Communication.SerialClient with the specified data.
//        /// </summary>
//        /// <param name="connectionString">The data that is required by the client to initialize.</param>
//        public SerialClient(string connectionString)
//            : this()
//        {
//            base.ConnectionString = connectionString;
//        }

//        #endregion

//        #region [ Methods ]

//        /// <summary>
//        /// Releases the unmanaged resources used by an instance of the <see cref="FileClient" /> class and optionally releases the managed resources.
//        /// </summary>
//        /// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (!m_disposed)
//            {
//                try
//                {
//                    if (disposing)
//                    {
//                        if (m_serialClient != null)
//                        {
//                            m_serialClient.DataReceived -= m_serialClient_DataReceived;
//                            m_serialClient.Close();
//                        }
//                        m_serialClient = null;
//                    }
//                }
//                finally
//                {
//                    base.Dispose(disposing);    // Call base class Dispose().
//                    m_disposed = true;          // Prevent duplicate dispose.
//                }
//            }
//        }

//        /// <summary>
//        /// Cancels any active attempts of connecting to the serial port.
//        /// </summary>
//        public override void CancelConnect()
//        {
//            if (base.Enabled && m_connectionThread.IsAlive)
//                m_connectionThread.Abort();
//        }

//        /// <summary>
//        /// Connects to the serial port asynchronously.
//        /// </summary>
//        public override void Connect()
//        {
//            if (base.Enabled && !base.IsConnected && ValidConnectionString(ConnectionString))
//            {
//                m_serialClient.PortName = m_connectionData["port"];
//                m_serialClient.BaudRate = int.Parse(m_connectionData["baudrate"]);
//                m_serialClient.DataBits = int.Parse(m_connectionData["databits"]);
//                m_serialClient.Parity = (Parity)(System.Enum.Parse(typeof(Parity), m_connectionData["parity"]));
//                m_serialClient.StopBits = (StopBits)(System.Enum.Parse(typeof(StopBits), m_connectionData["stopbits"]));

//                if (m_connectionData.ContainsKey("dtrenable")) m_serialClient.DtrEnable = m_connectionData["dtrenable"].ParseBoolean();
//                if (m_connectionData.ContainsKey("rtsenable")) m_serialClient.RtsEnable = m_connectionData["rtsenable"].ParseBoolean();

//#if ThreadTracking
//                m_connectionThread = new ManagedThread(ConnectToPort);
//                m_connectionThread.Name = "PCS.Communication.SerialClient.ConnectToPort()";
//#else
//                m_connectionThread = new Thread(ConnectToPort);
//#endif
//                m_connectionThread.Start();
//            }
//        }

//        /// <summary>
//        /// Disconnects from serial port.
//        /// </summary>
//        public override void Disconnect(int timeout)
//        {
//            CancelConnect();

//            if (base.Enabled && base.IsConnected)
//            {
//                m_serialClient.Close();
//                OnDisconnected(EventArgs.Empty);
//            }
//        }


//        /// <summary>
//        /// Sends prepared data to the server.
//        /// </summary>
//        /// <param name="data">The prepared data that is to be sent to the server.</param>
//        protected override void SendPreparedData(byte[] data)
//        {
//            if (base.Enabled && base.IsConnected)
//            {
//                OnSendDataBegin(new IdentifiableItem<Guid, byte[]>(ClientID, data));
//                m_serialClient.Write(data, 0, data.Length);
//                OnSendDataComplete(new IdentifiableItem<Guid, byte[]>(ClientID, data));
//            }
//        }

//        /// <summary>
//        /// Determines whether specified connection string required for connecting to the serial port is valid.
//        /// </summary>
//        /// <param name="connectionString">The connection string to be validated.</param>
//        /// <returns>True is the connection string is valid; otherwise False.</returns>
//        protected override bool ValidConnectionString(string connectionString)
//        {
//            if (!string.IsNullOrEmpty(connectionString))
//            {
//                m_connectionData = connectionString.ParseKeyValuePairs();

//                if (m_connectionData.ContainsKey("port") &&
//                    m_connectionData.ContainsKey("baudrate") &&
//                    m_connectionData.ContainsKey("parity") &&
//                    m_connectionData.ContainsKey("stopbits") &&
//                    m_connectionData.ContainsKey("databits"))
//                {
//                    return true;
//                }
//                else
//                {
//                    // Connection string is not in the expected format.
//                    StringBuilder exceptionMessage = new StringBuilder();

//                    exceptionMessage.Append("Connection string must be in the following format:");
//                    exceptionMessage.AppendLine();
//                    exceptionMessage.Append("   Port=[Name of the COM port]; BaudRate=[9600|4800|2400|1200]; Parity=[None|Odd|Even|Mark|Space]; StopBits=[None|One|Two|OnePointFive]; DataBits=[Number of data bits per byte]");

//                    throw new ArgumentException(exceptionMessage.ToString());
//                }
//            }
//            else
//            {
//                throw new ArgumentNullException("ConnectionString");
//            }
//        }

//        /// <summary>
//        /// Connects to the serial port.
//        /// </summary>
//        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
//        private void ConnectToPort()
//        {
//            int connectionAttempts = 0;

//            while (MaximumConnectionAttempts == -1 || connectionAttempts < MaximumConnectionAttempts)
//            {
//                try
//                {
//                    OnConnecting(EventArgs.Empty);
//                    m_serialClient.Open();
//                    OnConnected(EventArgs.Empty);

//                    break;
//                }
//                catch (ThreadAbortException)
//                {
//                    OnConnectingCancelled(EventArgs.Empty);
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    connectionAttempts++;
//                    OnConnectingException(ex);
//                }
//            }
//        }

//        /// <summary>
//        /// Receive data from the serial port (.NET serial port class raises this event when data is available)
//        /// </summary>
//        private void m_serialClient_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
//        {
//            int received;

//            // JRC: modified this code to make sure all available data on the serial port buffer is read, regardless of size of communication buffer
//            for (int x = 1; x <= (int)(System.Math.Ceiling((double)m_serialClient.BytesToRead / m_buffer.Length)); x++)
//            {
//                // Retrieve data from the serial port
//                received = m_serialClient.Read(m_buffer, 0, m_buffer.Length);

//                // Post raw data to real-time function delegate if defined - this bypasses all other activity
//                if (m_receiveRawDataFunction != null)
//                {
//                    m_receiveRawDataFunction(m_buffer, 0, received);
//                    m_totalBytesReceived += received;
//                }
//                else
//                {
//                    // Unpack data and make available via event
//                    OnReceivedData(new IdentifiableItem<Guid, byte[]>(ServerID, m_buffer.CopyBuffer(0, received)));
//                }
//            }
//        }

//        #endregion
//    }
//}