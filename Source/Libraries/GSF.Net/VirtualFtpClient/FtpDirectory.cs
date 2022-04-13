//******************************************************************************************************
//  FtpDirectory.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  05/12/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSF.Net.VirtualFtpClient;

/// <summary>
/// Represents a virtual FTP directory for the specified target <see cref="FtpType"/>.
/// </summary>
public abstract class FtpDirectory
{
    /// <summary>
    /// Name of directory.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Full path of directory.
    /// </summary>
    public abstract string FullPath { get; }

    /// <summary>
    /// Gets the list of files in this <see cref="FtpDirectory"/>.
    /// </summary>
    public abstract Dictionary<string, FtpFile> Files { get; }

    /// <summary>
    /// Gets the list of subdirectories in this <see cref="FtpDirectory"/>.
    /// </summary>
    public abstract Dictionary<string, FtpDirectory> SubDirectories { get; }

    /// <summary>
    /// Gets the associated <see cref="FtpClient"/> of this <see cref="FtpDirectory"/>.
    /// </summary>
    public FtpClient Client { get; }

    /// <summary>
    /// Creates a new <see cref="FtpDirectory"/>.
    /// </summary>
    /// <param name="client">Associated <see cref="FtpClient"/>.</param>
    protected FtpDirectory(FtpClient client)
    {
        Client = client;
    }
}

internal class NativeFtpDirectory : FtpDirectory
{
    private readonly Ftp.FtpDirectory m_directory;

    internal NativeFtpDirectory(FtpClient client, Ftp.FtpDirectory directory) : base(client)
    {
        m_directory = directory;
    }

    public override string Name => m_directory.Name;

    public override string FullPath => m_directory.FullPath;

    public override Dictionary<string, FtpFile> Files =>
        m_directory.Files.ToDictionary(file => file.Name, file => (FtpFile)new NativeFtpFile(this, file),
            m_directory.CaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

    public override Dictionary<string, FtpDirectory> SubDirectories =>
        m_directory.SubDirectories.ToDictionary(dir => dir.Name, dir => (FtpDirectory)new NativeFtpDirectory(Client, dir),
            m_directory.CaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
}

internal class TFtpDirectory : FtpDirectory
{
    public TFtpDirectory(FtpClient client, string directoryFile) : base(client)
    {
        if (string.IsNullOrWhiteSpace(directoryFile))
            directoryFile = FtpClient.DefaultTFtpDirectoryFile;

        TFtpFile dirFile = new(this, directoryFile);
        using MemoryStream target = new();
        dirFile.Get(target, directoryFile);

        string dirFileText = Encoding.UTF8.GetString(target.ToArray());

        foreach (string line in dirFileText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
        {
            int colon = line.IndexOf(':');

            if (colon == -1)
                continue;

            string fileName = line.Substring(0, colon).Trim();
            TFtpFiles.Add(new(this, fileName));
        }
    }

    public List<TFtpFile> TFtpFiles = new();

    public override string Name => "/";

    public override string FullPath => "/";

    public override Dictionary<string, FtpFile> Files =>
        TFtpFiles.ToDictionary(file => file.Name, file => (FtpFile)file,
            Client.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

    public override Dictionary<string, FtpDirectory> SubDirectories => new();
}