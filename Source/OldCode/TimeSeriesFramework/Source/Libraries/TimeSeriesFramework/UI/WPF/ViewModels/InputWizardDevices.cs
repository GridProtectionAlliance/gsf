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
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using TimeSeriesFramework.UI.Commands;
using TimeSeriesFramework.UI.DataModels;

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
        private string m_connectionString;
        private string m_alternateCommandChannel;
        private int m_deviceIdCode;
        private int m_protocolId;
        private bool m_connectToConcentrator;
        private string m_pdcAcronym;
        private string m_pdcName;
        private int m_pdcVendorDeviceId;
        private int m_companyId;
        private int m_historianId;
        private int m_interconnectionId;

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

        public int DeviceIdCode
        {
            get
            {
                return m_deviceIdCode;
            }
            set
            {
                m_deviceIdCode = value;
                OnPropertyChanged("DeviceIdCode");
            }
        }

        public int ProtocolId
        {
            get
            {
                return m_protocolId;
            }
            set
            {
                m_protocolId = value;
                OnPropertyChanged("ProtocolId");
            }
        }

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
            }
        }

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

        public int PdcVendorDeviceId
        {
            get
            {
                return m_pdcVendorDeviceId;
            }
            set
            {
                m_pdcVendorDeviceId = value;
                OnPropertyChanged("PdcVendorDeviceId");
            }
        }

        public int CompanyId
        {
            get
            {
                return m_companyId;
            }
            set
            {
                m_companyId = value;
                OnPropertyChanged("CompanyId");
            }
        }

        public int HistorianId
        {
            get
            {
                return m_historianId;
            }
            set
            {
                m_historianId = value;
                OnPropertyChanged("HistorianId");
            }
        }

        public int InterconnectionId
        {
            get
            {
                return m_interconnectionId;
            }
            set
            {
                m_interconnectionId = value;
                OnPropertyChanged("InterconnectionId");
            }
        }

        public ICommand BrowseConnectionFileCommand
        {
            get
            {
                if (m_browseConnectionFileCommand == null)
                    m_browseConnectionFileCommand = new RelayCommand(BrowseConnectionFile, (param) => CanSave);

                return m_browseConnectionFileCommand;
            }
        }

        public ICommand BuildConnectionStringCommand
        {
            get
            {
                if (m_buildConnectionStringCommand == null)
                    m_buildConnectionStringCommand = new RelayCommand(BuildConnectionString, (param) => CanSave);

                return m_buildConnectionStringCommand;
            }
        }

        public ICommand BuildAlternateCommandChannelCommand
        {
            get
            {
                if (m_buildAlternateCommandChannelCommand == null)
                    m_buildAlternateCommandChannelCommand = new RelayCommand(BuildAlternateCommandChannel, (param) => CanSave);

                return m_buildAlternateCommandChannelCommand;
            }
        }

        public ICommand BrowseIniFileCommand
        {
            get
            {
                if (m_browseIniFileCommand == null)
                    m_browseIniFileCommand = new RelayCommand(BrowseIniFile, (param) => CanSave);

                return m_browseIniFileCommand;
            }
        }

        public ICommand BrowseConfigurationFileCommand
        {
            get
            {
                if (m_browseConfigurationFileCommand == null)
                    m_browseConfigurationFileCommand = new RelayCommand(BrowseConfigurationFile, (param) => CanSave);

                return m_browseConfigurationFileCommand;
            }
        }

        public ICommand RequestConfigurationCommand
        {
            get
            {
                if (m_requestConfigurationCommand == null)
                    m_requestConfigurationCommand = new RelayCommand(RequestConfiguration, (param) => CanSave);

                return m_requestConfigurationCommand;
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
        /// Saves CurrentItem information into database.
        /// </summary>
        public override void Save()
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new instance of <see cref="InputWizardDevice"/> and assigns it to CurrentItem.
        /// </summary>
        public override void Clear()
        {
            // Do nothing.
        }

        private void BrowseConnectionFile(object parameter)
        {

        }

        private void BuildConnectionString(object parameter)
        {
            Stream fileData = null;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "PMU Connection Files (*.PmuConnection)|*.PmuConnection|All Files (*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((fileData = fileDialog.OpenFile()) != null)
                    {
                        using (fileData)
                        {
                            ConnectionSettings connectionSettings = new ConnectionSettings();

                            //SoapFormatter sf = new SoapFormatter();
                            //sf.AssemblyFormat = FormatterAssemblyStyle.Simple;
                            //sf.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
                            //sf.Binder = new VersionConfigToNamespaceAssemblyObjectBinder();
                            //connectionSettings = sf.Deserialize(inputStream) as ConnectionSettings;

                            //if (connectionSettings.ConnectionParameters != null)
                            //{
                            //    ConnectionSettings cs = new ConnectionSettings();
                            //    cs = (ConnectionSettings)connectionSettings.ConnectionParameters;
                            //    connectionSettings.configurationFileName = cs.configurationFileName;
                            //    connectionSettings.refreshConfigurationFileOnChange = cs.refreshConfigurationFileOnChange;
                            //    connectionSettings.parseWordCountFromByte = cs.parseWordCountFromByte;
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    Popup(ex.Message, "Open PMU Connection File", MessageBoxImage.Error);
                }
            }
        }

        private void BuildAlternateCommandChannel(object parameter)
        {

        }

        private void BrowseIniFile(object parameter)
        {

        }

        private void BrowseConfigurationFile(object parameter)
        {

        }

        private void RequestConfiguration(object parameter)
        {

        }

        #endregion
    }
}
