//******************************************************************************************************
//  InputWizardDevices.cs - Gbtc
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
//  05/24/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using GSF.Communication;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;
using GSF.PhasorProtocols.BPAPDCstream;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.Modal;
using GSF.PhasorProtocols.UI.UserControls;
using GSF.ServiceProcess;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using GSF.TimeSeries.UI.DataModels;
using GSF.Units.EE;
using PhasorProtocolAdapters;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using Phasor = GSF.PhasorProtocols.UI.DataModels.Phasor;

namespace GSF.PhasorProtocols.UI.ViewModels
{
    /// <summary>
    /// Bindable class to hold collection of <see cref="InputWizardDevice"/>.
    /// </summary>
    internal class InputWizardDevices : PagedViewModelBase<InputWizardDevice, string>, IDataErrorInfo
    {
        #region [ Members ]

        // Fields
        private RelayCommand m_launchWalkthroughCommand;
        private RelayCommand m_browseConnectionFileCommand;
        private RelayCommand m_buildConnectionStringCommand;
        private RelayCommand m_buildAlternateCommandChannelCommand;
        private RelayCommand m_browseIniFileCommand;
        private RelayCommand m_browseConfigurationFileCommand;
        private RelayCommand m_requestConfigurationCommand;
        private RelayCommand m_saveConfigurationFileCommand;
        private RelayCommand m_manualConfigurationCommand;
        private RelayCommand m_cancelConfigurationRequestCommand;
        private bool m_stepsEnabled;
        private bool m_stepOneExpanded;
        private bool m_stepTwoExpanded;
        private bool m_stepThreeExpanded;
        private string m_connectionString;
        private string m_alternateCommandChannel;
        private int m_accessID;
        private int[] m_deviceIDs;
        private string[] m_deviceAcronyms;
        private int m_protocolID;
        private string m_protocolAcronym;
        private bool m_connectToConcentrator;
        private int? m_pdcID;
        private string m_pdcAcronym;
        private string m_pdcName;
        private int m_pdcVendorDeviceID;
        private int m_pdcFrameRate;
        private int m_companyID;
        private int m_historianID;
        private int m_interconnectionID;
        private bool m_skipDisableRealTimeData;
        private bool m_requestConfigurationPopupIsOpen;
        private string m_requestConfigurationPopupText;
        private bool m_requestConfigurationSuccess;
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
        private readonly Dispatcher m_dispatcher;
        private readonly Dictionary<string, string> m_errorMessages;

        #endregion

        #region [ Constructors ]

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
            m_protocolLookupList = Protocol.GetLookupList(null);
            m_vendorDeviceLookupList = VendorDevice.GetLookupList(null, true);
            m_protocolList = Protocol.Load(null);
            PdcFrameRate = 30;
            StepsEnabled = true;
            StepOneExpanded = true;
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

            m_dispatcher = Dispatcher.CurrentDispatcher;
            m_errorMessages = new Dictionary<string, string>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord => CurrentItem.ID == 0; // string.IsNullOrEmpty(CurrentItem.Acronym);

        /// <summary>
        /// Gets or sets flag that determines whether the wizard steps are enabled.
        /// </summary>
        public bool StepsEnabled
        {
            get
            {
                return m_stepsEnabled;
            }
            private set
            {
                m_stepsEnabled = value;
                OnPropertyChanged(nameof(StepsEnabled));
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether step 1 is expanded.
        /// </summary>
        public bool StepOneExpanded
        {
            get
            {
                return m_stepOneExpanded;
            }
            set
            {
                m_stepOneExpanded = value;
                OnPropertyChanged(nameof(StepOneExpanded));
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether step 2 is expanded.
        /// </summary>
        public bool StepTwoExpanded
        {
            get
            {
                return m_stepTwoExpanded;
            }
            set
            {
                m_stepTwoExpanded = value;
                OnPropertyChanged(nameof(StepTwoExpanded));
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether step 3 is expanded.
        /// </summary>
        public bool StepThreeExpanded
        {
            get
            {
                return m_stepThreeExpanded;
            }
            set
            {
                m_stepThreeExpanded = value;
                OnPropertyChanged(nameof(StepThreeExpanded));
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
                OnPropertyChanged(nameof(ConnectionFileName));
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
                OnPropertyChanged(nameof(ConfigurationFileName));
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
                OnPropertyChanged(nameof(IniFileName));
            }
        }

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Company"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> CompanyLookupList => m_companyLookupList;

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Historian"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> HistorianLookupList => m_historianLookupList;

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Interconnection"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> InterconnectionLookupList => m_interconnectionLookupList;

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="Protocol"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> ProtocolLookupList => m_protocolLookupList;

        /// <summary>
        /// Gets <see cref="Dictionary{T1,T2}"/> type collection of <see cref="VendorDevice"/> defined in the database.
        /// </summary>
        public Dictionary<int, string> VendorDeviceLookupList => m_vendorDeviceLookupList;

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
                OnPropertyChanged(nameof(ConnectionString));
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
                OnPropertyChanged(nameof(AlternateCommandChannel));
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
                OnPropertyChanged(nameof(AccessID));
            }
        }

        /// <summary>
        /// Gets or sets device IDs.
        /// </summary>
        public int[] DeviceIDs
        {
            get
            {
                return m_deviceIDs ?? (m_deviceIDs = Array.Empty<int>());
            }
            set
            {
                m_deviceIDs = value;
                OnPropertyChanged(nameof(DeviceIDs));
            }
        }

        /// <summary>
        /// Gets or sets device acronyms.
        /// </summary>
        public string[] DeviceAcronyms
        {
            get
            {
                return m_deviceAcronyms ?? (m_deviceAcronyms = Array.Empty<string>());
            }
            set
            {
                m_deviceAcronyms = value;
                OnPropertyChanged(nameof(DeviceAcronyms));
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
                OnPropertyChanged(nameof(ProtocolID));
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
                OnPropertyChanged(nameof(ProtocolAcronym));
                OnPropertyChanged(nameof(ProtocolIsBpaPdcStream));
            }
        }

        /// <summary>
        /// Gets a flag indicating if selected protocol is BpaPdcStream. 
        /// This is used to hide or display INI file selection on input wizard screen.
        /// </summary>
        public bool ProtocolIsBpaPdcStream => !string.IsNullOrEmpty(m_protocolAcronym) && m_protocolAcronym.ToLower() == "bpapdcstream";

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
                OnPropertyChanged(nameof(ConnectToConcentrator));
                ValidatePdcAcronym();
            }
        }

        /// <summary>
        /// Gets or sets ID of the concentrator device.
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
                OnPropertyChanged(nameof(PdcID));
            }
        }

        /// <summary>
        /// Gets or sets acronym of the concentrator device.
        /// </summary>
        public string PdcAcronym
        {
            get
            {
                return m_pdcAcronym;
            }
            set
            {
                m_pdcAcronym = value.Replace(" ", "_").Replace("'", "").ToUpper();
                ValidatePdcAcronym();
            }
        }

        /// <summary>
        /// Gets or sets message to be displayed on the screen when PDC acronym changes.
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
                OnPropertyChanged(nameof(PdcMessage));
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
                OnPropertyChanged(nameof(PdcName));
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
                OnPropertyChanged(nameof(PdcVendorDeviceID));
            }
        }

        /// <summary>
        /// Gets or sets frame rate for this configuration.
        /// </summary>
        public int PdcFrameRate
        {
            get
            {
                return m_pdcFrameRate;
            }
            set
            {
                m_pdcFrameRate = value;
                OnPropertyChanged(nameof(PdcFrameRate));
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
                OnPropertyChanged(nameof(CompanyID));
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
                OnPropertyChanged(nameof(HistorianID));
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
                OnPropertyChanged(nameof(InterconnectionID));
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
                OnPropertyChanged(nameof(SkipDisableRealTimeData));
            }
        }

        /// <summary>
        /// Gets or set a boolean flag for whether the  configuration frame request popup is open.
        /// </summary>
        public bool RequestConfigurationPopupIsOpen
        {
            get
            {
                return m_requestConfigurationPopupIsOpen;
            }
            set
            {
                m_requestConfigurationPopupIsOpen = value;
                OnPropertyChanged(nameof(RequestConfigurationPopupIsOpen));
            }
        }

        /// <summary>
        /// Gets or sets the text displayed on the configuration frame request popup.
        /// </summary>
        public string RequestConfigurationPopupText
        {
            get
            {
                return m_requestConfigurationPopupText;
            }
            set
            {
                m_requestConfigurationPopupText = value;
                OnPropertyChanged(nameof(RequestConfigurationPopupText));
            }
        }

        /// <summary>
        /// Gets a boolean flag that indicates whether the last configuration request operation was successful.
        /// </summary>
        public bool RequestConfigurationSuccess
        {
            get
            {
                return m_requestConfigurationSuccess;
            }
            set
            {
                m_requestConfigurationSuccess = value;
                OnPropertyChanged(nameof(RequestConfigurationSuccess));
            }
        }

        /// <summary>
        /// Gets <see cref="ICommand"/> to launch the wizard walkthrough.
        /// </summary>
        public ICommand LaunchWalkthroughCommand => m_launchWalkthroughCommand ?? (m_launchWalkthroughCommand = new RelayCommand(LaunchWalkthrough));

        /// <summary>
        /// Gets <see cref="ICommand"/> to browse to connection file.
        /// </summary>
        public ICommand BrowseConnectionFileCommand => m_browseConnectionFileCommand ?? (m_browseConnectionFileCommand = new RelayCommand(BrowseConnectionFile, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> to build connection string.
        /// </summary>
        public ICommand BuildConnectionStringCommand => m_buildConnectionStringCommand ?? (m_buildConnectionStringCommand = new RelayCommand(BuildConnectionString, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> to build alternate command channel.
        /// </summary>
        public ICommand BuildAlternateCommandChannelCommand => m_buildAlternateCommandChannelCommand ?? (m_buildAlternateCommandChannelCommand = new RelayCommand(BuildAlternateCommandChannel, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> to browse to INI file.
        /// </summary>
        public ICommand BrowseIniFileCommand => m_browseIniFileCommand ?? (m_browseIniFileCommand = new RelayCommand(BrowseIniFile, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> to browse to configuration file.
        /// </summary>
        public ICommand BrowseConfigurationFileCommand => m_browseConfigurationFileCommand ?? (m_browseConfigurationFileCommand = new RelayCommand(BrowseConfigurationFile, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> to request configuration frame from backed service.
        /// </summary>
        public ICommand RequestConfigurationCommand => m_requestConfigurationCommand ?? (m_requestConfigurationCommand = new RelayCommand(RequestConfiguration, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> to cancel a configuration frame request.
        /// </summary>
        public ICommand CancelConfigurationRequestCommand => m_cancelConfigurationRequestCommand ?? (m_cancelConfigurationRequestCommand = new RelayCommand(CancelConfigurationRequest));

        /// <summary>
        /// Gets <see cref="ICommand"/> to save current configuration to XML file.
        /// </summary>
        public ICommand SaveConfigurationFileCommand => m_saveConfigurationFileCommand ?? (m_saveConfigurationFileCommand = new RelayCommand(SaveConfigurationFile, () => CanSave));

        /// <summary>
        /// Gets <see cref="ICommand"/> to create or update configuration manually.
        /// </summary>
        public ICommand ManualConfigurationCommand => m_manualConfigurationCommand ?? (m_manualConfigurationCommand = new RelayCommand(ManualConfiguration, () => CanSave));

        /// <summary>
        /// Gets or sets summary message to be displayed on UI after parsing configuration file or frame.
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
                OnPropertyChanged(nameof(ConfigurationSummary));
            }
        }

        /// <summary>
        /// Gets boolean flag indicating if configuration can be saved or not.
        /// </summary>
        public override bool CanSave => CommonFunctions.CurrentPrincipal.IsInRole("Administrator, Editor");

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
        public bool DisconnectedCurrentDevice => m_disconnectedCurrentDevice;

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
                OnPropertyChanged(nameof(NewDeviceConfiguration));
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="propertyName">The name of the property whose error message to get. </param>
        public string this[string propertyName] => m_errorMessages.TryGetValue(propertyName, out string errorMessage) ? errorMessage : string.Empty;

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error
        {
            get
            {
                IEnumerable<string> errorMessages = m_errorMessages .Select(kvp => $"{kvp.Key}: {kvp.Value}");
                return string.Join(Environment.NewLine, errorMessages);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemKey() => CurrentItem.Acronym;

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName() => CurrentItem.Name;

        /// <summary>
        /// Handles BrowseConnectionFileCommand.
        /// </summary>        
        private void BrowseConnectionFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Multiselect = false;
            fileDialog.Filter = "PMU Connection Files (*.PmuConnection)|*.PmuConnection|All Files (*.*)|*.*";

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ConnectionFileName = fileDialog.FileName;

                Stream fileData = fileDialog.OpenFile();
                ConnectionSettings connectionSettings;

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

                    ConnectionString = $"transportprotocol={connectionSettings.TransportProtocol};{connectionStringKeyValues.JoinKeyValuePairs()}";

                    if (connectionSettings.ConnectionParameters != null)
                    {
                        switch (connectionSettings.PhasorProtocol)
                        {
                            case PhasorProtocol.BPAPDCstream:
                                if (connectionSettings.ConnectionParameters is ConnectionParameters bpaParameters)
                                    ConnectionString += $"; iniFileName={bpaParameters.ConfigurationFileName}; refreshConfigFileOnChange={bpaParameters.RefreshConfigurationFileOnChange}; parseWordCountFromByte={bpaParameters.ParseWordCountFromByte}";
                                break;
                            case PhasorProtocol.FNET:
                                if (connectionSettings.ConnectionParameters is FNET.ConnectionParameters fnetParameters)
                                    ConnectionString += $"; timeOffset={fnetParameters.TimeOffset}; stationName={fnetParameters.StationName}; frameRate={fnetParameters.FrameRate}; nominalFrequency={(int)fnetParameters.NominalFrequency}";
                                break;
                            case PhasorProtocol.SelFastMessage:
                                if (connectionSettings.ConnectionParameters is SelFastMessage.ConnectionParameters selParameters)
                                    ConnectionString += $"; messagePeriod={selParameters.MessagePeriod}";
                                AccessID = 0;
                                break;
                            case PhasorProtocol.IEC61850_90_5:
                                if (connectionSettings.ConnectionParameters is IEC61850_90_5.ConnectionParameters iecParameters)
                                    ConnectionString += $"; useETRConfiguration={iecParameters.UseETRConfiguration}; guessConfiguration={iecParameters.GuessConfiguration}; parseRedundantASDUs={iecParameters.ParseRedundantASDUs}; ignoreSignatureValidationFailures={iecParameters.IgnoreSignatureValidationFailures}; ignoreSampleSizeValidationFailures={iecParameters.IgnoreSampleSizeValidationFailures}";
                                break;
                            case PhasorProtocol.Macrodyne:
                                if (connectionSettings.ConnectionParameters is Macrodyne.ConnectionParameters macrodyneParameters)
                                    ConnectionString += $"; protocolVersion={macrodyneParameters.ProtocolVersion}; iniFileName={macrodyneParameters.ConfigurationFileName}; refreshConfigFileOnChange={macrodyneParameters.RefreshConfigurationFileOnChange}; deviceLabel={macrodyneParameters.DeviceLabel}";
                                AccessID = 0;
                                break;
                        }
                    }

                    AccessID = connectionSettings.PmuID;
                    ProtocolAcronym = connectionSettings.PhasorProtocol.ToString();

                    if (m_protocolLookupList.Count > 0)
                    {
                        Protocol protocol = m_protocolList.FirstOrDefault(protocolRecord => string.Equals(protocolRecord.Acronym, connectionSettings.PhasorProtocol.ToString(), StringComparison.OrdinalIgnoreCase));

                        if (protocol != null)
                            ProtocolID = protocol.ID;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayPopup(ex.Message, "Open Connection File", MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Handles BrowseConfigurationFileCommand.
        /// </summary>        
        private void BrowseConfigurationFile()
        {
            Stream fileData;
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

                    fileData = fileDialog.OpenFile();

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
                            string leftPart = configFileDataString.Substring(0, configFileDataString.IndexOf("</configurationFileName>", StringComparison.Ordinal));
                            string rightPart = configFileDataString.Substring(configFileDataString.IndexOf("</configurationFileName>", StringComparison.Ordinal));
                            leftPart = leftPart.Substring(0, leftPart.LastIndexOf(">", StringComparison.Ordinal) + 1);
                            configFileDataString = leftPart + m_iniFileName + rightPart;

                            Byte[] fileBytes = Encoding.UTF8.GetBytes(configFileDataString);

                            using (MemoryStream ms = new MemoryStream(fileBytes))
                            {
                                m_configurationFrame = sf.Deserialize(ms) as IConfigurationFrame;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error occurred in processing INI file. {ex.Message}");
                        }
                    }
                    else
                    {
                        m_configurationFrame = sf.Deserialize(fileData) as IConfigurationFrame;
                    }

                    ParseConfiguration();
                }
                catch (Exception ex)
                {
                    DisplayPopup(ex.Message, "Open Configuration File", MessageBoxImage.Error);
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
            bool isConcentrator = false;

            if (m_configurationFrame != null)
            {
                PdcFrameRate = m_configurationFrame.FrameRate;

                for (int i = 0; i < m_configurationFrame.Cells.Count; i++)
                {
                    IConfigurationCell cell = m_configurationFrame.Cells[i];
                    Device existingDevice = null;
                    string stationAcronym = cell.StationName?.Replace(" ", "_").Replace("'", "").ToUpper() ?? "UNDEFINED";
                    string stationName = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(cell.StationName?.ToLower() ?? stationAcronym);
                    string deviceAcronym = i < DeviceAcronyms.Length ? DeviceAcronyms[i] : stationAcronym;
                    int deviceID = i < DeviceIDs.Length ? DeviceIDs[i] : 0;

                    if (deviceID > 0)
                        existingDevice = Device.GetDevice(null, $"WHERE ID = {deviceID}");

                    if (existingDevice == null)
                        existingDevice = Device.GetDevice(null, $"WHERE Acronym = '{deviceAcronym}'");

                    Dictionary<int, Phasor> existingPhasors = null;

                    if (existingDevice?.ID > 0)
                        existingPhasors = Phasor.Load(null, Phasor.LoadKeys(null, existingDevice.ID)).ToDictionary(phasor => phasor.SourceIndex - 1, phasor => phasor);

                    // Make sure any updates to phase guessing is synchronized with "AddSynchrophasorDevice.cshtml" in the "PhasorWebUI" assembly
                    bool phaseMatchExact(string phaseLabel, string[] phaseMatches) =>
                        phaseMatches.Any(match => phaseLabel.Equals(match, StringComparison.Ordinal));

                    bool phaseEndsWith(string phaseLabel, string[] phaseMatches, bool ignoreCase) => 
                        phaseMatches.Any(match => phaseLabel.EndsWith(match, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

                    bool phaseContains(string phaseLabel, string[] phaseMatches, bool ignoreCase) =>
                        phaseMatches.Any(match => phaseLabel.IndexOf(match, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) > -1);

                    bool phaseMatchHighConfidence(string phaseLabel, string[] containsMatches, string[] endsWithMatches)
                    {
                        if (phaseEndsWith(phaseLabel, containsMatches, true))
                            return true;

                        foreach (string match in containsMatches.Concat(endsWithMatches))
                        {
                            string[] variations = { $" {match}", $"_{match}", $"-{match}", $".{match}" };

                            if (phaseEndsWith(phaseLabel, variations, false))
                                return true;
                        }

                        foreach (string match in containsMatches)
                        {
                            string[] variations = { $" {match} ", $"_{match}_", $"-{match}-", $"-{match}_", $"_{match}-", $".{match}." };

                            if (phaseContains(phaseLabel, variations, false))
                                return true;
                        }

                        return false;
                    }

                    bool phaseMatchLowConfidence(string phaseLabel, string[] phaseMatches)
                    {
                        foreach (string match in phaseMatches)
                        {
                            string[] variations = { $" {match}", $"{match} ", $"_{match}", $"{match}_", $"_{match}_", $"-{match}", $"{match}-", $"-{match}-", $"-{match}_", $"_{match}-", $".{match}", $"{match}.", $".{match}." };

                            if (phaseContains(phaseLabel, variations, true))
                                return true;
                        }

                        return false;
                    }

                    string guessPhase(string phase, string phasorLabel)
                    {
                        if (!string.IsNullOrWhiteSpace(phase) && phase != "+")
                            return phase;

                        // Handle high confidence phase matches when no phase is defined or when phase is "+" - since positive sequence is often default value, it's treated with suspicion
                        if (string.IsNullOrWhiteSpace(phase) || phase == "+")
                        {
                            if (phaseMatchExact(phasorLabel, new[] { "V1PM", "I1PM" }) || phaseMatchHighConfidence(phasorLabel, new[] { "V1", "I1" }, new[] { "POS", "V1PM", "I1PM", "PS", "PSV", "PSI" }) || phaseEndsWith(phasorLabel, new[] { "+SV", "+SI", "+V", "+I" }, true))
                                return "+";

                            if (phaseMatchExact(phasorLabel, new[] { "V0PM", "I0PM", "VZPM", "IZPM" }) || phaseMatchHighConfidence(phasorLabel, new[] { "V0", "I0" }, new[] { "ZERO", "ZPV", "ZPI", "VSPM", "V0PM", "I0PM", "VZPM", "IZPM", "ZS", "ZSV", "ZSI" }) || phaseEndsWith(phasorLabel, new[] { "0SV", "0SI" }, true))
                                return "0";

                            if (phaseMatchExact(phasorLabel, new[] { "VAPM", "IAPM" }) || phaseMatchHighConfidence(phasorLabel, new[] { "VA", "IA" }, new[] { "APV", "API", "VAPM", "IAPM", "AV", "AI" }))
                                return "A";

                            if (phaseMatchExact(phasorLabel, new[] { "VBPM", "IBPM" }) || phaseMatchHighConfidence(phasorLabel, new[] { "VB", "IB" }, new[] { "BPV", "BPI", "VBPM", "IBPM", "BV", "BI" }))
                                return "B";

                            if (phaseMatchExact(phasorLabel, new[] { "VCPM", "ICPM" }) || phaseMatchHighConfidence(phasorLabel, new[] { "VC", "IC" }, new[] { "CPV", "CPI", "VCPM", "ICPM", "CV", "CI" }))
                                return "C";

                            if (phaseMatchExact(phasorLabel, new[] { "VNPM", "INPM" }) || phaseMatchHighConfidence(phasorLabel, new[] { "VN", "IN" }, new[] { "NEUT", "NPV", "NPI", "VNPM", "INPM", "NV", "NI" }))
                                return "N";

                            if (phaseMatchExact(phasorLabel, new[] { "V2PM", "I2PM" }) || phaseMatchHighConfidence(phasorLabel, new[] { "V2", "I2" }, new[] { "NEG", "-SV", "-SI", "V2PM", "I2PM", "NS", "NSV", "NSI" }))
                                return "-";
                        }

                        // Handle lower confidence phase matches only when phase is not defined
                        if (string.IsNullOrWhiteSpace(phase))
                        {
                            // Since positive sequence is the default and always treated with accuracy suspicion, verify it's value first
                            if (phaseMatchLowConfidence(phasorLabel, new[] { "V1", "I1", "POS", "V1PM", "I1PM", "PS", "PSV", "PSI", "+SV", "+SI", "+V", "+I" }))
                                return "+?";

                            if (phaseMatchLowConfidence(phasorLabel, new[] { "V0", "I0", "ZERO", "ZPV", "ZPI", "VSPM", "VZPM", "IZPM", "ZS", "ZSV", "ZSI", "0SV", "0SI" }))
                                return "0?";

                            if (phaseMatchLowConfidence(phasorLabel, new[] { "VA", "IA", "APV", "API", "VAPM", "IAPM", "AV", "AI" }))
                                return "A?";

                            if (phaseMatchLowConfidence(phasorLabel, new[] { "VB", "IB", "BPV", "BPI", "VBPM", "IBPM", "BV", "BI" }))
                                return "B?";

                            if (phaseMatchLowConfidence(phasorLabel, new[] { "VC", "IC", "CPV", "CPI", "VCPM", "ICPM", "CV", "CI" }))
                                return "C?";

                            if (phaseMatchLowConfidence(phasorLabel, new[] { "VN", "IN", "NEUT", "NPV", "NPI", "VNPM", "INPM", "NV", "NI" }))
                                return "N?";

                            if (phaseMatchLowConfidence(phasorLabel, new[] { "V2", "I2", "NEG", "-SV", "-SI", "V2PM", "I2PM", "NS", "NSV", "NSI" }))
                                return "-?";

                            // Test for contains after checks with separators
                            if (phaseContains(phasorLabel, new[] { "V1", "I1", "POS", "V1PM", "I1PM", "PS", "PSV", "PSI", "+SV", "+SI", "+V", "+I" }, true))
                                return "+?";

                            if (phaseContains(phasorLabel, new[] { "V0", "I0", "ZERO", "ZPV", "ZPI", "VSPM", "VZPM", "IZPM", "ZS", "ZSV", "ZSI", "0SV", "0SI" }, true))
                                return "0?";

                            if (phaseContains(phasorLabel, new[] { "VA", "IA", "APV", "API", "VAPM", "IAPM", "AV", "AI" }, true))
                                return "A?";

                            if (phaseContains(phasorLabel, new[] { "VB", "IB", "BPV", "BPI", "VBPM", "IBPM", "BV", "BI" }, true))
                                return "B?";

                            if (phaseContains(phasorLabel, new[] { "VC", "IC", "CPV", "CPI", "VCPM", "ICPM", "CV", "CI" }, true))
                                return "C?";

                            if (phaseContains(phasorLabel, new[] { "VN", "IN", "NEUT", "NPV", "NPI", "VNPM", "INPM", "NV", "NI" }, true))
                                return "N?";

                            if (phaseContains(phasorLabel, new[] { "V2", "I2", "NEG", "-SV", "-SI", "V2PM", "I2PM", "NS", "NSV", "NSI" }, true))
                                return "-?";

                            // -V and -I may match too often, so check these last
                            if (phaseMatchLowConfidence(phasorLabel, new[] { "-V", "-I" }) || phaseContains(phasorLabel, new[] { "-V", "-I" }, true))
                                return "-?";

                            return "?";
                        }

                        return phase;
                    }

                    string guessBaseKV(string baseKV, string phasorLabel, string deviceLabel)
                    {
                        if (!string.IsNullOrWhiteSpace(baseKV) && int.TryParse(baseKV, out int value) && value > 0)
                            return baseKV;

                        // Check phasor label before device
                        foreach (string voltageLevel in s_commonVoltageLevels)
                        {
                            if (phasorLabel.IndexOf(voltageLevel, StringComparison.Ordinal) > -1)
                                return voltageLevel;
                        }

                        foreach (string voltageLevel in s_commonVoltageLevels)
                        {
                            if (deviceLabel.IndexOf(voltageLevel, StringComparison.Ordinal) > -1)
                                return voltageLevel;
                        }

                        return "0";
                    }

                    bool phasorExists(IPhasorDefinition phasor) => phasor != null && (existingPhasors?.ContainsKey(phasor.Index) ?? false);

                    string getPhasorLabel(IPhasorDefinition phasor) => phasorExists(phasor) ? existingPhasors?[phasor.Index].Label : phasor.Label;

                    string getPhasorType(IPhasorDefinition phasor) => phasorExists(phasor) ? existingPhasors?[phasor.Index].Type : phasor.PhasorType == PhasorType.Current ? "I" : "V";

                    string getPhasorPhase(IPhasorDefinition phasor) => guessPhase(phasorExists(phasor) ? existingPhasors?[phasor.Index].Phase : "", phasor.Label);

                    string getPhasorBaseKV(IPhasorDefinition phasor) => guessBaseKV(phasorExists(phasor) ? existingPhasors?[phasor.Index].BaseKV.ToString() : "0", phasor.Label, string.IsNullOrWhiteSpace(existingDevice?.Name) ? existingDevice?.Acronym ?? "" : existingDevice?.Name);

                    string deviceIndex = m_configurationFrame.Cells.Count > 1 ? $" {i + 1:N0}" : "";

                    wizardDeviceList.Add(new InputWizardDevice()
                    {
                        ID = deviceID,
                        Acronym = string.IsNullOrWhiteSpace(existingDevice?.Acronym) ? stationAcronym : existingDevice.Acronym,
                        Name = string.IsNullOrWhiteSpace(existingDevice?.Name) ? stationName : existingDevice.Name,
                        ConfigAcronym = $"Device{deviceIndex} label from config: {stationAcronym}{(string.IsNullOrWhiteSpace(cell.IDLabel) ? "" : $" ({cell.IDLabel})")}",
                        ConfigName = $"Device{deviceIndex} name derived from config: {stationName}",
                        Longitude = existingDevice?.Longitude ?? -98.6m,
                        Latitude = existingDevice?.Latitude ?? 37.5m,
                        VendorDeviceID = existingDevice?.VendorDeviceID,
                        AccessID = cell.IDCode,
                        ParentAccessID = m_configurationFrame.IDCode,
                        Include = true,
                        DigitalCount = cell.DigitalDefinitions.Count,
                        AnalogCount = cell.AnalogDefinitions.Count,
                        AddDigitals = cell.DigitalDefinitions.Count > 0,
                        AddAnalogs = cell.AnalogDefinitions.Count > 0,
                        Existing = existingDevice != null,
                        DigitalLabels = GetAnalogOrDigitalLables(cell.DigitalDefinitions),
                        AnalogLabels = GetAnalogOrDigitalLables(cell.AnalogDefinitions),
                        PhasorList = new ObservableCollection<InputWizardDevicePhasor>((from phasor in cell.PhasorDefinitions
                                                                                        select new InputWizardDevicePhasor()
                                                                                        {
                                                                                            Label = getPhasorLabel(phasor),
                                                                                            Type = getPhasorType(phasor),
                                                                                            ConfigLabel = $"Phasor {phasor.Index + 1:N0} label from config: {phasor.Label}",
                                                                                            ConfigType = $"Phasor {phasor.Index + 1:N0} type from config: {phasor.PhasorType}",
                                                                                            Phase = getPhasorPhase(phasor),
                                                                                            BaseKVInput = getPhasorBaseKV(phasor),
                                                                                            Include = true
                                                                                        }).ToList())
                    });
                }

                isConcentrator = wizardDeviceList.Count > 1 || (wizardDeviceList.Count == 1 && m_configurationFrame.IDCode != m_configurationFrame.Cells.First().IDCode);
            }

            m_dispatcher.BeginInvoke(new Action(() =>
            {
                ItemsSource = wizardDeviceList;

                ConfigurationSummary = $"Current Configuration Summary: {wizardDeviceList.Count}";

                if (isConcentrator)
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
                    DisplayPopup(ConfigurationSummary, "Parsed Configuration Successfully.", MessageBoxImage.Information);
            }));
        }

        private List<string> GetAnalogOrDigitalLables(object analogOrDigitalCollection)
        {
            List<string> returnCollection = new List<string>();

            if (analogOrDigitalCollection is DigitalDefinitionCollection digitalCollection)
            {
                foreach (IDigitalDefinition digital in digitalCollection)
                {
                    returnCollection.Add(digital.Label.TruncateRight(256));
                }
            }
            else if (analogOrDigitalCollection is AnalogDefinitionCollection analogCollection)
            {
                foreach (IAnalogDefinition analog in analogCollection)
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
                            DisplayPopup("Configuration file saved successfully.", "Save Current Configuration", MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    DisplayPopup("Current configuration is undefined and cannot be saved.", "Save Current Configuration", MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                DisplayPopup($"ERROR: {ex.Message}", "Save Current Configuration", MessageBoxImage.Error);
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
                    DisplayPopup(ex.Message, "Browse to INI File", MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Pops up another window to assist in filling out the fields on the input wizard.
        /// </summary>
        private void LaunchWalkthrough()
        {
            InputWizardWalkthrough walkthrough = new InputWizardWalkthrough();

            walkthrough.Owner = Application.Current.MainWindow;
            walkthrough.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            walkthrough.DataContext = this;

            walkthrough.ShowDialog();
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
                if (csb.DialogResult != null && csb.DialogResult.GetValueOrDefault())
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
                if (csb.DialogResult != null && csb.DialogResult.GetValueOrDefault())
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
            DispatcherTimer timer = new DispatcherTimer();

            StepsEnabled = false;
            RequestConfigurationPopupIsOpen = true;
            RequestConfigurationPopupText = $"Requesting configuration{Environment.NewLine}.";

            timer.Tick += (sender, args) => RequestConfigurationPopupText += ".";
            timer.Interval = TimeSpan.FromSeconds(1.0D);
            timer.Start();

            Thread requestConfigurationThread = new Thread(() =>
            {
                AdoDataConnection database = null;
                WindowsServiceClient windowsServiceClient = null;

                try
                {
                    m_requestConfigurationError = string.Empty;
                    m_dispatcher.BeginInvoke(new Action(() => RequestConfigurationSuccess = false));

                    if (m_currentDeviceRuntimeID > 0)
                    {
                        CommonFunctions.SendCommandToService($"Disconnect {m_currentDeviceRuntimeID}");
                        m_disconnectedCurrentDevice = true;
                    }

                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    m_dispatcher.BeginInvoke(new Action(() => Mouse.OverrideCursor = Cursors.Wait));
                    s_responseWaitHandle = new ManualResetEvent(false);

                    windowsServiceClient = CommonFunctions.GetWindowsServiceClient();

                    if (windowsServiceClient?.Helper?.RemotingClient != null && windowsServiceClient.Helper.RemotingClient.CurrentState == ClientState.Connected)
                    {
                        windowsServiceClient.Helper.RemotingClient.ConnectionTerminated += RemotingClient_ConnectionTerminated;
                        windowsServiceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                        windowsServiceClient.Helper.ReceivedServiceUpdate += Helper_ReceivedServiceUpdate;

                        string connectionString = GenerateConnectionString();

                        if (!connectionString.EndsWith(";"))
                            connectionString += ";";

                        connectionString += $"accessid={AccessID};";

                        if (!connectionString.ToLower().Contains("phasorprotocol"))
                        {
                            Protocol protocol = m_protocolList.FirstOrDefault(p => p.ID == ProtocolID);

                            if (protocol != null)
                                connectionString += $"phasorprotocol={protocol.Acronym};";
                        }

                        s_responseWaitHandle.Reset();
                        m_requestConfigurationAttachment = null;
                        CommonFunctions.SendCommandToService($"invoke 0 requestdeviceconfiguration \"{connectionString}\"");

                        if (s_responseWaitHandle.WaitOne(65000))
                        {
                            if (m_requestConfigurationAttachment is ConfigurationErrorFrame)
                            {
                                m_dispatcher.BeginInvoke(new Action(() => RequestConfigurationPopupText = $"Accumulating error messages{Environment.NewLine}."));

                                Thread.Sleep(3000);

                                m_requestConfigurationError = $"Received configuration error frame.{Environment.NewLine}{m_requestConfigurationError}";

                                throw new ApplicationException(m_requestConfigurationError);
                            }

                            if (m_requestConfigurationAttachment is IConfigurationFrame frame)
                            {
                                m_configurationFrame = frame;
                                ParseConfiguration();
                                m_dispatcher.BeginInvoke(new Action(() => RequestConfigurationSuccess = true));
                            }
                            else
                            {
                                throw new ApplicationException("Invalid frame received, invocation for device configuration has failed.");
                            }
                        }
                        else
                        {
                            throw new ApplicationException("Response timeout occurred. Waited 60 seconds for Configuration Frame to arrive.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Connection timeout occurred. Tried 10 times to connect to windows service.");
                    }

                    m_dispatcher.BeginInvoke(new Action(() =>
                    {
                        Mouse.OverrideCursor = null;
                        StepsEnabled = true;
                        RequestConfigurationPopupIsOpen = false;
                        timer.Stop();
                    }));
                }
                catch (Exception ex)
                {
                    m_dispatcher.BeginInvoke(new Action(() =>
                    {
                        Mouse.OverrideCursor = null;
                        StepsEnabled = true;
                        RequestConfigurationPopupIsOpen = false;
                        timer.Stop();

                        DisplayPopup($"ERROR: {ex.Message}", "Request Configuration", MessageBoxImage.Error);
                    }));
                }
                finally
                {
                    database?.Dispose();

                    if (windowsServiceClient?.Helper?.RemotingClient != null)
                    {
                        windowsServiceClient.Helper.RemotingClient.ConnectionTerminated -= RemotingClient_ConnectionTerminated;
                        windowsServiceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
                        windowsServiceClient.Helper.ReceivedServiceUpdate -= Helper_ReceivedServiceUpdate;
                    }
                }
            });

            requestConfigurationThread.IsBackground = true;
            requestConfigurationThread.Start();
        }

        private void CancelConfigurationRequest()
        {
            try
            {
                CommonFunctions.SendCommandToService("Invoke 0 CancelConfigurationFrameRequest");
            }
            catch (Exception ex)
            {
                DisplayPopup($"ERROR: {ex.Message}", "Cancel Configuration Request", MessageBoxImage.Error);
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
                if (cc.DialogResult != null && cc.DialogResult.GetValueOrDefault())
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
            foreach (string message in e.Argument2.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (message.StartsWith("[PHASOR!SERVICES]") && !message.Contains("*"))
                    m_requestConfigurationError += message.Replace("[PHASOR!SERVICES]", "") + Environment.NewLine;
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

        private void RemotingClient_ConnectionTerminated(object sender, EventArgs eventArgs)
        {
            m_configurationFrame = new ConfigurationErrorFrame();
            m_requestConfigurationError = "Connection to service was interrupted.";
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
                    Device device = new Device
                    {
                        IsConcentrator = true,
                        Acronym = PdcAcronym.ToUpper(),
                        Name = PdcName,
                        ParentID = null,
                        AccessID = AccessID,
                        CompanyID = CompanyID == 0 ? (int?)null : CompanyID,
                        HistorianID = HistorianID == 0 ? (int?)null : HistorianID,
                        ProtocolID = ProtocolID == 0 ? (int?)null : ProtocolID,
                        InterconnectionID = InterconnectionID == 0 ? (int?)null : InterconnectionID,
                        VendorDeviceID = PdcVendorDeviceID == 0 ? (int?)null : PdcVendorDeviceID,
                        FramesPerSecond = PdcFrameRate,
                        SkipDisableRealTimeData = SkipDisableRealTimeData,
                        ConnectionString = GenerateConnectionString(),
                        Enabled = true
                    };

                    Device.SaveWithAnalogsDigitals(null, device, false, 0, 0);

                    device = Device.GetDevice(null, $"WHERE Acronym = '{PdcAcronym.ToUpper()}'");
                    PdcID = device.ID;
                    m_pdcDevice = device;
                }
            }
            catch (Exception ex)
            {
                DisplayPopup($"ERROR: {ex.Message}", "Save PDC information", MessageBoxImage.Error);
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
                        Device device = null;

                        if (inputWizardDevice.ID > 0)
                            device = Device.GetDevice(database, $"WHERE ID = {inputWizardDevice.ID}");

                        if (device == null)
                            device = Device.GetDevice(database, $"WHERE Acronym = '{inputWizardDevice.Acronym.ToUpper()}' AND NodeID = '{database.CurrentNodeID()}'");
                        else
                            device.Acronym = inputWizardDevice.Acronym.ToUpper();

                        if (device == null)
                            device = new Device { Acronym = inputWizardDevice.Acronym.ToUpper() };

                        device.Name = inputWizardDevice.Name;
                        device.CompanyID = CompanyID == 0 ? (int?)null : CompanyID;
                        device.HistorianID = HistorianID == 0 ? (int?)null : HistorianID;
                        device.ProtocolID = ProtocolID == 0 ? (int?)null : ProtocolID;
                        device.InterconnectionID = InterconnectionID == 0 ? (int?)null : InterconnectionID;
                        device.FramesPerSecond = PdcFrameRate;
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
                            // If device is connected to concentrator then do not send initialize command when device is saved.
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
                        {
                            device.ID = Device.GetDevice(database, $"WHERE Acronym = '{inputWizardDevice.Acronym.ToUpper()}'").ID;
                            inputWizardDevice.ID = device.ID;
                        }

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

                            if (oldPhasor != null)
                            {
                                int oldSourceIndex = oldPhasor.SourceIndex;

                                // Update old phasor and save
                                oldPhasor.DeviceID = device.ID;
                                oldPhasor.SourceIndex = index + 1;
                                oldPhasor.Type = inputPhasor.Type;
                                oldPhasor.Phase = inputPhasor.Phase;
                                oldPhasor.BaseKV = inputPhasor.BaseKV;
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
                                    oldPhasor.BaseKV = inputPhasor.BaseKV;
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

                            inputPhasor = inputPhasorList.First(unsavedInputPhasorSet.Contains);
                            oldPhasor = oldPhasorList.First(unsavedOldPhasorSet.Contains);

                            // Update old phasor and save
                            oldSourceIndex = oldPhasor.SourceIndex;
                            oldPhasor.DeviceID = device.ID;
                            oldPhasor.SourceIndex = index + 1;
                            oldPhasor.Label = inputPhasor.Label;
                            oldPhasor.Type = inputPhasor.Type;
                            oldPhasor.Phase = inputPhasor.Phase;
                            oldPhasor.BaseKV = inputPhasor.BaseKV;
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
                                oldPhasor.BaseKV = inputPhasor.BaseKV;
                                Phasor.Save(database, oldPhasor);
                            }
                        }
                    }

                    deviceCount++;
                }

                // Find and remove child devices which are not included in this configuration update
                foreach (Device device in Device.GetDevices(database, $"WHERE ParentID = {PdcID}") ?? Enumerable.Empty<Device>())
                {
                    if (!ItemsSource.Any(child => child.Include && device.Acronym == child.Acronym))
                        Device.Delete(database, device);
                }

                DisplayPopup("Configuration information saved successfully.", "Input Wizard Configuration", MessageBoxImage.Information);

                // if configuration was set against a PDC then when all devices are added successfully, notify service about it.
                if (ConnectToConcentrator && PdcID != null && PdcID > 0)
                {
                    if (m_pdcDevice == null)
                        m_pdcDevice = Device.GetDevice(database, $"WHERE ID = {PdcID}");

                    if (m_pdcDevice != null)
                        Device.NotifyService(m_pdcDevice);
                }

                CommonFunctions.LoadUserControl("Browse Devices", typeof(DeviceListUserControl));

                m_disconnectedCurrentDevice = false;
                m_currentDeviceRuntimeID = 0;
            }
            catch (Exception ex)
            {
                DisplayPopup($"ERROR: {ex.Message}", "Input Wizard Configuration", MessageBoxImage.Error);
                CommonFunctions.LogException(database, "Input wizard save configuration", ex);
            }
            finally
            {
                database?.Dispose();

                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Determines whether the PDC acronym is valid.
        /// </summary>
        private void ValidatePdcAcronym()
        {
            Device device;
            string errorMessage;

            PdcID = 0;
            PdcMessage = "";
            errorMessage = "";

            // If the connection is not to a concentrator,
            // there is no need to validate the PDC acronym
            if (m_connectToConcentrator)
            {
                if (string.IsNullOrEmpty(m_pdcAcronym))
                {
                    // If the connection is to a concentrator,
                    // the PDC acronym must be specified
                    errorMessage = "PDC acronym must not be empty.";
                }
                else if (!Regex.IsMatch(m_pdcAcronym, AcronymValidationAttribute.ValidationPattern))
                {
                    // Check if the acronym is formatted properly
                    errorMessage = AcronymValidationAttribute.DefaultErrorMessage;
                }
                else
                {
                    // If the acronym is formatted properly, check in the database to see if it already exists
                    device = Device.GetDevice(null, $" WHERE Acronym = '{m_pdcAcronym.ToUpper()}'");

                    if (device != null)
                    {
                        if (device.IsConcentrator)
                        {
                            PdcID = device.ID;
                            PdcMessage = "PDC already exists in the database. All devices will be assigned to this PDC.";
                        }
                        else
                        {
                            errorMessage = "A non-PDC device with the same acronym exists in the database. Please change acronym.";
                        }
                    }
                }

                // If an error occurred during validation,
                // set the PdcMessage to that error message as well
                if (!string.IsNullOrEmpty(errorMessage))
                    PdcMessage = errorMessage;
            }

            // Update the error message
            m_errorMessages["PdcAcronym"] = errorMessage;

            // Notify of changes in validation state
            OnPropertyChanged("PdcAcronym");
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

                connectionString += $"commandchannel={{{AlternateCommandChannel}}}";
            }

            return connectionString;
        }

        private void DisplayPopup(string message, string source, MessageBoxImage image)
        {
            if (m_dispatcher.Thread != Thread.CurrentThread)
                m_dispatcher.BeginInvoke((Action<string, string, MessageBoxImage>)DisplayPopup, message, source, image);
            else
                Popup(message, source, image);
        }

        #endregion

        #region [ Static ]

        // Fields
        private static ManualResetEvent s_responseWaitHandle;
        private static readonly string[] s_commonVoltageLevels = { "69", "115", "138", "161", "230", "345", "500", "765" };

        #endregion
    }
}
