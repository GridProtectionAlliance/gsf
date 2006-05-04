'*******************************************************************************************************
'  EventProcessor.vb - Standard DatAWare data event processor
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
'  05/03/2006 - James R Carroll
'       Initial version of source imported from 1.1 code library
'
'*******************************************************************************************************

' Standard event processor (used to convert eventBuffer (e.g., buffer received over socket stream) into standard events)
Public Class EventProcessor

    Private m_processEvent As ProcessEventSignature

    Public Sub New(ByVal processEventFunction As ProcessEventSignature)

        m_processEvent = processEventFunction

    End Sub

    Public Sub ProcessEventBuffer(ByVal eventBuffer As Byte(), ByVal offset As Integer, ByVal length As Integer)

        If eventBuffer Is Nothing Then Throw New ArgumentNullException("No event buffer was provided for processing")

        ' Parse standard DatAWare events out of network data packet
        For packetIndex As Integer = offset To length - 1 Step StandardEvent.BinaryLength
            If packetIndex + StandardEvent.BinaryLength < eventBuffer.Length Then
                m_processEvent(New StandardEvent(eventBuffer, packetIndex))
            End If
        Next

    End Sub

End Class
