'*******************************************************************************************************
'  IAdapter.vb - Abstract adpater interface
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/01/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Public Interface IAdapter

    Inherits IDisposable

    ''' <summary>Sends informational message from adapter to host</summary>
    Event StatusMessage(ByVal status As String)

    ''' <summary>Name of the adapter</summary>
    ReadOnly Property Name() As String

    ''' <summary>Provides current status information about the adapter</summary>
    ReadOnly Property Status() As String

End Interface
