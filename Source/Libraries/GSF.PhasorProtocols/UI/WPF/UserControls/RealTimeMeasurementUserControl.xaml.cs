//******************************************************************************************************
//  RealTimeMeasurementUserControl.xaml.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  09/26/2011 - Mehulbhai P Thakkar
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
    /// Interaction logic for RealTimeMeasurementUserControl.xaml
    /// </summary>
    public partial class RealTimeMeasurementUserControl
    {
        #region [ Members ]

        // Fields
        private int m_measurementsDataRefreshInterval = 5;
        private RealTimeStreams m_dataContext;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeMeasurementUserControl"/>.
        /// </summary>
        public RealTimeMeasurementUserControl()
        {
            InitializeComponent();
            this.Loaded += RealTimeMeasurementUserControl_Loaded;
            this.Unloaded += RealTimeMeasurementUserControl_Unloaded;
        }

        #endregion

        #region [ Methods ]

        private RealTimeStreams CreateDataContext()
        {
            RealTimeStreams dataContext = new(1, m_measurementsDataRefreshInterval);
            List<RealTimeStream> realTimeStreams = dataContext.ItemsSource.ToList();
            List<RealTimeDevice> realTimeDevices = realTimeStreams.SelectMany(stream => stream.DeviceList).ToList();
            List<RealTimeMeasurement> realTimeMeasurements = realTimeDevices.SelectMany(device => device.MeasurementList).ToList();

            if (realTimeMeasurements.Count < 100)
                realTimeDevices.ForEach(device => device.Expanded = true);

            if (realTimeDevices.Count < 100)
                realTimeStreams.ForEach(stream => stream.Expanded = true);

            return dataContext;
        }

        private void RealTimeMeasurementUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            m_dataContext.RestartConnectionCycle = false;
            m_dataContext.UnsubscribeUnsynchronizedData();
        }

        private void RealTimeMeasurementUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("RealtimeMeasurementsDataRefreshInterval").ToString(), out m_measurementsDataRefreshInterval);

            if (m_measurementsDataRefreshInterval == 0)
            {
                m_measurementsDataRefreshInterval = 5;
                IsolatedStorageManager.InitializeStorageForRealTimeMeasurements(true);
            }

            TextBlockMeasurementRefreshInterval.Text = m_measurementsDataRefreshInterval + " sec";
            TextBoxRefreshInterval.Text = m_measurementsDataRefreshInterval.ToString();
            m_dataContext = CreateDataContext();
            this.DataContext = m_dataContext;
            this.KeyUp += RealTimeMeasurementUserControl_KeyUp;
        }

        private void RealTimeMeasurementUserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && PopupSettings.IsOpen)
                PopupSettings.IsOpen = false;
        }

        private void ButtonOutOfSync_Click(object sender, RoutedEventArgs e)
        {
            Device device = Device.GetDevice(null, "WHERE Acronym = '" + ((Button)sender).Tag + "'");
            CommonFunctions.LoadUserControl("Input Device Configuration Wizard", typeof(InputWizardUserControl), device);
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Device device = Device.GetDevice(null, "WHERE Acronym = '" + ((Button)sender).Tag + "'");
            CommonFunctions.LoadUserControl("Manage Device Configuration", typeof(DeviceUserControl), device);
        }

        private void ButtonDisplaySettings_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = true;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBoxRefreshInterval.Text, out m_measurementsDataRefreshInterval))
            {
                m_dataContext.RestartConnectionCycle = false;
                m_dataContext.UnsubscribeUnsynchronizedData();
                IsolatedStorageManager.WriteToIsolatedStorage("RealtimeMeasurementsDataRefreshInterval", m_measurementsDataRefreshInterval);
                //int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("RealtimeMeasurementsDataRefreshInterval").ToString(), out m_measurementsDataRefreshInterval);
                PopupSettings.IsOpen = false;
                CommonFunctions.LoadUserControl(CommonFunctions.GetHeaderText("Monitor Device Outputs"), typeof(RealTimeMeasurementUserControl));
            }
            else
            {
                MessageBox.Show("Please provide integer value.", "ERROR: Invalid Value", MessageBoxButton.OK);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = false;
        }

        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext.RestartConnectionCycle = false;
            m_dataContext.UnsubscribeUnsynchronizedData();
            IsolatedStorageManager.InitializeStorageForRealTimeMeasurements(true);
            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("RealtimeMeasurementsDataRefreshInterval").ToString(), out m_measurementsDataRefreshInterval);
            PopupSettings.IsOpen = false;
            CommonFunctions.LoadUserControl(CommonFunctions.GetHeaderText("Monitor Device Outputs"), typeof(RealTimeMeasurementUserControl));
        }

        private void ButtonStatusFlagReference_Click(object sender, RoutedEventArgs e)
        {
            ShowuserStatusDoc.IsOpen = true;
        }

        private void ButtonCancelShowuserStatusDoc_Click(object sender, RoutedEventArgs e)
        {
            ShowuserStatusDoc.IsOpen = false;
        }

        #endregion
    }
}
