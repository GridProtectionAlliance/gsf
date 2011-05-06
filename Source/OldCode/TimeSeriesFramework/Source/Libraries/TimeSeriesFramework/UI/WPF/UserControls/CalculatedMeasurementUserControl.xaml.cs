//******************************************************************************************************
//  CalculatedMeasurementUserControl.xaml.cs - Gbtc
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
//  05/05/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeSeriesFramework.UI.ViewModels;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for CalculatedMeasurementUserControl.xaml
    /// </summary>
    public partial class CalculatedMeasurementUserControl : UserControl
    {
        /// <summary>
        /// Creates an instance of <see cref="CalculatedMeasurementUserControl"/> class.
        /// </summary>
        public CalculatedMeasurementUserControl()
        {
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(CalculatedMeasurementUserControl_Unloaded);
            this.DataContext = new CalculatedMeasurements(10);
        }

        void CalculatedMeasurementUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            (this.DataContext as CalculatedMeasurements).ProcessPropertyChange();
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
    }
}
