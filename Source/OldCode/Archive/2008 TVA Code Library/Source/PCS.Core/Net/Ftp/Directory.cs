//*******************************************************************************************************
//  Directory.cs
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
//  05/22/2003 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PCS.Net.Ftp
{
    public class Directory : IFile, IComparable<Directory>
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
            public TimeStampParser TimeStamp = new TimeStampParser();
        }

        // Events
        public event Action<string> DirectoryListLineScan;
        public event Action<ExceptionBase> DirectoryScanException;

        // Fields
        private SessionConnected m_session;
        private Directory m_parent;
        private string m_name;
        private string m_fullPath;
        private Dictionary<string, Directory> m_subDirectories;
        private Dictionary<string, File> m_files;
        private bool m_caseInsensitive;
        private long m_size;
        private string m_permission;
        private DateTime m_timestamp;

        #endregion

        #region [ Constructors ]

        internal Directory(SessionConnected s, bool caseInsensitive, string fullPath)
        {
            m_session = s;
            m_parent = null;
            m_caseInsensitive = caseInsensitive;

            if (fullPath.Substring(fullPath.Length - 1, 1) == "/")
                fullPath = fullPath.Substring(0, fullPath.Length - 1);

            if (fullPath.Length == 0)
            {
                m_name = "/";
            }
            else
            {
                string[] directories = fullPath.Split('/');
                m_name = directories[directories.Length - 1];
                m_fullPath = fullPath + "/";
            }
        }

        internal Directory(SessionConnected s, Directory parent, bool caseInsensitive, ItemInfo info)
        {
            m_session = s;
            m_parent = parent;
            m_caseInsensitive = caseInsensitive;

            if (info.Name.Length > 0)
            {
                m_name = info.Name;

                if (parent == null)
                    m_fullPath = m_name + "/";
                else
                    m_fullPath = parent.FullPath + m_name + "/";
            }
            else
            {
                m_name = "/";
            }

            m_size = info.Size;
            m_permission = info.Permission;
            m_timestamp = info.TimeStamp.Value;
        }

        #endregion

        #region [ Properties ]

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

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string FullPath
        {
            get
            {
                return m_fullPath;
            }
        }

        public bool IsFile
        {
            get
            {
                return false;
            }
        }

        public bool IsDirectory
        {
            get
            {
                return true;
            }
        }

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

        public DateTime TimeStamp
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

        public Directory Parent
        {
            get
            {
                if (string.Compare(m_fullPath, m_session.RootDirectory.m_fullPath, m_caseInsensitive) == 0)
                    return null;

                // If we don't have a reference to parent directory, we try to derive it...
                if (m_parent == null)
                {
                    CheckSessionCurrentDirectory();

                    StringBuilder parentPath = new StringBuilder();
                    var fullPath = m_session.ControlChannel.PWD();

                    if (fullPath.Substring(fullPath.Length - 1, 1) != "/")
                        fullPath += "/";

                    string[] paths = fullPath.Split('/');
                    int i;

                    for (i = 0; i <= paths.Length - 2; i++)
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

                    Directory parentDir = new Directory(m_session, m_caseInsensitive, parentPath.ToString());

                    if (string.Compare(parentDir.m_fullPath, m_session.RootDirectory.m_fullPath, m_caseInsensitive) == 0)
                        m_parent = m_session.RootDirectory;
                    else
                        m_parent = parentDir;
                }

                return m_parent;
            }
        }

        public Dictionary<string, Directory>.ValueCollection SubDirectories
        {
            get
            {
                InitHashtable();
                return m_subDirectories.Values;
            }
        }

        public Dictionary<string, File>.ValueCollection Files
        {
            get
            {
                InitHashtable();
                return m_files.Values;
            }
        }

        internal SessionConnected Session
        {
            get
            {
                return m_session;
            }
        }

        #endregion

        #region [ Methods ]

        public File FindFile(string fileName)
        {
            InitHashtable();

            File file;

            if (m_files.TryGetValue(fileName, out file))
                return file;
            else
                return null;
        }

        public Directory FindSubDirectory(string dirName)
        {
            InitHashtable();

            Directory directory;

            if (m_subDirectories.TryGetValue(dirName, out directory))
                return directory;
            else
                return null;
        }

        public void PutFile(string localFile)
        {
            PutFile(localFile, null);
        }

        public void PutFile(string localFile, string remoteFile)
        {
            CheckSessionCurrentDirectory();

            FileInfo fi = new FileInfo(localFile);

            if (remoteFile == null)
                remoteFile = fi.Name;

            FileTransferer transfer = new FileTransferer(this, localFile, remoteFile, fi.Length, TransferDirection.Upload);

            transfer.StartTransfer();
        }

        public void GetFile(string remoteFile)
        {
            GetFile(remoteFile, remoteFile);
        }

        public void GetFile(string localFile, string remoteFile)
        {
            InitHashtable();

            File file;

            if (m_files.TryGetValue(remoteFile, out file))
            {
                FileTransferer transfer = new FileTransferer(this, localFile, remoteFile, file.Size, TransferDirection.Download);
                transfer.StartTransfer();
            }
            else
            {
                throw new FileNotFoundException(remoteFile);
            }
        }

        public void BeginPutFile(string localFile)
        {
            BeginPutFile(localFile, null);
        }

        public void BeginPutFile(string localFile, string remoteFile)
        {
            CheckSessionCurrentDirectory();

            FileInfo fi = new FileInfo(localFile);

            if (remoteFile == null)
                remoteFile = fi.Name;

            FileTransferer transfer = new FileTransferer(this, localFile, remoteFile, fi.Length, TransferDirection.Upload);

            transfer.StartAsyncTransfer();
        }

        public void BeginGetFile(string remoteFile)
        {
            BeginGetFile(remoteFile, remoteFile);
        }

        public void BeginGetFile(string localFile, string remoteFile)
        {
            InitHashtable();

            File file;

            if (m_files.TryGetValue(remoteFile, out file))
            {
                FileTransferer transfer = new FileTransferer(this, localFile, remoteFile, file.Size, TransferDirection.Download);
                transfer.StartAsyncTransfer();
            }
            else
            {
                throw new FileNotFoundException(remoteFile);
            }
        }

        public void RemoveFile(string fileName)
        {
            CheckSessionCurrentDirectory();

            m_session.ControlChannel.DELE(fileName);

            m_files.Remove(fileName);
        }

        public void RemoveSubDir(string dirName)
        {
            CheckSessionCurrentDirectory();

            m_session.ControlChannel.RMD(dirName);

            m_subDirectories.Remove(dirName);
        }

        public File CreateFile(string newFileName)
        {
            DataStream stream = CreateFileStream(newFileName);

            stream.Close();

            return m_files[newFileName];
        }

        public OutputDataStream CreateFileStream(string newFileName)
        {
            InitHashtable();

            DataStream stream = m_session.ControlChannel.GetPassiveDataStream(TransferDirection.Upload);

            try
            {
                m_session.ControlChannel.STOR(newFileName);

                File newFile = new File(this, newFileName);

                m_files[newFileName] = newFile;

                return ((OutputDataStream)stream);
            }
            catch
            {
                stream.Close();
                throw;
            }
        }

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
                if (DirectoryListLineScan != null)
                    DirectoryListLineScan(line);

                try
                {
                    info = new ItemInfo();

                    if (ParseListLine(line, info))
                    {
                        if (info.IsDirectory)
                            m_subDirectories.Add(info.Name, new Directory(m_session, this, m_caseInsensitive, info));
                        else
                            m_files.Add(info.Name, new File(this, info));
                    }
                }
                catch (ExceptionBase ex)
                {
                    if (DirectoryScanException != null)
                        DirectoryScanException(ex);
                }
            }
        }

        private void InitHashtable()
        {
            CheckSessionCurrentDirectory();

            if (m_subDirectories != null && m_files != null)
                return;

            if (m_subDirectories == null)
            {
                if (m_caseInsensitive)
                    m_subDirectories = new Dictionary<string, Directory>(StringComparer.CurrentCultureIgnoreCase); // New Hashtable(CaseInsensitiveComparer.Default)
                else
                    m_subDirectories = new Dictionary<string, Directory>(StringComparer.CurrentCulture); // New Hashtable
            }

            if (m_files == null)
            {
                if (m_caseInsensitive)
                    m_files = new Dictionary<string, File>(StringComparer.CurrentCultureIgnoreCase); // New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
                else
                    m_files = new Dictionary<string, File>(StringComparer.CurrentCulture); // New Hashtable
            }

            LoadDirectoryItems();
        }

        private bool ParseListLine(string line, ItemInfo info)
        {
            Match m = MatchingListLine(line, ref info.TimeStamp.Style);

            if (m == null)
                return false;

            info.Name = m.Groups["name"].Value;
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

        private Match MatchingListLine(string line, ref TimeStampParser.RawDataStyle tsStyle)
        {
            Match m = m_UnixListLineStyle1.Match(line);

            if (m.Success)
            {
                tsStyle = TimeStampParser.RawDataStyle.UnixDate;
                return m;
            }

            m = m_UnixListLineStyle3.Match(line);

            if (m.Success)
            {
                tsStyle = TimeStampParser.RawDataStyle.UnixDateTime;
                return m;
            }

            m = m_UnixListLineStyle2.Match(line);

            if (m.Success)
            {
                tsStyle = TimeStampParser.RawDataStyle.UnixDate;
                return m;
            }

            m = m_DosListLineStyle1.Match(line);

            if (m.Success)
            {
                tsStyle = TimeStampParser.RawDataStyle.DosDateTime;
                return m;
            }

            m = m_DosListLineStyle2.Match(line);

            if (m.Success)
            {
                tsStyle = TimeStampParser.RawDataStyle.UnixDateTime;
                return m;
            }

            tsStyle = TimeStampParser.RawDataStyle.Undetermined;
            return null;
        }

        int IComparable<Directory>.CompareTo(Directory other)
        {
            // Directories are sorted by name
            return string.Compare(m_name, other.Name, m_parent.CaseInsensitive);
        }

        int IComparable<IFile>.CompareTo(IFile other)
        {
            // Directories are sorted by name
            return string.Compare(m_name, other.Name, m_parent.CaseInsensitive);
        }

        public int CompareTo(object obj)
        {
            IFile file = obj as IFile;

            if (file != null)
                return CompareTo(file);
            else
                throw new ArgumentException("File can only be compared to other Files or Directories");
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static Regex m_UnixListLineStyle1 = new Regex("(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)");
        private static Regex m_UnixListLineStyle2 = new Regex("(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)");
        private static Regex m_UnixListLineStyle3 = new Regex("(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d+:\\d+)\\s+(?<name>.+)");
        private static Regex m_DosListLineStyle1 = new Regex("(?<timestamp>\\d{2}\\-\\d{2}\\-\\d{2}\\s+\\d{2}:\\d{2}[Aa|Pp][mM])\\s+(?<dir>\\<\\w+\\>){0,1}(?<size>\\d+){0,1}\\s+(?<name>.+)"); // IIS FTP Service
        private static Regex m_DosListLineStyle2 = new Regex("(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d+:\\d+)\\s+(?<name>.+)"); // IIS FTP Service in Unix Mode

        #endregion
    }
}