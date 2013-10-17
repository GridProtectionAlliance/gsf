//******************************************************************************************************
//  ControlPanelViewModel.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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

namespace NAudioWpfDemo
{
    class ControlPanelViewModel : INotifyPropertyChanged, IDisposable
    {
        List<string> songList;
        string songName;
        string currentSong;
        bool isPlaying;

        int captureSeconds;
        readonly AudioGraph audioGraph;
        readonly IWaveFormRenderer waveFormRenderer;

        public ControlPanelViewModel(IWaveFormRenderer waveFormRenderer)
        {
            this.waveFormRenderer = waveFormRenderer;
            this.audioGraph = new AudioGraph();
            audioGraph.MaximumCalculated += audioGraph_MaximumCalculated;
            audioGraph.GotSongList += audioGraph_GotSongList;
            audioGraph.PlaybackStateChanged += audioGraph_PlaybackStateChanged;
            this.captureSeconds = 10;
            this.NotificationsPerSecond = 100;
            this.ConnectionUri = "localhost:6170?udp=9500";
            this.EnableCompression = false;

            PlayStreamCommand = new RelayCommand(
                        () => this.PlayStream(),
                        () => true);
            CaptureCommand = new RelayCommand(
                        () => this.Capture(),
                        () => true);
            PlayCapturedAudioCommand = new RelayCommand(
                        () => this.PlayCapturedAudio(),
                        () => this.HasCapturedAudio());
            SaveCapturedAudioCommand = new RelayCommand(
                        () => this.SaveCapturedAudio(),
                        () => this.HasCapturedAudio());
            StopCommand = new RelayCommand(
                        () => this.Stop(),
                        () => true);
        }

        public AudioGraph AudioGraph
        {
            get { return audioGraph; }
        }

        public string ConnectionUri
        {
            get { return audioGraph.ConnectionUri; }
            set
            {
                audioGraph.ConnectionUri = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ConnectionUri"));
            }
        }

        public List<string> SongList
        {
            get { return songList; }
            set
            {
                songList = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SongList"));
            }
        }

        public string SongName
        {
            get { return songName; }
            set
            {
                songName = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SongName"));

                if (isPlaying)
                    PlayStream();
            }
        }

        public bool EnableCompression
        {
            get { return audioGraph.EnableCompression; }
            set
            {
                if (audioGraph.EnableCompression != value)
                {
                    audioGraph.EnableCompression = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("EnableCompression"));
                }
            }
        }

        public bool EnableEncryption
        {
            get { return audioGraph.EnableEncryption; }
            set
            {
                if (audioGraph.EnableEncryption != value)
                {
                    audioGraph.EnableEncryption = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("EnableEncryption"));
                }
            }
        }

        public bool IPv6Enabled
        {
            get { return audioGraph.IPv6Enabled; }
            set
            {
                if (audioGraph.IPv6Enabled != value)
                {
                    audioGraph.IPv6Enabled = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IPv6Enabled"));
                }
            }
        }

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

        public ICommand PlayStreamCommand { get; private set; }
        public ICommand CaptureCommand { get; private set; }
        public ICommand PlayCapturedAudioCommand { get; private set; }
        public ICommand SaveCapturedAudioCommand { get; private set; }
        public ICommand StopCommand { get; private set; }

        public void ConnectToStreamSource()
        {
            audioGraph.ConnectToStreamSource();
        }

        private void PlayStream()
        {
            if (!isPlaying || songName != currentSong)
            {
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
            saveFileDialog.Filter = "WAVE File (*.wav)|*.wav";
            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                audioGraph.SaveRecordedAudio(saveFileDialog.FileName);
            }
        }

        private void Stop()
        {
            audioGraph.Stop();
            waveFormRenderer.Reset();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public int CaptureSeconds
        {
            get
            {
                return captureSeconds;
            }
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
            get
            {
                return audioGraph.NotificationsPerSecond;
            }
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
