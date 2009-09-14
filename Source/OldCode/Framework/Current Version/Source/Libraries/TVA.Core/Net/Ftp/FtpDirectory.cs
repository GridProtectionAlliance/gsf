//*******************************************************************************************************
//  FtpDirectory.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/22/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
//  9/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TVA.Net.Ftp
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
        public event Action<string> DirectoryListLineScan;

        /// <summary>
        /// Raised when there is an exception scanning a directory.
        /// </summary>
        public event Action<FtpExceptionBase> DirectoryScanException;

        // Fields
        private FtpSessionConnected m_session;
        private FtpDirectory m_parent;
        private string m_name;
        private string m_fullPath;
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
                m_name = "/";
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
        /// Gets parent directory of directory.
        /// </summary>
        public FtpDirectory Parent
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

                    for (i = 0; i < paths.Length - 1; i++)
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

            if (remoteFile == null)
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

            if (remoteFile == null)
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

            FtpDataStream stream = m_session.ControlChannel.GetPassiveDataStream(TransferDirection.Upload);

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
                if (DirectoryListLineScan != null)
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
                    m_subDirectories = new Dictionary<string, FtpDirectory>(StringComparer.CurrentCultureIgnoreCase); // New Hashtable(CaseInsensitiveComparer.Default)
                else
                    m_subDirectories = new Dictionary<string, FtpDirectory>(StringComparer.CurrentCulture); // New Hashtable
            }

            if (m_files == null)
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

        private Match MatchingListLine(string line, ref FtpTimeStampParser.RawDataStyle tsStyle)
        {
            Match m = m_UnixListLineStyle1.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.UnixDate;
                return m;
            }

            m = m_UnixListLineStyle3.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.UnixDateTime;
                return m;
            }

            m = m_UnixListLineStyle2.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.UnixDate;
                return m;
            }

            m = m_DosListLineStyle1.Match(line);

            if (m.Success)
            {
                tsStyle = FtpTimeStampParser.RawDataStyle.DosDateTime;
                return m;
            }

            m = m_DosListLineStyle2.Match(line);

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

            if (other != null)
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
            return string.Compare(m_name, other.Name, m_parent.CaseInsensitive);
        }

        int IComparable<IFtpFile>.CompareTo(IFtpFile other)
        {
            // Directories are sorted by name
            return string.Compare(m_name, other.Name, m_parent.CaseInsensitive);
        }

        /// <summary>
        /// Compares directory or file to another.
        /// </summary>
        /// <param name="obj">An <see cref="Object"/> to compare against.</param>
        /// <returns>An <see cref="Int32"/> value representing the result. 1 - obj is greater than, 0 - obj is equal to, -1 - obj is less than.</returns>
        public int CompareTo(object obj)
        {
            IFtpFile file = obj as IFtpFile;

            if (file != null)
                return CompareTo(file);
            else
                throw new ArgumentException("File can only be compared to other Files or Directories");
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
            return (value1.CompareTo(value2) != 0);
        }

        /// <summary>
        /// Returns true if left value is less than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator <(FtpDirectory value1, FtpDirectory value2)
        {
            return (value1.CompareTo(value2) < 0);
        }

        /// <summary>
        /// Returns true if left value is less or equal to than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator <=(FtpDirectory value1, FtpDirectory value2)
        {
            return (value1.CompareTo(value2) <= 0);
        }

        /// <summary>
        /// Returns true if left value is greater than right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator >(FtpDirectory value1, FtpDirectory value2)
        {
            return (value1.CompareTo(value2) > 0);
        }

        /// <summary>
        /// Returns true if left value is greater than or equal to right value.
        /// </summary>
        /// <param name="value1">A <see cref="FtpDirectory"/> left hand operand.</param>
        /// <param name="value2">A <see cref="FtpDirectory"/> right hand operand.</param>
        /// <returns>A <see cref="Boolean"/> value indicating the result.</returns>
        public static bool operator >=(FtpDirectory value1, FtpDirectory value2)
        {
            return (value1.CompareTo(value2) >= 0);
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