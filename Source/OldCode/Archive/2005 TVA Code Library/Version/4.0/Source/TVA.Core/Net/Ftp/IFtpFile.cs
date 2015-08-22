//*******************************************************************************************************
//  IFtpFile.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/23/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Net.Ftp
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
        DateTime TimeStamp { get; set; }
    }
}
