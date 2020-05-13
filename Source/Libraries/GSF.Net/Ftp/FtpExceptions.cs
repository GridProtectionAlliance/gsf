//******************************************************************************************************
//  FtpExceptions.cs - Gbtc
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

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// FTP exception base class.
    /// </summary>
    [Serializable]
    public abstract class FtpExceptionBase : Exception
    {
        private readonly FtpResponse m_ftpResponse;

        internal FtpExceptionBase(string message)
            : base(message)
        {
        }

        internal FtpExceptionBase(string message, FtpExceptionBase inner)
            : base(message, inner)
        {
        }

        internal FtpExceptionBase(string message, FtpResponse ftpResponse)
            : base(message)
        {
            m_ftpResponse = ftpResponse;
        }

        /// <summary>
        /// Response message related to exception, if any.
        /// </summary>
        public string ResponseMessage
        {
            get
            {
                if (m_ftpResponse != null)
                    return m_ftpResponse.Message;
                return "";
            }
        }

        /// <summary>
        /// Deserializes the <see cref="FtpExceptionBase"/>.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Invalid FTP response exception.
    /// </summary>
    [Serializable]
    public class FtpInvalidResponseException : FtpExceptionBase
    {
        internal FtpInvalidResponseException(string message, FtpResponse ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    /// <summary>
    /// FTP authentication exception.
    /// </summary>
    [Serializable]
    public class FtpAuthenticationException : FtpExceptionBase
    {
        internal FtpAuthenticationException(string message, FtpResponse ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    /// <summary>
    /// FTP file not found exception.
    /// </summary>
    [Serializable]
    public class FtpFileNotFoundException : FtpExceptionBase
    {
        internal FtpFileNotFoundException(string remoteFile)
            : base($"Remote file ({remoteFile}) not found.  Try refreshing the directory.")
        {
        }
    }

    /// <summary>
    /// FTP server down exception.
    /// </summary>
    [Serializable]
    public class FtpServerDownException : FtpExceptionBase
    {
        internal FtpServerDownException(FtpResponse ftpResponse)
            : this("FTP service was down.", ftpResponse)
        {
        }

        internal FtpServerDownException(string message, FtpResponse ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    /// <summary>
    /// FTP command exception.
    /// </summary>
    [Serializable]
    public class FtpCommandException : FtpExceptionBase
    {

        internal FtpCommandException(string message, FtpResponse ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    /// <summary>
    /// FTP data transfer exception.
    /// </summary>
    [Serializable]
    public class FtpDataTransferException : FtpExceptionBase
    {
        internal FtpDataTransferException()
            : base("Data transfer error: previous transfer not finished.")
        {
        }

        internal FtpDataTransferException(string message, FtpResponse ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    /// <summary>
    /// FTP user abort exception.
    /// </summary>
    [Serializable]
    public class FtpUserAbortException : FtpExceptionBase
    {
        internal FtpUserAbortException()
            : base("File Transfer aborted by user.")
        {
        }
    }

    /// <summary>
    /// FTP resume not supported exception.
    /// </summary>
    [Serializable]
    public class FtpResumeNotSupportedException : FtpExceptionBase
    {
        internal FtpResumeNotSupportedException(FtpResponse ftpResponse)
            : base("Data transfer error: server does not support resuming.", ftpResponse)
        {
        }
    }
}