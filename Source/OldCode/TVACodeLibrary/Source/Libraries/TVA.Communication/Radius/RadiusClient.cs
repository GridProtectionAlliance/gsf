//*******************************************************************************************************
//  RadiusClient.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/26/2010 - Pinal C. Patel
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
 Original Software Title: The TVA Open Source Phasor Data Concentrator
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
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
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

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using TVA.Parsing;

namespace TVA.Communication.Radius
{
    /// <summary>
    /// Represents a RADIUS communication client.
    /// </summary>
    public class RadiusClient : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default port of the RADIUS server.
        /// </summary>
        public const int DefaultServerPort = 1812;

        /// <summary>
        /// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 1 (ensuring that the user account is in
        /// the "New Pin" mode) of creating a new pin was successful.
        /// </summary>
        public const string DefaultNewPinModeMessage1 = "Enter a new PIN";

        /// <summary>
        /// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 2 (new pin is accepted in attempt #1)
        ///  of creating a new pin was successful.
        /// </summary>
        public const string DefaultNewPinModeMessage2 = "Please re-enter new PIN";

        /// <summary>
        /// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 3 (new pin is accepted in attempts #2)
        /// of creating a new pin was successful.
        /// </summary>
        public const string DefaultNewPinModeMessage3 = "PIN Accepted";

        /// <summary>
        /// Default text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not a user account is in the "Next Token" mode.
        /// </summary>
        public const string DefaultNextTokenModeMessage = "Wait for token to change";

        // Fields
        private short m_requestAttempts;
        private int m_reponseTimeout;
        private string m_sharedSecret;
        private string m_newPinModeMessage1;
        private string m_newPinModeMessage2;
        private string m_newPinModeMessage3;
        private string m_nextTokenModeMessage;
        private bool m_disposed;
        private byte[] m_responseBytes;
        private UdpClient m_udpClient;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of RADIUS client for sending request to a RADIUS server.
        /// </summary>
        /// <param name="serverName">Name or address of the RADIUS server.</param>
        /// <param name="sharedSecret">Shared secret used for encryption and authentication.</param>
        public RadiusClient(string serverName, string sharedSecret)
            : this(serverName, DefaultServerPort, sharedSecret)
        {
        }

        /// <summary>
        /// Creates an instance of RADIUS client for sending request to a RADIUS server.
        /// </summary>
        /// <param name="serverName">Name or address of the RADIUS server.</param>
        /// <param name="serverPort">Port number of the RADIUS server.</param>
        /// <param name="sharedSecret">Shared secret used for encryption and authentication.</param>
        /// <remarks></remarks>
        public RadiusClient(string serverName, int serverPort, string sharedSecret)
        {
            this.SharedSecret = sharedSecret;
            this.RequestAttempts = 1;
            this.ReponseTimeout = 15000;
            this.NewPinModeMessage1 = DefaultNewPinModeMessage1;
            this.NewPinModeMessage2 = DefaultNewPinModeMessage2;
            this.NewPinModeMessage3 = DefaultNewPinModeMessage3;
            this.NextTokenModeMessage = DefaultNextTokenModeMessage;
            m_udpClient = new UdpClient(string.Format("Server={0}; RemotePort={1}; LocalPort=0", serverName, serverPort));
            m_udpClient.ReceiveDataComplete += m_udpClient_ReceivedData;
            m_udpClient.Connect(); // Start the connection cycle.
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name or address of the RADIUS server.
        /// </summary>
        /// <value></value>
        /// <returns>Name or address of RADIUS server.</returns>
        public string ServerName
        {
            get
            {
                return m_udpClient.ConnectionString.ParseKeyValuePairs()["server"];
            }
            set
            {
                CheckDisposed();
                if (!string.IsNullOrEmpty(value))
                {
                    Dictionary<string, string> parts = m_udpClient.ConnectionString.ParseKeyValuePairs();
                    parts["server"] = value;
                    m_udpClient.ConnectionString = parts.JoinKeyValuePairs();
                }
                else
                {
                    throw (new ArgumentNullException("ServerName"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the port number of the RADIUS server.
        /// </summary>
        /// <value></value>
        /// <returns>Port number of the RADIUS server.</returns>
        public int ServerPort
        {
            get
            {
                return System.Convert.ToInt32(m_udpClient.ConnectionString.ParseKeyValuePairs()["remoteport"]);
            }
            set
            {
                CheckDisposed();
                if (value >= 0 && value <= 65535)
                {
                    Dictionary<string, string> parts = m_udpClient.ConnectionString.ParseKeyValuePairs();
                    parts["remoteport"] = value.ToString();
                    m_udpClient.ConnectionString = parts.JoinKeyValuePairs();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("ServerPort", "Value must be between 0 and 65535.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of time a request is to sent to the server until a valid response is received.
        /// </summary>
        /// <value></value>
        /// <returns>Number of time a request is to sent to the server until a valid response is received.</returns>
        public short RequestAttempts
        {
            get
            {
                return m_requestAttempts;
            }
            set
            {
                CheckDisposed();
                if (value >= 1 && value <= 10)
                {
                    m_requestAttempts = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("RequestAttempts", "Value must be between 1 and 10.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the time (in milliseconds) to wait for a response from server after sending a request.
        /// </summary>
        /// <value></value>
        /// <returns>Time (in milliseconds) to wait for a response from server after sending a request.</returns>
        public int ReponseTimeout
        {
            get
            {
                return m_reponseTimeout;
            }
            set
            {
                CheckDisposed();
                if (value >= 1000 && value <= 60000)
                {
                    m_reponseTimeout = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("ResponseTimeout", "Value must be between 1000 and 60000.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the shared secret used between the client and server for encryption and authentication.
        /// </summary>
        /// <value></value>
        /// <returns>Shared secret used between the client and server for encryption and authentication.</returns>
        public string SharedSecret
        {
            get
            {
                return m_sharedSecret;
            }
            set
            {
                CheckDisposed();
                if (!string.IsNullOrEmpty(value))
                {
                    m_sharedSecret = value;
                }
                else
                {
                    throw new ArgumentNullException("SharedSecret");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 1 (ensuring that the user account is in  the
        /// "New Pin" mode) of creating a new pin was successful.
        /// </summary>
        /// <value></value>
        /// <returns>Text for "New Pin" mode's first message.</returns>
        public string NewPinModeMessage1
        {
            get
            {
                return m_newPinModeMessage1;
            }
            set
            {
                CheckDisposed();
                if (!string.IsNullOrEmpty(value))
                {
                    m_newPinModeMessage1 = value;
                }
                else
                {
                    throw new ArgumentNullException("NewPinModeMessage1");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 2 (new pin is accepted in attempt #1) of creating
        /// a new pin was successful.
        /// </summary>
        /// <value></value>
        /// <returns>Text for "New Pin" mode's second message.</returns>
        public string NewPinModeMessage2
        {
            get
            {
                return m_newPinModeMessage2;
            }
            set
            {
                CheckDisposed();
                if (!string.IsNullOrEmpty(value))
                {
                    m_newPinModeMessage2 = value;
                }
                else
                {
                    throw new ArgumentNullException("NewPinModeMessage2");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not Step 3 (new pin is accepted in attempts #2) of creating
        /// a new pin was successful.
        /// </summary>
        /// <value></value>
        /// <returns>Text for "New Pin" mode's third message.</returns>
        public string NewPinModeMessage3
        {
            get
            {
                return m_newPinModeMessage3;
            }
            set
            {
                CheckDisposed();
                if (!string.IsNullOrEmpty(value))
                {
                    m_newPinModeMessage3 = value;
                }
                else
                {
                    throw new ArgumentNullException("NewPinModeMessage3");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text for comparing with the text of ReplyMessage attribute in an AccessChallenge
        /// server response to determine whether or not a user account is in the "Next Token" mode.
        /// </summary>
        /// <value></value>
        /// <returns>Text for "Next Token" mode.</returns>
        public string NextTokenModeMessage
        {
            get
            {
                return m_nextTokenModeMessage;
            }
            set
            {
                CheckDisposed();
                if (!string.IsNullOrEmpty(value))
                {
                    m_nextTokenModeMessage = value;
                }
                else
                {
                    throw new ArgumentNullException("NextTokenModeMessage");
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Send a request to the server and waits for a response back.
        /// </summary>
        /// <param name="request">Request to be sent to the server.</param>
        /// <returns>Response packet if a valid reponse is received from the server; otherwise Nothing.</returns>
        public RadiusPacket ProcessRequest(RadiusPacket request)
        {
            CheckDisposed();
            RadiusPacket response = null;
            // We wait indefinately for the connection to establish. But since this is UDP, the connection
            // will always be successful (locally we're binding to any available UDP port).
            if (m_udpClient.CurrentState == ClientState.Connected)
            {
                // We have a UDP socket we can use for exchanging packets.
                DateTime stopTime;
                for (int i = 1; i <= m_requestAttempts; i++)
                {
                    m_responseBytes = null;
                    m_udpClient.Send(request.BinaryImage());

                    stopTime = DateTime.Now.AddMilliseconds(m_reponseTimeout);
                    while (true)
                    {
                        Thread.Sleep(1);
                        // Stay in the loop until:
                        // 1) We receive a response OR
                        // 2) We exceed the response timeout duration
                        if ((m_responseBytes != null) || DateTime.Now > stopTime)
                        {
                            break;
                        }
                    }

                    if (m_responseBytes != null)
                    {
                        // The server sent a response.
                        response = new RadiusPacket(m_responseBytes, 0, m_responseBytes.Length);
                        if (response.Identifier == request.Identifier && response.Authenticator.CompareTo(RadiusPacket.CreateResponseAuthenticator(m_sharedSecret, request, response)) == 0)
                        {
                            // The response has passed the verification.
                            break;
                        }
                        else
                        {
                            // The response failed the verification, so we'll silently discard it.
                            response = null;
                        }
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Create a new pin for the user.
        /// </summary>
        /// <param name="username">Name of the user.</param>
        /// <param name="token">Current token of the user.</param>
        /// <param name="pin">New pin of the user.</param>
        /// <returns>True if a new pin is created for the user successfully; otherwise False.</returns>
        /// <remarks>NOTE: This method is specific to RSA RADIUS implementation.</remarks>
        public bool CreateNewPin(string username, string token, string pin)
        {
            CheckDisposed();
            if (!string.IsNullOrEmpty(pin))
            {
                byte[] reply;
                RadiusPacket response;

                // Step 1: Send username and token for password, receive a challenge response with reply
                //         message worded "Enter a new PIN". [Verification]
                // Step 2: Send username and new ping for password, receive a challenge response with reply
                //         message worded "Please re-enter.  [Attempt #1]
                // Step 3: Send username and new ping for password, receive a challenge response with reply
                //         message worded "PIN Accepted".    [Attempt #2]

                response = Authenticate(username, token);
                if (IsUserInNewPinMode(response))
                {
                    // User account is really in "New Pin" mode.
                    response = Authenticate(username, pin, response.GetAttributeValue(AttributeType.State));
                    reply = response.GetAttributeValue(AttributeType.ReplyMessage);
                    if (!RadiusPacket.Encoding.GetString(reply, 0, reply.Length).ToLower().Contains(m_newPinModeMessage2.ToLower()))
                    {
                        return false; // New pin not accepted in attempt #1.
                    }

                    response = Authenticate(username, pin, response.GetAttributeValue(AttributeType.State));
                    reply = response.GetAttributeValue(AttributeType.ReplyMessage);
                    if (!RadiusPacket.Encoding.GetString(reply, 0, reply.Length).ToLower().Contains(m_newPinModeMessage3.ToLower()))
                    {
                        return false; // New pin not accepted in attempt #2.
                    }

                    return true; // All is good - new pin is created for the user.
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new ArgumentNullException("pin");
            }
        }

        /// <summary>
        /// Authenticates the username and password against the RADIUS server.
        /// </summary>
        /// <param name="username">Username to be authenticated.</param>
        /// <param name="password">Password to be authenticated.</param>
        /// <returns>Response packet received from the server for the authentication request.</returns>
        /// <remarks>
        /// <para>
        /// The type of response packet (if any) will be one of the following:
        /// <list>
        /// <item>AccessAccept: If the authentication is successful.</item>
        /// <item>AccessReject: If the authentication is not successful.</item>
        /// <item>AccessChallenge: If the server need more information from the user.</item>
        /// </list>
        /// </para>
        /// <para>
        /// When an AccessChallenge response packet is received from the server, it contains a State attribute
        /// that must be included in the AccessRequest packet that is being sent in response to the AccessChallenge
        /// response. So if this method returns an AccessChallenge packet, then this method is to be called again
        /// with the requested information (from ReplyMessage attribute) in the password field and the value State
        /// attribute.
        /// </para>
        /// </remarks>
        public RadiusPacket Authenticate(string username, string password)
        {
            return Authenticate(username, password, null);
        }

        /// <summary>
        /// Authenticates the username and password against the RADIUS server.
        /// </summary>
        /// <param name="username">Username to be authenticated.</param>
        /// <param name="password">Password to be authenticated.</param>
        /// <param name="state">State value from a previous challenge response.</param>
        /// <returns>Response packet received from the server for the authentication request.</returns>
        /// <remarks>
        /// <para>
        /// The type of response packet (if any) will be one of the following:
        /// <list>
        /// <item>AccessAccept: If the authentication is successful.</item>
        /// <item>AccessReject: If the authentication is not successful.</item>
        /// <item>AccessChallenge: If the server need more information from the user.</item>
        /// </list>
        /// </para>
        /// <para>
        /// When an AccessChallenge response packet is received from the server, it contains a State attribute
        /// that must be included in the AccessRequest packet that is being sent in response to the AccessChallenge
        /// response. So if this method returns an AccessChallenge packet, then this method is to be called again
        /// with the requested information (from ReplyMessage attribute) in the password field and the value State
        /// attribute.
        /// </para>
        /// </remarks>
        public RadiusPacket Authenticate(string username, string password, byte[] state)
        {
            CheckDisposed();
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                RadiusPacket request = new RadiusPacket(PacketType.AccessRequest);
                byte[] authenticator = RadiusPacket.CreateRequestAuthenticator(m_sharedSecret);

                request.Authenticator = authenticator;
                request.Attributes.Add(new RadiusPacketAttribute(AttributeType.UserName, username));
                request.Attributes.Add(new RadiusPacketAttribute(AttributeType.UserPassword, RadiusPacket.EncryptPassword(password, m_sharedSecret, authenticator)));
                if (state != null)
                {
                    // State attribute is used when responding to a AccessChallenge reponse.
                    request.Attributes.Add(new RadiusPacketAttribute(AttributeType.State, state));
                }

                return ProcessRequest(request);
            }
            else
            {
                throw new ArgumentException("Username and Password cannot be null.");
            }
        }

        /// <summary>
        /// Determines whether or not the response indicates that the user account is in "New Pin" mode.
        /// </summary>
        /// <param name="response">Response packet sent by the server.</param>
        /// <returns>True if the user account is in "New Pin" mode; otherwise False.</returns>
        /// <remarks>
        /// <para>A user's account can be in the "New Pin" mode when set on the server.</para>
        /// <para>NOTE: This method is specific to RSA RADIUS implementation.</para>
        /// </remarks>
        public bool IsUserInNewPinMode(RadiusPacket response)
        {
            CheckDisposed();

            if (response != null)
            {
                byte[] messageBytes = response.GetAttributeValue(AttributeType.ReplyMessage);
                if (messageBytes != null)
                {
                    // Unfortunately, the only way of determining whether or not a user account is in the
                    // "New Pin" mode is from the text present in the ReplyMessage attribute of the
                    // AccessChallenge response from server.
                    string messageString = RadiusPacket.Encoding.GetString(messageBytes, 0, messageBytes.Length);
                    if (messageString.ToLower().Contains(m_newPinModeMessage1.ToLower()))
                    {
                        return true; // User account is in "New Pin" mode.
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new ArgumentException("ReplyMessage attribute is not present", "response");
                }
            }
            else
            {
                throw new ArgumentNullException("response");
            }
        }

        /// <summary>
        /// Determines whether or not the response indicates that the user account is in "Next Token" mode.
        /// </summary>
        /// <param name="response">Response packet sent by the server.</param>
        /// <returns>True if the user account is in "Next Token" mode; otherwise False.</returns>
        /// <remarks>
        /// <para>
        /// A user's account can enter the "Next Token" mode after the user enters incorrect passwords for a few
        /// times (3 times by default) and then enters the correct password. Note that repeatedly entering
        /// incorrect passwords will disable the user account.
        /// </para>
        /// <para>NOTE: This method is specific to RSA RADIUS implementation.</para>
        /// </remarks>
        public bool IsUserInNextTokenMode(RadiusPacket response)
        {
            CheckDisposed();
            if (response != null)
            {
                byte[] messageBytes = response.GetAttributeValue(AttributeType.ReplyMessage);
                if (messageBytes != null)
                {
                    // Unfortunately, the only way of determining whether or not a user account is in the
                    // "Next Token" mode is from the text present in the ReplyMessage attribute of the
                    // AccessChallenge response from server.
                    string messageString = RadiusPacket.Encoding.GetString(messageBytes, 0, messageBytes.Length);
                    if (messageString.ToLower().Contains(m_nextTokenModeMessage.ToLower()))
                    {
                        return true; // User account is in "Next Token" mode.
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new ArgumentException("ReplyMessage attribute is not present", "response");
                }
            }
            else
            {
                throw new ArgumentNullException("response");
            }
        }

        /// <summary>
        /// Releases the used resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Helper method to check whether or not the object instance has been disposed.
        /// </summary>
        /// <remarks>This method is to be called before performing any operation.</remarks>
        protected void CheckDisposed()
        {
            if (m_disposed)
            {
                throw (new ObjectDisposedException(this.GetType().Name));
            }
        }

        /// <summary>
        /// Releases the used resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    if (m_udpClient != null)
                    {
                        m_udpClient.ReceiveDataComplete -= m_udpClient_ReceivedData;
                        m_udpClient.Dispose();
                    }
                }
            }
            m_disposed = true;
        }

        private void m_udpClient_ReceivedData(object sender, EventArgs<byte[], int> e)
        {
            m_responseBytes = e.Argument1;
        }

        #endregion
    }
}