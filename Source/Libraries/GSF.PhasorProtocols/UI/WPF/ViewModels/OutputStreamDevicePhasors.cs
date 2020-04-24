//******************************************************************************************************
//  OutputStreamDevicePhasors.cs - Gbtc
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
//  08/16/2011 - Aniket Salver
//       Generated original version of source code.
//  08/31/2011 - Aniket Salver
//       Added few properties which helps in binding.
//  09/16/2011 - Mehulbhai P Thakkar
//       Modified constructor to filter data by output stream device id.
//       Overrode Load() method to apply above metioned filter.
//  09/15/2012 - Aniket Salver 
//          Added paging and sorting technique. 
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
    ///<summary>
    /// Class to hold bindable <see cref="OutputStreamDevicePhasors"/> collection and selected OutputStreamDevicePhasor for UI.
    /// </summary>
    internal class OutputStreamDevicePhasors : PagedViewModelBase<OutputStreamDevicePhasor, int>
    {
        #region [ Members ]

        private int m_outputStreamDeviceID;
        private readonly Dictionary<string, string> m_phaseLookupList;
        private readonly Dictionary<string, string> m_typeLookupList;

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
        /// Gets <see cref="Dictionary{T1,T2}"/> Phase collection of type defined in the database.
        /// </summary>
        public Dictionary<string, string> PhaseLookupList
        {
            get
            {
                return m_phaseLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> Type collection of type defined in the database.
        /// </summary>
        public Dictionary<string, string> TypeLookupList
        {
            get
            {
                return m_typeLookupList;
            }
        }

        /// <summary>
        /// Gets or sets output stream device id.
        /// </summary>
        public int OutputStreamDeviceID
        {
            get
            {
                return m_outputStreamDeviceID;
            }
            set
            {
                m_outputStreamDeviceID = value;
            }
        }

        public override bool CanSave
        {
            get
            {
                return (base.CanSave && !Convert.ToBoolean(IsolatedStorageManager.ReadFromIsolatedStorage("MirrorMode").ToString()));
            }
        }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="OutputStreamDevicePhasors "/> class.
        /// </summary>
        /// <param name="outputStreamDeviceID">ID of the output stream device to filter data.</param>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public OutputStreamDevicePhasors(int outputStreamDeviceID, int itemsPerPage, bool autoSave = true)
            : base(0, autoSave)
        {
            ItemsPerPage = itemsPerPage;
            OutputStreamDeviceID = outputStreamDeviceID;
            Load();

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
        /// Load output stream device phasors.
        /// </summary>
        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            List<int> pageKeys = null;

            try
            {
                if (OnBeforeLoadCanceled())
                    throw new OperationCanceledException("Load was canceled.");

                // Load keys if LoadKeys method exists in data model
                if ((object)ItemsKeys == null)
                {
                    ItemsKeys = OutputStreamDevicePhasor.LoadKeys(null, m_outputStreamDeviceID, SortMember, SortDirection);

                    if ((object)SortSelector != null)
                    {
                        if (SortDirection == "ASC")
                            ItemsKeys = ItemsKeys.OrderBy(SortSelector).ToList();
                        else
                            ItemsKeys = ItemsKeys.OrderByDescending(SortSelector).ToList();
                    }
                }

                // Extract a single page of keys
                pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();

                // If we were able to extract a page of keys, load only that page.
                // Otherwise, load the whole recordset.
                ItemsSource = OutputStreamDevicePhasor.Load(null, pageKeys);

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

        public override void Clear()
        {
            base.Clear();
            CurrentItem.OutputStreamDeviceID = m_outputStreamDeviceID;
        }

        #endregion

    }
}
