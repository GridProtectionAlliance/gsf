//*******************************************************************************************************
//  IServiceComponent.cs
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
//  02/23/2003 - J. Ritchie Carroll
//       Original version of source code generated
//  01/23/2006 - J. Ritchie Carroll
//       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
//  09/23/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************
using System;

namespace TVA.Services
{
    #region [ Enumerations ]

    /// <summary>Windows service states</summary>
    public enum ServiceState
    {
        Started,
        Stopped,
        Paused,
        Resumed,
        Shutdown
    }

    /// <summary>Windows service process states</summary>
    public enum ProcessState
    {
        Unprocessed,
        Processing,
        Processed,
        Aborted,
        Exception
    }

    #endregion

    /// <summary>
    /// Defines an interface for user created components used by the service so that components can inform service
    /// of current status and automatically react to service events.
    /// </summary>
    public interface IServiceComponent : IStatusProvider
    {
        void ServiceStateChanged(ServiceState newState);

        void ProcessStateChanged(string processName, ProcessState newState);
    }
}