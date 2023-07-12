//******************************************************************************************************
//  AudioPlayback.cs - Gbtc
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
//  07/07/2011 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Code translated from "NAudioWpfDemo" developed by Mark Heath
//  found in the "NAudio" project: http://naudio.codeplex.com/
//
//  Copyright (c) Mark Heath 2008
//  Microsoft Public License (Ms-PL): http://www.opensource.org/licenses/ms-pl
//
//******************************************************************************************************

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Timers;
using GSF;
using GSF.IO;
using GSF.Media.Music;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
using NAudio.Wave;
using Timer = System.Timers.Timer;

namespace WavSubscriptionDemo
{
    /// <summary>
    /// Represents the states that playback can be in.
    /// </summary>
    public enum PlaybackState
    {
        Connecting,
        Connected,
        Disconnected,
        TimedOut,
        Buffering,
        Playing,
        Stopped,
        Disposed,
        Exception
    }

    class AudioPlayback : IDisposable
    {
        public const int MINIMUM_SAMPLE_RATE = 8000;
        public const int DEFAULT_SAMPLE_RATE = 44100;
        public const int DEFAULT_NUM_CHANNELS = 2;
        public const int PERIOD_COUNT = 10;
        public const string URI_SEPARATOR = "?";

        private DataSubscriber m_dataSubscriber;
        private Timer m_timeoutTimer;
        private DataSet m_metadata;

        private ConcurrentQueue<IMeasurement> m_buffer;
        private PrecisionTimer m_dumpTimer;
        private IWavePlayer m_wavePlayer;
        private BufferedWaveProvider m_waveProvider;
        private WaveFileWriter m_waveFileWriter;

        private ConcurrentDictionary<Guid, int> m_channelIndexes;
        private int m_sampleRate;
        private int m_numChannels;

        private Timer m_statTimer;
        private long m_startTime;
        private long m_measurementCount;
        private long m_lastStatCalcTime;
        private long m_lastMeasurementCount;
        private long m_lastTotalBytesReceived;

        private int[] m_samplesPerSecondPeriod;
        private int[] m_kilobitsPerSecondPeriod;
        private float[] m_smoothnessPeriod;
        private double m_lostSamples;
        private int m_periodIndexCount;

        /// <summary>
        /// Provides the song list to the user interface.
        /// </summary>
        public event EventHandler<EventArgs<List<string>>> GotSongList;

        /// <summary>
        /// Provides sample data to the user interface.
        /// </summary>
        public event EventHandler<SampleEventArgs> OnSample;

        /// <summary>
        /// Provides the statistics calculated in this class to the user interface.
        /// </summary>
        public event EventHandler<EventArgs<int, int, float, double>> StatsUpdated;

        /// <summary>
        /// Notifies the user interface of the state of this player.
        /// </summary>
        public event EventHandler<EventArgs<PlaybackState, string>> StateChanged;

        /// <summary>
        /// Gets or sets the connection URI.
        /// </summary>
        /// <remarks>
        /// This URI is supplied by the user and takes the form <code>hostname:port[?udp=port]</code>.
        /// The hostname and port define how to communicate with the stream server.
        /// If the udp parameter is supplied, the stream server will initiate a UDP stream with
        /// the client when a song is chosen. The port value of the UDP parameter defines the port
        /// on which the client will listen for data from the server.
        /// </remarks>
        public string ConnectionUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a flag that determines whether to enable compression on the UDP stream.
        /// </summary>
        public bool EnableCompression
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a flag that determines whether to enable encryption on the data stream.
        /// </summary>
        public bool EnableEncryption
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a flag that determines whether to enable IPv6.
        /// </summary>
        /// <remarks>
        /// This application forces the use of either IPv4 or IPv6.
        /// If IPv6 is not enabled, IPv4 will be forced.
        /// </remarks>
        public bool IPv6Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a flag that determines if ZeroMQ channel should be used.
        /// </summary>
        public bool UseZeroMQChannel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if historical replay is enabled.
        /// </summary>
        public bool ReplayEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time at which the playback should start.
        /// </summary>
        public string ReplayStartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time at which the playback should stop.
        /// </summary>
        public string ReplayStopTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the sample rate of the playback.
        /// </summary>
        /// <remarks>
        /// This is used for visualization tools that need to operate at the
        /// sample rate for synchronization. It should also be noted that the
        /// <see cref="AudioPlayback"/> buffers a full second of data before
        /// starting playback.
        /// </remarks>
        public int SampleRate => m_sampleRate;

        /// <summary>
        /// Connects to the stream server defined by the <see cref="ConnectionUri"/>.
        /// </summary>
        public void ConnectToStreamSource()
        {
            OnStateChanged(PlaybackState.Connecting);

            try
            {
                m_dataSubscriber = CreateDataSubscriber();
                m_timeoutTimer = CreateTimeoutTimer();
                m_timeoutTimer.Start();
            }
            catch (Exception ex)
            {
                DisconnectFromStreamSource();
                OnStateChanged(PlaybackState.Exception, "Connection attempt failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Disconnects from the stream server and releases resources.
        /// </summary>
        public void DisconnectFromStreamSource()
        {
            DataSubscriber subscriber;
            Timer timeoutTimer;
            DataSet metadata;

            Stop();

            subscriber = m_dataSubscriber;
            timeoutTimer = m_timeoutTimer;
            metadata = m_metadata;

            m_dataSubscriber = null;
            m_timeoutTimer = null;
            m_metadata = null;

            ReleaseDataSubscriber(subscriber);
            ReleaseTimeoutTimer(timeoutTimer);
            ReleaseMetadata(metadata);
            StopRecording();

            OnStateChanged(PlaybackState.Disconnected);
            OnGotSongList(null);
        }

        /// <summary>
        /// Opens the stream and starts playback.
        /// </summary>
        /// <param name="songName">The name of the song to be played.</param>
        public void Play(string songName)
        {
            if (m_dataSubscriber is null || m_metadata is null)
                return;

            UnsynchronizedSubscriptionInfo info;

            StringBuilder filterExpression = new StringBuilder();
            DataTable deviceTable = m_metadata.Tables["DeviceDetail"];
            DataTable measurementTable = m_metadata.Tables["MeasurementDetail"];

            Dictionary<string, string> uriSettings;
            string dataChannel = null;
            int uriIndex = ConnectionUri.IndexOf(URI_SEPARATOR, StringComparison.Ordinal);

            m_channelIndexes = new ConcurrentDictionary<Guid, int>();
            m_sampleRate = DEFAULT_SAMPLE_RATE;
            m_numChannels = DEFAULT_NUM_CHANNELS;

            // Get sample rate from metadata.
            string sampleRate = deviceTable?.Rows.Cast<DataRow>()
                .Single(row => row["Acronym"].ToNonNullString() == songName)["FramesPerSecond"].ToNonNullString();

            if (!string.IsNullOrEmpty(sampleRate))
                m_sampleRate = int.Parse(sampleRate);

            // Get measurements from metadata.
            if (measurementTable is not null)
            {
                IEnumerable<DataRow> measurementRows = measurementTable.Rows.Cast<DataRow>()
                    .Where(row => row["DeviceAcronym"].ToNonNullString() == songName)
                    .Where(row => new[] { "ALOG", "VPHM", "VPHA" }.Contains(row["SignalAcronym"].ToNonNullString()))
                    .Where(row => row["Enabled"].ToNonNullString().ParseBoolean())
                    .OrderBy(row => row["ID"].ToNonNullString());

                m_numChannels = 0;

                foreach (DataRow row in measurementRows)
                {
                    Guid measurementID = Guid.Parse(row["SignalID"].ToNonNullString());

                    if (m_numChannels > 0)
                        filterExpression.Append(';');

                    filterExpression.Append(measurementID);
                    m_channelIndexes[measurementID] = m_numChannels;
                    m_numChannels++;
                }
            }

            // Create UDP data channel if specified.
            if (uriIndex >= 0)
            {
                uriSettings = ConnectionUri.Substring(uriIndex + URI_SEPARATOR.Length).ParseKeyValuePairs('&');

                if (uriSettings.ContainsKey("udp"))
                    dataChannel = $"dataChannel={{port={uriSettings["udp"]}; interface={(IPv6Enabled ? "::0" : "0.0.0.0")}}}";
            }

            m_buffer = new ConcurrentQueue<IMeasurement>();
            m_dumpTimer = CreateDumpTimer();
            m_statTimer = CreateStatTimer();
            m_waveProvider = new BufferedWaveProvider(new WaveFormat(m_sampleRate < MINIMUM_SAMPLE_RATE ? MINIMUM_SAMPLE_RATE : m_sampleRate, m_numChannels));
            m_wavePlayer = CreateWavePlayer(m_waveProvider);
            m_waveProvider.DiscardOnBufferOverflow = true;

            info = new UnsynchronizedSubscriptionInfo(false)
            {
                FilterExpression = filterExpression.ToString(),
                ExtraConnectionStringParameters = dataChannel
            };

            if (ReplayEnabled)
            {
                const string DateTimeFormat = "MM-dd-yyyy HH:mm:ss.fff";

                info.StartTime = DateTime.TryParse(ReplayStartTime, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeLocal, out DateTime startTime) ? 
                    startTime.ToUniversalTime().ToString(DateTimeFormat) : 
                    ReplayStartTime;

                info.StopTime = DateTime.TryParse(ReplayStopTime, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeLocal, out DateTime stopTime) ?
                    stopTime.ToUniversalTime().ToString(DateTimeFormat) :
                    ReplayStopTime;

                info.ProcessingInterval = 1000;

                //if (info.ProcessingInterval == 0)
                //    info.ProcessingInterval = 1;
            }

            m_statTimer.Start();
            m_wavePlayer.Play();
            m_dataSubscriber.UnsynchronizedSubscribe(info);
            m_timeoutTimer.Start();
            OnStateChanged(PlaybackState.Buffering);
        }

        /// <summary>
        /// Stops playback and releases resources, but does not close the stream.
        /// </summary>
        public void Stop()
        {
            PrecisionTimer dumpTimer = m_dumpTimer;
            IWavePlayer player = m_wavePlayer;
            Timer statTimer = m_statTimer;

            m_dumpTimer = null;
            m_wavePlayer = null;
            m_statTimer = null;

            ReleaseDumpTimer(dumpTimer);
            ReleaseWavePlayer(player);
            ReleaseStatTimer(statTimer);
            StopRecording();

            m_dataSubscriber?.Unsubscribe();

            OnStateChanged(PlaybackState.Stopped);
        }

        /// <summary>
        /// Starts recording to a WAV file.
        /// </summary>
        /// <param name="fileName">WAV file name.</param>
        public void StartRecording(string fileName)
        {
            if (m_waveProvider is null)
                return;

            if (m_waveFileWriter is not null)
                StopRecording();

            m_waveFileWriter = new WaveFileWriter(fileName, m_waveProvider.WaveFormat);
        }

        /// <summary>
        /// Stops recording to a WAV file.
        /// </summary>
        public void StopRecording()
        {
            m_waveFileWriter?.Dispose();
            m_waveFileWriter = null;
        }

        /// <summary>
        /// Stops playback, disconnects, and releases all resources held by the <see cref="AudioPlayback"/>.
        /// </summary>
        public void Dispose()
        {
            DisconnectFromStreamSource();
            OnStateChanged(PlaybackState.Disposed);
        }

        // Triggers the GotSongList event.
        private void OnGotSongList(List<string> songList)
        {
            GotSongList?.Invoke(this, new EventArgs<List<string>>(songList));
        }

        // Triggers the StateChanged event.
        private void OnStateChanged(PlaybackState state, string message = null)
        {
            StateChanged?.Invoke(this, new EventArgs<PlaybackState, string>(state, message));
        }

        // ------ Data Subscriber Methods ------

        // Creates and starts a data subscriber using the connection information supplied by the user in ConnectionUri.
        // Subscribers created through this method should also be released by the ReleaseDataSubscriber method.
        private DataSubscriber CreateDataSubscriber()
        {
            DataSubscriber subscriber = new DataSubscriber();
            int index = ConnectionUri.IndexOf(URI_SEPARATOR, StringComparison.Ordinal);
            string server = (index >= 0) ? ConnectionUri.Substring(0, index) : ConnectionUri;

            subscriber.StatusMessage += DataSubscriber_StatusMessage;
            subscriber.ProcessException += DataSubscriber_ProcessException;
            subscriber.ConnectionEstablished += DataSubscriber_ConnectionEstablished;
            subscriber.ConnectionTerminated += DataSubscriber_ConnectionTerminated;
            subscriber.MetaDataReceived += DataSubscriber_MetaDataReceived;
            subscriber.DataStartTime += DataSubscriber_DataStartTime;
            subscriber.NewMeasurements += DataSubscriber_NewMeasurements;

            subscriber.ConnectionString = $"server={server}; interface={(IPv6Enabled ? "::0" : "0.0.0.0")};{(UseZeroMQChannel ? " useZeroMQChannel=true;" : "")} localCertificate={FilePath.GetAbsolutePath("Local.cer")}; remoteCertificate={FilePath.GetAbsolutePath("Remote.cer")}; validPolicyErrors={~SslPolicyErrors.None}; validChainFlags={~X509ChainStatusFlags.NoError}";
            subscriber.SecurityMode = EnableEncryption ? SecurityMode.TLS : SecurityMode.None;

            subscriber.OperationalModes = DataSubscriber.DefaultOperationalModes;
            subscriber.CompressionModes = CompressionModes.GZip;

            if (EnableCompression)
            {
                subscriber.OperationalModes |= OperationalModes.CompressPayloadData;

                bool usingUDP = false;

                if (index >= 0)
                    usingUDP = ConnectionUri.Substring(index + URI_SEPARATOR.Length).ParseKeyValuePairs('&').ContainsKey("udp");

                if (!usingUDP)
                    subscriber.CompressionModes |= CompressionModes.TSSC; // TSSC mode requires TCP connection
            }

            subscriber.ReceiveInternalMetadata = true;
            subscriber.ReceiveExternalMetadata = true;

            subscriber.Initialize();
            subscriber.Start();

            return subscriber;
        }

        // Unsubscribes, detaches from events, and disposes of the subscriber.
        // Subscribers created through the CreateDataSubscriber should be released by this method.
        private void ReleaseDataSubscriber(DataSubscriber subscriber)
        {
            if (subscriber is not null)
            {
                subscriber.Unsubscribe();

                subscriber.StatusMessage -= DataSubscriber_StatusMessage;
                subscriber.ProcessException -= DataSubscriber_ProcessException;
                subscriber.ConnectionEstablished -= DataSubscriber_ConnectionEstablished;
                subscriber.ConnectionTerminated -= DataSubscriber_ConnectionTerminated;
                subscriber.MetaDataReceived -= DataSubscriber_MetaDataReceived;
                subscriber.DataStartTime -= DataSubscriber_DataStartTime;
                subscriber.NewMeasurements -= DataSubscriber_NewMeasurements;

                subscriber.Dispose();
            }
        }

        // Disposes of the metadata object. There is no corresponding creation method
        // because the metadata is received through the subscriber event.
        private void ReleaseMetadata(DataSet metadata)
        {
            metadata?.Dispose();
        }

        // Handles the subscriber's StatusMessage event.
        private void DataSubscriber_StatusMessage(object sender, EventArgs<string> e)
        {
            Console.WriteLine(e.Argument);
        }

        // Handles the subscriber's ProcessException event.
        private void DataSubscriber_ProcessException(object sender, EventArgs<Exception> e)
        {
            Console.Error.WriteLine(e.Argument.Message);
        }

        // Handles the subscriber's ConnectionEstablished event.
        private void DataSubscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            // It may seem redundant to stop and start the timeout timer,
            // but we are restarting the timer for the full ten seconds.
            // This is because the connection was successful,
            // but metadata refresh can still timeout.
            if (m_timeoutTimer is not null && m_dataSubscriber is not null)
            {
                m_timeoutTimer.Stop();

                m_dataSubscriber.SendServerCommand(ServerCommand.MetaDataRefresh);

                //if (EnableEncryption)
                //    m_dataSubscriber.SendServerCommand(ServerCommand.RotateCipherKeys);

                m_timeoutTimer.Start();
            }
        }

        // Handles the subscriber's ConnectionTerminated event.
        private void DataSubscriber_ConnectionTerminated(object sender, EventArgs e)
        {
            DisconnectFromStreamSource();
        }

        // Handles the subscriber's MetaDataReceived event.
        private void DataSubscriber_MetaDataReceived(object sender, EventArgs<DataSet> e)
        {
            List<string> songs;
            DataTable deviceTable;

            // Stop the timeout timer because the connection was successful.
            m_timeoutTimer?.Stop();

            m_metadata = e.Argument;
            deviceTable = m_metadata.Tables["DeviceDetail"];

            // Get the song list from the metadata.
            if (deviceTable is not null)
            {
                songs = deviceTable.Rows.Cast<DataRow>()
                    .Where(row => row["Enabled"].ToNonNullString("0").ParseBoolean())
                    .Select(row => row["Acronym"].ToNonNullString()).ToList();

                //.Where(row => row["ProtocolName"].ToNonNullString() == "Wave Form Input Adapter")

                OnGotSongList(songs);
            }

            // Set playback state to connected.
            OnStateChanged(PlaybackState.Connected);
        }

        // Handles the subscriber's DataStartTime event.
        private void DataSubscriber_DataStartTime(object sender, EventArgs<Ticks> e)
        {
            // We cannot use the time sent by the server because we don't
            // know whether our clock is synchronized with the server's.
            m_startTime = DateTime.UtcNow.Ticks;
        }

        // Handles the subscriber's NewMeasurements event.
        private void DataSubscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            // Gather statistics.
            Interlocked.Add(ref m_measurementCount, e.Argument.Count);

            // Queue the measurements up in the buffer.
            foreach (IMeasurement measurement in e.Argument)
            {
                if (m_channelIndexes.ContainsKey(measurement.ID))
                {
                    // Perform a rough per signal upsample if minimum sample rate is not met
                    if (m_sampleRate < MINIMUM_SAMPLE_RATE)
                    {
                        IMeasurement upsampledMeasurement;
                        double frequency = measurement.Value;

                        for (int i = 0; i < MINIMUM_SAMPLE_RATE / m_sampleRate; i++)
                        {
                            upsampledMeasurement = Measurement.Clone(measurement);
                            upsampledMeasurement.Value = Timbre.PureTone(frequency, i, 0, MINIMUM_SAMPLE_RATE) * Damping.Natural(i, MINIMUM_SAMPLE_RATE / m_sampleRate, MINIMUM_SAMPLE_RATE) * (Int16.MaxValue * 0.90D);
                            m_buffer.Enqueue(upsampledMeasurement);
                        }
                    }
                    else
                    {
                        m_buffer.Enqueue(measurement);
                    }
                }
            }

            // If we've managed to buffer a full second of data,
            // stop the timeout timer and start the dump timer.
            // The dump timer is sending measurements to the sound card
            // so change the playback state to playing.
            if (m_dumpTimer is not null && !m_dumpTimer.IsRunning && m_buffer.Count / 2 > m_sampleRate)
            {
                m_timeoutTimer?.Stop();
                m_dumpTimer.Start();
                OnStateChanged(PlaybackState.Playing);
            }
        }

        // ------ Timer Methods ------

        // Create the timeout timer. Timers created through this method
        // should be released by the ReleaseTimeoutTimer method.
        private Timer CreateTimeoutTimer()
        {
            Timer timeoutTimer = new Timer();

            timeoutTimer.AutoReset = false;
            timeoutTimer.Interval = 15000.0;
            timeoutTimer.Elapsed += TimeoutTimer_Elapsed;

            return timeoutTimer;
        }

        // Detaches from events, stops, and disposes of the timeout timer. Timers created
        // through the CreateTimeoutTimer method should be released through this method.
        private void ReleaseTimeoutTimer(Timer timeoutTimer)
        {
            if (timeoutTimer is not null)
            {
                timeoutTimer.Elapsed -= TimeoutTimer_Elapsed;
                timeoutTimer.Stop();
                timeoutTimer.Dispose();
            }
        }

        // Creates the dump timer. Timers created through this method
        // should be released through the ReleaseDumpTimer method.
        private PrecisionTimer CreateDumpTimer()
        {
            PrecisionTimer dumpTimer = new PrecisionTimer();

            dumpTimer.AutoReset = true;
            dumpTimer.Period = 100;
            dumpTimer.Tick += DumpTimer_Tick;

            return dumpTimer;
        }

        // Detaches from events, stops, and disposes of the dump timer. Timers created
        // through the CreateDumpTimer method should be released through this method.
        private void ReleaseDumpTimer(PrecisionTimer dumpTimer)
        {
            if (dumpTimer is not null)
            {
                dumpTimer.Tick -= DumpTimer_Tick;
                dumpTimer.Stop();
                dumpTimer.Dispose();
            }
        }

        // Creates the stat timer. Timers created through this method
        // should be released through the ReleaseStatTimer method.
        private Timer CreateStatTimer()
        {
            Timer statTimer = new Timer();

            statTimer.AutoReset = true;
            statTimer.Interval = 1000.0;
            statTimer.Elapsed += StatTimer_Elapsed;

            // Create stat periods
            m_samplesPerSecondPeriod = new int[PERIOD_COUNT];
            m_kilobitsPerSecondPeriod = new int[PERIOD_COUNT];
            m_smoothnessPeriod = new float[PERIOD_COUNT];
            m_periodIndexCount = 0;

            return statTimer;
        }

        // Detaches from events, stops, and disposes of the dump timer.
        // Also resets the statistics that were gathered since the creation
        // of the stat timer. Timers created through the CreateStatTimer
        // method should be released through this method.
        private void ReleaseStatTimer(Timer statTimer)
        {
            if (statTimer is not null)
            {
                statTimer.Elapsed -= StatTimer_Elapsed;
                statTimer.Stop();
                statTimer.Dispose();
            }

            // Reset statistics
            m_measurementCount = 0L;
            m_lastMeasurementCount = 0L;
            m_lastTotalBytesReceived = 0L;
            m_lastStatCalcTime = 0L;

            StatsUpdated?.Invoke(this, new EventArgs<int, int, float, double>(0, 0, 0.0F, double.NaN));
        }

        // Handles the timeout timer's Elapsed event.
        private void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DisconnectFromStreamSource();
            OnStateChanged(PlaybackState.TimedOut);
        }

        // Handles the dump timer's Tick event.
        private void DumpTimer_Tick(object sender, EventArgs e)
        {
            byte[] buffer = new byte[65536];
            List<IMeasurement[]> dumpMeasurements = new List<IMeasurement[]>();
            IMeasurement dequeuedMeasurement;
            int count = 0;

            // Remove all the measurements from the buffer and place them in the list of samples.
            while (m_buffer.Count > m_numChannels)
            {
                IMeasurement[] sample = new IMeasurement[m_numChannels];

                for (int i = 0; i < m_numChannels; i++)
                {
                    Guid id;
                    int index;

                    m_buffer.TryDequeue(out dequeuedMeasurement);
                    id = dequeuedMeasurement.ID;
                    index = m_channelIndexes[id];
                    sample[index] = dequeuedMeasurement;
                }

                dumpMeasurements.Add(sample);
            }

            foreach (IMeasurement[] sample in dumpMeasurements)
            {
                // Put the sample in the buffer.
                for (int i = 0; i < m_numChannels; i++)
                {
                    byte[] channelValue;

                    // Assuming 16-bit integer samples for WAV files
                    if (sample[i] is not null)
                        channelValue = LittleEndian.GetBytes((short)sample[i].Value);
                    else
                        channelValue = new byte[2];

                    Buffer.BlockCopy(channelValue, 0, buffer, count, 2);
                    count += 2;
                }

                // If the buffer is full, send it to the
                // sound card and start a new buffer.
                if (count + (m_numChannels * 2) > buffer.Length)
                {
                    m_waveProvider.AddSamples(buffer, 0, count);
                    m_waveFileWriter?.Write(buffer, 0, count);
                    count = 0;
                }

                // Notify the user interface of new samples.
                if (OnSample is not null)
                {
                    const float volume = 0.000035F;
                    float left = (sample[0] is not null) ? (float)sample[0].Value : 0.0F;
                    float right = m_numChannels > 1 ? ((sample[1] is not null) ? (float)sample[1].Value : 0.0F) : left;
                    OnSample(this, new SampleEventArgs(left * volume, right * volume));
                }
            }

            // Send remaining samples to the sound card.
            if (count > 0)
            {
                m_waveProvider.AddSamples(buffer, 0, count);
                m_waveFileWriter?.Write(buffer, 0, count);
            }

            m_waveFileWriter?.Flush();

            // If the buffer was empty, we're missing a full quarter second of data!
            // Go back to buffering, stop the dump timer, and start the timeout timer.
            //if (dumpMeasurements.Count == 0 && m_dumpTimer is not null)
            //{
            //    OnStateChanged(PlaybackState.Buffering);
            //    m_dumpTimer.Stop();
            //    m_timeoutTimer.Start();
            //}
        }

        // Handles the stat timer's Elapsed event. Calculates the statistics since the last
        // time this was called and sends update notifications to the user interface.
        private void StatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (m_lastStatCalcTime != 0)
            {
                long now = DateTime.UtcNow.Ticks;
                long measurementCount = Interlocked.Read(ref m_measurementCount);
                double totalTime = (now - m_startTime) / (double)Ticks.PerSecond;
                double instantaneousTime = (now - m_lastStatCalcTime) / (double)Ticks.PerSecond;
                long instMeasurementCount = measurementCount - m_lastMeasurementCount;
                long instTotalBytesReceived = m_dataSubscriber.TotalBytesReceived - m_lastTotalBytesReceived;
                long instExpectedCount = (long)(instantaneousTime * m_sampleRate) * 2L;
                long expectedCount = (long)(totalTime * m_sampleRate) * 2L;
                int periodIndex = m_periodIndexCount % PERIOD_COUNT;

                m_samplesPerSecondPeriod[periodIndex] = (int)(instMeasurementCount / instantaneousTime);
                m_kilobitsPerSecondPeriod[periodIndex] = (int)(instTotalBytesReceived * 8 / instantaneousTime) / 1024;
                m_smoothnessPeriod[periodIndex] = instMeasurementCount / (float)instExpectedCount;
                m_lostSamples = 1.0D - measurementCount / (double)expectedCount;
                m_periodIndexCount++;
            }

            if (StatsUpdated is not null && m_periodIndexCount > 0)
            {
                int samplesPerSecond = (int)m_samplesPerSecondPeriod.Take(m_periodIndexCount).Average();
                int kilobitsPerSecond = (int)m_kilobitsPerSecondPeriod.Take(m_periodIndexCount).Average();
                float smoothness = m_smoothnessPeriod.Take(m_periodIndexCount).Average();
                double lostSamples = (m_periodIndexCount <= 5) ? double.NaN : m_lostSamples;

                StatsUpdated(this, new EventArgs<int, int, float, double>(samplesPerSecond, kilobitsPerSecond, smoothness, lostSamples));
            }

            m_lastMeasurementCount = Interlocked.Read(ref m_measurementCount);
            m_lastTotalBytesReceived = m_dataSubscriber.TotalBytesReceived;
            m_lastStatCalcTime = DateTime.UtcNow.Ticks;
        }

        // ------ Wave Player Methods ------

        // Creates the wave player. The wave provider is required to initialize the wave player.
        // Wave players created through this method should be released by the ReleaseWavePlayer method.
        private IWavePlayer CreateWavePlayer(IWaveProvider waveProvider)
        {
            IWavePlayer player = new WaveOut(WaveCallbackInfo.FunctionCallback());
            player.Init(waveProvider);
            return player;
        }

        // Stops and disposes of the wave player. Wave players created through
        // the CreateWavePlayer method should be released by this method.
        private void ReleaseWavePlayer(IWavePlayer player)
        {
            if (player is not null)
            {
                player.Stop();
                player.Dispose();
            }
        }
    }
}
