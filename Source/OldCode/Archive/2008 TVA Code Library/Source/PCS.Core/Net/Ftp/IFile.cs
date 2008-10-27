//*******************************************************************************************************
//  IFile.cs
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
//  09/23/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.Net.Ftp
{
    public interface IFile : IComparable, IComparable<IFile>
    {
        Directory Parent { get; }

        string Name { get; }

        string FullPath { get; }

        bool IsFile { get; }

        bool IsDirectory { get; }

        long Size { get; set; }

        string Permission { get; set; }

        DateTime TimeStamp { get; set; }
    }
}
