//******************************************************************************************************
//  MeasurementGroups.cs - Gbtc
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
//  05/16/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="MeasurementGroup"/> collection and selected measurement group for UI.
    /// </summary>
    internal class MeasurementGroups : PagedViewModelBase<MeasurementGroup, int>
    {
        #region [ Members ]

        // Fields

        private RelayCommand m_addMeasurementCommand;
        private RelayCommand m_removeMeasurementCommand;

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
        /// Gets <see cref="ICommand"/> for add measurements operation.
        /// </summary>
        public ICommand AddMeasurementCommand
        {
            get
            {
                if (m_addMeasurementCommand == null)
                    m_addMeasurementCommand = new RelayCommand(AddMeasurement, (param) => CanSave);

                return m_addMeasurementCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to remove measurements operation.
        /// </summary>
        public ICommand RemoveMeasurementCommand
        {
            get
            {
                if (m_removeMeasurementCommand == null)
                    m_removeMeasurementCommand = new RelayCommand(RemoveMeasurement, (param) => CanSave);

                return m_removeMeasurementCommand;
            }
        }

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
        /// Handles <see cref="AddMeasurementCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurements to be added.</param>
        private void AddMeasurement(object parameter)
        {
            ObservableCollection<TimeSeriesFramework.UI.DataModels.Measurement> measurementsToBeAdded = (ObservableCollection<TimeSeriesFramework.UI.DataModels.Measurement>)parameter;

            if (measurementsToBeAdded.Count > 0)
            {
                List<Guid> measurementIDs = new List<Guid>();

                foreach (TimeSeriesFramework.UI.DataModels.Measurement measurement in measurementsToBeAdded)
                {
                    if (!CurrentItem.CurrentMeasurements.ContainsKey(measurement.SignalID) && measurement.Selected)
                        measurementIDs.Add(measurement.SignalID);
                }

                if (measurementIDs.Count > 0)
                {
                    string result = MeasurementGroup.AddMeasurements(null, CurrentItem.ID, measurementIDs);
                    Popup(result, "Add Group Measurements", MessageBoxImage.Information);
                }
                else
                {
                    Popup("Selected measurements already exists in group or no measurements were selected.", "Add Group Measurements", MessageBoxImage.Information);
                }

                CurrentItem.CurrentMeasurements = MeasurementGroup.GetCurrentMeasurements(null, CurrentItem.ID);
                CurrentItem.PossibleMeasurements = MeasurementGroup.GetPossibleMeasurements(null, CurrentItem.ID);
            }
        }

        /// <summary>
        /// Handles <see cref="RemoveMeasurementCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurements to be removed.</param>
        private void RemoveMeasurement(object parameter)
        {
            ObservableCollection<object> measurementsToBeRemoved = (ObservableCollection<object>)parameter;

            if (measurementsToBeRemoved.Count > 0)
            {
                List<Guid> measurementIDs = new List<Guid>();

                foreach (object item in measurementsToBeRemoved)
                    measurementIDs.Add(((KeyValuePair<Guid, string>)item).Key);

                string result = MeasurementGroup.RemoveMeasurements(null, CurrentItem.ID, measurementIDs);

                Popup(result, "Remove Group Measurements", MessageBoxImage.Information);

                CurrentItem.CurrentMeasurements = MeasurementGroup.GetCurrentMeasurements(null, CurrentItem.ID);
                CurrentItem.PossibleMeasurements = MeasurementGroup.GetPossibleMeasurements(null, CurrentItem.ID);
            }
        }

        #endregion
    }
}
