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
'  01/16/2007 - Jian Zuo(Ryan) jrzuo@tva.gov
'       Implement the unwrap offset of the angle
'  01/17/2006 - J. Ritchie Carroll
'       Added code to detect data set changes (i.e., PMU's online/offline)
'  01/17/2007 - Jian Zuo(Ryan) jrzuo@tva.gov
'       Added code to handle unwrap offset initialization and reset
'
'*******************************************************************************************************

Imports System.Text
Imports System.Math
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
    Private m_unwrapoffset As Dictionary(Of MeasurementKey, Double)
    Private m_latestCalculatedAngles As List(Of Double)
    Private m_measurements As IMeasurement()
    Private m_lastMeasurements As List(Of MeasurementKey)

#If DEBUG Then
    Private m_frameLog As LogFile
#End If

    Public Sub New()

        m_lastAngles = New Dictionary(Of MeasurementKey, Double)
        m_unwrapoffset = New Dictionary(Of MeasurementKey, Double)
        m_latestCalculatedAngles = New List(Of Double)
        m_lastMeasurements = New List(Of MeasurementKey)

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
        Dim angleTotal, angleAverage, lastValue, unwrapOffset As Double
        Dim angleRef, angleDelta0, angleDelta1, angleDelta2 As Double
        Dim dataSetChanged As Boolean
        Dim x As Integer

#If DEBUG Then
        LogFrameDetail(frame, index)
#End If

        ' Attempt to get minimum needed reporting set of reference angle PMU's
        If TryGetMinimumNeededMeasurements(frame, m_measurements) Then
            ' See if data set has changed since last run
            If m_lastMeasurements.Count > 0 AndAlso m_lastMeasurements.Count = m_measurements.Length Then
                For x = 0 To MinimumMeasurementsToUse - 1
                    If m_lastMeasurements.BinarySearch(m_measurements(x).Key) < 0 Then
                        dataSetChanged = True
                        Exit For
                    End If
                Next
            Else
                dataSetChanged = True
            End If

            ' Recreate data set list if data has changed
            If dataSetChanged Then
                m_lastMeasurements.Clear()

                For x = 0 To MinimumMeasurementsToUse - 1
                    m_lastMeasurements.Add(m_measurements(x).Key)
                Next

                m_lastMeasurements.Sort()

                ' Reinitialize all angle calculation variables
                m_lastAngles.Clear()
                m_unwrapoffset.Clear()

                angleRef = m_measurements(0).AdjustedValue

                For x = 0 To MinimumMeasurementsToUse - 1
                    angleDelta0 = Abs(m_measurements(x).AdjustedValue - angleRef)
                    angleDelta1 = Abs(m_measurements(x).AdjustedValue + 360.0R - angleRef)
                    angleDelta2 = Abs(m_measurements(x).AdjustedValue - 360.0R - angleRef)

                    If angleDelta0 < angleDelta1 AndAlso angleDelta0 < angleDelta2 Then
                        unwrapOffset = 0.0R
                    ElseIf angleDelta1 < angleDelta2 Then
                        unwrapOffset = 360.0R
                    Else
                        unwrapOffset = -360.0R
                    End If

                    m_unwrapoffset(m_measurements(x).Key) = unwrapOffset
                Next
            End If

            ' Total all phase angles, unwrapping angles if needed
            For x = 0 To MinimumMeasurementsToUse - 1
                ' Get the unwrap offset for this angle
                If m_unwrapoffset.TryGetValue(m_measurements(x).Key, unwrapOffset) Then
                    angleTotal += GetUnwrappedPhaseAngle(m_measurements(x).AdjustedValue, lastValue, unwrapOffset)
                Else
                    ' Not finding the unwrap offset would be unexpected, but we'll try to handle gracefully
                    unwrapOffset = 0.0R
                End If

                ' Get angle difference from last run (if there was a last run)
                If m_lastAngles.TryGetValue(m_measurements(x).Key, lastValue) Then
                    angleTotal += GetUnwrappedPhaseAngle(m_measurements(x).AdjustedValue, lastValue, unwrapOffset)

                    If unwrapOffset > m_phaseResetAngle Then
                        unwrapOffset -= m_phaseResetAngle
                    ElseIf unwrapOffset < -m_phaseResetAngle Then
                        unwrapOffset += m_phaseResetAngle
                    End If

                    m_unwrapoffset(m_measurements(x).Key) = unwrapOffset
                Else
                    ' No last run (starting up or reinitialized), so take raw angle and add offset
                    angleTotal += (m_measurements(x).AdjustedValue + unwrapOffset)
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

    Private Function GetUnwrappedPhaseAngle(ByVal angle As Double, ByVal lastAngle As Double, ByRef unwrapoffset As Double) As Double

        Dim deltaAngle As Double = angle - lastAngle

        ' Unwrap phase angle, if needed
        If deltaAngle > 300 Then
            unwrapoffset -= 360
        ElseIf deltaAngle < -300 Then
            unwrapoffset += 360
        End If

        angle += unwrapoffset

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
