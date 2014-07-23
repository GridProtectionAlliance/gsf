//******************************************************************************************************
//  IFtpFile.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/23/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        http://www.codeproject.com/KB/IP/net_ftp_upload.aspx
//  
//   Copyright Alex Kwok & Uwe Keim 
//
//   The Code Project Open License (CPOL):
//        http://www.codeproject.com/info/cpol10.aspx
//
//*******************************************************************************************************

#endregion

using System;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Abstract representation of a FTP file or directory.
    /// </summary>
    public interface IFtpFile : IComparable, IComparable<IFtpFile>
    {
        /// <summary>
        /// Gets parent directory of file or directory.
        /// </summary>
        FtpDirectory Parent { get; }

        /// <summary>
        /// Gets name of file or directory.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets full path of file or directory.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// Returns true if <see cref="IFtpFile"/> represents a file, otherwise false.
        /// </summary>
        bool IsFile { get; }

        /// <summary>
        /// Returns true if <see cref="IFtpFile"/> represents a directory, otherwise false.
        /// </summary>
        bool IsDirectory { get; }
        
        /// <summary>
        /// Gets or sets size of file or directory.
        /// </summary>
        long Size { get; set; }

        /// <summary>
        /// Gets or sets permission of file or directory.
        /// </summary>
        string Permission { get; set; }

        /// <summary>
        /// Gets or sets timestamp of file or directory.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
