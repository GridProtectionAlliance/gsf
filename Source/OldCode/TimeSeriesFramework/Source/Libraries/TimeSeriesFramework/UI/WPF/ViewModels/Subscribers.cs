//******************************************************************************************************
//  Subscribers.cs - Gbtc
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
//  05/20/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  05/20/2011 - Mehulbhai P Thakkar
//       Added commands and other logic to add or remove measurements and measurement groups.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI.DataModels;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Subscriber"/> collection and current selection information for UI.
    /// </summary>
    internal class Subscribers : PagedViewModelBase<Subscriber, Guid>
    {
        #region [ Members ]

        // Fields
        private Dictionary<Guid, string> m_nodeLookupList;
        private RelayCommand m_addAllowedMeasurementCommand;
        private RelayCommand m_removeAllowedMeasurementCommand;
        private RelayCommand m_addDeniedMeasurementCommand;
        private RelayCommand m_removeDeniedMeasurementCommand;
        private RelayCommand m_addAllowedMeasurementGroupCommand;
        private RelayCommand m_removeAllowedMeasurementGroupCommand;
        private RelayCommand m_addDeniedMeasurementGroupCommand;
        private RelayCommand m_removeDeniedMeasurementGroupCommand;

        // Delegates

        /// <summary>
        /// Method signature for a function which handles MeasurementsAdded event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void OnMeasurementsAdded(object sender, RoutedEventArgs e);

        // Event

        /// <summary>
        /// Event to notify subscribers that measurements have been added to the group.
        /// </summary>
        public event OnMeasurementsAdded MeasurementsAdded;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="Subscribers"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public Subscribers(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_nodeLookupList = Node.GetLookupList(null);
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
                return CurrentItem.ID == Guid.Empty;
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

        /// <summary>
        /// Gets <see cref="ICommand"/> to add allowed measurements.
        /// </summary>
        public ICommand AddAllowedMeasurementCommand
        {
            get
            {
                if (m_addAllowedMeasurementCommand == null)
                    m_addAllowedMeasurementCommand = new RelayCommand(AddAllowedMeasurement, (param) => CanSave);

                return m_addAllowedMeasurementCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to remove allowed measurements.
        /// </summary>
        public ICommand RemoveAllowedMeasurementCommand
        {
            get
            {
                if (m_removeAllowedMeasurementCommand == null)
                    m_removeAllowedMeasurementCommand = new RelayCommand(RemoveAllowedMeasurement, (param) => CanSave);

                return m_removeAllowedMeasurementCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to add denied measurements.
        /// </summary>
        public ICommand AddDeniedMeasurementCommand
        {
            get
            {
                if (m_addDeniedMeasurementCommand == null)
                    m_addDeniedMeasurementCommand = new RelayCommand(AddDeniedMeasurement, (param) => CanSave);

                return m_addDeniedMeasurementCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to remove denied measurements.
        /// </summary>
        public ICommand RemoveDeniedMeasurementCommand
        {
            get
            {
                if (m_removeDeniedMeasurementCommand == null)
                    m_removeDeniedMeasurementCommand = new RelayCommand(RemoveDeniedMeasurement, (param) => CanSave);

                return m_removeDeniedMeasurementCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to add allowed measurement groups.
        /// </summary>
        public ICommand AddAllowedMeasurementGroupCommand
        {
            get
            {
                if (m_addAllowedMeasurementGroupCommand == null)
                    m_addAllowedMeasurementGroupCommand = new RelayCommand(AddAllowedMeasurementGroup, (param) => CanSave);

                return m_addAllowedMeasurementGroupCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to remove allowed measurement groups.
        /// </summary>
        public ICommand RemoveAllowedMeasurementGroupCommand
        {
            get
            {
                if (m_removeAllowedMeasurementGroupCommand == null)
                    m_removeAllowedMeasurementGroupCommand = new RelayCommand(RemoveAllowedMeasurementGroup, (param) => CanSave);

                return m_removeAllowedMeasurementGroupCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to add denied measurement groups.
        /// </summary>
        public ICommand AddDeniedMeasurementGroupCommand
        {
            get
            {
                if (m_addDeniedMeasurementGroupCommand == null)
                    m_addDeniedMeasurementGroupCommand = new RelayCommand(AddDeniedMeasurementGroup, (param) => CanSave);

                return m_addDeniedMeasurementGroupCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to remove denied measurement groups.
        /// </summary>
        public ICommand RemoveDeniedMeasurementGroupCommand
        {
            get
            {
                if (m_removeDeniedMeasurementGroupCommand == null)
                    m_removeDeniedMeasurementGroupCommand = new RelayCommand(RemoveDeniedMeasurementGroup, (param) => CanSave);

                return m_removeDeniedMeasurementGroupCommand;
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
        /// Creates a new instance of <see cref="Subscriber"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            if (m_nodeLookupList.Count > 0)
                CurrentItem.NodeID = m_nodeLookupList.First().Key;
        }

        /// <summary>
        /// Handles <see cref="AddAllowedMeasurementCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurements to be allowed.</param>
        private void AddAllowedMeasurement(object parameter)
        {
            ObservableCollection<TimeSeriesFramework.UI.DataModels.Measurement> measurementsToBeAdded = (ObservableCollection<TimeSeriesFramework.UI.DataModels.Measurement>)parameter;

            if (measurementsToBeAdded.Count > 0)
            {
                List<Guid> measurementIDs = new List<Guid>();

                foreach (TimeSeriesFramework.UI.DataModels.Measurement measurement in measurementsToBeAdded)
                {
                    if (!CurrentItem.AllowedMeasurements.ContainsKey(measurement.SignalID) &&
                        !CurrentItem.DeniedMeasurements.ContainsKey(measurement.SignalID) && measurement.Selected)
                        measurementIDs.Add(measurement.SignalID);
                }

                if (measurementIDs.Count > 0)
                {
                    string result = Subscriber.AddMeasurements(null, CurrentItem.ID, measurementIDs, true);
                    Popup(result, "Allow Measurements", MessageBoxImage.Information);
                }
                else
                {
                    Popup("Selected measurements already exists or no measurements were selected.", "Allow Measurements", MessageBoxImage.Information);
                }

                if (MeasurementsAdded != null)
                    MeasurementsAdded(this, null);

                CurrentItem.AllowedMeasurements = Subscriber.GetAllowedMeasurements(null, CurrentItem.ID);
                CurrentItem.AvailableMeasurements = Subscriber.GetAvailableMeasurements(null, CurrentItem.ID);
            }
        }

        /// <summary>
        /// Handles <see cref="RemoveAllowedMeasurementCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurements to be disallowed.</param>
        private void RemoveAllowedMeasurement(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items.Count > 0)
            {
                List<Guid> measurementIDs = new List<Guid>();

                foreach (object item in items)
                    measurementIDs.Add(((KeyValuePair<Guid, string>)item).Key);

                string result = Subscriber.RemoveMeasurements(null, CurrentItem.ID, measurementIDs);

                Popup(result, "Remove Measurements", MessageBoxImage.Information);

                CurrentItem.AllowedMeasurements = Subscriber.GetAllowedMeasurements(null, CurrentItem.ID);
            }
        }

        /// <summary>
        /// Handles <see cref="AddDeniedMeasurementCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurements to be denied.</param>
        private void AddDeniedMeasurement(object parameter)
        {
            ObservableCollection<TimeSeriesFramework.UI.DataModels.Measurement> measurementsToBeAdded = (ObservableCollection<TimeSeriesFramework.UI.DataModels.Measurement>)parameter;

            if (measurementsToBeAdded.Count > 0)
            {
                List<Guid> measurementIDs = new List<Guid>();

                foreach (TimeSeriesFramework.UI.DataModels.Measurement measurement in measurementsToBeAdded)
                {
                    if (!CurrentItem.AllowedMeasurements.ContainsKey(measurement.SignalID) &&
                        !CurrentItem.DeniedMeasurements.ContainsKey(measurement.SignalID) && measurement.Selected)
                        measurementIDs.Add(measurement.SignalID);
                }

                if (measurementIDs.Count > 0)
                {
                    string result = Subscriber.AddMeasurements(null, CurrentItem.ID, measurementIDs, false);
                    Popup(result, "Allow Measurements", MessageBoxImage.Information);
                }
                else
                {
                    Popup("Selected measurements already exists or no measurements were selected.", "Allow Measurements", MessageBoxImage.Information);
                }

                if (MeasurementsAdded != null)
                    MeasurementsAdded(this, null);

                CurrentItem.DeniedMeasurements = Subscriber.GetDeniedMeasurements(null, CurrentItem.ID);
                CurrentItem.AvailableMeasurements = Subscriber.GetAvailableMeasurements(null, CurrentItem.ID);
            }
        }

        /// <summary>
        /// Handles <see cref="RemoveDeniedMeasurementCommand"/>
        /// </summary>
        /// <param name="parameter">Collection of measurements to be removed.</param>
        private void RemoveDeniedMeasurement(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items.Count > 0)
            {
                List<Guid> measurementIDs = new List<Guid>();

                foreach (object item in items)
                    measurementIDs.Add(((KeyValuePair<Guid, string>)item).Key);

                string result = Subscriber.RemoveMeasurements(null, CurrentItem.ID, measurementIDs);

                Popup(result, "Remove Measurements", MessageBoxImage.Information);

                CurrentItem.DeniedMeasurements = Subscriber.GetDeniedMeasurements(null, CurrentItem.ID);
            }
        }

        /// <summary>
        /// Handles <see cref="AddAllowedMeasurementGroupCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurement groups to be added.</param>
        private void AddAllowedMeasurementGroup(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items.Count > 0)
            {
                List<int> measurementGroupIDs = new List<int>();

                foreach (object item in items)
                    measurementGroupIDs.Add(((KeyValuePair<int, string>)item).Key);

                string result = Subscriber.AddMeasurementGroups(null, CurrentItem.ID, measurementGroupIDs, true);

                Popup(result, "Allow Measurement Groups", MessageBoxImage.Information);

                CurrentItem.AllowedMeasurementGroups = Subscriber.GetAllowedMeasurementGroups(null, CurrentItem.ID);
                CurrentItem.AvailableMeasurementGroups = Subscriber.GetAvailableMeasurementGroups(null, CurrentItem.ID);
            }
        }

        /// <summary>
        /// Handles <see cref="RemoveAllowedMeasurementGroupCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurement groups to be removed.</param>
        private void RemoveAllowedMeasurementGroup(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items.Count > 0)
            {
                List<int> measurementGroupIDs = new List<int>();

                foreach (object item in items)
                    measurementGroupIDs.Add(((KeyValuePair<int, string>)item).Key);

                string result = Subscriber.RemoveMeasurementGroups(null, CurrentItem.ID, measurementGroupIDs);

                Popup(result, "Remove Measurement Groups", MessageBoxImage.Information);

                CurrentItem.AllowedMeasurementGroups = Subscriber.GetAllowedMeasurementGroups(null, CurrentItem.ID);
                CurrentItem.AvailableMeasurementGroups = Subscriber.GetAvailableMeasurementGroups(null, CurrentItem.ID);
            }
        }

        /// <summary>
        /// Handles <see cref="AddDeniedMeasurementGroupCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurement groups to be added.</param>
        private void AddDeniedMeasurementGroup(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items.Count > 0)
            {
                List<int> measurementGroupIDs = new List<int>();

                foreach (object item in items)
                    measurementGroupIDs.Add(((KeyValuePair<int, string>)item).Key);

                string result = Subscriber.AddMeasurementGroups(null, CurrentItem.ID, measurementGroupIDs, false);

                Popup(result, "Deny Measurement Groups", MessageBoxImage.Information);

                CurrentItem.AvailableMeasurementGroups = Subscriber.GetAvailableMeasurementGroups(null, CurrentItem.ID);
                CurrentItem.DeniedMeasurementGroups = Subscriber.GetDeniedMeasurementGroups(null, CurrentItem.ID);
            }
        }

        /// <summary>
        /// Hanldes <see cref="RemoveDeniedMeasurementGroupCommand"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurement groups to be removed.</param>
        private void RemoveDeniedMeasurementGroup(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items.Count > 0)
            {
                List<int> measurementGroupIDs = new List<int>();

                foreach (object item in items)
                    measurementGroupIDs.Add(((KeyValuePair<int, string>)item).Key);

                string result = Subscriber.RemoveMeasurementGroups(null, CurrentItem.ID, measurementGroupIDs);

                Popup(result, "Remove Measurement Groups", MessageBoxImage.Information);

                CurrentItem.AvailableMeasurementGroups = Subscriber.GetAvailableMeasurementGroups(null, CurrentItem.ID);
                CurrentItem.DeniedMeasurementGroups = Subscriber.GetDeniedMeasurementGroups(null, CurrentItem.ID);
            }
        }

        #endregion
    }
}
