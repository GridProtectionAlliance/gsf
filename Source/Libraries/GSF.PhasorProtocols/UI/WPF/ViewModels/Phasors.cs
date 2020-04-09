//******************************************************************************************************
//  Phasors.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/12/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added constructor overload and other changes to handle device specific data.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Phasor"/> collection and current selection information for UI.
    /// </summary>
    internal class Phasors : PagedViewModelBase<Phasor, int>
    {
        #region [ Members ]

        // Fields
        private readonly Dictionary<string, string> m_phaseLookupList;
        private readonly Dictionary<string, string> m_typeLookupList;
        private readonly int m_deviceID;
        

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="Phasors"/> class.
        /// </summary>
        /// <param name="deviceID">ID of the device to filter phasors.</param>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public Phasors(int deviceID, int itemsPerPage, bool autoSave = true)
            : base(0, autoSave)
        {
            ItemsPerPage = itemsPerPage;
            m_deviceID = deviceID;

            m_phaseLookupList = new Dictionary<string, string>();
            m_phaseLookupList.Add("+", "Positive Sequence");
            m_phaseLookupList.Add("-", "Negative Sequence");
            m_phaseLookupList.Add("0", "Zero Sequence");
            m_phaseLookupList.Add("A", "Phase A");
            m_phaseLookupList.Add("B", "Phase B");
            m_phaseLookupList.Add("C", "Phase C");
            m_phaseLookupList.Add("L", "Line-to-line");
            m_phaseLookupList.Add("N", "Neutral");

            m_typeLookupList = new Dictionary<string, string>();
            m_typeLookupList.Add("V", "Voltage");
            m_typeLookupList.Add("I", "Current");

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
        /// Loads collection of <see cref="Phasor"/> defined in the database for a <see cref="Device"/>.
        /// </summary>
        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            List<int> pageKeys = null;

            try
            {
                if (OnBeforeLoadCanceled())
                    throw new OperationCanceledException("Load was canceled.");

                if ((object)ItemsKeys == null && m_deviceID > 0)
                {
                    ItemsKeys = Phasor.LoadKeys(null, m_deviceID, SortMember, SortDirection);

                    if ((object)SortSelector != null)
                    {
                        if (SortDirection == "ASC")
                            ItemsKeys = ItemsKeys.OrderBy(SortSelector).ToList();
                        else
                            ItemsKeys = ItemsKeys.OrderByDescending(SortSelector).ToList();
                    }
                }

                if ((object)ItemsKeys != null)
                    pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();

                ItemsSource = Phasor.Load(null, pageKeys);

                if (ItemsSource != null && ItemsSource.Count == 0 && CurrentItem != null)
                    CurrentItem.DeviceID = m_deviceID;

                OnLoaded();
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
        /// Creates a new instance of <see cref="Phasor"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            CurrentItem.DeviceID = m_deviceID;
        }

        #endregion
    }
}
