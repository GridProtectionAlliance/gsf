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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GSF.Reflection;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.UI.DataModels;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for AdapterUserControl.xaml
    /// </summary>
    public partial class AdapterUserControl : UserControl
    {
        #region [ Members ]

        private readonly ViewModels.Adapters m_dataContext;
        private DataGridColumn m_sortColumn;
        private string m_sortMemberPath;
        private ListSortDirection m_sortDirection;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="AdapterUserControl"/> class.
        /// </summary>
        public AdapterUserControl(AdapterType adapterType)
        {
            InitializeComponent();
            m_dataContext = new ViewModels.Adapters(7, adapterType);
            m_dataContext.PropertyChanged += ViewModel_PropertyChanged;
            this.DataContext = m_dataContext;
        }

        #endregion

        #region [ Methods ]

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

        private void DataGridEnabledCheckBox_Click(object sender, RoutedEventArgs e)
        {
            // Get a reference to the enabled checkbox that was clicked
            CheckBox enabledCheckBox = sender as CheckBox;

            if ((object)enabledCheckBox != null)
            {
                // Get the runtime ID of the currently selected adapter
                string runtimeID = m_dataContext.RuntimeID;

                if (!string.IsNullOrWhiteSpace(runtimeID))
                {
                    try
                    {
                        // Auto-save changes to the adapter
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
                            CommonFunctions.LogException(null, "Adapter Autosave", ex.InnerException);
                        else
                            CommonFunctions.LogException(null, "Adapter Autosave", ex);
                    }
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
            FolderBrowserDialog browser = new FolderBrowserDialog();

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
            ViewModels.Adapters dataContext = this.DataContext as ViewModels.Adapters;

            if (dataContext != null && dataContext.SelectedParameter != null)
            {
                Dictionary<string, string> settings = dataContext.CurrentItem.ConnectionString.ToNonNullString().ParseKeyValuePairs();
                settings.Remove(dataContext.SelectedParameter.Name);
                dataContext.CurrentItem.ConnectionString = settings.JoinKeyValuePairs();
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

        private void ButtonOpenCustomConfiguration_Click(object sender, RoutedEventArgs e)
        {
            CustomConfigurationEditorAttribute customConfigurationEditorAttribute;
            UIElement customConfigurationElement;
            int adapterTypeIndex;

            try
            {
                adapterTypeIndex = m_dataContext.AdapterTypeSelectedIndex;

                if (adapterTypeIndex >= 0)
                {
                    CustomConfigurationPanel.Children.Clear();

                    if (m_dataContext.AdapterTypeList[adapterTypeIndex].Item1.TryGetAttribute(out customConfigurationEditorAttribute))
                    {
                        if ((object)customConfigurationEditorAttribute.ConnectionString != null)
                            customConfigurationElement = Activator.CreateInstance(customConfigurationEditorAttribute.EditorType, m_dataContext.CurrentItem, customConfigurationEditorAttribute.ConnectionString) as UIElement;
                        else
                            customConfigurationElement = Activator.CreateInstance(customConfigurationEditorAttribute.EditorType, m_dataContext.CurrentItem) as UIElement;

                        if ((object)customConfigurationElement != null)
                        {
                            CustomConfigurationPanel.Children.Insert(0, customConfigurationElement);
                            CustomConfigurationPopup.IsOpen = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to open custom configuration control due to exception: {0}", ex.Message);
                m_dataContext.Popup(message, "Custom Configuration Error", MessageBoxImage.Error);
            }
        }

        private void ButtonOpenParameterConfiguration_Click(object sender, RoutedEventArgs e)
        {
            CustomConfigurationEditorAttribute customConfigurationEditorAttribute;
            UIElement customConfigurationElement;
            int adapterTypeIndex;

            try
            {
                adapterTypeIndex = m_dataContext.AdapterTypeSelectedIndex;

                if (adapterTypeIndex >= 0)
                {
                    CustomConfigurationPanel.Children.Clear();
                    customConfigurationEditorAttribute = m_dataContext.SelectedParameter.Info.GetCustomAttribute<CustomConfigurationEditorAttribute>();

                    if ((object)customConfigurationEditorAttribute != null)
                    {
                        if ((object)customConfigurationEditorAttribute.ConnectionString != null)
                            customConfigurationElement = Activator.CreateInstance(customConfigurationEditorAttribute.EditorType, m_dataContext.CurrentItem, m_dataContext.SelectedParameter.Name, customConfigurationEditorAttribute.ConnectionString) as UIElement;
                        else
                            customConfigurationElement = Activator.CreateInstance(customConfigurationEditorAttribute.EditorType, m_dataContext.CurrentItem, m_dataContext.SelectedParameter.Name) as UIElement;

                        if ((object)customConfigurationElement != null)
                        {
                            CustomConfigurationPanel.Children.Add(customConfigurationElement);
                            CustomConfigurationPopup.IsOpen = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to open custom configuration control due to exception: {0}", ex.Message);
                m_dataContext.Popup(message, "Custom Configuration Error", MessageBoxImage.Error);
            }
        }

        private void CustomConfigurationPopup_ButtonClick(object sender, RoutedEventArgs e)
        {
            Button originalSource = e.OriginalSource as Button;

            if ((object)originalSource != null && (originalSource.IsDefault || originalSource.IsCancel))
                CustomConfigurationPopup.IsOpen = false;
        }

        #endregion
    }
}
