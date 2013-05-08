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
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GSF.Communication;
using GSF.Data;
using GSF.PhasorProtocols.BpaPdcStream;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.Modal;
using GSF.PhasorProtocols.UI.UserControls;
using GSF.ServiceProcess;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.DataModels;
using PhasorProtocolAdapters;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;

namespace GSF.PhasorProtocols.UI.ViewModels
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
        private RelayCommand m_manualConfigurationCommand;
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
        private readonly Dictionary<int, string> m_companyLookupList;
        private readonly Dictionary<int, string> m_historianLookupList;
        private readonly Dictionary<int, string> m_interconnectionLookupList;
        private readonly Dictionary<int, string> m_protocolLookupList;
        private readonly ObservableCollection<Protocol> m_protocolList;
        private readonly Dictionary<int, string> m_vendorDeviceLookupList;
        private string m_connectionFileName;
        private string m_configurationFileName;
        private string m_iniFileName;
        private string m_pdcMessage;
        private string m_configurationSummary;
        private IConfigurationFrame m_configurationFrame;
        private string m_requestConfigurationError;
        private object m_requestConfigurationAttachment;
        private Device m_pdcDevice;
        private int m_currentDeviceRuntimeID;
        private bool m_disconnectedCurrentDevice;
        private bool m_newDeviceConfiguration;

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
                ProtocolAcronym = m_protocolLookupList[m_protocolID];
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
                OnPropertyChanged("ProtocolIsBpaPdcStream");
            }
        }

        /// <summary>
        /// Gets a flag indicating if selected protocol is BpaPdcStream. 
        /// This is used to hide or display Ini file selection on input wizard screen.
        /// </summary>
        public bool ProtocolIsBpaPdcStream
        {
            get
            {
                if (!string.IsNullOrEmpty(m_protocolAcronym))
                    return (m_protocolAcronym.ToLower() == "bpapdcstream");
                else
                    return false;
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
                m_pdcAcronym = value.Replace(" ", "").Replace("'", "").ToUpper();
                OnPropertyChanged("PdcAcronym");

                // Everytime acronym changes, check in the database to see if it already exists.
                // Checks if the Acronym is duplicated or No, If no then assigns that to zero.
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
                else
                    PdcID = 0;
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
                    m_browseConnectionFileCommand = new RelayCommand(BrowseConnectionFile, () => CanSave);

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
                    m_buildConnectionStringCommand = new RelayCommand(BuildConnectionString, () => CanSave);

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
                    m_buildAlternateCommandChannelCommand = new RelayCommand(BuildAlternateCommandChannel, () => CanSave);

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
                    m_browseIniFileCommand = new RelayCommand(BrowseIniFile, () => CanSave);

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
                    m_browseConfigurationFileCommand = new RelayCommand(BrowseConfigurationFile, () => CanSave);

                return m_browseConfigurationFileCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to request configuration frame from backed service.
        /// </summary>
        public ICommand RequestConfigurationCommand
        {
            get
            {
                if (m_requestConfigurationCommand == null)
                    m_requestConfigurationCommand = new RelayCommand(RequestConfiguration, () => CanSave);

                return m_requestConfigurationCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to save current configuration to XML file.
        /// </summary>
        public ICommand SaveConfigurationFileCommand
        {
            get
            {
                if (m_saveConfigurationFileCommand == null)
                    m_saveConfigurationFileCommand = new RelayCommand(SaveConfigurationFile, () => CanSave);

                return m_saveConfigurationFileCommand;
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to create or update configuration manually.
        /// </summary>
        public ICommand ManualConfigurationCommand
        {
            get
            {
                if (m_manualConfigurationCommand == null)
                    m_manualConfigurationCommand = new RelayCommand(ManualConfiguration, () => CanSave);

                return m_manualConfigurationCommand;
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
                return CommonFunctions.CurrentPrincipal.IsInRole("Administrator, Editor");
            }
        }

        /// <summary>
        /// Gets or sets ID of the device in case where configuration is being updated.
        /// </summary>
        public int CurrentDeviceRuntimeID
        {
            get
            {
                return m_currentDeviceRuntimeID;
            }
            set
            {
                m_currentDeviceRuntimeID = value;
            }
        }

        /// <summary>
        /// Gets a flag indicating if current device was disconnected before requesting configuration.
        /// </summary>
        public bool DisconnectedCurrentDevice
        {
            get
            {
                return m_disconnectedCurrentDevice;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the wizard was launched in
        /// order to set up a new device configuration or update an existing one.
        /// </summary>
        public bool NewDeviceConfiguration
        {
            get
            {
                return m_newDeviceConfiguration;
            }
            set
            {
                m_newDeviceConfiguration = value;
                OnPropertyChanged("NewDeviceConfiguration");
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
            m_historianLookupList = TimeSeries.UI.DataModels.Historian.GetLookupList(null, true, false);
            m_interconnectionLookupList = Interconnection.GetLookupList(null, true);
            m_protocolLookupList = Protocol.GetLookupList(null, false);
            m_vendorDeviceLookupList = VendorDevice.GetLookupList(null, true);
            m_protocolList = Protocol.Load(null);
            NewDeviceConfiguration = true;

            if (m_companyLookupList.Count > 0)
                CompanyID = m_companyLookupList.First().Key;

            if (m_historianLookupList.Count > 1)
                HistorianID = m_historianLookupList.Skip(1).First().Key;
            else if (m_historianLookupList.Count > 0)
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
        private void BrowseConnectionFile()
        {
            Stream fileData = null;
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Multiselect = false;
            fileDialog.Filter = "PMU Connection Files (*.PmuConnection)|*.PmuConnection|All Files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    ConnectionFileName = fileDialog.FileName;

                    if ((fileData = fileDialog.OpenFile()) != null)
                    {
                        ConnectionSettings connectionSettings = new ConnectionSettings();

                        using (fileData)
                        {
                            SoapFormatter sf = new SoapFormatter();
                            sf.AssemblyFormat = FormatterAssemblyStyle.Simple;
                            sf.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
                            sf.Binder = Serialization.LegacyBinder;
                            connectionSettings = sf.Deserialize(fileData) as ConnectionSettings;
                        }

                        if (connectionSettings != null)
                        {
                            ConnectionString = connectionSettings.ConnectionString;
                            Dictionary<string, string> connectionStringKeyValues = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                            connectionStringKeyValues = ConnectionString.ParseKeyValuePairs();

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

                            if ((object)connectionSettings.ConnectionParameters != null)
                            {
                                switch (connectionSettings.PhasorProtocol)
                                {
                                    case PhasorProtocol.BpaPdcStream:
                                        ConnectionParameters bpaParameters = connectionSettings.ConnectionParameters as ConnectionParameters;
                                        if ((object)bpaParameters != null)
                                            ConnectionString += "; iniFileName=" + bpaParameters.ConfigurationFileName + "; refreshConfigFileOnChange=" + bpaParameters.RefreshConfigurationFileOnChange + "; parseWordCountFromByte=" + bpaParameters.ParseWordCountFromByte;
                                        break;
                                    case PhasorProtocol.FNet:
                                        FNet.ConnectionParameters fnetParameters = connectionSettings.ConnectionParameters as FNet.ConnectionParameters;
                                        if ((object)fnetParameters != null)
                                            ConnectionString += "; timeOffset=" + fnetParameters.TimeOffset + "; stationName=" + fnetParameters.StationName + "; frameRate=" + fnetParameters.FrameRate + "; nominalFrequency=" + (int)fnetParameters.NominalFrequency;
                                        break;
                                    case PhasorProtocol.SelFastMessage:
                                        SelFastMessage.ConnectionParameters selParameters = connectionSettings.ConnectionParameters as SelFastMessage.ConnectionParameters;
                                        if ((object)selParameters != null)
                                            ConnectionString += "; messagePeriod=" + selParameters.MessagePeriod;
                                        break;
                                    case PhasorProtocol.Iec61850_90_5:
                                        Iec61850_90_5.ConnectionParameters iecParameters = connectionSettings.ConnectionParameters as Iec61850_90_5.ConnectionParameters;
                                        if ((object)iecParameters != null)
                                            ConnectionString += "; useETRConfiguration=" + iecParameters.UseETRConfiguration + "; guessConfiguration=" + iecParameters.GuessConfiguration + "; parseRedundantASDUs=" + iecParameters.ParseRedundantASDUs + "; ignoreSignatureValidationFailures=" + iecParameters.IgnoreSignatureValidationFailures + "; ignoreSampleSizeValidationFailures=" + iecParameters.IgnoreSampleSizeValidationFailures;
                                        break;
                                    case PhasorProtocol.Macrodyne:
                                        Macrodyne.ConnectionParameters macrodyneParameters = connectionSettings.ConnectionParameters as Macrodyne.ConnectionParameters;
                                        if ((object)macrodyneParameters != null)
                                            ConnectionString += "; protocolVersion=" + macrodyneParameters.ProtocolVersion + "; iniFileName=" + macrodyneParameters.ConfigurationFileName + "; refreshConfigFileOnChange=" + macrodyneParameters.RefreshConfigurationFileOnChange + "; deviceLabel=" + macrodyneParameters.DeviceLabel;
                                        break;
                                }
                            }

                            AccessID = connectionSettings.PmuID;
                            ProtocolAcronym = connectionSettings.PhasorProtocol.ToString();

                            if (m_protocolLookupList.Count > 0)
                                ProtocolID = m_protocolList.FirstOrDefault(p => p.Acronym.ToLower() == connectionSettings.PhasorProtocol.ToString().ToLower()).ID; // m_protocolLookupList.FirstOrDefault(p => p.Value == connectionSettings.PhasorProtocol.ToString()).Key;
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
        private void BrowseConfigurationFile()
        {
            Stream fileData = null;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    ConfigurationFileName = fileDialog.FileName;
                    ConfigurationSummary = string.Empty;

                    if ((fileData = fileDialog.OpenFile()) != null)
                    {
                        SoapFormatter sf = new SoapFormatter();
                        sf.AssemblyFormat = FormatterAssemblyStyle.Simple;
                        sf.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
                        sf.Binder = Serialization.LegacyBinder;

                        // If protocol is BpaPdcStream and INI file path is provided then replace existing path with the provided one.
                        if (ProtocolIsBpaPdcStream && !string.IsNullOrEmpty(IniFileName))
                        {
                            try
                            {
                                string configFileDataString = (new StreamReader(fileData)).ReadToEnd();
                                string leftPart = configFileDataString.Substring(0, configFileDataString.IndexOf("</configurationFileName>"));
                                string rightPart = configFileDataString.Substring(configFileDataString.IndexOf("</configurationFileName>"));
                                leftPart = leftPart.Substring(0, leftPart.LastIndexOf(">") + 1);
                                configFileDataString = leftPart + m_iniFileName + rightPart;

                                Byte[] fileBytes = Encoding.UTF8.GetBytes(configFileDataString);
                                MemoryStream ms = new MemoryStream();
                                ms.Write(fileBytes, 0, fileBytes.Length);
                                ms.Position = 0;
                                m_configurationFrame = sf.Deserialize(ms) as IConfigurationFrame;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error occured in processing INI file. " + ex.Message);
                            }
                        }
                        else
                        {
                            m_configurationFrame = sf.Deserialize(fileData) as IConfigurationFrame;
                        }

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
        private void ParseConfiguration(bool displayPopup = true)
        {
            ObservableCollection<InputWizardDevice> wizardDeviceList = new ObservableCollection<InputWizardDevice>();

            if (m_configurationFrame != null)
            {
                foreach (IConfigurationCell cell in m_configurationFrame.Cells)
                {
                    Device existingDevice = Device.GetDevice(null, "WHERE Acronym = '" + cell.StationName.Replace(" ", "").Replace("'", "").ToUpper() + "'");

                    wizardDeviceList.Add(new InputWizardDevice
                        {
                        Acronym = cell.StationName.Replace(" ", "").Replace("'", "").ToUpper(),
                        Name = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(cell.StationName.ToLower()),
                        Longitude = existingDevice == null ? -98.6m : existingDevice.Longitude == null ? -98.6m : (decimal)existingDevice.Longitude,
                        Latitude = existingDevice == null ? 37.5m : existingDevice.Latitude == null ? 37.5m : (decimal)existingDevice.Latitude,
                        VendorDeviceID = existingDevice == null ? (int?)null : existingDevice.VendorDeviceID,
                        AccessID = cell.IDCode,
                        ParentAccessID = m_configurationFrame.IDCode,
                        Include = true,
                        DigitalCount = cell.DigitalDefinitions.Count,
                        AnalogCount = cell.AnalogDefinitions.Count,
                        AddDigitals = cell.DigitalDefinitions.Count > 0 ? true : false,
                        AddAnalogs = cell.AnalogDefinitions.Count > 0 ? true : false,
                        Existing = existingDevice == null ? false : true,
                        DigitalLabels = GetAnalogOrDigitalLables(cell.DigitalDefinitions),
                        AnalogLabels = GetAnalogOrDigitalLables(cell.AnalogDefinitions),
                        PhasorList = new ObservableCollection<InputWizardDevicePhasor>((from phasor in cell.PhasorDefinitions
                                                                                                              select new InputWizardDevicePhasor
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

            if (displayPopup)
                Popup(ConfigurationSummary, "Parsed Configuration Successfully.", MessageBoxImage.Information);
        }

        private List<string> GetAnalogOrDigitalLables(object analogOrDigitalCollection)
        {
            List<string> returnCollection = new List<string>();

            if (analogOrDigitalCollection is DigitalDefinitionCollection)
            {
                DigitalDefinitionCollection collection = (DigitalDefinitionCollection)analogOrDigitalCollection;
                foreach (IDigitalDefinition digital in collection)
                {
                    returnCollection.Add(digital.Label.TruncateRight(256));
                }
            }
            else if (analogOrDigitalCollection is AnalogDefinitionCollection)
            {
                AnalogDefinitionCollection collection = (AnalogDefinitionCollection)analogOrDigitalCollection;
                foreach (IAnalogDefinition analog in collection)
                {
                    returnCollection.Add(analog.Label.TruncateRight(16));
                }
            }

            return returnCollection;
        }

        /// <summary>
        /// Saves current configuration information into XML file.
        /// </summary>
        private void SaveConfigurationFile()
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

        /// <summary>
        /// Handles BroseIniFileCommand.
        /// </summary>
        private void BrowseIniFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    IniFileName = fileDialog.FileName;
                }
                catch (Exception ex)
                {
                    Popup(ex.Message, "Browse to INI File", MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Pops up another window to build or modify connection string.
        /// </summary>
        private void BuildConnectionString()
        {
            ConnectionStringBuilder csb = new ConnectionStringBuilder(ConnectionStringBuilder.ConnectionType.DeviceConnection);
            if (!string.IsNullOrEmpty(ConnectionString))
                csb.ConnectionString = ConnectionString;

            csb.Closed += delegate
                {
                    if ((bool)csb.DialogResult)
                        ConnectionString = csb.ConnectionString;
                };
            csb.Owner = Application.Current.MainWindow;
            csb.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            csb.ShowDialog();
        }

        /// <summary>
        /// Pops up another window to build or modify alternate command channel.
        /// </summary>
        private void BuildAlternateCommandChannel()
        {
            ConnectionStringBuilder csb = new ConnectionStringBuilder(ConnectionStringBuilder.ConnectionType.AlternateCommandChannel);
            if (!string.IsNullOrEmpty(AlternateCommandChannel))
                csb.ConnectionString = AlternateCommandChannel;

            csb.Closed += delegate
                {
                    if ((bool)csb.DialogResult)
                        AlternateCommandChannel = csb.ConnectionString;
                };
            csb.Owner = Application.Current.MainWindow;
            csb.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            csb.ShowDialog();
        }

        /// <summary>
        /// Handles RequestConfigurationCommand.
        /// </summary>        
        private void RequestConfiguration()
        {
            m_requestConfigurationError = string.Empty;
            AdoDataConnection database = null;
            WindowsServiceClient windowsServiceClient = null;

            try
            {
                if (m_currentDeviceRuntimeID > 0)
                {
                    CommonFunctions.SendCommandToService("Disconnect " + m_currentDeviceRuntimeID);
                    m_disconnectedCurrentDevice = true;
                }

                database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                Mouse.OverrideCursor = Cursors.Wait;
                s_responseWaitHandle = new ManualResetEvent(false);

                windowsServiceClient = CommonFunctions.GetWindowsServiceClient();
                if (windowsServiceClient != null && windowsServiceClient.Helper != null &&
                   windowsServiceClient.Helper.RemotingClient != null && windowsServiceClient.Helper.RemotingClient.CurrentState == ClientState.Connected)
                {
                    windowsServiceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                    windowsServiceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;

                    string connectionString = GenerateConnectionString();
                    if (!connectionString.EndsWith(";"))
                        connectionString += ";";
                    connectionString += "accessid=" + AccessID + ";";

                    if (!connectionString.ToLower().Contains("phasorprotocol"))
                        connectionString += "phasorprotocol=" + m_protocolList.FirstOrDefault(p => p.ID == ProtocolID).Acronym + ";";

                    s_responseWaitHandle.Reset();
                    m_requestConfigurationAttachment = null;
                    CommonFunctions.SendCommandToService(string.Format("invoke 0 requestdeviceconfiguration \"{0}\"", connectionString));

                    if (s_responseWaitHandle.WaitOne(65000))
                    {
                        if (m_requestConfigurationAttachment is ConfigurationErrorFrame)
                        {
                            Thread.Sleep(3000);
                            m_requestConfigurationError = "Received configuration error frame." + Environment.NewLine + m_requestConfigurationError;
                            throw new ApplicationException(m_requestConfigurationError);
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
                Mouse.OverrideCursor = null;
                if (database != null)
                    database.Dispose();

                if (windowsServiceClient != null)
                {
                    windowsServiceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
                    windowsServiceClient.Helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;
                }
            }
        }

        /// <summary>
        /// Handles ManualConfigurationCommand.
        /// </summary>
        private void ManualConfiguration()
        {
            ConfigurationCreator cc = new ConfigurationCreator();
            if (m_configurationFrame != null)
                cc.ConfigurationFrame = m_configurationFrame;

            cc.Closed += delegate
                {
                    if ((bool)cc.DialogResult)
                    {
                        m_configurationFrame = cc.ConfigurationFrame;
                        ParseConfiguration(false);
                    }
                };

            cc.Owner = Application.Current.MainWindow;
            cc.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            cc.ShowDialog();
        }

        /// <summary>
        /// Handles ReceivedServiceUpdate event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Helper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            if (e.Argument2.StartsWith("[PHASOR!SERVICES]") && !e.Argument2.Contains("*"))
            {
                m_requestConfigurationError += e.Argument2.Replace("[PHASOR!SERVICES]", "").Replace("\r\n\r\n", "\r\n");
            }
        }

        /// <summary>
        /// Handles ReceivedServiceResponse event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            List<object> attachments = e.Argument.Attachments;

            // Handle any special attachments coming in from service
            if (attachments != null)
            {
                // Return value is always the first attachment
                m_requestConfigurationAttachment = attachments.First();
            }

            if (m_requestConfigurationAttachment is IConfigurationFrame)
                s_responseWaitHandle.Set();
        }

        /// <summary>
        /// Saves concentrating device information in to database.
        /// </summary>
        public void SavePDC()
        {
            Mouse.OverrideCursor = Cursors.Wait;
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
                    device.SkipDisableRealTimeData = SkipDisableRealTimeData;
                    device.ConnectionString = GenerateConnectionString();
                    device.Enabled = true;
                    Device.SaveWithAnalogsDigitals(null, device, false, 0, 0);

                    device = Device.GetDevice(null, "WHERE Acronym = '" + PdcAcronym.ToUpper() + "'");
                    PdcID = device.ID;
                    m_pdcDevice = device;
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

        /// <summary>
        /// Saves configuration information in to database.
        /// </summary>
        public void SaveConfiguration()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);

            try
            {
                int deviceCount = 0;

                foreach (InputWizardDevice inputWizardDevice in ItemsSource)
                {
                    if (inputWizardDevice.Include)
                    {
                        // Included NodeID to find devices
                        Device device = Device.GetDevice(database, "WHERE Acronym = '" + inputWizardDevice.Acronym.ToUpper() + "' and NodeID = '"+ database.CurrentNodeID() + "'");

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

                        device.SkipDisableRealTimeData = SkipDisableRealTimeData;
                        device.Enabled = true;
                        device.Longitude = inputWizardDevice.Longitude;
                        device.Latitude = inputWizardDevice.Latitude;

                        device.IsConcentrator = false;
                        device.LoadOrder = deviceCount;

                        if (!inputWizardDevice.AddAnalogs)
                            inputWizardDevice.AnalogCount = 0;

                        if (!inputWizardDevice.AddDigitals)
                            inputWizardDevice.DigitalCount = 0;

                        if (ConnectToConcentrator && PdcID != null && PdcID > 0)
                        {
                            device.AccessID = inputWizardDevice.AccessID;
                            device.ParentID = PdcID;
                            device.ConnectionString = string.Empty;
                            // IF it is connected to concentrator then do not send initialize command when device is saved.
                            Device.SaveWithAnalogsDigitals(database, device, false, inputWizardDevice.DigitalCount, inputWizardDevice.AnalogCount, inputWizardDevice.DigitalLabels, inputWizardDevice.AnalogLabels);
                        }
                        else
                        {
                            device.AccessID = AccessID;
                            device.ConnectionString = GenerateConnectionString();
                            //If device is direct connected then notify service about it and hence send initialize.
                            Device.SaveWithAnalogsDigitals(database, device, true, inputWizardDevice.DigitalCount, inputWizardDevice.AnalogCount, inputWizardDevice.DigitalLabels, inputWizardDevice.AnalogLabels);
                        }

                        if (device.ID == 0)
                            device.ID = Device.GetDevice(database, "WHERE Acronym = '" + inputWizardDevice.Acronym.ToUpper() + "'").ID;

                        IList<InputWizardDevicePhasor> inputPhasorList = inputWizardDevice.PhasorList;
                        HashSet<InputWizardDevicePhasor> unsavedInputPhasorSet = new HashSet<InputWizardDevicePhasor>(inputWizardDevice.PhasorList);
                        IList<Phasor> oldPhasorList = Phasor.Load(database, Phasor.LoadKeys(database, device.ID));
                        HashSet<Phasor> unsavedOldPhasorSet = new HashSet<Phasor>(oldPhasorList);

                        InputWizardDevicePhasor inputPhasor;
                        Phasor oldPhasor;
                        int index;

                        // Attempt to reorder old phasors based on phasor label
                        for (index = 0; (index < inputWizardDevice.PhasorList.Count) && (unsavedOldPhasorSet.Count > 0); index++)
                        {
                            inputPhasor = inputPhasorList[index];
                            oldPhasor = unsavedOldPhasorSet.FirstOrDefault(p => inputPhasor.Label == p.Label);

                            if ((object)oldPhasor != null)
                            {
                                int oldSourceIndex = oldPhasor.SourceIndex;

                                // Update old phasor and save
                                oldPhasor.DeviceID = device.ID;
                                oldPhasor.SourceIndex = index + 1;
                                oldPhasor.Type = inputPhasor.Type;
                                oldPhasor.Phase = inputPhasor.Phase;
                                Phasor.SaveAndReorder(database, oldPhasor, oldSourceIndex);

                                // Remove phasor from the sets of unsaved phasors
                                unsavedInputPhasorSet.Remove(inputPhasor);
                                unsavedOldPhasorSet.Remove(oldPhasor);
                            }
                        }

                        // Attempt to update old phasors based on phasor index
                        foreach (Phasor unsavedOldPhasor in unsavedOldPhasorSet.ToList())
                        {
                            oldPhasor = unsavedOldPhasor;
                            index = oldPhasor.SourceIndex - 1;

                            if (index < inputPhasorList.Count)
                            {
                                inputPhasor = inputPhasorList[index];

                                if (unsavedInputPhasorSet.Contains(inputPhasor))
                                {
                                    // Update old phasor and save
                                    oldPhasor.DeviceID = device.ID;
                                    oldPhasor.Label = inputPhasor.Label;
                                    oldPhasor.Type = inputPhasor.Type;
                                    oldPhasor.Phase = inputPhasor.Phase;
                                    Phasor.Save(database, oldPhasor);

                                    // Remove phasor from the sets of unsaved phasors
                                    unsavedInputPhasorSet.Remove(inputPhasor);
                                    unsavedOldPhasorSet.Remove(oldPhasor);
                                }
                            }
                        }

                        // Attempt to reorder and update old phasors based on position in phasor lists
                        while (unsavedInputPhasorSet.Count > 0 && unsavedOldPhasorSet.Count > 0)
                        {
                            int oldSourceIndex;

                            inputPhasor = inputPhasorList.First(unsavedInputPhasor => unsavedInputPhasorSet.Contains(unsavedInputPhasor));
                            oldPhasor = oldPhasorList.First(unsavedOldPhasor => unsavedOldPhasorSet.Contains(unsavedOldPhasor));

                            // Update old phasor and save
                            oldSourceIndex = oldPhasor.SourceIndex;
                            oldPhasor.DeviceID = device.ID;
                            oldPhasor.SourceIndex = index + 1;
                            oldPhasor.Label = inputPhasor.Label;
                            oldPhasor.Type = inputPhasor.Type;
                            oldPhasor.Phase = inputPhasor.Phase;
                            Phasor.SaveAndReorder(database, oldPhasor, oldSourceIndex);

                            // Remove phasor from the sets of unsaved phasors
                            unsavedInputPhasorSet.Remove(inputPhasor);
                            unsavedOldPhasorSet.Remove(oldPhasor);
                        }

                        // Remove old phasors that cannot be mapped to input phasors
                        foreach (Phasor unsavedOldPhasor in unsavedOldPhasorSet)
                        {
                            Phasor.Delete(database, unsavedOldPhasor.ID);
                        }

                        // Add new phasor for any input phasors that have yet to be saved
                        for (index = 0; (index < inputPhasorList.Count) && (unsavedInputPhasorSet.Count > 0); index++)
                        {
                            inputPhasor = inputPhasorList[index];

                            if (unsavedInputPhasorSet.Contains(inputPhasor))
                            {
                                oldPhasor = new Phasor();
                                oldPhasor.DeviceID = device.ID;
                                oldPhasor.SourceIndex = index + 1;
                                oldPhasor.Label = inputPhasor.Label;
                                oldPhasor.Type = inputPhasor.Type;
                                oldPhasor.Phase = inputPhasor.Phase;
                                Phasor.Save(database, oldPhasor);
                            }
                        }
                    }

                    deviceCount++;
                }

                Popup("Configuration information saved successfully.", "Input Wizard Configuration", MessageBoxImage.Information);

                // if configuration was set against a PDC then when all devices are added successfully, notify service about it.
                if (ConnectToConcentrator && PdcID != null && PdcID > 0)
                {
                    if (m_pdcDevice == null)
                        m_pdcDevice = Device.GetDevice(database, "WHERE ID = " + PdcID.ToString());

                    if (m_pdcDevice != null)
                        Device.NotifyService(m_pdcDevice);
                }

                DeviceListUserControl deviceListUserControl = new DeviceListUserControl();
                CommonFunctions.LoadUserControl(deviceListUserControl, "Browse Devices");

                m_disconnectedCurrentDevice = false;
                m_currentDeviceRuntimeID = 0;
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Input Wizard Configuration", MessageBoxImage.Error);
                CommonFunctions.LogException(database, "Input wizard save configuration", ex);
            }
            finally
            {
                if (database != null)
                    database.Dispose();

                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Determines whether PDC acronym is required to
        /// save the configuration, and also determines
        /// whether the user has entered it.
        /// </summary>
        /// <param name="errorMessage">Error message to be displayed when validation fails.</param>
        /// <returns>True if the PDC details are valid; false otherwise.</returns>
        public bool ValidatePDCDetails(out string errorMessage)
        {
            if (ItemsSource.Count > 1 && string.IsNullOrEmpty(PdcAcronym))
            {
                errorMessage = "PDC Acronym required before proceeding to the next step.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Generates connection string to save into database by merging connection string and alternate command channel.
        /// </summary>
        /// <returns>string, to store into database.</returns>
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
