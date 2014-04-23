//******************************************************************************************************
//  TransportStatistics.cs - Gbtc
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
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.Communication
{
    /// <summary>
    /// A class for statistics related to server-client communication.
    /// </summary>
    public class TransportStatistics
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// <see cref="DateTime"/> of the last send operation. (UTC Time)
        /// </summary>
        public DateTime LastSend;

        /// <summary>
        /// <see cref="DateTime"/> of the last receive operation. (UTC Time)
        /// </summary>
        public DateTime LastReceive;

        /// <summary>
        /// Number of bytes sent in the last send operation.
        /// </summary>
        public int LastBytesSent;

        /// <summary>
        /// Number of bytes received in the last receive operation.
        /// </summary>
        public int LastBytesReceived;

        /// <summary>
        /// Total number of bytes sent.
        /// </summary>
        public long TotalBytesSent;

        /// <summary>
        /// Total number of bytes received.
        /// </summary>
        public long TotalBytesReceived;

        ///// <summary>
        ///// Size of the current payload being received.
        ///// </summary>
        //public int ReceivePayloadLength;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Resets the <see cref="TransportStatistics"/>.
        /// </summary>
        public void Reset()
        {
            LastSend = DateTime.MinValue;
            LastReceive = DateTime.MinValue;
            LastBytesSent = 0;
            LastBytesReceived = 0;
            TotalBytesSent = 0;
            TotalBytesReceived = 0;
        }

        /// <summary>
        /// Updates statistics related to sending of data.
        /// </summary>
        /// <param name="bytesSent">Number of bytes sent in the send operation.</param>
        public void UpdateBytesSent(int bytesSent)
        {
            LastSend = DateTime.UtcNow;
            LastBytesSent = bytesSent;
            TotalBytesSent += LastBytesSent;
        }

        /// <summary>
        /// Updates statistics related to receiving of data.
        /// </summary>
        /// <param name="bytesReceived">Number of bytes received in the receive operation.</param>
        public void UpdateBytesReceived(int bytesReceived)
        {
            LastReceive = DateTime.UtcNow;
            LastBytesReceived = bytesReceived;
            TotalBytesReceived += LastBytesReceived;
        }

        #endregion
    }
}
