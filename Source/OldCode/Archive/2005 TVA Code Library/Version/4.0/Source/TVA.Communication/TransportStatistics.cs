//*******************************************************************************************************
//  TransportStatistics.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2008 - Pinal C Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Communication
{
    /// <summary>
    /// A class for statistics related to server-client communication.
    /// </summary>
    public class TransportStatistics
    {
        #region [ Members ]

        // Fields
        
        /// <summary>
        /// <see cref="DateTime"/> of the last send operation.
        /// </summary>
        public DateTime LastSend;

        /// <summary>
        /// <see cref="DateTime"/> of the last receive operation.
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
            LastSend = DateTime.Now;
            LastBytesSent = bytesSent;
            TotalBytesSent += LastBytesSent;
        }

        /// <summary>
        /// Updates statistics related to receiving of data.
        /// </summary>
        /// <param name="bytesReceived">Number of bytes received in the receive operation.</param>
        public void UpdateBytesReceived(int bytesReceived)
        {
            LastReceive = DateTime.Now;
            LastBytesReceived = bytesReceived;
            TotalBytesReceived += LastBytesReceived;
        }

        #endregion
    }
}
