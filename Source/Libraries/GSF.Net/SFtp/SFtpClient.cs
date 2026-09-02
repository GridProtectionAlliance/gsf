//******************************************************************************************************
//  SFtpClient.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//  08/28/2026 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GSF.Diagnostics;
using Renci.SshNet;

namespace GSF.Net.SFtp
{
    public class SFtpClient : IDisposable
    {
        #region [ Members ]

        // Constants
        public const int DefaultPort = 22;

        #endregion

        #region [ Properties ]

        public string Server { get; set; }
        public int Port { get; set; } = DefaultPort;

        public string RootDirectory { get; private set; }
        public string CurrentDirectory => Client.WorkingDirectory;

        internal Renci.SshNet.SftpClient Client { get; set; }
        private bool IsDisposed { get; set; }

        #endregion

        #region [ Methods ]

        public void Connect(string username, string password)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SFtpClient));
            if (Client is not null)
                throw new InvalidOperationException("Client is already connected");
            if (string.IsNullOrEmpty(Server))
                throw new InvalidOperationException("Server IP or hostname must be provided to connect to an SSH server");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            PasswordAuthenticationMethod authentication = new(username, password);
            Connect(username, authentication);
        }

        public void Connect(string username, string keyFile, string passPhrase = null, string certificateFile = null)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SFtpClient));
            if (Client is not null)
                throw new InvalidOperationException("Client is already connected");
            if (string.IsNullOrEmpty(Server))
                throw new InvalidOperationException("Server IP or hostname must be provided to connect to an SSH server");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(keyFile))
                throw new ArgumentNullException(nameof(keyFile));

            PrivateKeyFile key = new(keyFile, passPhrase, certificateFile);
            PrivateKeyAuthenticationMethod authentication = new(username, key);
            Connect(username, authentication);
        }

        private void Connect(string username, AuthenticationMethod authentication)
        {
            ConnectionInfo info = new(Server, Port, username, authentication);
            Client = new(info);
            Client.ErrorOccurred += Client_ErrorOccurred;
            Client.Connect();
            RootDirectory = CurrentDirectory;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                Client?.Dispose();
            }
            finally
            {
                IsDisposed = true;
            }
        }

        private void Client_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            const string EventName = "SFTP Error";
            Exception ex = e.Exception;
            Log.Publish(MessageLevel.Debug, EventName, ex.Message, exception: ex);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(SFtpClient), MessageClass.Component);

        #endregion
    }
}
