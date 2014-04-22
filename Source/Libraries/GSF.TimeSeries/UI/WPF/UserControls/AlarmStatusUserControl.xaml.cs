//******************************************************************************************************
//  AlarmStatusUserControl.xaml.cs - Gbtc
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
//  02/16/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GSF.TimeSeries.UI.ViewModels;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for AlarmStatusUserControl.xaml
    /// </summary>
    public partial class AlarmStatusUserControl
    {
        #region [ Members ]

        // Fields
        private readonly RaisedAlarms m_dataContext;
        private AlarmMonitor m_monitor;
        private DataGridColumn m_sortColumn;
        private string m_sortMemberPath;
        private ListSortDirection m_sortDirection;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AlarmStatusUserControl"/> class.
        /// </summary>
        public AlarmStatusUserControl()
        {
            InitializeComponent();
            m_dataContext = new RaisedAlarms(20, false);
            m_dataContext.PropertyChanged += ViewModel_PropertyChanged;
            this.DataContext = m_dataContext;
        }

        #endregion

        #region [ Methods ]

        // Sets up the alarm monitor when the user control is loaded.
        private void AlarmStatusUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            int refreshInterval;

            if ((object)AlarmMonitor.Default == null)
                m_monitor = new AlarmMonitor();

            m_dataContext.Monitor = AlarmMonitor.Default ?? m_monitor;

            refreshInterval = m_dataContext.Monitor.RefreshInterval;
            TextBlockAlarmRefreshInterval.Text = refreshInterval.ToString();
            TextBoxRefreshInterval.Text = refreshInterval.ToString();
        }

        // When the user control is unloaded, disposes of the alarm
        // monitor if this user control is the owner of the monitor.
        private void AlarmStatusUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if ((object)m_monitor != null)
                m_monitor.Dispose();
        }

        // If escape is pressed, this either closes the popup window for
        // entering settings or deselects the current item in the data grid.
        private void AlarmStatusUserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (PopupSettings.IsOpen)
                {
                    PopupSettings.IsOpen = false;
                }
                else
                {
                    m_dataContext.Clear();
                    DataGridList.SelectedIndex = -1;
                }
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

        // Sets the color of the rows as they are loaded.
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            m_dataContext.SetRowColor(e.Row);
        }

        // Opens the popup for entering settings.
        private void ButtonDisplaySettings_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = true;
        }

        // Saves changes to settings made in the popup.
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            int refreshInterval;

            if (int.TryParse(TextBoxRefreshInterval.Text, out refreshInterval))
            {
                IsolatedStorageManager.WriteToIsolatedStorage("AlarmStatusRefreshInterval", refreshInterval);
                TextBlockAlarmRefreshInterval.Text = refreshInterval.ToString();
                m_dataContext.Monitor.RefreshInterval = refreshInterval;
                PopupSettings.IsOpen = false;
            }
            else
            {
                m_dataContext.DisplayStatusMessage("Please provide integer value.");
            }
        }

        // Restores original setting based on what is stored in the isolated storage manager.
        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            int refreshInterval;

            IsolatedStorageManager.InitializeStorageForAlarmStatus(true);

            if (!int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("AlarmStatusRefreshInterval").ToString(), out refreshInterval) || refreshInterval < 0)
                refreshInterval = AlarmMonitor.DefaultRefreshInterval;

            TextBlockAlarmRefreshInterval.Text = refreshInterval.ToString();
            TextBoxRefreshInterval.Text = refreshInterval.ToString();
            m_dataContext.Monitor.RefreshInterval = refreshInterval;
            PopupSettings.IsOpen = false;
        }

        // Closes the popup for entering settings.
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupSettings.IsOpen = false;
        }

        #endregion
    }
}
