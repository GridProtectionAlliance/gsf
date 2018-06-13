//******************************************************************************************************
//  FtpDirectory.cs - Gbtc
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
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/13/2003 - Pinal C. Patel
//       Fixed bug in initialization of Name, FullPath and Parent properties.
//       Fixed null-reference exception occurrences in comparision methods/overloads.
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Represents a FTP directory.
    /// </summary>
    public class FtpDirectory : IFtpFile, IComparable<FtpDirectory>
    {
        #region [ Members ]

        // Nested Types
        internal class ItemInfo
        {
            public string Name;
            public string FullPath;
            public string Permission;
            public bool IsDirectory;
            public long Size;
            public FtpTimeStampParser TimeStamp = new FtpTimeStampParser();
        }

        // Events

        /// <summary>
        /// Raised when new directory line is scanned.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event Action<string> DirectoryListLineScan;

        /// <summary>
        /// Raised when there is an exception scanning a directory.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event Action<FtpExceptionBase> DirectoryScanException;

        // Fields
        private readonly FtpSessionConnected m_session;
        private FtpDirectory m_parent;
        private readonly string m_name;
        private readonly string m_fullPath;
        private Dictionary<string, FtpDirectory> m_subDirectories;
        private Dictionary<string, FtpFile> m_files;
        private bool m_caseInsensitive;
        private long m_size;
        private string m_permission;
        private DateTime m_timestamp;

        #endregion

        #region [ Constructors ]

        internal FtpDirectory(FtpSessionConnected s, bool caseInsensitive, string fullPath)
        {
            m_session = s;
            m_parent = null;
            m_caseInsensitive = caseInsensitive;

            if (fullPath.Substring(fullPath.Length - 1, 1) == "/")
                fullPath = fullPath.Substring(0, fullPath.Length - 1);

            if (fullPath.Length == 0)
            {
                m_name = "";
                m_fullPath = "/";
            }
            else
            {
                string[] directories = fullPath.Split('/');
                m_name = directories[directories.Length - 1];
                m_fullPath = fullPath + "/";
            }
        }

        internal FtpDirectory(FtpSessionConnected s, FtpDirectory parent, bool caseInsensitive, ItemInfo info)
        {
            m_session = s;
            m_parent = parent;
            m_caseInsensitive = caseInsensitive;

            if (info.Name.Length > 0)
            {
                m_name = info.Name;

                if ((object)parent == null)
                    m_fullPath = m_name + "/";
                else
                    m_fullPath = parent.FullPath + m_name + "/";
            }
            else
            {
                m_name = "";
                m_fullPath = "/";
            }

            m_size = info.Size;
            m_permission = info.Permission;
            m_timestamp = info.TimeStamp.Value;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets FTP case sensitivity of file and directory names.
        /// </summary>
        /// <remarks>
        /// Set to true to not be case sensitive with FTP file and directory names.
        /// </remarks>
        public bool CaseInsensitive
        {
            get
            {
                return m_caseInsensitive;
            }
            set
            {
                m_caseInsensitive = value;
                Refresh();
            }
        }

        /// <summary>
        /// Name of directory.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Full path of directory.
        /// </summary>
        public string FullPath
        {
            get
            {
                return m_fullPath;
            }
        }

        /// <summary>
        /// Returns false for directory entries.
        /// </summary>
        public bool IsFile
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true for directory entries.
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets size of directory.
        /// </summary>
        public long Size
        {
            get
            {
                return m_size;
            }
            set
            {
                m_size = value;
            }
        }

        /// <summary>
        /// Gets or sets permission of directory.
        /// </summary>
        public string Permission
        {
            get
            {
                return m_permission;
            }
            set
            {
                m_permission = value;
            }
        }

        /// <summary>
        /// Gets or sets timestamp of directory.
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }

        /// <summary>
        /// Gets parent directory of directory.
        /// </summary>
        public FtpDirectory Parent
        {
            get
            {
                if (string.Compare(m_fullPath, m_session.RootDirectory.m_fullPath, m_caseInsensitive) == 0)
                    return null;

                // If we don't have a reference to parent directory, we try to derive it...
                if ((object)m_parent == null)
                {
                    CheckSessionCurrentDirectory();

                    StringBuilder parentPath = new StringBuilder();
                    var fullPath = m_session.ControlChannel.PWD();

                    if (fullPath.Substring(fullPath.Length - 1, 1) != "/")
                        fullPath += "/";

                    string[] paths = fullPath.Split('/');
                    int i;

                    for (i = 0; i < paths.Length - 2; i++)
                    {
                        if (paths[i].Length == 0)
                        {
                            parentPath.Append("/");
                        }
                        else
                        {
                            parentPath.Append(paths[i]);
                            parentPath.Append("/");
                        }
                    }

                    FtpDirectory parentDir = new FtpDirectory(m_session, m_caseInsensitive, parentPath.ToString());

                    if (string.Compare(parentDir.m_fullPath, m_session.RootDirectory.m_fullPath, m_caseInsensitive) == 0)
                        m_parent = m_session.RootDirectory;
                    else
                        m_parent = parentDir;
                }

                return m_parent;
            }
        }

        /// <summary>
        /// Gets sub directories of directory.
        /// </summary>
        public Dictionary<string, FtpDirectory>.ValueCollection SubDirectories
        {
            get
            {
                InitHashtable();
                return m_subDirectories.Values;
            }
        }

        /// <summary>
        /// Gets files of directory.
        /// </summary>
        public Dictionary<string, FtpFile>.ValueCollection Files
        {
            get
            {
                InitHashtable();
                return m_files.Values;
            }
        }

        internal FtpSessionConnected Session
        {
            get
            {
                return m_session;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Finds matching file name in directory.
        /// </summary>
        /// <param name="fileName">FileName to find in directory.</param>
        /// <returns>File reference, if found, otherwise null if file is not found.</returns>
        public FtpFile FindFile(string fileName)
        {
            InitHashtable();

            FtpFile file;

            if (m_files.TryGetValue(fileName, out file))
                return file;
            else
                return null;
        }

        /// <summary>
        /// Finds matching subdirectory name in directory.
        /// </summary>
        /// <param name="dirName">Subdirectory name to find in directory.</param>
        /// <returns>Subdirectory reference, if found, otherwise null if subdirectory is not found.</returns>
        public FtpDirectory FindSubDirectory(string dirName)
        {
            InitHashtable();

            FtpDirectory directory;

            if (m_subDirectories.TryGetValue(dirName, out directory))
                return directory;
            else
                return null;
        }

        /// <summary>
        /// Uploads local file to directory.
        /// </summary>
        /// <param name="localFile">Local file to upload.</param>
        public void PutFile(string localFile)
        {
            PutFile(localFile, null);
        }

        /// <summary>
        /// Uploads local file to directory using alternate name.
        /// </summary>
        /// <param name="localFile">Local file to upload.</param>
        /// <param name="remoteFile">Remote filename to use for upload.</param>
        public void PutFile(string localFile, string remoteFile)
        {
            CheckSessionCurrentDirectory();

            FileInfo fi = new FileInfo(localFile);

            if ((object)remoteFile == null)
                remoteFile = fi.Name;

            FtpFileTransferer transfer = new FtpFileTransferer(this, localFile, remoteFile, fi.Length, TransferDirection.Upload);

            transfer.StartTransfer();
        }

        /// <summary>
        /// Downloads remote file from directory.
        /// </summary>
        /// <param name="remoteFile">Remote filename to download.</param>
        public void GetFile(string remoteFile)
        {
            GetFile(remoteFile, remoteFile);
        }

        /// <summary>
        /// Downloads remote file from directory using alternate local filename.
        /// </summary>
        /// <param name="localFile">Local filename to use for download.</param>
        /// <param name="remoteFile">Remote filename to download.</param>
        public void GetFile(string localFile, string remoteFile)
        {
            InitHashtable();

            FtpFile file;

            if (m_files.TryGetValue(remoteFile, out file))
            {
                FtpFileTransferer transfer = new FtpFileTransferer(this, localFile, remoteFile, file.Size, TransferDirection.Download);
                transfer.StartTransfer();
            }
            else
            {
                throw new FtpFileNotFoundException(remoteFile);
            }
        }

        /// <summary>
        /// Starts asynchrnonous local file upload to directory.
        /// </summary>
        /// <param name="localFile">Local file to upload.</param>
        public void BeginPutFile(string localFile)
        {
            BeginPutFile(localFile, null);
        }

        /// <summary>
        /// Starts asynchrnonous local file upload to directory using alternate name.
        /// </summary>
        /// <param name="localFile">Local file to upload.</param>
        /// <param name="remoteFile">Remote filename to use for upload.</param>
        public void BeginPutFile(string localFile, string remoteFile)
        {
            CheckSessionCurrentDirectory();

            FileInfo fi = new FileInfo(localFile);

            if ((object)remoteFile == null)
                remoteFile = fi.Name;

            FtpFileTransferer transfer = new FtpFileTransferer(this, localFile, remoteFile, fi.Length, TransferDirection.Upload);

            transfer.StartAsyncTransfer();
        }

        /// <summary>
        /// Starts asynchronous remote file download from directory.
        /// </summary>
        /// <param name="remoteFile">Remote filename to download.</param>
        public void BeginGetFile(string remoteFile)
        {
            BeginGetFile(remoteFile, remoteFile);
        }

        /// <summary>
        /// Starts asynchronous remote file download from directory using alternate local filename.
        /// </summary>
        /// <param name="localFile">Local filename to use for download.</param>
        /// <param name="remoteFile">Remote filename to download.</param>
        public void BeginGetFile(string localFile, string remoteFile)
        {
            InitHashtable();

            FtpFile file;

            if (m_files.TryGetValue(remoteFile, out file))
            {
                FtpFileTransferer transfer = new FtpFileTransferer(this, localFile, remoteFile, file.Size, TransferDirection.Download);
                transfer.StartAsyncTransfer();
            }
            else
            {
                throw new FtpFileNotFoundException(remoteFile);
            }
        }

        /// <summary>
        /// Removes file from directory.
        /// </summary>
        /// <param name="fileName">Remote filename to remove.</param>
        public void RemoveFile(string fileName)
        {
            CheckSessionCurrentDirectory();

            m_session.ControlChannel.DELE(fileName);

            m_files.Remove(fileName);
        }

        /// <summary>
        /// Removes subdirectory from directory.
        /// </summary>
        /// <param name="dirName">Subdirectory name to remove.</param>
        public void RemoveSubDir(string dirName)
        {
            CheckSessionCurrentDirectory();

            m_session.ControlChannel.RMD(dirName);

            m_subDirectories.Remove(dirName);
        }

        /// <summary>
        /// Creates a new zero-length remote file in directory.
        /// </summary>
        /// <param name="newFileName">New remote file name.</param>
        /// <returns>File reference to new zero-length remote file.</returns>
        public FtpFile CreateFile(string newFileName)
        {
            FtpDataStream stream = CreateFileStream(newFileName);

            stream.Close();

            return m_files[newFileName];
        }

        /// <summary>
        /// Creates a new data stream for remote file in directory.
        /// </summary>
        /// <param name="newFileName">New remote file name.</param>
        /// <returns>Output data stream for new remote file.</returns>
        public FtpOutputDataStream CreateFileStream(string newFileName)
        {
            InitHashtable();

            FtpDataStream stream = m_session.ControlChannel.GetDataStream(TransferDirection.Upload);

            try
            {
                m_session.ControlChannel.STOR(newFileName);

                FtpFile newFile = new FtpFile(this, newFileName);

                m_files[newFileName] = newFile;

                return ((FtpOutputDataStream)stream);
            }
            catch
            {
                stream.Close();
                throw;
            }
        }

        /// <summary>
        /// Refreshes directory listing.
        /// </summary>
        public void Refresh()
        {
            ClearItems();
            InitHashtable();
        }

        internal void ClearItems()
        {
            m_subDirectories = null;
            m_files = null;
        }

        internal void CheckSessionCurrentDirectory()
        {
            if (m_session.CurrentDirectory.m_fullPath != m_fullPath)
                throw new InvalidOperationException(m_fullPath + " is not current directory.");
        }

        private void LoadDirectoryItems()
        {
            if (m_session.CurrentDirectory != this)
                throw new InvalidOperationException(m_name + " is not current active directory");

            Queue lineQueue = m_session.ControlChannel.List(false);
            ItemInfo info;

            foreach (string line in lineQueue)
            {
                // We allow users to inspect FTP lineQueue if desired...
                if ((object)DirectoryListLineScan != null)
                    DirectoryListLineScan(line);

                try
                {
                    info = new ItemInfo();

                    if (ParseListLine(line, info))
                    {
                        if (info.IsDirectory)
                            m_subDirectories.Add(info.Name, new FtpDirectory(m_session, this, m_caseInsensitive, info));
                        else
                            m_files.Add(info.Name, new FtpFile(this, info));
                    }
                }
                catch (FtpExceptionBase ex)
                {
                    if ((object)DirectoryScanException != null)
                        DirectoryScanException(ex);
                }
            }
        }

        private void InitHashtable()
        {
            CheckSessionCurrentDirectory();

            if ((object)m_subDirectories != null && (object)m_files != null)
                return;

            if ((object)m_subDirectories == null)
            {
                if (m_caseInsensitive)
                    m_subDirectories = new Dictionary<string, FtpDirectory>(StringComparer.CurrentCultureIgnoreCase); // New Hashtable(CaseInsensitiveComparer.Default)
                else
                    m_subDirectories = new Dictionary<string, FtpDirectory>(StringComparer.CurrentCulture); // New Hashtable
            }

            if ((object)m_files == null)
            {
                if (m_caseInsensitive)
                    m_files = new Dictionary<string, FtpFile>(StringComparer.CurrentCultureIgnoreCase); // New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
                else
                    m_files = new Dictionary<string, FtpFile>(StringComparer.CurrentCulture); // New Hashtable
            }

            LoadDirectoryItems();
        }

        private bool ParseListLine(string line, ItemInfo info)
        {
            Match m = MatchingListLine(line, ref info.TimeStamp.Style);

            if ((object)m == null)
                return false;

            info.Name = new string(m.Groups["name"].Value
                .TakeWhile(c => c != '\0')
                .ToArray());

            info.FullPath = m_fullPath + info.Name;

            string dir = m.Groups["dir"].Value;

            if (dir.Length > 0 && dir != "-")
            {
                info.IsDirectory = true;
                info.FullPath += "/";
            }
            else
            {
                info.Size = long.Parse(m.Groups["size"].Value);
            }

            info.Permission = m.Groups["permission"].Value;
            info.TimeStamp.RawValue = m.Groups["timestamp"].Value;

            return true;
        }

        private Match MatchingListLine(string line, ref FtpTimeStampParser.RawDataStyle tsStyle)
        {
            Match m = s_unixListLineStyle1.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.UnixDate;
                return m;
            }

            m = s_unixListLineStyle3.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.UnixDateTime;
                return m;
            }

            m = s_unixListLineStyle2.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.UnixDate;
                return m;
            }

            m = s_dosListLineStyle1.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.DosDateTime;
                return m;
            }

            m = s_dosListLineStyle2.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.UnixDateTime;
                return m;
            }

            tsStyle = FtpTimeStampParser.RawDataStyle.Undetermined;
            return null;
        }

        /// <summary>
        /// Determines if the two <see cref="FtpDirectory"/> objects are equal.
        /// </summary>
        /// <param name="obj">Other object to compare.</param>
        /// <returns><c>true</c> if both objects are equal.</returns>
        public override bool Equals(object obj)
        {
            FtpDirectory other = obj as FtpDirectory;

            if ((object)other != null)
                return (CompareTo(other) == 0);

            return false;
        }

        /// <summary>
        /// Generates hash code for this <see cref="FtpDirectory"/>.
        /// </summary>
        /// <returns>An <see cref="Int32"/> value as the result.</returns>
        public override int GetHashCode()
        {
            return m_name.GetHashCode();
        }

        int IComparable<FtpDirectory>.CompareTo(FtpDirectory other)
        {
            // Directories are sorted by name
            return string.Compare(m_name, other.Name, m_caseInsensitive);
        }

        int IComparable<IFtpFile>.CompareTo(IFtpFile other)
        {
            // Directories are sorted by name
            return string.Compare(m_name, other.Name, m_caseInsensitive);
        }

        /// <summary>
        /// Compares directory or file to another.
        /// </summary>
        /// <param name="obj">An <see cref="Object"/> to compare against.</param>
        /// <returns>An <see cref="Int32"/> value representing the result. 1 - obj is greater than, 0 - obj is equal to, -1 - obj is less than.</returns>
        public int CompareTo(object obj)
        {
            if (Equals(obj, null))
            {
                return 1;
            }
            else
            {
                IFtpFile file = obj as IFtpFile;
                if ((object)file == null)
                    return 1;
                else
                    return ((IComparable<IFtpFile>)this).CompareTo(file);
            }
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator ==(FtpDirectory value1, FtpDirectory value2)
        {
            if (Equals(value1, null))
                return Equals(value2, null);
            else
                return (value1.CompareTo(value2) == 0);
        }

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator !=(FtpDirectory value1, FtpDirectory value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Returns true if left value is less than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator <(FtpDirectory value1, FtpDirectory value2)
        {
            if (Equals(value1, null))
                return Equals(value2, null);
            else
                return (value1.CompareTo(value2) < 0);
        }

        /// <summary>
        /// Returns true if left value is greater than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator >(FtpDirectory value1, FtpDirectory value2)
        {
            return !(value1 <= value2);
        }

        /// <summary>
        /// Returns true if left value is less or equal to than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator <=(FtpDirectory value1, FtpDirectory value2)
        {
            if (Equals(value1, null))
                return Equals(value2, null);
            else
                return (value1.CompareTo(value2) <= 0);
        }

        /// <summary>
        /// Returns true if left value is greater than or equal to right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator >=(FtpDirectory value1, FtpDirectory value2)
        {
            return !(value1 < value2);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex s_unixListLineStyle1 = new Regex("(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)");
        private static readonly Regex s_unixListLineStyle2 = new Regex("(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)");
        private static readonly Regex s_unixListLineStyle3 = new Regex("(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d+:\\d+)\\s+(?<name>.+)");
        private static readonly Regex s_dosListLineStyle1 = new Regex("(?<timestamp>\\d{2}\\-\\d{2}\\-\\d{2}\\s+\\d{2}:\\d{2}[Aa|Pp][mM])\\s+(?<dir>\\<\\w+\\>){0,1}(?<size>\\d+){0,1}\\s+(?<name>.+)"); // IIS FTP Service
        private static readonly Regex s_dosListLineStyle2 = new Regex("(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d+:\\d+)\\s+(?<name>.+)"); // IIS FTP Service in Unix Mode

        #endregion
    }
}