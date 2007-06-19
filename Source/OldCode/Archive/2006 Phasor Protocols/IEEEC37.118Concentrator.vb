Imports System.Data.OleDb
Imports TVA.Data.Common
Imports TVA.ErrorManagement
Imports PhasorProtocols

Public Class IeeeC37_118Concentrator

    Inherits PhasorDataConcentratorBase

    Public Sub New( _
        ByVal connection As OleDbConnection, _
        ByVal idCode As UInt16, _
        ByVal framesPerSecond As Integer, _
        ByVal lagTime As Double, _
        ByVal leadTime As Double, _
        ByVal exceptionLogger As GlobalExceptionLogger)

        MyBase.New(connection, idCode, framesPerSecond, lagTime, leadTime, exceptionLogger)

    End Sub

    Protected Overrides Function CreateConfigurationFrame(ByVal connection As OleDbConnection, ByVal idCode As UShort) As PhasorProtocols.IConfigurationFrame

        Dim configurationFrame As New IeeeC37_118.ConfigurationFrame(IeeeC37_118.FrameType.ConfigurationFrame2, 16777215, idCode, DateTime.UtcNow.Ticks, FramesPerSecond, 1)

        With RetrieveData("SELECT * FROM Pmu WHERE Enabled <> 0", connection).Rows
            For x As Integer = 0 To .Count - 1
                With .Item(x)
                    Dim cell As New IeeeC37_118.ConfigurationCell(configurationFrame, Convert.ToUInt16(.Item("AccessID")), LineFrequency.Hz60)

                    ' It will be critical to assign an ID label to the PMU in this virtual configuration

                    cell.AnalogDataFormat = DataFormat.FloatingPoint
                    cell.PhasorDataFormat = DataFormat.FloatingPoint
                    cell.PhasorCoordinateFormat = CoordinateFormat.Polar
                    cell.FrequencyDataFormat = DataFormat.FloatingPoint
                    'cell.NominalFrequency = LineFrequency.Hz60

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

                    configurationFrame.Cells.Add(cell)
                End With
            Next
        End With

        Return configurationFrame

    End Function

    Protected Overrides Function CreateNewFrame(ByVal ticks As Long) As TVA.Measurements.IFrame

        Dim configFrame As IeeeC37_118.ConfigurationFrame = DirectCast(MyBase.ConfigurationFrame, IeeeC37_118.ConfigurationFrame)
        Dim dataFrame As New IeeeC37_118.DataFrame(ticks, configFrame)

        For x As Integer = 0 To configFrame.Cells.Count - 1
            dataFrame.Cells.Add(New IeeeC37_118.DataCell(dataFrame, configFrame.Cells(x)))
        Next

        Return dataFrame

    End Function

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "C37.118 Concentrator"
        End Get
    End Property

End Class
