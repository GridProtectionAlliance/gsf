//******************************************************************************************************
//  TFtpTransferError.cs - Gbtc
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

// ReSharper disable InconsistentNaming
namespace GSF.Net.TFtp
{
    /// <summary>
    /// Base class for all errors that may be passed to <code>ITFtpTransfer.OnError</code>.
    /// </summary>
    public abstract class TFtpTransferError
    {
        public abstract override string ToString();
    }

    /// <summary>
    /// Errors that are sent from the remote party using the TFTP Error Packet are represented
    /// by this class.
    /// </summary>
    public class TFtpErrorPacket : TFtpTransferError
    {
        /// <summary>
        /// Error code that was sent from the other party.
        /// </summary>
        public ushort ErrorCode { get; }

        /// <summary>
        /// Error description that was sent by the other party.
        /// </summary>
        public string ErrorMessage { get; }

        public TFtpErrorPacket(ushort errorCode, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentException("You must provide an errorMessage.");

            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public override string ToString() => $"{ErrorCode} - {ErrorMessage}";

        #region Predefined error packets from RFC 1350

        public static readonly TFtpErrorPacket FileNotFound = new TFtpErrorPacket(1, "File not found");
        public static readonly TFtpErrorPacket AccessViolation = new TFtpErrorPacket(2, "Access violation");
        public static readonly TFtpErrorPacket DiskFull = new TFtpErrorPacket(3, "Disk full or allocation exceeded");
        public static readonly TFtpErrorPacket IllegalOperation = new TFtpErrorPacket(4, "Illegal TFTP operation");
        public static readonly TFtpErrorPacket UnknownTransferId = new TFtpErrorPacket(5, "Unknown transfer ID");
        public static readonly TFtpErrorPacket FileAlreadyExists = new TFtpErrorPacket(6, "File already exists");
        public static readonly TFtpErrorPacket NoSuchUser = new TFtpErrorPacket(7, "No such user");

        #endregion
    }

    /// <summary>
    /// Network errors (i.e. socket exceptions) are represented by this class.
    /// </summary>
    public class NetworkError : TFtpTransferError
    {
        public Exception Exception { get; }

        public NetworkError(Exception exception) => Exception = exception;

        public override string ToString() => Exception.ToString();
    }

    /// <summary>
    /// $(ITFtpTransfer.RetryTimeout) has been exceeded more than $(ITFtpTransfer.RetryCount) times in a row.
    /// </summary>
    public class TimeoutError : TFtpTransferError
    {
        private readonly TimeSpan m_retryTimeout;
        private readonly int m_retryCount;

        public TimeoutError(TimeSpan retryTimeout, int retryCount)
        {
            m_retryTimeout = retryTimeout;
            m_retryCount = retryCount;
        }

        public override string ToString()
        {
            return $"Timeout error. RetryTimeout ({m_retryTimeout}) violated more than {m_retryCount} times in a row";
        }
    }
}
