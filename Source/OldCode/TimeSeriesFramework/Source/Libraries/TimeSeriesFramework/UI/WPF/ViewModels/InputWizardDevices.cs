//******************************************************************************************************
//  InputWizardDevices.cs - Gbtc
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
//  05/24/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI.DataModels;
using TVA;
using TVA.Data;
using TVA.PhasorProtocols;
using TVA.Services.ServiceProcess;

namespace TimeSeriesFramework.UI.ViewModels
{
    /// <summary>
    /// Bindable class to hold collection of <see cref="InputWizardDevice"/>.
    /// </summary>
    internal class InputWizardDevices : PagedViewModelBase<InputWizardDevice, string>
    {
        #region [ Members ]

        // Fields
        private RelayCommand m_browseConnectionFileCommand;
        private RelayCommand m_buildConnectionStringCommand;
        private RelayCommand m_buildAlternateCommandChannelCommand;
        private RelayCommand m_browseIniFileCommand;
        private RelayCommand m_browseConfigurationFileCommand;
        private RelayCommand m_requestConfigurationCommand;
        private RelayCommand m_saveConfigurationFileCommand;
        private string m_connectionString;
        private string m_alternateCommandChannel;
        private int m_accessID;
        private int m_protocolID;
        private string m_protocolAcronym;
        private bool m_connectToConcentrator;
        private int? m_pdcID;
        private string m_pdcAcronym;
        private string m_pdcName;
        private int m_pdcVendorDeviceID;
        private int m_companyID;
        private int m_historianID;
        private int m_interconnectionID;
        private bool m_skipDisableRealTimeData;
        private Dictionary<int, string> m_companyLookupList;
        private Dictionary<int, string> m_historianLookupList;
        private Dictionary<int, string> m_interconnectionLookupList;
        private Dictionary<int, string> m_protocolLookupList;
        private Dictionary<int, string> m_vendorDeviceLookupList;
        private string m_connectionFileName;
        private string m_configurationFileName;
        private string m_iniFileName;
        private string m_pdcMessage;
        private string m_configurationSummary;
        private IConfigurationFrame m_configurationFrame;
        private string m_requestConfigurationError;
        private object m_requestConfigurationAttachment;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return string.IsNullOrEmpty(CurrentItem.Acronym);
            }
        }

        /// <summary>
        /// Gets or sets name of the connection file used for configuration.
        /// </summary>
        public string ConnectionFileName
        {
            get
            {
                return m_connectionFileName;
            }
            set
            {
                m_connectionFileName = value;
                OnPropertyChanged("ConnectionFileName");
            }
        }

        /// <summary>
        /// Gets or sets name of the xml configuration file used for configuration.
        /// </summary>
        public string ConfigurationFileName
        {
            get
            {
                return m_configurationFileName;
            }
            set
            {
                m_configurationFileName = value;
                OnPropertyChanged("ConfigurationFileName");
            }
        }

        /// <summary>
        /// Gets or sets name of the INI file used for configuration.
        /// </summary>
        public string IniFileName
        {
            get
            {
                return m_iniFileName;
            }
            set
            {
                m_iniFileName = value;
                OnPropertyChanged("IniFileName");
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
        /// Gets or sets connection string to backend service.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
                OnPropertyChanged("ConnectionString");
            }
        }

        /// <summary>
        /// Gets or sets command channel to communicate with backend service.
        /// </summary>
        public string AlternateCommandChannel
        {
            get
            {
                return m_alternateCommandChannel;
            }
            set
            {
                m_alternateCommandChannel = value;
                OnPropertyChanged("AlternateCommandChannel");
            }
        }

        /// <summary>
        /// Gets or sets ID of the device to be configured.
        /// </summary>
        public int AccessID
        {
            get
            {
                return m_accessID;
            }
            set
            {
                m_accessID = value;
                OnPropertyChanged("AccessID");
            }
        }

        /// <summary>
        /// Gets or sets protocol id for devices to be configured.
        /// </summary>
        public int ProtocolID
        {
            get
            {
                return m_protocolID;
            }
            set
            {
                m_protocolID = value;
                OnPropertyChanged("ProtocolID");
            }
        }

        /// <summary>
        /// Gets or sets protocol acronym for devices to be configured.
        /// </summary>
        public string ProtocolAcronym
        {
            get
            {
                return m_protocolAcronym;
            }
            set
            {
                m_protocolAcronym = value;
                OnPropertyChanged("ProtocolAcronym");
            }
        }


        /// <summary>
        /// Gets or sets boolean value indicating if connection is to concentrator.
        /// </summary>
        public bool ConnectToConcentrator
        {
            get
            {
                return m_connectToConcentrator;
            }
            set
            {
                m_connectToConcentrator = value;
                OnPropertyChanged("ConnectToConcentrator");
            }
        }

        /// <summary>
        /// Gets or sets ID of the concetrator device.
        /// </summary>
        public int? PdcID
        {
            get
            {
                return m_pdcID;
            }
            set
            {
                m_pdcID = value;
                OnPropertyChanged("PdcID");
            }
        }

        /// <summary>
        /// Gets or setc acronym of the concentrator device.
        /// </summary>
        public string PdcAcronym
        {
            get
            {
                return m_pdcAcronym;
            }
            set
            {
                m_pdcAcronym = value;
                OnPropertyChanged("PdcAcronym");

                // Everytime acronym changes, check in the database to see if it already exists.
                PdcMessage = "";
                Device device = Device.GetDevice(null, " WHERE Acronym = '" + m_pdcAcronym.ToUpper() + "'");
                if (device != null)
                {
                    if (device.IsConcentrator)
                    {
                        PdcID = device.ID;
                        PdcMessage = "PDC already exists in the database. All devices will be assigned to this PDC.";
                    }
                    else
                    {
                        PdcMessage = "A non-PDC device with the same acronym exists in the database. Please change acronym.";
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets message to be displayed on the screen when pdc acronym changes.
        /// </summary>
        public string PdcMessage
        {
            get
            {
                return m_pdcMessage;
            }
            set
            {
                m_pdcMessage = value;
                OnPropertyChanged("PdcMessage");
            }
        }

        /// <summary>
        /// Gets or sets name of the concentrator device.
        /// </summary>
        public string PdcName
        {
            get
            {
                return m_pdcName;
            }
            set
            {
                m_pdcName = value;
                OnPropertyChanged("PdcName");
            }
        }

        /// <summary>
        /// Gets or sets vendor device id for this configuration.
        /// </summary>
        public int PdcVendorDeviceID
        {
            get
            {
                return m_pdcVendorDeviceID;
            }
            set
            {
                m_pdcVendorDeviceID = value;
                OnPropertyChanged("PdcVendorDeviceID");
            }
        }

        /// <summary>
        /// Gets or sets company ID to which devices are to be associated in this configuration.
        /// </summary>
        public int CompanyID
        {
            get
            {
                return m_companyID;
            }
            set
            {
                m_companyID = value;
                OnPropertyChanged("CompanyID");
            }
        }

        /// <summary>
        /// Gets or sets Historian ID to which devices are to be associated in this configuration.
        /// </summary>
        public int HistorianID
        {
            get
            {
                return m_historianID;
            }
            set
            {
                m_historianID = value;
                OnPropertyChanged("HistorianID");
            }
        }

        /// <summary>
        /// Gets or sets Interconnection ID to which devices are to be associated in this configuration.
        /// </summary>
        public int InterconnectionID
        {
            get
            {
                return m_interconnectionID;
            }
            set
            {
                m_interconnectionID = value;
                OnPropertyChanged("InterconnectionID");
            }
        }

        /// <summary>
        /// Gets or sets a boolean flag for SkipDisableRealTimeData.
        /// </summary>
        public bool SkipDisableRealTimeData
        {
            get
            {
                return m_skipDisableRealTimeData;
            }
            set
            {
                m_skipDisableRealTimeData = value;
                OnPropertyChanged("SkipDisableRealTimeData");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to browse to connection file.
        /// </summary>
        public ICommand BrowseConnectionFileCommand
        {
            get
            {
                if (m_browseConnectionFileCommand == null)
                    m_browseConnectionFileCommand = new RelayCommand(BrowseConnectionFile, (param) => CanSave);

                return m_browseConnectionFileCommand;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to build connection string.
        /// </summary>
        public ICommand BuildConnectionStringCommand
        {
            get
            {
                if (m_buildConnectionStringCommand == null)
                    m_buildConnectionStringCommand = new RelayCommand(BuildConnectionString, (param) => CanSave);

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
                if (m_buildAlternateCommandChannelCommand == null)
                    m_buildAlternateCommandChannelCommand = new RelayCommand(BuildAlternateCommandChannel, (param) => CanSave);

                return m_buildAlternateCommandChannelCommand;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to browse to INI file.
        /// </summary>
        public ICommand BrowseIniFileCommand
        {
            get
            {
                if (m_browseIniFileCommand == null)
                    m_browseIniFileCommand = new RelayCommand(BrowseIniFile, (param) => CanSave);

                return m_browseIniFileCommand;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to browse to configuration file.
        /// </summary>
        public ICommand BrowseConfigurationFileCommand
        {
            get
            {
                if (m_browseConfigurationFileCommand == null)
                    m_browseConfigurationFileCommand = new RelayCommand(BrowseConfigurationFile, (param) => CanSave);

                return m_browseConfigurationFileCommand;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ICommand"/> to request configuration frame from backed service.
        /// </summary>
        public ICommand RequestConfigurationCommand
        {
            get
            {
                if (m_requestConfigurationCommand == null)
                    m_requestConfigurationCommand = new RelayCommand(RequestConfiguration, (param) => CanSave);

                return m_requestConfigurationCommand;
            }
        }

        public ICommand SaveConfigurationFileCommand
        {
            get
            {
                if (m_saveConfigurationFileCommand == null)
                    m_saveConfigurationFileCommand = new RelayCommand(SaveConfigurationFile, (param) => CanSave);

                return m_saveConfigurationFileCommand;
            }
        }

        /// <summary>
        /// Gets or sets summary message to be displayed on UI aftern parsing configuration file or frame.
        /// </summary>
        public string ConfigurationSummary
        {
            get
            {
                return m_configurationSummary;
            }
            set
            {
                m_configurationSummary = value;
                OnPropertyChanged("ConfigurationSummary");
            }
        }

        /// <summary>
        /// Gets boolean flag indicating if configuration can be saved or not.
        /// </summary>
        public override bool CanSave
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="InputWizardDevices"/> class.
        /// </summary>
        /// <param name="itemsPerPage">Integer value to determine number of items per page.</param>
        /// <param name="autoSave">Boolean value to determine is user changes should be saved automatically.</param>
        public InputWizardDevices(int itemsPerPage, bool autoSave = true)
            : base(itemsPerPage, autoSave)
        {
            m_companyLookupList = Company.GetLookupList(null, true);
            m_historianLookupList = Historian.GetLookupList(null, true, false);
            m_interconnectionLookupList = Interconnection.GetLookupList(null, true);
            m_protocolLookupList = Protocol.GetLookupList(null, true);
            m_vendorDeviceLookupList = VendorDevice.GetLookupList(null, true);

            if (m_companyLookupList.Count > 0)
                CompanyID = m_companyLookupList.First().Key;

            if (m_historianLookupList.Count > 0)
                HistorianID = m_historianLookupList.First().Key;

            if (m_interconnectionLookupList.Count > 0)
                InterconnectionID = m_interconnectionLookupList.First().Key;

            if (m_protocolLookupList.Count > 0)
                ProtocolID = m_protocolLookupList.First().Key;

            if (m_vendorDeviceLookupList.Count > 0)
                PdcVendorDeviceID = m_vendorDeviceLookupList.First().Key;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemKey()
        {
            return CurrentItem.Acronym;
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
        /// Handles BrowseConnectionFileCommand.
        /// </summary>
        /// <param name="parameter">Parameter to be used in command execution.</param>
        private void BrowseConnectionFile(object parameter)
        {
            Stream fileData = null;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "PMU Connection Files (*.PmuConnection)|*.PmuConnection|All Files (*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    ConnectionFileName = fileDialog.FileName;
                    if ((fileData = fileDialog.OpenFile()) != null)
                    {
                        ConnectionSettings connectionSettings = new ConnectionSettings();
                        using (fileData)
                        {
                            SoapFormatter sf = new SoapFormatter();
                            sf.AssemblyFormat = FormatterAssemblyStyle.Simple;
                            sf.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
                            sf.Binder = new VersionConfigToNamespaceAssemblyObjectBinder();
                            connectionSettings = sf.Deserialize(fileData) as ConnectionSettings;

                            if (connectionSettings.ConnectionParameters != null)
                            {
                                ConnectionSettings cs = new ConnectionSettings();
                                cs = (ConnectionSettings)connectionSettings.ConnectionParameters;
                                connectionSettings.ConfigurationFileName = cs.ConfigurationFileName;
                                connectionSettings.RefreshConfigurationFileOnChange = cs.RefreshConfigurationFileOnChange;
                                connectionSettings.ParseWordCountFromByte = cs.ParseWordCountFromByte;
                            }
                        }

                        if (connectionSettings != null)
                        {
                            ConnectionString = connectionSettings.ConnectionString.ToLower();
                            Dictionary<string, string> connectionStringKeyValues = ConnectionString.ParseKeyValuePairs();

                            if (connectionStringKeyValues.ContainsKey("commandchannel"))
                            {
                                AlternateCommandChannel = connectionStringKeyValues["commandchannel"];
                                connectionStringKeyValues.Remove("commandchannel");
                            }

                            if (connectionStringKeyValues.ContainsKey("skipdisablerealtimedata"))
                            {
                                SkipDisableRealTimeData = Convert.ToBoolean(connectionStringKeyValues["skipdisablerealtimedata"]);
                                connectionStringKeyValues.Remove("skipdisablerealtimedata");
                            }

                            ConnectionString = "transportprotocol=" + connectionSettings.TransportProtocol.ToString() + ";" + connectionStringKeyValues.JoinKeyValuePairs();

                            if (connectionSettings.ConnectionParameters != null)
                                ConnectionString += ";inifilename=" + connectionSettings.ConfigurationFileName + ";refreshconfigfileonchange=" + connectionSettings.RefreshConfigurationFileOnChange.ToString() +
                                    ";parsewordcountfrombyte=" + connectionSettings.ParseWordCountFromByte;

                            AccessID = connectionSettings.PmuID;
                            ProtocolAcronym = connectionSettings.PhasorProtocol.ToString();
                            if (m_protocolLookupList.Count > 0)
                                ProtocolID = m_protocolLookupList.FirstOrDefault(p => p.Value == connectionSettings.PhasorProtocol.ToString()).Key;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Popup(ex.Message, "Open Connection File", MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// Handles BrowseConfigurationFileCommand.
        /// </summary>
        /// <param name="parameter">Parameter to be used in command execution.</param>
        private void BrowseConfigurationFile(object parameter)
        {
            Stream fileData = null;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    ConfigurationFileName = fileDialog.FileName;
                    ConfigurationSummary = string.Empty;

                    if ((fileData = fileDialog.OpenFile()) != null)
                    {
                        SoapFormatter sf = new SoapFormatter();
                        sf.AssemblyFormat = FormatterAssemblyStyle.Simple;
                        sf.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
                        m_configurationFrame = sf.Deserialize(fileData) as IConfigurationFrame;
                        ParseConfiguration();
                    }
                }
                catch (Exception ex)
                {
                    Popup(ex.Message, "Open Configuration File", MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// Parses IConfigurationFrame to retrieve devices and other configuration information.
        /// </summary>        
        private void ParseConfiguration()
        {
            ObservableCollection<InputWizardDevice> wizardDeviceList = new ObservableCollection<InputWizardDevice>();

            if (m_configurationFrame != null)
            {
                foreach (IConfigurationCell cell in m_configurationFrame.Cells)
                {
                    Device existingDevice = Device.GetDevice(null, "WHERE Acronym = '" + cell.StationName.Replace(" ", "").ToUpper() + "'");

                    wizardDeviceList.Add(new InputWizardDevice()
                    {
                        Acronym = cell.StationName.Replace(" ", "").ToUpper(),
                        Name = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(cell.StationName.ToLower()),
                        Longitude = existingDevice == null ? -98.6m : existingDevice.Longitude == null ? -98.6m : (decimal)existingDevice.Longitude,
                        Latitude = existingDevice == null ? 37.5m : existingDevice.Latitude == null ? 37.5m : (decimal)existingDevice.Latitude,
                        VendorDeviceID = existingDevice == null ? (int?)null : existingDevice.VendorDeviceID,
                        AccessID = cell.IDCode,
                        ParentAccessID = m_configurationFrame.IDCode,
                        Include = true,
                        DigitalCount = cell.DigitalDefinitions.Count,
                        AnalogCount = cell.AnalogDefinitions.Count,
                        AddDigitals = false,
                        AddAnalogs = false,
                        Existing = existingDevice == null ? false : true,
                        PhasorList = new ObservableCollection<InputWizardDevicePhasor>((from phasor in cell.PhasorDefinitions
                                                                                        select new InputWizardDevicePhasor()
                                                                                        {
                                                                                            Label = phasor.Label,
                                                                                            Type = phasor.PhasorType == PhasorType.Current ? "I" : "V",
                                                                                            Phase = "+",
                                                                                            //DestinationLabel = "",
                                                                                            Include = true
                                                                                        }).ToList())
                    });
                }
            }

            ItemsSource = wizardDeviceList;

            ConfigurationSummary = "Current Configuration Summary: " + wizardDeviceList.Count;

            if (wizardDeviceList.Count > 1)
            {
                ConfigurationSummary += " Devices. Please provide PDC information below.";
                ConnectToConcentrator = true;
            }
            else
            {
                ConfigurationSummary += " Device";
                ConnectToConcentrator = false;
            }
        }

        private void SaveConfigurationFile(object parameter)
        {
            try
            {
                if (m_configurationFrame != null)
                {
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Title = "Save Current Configuration";
                    saveDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                    saveDialog.FileName = "";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (FileStream stream = File.Create(saveDialog.FileName))
                        {
                            SoapFormatter sf = new SoapFormatter();
                            sf.AssemblyFormat = FormatterAssemblyStyle.Simple;
                            sf.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
                            sf.Serialize(stream, m_configurationFrame);
                            Popup("Configuration file saved successfully.", "Save Current Configuration", MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    Popup("Current configuration is undefined and cannot be saved.", "Save Current Configuration", MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Save Current Configuration", MessageBoxImage.Error);
            }
        }

        private void BrowseIniFile(object parameter)
        {

        }

        private void BuildConnectionString(object parameter)
        {

        }

        private void BuildAlternateCommandChannel(object parameter)
        {

        }

        /// <summary>
        /// Handles RequestConfigurationCommand.
        /// </summary>
        /// <param name="parameter">Parameters to be used in command execution.</param>
        private void RequestConfiguration(object parameter)
        {
            m_requestConfigurationError = string.Empty;
            AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
            WindowsServiceClient windowsServiceClient = null;

            try
            {
                s_responseWaitHandle = new ManualResetEvent(false);

                windowsServiceClient = CommonFunctions.GetWindowsServiceClient();
                if (windowsServiceClient != null && windowsServiceClient.Helper != null &&
                   windowsServiceClient.Helper.RemotingClient != null && windowsServiceClient.Helper.RemotingClient.CurrentState == TVA.Communication.ClientState.Connected)
                {
                    windowsServiceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                    windowsServiceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;

                    string connectionString = GenerateConnectionString();
                    if (!connectionString.EndsWith(";"))
                        connectionString += ";";
                    connectionString += "accessid=" + AccessID + ";";

                    if (!connectionString.ToLower().Contains("phasorprotocol"))
                        connectionString += "phasorprotocol=" + ProtocolAcronym + ";";

                    windowsServiceClient.Helper.SendRequest(string.Format("invoke 0 requestdeviceconfiguration \"{0}\"", connectionString));

                    if (s_responseWaitHandle.WaitOne(65000))
                    {
                        if (m_requestConfigurationAttachment is ConfigurationErrorFrame)
                        {
                            throw new ApplicationException("Received configuration error frame." + Environment.NewLine + m_requestConfigurationError);
                        }
                        else if (m_requestConfigurationAttachment is IConfigurationFrame)
                        {
                            m_configurationFrame = m_requestConfigurationAttachment as IConfigurationFrame;
                            ParseConfiguration();
                        }
                        else
                        {
                            throw new ApplicationException("Invalid frame received, invocation for device configuration has failed.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Response timeout occured. Waited 60 seconds for Configuration Frame to arrive.");
                    }
                }
                else
                {
                    throw new ApplicationException("Connection timeout occured. Tried 10 times to connect to windows service.");
                }
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Request Configuration", MessageBoxImage.Error);
            }
            finally
            {
                if (database != null)
                    database.Dispose();

                if (windowsServiceClient != null)
                {
                    windowsServiceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
                    windowsServiceClient.Helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;
                }
            }
        }

        private void Helper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            if (e.Argument2.StartsWith("[PHASOR!SERVICES]") && !e.Argument2.Contains("*"))
                m_requestConfigurationError += e.Argument2.Replace("[PHASOR!SERVICES]", "").Replace("\r\n\r\n", "\r\n");
        }

        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            List<object> attachments = e.Argument.Attachments;

            // Handle any special attachments coming in from service
            if (attachments != null)
            {
                foreach (object attachment in attachments)
                {
                    m_requestConfigurationAttachment = attachment;
                    s_responseWaitHandle.Set();
                }
            }
        }

        public void SavePDC()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            try
            {
                if (ConnectToConcentrator && (PdcID == null || PdcID == 0))
                {
                    Device device = new Device();
                    device.IsConcentrator = true;
                    device.Acronym = PdcAcronym.ToUpper();
                    device.Name = PdcName;
                    device.ParentID = null;
                    device.AccessID = AccessID;
                    device.CompanyID = CompanyID == 0 ? (int?)null : CompanyID;
                    device.HistorianID = HistorianID == 0 ? (int?)null : HistorianID;
                    device.ProtocolID = ProtocolID == 0 ? (int?)null : ProtocolID;
                    device.InterconnectionID = InterconnectionID == 0 ? (int?)null : InterconnectionID;
                    device.AccessID = AccessID;
                    device.SkipDisableRealTimeData = SkipDisableRealTimeData;
                    device.ConnectionString = GenerateConnectionString();
                    device.Enabled = true;
                    Device.Save(null, device);

                    device = Device.GetDevice(null, "WHERE Acronym = '" + PdcAcronym.ToUpper() + "'");
                    PdcID = device.ID;
                }
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Save PDC information", MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        public void SaveConfiguration()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
            try
            {
                int deviceCount = 0;
                foreach (InputWizardDevice inputWizardDevice in ItemsSource)
                {
                    if (inputWizardDevice.Include)
                    {
                        Device device = Device.GetDevice(database, "WHERE Acronym = '" + inputWizardDevice.Acronym.ToUpper() + "'");
                        if (device == null)
                        {
                            device = new Device();
                            device.Acronym = inputWizardDevice.Acronym.ToUpper();
                        }

                        device.Name = inputWizardDevice.Name;
                        device.CompanyID = CompanyID == 0 ? (int?)null : CompanyID;
                        device.HistorianID = HistorianID == 0 ? (int?)null : HistorianID;
                        device.ProtocolID = ProtocolID == 0 ? (int?)null : ProtocolID;
                        device.InterconnectionID = InterconnectionID == 0 ? (int?)null : InterconnectionID;
                        device.AccessID = ItemsSource.Count == 1 ? AccessID : inputWizardDevice.AccessID;
                        device.SkipDisableRealTimeData = SkipDisableRealTimeData;
                        device.Enabled = true;
                        device.Longitude = inputWizardDevice.Longitude;
                        device.Latitude = inputWizardDevice.Latitude;

                        device.IsConcentrator = false;
                        device.LoadOrder = deviceCount;
                        if (ConnectToConcentrator && PdcID != null && PdcID > 0)
                        {
                            device.ParentID = PdcID;
                            device.ConnectionString = string.Empty;
                        }
                        else
                            device.ConnectionString = GenerateConnectionString();

                        Device.Save(database, device);

                        if (device.ID == 0)
                            device.ID = Device.GetDevice(database, "WHERE Acronym = '" + inputWizardDevice.Acronym.ToUpper() + "'").ID;

                        int phasorCount = 1;
                        foreach (InputWizardDevicePhasor inputWizardDevicePhasor in inputWizardDevice.PhasorList)
                        {
                            Phasor phasor = Phasor.GetPhasor(database, "WHERE DeviceID = " + device.ID + " AND SourceIndex = " + phasorCount);

                            if (phasor == null)
                            {
                                phasor = new Phasor();
                            }

                            phasor.DeviceID = device.ID;
                            phasor.SourceIndex = phasorCount;
                            phasor.Label = inputWizardDevicePhasor.Label;
                            phasor.Type = inputWizardDevicePhasor.Type;
                            phasor.Phase = inputWizardDevicePhasor.Phase;
                            Phasor.Save(database, phasor);

                            phasorCount++;
                        }
                    }
                    deviceCount++;
                }

                Popup("Configuration information saved successfully.", "Input Wizard Configuration", MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Input Wizard Configuration", MessageBoxImage.Error);
            }
            finally
            {
                if (database != null)
                    database.Dispose();

                Mouse.OverrideCursor = null;
            }
        }

        private string GenerateConnectionString()
        {
            string connectionString = ConnectionString;

            if (!string.IsNullOrEmpty(AlternateCommandChannel))
            {
                if (!connectionString.EndsWith(";"))
                    connectionString += ";";

                connectionString += "commandchannel={" + AlternateCommandChannel + "}";
            }


            return connectionString;
        }

        #endregion

        #region [ Static ]

        // Fields

        private static ManualResetEvent s_responseWaitHandle;

        #endregion
    }
}
