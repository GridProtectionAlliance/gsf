//******************************************************************************************************
//  SelectMeasurementUserControl.xaml.cs - Gbtc
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
//  05/17/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GSF.TimeSeries.Transport.UI.ViewModels;
using GSF.TimeSeries.UI;
using DataModelMeasurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SelectMeasurementUserControl.xaml
    /// </summary>
    public partial class SelectMeasurementUserControl : UserControl
    {
        #region [ Members ]

        // Delegates

        /// <summary>
        /// Method signature for function used to handle SourceCollectionChanged event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        public delegate void OnSourceCollectionChanged(object sender, RoutedEventArgs e);

        // Events

        /// <summary>
        /// Event to notify subcribers when collection changes.
        /// </summary>
        public event OnSourceCollectionChanged SourceCollectionChanged;

        // Fields
        private SelectMeasurements m_dataContext;
        private ObservableCollection<DataModelMeasurement> m_itemsSource;
        private int m_currentDeviceID;

        #endregion

        #region [ Properties ]

        // Dependency Properties

        /// <summary>
        /// <see cref="DependencyProperty"/> to determine number of measurements per page.
        /// </summary>
        public static readonly DependencyProperty ItemsPerPageProperty = DependencyProperty.Register("ItemsPerPage", typeof(int),
            typeof(SelectMeasurementUserControl), new UIPropertyMetadata(0));

        /// <summary>
        /// <see cref="DependencyProperty"/> to determine if only records with Internal flag set to true are displayed.
        /// </summary>
        public static readonly DependencyProperty FilterByInternalProperty = DependencyProperty.Register("FilterByInternal", typeof(bool),
            typeof(SelectMeasurementUserControl), new UIPropertyMetadata(false));

        /// <summary>
        /// Gets or sets number of measurements to display on a page.
        /// </summary>
        public int ItemsPerPage
        {
            get
            {
                return (int)GetValue(ItemsPerPageProperty);
            }
            set
            {
                SetValue(ItemsPerPageProperty, value);
            }
        }

        /// <summary>
        /// Gets or set falg to determine if only measurements with Internal flag set true are returned.
        /// </summary>
        public bool FilterByInternal
        {
            get
            {
                return (bool)GetValue(FilterByInternalProperty);
            }
            set
            {
                SetValue(FilterByInternalProperty, value);
            }
        }

        /// <summary>
        /// Gets updated measurement list.
        /// </summary>
        public ObservableCollection<DataModelMeasurement> UpdatedMeasurements
        {
            get
            {
                return m_dataContext.ItemsSource;
            }
        }

        /// <summary>
        /// Gets or sets current device selected on the parent screen for filtering purpose.
        /// </summary>
        public int CurrentDeviceID
        {
            get
            {
                return m_currentDeviceID;
            }
            set
            {
                m_currentDeviceID = value;
            }
        }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="SelectMeasurementUserControl"/> class.
        /// </summary>
        public SelectMeasurementUserControl()
        {
            InitializeComponent();
            this.Loaded += SelectMeasurementUserControl_Loaded;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles loaded event for select measurement user control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectMeasurementUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsolatedStorageManager.ReadFromIsolatedStorage("DisplayInternal") == null)
                IsolatedStorageManager.WriteToIsolatedStorage("DisplayInternal", false);

            if (FilterByInternal)
            {
                CheckboxDisplayInternal.Visibility = Visibility.Visible;
                CheckboxDisplayInternal.IsChecked = IsolatedStorageManager.ReadFromIsolatedStorage("DisplayInternal").ToString().ParseBoolean();
            }
            else
            {
                CheckboxDisplayInternal.Visibility = Visibility.Collapsed;
            }

            if (!DesignerProperties.GetIsInDesignMode(this))
                Refresh();
        }

        /// <summary>
        /// Handles property changed event for select measurement user control.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void measurement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (SourceCollectionChanged != null)
                SourceCollectionChanged(this, null);
        }

        /// <summary>
        /// Hanldes checked event on the select all check box.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (DataModelMeasurement measurement in m_itemsSource)
            {
                measurement.Selected = true;
            }
        }

        /// <summary>
        /// Handles unchecked event on the select all check box.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (DataModelMeasurement measurement in m_itemsSource)
            {
                measurement.Selected = false;
            }
        }

        /// <summary>
        /// Method to uncheck check boxes checked by user.
        /// </summary>
        public void UncheckSelection()
        {
            foreach (DataModelMeasurement measurement in m_itemsSource)
            {
                measurement.Selected = false;
            }
        }

        /// <summary>
        /// Refreshes data bound to grid.
        /// </summary>
        /// <param name="deviceID">ID of the device to filter data.</param>
        public void Refresh(int deviceID = 0)
        {
            m_dataContext = new SelectMeasurements(ItemsPerPage, true, FilterByInternal, deviceID, (bool)CheckboxDisplayInternal.IsChecked);
            m_itemsSource = m_dataContext.ItemsSource;

            foreach (DataModelMeasurement measurement in m_itemsSource)
            {
                measurement.PropertyChanged += measurement_PropertyChanged;
            }

            this.DataContext = m_dataContext;
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            m_dataContext.SortData(e.Column.SortMemberPath);
        }

        private void CheckboxDisplayInternal_Checked(object sender, RoutedEventArgs e)
        {
            Refresh(m_currentDeviceID);
            IsolatedStorageManager.WriteToIsolatedStorage("DisplayInternal", true);
        }

        private void CheckboxDisplayInternal_Unchecked(object sender, RoutedEventArgs e)
        {
            Refresh(m_currentDeviceID);
            IsolatedStorageManager.WriteToIsolatedStorage("DisplayInternal", false);
        }

        #endregion

    }
}
