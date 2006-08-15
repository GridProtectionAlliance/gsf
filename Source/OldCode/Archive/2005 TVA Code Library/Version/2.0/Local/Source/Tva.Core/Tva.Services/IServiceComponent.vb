'*******************************************************************************************************
'  Tva.Xml.Common.vb - Common XML Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - J. Ritchie Carroll
'       Original version of source code generated
'  01/23/2006 - J. Ritchie Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
'
'*******************************************************************************************************

Namespace Services

    ' Defines an interface for user created components used by the service so that components
    ' can inform service of current status and automatically react to service events
    Public Interface IServiceComponent

        Inherits IDisposable

        ReadOnly Property Name() As String
        ReadOnly Property Status() As String
        Sub ServiceStateChanged(ByVal newState As ServiceState)
        Sub ProcessStateChanged(ByVal processName As String, ByVal newState As ProcessState)

    End Interface

End Namespace