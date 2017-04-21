//******************************************************************************************************
//  FtpFile.cs - Gbtc
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

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Represents a FTP file.
    /// </summary>
    public class FtpFile : IFtpFile, IComparable<FtpFile>
    {
        #region [ Members ]

        // Fields
        private readonly FtpDirectory m_parent;
        private readonly string m_name;
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
        /// Downloads remote file.
        /// </summary>
        public void Get()
        {
            Parent.GetFile(Name);
        }

        /// <summary>
        /// Downloads remote file using alternate local filename.
        /// </summary>
        /// <param name="localFile">Local filename to use for download.</param>
        public void Get(string localFile)
        {
            Parent.GetFile(localFile, Name);
        }

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

            FtpDataStream stream = Session.ControlChannel.GetDataStream(dir);

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

        /// <summary>
        /// Determines if the two <see cref="FtpFile"/> objects are equal.
        /// </summary>
        /// <param name="obj">Other object to compare.</param>
        /// <returns><c>true</c> if both objects are equal.</returns>
        public override bool Equals(object obj)
        {
            FtpFile other = obj as FtpFile;

            if ((object)other != null)
                return (CompareTo(other) == 0);

            return false;
        }

        /// <summary>
        /// Generates hash code for this <see cref="FtpFile"/>.
        /// </summary>
        /// <returns>An <see cref="Int32"/> representing the hash code.</returns>
        public override int GetHashCode()
        {
            return m_name.GetHashCode();
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
        /// <param name="obj">An <see cref="Object"/> to compare against.</param>
        /// <returns>An <see cref="Int32"/> that represents the result of the comparison. 1 - object is greater than, 0 - object is equal to, -1 - object is less than.</returns>
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
                    return CompareTo(file);
            }
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares the two values for equality.
        /// </summary>
        /// <param name="value1">A <see cref="FtpFile"/> as the left hand operand.</param>
        /// <param name="value2">A <see cref="FtpFile"/> as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result of the comparison.</returns>
        public static bool operator ==(FtpFile value1, FtpFile value2)
        {
            if (Equals(value1, null))
                return Equals(value2, null);
            else
                return (value1.CompareTo(value2) == 0);
        }

        /// <summary>
        /// Compares the two values for inequality.
        /// </summary>
        /// <param name="value1">A <see cref="FtpFile"/> as the left hand operand.</param>
        /// <param name="value2">A <see cref="FtpFile"/> as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result of the comparison.</returns>
        public static bool operator !=(FtpFile value1, FtpFile value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Returns true if left value is less than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpFile"/> as the left hand operand.</param>
        /// <param name="value2">A <see cref="FtpFile"/> as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result of the comparison.</returns>
        public static bool operator <(FtpFile value1, FtpFile value2)
        {
            if (Equals(value1, null))
                return Equals(value2, null);
            else
                return (value1.CompareTo(value2) < 0);
        }

        /// <summary>
        /// Returns true if left value is greater than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpFile"/> as the left hand operand.</param>
        /// <param name="value2">A <see cref="FtpFile"/> as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result of the comparison.</returns>
        public static bool operator >(FtpFile value1, FtpFile value2)
        {
            return !(value1 <= value2);
        }

        /// <summary>
        /// Returns true if left value is less or equal to than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpFile"/> as the left hand operand.</param>
        /// <param name="value2">A <see cref="FtpFile"/> as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result of the comparison.</returns>
        public static bool operator <=(FtpFile value1, FtpFile value2)
        {
            if (Equals(value1, null))
                return Equals(value2, null);
            else
                return (value1.CompareTo(value2) <= 0);
        }

        /// <summary>
        /// Returns true if left value is greater than or equal to right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpFile"/> as the left hand operand.</param>
        /// <param name="value2">A <see cref="FtpFile"/> as the right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result of the comparison.</returns>
        public static bool operator >=(FtpFile value1, FtpFile value2)
        {
            return !(value1 < value2);
        }

        #endregion
    }
}