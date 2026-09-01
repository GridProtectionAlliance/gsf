//******************************************************************************************************
//  SFtpDirectory.cs - Gbtc
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
using System.Collections.Generic;
using System.Linq;
using GSF.Net.VirtualFtpClient;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace GSF.Net.SFtp
{
    public class SFtpDirectory : FtpDirectory
    {
        #region [ Constructors ]

        public SFtpDirectory(FtpClient client, string fullPath)
            : base(client)
        {
            FullPath = fullPath;
            LazyListing = new(ListDirectory);
        }

        #endregion

        #region [ Properties ]

        public override string Name => FullPath != "/"
            ? FullPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries).Last()
            : FullPath;

        public override string FullPath { get; }

        public override Dictionary<string, FtpFile> Files => LazyListing.Value
            .Where(file => file.IsRegularFile)
            .ToDictionary(file => file.Name, ToVirtualFtpFile);

        public override Dictionary<string, FtpDirectory> SubDirectories => LazyListing.Value
            .Where(file => file.IsDirectory)
            .ToDictionary(file => file.Name, ToVirtualFtpDirectory);

        private new ISftpClient Client => base.Client.SFtpClientConnection.Client
            ?? throw new InvalidOperationException("SFTP client is not connected");

        private Lazy<List<ISftpFile>> LazyListing { get; }

        #endregion

        #region [ Methods ]

        private List<ISftpFile> ListDirectory()
        {
            return [.. Client.ListDirectory(FullPath)];
        }

        private FtpFile ToVirtualFtpFile(ISftpFile file)
        {
            return new SFtpFile(this, file);
        }

        private FtpDirectory ToVirtualFtpDirectory(ISftpFile file)
        {
            return new SFtpDirectory(base.Client, file.FullName);
        }

        #endregion
    }
}
