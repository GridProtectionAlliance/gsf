//******************************************************************************************************
//  SubscribeMeasurementUserControl.xaml.cs - Gbtc
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
//  05/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GSF.TimeSeries.Transport.UI.ViewModels;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscribeMeasurementUserControl.xaml
    /// </summary>
    public partial class SubscribeMeasurementUserControl : UserControl
    {
        #region [ Members ]

        private readonly SubscribeMeasurements m_dataContext;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SubscribeMeasurementUserControl"/> class.
        /// </summary>
        public SubscribeMeasurementUserControl()
        {
            InitializeComponent();
            m_dataContext = new SubscribeMeasurements(1);
            m_dataContext.PropertyChanged += DataContext_PropertyChanged;
            StackPanelSubscribeMeasurements.DataContext = m_dataContext;
            this.Unloaded += SubscribeMeasurementUserControl_Unloaded;
        }

        #endregion

        #region [ Methods ]

        private void SubscribedMeasurementsPager_CurrentPageChanged(object sender, EventArgs e)
        {
            m_dataContext.CurrentSubscribedMeasurementsPage = SubscribedMeasurementsPager.CurrentPage;
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentItem" || e.PropertyName == "CurrentDevice")
                UpdateAvailableFilterExpression();
        }

        private void DisplayInternalCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateAvailableFilterExpression();
        }

        private void AddSubscribedMeasurementsButton_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext.AddSubscribedMeasurements(AvailableMeasurementsPager.SelectedMeasurements);
            AvailableMeasurementsPager.ClearSelections();
            SubscribedMeasurementsPager.ReloadDataGrid();
        }

        private void RemoveSubscribedMeasurementsButton_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext.RemoveSubscribedMeasurements(SubscribedMeasurementsPager.SelectedMeasurements);
            SubscribedMeasurementsPager.ReloadDataGrid();
            SubscribedMeasurementsPager.ClearSelections();
        }

        private void SubscribeMeasurementUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            m_dataContext.Unload();
        }

        private void UpdateAvailableFilterExpression()
        {
            bool displayInternal = DisplayInternalCheckBox.IsChecked ?? false;
            int deviceID = m_dataContext.CurrentDevice.Key;

            if (!displayInternal && deviceID > 0)
                AvailableMeasurementsPager.FilterExpression = string.Format("Internal = 0 AND DeviceID = {0}", deviceID);
            else if (!displayInternal)
                AvailableMeasurementsPager.FilterExpression = "Internal = 0";
            else if (deviceID > 0)
                AvailableMeasurementsPager.FilterExpression = string.Format("DeviceID = {0}", deviceID);
            else
                AvailableMeasurementsPager.FilterExpression = string.Empty;

            AvailableMeasurementsPager.ReloadDataGrid();
        }

        #endregion
    }
}
