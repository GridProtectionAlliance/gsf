'*******************************************************************************************************
'  ConnectionSettings.vb - Serializable Connection Settings
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
'  03/16/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports TVA.Communication
Imports TVA.PhasorProtocols

<Serializable()> _
Public Class ConnectionSettings

    Public PhasorProtocol As PhasorProtocol
    Public TransportProtocol As TransportProtocol
    Public ConnectionString As String
    Public PmuID As Integer
    Public FrameRate As Integer
    Public AutoRepeatPlayback As Boolean
    Public ByteEncodingDisplayFormat As Integer
    Public ConnectionParameters As IConnectionParameters

    Public ReadOnly Property This() As ConnectionSettings
        Get
            Return Me
        End Get
    End Property

End Class
