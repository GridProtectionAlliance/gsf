'*******************************************************************************************************
'  ReferenceMagnitudeCalculator.vb - Reference phase magnitude calculator
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
'  11/22/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports TVA.Measurements
Imports InterfaceAdapters

Public Class ReferenceMagnitudeCalculator

    Inherits CalculatedMeasurementAdapterBase

    Private m_referenceMagnitude As Double

    Public Sub New()
    End Sub

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append(" Last calculated magnitude: ")
                .Append(m_referenceMagnitude)
                .AppendLine()
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    ''' <summary>
    ''' Calculates the average magnitude for PMU's that contribute to the virtual Eastern Interconnect reference angle
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
            ' We need at least one reference angle PMU magnitude for this calculation...
            If .Count > 0 Then
                Dim magnitudeTotal As Double

                ' Calculate average magnitude
                For Each measurement As IMeasurement In .Values
                    magnitudeTotal += measurement.AdjustedValue
                Next

                m_referenceMagnitude = (magnitudeTotal / .Count)

                ' Provide calculated measurement for external consumption
                PublishNewCalculatedMeasurement(Measurement.Clone(OutputMeasurements(0), m_referenceMagnitude, frame.Ticks))
            Else
                m_referenceMagnitude = 0.0R

                ' TODO: Raise warning when minimum set of PMU's is not available for reference magnitude calculation - but not 30 times per second :)
                'RaiseCalculationException(
            End If
        End With

    End Sub

End Class
