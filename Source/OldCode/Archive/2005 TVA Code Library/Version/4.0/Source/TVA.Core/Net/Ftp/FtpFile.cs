//*******************************************************************************************************
//  FtpFile.cs
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

namespace TVA.Net.Ftp
{
    /// <summary>
    /// Represents a FTP file.
    /// </summary>
    public class FtpFile : IFtpFile, IComparable<FtpFile>
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

        internal FtpFile(FtpDirectory parent, FtpDirectory.ItemInfo info)
        {
            m_parent = parent;
            m_name = info.Name;
            m_size = info.Size;
            m_permission = info.Permission;
            m_timestamp = info.TimeStamp.Value;
        }

        internal FtpFile(FtpDirectory parent, string name)
        {
            m_parent = parent;
            m_name = name;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Name of file.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Full path of file.
        /// </summary>
        public string FullPath
        {
            get
            {
                return m_parent.FullPath + m_name;
            }
        }

        /// <summary>
        /// Returns true for file entries.
        /// </summary>
        public bool IsFile
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns false for directory entries.
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets size of file.
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
        /// Gets or sets permission of file.
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
        /// Gets or sets timestamp of file.
        /// </summary>
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

        /// <summary>
        /// Gets parent directory of file.
        /// </summary>
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

        /// <summary>
        /// Gets FTP input stream for file.
        /// </summary>
        /// <returns>FTP input stream for file.</returns>
        public FtpInputDataStream GetInputStream()
        {
            return ((FtpInputDataStream)(GetStream(0, TransferDirection.Download)));
        }

        /// <summary>
        /// Gets FTP output stream for file.
        /// </summary>
        /// <returns>FTP output stream for file.</returns>
        public FtpOutputDataStream GetOutputStream()
        {
            return ((FtpOutputDataStream)(GetStream(0, TransferDirection.Upload)));
        }

        /// <summary>
        /// Gets FTP input stream for file at given offset.
        /// </summary>
        /// <param name="offset">Offset into stream to start.</param>
        /// <returns>FTP input stream for file.</returns>
        public FtpInputDataStream GetInputStream(long offset)
        {
            return ((FtpInputDataStream)(GetStream(offset, TransferDirection.Download)));
        }

        /// <summary>
        /// Gets FTP output stream for file at given offset.
        /// </summary>
        /// <param name="offset">Offset into stream to start.</param>
        /// <returns>FTP output stream for file.</returns>
        public FtpOutputDataStream GetOutputStream(long offset)
        {
            return ((FtpOutputDataStream)(GetStream(offset, TransferDirection.Upload)));
        }

        private FtpDataStream GetStream(long offset, TransferDirection dir)
        {
            m_parent.CheckSessionCurrentDirectory();

            FtpSessionConnected Session = m_parent.Session;

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
        
        int IComparable<FtpFile>.CompareTo(FtpFile other)
        {
            // Files are sorted by name
            return string.Compare(m_name, other.Name, m_parent.CaseInsensitive);
        }

        int IComparable<IFtpFile>.CompareTo(IFtpFile other)
        {
            // Files are sorted by name
            return string.Compare(m_name, other.Name, m_parent.CaseInsensitive);
        }

        /// <summary>
        /// Compares directory or file to another.
        /// </summary>
        public int CompareTo(object obj)
        {
            IFtpFile file = obj as IFtpFile;

            if (file != null)
                return CompareTo(file);
            else
                throw new ArgumentException("File can only be compared to other Files or Directories");
        }

        #endregion
    }
}