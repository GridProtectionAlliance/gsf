//******************************************************************************************************
//  MeasurementGroupUserControl.xaml.cs - Gbtc
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
//  05/19/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GSF.TimeSeries.Transport.UI.DataModels;
using GSF.TimeSeries.Transport.UI.ViewModels;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for MeasurementGroupUserControl.xaml
    /// </summary>
    public partial class MeasurementGroupUserControl : UserControl
    {
        private readonly MeasurementGroups m_dataContext;

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="MeasurementGroupUserControl"/>.
        /// </summary>
        public MeasurementGroupUserControl()
        {
            InitializeComponent();
            m_dataContext = new MeasurementGroups(1);
            m_dataContext.PropertyChanged += DataContext_PropertyChanged;
            StackPanelManageMeasurementGroup.DataContext = m_dataContext;
        }

        #endregion

        #region [ Methods ]

        private void MemberMeasurementsPager_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadMemberMeasurementsPager();
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;

            if (propertyName == "CurrentItem")
                ReloadMemberMeasurementsPager();
        }

        private void AddMeasurementsButton_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext.AddMeasurement(AvailableMeasurementsPager.SelectedMeasurements);
            AvailableMeasurementsPager.ClearSelections();
            MemberMeasurementsPager.ReloadDataGrid();
        }

        private void RemoveMeasurementsButton_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext.RemoveMeasurement(MemberMeasurementsPager.SelectedMeasurements);
            MemberMeasurementsPager.ReloadDataGrid();
            MemberMeasurementsPager.ClearSelections();
        }

        private void ReloadMemberMeasurementsPager()
        {
            MeasurementGroup currentItem = m_dataContext.CurrentItem;
            MemberMeasurementsPager.FilterExpression = string.Format("SignalID IN (SELECT SignalID FROM MeasurementGroupMeasurement WHERE NodeID = '{0}' AND MeasurementGroupID = {1})", currentItem.NodeID.ToString().ToLower(), currentItem.ID);
            MemberMeasurementsPager.ReloadDataGrid();
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
