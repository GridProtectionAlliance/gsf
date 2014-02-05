//******************************************************************************************************
//  DataSubscriberSettings.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  01/17/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GSF.Communication;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport;

namespace GatewayProtocolAdapters
{
    /// <summary>
    /// Connection string settings for <see cref="DataSubscriberAdapter"/>.
    /// </summary>
    public class DataSubscriberSettings : InputAdapterSettingsBase
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Data subscriber settings specific to Gateway security mode.
        /// </summary>
        public class GatewaySecurity
        {
            private string m_sharedSecret;
            private string m_authenticationID;

            /// <summary>
            /// Gets or sets the shared secret used for command channel encryption.
            /// </summary>
            [ConnectionStringParameter,
            Description("Defines the shared secret used for command channel encryption.")]
            public string SharedSecret
            {
                get
                {
                    return m_sharedSecret;
                }
                set
                {
                    m_sharedSecret = value;
                }
            }

            /// <summary>
            /// Gets or sets the authentication ID used by the publisher to identify the subscriber.
            /// </summary>
            [ConnectionStringParameter,
            Description("Defines the authentication ID used by the publisher to identify the subscriber.")]
            public string AuthenticationID
            {
                get
                {
                    return m_authenticationID;
                }
                set
                {
                    m_authenticationID = value;
                }
            }
        }

        /// <summary>
        /// Data subscriber settings specific to TLS security mode.
        /// </summary>
        public class TlsSecurity
        {
            #region [ Members ]

            // Constants

            /// <summary>
            /// Specifies the default value for the <see cref="LocalCertificate"/> property.
            /// </summary>
            public const string DefaultLocalCertificate = null;

            /// <summary>
            /// Specifies the default value for the <see cref="ValidPolicyErrors"/> property.
            /// </summary>
            public const SslPolicyErrors DefaultValidPolicyErrors = SslPolicyErrors.None;

            /// <summary>
            /// Specifies the default value for the <see cref="ValidChainFlags"/> property.
            /// </summary>
            public const X509ChainStatusFlags DefaultValidChainFlags = X509ChainStatusFlags.NoError;

            /// <summary>
            /// Specifies the default value for the <see cref="CheckCertificateRevocation"/> property.
            /// </summary>
            public const bool DefaultCheckCertificateRevocation = true;

            // Fields
            private string m_localCertificate;
            private string m_remoteCertificate;
            private SslPolicyErrors m_validPolicyErrors;
            private X509ChainStatusFlags m_validChainFlags;
            private bool m_checkCertificateRevocation;

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets or sets the path to the certificate used by the publisher to identify the subscriber.
            /// </summary>
            [ConnectionStringParameter,
            DefaultValue(DefaultLocalCertificate),
            Description("Defines the path to the certificate used by the publisher to identify the subscriber.")]
            public string LocalCertificate
            {
                get
                {
                    return m_localCertificate;
                }
                set
                {
                    m_localCertificate = value;
                }
            }

            /// <summary>
            /// Gets or sets the path to the certificate used by the subscriber to identify the publisher.
            /// </summary>
            [ConnectionStringParameter,
            Description("Defines the path to the certificate used by the subscriber to identify the publisher.")]
            public string RemoteCertificate
            {
                get
                {
                    return m_remoteCertificate;
                }
                set
                {
                    m_remoteCertificate = value;
                }
            }

            /// <summary>
            /// Gets or sets the set of valid policy errors that can occur when establishing secure communications with the publisher.
            /// </summary>
            [ConnectionStringParameter,
            DefaultValue(DefaultValidPolicyErrors),
            Description("Defines the set of valid policy errors that can occur when establishing secure communications with the publisher.")]
            public SslPolicyErrors ValidPolicyErrors
            {
                get
                {
                    return m_validPolicyErrors;
                }
                set
                {
                    m_validPolicyErrors = value;
                }
            }

            /// <summary>
            /// Gets or sets the set of valid chain flags that can occur when establishing secure communications with the publisher.
            /// </summary>
            [ConnectionStringParameter,
            DefaultValue(DefaultValidChainFlags),
            Description("Defines the set of valid chain flags that can occur when establishing secure communications with the publisher.")]
            public X509ChainStatusFlags ValidChainFlags
            {
                get
                {
                    return m_validChainFlags;
                }
                set
                {
                    m_validChainFlags = value;
                }
            }

            /// <summary>
            /// Gets or sets the flag that determines whether to check if the publisher's certificate has been revoked when validating the publisher's identity.
            /// </summary>
            [ConnectionStringParameter,
            DefaultValue(DefaultCheckCertificateRevocation),
            Description("Defines the flag that determines whether to check if the publisher's certificate has been revoked when validating the publisher's identity.")]
            public bool CheckCertificateRevocation
            {
                get
                {
                    return m_checkCertificateRevocation;
                }
                set
                {
                    m_checkCertificateRevocation = value;
                }
            }

            #endregion
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="RequireAuthentication"/> property.
        /// </summary>
        public const bool DefaultRequireAuthentication = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SecurityMode"/> property.
        /// </summary>
        public const SecurityMode DefaultSecurityMode = SecurityMode.None;

        /// <summary>
        /// Specifies the default value for the <see cref="Internal"/> property.
        /// </summary>
        public const bool DefaultInternal = false;

        /// <summary>
        /// Specifies the default value for the <see cref="OperationalModes"/> property.
        /// </summary>
        public const OperationalModes DefaultOperationalModes = OperationalModes.CompressMetadata | OperationalModes.CompressSignalIndexCache | OperationalModes.CompressPayloadData | OperationalModes.ReceiveInternalMetadata | OperationalModes.UseCommonSerializationFormat;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveInternalMetadata"/> property.
        /// </summary>
        public const bool DefaultReceiveInternalMetadata = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveExternalMetadata"/> property.
        /// </summary>
        public const bool DefaultReceiveExternalMetadata = false;

        /// <summary>
        /// Specifies the default value for the <see cref="MetadataSynchronizationTimeout"/> property.
        /// </summary>
        public const int DefaultMetadataSynchronizationTimeout = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="UseTransactionForMetadata"/> property.
        /// </summary>
        public const bool DefaultUseTransactionForMetadata = true;

        /// <summary>
        /// Specifies the default value for the <see cref="UseMillisecondResolution"/> property.
        /// </summary>
        public const bool DefaultUseMillisecondResolution = false;

        /// <summary>
        /// Specifies the default value for the <see cref="MetadataFilters"/> property.
        /// </summary>
        public const string DefaultMetadataFilters = null;

        /// <summary>
        /// Specifies the default value for the <see cref="SynchronizeMetadata"/> property.
        /// </summary>
        public const bool DefaultSynchronizeMetadata = false;

        /// <summary>
        /// Specifies the default value for the <see cref="DataLossInterval"/> property.
        /// </summary>
        public const double DefaultDataLossInterval = 10.0D;

        /// <summary>
        /// Specifies the default value for the <see cref="BufferSize"/> property.
        /// </summary>
        public const int DefaultBufferSize = ClientBase.DefaultReceiveBufferSize;

        /// <summary>
        /// Specifies the default value for the <see cref="CommandChannel"/> property.
        /// </summary>
        public const string DefaultCommandChannel = null;

        // Fields
        private bool m_requireAuthentication;
        private SecurityMode m_securityMode;
        private bool m_internal;
        private OperationalModes m_operationalModes;
        private bool m_receiveInternalMetadata;
        private bool m_receiveExternalMetadata;
        private int m_metadataSynchronizationTimeout;
        private bool m_useTransactionForMetadata;
        private bool m_useMillisecondResolution;
        private string m_metadataFilters;
        private bool m_synchronizeMetadata;
        private double m_dataLossInterval;
        private int m_bufferSize;
        private string m_commandChannel;
        private GatewaySecurity m_gatewaySecuritySettings;
        private TlsSecurity m_tlsSecuritySettings;

        #endregion

        #region [ Constructors ]

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if <see cref="DataPublisher"/> requires subscribers to authenticate before making data requests.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never),
        ConnectionStringParameter,
        DefaultValue(DefaultRequireAuthentication),
        Description("Defines the flag that determines if the publisher requires subscribers to authenticate before making data requests.")]
        public bool RequireAuthentication
        {
            get
            {
                return m_requireAuthentication;
            }
            set
            {
                m_requireAuthentication = value;
            }
        }

        /// <summary>
        /// Gets or sets the security mode used for communications over the command channel.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultSecurityMode),
        Description("Defines the security mode used for communications over the command channel.")]
        public SecurityMode SecurityMode
        {
            get
            {
                return m_securityMode;
            }
            set
            {
                m_securityMode = value;

                switch (value)
                {
                    case SecurityMode.Gateway:
                        m_gatewaySecuritySettings = new GatewaySecurity();
                        m_tlsSecuritySettings = null;
                        break;

                    case SecurityMode.TLS:
                        m_gatewaySecuritySettings = null;
                        m_tlsSecuritySettings = new TlsSecurity();
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether measurement metadata that is synchronized by the subscriber should be marked as internal.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultInternal),
        Description("Defines the flag that determines whether measurement metadata that is synchronized by the subscriber should be marked as internal.")]
        public bool Internal
        {
            get
            {
                return m_internal;
            }
            set
            {
                m_internal = value;
            }
        }

        /// <summary>
        /// Gets or sets a set of flags that define ways in
        /// which the subscriber and publisher communicate.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultOperationalModes),
        Description("Defines a set of flags that define ways in which the subscriber and publisher communicate.")]
        public OperationalModes OperationalModes
        {
            get
            {
                return m_operationalModes;
            }
            set
            {
                m_operationalModes = value;
            }
        }

        /// <summary>
        /// Gets or sets the operational mode flag to receive internal meta-data.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultReceiveInternalMetadata),
        Description("Defines the operational mode flag to receive internal meta-data.")]
        public bool ReceiveInternalMetadata
        {
            get
            {
                return m_receiveInternalMetadata;
            }
            set
            {
                m_receiveInternalMetadata = value;
            }
        }

        /// <summary>
        /// Gets or sets the operational mode flag to receive external meta-data.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultReceiveExternalMetadata),
        Description("Defines the operational mode flag to receive external meta-data.")]
        public bool ReceiveExternalMetadata
        {
            get
            {
                return m_receiveExternalMetadata;
            }
            set
            {
                m_receiveExternalMetadata = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout used when executing database queries during meta-data synchronization.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultMetadataSynchronizationTimeout),
        Description("Defines the timeout used when executing database queries during meta-data synchronization.")]
        public int MetadataSynchronizationTimeout
        {
            get
            {
                return m_metadataSynchronizationTimeout;
            }
            set
            {
                m_metadataSynchronizationTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if meta-data synchronization should be performed within a transaction.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultUseTransactionForMetadata),
        Description("Defines the flag that determines if meta-data synchronization should be performed within a transaction.")]
        public bool UseTransactionForMetadata
        {
            get
            {
                return m_useTransactionForMetadata;
            }
            set
            {
                m_useTransactionForMetadata = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that informs publisher if base time-offsets can use millisecond resolution to conserve bandwidth.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultUseMillisecondResolution),
        Description("Defines the flag that informs publisher if base time-offsets can use millisecond resolution to conserve bandwidth.")]
        public bool UseMillisecondResolution
        {
            get
            {
                return m_useMillisecondResolution;
            }
            set
            {
                m_useMillisecondResolution = value;
            }
        }

        /// <summary>
        /// Gets or sets requested meta-data filter expressions to be applied by <see cref="DataPublisher"/> before meta-data is sent.
        /// </summary>
        /// <remarks>
        /// Multiple meta-data filters, such filters for different data tables, should be separated by a semicolon. Specifying fields in the filter
        /// expression that do not exist in the data publisher's current meta-data set could cause filter expressions to not be applied and possibly
        /// result in no meta-data being received for the specified data table.
        /// </remarks>
        /// <example>
        /// FILTER MeasurementDetail WHERE SignalType &lt;&gt; 'STAT'; FILTER PhasorDetail WHERE Phase = '+'
        /// </example>
        [ConnectionStringParameter,
        DefaultValue(DefaultMetadataFilters),
        Description("Defines the requested meta-data filter expressions to be applied by the publisher before meta-data is sent.")]
        public string MetadataFilters
        {
            get
            {
                return m_metadataFilters;
            }
            set
            {
                m_metadataFilters = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="DataSubscriber"/> should
        /// automatically request meta-data synchronization and synchronize publisher
        /// meta-data with its own database configuration.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultSynchronizeMetadata),
        Description("Defines the flag that determines if the subscriber should automatically request meta-data synchronization and synchronize publisher meta-data with its own database configuration.")]
        public bool SynchronizeMetadata
        {
            get
            {
                return m_synchronizeMetadata;
            }
            set
            {
                m_synchronizeMetadata = value;
            }
        }

        /// <summary>
        /// Gets or sets data loss monitoring interval, in seconds. Set to zero to disable monitoring.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultDataLossInterval),
        Description("Defines the data loss monitoring interval, in seconds. Set to zero to disable monitoring.")]
        public double DataLossInterval
        {
            get
            {
                return m_dataLossInterval;
            }
            set
            {
                m_dataLossInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffers used to send and receive data in the transport layer.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultBufferSize),
        Description("Defines the size of the buffers used to send and receive data in the transport layer.")]
        public int BufferSize
        {
            get
            {
                return m_bufferSize;
            }
            set
            {
                m_bufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the connection string which defines connection parameters to connect to the publisher's command channel.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultCommandChannel),
        Description("Defines the connection parameters to connect to the publisher's command channel. Use this only when also defining a separate data channel.")]
        public string CommandChannel
        {
            get
            {
                return m_commandChannel;
            }
            set
            {
                m_commandChannel = value;
            }
        }

        /// <summary>
        /// Gets connection string settings specific to the gateway <see cref="SecurityMode"/>.
        /// </summary>
        [NestedConnectionStringParameter]
        public GatewaySecurity GatewaySecuritySettings
        {
            get
            {
                return m_gatewaySecuritySettings;
            }
        }

        /// <summary>
        /// Gets connection string settings specific to the TLS <see cref="SecurityMode"/>.
        /// </summary>
        [NestedConnectionStringParameter]
        public TlsSecurity TlsSecuritySettings
        {
            get
            {
                return m_tlsSecuritySettings;
            }
        }

        #endregion
    }
}
