//******************************************************************************************************
//  OutputStreamDeviceMeasurementUserControl.cs - Gbtc
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
//  09/14/2011 - Aniket Salver
//       Generated original version of source code.
//  09/16/2011 - Mehulbhai Thakkar
//       Added code to attach this user control to parent Output Stream.
//       Added delete key handling logic.
//  09/15/2012 - Aniket Salver 
//          Added paging and sorting technique. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.ViewModels;
using GSF.TimeSeries.UI;
using Measurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.PhasorProtocols.UI.UserControls
{
    /// <summary>
    /// Interaction logic for OutputStreamMeasurementUserControl.xaml
    /// </summary>
    public partial class OutputStreamMeasurementUserControl : UserControl
    {
        #region [ Members ]

        private OutputStreamMeasurements m_dataContext;
        private readonly int m_outputStreamID;
        private ObservableCollection<Measurement> m_newMeasurements;
        private DataGridColumn m_sortColumn;
        private string m_sortMemberPath;
        private ListSortDirection m_sortDirection;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="OutputStreamMeasurementUserControl"/> class.
        /// </summary>
        public OutputStreamMeasurementUserControl(int outputStreamID)
        {
            InitializeComponent();
            m_outputStreamID = outputStreamID;
            m_dataContext = new OutputStreamMeasurements(outputStreamID, 23);
            this.DataContext = m_dataContext;
            m_dataContext.PropertyChanged += ViewModel_PropertyChanged;
            m_newMeasurements = new ObservableCollection<Measurement>();
        }

        #endregion

        #region [ Methods ]

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Are you sure you want to delete " + dataGrid.SelectedItems.Count + " selected item(s)?", "Delete Selected Items", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        e.Handled = true;
                }
            }
        }

        #region [ Popup Code ]

        private void ButtonAddMore_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Measurement> selectedMeasurements;
            ObservableCollection<Measurement> addedMeasurements;

            // Determine which of the selected measurements have been added to the output stream measurements
            selectedMeasurements = Measurement.LoadFromKeys(null, MeasurementPager.SelectedMeasurements.ToList());
            addedMeasurements = new ObservableCollection<Measurement>(selectedMeasurements);

            // Add measurements to output stream measurements
            OutputStreamMeasurement.AddMeasurements(null, m_outputStreamID, addedMeasurements);
            m_dataContext = new OutputStreamMeasurements(m_outputStreamID, 23);
            this.DataContext = m_dataContext;
            PopupAddMore.IsOpen = false;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext = new OutputStreamMeasurements(m_outputStreamID, 23);
            this.DataContext = m_dataContext;
            PopupAddMore.IsOpen = false;
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((object)CommonFunctions.CurrentPrincipal != null &&
                CommonFunctions.CurrentPrincipal.IsInRole("Administrator, Editor"))
            {
                MeasurementPager.ClearSelections();
                PopupAddMore.IsOpen = true;
            }
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortMemberPath != m_sortMemberPath)
                m_sortDirection = ListSortDirection.Ascending;
            else if (m_sortDirection == ListSortDirection.Ascending)
                m_sortDirection = ListSortDirection.Descending;
            else
                m_sortDirection = ListSortDirection.Ascending;

            m_sortColumn = e.Column;
            m_sortMemberPath = e.Column.SortMemberPath;
            m_dataContext.SortData(m_sortMemberPath, m_sortDirection);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemsSource")
                Dispatcher.BeginInvoke(new Action(SortDataGrid));
        }

        private void SortDataGrid()
        {
            if ((object)m_sortColumn != null)
            {
                m_sortColumn.SortDirection = m_sortDirection;
                DataGridList.Items.SortDescriptions.Clear();
                DataGridList.Items.SortDescriptions.Add(new SortDescription(m_sortMemberPath, m_sortDirection));
                DataGridList.Items.Refresh();
            }
        }

        private void GridDetailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_dataContext.IsNewRecord)
                DataGridList.SelectedIndex = -1;
        }

        #endregion
    }
}
