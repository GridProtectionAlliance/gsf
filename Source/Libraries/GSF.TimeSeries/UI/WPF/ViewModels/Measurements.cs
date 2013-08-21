//******************************************************************************************************
//  Measurements.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  05/13/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added constructor overload and other changes to handle device specific data.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GSF.Searching;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.DataModels;
using GSF.TimeSeries.UI.UserControls;

namespace GSF.TimeSeries.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="GSF.TimeSeries.UI.DataModels.Measurement"/> collection.
    /// </summary>
    public class Measurements : PagedViewModelBase<DataModels.Measurement, Guid>
    {
        #region [ Members ]

        private readonly Dictionary<int, string> m_historianLookupList;
        private readonly Dictionary<int, string> m_signalTypeLookupList;
        private readonly int m_deviceID;
        private IList<Guid> m_allKeys;
        private RelayCommand m_searchCommand;
        private RelayCommand m_advancedFindCommand;
        private RelayCommand m_closeAdvancedFindCommand;
        private string m_filterExpression;

        private string m_searchText;
        private bool m_ignoreCase;
        private bool m_useWildcards;
        private bool m_useRegex;
        private ICollection<AdvancedSearchCategory> m_searchCategories;
        private bool m_advancedFindIsOpen;

        private readonly AsyncSearcher<DataModels.Measurement> m_searcher;
        private string m_pendingSearch;
        private bool m_searching;
        private bool m_cancelSearch;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Measurements"/> class.
        /// </summary>
        /// <param name="deviceID">The ID of the device that the current measurement is associated with..</param>
        /// <param name="itemsPerPage">The number of measurements to display on each page of the data grid.</param>
        /// <param name="autosave">Determines whether the current item is saved automatically when a new item is selected.</param>
        public Measurements(int deviceID, int itemsPerPage, bool autosave = true)
            : base(0, autosave)     // Set ItemsPerPage to zero to avoid load() in the base class.
        {
            m_deviceID = deviceID;
            ItemsPerPage = itemsPerPage;

            m_historianLookupList = Historian.GetLookupList(null, isOptional: true);
            m_signalTypeLookupList = SignalType.GetLookupList(null);
            m_searcher = new AsyncSearcher<DataModels.Measurement>();
            m_searcher.MatchesFound += Searcher_MatchesFound;
            m_searcher.SearchComplete += Searcher_SearchComplete;

            Load();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Measurements"/> class.
        /// </summary>
        /// <param name="autosave">Determines whether the current item is saved automatically when a new item is selected.</param>
        public Measurements(bool autosave = true)
            : base(0, autosave)
        {
            m_historianLookupList = Historian.GetLookupList(null, isOptional: true);
            m_signalTypeLookupList = SignalType.GetLookupList(null);
            m_searcher = new AsyncSearcher<DataModels.Measurement>();
            m_searcher.MatchesFound += Searcher_MatchesFound;
            m_searcher.SearchComplete += Searcher_SearchComplete;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the filter expression used when querying records from the database.
        /// </summary>
        public string FilterExpression
        {
            get
            {
                string filterExpression = null;

                if (m_deviceID > 0 && !string.IsNullOrEmpty(m_filterExpression))
                    filterExpression = string.Format("DeviceID = {0} AND ({1})", m_deviceID, m_filterExpression);
                else if (m_deviceID > 0)
                    filterExpression = string.Format("DeviceID = {0}", m_deviceID);
                else if (!string.IsNullOrEmpty(m_filterExpression))
                    filterExpression = m_filterExpression;

                return filterExpression;
            }
            set
            {
                m_filterExpression = value;
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return (CurrentItem.SignalID == Guid.Empty);
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of historians defined in the database.
        /// </summary>
        public virtual Dictionary<int, string> HistorianLookupList
        {
            get
            {
                return m_historianLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of signal types defined in the database.
        /// </summary>
        public virtual Dictionary<int, string> SignalTypeLookupList
        {
            get
            {
                return m_signalTypeLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to search within measurements.
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                if (m_searchCommand == null)
                    m_searchCommand = new RelayCommand(Search);

                return m_searchCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to open the advanced find popup.
        /// </summary>
        public ICommand AdvancedFindCommand
        {
            get
            {
                if (m_advancedFindCommand == null)
                    m_advancedFindCommand = new RelayCommand(() => AdvancedFindIsOpen = true);

                return m_advancedFindCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to close the advanced find popup.
        /// </summary>
        public ICommand CloseAdvancedFindCommand
        {
            get
            {
                if ((object)m_closeAdvancedFindCommand == null)
                    m_closeAdvancedFindCommand = new RelayCommand(() => AdvancedFindIsOpen = false);

                return m_closeAdvancedFindCommand;
            }
        }

        /// <summary>
        /// Gets or sets the list of all keys without the search text applied.
        /// </summary>
        public IList<Guid> AllKeys
        {
            get
            {
                if ((object)m_allKeys == null)
                    m_allKeys = DataModels.Measurement.LoadSignalIDs(null, FilterExpression, SortMember, SortDirection);

                return m_allKeys;
            }
            set
            {
                m_allKeys = value;
            }
        }

        /// <summary>
        /// Gets or sets the text to search for in the measurements.
        /// </summary>
        public string SearchText
        {
            get
            {
                return m_searchText;
            }
            set
            {
                m_searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether to ignore casing in searches.
        /// </summary>
        public bool IgnoreCase
        {
            get
            {
                return m_ignoreCase;
            }
            set
            {
                m_ignoreCase = value;
                OnPropertyChanged("IgnoreCase");
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether to use wildcards in searches.
        /// </summary>
        public bool UseWildcards
        {
            get
            {
                return m_useWildcards;
            }
            set
            {
                m_useWildcards = value;
                OnPropertyChanged("UseWildcards");
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether search tokens are regular expressions.
        /// </summary>
        public bool UseRegex
        {
            get
            {
                return m_useRegex;
            }
            set
            {
                m_useRegex = value;
                OnPropertyChanged("UseRegex");
            }
        }

        /// <summary>
        /// Gets or sets the collection of categories on which to search for the <see cref="SearchText"/>.
        /// </summary>
        public ICollection<AdvancedSearchCategory> SearchCategories
        {
            get
            {
                return m_searchCategories;
            }
            set
            {
                m_searchCategories = value;
                OnPropertyChanged("SearchCategories");
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the advanced find dialog is open.
        /// </summary>
        public bool AdvancedFindIsOpen
        {
            get
            {
                return m_advancedFindIsOpen;
            }
            set
            {
                if (m_advancedFindIsOpen != value)
                {
                    m_advancedFindIsOpen = value;
                    OnPropertyChanged("AdvancedFindIsOpen");
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override Guid GetCurrentItemKey()
        {
            return CurrentItem.SignalID;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.PointTag;
        }

        /// <summary>
        /// Creates a new instance of <see cref="GSF.TimeSeries.UI.DataModels.Measurement"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            if (m_historianLookupList.Count > 1)
                CurrentItem.HistorianID = m_historianLookupList.Skip(1).First().Key;
            else if (m_historianLookupList.Count > 0)
                CurrentItem.HistorianID = m_historianLookupList.First().Key;

            if (m_signalTypeLookupList.Count > 0)
                CurrentItem.SignalTypeID = m_signalTypeLookupList.First().Key;
        }

        /// <summary>
        /// Initialization to be done before the initial call to <see cref="Load"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            SortMember = "PointID";
            SortDirection = "ASC";
        }

        /// <summary>
        /// Loads collection of <see cref="GSF.TimeSeries.UI.DataModels.Measurement"/> defined in the database.
        /// </summary>
        public override void Load()
        {
            List<Guid> pageKeys;

            try
            {
                if (!m_searching)
                    Mouse.OverrideCursor = Cursors.Wait;

                if ((object)ItemsKeys == null)
                {
                    ItemsKeys = DataModels.Measurement.LoadSignalIDs(null, FilterExpression, SortMember, SortDirection);

                    if ((object)SortSelector != null)
                    {
                        if (SortDirection == "ASC")
                            ItemsKeys = ItemsKeys.OrderBy(SortSelector).ToList();
                        else
                            ItemsKeys = ItemsKeys.OrderByDescending(SortSelector).ToList();
                    }

                    AllKeys = ItemsKeys;
                }

                pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
                ItemsSource = DataModels.Measurement.LoadFromKeys(null, pageKeys);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex);
                }
            }
            finally
            {
                if (!m_searching)
                    Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Handles <see cref="SearchCommand"/>.
        /// </summary>
        public virtual void Search()
        {
            AdvancedFindIsOpen = false;

            if (m_searching || m_searcher.Searching)
            {
                // We are in the middle of searching so we
                // need to set up a pending search and
                // cancel the current search operation
                m_pendingSearch = m_searchText;
                m_cancelSearch = true;
                m_searcher.Cancel();
            }
            else if (!string.IsNullOrEmpty(m_pendingSearch) || !string.IsNullOrEmpty(m_searchText))
            {
                // Begin the search
                SetCurrentPageNumber(1);
                ItemsKeys = new List<Guid>();
                Load();
                BeginSearching();
            }
            else
            {
                // Search text is empty so show all
                m_pendingSearch = null;
                SetCurrentPageNumber(1);
                ItemsKeys = AllKeys;
                Load();
            }
        }

        /// <summary>
        /// Sorts the keys by the given sort member in the given direction.
        /// </summary>
        /// <param name="sortMember">The member by which to sort the data.</param>
        /// <param name="sortDirection">The direction in which to sort the data.</param>
        public override void SortData(string sortMember, ListSortDirection sortDirection)
        {
            bool search = ((object)ItemsKeys != (object)AllKeys);

            SortSelector = null;
            SortMember = sortMember;
            SortDirection = (sortDirection == ListSortDirection.Descending) ? "DESC" : "ASC";
            AllKeys = null;
            ItemsKeys = null;

            if (search)
                Search();
            else
                Load();
        }

        /// <summary>
        /// Sorts model data.
        /// </summary>
        /// <param name="sortSelector">Transform function for sorting.</param>
        /// <param name="sortDirection">Ascending or descending.</param>
        public override void SortDataBy(Func<Guid, object> sortSelector, ListSortDirection sortDirection)
        {
            bool search = ((object)ItemsKeys != (object)AllKeys);

            SortSelector = sortSelector;
            SortMember = string.Empty;
            SortDirection = (sortDirection == ListSortDirection.Descending) ? "DESC" : "ASC";
            AllKeys = null;
            ItemsKeys = null;

            if (search)
                Search();
            else
                Load();
        }

        /// <summary>
        /// Loads advanced find settings from isolated storage.
        /// </summary>
        public void LoadSettings()
        {
            string searchText;
            string ignoreCase;
            string useWildcards;
            string useRegex;
            string searchCategories;
            HashSet<string> searchCategoriesSet;

            searchText = (string)IsolatedStorageManager.ReadFromIsolatedStorage("MeasurementSearchText") ?? string.Empty;
            ignoreCase = (string)IsolatedStorageManager.ReadFromIsolatedStorage("MeasurementSearchIgnoreCase") ?? string.Empty;
            useWildcards = (string)IsolatedStorageManager.ReadFromIsolatedStorage("MeasurementSearchUseWildcards") ?? string.Empty;
            useRegex = (string)IsolatedStorageManager.ReadFromIsolatedStorage("MeasurementSearchUseRegex") ?? string.Empty;
            searchCategories = (string)IsolatedStorageManager.ReadFromIsolatedStorage("MeasurementSearchCategories") ?? string.Empty;

            SearchText = searchText;
            IgnoreCase = string.IsNullOrEmpty(ignoreCase) || ignoreCase.ParseBoolean();
            UseWildcards = string.IsNullOrEmpty(useWildcards) || useWildcards.ParseBoolean();
            UseRegex = !string.IsNullOrEmpty(useRegex) && useRegex.ParseBoolean();
            searchCategoriesSet = new HashSet<string>(searchCategories.Split(','));

            foreach (AdvancedSearchCategory category in SearchCategories)
            {
                if (string.IsNullOrEmpty(searchCategories) || searchCategoriesSet.Contains(category.Name))
                    category.Selected = true;
            }
        }

        /// <summary>
        /// Saves advanced find settings to isolated storage.
        /// </summary>
        public void SaveSettings()
        {
            string searchCategories = string.Empty;

            if (SearchCategories.Any(category => category.Selected))
            {
                searchCategories = SearchCategories
                    .Where(category => category.Selected)
                    .Select(category => category.Name)
                    .Aggregate((str1, str2) => str1 + "," + str2);
            }

            IsolatedStorageManager.WriteToIsolatedStorage("MeasurementSearchText", SearchText);
            IsolatedStorageManager.WriteToIsolatedStorage("MeasurementSearchIgnoreCase", IgnoreCase.ToString());
            IsolatedStorageManager.WriteToIsolatedStorage("MeasurementSearchUseWildcards", UseWildcards.ToString());
            IsolatedStorageManager.WriteToIsolatedStorage("MeasurementSearchUseRegex", UseRegex.ToString());
            IsolatedStorageManager.WriteToIsolatedStorage("MeasurementSearchCategories", searchCategories);
        }

        private void BeginSearching()
        {
            Thread searchThread;

            try
            {
                m_searching = true;

                m_searcher.SearchText = m_pendingSearch ?? m_searchText;
                m_searcher.IgnoreCase = m_ignoreCase;
                m_searcher.UseWildcards = m_useWildcards;
                m_searcher.UseRegex = m_useRegex;
                m_searcher.SearchCategories.Clear();

                foreach (AdvancedSearchCategory category in m_searchCategories)
                {
                    if (category.Selected)
                        m_searcher.SearchCategories.Add(category.Name);
                }

                m_pendingSearch = null;
                Mouse.OverrideCursor = Cursors.AppStarting;
                searchThread = new Thread(SearchInBackground);
                searchThread.IsBackground = true;
                searchThread.Start();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                m_searching = false;
                CommonFunctions.LogException(null, "Search " + DataModelName, ex);
            }
        }

        private void SearchInBackground()
        {
            const int SearchGroupSize = 1000;

            IList<Guid> allKeys = AllKeys;
            IList<DataModels.Measurement> measurements;
            List<Guid> searchGroup;
            int searchIndex = 0;

            string sortMember = SortMember;
            string sortDirection = SortDirection;
            PropertyInfo sortProperty = typeof(DataModels.Measurement).GetProperty(sortMember);

            try
            {
                while (searchIndex < allKeys.Count && !m_cancelSearch)
                {
                    searchGroup = allKeys.Skip(searchIndex).Take(SearchGroupSize).ToList();
                    measurements = DataModels.Measurement.LoadFromKeys(null, searchGroup);

                    if ((object)sortProperty == null)
                        m_searcher.Add(measurements);
                    else if (sortDirection == "ASC")
                        m_searcher.Add(measurements.OrderBy(sortProperty.GetValue));
                    else
                        m_searcher.Add(measurements.OrderByDescending(sortProperty.GetValue));

                    searchIndex += SearchGroupSize;
                }

                if (m_cancelSearch)
                    m_searcher.Clear();
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "Search " + DataModelName, ex);
            }
            finally
            {
                m_searching = false;
                m_cancelSearch = false;
            }
        }

        private void Searcher_MatchesFound(object sender, EventArgs<IEnumerable<DataModels.Measurement>> e)
        {
            try
            {
                foreach (DataModels.Measurement measurement in e.Argument)
                    ItemsKeys.Add(measurement.SignalID);

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ItemsSource.Count == 0)
                    {
                        Load();
                    }
                    else
                    {
                        if (ItemsSource.Count < ItemsPerPage)
                        {
                            e.Argument.Take(ItemsPerPage - ItemsSource.Count).ToList()
                                .ForEach(measurement => ItemsSource.Add(measurement));
                        }

                        GeneratePages();
                    }
                }));
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "Search " + DataModelName, ex);
            }
        }

        private void Searcher_SearchComplete(object sender, EventArgs e)
        {
            if (!m_searching)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if ((object)m_pendingSearch == null)
                        Mouse.OverrideCursor = null;
                    else
                        Search();
                }));
            }
        }

        #endregion
    }
}
