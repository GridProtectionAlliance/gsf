//******************************************************************************************************
//  ExportDialog.xaml.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  03/13/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Controls;
using GSF;

namespace HistorianView
{
    /// <summary>
    /// File types used for data export.
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Comma-separated values.
        /// </summary>
        CSV,
        
        /// <summary>
        /// COMTRADE (ASCII format)
        /// </summary>
        ComtradeAscii,
        
        /// <summary>
        /// COMTRADE (binary format)
        /// </summary>
        ComtradeBinary
    }

    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        #region [ Members ]

        // Fields
        private int m_frameRate;
        private double m_nominalFrequency;
        private long m_timeResolution;
        private long m_recommendedTimeResolution;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ExportDialog"/> class.
        /// </summary>
        public ExportDialog()
        {
            InitializeComponent();
            m_recommendedTimeResolution = m_timeResolution;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the file type chosen from the drop down.
        /// </summary>
        public FileType FileType
        {
            get
            {
                return (FileType)FileTypeComboBox.SelectedIndex;
            }
            set
            {
                FileTypeComboBox.SelectedIndex = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the frame rate entered into the text box.
        /// </summary>
        public int FrameRate
        {
            get
            {
                return m_frameRate;
            }
            set
            {
                FrameRateTextBox.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the nominal frequency entered into the text box.
        /// </summary>
        public double NominalFrequency
        {
            get
            {
                return m_nominalFrequency;
            }
            set
            {
                NominalFrequencyTextBox.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the time resolution entered into the text box.
        /// </summary>
        public long TimeResolution
        {
            get
            {
                return m_timeResolution;
            }
            set
            {
                TimeResolutionTextBox.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the flag that indicates whether timestamps in the export should be aligned by frame rate.
        /// </summary>
        public bool AlignTimestamps
        {
            get
            {
                return AlignTimestampsCheckBox.IsChecked == true;
            }
            set
            {
                AlignTimestampsCheckBox.IsChecked = value;
            }
        }

        #endregion

        #region [ Methods ]

        // Occurs when the user selects a different file type.
        private void FileTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                if (FileType == FileType.ComtradeAscii || FileType == FileType.ComtradeBinary)
                {
                    // COMTRADE exports are required to be time aligned
                    AlignTimestampsCheckBox.IsChecked = true;
                    AlignTimestampsCheckBox.IsEnabled = false;

                    // Nominal frequency is only applicable for COMTRADE files
                    NominalFrequencyLabel.Visibility = Visibility.Visible;
                    NominalFrequencyStackPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    AlignTimestampsCheckBox.IsEnabled = true;
                    NominalFrequencyLabel.Visibility = Visibility.Collapsed;
                    NominalFrequencyStackPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Occurs when the user enters a different frame rate.
        private void FrameRateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int frameRate;
            long recommendedTimeResolution;

            if (string.IsNullOrEmpty(FrameRateTextBox.Text))
            {
                // Allow the text box to be empty,
                // but set frame rate to an invalid
                // value in case the user clicks "Save"
                m_frameRate = 0;
            }
            else if (int.TryParse(FrameRateTextBox.Text, out frameRate))
            {
                // Set the frame rate to what the user entered
                m_frameRate = frameRate;

                // If the frame rate entered is valid, also update the recommended time resolution
                if (IsInitialized && m_frameRate > 0)
                {
                    recommendedTimeResolution = GetRecommendedTimeResolution();

                    // Only update the time resolution if it matches the recommended resolution
                    if (m_timeResolution == m_recommendedTimeResolution)
                        TimeResolutionTextBox.Text = recommendedTimeResolution.ToString();

                    m_recommendedTimeResolution = recommendedTimeResolution;
                    RecommendedTimeResolutionButton.Content = string.Format("Recommended: {0}", recommendedTimeResolution);
                }
            }
            else
            {
                // Don't allow the user to enter invalid characters
                FrameRateTextBox.Text = m_frameRate.ToString();
            }
        }

        // Occurs when the user enters a different nominal frequency.
        private void NominalFrequencyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double nominalFrequency;

            // Attempt to update the nominal frequency,
            // but don't allow the user to enter invalid text
            if (string.IsNullOrEmpty(NominalFrequencyTextBox.Text))
                m_nominalFrequency = 0;
            else if (double.TryParse(NominalFrequencyTextBox.Text, out nominalFrequency))
                m_nominalFrequency = nominalFrequency;
            else
                NominalFrequencyTextBox.Text = m_nominalFrequency.ToString();
        }

        // Occurs when the user enters a different time resolution.
        private void TimeResolutionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            long timeResolution;

            // Attempt to update the time resolution,
            // but don't allow the user to enter invalid text
            if (string.IsNullOrEmpty(TimeResolutionTextBox.Text))
                m_timeResolution = m_recommendedTimeResolution;
            else if (long.TryParse(TimeResolutionTextBox.Text, out timeResolution))
                m_timeResolution = timeResolution;
            else
                TimeResolutionTextBox.Text = m_timeResolution.ToString();
        }

        // Occurs when the user clicks the button to set time resolution to the recommended value.
        private void RecommendedTimeResolutionButton_Click(object sender, RoutedEventArgs e)
        {
            TimeResolutionTextBox.Text = m_recommendedTimeResolution.ToString();
        }

        // Occurs when the user chooses to align timestamps.
        private void AlignTimestampsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                FrameRateTextBox.IsEnabled = true;
                TimeResolutionTextBox.IsEnabled = true;
            }
        }

        // Occurs when the user chooses not to align timestamps.
        private void AlignTimestampsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                FrameRateTextBox.IsEnabled = false;
                TimeResolutionTextBox.IsEnabled = false;
            }
        }

        // Occurs when the user chooses to export the data.
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_frameRate > 0)
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Frame rate must be greater than 0 frames per second.");
                e.Handled = true;
            }
        }

        // Gets the recommended time resolution based on the currently entered frame rate.
        // The recommended time resolution is the number of ticks per frame truncated to
        // the least precise, non-zero unit of measure (in order of least to most precise -
        // seconds, milliseconds, microseconds, or ticks).
        private long GetRecommendedTimeResolution()
        {
            long ticksPerFrame = (Ticks.PerSecond / m_frameRate) * 100L;
            long divisor = 1L;

            while (ticksPerFrame / 1000L > 0L)
            {
                ticksPerFrame /= 1000L;
                divisor *= 1000L;
            }

            return (ticksPerFrame * divisor) / 100L;
        }

        #endregion
    }
}
