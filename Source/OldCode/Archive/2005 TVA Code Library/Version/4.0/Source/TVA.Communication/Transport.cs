//*******************************************************************************************************
//  Transport.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/01/2006 - Pinal C. Patel
//       Original version of source created.
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Net;
using System.Net.Sockets;

namespace TVA.Communication
{
    /// <summary>
    /// A helper class containing methods related to server-client communication.
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
        /// Creates an <see cref="IPEndPoint"/> for the specified host name and port number.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <param name="port">The port number to be associated with the address.</param>
        /// <returns>An <see cref="IPEndPoint"/> object.</returns>
        public static IPEndPoint CreateEndPoint(string hostNameOrAddress, int port)
        {
            if (string.IsNullOrEmpty(hostNameOrAddress))
            {
                // We use one of the local IP.
                return new IPEndPoint(IPAddress.Any, port);
            }
            else
            {
                try
                {
                    return new IPEndPoint(Dns.GetHostEntry(hostNameOrAddress).AddressList[0], port);
                }
                catch (SocketException)
                {
                    // SocketException will be thrown if the host is not found, so we'll try manual IP
                    return new IPEndPoint(IPAddress.Parse(hostNameOrAddress), port);
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="Socket"/> for the specified <paramref name="port"/> and <paramref name="protocol"/>.
        /// </summary>
        /// <param name="address">The local address where the <see cref="Socket"/> will be bound.</param>
        /// <param name="port">The port number at which the <see cref="Socket"/> will be bound.</param>
        /// <param name="protocol">One of the <see cref="ProtocolType"/> values.</param>
        /// <returns>An <see cref="Socket"/> object.</returns>
        public static Socket CreateSocket(string address, int port, ProtocolType protocol)
        {
            Socket socket = null;
            switch (protocol)
            {
                case ProtocolType.Tcp:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(Transport.CreateEndPoint(address, port));
                    break;
                case ProtocolType.Udp:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.Bind(Transport.CreateEndPoint(address, port));
                    break;
                default:
                    throw new NotSupportedException(string.Format("{0} is not supported.", protocol));
            }
            return socket;
        }

        /// <summary>
        /// Determines whether the specified port is valid.
        /// </summary>
        /// <param name="port">The port number to be validated.</param>
        /// <returns>True if the port number is valid.</returns>
        public static bool IsPortNumberValid(string port)
        {
            int portNumber;

            if (int.TryParse(port, out portNumber))
            {
                // The specified port is a valid integer value.
                if (portNumber >= PortRangeLow && portNumber <= PortRangeHigh)
                    // The port number is within the valid range.
                    return true;
                else
                    return false;
            }
            else
            {
                throw new ArgumentException("Port number is not a valid number.");
            }
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
                // We'll check if the target endpoint exist by sending empty data to it and then wait for data from it.
                // If the endpoint doesn't exist then we'll receive a ConnectionReset socket exception.
                EndPoint targetEndPoint = (EndPoint)targetIPEndPoint;
                using (Socket targetChecker = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    targetChecker.ReceiveTimeout = 1;
                    targetChecker.SendTo(new byte[] { }, targetEndPoint);
                    targetChecker.ReceiveFrom(new byte[] { }, ref targetEndPoint);
                }

            }
            catch (SocketException ex)
            {
                switch (ex.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                        // This means that the target endpoint is unreachable.
                        return false;
                }
            }
            catch
            {
                // We'll ignore any other exceptions we might encounter.
            }

            return true;
        }
    }
}