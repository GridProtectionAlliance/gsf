//******************************************************************************************************
//  FtpFile.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  05/12/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Net.VirtualFtpClient
{
    /// <summary>
    /// Represents a virtual FTP file for the specified target <see cref="FtpType"/>.
    /// </summary>
    public class FtpFile
    {
        /// <summary>
        /// Name of file.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Full path of file.
        /// </summary>
        public string FullPath => Parent.FullPath + Name;

        /// <summary>
        /// Returns true for file entries.
        /// </summary>
        public bool IsFile => true;

        /// <summary>
        /// Returns false for directory entries.
        /// </summary>
        public bool IsDirectory => false;

        /// <summary>
        /// Gets or sets size of file.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets permission of file.
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// Gets or sets timestamp of file.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets parent directory of file.
        /// </summary>
        public FtpDirectory Parent  { get; }
        
        /// <summary>
        /// Downloads remote file.
        /// </summary>
        public void Get()
        {
            Get(Name);
        }

        /// <summary>
        /// Downloads remote file using alternate local filename.
        /// </summary>
        /// <param name="localFile">Local filename to use for download.</param>
        public void Get(string localFile)
        {
        }

        /// <summary>
        /// Removes remote file.
        /// </summary>
        public void Remove()
        {
        }
    }
}
