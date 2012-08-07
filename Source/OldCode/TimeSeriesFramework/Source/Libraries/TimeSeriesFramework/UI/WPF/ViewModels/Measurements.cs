//******************************************************************************************************
//  Measurements.cs - Gbtc
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
//  05/13/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added constructor overload and other changes to handle device specific data.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI.DataModels;
using TVA;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="TimeSeriesFramework.UI.DataModels.Measurement"/> collection.
    /// </summary>
    public class Measurements : PagedViewModelBase<TimeSeriesFramework.UI.DataModels.Measurement, Guid>
    {
        #region [ Members ]

        private Dictionary<int, string> m_historianLookupList;
        private Dictionary<int, string> m_signalTypeLookupList;
        private int m_deviceID;
        private List<Guid> m_allKeys; 
        private RelayCommand m_searchCommand;
        private RelayCommand m_showAllCommand;

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
            Load();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return (CurrentItem.SignalID == null || CurrentItem.SignalID == Guid.Empty);
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
                    m_searchCommand = new RelayCommand(Search, (param) => true);

                return m_searchCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to show all measurements.
        /// </summary>
        public ICommand ShowAllCommand
        {
            get
            {
                if (m_showAllCommand == null)
                    m_showAllCommand = new RelayCommand(ShowAll);

                return m_showAllCommand;
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
        /// Creates a new instance of <see cref="TimeSeriesFramework.UI.DataModels.Measurement"/> and assigns it to CurrentItem.
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
        /// Loads collection of <see cref="TimeSeriesFramework.UI.DataModels.Measurement"/> defined in the database.
        /// </summary>
        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            List<Guid> pageKeys;

            try
            {
                if ((object)ItemsKeys == null)
                {
                    m_allKeys = DataModels.Measurement.LoadSignalIDs(null, m_deviceID);
                    ItemsKeys = m_allKeys;
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
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Hanldes <see cref="SearchCommand"/>.
        /// </summary>
        /// <param name="parameter">string value to search for in measurement collection.</param>
        public virtual void Search(object parameter)
        {
            if (parameter != null && !string.IsNullOrEmpty(parameter.ToString()))
            {
                string searchText = parameter.ToString();

                CurrentPageNumber = 1;
                ItemsKeys = DataModels.Measurement.LoadSignalIDs(null, m_deviceID, false, false, searchText);
                Load();
            }
        }

        /// <summary>
        /// Handles <see cref="ShowAllCommand"/>.
        /// </summary>
        public void ShowAll()
        {
            if (m_allKeys != ItemsKeys)
            {
                CurrentPageNumber = 1;
                ItemsKeys = m_allKeys;
                Load();
            }
        }

        #endregion

    }
}
