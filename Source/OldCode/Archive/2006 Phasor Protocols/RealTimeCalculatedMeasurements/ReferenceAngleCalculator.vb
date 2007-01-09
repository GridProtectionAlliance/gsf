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
Imports Tva.Math.Common
Imports Tva.Measurements
Imports Tva.Collections.Common
Imports Tva.IO
Imports Tva.IO.FilePath
Imports InterfaceAdapters

Public Class ReferenceAngleCalculator

    Inherits CalculatedMeasurementAdapterBase

    Private Const BackupQueueSize As Integer = 10
    Private m_phaseResetAngle As Double
    Private m_lastAngles As Dictionary(Of MeasurementKey, Double)
    Private m_latestCalculatedAngles As List(Of Double)
    Private m_measurements As IMeasurement()

#If DEBUG Then
    Private m_frameLog As LogFile
#End If

    Public Sub New()

        m_lastAngles = New Dictionary(Of MeasurementKey, Double)
        m_latestCalculatedAngles = New List(Of Double)

#If DEBUG Then
        m_frameLog = New LogFile(GetApplicationPath() & "ReferenceAngleLog.txt")
#End If

    End Sub

    Public Overrides Sub Initialize(ByVal outputMeasurements As IMeasurement(), ByVal inputMeasurementKeys As MeasurementKey(), ByVal minimumMeasurementsToUse As Integer, ByVal expectedMeasurementsPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        MyBase.Initialize(outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, LeadTime)
        MyClass.MinimumMeasurementsToUse = minimumMeasurementsToUse

    End Sub

    Public Overrides Property MinimumMeasurementsToUse() As Integer
        Get
            Return MyBase.MinimumMeasurementsToUse
        End Get
        Set(ByVal value As Integer)
            MyBase.MinimumMeasurementsToUse = value
            m_phaseResetAngle = value * 360
        End Set
    End Property

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "Interconnect Reference Angle Calculator"
        End Get
    End Property

    Public Overrides ReadOnly Property Status() As String
        Get
            Const ValuesToShow As Integer = 4

            With New StringBuilder
                .Append(Name & " Status:")
                .Append(Environment.NewLine)
                .Append("  Last " & ValuesToShow & " calculated angles: ")
                SyncLock m_latestCalculatedAngles
                    If m_latestCalculatedAngles.Count > ValuesToShow Then
                        .Append(ListToString(m_latestCalculatedAngles.GetRange(m_latestCalculatedAngles.Count - ValuesToShow - 1, ValuesToShow), ","c))
                    Else
                        .Append("Not enough values calculated yet...")
                    End If
                End SyncLock
                .Append(Environment.NewLine)
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    ''' <summary>
    ''' Calculates the "virtual" Eastern Interconnect reference angle
    ''' </summary>
    ''' <param name="frame">Single frame of measurement data within a one second sample</param>
    ''' <param name="index">Index of frame within the one second sample</param>
    ''' <remarks>
    ''' The frame.Measurements property references a dictionary, keyed on each measurement's MeasurementKey, containing
    ''' all available measurements as defined by the InputMeasurementKeys property that arrived within the specified
    ''' LagTime.  Note that this function will be called with a frequency specified by the ExpectedMeasurementsPerSecond
    ''' property, so make sure all work to be done is executed as efficiently as possible.
    ''' </remarks>
    Protected Overrides Sub PerformCalculation(ByVal frame As IFrame, ByVal index As Integer)

        Dim currentAngles As New Dictionary(Of MeasurementKey, Double)
        Dim calculatedMeasurement As Measurement = Measurement.Clone(OutputMeasurements(0), frame.Ticks)
        Dim angleTotal, angleAverage, lastValue As Double
        Dim x As Integer

#If DEBUG Then
        LogFrameDetail(frame, index)
#End If

        ' Attempt to get minimum needed reporting set of reference angle PMU's
        If TryGetMinimumNeededMeasurements(frame, m_measurements) Then
            ' Total all phase angles, unwrapping angles if needed
            For x = 0 To MinimumMeasurementsToUse - 1
                ' Get angle difference from last run (if there was a last run)
                If m_lastAngles.TryGetValue(m_measurements(x).Key, lastValue) Then
                    angleTotal += GetUnwrappedPhaseAngle(m_measurements(x).AdjustedValue, lastValue)
                Else
                    angleTotal += m_measurements(x).AdjustedValue
                End If
            Next

            ' We use modulus function to make sure angle is in range of 0 to 359
            angleAverage = (angleTotal / MinimumMeasurementsToUse) Mod 360

            ' Track last angles for next run
            m_lastAngles.Clear()

            For x = 0 To MinimumMeasurementsToUse - 1
                m_lastAngles(m_measurements(x).Key) = m_measurements(x).AdjustedValue
            Next
        Else
            ' Use stack average when minimum set is below specified angle count
            angleAverage = Average(m_latestCalculatedAngles) Mod 360

            ' We mark quality bad on measurement when we fall back to stack average
            calculatedMeasurement.ValueQualityIsGood = False

#If DEBUG Then
            ' TODO: Raise warning when minimum set of PMU's is not available for reference angle calculation - but not 30 times per second :)
            'RaiseCalculationException(
            LogFrameWarning("WARNING: Minimum set of PMU's not available for reference angle calculation - using rolling average")
#End If
        End If

        ' Slide angle value in range of -179 to +180
        If angleAverage > 180 Then angleAverage -= 360
        If angleAverage < -179 Then angleAverage += 360
        calculatedMeasurement.Value = angleAverage

#If DEBUG Then
        LogFrameWarning("Calculated reference angle: " & angleAverage)
#End If

        ' Provide calculated measurement for external consumption
        PublishNewCalculatedMeasurement(calculatedMeasurement)

        ' Add calculated reference angle to latest angle queue used as backup in case
        ' needed minimum number of PMU's go offline or are slow reporting
        SyncLock m_latestCalculatedAngles
            With m_latestCalculatedAngles
                .Add(angleAverage)
                While .Count > BackupQueueSize
                    .RemoveAt(0)
                End While
            End With
        End SyncLock

    End Sub

    Private Function GetUnwrappedPhaseAngle(ByVal angle As Double, ByVal lastAngle As Double) As Double

        Dim deltaAngle As Double = angle - lastAngle

        ' Unwrap phase angle, if needed
        If deltaAngle > 300 Then
            angle -= 360
        ElseIf deltaAngle < -300 Then
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

#If DEBUG Then

    Private Sub LogFrameDetail(ByVal frame As IFrame, ByVal frameIndex As Integer)

        With m_frameLog
            ' Received frame to publish
            .AppendLine("***************************************************************************************")
            .AppendLine("   Frame Time: " & frame.Timestamp.ToString("HH:mm:ss.fff"))
            .AppendLine("  Frame Index: " & frameIndex)
            .AppendLine(" Measurement Detail - " & frame.Measurements.Values.Count & " total:")

            .Append("        Keys: ")
            For Each measurement As IMeasurement In frame.Measurements.Values
                .Append(measurement.Key.ToString().PadLeft(10) & " ")
            Next
            .AppendLine("")

            .Append("      Values: ")
            For Each measurement As IMeasurement In frame.Measurements.Values
                .Append(measurement.Value.ToString("0.000").PadLeft(10) & " ")
            Next
            .AppendLine("")
        End With

    End Sub

    Private Sub LogFrameWarning(ByVal warning As String)

        m_frameLog.AppendLine("[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "]: " & warning)

    End Sub

#End If

End Class
