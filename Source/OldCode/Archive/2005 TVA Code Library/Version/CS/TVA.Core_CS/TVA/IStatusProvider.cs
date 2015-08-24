//*******************************************************************************************************
//  IStatusProvider.cs
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
//  09/24/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA
{
    /// <summary>
    /// Defines an interface for any object to allow it to provide a name and status
    /// that can be displayed for informational purposes.
    /// </summary>
    public interface IStatusProvider
    {
        /// <summary>Name of the adapter</summary>
        string Name { get; }

        /// <summary>Provides current status information about the adapter</summary>
        string Status { get; }
    }
 }
