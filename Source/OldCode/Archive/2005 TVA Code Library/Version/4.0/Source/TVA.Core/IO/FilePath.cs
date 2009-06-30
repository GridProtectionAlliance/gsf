//*******************************************************************************************************
//  FilePath.cs
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
//  02/05/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.FilePath).
//  08/22/2007 - Darrell Zuercher
//       Edited code comments.
//  09/19/2008 - James R Carroll
//       Converted to C#.
//  10/24/2008 - Pinal C. Patel
//       Edited code comments.
//  12/17/2008 - Russell F. Robertson
//       Fixed bug in GetFilePatternRegularExpression().
//  06/30/2009 - Pinal C. Patel
//       Removed FilePathHasFileName() since the result was error prone.
//
//*******************************************************************************************************

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

        [DllImport("mpr.dll", EntryPoint = "WNetAddConnection2W", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, int dwFlags);

        [DllImport("mpr.dll", EntryPoint = "WNetCancelConnection2W", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

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