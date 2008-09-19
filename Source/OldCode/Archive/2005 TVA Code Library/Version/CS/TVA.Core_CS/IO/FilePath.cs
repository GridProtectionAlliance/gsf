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
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading;
using TVA.Reflection;
using TVA.Interop;

namespace TVA.IO
{
    /// <summary>
    /// File/Path Manipulation Functions.
    /// </summary>
    public static class FilePath
    {
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

        [DllImport("mpr.dll", EntryPoint = "WNetAddConnection2W", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, int dwFlags);

        [DllImport("mpr.dll", EntryPoint = "WNetCancelConnection2W", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        private const int RESOURCETYPE_DISK = 0x1;

        private static string m_fileNameCharPattern;

        /// <summary>Connects to a network share with the specified user's credentials.</summary>
        /// <param name="sharename">UNC share name to connect to.</param>
        /// <param name="username">Username to use for connection.</param>
        /// <param name="password">Password to use for connection.</param>
        /// <param name="domain">Domain name to use for connetion. Specify the computer name for local system accounts.</param>
        public static void ConnectToNetworkShare(string sharename, string username, string password, string domain)
        {
            NETRESOURCE resource;
            int result;

            resource.dwType = RESOURCETYPE_DISK;
            resource.lpRemoteName = sharename;

            if (domain.Length > 0)
                username = domain + "\\" + username;

            result = WNetAddConnection2(ref resource, password, username, 0);
            if (result != 0)
                throw new InvalidOperationException("Failed to connect to network share \"" + sharename + "\" as user " + username + ". " + WindowsApi.GetErrorMessage(result));
        }

        /// <summary>Disconnects the specified network share.</summary>
        /// <param name="sharename">UNC share name to disconnect from.</param>
        public static void DisconnectFromNetworkShare(string sharename)
        {
            DisconnectFromNetworkShare(sharename, true);
        }

        /// <summary>Disconnects the specified network share.</summary>
        /// <param name="sharename">UNC share name to disconnect from.</param>
        /// <param name="force">Set to True to force a disconnect.</param>
        public static void DisconnectFromNetworkShare(string sharename, bool force)
        {
            int result = WNetCancelConnection2(sharename, 0, force);
            if (result != 0)
                throw new InvalidOperationException("Failed to disconnect from network share \"" + sharename + "\".  " + WindowsApi.GetErrorMessage(result));
        }

        /// <summary>Returns True if the specified file name matches any of the given file specs (wildcards are defined as '*' or '?' characters).</summary>
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

        /// <summary>Returns True if the specified file name matches given file spec (wildcards are defined as '*' or '?' characters).</summary>
        public static bool IsFilePatternMatch(string fileSpec, string fileName, bool ignoreCase)
        {
            return (new Regex(GetFilePatternRegularExpression(fileSpec), (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))).IsMatch(fileName);
        }

        /// <summary>Returns a regular expression that simulates wildcard matching for filenames (wildcards are defined as '*' or '?' characters).</summary>
        public static string GetFilePatternRegularExpression(string fileSpec)
		{
			if (m_fileNameCharPattern == null)
			{
				StringBuilder pattern = new StringBuilder();

				// Defines a regular expression pattern for a valid file name character. We do this by
				// allowing any characters except those that would not be valid as part of a filename.
				// This essentially builds the "?" wildcard pattern match.
				pattern.Append("[^");
				pattern.Append(TVA.Text.Common.EncodeRegexChar(Path.DirectorySeparatorChar));
				pattern.Append(TVA.Text.Common.EncodeRegexChar(Path.AltDirectorySeparatorChar));
				pattern.Append(TVA.Text.Common.EncodeRegexChar(Path.PathSeparator));
				pattern.Append(TVA.Text.Common.EncodeRegexChar(Path.VolumeSeparatorChar));
				
				foreach (char c in Path.GetInvalidPathChars())
				{
					pattern.Append(TVA.Text.Common.EncodeRegexChar(c));
				}
				
				pattern.Append("]");
				m_fileNameCharPattern = pattern.ToString();
			}
			
			// Replaces wildcard file patterns with their equivalent regular expression.
			fileSpec = fileSpec.Replace("\\", "\\u005C"); // Backslash in Regex means special sequence. Here, we really want a backslash.
			fileSpec = fileSpec.Replace(".", "\\u002E"); // Dot in Regex means any character. Here, we really want a dot.
			fileSpec = fileSpec.Replace("", m_fileNameCharPattern);
			fileSpec = fileSpec.Replace("*", "(" + m_fileNameCharPattern + ")*");
			
			return "^" + fileSpec + "$";
		}

        /// <summary>Gets the size of the specified file.</summary>
        /// <param name="fileName">Name of file whose size is to be returned.</param>
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

        /// <summary>Gets a unique temporary file name with path.</summary>
        public static string GetTempFile()
        {
            return GetTempFile(true, true, "tmp");
        }

        /// <summary>Gets a unique temporary file name with path. If UseTempPath is false, application path is used for temp file.</summary>
        public static string GetTempFile(bool useTempPath, bool createZeroLengthFile, string fileExtension)
        {
            if (useTempPath && createZeroLengthFile)
            {
                string tempFileName = GetTempFilePath() + GetTempFileName(fileExtension);

                FileStream tempFile = File.Create(tempFileName);
                tempFile.Close();

                return tempFileName;
            }
            else if (useTempPath)
            {
                return GetTempFilePath() + GetTempFileName(fileExtension);
            }
            else if (createZeroLengthFile)
            {
                string tempFileName = GetApplicationPath() + GetTempFileName(fileExtension);

                FileStream tempFile = File.Create(tempFileName);
                tempFile.Close();

                return tempFileName;
            }
            else
            {
                return GetApplicationPath() + GetTempFileName(fileExtension);
            }
        }

        /// <summary>Gets a file name (with .tmp extension), guaranteed to be unique, with no path.</summary>
        public static string GetTempFileName()
        {
            return GetTempFileName("tmp");
        }

        /// <summary>Gets a file name, guaranteed to be unique, with no path.</summary>
        /// <param name="fileExtension">The extension of the temporary file.</param>
        /// <returns>The file name, guaranteed to be unique, with no path.</returns>
        /// <remarks>Use GetTempFile to return unique file name with path.</remarks>
        public static string GetTempFileName(string fileExtension)
        {
            if (fileExtension.Substring(0, 1) == ".")
                fileExtension = fileExtension.Substring(1);

            return Guid.NewGuid().ToString() + "." + fileExtension;
        }

        /// <summary>Gets the temporary file path, suffixed with standard directory separator.</summary>
        /// <returns>The temporary file path.</returns>
        public static string GetTempFilePath()
        {
            return AddPathSuffix(Path.GetTempPath());
        }

        /// <summary>Gets the path of the executing assembly, suffixed with standard directory separator.</summary>
        /// <returns>The path of the executing assembly.</returns>
        public static string GetApplicationPath()
        {
            return JustPath(AssemblyInformation.EntryAssembly.Location.Trim());
        }

        /// <summary>Returns just the drive letter (or UNC (\\server\share\) from a path), suffixed with standard directory separator.</summary>
        public static string JustDrive(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return Path.DirectorySeparatorChar.ToString();
            else
                return AddPathSuffix(Path.GetPathRoot(filePath));
        }

        /// <summary>Returns just the file name from a path.</summary>
        public static string JustFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "";
            else
                return Path.GetFileName(filePath);
        }

        /// <summary>Returns last directory name from a path (e.g., would return sub2 from c:\windows\sub2\filename.ext).</summary>
        public static string LastDirectoryName(string filePath)
        {
            char[] dirChars = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
            char[] dirVolChars = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar};

            filePath = JustPath(filePath);

            while (filePath.Substring(filePath.Length - 1, 1).IndexOfAny(dirChars) > -1)
            {
                filePath = filePath.Substring(0, filePath.Length - 1);
            }

            while (filePath.IndexOfAny(dirVolChars) > -1)
            {
                filePath = filePath.Substring(filePath.Length - filePath.Length - 1, filePath.Length - 1);
            }

            return filePath;
        }

        /// <summary>
        /// Returns the absolute path for the specified file name.
        /// </summary>
        /// <remarks>
        /// This function will prefix the the application's root path to the given file name, note
        /// that this will also work for Web applications.
        /// </remarks>
        public static string AbsolutePath(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
            {
                // The specified path is a relative one since it is not rooted.
                switch (Common.GetApplicationType())
                {
                    // Prepends the application's root to the file path.
                    case ApplicationType.Web:
                        filePath = System.Web.HttpContext.Current.Request.MapPath("~/") + filePath;
                        break;
                    default:
                        filePath = JustPath(AssemblyInformation.EntryAssembly.Location) + filePath;
                        break;
                }
            }

            return filePath;
        }

        /// <summary>Returns just the path, without a filename from a path, suffixed with standard directory separator.</summary>
        public static string JustPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return Path.DirectorySeparatorChar.ToString();
            else
                return Path.GetDirectoryName(filePath) + Path.DirectorySeparatorChar;
        }

        /// <summary>Returns just the file extension from a path, keeping the extension "dot".</summary>
        public static string JustFileExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "";
            else
                return Path.GetExtension(filePath);
        }

        /// <summary>Returns just the file name, with no extension, from a path.</summary>
        public static string NoFileExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "";
            else
                return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>Makes sure path is suffixed with standard directory separator.</summary>
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

        /// <summary>Makes sure path is not suffixed with any directory separator.</summary>
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

        /// <summary>Returns a file name, for display purposes, of the specified length using "..." to indicate a longer name.</summary>
        /// <remarks>
        /// <para>Minimum value for the <paramref name="length" /> parameter is 12.</para>
        /// <para>12 will be used for any value specified as less than 12.</para>
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
                string justName = JustFileName(fileName);

                if (justName.Length == fileName.Length)
                {
                    // This is just a file name. Make sure extension shows.
                    string justExtension = JustFileExtension(fileName);
                    string trimName = NoFileExtension(fileName);

                    if (trimName.Length > 8)
                    {
                        if (justExtension.Length > length - 8)
                            justExtension = justExtension.Substring(0, length - 8);
                        
                        double offset = (length - justExtension.Length - 3) / 2.0D;
                        
                        return trimName.Substring(0, (int)(System.Math.Ceiling(offset))) + "..." + 
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
                    string justFilePath = JustPath(fileName);
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

        /// <summary>Gets a list of files for the given path and wildcard pattern (e.g., "c:\*.*").</summary>
        /// <param name="path">The path for which a list of files is to be returned.</param>
        /// <returns>A list of files for the given path.</returns>
        public static string[] GetFileList(string path)
        {
            return Directory.GetFiles(JustPath(path), JustFileName(path));
        }

        /// <summary>Waits for the default duration (5 seconds) for read access on a file.</summary>
        /// <param name="fileName">The name of the file to wait for to obtain read access.</param>
        public static void WaitForReadLock(string fileName)
        {
            WaitForReadLock(fileName, 5);
        }

        /// <summary>Waits for read access on a file for the specified number of seconds.</summary>
        /// <param name="fileName">The name of the file to wait for to obtain read access.</param>
        /// <param name="secondsToWait">The time to wait for in seconds to obtain read access on a file.</param>
        /// <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
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

        /// <summary>Waits for the default duration (5 seconds) for write access on a file.</summary>
        /// <param name="fileName">The name of the file to wait for to obtain write access.</param>
        public static void WaitForWriteLock(string fileName)
        {
            WaitForWriteLock(fileName, 5);
        }

        /// <summary>Waits for write access on a file for the specified number of seconds.</summary>
        /// <param name="fileName">The name of the file to wait for to obtain write access.</param>
        /// <param name="secondsToWait">The time to wait for in seconds to obtain write access on a file.</param>
        /// <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
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

        /// <summary>Waits for the default duration (5 seconds) for a file to exist.</summary>
        /// <param name="fileName">The name of the file to wait for until it is created.</param>
        public static void WaitTillExists(string fileName)
        {
            WaitTillExists(fileName, 5);
        }

        /// <summary>Waits for a file to exist for the specified number of seconds.</summary>
        /// <param name="fileName">The name of the file to wait for until it is created.</param>
        /// <param name="secondsToWait">The time to wait for in seconds for the file to be created.</param>
        /// <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
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
    }
}