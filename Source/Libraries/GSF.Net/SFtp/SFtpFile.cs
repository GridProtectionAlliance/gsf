//******************************************************************************************************
//  SFtpFile.cs - Gbtc
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
using System.IO;
using GSF.Net.VirtualFtpClient;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace GSF.Net.SFtp
{
    public class SFtpFile : FtpFile
    {
        #region [ Constructors ]

        public SFtpFile(FtpDirectory parent, ISftpFile file)
            : base(parent)
        {
            File = file;
        }

        #endregion

        #region [ Properties ]

        public override string Name => File.Name;

        public override long Size => File.Attributes.Size;

        public override DateTime Timestamp
        {
            get => File.LastWriteTime;
            set => File.LastWriteTime = value;
        }

        public override string Permission
        {
            get => GetPermission(File);
            set => SetPermission(File, value);
        }

        private ISftpFile File { get; }

        private new ISftpClient Client => base.Client.SFtpClientConnection.Client
            ?? throw new InvalidOperationException("SFTP client is not connected");

        #endregion

        #region [ Methods ]

        public override void Get(string localFile)
        {
            using (FileStream localStream = System.IO.File.OpenWrite(localFile))
            {
                Client.DownloadFile(File.FullName, localStream);
            }
        }

        public override void Remove()
        {
            File.Delete();
        }

        #endregion

        #region [ Static ]

        // Static Methods
        private string GetPermission(ISftpFile file)
        {
            string u = GetPermission(file.OwnerCanRead, file.OwnerCanWrite, file.OwnerCanExecute);
            string g = GetPermission(file.GroupCanRead, file.GroupCanWrite, file.GroupCanExecute);
            string o = GetPermission(file.OthersCanRead, file.OthersCanWrite, file.OthersCanExecute);
            return $"{u}{g}{o}";
        }

        private string GetPermission(bool canRead, bool canWrite, bool canExecute)
        {
            char read = canRead ? 'r' : '-';
            char write = canWrite ? 'w' : '-';
            char execute = canExecute ? 'x' : '-';
            return $"{read}{write}{execute}";
        }

        private void SetPermission(ISftpFile file, string permission)
        {
            if (permission.Length != 9)
                throw new FormatException("SFTP permissions must be exactly 9 characters in length");

            file.OwnerCanRead = ParsePermission(permission[0], 'r');
            file.OwnerCanWrite = ParsePermission(permission[1], 'w');
            file.OwnerCanExecute = ParsePermission(permission[2], 'x');
            file.GroupCanRead = ParsePermission(permission[3], 'r');
            file.GroupCanWrite = ParsePermission(permission[4], 'w');
            file.GroupCanExecute = ParsePermission(permission[5], 'x');
            file.OthersCanRead = ParsePermission(permission[6], 'r');
            file.OthersCanWrite = ParsePermission(permission[7], 'w');
            file.OthersCanExecute = ParsePermission(permission[8], 'x');
        }

        private bool ParsePermission(char permission, char expected)
        {
            if (permission == expected)
                return true;

            if (permission == '-')
                return false;

            throw new FormatException($"Invalid permission character; expected '{expected}' or '-', got '{permission}'");
        }

        #endregion
    }
}
