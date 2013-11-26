//******************************************************************************************************
//  MeasurementPagerUserControl.xaml.cs - Gbtc
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
//  10/01/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GSF.TimeSeries.UI.ViewModels;

namespace GSF.TimeSeries.UI.UserControls
{
    /// <summary>
    /// Interaction logic for MeasurementPagerUserControl.xaml
    /// </summary>
    public partial class MeasurementPagerUserControl : UserControl
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event triggered whenever the current page changes.
        /// </summary>
        public event EventHandler CurrentPageChanged;

        /// <summary>
        /// Event triggered whenever the currently highlighted item changes.
        /// </summary>
        public event EventHandler CurrentItemChanged;

        /// <summary>
        /// Event triggered whenever the set of selected measurements changes.
        /// </summary>
        public event EventHandler SelectedMeasurementsChanged;

        // Fields
        private Measurements m_dataContext;
        private readonly ISet<Guid> m_selectedMeasurements;
        private readonly ObservableCollection<DataGridColumn> m_dataGridColumns;
        private bool m_overrideDefaultColumns;

        private Func<Guid, object> m_sortKeySelector;
        private DataGridColumn m_sortColumn;
        private string m_sortMemberPath;
        private ListSortDirection m_sortDirection;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MeasurementPagerUserControl"/>.
        /// </summary>
        public MeasurementPagerUserControl()
        {
            InitializeComponent();
            m_selectedMeasurements = new HashSet<Guid>();
            m_dataGridColumns = new ObservableCollection<DataGridColumn>();
            m_dataGridColumns.CollectionChanged += DataGridColumns_CollectionChanged;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of measurements displayed on the data grid per page.
        /// </summary>
        public int ItemsPerPage
        {
            get
            {
                return (int)this.GetValue(ItemsPerPageProperty);
            }
            set
            {
                this.SetValue(ItemsPerPageProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the filter expression used when reading measurements from the database.
        /// </summary>
        public string FilterExpression
        {
            get
            {
                return (string)this.GetValue(FilterExpressionProperty);
            }
            set
            {
                this.SetValue(FilterExpressionProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the boolean flag which determines whether this control
        /// provides checkboxes that allow the user to select measurements to
        /// be placed in the <see cref="SelectedMeasurements"/> set.
        /// </summary>
        public bool Selectable
        {
            get
            {
                return (bool)this.GetValue(SelectableProperty);
            }
            set
            {
                this.SetValue(SelectableProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the boolean flag which determines whether
        /// this control can be filtered by a search expression.
        /// </summary>
        public bool Searchable
        {
            get
            {
                return (bool)this.GetValue(SearchableProperty);
            }
            set
            {
                this.SetValue(SearchableProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the boolean flag which determines whether
        /// the page size should be shown on the control.
        /// </summary>
        public bool ShowPageSize
        {
            get
            {
                return (bool)this.GetValue(ShowPageSizeProperty);
            }
            set
            {
                this.SetValue(ShowPageSizeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the boolean flag which determines whether the
        /// hotkey is enabled for displaying the advanced find popup.
        /// </summary>
        public bool AdvancedHotkeyIsEnabled
        {
            get
            {
                return (bool)this.GetValue(AdvancedHotkeyIsEnabledProperty);
            }
            set
            {
                this.SetValue(AdvancedHotkeyIsEnabledProperty, value);
            }
        }

        /// <summary>
        /// Gets the collection of data grid columns to be displayed.
        /// </summary>
        public ObservableCollection<DataGridColumn> DataGridColumns
        {
            get
            {
                return m_dataGridColumns;
            }
        }

        /// <summary>
        /// Gets the current page of measurements in the measurement pager.
        /// </summary>
        public ObservableCollection<DataModels.Measurement> CurrentPage
        {
            get
            {
                return m_dataContext.ItemsSource;
            }
        }

        /// <summary>
        /// Gets the currently highlighted item on the data grid.
        /// </summary>
        public DataModels.Measurement CurrentItem
        {
            get
            {
                return m_dataContext.CurrentItem;
            }
        }

        /// <summary>
        /// Gets the set of selected measurements.
        /// </summary>
        public ISet<Guid> SelectedMeasurements
        {
            get
            {
                return m_selectedMeasurements;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Updates the collection of selected measurements based on which items are selected
        /// or unselected on the current page. This method also updates the selected measurements
        /// label to reflect the total number of selected measurements.
        /// </summary>
        public void UpdateSelections()
        {
            DataGridRow row;
            CheckBox itemCheckBox;

            DataModels.Measurement measurement;

            // Iterate over all the rows on the current page
            for (int i = 0; i < DataGridList.Items.Count; i++)
            {
                row = DataGridList.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;

                if ((object)row != null)
                {
                    // Get the measurement and its corresponding check box
                    itemCheckBox = FindChildByName("ItemCheckBox", row) as CheckBox;
                    measurement = row.Item as DataModels.Measurement;

                    if ((object)itemCheckBox != null && (object)measurement != null)
                    {
                        // If it's checked, add it to the set of selections;
                        // otherwise, remove it from the set of selection
                        if (itemCheckBox.IsChecked ?? false)
                            m_selectedMeasurements.Add(measurement.SignalID);
                        else
                            m_selectedMeasurements.Remove(measurement.SignalID);
                    }
                }
            }

            SelectedMeasurementsLabel.Content = string.Format("Selected: {0}", m_selectedMeasurements.Count);
        }

        /// <summary>
        /// Causes all selections to be cleared from the data grid.
        /// </summary>
        public void ClearSelections()
        {
            SelectedMeasurements.Clear();
            m_dataContext.Load();
            UpdateSelections();
            OnSelectedMeasurementsChanged();
        }

        /// <summary>
        /// Causes the data grid to be reloaded with a new set of data.
        /// </summary>
        public void ReloadDataGrid()
        {
            bool search = ((object)m_dataContext.ItemsKeys != (object)m_dataContext.AllKeys);

            m_dataContext.ItemsKeys = null;

            if (search)
                m_dataContext.Search();
            else
                m_dataContext.Load();
        }

        /// <summary>
        /// Causes the data grid to be sorted by the values returned by the given key selector.
        /// </summary>
        /// <param name="keySelector">The function to transform measurement IDs to a key to sort by.</param>
        public void SortBy(Func<Guid, object> keySelector)
        {
            if (m_sortKeySelector != keySelector)
                m_sortDirection = ListSortDirection.Ascending;
            else if (m_sortDirection == ListSortDirection.Descending)
                m_sortDirection = ListSortDirection.Ascending;
            else
                m_sortDirection = ListSortDirection.Descending;

            m_sortKeySelector = keySelector;
            m_sortColumn = null;
            m_sortMemberPath = null;
            DataGridList.Items.SortDescriptions.Clear();
            DataGridList.Items.Refresh();

            m_dataContext.SortDataBy(keySelector, m_sortDirection);
        }

        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this <see cref="System.Windows.FrameworkElement"/>
        /// has been updated. The specific dependency property that changed is reported in the arguments parameter.
        /// Overrides <see cref="System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)"/>.
        /// </summary>
        /// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == SelectableProperty)
                DataGridList.Columns[0].Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            else if (e.Property == FilterExpressionProperty && (object)m_dataContext != null)
                m_dataContext.FilterExpression = FilterExpression;
            else if (e.Property == ShowPageSizeProperty)
                DataPager.ShowPageSize = ShowPageSize;
        }

        // Creates and sets the data context once the user control has loaded.
        private void MeasurementPager_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                m_dataContext = new Measurements(false);
                m_dataContext.PropertyChanged += ViewModel_PropertyChanged;
                m_dataContext.FilterExpression = FilterExpression;
                m_dataContext.ItemsPerPage = ItemsPerPage;
                m_dataContext.SearchCategories = AdvancedSearch.Categories;
                m_dataContext.LoadSettings();
                m_dataContext.Load();
                RootPanel.DataContext = m_dataContext;

                Application.Current.Exit += Application_Exit;
            }
        }

        // Saves advanced find settings when the pager is unloaded.
        private void MeasurementPager_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Application.Current.Exit -= Application_Exit;
                m_dataContext.SaveSettings();
            }
        }

        // Handles the hotkey for the advanced find popup.
        private void MeasurementPagerUserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control && AdvancedHotkeyIsEnabled)
            {
                m_dataContext.AdvancedFindIsOpen = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the application exit event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            m_dataContext.SaveSettings();
        }

        // Edits the columns of the data grid so that they reflect any changes made to the DataGridColumns collection.
        private void DataGridColumns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!m_overrideDefaultColumns)
            {
                // If ever this collection changes, it will indicate
                // that the default columns should be overridden
                DataGridColumn selectColumn = DataGridList.Columns[0];
                DataGridList.Columns.Clear();
                DataGridList.Columns.Add(selectColumn);
                m_overrideDefaultColumns = true;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        DataGridList.Columns.Insert(e.NewStartingIndex + i + 1, (DataGridColumn)e.NewItems[i]);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    DataGridList.Columns.Move(e.OldStartingIndex + 1, e.NewStartingIndex + 1);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object column in e.OldItems)
                    {
                        DataGridList.Columns.RemoveAt(e.OldStartingIndex + 1);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        DataGridList.Columns[e.OldStartingIndex + i + 1] = (DataGridColumn)e.NewItems[i];
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    DataGridColumn selectColumn = DataGridList.Columns[0];
                    DataGridList.Columns.Clear();
                    DataGridList.Columns.Add(selectColumn);
                    break;
            }
        }

        // Executes search on the pager when the user presses Enter.
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                m_dataContext.Search();
                e.Handled = true;
            }
        }

        // Selects/unselects all the measurements on all pages.
        private void SelectAllHyperlink_Click(object sender, RoutedEventArgs e)
        {
            // Get the select-all check box
            CheckBox selectAllCheckBox = FindChildByName("SelectAllCheckBox", DataGridList) as CheckBox;

            if ((object)selectAllCheckBox != null)
            {
                if (selectAllCheckBox.IsChecked ?? false)
                {
                    // Select measurements from all pages
                    foreach (Guid signalID in m_dataContext.ItemsKeys)
                        m_selectedMeasurements.Add(signalID);
                }
                else
                {
                    // Deselect measurements from all pages
                    foreach (Guid signalID in m_dataContext.ItemsKeys)
                        m_selectedMeasurements.Remove(signalID);
                }
            }

            // Hide the hyperlink and update selections
            SelectAllHyperlink.Visibility = Visibility.Collapsed;
            UpdateSelections();
            OnSelectedMeasurementsChanged();
        }

        // Selects/unselects all the measurements on the current page.
        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject item;
            CheckBox itemCheckBox;
            CheckBox selectAllCheckBox;

            // Get select-all check box
            selectAllCheckBox = e.Source as CheckBox;

            if ((object)selectAllCheckBox != null)
            {
                for (int i = 0; i < DataGridList.Items.Count; i++)
                {
                    // Get check box for current row
                    item = DataGridList.ItemContainerGenerator.ContainerFromIndex(i);
                    itemCheckBox = FindChildByName("ItemCheckBox", item) as CheckBox;

                    // Update the IsChecked property for the check box
                    if ((object)itemCheckBox != null)
                        itemCheckBox.IsChecked = selectAllCheckBox.IsChecked;
                }

                // Update the hyperlink text to reflect whether items are being selected or unselected
                if (selectAllCheckBox.IsChecked ?? false)
                    SelectAllHyperlink.Content = "Click here to select all measurements on all pages.";
                else
                    SelectAllHyperlink.Content = "Click here to unselect all measurements on all pages.";

                // Make hyperlink visible to the user
                SelectAllHyperlink.Visibility = Visibility.Visible;
            }

            UpdateSelections();
            OnSelectedMeasurementsChanged();
        }

        // Selects the mesurement that the user clicked on.
        private void ItemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox itemCheckBox;
            CheckBox selectAllCheckBox;

            // Update selections first so that we can determine
            // whether all items on the page are selected
            UpdateSelections();

            // Get check boxes
            itemCheckBox = e.Source as CheckBox;
            selectAllCheckBox = FindChildByName("SelectAllCheckBox", DataGridList) as CheckBox;

            if ((object)itemCheckBox != null && (object)selectAllCheckBox != null)
            {
                // Update select-all check box, if necessary
                if (!(itemCheckBox.IsChecked ?? false))
                    selectAllCheckBox.IsChecked = false;
                else if (m_dataContext.ItemsSource.All(measurement => m_selectedMeasurements.Contains(measurement.SignalID)))
                    selectAllCheckBox.IsChecked = true;
            }

            // Hide hyperlink because user has deliberately ignored it
            SelectAllHyperlink.Visibility = Visibility.Collapsed;
            OnSelectedMeasurementsChanged();
        }

        // Receives notification any time the data grid is loading a row.
        // We need to ensure that the selection check box is properly checked or unchecked.
        // This cannot be done until the row has finished loading, so we attach to that event here.
        private void DataGridList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Loaded += DataGridRow_Loaded;
        }

        // Ensures that the selection check box is properly
        // checked or unchecked based on the current selections.
        private void DataGridRow_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridRow row;
            CheckBox itemCheckBox;
            DataModels.Measurement measurement;
            CheckBox selectAllCheckBox;

            // Get loaded row and select-all check box
            row = e.Source as DataGridRow;
            selectAllCheckBox = FindChildByName("SelectAllCheckBox", DataGridList) as CheckBox;

            if ((object)row != null)
            {
                // Get measurement and its corresponding check box
                row.Loaded -= DataGridRow_Loaded;
                itemCheckBox = FindChildByName("ItemCheckBox", row) as CheckBox;
                measurement = row.Item as DataModels.Measurement;

                if ((object)itemCheckBox != null && (object)measurement != null)
                {
                    // Set the state of the check box based on whether the measurement is selected
                    itemCheckBox.IsChecked = m_selectedMeasurements.Contains(measurement.SignalID);
                }
            }

            // Set the state of the select-all check box based on
            // the selected state of all measurements on the page
            if ((object)selectAllCheckBox != null)
                selectAllCheckBox.IsChecked = m_dataContext.ItemsSource.All(item => m_selectedMeasurements.Contains(item.SignalID));

            // Hide the select all hyperlink as it's no longer relevant on the new page
            SelectAllHyperlink.Visibility = Visibility.Collapsed;
        }

        // Sorts the data grid by the selected sort member.
        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortMemberPath != m_sortMemberPath)
                m_sortDirection = ListSortDirection.Ascending;
            else if (m_sortDirection == ListSortDirection.Ascending)
                m_sortDirection = ListSortDirection.Descending;
            else
                m_sortDirection = ListSortDirection.Ascending;

            m_sortKeySelector = null;
            m_sortColumn = e.Column;
            m_sortMemberPath = e.Column.SortMemberPath;
            m_dataContext.SortData(m_sortMemberPath, m_sortDirection);
        }

        // Sorts the current page of the data grid each time the view model's ItemsSource is modified.
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentItem")
            {
                OnCurrentItemChanged();
            }
            else if (e.PropertyName == "ItemsSource")
            {
                Dispatcher.BeginInvoke(new Action(ResizeDataGridContent));
                Dispatcher.BeginInvoke(new Action(SortDataGrid));
                OnCurrentPageChanged();
            }
            else if (e.PropertyName == "AdvancedFindIsOpen")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextBox searchTextBox = m_dataContext.AdvancedFindIsOpen
                        ? AdvancedSearch.FindName("SearchTextBox") as TextBox
                        : SearchTextBox;

                    if ((object)searchTextBox != null)
                    {
                        searchTextBox.Focus();
                        searchTextBox.SelectAll();
                    }
                }));
            }
        }

        // Forces resize on the columns in the data grid
        // so that they can shrink as well as grow.
        private void ResizeDataGridContent()
        {
            List<DataGridLength> columnWidths = DataGridList.Columns.Select(column => column.Width).ToList();

            foreach (DataGridColumn column in DataGridList.Columns)
                column.Width = 0;

            DataGridList.UpdateLayout();

            for (int i = 0; i < DataGridList.Columns.Count; i++)
                DataGridList.Columns[i].Width = new DataGridLength(columnWidths[i].Value, columnWidths[i].UnitType);
        }

        // Sorts the current page of the data grid.
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

        // Traverses the visual tree, starting at the given parent object, toward
        // the leaf nodes in order to find a FrameworkElement with the given name.
        private FrameworkElement FindChildByName(string name, DependencyObject parent)
        {
            DependencyObject dependencyChild;
            FrameworkElement frameworkChild;
            int childrenCount;

            childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                dependencyChild = VisualTreeHelper.GetChild(parent, i);
                frameworkChild = dependencyChild as FrameworkElement;

                if ((object)frameworkChild != null && frameworkChild.Name == name)
                    return frameworkChild;

                frameworkChild = FindChildByName(name, dependencyChild);

                if ((object)frameworkChild != null)
                    return frameworkChild;
            }

            return null;
        }

        // Triggers the CurrentPageChanged event.
        private void OnCurrentPageChanged()
        {
            if ((object)CurrentPageChanged != null)
                CurrentPageChanged(this, EventArgs.Empty);
        }

        // Triggers the CurrentItemChanged event.
        private void OnCurrentItemChanged()
        {
            if ((object)CurrentItemChanged != null)
                CurrentItemChanged(this, EventArgs.Empty);
        }

        // Triggers the SelectedMeasurementsChanged event.
        private void OnSelectedMeasurementsChanged()
        {
            if ((object)SelectedMeasurementsChanged != null)
                SelectedMeasurementsChanged(this, EventArgs.Empty);
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="ItemsPerPage"/> property.
        /// </summary>
        public static DependencyProperty ItemsPerPageProperty = DependencyProperty.Register("ItemsPerPage", typeof(int), typeof(MeasurementPagerUserControl), new PropertyMetadata(10));

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="FilterExpression"/> property.
        /// </summary>
        public static DependencyProperty FilterExpressionProperty = DependencyProperty.Register("FilterExpression", typeof(string), typeof(MeasurementPagerUserControl));

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="Selectable"/> property.
        /// </summary>
        public static DependencyProperty SelectableProperty = DependencyProperty.Register("Selectable", typeof(bool), typeof(MeasurementPagerUserControl), new PropertyMetadata(false));

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="Searchable"/> property.
        /// </summary>
        public static DependencyProperty SearchableProperty = DependencyProperty.Register("Searchable", typeof(bool), typeof(MeasurementPagerUserControl), new PropertyMetadata(false));

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="ShowPageSize"/> property.
        /// </summary>
        public static DependencyProperty ShowPageSizeProperty = DependencyProperty.Register("ShowPageSize", typeof(bool), typeof(MeasurementPagerUserControl), new PropertyMetadata(true));

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="AdvancedHotkeyIsEnabled"/> property.
        /// </summary>
        public static DependencyProperty AdvancedHotkeyIsEnabledProperty = DependencyProperty.Register("AdvancedHotkeyIsEnabled", typeof(bool), typeof(MeasurementPagerUserControl), new PropertyMetadata(true));

        #endregion
    }
}
