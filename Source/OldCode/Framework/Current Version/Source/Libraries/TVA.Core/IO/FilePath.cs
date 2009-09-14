//*******************************************************************************************************
//  FilePath.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/05/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.FilePath).
//  08/22/2007 - Darrell Zuercher
//       Edited code comments.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/24/2008 - Pinal C. Patel
//       Edited code comments.
//  12/17/2008 - F. Russell Robertson
//       Fixed bug in GetFilePatternRegularExpression().
//  06/30/2009 - Pinal C. Patel
//       Removed FilePathHasFileName() since the result was error prone.
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TVA.Interop;
using TVA.Reflection;

namespace TVA.IO
{
    /// <summary>
    /// Contains File and Path manipulation methods.
    /// </summary>
    public static class FilePath
    {
        #region [ Members ]

        // Nested Types

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }

        // Constants

        private const int RESOURCETYPE_DISK = 0x1;

        // Delegates

        [DllImport("mpr.dll", EntryPoint = "WNetAddConnection2W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, int dwFlags);

        [DllImport("mpr.dll", EntryPoint = "WNetCancelConnection2W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, [MarshalAs(UnmanagedType.Bool)] bool fForce);

        // Fields

        private static string m_fileNameCharPattern;

        #endregion

        #region [ Constructor ]

        static FilePath()
        {
            StringBuilder pattern = new StringBuilder();

            // Defines a regular expression pattern for a valid file name character. We do this by
            // allowing any characters except those that would not be valid as part of a filename.
            // This essentially builds the "?" wildcard pattern match.
            pattern.Append("[^");
            pattern.Append(Path.DirectorySeparatorChar.RegexEncode());
            pattern.Append(Path.AltDirectorySeparatorChar.RegexEncode());
            pattern.Append(Path.PathSeparator.RegexEncode());
            pattern.Append(Path.VolumeSeparatorChar.RegexEncode());

            foreach (char c in Path.GetInvalidPathChars())
            {
                pattern.Append(c.RegexEncode());
            }

            pattern.Append("]");
            m_fileNameCharPattern = pattern.ToString();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Connects to a network share with the specified user's credentials.
        /// </summary>
        /// <param name="sharename">UNC share name to connect to.</param>
        /// <param name="username">Username to use for connection.</param>
        /// <param name="password">Password to use for connection.</param>
        /// <param name="domain">Domain name to use for connetion. Specify the computer name for local system accounts.</param>
        public static void ConnectToNetworkShare(string sharename, string username, string password, string domain)
        {
            NETRESOURCE resource = new NETRESOURCE();
            int result;

            resource.dwType = RESOURCETYPE_DISK;
            resource.lpRemoteName = sharename;

            if (domain.Length > 0)
                username = domain + "\\" + username;

            result = WNetAddConnection2(ref resource, password, username, 0);
            if (result != 0)
                throw new InvalidOperationException("Failed to connect to network share \"" + sharename + "\" as user " + username + ". " + WindowsApi.GetErrorMessage(result));
        }

        /// <summary>
        /// Disconnects the specified network share.
        /// </summary>
        /// <param name="sharename">UNC share name to disconnect from.</param>
        public static void DisconnectFromNetworkShare(string sharename)
        {
            DisconnectFromNetworkShare(sharename, true);
        }

        /// <summary>
        /// Disconnects the specified network share.
        /// </summary>
        /// <param name="sharename">UNC share name to disconnect from.</param>
        /// <param name="force">true to force a disconnect; otherwise false.</param>
        public static void DisconnectFromNetworkShare(string sharename, bool force)
        {
            int result = WNetCancelConnection2(sharename, 0, force);
            if (result != 0)
                throw new InvalidOperationException("Failed to disconnect from network share \"" + sharename + "\".  " + WindowsApi.GetErrorMessage(result));
        }

        /// <summary>
        /// Determines whether the specified file name matches any of the given file specs (wildcards are defined as '*' or '?' characters).
        /// </summary>
        /// <param name="fileSpecs">The file specs used for matching the specified file name.</param>
        /// <param name="fileName">The file name to be tested against the specified file specs for a match.</param>
        /// <param name="ignoreCase">true to specify a case-insensitive match; otherwise false.</param>
        /// <returns>true if the specified file name matches any of the given file specs; otherwise false.</returns>
        public static bool IsFilePatternMatch(string[] fileSpecs, string fileName, bool ignoreCase)
        {
            bool found = false;

            foreach (string fileSpec in fileSpecs)
            {
                if (IsFilePatternMatch(fileSpec, fileName, ignoreCase))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Determines whether the specified file name matches the given file spec (wildcards are defined as '*' or '?' characters).
        /// </summary>
        /// <param name="fileSpec">The file spec used for matching the specified file name.</param>
        /// <param name="fileName">The file name to be tested against the specified file spec for a match.</param>
        /// <param name="ignoreCase">true to specify a case-insensitive match; otherwise false.</param>
        /// <returns>true if the specified file name matches the given file spec; otherwise false.</returns>
        public static bool IsFilePatternMatch(string fileSpec, string fileName, bool ignoreCase)
        {
            return (new Regex(GetFilePatternRegularExpression(fileSpec), (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))).IsMatch(fileName);
        }

        /// <summary>
        /// Gets the file name and extension from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path from which the file name and extension is to be obtained.</param>
        /// <returns>File name and extension if the file path has it; otherwise empty string.</returns>
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(RemovePathSuffix(filePath));
        }

        /// <summary>
        /// Gets the extension from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path from which the extension is to be obtained.</param>
        /// <returns>File extension.</returns>
        public static string GetExtension(string filePath)
        {
            return Path.GetExtension(RemovePathSuffix(filePath));
        }

        /// <summary>
        /// Gets the file name without extension from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path from which the file name is to be obtained.</param>
        /// <returns>File name without the extension if the file path has it; otherwise empty string.</returns>
        public static string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(RemovePathSuffix(filePath));
        }

        /// <summary>
        /// Gets the size of the specified file.
        /// </summary>
        /// <param name="fileName">Name of file whose size is to be retrieved.</param>
        /// <returns>The size of the specified file.</returns>
        public static long GetFileLength(string fileName)
        {
            try
            {
                return (new FileInfo(fileName)).Length;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets a list of files under the specified path. Search wildcard pattern (c:\Data\*.dat) can be used for 
        /// including only the files matching the pattern or path wildcard pattern (c:\Data\*\*.dat) to indicate the 
        /// inclusion of files under all subdirectories in the list.
        /// </summary>
        /// <param name="path">The path for which a list of files is to be returned.</param>
        /// <returns>A list of files under the given path.</returns>
        public static string[] GetFileList(string path)
        {
            string directory = GetDirectoryName(path);
            string filePattern = GetFileName(path);
            SearchOption options = SearchOption.TopDirectoryOnly;

            if (string.IsNullOrEmpty(filePattern))
            {
                // No wildcard pattern was specified, so get a listing of all files.
                filePattern = "*.*";
            }

            if (GetLastDirectoryName(directory) == "*")
            {
                // Path wildcard pattern is used to specify the option to include subdirectories.
                options = SearchOption.AllDirectories;
                directory = directory.Remove(directory.LastIndexOf("*"));
            }

            return Directory.GetFiles(directory, filePattern, options);
        }

        /// <summary>
        /// Gets a regular expression pattern that simulates wildcard matching for filenames (wildcards are defined as '*' or '?' characters).
        /// </summary>
        /// <param name="fileSpec">The file spec for which the regular expression pattern if to be generated.</param>
        /// <returns>Regular expression pattern that simulates wildcard matching for filenames.</returns>
        public static string GetFilePatternRegularExpression(string fileSpec)
        {
            // Replaces wildcard file patterns with their equivalent regular expression.
            fileSpec = fileSpec.Replace("\\", "\\u005C"); // Backslash in Regex means special sequence. Here, we really want a backslash.
            fileSpec = fileSpec.Replace(".", "\\u002E"); // Dot in Regex means any character. Here, we really want a dot.
            fileSpec = fileSpec.Replace("?", m_fileNameCharPattern);
            fileSpec = fileSpec.Replace("*", "(" + m_fileNameCharPattern + ")*");

            return "^" + fileSpec + "$";
        }

        /// <summary>
        /// Gets the directory information from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path from which the directory information is to be obtained.</param>
        /// <returns>Directory information.</returns>
        public static string GetDirectoryName(string filePath)
        {
            return AddPathSuffix(Path.GetDirectoryName(RemovePathSuffix(filePath)));
        }

        /// <summary>
        /// Gets the last directory name from a file path.
        /// </summary>
        /// <param name="filePath">The file path from where the last directory name is to be retrieved.</param>
        /// <returns>The last directory name from a file path.</returns>
        /// <remarks>
        /// <see cref="GetLastDirectoryName(string)"/> would return sub2 from c:\windows\sub2\filename.ext.
        /// </remarks>
        public static string GetLastDirectoryName(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                int index;
                char[] dirVolChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };

                // Remove file name and trailing directory seperator character from the file path.
                filePath = RemovePathSuffix(GetDirectoryName(filePath));
                // Keep going through the file path until all directory seperator characters are removed.
                while ((index = filePath.IndexOfAny(dirVolChars)) > -1)
                {
                    filePath = filePath.Substring(index + 1);
                }

                return filePath;
            }
            else
            {
                throw new ArgumentNullException("filePath");
            }
        }

        /// <summary>
        /// Gets the absolute file path for the specified file name or relative file path.
        /// </summary>
        /// <param name="filePath">File name or relative file path.</param>
        /// <returns>Absolute file path for the specified file name or relative file path.</returns>
        public static string GetAbsolutePath(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
            {
                // The specified path is a relative one since it is not rooted.
                switch (Common.GetApplicationType())
                {
                    // Prepends the application's root to the file path.
                    case ApplicationType.Web:
                        filePath = Path.Combine(System.Web.HttpContext.Current.Request.MapPath("~/"), filePath);
                        break;
                    case ApplicationType.WindowsCui:
                    case ApplicationType.WindowsGui:
                        filePath = Path.Combine(GetDirectoryName(AssemblyInfo.EntryAssembly.Location), filePath);
                        break;
                }
            }

            return RemovePathSuffix(filePath);
        }

        /// <summary>
        /// Makes sure path is suffixed with standard <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        /// <param name="filePath">The file path to be suffixed.</param>
        /// <returns>Suffixed path.</returns>
        public static string AddPathSuffix(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = Path.DirectorySeparatorChar.ToString();
            }
            else
            {
                char suffixChar = filePath[filePath.Length - 1];

                if (suffixChar != Path.DirectorySeparatorChar && suffixChar != Path.AltDirectorySeparatorChar)
                    filePath += Path.DirectorySeparatorChar;
            }

            return filePath;
        }

        /// <summary>
        /// Makes sure path is not suffixed with <see cref="Path.DirectorySeparatorChar"/> or <see cref="Path.AltDirectorySeparatorChar"/>.
        /// </summary>
        /// <param name="filePath">The file path to be unsuffixed.</param>
        /// <returns>Unsuffixed path.</returns>
        public static string RemovePathSuffix(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "";
            }
            else
            {
                char suffixChar = filePath[filePath.Length - 1];

                while ((suffixChar == Path.DirectorySeparatorChar || suffixChar == Path.AltDirectorySeparatorChar) && filePath.Length > 0)
                {
                    filePath = filePath.Substring(0, filePath.Length - 1);

                    if (filePath.Length > 0)
                        suffixChar = filePath[filePath.Length - 1];
                }
            }

            return filePath;
        }

        /// <summary>
        /// Remove any path root present in the path.
        /// </summary>
        /// <param name="filePath">The file path whose root is to be removed.</param>
        /// <returns>The path with the root removed if it was present.</returns>
        public static string DropPathRoot(string filePath)
        {
            // JRC: Changed this to the following more simple algorithm
            if (string.IsNullOrEmpty(filePath))
                return "";
            else
                return filePath.Remove(0, Path.GetPathRoot(filePath).Length);

            #region [ Original Code ]
            //string result = filePath;

            //if (!string.IsNullOrEmpty(filePath))
            //{
            //    if ((filePath[0] == '\\') || (filePath[0] == '/'))
            //    {
            //        // UNC name ?
            //        if ((filePath.Length > 1) && ((filePath[1] == '\\') || (filePath[1] == '/')))
            //        {
            //            int index = 2;
            //            int elements = 2;

            //            // Scan for two separate elements \\machine\share\restofpath
            //            while ((index <= filePath.Length) &&
            //                (((filePath[index] != '\\') && (filePath[index] != '/')) || (--elements > 0)))
            //            {
            //                index++;
            //            }

            //            index++;

            //            if (index < filePath.Length)
            //            {
            //                result = filePath.Substring(index);
            //            }
            //            else
            //            {
            //                result = "";
            //            }
            //        }
            //    }
            //    else if ((filePath.Length > 1) && (filePath[1] == ':'))
            //    {
            //        int dropCount = 2;
            //        if ((filePath.Length > 2) && ((filePath[2] == '\\') || (filePath[2] == '/')))
            //        {
            //            dropCount = 3;
            //        }
            //        result = result.Remove(0, dropCount);
            //    }
            //}

            //return result;
            #endregion
        }

        /// <summary>
        /// Returns a file name, for display purposes, of the specified length using "..." to indicate a longer name.
        /// </summary>
        /// <param name="fileName">The file path to be trimmed.</param>
        /// <param name="length">The maximum length of the trimmed file path.</param>
        /// <returns>Trimmed file path.</returns>
        /// <remarks>
        /// Minimum value for the <paramref name="length" /> parameter is 12. 12 will be used for any value 
        /// specified as less than 12.
        /// </remarks>
        public static string TrimFileName(string fileName, int length)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "";
            else
                fileName = fileName.Trim();

            if (length < 12) length = 12;

            if (fileName.Length > length)
            {
                string justName = GetFileName(fileName);

                if (justName.Length == fileName.Length)
                {
                    // This is just a file name. Make sure extension shows.
                    string justExtension = GetExtension(fileName);
                    string trimName = GetFileNameWithoutExtension(fileName);

                    if (trimName.Length > 8)
                    {
                        if (justExtension.Length > length - 8)
                            justExtension = justExtension.Substring(0, length - 8);

                        double offset = (length - justExtension.Length - 3) / 2.0D;

                        return trimName.Substring(0, (int)(Math.Ceiling(offset))) + "..." +
                            trimName.Substring((int)Math.Round(trimName.Length - Math.Floor(offset) + 1.0D)) + justExtension;
                    }
                    else
                    {
                        // We can not trim file names less than 8 with a "...", so we truncate long extension.
                        return trimName + justExtension.Substring(0, length - trimName.Length);
                    }
                }
                else if (justName.Length > length)
                {
                    // File name alone exceeds length. Recurses into function without path.
                    return TrimFileName(justName, length);
                }
                else
                {
                    // File name contains path. Trims path before file name.
                    string justFilePath = GetDirectoryName(fileName);
                    int offset = length - justName.Length - 4;

                    if (justFilePath.Length > offset && offset > 0)
                    {
                        return justFilePath.Substring(0, offset) + "...\\" + justName;
                    }
                    else
                    {
                        // Can not fit path. Trims file name.
                        return TrimFileName(justName, length);
                    }
                }
            }
            else
            {
                // Full file name fits within requested length.
                return fileName;
            }
        }

        /// <summary>
        /// Waits for the default duration (5 seconds) for read access on a file.
        /// </summary>
        /// <param name="fileName">The name of the file to wait for to obtain read access.</param>
        public static void WaitForReadLock(string fileName)
        {
            WaitForReadLock(fileName, 5);
        }

        /// <summary>
        /// Waits for read access on a file for the specified number of seconds.
        /// </summary>
        /// <param name="fileName">The name of the file to wait for to obtain read access.</param>
        /// <param name="secondsToWait">The time to wait for in seconds to obtain read access on a file. Set to zero to wait infinitely.</param>
        public static void WaitForReadLock(string fileName, double secondsToWait)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Could not test file lock for \"" + fileName + "\", file does not exist", fileName);

            // Keeps trying for a file lock.
            FileStream targetFile = null;
            double startTime = Common.SystemTimer;

            while (true)
            {
                try
                {
                    targetFile = File.OpenRead(fileName);
                    targetFile.Close();
                    break;
                }
                catch
                {
                    // Keeps trying to open the file.
                }

                if (targetFile != null)
                {
                    try
                    {
                        targetFile.Close();
                    }
                    catch
                    {
                    }
                    targetFile = null;
                }

                if (secondsToWait > 0)
                {
                    if (Common.SystemTimer > startTime + secondsToWait)
                        throw new IOException("Could not open \"" + fileName + "\" for read access, tried for " + secondsToWait + " seconds");
                }

                // Yields to all other system threads.
                Thread.Sleep(250);
            }
        }

        /// <summary>
        /// Waits for the default duration (5 seconds) for write access on a file.
        /// </summary>
        /// <param name="fileName">The name of the file to wait for to obtain write access.</param>
        public static void WaitForWriteLock(string fileName)
        {
            WaitForWriteLock(fileName, 5);
        }

        /// <summary>
        /// Waits for write access on a file for the specified number of seconds.
        /// </summary>
        /// <param name="fileName">The name of the file to wait for to obtain write access.</param>
        /// <param name="secondsToWait">The time to wait for in seconds to obtain write access on a file. Set to zero to wait infinitely.</param>
        public static void WaitForWriteLock(string fileName, double secondsToWait)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Could not test file lock for \"" + fileName + "\", file does not exist", fileName);

            // Keeps trying for a file lock.
            FileStream targetFile = null;
            double startTime = Common.SystemTimer;

            while (true)
            {
                try
                {
                    targetFile = File.OpenWrite(fileName);
                    targetFile.Close();
                    break;
                }
                catch
                {
                    // Keeps trying to open the file.
                }

                if (targetFile != null)
                {
                    try
                    {
                        targetFile.Close();
                    }
                    catch
                    {
                    }

                    targetFile = null;
                }

                if (secondsToWait > 0)
                {
                    if (Common.SystemTimer > startTime + secondsToWait)
                        throw new IOException("Could not open \"" + fileName + "\" for write access, tried for " + secondsToWait + " seconds");
                }

                // Yields to all other system threads.
                Thread.Sleep(250);
            }
        }

        /// <summary>
        /// Waits for the default duration (5 seconds) for a file to exist.
        /// </summary>
        /// <param name="fileName">The name of the file to wait for until it is created.</param>
        public static void WaitTillExists(string fileName)
        {
            WaitTillExists(fileName, 5);
        }

        /// <summary>
        /// Waits for a file to exist for the specified number of seconds.
        /// </summary>
        /// <param name="fileName">The name of the file to wait for until it is created.</param>
        /// <param name="secondsToWait">The time to wait for in seconds for the file to be created. Set to zero to wait infinitely.</param>
        public static void WaitTillExists(string fileName, double secondsToWait)
        {
            // Keeps waiting for a file to be created.
            double startTime = Common.SystemTimer;

            while (!File.Exists(fileName))
            {
                if (secondsToWait > 0)
                {
                    if (Common.SystemTimer > startTime + secondsToWait)
                        throw new IOException("Waited for \"" + fileName + "\" to exist for " + secondsToWait + " seconds, but it was never created");
                }

                // Yields to all other system threads.
                Thread.Sleep(250);
            }
        }

        #endregion
    }
}