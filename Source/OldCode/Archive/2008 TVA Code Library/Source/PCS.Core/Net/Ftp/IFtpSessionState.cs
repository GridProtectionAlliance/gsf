//*******************************************************************************************************
//  IFtpSessionState.cs
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
//  09/23/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace PCS.Net.Ftp
{
    /// <summary>
    /// Abstract representation of a FTP session state (e.g., connected or disconnected).
    /// </summary>
    internal interface IFtpSessionState
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