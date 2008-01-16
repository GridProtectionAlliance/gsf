'*******************************************************************************************************
'  AverageFrequencyCalculator.vb - Average interconnect frequency calculator
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
'  11/22/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports TVA.Measurements
Imports InterfaceAdapters

Public Class AverageFrequencyCalculator

    Inherits CalculatedMeasurementAdapterBase

    Const LoFrequency As Double = 57.0R
    Const HiFrequency As Double = 62.0R

    Private m_averageFrequency As Double
    Private m_maximumFrequency As Double
    Private m_minimumFrequency As Double

    ' IMPORTANT: Make sure output SQL definition defines points in the following order
    Private Enum Output
        Avg
        Max
        Min
    End Enum

    Public Sub New()
    End Sub

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("    Last average frequency: ")
                .Append(m_averageFrequency)
                .AppendLine()
                .Append("    Last maximum frequency: ")
                .Append(m_maximumFrequency)
                .AppendLine()
                .Append("    Last minimum frequency: ")
                .Append(m_minimumFrequency)
                .AppendLine()
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    ''' <summary>
    ''' Calculates the average Eastern Interconnect frequency for all frequencies that have reported in specified lag time
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

        With frame.Measurements
            ' We need to get at least one frequency for this calculation...
            If .Count > 0 Then
                Dim frequency As Double
                Dim frequencyTotal As Double
                Dim maximumFrequency As Double = LoFrequency
                Dim minimumFrequency As Double = HiFrequency
                Dim total As Integer

                ' Calculate average magnitude
                For Each measurement As IMeasurement In .Values
                    frequency = measurement.AdjustedValue

                    ' Validate frequency
                    If frequency > LoFrequency AndAlso frequency < HiFrequency Then
                        frequencyTotal += frequency
                        If frequency > maximumFrequency Then maximumFrequency = frequency
                        If frequency < minimumFrequency Then minimumFrequency = frequency
                        total += 1
                    End If
                Next

                If total > 0 Then
                    m_averageFrequency = (frequencyTotal / total)
                    m_maximumFrequency = maximumFrequency
                    m_minimumFrequency = minimumFrequency
                End If

                ' Provide calculated measurements for external consumption
                PublishNewCalculatedMeasurements(New IMeasurement() { _
                    Measurement.Clone(OutputMeasurements(Output.Avg), m_averageFrequency, frame.Ticks), _
                    Measurement.Clone(OutputMeasurements(Output.Max), m_maximumFrequency, frame.Ticks), _
                    Measurement.Clone(OutputMeasurements(Output.Min), m_minimumFrequency, frame.Ticks)})
            Else
                m_averageFrequency = 0.0R
                m_maximumFrequency = 0.0R
                m_minimumFrequency = 0.0R

                ' TODO: Raise warning when minimum set of frequency's are not available for calculation - but not 30 times per second :)
                'RaiseCalculationException(
            End If
        End With

    End Sub

End Class
