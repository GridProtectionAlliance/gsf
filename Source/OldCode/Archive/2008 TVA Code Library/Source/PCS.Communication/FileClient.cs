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
        /// Specifies the default value for the <see cref="ReceiveOnDemand"/> property.
        /// </summary>
        public const bool DefaultReceiveOnDemand = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveInterval"/> property.
        /// </summary>
        public const int DefaultReceiveInterval = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="StartingOffset"/> property.
        /// </summary>
        public const long DefaultStartingOffset = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="FileOpenMode"/> property.
        /// </summary>
        public const FileMode DefaultFileOpenMode = FileMode.OpenOrCreate;

        /// <summary>
        /// Specifies the default value for the <see cref="FileAccessMode"/> property.
        /// </summary>
        public const FileAccess DefaultFileAccessMode = FileAccess.ReadWrite;

        /// <summary>
        /// Specifies the default value for the <see cref="FileShareMode"/> property.
        /// </summary>
        public const FileShare DefaultFileShareMode = FileShare.ReadWrite;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "File=DataFile.txt";

        // Fields
        private bool m_autoRepeat;
        private bool m_receiveOnDemand;
        private double m_receiveInterval;
        private long m_startingOffset;
        private FileMode m_fileOpenMode;
        private FileShare m_fileShareMode;
        private FileAccess m_fileAccessMode;
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
            m_receiveOnDemand = DefaultReceiveOnDemand;
            m_receiveInterval = DefaultReceiveInterval;
            m_startingOffset = DefaultStartingOffset;
            m_fileOpenMode = DefaultFileOpenMode;
            m_fileAccessMode = DefaultFileAccessMode;
            m_fileShareMode = DefaultFileShareMode;
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
        /// Gets or sets the starting point relative to the beginning of the file from where the data is to be received (read).
        /// </summary>
        /// <exception cref="ArgumentException">The value specified is not a positive number.</exception>
        [Category("File"),
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
        /// Gets or sets the <see cref="FileMode"/> value to be used when opening the file.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FileMode FileOpenMode
        {
            get
            {
                return m_fileOpenMode;
            }
            set
            {
                m_fileOpenMode = value;
            }
        }

        /// <summary>
        /// Gets or set the <see cref="FileShare"/> value to be used when opening the file.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FileShare FileShareMode
        {
            get
            {
                return m_fileShareMode;
            }
            set
            {
                m_fileShareMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FileAccess"/> value to be used when opening the file.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FileAccess FileAccessMode
        {
            get
            {
                return m_fileAccessMode;
            }
            set
            {
                m_fileAccessMode = value;
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
                m_fileClient.ID = this.ClientID;
                m_fileClient.Passphrase = this.HandshakePassphrase;
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

                    m_fileClient.Provider = new FileStream(m_connectData["file"], m_fileOpenMode, m_fileAccessMode, m_fileShareMode);
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