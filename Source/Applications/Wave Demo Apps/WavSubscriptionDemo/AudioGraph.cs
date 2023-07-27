//******************************************************************************************************
//  AudioGraph.cs - Gbtc
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
using System.Collections.Generic;
using GSF;
using NAudio.Wave;

namespace WavSubscriptionDemo
{
    /// <summary>
    /// Audio Graph
    /// </summary>
    public class AudioGraph : IDisposable
    {
        private AudioPlayback m_playback;
        private readonly SampleAggregator m_aggregator;

        public event EventHandler<MaxSampleEventArgs> MaximumCalculated
        {
            add => m_aggregator.MaximumCalculated += value;
            remove => m_aggregator.MaximumCalculated -= value;
        }

        public event EventHandler<EventArgs<Dictionary<string, string>>> GotSongList
        {
            add => m_playback.GotSongList += value;
            remove => m_playback.GotSongList -= value;
        }

        public event EventHandler<EventArgs<(int, int, float, double, Ticks)>> StatsUpdated
        {
            add => m_playback.StatsUpdated += value;
            remove => m_playback.StatsUpdated -= value;
        }

        public event EventHandler<EventArgs<PlaybackState, string>> PlaybackStateChanged
        {
            add => m_playback.StateChanged += value;
            remove => m_playback.StateChanged -= value;
        }

        public AudioGraph()
        {
            m_playback = new AudioPlayback();
            m_playback.OnSample += OnSample;
            m_aggregator = new SampleAggregator { NotificationCount = 100 };
        }

        private void OnSample(object sender, SampleEventArgs e) => 
            m_aggregator.Add(e.Left, e.Right);

        public int NotificationsPerSecond
        {
            get => m_aggregator.NotificationCount;
            set => m_aggregator.NotificationCount = value;
        }

        public string ConnectionUri
        {
            get => m_playback.ConnectionUri;
            set => m_playback.ConnectionUri = value;
        }

        public bool EnableCompression
        {
            get => m_playback.EnableCompression;
            set => m_playback.EnableCompression = value;
        }

        public bool EnableEncryption
        {
            get => m_playback.EnableEncryption;
            set => m_playback.EnableEncryption = value;
        }

        public bool IPv6Enabled
        {
            get => m_playback.IPv6Enabled;
            set => m_playback.IPv6Enabled = value;
        }

        public bool UseZeroMQChannel
        {
            get => m_playback.UseZeroMQChannel;
            set => m_playback.UseZeroMQChannel = value;
        }

        public bool ReplayEnabled
        {
            get => m_playback.ReplayEnabled;
            set => m_playback.ReplayEnabled = value;
        }

        public string ReplayStartTime
        {
            get => m_playback.ReplayStartTime;
            set => m_playback.ReplayStartTime = value;
        }

        public string ReplayStopTime
        {
            get => m_playback.ReplayStopTime;
            set => m_playback.ReplayStopTime = value;
        }

        public int PlaybackSampleRate => m_playback.SampleRate;

        public bool HasCapturedAudio { get; private set; }

        public void ConnectToStreamSource()
        {
            m_playback.DisconnectFromStreamSource();
            m_playback.ConnectToStreamSource();
        }

        public void PlayStream(string songName)
        {
            CancelCurrentOperation();
            m_aggregator.NotificationCount = 200;
            m_playback.Play(songName);
        }

        private void CancelCurrentOperation() => 
            m_playback.Stop();

        public void Stop() => 
            CancelCurrentOperation();

        public void SaveRecordedAudio(string fileName) => 
            m_playback.StartRecording(fileName);

        public void PlayCapturedAudio() => 
            throw new NotImplementedException();

        public void StartCapture(int captureSeconds) => 
            m_aggregator.NotificationCount = 200;

        public void Dispose()
        {
            if (m_playback is null)
                return;

            m_playback.Dispose();
            m_playback = null;
        }
    }
}
