//******************************************************************************************************
//  SerialClient.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/24/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed serial port data access.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  07/08/2009 - J. Ritchie Carroll
//       Added WaitHandle return value from asynchronous connection.
//  07/15/2009 - Pinal C. Patel
//       Modified Disconnect() to add error checking.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/29/2010 - Pinal C. Patel
//       Corrected the implementation of ConnectAsync() method.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a communication client based on <see cref="SerialPort"/>.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="SerialClient"/> for communicating with <see cref="SerialPort"/>:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Communication;
    /// 
    /// class Program
    /// {
    ///     static SerialClient s_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         s_client = new SerialClient("Port=COM1; BaudRate=9600; Parity=None; StopBits=One; DataBits=8; DtrEnable=False; RtsEnable=False");
    ///         s_client.Initialize();
    ///         // Register event handlers.
    ///         s_client.ConnectionAttempt += s_client_ConnectionAttempt;
    ///         s_client.ConnectionEstablished += s_client_ConnectionEstablished;
    ///         s_client.ConnectionTerminated += s_client_ConnectionTerminated;
    ///         s_client.SendDataComplete += s_client_SendDataComplete;
    ///         s_client.ReceiveDataComplete += s_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         s_client.Connect();
    /// 
    ///         // Write user input to the serial port.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             s_client.Send(input);
    ///         }
    /// 
    ///         // Disconnect the client on shutdown.
    ///         s_client.Dispose();
    ///     }
    /// 
    ///     static void s_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to serial port.");
    ///     }
    /// 
    ///     static void s_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to serial port.");
    ///     }
    /// 
    ///     static void s_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from serial port.");
    ///     }
    /// 
    ///     static void s_client_SendDataComplete(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine(string.Format("Sent data - {0}", s_client.TextEncoding.GetString(s_client.Client.SendBuffer)));
    ///     }
    /// 
    ///     static void s_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}", s_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
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
        private readonly TransportProvider<SerialPort> m_serialClient;
        private Dictionary<string, string> m_connectData;
        private ManualResetEvent m_connectionHandle;
        private Thread m_connectionThread;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialClient"/> class.
        /// </summary>
        public SerialClient() : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="SerialClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public SerialClient(string connectString) : base(TransportProtocol.Serial, connectString)
        {
            m_serialClient = new TransportProvider<SerialPort>();
            ReceivedBytesThreshold = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="SerialClient"/>.</param>
        public SerialClient(IContainer container) : this()
        {
            container?.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="SerialPort"/> object for the <see cref="SerialClient"/>.
        /// </summary>
        [Browsable(false)]
        public SerialPort Client => m_serialClient.Provider;

        /// <summary>
        /// Gets the server URI of the <see cref="SerialClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri => $"{TransportProtocol}://{m_connectData["port"]}".ToLower();

        /// <summary>
        /// Gets or sets the needed number of bytes in the internal input buffer before a <see cref="ClientBase.OnReceiveDataComplete"/> event occurs.
        /// </summary>
        /// <remarks>
        /// This option is ignored under Mono deployments.
        /// </remarks>
        public int ReceivedBytesThreshold { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ClientBase.ReceiveData"/> event handler. Calling this method outside
        /// this event will have unexpected results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">No received data buffer has been defined to read.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            if (m_serialClient.ReceiveBuffer == null)
                throw new InvalidOperationException("No received data buffer has been defined to read.");

            int sourceLength = m_serialClient.BytesReceived - ReadIndex;
            int readBytes = length > sourceLength ? sourceLength : length;
            Buffer.BlockCopy(m_serialClient.ReceiveBuffer, ReadIndex, buffer, startIndex, readBytes);

            // Update read index for next call
            ReadIndex += readBytes;

            if (ReadIndex >= m_serialClient.BytesReceived)
                ReadIndex = 0;

            return readBytes;

        }

        /// <summary>
        /// Disconnects the <see cref="SerialClient"/> from the <see cref="SerialPort"/>.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState == ClientState.Disconnected)
                return;

            if (m_serialClient.Provider != null)
            {
                m_serialClient.Provider.DataReceived -= SerialPort_DataReceived;
                m_serialClient.Provider.ErrorReceived -= SerialPort_ErrorReceived;
            }

            m_serialClient.Reset();

            m_connectionThread?.Abort();

            OnConnectionTerminated();
        }

        /// <summary>
        /// Connects the <see cref="SerialClient"/> to the <see cref="SerialPort"/> asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="SerialClient"/> when it is connected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            m_connectionHandle = (ManualResetEvent)base.ConnectAsync();

            m_serialClient.SetReceiveBuffer(ReceiveBufferSize);

            m_serialClient.Provider = new SerialPort
            {
#if !MONO
                ReceivedBytesThreshold = ReceivedBytesThreshold,
#endif
                PortName = m_connectData["port"],
                BaudRate = int.Parse(m_connectData["baudRate"]),
                DataBits = int.Parse(m_connectData["dataBits"]),
                Parity = (Parity)Enum.Parse(typeof(Parity), m_connectData["parity"], true),
                StopBits = (StopBits)Enum.Parse(typeof(StopBits), m_connectData["stopBits"], true)
            };

            m_serialClient.Provider.DataReceived += SerialPort_DataReceived;
            m_serialClient.Provider.ErrorReceived += SerialPort_ErrorReceived;

            if (m_connectData.ContainsKey("dtrEnable"))
                m_serialClient.Provider.DtrEnable = m_connectData["dtrEnable"].ParseBoolean();

            if (m_connectData.ContainsKey("rtsEnable"))
                m_serialClient.Provider.RtsEnable = m_connectData["rtsEnable"].ParseBoolean();

            m_connectionThread = new Thread(OpenPort);
            m_connectionThread.Start();

            return m_connectionHandle;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SerialClient"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                // This will be done regardless of whether the object is finalized or disposed.
                if (!disposing)
                    return;

                // This will be done only when the object is disposed by calling Dispose().
                m_connectionHandle?.Dispose();
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
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
                throw new ArgumentException($"Port property is missing (Example: {DefaultConnectionString})");

            if (!m_connectData.ContainsKey("baudRate"))
                throw new ArgumentException($"BaudRate property is missing (Example: {DefaultConnectionString})");

            if (!m_connectData.ContainsKey("parity"))
                throw new ArgumentException($"Parity property is missing (Example: {DefaultConnectionString})");

            if (!m_connectData.ContainsKey("stopBits"))
                throw new ArgumentException($"StopBits property is missing (Example: {DefaultConnectionString})");

            if (!m_connectData.ContainsKey("dataBits"))
                throw new ArgumentException($"DataBits property is missing (Example: {DefaultConnectionString})");
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
            // Send data to the file asynchronously.
            WaitHandle handle = m_serialClient.Provider.BaseStream.BeginWrite(data, offset, length, SendDataAsyncCallback, null).AsyncWaitHandle;

            // Notify that the send operation has started.
            m_serialClient.Statistics.UpdateBytesSent(length);
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
                // Send operation failed to complete - don't raised exceptions for a closed port
                if (!m_disposed && m_serialClient.Provider != null && m_serialClient.Provider.IsOpen)
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
                    m_connectionHandle.Set();
                    OnConnectionEstablished();

                    break;
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
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

                // Retrieve data from the port.
                while (bytesRead < m_serialClient.Provider.BytesToRead)
                    bytesRead += m_serialClient.Provider.Read(m_serialClient.ReceiveBuffer, bytesRead, m_serialClient.ReceiveBuffer.Length - bytesRead);

                m_serialClient.BytesReceived = bytesRead;
                m_serialClient.Statistics.UpdateBytesReceived(bytesRead);

                // Notify of the retrieved data.
                OnReceiveDataComplete(m_serialClient.ReceiveBuffer, bytesRead);
            }
            catch (Exception ex)
            {
                // Don't raised exceptions for a closed port
                if (!m_disposed && m_serialClient.Provider != null && m_serialClient.Provider.IsOpen)
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

        /// <summary>
        /// Raises the <see cref="ClientBase.ConnectionException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ConnectionException"/> event.</param>
        protected override void OnConnectionException(Exception ex)
        {
            base.Disconnect();
            base.OnConnectionException(ex);
        }

        #endregion
    }
}