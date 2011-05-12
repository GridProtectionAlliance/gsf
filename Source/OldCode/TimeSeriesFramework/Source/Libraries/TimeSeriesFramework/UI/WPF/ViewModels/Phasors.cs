//******************************************************************************************************
//  Phasors.cs - Gbtc
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
//  05/12/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Phasor"/> collection and current selection information for UI.
    /// </summary>
    internal class Phasors : PagedViewModelBase<Phasor, int>
    {

        #region [ Members ]

        private Dictionary<string, string> m_phaseLookupList;
        private Dictionary<string, string> m_typeLookupList;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="Phasors"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public Phasors(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_phaseLookupList = new Dictionary<string, string>();
            m_phaseLookupList.Add("+", "Positive Sequence");
            m_phaseLookupList.Add("-", "Negative Sequence");
            m_phaseLookupList.Add("0", "Zero Sequence");
            m_phaseLookupList.Add("A", "Phase A");
            m_phaseLookupList.Add("B", "Phase B");
            m_phaseLookupList.Add("C", "Phase C");

            m_typeLookupList = new Dictionary<string, string>();
            m_typeLookupList.Add("V", "Voltage");
            m_typeLookupList.Add("I", "Current");
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
                return CurrentItem.ID == 0;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of phase defined in the database.
        /// </summary>
        public Dictionary<string, string> PhaseLookupList
        {
            get
            {
                return m_phaseLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of type defined in the database.
        /// </summary>
        public Dictionary<string, string> TypeLookupList
        {
            get
            {
                return m_typeLookupList;
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
            return CurrentItem.Label;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Historian"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
        }

        #endregion
        
    }
}
