//******************************************************************************************************
//  SelectMeasurements.cs - Gbtc
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

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GSF.TimeSeries.Transport.UI.DataModels;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using DataModelMeasurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.TimeSeries.Transport.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="DataModelMeasurement"/> collection.
    /// </summary>
    internal class SelectMeasurements : PagedViewModelBase<DataModelMeasurement, Guid>
    {
        #region [ Members ]

        // Fields
        private ObservableCollection<DataModelMeasurement> m_measurements;
        private RelayCommand m_searchCommand;
        private RelayCommand m_showAllCommand;
        private readonly bool m_filterByInternal;
        private readonly bool m_includeInternal;
        private readonly int m_deviceID;

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

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="SelectMeasurements"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Number of items to display on a page.</param>
        /// <param name="autosave">Boolean indicator to save data automatically.</param>
        /// <param name="filterByInternal">Boolean indicator to determine if only measurements with internal flag set to true needs to be displayed.</param>
        /// <param name="deviceID">ID of the device to filter data.</param>
        /// <param name="includeInternal">Flag to determine if internal measurements should be included.</param>
        public SelectMeasurements(int itemsPerPage, bool autosave = true, bool filterByInternal = false, int deviceID = 0, bool includeInternal = false)
            : base(0, autosave)     // Set ItemsPerPage to zero to avoid load() in the base class.
        {
            ItemsPerPage = itemsPerPage;
            m_filterByInternal = filterByInternal;
            m_includeInternal = includeInternal;
            m_deviceID = deviceID;
            Load();
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
        /// Loads collection of <see cref="DataModelMeasurement"/> defined in the database.
        /// </summary>
        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                if (m_filterByInternal)
                    m_measurements = DataModelMeasurement.Load(null, m_deviceID, true, m_includeInternal);
                else
                    m_measurements = MeasurementGroup.GetPossibleMeasurements(null, 0);

                ItemsSource = m_measurements;
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
        /// Saves CurrentItem information into database.
        /// </summary>
        public override void Save()
        {
            //Don't do anything here. This view model is used in SelecteMeasurementUserControl and we do not need to save anything from here.
        }

        /// <summary>
        /// Hanldes <see cref="SearchCommand"/>.
        /// </summary>
        /// <param name="paramter">string value to search for in measurement collection.</param>
        public void Search(object paramter)
        {
            if (paramter != null && !string.IsNullOrEmpty(paramter.ToString()))
            {
                string searchText = paramter.ToString().ToLower();
                ItemsSource = new ObservableCollection<DataModelMeasurement>
                    (m_measurements.Where(m => m.PointTag.ToLower().Contains(searchText) ||
                                            m.SignalReference.ToLower().Contains(searchText) ||
                                            m.Description.ToNonNullString().ToLower().Contains(searchText) ||
                                            m.DeviceAcronym.ToNonNullString().ToLower().Contains(searchText) ||
                                            m.PhasorLabel.ToNonNullString().ToLower().Contains(searchText) ||
                                            m.SignalName.ToNonNullString().ToLower().Contains(searchText) ||
                                            m.SignalAcronym.ToNonNullString().ToLower().Contains(searchText) ||
                                            m.CompanyName.ToNonNullString().ToLower().Contains(searchText) ||
                                            m.CompanyAcronym.ToNonNullString().ToLower().Contains(searchText)
                                            ));
            }
        }

        /// <summary>
        /// Handles <see cref="ShowAllCommand"/>.
        /// </summary>
        public void ShowAll()
        {
            ItemsSource = m_measurements;
        }

        #endregion
    }
}
