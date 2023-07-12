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
        private AudioPlayback playback;
        private readonly SampleAggregator aggregator;

        public event EventHandler<MaxSampleEventArgs> MaximumCalculated
        {
            add => aggregator.MaximumCalculated += value;
            remove => aggregator.MaximumCalculated -= value;
        }

        public event EventHandler<EventArgs<List<string>>> GotSongList
        {
            add => playback.GotSongList += value;
            remove => playback.GotSongList -= value;
        }

        public event EventHandler<EventArgs<int, int, float, double>> StatsUpdated
        {
            add => playback.StatsUpdated += value;
            remove => playback.StatsUpdated -= value;
        }

        public event EventHandler<EventArgs<PlaybackState, string>> PlaybackStateChanged
        {
            add => playback.StateChanged += value;
            remove => playback.StateChanged -= value;
        }

        public AudioGraph()
        {
            playback = new AudioPlayback();
            playback.OnSample += OnSample;
            aggregator = new SampleAggregator();
            aggregator.NotificationCount = 100;
        }

        void OnSample(object sender, SampleEventArgs e)
        {
            aggregator.Add(e.Left);
        }

        public int NotificationsPerSecond
        {
            get => aggregator.NotificationCount;
            set => aggregator.NotificationCount = value;
        }

        public string ConnectionUri
        {
            get => playback.ConnectionUri;
            set => playback.ConnectionUri = value;
        }

        public bool EnableCompression
        {
            get => playback.EnableCompression;
            set => playback.EnableCompression = value;
        }

        public bool EnableEncryption
        {
            get => playback.EnableEncryption;
            set => playback.EnableEncryption = value;
        }

        public bool IPv6Enabled
        {
            get => playback.IPv6Enabled;
            set => playback.IPv6Enabled = value;
        }

        public bool UseZeroMQChannel
        {
            get => playback.UseZeroMQChannel;
            set => playback.UseZeroMQChannel = value;
        }

        public bool ReplayEnabled
        {
            get => playback.ReplayEnabled;
            set => playback.ReplayEnabled = value;
        }

        public string ReplayStartTime
        {
            get => playback.ReplayStartTime;
            set => playback.ReplayStartTime = value;
        }

        public string ReplayStopTime
        {
            get => playback.ReplayStopTime;
            set => playback.ReplayStopTime = value;
        }

        public int PlaybackSampleRate => playback.SampleRate;

        public bool HasCapturedAudio
        {
            get;
            private set;
        }

        public void ConnectToStreamSource()
        {
            playback.DisconnectFromStreamSource();
            playback.ConnectToStreamSource();
        }

        public void PlayStream(string songName)
        {
            CancelCurrentOperation();
            aggregator.NotificationCount = 200;
            playback.Play(songName);
        }

        private void CancelCurrentOperation()
        {
            playback.Stop();
        }

        public void Stop()
        {
            CancelCurrentOperation();
        }

        public void SaveRecordedAudio(string fileName)
        {
            playback.StartRecording(fileName);
        }

        public void PlayCapturedAudio()
        {
            throw new NotImplementedException();
        }

        public void StartCapture(int captureSeconds)
        {
            aggregator.NotificationCount = 200;
        }

        public void Dispose()
        {
            if (playback is not null)
            {
                playback.Dispose();
                playback = null;
            }
        }
    }
}
