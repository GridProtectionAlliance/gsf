//******************************************************************************************************
//  DataSubscriber.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/20/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TimeSeriesFramework.Adapters;
using TVA;
using TVA.Communication;
using TVA.Security.Cryptography;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Represents a data subscribing client that will connect to a data publisher for a data subscription.
    /// </summary>
    public class DataSubscriber : InputAdapterBase
    {
        #region [ Members ]

        /// <summary>
        /// Occurs when client connection to the data publication server is established.
        /// </summary>
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Occurs when client connection to the data publication server is terminated.
        /// </summary>
        public event EventHandler ConnectionTerminated;

        // Fields
        private TVA.Communication.TcpClient m_dataClient;
        private List<ServerCommand> m_requests;
        private bool m_synchronizedSubscription;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataSubscriber"/>.
        /// </summary>
        public DataSubscriber()
        {
            // Create a new TCP server
            m_dataClient = new TcpClient();

            // Initialize default settings
            m_dataClient.ConnectionString = "server=localhost:6165";
            m_dataClient.PayloadAware = true;
            m_dataClient.PersistSettings = false;

            // Attach to desired events
            m_dataClient.ConnectionAttempt += m_dataClient_ConnectionAttempt;
            m_dataClient.ConnectionEstablished += m_dataClient_ConnectionEstablished;
            m_dataClient.ConnectionException += m_dataClient_ConnectionException;
            m_dataClient.ConnectionTerminated += m_dataClient_ConnectionTerminated;
            m_dataClient.HandshakeProcessTimeout += m_dataClient_HandshakeProcessTimeout;
            m_dataClient.HandshakeProcessUnsuccessful += m_dataClient_HandshakeProcessUnsuccessful;
            m_dataClient.ReceiveDataComplete += m_dataClient_ReceiveDataComplete;
            m_dataClient.ReceiveDataException += m_dataClient_ReceiveDataException;
            m_dataClient.ReceiveDataTimeout += m_dataClient_ReceiveDataTimeout;
            m_dataClient.SendDataException += m_dataClient_SendDataException;

            m_requests = new List<ServerCommand>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a flag that determines if this <see cref="DataSubscriber"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the status of this <see cref="DataSubscriber"/>.
        /// </summary>
        /// <remarks>
        /// Derived classes should provide current status information about the adapter for display purposes.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("         Subscription mode: {0}", m_synchronizedSubscription ? "Synchronized" : "Unsynchronized");
                status.AppendLine();
                status.AppendFormat("  Pending command requests: {0}", m_requests.Count);
                status.AppendLine();

                if (m_dataClient != null)
                    status.Append(m_dataClient.Status);

                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataSubscriber"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_dataClient != null)
                        {
                            if (m_dataClient.CurrentState == ClientState.Connected)
                                m_dataClient.Disconnect();

                            m_dataClient.ConnectionAttempt -= m_dataClient_ConnectionAttempt;
                            m_dataClient.ConnectionEstablished -= m_dataClient_ConnectionEstablished;
                            m_dataClient.ConnectionException -= m_dataClient_ConnectionException;
                            m_dataClient.ConnectionTerminated -= m_dataClient_ConnectionTerminated;
                            m_dataClient.HandshakeProcessTimeout -= m_dataClient_HandshakeProcessTimeout;
                            m_dataClient.HandshakeProcessUnsuccessful -= m_dataClient_HandshakeProcessUnsuccessful;
                            m_dataClient.ReceiveDataComplete -= m_dataClient_ReceiveDataComplete;
                            m_dataClient.ReceiveDataException -= m_dataClient_ReceiveDataException;
                            m_dataClient.ReceiveDataTimeout -= m_dataClient_ReceiveDataTimeout;
                            m_dataClient.SendDataException -= m_dataClient_SendDataException;
                            m_dataClient.Dispose();
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Initializes <see cref="DataSubscriber"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Pass all connection string settings to data client
            if (m_dataClient != null)
                m_dataClient.ConnectionString = ConnectionString;

            Initialized = true;
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a synchronized set of data points.
        /// </summary>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="password">Pass-phrase required to connect to subscriber.</param>
        /// <param name="framesPerSecond">The desired number of data frames per second.</param>
        /// <param name="lagTime">Allowed past time deviation tolerance, in seconds (can be subsecond).</param>
        /// <param name="leadTime">Allowed future time deviation tolerance, in seconds (can be subsecond).</param>
        /// <param name="filterExpression">Filtering expression that defines the measurements that are being subscribed.</param>
        /// <param name="useLocalClockAsRealTime">Boolean value that determines whether or not to use the local clock time as real-time.</param>
        /// <param name="ignoreBadTimestamps">Boolean value that determines if bad timestamps (as determined by measurement's timestamp quality) should be ignored when sorting measurements.</param>
        /// <param name="allowSortsByArrival"> Gets or sets flag that determines whether or not to allow incoming measurements with bad timestamps to be sorted by arrival time.</param>
        /// <param name="timeResolution">Gets or sets the maximum time resolution, in ticks, to use when sorting measurements by timestamps into their proper destination frame.</param>
        /// <param name="allowPreemptivePublishing">Gets or sets flag that allows system to preemptively publish frames assuming all expected measurements have arrived.</param>
        /// <param name="downsamplingMethod">Gets the total number of downsampled measurements processed by the concentrator.</param>
        /// <returns><c>true</c> if subscribe was successful; otherwise <c>false</c>.</returns>
        public virtual bool SynchronizedSubscribe(bool compactFormat, string password, int framesPerSecond, double lagTime, double leadTime, string filterExpression, bool useLocalClockAsRealTime = false, bool ignoreBadTimestamps = false, bool allowSortsByArrival = true, long timeResolution = Ticks.PerMillisecond, bool allowPreemptivePublishing = true, DownsamplingMethod downsamplingMethod = DownsamplingMethod.LastReceived)
        {
            StringBuilder connectionString = new StringBuilder();

            connectionString.AppendFormat("password={0}; ", password); //password.Encrypt(DataPublisher.CipherLookupKey, CipherStrength.Aes256));
            connectionString.AppendFormat("framesPerSecond={0}; ", framesPerSecond);
            connectionString.AppendFormat("lagTime={0}; ", lagTime);
            connectionString.AppendFormat("leadTime={0}; ", leadTime);
            connectionString.AppendFormat("inputMeasurementKeys={{{0}}}; ", filterExpression);
            connectionString.AppendFormat("useLocalClockAsRealTime={0}; ", useLocalClockAsRealTime);
            connectionString.AppendFormat("ignoreBadTimestamps={0}; ", ignoreBadTimestamps);
            connectionString.AppendFormat("allowSortsByArrival={0}; ", allowSortsByArrival);
            connectionString.AppendFormat("timeResolution={0}; ", (long)timeResolution);
            connectionString.AppendFormat("allowPreemptivePublishing={0}; ", allowPreemptivePublishing);
            connectionString.AppendFormat("downsamplingMethod={0}", downsamplingMethod.ToString());

            return Subscribe(true, compactFormat, connectionString.ToString());
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for an unsynchronized set of data points.
        /// </summary>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="password">Pass-phrase required to connect to subscriber.</param>
        /// <param name="throttled">Boolean value that determines if data should be throttled at a set transmission interval or sent on change.</param>
        /// <param name="filterExpression">Filtering expression that defines the measurements that are being subscribed.</param>
        /// <param name="lagTime">When <paramref name="throttled"/> is <c>true</c>, defines the data transmission speed in seconds (can be subsecond).</param>
        /// <param name="leadTime">When <paramref name="throttled"/> is <c>true</c>, defines the allowed time deviation tolerance to real-time in seconds (can be subsecond).</param>
        /// <param name="useLocalClockAsRealTime">When <paramref name="throttled"/> is <c>true</c>, defines boolean value that determines whether or not to use the local clock time as real-time. Set to <c>false</c> to use latest received measurement timestamp as real-time.</param>
        /// <returns><c>true</c> if subscribe was successful; otherwise <c>false</c>.</returns>
        public virtual bool UnsynchronizedSubscribe(bool compactFormat, string password, bool throttled, string filterExpression, double lagTime = 10.0D, double leadTime = 5.0D, bool useLocalClockAsRealTime = false)
        {
            StringBuilder connectionString = new StringBuilder();

            connectionString.AppendFormat("password={0}; ", password); //password.Encrypt(DataPublisher.CipherLookupKey, CipherStrength.Aes256));
            connectionString.AppendFormat("trackLatestMeasurements={0}; ", throttled);
            connectionString.AppendFormat("lagTime={0}; ", lagTime);
            connectionString.AppendFormat("leadTime={0}; ", leadTime);
            connectionString.AppendFormat("inputMeasurementKeys={{{0}}}; ", filterExpression);
            connectionString.AppendFormat("useLocalClockAsRealTime={0}", useLocalClockAsRealTime);
            
            return Subscribe(false, compactFormat, connectionString.ToString());
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a set of data points.
        /// </summary>
        /// <param name="synchronized">Boolean value that determines if subscription should be synchronized.</param>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="connectionString">Connection string that defines required and optional parameters for the subscription.</param>
        /// <returns><c>true</c> if subscribe was successful; otherwise <c>false</c>.</returns>
        public virtual bool Subscribe(bool synchronized, bool compactFormat, string connectionString)
        {
            bool success = false;

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                try
                {
                    MemoryStream buffer = new MemoryStream();
                    DataPacketFlags flags = DataPacketFlags.NoFlags;
                    byte[] bytes;

                    if (synchronized)
                        flags |= DataPacketFlags.Synchronized;

                    if (compactFormat)
                        flags |= DataPacketFlags.Compact;

                    // Write data packet flags into buffer
                    buffer.WriteByte((byte)flags);

                    // Get encoded bytes of connection string
                    bytes = Encoding.Unicode.GetBytes(connectionString);

                    // Write encoded connection string length into buffer
                    buffer.Write(EndianOrder.BigEndian.GetBytes(bytes.Length), 0, 4);

                    // Encode connection string into buffer
                    buffer.Write(bytes, 0, bytes.Length);

                    // Cache subscribed synchronization state
                    m_synchronizedSubscription = synchronized;

                    // Send subscribe server command with associated command buffer
                    success = SendServerCommand(ServerCommand.Subscribe, buffer.ToArray());                        
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Exception occurred while trying to make publisher subscription: " + ex.Message, ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException("Cannot make publisher subscription without a connection string."));

            return success;
        }
        
        /// <summary>
        /// Unsubscribes from a data publisher.
        /// </summary>
        /// <returns><c>true</c> if unsubscribe was successful; otherwise <c>false</c>.</returns>
        public virtual bool Unsubscribe()
        {
            // Send unsubscribe server command
            return SendServerCommand(ServerCommand.Unsubscribe, null);
        }

        /// <summary>
        /// Sends a server command to the publisher connection.
        /// </summary>
        /// <param name="commandCode"><see cref="ServerCommand"/> to send.</param>
        /// <param name="data">Command data to send.</param>
        /// <returns><c>true</c> if send was successful; otherwise <c>false</c>.</returns>
        protected virtual bool SendServerCommand(ServerCommand commandCode, byte[] data)
        {
            bool success = false;

            if (m_dataClient != null && m_dataClient.CurrentState == ClientState.Connected)
            {
                try
                {
                    MemoryStream commandPacket = new MemoryStream();

                    // Write command code into command packet
                    commandPacket.WriteByte((byte)commandCode);

                    // Write command buffer into command packet
                    if (data != null && data.Length > 0)
                        commandPacket.Write(data, 0, data.Length);

                    // Send command packet to publisher
                    m_dataClient.SendAsync(commandPacket.ToArray());

                    // Track server command in pending request queue
                    lock (m_requests)
                    {
                        // Make sure a pending request does not already exist
                        int index = m_requests.BinarySearch(commandCode);

                        if (index < 0)
                        {
                            // Add the new server command to the request list
                            m_requests.Add(commandCode);

                            // Make sure requests are sorted to allow for binary searching
                            m_requests.Sort();
                        }
                    }

                    success = true;
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("Exception occurred while trying to send server command \"{0}\" to publisher: {1}", commandCode, ex.Message), ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException(string.Format("Subscriber is currently unconnected. Cannot send server command \"{0}\" to publisher.", commandCode)));

            return success;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="DataSubscriber"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_dataClient.Connect();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="DataSubscriber"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_dataClient.Disconnect();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="DataSubscriber"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if (m_dataClient != null && m_dataClient.CurrentState == ClientState.Connected)
                return string.Format("Subscriber is connected and receiving {0} data points", m_synchronizedSubscription ? "synchronized" : "unsynchronized").CenterText(maxLength); 

            return "Subscriber is not connected.".CenterText(maxLength);
        }

        /// <summary>
        /// Get message from string based response.
        /// </summary>
        /// <param name="buffer">Response buffer.</param>
        /// <param name="startIndex">Start index of response message.</param>
        /// <param name="length">Length of response message.</param>
        /// <returns>Decoded response string.</returns>
        protected string InterpretResponseMessage(byte[] buffer, int startIndex, int length)
        {
            return Encoding.Unicode.GetString(buffer, startIndex, length);
        }

        // Process server response
        private void m_dataClient_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
        {
            byte[] buffer = e.Argument1;
            int length = e.Argument2;

            if (buffer != null && length > 0)
            {
                try
                {
                    ServerResponse responseCode = (ServerResponse)buffer[0];
                    ServerCommand commandCode = (ServerCommand)buffer[1];
                    int responseLength = EndianOrder.BigEndian.ToInt32(buffer, 2);
                    int responseIndex = 6;
                    bool solicited = false;

                    // See if this was a solicited response to a requested server command
                    if (responseCode != ServerResponse.DataPacket)
                    {
                        lock (m_requests)
                        {
                            int index = m_requests.BinarySearch(commandCode);

                            if (index >= 0)
                            {
                                solicited = true;
                                m_requests.RemoveAt(index);
                            }
                        }
                    }

                    switch (responseCode)
                    {
                        case ServerResponse.Succeeded:
                            if (solicited)
                                OnStatusMessage("Success code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                            else
                                OnProcessException(new InvalidOperationException("Publisher sent a success code for an unsolicited server command: " + commandCode));
                            break;
                        case ServerResponse.Failed:
                            if (solicited)
                                OnStatusMessage("Failure code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                            else
                                OnProcessException(new InvalidOperationException("Publisher sent a failed code for an unsolicited server command: " + commandCode));
                            break;
                        case ServerResponse.DataPacket:
                            // Deserialize data packet
                            List<IMeasurement> measurements = new List<IMeasurement>();
                            SerializableMeasurement measurement;
                            SerializableMeasurementSlim slimMeasurement;
                            DataPacketFlags flags;
                            Ticks timestamp = 0;
                            int count;

                            // Get data packet flags
                            flags = (DataPacketFlags)buffer[responseIndex];
                            responseIndex++;

                            bool synchronizedMeasurements = ((byte)(flags & DataPacketFlags.Synchronized) > 0);
                            bool compactMeasurementFormat = ((byte)(flags & DataPacketFlags.Compact) > 0);
                            
                            // Synchronized packets contain a frame level timestamp
                            if (synchronizedMeasurements)
                            {
                                timestamp = EndianOrder.BigEndian.ToInt64(buffer, responseIndex);
                                responseIndex += 8;
                            }

                            // Deserialize number of measurements that follow
                            count = EndianOrder.BigEndian.ToInt32(buffer, responseIndex);
                            responseIndex += 4;

                            // Deserialize measurements
                            for (int i = 0; i < count; i++)
                            {
                                if (compactMeasurementFormat)
                                {
                                    // Deserialize compact measurement format
                                    slimMeasurement = new SerializableMeasurementSlim(!synchronizedMeasurements);
                                    responseIndex += slimMeasurement.Initialize(buffer, responseIndex, length - responseIndex);

                                    // Apply timestamp from frame if not included in transmission
                                    if (!slimMeasurement.IncludeTime)
                                        slimMeasurement.Timestamp = timestamp;

                                    measurements.Add(slimMeasurement);
                                }
                                else
                                {
                                    // Deserialize full measurement format
                                    measurement = new SerializableMeasurement();
                                    responseIndex += measurement.Initialize(buffer, responseIndex, length - responseIndex);
                                    measurements.Add(measurement);
                                }
                            }

                            // Expose new measurements to consumer
                            OnNewMeasurements(measurements);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Failed to process publisher response packet due to exception: " + ex.Message, ex));
                }
            }
        }

        private void m_dataClient_ConnectionEstablished(object sender, EventArgs e)
        {
            // Make sure no existing requests are queued for a new publisher connection
            lock (m_requests)
            {
                m_requests.Clear();
            }

            if (ConnectionEstablished != null)
                ConnectionEstablished(sender, e);

            OnStatusMessage("Data subscriber connection to publisher was established.");
        }

        private void m_dataClient_ConnectionTerminated(object sender, EventArgs e)
        {
            if (ConnectionTerminated != null)
                ConnectionTerminated(sender, e);

            OnStatusMessage("Data subscriber connection to publisher was terminated.");
        }

        private void m_dataClient_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while attempting publisher connection: " + ex.Message, ex));
        }

        private void m_dataClient_ConnectionAttempt(object sender, EventArgs e)
        {
            OnStatusMessage("Data subscriber attempting connection to publisher...");
        }

        private void m_dataClient_SendDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            
            if (!(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while sending data to publisher connection: " + ex.Message, ex));
        }

        private void m_dataClient_ReceiveDataTimeout(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data subscriber timed out while receiving data from publisher connection"));
        }

        private void m_dataClient_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while receiving data from publisher connection: " + ex.Message, ex));
        }

        private void m_dataClient_HandshakeProcessUnsuccessful(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data subscriber failed to be validated by publisher"));
        }

        private void m_dataClient_HandshakeProcessTimeout(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data subscriber timed out while waiting for publisher validation"));
        }

        #endregion
    }
}
