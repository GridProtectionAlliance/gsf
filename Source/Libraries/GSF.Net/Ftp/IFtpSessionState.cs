//******************************************************************************************************
//  IFtpSessionState.cs - Gbtc
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
//  09/23/2008 - J. Ritchie Carroll
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
using System.Net;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Abstract representation of a FTP session state (e.g., connected or disconnected).
    /// </summary>
    internal interface IFtpSessionState : IDisposable
    {
        /// <summary>
        /// Gets or sets FTP server name (DNS name or IP).
        /// </summary>
        string Server { get; set; }

        /// <summary>
        /// Gets or sets FTP server port to use, defaults to 21.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Gets or sets the timeout, in milliseconds, for
        /// read and write operations, defaults to 30 seconds.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Gets or sets the passive/active mode of the FTP server.
        /// </summary>
        bool Passive { get; set; }

        /// <summary>
        /// Gets or sets the IP address to send with the PORT command.
        /// </summary>
        IPAddress ActiveAddress { get; set; }

        /// <summary>
        /// Gets or sets the minimum value in the range of ports
        /// used when listening for connections in active mode.
        /// </summary>
        int MinActivePort { get; set; }

        /// <summary>
        /// Gets or sets the maximum value in the range of ports
        /// used when listening for connections in active mode.
        /// </summary>
        int MaxActivePort { get; set; }

        /// <summary>
        /// Gets or sets current FTP session directory.
        /// </summary>
        FtpDirectory CurrentDirectory { get; set; }

        /// <summary>
        /// Gets FTP session root directory entry.
        /// </summary>

        FtpDirectory RootDirectory { get; }

        /// <summary>
        /// Gets the current FTP control channel.
        /// </summary>
        FtpControlChannel ControlChannel { get; }

        /// <summary>
        /// Returns true if FTP session is currently busy.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Aborts current file transfer.
        /// </summary>
        void AbortTransfer();

        /// <summary>
        /// Connects to FTP server using specified credentials.
        /// </summary>
        /// <param name="userName">User name used to authenticate to FTP server.</param>
        /// <param name="password">Password used to authenticate to FTP server.</param>
        void Connect(string userName, string password);

        /// <summary>
        /// Closes current FTP session.
        /// </summary>
        void Close();
    }
}