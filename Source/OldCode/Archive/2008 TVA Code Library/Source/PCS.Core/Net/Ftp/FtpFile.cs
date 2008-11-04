//*******************************************************************************************************
//  File.cs
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

namespace PCS.Net.Ftp
{
    public class File : IFile, IComparable<File>
    {
        #region [ Members ]

        // Fields
        private FtpDirectory m_parent;
        private string m_name;
        private long m_size;
        private string m_permission;
        private DateTime m_timestamp;

        #endregion

        #region [ Constructors ]

        internal File(FtpDirectory parent, FtpDirectory.ItemInfo info)
        {
            m_parent = parent;
            m_name = info.Name;
            m_size = info.Size;
            m_permission = info.Permission;
            m_timestamp = info.TimeStamp.Value;
        }

        internal File(FtpDirectory parent, string name)
        {
            m_parent = parent;
            m_name = name;
        }

        #endregion

        #region [ Properties ]

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
                return m_parent.FullPath + m_name;
            }
        }

        public bool IsFile
        {
            get
            {
                return true;
            }
        }

        public bool IsDirectory
        {
            get
            {
                return false;
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

        public FtpDirectory Parent
        {
            get
            {
                m_parent.CheckSessionCurrentDirectory();
                return m_parent;
            }
        }

        #endregion

        #region [ Methods ]

        public InputDataStream GetInputStream()
        {
            return ((InputDataStream)(GetStream(0, TransferDirection.Download)));
        }

        public OutputDataStream GetOutputStream()
        {
            return ((OutputDataStream)(GetStream(0, TransferDirection.Upload)));
        }

        public InputDataStream GetInputStream(long offset)
        {
            return ((InputDataStream)(GetStream(offset, TransferDirection.Download)));
        }

        public OutputDataStream GetOutputStream(long offset)
        {
            return ((OutputDataStream)(GetStream(offset, TransferDirection.Upload)));
        }

        private FtpDataStream GetStream(long offset, TransferDirection dir)
        {
            m_parent.CheckSessionCurrentDirectory();

            SessionConnected Session = m_parent.Session;

            if (offset != 0)
                Session.ControlChannel.REST(offset);

            FtpDataStream stream = Session.ControlChannel.GetPassiveDataStream(dir);

            try
            {
                if (dir == TransferDirection.Download)
                    Session.ControlChannel.RETR(m_name);
                else
                    Session.ControlChannel.STOR(m_name);
            }
            catch
            {
                stream.Close();
                throw;
            }

            return stream;
        }
        
        int IComparable<File>.CompareTo(File other)
        {
            // Files are sorted by name
            return string.Compare(m_name, other.Name, m_parent.CaseInsensitive);
        }

        int IComparable<IFile>.CompareTo(IFile other)
        {
            // Files are sorted by name
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
    }
}