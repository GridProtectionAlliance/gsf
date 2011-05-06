//******************************************************************************************************
//  Devices.cs - Gbtc
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
//  05/06/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Device"/> collection and current selection information for UI.
    /// </summary>
    internal class Devices : PagedViewModelBase<Device, int>
    {
        #region [ Members ]

        // Fields
        private Dictionary<Guid, string> m_nodeLookupList;
        private Dictionary<int, string> m_concentratorDeviceLookupList;
        private Dictionary<int, string> m_companyLookupList;
        private Dictionary<int, string> m_historianLookupList;
        private Dictionary<int, string> m_interconnectionLookupList;
        private Dictionary<int, string> m_protocolLookupList;
        private Dictionary<int, string> m_vendorDeviceLookupList;
        private Dictionary<string, string> m_timezoneLookupList;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="Devices"/> class.
        /// </summary>
        /// <param name="itemsPerPage"></param>
        /// <param name="autoSave"></param>
        public Devices(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_nodeLookupList = Node.GetLookupList(null);
            m_concentratorDeviceLookupList = Device.GetLookupList(null, DeviceType.Concentrator, true);
            m_companyLookupList = Company.GetLookupList(null, true);
            m_historianLookupList = Historian.GetLookupList(null, true, false);
            m_interconnectionLookupList = Interconnection.GetLookupList(null, true);
            m_protocolLookupList = Protocol.GetLookupList(null, true);
            m_vendorDeviceLookupList = VendorDevice.GetLookupList(null, true);
            m_timezoneLookupList = CommonFunctions.GetTimeZones(true);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Node"/> defined in the database.
        /// </summary>
        public Dictionary<Guid, string> NodeLookupList
        {
            get
            {
                return m_nodeLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of concentrator <see cref="Device"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> ConcentratorDeviceLookupList
        {
            get
            {
                return m_concentratorDeviceLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Company"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> CompanyLookupList
        {
            get
            {
                return m_companyLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Historian"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> HistorianLookupList
        {
            get
            {
                return m_historianLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Interconnection"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> InterconnectionLookupList
        {
            get
            {
                return m_interconnectionLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Protocol"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> ProtocolLookupList
        {
            get
            {
                return m_protocolLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="VendorDevice"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> VendorDeviceLookupList
        {
            get
            {
                return m_vendorDeviceLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of system time zones.
        /// </summary>
        public Dictionary<string, string> TimezoneLookupList
        {
            get
            {
                return m_timezoneLookupList;
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return CurrentItem.ID == 0;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override int GetCurrentItemKey()
        {
            return CurrentItem.ID;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.Name;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Historian"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            CurrentItem.NodeID = m_nodeLookupList.First().Key;
            CurrentItem.ParentID = m_concentratorDeviceLookupList.First().Key;
            CurrentItem.CompanyID = m_companyLookupList.First().Key;
            CurrentItem.HistorianID = m_historianLookupList.First().Key;
            CurrentItem.InterconnectionID = m_interconnectionLookupList.First().Key;
            CurrentItem.ProtocolID = m_protocolLookupList.First().Key;
            CurrentItem.VendorDeviceID = m_vendorDeviceLookupList.First().Key;
            CurrentItem.TimeZone = m_timezoneLookupList.First().Key;
        }

        #endregion
    }
}
