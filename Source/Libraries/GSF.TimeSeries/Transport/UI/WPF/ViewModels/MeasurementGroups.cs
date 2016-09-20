//******************************************************************************************************
//  MeasurementGroups.cs - Gbtc
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
//  05/16/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GSF.TimeSeries.Transport.UI.DataModels;
using GSF.TimeSeries.UI;

namespace GSF.TimeSeries.Transport.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="MeasurementGroup"/> collection and selected measurement group for UI.
    /// </summary>
    internal class MeasurementGroups : PagedViewModelBase<MeasurementGroup, int>
    {
        #region [ Members ]

        // Fields
        private bool m_firstLoad;

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="MeasurementGroups"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public MeasurementGroups(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
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
        /// Creates a new instance of <see cref="MeasurementGroup"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            CurrentItem = ItemsSource[0];
        }

        /// <summary>
        /// Loads the records for the associated <see cref="MeasurementGroup"/>.
        /// </summary>
        public override void Load()
        {
            base.Load();

            if (!m_firstLoad && ItemsSource.Count > 1 && IsNewRecord)
                CurrentItem = ItemsSource[1];

            m_firstLoad = true;
        }

        /// <summary>
        /// Saves the record for the associated <see cref="MeasurementGroup"/>.
        /// </summary>
        public override void Save()
        {
            string groupName = CurrentItem.Name;

            base.Save();

            if (IsNewRecord)
                CurrentItem = ItemsSource.FirstOrDefault(group => group.Name == groupName);
        }

        /// <summary>
        /// Adds the measurements identified by the given collection to the measurement group.
        /// </summary>
        /// <param name="measurementIDs">Collection of signal identifiers for the measurements to be added.</param>
        public void AddMeasurement(ICollection<Guid> measurementIDs)
        {
            int result = MeasurementGroup.AddMeasurements(null, CurrentItem.ID, measurementIDs);

            if (result > 0)
                Popup("Measurements added to group successfully", "Add Group Measurements", MessageBoxImage.Information);
            else
                Popup("Selected measurements already exist in group or no measurements were selected.", "Add Group Measurements", MessageBoxImage.Information);
        }

        /// <summary>
        /// Removes the measurements identified by the given collection from the measurement group.
        /// </summary>
        /// <param name="measurementIDs">Collection of signal identifiers for the measurements to be removed.</param>
        public void RemoveMeasurement(ICollection<Guid> measurementIDs)
        {
            string result = MeasurementGroup.RemoveMeasurements(null, CurrentItem.ID, measurementIDs.ToList());

            Popup(result, "Remove Group Measurements", MessageBoxImage.Information);
        }

        #endregion
    }
}
