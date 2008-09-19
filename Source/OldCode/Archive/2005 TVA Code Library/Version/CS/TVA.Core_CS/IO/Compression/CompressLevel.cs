//*******************************************************************************************************
//  Enumerations.cs
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
//  09/19/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.IO.Compression
{
    /// <summary>
    /// Specifies the level of compression to be performed on data.
    /// </summary>
    public enum CompressLevel
    {
        DefaultCompression = -1,
        NoCompression = 0,
        BestSpeed = 1,
        BestCompression = 9,
        MultiPass = 10
    }
}