//******************************************************************************************************
//  SubscriberRequestViewModel.cs - Gbtc
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
//  11/29/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using GSF.Collections;
using GSF.Data;
using GSF.IO;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.Security.Cryptography;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using Microsoft.Win32;
using Random = GSF.Security.Cryptography.Random;

namespace GSF.TimeSeries.Transport.UI.ViewModels
{
    internal class SubscriberRequestViewModel : ViewModelBase
    {
        #region [ Members ]

        // Fields
        private string m_acronym;
        private string m_name;
        private string m_hostname;
        private int m_port;
        private SecurityMode m_securityMode;

        private string m_localCertificateFile;
        private string m_remoteCertificateFile;
        private bool m_isRemoteCertificateSelfSigned;
        private string m_validPolicyErrors;
        private string m_validChainFlags;

        private string m_sharedKey;
        private string m_identityCertificate;
        private string m_validIPAddresses;

        private ICommand m_generateCommand;
        private ICommand m_localBrowseCommand;
        private ICommand m_remoteBrowseCommand;
        private ICommand m_createCommand;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SubscriberRequestViewModel"/> class.
        /// </summary>
        public SubscriberRequestViewModel()
        {
            m_port = 6172;
            m_securityMode = SecurityMode.TLS;
            Load();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the string identifier used to identify the subscriber.
        /// </summary>
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
                OnPropertyChanged("Acronym");
            }
        }

        /// <summary>
        /// Gets or sets the name of the subscriber.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the host name or IP address of the server hosting the data publisher.
        /// </summary>
        public string Hostname
        {
            get
            {
                return m_hostname;
            }
            set
            {
                m_hostname = value;
                OnPropertyChanged("Hostname");
            }
        }

        /// <summary>
        /// Gets or sets the port that the data publisher is listening on.
        /// </summary>
        public int Port
        {
            get
            {
                return m_port;
            }
            set
            {
                m_port = value;
                OnPropertyChanged("Port");
            }
        }

        /// <summary>
        /// Gets or sets the security mode used by the data publisher.
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
        /// Gets or sets the path to the local certificate used to identify the subscriber.
        /// </summary>
        public string LocalCertificateFile
        {
            get
            {
                return m_localCertificateFile;
            }
            set
            {
                m_localCertificateFile = value;
                OnPropertyChanged("LocalCertificateFile");
            }
        }

        /// <summary>
        /// Gets or sets the path to the remote certificate used to identify the publisher.
        /// </summary>
        public string RemoteCertificateFile
        {
            get
            {
                return m_remoteCertificateFile;
            }
            set
            {
                m_remoteCertificateFile = value;
                OnPropertyChanged("RemoteCertificateFile");
            }
        }

        /// <summary>
        /// Gets or sets the flag that indicates whether the remote certificate is a self-signed certificate.
        /// </summary>
        public bool RemoteCertificateIsSelfSigned
        {
            get
            {
                return m_isRemoteCertificateSelfSigned;
            }
            set
            {
                m_isRemoteCertificateSelfSigned = value;
                OnPropertyChanged("SelfSigned");
            }
        }

        /// <summary>
        /// Gets or sets the list of valid policy errors that can occur during remote certificate validation.
        /// </summary>
        public string ValidPolicyErrors
        {
            get
            {
                return m_validPolicyErrors;
            }
            set
            {
                m_validPolicyErrors = value;
                OnPropertyChanged("ValidPolicyErrors");
            }
        }

        /// <summary>
        /// Gets or sets the list of valid chain flags which can be set during remote certificate validation.
        /// </summary>
        public string ValidChainFlags
        {
            get
            {
                return m_validChainFlags;
            }
            set
            {
                m_validChainFlags = value;
                OnPropertyChanged("ValidChainFlags");
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
        /// Gets or sets the shared key sent to the publisher for Gateway security encryption.
        /// </summary>
        public string SharedKey
        {
            get
            {
                return m_sharedKey;
            }
            set
            {
                m_sharedKey = value;
                OnPropertyChanged("SharedKey");
            }
        }

        /// <summary>
        /// Gets or sets the identity certificate exchanged during Gateway security authentication.
        /// </summary>
        public string IdentityCertificate
        {
            get
            {
                return m_identityCertificate;
            }
            set
            {
                m_identityCertificate = value;
                OnPropertyChanged("IdentityCertificate");
            }
        }

        /// <summary>
        /// Gets or sets the set of valid IP addresses used by the publisher in order to validate the subscriber's identity.
        /// </summary>
        public string ValidIPAddresses
        {
            get
            {
                return m_validIPAddresses;
            }
            set
            {
                m_validIPAddresses = value;
                OnPropertyChanged("ValidIPAddresses");
            }
        }

        /// <summary>
        /// Gets the command that executes when the user chooses to browse for a local certificate.
        /// </summary>
        public ICommand LocalBrowseCommand
        {
            get
            {
                if ((object)m_localBrowseCommand == null)
                    m_localBrowseCommand = new RelayCommand(BrowseLocalCertificateFile, () => true);

                return m_localBrowseCommand;
            }
        }

        /// <summary>
        /// Gets the command that executes when the user chooses to browse for a remote certificate.
        /// </summary>
        public ICommand RemoteBrowseCommand
        {
            get
            {
                if ((object)m_remoteBrowseCommand == null)
                    m_remoteBrowseCommand = new RelayCommand(BrowseRemoteCertificateFile, () => true);

                return m_remoteBrowseCommand;
            }
        }

        /// <summary>
        /// Gets the command that executes when the user chooses to create the authentication request.
        /// </summary>
        public ICommand CreateCommand
        {
            get
            {
                if ((object)m_createCommand == null)
                    m_createCommand = new RelayCommand(CreateAuthenticationRequest, () => true);

                return m_createCommand;
            }
        }

        #endregion

        #region [ Methods ]

        private void Load()
        {
            // Connect to database to retrieve company information for current node
            using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
            {
                try
                {
                    string query = database.ParameterizedQueryString("SELECT Company.Acronym, Company.Name FROM Company, Node WHERE Company.ID = Node.CompanyID AND Node.ID = {0}", "id");
                    DataRow row = database.Connection.RetrieveRow(database.AdapterType, query, database.CurrentNodeID());

                    Acronym = row.Field<string>("Acronym");
                    Name = row.Field<string>("Name");

                    // Generate a default shared secret password for subscriber key and initialization vector
                    byte[] buffer = new byte[4];
                    Random.GetBytes(buffer);

                    string generatedSecret = Convert.ToBase64String(buffer).RemoveCrLfs();

                    if (generatedSecret.Contains("="))
                        generatedSecret = generatedSecret.Split('=')[0];

                    SharedKey = generatedSecret;

                    // Generate an identity for this subscriber
                    AesManaged sa = new AesManaged();
                    sa.GenerateKey();
                    IdentityCertificate = Convert.ToBase64String(sa.Key);

                    // Generate valid local IP addresses for this connection
                    IEnumerable<IPAddress> addresses = Dns.GetHostAddresses(Dns.GetHostName()).OrderBy(key => key.AddressFamily);
                    ValidIPAddresses = addresses.ToDelimitedString("; ");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR: " + ex.Message, "Subscriber Request", MessageBoxButton.OK);
                }

                try
                {
                    Dictionary<string, string> settings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    IPAddress[] hostIPs = null;
                    IEnumerable<IPAddress> localIPs;

                    settings = database.ServiceConnectionString().ParseKeyValuePairs();
                    localIPs = Dns.GetHostAddresses("localhost").Concat(Dns.GetHostAddresses(Dns.GetHostName()));

                    if (settings.ContainsKey("server"))
                        hostIPs = Dns.GetHostAddresses(settings["server"].Split(':')[0]);

                    // Check to see if entered host name corresponds to a local IP address
                    if (hostIPs == null)
                        MessageBox.Show("Failed to find service host address. Secure key exchange may not succeed." + Environment.NewLine + "Please make sure to run manager application with administrative privileges on the server where service is hosted.", "Subscription Request", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else if (!hostIPs.Any(ip => localIPs.Contains(ip)))
                        MessageBox.Show("Secure key exchange may not succeed." + Environment.NewLine + "Please make sure to run manager application with administrative privileges on the server where service is hosted.", "Subscription Request", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch
                {
                    MessageBox.Show("Please make sure to run manager application with administrative privileges on the server where service is hosted.", "Subscription Request", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BrowseLocalCertificateFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.FileName = LocalCertificateFile;
            fileDialog.DefaultExt = ".cer";
            fileDialog.Filter = "Certificate files|*.cer|All Files|*.*";

            if (fileDialog.ShowDialog() == true)
                LocalCertificateFile = fileDialog.FileName;
        }

        private void BrowseRemoteCertificateFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.FileName = RemoteCertificateFile;
            fileDialog.DefaultExt = ".cer";
            fileDialog.Filter = "Certificate files|*.cer|All Files|*.*";

            if (fileDialog.ShowDialog() == true)
                RemoteCertificateFile = fileDialog.FileName;
        }

        private void CreateAuthenticationRequest()
        {
            try
            {
                ExportAuthorizationRequest();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Subscription Request Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Subscription Request", ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Subscription Request Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Subscription Request", ex);
                }
            }
        }

        // Export the authorization request.
        private void ExportAuthorizationRequest()
        {
            const string messageFormat = "Data subscription adapter \"{0}\" already exists. Unable to create subscription request.";

            Device device;
            SaveFileDialog saveFileDialog;

            AuthenticationRequest request;
            string requestAcronym;
            string requestName;
            string[] keyIV;

            if (!string.IsNullOrWhiteSpace(Acronym))
            {
                // Check if the device already exists
                device = GetDeviceByAcronym(Acronym.Replace(" ", ""));

                if ((object)device != null)
                    throw new Exception(string.Format(messageFormat, device.Acronym));

                if (SecurityMode == SecurityMode.TLS)
                {
                    // TLS security mode saves Device record for subscriber,
                    // but does not generate XML authorization request file
                    SaveDevice();
                }
                else
                {
                    // Gateway security mode needs to generate XML authorization request
                    saveFileDialog = new SaveFileDialog();
                    saveFileDialog.DefaultExt = ".xml";
                    saveFileDialog.Filter = "XML Files|*.xml|All Files|*.*";

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        request = new AuthenticationRequest();

                        // Get the name and acronym that go into the authentication request
                        if (TryGetCompanyAcronym(out requestAcronym))
                        {
                            requestName = string.Format("{0} subscription authorization", requestAcronym);
                        }
                        else
                        {
                            requestAcronym = Acronym;
                            requestName = "Subscription authorization";
                        }

                        // Export cipher key to common crypto cache
                        if (!ExportCipherKey(SharedKey, 256))
                            throw new Exception("Failed to export cipher keys from common key cache.");

                        // Reload local crypto cache and get key and IV
                        // that go into the authentication request
                        Cipher.ReloadCache();
                        keyIV = Cipher.ExportKeyIV(SharedKey, 256).Split('|');

                        // Set up the request
                        request.Acronym = requestAcronym;
                        request.Name = requestName;
                        request.SharedSecret = SharedKey;
                        request.AuthenticationID = IdentityCertificate;
                        request.Key = keyIV[0];
                        request.IV = keyIV[1];
                        request.ValidIPAddresses = ValidIPAddresses;

                        // Create the request
                        File.WriteAllBytes(saveFileDialog.FileName, Serialization.Serialize(request, SerializationFormat.Xml));

                        // Send ReloadCryptoCache to service
                        ReloadServiceCryptoCache();

                        // Save the associated device
                        SaveDevice();
                    }
                }
            }
            else
            {
                MessageBox.Show("Acronym is a required field. Please provide value.");
            }
        }

        // Gets the device from the database with the given acronym for the currently selected node.
        private Device GetDeviceByAcronym(string acronym)
        {
            AdoDataConnection database = null;
            string nodeID;

            try
            {
                database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                nodeID = database.CurrentNodeID().ToString();
                return Device.GetDevice(database, string.Format(" WHERE NodeID = '{0}' AND Acronym = '{1}'", nodeID, acronym));
            }
            finally
            {
                if ((object)database != null)
                    database.Dispose();
            }
        }

        // Attempt to get the company acronym stored in the openPG configuration file.
        private bool TryGetCompanyAcronym(out string acronym)
        {
            string configFileName;
            FileStream configStream = null;
            StreamReader configStreamReader = null;
            XElement configRoot;
            XElement systemSettings;

            try
            {
                // Set up XML searching
                configFileName = FilePath.GetAbsolutePath("openPG.exe.config");
                configStream = File.Open(configFileName, FileMode.Open, FileAccess.Read);
                configStreamReader = new StreamReader(configStream);
                configRoot = XElement.Parse(configStreamReader.ReadToEnd());

                // Find company name and company acronym
                systemSettings = configRoot.Element("categorizedSettings").Element("systemSettings");
                acronym = systemSettings.Elements().Single(e => e.Attribute("name").Value == "CompanyAcronym").Attribute("value").Value;

                // Indicate success
                return true;
            }
            catch
            {
                // Company info retrieval failed
                acronym = null;
                return false;
            }
            finally
            {
                if (configStreamReader != null)
                    configStreamReader.Dispose();

                if (configStream != null)
                    configStream.Dispose();
            }
        }

        // Exports the given cipher key from the common key cache.
        private bool ExportCipherKey(string password, int keySize)
        {
            ProcessStartInfo configCrypterInfo = new ProcessStartInfo();
            Process configCrypter;

            configCrypterInfo.FileName = FilePath.GetAbsolutePath("ConfigCrypter.exe");
            configCrypterInfo.Arguments = string.Format("-password {0} -keySize {1}", password, keySize);
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

        // Associate the given device with the
        // authorization request and save it.
        private void SaveDevice()
        {
            const string connectionStringFormat = "interface=0.0.0.0; compression=false; autoConnect=true; securityMode={0}; " +
                "localport=6175; transportprotocol=udp; commandChannel={{server={1}:{2}; interface=0.0.0.0}}; {3}";

            Device device;
            SslPolicyErrors validPolicyErrors;
            X509ChainStatusFlags validChainFlags;
            string securitySpecificSettings = string.Empty;

            if (SecurityMode == SecurityMode.Gateway)
            {
                securitySpecificSettings = string.Format("sharedSecret={0}; authenticationID={{{1}}}", SharedKey, IdentityCertificate);
            }
            else if (SecurityMode == SecurityMode.TLS)
            {
                if (!Enum.TryParse(ValidPolicyErrors, out validPolicyErrors))
                    validPolicyErrors = SslPolicyErrors.None;

                if (!Enum.TryParse(ValidChainFlags, out validChainFlags))
                    validChainFlags = X509ChainStatusFlags.NoError;

                if (RemoteCertificateIsSelfSigned)
                {
                    validPolicyErrors |= SslPolicyErrors.RemoteCertificateChainErrors;
                    validChainFlags |= X509ChainStatusFlags.UntrustedRoot;
                }
                    
                securitySpecificSettings = string.Format("localCertificate={0}; remoteCertificate={1}; validPolicyErrors={2}; validChainFlags={3}",
                    LocalCertificateFile, RemoteCertificateFile, validPolicyErrors, validChainFlags);
            }

            device = new Device();
            device.Acronym = Acronym.Replace(" ", "");
            device.Name = Name;
            device.Enabled = false;
            device.IsConcentrator = true;
            device.ProtocolID = GetGatewayProtocolID();
            device.ConnectionString = string.Format(connectionStringFormat, SecurityMode, Hostname, Port, securitySpecificSettings);

            Device.Save(null, device);
        }

        // Get the Gateway Transport protocol ID by querying the database.
        private int? GetGatewayProtocolID()
        {
            const string query = "SELECT ID FROM Protocol WHERE Acronym = 'GatewayTransport'";
            AdoDataConnection database = null;
            object queryResult;

            try
            {
                database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                queryResult = database.Connection.ExecuteScalar(query);
                return (queryResult != null) ? Convert.ToInt32(queryResult) : 8;
            }
            finally
            {
                if ((object)database != null)
                    database.Dispose();
            }
        }

        #endregion
    }
}
