//******************************************************************************************************
//  FtpPath.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  03/14/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Contains File and Path manipulation methods for FTP communications.
    /// </summary>
    public class FtpPath
    {
        /// <summary>
        /// Combines an array of strings into an FTP path.
        /// </summary>
        /// <param name="paths">An array of parts of the path.</param>
        /// <returns>The combined paths.</returns>
        /// <exception cref="ArgumentNullException">One of the strings in the array is null.</exception>
        /// <remarks>
        /// <paramref name="paths"/> should be an array of the parts of the path to combine.
        /// If the one of the subsequent paths is an absolute path,
        /// then the combine operation resets starting with that absolute path,
        /// discarding all previous combined paths.
        ///
        /// Zero-length strings are omitted from the combined path.
        /// </remarks>
        public static string Combine(params string[] paths)
        {
            const string Separator = "/";
            string combinedPath = "";

            foreach (string path in paths)
            {
                if ((object)path == null)
                    throw new ArgumentNullException("One of the strings in the array is null.");

                if (path.Length == 0)
                    continue;

                if (path.StartsWith(Separator))
                    combinedPath = path;
                else if (combinedPath.EndsWith(Separator))
                    combinedPath += path;
                else
                    combinedPath += Separator + path;
            }

            return combinedPath;
        }
    }
}
