//******************************************************************************************************
//  StatViewModel.cs - Gbtc
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
//      Modified Header.
//
//******************************************************************************************************

using System.ComponentModel;
using GSF;

namespace WavSubscriptionDemo
{
    /// <summary>
    /// View-model for the <see cref="StatControl"/>.
    /// </summary>
    public class StatViewModel : INotifyPropertyChanged
    {
        private int m_samplesPerSecond;
        private int m_kilobitsPerSecond;
        private string m_smoothness;
        private string m_fractionSamplesLost;

        /// <summary>
        /// Occurs when a property of the view-model changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new instance of the <see cref="StatViewModel"/> class.
        /// </summary>
        /// <param name="audioGraph">The audio graph which notifies of stat changes.</param>
        public StatViewModel(AudioGraph audioGraph)
        {
            audioGraph.StatsUpdated += AudioGraph_StatsUpdated;
            Smoothness = "0.0000";
            FractionSamplesLost = ". . .";
        }

        /// <summary>
        /// Gets or sets the samples per second received from the stream source.
        /// </summary>
        public int SamplesPerSecond
        {
            get => m_samplesPerSecond;
            set
            {
                m_samplesPerSecond = value;
                OnPropertyChanged("SamplesPerSecond");
            }
        }

        /// <summary>
        /// Gets or sets the kilobits per second received from the stream source.
        /// </summary>
        public int KilobitsPerSecond
        {
            get => m_kilobitsPerSecond;
            set
            {
                m_kilobitsPerSecond = value;
                OnPropertyChanged("KilobitsPerSecond");
            }
        }

        /// <summary>
        /// Gets or sets the smoothness calculated by the playback.
        /// </summary>
        public string Smoothness
        {
            get => m_smoothness;
            set
            {
                m_smoothness = value;
                OnPropertyChanged("Smoothness");
            }
        }

        /// <summary>
        /// Gets or sets the fraction of samples lost in transit.
        /// </summary>
        public string FractionSamplesLost
        {
            get => m_fractionSamplesLost;
            set
            {
                m_fractionSamplesLost = value;
                OnPropertyChanged("FractionSamplesLost");
            }
        }

        // Handles the audio graph's StatsUpdated event. Updates the properties
        // in this view-model so that they can be displayed to the user.
        private void AudioGraph_StatsUpdated(object sender, EventArgs<int, int, float, double> e)
        {
            double lostSamples = e.Argument4;

            SamplesPerSecond = e.Argument1;
            KilobitsPerSecond = e.Argument2;
            Smoothness = string.Format("{0:0.0000}", e.Argument3);

            if (double.IsNaN(lostSamples))
                FractionSamplesLost = ". . .";
            else if (lostSamples <= 0.0)
                FractionSamplesLost = "0.0000";
            else
                FractionSamplesLost = string.Format("{0:0.0000}", lostSamples);
        }

        // Triggers the PropertyChanged event.
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
