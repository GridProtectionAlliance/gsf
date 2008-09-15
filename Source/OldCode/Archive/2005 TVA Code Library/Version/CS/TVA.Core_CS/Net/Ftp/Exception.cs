using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// James Ritchie Carroll - 2003

namespace TVA
{
    namespace Net
    {
        namespace Ftp
        {


            public class Exception : System.Exception
            {



                private Response m_ftpResponse = null;

                internal Exception(string message)
                    : base(message)
                {


                }

                internal Exception(string message, Exception inner)
                    : base(message, inner)
                {


                }

                internal Exception(string message, Response ftpResponse)
                    : base(message)
                {

                    m_ftpResponse = ftpResponse;

                }

                public string ResponseMessage
                {
                    get
                    {
                        if (!(m_ftpResponse == null))
                        {
                            return m_ftpResponse.Message;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }

                public override string Message
                {
                    get
                    {
                        return base.Message;
                    }
                }

            }

            public class InvalidResponseException : Exception
            {



                internal InvalidResponseException(string message, Response ftpResponse)
                    : base(message, ftpResponse)
                {


                }

            }

            public class AuthenticationException : Exception
            {



                internal AuthenticationException(string message, Response ftpResponse)
                    : base(message, ftpResponse)
                {


                }

            }

            public class FileNotFoundException : Exception
            {



                internal FileNotFoundException(string remoteFile)
                    : base("Remote file (" + remoteFile + ") not found.  Try refreshing the directory.")
                {


                }

            }

            public class ServerDownException : Exception
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

            public class CommandException : Exception
            {



                internal CommandException(string message, Response ftpResponse)
                    : base(message, ftpResponse)
                {


                }

            }

            public class DataTransferException : Exception
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

            public class UserAbortException : Exception
            {



                internal UserAbortException()
                    : base("File Transfer aborted by user.")
                {


                }

            }

            public class ResumeNotSupportedException : Exception
            {



                internal ResumeNotSupportedException(Response ftpResponse)
                    : base("Data transfer error: server does not support resuming.", ftpResponse)
                {


                }

            }

        }
    }
}
