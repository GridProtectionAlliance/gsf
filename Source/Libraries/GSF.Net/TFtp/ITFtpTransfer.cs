//******************************************************************************************************
//  ITFtpTransfer.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/12/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        https://github.com/Callisto82/tftp.net
//  
//   Copyright © 2011, Michael Baer
//
//*******************************************************************************************************

#endregion

using System;
using System.IO;

// ReSharper disable InconsistentNaming
namespace GSF.Net.TFtp
{
    public delegate void TFtpEventHandler(ITFtpTransfer transfer);
    public delegate void TFtpProgressHandler(ITFtpTransfer transfer, TFtpTransferProgress progress);
    public delegate void TFtpErrorHandler(ITFtpTransfer transfer, TFtpTransferError error);

    public enum TFtpTransferMode
    {
        ascii,
        octet,
        mail
    }

    /// <summary>
    /// Represents a single data transfer between a TFTP server and client.
    /// </summary>
    public interface ITFtpTransfer : IDisposable
    {
        /// <summary>
        /// Event that is being called while data is being transferred.
        /// </summary>
        event TFtpProgressHandler OnProgress;

        /// <summary>
        /// Event that will be called once the data transfer is finished.
        /// </summary>
        event TFtpEventHandler OnFinished; 

        /// <summary>
        /// Event that will be called if there is an error during the data transfer.
        /// Currently, this will return instances of ErrorFromRemoteEndpoint or NetworkError.
        /// </summary>
        event TFtpErrorHandler OnError;

        /// <summary>
        /// Requested TFTP transfer mode. For outgoing transfers, this member may be used to set the transfer mode.
        /// </summary>
        TFtpTransferMode TransferMode { get; set; }

        /// <summary>
        /// Transfer block size. Set this member to control the TFTP block size option (RFC 2349).
        /// </summary>
        int BlockSize { get; set; }

        /// <summary>
        /// Timeout after which commands are sent again.
        /// This member is also transmitted as the TFTP timeout interval option (RFC 2349).
        /// </summary>
        TimeSpan RetryTimeout { get; set; }

        /// <summary>
        /// Number of times that a RetryTimeout may occur before the transfer is cancelled with a TimeoutError.
        /// </summary>
        int RetryCount { get; set; }

        /// <summary>
        /// TFTP can transfer up to 65535 blocks. After that, the block counter wraps to either zero or one, depending on the expectations of the client.
        /// </summary>
        BlockCounterWrapAround BlockCounterWrapping { get; set; }

        /// <summary>
        /// Expected transfer size in bytes. 0 if size is unknown.
        /// </summary>
        long ExpectedSize { get; set; }

        /// <summary>
        /// Filename for the transferred file.
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// You can set your own object here to associate custom data with this transfer.
        /// </summary>
        object UserContext { get; set; }

        /// <summary>
        /// Call this function to start the transfer.
        /// </summary>
        /// <param name="data">The stream from which data is either read (when sending) or written to (when receiving).</param>
        void Start(Stream data);

        /// <summary>
        /// Cancel the currently running transfer, possibly sending the provided reason to the remote endpoint.
        /// </summary>
        void Cancel(TFtpErrorPacket reason);
    }
}
