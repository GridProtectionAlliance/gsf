//******************************************************************************************************
//  SubscriberUserControl.xaml.cs - Gbtc
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
//  05/19/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  05/19/2011 - Mehulbhai P Thakkar
//       Added code regions, comments etc.
//  05/25/2011 - J. Ritchie Carroll
//       Attached to before save event so class could import new keyIV's into local crypto cache.
//  07/16/2012 - Alex Foglia
//       Modified some controls' properties upon user's role so he/she would or wouldn't be able to
//       modify their contents.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using GSF.Data;
using GSF.IO;
using GSF.Security.Cryptography;
using GSF.TimeSeries.Transport.UI.DataModels;
using GSF.TimeSeries.Transport.UI.ViewModels;
using GSF.TimeSeries.UI;
using Application = System.Windows.Application;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using RadioButton = System.Windows.Controls.RadioButton;
using UserControl = System.Windows.Controls.UserControl;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SubscriberUserControl.xaml
    /// </summary>
    public partial class SubscriberUserControl : UserControl
    {
        #region [ Members ]

        private readonly Subscribers m_dataContext;
        private string m_key;
        private string m_iv;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="SubscriberUserControl"/>.
        /// </summary>
        public SubscriberUserControl()
        {
            InitializeComponent();
            this.Loaded += SubscriberUserControl_Loaded;
            this.Unloaded += SubscriberUserControl_Unloaded;
            m_dataContext = new Subscribers(10);
            this.DataContext = m_dataContext;
            m_dataContext.PropertyChanged += DataContext_PropertyChanged;
            m_dataContext.SecurityMode = !string.IsNullOrEmpty(m_dataContext.CurrentItem.RemoteCertificateFile) ? SecurityMode.TLS : SecurityMode.Gateway;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles loaded event for <see cref="SubscriberUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void SubscriberUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach to load/before save event so class can load/add crypto keys from/to local keyIV cache
            m_dataContext.PropertyChanged += SubscriberUserControl_PropertyChanged;
            m_dataContext.BeforeSave += SubscriberUserControl_BeforeSave;
            LoadCurrentKeyIV();

            try
            {
                using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                {
                    Dictionary<string, string> settings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    settings = database.ServiceConnectionString().ParseKeyValuePairs();
                    IPAddress[] hostIPs = null;
                    if (settings.ContainsKey("server"))
                        hostIPs = Dns.GetHostAddresses(settings["server"].Split(':')[0]);

                    IEnumerable<IPAddress> localIPs = Dns.GetHostAddresses("localhost").Concat(Dns.GetHostAddresses(Dns.GetHostName()));

                    // Check to see if entered host name corresponds to a local IP address
                    if (hostIPs == null)
                        MessageBox.Show("Failed to find service host address. Secure key exchange may not succeed." + Environment.NewLine + "Please make sure to run manager application with administrative privileges on the server where service is hosted.", "Authorize Subcriber", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else if (!hostIPs.Any(ip => localIPs.Contains(ip)))
                        MessageBox.Show("Secure key exchange may not succeed." + Environment.NewLine + "Please make sure to run manager application with administrative privileges on the server where service is hosted.", "Authorize Subscriber", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // If the user is not an Administrator, then the following properties for these controls are readonly and not enable
                bool isAdmin = CommonFunctions.CurrentPrincipal.IsInRole("Administrator") ? false : true;

                m_acronymField.IsReadOnly = isAdmin;
                m_nameField.IsReadOnly = isAdmin;
                m_sharedSecretField.IsReadOnly = isAdmin;
                m_authenticationID.IsReadOnly = isAdmin;
                m_validIpAddressesField.IsReadOnly = isAdmin;
                m_enablePGConnection.IsEnabled = !isAdmin;
                m_buttonClick.IsEnabled = !isAdmin;
                m_footerControl.IsEnabled = !isAdmin;
            }
            catch
            {
                MessageBox.Show("Please make sure to run manager application with administrative privileges on the server where service is hosted.", "Authorize Subscriber", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Handles unloaded event for <see cref="SubscriberUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void SubscriberUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            m_dataContext.ProcessPropertyChange();
            m_dataContext.BeforeSave -= SubscriberUserControl_BeforeSave;
            m_dataContext.PropertyChanged -= SubscriberUserControl_PropertyChanged;
        }

        /// <summary>
        /// Handles property changed event for <see cref="SubscriberUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void SubscriberUserControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update information when current item changes...
            if (string.Compare(e.PropertyName, "CurrentItem", true) == 0)
                LoadCurrentKeyIV();
        }

        /// <summary>
        /// Handles before save event for <see cref="SubscriberUserControl"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Cancel event arguments.</param>
        private void SubscriberUserControl_BeforeSave(object sender, CancelEventArgs e)
        {
            try
            {
                if (m_dataContext.SecurityMode == SecurityMode.Gateway)
                {
                    if (string.IsNullOrWhiteSpace(m_sharedSecretField.Text) || string.IsNullOrWhiteSpace(m_key) || string.IsNullOrWhiteSpace(m_iv))
                    {
                        MessageBox.Show("Failed to import key and initialization vectors for associated shared secret - these fields cannot be blank.", "Crypto Key Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                    }
                    else
                    {
                        // Import key and initialization vector for subscriber into common crypto cache
                        if (ImportCipherKey(m_sharedSecretField.Text.Trim(), 256, m_key.Trim() + "|" + m_iv.Trim()))
                        {
                            ReloadServiceCryptoCache();
                            Cipher.ReloadCache();
                        }
                        else
                        {
                            MessageBox.Show("Failed to import key and initialization vectors for associated shared secret.", "Crypto Key Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to import key and initialization vectors for associated shared secret due to exception: " + ex.Message, "Crypto Key Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }

        // Imports the given cipher key to the common key cache.
        private bool ImportCipherKey(string password, int keySize, string keyIVText)
        {
            ProcessStartInfo configCrypterInfo = new ProcessStartInfo();
            Process configCrypter;

            configCrypterInfo.FileName = FilePath.GetAbsolutePath("ConfigCrypter.exe");
            configCrypterInfo.Arguments = string.Format("-password {0} -keySize {1} -keyIVText {2}", password, keySize, keyIVText);
            configCrypterInfo.CreateNoWindow = true;

            configCrypter = Process.Start(configCrypterInfo);
            configCrypter.WaitForExit();

            return configCrypter.ExitCode == 0;
        }

        // Send service command to reload crypto cache.
        private void ReloadServiceCryptoCache()
        {
            try
            {
                CommonFunctions.SendCommandToService("ReloadCryptoCache");
            }
            catch (Exception ex)
            {
                string message = "Unable to notify service about updated crypto cache:" + Environment.NewLine;

                if (ex.InnerException != null)
                {
                    message += ex.Message + Environment.NewLine;
                    message += "Inner Exception: " + ex.InnerException.Message;
                    Popup(message, "Subscription Request Exception:", MessageBoxImage.Information);
                    CommonFunctions.LogException(null, "Subscription Request", ex.InnerException);
                }
                else
                {
                    message += ex.Message;
                    Popup(message, "Subscription Request Exception:", MessageBoxImage.Information);
                    CommonFunctions.LogException(null, "Subscription Request", ex);
                }
            }
        }

        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (m_dataContext.SecurityMode == SecurityMode.TLS)
                TlsRadioButton.IsChecked = true;
            else
                GatewayRadioButton.IsChecked = true;
        }

        private void SecurityModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton tlsRadioButton = sender as RadioButton;
            SecurityMode securityMode;

            if ((object)tlsRadioButton != null && (object)m_dataContext != null)
            {
                if (Enum.TryParse(tlsRadioButton.Content.ToString(), out securityMode))
                    m_dataContext.SecurityMode = securityMode;
            }

        }

        // Display popup message for the user
        private void Popup(string message, string caption, MessageBoxImage image)
        {
            MessageBox.Show(Application.Current.MainWindow, message, caption, MessageBoxButton.OK, image);
        }

        /// <summary>
        /// Handles preview key down event on datagrid.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Are you sure you want to delete " + dataGrid.SelectedItems.Count + " selected item(s)?", "Delete Selected Items", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles click event on button.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationRequest m_request = new AuthenticationRequest();
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog.DefaultExt = ".xml";
            DialogResult res = openFileDialog.ShowDialog();

            if (res != DialogResult.Cancel)
            {
                m_request = Serialization.Deserialize<AuthenticationRequest>(File.ReadAllBytes(openFileDialog.FileName), SerializationFormat.Xml);

                Subscriber subscriber = new Subscriber
                    {
                    //NodeID = ((KeyValuePair<Guid, string>)ComboboxNode.SelectedItem).Key,
                    Acronym = m_request.Acronym.ToUpper(),
                    Name = m_request.Name,
                    SharedSecret = m_request.SharedSecret,
                    AuthKey = m_request.AuthenticationID,
                    ValidIPAddresses = m_request.ValidIPAddresses
                };

                m_dataContext.CurrentItem = subscriber;
                m_key = m_request.Key;
                m_iv = m_request.IV;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.FileName = m_dataContext.CurrentItem.RemoteCertificateFile;
            openFileDialog.DefaultExt = ".cer";
            openFileDialog.Filter = "Certificate files|*.cer|All Files|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                if ((object)m_dataContext.CurrentItem != null)
                    m_dataContext.CurrentItem.RemoteCertificateFile = openFileDialog.FileName;
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Load current key and initialization vector for given shared secret.
        /// </summary>
        private void LoadCurrentKeyIV()
        {
            // After record has been loaded, load existing key and IV from crypto cache
            if (string.IsNullOrWhiteSpace(m_sharedSecretField.Text))
            {
                m_key = "";
                m_iv = "";
            }
            else
            {
                string keyIV = Cipher.ExportKeyIV(m_sharedSecretField.Text, 256);
                string[] parts = keyIV.Split('|');

                m_key = parts[0];
                m_iv = parts[1];
            }
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            m_dataContext.SortData(e.Column.SortMemberPath);
        }

        private void DetailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_dataContext.IsNewRecord)
                DataGridList.SelectedIndex = -1;
        }

        #endregion

    }
}
