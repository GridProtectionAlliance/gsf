//*******************************************************************************************************
//  FtpExceptions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/22/2003 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TVA.Net.Ftp
{
    /// <summary>
    /// FTP exception base class.
    /// </summary>
    [Serializable()]
    public abstract class FtpExceptionBase : Exception
    {
        private FtpResponse m_ftpResponse = null;

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
                if (!(m_ftpResponse == null))
                    return m_ftpResponse.Message;
                else
                    return "";
            }
        }

        /// <summary>
        /// Derserializes the <see cref="FtpExceptionBase"/>.
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
    [Serializable()]
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
    [Serializable()]
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
    [Serializable()]
    public class FtpFileNotFoundException : FtpExceptionBase
    {
        internal FtpFileNotFoundException(string remoteFile)
            : base("Remote file (" + remoteFile + ") not found.  Try refreshing the directory.")
        {
        }
    }

    /// <summary>
    /// FTP server down exception.
    /// </summary>
    [Serializable()]
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
    [Serializable()]
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
    [Serializable()]
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
    [Serializable()]
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
    [Serializable()]
    public class FtpResumeNotSupportedException : FtpExceptionBase
    {
        internal FtpResumeNotSupportedException(FtpResponse ftpResponse)
            : base("Data transfer error: server does not support resuming.", ftpResponse)
        {
        }
    }
}