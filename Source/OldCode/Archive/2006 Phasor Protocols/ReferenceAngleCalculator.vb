'*******************************************************************************************************
'  ReferenceAngleCalculator.vb - Reference phase angle calculator
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

Imports System.Text
Imports Tva.Common
Imports Tva.Measurements
Imports Tva.Collections.Common

' TODO: Move into an external class...
Public Class ReferenceAngleCalculator

    Public Event NewCalculatedMeasurement(ByVal measurement As IMeasurement)
    Public Event CalculationStatus(ByVal message As String)

    Private Const BackupQueueSize As Integer = 10

    ' We need to time align data before attempting to calculate reference angle
    Private WithEvents m_concentrator As Concentrator
    Private m_referenceAngleMeasurementID As Integer
    Private m_angleMeasurements As List(Of Integer)
    Private m_angleCount As Integer
    Private m_phaseResetAngle As Double
    Private m_lastAngles As Double()
    Private m_latestCalculatedAngles As List(Of Double)
    Private m_currentAngle As Double

    Public Sub New(ByVal referenceAngleMeasurementID As Integer, ByVal angleMeasurements As List(Of Integer), ByVal angleCount As Integer, ByVal framesPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        ' Because you want absolutue "minimum" possible delay when calculating phase angle, we pre-sort measurements
        ' used in phase angle calculation - this means you should use quick reporting, reliable PMU's to calculate
        ' phase angle and your local clock is required to be GPS synchronized...
        m_concentrator = New Concentrator(AddressOf PublishFrame, framesPerSecond, lagTime, leadTime)
        m_referenceAngleMeasurementID = referenceAngleMeasurementID
        m_latestCalculatedAngles = New List(Of Double)
        m_angleMeasurements = angleMeasurements
        m_angleCount = angleCount
        m_phaseResetAngle = m_angleCount * 360

    End Sub

    Public Sub QueueMeasurementsForCalculation(ByVal measurements As Dictionary(Of Integer, IMeasurement))

        Threading.ThreadPool.QueueUserWorkItem(AddressOf SortMeasurements, measurements)

    End Sub

    Private Sub SortMeasurements(ByVal state As Object)

        Dim measurements As Dictionary(Of Integer, IMeasurement) = state
        Dim measurement As IMeasurement = Nothing

        ' Sort all the relevant measurements that were loaded from the parsed data frame
        For x As Integer = 0 To m_angleMeasurements.Count - 1
            If measurements.TryGetValue(m_angleMeasurements(x), measurement) Then
                m_concentrator.SortMeasurement(measurement)
            End If
        Next

    End Sub

    Public ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("  Latest calculated angles: ")
                SyncLock m_latestCalculatedAngles
                    .Append(ListToString(m_latestCalculatedAngles, ","c))
                End SyncLock
                .Append(Environment.NewLine)
                .Append("      Ouput measurement ID: ")
                .Append(m_referenceAngleMeasurementID)
                .Append(Environment.NewLine)
                .Append("    Input measurement ID's: ")
                .Append(ListToString(m_angleMeasurements, ","c))
                .Append(Environment.NewLine)
                .Append("   Minimum required angles: ")
                .Append(m_angleCount)
                .Append(Environment.NewLine)
                .Append(m_concentrator.Status)
                Return .ToString()
            End With
        End Get
    End Property

    Private Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        Dim currentAngles As Double() = CreateArray(Of Double)(m_angleCount)
        Dim calculatedAngleMeasurement As New Measurement(m_referenceAngleMeasurementID, Double.NaN, frame.Ticks)
        Dim angleTotal, angleAverage As Double

        ' Attempt to get minimum needed reporting set of reference angle PMU's
        If TryGetAngles(frame, currentAngles) Then
            If m_lastAngles IsNot Nothing Then
                ' Calculate reference angle
                For x As Integer = 0 To m_angleCount - 1
                    angleTotal += currentAngles(x)
                Next

                ' We use modulus function to make sure angle is in range of 0 to 359
                angleAverage = (angleTotal / m_angleCount) Mod 360
            End If

            ' Track last used set of measurement angles
            m_lastAngles = currentAngles
        Else
            If m_latestCalculatedAngles.Count > 0 Then
                ' Use stack average when minimum set is below specified angle count
                For x As Integer = 0 To m_latestCalculatedAngles.Count - 1
                    angleTotal += m_latestCalculatedAngles(x)
                Next

                ' We use modulus function to make sure angle is in range of 0 to 359
                angleAverage = (angleTotal / m_latestCalculatedAngles.Count) Mod 360
            End If

            ' We mark quality bad on measurement when we fall back to stack average
            calculatedAngleMeasurement.ValueQualityIsGood = False

            ' TODO: Raise warning when minimum set of PMU's is not available for reference angle calculation - but not 30 times per second :)
        End If

        ' Slide angle value in range of -179 to +180
        If angleAverage > 180 Then angleAverage -= 360
        calculatedAngleMeasurement.Value = angleAverage
        m_currentAngle = angleAverage

        ' Provide calculated measurement for external consumption
        RaiseEvent NewCalculatedMeasurement(calculatedAngleMeasurement)

        ' Add calculated reference angle to latest angle queue (used as backup in case PMU's go offline)
        SyncLock m_latestCalculatedAngles
            With m_latestCalculatedAngles
                .Add(angleAverage)
                While .Count > BackupQueueSize
                    .RemoveAt(0)
                End While
            End With
        End SyncLock

    End Sub

    Private Function TryGetAngles(ByVal frame As IFrame, ByVal angles As Double()) As Boolean

        Dim index As Integer
        Dim measurement As IMeasurement = Nothing

        ' Move through angle measurements in priority order and try to get minimum needed set of reporting angles
        For x As Integer = 0 To m_angleMeasurements.Count - 1
            If frame.Measurements.TryGetValue(m_angleMeasurements(x), measurement) Then
                angles(index) = GetUnwrappedPhaseAngle(index, measurement.AdjustedValue)
                index += 1
                If index = m_angleCount Then Exit For
            End If
        Next

        Return (index = m_angleCount)

    End Function

    Private Function GetUnwrappedPhaseAngle(ByVal index As Integer, ByVal angle As Double) As Double

        ' Get angle difference from last run (if there was a last run)
        If m_lastAngles IsNot Nothing Then angle -= m_lastAngles(index)

        ' Unwrap phase angle, if needed
        If angle > 300 Then
            angle -= 360
        ElseIf angle < -300 Then
            angle += 360
        End If

        ' Reset unwrapped phase angle, if needed
        If angle > m_phaseResetAngle Then
            angle -= m_phaseResetAngle
        ElseIf angle < -m_phaseResetAngle Then
            angle += m_phaseResetAngle
        End If

        Return angle

    End Function

End Class
