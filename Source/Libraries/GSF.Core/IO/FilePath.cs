//******************************************************************************************************
//  FilePath.cs - Gbtc
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
//  02/05/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (GSF.Shared.FilePath).
//  08/22/2007 - Darrell Zuercher
//       Edited code comments.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/24/2008 - Pinal C. Patel
//       Edited code comments.
//  12/17/2008 - F. Russell Robertson
//       Fixed issue in GetFilePatternRegularExpression().
//  06/30/2009 - Pinal C. Patel
//       Removed FilePathHasFileName() since the result was error prone.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/17/2009 - Pinal C. Patel
//       Modified GetAbsolutePath() to remove dependency on HttpContext.Current.
//  04/19/2010 - Pinal C. Patel
//       Added GetApplicationDataFolder() method.
//  04/21/2010 - Pinal C. Patel
//       Updated GetApplicationDataFolder() to include the company name if available.
//  01/28/2011 - J. Ritchie Carroll
//       Added IsValidFileName function.
//  02/14/2011 - J. Ritchie Carroll
//       Fixed issue in GetDirectoryName where last directory was being truncated as a file name.
//  06/06/2011 - Stephen C. Wills
//       Fixed issue in GetFileName where path suffix was being removed before extracting the file name.
//  07/29/2011 - Pinal C. Patel
//       Updated GetApplicationDataFolder() to use the TEMP directory for web applications.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web.Hosting;
using GSF.Console;
using GSF.Identity;
using GSF.Interop;
using GSF.Reflection;
using GSF.Units;

namespace GSF.IO
{
    /// <summary>
    /// Contains File and Path manipulation methods.
    /// </summary>
    public static partial class FilePath
    {
        #region [ Members ]

        // Nested Types

        // ReSharper disable MemberCanBePrivate.Local
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NETRESOURCE
        {
            public readonly int dwScope;
            public int dwType;
            public readonly int dwDisplayType;
            public readonly int dwUsage;
            public readonly string lpLocalName;
            public string lpRemoteName;
            public readonly string lpComment;
            public readonly string lpProvider;
        }

        // Constants
        private const int RESOURCETYPE_DISK = 0x1;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Connects to a network share with the specified user's credentials.
        /// </summary>
        /// <param name="sharename">UNC share name to connect to.</param>
        /// <param name="userName">User name to use for connection.</param>
        /// <param name="password">Password to use for connection.</param>
        /// <param name="domain">Domain name to use for connection. Specify the computer name for local system accounts.</param>
        public static void ConnectToNetworkShare(string sharename, string userName, string password, string domain)
        {
            if (Common.IsPosixEnvironment)
                throw new NotImplementedException("Failed to connect to network share \"" + sharename + "\" as user " + userName + " - not implemented in POSIX environment");

            NETRESOURCE resource = new NETRESOURCE();
            int result;

            resource.dwType = RESOURCETYPE_DISK;
            resource.lpRemoteName = sharename;

            if (domain.Length > 0)
                userName = domain + "\\" + userName;

            result = WNetAddConnection2(ref resource, password, userName, 0);

            if (result != 0)
                throw new InvalidOperationException("Failed to connect to network share \"" + sharename + "\" as user " + userName + " - " + WindowsApi.GetErrorMessage(result));
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
        /// <param name="force"><c>true</c> to force a disconnect; otherwise <c>false</c>.</param>
        public static void DisconnectFromNetworkShare(string sharename, bool force)
        {
            if (Common.IsPosixEnvironment)
                throw new NotImplementedException("Failed to disconnect from network share \"" + sharename + "\" - not implemented in POSIX environment");

            int result = WNetCancelConnection2(sharename, 0, force);

            if (result != 0)
                throw new InvalidOperationException("Failed to disconnect from network share \"" + sharename + "\" - " + WindowsApi.GetErrorMessage(result));
        }

        /// <summary>
        /// Tries to get the free space values for a given path. This path can be a network share or a mount point.
        /// </summary>
        /// <param name="pathName">The path to the location</param>
        /// <param name="freeSpace">The number of user space bytes</param>
        /// <param name="totalSize">The total number of bytes on the drive.</param>
        /// <returns><c>true</c> if successful; otherwise <c>false</c> if there was an error.</returns>
        public static bool GetAvailableFreeSpace(string pathName, out long freeSpace, out long totalSize)
        {
            try
            {
                if (Common.IsPosixEnvironment)
                {
                    string output = Command.Execute("df", $"-k {pathName}").StandardOutput;
                    string[] lines = output.Split('\n');

                    if (lines.Length > 1)
                    {
                        string[] elems = lines[1].Trim().RemoveDuplicateWhiteSpace().Split(' ');

                        if (elems.Length > 4)
                        {
                            long totalKB, availableKB;

                            if (long.TryParse(elems[1], out totalKB) && long.TryParse(elems[3], out availableKB))
                            {
                                freeSpace = availableKB * SI2.Kilo;
                                totalSize = totalKB * SI2.Kilo;
                                return true;
                            }
                        }
                    }

                    freeSpace = 0L;
                    totalSize = 0L;
                    return false;
                }

                string fullPath = Path.GetFullPath(pathName);

                ulong lpFreeBytesAvailable;
                ulong lpTotalNumberOfBytes;
                ulong lpTotalNumberOfFreeBytes;

                bool success = GetDiskFreeSpaceEx(fullPath, out lpFreeBytesAvailable, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes);

                freeSpace = (long)lpFreeBytesAvailable;
                totalSize = (long)lpTotalNumberOfBytes;
                return success;
            }
            catch
            {
                freeSpace = 0L;
                totalSize = 0L;
                return false;
            }
        }

        /// <summary>
        /// Determines if the specified <paramref name="filePath"/> is contained with the current executable path.
        /// </summary>
        /// <param name="filePath">File name or relative file path.</param>
        /// <returns><c>true</c> if the specified <paramref name="filePath"/> is contained with the current executable path; otherwise <c>false</c>.</returns>
        public static bool InApplicationPath(string filePath)
        {
            return GetAbsolutePath(filePath).StartsWith(GetAbsolutePath(""), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the path to the folder where data related to the current
        /// application can be stored as well as shared among different users.
        /// </summary>
        /// <returns>Path to the folder where data related to the current application can be stored.</returns>
        public static string GetCommonApplicationDataFolder()
        {
            string rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            if (string.IsNullOrEmpty(AssemblyInfo.EntryAssembly.Company))
                return Path.Combine(rootFolder, AssemblyInfo.EntryAssembly.Name);

            return Path.Combine(rootFolder, AssemblyInfo.EntryAssembly.Company, AssemblyInfo.EntryAssembly.Name);
        }

        /// <summary>
        /// Gets the path to the folder where data related to the current application can be stored.
        /// </summary>
        /// <returns>Path to the folder where data related to the current application can be stored.</returns>
        public static string GetApplicationDataFolder()
        {
            string rootFolder;

            switch (Common.GetApplicationType())
            {
                case ApplicationType.Web:
                    // Treat web application special.
                    if (HostingEnvironment.ApplicationVirtualPath == "/")
                        rootFolder = Path.Combine(Path.GetTempPath(), HostingEnvironment.SiteName);
                    else
                        rootFolder = Path.Combine(Path.GetTempPath(), HostingEnvironment.ApplicationVirtualPath.ToNonNullString().Trim('/'));

                    // Create a user folder if ID is available.
                    string userID = UserInfo.RemoteUserID;

                    // ID is not available.
                    if (string.IsNullOrEmpty(userID))
                        return rootFolder;

                    // Remove domain from ID.
                    if (!userID.Contains("\\"))
                        return Path.Combine(rootFolder, userID);

                    return Path.Combine(rootFolder, userID.Remove(0, userID.IndexOf('\\') + 1));
                default:
                    rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    if (string.IsNullOrEmpty(AssemblyInfo.EntryAssembly.Company))
                        return Path.Combine(rootFolder, AssemblyInfo.EntryAssembly.Name);

                    return Path.Combine(rootFolder, AssemblyInfo.EntryAssembly.Company, AssemblyInfo.EntryAssembly.Name);
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
                        filePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, filePath);
                        break;
                    default:
                        try
                        {
                            filePath = Path.Combine(GetDirectoryName(AssemblyInfo.EntryAssembly.Location), filePath);
                        }
                        catch
                        {
                            // Fall back on executing assembly path if entry assembly is not available
                            filePath = Path.Combine(GetDirectoryName(AssemblyInfo.ExecutingAssembly.Location), filePath);
                        }
                        break;
                }
            }

            return RemovePathSuffix(filePath);
        }

        /// <summary>
        /// Gets a unique file path for the given file by checking for name collisions and
        /// adding a sequence number to the end of the file name if there is a collision.
        /// </summary>
        /// <param name="originalFilePath">The path to the original file before adding the sequence number.</param>
        /// <returns>The unique path to the file.</returns>
        /// <remarks>
        /// This method is designed to handle the case where the user wishes to create a file in a folder
        /// with a given name when there is a possibility that the name is already taken. Using this method,
        /// it is possible to create files with names in the following format:
        /// 
        /// <ul>
        ///     <li>File.ext</li>
        ///     <li>File (1).ext</li>
        ///     <li>File (2).ext</li>
        ///     <li>...</li>
        /// </ul>
        /// 
        /// This method uses a linear search to find a unique file name, so it is suitable for situations where
        /// there are a small number of collisions for each file name. This will detect and fill gaps that can
        /// occur when files are deleted (for instance, if "File (1).ext" were deleted from the list above).
        /// </remarks>
        public static string GetUniqueFilePath(string originalFilePath)
        {
            string uniqueFilePath = GetAbsolutePath(originalFilePath);
            string directory = GetDirectoryName(uniqueFilePath);
            string originalFileRoot = GetFileNameWithoutExtension(uniqueFilePath);
            string fileExtension = GetExtension(uniqueFilePath);
            int i = 1;

            while (File.Exists(uniqueFilePath))
            {
                uniqueFilePath = Path.Combine(directory, $"{originalFileRoot} ({i}){fileExtension}");
                i++;
            }

            return uniqueFilePath;
        }

        /// <summary>
        /// Gets a unique file path for the given file by checking for name collisions and
        /// adding a sequence number to the end of the file name if there is a collision.
        /// </summary>
        /// <param name="originalFilePath">The path to the original file before adding the sequence number.</param>
        /// <returns>The unique path to the file.</returns>
        /// <remarks>
        /// This method is designed to handle the case where the user wishes to create a file in a folder
        /// with a given name when there is a possibility that the name is already taken. Using this method,
        /// it is possible to create files with names in the following format:
        /// 
        /// <ul>
        ///     <li>File.ext</li>
        ///     <li>File (1).ext</li>
        ///     <li>File (2).ext</li>
        ///     <li>...</li>
        /// </ul>
        /// 
        /// This method uses a binary search to find a unique file name, so it is suitable for situations where
        /// a large number of files will be created with the same file name, and the next unique file name needs
        /// to be found relatively quickly. It will not always detect gaps in the sequence numbers that can occur
        /// when files are deleted (for instance, if "File (1).ext" were deleted from the list above).
        /// </remarks>
        public static string GetUniqueFilePathWithBinarySearch(string originalFilePath)
        {
            string uniqueFilePath = GetAbsolutePath(originalFilePath);
            string directory = GetDirectoryName(uniqueFilePath);
            string originalFileRoot = GetFileNameWithoutExtension(uniqueFilePath);
            string fileExtension = GetExtension(uniqueFilePath);

            int i = 1;
            int j = 1;
            int k = 1;

            if (!File.Exists(uniqueFilePath))
                return uniqueFilePath;

            while (File.Exists(uniqueFilePath))
            {
                uniqueFilePath = Path.Combine(directory, $"{originalFileRoot} ({i}){fileExtension}");
                j = k;
                k = i;
                i *= 2;
            }

            while (j < k)
            {
                i = (j + k) / 2;
                uniqueFilePath = Path.Combine(directory, $"{originalFileRoot} ({i}){fileExtension}");

                if (File.Exists(uniqueFilePath))
                    j = i + 1;
                else
                    k = i;
            }

            return Path.Combine(directory, $"{originalFileRoot} ({k}){fileExtension}");
        }

        /// <summary>
        /// Sets the permissions of the file or directory at the given
        /// <paramref name="path"/> to the object's inherited permissions.
        /// </summary>
        /// <param name="path">The path to the file or directory.</param>
        /// <exception cref="FileNotFoundException">No file or directory was found at the given <paramref name="path"/>.</exception>
        public static void ResetPermissions(string path)
        {
            // This removes access rule protection so that the permissions will
            // be modified by the object's parents (the object's inherited permissions)
            void ResetPermissions(ObjectSecurity objectSecurity) =>
                objectSecurity.SetAccessRuleProtection(false, false);

            string absolutePath = GetAbsolutePath(path);

            if (Directory.Exists(absolutePath))
            {
                DirectorySecurity directorySecurity = new DirectorySecurity();
                ResetPermissions(directorySecurity);
                Directory.SetAccessControl(absolutePath, directorySecurity);
            }
            else if (File.Exists(absolutePath))
            {
                FileSecurity fileSecurity = new FileSecurity();
                ResetPermissions(fileSecurity);
                File.SetAccessControl(absolutePath, fileSecurity);
            }
            else
            {
                throw new FileNotFoundException($"Unable to find file or directory at the given path: {absolutePath}");
            }
        }

        /// <summary>
        /// Copies permissions from the object at the given <paramref name="sourcePath"/>
        /// to the object at the given <paramref name="targetPath"/>.
        /// </summary>
        /// <param name="sourcePath">The path to the object from which to copy permissions.</param>
        /// <param name="targetPath">The path to the object to which to apply permissions.</param>
        /// <exception cref="FileNotFoundException">No file or directory was found at either the <paramref name="sourcePath"/> or <paramref name="targetPath"/>.</exception>
        /// <exception cref="InvalidOperationException">The type of object at <paramref name="sourcePath"/> does not match the type of object at <paramref name="targetPath"/>.</exception>
        public static void CopyPermissions(string sourcePath, string targetPath)
        {
            string sourceAbsolutePath = GetAbsolutePath(sourcePath);
            string targetAbsolutePath = GetAbsolutePath(targetPath);

            bool sourceIsDirectory = Directory.Exists(sourceAbsolutePath);
            bool sourceIsFile = File.Exists(sourceAbsolutePath);
            bool targetIsDirectory = Directory.Exists(sourceAbsolutePath);
            bool targetIsFile = File.Exists(sourceAbsolutePath);

            if (!sourceIsDirectory && !sourceIsFile)
                throw new FileNotFoundException($"Unable to find file or directory at the given path: {sourceAbsolutePath}");

            if (!targetIsDirectory && !targetIsFile)
                throw new FileNotFoundException($"Unable to find file or directory at the given path: {targetAbsolutePath}");

            if (sourceIsDirectory && targetIsFile)
                throw new InvalidOperationException("Unable to copy permissions from a directory to a file");

            if (sourceIsFile && targetIsDirectory)
                throw new InvalidOperationException("Unable to copy permissions from a file to a directory");

            if (sourceIsDirectory && targetIsDirectory)
                CopyDirectoryPermissions(sourceAbsolutePath, targetAbsolutePath);
            else if (sourceIsFile && targetIsFile)
                CopyFilePermissions(sourceAbsolutePath, targetAbsolutePath);
        }

        /// <summary>
        /// Copies the permissions of the directory at the given <paramref name="sourcePath"/>
        /// to the directory at the given <paramref name="targetPath"/>.
        /// </summary>
        /// <param name="sourcePath">The path to the directory from which to copy permissions.</param>
        /// <param name="targetPath">The path to the directory to which to apply permissions.</param>
        public static void CopyDirectoryPermissions(string sourcePath, string targetPath)
        {
            string sourceAbsolutePath = GetAbsolutePath(sourcePath);
            string targetAbsolutePath = GetAbsolutePath(targetPath);
            DirectorySecurity sourceSecurity = Directory.GetAccessControl(sourceAbsolutePath);
            DirectorySecurity targetSecurity = new DirectorySecurity();

            // This prevents permissions modifications by the target directory's parents (the target's inherited permissions)
            targetSecurity.SetAccessRuleProtection(true, false);

            // Now apply ALL of the source directory's permissions to the target directory,
            // including inherited permissions from the source directory's parents
            foreach (FileSystemAccessRule rule in sourceSecurity.GetAccessRules(true, true, typeof(NTAccount)))
                targetSecurity.AddAccessRule(rule);

            Directory.SetAccessControl(targetAbsolutePath, targetSecurity);
        }

        /// <summary>
        /// Copies the permissions of the file at the given <paramref name="sourcePath"/>
        /// to the file at the given <paramref name="targetPath"/>.
        /// </summary>
        /// <param name="sourcePath">The path to the file from which to copy permissions.</param>
        /// <param name="targetPath">The path to the file to which to apply permissions.</param>
        public static void CopyFilePermissions(string sourcePath, string targetPath)
        {
            string sourceAbsolutePath = GetAbsolutePath(sourcePath);
            string targetAbsolutePath = GetAbsolutePath(targetPath);
            FileSecurity sourceSecurity = File.GetAccessControl(sourceAbsolutePath);
            FileSecurity targetSecurity = new FileSecurity();

            // This prevents permissions modifications by the target file's parents (the target's inherited permissions)
            targetSecurity.SetAccessRuleProtection(true, false);

            // Now apply ALL of the source file's permissions to the target directory,
            // including inherited permissions from the source file's parents
            foreach (FileSystemAccessRule rule in sourceSecurity.GetAccessRules(true, true, typeof(NTAccount)))
                targetSecurity.AddAccessRule(rule);

            File.SetAccessControl(targetAbsolutePath, targetSecurity);
        }

        /// <summary>
        /// Replaces the permissions of the directory or file at the given <paramref name="targetPath"/>
        /// with the inheritable permissions from the directory at the given <paramref name="sourcePath"/>.
        /// </summary>
        /// <param name="sourcePath">The path to the directory from which to derive inheritable permissions.</param>
        /// <param name="targetPath">The path to the directory or file to which to apply the derived permissions.</param>
        /// <exception cref="DirectoryNotFoundException">No directory was found at the given <paramref name="sourcePath"/>.</exception>
        /// <exception cref="FileNotFoundException">No file or directory was found at the given <paramref name="targetPath"/>.</exception>
        public static void ApplyInheritablePermissions(string sourcePath, string targetPath)
        {
            string sourceAbsolutePath = GetAbsolutePath(sourcePath);
            string targetAbsolutePath = GetAbsolutePath(targetPath);

            if (!Directory.Exists(sourceAbsolutePath))
                throw new DirectoryNotFoundException($"Unable to find directory at the given path: {sourceAbsolutePath}");

            if (Directory.Exists(targetAbsolutePath))
                ApplyInheritableDirectoryPermissions(sourceAbsolutePath, targetAbsolutePath);
            else if (File.Exists(targetAbsolutePath))
                ApplyInheritableFilePermissions(sourceAbsolutePath, targetAbsolutePath);
            else
                throw new FileNotFoundException($"Unable to find file or directory at the given path: {targetAbsolutePath}");
        }

        /// <summary>
        /// Replaces the permissions of the directory at the given <paramref name="targetPath"/>
        /// with the inheritable permissions from the directory at the given <paramref name="sourcePath"/>.
        /// </summary>
        /// <param name="sourcePath">The path to the directory from which to derive inheritable permissions.</param>
        /// <param name="targetPath">The path to the directory to which to apply the derived permissions.</param>
        public static void ApplyInheritableDirectoryPermissions(string sourcePath, string targetPath)
        {
            string sourceAbsolutePath = GetAbsolutePath(sourcePath);
            string targetAbsolutePath = GetAbsolutePath(targetPath);
            DirectorySecurity sourceSecurity = Directory.GetAccessControl(sourceAbsolutePath);
            DirectorySecurity targetSecurity = Directory.GetAccessControl(targetAbsolutePath);

            IdentityReference targetOwner = targetSecurity.GetOwner(typeof(NTAccount));
            IdentityReference targetGroup = targetSecurity.GetGroup(typeof(NTAccount));
            targetSecurity = new DirectorySecurity();

            // This prevents permissions modifications by the target directory's parents (the target's inherited permissions)
            targetSecurity.SetAccessRuleProtection(true, false);

            foreach (FileSystemAccessRule rule in sourceSecurity.GetAccessRules(true, true, typeof(NTAccount)))
            {
                InheritanceFlags inheritanceFlags = rule.InheritanceFlags;

                // If the inheritance flags indicate that this rule
                // is not inheritable by subfolders, skip it
                if (!inheritanceFlags.HasFlag(InheritanceFlags.ContainerInherit))
                    continue;

                IdentityReference identityReference = rule.IdentityReference;
                FileSystemRights fileSystemRights = rule.FileSystemRights;
                AccessControlType accessControlType = rule.AccessControlType;

                // If the rule is associated with the CREATOR OWNER identity, add an additional rule
                // for the target's owner that applies only to the target directory (not inheritable)
                if (identityReference.Value == "CREATOR OWNER")
                    targetSecurity.AddAccessRule(new FileSystemAccessRule(targetOwner, fileSystemRights, accessControlType));

                // If the rule is associated with the CREATOR GROUP identity, add an additional rule
                // for the target's group that applies only to the target directory (not inheritable)
                if (identityReference.Value == "CREATOR GROUP")
                    targetSecurity.AddAccessRule(new FileSystemAccessRule(targetGroup, fileSystemRights, accessControlType));

                // If the rule applies only to objects within the source directory,
                // clear inheritance flags so it will not propagate to subfolders
                // and files within the target directory
                if (rule.PropagationFlags.HasFlag(PropagationFlags.NoPropagateInherit))
                    inheritanceFlags = InheritanceFlags.None;

                // Inherited permissions never inherit propagation flags
                PropagationFlags propagationFlags = PropagationFlags.None;

                targetSecurity.AddAccessRule(new FileSystemAccessRule(identityReference, fileSystemRights, inheritanceFlags, propagationFlags, accessControlType));
            }

            Directory.SetAccessControl(targetAbsolutePath, targetSecurity);
        }

        /// <summary>
        /// Replaces the permissions of the file at the given <paramref name="targetPath"/>
        /// with the inheritable permissions from the directory at the given <paramref name="sourcePath"/>.
        /// </summary>
        /// <param name="sourcePath">The path to the directory from which to derive inheritable permissions.</param>
        /// <param name="targetPath">The path to the file to which to apply the derived permissions.</param>
        public static void ApplyInheritableFilePermissions(string sourcePath, string targetPath)
        {
            string sourceAbsolutePath = GetAbsolutePath(sourcePath);
            string targetAbsolutePath = GetAbsolutePath(targetPath);
            DirectorySecurity sourceSecurity = Directory.GetAccessControl(sourceAbsolutePath);
            FileSecurity targetSecurity = File.GetAccessControl(targetAbsolutePath);

            IdentityReference targetOwner = targetSecurity.GetOwner(typeof(NTAccount));
            IdentityReference targetGroup = targetSecurity.GetGroup(typeof(NTAccount));
            targetSecurity = new FileSecurity();

            // This prevents permissions modifications by the target file's parents (the target's inherited permissions)
            targetSecurity.SetAccessRuleProtection(true, false);

            foreach (FileSystemAccessRule rule in sourceSecurity.GetAccessRules(true, true, typeof(NTAccount)))
            {
                // If the inheritance flags indicate that this rule
                // is not inheritable by subfolders, skip it
                if (!rule.InheritanceFlags.HasFlag(InheritanceFlags.ObjectInherit))
                    continue;

                IdentityReference identityReference = rule.IdentityReference;
                FileSystemRights fileSystemRights = rule.FileSystemRights;
                AccessControlType accessControlType = rule.AccessControlType;

                // If the rule is associated with the CREATOR OWNER identity or the CREATOR GROUP identity,
                // the new rule must instead be associated with the actual owner or group of the target file
                if (identityReference.Value == "CREATOR OWNER")
                    identityReference = targetOwner;
                else if (identityReference.Value == "CREATOR GROUP")
                    identityReference = targetGroup;

                targetSecurity.AddAccessRule(new FileSystemAccessRule(identityReference, fileSystemRights, accessControlType));
            }

            File.SetAccessControl(targetAbsolutePath, targetSecurity);
        }

        [DllImport("mpr.dll", EntryPoint = "WNetAddConnection2W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, int dwFlags);

        [DllImport("mpr.dll", EntryPoint = "WNetCancelConnection2W", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, [MarshalAs(UnmanagedType.Bool)] bool fForce);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        #endregion
    }
}