//******************************************************************************************************
//  OutputStreamUserControl.xaml.cs - Gbtc
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
//  08/16/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  09/12/2011 - Mehulbhai Thakkar
//       Modified code to use MVVM pattern.
//  09/14/2012 - Aniket Salver 
//          Added paging and sorting technique. 
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.UserControls;
using GSF.PhasorProtocols.UI.ViewModels;

namespace GSF.PhasorProtocols.UI.UserControls
{
    /// <summary>
    /// Interaction logic for OutputStreamUserControl.xaml
    /// </summary>
    public partial class OutputStreamUserControl : UserControl
    {
        #region [ Members ]

        private bool m_committing;
        private readonly OutputStreams m_dataContext;
        private DataGridColumn m_sortColumn;
        private string m_sortMemberPath;
        private ListSortDirection m_sortDirection;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="OutputStreamUserControl"/>.
        /// </summary>
        public OutputStreamUserControl()
        {
            InitializeComponent();
            m_dataContext = new OutputStreams(5);
            m_dataContext.PropertyChanged += ViewModel_PropertyChanged;
            this.DataContext = m_dataContext;
        }

        #endregion

        #region [ Methods ]

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            PanAndZoomViewer viewer = new PanAndZoomViewer(new BitmapImage(new Uri(@"/GSF.PhasorProtocols.UI;component/Images/" + ((Button)sender).Tag, UriKind.Relative)), "Help Me Choose");
            viewer.Owner = Window.GetWindow(this);
            viewer.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            viewer.ShowDialog();
        }

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

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!m_committing && e.EditAction == DataGridEditAction.Commit)
            {
                m_committing = true;
                DataGridList.CommitEdit(DataGridEditingUnit.Row, true);
                m_dataContext.ProcessPropertyChange();
                m_committing = false;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxMirrorSource.SelectedItem = new KeyValuePair<string, string>(m_dataContext.CurrentItem.MirroringSourceDevice, m_dataContext.CurrentItem.MirroringSourceDevice);
        }

        private void OSGrid_Sorting(object sender, DataGridSortingEventArgs e)
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

        private void DataGridEnabledCheckBox_Click(object sender, RoutedEventArgs e)
        {
            // Get a reference to the enabled checkbox that was clicked
            CheckBox enabledCheckBox = sender as CheckBox;

            if ((object)enabledCheckBox != null)
            {
                // Get the runtime ID of the currently selected output stream
                string runtimeID = m_dataContext.RuntimeID;

                if (!string.IsNullOrWhiteSpace(runtimeID))
                {
                    try
                    {
                        // Auto-save changes to the output stream
                        m_dataContext.ProcessPropertyChange();

                        if (m_dataContext.CanSave)
                        {
                            if (enabledCheckBox.IsChecked.GetValueOrDefault())
                                CommonFunctions.SendCommandToService("Initialize " + runtimeID);
                            else
                                CommonFunctions.SendCommandToService("ReloadConfig");
                        }
                    }
                    catch (Exception ex)
                    {
                        if ((object)ex.InnerException != null)
                            CommonFunctions.LogException(null, "Output Stream Autosave", ex.InnerException);
                        else
                            CommonFunctions.LogException(null, "Output Stream Autosave", ex);
                    }
                }
            }
        }

        #endregion
    }
}
