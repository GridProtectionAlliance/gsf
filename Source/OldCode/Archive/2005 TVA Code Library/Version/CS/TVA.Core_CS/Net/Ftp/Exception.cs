//*******************************************************************************************************
//  Exception.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/22/2003 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Net.Ftp
{
    public abstract class ExceptionBase : Exception
    {
        private Response m_ftpResponse = null;

        internal ExceptionBase(string message)
            : base(message)
        {
        }

        internal ExceptionBase(string message, ExceptionBase inner)
            : base(message, inner)
        {
        }

        internal ExceptionBase(string message, Response ftpResponse)
            : base(message)
        {
            m_ftpResponse = ftpResponse;
        }

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
    }

    public class InvalidResponseException : ExceptionBase
    {
        internal InvalidResponseException(string message, Response ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    public class AuthenticationException : ExceptionBase
    {
        internal AuthenticationException(string message, Response ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    public class FileNotFoundException : ExceptionBase
    {
        internal FileNotFoundException(string remoteFile)
            : base("Remote file (" + remoteFile + ") not found.  Try refreshing the directory.")
        {
        }
    }

    public class ServerDownException : ExceptionBase
    {
        internal ServerDownException(Response ftpResponse)
            : this("FTP service was down.", ftpResponse)
        {
        }

        internal ServerDownException(string message, Response ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    public class CommandException : ExceptionBase
    {

        internal CommandException(string message, Response ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    public class DataTransferException : ExceptionBase
    {
        internal DataTransferException()
            : base("Data transfer error: previous transfer not finished.")
        {
        }

        internal DataTransferException(string message, Response ftpResponse)
            : base(message, ftpResponse)
        {
        }
    }

    public class UserAbortException : ExceptionBase
    {
        internal UserAbortException()
            : base("File Transfer aborted by user.")
        {
        }
    }

    public class ResumeNotSupportedException : ExceptionBase
    {
        internal ResumeNotSupportedException(Response ftpResponse)
            : base("Data transfer error: server does not support resuming.", ftpResponse)
        {
        }
    }
}