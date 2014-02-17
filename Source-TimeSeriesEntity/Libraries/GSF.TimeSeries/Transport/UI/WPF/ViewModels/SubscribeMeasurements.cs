//******************************************************************************************************
//  SubscribeMeasurements.cs - Gbtc
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
//  05/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GSF.Data;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.TimeSeries.UI;
using DataModelMeasurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.TimeSeries.Transport.UI.ViewModels
{
    internal class SubscribeMeasurements : PagedViewModelBase<Device, int>
    {
        #region [ Members ]

        // Fields
        private Dictionary<int, string> m_deviceList;
        private ICollection<DataModelMeasurement> m_currentSubscribedMeasurementsPage;
        private readonly object m_subscribedMeasurementsLock;
        //private RelayCommand m_subscribeMeasurementCommand;
        //private RelayCommand m_unsubscribeMeasurementCommand;
        private KeyValuePair<int, string> m_currentDevice;
        private int m_currentDeviceID;
        private ObservableCollection<DataModelMeasurement> m_measurementsToBeSubscribed;
        private AuthorizedMeasurementsQuery m_authorizationQuery;
        private DispatcherTimer m_refreshTimer;

        // Delegates

        /// <summary>
        /// Method signature for a function which handles MeasurementsSubscribed event.
        /// </summary>
        /// <param name="sender">Source of an event.</param>
        /// <param name="e">Event arguments.</param>
        public delegate void OnSubscriptionChanged(object sender, RoutedEventArgs e);

        /// <summary>
        /// Method signature for a function which handles CurrentDeviceChanged event.
        /// </summary>
        /// <param name="sender">Source of an event.</param>
        /// <param name="e">Event arguments.</param>
        public delegate void OnCurrentDeviceChanged(object sender, RoutedEventArgs e);

        // Event

        /// <summary>
        /// Event to notify that measurements have been subscribed.
        /// </summary>
        public event OnSubscriptionChanged SubscriptionChanged;

        /// <summary>
        /// Event to notify that current device has changed.
        /// </summary>
        public event OnCurrentDeviceChanged CurrentDeviceChanged;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets boolean value to indicate if user has rights to save.
        /// </summary>
        public override bool CanSave
        {
            get
            {
                return ((object)CommonFunctions.CurrentPrincipal != null && CommonFunctions.CurrentPrincipal.IsInRole("Administrator, Editor"));
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return (CurrentItem.ID == 0);
            }
        }

        /// <summary>
        /// Gets or sets list of <see cref="Device"/> with protocol type set to Measurement.
        /// </summary>
        public Dictionary<int, string> DeviceList
        {
            get
            {
                return m_deviceList;
            }
            set
            {
                m_deviceList = value;
                OnPropertyChanged("DeviceList");
            }
        }

        /// <summary>
        /// Gets or sets measurements with Subscribed flag set to true.
        /// </summary>
        public ICollection<DataModelMeasurement> CurrentSubscribedMeasurementsPage
        {
            get
            {
                lock (m_subscribedMeasurementsLock)
                {
                    // Return a copy of the measurements since we can't request consumer to lock the collection
                    return new ObservableCollection<DataModelMeasurement>(m_currentSubscribedMeasurementsPage);
                }
            }
            set
            {
                lock (m_subscribedMeasurementsLock)
                {
                    m_currentSubscribedMeasurementsPage = value;
                }
                OnPropertyChanged("SubscribedMeasurements");
            }
        }

        /// <summary>
        /// Gets or sets current device user has selected on UI.
        /// </summary>
        public KeyValuePair<int, string> CurrentDevice
        {
            get
            {
                return m_currentDevice;
            }
            set
            {
                m_currentDevice = value;
                OnPropertyChanged("CurrentDevice");
                CurrentDeviceID = m_currentDevice.Key;
                if (CurrentDeviceChanged != null)
                    CurrentDeviceChanged(this, null);
            }
        }

        /// <summary>
        /// Gets or set ID of the device user has currently selected on UI.
        /// </summary>
        public int CurrentDeviceID
        {
            get
            {
                return m_currentDeviceID;
            }
            set
            {
                m_currentDeviceID = value;
                OnPropertyChanged("CurrentDeviceID");
            }
        }

        /// <summary>
        /// Gets or sets collection of measurements where Internal = false and Subscribed = false.
        /// </summary>
        public ObservableCollection<DataModelMeasurement> MeasurementsToBeSubscribed
        {
            get
            {
                return m_measurementsToBeSubscribed;
            }
            set
            {
                m_measurementsToBeSubscribed = value;
                OnPropertyChanged("MeasurementsToBeSubscribed");
            }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="SubscribeMeasurements"/> class.
        /// </summary>
        /// <param name="itemsPerPage"></param>
        /// <param name="autoSave"></param>
        public SubscribeMeasurements(int itemsPerPage, bool autoSave = true)
            : base(0, autoSave)
        {
            m_subscribedMeasurementsLock = new object();
            m_authorizationQuery = new AuthorizedMeasurementsQuery();
            m_authorizationQuery.AuthorizedMeasurements += m_authorizationQuery_AuthorizedMeasurements;
            m_refreshTimer = new DispatcherTimer();
            m_refreshTimer.Interval = TimeSpan.FromSeconds(5);
            m_refreshTimer.Tick += m_refreshTimer_Tick;
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
            return CurrentItem.Acronym;
        }

        public override void Load()
        {
            try
            {
                DeviceList = Device.GetLookupList(null, "Measurement", true);
                CurrentDevice = DeviceList.First();

                lock (m_subscribedMeasurementsLock)
                {
                    if ((object)m_currentSubscribedMeasurementsPage != null && m_currentSubscribedMeasurementsPage.Count > 0)
                        m_authorizationQuery.RequestAuthorizationStatus(m_currentSubscribedMeasurementsPage.Select(measurement => measurement.SignalID));
                }

                m_refreshTimer.Start();
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
        }

        /// <summary>
        /// Handles unloading of the subscriber measurement view model.
        /// </summary>
        public void Unload()
        {
            if ((object)m_authorizationQuery != null)
            {
                m_authorizationQuery.AuthorizedMeasurements -= m_authorizationQuery_AuthorizedMeasurements;
                m_authorizationQuery.Dispose();
            }

            m_authorizationQuery = null;

            if (m_refreshTimer != null)
            {
                try
                {
                    m_refreshTimer.Stop();
                }
                finally
                {
                    m_refreshTimer = null;
                }
            }
        }

        public override void Save()
        {
            // Do Nothing.
        }

        public void AddSubscribedMeasurements(ICollection<Guid> measurementIDs)
        {
            ObservableCollection<int> deviceIDsForMeasurementsToBeAdded = new ObservableCollection<int>();
            ObservableCollection<DataModelMeasurement> measurementsToBeAdded;

            //If All Devices is not selected on the screen and a specific device is selected then we will just initialize that device.
            //Otherwise, maintain a list of unique device ids for which measurements are being subscribed and initialize all of them.
            if (CurrentDevice.Key > 0)
                deviceIDsForMeasurementsToBeAdded.Add(CurrentDevice.Key);

            if (measurementIDs != null && measurementIDs.Count > 0)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                try
                {
                    measurementsToBeAdded = DataModelMeasurement.LoadFromKeys(database, measurementIDs.ToList());

                    foreach (DataModelMeasurement measurement in measurementsToBeAdded)
                    {
                        measurement.Subscribed = true;

                        DataModelMeasurement.Save(database, measurement);

                        if (CurrentDevice.Key == 0 && measurement.DeviceID != null)
                        {
                            if (!deviceIDsForMeasurementsToBeAdded.Contains((int)measurement.DeviceID))
                                deviceIDsForMeasurementsToBeAdded.Add((int)measurement.DeviceID);
                        }
                    }

                    if (SubscriptionChanged != null)
                        SubscriptionChanged(this, null);

                    if (deviceIDsForMeasurementsToBeAdded.Count > 0)
                        ThreadPool.QueueUserWorkItem(InitializeDeviceConnection, deviceIDsForMeasurementsToBeAdded);
                }
                catch (Exception ex)
                {
                    Popup("ERROR: " + ex.Message, "Subscribe Measurements", MessageBoxImage.Error);
                }
                finally
                {
                    if (database != null)
                        database.Dispose();

                    Mouse.OverrideCursor = null;
                }
            }
        }

        public void RemoveSubscribedMeasurements(ICollection<Guid> measurementIDs)
        {
            ObservableCollection<int> deviceIDsForMeasurementsToBeAdded = new ObservableCollection<int>();
            ObservableCollection<DataModelMeasurement> measurementsToBeRemoved;

            //If All Devices is not selected on the screen and a specific device is selected then we will just initialize that device.
            //Otherwise, maintain a list of unique device ids for which measurements are being subscribed and initialize all of them.
            if (CurrentDevice.Key > 0)
                deviceIDsForMeasurementsToBeAdded.Add(CurrentDevice.Key);

            if (measurementIDs != null && measurementIDs.Count > 0)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                try
                {
                    measurementsToBeRemoved = DataModelMeasurement.LoadFromKeys(database, measurementIDs.ToList());

                    foreach (DataModelMeasurement measurement in measurementsToBeRemoved)
                    {
                        measurement.Subscribed = false;

                        DataModelMeasurement.Save(database, measurement);

                        if (CurrentDevice.Key == 0 && measurement.DeviceID != null)
                        {
                            if (!deviceIDsForMeasurementsToBeAdded.Contains((int)measurement.DeviceID))
                                deviceIDsForMeasurementsToBeAdded.Add((int)measurement.DeviceID);
                        }
                    }

                    if (SubscriptionChanged != null)
                        SubscriptionChanged(this, null);

                    if (deviceIDsForMeasurementsToBeAdded.Count > 0)
                        ThreadPool.QueueUserWorkItem(InitializeDeviceConnection, deviceIDsForMeasurementsToBeAdded);
                }
                catch (Exception ex)
                {
                    Popup("ERROR: " + ex.Message, "Unsubscribe Measurements", MessageBoxImage.Error);
                }
                finally
                {
                    if (database != null)
                        database.Dispose();

                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// Initializes each device's connection.
        /// </summary>
        /// <param name="state">List of device ids need to be initialized.</param>
        private void InitializeDeviceConnection(object state)
        {
            ObservableCollection<int> deviceIDs = (ObservableCollection<int>)state;
            Dictionary<int, Device> devicesToBeNotified = new Dictionary<int, Device>();

            foreach (int id in deviceIDs)
            {
                Device device = Device.GetDevice(null, "WHERE ID = " + id);
                if (device.ParentID != null)
                {
                    int idToBeAdded = (int)device.ParentID;
                    if (!devicesToBeNotified.ContainsKey(idToBeAdded))
                        devicesToBeNotified[idToBeAdded] = Device.GetDevice(null, "WHERE ID = " + idToBeAdded);
                }
                else
                {
                    if (!devicesToBeNotified.ContainsKey(id))
                        devicesToBeNotified[id] = device;
                }
            }

            foreach (KeyValuePair<int, Device> item in devicesToBeNotified)
                Device.NotifyService(item.Value);
        }

        private void m_authorizationQuery_AuthorizedMeasurements(object sender, EventArgs<Guid[]> e)
        {
            List<Guid> authorizedMeasurements = new List<Guid>(e.Argument);
            authorizedMeasurements.Sort();

            lock (m_subscribedMeasurementsLock)
            {
                foreach (DataModelMeasurement measurement in m_currentSubscribedMeasurementsPage)
                {
                    measurement.Selected = (authorizedMeasurements.BinarySearch(measurement.SignalID) >= 0);
                }
            }

            OnPropertyChanged("SubscribedMeasurements");
        }

        // We continually refresh authorization status since this can change at any time...
        private void m_refreshTimer_Tick(object sender, EventArgs e)
        {
            lock (m_subscribedMeasurementsLock)
            {
                m_authorizationQuery.RequestAuthorizationStatus(m_currentSubscribedMeasurementsPage.Select(measurement => measurement.SignalID));
            }
        }

        #endregion
    }
}
