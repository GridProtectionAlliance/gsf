Imports System.Data.OleDb
Imports Tva.Measurements
Imports Tva.Data.Common
Imports Tva.Common
Imports Tva.Phasors

Public Class PhasorDataConcentrator

    Private m_pmuCells As PmuInfoCollection
    Private WithEvents m_concentrator As Concentrator
    Private m_configuration As IeeeC37_118.ConfigurationFrame

    Public Sub New(ByVal connection As OleDbConnection, ByVal idCode As UInt16, ByVal framesPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        m_configuration = New IeeeC37_118.ConfigurationFrame(IeeeC37_118.FrameType.ConfigurationFrame2, 100000, idCode, DateTime.UtcNow.Ticks, framesPerSecond, 1)

        m_pmuCells = New PmuInfoCollection

        With RetrieveData("SELECT * FROM Pmu WHERE Enabled <> 0", connection).Rows
            For x As Integer = 0 To .Count - 1
                With .Item(x)
                    Dim pmu As New PmuInfo(.Item("AccessID"), .Item("ID"))
                    Dim cell As New IeeeC37_118.ConfigurationCell(m_configuration, Convert.ToUInt16(.Item("AccessID")), LineFrequency.Hz60)

                    cell.AnalogDataFormat = DataFormat.FloatingPoint
                    cell.PhasorDataFormat = DataFormat.FloatingPoint
                    cell.PhasorCoordinateFormat = CoordinateFormat.Polar
                    cell.FrequencyDataFormat = DataFormat.FloatingPoint
                    cell.NominalFrequency = LineFrequency.Hz60

                    ' Calculated measurements can be added as analogs or digitals in the system
                    'cell.AnalogDefinitions.Add(
                    'cell.DigitalDefinitions.Add(

                    ' Load all phasors as defined in the database
                    'cell.PhasorDefinitions.Add(New IeeeC37_118.PhasorDefinition(cell, index, label, scale, offset, CoordinateFormat.Polar, PhasorType.Current))

                    cell.FrequencyDefinition = New IeeeC37_118.FrequencyDefinition( _
                        cell, cell.IDLabel & " Frequency", _
                        Convert.ToInt32(.Item("FreqScale")), _
                        Convert.ToSingle(.Item("FreqOffset")), _
                        Convert.ToInt32(.Item("DfDtScale")), _
                        Convert.ToSingle(.Item("DfDtOffset")))

                    ' Initialize measurement list for each pmu keyed on the signal reference field
                    With RetrieveData("SELECT * FROM Measurements WHERE PmuID=" & .Item("ID"), connection)
                        For y As Integer = 0 To .Rows.Count - 1
                            With .Rows(y)
                                pmu.Measurements.Add(.Item("SignalReference"), _
                                    New Measurement( _
                                        Convert.ToInt32(.Item("PointID")), _
                                        .Item("HistorianID").ToString(), _
                                        .Item("SignalReference").ToString(), _
                                        Convert.ToDouble(.Item("Adder")), _
                                        Convert.ToDouble(.Item("Multiplier"))))
                            End With
                        Next
                    End With

                    m_configuration.Cells.Add(cell)
                    m_pmuCells.Add(pmu)
                End With
            Next
        End With

        m_concentrator = New Concentrator(AddressOf PublishFrame, Nothing, framesPerSecond, lagTime, leadTime)

    End Sub

    Private Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

    End Sub

    'Protected Overridable Function TryGetMeasurements(ByVal frame As IFrame, ByRef measurements As IMeasurement()) As Boolean

    '    Dim index As Integer
    '    Dim measurement As IMeasurement = Nothing

    '    If measurements Is Nothing Then measurements = CreateArray(Of IMeasurement)(m_minimumMeasurementsToUse)

    '    ' Loop through all input measurements to see if they exist in this frame
    '    For x As Integer = 0 To m_inputMeasurements.Length - 1
    '        If frame.Measurements.TryGetValue(m_inputMeasurements(x).Key, measurement) Then
    '            measurements(index) = measurement
    '            index += 1
    '            If index = m_minimumMeasurementsToUse Then Exit For
    '        End If
    '    Next

    '    Return (index = m_minimumMeasurementsToUse)

    'End Function

    Private Sub m_concentrator_ProcessException(ByVal ex As System.Exception) Handles m_concentrator.ProcessException

    End Sub

    Private Sub m_concentrator_SamplePublished(ByVal sample As Tva.Measurements.Sample) Handles m_concentrator.SamplePublished

    End Sub

    Private Sub m_concentrator_UnpublishedSamples(ByVal total As Integer) Handles m_concentrator.UnpublishedSamples

    End Sub

End Class
