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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GSF.Collections;
using GSF.Data;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport.UI.DataModels;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using DataModelMeasurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.TimeSeries.Transport.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Subscriber"/> collection and current selection information for UI.
    /// </summary>
    internal class Subscribers : PagedViewModelBase<Subscriber, Guid>
    {
        #region [ Members ]

        // Fields
        private DataSet m_subscriberPermissionsDataSet;
        private ICollection<DataModelMeasurement> m_currentAvailableMeasurementsPage;
        private object m_availableMeasurementsLock;

        private RelayCommand m_addAllowedMeasurementGroupCommand;
        private RelayCommand m_removeAllowedMeasurementGroupCommand;
        private RelayCommand m_addDeniedMeasurementGroupCommand;
        private RelayCommand m_removeDeniedMeasurementGroupCommand;
        private DispatcherTimer m_refreshTimer;
        private readonly SubscriberStatusQuery m_subscriberStatusQuery;
        private readonly object m_subscriberStatusQueryLock;
        private readonly List<Guid> m_subscriberIDs;
        private SecurityMode m_securityMode;
        private byte[] m_remoteCertificateData;

        // Delegates

        /// <summary>
        /// Method signature for a function which handles MeasurementsAdded event.
        /// </summary>
        /// <param name="sender">Source of an event.</param>
        /// <param name="e">Event arguments.</param>
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
            m_subscriberIDs = new List<Guid>();
            m_subscriberStatusQueryLock = new object();
            m_subscriberStatusQuery = new SubscriberStatusQuery();
            m_subscriberStatusQuery.SubscriberStatuses += m_subscriberStatusQuery_SubscriberStatuses;
            PropertyChanged += PropertyChangedHandler;
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
        /// Gets or sets measurements with Subscribed flag set to true.
        /// </summary>
        public ICollection<DataModelMeasurement> CurrentAvailableMeasurementsPage
        {
            get
            {
                lock (m_availableMeasurementsLock)
                {
                    // Return a copy of the measurements since we can't request consumer to lock the collection
                    return new ObservableCollection<DataModelMeasurement>(m_currentAvailableMeasurementsPage ?? new DataModelMeasurement[0]);
                }
            }
            set
            {
                lock (m_availableMeasurementsLock)
                {
                    m_currentAvailableMeasurementsPage = value;
                }

                UpdateEffectivePermissions();
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

        /// <summary>
        /// Gets or sets the security mode used by the data Subscriber.
        /// </summary>
        public SecurityMode SecurityMode
        {
            get
            {
                return m_securityMode;
            }
            set
            {
                m_securityMode = value;
                OnPropertyChanged("SecurityMode");
                OnPropertyChanged("TransportLayerSecuritySelected");
                OnPropertyChanged("GatewaySecuritySelected");
            }
        }

        /// <summary>
        /// Gets the flag that indicates whether the user has selected TLS as the security mode.
        /// </summary>
        public bool TransportLayerSecuritySelected
        {
            get
            {
                return m_securityMode == SecurityMode.TLS;
            }
        }

        /// <summary>
        /// Gets the flag that indicates whether the user has selected Gateway security.
        /// </summary>
        public bool GatewaySecuritySelected
        {
            get
            {
                return m_securityMode == SecurityMode.Gateway;
            }
        }

        /// <summary>
        /// Gets or sets the remote certificate file data that was imported from the subscription request file.
        /// </summary>
        public byte[] RemoteCertificateData
        {
            get
            {
                return m_remoteCertificateData;
            }
            set
            {
                m_remoteCertificateData = value;
            }
        }

        #endregion

        #region [ Methods ]

        private void m_subscriberStatusQuery_SubscriberStatuses(object sender, EventArgs<Dictionary<Guid, Tuple<bool, string>>> e)
        {
            Dictionary<Guid, Tuple<bool, string>> subscriberStatuses = e.Argument;

            lock (m_subscriberStatusQueryLock)
            {
                foreach (Subscriber subscriber in ItemsSource)
                {
                    Tuple<bool, string> status;
                    if (subscriberStatuses.TryGetValue(subscriber.ID, out status))
                    {
                        subscriber.StatusColor = status.Item1 ? "green" : "red";
                        subscriber.Version = status.Item2;
                    }
                }
            }

            //OnPropertyChanged("ItemsSource");
        }

        /// <summary>
        /// Start refresh timer.
        /// </summary>
        public void StartTimer()
        {
            try
            {
                if (m_refreshTimer == null)
                {
                    m_refreshTimer = new DispatcherTimer();
                    m_refreshTimer.Interval = TimeSpan.FromSeconds(10);
                    m_refreshTimer.Tick += m_refreshTimer_Tick;
                }

                foreach (Subscriber subscriber in ItemsSource)
                    m_subscriberIDs.Add(subscriber.ID);

                m_subscriberStatusQuery.RequestSubscriberStatus(m_subscriberIDs);
                m_refreshTimer.Start();
            }
            catch (Exception ex)
            {
                Popup("Failed to start data refresh timer. Please reload page." + Environment.NewLine + ex.Message, "Refresh Data", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Stop refresh timer.
        /// </summary>
        public void StopTimer()
        {
            try
            {
                if (m_refreshTimer != null)
                    m_refreshTimer.Stop();
            }
            finally
            {
                m_refreshTimer = null;
            }
        }

        private void m_refreshTimer_Tick(object sender, EventArgs e)
        {
            lock (m_subscriberStatusQueryLock)
            {
                m_subscriberStatusQuery.RequestSubscriberStatus(m_subscriberIDs);
            }
        }

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
        /// Initialization to be done before the initial call to <see cref="PagedViewModelBase{TDataModel,TPrimaryKey}.Load"/>.
        /// </summary>
        public override void Initialize()
        {
            m_availableMeasurementsLock = new object();
            base.Initialize();
            LoadPermissionsDataSet();
        }

        /// <summary>
        /// Saves the record for the associated <see cref="Subscriber"/>.
        /// </summary>
        public override void Save()
        {
            base.Save();
            LoadPermissionsDataSet();
        }

        /// <summary>
        /// Handles <see cref="AddAllowedMeasurements"/>.
        /// </summary>
        /// <param name="measurementIDs">Collection of measurements to be allowed.</param>
        public void AddAllowedMeasurements(ICollection<Guid> measurementIDs)
        {
            List<Guid> filteredIDs = measurementIDs
                .Where(id => !CurrentItem.AllowedMeasurements.ContainsKey(id))
                .Where(id => !CurrentItem.DeniedMeasurements.ContainsKey(id))
                .ToList();

            if (filteredIDs.Count > 0)
            {
                string result = Subscriber.AddMeasurements(null, CurrentItem.ID, filteredIDs, true);
                //Popup(result, "Allow Measurements", MessageBoxImage.Information);
                DisplayStatusMessage(result);

                // Log changes to event log
                int currentCount = CurrentItem.AllowedMeasurements.Count;
                int addedCount = filteredIDs.Count;
                CommonFunctions.LogEvent(string.Format("Allowed {0} new measurements in addition to the {1} already allowed for subscriber {2} [{3}] for a total of {4} now allowed. See audit log for complete details.\r\n\r\nAdded measurements: {5}", addedCount, currentCount, CurrentItem.Name.ToNonNullNorWhiteSpace("Subscriber"), CurrentItem.Acronym, currentCount + addedCount, ShortFormattedList(filteredIDs.Select(GetMeasurementName))), 0);
            }
            else
            {
                Popup("Selected measurements already exists or no measurements were selected.", "Allow Measurements", MessageBoxImage.Information);
            }

            if (MeasurementsAdded != null)
                MeasurementsAdded(this, null);

            CurrentItem.AllowedMeasurements = Subscriber.GetAllowedMeasurements(null, CurrentItem.ID);
            CurrentItem.AvailableMeasurements = Subscriber.GetAvailableMeasurements(null, CurrentItem.ID);

            try
            {
                CommonFunctions.SendCommandToService("ReloadConfig");
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "", ex);
            }

            LoadPermissionsDataSet();
        }

        /// <summary>
        /// Handles <see cref="RemoveAllowedMeasurements"/>.
        /// </summary>
        /// <param name="measurementIDs">Collection of measurements to be disallowed.</param>
        public void RemoveAllowedMeasurements(ICollection<Guid> measurementIDs)
        {
            string result = Subscriber.RemoveMeasurements(null, CurrentItem.ID, measurementIDs.ToList());

            //Popup(result, "Remove Measurements", MessageBoxImage.Information);
            DisplayStatusMessage(result);

            // Log changes to event log
            int currentCount = CurrentItem.AllowedMeasurements.Count;
            int removedCount = measurementIDs.Count;
            CommonFunctions.LogEvent(string.Format("Removed {0} measurements from the {1} currently allowed for subscriber {2} [{3}] for a total of {4} now allowed. See audit log for complete details.\r\n\r\nRemoved measurements: {5}.", removedCount, currentCount, CurrentItem.Name.ToNonNullNorWhiteSpace("Subscriber"), CurrentItem.Acronym, currentCount - removedCount, ShortFormattedList(measurementIDs.Select(GetMeasurementName))), 0);

            CurrentItem.AllowedMeasurements = Subscriber.GetAllowedMeasurements(null, CurrentItem.ID);

            try
            {
                CommonFunctions.SendCommandToService("ReloadConfig");
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "", ex);
            }

            LoadPermissionsDataSet();
        }

        /// <summary>
        /// Handles <see cref="AddDeniedMeasurements"/>.
        /// </summary>
        /// <param name="measurementIDs">Collection of measurements to be denied.</param>
        public void AddDeniedMeasurements(ICollection<Guid> measurementIDs)
        {
            List<Guid> filteredIDs = measurementIDs
                .Where(id => !CurrentItem.AllowedMeasurements.ContainsKey(id))
                .Where(id => !CurrentItem.DeniedMeasurements.ContainsKey(id))
                .ToList();

            if (filteredIDs.Count > 0)
            {
                string result = Subscriber.AddMeasurements(null, CurrentItem.ID, filteredIDs, false);
                //Popup(result, "Allow Measurements", MessageBoxImage.Information);
                DisplayStatusMessage(result);

                // Log changes to event log
                int currentCount = CurrentItem.AllowedMeasurements.Count;
                int addedCount = filteredIDs.Count;
                CommonFunctions.LogEvent(string.Format("Denied {0} new measurements in addition to the {1} already denied for subscriber {2} [{3}] for a total of {4} now denied. See audit log for complete details.\r\n\r\nDenied measurements: {5}", addedCount, currentCount, CurrentItem.Name.ToNonNullNorWhiteSpace("Subscriber"), CurrentItem.Acronym, currentCount + addedCount, ShortFormattedList(filteredIDs.Select(GetMeasurementName))), 0);
            }
            else
            {
                Popup("Selected measurements already exists or no measurements were selected.", "Allow Measurements", MessageBoxImage.Information);
            }

            if (MeasurementsAdded != null)
                MeasurementsAdded(this, null);

            CurrentItem.DeniedMeasurements = Subscriber.GetDeniedMeasurements(null, CurrentItem.ID);
            CurrentItem.AvailableMeasurements = Subscriber.GetAvailableMeasurements(null, CurrentItem.ID);

            try
            {
                CommonFunctions.SendCommandToService("ReloadConfig");
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "", ex);
            }

            LoadPermissionsDataSet();
        }

        /// <summary>
        /// Handles <see cref="RemoveDeniedMeasurements"/>
        /// </summary>
        /// <param name="measurementIDs">Collection of measurements to be removed.</param>
        public void RemoveDeniedMeasurements(ICollection<Guid> measurementIDs)
        {
            string result = Subscriber.RemoveMeasurements(null, CurrentItem.ID, measurementIDs.ToList());

            //Popup(result, "Remove Measurements", MessageBoxImage.Information);
            DisplayStatusMessage(result);

            // Log changes to event log
            int currentCount = CurrentItem.AllowedMeasurements.Count;
            int removedCount = measurementIDs.Count;
            CommonFunctions.LogEvent(string.Format("Removed {0} measurements from the {1} currently denied for subscriber {2} [{3}] for a total of {4} now denied. See audit log for complete details.\r\n\r\nRemoved measurements: {5}.", removedCount, currentCount, CurrentItem.Name.ToNonNullNorWhiteSpace("Subscriber"), CurrentItem.Acronym, currentCount - removedCount, ShortFormattedList(measurementIDs.Select(GetMeasurementName))), 0);

            CurrentItem.DeniedMeasurements = Subscriber.GetDeniedMeasurements(null, CurrentItem.ID);

            try
            {
                CommonFunctions.SendCommandToService("ReloadConfig");
            }
            catch (Exception ex)
            {
                CommonFunctions.LogException(null, "", ex);
            }

            LoadPermissionsDataSet();
        }

        /// <summary>
        /// Determines if subscriber has rights to specified <paramref name="signalID"/>.
        /// </summary>
        /// <param name="signalID"><see cref="Guid"/> signal ID to lookup.</param>
        /// <returns><c>true</c> if subscriber has rights to specified <paramref name="signalID"/>; otherwise <c>false</c>.</returns>
        public bool SubscriberHasRights(Guid signalID)
        {
            // TODO: Abstract publisher ACL logic -- except subscriber enabled flag does not need to be checked
            const string FilterRegex = @"(ALLOW|DENY)\s+WHERE\s+([^;]*)";

            DataRow subscriber;
            DataRow[] subscriberMeasurementGroups;

            IEnumerable<bool> explicitAuthorizationFlags;
            IEnumerable<bool> explicitGroupAuthorizationFlags;
            IEnumerable<bool> implicitFilterAuthorizationFlags;

            bool explicitlyAuthorized = false;
            bool explicitlyAuthorizedByGroup = false;
            bool implicitlyAuthorizedByFilter = false;

            // This is different from the ACL logic used by the system --
            // because we are only calculating effective permissions to aid with configuration,
            // we do not care whether the Subscriber is enabled
            subscriber = m_subscriberPermissionsDataSet.Tables["Subscribers"].Select(string.Format("ID = '{0}'", CurrentItem.ID)).FirstOrDefault();

            // If subscriber has been disabled or removed
            // from the list of valid subscribers,
            // they no longer have rights to any signals
            if ((object)subscriber == null)
                return false;

            // Look up explicitly defined individual measurements
            explicitAuthorizationFlags = m_subscriberPermissionsDataSet.Tables["SubscriberMeasurements"].Select(string.Format("SubscriberID = '{0}' AND SignalID = '{1}'", CurrentItem.ID, signalID))
                .Select(measurement => measurement["Allowed"].ToNonNullString("0").ParseBoolean());

            foreach (bool flag in explicitAuthorizationFlags)
            {
                if (flag)
                    explicitlyAuthorized = true;
                else
                    return false;
            }

            if (explicitlyAuthorized)
                return true;

            // Look up explicitly defined group based measurements
            subscriberMeasurementGroups = m_subscriberPermissionsDataSet.Tables["SubscriberMeasurementGroups"].Select(string.Format("SubscriberID = '{0}'", CurrentItem.ID));

            explicitGroupAuthorizationFlags = subscriberMeasurementGroups
                .Where(subscriberMeasurementGroup => m_subscriberPermissionsDataSet.Tables["MeasurementGroupMeasurements"].Select(string.Format("SignalID = '{0}' AND MeasurementGroupID = {1}", signalID, subscriberMeasurementGroup["MeasurementGroupID"])).Length > 0)
                .Select(subscriberMeasurementGroup => subscriberMeasurementGroup["Allowed"].ToNonNullString("0").ParseBoolean());

            foreach (bool flag in explicitGroupAuthorizationFlags)
            {
                if (flag)
                    explicitlyAuthorizedByGroup = true;
                else
                    return false;
            }

            if (explicitlyAuthorizedByGroup)
                return true;

            // Look up implicitly defined filter based measurements
            implicitFilterAuthorizationFlags = Regex.Matches(subscriber["AccessControlFilter"].ToNonNullString().ReplaceControlCharacters(), FilterRegex, RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Where(match => m_subscriberPermissionsDataSet.Tables["ActiveMeasurements"].Select(string.Format("SignalID = '{0}' AND ({1})", signalID, match.Groups[2].Value)).Length > 0)
                .Select(match => (match.Groups[1].Value == "ALLOW"));

            foreach (bool flag in implicitFilterAuthorizationFlags)
            {
                if (flag)
                    implicitlyAuthorizedByFilter = true;
                else
                    return false;
            }

            if (implicitlyAuthorizedByFilter)
                return true;

            // Look up implicitly defined group based measurements
            return subscriberMeasurementGroups
                .Select(subscriberMeasurementGroup => Tuple.Create(subscriberMeasurementGroup, m_subscriberPermissionsDataSet.Tables["MeasurementGroups"].Select(string.Format("ID = {0}", subscriberMeasurementGroup["MeasurementGroupID"]))))
                .Where(tuple => tuple.Item2.Any(measurementGroup => AdapterBase.ParseInputMeasurementKeys(m_subscriberPermissionsDataSet, false, measurementGroup["FilterExpression"].ToNonNullString()).Select(key => key.SignalID).Contains(signalID)))
                .Select(tuple => tuple.Item1["Allowed"].ToNonNullString("0").ParseBoolean())
                .DefaultIfEmpty(false)
                .All(allowed => allowed);
        }

        /// <summary>
        /// Handles <see cref="AddAllowedMeasurementGroup"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurement groups to be added.</param>
        private void AddAllowedMeasurementGroup(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items != null && items.Count > 0)
            {
                List<int> measurementGroupIDs = new List<int>();

                foreach (object item in items)
                {
                    measurementGroupIDs.Add(((KeyValuePair<int, string>)item).Key);
                }

                string result = Subscriber.AddMeasurementGroups(null, CurrentItem.ID, measurementGroupIDs, true);

                //Popup(result, "Allow Measurement Groups", MessageBoxImage.Information);
                DisplayStatusMessage(result);

                // Log changes to event log
                int currentCount = CurrentItem.AllowedMeasurements.Count;
                int addedCount = measurementGroupIDs.Count;
                CommonFunctions.LogEvent(string.Format("Allowed {0} new measurement groups in addition to the {1} already allowed for subscriber {2} [{3}] for a total of {4} now allowed. See audit log for complete details.\r\n\r\nAdded measurement groups: {5}", addedCount, currentCount, CurrentItem.Name.ToNonNullNorWhiteSpace("Subscriber"), CurrentItem.Acronym, currentCount + addedCount, ShortFormattedList(measurementGroupIDs.Select(GetGroupName))), 0);

                CurrentItem.AllowedMeasurementGroups = Subscriber.GetAllowedMeasurementGroups(null, CurrentItem.ID);
                CurrentItem.AvailableMeasurementGroups = Subscriber.GetAvailableMeasurementGroups(null, CurrentItem.ID);

                try
                {
                    CommonFunctions.SendCommandToService("ReloadConfig");
                }
                catch (Exception ex)
                {
                    CommonFunctions.LogException(null, "", ex);
                }

                LoadPermissionsDataSet();
            }
        }

        /// <summary>
        /// Handles <see cref="RemoveAllowedMeasurementGroup"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurement groups to be removed.</param>
        private void RemoveAllowedMeasurementGroup(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items != null && items.Count > 0)
            {
                List<int> measurementGroupIDs = new List<int>();

                foreach (object item in items)
                {
                    measurementGroupIDs.Add(((KeyValuePair<int, string>)item).Key);
                }

                string result = Subscriber.RemoveMeasurementGroups(null, CurrentItem.ID, measurementGroupIDs);

                //Popup(result, "Remove Measurement Groups", MessageBoxImage.Information);
                DisplayStatusMessage(result);

                // Log changes to event log
                int currentCount = CurrentItem.AllowedMeasurements.Count;
                int removedCount = measurementGroupIDs.Count;
                CommonFunctions.LogEvent(string.Format("Removed {0} measurement group from the {1} currently allowed for subscriber {2} [{3}] for a total of {4} now allowed. See audit log for complete details.\r\n\r\nRemoved measurement groups: {5}.", removedCount, currentCount, CurrentItem.Name.ToNonNullNorWhiteSpace("Subscriber"), CurrentItem.Acronym, currentCount - removedCount, ShortFormattedList(measurementGroupIDs.Select(GetGroupName))), 0);

                CurrentItem.AllowedMeasurementGroups = Subscriber.GetAllowedMeasurementGroups(null, CurrentItem.ID);
                CurrentItem.AvailableMeasurementGroups = Subscriber.GetAvailableMeasurementGroups(null, CurrentItem.ID);

                try
                {
                    CommonFunctions.SendCommandToService("ReloadConfig");
                }
                catch (Exception ex)
                {
                    CommonFunctions.LogException(null, "", ex);
                }

                LoadPermissionsDataSet();
            }
        }

        /// <summary>
        /// Handles <see cref="AddDeniedMeasurementGroup"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurement groups to be added.</param>
        private void AddDeniedMeasurementGroup(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items != null && items.Count > 0)
            {
                List<int> measurementGroupIDs = new List<int>();

                foreach (object item in items)
                {
                    measurementGroupIDs.Add(((KeyValuePair<int, string>)item).Key);
                }

                string result = Subscriber.AddMeasurementGroups(null, CurrentItem.ID, measurementGroupIDs, false);

                //Popup(result, "Deny Measurement Groups", MessageBoxImage.Information);
                DisplayStatusMessage(result);

                // Log changes to event log
                int currentCount = CurrentItem.AllowedMeasurements.Count;
                int addedCount = measurementGroupIDs.Count;
                CommonFunctions.LogEvent(string.Format("Denied {0} new measurement groups in addition to the {1} already denied for subscriber {2} [{3}] for a total of {4} now denied. See audit log for complete details.\r\n\r\nDenied measurement groups: {5}", addedCount, currentCount, CurrentItem.Name.ToNonNullNorWhiteSpace("Subscriber"), CurrentItem.Acronym, currentCount + addedCount, ShortFormattedList(measurementGroupIDs.Select(GetGroupName))), 0);

                CurrentItem.AvailableMeasurementGroups = Subscriber.GetAvailableMeasurementGroups(null, CurrentItem.ID);
                CurrentItem.DeniedMeasurementGroups = Subscriber.GetDeniedMeasurementGroups(null, CurrentItem.ID);

                try
                {
                    CommonFunctions.SendCommandToService("ReloadConfig");
                }
                catch (Exception ex)
                {
                    CommonFunctions.LogException(null, "", ex);
                }

                LoadPermissionsDataSet();
            }
        }

        /// <summary>
        /// Handles <see cref="RemoveDeniedMeasurementGroup"/>.
        /// </summary>
        /// <param name="parameter">Collection of measurement groups to be removed.</param>
        private void RemoveDeniedMeasurementGroup(object parameter)
        {
            ObservableCollection<object> items = (ObservableCollection<object>)parameter;

            if (items != null && items.Count > 0)
            {
                List<int> measurementGroupIDs = new List<int>();

                foreach (object item in items)
                {
                    measurementGroupIDs.Add(((KeyValuePair<int, string>)item).Key);
                }

                string result = Subscriber.RemoveMeasurementGroups(null, CurrentItem.ID, measurementGroupIDs);

                //Popup(result, "Remove Measurement Groups", MessageBoxImage.Information);
                DisplayStatusMessage(result);

                // Log changes to event log
                int currentCount = CurrentItem.AllowedMeasurements.Count;
                int removedCount = measurementGroupIDs.Count;
                CommonFunctions.LogEvent(string.Format("Removed {0} measurement group from the {1} currently denied for subscriber {2} [{3}] for a total of {4} now denied. See audit log for complete details.\r\n\r\nRemoved measurement groups: {5}.", removedCount, currentCount, CurrentItem.Name.ToNonNullNorWhiteSpace("Subscriber"), CurrentItem.Acronym, currentCount - removedCount, ShortFormattedList(measurementGroupIDs.Select(GetGroupName))), 0);

                CurrentItem.AvailableMeasurementGroups = Subscriber.GetAvailableMeasurementGroups(null, CurrentItem.ID);
                CurrentItem.DeniedMeasurementGroups = Subscriber.GetDeniedMeasurementGroups(null, CurrentItem.ID);

                try
                {
                    CommonFunctions.SendCommandToService("ReloadConfig");
                }
                catch (Exception ex)
                {
                    CommonFunctions.LogException(null, "", ex);
                }

                LoadPermissionsDataSet();
            }
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "CurrentItem")
            {
                LoadPermissionsDataSet();
                m_securityMode = IsNewRecord || !string.IsNullOrEmpty(CurrentItem.RemoteCertificateFile) ? SecurityMode.TLS : SecurityMode.Gateway;
            }
        }

        // Attempt to get measurement name from signal ID
        private string GetMeasurementName(Guid signalID)
        {
            DataModelMeasurement measurement = CurrentItem.AvailableMeasurements.FirstOrDefault(m => m.SignalID == signalID);

            if ((object)measurement != null)
                return string.Format("{0} [{1}:{2}]", measurement.PointTag, measurement.HistorianAcronym, measurement.PointID);

            return signalID.ToString();
        }

        // Attempt to get group name from group ID
        private string GetGroupName(int groupID)
        {
            string name;
            return CurrentItem.AvailableMeasurementGroups.TryGetValue(groupID, out name) ? name : "Group";
        }

        // Convert enumerated list to a delimited list limiting the total enumerations
        private static string ShortFormattedList<T>(IEnumerable<T> items, int total = 10)
        {
            return items.Take(total).Select(item => item.ToString()).ToDelimitedString(", ") + (items.Count() > total ? ", ..." : "");
        }

        // Determines the effective permissions of each of the measurements in the current page
        private void UpdateEffectivePermissions()
        {
            Dictionary<string, Exception> exceptions = new Dictionary<string, Exception>();

            foreach (DataModelMeasurement measurement in CurrentAvailableMeasurementsPage)
            {
                try
                {
                    measurement.Selected = SubscriberHasRights(measurement.SignalID);
                }
                catch (Exception ex)
                {
                    exceptions[ex.Message] = ex;
                }
            }

            foreach (Exception exception in exceptions.Values)
                CommonFunctions.LogException(null, "Subscriber Effective Permissions", exception);
        }

        // Loads the data set used to determine effective permissions
        private void LoadPermissionsDataSet()
        {
            Dictionary<string, string> dataTableDefinitions = new Dictionary<string, string>()
            {
                { "ActiveMeasurement", "ActiveMeasurements" },
                { "Subscriber", "Subscribers" },
                { "SubscriberMeasurement", "SubscriberMeasurements" },
                { "SubscriberMeasurementGroup", "SubscriberMeasurementGroups" },
                { "MeasurementGroup", "MeasurementGroups" },
                { "MeasurementGroupMeasurement", "MeasurementGroupMeasurements" }
            };

            DataTable dataTable;
            string queryFormat;
            string parameterizedQuery;

            m_subscriberPermissionsDataSet = new DataSet();

            using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
            {
                foreach (KeyValuePair<string, string> definition in dataTableDefinitions)
                {
                    queryFormat = string.Format("SELECT * FROM {0} WHERE NodeID = {{0}}", definition.Key);
                    parameterizedQuery = database.ParameterizedQueryString(queryFormat, "nodeID");
                    dataTable = database.Connection.RetrieveData(database.AdapterType, parameterizedQuery, DataExtensions.DefaultTimeoutDuration, database.CurrentNodeID());
                    dataTable.TableName = definition.Value;
                    dataTable.DataSet.Tables.Remove(dataTable);
                    m_subscriberPermissionsDataSet.Tables.Add(dataTable);
                }
            }

            UpdateEffectivePermissions();
        }

        #endregion
    }
}
