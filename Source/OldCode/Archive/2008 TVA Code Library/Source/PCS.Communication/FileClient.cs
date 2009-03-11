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
using PCS.Configuration;

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
    /// 
    /// class Program
    /// {
    ///     static FileClient m_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         m_client = new FileClient(@"File=c:\File.txt");
    ///         m_client.Initialize();
    ///         // Register event handlers.
    ///         m_client.ConnectionAttempt += m_client_ConnectionAttempt;
    ///         m_client.ConnectionEstablished += m_client_ConnectionEstablished;
    ///         m_client.ConnectionTerminated += m_client_ConnectionTerminated;
    ///         m_client.SendDataComplete += m_client_SendDataComplete;
    ///         // Connect the client.
    ///         m_client.ConnectAsync();
    /// 
    ///         // Write user input to the file.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_client.Send(input + "\r\n");
    ///         }
    /// 
    ///         // Disconnect the client on shutdown.
    ///         m_client.Disconnect();
    ///     }
    /// 
    ///     static void m_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to file.");
    ///     }
    /// 
    ///     static void m_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to file.");
    ///     }
    /// 
    ///     static void m_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from file.");
    ///     }
    /// 
    ///     static void m_client_SendDataComplete(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine(string.Format("Sent data - {0}", m_client.TextEncoding.GetString(m_client.Client.SendBuffer)));
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
    ///         m_client.Initialize();
    ///         // Register event handlers.
    ///         m_client.ConnectionAttempt += m_client_ConnectionAttempt;
    ///         m_client.ConnectionEstablished += m_client_ConnectionEstablished;
    ///         m_client.ConnectionTerminated += m_client_ConnectionTerminated;
    ///         m_client.ReceiveDataComplete += m_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         m_client.ConnectAsync();
    /// 
    ///         // Wait for client to read data.
    ///         Console.ReadLine();
    /// 
    ///         // Disconnect the client on shutdown.
    ///         m_client.Disconnect();
    ///     }
    /// 
    ///     static void m_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to file.");
    ///     }
    /// 
    ///     static void m_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to file.");
    ///     }
    /// 
    ///     static void m_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from file.");
    ///     }
    /// 
    ///     static void m_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}", m_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
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
        /// Specifies the default value for the <see cref="FileShareMode"/> property.
        /// </summary>
        public const FileShare DefaultFileShareMode = FileShare.ReadWrite;

        /// <summary>
        /// Specifies the default value for the <see cref="FileAccessMode"/> property.
        /// </summary>
        public const FileAccess DefaultFileAccessMode = FileAccess.ReadWrite;
        
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
        /// <param name="connectString">Connect string of the <see cref="FileClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public FileClient(string connectString)
            : base(TransportProtocol.File, connectString)
        {
            m_autoRepeat = DefaultAutoRepeat;
            m_receiveOnDemand = DefaultReceiveOnDemand;
            m_receiveInterval = DefaultReceiveInterval;
            m_startingOffset = DefaultStartingOffset;
            m_fileOpenMode = DefaultFileOpenMode;
            m_fileShareMode = DefaultFileShareMode;
            m_fileAccessMode = DefaultFileAccessMode;
            m_fileClient = new TransportProvider<FileStream>();
            m_receiveDataTimer = new System.Timers.Timer();
            m_receiveDataTimer.Elapsed += m_receiveDataTimer_Elapsed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether receiving (reading) of data is to be repeated endlessly.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="AutoRepeat"/> is enabled when <see cref="FileAccessMode"/> is <see cref="FileAccess.ReadWrite"/></exception>
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
                if (value && m_fileAccessMode == FileAccess.ReadWrite)
                    throw new InvalidOperationException("AutoRepeat cannot be enabled when FileAccessMode is FileAccess.ReadWrite.");

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
        /// <remarks>
        /// Set <see cref="ReceiveInterval"/> = -1 to receive (read) data continuously without pausing.
        /// </remarks>
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
        [Category("File"),
        DefaultValue(DefaultFileOpenMode),
        Description("Gets or sets the System.IO.FileMode value to be used when opening the file.")]
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
        [Category("File"),
        DefaultValue(DefaultFileShareMode),
        Description("Gets or sets the System.IO.FileShare value to be used when opening the file.")]
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
        /// <exception cref="InvalidOperationException"><see cref="FileAccessMode"/> is set to <see cref="FileAccess.ReadWrite"/> when <see cref="AutoRepeat"/> is enabled.</exception>
        [Category("File"),
        DefaultValue(DefaultFileAccessMode),
        Description("Gets or sets the System.IO.FileAccess value to be used when opening the file.")]
        public FileAccess FileAccessMode
        {
            get
            {
                return m_fileAccessMode;
            }
            set
            {
                if (value == FileAccess.ReadWrite && m_autoRepeat)
                    throw new InvalidOperationException("FileAccessMode cannot be set to FileAccess.ReadWrite when AutoRepeat is enabled.");

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

        /// <summary>
        /// Gets the server URI of the <see cref="FileClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri
        {
            get
            {
                return string.Format("{0}://{1}", TransportProtocol, m_connectData["file"]).ToLower();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Receives (reads) data from the <see cref="FileStream"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="ReceiveData()"/> is called when <see cref="FileClient"/> is not connected.</exception>
        /// <exception cref="InvalidOperationException"><see cref="ReceiveData()"/> is called when <see cref="ReceiveOnDemand"/> is disabled.</exception>
        public void ReceiveData()
        {
            if (m_receiveOnDemand)
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
            else
            {
                throw new InvalidOperationException("ReceiveData() cannot be used when ReceiveOnDemand is disabled.");
            }
        }

        /// <summary>
        /// Disconnects the <see cref="FileClient"/> from the <see cref="FileStream"/>.
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
        /// Connects the <see cref="FileClient"/> to the <see cref="FileStream"/> asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="FileClient"/> when it is not disconnected.</exception>
        public override void ConnectAsync()
        {
            if (CurrentState == ClientState.Disconnected)
            {
                // Initialize if unitialized.
                Initialize();

                m_fileClient.ID = this.ClientID;
                m_fileClient.Passphrase = this.HandshakePassphrase;
                m_fileClient.ReceiveBuffer = new byte[ReceiveBufferSize];
#if ThreadTracking
                m_connectionThread = new ManagedThread(OpenFile);
                m_connectionThread.Name = "PCS.Communication.FileClient.OpenFile()";
#else
                m_connectionThread = new Thread(OpenFile);
#endif
                m_connectionThread.Start();
            }
            else
            {
                throw new InvalidOperationException("Client is currently not disconnected.");
            }
        }

        /// <summary>
        /// Saves <see cref="FileClient"/> settings to the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["AutoRepeat", true].Update(m_autoRepeat, "True if receiving (reading) of data is to be repeated endlessly, otherwise False.");
                settings["ReceiveOnDemand", true].Update(m_receiveOnDemand, "True if receiving (reading) of data will be initiated manually, otherwise False.");
                settings["ReceiveInterval", true].Update(m_receiveInterval, "Number of milliseconds to pause before receiving (reading) the next available set of data.");
                settings["StartingOffset", true].Update(m_startingOffset, "Starting point relative to the beginning of the file from where the data is to be received (read).");
                settings["FileOpenMode", true].Update(m_fileOpenMode, "Mode (CreateNew; Create; Open; OpenOrCreate; Truncate; Append) to be used when opening the file.");
                settings["FileShareMode", true].Update(m_fileShareMode, "Mode (None; Read; Write; ReadWrite; Delete; Inheritable) to be used for sharing the file.");
                settings["FileAccessMode", true].Update(m_fileAccessMode, "Mode (Read; Write; ReadWrite) to be used for accessing the file.");
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="FileClient"/> settings from the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();
            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                AutoRepeat = settings["AutoRepeat", true].ValueAs(m_autoRepeat);
                ReceiveOnDemand = settings["ReceiveOnDemand", true].ValueAs(m_receiveOnDemand);
                ReceiveInterval = settings["ReceiveInterval", true].ValueAs(m_receiveInterval);
                StartingOffset = settings["StartingOffset", true].ValueAs(m_startingOffset);
                FileOpenMode = settings["FileOpenMode", true].ValueAs(m_fileOpenMode);
                FileShareMode = settings["FileShareMode", true].ValueAs(m_fileShareMode);
                FileAccessMode = settings["FileAccessMode", true].ValueAs(m_fileAccessMode);
            }
        }

        /// <summary>
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">File property is missing.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            if (!m_connectData.ContainsKey("file"))
                throw new ArgumentException(string.Format("File property is missing. Example: {0}.", DefaultConnectionString));
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
        /// Callback method for asynchronous send operation.
        /// </summary>
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

        /// <summary>
        /// Connects to the <see cref="FileStream"/>.
        /// </summary>
        private void OpenFile()
        {
            int connectionAttempts = 0;
            while (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts)
            {
                try
                {
                    OnConnectionAttempt(); ;

                    // Open the file.
                    m_fileClient.Provider = new FileStream(m_connectData["file"], m_fileOpenMode, m_fileAccessMode, m_fileShareMode);
                    // Move to the specified offset.
                    m_fileClient.Provider.Seek(m_startingOffset, SeekOrigin.Begin); 

                    OnConnectionEstablished();

                    if (!m_receiveOnDemand)
                    {
                        if (m_receiveInterval > 0)
                        {
                            // Start receiving data at interval.
                            m_receiveDataTimer.Interval = m_receiveInterval;
                            m_receiveDataTimer.Start();
                        }
                        else
                        {
                            // Start receiving data continuously.
                            while (true)
                            {
                                ReadData();         // Read all available data.
                                Thread.Sleep(1000); // Wait for more data to be available.
                            }
                        }
                    }

                    break;  // We're done here.
                }
                catch (ThreadAbortException)
                {
                    // Exit gracefully.
                    break;
                }
                catch (Exception ex)
                {
                    // Keep retrying connecting to the file.
                    connectionAttempts++;
                    OnConnectionException(ex);
                }
            }
        }

        /// <summary>
        /// Receive (reads) data from the <see cref="FileStream"/>.
        /// </summary>
        private void ReadData()
        {
            try
            {
                // Process the entire file content
                while (m_fileClient.Provider.Position < m_fileClient.Provider.Length)
                {
                    // Retrieve data from the file.
                    lock (m_fileClient.Provider)
                    {
                        m_fileClient.ReceiveBufferLength = m_fileClient.Provider.Read(m_fileClient.ReceiveBuffer, m_fileClient.ReceiveBufferOffset, m_fileClient.ReceiveBuffer.Length);
                    }
                    m_fileClient.Statistics.UpdateBytesReceived(m_fileClient.ReceiveBufferLength);

                    // Notify of the retrieved data.
                    OnReceiveDataComplete(m_fileClient.ReceiveBuffer, m_fileClient.ReceiveBufferLength);

                    // Re-read the file if the user wants to repeat when done reading the file.
                    if (m_autoRepeat && m_fileClient.Provider.Position == m_fileClient.Provider.Length)
                    {
                        lock (m_fileClient.Provider)
                        {
                            m_fileClient.Provider.Seek(m_startingOffset, SeekOrigin.Begin);
                        }
                    }

                    // Stop processing the file if user has either opted to receive data on demand or receive data at a predefined interval.
                    if (m_receiveOnDemand || m_receiveInterval > 0)
                    {
                        break;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Exit gracefully.
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                OnReceiveDataException(ex);
            }
        }

        private void m_receiveDataTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ReadData();
        }

        #endregion
    }
}