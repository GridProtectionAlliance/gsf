//******************************************************************************************************
//  FtpDirectory.cs - Gbtc
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

using System.Collections.Generic;

namespace GSF.Net.AbstractFtpClient
{
    /// <summary>
    /// Represents an abstract FTP directory for the specified target <see cref="FtpType"/>.
    /// </summary>
    public class FtpDirectory
    {
        /// <summary>
        /// Name of directory.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Full path of directory.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Gets the list of files in this <see cref="FtpDirectory"/>.
        /// </summary>
        IEnumerable<FtpFile> Files { get; }

        /// <summary>
        /// Gets the list of subdirectories in this <see cref="FtpDirectory"/>.
        /// </summary>
        IEnumerable<FtpDirectory> SubDirectories { get; }
    }
}
