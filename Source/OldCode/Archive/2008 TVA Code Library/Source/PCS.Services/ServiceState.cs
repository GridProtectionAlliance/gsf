//*******************************************************************************************************
//  ServiceState.cs
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
    /// <summary>Windows service states</summary>
    public enum ServiceState
    {
        /// <summary>Service has started.</summary>
        Started,
        /// <summary>Service has stopped.</summary>
        Stopped,
        /// <summary>Service has paused.</summary>
        Paused,
        /// <summary>Service has resumed.</summary>
        Resumed,
        /// <summary>Service has shutdown.</summary>
        Shutdown
    }
}
