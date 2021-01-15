//******************************************************************************************************
//  Transport.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//       Fixed issue in CreateSocket() that was breaking one-way communication support in UDP components.
//  04/29/2010 - Pinal C. Patel
//       Added EndpointFormatRegex constant to be used for parsing endpoint strings.
//  08/18/2011 - J. Ritchie Carroll
//       Multiple additions and updates to accomodate easier IPv6 or IPv4 selection as well as
//       dual-mode socket support.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using GSF.Diagnostics;

namespace GSF.Communication
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
                throw new NotSupportedException($"IPv6 stack is not available for socket creation on {hostNameOrAddress.ToNonNullNorWhiteSpace("localhost")}:{port}");
        #if !MONO
            if (stack == IPStack.IPv4 && !Socket.OSSupportsIPv4)
                throw new NotSupportedException($"IPv4 stack is not available for socket creation on {hostNameOrAddress.ToNonNullNorWhiteSpace("localhost")}:{port}");
        #endif

            // No host name or IP address was specified, use local IPs
            if (string.IsNullOrWhiteSpace(hostNameOrAddress))
                return stack == IPStack.IPv6 ? new IPEndPoint(IPAddress.IPv6Any, port) : new IPEndPoint(IPAddress.Any, port);

            bool ipStackMismatch = false;

            // Attempt to parse provided address name as a literal IP address
            if (IPAddress.TryParse(hostNameOrAddress, out IPAddress address))
            {
                // As long as desired IP stack matches format of specified IP address, return end point for address
                if (stack == IPStack.IPv6 && address.AddressFamily == AddressFamily.InterNetworkV6 ||
                    stack == IPStack.IPv4 && address.AddressFamily == AddressFamily.InterNetwork)
                    return new IPEndPoint(address, port);

                // User specified an IP address that is mismatch with the desired IP stack. If the DNS server
                // responds to this IP, we can attempt to see if an IP is defined for the desired IP stack, 
                // otherwise this is an exception
                ipStackMismatch = true;
            }

            try
            {
                // Handle "localhost" as a special case, returning proper loopback address for the desired IP stack
                if (string.Compare(hostNameOrAddress, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
                    return new IPEndPoint(stack == IPStack.IPv6 ? IPAddress.IPv6Loopback : IPAddress.Loopback, port);

                // Failed to parse an IP address for the desired stack - this may simply be that a host name was provided
                // so we attempt a DNS lookup. Note that exceptions will occur if DNS lookup fails.
                IPAddress[] dnsAddressList = Dns.GetHostEntry(hostNameOrAddress).AddressList;

                if (dnsAddressList.Length > 0)
                {
                    // Traverse address list looking for first match on desired IP stack
                    foreach (IPAddress dnsAddress in dnsAddressList)
                    {
                        if (stack == IPStack.IPv6 && dnsAddress.AddressFamily == AddressFamily.InterNetworkV6 ||
                            stack == IPStack.IPv4 && dnsAddress.AddressFamily == AddressFamily.InterNetwork)
                            return new IPEndPoint(dnsAddress, port);
                    }

                    // If no available matching address was found for desired IP stack, this is an IP stack mismatch
                    ipStackMismatch = true;
                }

                throw new InvalidOperationException($"No valid {stack} addresses could be found for \"{hostNameOrAddress}\"");
            }
            catch
            {
                // Spell out a specific error message for IP stack mismatches
                if (ipStackMismatch)
                    throw new InvalidOperationException($"IP address mismatch: unable to find an {stack} address for \"{hostNameOrAddress}\"");

                // Otherwise report original exception
                throw;
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
            Socket socket;
            IPEndPoint endpoint;

            switch (protocol)
            {
                case ProtocolType.Tcp:
                    endpoint = CreateEndPoint(address, port, stack);
                    socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                #if !MONO
                    // If allowDualModeSocket is true and the endpoint is IPv6, we setup a dual-mode socket
                    // by setting the IPv6Only socket option to false
                    if (allowDualStackSocket && endpoint.AddressFamily == AddressFamily.InterNetworkV6 && Environment.OSVersion.Version.Major > 5)
                        socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                #endif
                    // Associate the socket with the local endpoint
                    socket.Bind(endpoint);

                    break;
                case ProtocolType.Udp:
                    // Allow negative port number to be specified for unbound socket.
                    if (port >= 0)
                    {
                        endpoint = CreateEndPoint(address, port, stack);
                        socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    #if !MONO
                        // If allowDualModeSocket is true and the endpoint is IPv6, we setup a dual-mode socket
                        // by setting the IPv6Only socket option to false
                        if (allowDualStackSocket && endpoint.AddressFamily == AddressFamily.InterNetworkV6 && Environment.OSVersion.Version.Major > 5)
                            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                    #endif
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
                    throw new NotSupportedException($"Communications library does not support socket creation for protocol {protocol}");
            }
            return socket;
        }

        /// <summary>
        /// Determines if an IP address (or DNS name) is a local IP address.
        /// </summary>
        /// <param name="hostNameOrAddress">DNS name or IP address to test.</param>
        /// <returns><c>true</c> if <paramref name="hostNameOrAddress"/> is a local IP address; otherwise <c>false</c>.</returns>
        /// <exception cref="SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="hostNameOrAddress"/> is an invalid IP address.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress"/> is greater than 255 characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="hostNameOrAddress"/> is null.</exception>
        public static bool IsLocalAddress(string hostNameOrAddress)
        {
            IPAddress[] hostIPs = Dns.GetHostAddresses(hostNameOrAddress);
            IEnumerable<IPAddress> localIPs = Dns.GetHostAddresses("localhost").Concat(Dns.GetHostAddresses(Dns.GetHostName()));

            // Check to see if entered host name corresponds to a local IP address
            return hostIPs.Any(localIPs.Contains);
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
                    return hostEntry.AddressList[0].AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6 ? IPStack.IPv6 : IPStack.IPv4;
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex, "DNS lookup failure for local machine");
            }

            // If default stack cannot be determined, assume IPv4
            return IPStack.IPv4;
        }

        /// <summary>
        /// Derives the desired <see cref="IPStack"/> from the "interface" setting in the connection string key/value pairs.
        /// If interface is not specified, <see cref="IPStack"/> is derived from the server value in connectionStringEntries
        /// </summary>
        /// <param name="connectionStringEntries">Connection string key/value pairs.</param>
        /// <returns>Desired <see cref="IPStack"/> based on "interface" setting.</returns>
        /// <remarks>
        /// The "interface" setting will be added to the <paramref name="connectionStringEntries"/> if it
        /// doesn't exist. in this case the return value will be based off of the "server" value. If the server parameter 
        /// doesn't exist, the return value will be <see cref="IPStack.Default"/>.
        /// </remarks>
        public static IPStack GetInterfaceIPStack(Dictionary<string, string> connectionStringEntries)
        {
            if (connectionStringEntries.TryGetValue("interface", out string ipAddress))
                return IsIPv6IP(ipAddress) ? IPStack.IPv6 : IPStack.IPv4;

            connectionStringEntries.Add("interface", string.Empty);

            if (connectionStringEntries.TryGetValue("server", out ipAddress))
                return IsIPv6IP(ipAddress) ? IPStack.IPv6 : IPStack.IPv4;

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
                throw new ArgumentNullException(nameof(ipAddress));

            if (IPAddress.TryParse(ipAddress, out IPAddress address))
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
                throw new ArgumentNullException(nameof(ipAddress));

            // Check for IPv6
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                return ipAddress.IsIPv6Multicast;

            // IP is IPv4, check octet for multicast range
            int firstOctet = int.Parse(ipAddress.ToString().Split('.')[0]);

            // Check first octet to see if IP is a Class D multicast IP
            return firstOctet >= 224 && firstOctet <= 247;
        }

        /// <summary>
        /// Determines whether the specified port is valid.
        /// </summary>
        /// <param name="port">The port number to be validated.</param>
        /// <returns>True if the port number is valid.</returns>
        public static bool IsPortNumberValid(string port)
        {
            // Check to see if the specified port is a valid integer value
            if (!int.TryParse(port, out int portNumber))
                throw new ArgumentException("Specified port is not a valid number");

            // Check to see if the port number is within the valid range
            return portNumber >= PortRangeLow && portNumber <= PortRangeHigh;
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
                EndPoint targetEndPoint = targetIPEndPoint;

                using (Socket targetChecker = new Socket(targetIPEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
                {
                    targetChecker.ReceiveTimeout = 1;
                    targetChecker.SendTo(Array.Empty<byte>(), targetEndPoint);
                    targetChecker.ReceiveFrom(Array.Empty<byte>(), ref targetEndPoint);
                }

            }
            catch (SocketException ex)
            {
                // Connection reset means that the target endpoint is unreachable
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    return false;
            }
            catch (Exception ex)
            {
                // We'll ignore any other exceptions we might encounter and assume destination is reachable
                Logger.SwallowException(ex, "Failed while checking if IP end point destination is reachable via UDP");
            }

            return true;
        }
    }
}