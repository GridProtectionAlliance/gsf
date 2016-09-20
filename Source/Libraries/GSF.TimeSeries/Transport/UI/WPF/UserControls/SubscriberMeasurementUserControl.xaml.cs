//******************************************************************************************************
//  SubscriberMeasurementUserControl.xaml.cs - Gbtc
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
//  05/20/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GSF.TimeSeries.Transport.UI.DataModels;
using GSF.TimeSeries.Transport.UI.ViewModels;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscriberMeasurementUserControl.xaml
    /// </summary>
    public partial class SubscriberMeasurementUserControl : UserControl
    {
        #region [ Members ]

        private readonly Subscribers m_dataContext;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SubscriberMeasurementUserControl"/>.
        /// </summary>
        public SubscriberMeasurementUserControl()
        {
            InitializeComponent();
            m_dataContext = new Subscribers(1);
            m_dataContext.PropertyChanged += DataContext_PropertyChanged;
            StackPanelManageSubscriberMeasurements.DataContext = m_dataContext;
            UpdateFilterExpressions();
        }

        #endregion

        #region [ Methods ]

        private void AvailableMeasurementsPager_CurrentPageChanged(object sender, EventArgs e)
        {
            m_dataContext.CurrentAvailableMeasurementsPage = AvailableMeasurementsPager.CurrentPage;
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentItem")
                UpdateFilterExpressions();
        }

        private void AddMeasurementsButton_Click(object sender, RoutedEventArgs e)
        {
            int count = AvailableMeasurementsPager.SelectedMeasurements.Count;

            if (count > 1000)
            {
                string message = 
                    $"You have selected {count} measurements. " +
                    $"It may take some time to complete this operation. " +
                    $"Would you like to continue?";

                if (!m_dataContext.Confirm(message, "Too many measurements"))
                    return;
            }

            if (AllowedTab.IsSelected)
            {
                m_dataContext.AddAllowedMeasurements(AvailableMeasurementsPager.SelectedMeasurements);
                AllowedMeasurementsPager.ReloadDataGrid();
            }
            else
            {
                m_dataContext.AddDeniedMeasurements(AvailableMeasurementsPager.SelectedMeasurements);
                DeniedMeasurementsPager.ReloadDataGrid();
            }

            AvailableMeasurementsPager.ClearSelections();
        }

        private void RemoveMeasurementsButton_Click(object sender, RoutedEventArgs e)
        {
            int count = AllowedTab.IsSelected
                ? AllowedMeasurementsPager.SelectedMeasurements.Count
                : DeniedMeasurementsPager.SelectedMeasurements.Count;

            if (count > 1000)
            {
                string message =
                    $"You have selected {count} measurements. " +
                    $"It may take some time to complete this operation. " +
                    $"Would you like to continue?";

                if (!m_dataContext.Confirm(message, "Too many measurements"))
                    return;
            }

            if (AllowedTab.IsSelected)
            {
                m_dataContext.RemoveAllowedMeasurements(AllowedMeasurementsPager.SelectedMeasurements);
                AllowedMeasurementsPager.ReloadDataGrid();
                AllowedMeasurementsPager.ClearSelections();
            }
            else
            {
                m_dataContext.RemoveDeniedMeasurements(DeniedMeasurementsPager.SelectedMeasurements);
                DeniedMeasurementsPager.ReloadDataGrid();
                DeniedMeasurementsPager.ClearSelections();
            }
        }

        private void AuthorizedColumn_Click(object sender, RoutedEventArgs e)
        {
            AvailableMeasurementsPager.SortBy(signalID => m_dataContext.SubscriberHasRights(signalID));
        }

        private void UpdateFilterExpressions()
        {
            Subscriber currentItem = m_dataContext.CurrentItem;

            AllowedMeasurementsPager.FilterExpression = string.Format("SignalID IN (SELECT SignalID FROM SubscriberMeasurement WHERE NodeID = '{0}' AND SubscriberID = '{1}' AND Allowed <> 0)", currentItem.NodeID.ToString().ToLower(), currentItem.ID.ToString().ToLower());
            DeniedMeasurementsPager.FilterExpression = string.Format("SignalID IN (SELECT SignalID FROM SubscriberMeasurement WHERE NodeID = '{0}' AND SubscriberID = '{1}' AND Allowed = 0)", currentItem.NodeID.ToString().ToLower(), currentItem.ID.ToString().ToLower());

            if (AllowedMeasurementsPager.IsLoaded)
                AllowedMeasurementsPager.ReloadDataGrid();

            if (DeniedMeasurementsPager.IsLoaded)
                DeniedMeasurementsPager.ReloadDataGrid();
        }

        private void OpenPopupButton_Click(object sender, RoutedEventArgs e)
        {
            AccessControlPrecedencePopup.IsOpen = true;
        }

        private void ClosePopupButton_Click(object sender, RoutedEventArgs e)
        {
            AccessControlPrecedencePopup.IsOpen = false;
        }

        #endregion
    }
}
