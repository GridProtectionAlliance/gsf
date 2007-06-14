Imports System.Data.OleDb
Imports TVA.Measurements
Imports TVA.Communication
Imports TVA.Data.Common
Imports TVA.Common
Imports PhasorProtocols

Public MustInherit Class PhasorDataConcentratorBase

    Inherits ConcentratorBase

    Public Event StatusMessage(ByVal status As String)

    Private WithEvents m_communicationServer As ICommunicationServer
    Private m_configurationFrame As IConfigurationFrame
    Private m_signalReferences As Dictionary(Of MeasurementKey, SignalReference)
    Private m_sentDescriptor As Boolean

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

    Public Overridable ReadOnly Property Name() As String
        Get
            Return Me.GetType().Name
        End Get
    End Property

    Protected Overridable Sub UpdateStatus(ByVal status As String)

        RaiseEvent StatusMessage(String.Format("[{0}]: {1}", Name, status))

    End Sub

    Protected MustOverride Function CreateConfigurationFrame(ByVal connection As OleDbConnection, ByVal idCode As UInt16) As IConfigurationFrame

    Protected MustOverride Function CreateDataFrame(ByVal ticks As Long) As IDataFrame

    Protected Overrides Function CreateNewFrame(ByVal ticks As Long) As IFrame

        ' Create a new data frame to publish
        Return CreateDataFrame(ticks)

    End Function

    Protected Overrides Sub AssignMeasurementToFrame(ByVal frame As IFrame, ByVal measurement As IMeasurement)

        ' Base class assigns measurement to frame's measurement dictionary - we go ahead and do this just
        ' in case this measurement collection needs to be used elsewhere (in a more abstract fashion)
        MyBase.AssignMeasurementToFrame(frame, measurement)

        ' Assign all time-aligned measurements to their appropriate PMU (i.e., data frame cell)
        Dim signalRef As SignalReference

        ' Look up signal reference from measurement key
        If m_signalReferences.TryGetValue(measurement.Key, signalRef) Then
            ' Get associated data cell
            Dim dataCell As IDataCell = DirectCast(frame, IDataFrame).Cells(signalRef.PmuCellIndex)

            ' Assign value to appropriate cell property based on signal type
            Select Case signalRef.SignalType
                Case SignalType.Angle
                    dataCell.PhasorValues(signalRef.SignalIndex - 1).Angle = Convert.ToSingle(measurement.Value)
                Case SignalType.Magnitude
                    dataCell.PhasorValues(signalRef.SignalIndex - 1).Magnitude = Convert.ToSingle(measurement.Value)
                Case SignalType.Frequency
                    dataCell.FrequencyValue.Frequency = Convert.ToSingle(measurement.Value)
                Case SignalType.dfdt
                    dataCell.FrequencyValue.DfDt = Convert.ToSingle(measurement.Value)
                Case SignalType.Status
                    dataCell.StatusFlags = Convert.ToInt16(measurement.Value)
                Case SignalType.Digital
                    dataCell.DigitalValues(signalRef.SignalIndex - 1).Value = Convert.ToInt16(measurement.Value)
            End Select
        End If

    End Sub

    Protected Overrides Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        Dim dataFrame As IDataFrame = DirectCast(frame, IDataFrame)

        ' Send a descriptor packet at the top of each minute...
        If dataFrame.Timestamp.Second = 0 Then
            If Not m_sentDescriptor Then
                m_sentDescriptor = True

                ' Publish binary image over specified communication layer
                m_communicationServer.Multicast(m_configurationFrame.BinaryImage())
            End If
        Else
            m_sentDescriptor = False
        End If

        ' Publish binary image over specified communication layer
        m_communicationServer.Multicast(dataFrame.BinaryImage())

    End Sub

    Protected Overridable Sub HandleIncomingData(ByVal commandBuffer As Byte())

        ' This is optionally overridden to handle incoming data - such as IEEE commands

    End Sub

    Private Sub PhasorDataConcentrator_ProcessException(ByVal ex As System.Exception) Handles Me.ProcessException

        UpdateStatus(String.Format("Processing exception: {0}", ex.Message))

    End Sub

    Private Sub PhasorDataConcentrator_UnpublishedSamples(ByVal total As Integer) Handles Me.UnpublishedSamples

        If total > 2 * LagTime Then UpdateStatus(String.Format("There are {0} unpublished samples in the concentration queue...", total))

    End Sub

    Private Sub m_communicationServer_ClientConnected(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of System.Guid)) Handles m_communicationServer.ClientConnected

        UpdateStatus("Client connected.")

    End Sub

    Private Sub m_communicationServer_ClientDisconnected(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of System.Guid)) Handles m_communicationServer.ClientDisconnected

        UpdateStatus("Client disconnected.")

    End Sub

    Private Sub m_communicationServer_ServerStarted(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationServer.ServerStarted

        UpdateStatus("Server started.")

    End Sub

    Private Sub m_communicationServer_ServerStopped(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_communicationServer.ServerStopped

        UpdateStatus("Server stopped.")

    End Sub

    Private Sub m_communicationServer_ServerStartupException(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of System.Exception)) Handles m_communicationServer.ServerStartupException

        UpdateStatus(String.Format("Server startup exception: {0}", e.Argument.Message))

    End Sub

    Private Sub m_communicationServer_ReceivedClientData(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of TVA.IdentifiableItem(Of System.Guid, Byte()))) Handles m_communicationServer.ReceivedClientData

        HandleIncomingData(e.Argument.Item)

    End Sub

End Class
