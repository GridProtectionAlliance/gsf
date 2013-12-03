//******************************************************************************************************
//  PhasorMeasurements.cs - Gbtc
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
//  05/13/2011 - Magdiel D. Lorenzo
//       Generated original version of source code.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added constructor overload and other changes to handle device specific data.
//  07/12/2011 - Stephen C. Wills
//       Moved phasor-specific code from Measurements in GSF.TimeSeries.UI.WPF to here.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.ViewModels;

namespace GSF.PhasorProtocols.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="GSF.TimeSeries.UI.DataModels.Measurement"/> collection.
    /// </summary>
    internal class PhasorMeasurements : Measurements
    {
        #region [ Members ]

        private readonly bool m_canLoad;
        private Dictionary<int, string> m_deviceLookupList;

        #endregion

        #region [ Constructors ]

        public PhasorMeasurements(int deviceID, int itemsPerPage, bool autosave = true)
            : base(deviceID, itemsPerPage, autosave)     // Set ItemsPerPage to zero to avoid load() in the base class.
        {
            m_canLoad = true;
            // Shows only those devices associated with the current node. 
            m_deviceLookupList = Device.GetLookupList(null, DeviceType.All, true, false);
            Load();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of devices defined in the database.
        /// </summary>
        public Dictionary<int, string> DeviceLookupList
        {
            get
            {
                return m_deviceLookupList;
            }
            set
            {
                m_deviceLookupList = value;
                OnPropertyChanged("DeviceLookupList");
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a new instance of <see cref="GSF.TimeSeries.UI.DataModels.Measurement"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            if (m_deviceLookupList.Count > 0)
                CurrentItem.DeviceID = m_deviceLookupList.First().Key;
        }

        public override void Save()
        {
            if (CurrentItem.HistorianID != null && (int)CurrentItem.HistorianID > 0)
            {
                base.Save();

                try
                {
                    CommonFunctions.SendCommandToService("Invoke " + CommonFunctions.GetRuntimeID("Historian", (int)CurrentItem.HistorianID) + " RefreshMetadata");
                }
                catch (Exception ex)
                {
                    if ((object)ex.InnerException != null)
                        CommonFunctions.LogException(null, "Save " + DataModelName, ex.InnerException);
                    else
                        CommonFunctions.LogException(null, "Save " + DataModelName, ex);
                }
            }
            else
            {
                base.Save();
            }
        }

        /// <summary>
        /// Loads collection of <see cref="GSF.TimeSeries.UI.DataModels.Measurement"/> defined in the database.
        /// </summary>
        public override void Load()
        {
            if (m_canLoad)
                base.Load();
        }

        #endregion

    }
}
