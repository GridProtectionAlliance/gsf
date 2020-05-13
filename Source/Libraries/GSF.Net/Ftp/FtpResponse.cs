//******************************************************************************************************
//  FtpResponse.cs - Gbtc
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
//  12/23/2009 - Pinal C. Patel
//       Modified the message in the exception thrown when no data is available on the wire to be read.
//  12/24/2009 - Pinal C. Patel
//       Changed to wait indefinitely on a response from the server - need to introduce a timeout.
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
using System.Net.Sockets;
using System.Text;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Defines a FTP response.
    /// </summary>
    public class FtpResponse
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// FTP response code for invalid code.
        /// </summary>
        public const int InvalidCode = -1;

        /// <summary>
        /// FTP response code for data channel opened, transfer start.
        /// </summary>
        public const int DataChannelOpenedTransferStart = 125;

        /// <summary>
        /// FTP response code for file OK, begin open data channel.
        /// </summary>
        public const int FileOkBeginOpenDataChannel = 150;

        /// <summary>
        /// FTP response code for service ready.
        /// </summary>
        public const int ServiceReady = 220;

        /// <summary>
        /// FTP response code for closing data channel.
        /// </summary>
        public const int ClosingDataChannel = 226;

        /// <summary>
        /// FTP response code for enter passive mode.
        /// </summary>
        public const int EnterPassiveMode = 227;

        /// <summary>
        /// FTP response code for request file action complete.
        /// </summary>
        public const int RequestFileActionComplete = 250;

        /// <summary>
        /// FTP response code for user logged in.
        /// </summary>
        public const int UserLoggedIn = 230;

        /// <summary>
        /// FTP response code for user accepted waiting pass.
        /// </summary>
        public const int UserAcceptedWaitingPass = 331;

        /// <summary>
        /// FTP response code for request file action pending.
        /// </summary>
        public const int RequestFileActionPending = 350;

        /// <summary>
        /// FTP response code for service unavailable.
        /// </summary>
        public const int ServiceUnavailable = 421;

        /// <summary>
        /// FTP response code for transfer aborted.
        /// </summary>
        public const int TransferAborted = 426;

        #endregion

        #region [ Constructors ]

        internal FtpResponse(NetworkStream stream)
        {
            string response;

            Responses = new Queue();

            do
            {
                response = GetLine(stream);

                try
                {
                    Code = InvalidCode;
                    Code = int.Parse(response.Substring(0, 3));
                }
                catch
                {
                    throw new FtpInvalidResponseException("Invalid response", this);
                }

                Responses.Enqueue(response);
            }
            while (response.Length >= 4 && response[3] == '-');

            if (Code == ServiceUnavailable)
                throw new FtpServerDownException(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets FTP response message.
        /// </summary>
        public string Message => Responses.Peek().ToString();

        /// <summary>
        /// Gets FTP response queue.
        /// </summary>
        public Queue Responses { get; }

        /// <summary>
        /// Gets FTP response code.
        /// </summary>
        public int Code { get; }

        #endregion

        #region [ Methods ]

        private char ReadAppendChar(NetworkStream stream, StringBuilder toAppend)
        {
            int i = stream.ReadByte();

            if (i >= 0)
            {
                char c = Encoding.ASCII.GetChars(new[] { (byte)i })[0];
                toAppend.Append(c);
                return c;
            }

            return '\n';
        }

        private string GetLine(NetworkStream stream)
        {
            StringBuilder response = new StringBuilder(256);
            while (ReadAppendChar(stream, response) != '\n') { }
            return response.ToString();
        }

        #endregion
    }
}