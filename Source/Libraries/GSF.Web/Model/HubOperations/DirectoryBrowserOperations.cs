//******************************************************************************************************
//  DirectoryBrowserOperations.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/08/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GSF.IO;

namespace GSF.Web.Model.HubOperations
{
    /// <summary>
    /// Defines an interface for <see cref="DirectoryBrowserOperations"/> to make sure all needed methods
    /// are implemented in a hub when using the DirectoryBrowserOperations.cshtml view.
    /// </summary>
    public interface IDirectoryBrowserOperations
    {
        /// <summary>
        /// Retrieves list of directories from specified <paramref name="rootFolder"/>.
        /// </summary>
        /// <param name="rootFolder">Folder to load directories from.</param>
        /// <param name="showHidden"><c>true</c> to show hidden directories; otherwise, <c>false</c>.</param>
        /// <returns>List of directories from specified <paramref name="rootFolder"/>.</returns>
        IEnumerable<string> LoadDirectories(string rootFolder, bool showHidden);

        /// <summary>
        /// Determines if the specified <paramref name="path"/> is a logical drive, e.g., C:\.
        /// </summary>
        /// <param name="path">Path to test.</param>
        /// <returns><c>true</c> if the specified <paramref name="path"/> is a logical drive; otherwise, <c>false</c>.</returns>
        bool IsLogicalDrive(string path);

        /// <summary>
        /// Resolves the specified <paramref name="path"/> to an absolute full path and expands any environmental variables.
        /// </summary>
        /// <param name="path">Path to resolve.</param>
        /// <returns></returns>
        string ResolvePath(string path);

        /// <summary>
        /// Combines the specified <paramref name="path1"/> and <paramref name="path2"/> strings into a single path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combined paths.</returns>
        string CombinePath(string path1, string path2);

        /// <summary>
        /// Creates all directories and subdirectories in the specified <paramref name="path"/> unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        void CreatePath(string path);
    }

    /// <summary>
    /// Represents hub based operations for the DirectoryBrowserOperations.cshtml view.
    /// </summary>
    public static class DirectoryBrowserOperations
    {
        /// <summary>
        /// Retrieves list of directories from specified <paramref name="rootFolder"/>.
        /// </summary>
        /// <param name="rootFolder">Folder to load directories from.</param>
        /// <param name="showHidden"><c>true</c> to show hidden directories; otherwise, <c>false</c>.</param>
        /// <returns>List of directories from specified <paramref name="rootFolder"/>.</returns>
        public static IEnumerable<string> LoadDirectories(string rootFolder, bool showHidden)
        {
            if (string.IsNullOrWhiteSpace(rootFolder))
                return Directory.GetLogicalDrives();

            IEnumerable<string> directories = Directory.GetDirectories(rootFolder);

            if (!showHidden)
                directories = directories.Where(path => !new DirectoryInfo(path).Attributes.HasFlag(FileAttributes.Hidden));

            return new[] { "..\\" }.Concat(directories.Select(path => FilePath.AddPathSuffix(FilePath.GetLastDirectoryName(path))));
        }

        /// <summary>
        /// Determines if the specified <paramref name="path"/> is a logical drive, e.g., C:\.
        /// </summary>
        /// <param name="path">Path to test.</param>
        /// <returns><c>true</c> if the specified <paramref name="path"/> is a logical drive; otherwise, <c>false</c>.</returns>
        public static bool IsLogicalDrive(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            DirectoryInfo info = new DirectoryInfo(path);
            return info.FullName == info.Root.FullName;
        }

        /// <summary>
        /// Resolves the specified <paramref name="path"/> to an absolute full path and expands any environmental variables.
        /// </summary>
        /// <param name="path">Path to resolve.</param>
        /// <returns></returns>
        public static string ResolvePath(string path)
        {
            if (IsLogicalDrive(path) && Path.GetFullPath(path) == path)
                return path;

            return Path.GetFullPath(FilePath.GetAbsolutePath(Environment.ExpandEnvironmentVariables(path)));
        }

        /// <summary>
        /// Combines the specified <paramref name="path1"/> and <paramref name="path2"/> strings into a single path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combined paths.</returns>
        public static string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>
        /// Creates all directories and subdirectories in the specified <paramref name="path"/> unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        public static void CreatePath(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
}
