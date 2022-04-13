//******************************************************************************************************
//  FtpFile.cs - Gbtc
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
using System.IO;
using System.Threading;
using GSF.Net.Ftp;
using GSF.Net.TFtp;
using GSF.Net.TFtp.Channel;
using GSF.Net.TFtp.Transfer;
using GSF.Units;

namespace GSF.Net.VirtualFtpClient;

/// <summary>
/// Represents a virtual FTP file for the specified target <see cref="FtpType"/>.
/// </summary>
public abstract class FtpFile
{
    /// <summary>
    /// Name of file.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Full path of file.
    /// </summary>
    public string FullPath => Parent.FullPath + Name;

    /// <summary>
    /// Gets size of file.
    /// </summary>
    public abstract long Size { get; }

    /// <summary>
    /// Gets or sets permission of file.
    /// </summary>
    public virtual string Permission { get; set; }

    /// <summary>
    /// Gets or sets timestamp of file.
    /// </summary>
    public virtual DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets parent directory of file.
    /// </summary>
    public FtpDirectory Parent { get; }

    /// <summary>
    /// Gets the associated <see cref="FtpClient"/> of this <see cref="FtpFile"/>.
    /// </summary>
    public FtpClient Client => Parent.Client;
        
    /// <summary>
    /// Downloads remote file, synchronously.
    /// </summary>
    public void Get() => Get(Name);

    /// <summary>
    /// Downloads remote file using alternate local filename.
    /// </summary>
    /// <param name="localFile">Local filename to use for download.</param>
    public abstract void Get(string localFile);

    /// <summary>
    /// Removes remote file.
    /// </summary>
    public abstract void Remove();

    /// <summary>
    /// Creates a new <see cref="FtpFile"/>.
    /// </summary>
    /// <param name="parent">Parent directory of file.</param>
    protected FtpFile(FtpDirectory parent)
    {
        Parent = parent;
    }
}

internal class NativeFtpFile : FtpFile
{
    private readonly Ftp.FtpFile m_file;

    internal NativeFtpFile(FtpDirectory parent, Ftp.FtpFile file) : base(parent)
    {
        m_file = file;
    }

    public override string Name => m_file.Name;

    public override long Size => m_file.Size;

    public override string Permission
    {
        get => m_file.Permission;
        set => m_file.Permission = value;
    }

    public override DateTime Timestamp
    { 
        get => m_file.Timestamp; 
        set => m_file.Timestamp = value;
    }

    public override void Get(string localFile) => 
        m_file.Get(localFile);

    public override void Remove() => 
        m_file.Remove();
}

internal class TFtpFile : FtpFile
{
    private readonly ManualResetEventSlim m_waitHandle = new();
    private long m_getActive;
    private long m_bytesTransferred;

    internal TFtpFile(FtpDirectory parent, string name, long size = -1) : base(parent)
    {
        Name = name;
        Size = size;
        base.Timestamp = DateTime.UtcNow;
    }

    ~TFtpFile()
    {
        m_waitHandle?.Dispose();
    }

    public override string Name { get; }
        
    public override long Size { get; }
        
    public void Get(Stream target, string targetName)
    {
        if (Interlocked.CompareExchange(ref m_getActive, 1, 0) != 0)
        {
            Client.OnFileTransferNotification(new($"TFTP transfer for {Name} already has an active download - cancelling get.", FtpResponse.TransferAborted, FtpTransferResult.Abort));
            return;
        }

        try
        {
            m_waitHandle.Reset();

            using ITFtpTransfer transfer = Client.TFtpClientConnection.Download(Name);

            transfer.TransferMode = TFtpTransferMode.octet;
            transfer.OnError += Transfer_OnError;
            transfer.OnProgress += Transfer_OnProgress;
            transfer.OnFinished += Transfer_OnFinished;

            ITransferChannel connection = (transfer as TFtpTransfer)?.GetConnection();

            if (connection is not null)
                connection.OnCommandReceived += Connection_OnCommandReceived;

            try
            {
                Client.OnBeginFileTransfer(targetName, Name, TransferDirection.Download);

                transfer.Start(target);

                // Keep waiting so long as there is progress with download...
                while (!m_waitHandle.Wait(Client.Timeout))
                {
                    if (Interlocked.Exchange(ref m_bytesTransferred, 0) == 0)
                        throw new TimeoutException($"timeout while downloading data, no data received after {Time.ToElapsedTimeString(Client.Timeout / 1000.0D, 2)} - cancelling get.");
                }

                target.Flush();

                Client.OnEndFileTransfer(targetName, Name, TransferDirection.Download);
                Client.OnFileTransferNotification(new($"TFTP transfer for {Name} succeeded", FtpResponse.RequestFileActionComplete, FtpTransferResult.Complete));
            }
            catch (Exception ex)
            {
                Client.OnFileTransferNotification(new($"TFTP transfer for {Name} failed: {ex.Message}", FtpResponse.TransferAborted, FtpTransferResult.Fail));
            }
            finally
            {
                if (connection is not null)
                    connection.OnCommandReceived -= Connection_OnCommandReceived;

                transfer.OnError -= Transfer_OnError;
                transfer.OnProgress -= Transfer_OnProgress;
                transfer.OnFinished -= Transfer_OnFinished;

                target.Close();
            }
        }
        finally
        {
            Interlocked.Exchange(ref m_getActive, 0);
        }
    }

    public override void Get(string localFile)
    {
        using FileStream target = File.Create(localFile);
        Get(target, localFile);
    }

    public override void Remove()
    {
        // Ignored - not supported by TFTP
    }

    private void Transfer_OnError(ITFtpTransfer _, TFtpTransferError error)
    {
        Client.OnFileTransferNotification(new($"TFTP transfer for {Name} failed: {error}", (error as TFtpErrorPacket)?.ErrorCode ?? FtpResponse.TransferAborted, FtpTransferResult.Fail));
        m_waitHandle.Set();
    }

    private void Transfer_OnFinished(ITFtpTransfer _) => 
        m_waitHandle.Set();

    private void Transfer_OnProgress(ITFtpTransfer _, TFtpTransferProgress progress)
    {
        Interlocked.Add(ref m_bytesTransferred, progress.TransferredBytes);
        Client.OnFileTransferProgress(new("TFTP Download File Transfer", $"Transferring file \"{Name}\"...", progress.TotalBytes, progress.TransferredBytes), TransferDirection.Download);
    }

    private void Connection_OnCommandReceived(TFtp.Commands.ITFtpCommand command, System.Net.EndPoint _) => 
        Client.OnResponseReceived(command.ToString());
}