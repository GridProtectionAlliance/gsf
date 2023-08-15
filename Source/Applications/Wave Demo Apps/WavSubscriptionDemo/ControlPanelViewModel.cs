//******************************************************************************************************
//  ControlPanelViewModel.cs - Gbtc
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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GSF;
using Microsoft.Win32;

namespace WavSubscriptionDemo
{
    internal class ControlPanelViewModel : INotifyPropertyChanged, IDisposable
    {
        private Dictionary<string, string> m_songList;
        private string m_songName;
        private string m_currentSong;
        private bool m_isPlaying;
        private bool m_isConnected;
        private bool m_showReplayControls;

        private int m_captureSeconds;
        private readonly AudioGraph m_audioGraph;
        private readonly IWaveFormRenderer m_waveFormRenderer;

        public ControlPanelViewModel(IWaveFormRenderer waveFormRenderer)
        {
            m_waveFormRenderer = waveFormRenderer;
            m_audioGraph = new AudioGraph();
            m_audioGraph.MaximumCalculated += audioGraph_MaximumCalculated;
            m_audioGraph.GotSongList += audioGraph_GotSongList;
            m_audioGraph.PlaybackStateChanged += audioGraph_PlaybackStateChanged;
            m_captureSeconds = 10;
            NotificationsPerSecond = 100;
            ConnectionUri = "localhost:6175";
            EnableCompression = false;

            PlayStreamCommand = new RelayCommand(
                        PlayStream,
                        () => true);

            CaptureCommand = new RelayCommand(
                        Capture,
                        () => true);

            PlayCapturedAudioCommand = new RelayCommand(
                        PlayCapturedAudio,
                        HasCapturedAudio);

            SaveCapturedAudioCommand = new RelayCommand(
                        SaveCapturedAudio,
                        HasCapturedAudio);

            StopCommand = new RelayCommand(
                        Stop,
                        () => true);
        }

        public AudioGraph AudioGraph => m_audioGraph;

        public string ConnectionUri
        {
            get => m_audioGraph.ConnectionUri;
            set
            {
                m_audioGraph.ConnectionUri = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectionUri"));
            }
        }

        public Dictionary<string, string> SongList
        {
            get => m_songList;
            set
            {
                m_songList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongList"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongNames"));
            }
        }

        public string[] SongNames => 
            m_songList?.Select(kvp => kvp.Key).ToArray() ?? Array.Empty<string>();

        public string SongName
        {
            get => m_songName;
            set
            {
                m_songName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongName"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongDescription"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PlayDescription"));

                if (m_isPlaying)
                    PlayStream();
            }
        }

        public string SongDescription => 
            m_songList is null ? string.Empty : 
            m_songList.TryGetValue(m_songName, out string description) ? description : string.Empty;

        public string PlayDescription => $"Play {SongDescription}";

        public bool EnableCompression
        {
            get => m_audioGraph.EnableCompression;
            set
            {
                if (m_audioGraph.EnableCompression == value)
                    return;

                m_audioGraph.EnableCompression = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableCompression"));
            }
        }

        public bool EnableEncryption
        {
            get => m_audioGraph.EnableEncryption;
            set
            {
                if (m_audioGraph.EnableEncryption == value)
                    return;

                m_audioGraph.EnableEncryption = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableEncryption"));
            }
        }

        public bool IPv6Enabled
        {
            get => m_audioGraph.IPv6Enabled;
            set
            {
                if (m_audioGraph.IPv6Enabled == value)
                    return;

                m_audioGraph.IPv6Enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IPv6Enabled"));
            }
        }

        public bool UseZeroMQChannel
        {
            get => m_audioGraph.UseZeroMQChannel;
            set
            {
                if (m_audioGraph.UseZeroMQChannel == value)
                    return;

                m_audioGraph.UseZeroMQChannel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UseZeroMQChannel"));
            }
        }

        public bool ShowReplayControls
        {
            get => m_showReplayControls;
            set
            {
                m_showReplayControls = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowReplayControls"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReplayControlsVisibility"));
            }
        }

        public Visibility ReplayControlsVisibility => 
            m_showReplayControls ? Visibility.Visible : Visibility.Collapsed;

        public bool ReplayEnabled
        {
            get => m_audioGraph.ReplayEnabled;
            set
            {
                if (m_audioGraph.ReplayEnabled == value)
                    return;

                m_audioGraph.ReplayEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReplayEnabled"));
            }
        }

        public string ReplayStartTime
        {
            get => m_audioGraph.ReplayStartTime;
            set
            {
                m_audioGraph.ReplayStartTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReplayStartTime"));
            }
        }

        public string ReplayStopTime
        {
            get => m_audioGraph.ReplayStopTime;
            set
            {
                m_audioGraph.ReplayStopTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReplayStopTime"));
            }
        }

        public bool IsConnected => m_isConnected;

        public bool IsPlaying => m_isPlaying;

        private void audioGraph_MaximumCalculated(object sender, MaxSampleEventArgs e) => 
            m_waveFormRenderer.AddValue(e.MaxSample, e.MinSample);

        private void audioGraph_CaptureComplete(object sender, EventArgs e) => 
            CommandManager.InvalidateRequerySuggested();

        private void audioGraph_GotSongList(object sender, EventArgs<Dictionary<string, string>> e) => 
            SongList = e.Argument;

        private void audioGraph_PlaybackStateChanged(object sender, EventArgs<PlaybackState, string> e)
        {
            m_isPlaying = (e.Argument1 == PlaybackState.Playing);
            m_isConnected = (e.Argument1 != PlaybackState.Disconnected && e.Argument1 != PlaybackState.TimedOut && e.Argument1 != PlaybackState.Disposed);

            switch (e.Argument1)
            {
                case PlaybackState.Disconnected:
                case PlaybackState.TimedOut:
                case PlaybackState.Stopped:
                case PlaybackState.Disposed:
                    Application.Current.Dispatcher.Invoke(m_waveFormRenderer.Reset);
                    break;
            }
        }

        public ICommand PlayStreamCommand { get; }

        public ICommand CaptureCommand { get; }

        public ICommand PlayCapturedAudioCommand { get; }

        public ICommand SaveCapturedAudioCommand { get; }

        public ICommand StopCommand { get; }

        public void ConnectToStreamSource() => 
            m_audioGraph.ConnectToStreamSource();

        private void PlayStream()
        {
            if (m_isPlaying && m_songName == m_currentSong)
                return;

            if (Application.Current?.MainWindow is not null)
                Application.Current.MainWindow.Title = $"{Application.Current.MainWindow.Tag} - {m_songName}";

            m_currentSong = m_songName;
            m_waveFormRenderer.Reset();
            m_audioGraph.PlayStream(m_songName);
            m_waveFormRenderer.SampleRate = m_audioGraph.PlaybackSampleRate;
        }

        private void Capture()
        {
            try
            {
                m_audioGraph.StartCapture(CaptureSeconds);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void PlayCapturedAudio() => 
            m_audioGraph.PlayCapturedAudio();

        private bool HasCapturedAudio() => 
            m_audioGraph.HasCapturedAudio;

        private void SaveCapturedAudio()
        {
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.DefaultExt = ".wav";
            saveFileDialog.Filter = "WAV Audio File (*.wav)|*.wav";
            bool? result = saveFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
                m_audioGraph.SaveRecordedAudio(saveFileDialog.FileName);
        }

        private void Stop()
        {
            m_audioGraph.Stop();
            m_waveFormRenderer.Reset();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent(string propertyName) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int CaptureSeconds
        {
            get => m_captureSeconds;
            set
            {
                if (m_captureSeconds == value)
                    return;

                m_captureSeconds = value;
                RaisePropertyChangedEvent("CaptureSeconds");
            }
        }

        public int NotificationsPerSecond
        {
            get => m_audioGraph.NotificationsPerSecond;
            set
            {
                if (NotificationsPerSecond == value)
                    return;

                m_audioGraph.NotificationsPerSecond = value;
                RaisePropertyChangedEvent("NotificationsPerSecond");
            }
        }

        public void Dispose() => 
            m_audioGraph.Dispose();
    }
}
