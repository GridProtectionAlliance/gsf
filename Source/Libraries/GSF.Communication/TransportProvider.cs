//******************************************************************************************************
//  TransportProvider.cs - Gbtc
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
using GSF.Diagnostics;

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
        public byte[] SendBuffer { get; private set; }

        /// <summary>
        /// Gets send buffer requested size.
        /// </summary>
        public int SendBufferSize => SendBuffer?.Length ?? 0;

        /// <summary>
        /// Gets buffer used for receiving data.
        /// </summary>
        /// <remarks>
        /// Use <see cref="SetReceiveBuffer"/> to reset and/or establish receive buffer size.
        /// </remarks>
        public byte[] ReceiveBuffer { get; private set; }

        /// <summary>
        /// Gets receive buffer requested size.
        /// </summary>
        public int ReceiveBufferSize => ReceiveBuffer?.Length ?? 0;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Establishes (or reestablishes) a send buffer of a given size.
        /// </summary>
        /// <param name="size">Desired minimum size of send buffer.</param>
        /// <returns>New send buffer.</returns>
        public byte[] SetSendBuffer(int size)
        {
            SendBuffer = new byte[size];
            return SendBuffer;
        }

        /// <summary>
        /// Establishes (or reestablishes) a receive buffer of a given size.
        /// </summary>
        /// <param name="size">Desired minimum size of receive buffer.</param>
        /// <returns>New receive buffer.</returns>
        public byte[] SetReceiveBuffer(int size)
        {
            ReceiveBuffer = new byte[size];
            return ReceiveBuffer;
        }

        /// <summary>
        /// Resets <see cref="TransportProvider{T}"/>.
        /// </summary>
        public void Reset()
        {
            ReceiveBuffer = null;
            SendBuffer = null;

            BytesReceived = -1;

            // Reset the statistics.
            Statistics.Reset();

            // Cleanup the provider.
            try
            {
                if (Provider is IDisposable provider)
                    provider.Dispose();
            }
            catch (Exception ex)
            {
                // Ignore encountered exception during dispose.
                Logger.SwallowException(ex);
            }
            finally
            {
                Provider = default(T);
            }
        }

        #endregion
    }
}
