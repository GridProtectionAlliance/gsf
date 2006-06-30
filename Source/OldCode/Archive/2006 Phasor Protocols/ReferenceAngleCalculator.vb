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

Imports Tva.Common
Imports Tva.Measurements

Public Class ReferenceAngleCalculator

    ' We need to time align data before attempting to calculate reference angle
    Private m_concentrator As Concentrator
    Private m_referenceAngleMeasurementID As Integer
    Private m_angleMeasurements As List(Of Integer)
    Private m_angleCount As Integer
    Private m_lastAngles As Double()
    Private m_referenceAngles As List(Of IMeasurement)

    Public Sub New(ByVal referenceAngleMeasurementID As Integer, ByVal angleMeasurements As List(Of Integer), ByVal angleCount As Integer, ByVal framesPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        ' Because you want absolutue "minimum" possible delay when calculating phase angle, we pre-sort measurements
        ' used in phase angle calculation - this means you should use quick reporting, reliable PMU's to calculate
        ' phase angle and your local clock is required to be GPS synchronized...
        m_concentrator = New Concentrator(AddressOf PublishFrame, framesPerSecond, lagTime, leadTime)
        m_referenceAngleMeasurementID = referenceAngleMeasurementID
        m_referenceAngles = New List(Of IMeasurement)
        m_angleMeasurements = angleMeasurements
        m_angleCount = angleCount

    End Sub

    Public Sub SortReferenceMeasurements(ByVal measurements As Dictionary(Of Integer, IMeasurement))

        Dim measurement As IMeasurement = Nothing

        ' Sort all the reference angle measurements that were loaded from this frame
        For x As Integer = 0 To m_angleMeasurements.Count - 1
            If measurements.TryGetValue(m_angleMeasurements(x), measurement) Then
                m_concentrator.SortMeasurement(measurement)
            End If
        Next

    End Sub

    Public Function GetQueuedReferenceAngles() As IMeasurement()

        Dim measurements As IMeasurement()

        SyncLock m_referenceAngles
            measurements = m_referenceAngles.ToArray()
            m_referenceAngles.Clear()
        End SyncLock

        Return measurements

    End Function

    Private Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        If m_lastAngles IsNot Nothing Then
            ' Calculate reference angle
            Dim currentAngles As Double() = CreateArray(Of Double)(m_angleCount)

            If TryGetAngles(frame, currentAngles) Then
                Dim calculatedAngleMeasurement As New Measurement

                ' Queue up calculated angle for query by receiver that will archiver reference angle...
                SyncLock m_referenceAngles
                    m_referenceAngles.Add(calculatedAngleMeasurement)
                End SyncLock

                ' Track last gathered angles
                m_lastAngles = currentAngles
            End If
        End If

    End Sub

    Private Function TryGetAngles(ByVal frame As IFrame, ByVal angles As Double()) As Boolean

        Dim index As Integer
        Dim measurement As IMeasurement = Nothing

        For x As Integer = 0 To m_angleMeasurements.Count - 1
            If frame.Measurements.TryGetValue(m_angleMeasurements(x), measurement) Then
                angles(index) = measurement.Value
                index += 1
                If index = angles.Length Then Exit For
            End If
        Next

        Return (index = angles.Length)

    End Function

End Class
