'*******************************************************************************************************
'  CumberlandPowerDeviationCalculator.vb - Cumberland MW standard deviation calculator
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
'  11/08/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Math
Imports TVA.Common
Imports TVA.Measurements
Imports TVA.Collections.Common
Imports TVA.Math.Common
Imports TVA.IO
Imports TVA.IO.FilePath
Imports InterfaceAdapters

Public Class CumberlandPowerDeviationCalculator

    Inherits CalculatedMeasurementAdapterBase

    Private Const SampleSize As Integer = 15                        ' In seconds
    Private Const DegreesToRadians As Double = 180.0R / Math.PI     ' 180 / PI
    Private Const MegaVoltsOver3 As Double = 1000000.0R / 3.0R      ' MV / 3
    Private Const EnergizedThreshold As Double = 58000.0R           ' 20% of nominal line-to-neutral voltage

    ' Define the input measurement ID's needed to perform this calculation 
    Private m_bus1VM As New MeasurementKey(1608, "P2")      ' TVA_CUMB-BUS1:ABBV
    Private m_bus1VA As New MeasurementKey(1609, "P2")      ' TVA_CUMB-BUS1:ABBVH
    Private m_bus2VM As New MeasurementKey(1610, "P2")      ' TVA_CUMB-BUS2:ABBV
    Private m_bus2VA As New MeasurementKey(1611, "P2")      ' TVA_CUMB-BUS2:ABBVH
    Private m_marsIM As New MeasurementKey(1612, "P2")      ' TVA_CUMB-MARS:ABBI
    Private m_marsIA As New MeasurementKey(1615, "P2")      ' TVA_CUMB-MARS:ABBIH
    Private m_johnIM As New MeasurementKey(1616, "P2")      ' TVA_CUMB-JOHN:ABBI
    Private m_johnIA As New MeasurementKey(1619, "P2")      ' TVA_CUMB-JOHN:ABBIH
    Private m_davdIM As New MeasurementKey(1620, "P2")      ' TVA_CUMB-DAVD:ABBI
    Private m_davdIA As New MeasurementKey(1623, "P2")      ' TVA_CUMB-DAVD:ABBIH

    ' Define the output measurements that will be produced by this calculation
    Private m_cumbPower As New Measurement(2711, "P2")      ' TVA_CUMB-MW
    Private m_cumbStDev As New Measurement(2712, "P2")      ' TVA_CUMB-MWSTDEV

    Private m_minimumSamples As Integer
    Private m_powerDataSample As List(Of Double)
    Private m_lastStDev As Double

    '#If DEBUG Then
    '    Private m_frameLog As LogFile
    '    Private m_publishedFrames As Long
    '#End If

    Public Sub New()

        m_powerDataSample = New List(Of Double)

        '#If DEBUG Then
        '        m_frameLog = New LogFile(GetApplicationPath() & "PowerDeviationDetectorLog.txt")
        '#End If

    End Sub

    Public Overrides Sub Initialize(ByVal calculationName As String, ByVal configurationSection As String, ByVal outputMeasurements As IMeasurement(), ByVal inputMeasurementKeys As MeasurementKey(), ByVal minimumMeasurementsToUse As Integer, ByVal expectedMeasurementsPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        ' Base class will automatically filter and time-align needed measurements from all real-time incoming data
        MyBase.Initialize(calculationName, configurationSection, outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime)

        ' In this calculation, we manually initialize the output measurements to use for the base class since they are
        ' hard-coded set of outputs that will not change (i.e., no need to specify output measurements from SQL)
        If outputMeasurements Is Nothing OrElse outputMeasurements.Length = 0 Then
            MyClass.OutputMeasurements = New IMeasurement() _
                { _
                    m_cumbPower, _
                    m_cumbStDev _
                }
        End If

        ' In this calculation, we manually initialize the input measurement ID's to use for the base class since they are
        ' a hard-coded set of inputs that will not change (i.e., no need to specifiy input measurement ID's from SQL)
        If inputMeasurementKeys Is Nothing OrElse inputMeasurementKeys.Length = 0 Then
            With New List(Of MeasurementKey)
                .Add(m_bus1VM)
                .Add(m_bus1VA)
                .Add(m_bus2VM)
                .Add(m_bus2VA)
                .Add(m_marsIM)
                .Add(m_marsIA)
                .Add(m_johnIM)
                .Add(m_johnIA)
                .Add(m_davdIM)
                .Add(m_davdIA)
                MyClass.InputMeasurementKeys = .ToArray()
                MyClass.MinimumMeasurementsToUse = .Count
            End With
        End If

        ' Calculate minimum needed sample size
        m_minimumSamples = SampleSize * expectedMeasurementsPerSecond

    End Sub

    ''' <summary>
    ''' Calculates the Cumberland standard deviation of power output used to detect power system events
    ''' </summary>
    ''' <param name="frame">Single frame of measurement data within a one second sample</param>
    ''' <param name="index">Index of frame within the one second sample</param>
    ''' <remarks>
    ''' The frame.Measurements property references a dictionary, keyed on each measurement's MeasurementKey, containing
    ''' all available measurements as defined by the InputMeasurementKeys property that arrived within the specified
    ''' LagTime.  Note that this function will be called with a frequency specified by the ExpectedMeasurementsPerSecond
    ''' property, so make sure all work to be done is executed as efficiently as possible.
    ''' </remarks>
    Protected Overrides Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        Dim bus1VM, bus1VA, bus2VM, bus2VA, marsIM, marsIA, johnIM, johnIA, davdIM, davdIA As IMeasurement
        Dim busVM, busVA, cumbMW As Double

        '#If DEBUG Then
        '        m_publishedFrames += 1
        '        LogFrameDetail(frame, index)
        '        If m_publishedFrames Mod 300 = 0 Then m_frameLog.Write(Status)
        '#End If

        ' Attempt to retrieve all the measurements needed for this calculation from the time-aligned data frame
        With frame.Measurements
            If _
                .TryGetValue(m_bus1VM, bus1VM) AndAlso _
                .TryGetValue(m_bus1VA, bus1VA) AndAlso _
                .TryGetValue(m_bus2VM, bus2VM) AndAlso _
                .TryGetValue(m_bus2VA, bus2VA) AndAlso _
                .TryGetValue(m_marsIM, marsIM) AndAlso _
                .TryGetValue(m_marsIA, marsIA) AndAlso _
                .TryGetValue(m_johnIM, johnIM) AndAlso _
                .TryGetValue(m_johnIA, johnIA) AndAlso _
                .TryGetValue(m_davdIM, davdIM) AndAlso _
                .TryGetValue(m_davdIA, davdIA) _
            Then
                ' In case bus 1 is de-energized, we fall back on bus 2
                If bus1VM.AdjustedValue > EnergizedThreshold Then
                    busVM = bus1VM.AdjustedValue
                    busVA = bus1VA.AdjustedValue
                Else
                    busVM = bus2VM.AdjustedValue
                    busVA = bus2VA.AdjustedValue
                End If

                'MW = (MARS:ABBI*Cos((MARS:ABBIH-BUS1:ABBVH)/57.2958)
                '     + JOHN:ABBI*Cos((JOHN:ABBIH-BUS1:ABBVH)/57.2958)
                '     + DAVD:ABBI*Cos((DAVD:ABBIH-BUS1:ABBVH)/57.2958))
                '     * BUS1:ABBV / 333333.3

                ' Calculate cumberland power based on combined line measurements at cumberland substation
                cumbMW = ( _
                    marsIM.AdjustedValue * Cos((marsIA.AdjustedValue - busVA) / DegreesToRadians) + _
                    johnIM.AdjustedValue * Cos((johnIA.AdjustedValue - busVA) / DegreesToRadians) + _
                    davdIM.AdjustedValue * Cos((davdIA.AdjustedValue - busVA) / DegreesToRadians)) * _
                    busVM / MegaVoltsOver3

                ' Add latest calculated power to data sample
                SyncLock m_powerDataSample
                    ' Add newst power calculation...
                    m_powerDataSample.Add(cumbMW)

                    ' Keep sample size steady...
                    While m_powerDataSample.Count > m_minimumSamples
                        m_powerDataSample.RemoveAt(0)
                    End While
                End SyncLock

                '#If DEBUG Then
                '            Else
                '                LogFrameWarning("Not all needed measurements were available to perform calculation")
                '#End If
            End If
        End With

        ' Check to see if the needed number of samples are available to begin producing the standard deviation output measurement
        If m_powerDataSample.Count >= m_minimumSamples Then
            ' Perform standard deviation of current power sample and publish measurements
            Dim powerMeasurement As Measurement = Measurement.Clone(m_cumbPower, frame.Ticks)
            Dim stdevMeasurement As Measurement = Measurement.Clone(m_cumbStDev, frame.Ticks)

            ' We sync-lock data sample because "Status" call can be invoked on another thread at any time...
            SyncLock m_powerDataSample
                powerMeasurement.Value = m_powerDataSample(m_powerDataSample.Count - 1)
                stdevMeasurement.Value = StandardDeviation(m_powerDataSample)
            End SyncLock

            ' Provide calculated measurements for external consumption
            PublishNewCalculatedMeasurements(New IMeasurement() {powerMeasurement, stdevMeasurement})

            ' Track last standard deviation...
            m_lastStDev = stdevMeasurement.Value
        ElseIf cumbMW > 0 Then
            ' If not, we can still start publishing power calculation as soon as we have one...
            PublishNewCalculatedMeasurement(Measurement.Clone(m_cumbPower, cumbMW, frame.Ticks))
        End If

    End Sub

    Public Overrides ReadOnly Property Status() As String
        Get
            Const ValuesToShow As Integer = 3

            With New StringBuilder
                .Append("  Latest " & ValuesToShow & " MegaWatt values: ")
                SyncLock m_powerDataSample
                    If m_powerDataSample.Count > ValuesToShow Then
                        .Append(ListToString(m_powerDataSample.GetRange(m_powerDataSample.Count - ValuesToShow - 1, ValuesToShow), ","c))
                    Else
                        .Append("Not enough values calculated yet...")
                    End If
                End SyncLock
                .AppendLine()
                .Append("     Latest StDev of Power: ")
                .Append(m_lastStDev)
                .AppendLine()
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    '#If DEBUG Then

    '    Private Sub LogFrameDetail(ByVal frame As IFrame, ByVal frameIndex As Integer)

    '        With m_frameLog
    '            ' Received frame to publish
    '            .WriteLine("***************************************************************************************")
    '            .WriteLine("   Frame Time: " & frame.Timestamp.ToString("HH:mm:ss.fff"))
    '            .WriteLine("  Frame Index: " & frameIndex)
    '            .WriteLine(" Measurement Detail - " & frame.Measurements.Values.Count & " total:")

    '            .Write("        Keys: ")
    '            For Each measurement As IMeasurement In frame.Measurements.Values
    '                .Write(measurement.Key.ToString().PadLeft(10) & " ")
    '            Next
    '            .WriteLine("")

    '            .Write("      Values: ")
    '            For Each measurement As IMeasurement In frame.Measurements.Values
    '                .Write(measurement.Value.ToString("0.000").PadLeft(10) & " ")
    '            Next
    '            .WriteLine("")
    '        End With

    '    End Sub

    '    Private Sub LogFrameWarning(ByVal warning As String)

    '        m_frameLog.WriteLine("[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "] WARNING: " & warning)

    '    End Sub

    '#End If

End Class
