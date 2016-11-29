//******************************************************************************************************
//  FtpReader.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  09/18/2013 - Ritchie
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Net.Ftp;
using GSF.TimeSeries.Adapters;

namespace FtpAdapters
{
    /// <summary>
    /// Defines an adapter that downloads FTP files.
    /// </summary>
    [Description("FTP Reader: Downloads a file from an FTP server")]
    public class FtpReader : FacileActionAdapterBase
    {
        #region [ Members ]

        // Fields
        private FtpClient m_client;
        private string m_host;
        private string m_userName;
        private string m_password;
        private string m_remotePath;
        private string m_remoteFileName;
        private string m_localPath;
        private string m_localFileName;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FtpReader"/>.
        /// </summary>
        public FtpReader()
        {
            m_client = new FtpClient(true);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets FTP host name.
        /// </summary>
        [ConnectionStringParameter, Description("Defines FTP host name.")]
        public string Host
        {
            get
            {
                return m_host;
            }
            set
            {
                m_host = value;
            }
        }

        /// <summary>
        /// Gets or sets FTP user name.
        /// </summary>
        [ConnectionStringParameter, Description("Defines FTP user name - leave blank for anonymous."), DefaultValue("")]
        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        /// <summary>
        /// Gets or sets FTP password.
        /// </summary>
        [ConnectionStringParameter, Description("Defines FTP password - leave blank for anonymous."), DefaultValue("")]
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
            }
        }

        /// <summary>
        /// Gets or sets remote FTP path.
        /// </summary>
        [ConnectionStringParameter, Description("Defines FTP remote path - leave blank for root folder."), DefaultValue("/")]
        public string RemotePath
        {
            get
            {
                return m_remotePath;
            }
            set
            {
                m_remotePath = value;
            }
        }

        /// <summary>
        /// Gets or sets remote FTP file name.
        /// </summary>
        [ConnectionStringParameter, Description("Defines FTP remote file name.")]
        public string RemoteFileName
        {
            get
            {
                return m_remoteFileName;
            }
            set
            {
                m_remoteFileName = value;
            }
        }

        /// <summary>
        /// Gets or sets local path.
        /// </summary>
        [ConnectionStringParameter, Description("Defines FTP local path - leave blank for app path."), DefaultValue("")]
        public string LocalPath
        {
            get
            {
                return m_localPath;
            }
            set
            {
                m_localPath = value;
            }
        }

        /// <summary>
        /// Gets or sets local file name.
        /// </summary>
        [ConnectionStringParameter, Description("Defines FTP local file name.")]
        public string LocalFileName
        {
            get
            {
                return m_localFileName;
            }
            set
            {
                m_localFileName = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FtpReader"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_client != null)
                        {
                            m_client.Dispose();
                            m_client = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="FtpReader"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="FtpReader"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"Downloads the FTP file \"{Path.Combine(m_remotePath, m_remoteFileName)}\"";
        }

        /// <summary>
        /// Initializes <see cref="FtpReader"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            if (!settings.TryGetValue("host", out m_host))
                throw new ArgumentException("Missing connection string parameter \"host\"");

            if (!settings.TryGetValue("userName", out m_userName) || string.IsNullOrWhiteSpace(m_userName))
                m_userName = "anonymous";

            if (!settings.TryGetValue("password", out m_password) || string.IsNullOrWhiteSpace(m_password))
                m_password = "anonymous";

            if (!settings.TryGetValue("remotePath", out m_remotePath) || string.IsNullOrWhiteSpace(m_remotePath))
                m_remotePath = "/";

            if (!settings.TryGetValue("remoteFileName", out m_remoteFileName))
                throw new ArgumentException("Missing connection string parameter \"remoteFileName\"");

            if (!settings.TryGetValue("localPath", out m_localPath) || string.IsNullOrWhiteSpace(m_localPath))
                m_localPath = FilePath.GetAbsolutePath("");

            if (!settings.TryGetValue("localFileName", out m_localFileName))
                throw new ArgumentException("Missing connection string parameter \"localFileName\"");

            // Kick-off file read after successful initialize
            ThreadPool.QueueUserWorkItem(state => DownloadFile());
        }

        /// <summary>
        /// Connects to FTP host and downloads file.
        /// </summary>
        [AdapterCommand("Connects to FTP host and downloads file")]
        public void DownloadFile()
        {
            try
            {
                m_client.Server = m_host;
                m_client.Connect(m_userName, m_password);
                m_client.SetCurrentDirectory(m_remotePath);
                m_client.CurrentDirectory.GetFile(Path.Combine(m_localPath, m_localFileName), m_remoteFileName);
                m_client.Close();
                OnStatusMessage(MessageLevel.Info, $"FTP file \"{Path.Combine(m_remotePath, m_remoteFileName)}\" downloaded to \"{Path.Combine(m_localPath, m_localFileName)}\" from {m_host}");
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to read FTP file: {ex.Message}", ex));
            }
        }

        #endregion
    }
}
