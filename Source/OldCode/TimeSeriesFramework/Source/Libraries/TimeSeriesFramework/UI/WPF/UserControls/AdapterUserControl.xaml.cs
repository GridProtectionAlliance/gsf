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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for AdapterUserControl.xaml
    /// </summary>
    public partial class AdapterUserControl : UserControl
    {
        /// <summary>
        /// Creates an instance of <see cref="AdapterUserControl"/> class.
        /// </summary>
        public AdapterUserControl(AdapterType adapterType)
        {
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(AdapterUserControl_Unloaded);
            this.DataContext = new ViewModels.Adapters(18, adapterType);
        }

        /// <summary>
        /// Handles unload event of the <see cref="AdapterUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Argument of the event.</param>
        void AdapterUserControl_Unloaded(object sender, RoutedEventArgs e)
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
    }
}
