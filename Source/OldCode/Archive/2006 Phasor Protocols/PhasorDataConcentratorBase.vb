Imports System.Data.OleDb
Imports TVA.Measurements
Imports TVA.Data.Common
Imports TVA.Common
Imports PhasorProtocols

Public MustInherit Class PhasorDataConcentrator

    Inherits Concentrator

    Private m_configurationFrame As IConfigurationFrame
    Private m_signalReferences As Dictionary(Of MeasurementKey, SignalReference)

    Protected Sub New(ByVal connection As OleDbConnection, ByVal idCode As UInt16, ByVal framesPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        MyBase.New(framesPerSecond, lagTime, leadTime)

        Dim signal As SignalReference
        Dim idLabelCellIndex As New Dictionary(Of String, Integer)

        ' Define configuration frame
        m_configurationFrame = CreateConfigurationFrame(connection, idCode)

        ' Define measurement to signal cross reference dictionary
        m_signalReferences = New Dictionary(Of MeasurementKey, SignalReference)

        ' Initialize measurement list for each pmu keyed on the signal reference field
        With RetrieveData("SELECT * FROM ActiveMeasurements", connection).Rows
            For x As Integer = 0 To .Count - 1
                signal = New SignalReference(.Item("SignalReference").ToString())

                ' Lookup cell index by acronym. Doing this work upfront will save a huge amount
                ' of work during primary measurement sorting
                If Not idLabelCellIndex.TryGetValue(signal.PmuAcronym, signal.PmuCellIndex) Then
                    ' We cache these indicies locally to speed up initialization - we'll be
                    ' requesting these indexes for the same PMU's over and over
                    signal.PmuCellIndex = m_configurationFrame.Cells.IndexOfIDLabel(signal.PmuAcronym)
                    idLabelCellIndex.Add(signal.PmuAcronym, signal.PmuCellIndex)
                End If

                m_signalReferences.Add(New MeasurementKey(Convert.ToInt32(.Item("PointID")), .Item("Historian").ToString()), signal)
            Next
        End With

        ' TODO: Start Concentrator!
        'Me.Enabled = True

    End Sub

    Public ReadOnly Property ConfigurationFrame() As IConfigurationFrame
        Get
            Return m_configurationFrame
        End Get
    End Property

    Protected MustOverride Function CreateConfigurationFrame(ByVal connection As OleDbConnection, ByVal idCode As UInt16) As IConfigurationFrame

    Protected MustOverride Function CreateDataFrame(ByVal ticks As Long) As IDataFrame

    Protected Overrides Function CreateNewFrame(ByVal ticks As Long) As TVA.Measurements.IFrame

        ' Create a new data frame to publish
        Return CreateDataFrame(ticks)

    End Function

    Protected Overrides Sub AssignMeasurementToFrame(ByVal frame As TVA.Measurements.IFrame, ByVal measurement As TVA.Measurements.IMeasurement)
        MyBase.AssignMeasurementToFrame(frame, measurement)
    End Sub

    Protected Overrides Sub PublishFrame(ByVal frame As TVA.Measurements.IFrame, ByVal index As Integer)

    End Sub

    Private Sub PhasorDataConcentrator_ProcessException(ByVal ex As System.Exception) Handles Me.ProcessException

    End Sub

    Private Sub PhasorDataConcentrator_SamplePublished(ByVal sample As TVA.Measurements.Sample) Handles Me.SamplePublished

    End Sub

    Private Sub PhasorDataConcentrator_UnpublishedSamples(ByVal total As Integer) Handles Me.UnpublishedSamples

    End Sub


    'Private Function CreateNewDataFrame(ByVal ticks As Long) As IFrame

    '    ' Create a new data frame to publish
    '    Dim sampleIndex As Integer = Convert.ToInt32(TicksBeyondSecond(ticks) / m_concentrator.TicksPerFrame) + 1
    '    Dim dataFrame As New BpaPdcStream.DataFrame(sampleIndex)
    '    Dim dataCell As BpaPdcStream.DataCell
    '    Dim cellCache As New Dictionary(Of String, BpaPdcStream.DataCell)

    '    dataFrame.ConfigurationFrame = m_configurationFrame
    '    dataFrame.Ticks = ticks

    '    For x As Integer = 0 To m_configurationFrame.Cells.Count - 1
    '        dataCell = New BpaPdcStream.DataCell(dataFrame, m_configurationFrame.Cells(x), sampleIndex)
    '        dataFrame.Cells.Add(dataCell)
    '        cellCache.Add(dataCell.ConfigurationCell.SectionEntry, dataCell)
    '    Next

    '    ' Hold on to cell cache for future use...
    '    dataFrame.Tag = cellCache

    '    Return dataFrame

    'End Function

    'Private Sub AssignMeasurementToFrame(ByVal measurement As IMeasurement, ByVal frame As IFrame)

    '    ' Assign all time-aligned measurements to their appropriate PMU (i.e., data frame)
    '    Dim dataFrame As BpaPdcStream.DataFrame = frame
    '    Dim dataCell As BpaPdcStream.DataCell
    '    Dim signalReference As SignalReference
    '    Dim cellCache As Dictionary(Of String, BpaPdcStream.DataCell) = dataFrame.Tag

    '    ' Look up signal reference from measurement key
    '    If m_signalReference.TryGetValue(measurement.Key, signalReference) Then
    '        ' Look up associated data cell by ID
    '        If cellCache.TryGetValue(signalReference.PmuAcronym, dataCell) Then
    '            ' Assign appropriate value to cell based on signal type
    '            Select Case signalReference.SignalType
    '                Case SignalType.Angle
    '                    dataCell.PhasorValues(signalReference.SignalIndex - 1).Angle = measurement.Value
    '                Case SignalType.Magnitude
    '                    dataCell.PhasorValues(signalReference.SignalIndex - 1).Magnitude = measurement.Value
    '                Case SignalType.Frequency
    '                    dataCell.FrequencyValue.Frequency = measurement.Value
    '                Case SignalType.dfdt
    '                    dataCell.FrequencyValue.DfDt = measurement.Value
    '                Case SignalType.Status
    '                    dataCell.StatusFlags = measurement.Value
    '                    'Case "DV"
    '            End Select
    '        End If
    '    End If

    'End Sub

    'Private Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

    '    Dim dataFrame As BpaPdcStream.DataFrame = frame

    '    ' Send a descriptor packet at the top of each minute...
    '    If dataFrame.Timestamp.Second = 0 Then
    '        If Not m_sentDescriptor Then
    '            m_sentDescriptor = True

    '            ' Publish binary image over specified communication layer
    '            m_communicationServer.Multicast(m_configurationFrame.BinaryImage())
    '        End If
    '    Else
    '        m_sentDescriptor = False
    '    End If

    '    ' Publish binary image over specified communication layer
    '    m_communicationServer.Multicast(dataFrame.BinaryImage())

    'End Sub

    'Private Sub m_concentrator_ProcessException(ByVal ex As System.Exception) Handles m_concentrator.ProcessException

    '    UpdateStatus("Processing exception: " & ex.Message)

    'End Sub

    'Private Sub m_concentrator_SamplePublished(ByVal sample As LumiereLogic.Measurements.Sample) Handles m_concentrator.SamplePublished

    'End Sub

    'Private Sub m_concentrator_UnpublishedSamples(ByVal total As Integer) Handles m_concentrator.UnpublishedSamples

    '    If total > 2 * m_lagTime Then UpdateStatus("There are " & total & " unpublished samples in the concentration queue...")

    'End Sub

    'Private Sub m_communicationServer_ClientConnected(ByVal sender As Object, ByVal e As LumiereLogic.IdentifiableSourceEventArgs) Handles m_communicationServer.ClientConnected

    '    UpdateStatus("Client " & e.Source.ToString() & " connected.")

    'End Sub

    'Private Sub m_communicationServer_ClientDisconnected(ByVal sender As Object, ByVal e As LumiereLogic.IdentifiableSourceEventArgs) Handles m_communicationServer.ClientDisconnected

    '    UpdateStatus("Client " & e.Source.ToString() & " disconnected.")

    'End Sub

    'Private Sub m_communicationServer_ReceivedClientData(ByVal sender As Object, ByVal e As LumiereLogic.IdentifiableItemEventArgs(Of Byte())) Handles m_communicationServer.ReceivedClientData

    '    ' Handle incoming transmissions - this would represent a command frame when transport is TCP
    '    UpdateStatus("Received client data: " & e.Item.Length & " bytes")

    'End Sub

    'Private Sub m_communicationServer_ServerStarted(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationServer.ServerStarted

    '    UpdateStatus("Server started.")

    'End Sub

    'Private Sub m_communicationServer_ServerStopped(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationServer.ServerStopped

    '    UpdateStatus("Server stopped.")

    'End Sub

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

End Class
