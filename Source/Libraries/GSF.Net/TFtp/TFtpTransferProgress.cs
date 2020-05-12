//******************************************************************************************************
//  TFtpTransferProgress.cs - Gbtc
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

// ReSharper disable InconsistentNaming
namespace GSF.Net.TFtp
{
    public class TFtpTransferProgress
    {
        /// <summary>
        /// Number of bytes that have already been transferred.
        /// </summary>
        public long TransferredBytes { get; }

        /// <summary>
        /// Total number of bytes being transferred. May be 0 if unknown.
        /// </summary>
        public long TotalBytes { get; }

        public TFtpTransferProgress(long transferred, long total)
        {
            TransferredBytes = transferred;
            TotalBytes = total;
        }

        public override string ToString() => TotalBytes > 0 ? 
            $"{(TransferredBytes * 100L) / TotalBytes}% completed" : 
            $"{TransferredBytes} bytes transferred";
    }
}