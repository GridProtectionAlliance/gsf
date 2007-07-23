Imports System.Data.OleDb
Imports TVA.Communication
Imports TVA.ErrorManagement
Imports PhasorProtocols

Public Class IeeeC37_118Concentrator

    Inherits PhasorDataConcentratorBase

    Private m_configurationFrame As IeeeC37_118.ConfigurationFrame
    Private m_timeBase As Integer
    Private m_version As Byte

    Public Sub New( _
        ByVal communicationServer As ICommunicationServer, _
        ByVal name As String, _
        ByVal framesPerSecond As Integer, _
        ByVal lagTime As Double, _
        ByVal leadTime As Double, _
        ByVal timeBase As Integer, _
        ByVal version As Byte, _
        ByVal exceptionLogger As GlobalExceptionLogger)

        MyBase.New(communicationServer, name, framesPerSecond, lagTime, leadTime, exceptionLogger)

        m_timeBase = timeBase
        m_version = version

    End Sub

    Protected Overrides Function CreateNewConfigurationFrame(ByVal baseConfiguration As IConfigurationFrame) As PhasorProtocols.IConfigurationFrame

        Dim x, y As Integer

        ' We create a new IEEE C37.118 configuration frame 2 based on input configuration
        m_configurationFrame = New IeeeC37_118.ConfigurationFrame(IeeeC37_118.FrameType.ConfigurationFrame2, m_timeBase, baseConfiguration.IDCode, DateTime.UtcNow.Ticks, FramesPerSecond, m_version)

        For x = 0 To baseConfiguration.Cells.Count - 1
            Dim baseCell As ConfigurationCell = baseConfiguration.Cells(x)
            Dim newCell As New IeeeC37_118.ConfigurationCell(m_configurationFrame, baseCell.IDCode, baseCell.NominalFrequency)

            ' Update other cell level attributes
            newCell.StationName = baseCell.StationName
            newCell.IDLabel = baseCell.IDLabel
            newCell.PhasorDataFormat = baseCell.PhasorDataFormat
            newCell.PhasorCoordinateFormat = baseCell.PhasorCoordinateFormat
            newCell.FrequencyDataFormat = baseCell.FrequencyDataFormat
            newCell.AnalogDataFormat = baseCell.AnalogDataFormat

            ' Add phasor definitions
            For y = 0 To baseCell.PhasorDefinitions.Count - 1
                newCell.PhasorDefinitions.Add(New IeeeC37_118.PhasorDefinition(newCell, baseCell.PhasorDefinitions(y)))
            Next

            ' Add frequency definition
            newCell.FrequencyDefinition = New IeeeC37_118.FrequencyDefinition(newCell, baseCell.FrequencyDefinition)

            ' Add analog definitions
            For y = 0 To baseCell.AnalogDefinitions.Count - 1
                newCell.AnalogDefinitions.Add(New IeeeC37_118.AnalogDefinition(newCell, baseCell.AnalogDefinitions(y)))
            Next

            ' Add digital definitions
            For y = 0 To baseCell.DigitalDefinitions.Count - 1
                newCell.DigitalDefinitions.Add(New IeeeC37_118.DigitalDefinition(newCell, baseCell.DigitalDefinitions(y)))
            Next

            ' Add new PMU configuration (cell) to protocol specific configuration frame
            m_configurationFrame.Cells.Add(newCell)
        Next

        Return m_configurationFrame

    End Function

    Protected Overrides Function CreateNewDataFrame(ByVal ticks As Long) As IDataFrame

        ' We create a new IEEE C37.118 data frame based on current configuration frame
        Dim dataFrame As New IeeeC37_118.DataFrame(ticks, m_configurationFrame)

        For x As Integer = 0 To m_configurationFrame.Cells.Count - 1
            dataFrame.Cells.Add(New IeeeC37_118.DataCell(dataFrame, m_configurationFrame.Cells(x)))
        Next

        Return dataFrame

    End Function

    Public ReadOnly Property TimeBase() As Integer
        Get
            Return m_timeBase
        End Get
    End Property

    Public ReadOnly Property Version() As Byte
        Get
            Return m_version
        End Get
    End Property

End Class
