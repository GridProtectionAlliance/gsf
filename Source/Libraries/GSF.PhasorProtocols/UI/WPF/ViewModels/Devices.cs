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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.Modal;
using GSF.PhasorProtocols.UI.UserControls;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.DataModels;

namespace GSF.PhasorProtocols.UI.ViewModels
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
        private RelayCommand m_buildConnectionStringCommand;
        private RelayCommand m_buildAlternateCommandChannelCommand;
        private RelayCommand m_updateConfigurationCommand;
        private RelayCommand m_configureConcentratorCommand;
        private string m_runtimeID;
        //private ObservableCollection<Device> m_devices;
        private RelayCommand m_searchCommand;
        private RelayCommand m_showAllCommand;
        private bool m_stayOnConfigurationScreen;
        private ObservableCollection<Device> m_pdcDevices;
        private string m_searchText;
        private IList<int> m_allKeys;

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
            // When device is null, user wants to edit existing device's configuration. So we will load that by default.
            if ((object)device != null)
            {
                CurrentItem = device;
                OnPropertyChanged("IsNewRecord");
                OnPropertyChanged("CanGoToPhasorOrMeasurement");

                if (device.IsConcentrator)
                    PdcDevices = Device.GetDevices(null, "WHERE ParentID = " + device.ID);
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to build connection string.
        /// </summary>
        public ICommand BuildConnectionStringCommand
        {
            get
            {
                if ((object)m_buildConnectionStringCommand == null)
                    m_buildConnectionStringCommand = new RelayCommand(BuildConnectionString);

                return m_buildConnectionStringCommand;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to build alternate command channel.
        /// </summary>
        public ICommand BuildAlternateCommandChannelCommand
        {
            get
            {
                if ((object)m_buildAlternateCommandChannelCommand == null)
                    m_buildAlternateCommandChannelCommand = new RelayCommand(BuildAlternateCommandChannel);

                return m_buildAlternateCommandChannelCommand;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> RuntimeID.
        /// </summary>
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
                if ((object)m_editCommand == null)
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
                if ((object)m_copyCommand == null)
                    m_copyCommand = new RelayCommand(MakeCopy);

                return m_copyCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> for updating configuration.
        /// </summary>
        public ICommand UpdateConfigurationCommand
        {
            get
            {
                if ((object)m_updateConfigurationCommand == null)
                    m_updateConfigurationCommand = new RelayCommand(UpdateConfiguration);

                return m_updateConfigurationCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to go to phasors configuration.
        /// </summary>
        public ICommand PhasorCommand
        {
            get
            {
                if ((object)m_phasorCommand == null)
                    m_phasorCommand = new RelayCommand(GoToPhasors, () => CanGoToPhasorOrMeasurement);

                return m_phasorCommand;
            }
        }

        /// <summary>
        /// Gets a boolean flag indicating if Phasors and Measurements button are visible or not.
        /// </summary>
        public bool CanGoToPhasorOrMeasurement
        {
            get
            {
                return CurrentItem.ID > 0;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to go to measurements configuration.
        /// </summary>
        public ICommand MeasurementCommand
        {
            get
            {
                if ((object)m_measurementCommand == null)
                    m_measurementCommand = new RelayCommand(GoToMeasurements, () => CanGoToPhasorOrMeasurement);

                return m_measurementCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to send Initialize command to backed service.
        /// </summary>
        public ICommand InitializeCommand
        {
            get
            {
                if ((object)m_initializeCommand == null)
                    m_initializeCommand = new RelayCommand(InitializeDevice, () => CanSave);

                return m_initializeCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to search within measurements.
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                if ((object)m_searchCommand == null)
                    m_searchCommand = new RelayCommand(Search, param => true);

                return m_searchCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to show all measurements.
        /// </summary>
        public ICommand ShowAllCommand
        {
            get
            {
                if ((object)m_showAllCommand == null)
                    m_showAllCommand = new RelayCommand(ShowAll);

                return m_showAllCommand;
            }
        }

        public ICommand ConfigureConcentratorCommand
        {
            get
            {
                if ((object)m_configureConcentratorCommand == null)
                    m_configureConcentratorCommand = new RelayCommand(ConfigureConcentrator);

                return m_configureConcentratorCommand;
            }
        }

        public ObservableCollection<Device> PdcDevices
        {
            get
            {
                return m_pdcDevices;
            }
            set
            {
                m_pdcDevices = value;
                OnPropertyChanged("PdcDevices");
            }
        }

        public IList<int> AllKeys
        {
            get
            {
                if ((object)m_allKeys == null)
                    m_allKeys = Device.LoadKeys(null, 0, "", SortMember, SortDirection);

                return m_allKeys;
            }
            set
            {
                m_allKeys = value;
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
        /// Initialization to be done before the initial call to <see cref="Load"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            m_nodeLookupList = Node.GetLookupList(null);
            m_concentratorDeviceLookupList = Device.GetLookupList(null, DeviceType.Concentrator, true);
            m_companyLookupList = Company.GetLookupList(null, true);
            m_historianLookupList = TimeSeries.UI.DataModels.Historian.GetLookupList(null, true, false);
            m_interconnectionLookupList = Interconnection.GetLookupList(null, true);
            m_protocolLookupList = Protocol.GetLookupList(null, true);
            m_vendorDeviceLookupList = VendorDevice.GetLookupList(null, true);
            m_timezoneLookupList = CommonFunctions.GetTimeZones(true);
        }

        /// <summary>
        /// Loads collection of <see cref="Device"/> information stored in the database.
        /// </summary>
        /// <remarks>This method is overridden because MethodInfo.Invoke in the base class did not like optional parameters.</remarks>
        public override void Load()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            List<int> pageKeys;

            try
            {
                if (OnBeforeLoadCanceled())
                    throw new OperationCanceledException("Load was canceled.");

                if ((object)ItemsKeys == null)
                {
                    ItemsKeys = Device.LoadKeys(null, 0, m_searchText, SortMember, SortDirection);

                    if ((object)SortSelector != null)
                    {
                        if (SortDirection == "ASC")
                            ItemsKeys = ItemsKeys.OrderBy(SortSelector).ToList();
                        else
                            ItemsKeys = ItemsKeys.OrderByDescending(SortSelector).ToList();
                    }

                    if (string.IsNullOrEmpty(m_searchText))
                        AllKeys = ItemsKeys;
                }

                pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
                ItemsSource = Device.Load(null, pageKeys);

                OnLoaded();
            }
            catch (Exception ex)
            {
                if ((object)ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load Devices Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load Devices", ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Load Devices Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load Devices", ex);
                }
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
                Device originalDevice = null;

                // If it is an existing device being modified, get prior state so we can determine if a rename occurred.
                if (CurrentItem.ID > 0)
                    originalDevice = Device.GetDevice(null, "WHERE ID = " + CurrentItem.ID);

                // Attempt to save base record before possible rename attempt as base record save can fail
                base.Save();

                try
                {
                    // If renaming a phasor device (e.g., a PMU not a PDC), check if user would like to update device acronym in output streams - this happens here
                    // instead of in the view model such that a message box can be displayed in the UI to engage user about desire to update output streams. We only
                    // perform this task for phasor style connections since renaming subscribed devices, although functional, will not "stick" - renamed devices will
                    // be reverted back to their original names at the next meta-data refresh.
                    if (!CurrentItem.IsConcentrator && (object)originalDevice != null && string.Compare(CurrentItem.ProtocolCategory, "Phasor", StringComparison.OrdinalIgnoreCase) == 0 &&
                        (string.CompareOrdinal(CurrentItem.Acronym, originalDevice.Acronym) != 0 || string.CompareOrdinal(CurrentItem.Name, originalDevice.Name) != 0) &&
                        Confirm("Device was renamed, do you want to also update the device name where it may exist in output streams?", "Update Output Stream Device"))
                    {
                        ObservableCollection<OutputStreamDevice> outputStreamDevices = OutputStreamDevice.GetOutputStreamDevices(null, "WHERE Acronym = '" + originalDevice.Acronym + "'");

                        foreach (OutputStreamDevice device in outputStreamDevices)
                        {
                            device.Acronym = CurrentItem.Acronym;
                            device.BpaAcronym = CurrentItem.Acronym.Substring(0, 4);
                            device.Name = CurrentItem.Name;
                            OutputStreamDevice.Save(null, device);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if ((object)ex.InnerException != null)
                    {
                        Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Device Rename:", MessageBoxImage.Error);
                        CommonFunctions.LogException(null, "Device Rename", ex.InnerException);
                    }
                    else
                    {
                        Popup(ex.Message, "Device Rename Exception:", MessageBoxImage.Error);
                        CommonFunctions.LogException(null, "Device Rename", ex);
                    }
                }

                // If user is on form page then go back to list page after save.
                if (ItemsPerPage == 0)
                {
                    if (!m_stayOnConfigurationScreen)
                        CommonFunctions.LoadUserControl("Browse Devices", typeof(DeviceListUserControl));
                }
            }
            catch (Exception ex)
            {
                if ((object)ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Save Device Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Save Device", ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Save Device Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Save Device", ex);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Handles <see cref="EditCommand"/>.
        /// </summary>
        /// <param name="parameter">Parameter to use for the command provided by command parameter from UI.</param>
        private void GoToEdit(object parameter)
        {
            CommonFunctions.LoadUserControl("Manage Device Configuration", typeof(DeviceUserControl), (Device)parameter);
        }

        /// <summary>
        /// Handles <see cref="CopyCommand"/>.
        /// </summary>
        /// <param name="parameter">Parameter to use for the command provided by command parameter from UI.</param>
        private void MakeCopy(object parameter)
        {
            Device deviceToCopy = (Device)parameter;
            string newAcronym;

            int i = 1;

            // Find unique acronym.
            do
            {
                newAcronym = deviceToCopy.Acronym + i;
                i++;
            }
            while (DeviceExists(newAcronym));

            deviceToCopy.Acronym = newAcronym;                  // Change acronym of the device before going to edit screen.
            deviceToCopy.Name = "Copy of " + deviceToCopy.Name; // Change name of the device before going to edit screen.            
            deviceToCopy.ID = 0;                                // Set id to zero so that it will be added as a new device.
            ItemsPerPage = 0;                                   // Set this so that on Save() user will be sent back to list screen.
            deviceToCopy.Enabled = false;                       // Always set enabled to false for copied device.

            // Go to edit screen.
            CommonFunctions.LoadUserControl("Manage Device Configuration", typeof(DeviceUserControl), deviceToCopy);
        }

        private void UpdateConfiguration(object parameter)
        {
            CommonFunctions.LoadUserControl("Input Device Configuration Wizard", typeof(InputWizardUserControl), (Device)parameter);
        }

        private bool DeviceExists(string acronym)
        {
            foreach (Device device in ItemsSource)
            {
                if (device.Acronym == acronym)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Handles <see cref="MeasurementCommand"/>.
        /// </summary>
        private void GoToMeasurements()
        {
            CommonFunctions.LoadUserControl("Manage Measurements for " + CurrentItem.Acronym, typeof(PhasorMeasurementUserControl), CurrentItem.ID);
        }

        /// <summary>
        /// Handles <see cref="PhasorCommand"/>.
        /// </summary>        
        private void GoToPhasors()
        {
            CommonFunctions.LoadUserControl("Manage Phasors for " + CurrentItem.Acronym, typeof(PhasorUserControl), CurrentItem.ID);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "CurrentItem")
            {
                if ((object)CurrentItem == null)
                    RuntimeID = string.Empty;
                else
                    RuntimeID = CommonFunctions.GetRuntimeID("Device", CurrentItem.ID);
            }
        }

        private void InitializeDevice()
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

        private void BuildConnectionString()
        {
            if ((object)CurrentItem != null)
            {
                ConnectionStringBuilder csb = new ConnectionStringBuilder(ConnectionStringBuilder.ConnectionType.DeviceConnection);

                if (!string.IsNullOrEmpty(CurrentItem.ConnectionString))
                    csb.ConnectionString = CurrentItem.ConnectionString;

                csb.Closed += delegate
                {
                    if (csb.DialogResult.GetValueOrDefault())
                        CurrentItem.ConnectionString = csb.ConnectionString;
                };

                csb.Owner = Application.Current.MainWindow;
                csb.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                csb.ShowDialog();
            }
        }

        private void BuildAlternateCommandChannel()
        {
            if ((object)CurrentItem != null)
            {
                ConnectionStringBuilder csb = new ConnectionStringBuilder(ConnectionStringBuilder.ConnectionType.AlternateCommandChannel);

                if (!string.IsNullOrEmpty(CurrentItem.AlternateCommandChannel))
                    csb.ConnectionString = CurrentItem.AlternateCommandChannel;

                if (CurrentItem.ProtocolID != null && m_protocolLookupList[(int)CurrentItem.ProtocolID] == "Gateway Transport")
                    csb.PgConnection = true;
                else
                    csb.PgConnection = false;

                csb.Closed += delegate
                {
                    if (csb.DialogResult.GetValueOrDefault())
                        CurrentItem.AlternateCommandChannel = csb.ConnectionString;
                };

                csb.Owner = Application.Current.MainWindow;
                csb.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                csb.ShowDialog();
            }
        }

        /// <summary>
        /// Deletes current item from the database.
        /// </summary>
        public override void Delete()
        {
            try
            {
                if (CurrentItem.IsConcentrator)
                {
                    IList<int> keys = Device.LoadKeys(null, CurrentItem.ID);
                    ObservableCollection<Device> deviceList = Device.Load(null, keys);
                    int outputStreamDeviceCount = 0;

                    string result;

                    foreach (Device device in deviceList)
                    {
                        outputStreamDeviceCount += OutputStreamDevice.GetOutputStreamDevices(null, "WHERE Acronym = '" + device.Acronym + "'").Count;
                    }

                    string confirm = "Are you sure you want to delete concentrator device?";
                    if (deviceList.Count > 1)
                        confirm += Environment.NewLine + "There are " + deviceList.Count.ToString() + " associated children devices.";
                    else if (deviceList.Count > 0)
                        confirm += Environment.NewLine + "There is " + deviceList.Count.ToString() + " associated child device.";

                    if (outputStreamDeviceCount > 1)
                        confirm += Environment.NewLine + "There are " + outputStreamDeviceCount.ToString() + " output stream devices that exist.";
                    else if (outputStreamDeviceCount > 0)
                        confirm += Environment.NewLine + "There is " + outputStreamDeviceCount.ToString() + " output stream device that exists.";

                    if (Confirm(confirm, "Delete Device"))
                    {
                        foreach (Device device in deviceList)
                            Device.Delete(null, device);

                        result = Device.Delete(null, CurrentItem);
                        DisplayStatusMessage(result);
                    }
                }
                else
                {
                    ObservableCollection<OutputStreamDevice> outputStreamDevices = OutputStreamDevice.GetOutputStreamDevices(null, "WHERE Acronym = '" + CurrentItem.Acronym + "'");
                    string confirm = "Are you sure you want to delete this device?";

                    if (outputStreamDevices.Count > 1)
                        confirm += Environment.NewLine + "It exists in " + outputStreamDevices.Count.ToString() + " output streams.";
                    else if (outputStreamDevices.Count > 0)
                        confirm += Environment.NewLine + "It exists in " + outputStreamDevices.Count.ToString() + " output stream.";

                    if (Confirm(confirm, "Delete Device"))
                    {
                        string result = Device.Delete(null, CurrentItem);
                        DisplayStatusMessage(result);
                    }
                }

                Device.NotifyService(null, CurrentItem.HistorianID);
                CommonFunctions.LoadUserControl("Browse Devices", typeof(DeviceListUserControl));
            }
            catch (Exception ex)
            {
                if ((object)ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Delete Device Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Delete Device", ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Delete Device Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Delete Device", ex);
                }
            }
        }

        /// <summary>
        /// Handles <see cref="SearchCommand"/>.
        /// </summary>
        /// <param name="parameter">string value to search for in measurement collection.</param>
        public void Search(object parameter)
        {
            if ((object)parameter != null && !string.IsNullOrEmpty(parameter.ToString()))
            {
                SetCurrentPageNumber(1);
                m_searchText = parameter.ToString();
                ItemsKeys = null;
                Load();
            }
        }

        /// <summary>
        /// Handles <see cref="ShowAllCommand"/>.
        /// </summary>
        public void ShowAll()
        {
            if (!ReferenceEquals(AllKeys, ItemsKeys))
            {
                SetCurrentPageNumber(1);
                m_searchText = null;
                ItemsKeys = AllKeys;
                Load();
            }
        }

        private void ConfigureConcentrator()
        {
            if ((object)CurrentItem.ParentID != null && CurrentItem.ParentID > 0)
            {
                m_stayOnConfigurationScreen = true;

                Device device = Device.GetDevice(null, "WHERE ID = " + CurrentItem.ParentID);

                if ((object)device != null)
                {
                    PdcDevices = Device.GetDevices(null, "WHERE ParentID = " + CurrentItem.ParentID);
                    CurrentItem = device;
                }
            }
        }

        #endregion
    }
}
