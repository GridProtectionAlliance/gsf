//******************************************************************************************************
//  FilePath.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/21/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.IO;
using System.Runtime.InteropServices;

namespace GSF.Core.SqlClr.IO
{
    public static partial class FilePath
    {
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

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
    }
}
