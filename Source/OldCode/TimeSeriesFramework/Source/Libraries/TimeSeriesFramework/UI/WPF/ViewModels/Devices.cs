//******************************************************************************************************
//  Devices.cs - Gbtc
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
//  05/06/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI.DataModels;
using TimeSeriesFramework.UI.UserControls;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="Device"/> collection and current selection information for UI.
    /// </summary>
    internal class Devices : PagedViewModelBase<Device, int>
    {
        #region [ Members ]

        // Fields
        private Dictionary<Guid, string> m_nodeLookupList;
        private Dictionary<int, string> m_concentratorDeviceLookupList;
        private Dictionary<int, string> m_companyLookupList;
        private Dictionary<int, string> m_historianLookupList;
        private Dictionary<int, string> m_interconnectionLookupList;
        private Dictionary<int, string> m_protocolLookupList;
        private Dictionary<int, string> m_vendorDeviceLookupList;
        private Dictionary<string, string> m_timezoneLookupList;
        private RelayCommand m_editCommand;
        private RelayCommand m_phasorCommand;
        private RelayCommand m_measurementCommand;
        private RelayCommand m_copyCommand;
        private RelayCommand m_initializeCommand;
        private string m_runtimeID;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="Devices"/> class.
        /// </summary>
        /// <param name="itemsPerPage"></param>
        /// <param name="autoSave"></param>
        /// <param name="device"><see cref="Device"/> to be edited.</param>
        public Devices(int itemsPerPage, bool autoSave = true, Device device = null)
            : base(itemsPerPage, autoSave)
        {
            m_nodeLookupList = Node.GetLookupList(null);
            m_concentratorDeviceLookupList = Device.GetLookupList(null, DeviceType.Concentrator, true);
            m_companyLookupList = Company.GetLookupList(null, true);
            m_historianLookupList = Historian.GetLookupList(null, true, false);
            m_interconnectionLookupList = Interconnection.GetLookupList(null, true);
            m_protocolLookupList = Protocol.GetLookupList(null, true);
            m_vendorDeviceLookupList = VendorDevice.GetLookupList(null, true);
            m_timezoneLookupList = CommonFunctions.GetTimeZones(true);

            if (device != null)     // i.e. user wants to edit existing device's configuration. So we will load that by default.
                CurrentItem = device;
        }

        #endregion

        #region [ Properties ]

        public string RuntimeID
        {
            get
            {
                return m_runtimeID;
            }
            set
            {
                m_runtimeID = value;
                OnPropertyChanged("RuntimeID");
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
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of concentrator <see cref="Device"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> ConcentratorDeviceLookupList
        {
            get
            {
                return m_concentratorDeviceLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Company"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> CompanyLookupList
        {
            get
            {
                return m_companyLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Historian"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> HistorianLookupList
        {
            get
            {
                return m_historianLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Interconnection"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> InterconnectionLookupList
        {
            get
            {
                return m_interconnectionLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Protocol"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> ProtocolLookupList
        {
            get
            {
                return m_protocolLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="VendorDevice"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> VendorDeviceLookupList
        {
            get
            {
                return m_vendorDeviceLookupList;
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of system time zones.
        /// </summary>
        public Dictionary<string, string> TimezoneLookupList
        {
            get
            {
                return m_timezoneLookupList;
            }
        }

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
        /// Gets <see cref="ICommand"/> for edit operation.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (m_editCommand == null)
                    m_editCommand = new RelayCommand(GoToEdit);

                return m_editCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> for copy operation.
        /// </summary>
        public ICommand CopyCommand
        {
            get
            {
                if (m_copyCommand == null)
                    m_copyCommand = new RelayCommand(MakeCopy);

                return m_copyCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to go to phasors configuration.
        /// </summary>
        public ICommand PhasorCommand
        {
            get
            {
                if (m_phasorCommand == null)
                    m_phasorCommand = new RelayCommand(GoToPhasors);

                return m_phasorCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to go to measurements configuration.
        /// </summary>
        public ICommand MeasurementCommand
        {
            get
            {
                if (m_measurementCommand == null)
                    m_measurementCommand = new RelayCommand(GoToMeasurements);

                return m_measurementCommand;
            }
        }

        public ICommand InitializeCommand
        {
            get
            {
                if (m_initializeCommand == null)
                    m_initializeCommand = new RelayCommand(Initialize, () => CanSave);

                return m_initializeCommand;
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
        /// Creates a new instance of <see cref="Historian"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            // Reset all the dropdown fields so that first item displays as selected.
            if (m_nodeLookupList.Count > 0)
                CurrentItem.NodeID = m_nodeLookupList.First().Key;

            if (m_concentratorDeviceLookupList.Count > 0)
                CurrentItem.ParentID = m_concentratorDeviceLookupList.First().Key;

            if (m_companyLookupList.Count > 0)
                CurrentItem.CompanyID = m_companyLookupList.First().Key;

            if (m_historianLookupList.Count > 0)
                CurrentItem.HistorianID = m_historianLookupList.First().Key;

            if (m_interconnectionLookupList.Count > 0)
                CurrentItem.InterconnectionID = m_interconnectionLookupList.First().Key;

            if (m_protocolLookupList.Count > 0)
                CurrentItem.ProtocolID = m_protocolLookupList.First().Key;

            if (m_vendorDeviceLookupList.Count > 0)
                CurrentItem.VendorDeviceID = m_vendorDeviceLookupList.First().Key;

            if (m_timezoneLookupList.Count > 0)
                CurrentItem.TimeZone = m_timezoneLookupList.First().Key;
        }

        /// <summary>
        /// Loads collection of <see cref="Device"/> information stored in the database.
        /// </summary>
        /// <remarks>This method is overridden because MethodInfo.Invoke in the base class did not like optional parameters.</remarks>
        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                ItemsSource = Device.Load(null);
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Load Devices", MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Saves <see cref="Device"/> information into database.
        /// </summary>
        public override void Save()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                base.Save();

                if (ItemsPerPage == 0) // i.e. if user is on form page then go back to list page after save.
                {
                    UIElement frame = null;
                    UIElement groupBox = null;
                    CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(System.Windows.Controls.Frame), ref frame);
                    CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(GroupBox), ref groupBox);

                    if (frame != null)
                    {
                        DeviceListUserControl deviceListUserControl = new DeviceListUserControl();
                        ((System.Windows.Controls.Frame)frame).Navigate(deviceListUserControl);

                        if (groupBox != null)
                            ((GroupBox)groupBox).Header = "Browse Devices";

                    }
                }
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Save Device", MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Handles <see cref="EditCommand"/>.
        /// </summary>
        /// <param name="parameter">Parameter to use for the command provided by commandparameter from UI.</param>
        private void GoToEdit(object parameter)
        {
            Device deviceToEdit = (Device)parameter;
            UIElement frame = null;
            UIElement groupBox = null;
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(System.Windows.Controls.Frame), ref frame);
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(GroupBox), ref groupBox);

            if (frame != null)
            {
                DeviceUserControl deviceUserControl = new DeviceUserControl(deviceToEdit);
                ((System.Windows.Controls.Frame)frame).Navigate(deviceUserControl);

                if (groupBox != null)
                    ((GroupBox)groupBox).Header = "Manage Device Configuration";
            }
        }

        /// <summary>
        /// Handles <see cref="CopyCommand"/>.
        /// </summary>
        /// <param name="parameter">Parameter to use for the command provided by commandparameter from UI.</param>
        private void MakeCopy(object parameter)
        {
            Device deviceToCopy = new Device();
            deviceToCopy = (Device)parameter;
            string newAcronym; ;
            int i = 1;
            do  // Find unique acronym.
            {
                newAcronym = deviceToCopy.Acronym + i.ToString();
                i++;
            } while (DeviceExists(newAcronym));

            deviceToCopy.Acronym = newAcronym; // Change acronym of the device before going to edit screen.
            deviceToCopy.Name = "Copy of " + deviceToCopy.Name; // Change name of the device before going to edit screen.
            deviceToCopy.Enabled = false; // Always set enabled to false for copied device.
            deviceToCopy.ID = 0;    // Set id to zero so that it will be added as a new device.
            ItemsPerPage = 0; // Set this so that on Save() user will be sent back to list screen.

            // Go to edit screen.
            UIElement frame = null;
            UIElement groupBox = null;
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(System.Windows.Controls.Frame), ref frame);
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(GroupBox), ref groupBox);

            if (frame != null)
            {
                DeviceUserControl deviceUserControl = new DeviceUserControl(deviceToCopy);
                ((System.Windows.Controls.Frame)frame).Navigate(deviceUserControl);

                if (groupBox != null)
                    ((GroupBox)groupBox).Header = "Manage Device Configuration";
            }
        }

        private bool DeviceExists(string acronym)
        {
            bool deviceExists = false;
            foreach (Device device in ItemsSource)
            {
                if (device.Acronym == acronym)
                    return true;
            }
            return deviceExists;
        }

        /// <summary>
        /// Handles <see cref="MeasurementCommand"/>.
        /// </summary>
        /// <param name="parameter">Parameter to use for the command provided by commandparameter from UI.</param>
        private void GoToMeasurements(object parameter)
        {
            Device device = (Device)parameter;

            UIElement frame = null;
            UIElement groupBox = null;
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(System.Windows.Controls.Frame), ref frame);
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(GroupBox), ref groupBox);

            if (frame != null)
            {
                MeasurementUserControl measurementUserControl = new MeasurementUserControl(device.ID);
                ((System.Windows.Controls.Frame)frame).Navigate(measurementUserControl);

                if (groupBox != null)
                    ((GroupBox)groupBox).Header = "Manage Measurements for " + device.Acronym;
            }
        }

        /// <summary>
        /// Handles <see cref="PhasorCommand"/>.
        /// </summary>
        /// <param name="parameter">Parameter to use for the command provided by commandparameter from UI.</param>
        private void GoToPhasors(object parameter)
        {
            Device device = (Device)parameter;

            UIElement frame = null;
            UIElement groupBox = null;
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(System.Windows.Controls.Frame), ref frame);
            CommonFunctions.GetFirstChild(Application.Current.MainWindow, typeof(GroupBox), ref groupBox);

            if (frame != null)
            {
                PhasorUserControl phasorUserControl = new PhasorUserControl(device.ID);
                ((System.Windows.Controls.Frame)frame).Navigate(phasorUserControl);

                if (groupBox != null)
                    ((GroupBox)groupBox).Header = "Manage Phasors for " + device.Acronym;
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "CurrentItem")
            {
                if (CurrentItem == null)
                    RuntimeID = string.Empty;
                else
                    RuntimeID = CommonFunctions.GetRuntimeID("Device", CurrentItem.ID);
            }
        }

        private void Initialize()
        {
            try
            {
                if (!string.IsNullOrEmpty(RuntimeID))
                {
                    if (Confirm("Do you want to send Initialize " + GetCurrentItemName() + "?", "Confirm Initialize"))
                    {
                        Device.NotifyService(CurrentItem);

                        Popup("Successfully sent Initialize command.", "Initialize", MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Failed To Initialize", MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
