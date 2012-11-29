//*******************************************************************************************************
//  TlsServer.cs - Gbtc
//
//  Tennessee Valley Authority, 2012
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to GSF under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/12/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The GSF Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

//******************************************************************************************************
//
// Copyright © 2011, Board of Directors of the University of Illinois. All rights reserved.
// Developed by:
// Information Trust Institute
// University of Illinois
// www.iti.illinois.edu
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
// Neither the names of Information Trust Institute, University of Illinois, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
//
//******************************************************************************************************

#endregion

using GSF.Configuration;
using GSF.IO;
using GSF.Net.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a TCP-based communication server with SSL authentication and encryption.
    /// </summary>
    public class TlsServer : ServerBase
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents a socket that has been wrapped
        /// in an <see cref="SslStream"/> for encryption.
        /// </summary>
        public sealed class TlsSocket : IDisposable
        {
            /// <summary>
            /// Gets the <see cref="Socket"/> connected to the remote host.
            /// </summary>
            public Socket Socket;

            /// <summary>
            /// Gets the stream through which data is passed when
            /// sending to or receiving from the remote host.
            /// </summary>
            public SslStream SslStream;

            /// <summary>
            /// Performs application-defined tasks associated with
            /// freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if ((object)SslStream != null)
                    SslStream.Dispose();
            }
        }

        private class TlsClientInfo
        {
            public TransportProvider<TlsSocket> Client;
            public SpinLock SendLock;
            public ConcurrentQueue<TlsServerPayload> SendQueue;
            public int Sending;
        }

        private class TlsServerPayload
        {
            // Per payload state
            public byte[] Data;
            public int Offset;
            public int Length;
            public ManualResetEventSlim WaitHandle;

            // Per client state
            public TlsClientInfo ClientInfo;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="TrustedCertificatesPath"/> property.
        /// </summary>
        public readonly string DefaultTrustedCertificatesPath = FilePath.GetAbsolutePath("Trusted Certificates");

        /// <summary>
        /// Specifies the default value for the <see cref="PayloadAware"/> property.
        /// </summary>
        public const bool DefaultPayloadAware = false;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowDualStackSocket"/> property.
        /// </summary>
        public const bool DefaultAllowDualStackSocket = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxSendQueueSize"/> property.
        /// </summary>
        public const int DefaultMaxSendQueueSize = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ServerBase.ConfigurationString"/> property.
        /// </summary>
        public const string DefaultConfigurationString = "Port=8888";

        // Fields
        private SimpleCertificateChecker m_defaultCertificateChecker;
        private ICertificateChecker m_certificateChecker;
        private RemoteCertificateValidationCallback m_remoteCertificateValidationCallback;
        private LocalCertificateSelectionCallback m_localCertificateSelectionCallback;
        private string m_trustedCertificatesPath;
        private string m_certificateFile;
        private X509Certificate m_certificate;
        private SslProtocols m_enabledSslProtocols;
        private bool m_requireClientCertificate;
        private bool m_checkCertificateRevocation;

        private bool m_payloadAware;
        private byte[] m_payloadMarker;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_maxSendQueueSize;
        private Socket m_tlsServer;
        private SocketAsyncEventArgs m_acceptArgs;
        private ConcurrentDictionary<Guid, TlsClientInfo> m_clientInfoLookup;
        private Dictionary<string, string> m_configData;

        private EventHandler<SocketAsyncEventArgs> m_acceptHandler;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        public TlsServer()
            : this(DefaultConfigurationString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="configString">Config string of the <see cref="TcpServer"/>. See <see cref="DefaultConfigurationString"/> for format.</param>
        public TlsServer(string configString)
            : base(TransportProtocol.Tcp, configString)
        {
            m_defaultCertificateChecker = new SimpleCertificateChecker();
            m_localCertificateSelectionCallback = DefaultLocalCertificateSelectionCallback;
            m_enabledSslProtocols = SslProtocols.Default;
            m_checkCertificateRevocation = true;

            m_trustedCertificatesPath = DefaultTrustedCertificatesPath;
            m_payloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_allowDualStackSocket = DefaultAllowDualStackSocket;
            m_maxSendQueueSize = DefaultMaxSendQueueSize;
            m_clientInfoLookup = new ConcurrentDictionary<Guid, TlsClientInfo>();

            m_acceptHandler = (sender, args) => ProcessAccept();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="TcpServer"/>.</param>
        public TlsServer(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the payload boundaries are to be preserved during transmission.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultPayloadAware),
        Description("Indicates whether the payload boundaries are to be preserved during transmission.")]
        public bool PayloadAware
        {
            get
            {
                return m_payloadAware;
            }
            set
            {
                m_payloadAware = value;
            }
        }

        /// <summary>
        /// Gets or sets the byte sequence used to mark the beginning of a payload in a <see cref="PayloadAware"/> transmission.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty buffer.</exception>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] PayloadMarker
        {
            get
            {
                return m_payloadMarker;
            }
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException("value");

                m_payloadMarker = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if dual-mode socket is allowed when endpoint address is IPv6.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultAllowDualStackSocket),
        Description("Determines if dual-mode socket is allowed when endpoint address is IPv6.")]
        public bool AllowDualStackSocket
        {
            get
            {
                return m_allowDualStackSocket;
            }
            set
            {
                m_allowDualStackSocket = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size for the send queue before payloads are dumped from the queue.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultMaxSendQueueSize),
        Description("The maximum size for the send queue before payloads are dumped from the queue.")]
        public int MaxSendQueueSize
        {
            get
            {
                return m_maxSendQueueSize;
            }
            set
            {
                m_maxSendQueueSize = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="TcpServer"/>.
        /// </summary>
        [Browsable(false)]
        public Socket Server
        {
            get
            {
                return m_tlsServer;
            }
        }

        /// <summary>
        /// Gets or sets the certificate checker used to validate remote certificates.
        /// </summary>
        /// <remarks>
        /// The certificate checker will only be used to validate certificates if
        /// the <see cref="RemoteCertificateValidationCallback"/> is set to null.
        /// </remarks>
        public ICertificateChecker CertificateChecker
        {
            get
            {
                return m_certificateChecker ?? m_defaultCertificateChecker;
            }
            set
            {
                m_certificateChecker = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback used to validate remote certificates.
        /// </summary>
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback
        {
            get
            {
                return m_remoteCertificateValidationCallback;
            }
            set
            {
                m_remoteCertificateValidationCallback = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback used to select local certificates.
        /// </summary>
        public LocalCertificateSelectionCallback LocalCertificateSelectionCallback
        {
            get
            {
                return m_localCertificateSelectionCallback;
            }
            set
            {
                m_localCertificateSelectionCallback = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the certificate used for authentication.
        /// </summary>
        public string CertificateFile
        {
            get
            {
                return m_certificateFile;
            }
            set
            {
                m_certificateFile = value;

                if (File.Exists(value))
                    Certificate = new X509Certificate2(value);
            }
        }

        /// <summary>
        /// Gets or sets the certificate used to identify this server.
        /// </summary>
        public X509Certificate Certificate
        {
            get
            {
                return m_certificate;
            }
            set
            {
                m_certificate = value;
            }
        }

        /// <summary>
        /// Gets or sets a set of flags which determine the enabled <see cref="SslProtocols"/>.
        /// </summary>
        public SslProtocols EnabledSslProtocols
        {
            get
            {
                return m_enabledSslProtocols;
            }
            set
            {
                m_enabledSslProtocols = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether a client certificate is required during authentication.
        /// </summary>
        public bool RequireClientCertificate
        {
            get
            {
                return m_requireClientCertificate;
            }
            set
            {
                m_requireClientCertificate = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that determines whether the certificate revocation list is checked during authentication.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the path to the directory containing the trusted certificates.
        /// </summary>
        public string TrustedCertificatesPath
        {
            get
            {
                return m_trustedCertificatesPath;
            }
            set
            {
                m_trustedCertificatesPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the set of valid policy errors when validating remote certificates.
        /// </summary>
        public SslPolicyErrors ValidPolicyErrors
        {
            get
            {
                return m_defaultCertificateChecker.ValidPolicyErrors;
            }
            set
            {
                m_defaultCertificateChecker.ValidPolicyErrors = value;
            }
        }

        /// <summary>
        /// Gets or sets the set of valid chain flags used when validating remote certificates.
        /// </summary>
        public X509ChainStatusFlags ValidChainFlags
        {
            get
            {
                return m_defaultCertificateChecker.ValidChainFlags;
            }
            set
            {
                m_defaultCertificateChecker.ValidChainFlags = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="clientID">ID of the client from which data buffer should be read.</param>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ServerBase.ReceiveClientData"/> event handler. Calling this method
        /// outside this event will have unexpected results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// No received data buffer has been defined to read -or-
        /// Specified <paramref name="clientID"/> does not exist, cannot read buffer.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(Guid clientID, byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            TlsClientInfo clientInfo;
            TransportProvider<TlsSocket> tlsClient;

            if (m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
            {
                tlsClient = clientInfo.Client;

                if ((object)tlsClient.ReceiveBuffer != null)
                {
                    int readIndex = ReadIndicies[clientID];
                    int sourceLength = tlsClient.BytesReceived - readIndex;
                    int readBytes = length > sourceLength ? sourceLength : length;
                    Buffer.BlockCopy(tlsClient.ReceiveBuffer, readIndex, buffer, startIndex, readBytes);

                    // Update read index for next call
                    readIndex += readBytes;

                    if (readIndex >= tlsClient.BytesReceived)
                        readIndex = 0;

                    ReadIndicies[clientID] = readIndex;

                    return readBytes;
                }

                throw new InvalidOperationException("No received data buffer has been defined to read.");
            }

            throw new InvalidOperationException("Specified client ID does not exist, cannot read buffer.");
        }

        /// <summary>
        /// Saves <see cref="TcpServer"/> settings to the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();

            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["EnabledSslProtocols", true].Update(m_enabledSslProtocols);
                settings["RequireClientCertificate", true].Update(m_requireClientCertificate);
                settings["CheckCertificateRevocation", true].Update(m_checkCertificateRevocation);
                settings["CertificateFile", true].Update(m_certificateFile);
                settings["TrustedCertificatesPath", true].Update(m_trustedCertificatesPath);
                settings["ValidPolicyErrors", true].Update(ValidPolicyErrors);
                settings["ValidChainFlags", true].Update(ValidChainFlags);
                settings["PayloadAware", true].Update(m_payloadAware);
                settings["AllowDualStackSocket", true].Update(m_allowDualStackSocket);
                settings["MaxSendQueueSize", true].Update(m_maxSendQueueSize);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="TcpServer"/> settings from the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();

            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("EnabledSslProtocols", m_enabledSslProtocols, "The set of SSL protocols that are enabled for this server.");
                settings.Add("RequireClientCertificate", m_requireClientCertificate, "True if the client certificate is required during authentication, otherwise False.");
                settings.Add("CheckCertificateRevocation", m_checkCertificateRevocation, "True if the certificate revocation list is to be checked during authentication, otherwise False.");
                settings.Add("CertificateFile", m_certificateFile, "Path to the local certificate used by this server for authentication.");
                settings.Add("TrustedCertificatesPath", m_trustedCertificatesPath, "Path to the directory containing the trusted remote certificates.");
                settings.Add("ValidPolicyErrors", ValidPolicyErrors, "Set of valid policy errors when validating remote certificates.");
                settings.Add("ValidChainFlags", ValidChainFlags, "Set of valid chain flags used when validating remote certificates.");
                settings.Add("PayloadAware", m_payloadAware, "True if payload boundaries are to be preserved during transmission, otherwise False.");
                settings.Add("AllowDualStackSocket", m_allowDualStackSocket, "True if dual-mode socket is allowed when IP address is IPv6, otherwise False.");
                settings.Add("MaxSendQueueSize", m_maxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");
                EnabledSslProtocols = settings["EnabledSslProtocols"].ValueAs(m_enabledSslProtocols);
                RequireClientCertificate = settings["RequireClientCertificate"].ValueAs(m_requireClientCertificate);
                CheckCertificateRevocation = settings["CheckCertificateRevocation"].ValueAs(m_checkCertificateRevocation);
                CertificateFile = settings["CertificateFile"].ValueAs(m_certificateFile);
                TrustedCertificatesPath = settings["TrustedCertificatesPath"].ValueAs(m_trustedCertificatesPath);
                ValidPolicyErrors = settings["ValidPolicyErrors"].ValueAs(ValidPolicyErrors);
                ValidChainFlags = settings["ValidChainFlags"].ValueAs(ValidChainFlags);
                PayloadAware = settings["PayloadAware"].ValueAs(m_payloadAware);
                AllowDualStackSocket = settings["AllowDualStackSocket"].ValueAs(m_allowDualStackSocket);
                MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(m_maxSendQueueSize);
            }
        }

        /// <summary>
        /// Stops the <see cref="TcpServer"/> synchronously and disconnects all connected clients.
        /// </summary>
        public override void Stop()
        {
            SocketAsyncEventArgs acceptArgs = m_acceptArgs;
            m_acceptArgs = null;

            if (CurrentState == ServerState.Running)
            {
                DisconnectAll();        // Disconnection all clients.
                m_tlsServer.Close();    // Stop accepting new connections.

                // Clean up accept args.
                acceptArgs.Dispose();

                OnServerStopped();
            }
        }

        /// <summary>
        /// Starts the <see cref="TcpServer"/> synchronously and begins accepting client connections asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to <see cref="Start()"/> the <see cref="TcpServer"/> when it is running.</exception>
        public override void Start()
        {
            if (CurrentState == ServerState.NotRunning)
            {
                // Initialize if unitialized.
                if (!Initialized)
                    Initialize();

                // Bind server socket to local end-point and listen.
                m_tlsServer = Transport.CreateSocket(m_configData["interface"], int.Parse(m_configData["port"]), ProtocolType.Tcp, m_ipStack, m_allowDualStackSocket);
                m_tlsServer.Listen(1);

                // Begin accepting incoming connection asynchronously.
                m_acceptArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();

                m_acceptArgs.AcceptSocket = null;
                m_acceptArgs.SetBuffer(null, 0, 0);
                m_acceptArgs.SocketFlags = SocketFlags.None;
                m_acceptArgs.Completed += m_acceptHandler;

                if (!m_tlsServer.AcceptAsync(m_acceptArgs))
                    ThreadPool.QueueUserWorkItem(state => ProcessAccept());

                // Notify that the server has been started successfully.
                OnServerStarted();
            }
            else
            {
                throw new InvalidOperationException("Server is currently running");
            }
        }

        /// <summary>
        /// Disconnects the specified connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public override void DisconnectOne(Guid clientID)
        {
            TransportProvider<TlsSocket> tlsClient;

            if (!TryGetClient(clientID, out tlsClient))
                return;

            try
            {
                if ((object)tlsClient.Provider != null && tlsClient.Provider.Socket.Connected)
                    tlsClient.Provider.Socket.Disconnect(false);

                OnClientDisconnected(clientID);
                tlsClient.Reset();
            }
            catch (Exception ex)
            {
                OnSendClientDataException(clientID, new InvalidOperationException(string.Format("Client disconnection exception: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{TlsSocket}"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <param name="tlsClient">The TLS client.</param>
        /// <returns>An <see cref="TransportProvider{TlsSocket}"/> object.</returns>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public bool TryGetClient(Guid clientID, out TransportProvider<TlsSocket> tlsClient)
        {
            TlsClientInfo clientInfo;
            bool clientExists = m_clientInfoLookup.TryGetValue(clientID, out clientInfo);

            if (clientExists)
                tlsClient = clientInfo.Client;
            else
                tlsClient = null;

            return clientExists;
        }

        /// <summary>
        /// Validates the specified <paramref name="configurationString"/>.
        /// </summary>
        /// <param name="configurationString">Configuration string to be validated.</param>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Port property value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConfigurationString(string configurationString)
        {
            m_configData = configurationString.ParseKeyValuePairs();

            // Derive desired IP stack based on specified "interface" setting, adding setting if it's not defined
            m_ipStack = Transport.GetInterfaceIPStack(m_configData);

            if (!m_configData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port property is missing (Example: {0})", DefaultConfigurationString));

            if (!Transport.IsPortNumberValid(m_configData["port"]))
                throw new ArgumentOutOfRangeException("configurationString", string.Format("Port number must be between {0} and {1}", Transport.PortRangeLow, Transport.PortRangeHigh));
        }

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataToAsync(Guid clientID, byte[] data, int offset, int length)
        {
            TlsClientInfo clientInfo;
            ConcurrentQueue<TlsServerPayload> sendQueue;
            TlsServerPayload dequeuedPayload;

            TlsServerPayload payload;
            ManualResetEventSlim handle;

            bool lockTaken = false;

            if (!m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
                throw new InvalidOperationException(string.Format("No client found for ID {0}.", clientID));

            sendQueue = clientInfo.SendQueue;

            // Check to see if the client has reached the maximum send queue size.
            if (m_maxSendQueueSize > 0 && sendQueue.Count >= m_maxSendQueueSize)
            {
                for (int i = 0; i < m_maxSendQueueSize; i++)
                {
                    if (sendQueue.TryDequeue(out payload))
                    {
                        payload.WaitHandle.Set();
                        payload.WaitHandle.Dispose();
                        payload.WaitHandle = null;
                    }
                }

                throw new InvalidOperationException(string.Format("Client {0} connected to TCP server reached maximum send queue size. {1} payloads dumped from the queue.", clientID, m_maxSendQueueSize));
            }

            // Prepare for payload-aware transmission.
            if (m_payloadAware)
                Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker);

            // Create payload and wait handle.
            payload = ReusableObjectPool<TlsServerPayload>.Default.TakeObject();
            handle = ReusableObjectPool<ManualResetEventSlim>.Default.TakeObject();

            payload.Data = data;
            payload.Offset = offset;
            payload.Length = length;
            payload.WaitHandle = handle;
            payload.ClientInfo = clientInfo;
            handle.Reset();

            // Queue payload for sending.
            sendQueue.Enqueue(payload);

            try
            {
                clientInfo.SendLock.Enter(ref lockTaken);

                // Send next queued payload.
                if (Interlocked.CompareExchange(ref clientInfo.Sending, 1, 0) == 0)
                {
                    if (sendQueue.TryDequeue(out dequeuedPayload))
                        ThreadPool.QueueUserWorkItem(state => SendPayload((TlsServerPayload)state), dequeuedPayload);
                    else
                        Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
            finally
            {
                if (lockTaken)
                    clientInfo.SendLock.Exit();
            }

            // Notify that the send operation has started.
            OnSendClientDataStart(clientID);

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle.WaitHandle;
        }

        /// <summary>
        /// Callback method for asynchronous accept operation.
        /// </summary>
        private void ProcessAccept()
        {
            TransportProvider<TlsSocket> client = new TransportProvider<TlsSocket>();
            IPEndPoint remoteEndPoint = null;
            NetworkStream netStream;

            try
            {
                if (CurrentState == ServerState.NotRunning)
                    return;

                if (m_acceptArgs.SocketError != SocketError.Success)
                {
                    // Error is unrecoverable.
                    // We need to make sure to restart the
                    // server before we throw the error.
                    SocketError error = m_acceptArgs.SocketError;
                    ThreadPool.QueueUserWorkItem(state => ReStart());
                    throw new SocketException((int)error);
                }

                // Process the newly connected client.
                LoadTrustedCertificates();
                remoteEndPoint = m_acceptArgs.AcceptSocket.RemoteEndPoint as IPEndPoint;
                netStream = new NetworkStream(m_acceptArgs.AcceptSocket);

                client.Provider = new TlsSocket()
                {
                    Socket = m_acceptArgs.AcceptSocket,
                    SslStream = new SslStream(netStream, false, m_remoteCertificateValidationCallback ?? CertificateChecker.ValidateRemoteCertificate, m_localCertificateSelectionCallback)
                };

                client.Provider.SslStream.BeginAuthenticateAsServer(m_certificate, m_requireClientCertificate, m_enabledSslProtocols, m_checkCertificateRevocation, ProcessAuthenticate, client);

                // Return to accepting new connections.
                m_acceptArgs.AcceptSocket = null;

                if (!m_tlsServer.AcceptAsync(m_acceptArgs))
                {
                    ThreadPool.QueueUserWorkItem(state => ProcessAccept());
                }
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                if ((object)remoteEndPoint != null)
                {
                    string clientAddress = remoteEndPoint.Address.ToString();
                    string errorMessage = string.Format("Unable to accept connection to client [{0}]: {1}", clientAddress, ex.Message);
                    OnClientConnectingException(new Exception(errorMessage, ex));
                }

                TerminateConnection(client, false);
            }
        }

        /// <summary>
        /// Callback method for asynchronous authenticate operation.
        /// </summary>
        private void ProcessAuthenticate(IAsyncResult asyncResult)
        {
            TransportProvider<TlsSocket> client = (TransportProvider<TlsSocket>)asyncResult.AsyncState;
            IPEndPoint remoteEndPoint = client.Provider.Socket.RemoteEndPoint as IPEndPoint;
            SslStream stream = client.Provider.SslStream;

            try
            {
                stream.EndAuthenticateAsServer(asyncResult);

                if (EnabledSslProtocols != SslProtocols.None)
                {
                    if (!stream.IsAuthenticated)
                        throw new InvalidOperationException("Unable to authenticate.");

                    if (!stream.IsEncrypted)
                        throw new InvalidOperationException("Unable to encrypt data stream.");
                }

                if (MaxClientConnections != -1 && ClientIDs.Length >= MaxClientConnections)
                {
                    // Reject client connection since limit has been reached.
                    TerminateConnection(client, false);
                }
                else
                {
                    // We can proceed further with receiving data from the client.
                    m_clientInfoLookup.TryAdd(client.ID, new TlsClientInfo()
                    {
                        Client = client,
                        SendLock = new SpinLock(),
                        SendQueue = new ConcurrentQueue<TlsServerPayload>()
                    });

                    OnClientConnected(client.ID);
                    ReceivePayloadAsync(client);
                }
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                string clientAddress = remoteEndPoint.Address.ToString();
                string errorMessage = string.Format("Unable to authenticate connection to client [{0}]: {1}", clientAddress, CertificateChecker.ReasonForFailure ?? ex.Message);
                OnClientConnectingException(new Exception(errorMessage, ex));
                TerminateConnection(client, false);
            }
        }

        /// <summary>
        /// Asynchronous loop sends payloads on the socket.
        /// </summary>
        private void SendPayload(TlsServerPayload payload)
        {
            TlsClientInfo clientInfo = null;
            TransportProvider<TlsSocket> client = null;
            ManualResetEventSlim handle;
            byte[] data;
            int offset;
            int length;

            try
            {
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                handle = payload.WaitHandle;
                data = payload.Data;
                offset = payload.Offset;
                length = payload.Length;

                // Send payload to the client asynchronously.
                client.Provider.SslStream.BeginWrite(data, offset, length, ProcessSend, payload);
            }
            catch (Exception ex)
            {
                if ((object)client != null)
                    OnSendClientDataException(client.ID, ex);

                if ((object)clientInfo != null)
                {
                    // Assume process send was not able
                    // to continue the asynchronous loop.
                    Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void ProcessSend(IAsyncResult asyncResult)
        {
            TlsServerPayload payload = null;
            TlsClientInfo clientInfo = null;
            TransportProvider<TlsSocket> client = null;
            ConcurrentQueue<TlsServerPayload> sendQueue = null;
            ManualResetEventSlim handle = null;
            bool lockTaken = false;

            try
            {
                payload = (TlsServerPayload)asyncResult.AsyncState;
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                sendQueue = clientInfo.SendQueue;
                handle = payload.WaitHandle;

                // Send operation is complete.
                client.Provider.SslStream.EndWrite(asyncResult);
                client.Statistics.UpdateBytesSent(payload.Length);
                OnSendClientDataComplete(client.ID);
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                if ((object)client != null)
                    OnSendClientDataException(client.ID, ex);
            }
            finally
            {
                try
                {
                    payload.WaitHandle = null;
                    payload.ClientInfo = null;

                    // Return payload and wait handle to their respective object pools.
                    ReusableObjectPool<TlsServerPayload>.Default.ReturnObject(payload);
                    ReusableObjectPool<ManualResetEventSlim>.Default.ReturnObject(handle);

                    // Begin sending next client payload.
                    if (sendQueue.TryDequeue(out payload))
                    {
                        ThreadPool.QueueUserWorkItem(state => SendPayload((TlsServerPayload)state), payload);
                    }
                    else
                    {
                        try
                        {
                            clientInfo.SendLock.Enter(ref lockTaken);

                            if (sendQueue.TryDequeue(out payload))
                                ThreadPool.QueueUserWorkItem(state => SendPayload((TlsServerPayload)state), payload);
                            else
                                Interlocked.Exchange(ref clientInfo.Sending, 0);
                        }
                        finally
                        {
                            if (lockTaken)
                                clientInfo.SendLock.Exit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Exception encountered while attempting to send next payload: {0}", ex.Message);

                    if ((object)client != null)
                        OnSendClientDataException(client.ID, new Exception(errorMessage, ex));

                    if ((object)clientInfo != null)
                        Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data.
        /// </summary>
        private void ReceivePayloadAsync(TransportProvider<TlsSocket> client)
        {
            // Initialize bytes received.
            client.BytesReceived = 0;

            // Initiate receiving.
            if (m_payloadAware)
            {
                // Payload boundaries are to be preserved.
                client.SetReceiveBuffer(m_payloadMarker.Length + Payload.LengthSegment);
                ReceivePayloadAwareAsync(client, true);
            }
            else
            {
                // Payload boundaries are not to be preserved.
                client.SetReceiveBuffer(ReceiveBufferSize);
                ReceivePayloadUnawareAsync(client);
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsync(TransportProvider<TlsSocket> client, bool waitingForHeader)
        {
            client.Provider.SslStream.BeginRead(client.ReceiveBuffer,
                                                client.BytesReceived,
                                                client.ReceiveBufferSize - client.BytesReceived,
                                                ProcessReceivePayloadAware,
                                                new Tuple<Guid, bool>(client.ID, waitingForHeader));
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ProcessReceivePayloadAware(IAsyncResult asyncResult)
        {
            Tuple<Guid, bool> asyncState = (Tuple<Guid, bool>)asyncResult.AsyncState;
            bool waitingForHeader = asyncState.Item2;
            TransportProvider<TlsSocket> client;

            if (!TryGetClient(asyncState.Item1, out client))
                return;

            try
            {
                // Update statistics and pointers.
                client.Statistics.UpdateBytesReceived(client.Provider.SslStream.EndRead(asyncResult));
                client.BytesReceived += client.Statistics.LastBytesReceived;

                if (!client.Provider.Socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (client.Statistics.LastBytesReceived == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (waitingForHeader)
                {
                    // We're waiting on the payload length, so we'll check if the received data has this information.
                    int payloadLength = Payload.ExtractLength(client.ReceiveBuffer, client.BytesReceived, m_payloadMarker);

                    // We have the payload length.
                    // If it is set to zero, there is no payload; wait for another header.
                    // Otherwise we'll create a buffer that's big enough to hold the entire payload.
                    if (payloadLength == 0)
                    {
                        client.BytesReceived = 0;
                    }
                    else if (payloadLength != -1)
                    {
                        client.BytesReceived = 0;
                        client.SetReceiveBuffer(payloadLength);
                        waitingForHeader = false;
                    }

                    ReceivePayloadAwareAsync(client, waitingForHeader);
                }
                else
                {
                    // We're accumulating the payload in the receive buffer until the entire payload is received.
                    if (client.BytesReceived == client.ReceiveBufferSize)
                    {
                        // We've received the entire payload.
                        OnReceiveClientDataComplete(client.ID, client.ReceiveBuffer, client.BytesReceived);
                        ReceivePayloadAsync(client);
                    }
                    else
                    {
                        // We've not yet received the entire payload.
                        ReceivePayloadAwareAsync(client, false);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when server is disposed.
                TerminateConnection(client, true);
            }
            catch (SocketException ex)
            {
                // Terminate connection when socket exception is encountered.
                OnReceiveClientDataException(client.ID, ex);
                TerminateConnection(client, true);
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveClientDataException(client.ID, ex);
                    ReceivePayloadAsync(client);
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection(client, true);
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ReceivePayloadUnawareAsync(TransportProvider<TlsSocket> client)
        {
            client.Provider.SslStream.BeginRead(client.ReceiveBuffer,
                                                0,
                                                client.ReceiveBufferSize,
                                                ProcessReceivePayloadUnaware,
                                                client);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ProcessReceivePayloadUnaware(IAsyncResult asyncResult)
        {
            TransportProvider<TlsSocket> client = (TransportProvider<TlsSocket>)asyncResult.AsyncState;

            try
            {
                // Update statistics and pointers.
                client.Statistics.UpdateBytesReceived(client.Provider.SslStream.EndRead(asyncResult));
                client.BytesReceived = client.Statistics.LastBytesReceived;

                if (!client.Provider.Socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (client.Statistics.LastBytesReceived == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Notify of received data and resume receive operation.
                OnReceiveClientDataComplete(client.ID, client.ReceiveBuffer, client.BytesReceived);
                ReceivePayloadUnawareAsync(client);
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when server is disposed.
                TerminateConnection(client, true);
            }
            catch (SocketException ex)
            {
                // Terminate connection when socket exception is encountered.
                OnReceiveClientDataException(client.ID, ex);
                TerminateConnection(client, true);
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveClientDataException(client.ID, ex);
                    ReceivePayloadAsync(client);
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection(client, true);
                }
            }
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection(TransportProvider<TlsSocket> client, bool raiseEvent)
        {
            TlsClientInfo clientInfo;

            client.Reset();

            if (raiseEvent)
                OnClientDisconnected(client.ID);

            m_clientInfoLookup.TryRemove(client.ID, out clientInfo);
        }

        /// <summary>
        /// Returns the certificate set by the user.
        /// </summary>
        private X509Certificate DefaultLocalCertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return m_certificate;
        }

        /// <summary>
        /// Loads the list of trusted certificates into the default certificate checker.
        /// </summary>
        private void LoadTrustedCertificates()
        {
            string trustedCertificatesPath;

            if ((object)m_remoteCertificateValidationCallback == null && (object)m_certificateChecker == null)
            {
                m_defaultCertificateChecker.TrustedCertificates.Clear();
                trustedCertificatesPath = FilePath.AddPathSuffix(FilePath.GetAbsolutePath(m_trustedCertificatesPath));

                foreach (string fileName in FilePath.GetFileList(trustedCertificatesPath))
                    m_defaultCertificateChecker.TrustedCertificates.Add(new X509Certificate2(fileName));
            }
        }

        #endregion
    }
}
