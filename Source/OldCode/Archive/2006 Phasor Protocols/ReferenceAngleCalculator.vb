'*******************************************************************************************************
'  PhasorMeasurementReceiver.vb - Phasor measurement acquisition class
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
'  05/19/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Measurements

Public Class ReferenceAngleCalculator

    ' We need to time align data before attempting to calculate reference angle
    Private m_concentrator As Concentrator
    Private m_lastFrame As IFrame

    Public Sub New(ByVal framesPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        m_concentrator = New Concentrator(AddressOf PublishFrame, framesPerSecond, lagTime, leadTime)

    End Sub

    Private Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        If m_lastFrame IsNot Nothing Then
            ' Calculate reference angle

        End If

        m_lastFrame = frame

    End Sub

End Class
