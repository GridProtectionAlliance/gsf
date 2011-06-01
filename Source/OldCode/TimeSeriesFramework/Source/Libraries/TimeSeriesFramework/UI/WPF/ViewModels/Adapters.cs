//******************************************************************************************************
//  Adapters.cs - Gbtc
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
using System.Linq;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Adapter"/> collection and current selection information for UI.
    /// </summary>
    internal class Adapters : PagedViewModelBase<Adapter, int>
    {
        #region [ Members ]

        // Fields
        private Dictionary<Guid, string> m_nodeLookupList;
        private AdapterType m_adapterType;

        #endregion

        #region [ Properties ]

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

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="Adapters"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        /// <param name="adapterType"><see cref="AdapterType"/> to determine type.</param>
        public Adapters(int itemsPerPage, AdapterType adapterType, bool autoSave = true)
            : base(0, autoSave) // Set items per page to zero to avoid load in the base class.
        {
            m_nodeLookupList = Node.GetLookupList(null);
            ItemsPerPage = itemsPerPage;
            m_adapterType = adapterType;
            Load();
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
            return CurrentItem.AdapterName;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Historian"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            if (m_nodeLookupList.Count > 0)
                CurrentItem.NodeID = m_nodeLookupList.First().Key;
        }

        /// <summary>
        /// Loads collection of <see cref="Adapter"/> defined in the database.
        /// </summary>
        public override void Load()
        {
            ItemsSource = Adapter.Load(null, m_adapterType);
        }

        /// <summary>
        /// Deletes associated <see cref="Adapter"/> record.
        /// </summary>
        public override void Delete()
        {
            Adapter.Delete(null, m_adapterType, GetCurrentItemKey());
        }

        #endregion
    }
}
