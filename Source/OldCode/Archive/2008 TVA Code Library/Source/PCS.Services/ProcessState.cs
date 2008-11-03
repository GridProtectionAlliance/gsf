//*******************************************************************************************************
//  ProcessState.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/03/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace PCS.Services
{
    /// <summary>Windows service process states</summary>
    public enum ProcessState
    {
        /// <summary>Service process has not been started.</summary>
        Unprocessed,
        /// <summary>Service process is currently executing.</summary>
        Processing,
        /// <summary>Service process has completed processing.</summary>
        Processed,
        /// <summary>Service process was aborted.</summary>
        Aborted,
        /// <summary>Service process stopped due to exception.</summary>
        Exception
    }
}
