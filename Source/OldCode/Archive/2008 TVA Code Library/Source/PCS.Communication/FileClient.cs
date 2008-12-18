//*******************************************************************************************************
//  FileClient.cs
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
//       Added bypass optimizations for high-speed file data access
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using PCS.Threading;

namespace PCS.Communication
{
    /// <summary>
    /// Represents a communication client based on <see cref="FileStream"/>.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="FileClient"/> for writing data to a file:
    /// <code>
    /// using System;
    /// using PCS.Communication;
    /// using PCS.IO;
    /// 
    /// class Program
    /// {
    ///     static FileClient m_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         m_client = new FileClient(@"File=c:\File.txt");
    ///         m_client.StartingOffset = FilePath.GetFileLength(@"c:\File.txt");
    ///         m_client.ReceiveOnDemand = true;
    ///         m_client.MaxConnectionAttempts = 1;
    /// 
    ///         // Register event handlers.
    ///         m_client.ConnectionAttempt += m_client_ConnectionAttempt;
    ///         m_client.ConnectionEstablished += m_client_ConnectionEstablished;
    ///         m_client.ConnectionTerminated += m_client_ConnectionTerminated;
    ///         m_client.ReceiveDataComplete += m_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         m_client.Connect();
    /// 
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_client.Send(input);
    ///         }
    /// 
    ///         m_client.Disconnect();
    /// 
    ///         Console.ReadLine();
    ///     }
    /// 
    ///     static void m_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to server.");
    ///     }
    /// 
    ///     static void m_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to server.");
    ///     }
    /// 
    ///     static void m_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from server.");
    ///     }
    /// 
    ///     static void m_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}.", m_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This example shows how to use <see cref="FileClient"/> for reading data to a file:
    /// <code>
    /// using System;
    /// using PCS.Communication;
    /// 
    /// class Program
    /// {
    ///     static FileClient m_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         m_client = new FileClient(@"File=c:\File.txt");
    ///         m_client.ReceiveBufferSize = 1024;
    /// 
    ///         // Register event handlers.
    ///         m_client.ConnectionAttempt += m_client_ConnectionAttempt;
    ///         m_client.ConnectionEstablished += m_client_ConnectionEstablished;
    ///         m_client.ConnectionTerminated += m_client_ConnectionTerminated;
    ///         m_client.ReceiveDataComplete += m_client_ReceiveDataComplete;
    ///         // Connect to the server.
    ///         m_client.ConnectAsync();
    /// 
    ///         Console.ReadLine();
    /// 
    ///         m_client.Disconnect();
    ///     }
    /// 
    ///     static void m_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to server.");
    ///     }
    /// 
    ///     static void m_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to server.");
    ///     }
    /// 
    ///     static void m_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from server.");
    ///     }
    /// 
    ///     static void m_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}.", m_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class FileClient : ClientBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="AutoRepeat"/> property.
        /// </summary>
        public const bool DefaultAutoRepeat = false;

        /// <summary>
        /// Specifies the default value for the <see cref="StartingOffset"/> property.
        /// </summary>
        public const long DefaultStartingOffset = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveOnDemand"/> property.
        /// </summary>
        public const bool DefaultReceiveOnDemand = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveInterval"/> property.
        /// </summary>
        public const int DefaultReceiveInterval = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "File=DataFile.txt";

        // Fields
        private bool m_autoRepeat;
        private long m_startingOffset;
        private bool m_receiveOnDemand;
        private double m_receiveInterval;
        private TransportProvider<FileStream> m_fileClient;
        private Dictionary<string, string> m_connectData;
        private System.Timers.Timer m_receiveDataTimer;
#if ThreadTracking
        private ManagedThread m_connectionThread;
#else
        private Thread m_connectionThread;
#endif

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="FileClient"/> class.
        /// </summary>
        public FileClient()
            : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the client. See <see cref="DefaultConnectionString"/> for format.</param>
        public FileClient(string connectString)
            : base(TransportProtocol.File, connectString)
        {
            m_autoRepeat = DefaultAutoRepeat;
            m_startingOffset = DefaultStartingOffset;
            m_receiveOnDemand = DefaultReceiveOnDemand;
            m_receiveInterval = DefaultReceiveInterval;
            m_fileClient = new TransportProvider<FileStream>();
            m_receiveDataTimer = new System.Timers.Timer();
            m_receiveDataTimer.Elapsed += m_receiveDataTimer_Elapsed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether receiving (reading) of data is to be repeated endlessly.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultAutoRepeat),
        Description("Indicates whether receiving (reading) of data is to be repeated endlessly.")]
        public bool AutoRepeat
        {
            get
            {
                return m_autoRepeat;
            }
            set
            {
                m_autoRepeat = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting point relative to the beginning of the file from where the data is to be received (read).
        /// </summary>
        /// <exception cref="ArgumentException">The value specified is not a positive number.</exception>
        [Category("Data"),
        DefaultValue(DefaultStartingOffset),
        Description("The starting point relative to the beginning of the file from where the data is to be received (read).")]
        public long StartingOffset
        {
            get
            {
                return m_startingOffset;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive.");

                m_startingOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether receiving (reading) of data will be initiated manually by calling <see cref="ReceiveData()"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ReceiveInterval"/> will be set to -1 when <see cref="ReceiveOnDemand"/> is enabled.
        /// </remarks>
        [Category("Data"),
        DefaultValue(DefaultReceiveTimeout),
        Description("Indicates whether receiving (reading) of data will be initiated manually by calling ReceiveData().")]
        public bool ReceiveOnDemand
        {
            get
            {
                return m_receiveOnDemand;
            }
            set
            {
                m_receiveOnDemand = value;

                if (m_receiveOnDemand)
                    // We'll disable receiving data at a set interval if user wants to receive data on demand.
                    m_receiveInterval = -1;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds to pause before receiving (reading) the next available set of data.
        /// </summary>
        /// <remarks>Set <see cref="ReceiveInterval"/> = -1 to receive (read) data continuously without pausing.</remarks>
        [Category("Data"),
        DefaultValue(DefaultReceiveInterval),
        Description("The number of milliseconds to pause before receiving (reading) the next available set of data. Set ReceiveInterval = -1 to receive data continuously without pausing.")]
        public double ReceiveInterval
        {
            get
            {
                return m_receiveInterval;
            }
            set
            {
                if (value < 1)
                    m_receiveInterval = -1;
                else
                    m_receiveInterval = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{FileStream}"/> object for the <see cref="FileClient"/>.
        /// </summary>
        [Browsable(false)]
        public TransportProvider<FileStream> Client
        {
            get
            {
                return m_fileClient;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initiates receiving to data from the file.
        /// </summary>
        /// <remarks>This method is functional only when ReceiveOnDemand is enabled.</remarks>
        public void ReceiveData()
        {
            if (CurrentState == ClientState.Connected)
            {
                ReadData();
            }
            else
            {
                throw new InvalidOperationException("Client is currently not connected.");
            }
        }

        /// <summary>
        /// Disconnects from the file (i.e., closes the file stream).
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
                m_fileClient.Reset();
                m_receiveDataTimer.Stop();

                if (m_connectionThread != null)
                    m_connectionThread.Abort();

                OnConnectionTerminated();
            }
        }

        /// <summary>
        /// Connects to the file asynchronously.
        /// </summary>
        public override void ConnectAsync()
        {
            if (CurrentState == ClientState.Disconnected)
            {
                m_fileClient.Passphrase = HandshakePassphrase;
                m_fileClient.ReceiveBuffer = new byte[ReceiveBufferSize];
#if ThreadTracking
                m_connectionThread = new ManagedThread(OpenFile);
                m_connectionThread.Name = "PCS.Communication.FileClient.ConnectToFile()";
#else
                    m_connectionThread = new Thread(ConnectToFile);
#endif
                m_connectionThread.Start();
            }
            else
            {
                throw new InvalidOperationException("Client is currently not disconnected.");
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            WaitHandle handle;

            // Send payload to the file asynchronously.
            lock (m_fileClient.Provider)
            {
                handle = m_fileClient.Provider.BeginWrite(data, offset, length, SendDataAsyncCallback, null).AsyncWaitHandle;
            }

            // Notify that the send operation has started.
            m_fileClient.SendBuffer = data;
            m_fileClient.SendBufferOffset = offset;
            m_fileClient.SendBufferLength = length;
            m_fileClient.Statistics.UpdateBytesSent(m_fileClient.SendBufferLength);
            OnSendDataStart();

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle;
        }

        /// <summary>
        /// Gets the passphrase to be used for ciphering client data.
        /// </summary>
        /// <returns>Cipher passphrase.</returns>
        protected override string GetSessionPassphrase()
        {
            return m_fileClient.Passphrase;
        }

        /// <summary>
        /// Determines whether specified connection string required for connecting to the file is valid.
        /// </summary>
        /// <param name="connectionString">The connection string to be validated.</param>
        /// <returns>True is the connection string is valid; otherwise False.</returns>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            if (!m_connectData.ContainsKey("file"))
                throw new ArgumentException(string.Format("File property is missing. Example: {0}.", DefaultConnectionString));
        }

        /// <summary>
        /// Connects to the file.
        /// </summary>
        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
        private void OpenFile()
        {
            int connectionAttempts = 0;
            while (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts)
            {
                try
                {
                    OnConnectionAttempt(); ;

                    m_fileClient.Provider = new FileStream(m_connectData["file"], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    m_fileClient.Provider.Seek(m_startingOffset, SeekOrigin.Begin); // Move to the specified offset.

                    OnConnectionEstablished();

                    if (!m_receiveOnDemand)
                    {
                        if (m_receiveInterval > 0)
                        {
                            // We need to start receivng data at the specified interval.
                            m_receiveDataTimer.Interval = m_receiveInterval;
                            m_receiveDataTimer.Start();
                        }
                        else
                        {
                            while (true)
                            {
                                // We need to start receiving data continuously.
                                ReadData();
                                Thread.Sleep(1000);
                            }
                        }
                    }

                    break; // We've successfully connected to the file.
                }
                catch (ThreadAbortException)
                {
                    // We must abort connecting to the file.
                    break;
                }
                catch (Exception ex)
                {
                    // We must keep retrying connecting to the file.
                    connectionAttempts++;
                    OnConnectionException(ex);
                }
            }
        }

        /// <summary>
        /// Receive data from the file.
        /// </summary>
        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
        private void ReadData()
        {
            try
            {
                // Process the entire file content
                while (m_fileClient.Provider.Position < m_fileClient.Provider.Length)
                {
                    // Retrieve data from the file stream.
                    lock (m_fileClient.Provider)
                    {
                        m_fileClient.ReceiveBufferLength = m_fileClient.Provider.Read(m_fileClient.ReceiveBuffer, 0, m_fileClient.ReceiveBuffer.Length);
                    }
                    m_fileClient.Statistics.UpdateBytesReceived(m_fileClient.ReceiveBufferLength);

                    OnReceiveDataComplete(m_fileClient.ReceiveBuffer, m_fileClient.ReceiveBufferLength);

                    // We'll re-read the file if the user wants to repeat when we're done reading the file.
                    if (m_autoRepeat && m_fileClient.Provider.Position == m_fileClient.Provider.Length)
                    {
                        lock (m_fileClient.Provider)
                        {
                            m_fileClient.Provider.Seek(0, SeekOrigin.Begin);
                        }
                    }

                    // We must stop processing the file if user has either opted to receive data on
                    // demand or receive data at a predefined interval.
                    if (m_receiveOnDemand || m_receiveInterval > 0)
                    {
                        break;
                    }
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                OnReceiveDataException(ex);
            }
        }

        private void SendDataAsyncCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Send operation is complete.
                lock (m_fileClient)
                {
                    m_fileClient.Provider.EndWrite(asyncResult);
                    m_fileClient.Provider.Flush();                    
                }
                OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
        }

        private void m_receiveDataTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_receiveInterval > 0)
            {
                ReadData();
            }
        }

        #endregion
    }
}






















////*******************************************************************************************************
////  FileClient.cs
////  Copyright © 2008 - TVA, all rights reserved - Gbtc
////
////  Build Environment: C#, Visual Studio 2008
////  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
////      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
////       Phone: 423/751-3024
////       Email: pcpatel@tva.gov
////
////  Code Modification History:
////  -----------------------------------------------------------------------------------------------------
////  07/24/2006 - Pinal C. Patel
////       Original version of source code generated
////  09/06/2006 - J. Ritchie Carroll
////       Added bypass optimizations for high-speed file data access
////  09/29/2008 - James R Carroll
////       Converted to C#.
////
////*******************************************************************************************************

//using System;
//using System.IO;
//using System.Text;
//using System.Threading;
//using System.ComponentModel;
//using System.Collections.Generic;
//using PCS.Configuration;
//using PCS.Threading;

//namespace PCS.Communication
//{
//    /// <summary>
//    /// Represents a communication client based on <see cref="FileStream"/>.
//    /// </summary>
//    public class FileClient : ClientBase
//    {
//        #region [ Members ]

//        // Fields
//        private bool m_autoRepeat;
//        private bool m_receiveOnDemand;
//        private double m_receiveInterval;
//        private long m_startingOffset;
//        private StateInfo<FileStream> m_fileClient;
//#if ThreadTracking
//        private ManagedThread m_receivingThread;
//        private ManagedThread m_connectionThread;
//#else
//        private Thread m_receivingThread;
//        private Thread m_connectionThread;
//#endif
//        private Dictionary<string, string> m_connectionData;
//        private System.Timers.Timer m_receiveDataTimer;
//        private bool m_disposed;

//        #endregion

//        #region [ Constructors ]

//        /// <summary>
//        /// Initializes a instance of PCS.Communication.FileClient with the specified data.
//        /// </summary>
//        /// <param name="connectionString">The data that is required by the client to initialize.</param>
//        public FileClient(string connectionString)
//            : this()
//        {
//            base.ConnectionString = connectionString;
//        }

//        public FileClient()
//        {
//            m_receiveOnDemand = false;
//            m_receiveInterval = -1;
//            m_startingOffset = 0;
//            m_fileClient = new StateInfo<System.IO.FileStream>();
//            m_receiveDataTimer = new System.Timers.Timer();
//            m_receiveDataTimer.Elapsed += m_receiveDataTimer_Elapsed;

//            base.ConnectionString = "File=DataFile.txt";
//            base.Protocol = TransportProtocol.File;
//        }

//        #endregion

//        #region [ Properties ]

//        /// <summary>
//        /// Gets or sets a boolean value indicating whether receiving (reading) of data is to be repeated endlessly.
//        /// </summary>
//        /// <value></value>
//        /// <returns>True if receiving (reading) of data is to be repeated endlessly; otherwise False.</returns>
//        [Description("Indicates whether receiving (reading) of data is to be repeated endlessly."), Category("Data"), DefaultValue(typeof(bool), "False")]
//        public bool AutoRepeat
//        {
//            get
//            {
//                return m_autoRepeat;
//            }
//            set
//            {
//                m_autoRepeat = value;
//            }
//        }

//        /// <summary>
//        /// Gets or sets a boolean value indicating whether receiving (reading) of data will be initiated manually
//        /// by calling ReceiveData().
//        /// </summary>
//        /// <value></value>
//        /// <returns>True if receiving (reading) of data will be initiated manually; otherwise False.</returns>
//        [Description("Indicates whether receiving (reading) of data will be initiated manually by calling ReceiveData()."), Category("Data"), DefaultValue(typeof(bool), "False")]
//        public bool ReceiveOnDemand
//        {
//            get
//            {
//                return m_receiveOnDemand;
//            }
//            set
//            {
//                m_receiveOnDemand = value;

//                if (m_receiveOnDemand)
//                {
//                    // We'll disable receiving data at a set interval if user wants to receive data on demand.
//                    m_receiveInterval = -1;
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the time in milliseconds to pause before receiving (reading) the next available set of data.
//        /// </summary>
//        /// <value></value>
//        /// <returns>Time in milliseconds to pause before receiving (reading) the next available set of data.</returns>
//        /// <remarks>Set ReceiveInterval = -1 to receive (read) data continuously without pausing.</remarks>
//        [Description("Time in milliseconds to pause before receiving (reading) the next available set of data. Set ReceiveInterval = -1 to receive data continuously without pausing."), Category("Data"), DefaultValue(typeof(double), "-1")]
//        public double ReceiveInterval
//        {
//            get
//            {
//                return m_receiveInterval;
//            }
//            set
//            {
//                if (value == -1 || value > 0)
//                {
//                    m_receiveInterval = value;

//                    if (m_receiveInterval > 0)
//                    {
//                        // We'll disable the ReceiveOnDemand feature if the user specifies an interval for
//                        // automatically receiving data.
//                        m_receiveOnDemand = false;
//                    }
//                }
//                else
//                {
//                    throw new ArgumentOutOfRangeException("value");
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the starting point relative to the beginning of the file from where the data is to be received (read).
//        /// </summary>
//        /// <value></value>
//        /// <returns>The starting point relative to the beginning of the file from where the data is to be received (read).</returns>
//        [Description("The starting point relative to the beginning of the file from where the data is to be received (read)."), Category("Data"), DefaultValue(typeof(long), "0")]
//        public long StartingOffset
//        {
//            get
//            {
//                return m_startingOffset;
//            }
//            set
//            {
//                if (value >= 0)
//                {
//                    m_startingOffset = value;
//                }
//                else
//                {
//                    throw new ArgumentOutOfRangeException("value");
//                }
//            }
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
//                        if (m_receivingThread != null)
//                        {
//                            if (m_receivingThread.IsAlive)
//                                m_receivingThread.Abort();
//                        }
//                        m_receivingThread = null;

//                        if (m_connectionThread != null)
//                        {
//                            if (m_connectionThread.IsAlive)
//                                m_connectionThread.Abort();
//                        }
//                        m_connectionThread = null;

//                        if (m_receiveDataTimer != null)
//                        {
//                            m_receiveDataTimer.Elapsed -= m_receiveDataTimer_Elapsed;
//                            m_receiveDataTimer.Dispose();
//                        }
//                        m_receiveDataTimer = null;
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
//        /// Initiates receiving to data from the file.
//        /// </summary>
//        /// <remarks>This method is functional only when ReceiveOnDemand is enabled.</remarks>
//        public void ReceiveData()
//        {
//            if (base.Enabled && base.IsConnected && m_receiveOnDemand && !m_receivingThread.IsAlive)
//            {
//#if ThreadTracking
//                m_receivingThread = new ManagedThread(ReceiveFileData);
//                m_receivingThread.Name = "PCS.Communication.FileClient.ReceiveFileData()";
//#else
//                m_receivingThread = new Thread(ReceiveFileData);
//#endif
//                m_receivingThread.Start();
//            }
//        }

//        /// <summary>
//        /// Cancels any active attempts of connecting to the file.
//        /// </summary>
//        public override void CancelConnect()
//        {
//            if (base.Enabled && m_connectionThread.IsAlive)
//                m_connectionThread.Abort();
//        }

//        /// <summary>
//        /// Connects to the file asynchronously.
//        /// </summary>
//        public override void Connect()
//        {
//            if (base.Enabled && !base.IsConnected && ValidConnectionString(ConnectionString))
//            {
//                if (File.Exists(m_connectionData["file"]))
//                {
//#if ThreadTracking
//                    m_connectionThread = new ManagedThread(ConnectToFile);
//                    m_connectionThread.Name = "PCS.Communication.FileClient.ConnectToFile()";
//#else
//                    m_connectionThread = new Thread(ConnectToFile);
//#endif
//                    m_connectionThread.Start();
//                }
//                else
//                {
//                    throw new FileNotFoundException(m_connectionData["file"] + " does not exist.");
//                }
//            }
//        }

//        /// <summary>
//        /// Disconnects from the file (i.e., closes the file stream).
//        /// </summary>
//        public override void Disconnect(int timeout)
//        {
//            CancelConnect();

//            if (base.Enabled && base.IsConnected)
//            {
//                m_receiveDataTimer.Stop();
//                m_fileClient.Client.Close();
//                OnDisconnected(EventArgs.Empty);
//            }
//        }

//        public override void LoadSettings()
//        {
//            base.LoadSettings();

//            if (PersistSettings)
//            {
//                try
//                {
//                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategory];
//                    ReceiveOnDemand = settings["ReceiveOnDemand"].ValueAs(m_receiveOnDemand);
//                    ReceiveInterval = settings["ReceiveInterval"].ValueAs(m_receiveInterval);
//                    StartingOffset = settings["StartingOffset"].ValueAs(m_startingOffset);
//                }
//                catch
//                {
//                    // We'll encounter exceptions if the settings are not present in the config file.
//                }
//            }
//        }

//        public override void SaveSettings()
//        {
//            base.SaveSettings();

//            if (PersistSettings)
//            {
//                try
//                {
//                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategory];
//                    CategorizedSettingsElement setting;

//                    setting = settings["ReceiveOnDemand", true];
//                    setting.Value = m_receiveOnDemand.ToString();
//                    setting.Description = "True if receiving (reading) of data will be initiated manually; otherwise False.";

//                    setting = settings["ReceiveInterval", true];
//                    setting.Value = m_receiveInterval.ToString();
//                    setting.Description = "Time in milliseconds to pause before receiving (reading) the next available set of data.";

//                    setting = settings["StartingOffset", true];
//                    setting.Value = m_startingOffset.ToString();
//                    setting.Description = "The starting point relative to the beginning of the file from where the data is to be received (read).";

//                    ConfigurationFile.Current.Save();
//                }
//                catch
//                {
//                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
//                }
//            }
//        }

//        [EditorBrowsable(EditorBrowsableState.Never)]
//        protected override void SendPreparedData(byte[] data)
//        {
//            throw new NotSupportedException();
//        }

//        /// <summary>
//        /// Determines whether specified connection string required for connecting to the file is valid.
//        /// </summary>
//        /// <param name="connectionString">The connection string to be validated.</param>
//        /// <returns>True is the connection string is valid; otherwise False.</returns>
//        protected override bool ValidConnectionString(string connectionString)
//        {
//            if (!string.IsNullOrEmpty(connectionString))
//            {
//                m_connectionData = connectionString.ParseKeyValuePairs();

//                if (m_connectionData.ContainsKey("file"))
//                {
//                    return true;
//                }
//                else
//                {
//                    // Connection string is not in the expected format.
//                    StringBuilder exceptionMessage = new StringBuilder();

//                    exceptionMessage.Append("Connection string must be in the following format:");
//                    exceptionMessage.AppendLine();
//                    exceptionMessage.Append("   File=[Name of the file]");

//                    throw new ArgumentException(exceptionMessage.ToString());
//                }
//            }
//            else
//            {
//                throw new ArgumentNullException("ConnectionString");
//            }
//        }

//        /// <summary>
//        /// Connects to the file.
//        /// </summary>
//        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
//        private void ConnectToFile()
//        {
//            int connectionAttempts = 0;

//            while (MaximumConnectionAttempts == -1 || connectionAttempts < MaximumConnectionAttempts)
//            {
//                try
//                {
//                    OnConnecting(EventArgs.Empty);

//                    m_fileClient.Client = new FileStream(m_connectionData["file"], FileMode.Open);
//                    m_fileClient.Client.Seek(m_startingOffset, SeekOrigin.Begin); // Move to the specified offset.

//                    OnConnected(EventArgs.Empty);

//                    if (!m_receiveOnDemand)
//                    {
//                        if (m_receiveInterval > 0)
//                        {
//                            // We need to start receivng data at the specified interval.
//                            m_receiveDataTimer.Interval = m_receiveInterval;
//                            m_receiveDataTimer.Start();
//                        }
//                        else
//                        {
//                            // We need to start receiving data continuously.
//#if ThreadTracking
//                            m_receivingThread = new ManagedThread(ReceiveFileData);
//                            m_receivingThread.Name = "PCS.Communication.FileClient.ReceiveFileData()";
//#else
//                            m_receivingThread = new Thread(ReceiveFileData);
//#endif
//                            m_receivingThread.Start();
//                        }
//                    }

//                    break; // We've successfully connected to the file.
//                }
//                catch (ThreadAbortException)
//                {
//                    OnConnectingCancelled(EventArgs.Empty);
//                    break; // We must abort connecting to the file.
//                }
//                catch (Exception ex)
//                {
//                    connectionAttempts++;
//                    OnConnectingException(ex);
//                }
//            }
//        }

//        /// <summary>
//        /// Receive data from the file.
//        /// </summary>
//        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
//        private void ReceiveFileData()
//        {
//            try
//            {
//                int received;

//                // Process the entire file content
//                while (m_fileClient.Client.Position < m_fileClient.Client.Length)
//                {
//                    // Retrieve data from the file stream
//                    received = m_fileClient.Client.Read(m_buffer, 0, m_buffer.Length);

//                    // Post raw data to real-time function delegate if defined - this bypasses all other activity
//                    if (m_receiveRawDataFunction != null)
//                    {
//                        m_receiveRawDataFunction(m_buffer, 0, received);
//                        m_totalBytesReceived += received;
//                    }
//                    else
//                    {
//                        // Unpack data and make available via event
//                        OnReceivedData(new IdentifiableItem<Guid, byte[]>(ServerID, m_buffer.CopyBuffer(0, received)));
//                    }

//                    // We'll re-read the file if the user wants to repeat when we're done reading the file.
//                    if (m_autoRepeat && m_fileClient.Client.Position == m_fileClient.Client.Length)
//                    {
//                        m_fileClient.Client.Seek(0, SeekOrigin.Begin);
//                    }

//                    // We must stop processing the file if user has either opted to receive data on
//                    // demand or receive data at a predefined interval.
//                    if (m_receiveOnDemand || m_receiveInterval > 0)
//                    {
//                        break;
//                    }
//                }
//            }
//            catch
//            {
//                // Exit gracefully when an exception is encountered while receiving data.
//            }
//        }

//        private void m_receiveDataTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//        {
//            if (base.Enabled && base.IsConnected && m_receiveInterval > 0 && !m_receivingThread.IsAlive)
//            {
//#if ThreadTracking
//                m_receivingThread = new ManagedThread(ReceiveFileData);
//                m_receivingThread.Name = "PCS.Communication.FileClient.ReceiveFileData()";
//#else
//                m_receivingThread = new Thread(ReceiveFileData);
//#endif
//                m_receivingThread.Start();
//            }
//        }

//        #endregion
//    }
//}
