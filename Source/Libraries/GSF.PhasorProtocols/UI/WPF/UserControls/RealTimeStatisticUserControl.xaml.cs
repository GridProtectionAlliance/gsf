//******************************************************************************************************
//  RealTimeStatisticUserControl.xaml.cs - Gbtc
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
//  09/29/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.ViewModels;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.UserControls
{
    /// <summary>
    /// Interaction logic for RealTimeStatisticUserControl.xaml
    /// </summary>
    public partial class RealTimeStatisticUserControl : UserControl
    {
        #region [ Members ]

        // Fields
        private int m_statisticDataRefreshInterval = 5;
        private RealTimeStatistics m_dataContext;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeStatisticUserControl"/>.
        /// </summary>
        public RealTimeStatisticUserControl()
        {
            InitializeComponent();
            this.Loaded += RealTimeStatisticUserControl_Loaded;
            this.Unloaded += RealTimeStatisticUserControl_Unloaded;
        }

        #endregion

        #region [ Methods ]

        private RealTimeStatistics CreateDataContext()
        {
            RealTimeStatistics dataContext = new RealTimeStatistics(1, m_statisticDataRefreshInterval);
            List<RealTimeStatistic> realTimeStatistics = dataContext.ItemsSource.ToList();
            List<StreamStatistic> streamStatistics = realTimeStatistics.SelectMany(stream => stream.StreamStatisticList).ToList();
            List<PdcDeviceStatistic> deviceStatistics = streamStatistics.SelectMany(device => device.DeviceStatisticList).ToList();
            List<StatisticMeasurement> statisticMeasurements = deviceStatistics.SelectMany(device => device.StatisticMeasurementList).ToList();

            if (statisticMeasurements.Count < 100)
                deviceStatistics.ForEach(device => device.Expanded = true);

            if (deviceStatistics.Count < 100)
                streamStatistics.ForEach(streamStatistic => streamStatistic.Expanded = true);

            if (streamStatistics.Count < 100)
                realTimeStatistics.ForEach(realTimeStatistic => realTimeStatistic.Expanded = true);

            return dataContext;
        }

        private void RealTimeStatisticUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            m_dataContext.Stop();
        }

        private void RealTimeStatisticUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("StreamStatisticsDataRefreshInterval").ToString(), out m_statisticDataRefreshInterval);

            if (m_statisticDataRefreshInterval == 0)
            {
                m_statisticDataRefreshInterval = 5;
                IsolatedStorageManager.InitializeStorageForStreamStatistics(true);
            }

            TextBlockMeasurementRefreshInterval.Text = m_statisticDataRefreshInterval.ToString() + " sec";
            TextBoxRefreshInterval.Text = m_statisticDataRefreshInterval.ToString();
            m_dataContext = CreateDataContext();
            DataContext = m_dataContext;
            KeyUp += RealTimeStatisticUserControl_KeyUp;
        }

        private void RealTimeStatisticUserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && PopupSettings.IsOpen)
                PopupSettings.IsOpen = false;
        }

        private void ButtonDisplaySettings_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = true;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBoxRefreshInterval.Text, out m_statisticDataRefreshInterval))
            {
                m_dataContext.Stop();
                IsolatedStorageManager.WriteToIsolatedStorage("StreamStatisticsDataRefreshInterval", m_statisticDataRefreshInterval);
                //int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("StreamStatisticsDataRefreshInterval").ToString(), out m_statisticDataRefreshInterval);
                TextBlockMeasurementRefreshInterval.Text = m_statisticDataRefreshInterval.ToString();
                PopupSettings.IsOpen = false;
                CommonFunctions.LoadUserControl(CommonFunctions.GetHeaderText("Stream Statistics"), typeof(RealTimeStatisticUserControl));
            }
            else
            {
                MessageBox.Show("Please provide integer value.", "ERROR: Invalid Value", MessageBoxButton.OK);
            }
        }

        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext.Stop();
            IsolatedStorageManager.InitializeStorageForStreamStatistics(true);
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("StreamStatisticsDataRefreshInterval").ToString(), out m_statisticDataRefreshInterval);
            TextBlockMeasurementRefreshInterval.Text = m_statisticDataRefreshInterval.ToString();
            TextBoxRefreshInterval.Text = m_statisticDataRefreshInterval.ToString();
            PopupSettings.IsOpen = false;
            CommonFunctions.LoadUserControl(CommonFunctions.GetHeaderText("Stream Statistics"), typeof(RealTimeStatisticUserControl));
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = false;
        }

        #endregion
    }
}
