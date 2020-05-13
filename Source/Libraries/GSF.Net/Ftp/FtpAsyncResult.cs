//******************************************************************************************************
//  FtpAsyncResult.cs - Gbtc
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
//  05/22/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        http://www.codeproject.com/KB/IP/net_ftp_upload.aspx
//  
//   Copyright Alex Kwok & Uwe Keim 
//
//   The Code Project Open License (CPOL):
//        http://www.codeproject.com/info/cpol10.aspx
//
//*******************************************************************************************************

#endregion

using System.Collections;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Asynchronous transfer result.
    /// </summary>
    public class FtpAsyncResult
    {
        #region [ Members ]

        // Constants
        
        /// <summary>
        /// FTP transfer result completed index.
        /// </summary>
        public const int Complete = 0;
        
        /// <summary>
        /// FTP transfer result failed index.
        /// </summary>
        public const int Fail = 1;
        
        /// <summary>
        /// FTP transfer result aborted index.
        /// </summary>
        public const int Abort = 2;

        // Fields
        private readonly BitArray m_result;

        #endregion

        #region [ Constructors ]

        internal FtpAsyncResult()
            : this("Success.", FtpResponse.InvalidCode, Complete)
        {
        }

        internal FtpAsyncResult(string message, int result)
            : this(message, FtpResponse.InvalidCode, result)
        {
        }

        internal FtpAsyncResult(string message, int ftpCode, int result)
        {
            m_result = new BitArray(3);
            Message = message;
            ResponseCode = ftpCode;
            m_result[result] = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns true if asynchronous transfer completed successfully.
        /// </summary>
        public bool IsSuccess => m_result[Complete];

        /// <summary>
        /// Returns true if asynchronous transfer failed.
        /// </summary>
        public bool IsFailed => m_result[Fail];

        /// <summary>
        /// Returns true if asynchronous transfer was aborted.
        /// </summary>
        public bool IsAborted => m_result[Abort];

        /// <summary>
        /// Gets response code from asynchronous transfer.
        /// </summary>
        public int ResponseCode { get; }

        /// <summary>
        /// Gets any message associated with asynchronous transfer.
        /// </summary>
        public string Message { get; }

        #endregion
    }
}