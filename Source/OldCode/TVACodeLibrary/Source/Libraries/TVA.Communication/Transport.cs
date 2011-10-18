//*******************************************************************************************************
//  Transport.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/01/2006 - Pinal C. Patel
//       Original version of source created.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  08/22/2009 - Pinal C. Patel
//       Modified CreateEndPoint() to try parsing IP address first before doing a DNS lookup.
//  09/08/2009 - Pinal C. Patel
//       Modified CreateSocket() to create a socket for the AddressFamily of the endpoint.
//       Modified CreateEndPoint() to use IPv6 if supported when no IP address is specified.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/30/2009 - Pinal C. Patel
//       Added IsIPv6IP() and IsMulticastIP() methods.
//       Fixed bug in CreateSocket() that was breaking one-way communication support in UDP components.
//  04/29/2010 - Pinal C. Patel
//       Added EndpointFormatRegex constant to be used for parsing endpoint strings.
//  08/18/2011 - J. Ritchie Carroll
//       Multiple additions and updates to accomodate easier IPv6 or IPv4 selection as well as
//       dual-mode socket support.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
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
using System.Net;
using System.Net.Sockets;

namespace TVA.Communication
{
    #region [ Enumerations ]

    /// <summary>
    /// IP stack enumeration.
    /// </summary>
    public enum IPStack
    {
        /// <summary>
        /// IPv6 stack.
        /// </summary>
        /// <remarks>
        /// Requests to use IPv6 stack if possible.
        /// </remarks>
        IPv6,
        /// <summary>
        /// IPv4 stack.
        /// </summary>
        /// <remarks>
        /// Requests to use IPv4 stack if possible.
        /// </remarks>
        IPv4,
        /// <summary>
        /// Default stack.
        /// </summary>
        /// <remarks>
        /// Requests to use the default OS IP stack.
        /// </remarks>
        Default
    }

    #endregion

    /// <summary>
    /// Defines helper methods related to IP socket based communications.
    /// </summary>
    public static class Transport
    {
        /// <summary>
        /// Specifies the lowest valid port number for a <see cref="Socket"/>.
        /// </summary>
        public const int PortRangeLow = 0;

        /// <summary>
        /// Specifies the highest valid port number for a <see cref="Socket"/>.
        /// </summary>
        public const int PortRangeHigh = 65535;

        /// <summary>
        /// Regular expression used to validate the format for an endpoint.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Matches the following valid input:<br/>
        /// - localhost:80<br/>
        /// - 127.0.0.1:80<br/>
        /// - [::1]:80<br/>
        /// - [FEDC:BA98:7654:3210:FEDC:BA98:7654:3210]:80
        /// </para>
        /// </remarks>
        public const string EndpointFormatRegex = @"(?<host>.+)\:(?<port>\d+$)";

        /// <summary>
        /// Creates an <see cref="IPEndPoint"/> for the specified host name and port number.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <param name="port">The port number to be associated with the address.</param>
        /// <param name="stack">Desired IP stack to use.</param>
        /// <returns>An <see cref="IPEndPoint"/> object.</returns>
        public static IPEndPoint CreateEndPoint(string hostNameOrAddress, int port, IPStack stack)
        {
            // Determine system's default IP stack if the default stack was requested
            if (stack == IPStack.Default)
                stack = GetDefaultIPStack();

            // Make sure system can support specified stack
            if (stack == IPStack.IPv6 && !Socket.OSSupportsIPv6)
                throw new NotSupportedException(string.Format("IPv6 stack is not available for socket creation on {0}:{1}", hostNameOrAddress.ToNonNullNorWhiteSpace("localhost"), port));
#if !MONO
            else if (stack == IPStack.IPv4 && !Socket.OSSupportsIPv4)
                throw new NotSupportedException(string.Format("IPv4 stack is not available for socket creation on {0}:{1}", hostNameOrAddress.ToNonNullNorWhiteSpace("localhost"), port));
#endif

            if (string.IsNullOrWhiteSpace(hostNameOrAddress))
            {
                // No host name or IP address was specified, use local IPs
                if (stack == IPStack.IPv6)
                    return new IPEndPoint(IPAddress.IPv6Any, port);

                return new IPEndPoint(IPAddress.Any, port);
            }
            else
            {
                switch (hostNameOrAddress)
                {
                    // Handle IP "0" as a special case since DNS lookup is unavailable for this address
                    case "::0":
                    case "0.0.0.0":
                        return new IPEndPoint(IPAddress.Parse(hostNameOrAddress), port);
                    default:
                        // Host name or IP was provided, attempt lookup - note that exception can occur if DNS lookup fails
                        IPAddress[] addressList = Dns.GetHostEntry(hostNameOrAddress).AddressList;

                        if (addressList.Length > 0)
                        {
                            // Traverse address list looking for first match on desired IP stack
                            foreach (IPAddress address in addressList)
                            {
                                if ((stack == IPStack.IPv6 && address.AddressFamily == AddressFamily.InterNetworkV6) ||
                                    (stack == IPStack.IPv4 && address.AddressFamily == AddressFamily.InterNetwork))
                                    return new IPEndPoint(address, port);
                            }

                            // If no available matching address was found for desired IP stack, default to first address in list
                            return new IPEndPoint(addressList[0], port);
                        }

                        break;
                }

                throw new InvalidOperationException("No valid IP addresses could be found for host named " + hostNameOrAddress);
            }
        }

        /// <summary>
        /// Creates a <see cref="Socket"/> for the specified <paramref name="port"/> and <paramref name="protocol"/>.
        /// </summary>
        /// <param name="address">The local address where the <see cref="Socket"/> will be bound.</param>
        /// <param name="port">The port number at which the <see cref="Socket"/> will be bound.</param>
        /// <param name="protocol">One of the <see cref="ProtocolType"/> values.</param>
        /// <param name="stack">Desired IP stack to use.</param>
        /// <param name="allowDualStackSocket">Determines if dual-mode socket is allowed when endpoint address is IPv6.</param>
        /// <returns>An <see cref="Socket"/> object.</returns>
        public static Socket CreateSocket(string address, int port, ProtocolType protocol, IPStack stack, bool allowDualStackSocket = true)
        {
            Socket socket = null;
            IPEndPoint endpoint = null;

            switch (protocol)
            {
                case ProtocolType.Tcp:
                    endpoint = CreateEndPoint(address, port, stack);
                    socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // If allowDualModeSocket is true and the enpoint is IPv6, we setup a dual-mode socket
                    // by setting the IPv6Only socket option to false
                    if (allowDualStackSocket && endpoint.AddressFamily == AddressFamily.InterNetworkV6 && Environment.OSVersion.Version.Major > 5)
                        socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                    // Associate the socket with the local endpoint
                    socket.Bind(endpoint);

                    break;
                case ProtocolType.Udp:
                    // Allow negative port number to be specified for unbound socket.
                    if (port >= 0)
                    {
                        endpoint = CreateEndPoint(address, port, stack);
                        socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

                        // If allowDualModeSocket is true and the endpoint is IPv6, we setup a dual-mode socket
                        // by setting the IPv6Only socket option to false
                        if (allowDualStackSocket && endpoint.AddressFamily == AddressFamily.InterNetworkV6 && Environment.OSVersion.Version.Major > 5)
                            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                        socket.Bind(endpoint);
                    }
                    else
                    {
                        // Create a socket with no binding when -1 is used for port number
                        endpoint = CreateEndPoint(address, 0, stack);
                        socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    }
                    break;
                default:
                    throw new NotSupportedException("Communications library does not support socket creation for protocol " + protocol);
            }
            return socket;
        }

        /// <summary>
        /// Gets the default IP stack for this system.
        /// </summary>
        /// <returns>
        /// System's assumed default IP stack.
        /// </returns>
        public static IPStack GetDefaultIPStack()
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

                // IP's are normally ordered with default IP stack first
                if (hostEntry.AddressList.Length > 0)
                    return (hostEntry.AddressList[0].AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6 ? IPStack.IPv6 : IPStack.IPv4);
            }
            catch
            {
            }

            // If default stack cannot be determined, assume IPv4
            return IPStack.IPv4;
        }

        /// <summary>
        /// Derives the desired <see cref="IPStack"/> from the "interface" setting in the connection string key/value pairs.
        /// </summary>
        /// <param name="connectionStringKVPairs">Connection string key/value pairs.</param>
        /// <returns>Desired <see cref="IPStack"/> based on "interface" setting.</returns>
        /// <remarks>
        /// The "interface" setting will be added to the <paramref name="connectionStringKVPairs"/> if it
        /// doesn't exist, in this case return value will be <see cref="IPStack.Default"/>.
        /// </remarks>
        public static IPStack GetInterfaceIPStack(Dictionary<string, string> connectionStringKVPairs)
        {
            string ipAddress = null;

            if (connectionStringKVPairs.TryGetValue("interface", out ipAddress))
                return IsIPv6IP(ipAddress) ? IPStack.IPv6 : IPStack.IPv4;

            connectionStringKVPairs.Add("interface", string.Empty);

            return IPStack.Default;
        }

        /// <summary>
        /// Determines if the specified <paramref name="ipAddress"/> is an IPv6 IP.
        /// </summary>
        /// <param name="ipAddress">IP address to check.</param>
        /// <returns>true if the <paramref name="ipAddress"/> is IPv6 IP; otherwise false.</returns>
        public static bool IsIPv6IP(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentNullException("ipAddress");

            IPAddress address;

            if (IPAddress.TryParse(ipAddress, out address))
                return address.AddressFamily == AddressFamily.InterNetworkV6;

            return false;
        }

        /// <summary>
        /// Determines if the specified <paramref name="ipAddress"/> is a multicast IP.
        /// </summary>
        /// <param name="ipAddress">IP address to check.</param>
        /// <returns>true if the <paramref name="ipAddress"/> is multicast IP; otherwise false.</returns>
        public static bool IsMulticastIP(IPAddress ipAddress)
        {
            if (ipAddress == null)
                throw new ArgumentNullException("ipAddress");

            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // IP is IPv6
                return ipAddress.IsIPv6Multicast;
            }
            else
            {
                // IP is IPv4
                int firstOctet = int.Parse(ipAddress.ToString().Split('.')[0]);

                // Check first octet to see if IP is a Class D multicast IP
                if (firstOctet >= 224 && firstOctet <= 247)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified port is valid.
        /// </summary>
        /// <param name="port">The port number to be validated.</param>
        /// <returns>True if the port number is valid.</returns>
        public static bool IsPortNumberValid(string port)
        {
            int portNumber;

            // Check to see if the specified port is a valid integer value
            if (int.TryParse(port, out portNumber))
            {
                // Check to see if the port number is within the valid range
                if (portNumber >= PortRangeLow && portNumber <= PortRangeHigh)
                    return true;

                return false;
            }

            throw new ArgumentException("Port number is not a valid number");
        }

        /// <summary>
        /// Determines if the specified UDP destination is listening for data.
        /// </summary>
        /// <param name="targetIPEndPoint">The <see cref="IPEndPoint"/> for the UDP destination to be checked.</param>
        /// <returns>true if the UDP destination is listening for data; otherwise false.</returns>
        public static bool IsDestinationReachable(IPEndPoint targetIPEndPoint)
        {
            try
            {
                // Check if the target endpoint exists by sending empty data to it and waiting for a response,
                // if the endpoint doesn't exist then we'll receive a ConnectionReset socket exception
                EndPoint targetEndPoint = (EndPoint)targetIPEndPoint;

                using (Socket targetChecker = new Socket(targetIPEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
                {
                    targetChecker.ReceiveTimeout = 1;
                    targetChecker.SendTo(new byte[] { }, targetEndPoint);
                    targetChecker.ReceiveFrom(new byte[] { }, ref targetEndPoint);
                }

            }
            catch (SocketException ex)
            {
                // Connection reset means that the target endpoint is unreachable
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    return false;
            }
            catch
            {
                // We'll ignore any other exceptions we might encounter and assume destination is reachable
            }

            return true;
        }
    }
}