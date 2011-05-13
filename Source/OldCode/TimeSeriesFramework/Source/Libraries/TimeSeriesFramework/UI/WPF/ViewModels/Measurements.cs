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
using System.Linq;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="TimeSeriesFramework.UI.DataModels.Measurement"/> collection.
    /// </summary>
    internal class Measurements : PagedViewModelBase<TimeSeriesFramework.UI.DataModels.Measurement, Guid>
    {
        #region [ Members ]

        private Dictionary<int, string> m_historianLookupList;
        private Dictionary<int, string> m_deviceLookupList;
        private Dictionary<int, string> m_signalTypeLookupList;
        private Dictionary<int, string> m_phasorLookupList;
        private int m_deviceID;

        #endregion

        #region [ Constructors ]

        public Measurements(int deviceID, int itemsPerPage, bool autosave = true)
            : base(0, autosave)     // Set ItemsPerPage to zero to avoid load() in the base class.
        {
            m_deviceID = deviceID;
            ItemsPerPage = itemsPerPage;
            m_historianLookupList = Historian.GetLookupList(null);
            m_deviceLookupList = Device.GetLookupList(null);
            m_signalTypeLookupList = SignalType.GetLookupList(null);
            PhasorLookupList = Phasor.GetLookupList(null, m_deviceID);
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
        public Dictionary<int, string> HistorianLookupList
        {
            get
            {
                return m_historianLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of devices defined in the database.
        /// </summary>
        public Dictionary<int, string> DeviceLookupList
        {
            get
            {
                return m_deviceLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of signal types defined in the database.
        /// </summary>
        public Dictionary<int, string> SignalTypeLookupList
        {
            get
            {
                return m_signalTypeLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of phasors defined in the database.
        /// </summary>
        public Dictionary<int, string> PhasorLookupList
        {
            get
            {
                return m_phasorLookupList;
            }
            set
            {
                m_phasorLookupList = value;
                OnPropertyChanged("PhasorLookupList");
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
            return CurrentItem.SignalName;
        }

        /// <summary>
        /// Creates a new instance of <see cref="TimeSeriesFramework.UI.DataModels.Measurement"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            CurrentItem.HistorianID = m_historianLookupList.First().Key;
            CurrentItem.SignalTypeID = m_signalTypeLookupList.First().Key;
            CurrentItem.DeviceID = m_deviceLookupList.First().Key;
            CurrentItem.PhasorSourceIndex = m_phasorLookupList.First().Key;
        }

        /// <summary>
        /// Loads collection of <see cref="TimeSeriesFramework.UI.DataModels.Measurement"/> defined in the database.
        /// </summary>
        public override void Load()
        {
            ItemsSource = TimeSeriesFramework.UI.DataModels.Measurement.Load(null, m_deviceID);
            PhasorLookupList = Phasor.GetLookupList(null, CurrentItem.DeviceID == null ? 0 : (int)CurrentItem.DeviceID);
        }

        /// <summary>
        /// Handles PropertyChanged event on CurrentItem. If DeviceID is changed then get the associated phasors list.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        protected override void m_currentItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.m_currentItem_PropertyChanged(sender, e);

            if (string.Compare(e.PropertyName, "DeviceID", true) == 0)
                PhasorLookupList = Phasor.GetLookupList(null, CurrentItem.DeviceID == null ? 0 : (int)CurrentItem.DeviceID);
        }

        #endregion

    }
}
