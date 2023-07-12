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
using System.Windows;
using System.Windows.Input;
using GSF;
using Microsoft.Win32;

namespace WavSubscriptionDemo
{
    class ControlPanelViewModel : INotifyPropertyChanged, IDisposable
    {
        List<string> songList;
        string songName;
        string currentSong;
        bool isPlaying;
        bool isConnected;

        int captureSeconds;
        readonly AudioGraph audioGraph;
        readonly IWaveFormRenderer waveFormRenderer;

        public ControlPanelViewModel(IWaveFormRenderer waveFormRenderer)
        {
            this.waveFormRenderer = waveFormRenderer;
            audioGraph = new AudioGraph();
            audioGraph.MaximumCalculated += audioGraph_MaximumCalculated;
            audioGraph.GotSongList += audioGraph_GotSongList;
            audioGraph.PlaybackStateChanged += audioGraph_PlaybackStateChanged;
            captureSeconds = 10;
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

        public AudioGraph AudioGraph => audioGraph;

        public string ConnectionUri
        {
            get => audioGraph.ConnectionUri;
            set
            {
                audioGraph.ConnectionUri = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectionUri"));
            }
        }

        public List<string> SongList
        {
            get => songList;
            set
            {
                songList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongList"));
            }
        }

        public string SongName
        {
            get => songName;
            set
            {
                songName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SongName"));

                if (isPlaying)
                    PlayStream();
            }
        }

        public bool EnableCompression
        {
            get => audioGraph.EnableCompression;
            set
            {
                if (audioGraph.EnableCompression == value)
                    return;

                audioGraph.EnableCompression = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableCompression"));
            }
        }

        public bool EnableEncryption
        {
            get => audioGraph.EnableEncryption;
            set
            {
                if (audioGraph.EnableEncryption == value)
                    return;

                audioGraph.EnableEncryption = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableEncryption"));
            }
        }

        public bool IPv6Enabled
        {
            get => audioGraph.IPv6Enabled;
            set
            {
                if (audioGraph.IPv6Enabled == value)
                    return;

                audioGraph.IPv6Enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IPv6Enabled"));
            }
        }

        public bool UseZeroMQChannel
        {
            get => audioGraph.UseZeroMQChannel;
            set
            {
                if (audioGraph.UseZeroMQChannel == value)
                    return;

                audioGraph.UseZeroMQChannel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UseZeroMQChannel"));
            }
        }

        public bool ReplayEnabled
        {
            get => audioGraph.ReplayEnabled;
            set
            {
                if (audioGraph.ReplayEnabled == value)
                    return;

                audioGraph.ReplayEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReplayEnabled"));
            }
        }

        public string ReplayStartTime
        {
            get => audioGraph.ReplayStartTime;
            set
            {
                audioGraph.ReplayStartTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReplayStartTime"));
            }
        }

        public string ReplayStopTime
        {
            get => audioGraph.ReplayStopTime;
            set
            {
                audioGraph.ReplayStopTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReplayStopTime"));
            }
        }

        public bool IsConnected => isConnected;

        public bool IsPlaying => isPlaying;

        void audioGraph_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            waveFormRenderer.AddValue(e.MaxSample, e.MinSample);
        }

        void audioGraph_CaptureComplete(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        void audioGraph_GotSongList(object sender, EventArgs<List<string>> e)
        {
            SongList = e.Argument;
        }

        void audioGraph_PlaybackStateChanged(object sender, EventArgs<PlaybackState, string> e)
        {
            isPlaying = (e.Argument1 == PlaybackState.Playing);
            isConnected = (e.Argument1 != PlaybackState.Disconnected && e.Argument1 != PlaybackState.TimedOut && e.Argument1 != PlaybackState.Disposed);

            switch (e.Argument1)
            {
                case PlaybackState.Disconnected:
                case PlaybackState.TimedOut:
                case PlaybackState.Stopped:
                case PlaybackState.Disposed:
                    Application.Current.Dispatcher.Invoke(waveFormRenderer.Reset);
                    break;
            }
        }

        public ICommand PlayStreamCommand
        {
            get;
            private set;
        }
        public ICommand CaptureCommand
        {
            get;
            private set;
        }
        public ICommand PlayCapturedAudioCommand
        {
            get;
            private set;
        }
        public ICommand SaveCapturedAudioCommand
        {
            get;
            private set;
        }
        public ICommand StopCommand
        {
            get;
            private set;
        }

        public void ConnectToStreamSource()
        {
            audioGraph.ConnectToStreamSource();
        }

        private void PlayStream()
        {
            if (!isPlaying || songName != currentSong)
            {
                Application.Current.MainWindow.Title = "Wave Subscriber - " + songName;
                currentSong = songName;
                waveFormRenderer.Reset();
                audioGraph.PlayStream(songName);
                waveFormRenderer.SampleRate = audioGraph.PlaybackSampleRate;
            }
        }

        private void Capture()
        {
            try
            {
                audioGraph.StartCapture(CaptureSeconds);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void PlayCapturedAudio()
        {
            audioGraph.PlayCapturedAudio();
        }

        private bool HasCapturedAudio()
        {
            return audioGraph.HasCapturedAudio;
        }

        private void SaveCapturedAudio()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".wav";
            saveFileDialog.Filter = "WAV Audio File (*.wav)|*.wav";
            bool? result = saveFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
                audioGraph.SaveRecordedAudio(saveFileDialog.FileName);
        }

        private void Stop()
        {
            audioGraph.Stop();
            waveFormRenderer.Reset();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public int CaptureSeconds
        {
            get => captureSeconds;
            set
            {
                if (captureSeconds != value)
                {
                    captureSeconds = value;
                    RaisePropertyChangedEvent("CaptureSeconds");
                }
            }
        }

        public int NotificationsPerSecond
        {
            get => audioGraph.NotificationsPerSecond;
            set
            {
                if (NotificationsPerSecond != value)
                {
                    audioGraph.NotificationsPerSecond = value;
                    RaisePropertyChangedEvent("NotificationsPerSecond");
                }
            }
        }

        public void Dispose()
        {
            audioGraph.Dispose();
        }
    }
}
