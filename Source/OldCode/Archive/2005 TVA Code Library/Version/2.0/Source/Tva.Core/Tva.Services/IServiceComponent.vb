'*******************************************************************************************************
'  Tva.Xml.Common.vb - Common XML Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - James R Carroll
'       Original version of source code generated
'  01/23/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
'
'*******************************************************************************************************

Namespace Services

    ' Defines an interface for user created components used by the service so that components
    ' can inform service of current status and automatically react to service events
    Public Interface IServiceComponent

        Inherits IDisposable

        ' Define possible service states
        Enum ServiceState
            Started
            Stopped
            Paused
            Resumed
            ShutDown
        End Enum

        ' Define possible process states for service threads that will execute code
        Enum ProcessState
            Unprocessed
            Processing
            Processed
            Aborted
        End Enum

        ReadOnly Property Name() As String
        ReadOnly Property Status() As String
        Sub ServiceStateChanged(ByVal newState As ServiceState)
        Sub ProcessStateChanged(ByVal newState As ProcessState)

    End Interface

End Namespace