//******************************************************************************************************
//  SubscriberRequestViewModel.cs - Gbtc
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
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using GSF.Collections;
using GSF.Console;
using GSF.Data;
using GSF.IO;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.PhasorProtocols.UI.UserControls;
using GSF.Security.Cryptography;
using GSF.ServiceProcess;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.Commands;
using GSF.Units;
using Microsoft.Win32;
using Random = GSF.Security.Cryptography.Random;

namespace GSF.TimeSeries.Transport.UI.ViewModels
{
    internal class SubscriberRequestViewModel : ViewModelBase, IDisposable
    {
        #region [ Members ]

        // Fields
        private string m_publisherAcronym;
        private string m_publisherName;
        private string m_hostname;
        private int m_publisherPort;
        private SecurityMode m_securityMode;
        private bool m_receiveExternalMetadata;
        private bool m_receiveInternalMetadata;
        private bool m_useUdpDataChannel;
        private int m_udpDataChannelPort;
        private bool m_enableDataGapRecovery;
        private double m_recoveryStartDelay;
        private double m_dataMonitoringInterval;
        private double m_minimumRecoverySpan;
        private double m_maximumRecoverySpan;
        private int m_recoveryProcessingInterval;
        private string m_loggingPath;
        private bool m_disposed;

        private string m_subscriberAcronym;
        private string m_subscriberName;
        private int m_internalDataPublisherPort;

        private string m_sharedKey;
        private string m_identityCertificate;
        private string m_validIPAddresses;

        private string m_localCertificateFile;
        private string m_localCertificateServerPath;
        private string m_importCertificatePath;
        private byte[] m_localCertificateData;
        private string m_remoteCertificateFile;
        private byte[] m_remoteCertificateData;
        private bool m_isRemoteCertificateSelfSigned;
        private string m_validPolicyErrors;
        private string m_validChainFlags;

        private Visibility m_connectivityMessageVisibility;
        private bool m_advancedTlsSettingsPopupIsOpen;

        private AutoResetEvent m_responseComplete;
        private ICommand m_localBrowseCommand;
        private ICommand m_remoteBrowseCommand;
        private ICommand m_importCertificateCommand;
        private ICommand m_advancedTlsSettingsOpenCommand;
        private ICommand m_advancedTlsSettingsCloseCommand;
        private ICommand m_createCommand;
        private ICommand m_saveCommand;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SubscriberRequestViewModel"/> class.
        /// </summary>
        public SubscriberRequestViewModel()
        {
            m_internalDataPublisherPort = 6170;
            m_securityMode = SecurityMode.TLS;
            m_receiveInternalMetadata = true;
            m_udpDataChannelPort = 6175;
            m_publisherPort = DefaultPort;
            m_responseComplete = new AutoResetEvent(false);

            Load();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="SubscriberRequestViewModel"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~SubscriberRequestViewModel()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the string identifier used to identify the publisher.
        /// </summary>
        public string PublisherAcronym
        {
            get
            {
                return m_publisherAcronym;
            }
            set
            {
                bool updateRemoteCertificateFile = (string.IsNullOrEmpty(m_publisherAcronym) && string.IsNullOrEmpty(m_remoteCertificateFile)) ||
                    m_remoteCertificateFile.Equals(string.Format("{0}.cer", m_publisherAcronym), StringComparison.CurrentCultureIgnoreCase);

                m_publisherAcronym = value;
                OnPropertyChanged("Acronym");

                if (updateRemoteCertificateFile)
                    RemoteCertificateFile = string.IsNullOrEmpty(m_publisherAcronym) ? string.Empty : string.Format("{0}.cer", m_publisherAcronym);
            }
        }

        /// <summary>
        /// Gets or sets the name of the publisher.
        /// </summary>
        public string PublisherName
        {
            get
            {
                return m_publisherName;
            }
            set
            {
                m_publisherName = value;
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
        public int PublisherPort
        {
            get
            {
                return m_publisherPort;
            }
            set
            {
                m_publisherPort = value;
                OnPropertyChanged("PublisherPort");
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
                int oldDefaultPort = DefaultPort;

                m_securityMode = value;
                OnPropertyChanged("SecurityMode");
                OnPropertyChanged("TransportLayerSecuritySelected");
                OnPropertyChanged("GatewaySecuritySelected");

                if (m_publisherPort == oldDefaultPort)
                    PublisherPort = DefaultPort;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the subscriber is
        /// requesting metadata for internal signals from the publisher.
        /// </summary>
        public bool ReceiveInternalMetadata
        {
            get
            {
                return m_receiveInternalMetadata;
            }
            set
            {
                m_receiveInternalMetadata = value;
                OnPropertyChanged("ReceiveInternalMetadata");

                if (!m_receiveInternalMetadata && !m_receiveExternalMetadata)
                    ReceiveExternalMetadata = true;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the subscriber is
        /// requesting metadata for external signals from the publisher.
        /// </summary>
        public bool ReceiveExternalMetadata
        {
            get
            {
                return m_receiveExternalMetadata;
            }
            set
            {
                m_receiveExternalMetadata = value;
                OnPropertyChanged("ReceiveExternalMetadata");

                if (!m_receiveInternalMetadata && !m_receiveExternalMetadata)
                    ReceiveInternalMetadata = true;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the subscriber
        /// should be configured with or without a UDP data channel.
        /// </summary>
        public bool UseUdpDataChannel
        {
            get
            {
                return m_useUdpDataChannel;
            }
            set
            {
                m_useUdpDataChannel = value;
                OnPropertyChanged("UseUdpDataChannel");
            }
        }

        /// <summary>
        /// Gets or sets the port used for the UDP data channel.
        /// </summary>
        public int UdpDataChannelPort
        {
            get
            {
                return m_udpDataChannelPort;
            }
            set
            {
                m_udpDataChannelPort = value;
                OnPropertyChanged("UdpDataChannelPort");
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data gap recovery will be enabled.
        /// </summary>
        public bool EnableDataGapRecovery
        {
            get
            {
                return m_enableDataGapRecovery;
            }
            set
            {
                m_enableDataGapRecovery = value;
                OnPropertyChanged("EnableDataGapRecovery");
            }
        }

        /// <summary>
        /// Gets or sets the minimum time delay, in seconds, to wait before starting the data recovery for an <see cref="Outage"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For some archiving systems it may take a few seconds for data to make it to disk and therefore be readily
        /// available for a temporal subscription query. The <see cref="RecoveryStartDelay"/> should be adjusted based
        /// on the nature of the system used to archive data. If the archival system makes data immediately available
        /// because of internal caching or other means, this value can be zero.
        /// </para>
        /// <para>
        /// Use of this value depends on the local clock, as such the value should be increased by the uncertainty of
        /// accuracy of the local clock. For example, if it is know that the local clock floats +/-5 seconds from
        /// real-time, then increase the desired value of the <see cref="RecoveryStartDelay"/> by 5 seconds.
        /// </para>
        /// </remarks>
        public double RecoveryStartDelay
        {
            get
            {
                return m_recoveryStartDelay;
            }
            set
            {
                m_recoveryStartDelay = value;
                OnPropertyChanged("RecoveryStartDelay");
            }
        }

        /// <summary>
        /// Gets or sets the interval, in seconds, over which the data monitor will check for new data.
        /// </summary>
        /// <remarks>
        /// Once a connection is established a timer is enabled to monitor for new incoming data. The data monitoring timer
        /// exists to make sure data is being received so that the process of recovery does not wait endlessly for data that
        /// may never come because of a possible error in the recovery process. The <see cref="DataMonitoringInterval"/>
        /// allows the consumer to adjust the interval over which the timer will check for new incoming data.
        /// <para>
        /// It will take some time, perhaps a couple of seconds, to start the temporal subscription and begin the process
        /// of recovering data for an <see cref="Outage"/>. Make sure the value for <see cref="DataMonitoringInterval"/> is
        /// sufficiently large enough to handle any initial delays in data transmission.
        /// </para>
        /// </remarks>
        public double DataMonitoringInterval
        {
            get
            {
                return m_dataMonitoringInterval;
            }
            set
            {
                m_dataMonitoringInterval = value;
                OnPropertyChanged("DataMonitoringInterval");
            }
        }

        /// <summary>
        /// Gets to sets the minimum time span, in seconds, for which a data recovery will be attempted.
        /// Set to zero for no minimum.
        /// </summary>
        public double MinimumRecoverySpan
        {
            get
            {
                return m_minimumRecoverySpan;
            }
            set
            {
                m_minimumRecoverySpan = value;
                OnPropertyChanged("MinimumRecoverySpan");
            }
        }

        /// <summary>
        /// Gets to sets the maximum time span, in days for UI - API is seconds, for which a data recovery will be attempted.
        /// Set to <see cref="Double.MaxValue"/> for no maximum.
        /// </summary>
        public double MaximumRecoverySpan
        {
            get
            {
                return m_maximumRecoverySpan;
            }
            set
            {
                m_maximumRecoverySpan = value;
                OnPropertyChanged("MaximumRecoverySpan");
            }
        }

        /// <summary>
        /// Gets or sets the data recovery processing interval, in whole milliseconds, to use in the temporal data
        /// subscription when recovering data for an <see cref="Outage"/>.<br/>
        /// A value of <c>-1</c> indicates the default processing interval will be requested.<br/>
        /// A value of <c>0</c> indicates data will be processed as fast as possible.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, the <see cref="RecoveryProcessingInterval"/> value specifies
        /// the desired historical data playback processing interval in milliseconds. This is basically a delay, or timer
        /// interval, over which to process data. Setting this value to -1 means to use the default processing interval
        /// while setting the value to 0 means to process data as fast as possible, i.e., as fast as the historian can
        /// query the data. Depending on the available bandwidth, this parameter may need to be adjusted such that the
        /// data being recovered does not adversely interfere with the ongoing transmission of real-time data.
        /// </remarks>
        public int RecoveryProcessingInterval
        {
            get
            {
                return m_recoveryProcessingInterval;
            }
            set
            {
                m_recoveryProcessingInterval = value;
                OnPropertyChanged("RecoveryProcessingInterval");
            }
        }

        /// <summary>
        /// Gets or sets logging path to be used to be runtime and outage logs of the subscriber which are required for
        /// automated data recovery.
        /// </summary>
        /// <remarks>
        /// Leave value blank for default path, i.e., installation folder. Can be a fully qualified path or a path that
        /// is relative to the installation folder, e.g., a value of "ConfigurationCache" might resolve to
        /// "C:\Program Files\MyTimeSeriespPp\ConfigurationCache\".
        /// </remarks>
        public string LoggingPath
        {
            get
            {
                return m_loggingPath;
            }
            set
            {
                m_loggingPath = value;
                OnPropertyChanged("LoggingPath");
            }
        }

        /// <summary>
        /// Gets or sets the port used by the internal data publisher.
        /// </summary>
        public int InternalDataPublisherPort
        {
            get
            {
                return m_internalDataPublisherPort;
            }
            set
            {
                int oldDefaultPort = DefaultPort;

                m_internalDataPublisherPort = value;

                if (m_publisherPort == oldDefaultPort)
                    PublisherPort = DefaultPort;
            }
        }

        /// <summary>
        /// Gets or sets the string identifier used to identify the subscriber.
        /// </summary>
        public string SubscriberAcronym
        {
            get
            {
                return m_subscriberAcronym;
            }
            set
            {
                m_subscriberAcronym = value;
                OnPropertyChanged("SubscriberAcronym");
            }
        }

        /// <summary>
        /// Gets or sets the name of the subscriber.
        /// </summary>
        public string SubscriberName
        {
            get
            {
                return m_subscriberName;
            }
            set
            {
                m_subscriberName = value;
                OnPropertyChanged("SubscriberName");
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
                bool updateServerPath = (string.IsNullOrEmpty(m_localCertificateFile) && string.IsNullOrEmpty(m_localCertificateServerPath)) ||
                    ((object)m_localCertificateFile != null && m_localCertificateFile.Equals(m_localCertificateServerPath, StringComparison.CurrentCultureIgnoreCase));

                m_localCertificateFile = value;
                OnPropertyChanged("LocalCertificateFile");

                if (updateServerPath)
                    LocalCertificateServerPath = m_localCertificateFile;
            }
        }

        /// <summary>
        /// Gets or sets the path on the server to the local certificate used to identify the subscriber.
        /// </summary>
        public string LocalCertificateServerPath
        {
            get
            {
                return m_localCertificateServerPath;
            }
            set
            {
                m_localCertificateServerPath = value;
                OnPropertyChanged("LocalCertificateServerPath");
            }
        }

        /// <summary>
        /// Gets or sets the path to the certificate to be imported to the service.
        /// </summary>
        public string ImportCertificatePath
        {
            get
            {
                return m_importCertificatePath;
            }
            set
            {
                m_importCertificatePath = value;
                OnPropertyChanged("ImportCertificatePath");
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

        public ICommand RemoteBrowseCommand
        {
            get
            {
                return m_remoteBrowseCommand ?? (m_remoteBrowseCommand = new RelayCommand(BrowseRemoteCertificateFile, () => true));
            }
        }

        /// <summary>
        /// Gets the command that executes when the user chooses to browse for a remote certificate.
        /// </summary>
        public ICommand ImportCertificateCommand
        {
            get
            {
                if ((object)m_importCertificateCommand == null)
                    m_importCertificateCommand = new RelayCommand(ImportCertificateFile, () => true);

                return m_importCertificateCommand;
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

        /// <summary>
        /// Gets the command that executes when the user chooses to save their subscriber.
        /// </summary>
        public ICommand NextCommand
        {
            get
            {
                if ((object)m_saveCommand == null)
                    m_saveCommand = new RelayCommand(Save, () => true);

                return m_saveCommand;
            }
        }

        /// <summary>
        /// Gets the default port based on the current security mode selection.
        /// </summary>
        private int DefaultPort
        {
            get
            {
                switch (m_securityMode)
                {
                    case SecurityMode.None:
                        return m_internalDataPublisherPort;

                    case SecurityMode.Gateway:
                        return m_internalDataPublisherPort + 1;

                    case SecurityMode.TLS:
                        return m_internalDataPublisherPort + 2;

                    default:
                        return m_internalDataPublisherPort;
                }
            }
        }

        /// <summary>
        /// Gets or sets the visibility of the connectivity message.
        /// </summary>
        public Visibility ConnectivityMessageVisibility
        {
            get
            {
                return m_connectivityMessageVisibility;
            }
            set
            {
                m_connectivityMessageVisibility = value;
                OnPropertyChanged("ConnectivityMessageVisibility");
            }
        }

        /// <summary>
        /// Gets or sets whether the advanced TLS settings popup is open.
        /// </summary>
        public bool AdvancedTlsSettingsPopupIsOpen
        {
            get
            {
                return m_advancedTlsSettingsPopupIsOpen;
            }
            set
            {
                m_advancedTlsSettingsPopupIsOpen = value;
                OnPropertyChanged("AdvancedTlsSettingsPopupIsOpen");
            }
        }

        /// <summary>
        /// Gets the command that is executed when the user chooses to view the advanced TLS settings.
        /// </summary>
        public ICommand AdvancedTlsSettingsOpenCommand
        {
            get
            {
                return m_advancedTlsSettingsOpenCommand ?? (m_advancedTlsSettingsOpenCommand = new RelayCommand(() => AdvancedTlsSettingsPopupIsOpen = true, () => true));
            }
        }

        /// <summary>
        /// Gets the command that is executed went he user chooses to save adv
        /// </summary>
        public ICommand AdvancedTlsSettingsCloseCommand
        {
            get
            {
                return m_advancedTlsSettingsCloseCommand ?? (m_advancedTlsSettingsCloseCommand = new RelayCommand(() => AdvancedTlsSettingsPopupIsOpen = false, () => true));
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="SubscriberRequestViewModel"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SubscriberRequestViewModel"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_responseComplete != null)
                        {
                            // Release any waiting threads before disposing wait handle
                            m_responseComplete.Set();
                            m_responseComplete.Dispose();
                        }

                        m_responseComplete = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Initializes default settings for internal subscription.
        /// </summary>
        public void InitializeDefaultInternalSubscriptionSettings()
        {
            RecoveryStartDelay = DataGapRecoverer.DefaultRecoveryStartDelay;
            DataMonitoringInterval = DataGapRecoverer.DefaultDataMonitoringInterval;
            MinimumRecoverySpan = DataGapRecoverer.DefaultMinimumRecoverySpan;
            MaximumRecoverySpan = DataGapRecoverer.DefaultMaximumRecoverySpan / Time.SecondsPerDay;
            RecoveryProcessingInterval = DataGapRecoverer.DefaultRecoveryProcessingInterval;
            LoggingPath = DataSubscriber.DefaultLoggingPath;
        }

        private void Load()
        {
            string companyAcronym;

            // Try to populate defaults for subscriber acronym and name using company information from the host application configuration file
            if (TryGetCompanyAcronym(out companyAcronym))
            {
                SubscriberAcronym = companyAcronym;
                SubscriberName = string.Format("{0} Subscription Authorization", companyAcronym);
            }

            // Connect to database to retrieve company information for current node
            using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
            {
                try
                {
                    string query = database.ParameterizedQueryString("SELECT Company.Acronym, Company.Name FROM Company, Node WHERE Company.ID = Node.CompanyID AND Node.ID = {0}", "id");
                    DataRow row = database.Connection.RetrieveRow(database.AdapterType, query, database.CurrentNodeID());

                    PublisherAcronym = row.Field<string>("Acronym");
                    PublisherName = row.Field<string>("Name");

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
                    Dictionary<string, string> settings;
                    string server;
                    string[] splitServer;
                    int dataPublisherPort;

                    //IPAddress[] hostIPs = null;
                    //IEnumerable<IPAddress> localIPs;

                    settings = database.DataPublisherConnectionString().ToNonNullString().ParseKeyValuePairs();
                    //localIPs = Dns.GetHostAddresses("localhost").Concat(Dns.GetHostAddresses(Dns.GetHostName()));

                    if (settings.TryGetValue("server", out server))
                    {
                        splitServer = server.Split(':');
                        //hostIPs = Dns.GetHostAddresses(splitServer[0]);

                        if (splitServer.Length > 1 && int.TryParse(splitServer[1], out dataPublisherPort))
                            InternalDataPublisherPort = dataPublisherPort;
                    }

                    // These messages show up when not desired and are not very useful anymore...
                    //// Check to see if entered host name corresponds to a local IP address
                    //if (hostIPs == null)
                    //    MessageBox.Show("Failed to find service host address. If using Gateway security, secure key exchange may not succeed." + Environment.NewLine + "Please make sure to run manager application with administrative privileges on the server where service is hosted.", "Subscription Request", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //else if (!hostIPs.Any(ip => localIPs.Contains(ip)))
                    //    MessageBox.Show("If using Gateway security, secure key exchange may not succeed." + Environment.NewLine + "Please make sure to run manager application with administrative privileges on the server where service is hosted.", "Subscription Request", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            fileDialog.CheckFileExists = false;
            fileDialog.CheckPathExists = false;

            if (fileDialog.ShowDialog() == true)
                LocalCertificateFile = fileDialog.FileName;
        }

        private void BrowseRemoteCertificateFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.FileName = RemoteCertificateFile;
            fileDialog.DefaultExt = ".cer";
            fileDialog.Filter = "Certificate files|*.cer|All Files|*.*";
            fileDialog.CheckFileExists = false;
            fileDialog.CheckPathExists = false;

            if (fileDialog.ShowDialog() == true)
                RemoteCertificateFile = fileDialog.FileName;
        }

        private void ImportCertificateFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.FileName = RemoteCertificateFile;
            fileDialog.DefaultExt = ".cer";
            fileDialog.Filter = "Certificate files|*.cer|All Files|*.*";
            fileDialog.CheckFileExists = false;
            fileDialog.CheckPathExists = false;

            if (fileDialog.ShowDialog() == true && File.Exists(fileDialog.FileName))
            {
                ImportCertificatePath = fileDialog.FileName;
                m_remoteCertificateData = File.ReadAllBytes(m_importCertificatePath);
            }
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
            const string MessageFormat = "Data subscription adapter \"{0}\" already exists. Unable to create subscription request.";

            Device device;

            if (!string.IsNullOrWhiteSpace(PublisherAcronym))
            {
                // Check if the device already exists
                device = GetDeviceByAcronym(PublisherAcronym.Replace(" ", ""));

                if ((object)device != null)
                    throw new Exception(string.Format(MessageFormat, device.Acronym));

                // Save the associated device
                if (TryCreateRequest())
                    SaveDevice();
            }
            else
            {
                MessageBox.Show("Acronym is a required field. Please provide value.");
            }
        }

        private bool TryCreateRequest()
        {
            try
            {
                // Generate authorization request
                SaveFileDialog saveFileDialog;
                WindowsServiceClient serviceClient;
                ClientRequest clientRequest;
                AuthenticationRequest request;
                string[] keyIV;

                saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = ".srq";
                saveFileDialog.Filter = "Subscription Requests|*.srq|All Files|*.*";

                if (saveFileDialog.ShowDialog() == true)
                {
                    request = new AuthenticationRequest();

                    // Set up the request
                    request.Acronym = SubscriberAcronym;
                    request.Name = SubscriberName;
                    request.ValidIPAddresses = ValidIPAddresses;

                    // Cipher key only applies to Gateway security
                    if (SecurityMode == SecurityMode.Gateway)
                    {
                        // Export cipher key to common crypto cache
                        if (!ExportCipherKey(SharedKey, 256))
                            throw new Exception("Failed to export cipher keys from common key cache.");

                        // Reload local crypto cache and get key and IV
                        // that go into the authentication request
                        Cipher.ReloadCache();
                        keyIV = Cipher.ExportKeyIV(SharedKey, 256).Split('|');

                        // Set up crypto settings in the request
                        request.SharedSecret = SharedKey;
                        request.AuthenticationID = IdentityCertificate;
                        request.Key = keyIV[0];
                        request.IV = keyIV[1];
                    }

                    // Local certificate only applies to TLS security
                    if (SecurityMode == SecurityMode.TLS)
                    {
                        if (File.Exists(m_localCertificateFile))
                        {
                            request.CertificateFile = File.ReadAllBytes(m_localCertificateFile);
                        }
                        else
                        {
                            try
                            {
                                serviceClient = CommonFunctions.GetWindowsServiceClient();
                                serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;
                                serviceClient.Helper.SendRequest("INVOKE TLS!DATAPUBLISHER GetLocalCertificate");

                                // Wait for command response allowing for processing time
                                if ((object)m_responseComplete != null)
                                {
                                    if (!m_responseComplete.WaitOne(5000))
                                        throw new TimeoutException("Timed-out after 5 seconds waiting for service response.");
                                }

                                request.CertificateFile = m_localCertificateData;
                            }
                            catch (Exception ex)
                            {
                                string message = string.Format("Unable to get the local certificate used by the service: {0}", ex.Message);
                                throw new InvalidOperationException(message, ex);
                            }
                        }

                        if ((object)m_remoteCertificateData != null)
                        {
                            serviceClient = CommonFunctions.GetWindowsServiceClient();
                            serviceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;

                            clientRequest = new ClientRequest("INVOKE");
                            clientRequest.Arguments = new Arguments(string.Format("TLS!DATAPUBLISHER ImportCertificate {0}", m_remoteCertificateFile));
                            clientRequest.Attachments.Add(m_remoteCertificateData);
                            serviceClient.Helper.SendRequest(clientRequest);

                            if ((object)m_responseComplete != null)
                            {
                                if (!m_responseComplete.WaitOne(5000))
                                    throw new InvalidOperationException("Timeout waiting for response to ImportCertificate command.");
                            }

                            m_remoteCertificateData = null;
                        }
                    }

                    // Create the request
                    using (FileStream requestStream = File.OpenWrite(saveFileDialog.FileName))
                    {
                        Serialization.Serialize(request, SerializationFormat.Binary, requestStream);
                    }

                    // Send ReloadCryptoCache to service
                    if (SecurityMode == SecurityMode.Gateway)
                        ReloadServiceCryptoCache();

                    return true;
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Error creating authorization request: {0}", ex.Message);
                Popup(message, "Subscription Request Error", MessageBoxImage.Error);
                CommonFunctions.LogException(null, "Subscription Request", ex);
            }

            return false;
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

        // Attempt to get the company acronym stored in the the configuration file.
        private bool TryGetCompanyAcronym(out string acronym)
        {
            XDocument document;

            // Initial value if acronym
            // never gets set explicitly
            acronym = null;

            try
            {
                // Check all application config files for company info
                foreach (string configFilePath in FilePath.GetFileList(FilePath.GetAbsolutePath("*.exe.config")))
                {
                    try
                    {
                        // Load the configuration file and search for CompanyAcronym
                        document = XDocument.Load(configFilePath);

                        if ((object)document.Root != null)
                        {
                            acronym = document.Root
                                .Descendants("systemSettings")
                                .Elements("add")
                                .Where(e => (string)e.Attribute("name") == "CompanyAcronym")
                                .Select(e => (string)e.Attribute("value"))
                                .SingleOrDefault();
                        }
                    }
                    catch
                    {
                        // Ignore exceptions here - simply check the next config file
                        continue;
                    }

                    if ((object)acronym != null)
                        break;
                }

                // Indicate success or failure
                return ((object)acronym != null);
            }
            catch
            {
                // Company info retrieval failed
                return false;
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

        private void Save()
        {
            try
            {
                SaveDevice();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Internal Subscription Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Internal Subscription", ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Internal Subscription Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Internal Subscription", ex);
                }
            }
        }

        // Associate the given device with the
        // authorization request and save it.
        private void SaveDevice()
        {
            const string TcpConnectionStringFormat = "interface=0.0.0.0; compression=true; autoConnect=true; securityMode={0}; " +
                "server={1}:{2}; {3}{4}";

            const string UdpConnectionStringFormat = "interface=0.0.0.0; compression=true; autoConnect=true; securityMode={0}; " +
                "localport={1}; transportprotocol=udp; commandChannel={{server={2}:{3}; interface=0.0.0.0}}; {4}{5}";

            Device device;

            SslPolicyErrors validPolicyErrors;
            X509ChainStatusFlags validChainFlags;
            string securitySpecificSettings = string.Empty;
            string dataGapRecoverySettings = string.Empty;

            if (SecurityMode == SecurityMode.None)
            {
                securitySpecificSettings = string.Format("internal={0}; receiveInternalMetadata={1}; receiveExternalMetadata={2}; outputMeasurements={{FILTER ActiveMeasurements WHERE Protocol = 'GatewayTransport'}}", !(m_receiveExternalMetadata && !m_receiveInternalMetadata), m_receiveInternalMetadata, m_receiveExternalMetadata);
            }
            else if (SecurityMode == SecurityMode.Gateway)
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

                securitySpecificSettings = string.Format("{0}remoteCertificate={1}; validPolicyErrors={2}; validChainFlags={3}; checkCertificateRevocation={4}",
                    GetLocalCertificateSetting(), RemoteCertificateFile, validPolicyErrors, validChainFlags, !m_isRemoteCertificateSelfSigned);
            }

            if (m_enableDataGapRecovery)
                dataGapRecoverySettings = string.Format("; dataGapRecovery={{enabled=true; recoveryStartDelay={0}; dataMonitoringInterval={1}; minimumRecoverySpan={2}; maximumRecoverySpan={3}; recoveryProcessingInterval={4}}}{5}", m_recoveryStartDelay, m_dataMonitoringInterval, m_minimumRecoverySpan, m_maximumRecoverySpan * Time.SecondsPerDay, m_recoveryProcessingInterval, string.IsNullOrWhiteSpace(m_loggingPath) ? "" : "; loggingPath=" + m_loggingPath);

            device = new Device();
            device.Acronym = PublisherAcronym.Replace(" ", "");
            device.Name = PublisherName;
            device.Enabled = m_securityMode == SecurityMode.None;  // TODO: Is this good logic? What if I want to tweak my subscribed measurement filter before I synchronize metadata?
            device.IsConcentrator = true;
            device.ProtocolID = GetGatewayProtocolID();

            if (UseUdpDataChannel)
                device.ConnectionString = string.Format(UdpConnectionStringFormat, SecurityMode, UdpDataChannelPort, Hostname, PublisherPort, securitySpecificSettings, dataGapRecoverySettings);
            else
                device.ConnectionString = string.Format(TcpConnectionStringFormat, SecurityMode, Hostname, PublisherPort, securitySpecificSettings, dataGapRecoverySettings);

            try
            {
                Device.SaveWithAnalogsDigitals(null, device, false, 0, 0);
            }
            catch (ApplicationException ex)
            {
                CommonFunctions.LogException(null, "Save Subscriber Device", ex);
            }

            device = Device.GetDevice(null, "WHERE Acronym = '" + device.Acronym + "'");
            CommonFunctions.LoadUserControl("Manage Device Configuration", typeof(DeviceUserControl), device);
        }

        // Get the setting for the local certificate path
        private string GetLocalCertificateSetting()
        {
            if (string.IsNullOrEmpty(LocalCertificateServerPath))
                return string.Empty;

            return string.Format("localCertificate={0}; ", LocalCertificateServerPath);
        }

        // Get the Gateway Transport protocol ID by querying the database.
        private int? GetGatewayProtocolID()
        {
            const string Query = "SELECT ID FROM Protocol WHERE Acronym = 'GatewayTransport'";
            AdoDataConnection database = null;
            object queryResult;

            try
            {
                database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                queryResult = database.Connection.ExecuteScalar(Query);
                return (queryResult != null) ? Convert.ToInt32(queryResult) : 8;
            }
            finally
            {
                if ((object)database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Handles ReceivedServiceResponse event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            if ((object)e != null)
            {
                ServiceResponse response = e.Argument;

                if ((object)response != null)
                {
                    string sourceCommand;
                    bool responseSuccess;

                    if (ClientHelper.TryParseActionableResponse(response, out sourceCommand, out responseSuccess) && responseSuccess)
                    {
                        if (!string.IsNullOrWhiteSpace(sourceCommand) && string.Compare(sourceCommand.Trim(), "INVOKE", true) == 0)
                        {
                            List<object> attachments = response.Attachments;

                            // A GetHighestSeverityAlarms INVOKE will have two attachments: an alarm array, item 0, and the original command arguments, item 1
                            if ((object)attachments != null && attachments.Count > 1)
                            {
                                Arguments arguments = attachments[1] as Arguments;

                                // Check the method that was invoked - the second argument after the adapter ID
                                if ((object)arguments != null && string.Compare(arguments["OrderedArg2"], "GetLocalCertificate", true) == 0)
                                {
                                    m_localCertificateData = attachments[0] as byte[];

                                    // Release waiting thread once desired response has been received
                                    if ((object)m_responseComplete != null)
                                        m_responseComplete.Set();
                                }
                                else if ((object)arguments != null && string.Compare(arguments["OrderedArg2"], "ImportCertificate", true) == 0)
                                {
                                    m_remoteCertificateFile = attachments[0] as string;

                                    // Release waiting thread once desired response has been received
                                    if ((object)m_responseComplete != null)
                                        m_responseComplete.Set();
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
