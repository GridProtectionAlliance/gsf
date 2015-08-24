'*******************************************************************************************************
'  AdapterBase.vb - Adpater base class
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
'  07/29/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Public MustInherit Class AdapterBase

    Implements IAdapter

    Public Sub Dispose() Implements IDisposable.Dispose

        Dispose(True)
        GC.SuppressFinalize(Me)

    End Sub

    Public Event StatusMessage(ByVal status As String) Implements IAdapter.StatusMessage

    Public MustOverride ReadOnly Property Name() As String Implements IAdapter.Name

    Public MustOverride ReadOnly Property Status() As String Implements IAdapter.Status

    Protected Overrides Sub Finalize()

        Dispose(True)

    End Sub

    Protected Overridable Sub UpdateStatus(ByVal message As String)

        RaiseEvent StatusMessage(message)

    End Sub

    Protected MustOverride Sub Dispose(ByVal disposing As Boolean)

End Class
