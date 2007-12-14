'*******************************************************************************************************
'  SendToNull.vb - Virtual historian adpater (sends data to nowhere - just for debug sessions)
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
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

Imports TVA.Measurements
Imports InterfaceAdapters

Public Class SendToNull

    Inherits HistorianAdapterBase

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "Null Historian Adapter"
        End Get
    End Property

    Public Overrides Sub Initialize(ByVal connectionString As String)
    End Sub

    Protected Overrides Sub AttemptConnection()
    End Sub

    Protected Overrides Sub AttemptDisconnection()
    End Sub

    Protected Overrides Sub ArchiveMeasurements(ByVal measurements As IMeasurement())
    End Sub

End Class
