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
//       2.0 version of source code migrated from 1.1 source (PCS.Shared.Common)
//  09/23/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

namespace PCS.Services
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
    /// Defines an interface for components consumed by a Windows Service to allow notification
    /// and proper response to external service events.
    /// </summary>
    /// <remarks>
    /// Typically components will respond to service events in the following manner:
    /// <list type="table">
    /// <listheader>
    ///     <term>ServiceState</term>
    ///     <description>Component response</description>
    /// </listheader>
    /// <item>
    ///     <term>Paused</term>
    ///     <description>Stop processing (e.g., <c>this.Enabled = false;</c>)</description>
    /// </item>
    /// <item>
    ///     <term>Resumed</term>
    ///     <description>Start processing (e.g., <c>this.Enabled = true;</c>)</description>
    /// </item>
    /// <item>
    ///     <term>Shutdown</term>
    ///     <description>Dispose component (e.g., <c>this.Dispose();</c>)</description>
    /// </item>
    /// </list>
    /// Note that when component receives a "Paused" notification, the current enabled state
    /// should be cached so that it can be properly restored when the "Resumed" notification
    /// is received so that the component won't be started inadvertently.
    /// <example>
    /// Here is a typical implementation example:
    /// <code>
    /// private bool m_previouslyEnabled;
    /// .
    /// .
    /// .
    /// public void ServiceStateChanged(ServiceState newState)
    /// {
    ///    switch (newState)
    ///    {
    ///        case ServiceState.Paused:
    ///            m_previouslyEnabled = Enabled;
    ///            Enabled = false;
    ///            break;
    ///        case ServiceState.Resumed:
    ///            Enabled = m_previouslyEnabled;
    ///            break;
    ///        case ServiceState.Shutdown:
    ///            Dispose();
    ///            break;
    ///        default:
    ///            break;
    ///    }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public interface IServiceComponent : IStatusProvider
    {
        void ServiceStateChanged(ServiceState newState);

        void ProcessStateChanged(string processName, ProcessState newState);
    }
}