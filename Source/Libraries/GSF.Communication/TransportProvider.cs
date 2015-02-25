//******************************************************************************************************
//  TransportProvider.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  11/12/2008 - Pinal C. Patel
//       Generated original version of source code.
//  07/15/2009 - Pinal C. Patel
//       Added error checking to Reset().
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/05/2010 - J. Ritchie Carroll
//       Modified to make use of buffer pool instead of constant dynamic buffer allocations.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.Communication
{
    /// <summary>
    /// A class for managing the communication between server and client.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the object used for server-client communication.</typeparam>
    public class TransportProvider<T>
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// ID of the <see cref="TransportProvider{T}"/> object.
        /// </summary>
        public Guid ID;

        /// <summary>
        /// Provider for the transportation of data.
        /// </summary>
        public T Provider;

        /// <summary>
        /// Number of bytes received in <see cref="ReceiveBuffer"/>.
        /// </summary>
        public int BytesReceived;

        /// <summary>
        /// <see cref="TransportStatistics"/> for the <see cref="TransportProvider{T}"/> object.
        /// </summary>
        public TransportStatistics Statistics;

        /// <summary>
        /// Optional multicast membership addresses used when a multicast source address is specified.
        /// </summary>
        public byte[] MulticastMembershipAddresses;

        // Internally managed I/O buffers
        private byte[] m_sendBuffer;
        private byte[] m_receiveBuffer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportProvider{T}"/> class.
        /// </summary>
        public TransportProvider()
        {
            ID = Guid.NewGuid();
            Statistics = new TransportStatistics();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets buffer used for sending data.
        /// </summary>
        /// <remarks>
        /// Use <see cref="SetSendBuffer"/> to reset and/or establish send buffer size.
        /// </remarks>
        public byte[] SendBuffer
        {
            get
            {
                return m_sendBuffer;
            }
        }

        /// <summary>
        /// Gets send buffer requested size.
        /// </summary>
        public int SendBufferSize
        {
            get
            {
                return (object)m_sendBuffer != null ? m_sendBuffer.Length : 0;
            }
        }

        /// <summary>
        /// Gets buffer used for receiving data.
        /// </summary>
        /// <remarks>
        /// Use <see cref="SetReceiveBuffer"/> to reset and/or establish receive buffer size.
        /// </remarks>
        public byte[] ReceiveBuffer
        {
            get
            {
                return m_receiveBuffer;
            }
        }

        /// <summary>
        /// Gets receive buffer requested size.
        /// </summary>
        public int ReceiveBufferSize
        {
            get
            {
                return (object)m_receiveBuffer != null ? m_receiveBuffer.Length : 0;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Establishes (or reestablishes) a send buffer of a given size.
        /// </summary>
        /// <param name="size">Desired minimum size of send buffer.</param>
        /// <returns>New send buffer.</returns>
        public byte[] SetSendBuffer(int size)
        {
            m_sendBuffer = new byte[size];
            return m_sendBuffer;
        }

        /// <summary>
        /// Establishes (or reestablishes) a receive buffer of a given size.
        /// </summary>
        /// <param name="size">Desired minimum size of receive buffer.</param>
        /// <returns>New receive buffer.</returns>
        public byte[] SetReceiveBuffer(int size)
        {
            m_receiveBuffer = new byte[size];
            return m_receiveBuffer;
        }

        /// <summary>
        /// Resets <see cref="TransportProvider{T}"/>.
        /// </summary>
        public void Reset()
        {
            m_receiveBuffer = null;
            m_sendBuffer = null;

            BytesReceived = -1;

            // Reset the statistics.
            Statistics.Reset();

            // Cleanup the provider.
            try
            {
                if ((object)Provider != null)
                    ((IDisposable)Provider).Dispose();
            }
            catch
            {
                // Ignore encountered exception during dispose.
            }
            finally
            {
                Provider = default(T);
            }
        }

        #endregion
    }
}
