//******************************************************************************************************
//  AdapterUserControl.xaml.cs - Gbtc
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
//  05/05/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeSeriesFramework.UI.DataModels;
using TVA;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for AdapterUserControl.xaml
    /// </summary>
    public partial class AdapterUserControl : UserControl
    {
        #region [ Members ]

        private ViewModels.Adapters m_dataContext;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="AdapterUserControl"/> class.
        /// </summary>
        public AdapterUserControl(AdapterType adapterType)
        {
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(AdapterUserControl_Unloaded);
            m_dataContext = new ViewModels.Adapters(7, adapterType);
            this.DataContext = m_dataContext;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles unload event of the <see cref="AdapterUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Argument of the event.</param>
        private void AdapterUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ViewModels.Adapters).ProcessPropertyChange();
        }

        /// <summary>
        /// Handles PreviewKeyDown event on the datagrid.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments for the event.</param>
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

        /// <summary>
        /// Handles Click event on the button labeled "Browse..."
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments for the event.</param>
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browser = new System.Windows.Forms.FolderBrowserDialog();

            browser.SelectedPath = SearchDirectoryTextBox.Text;

            if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SearchDirectoryTextBox.Text = browser.SelectedPath;
        }

        /// <summary>
        /// Handles Click event on the button labeled "Default".
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments for the event.</param>
        private void Default_Click(object sender, RoutedEventArgs e)
        {
            TimeSeriesFramework.UI.ViewModels.Adapters dataContext = this.DataContext as TimeSeriesFramework.UI.ViewModels.Adapters;

            if (dataContext != null && dataContext.SelectedParameter != null)
            {
                Dictionary<string, string> settings = dataContext.CurrentItem.ConnectionString.ToNonNullString().ParseKeyValuePairs();
                settings.Remove(dataContext.SelectedParameter.Name);
                dataContext.CurrentItem.ConnectionString = settings.JoinKeyValuePairs();
            }
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            m_dataContext.SortData(e.Column.SortMemberPath);
        }

        private void GridDetailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_dataContext.IsNewRecord)
                DataGridList.SelectedIndex = -1;
        }

        #endregion
    }
}
