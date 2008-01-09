'*******************************************************************************************************
'  PhasorDataConcentratorBase.vb - Phasor data concentrator base class
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
'  04/20/2007 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.Data.OleDb
Imports System.Threading
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization.Formatters.Soap
Imports TVA.Measurements
Imports TVA.Communication
Imports TVA.Collections
Imports TVA.Data.Common
Imports TVA.Common
Imports TVA.IO.FilePath
Imports TVA.ErrorManagement
Imports TVA.Text.Common
Imports TVA.Services
Imports PhasorProtocols

Public MustInherit Class PhasorDataConcentratorBase

    Inherits ConcentratorBase
    Implements IServiceComponent

    Public Event StatusMessage(ByVal status As String)

    Private m_name As String
    Private m_configurationFrame As IConfigurationFrame
    Private m_signalReferences As Dictionary(Of MeasurementKey, SignalReference)
    Private m_publishDescriptor As Boolean
    Private m_exceptionLogger As GlobalExceptionLogger
    Private WithEvents m_communicationServer As ICommunicationServer

    ' This class associates a given measurement with its signal reference value - this comes in handy
    ' because we are able to "pre-filter" measurements that get sorted based on whether or not we have
    ' a signal reference defined for the measurement - once we know the signal reference, we make the
    ' association using this measurement wrapper class which saves us having to lookup the signal
    ' reference again during the measurement to frame assignment 
    Protected Class SignalReferenceMeasurement

        Implements IMeasurement

        Private m_measurement As IMeasurement
        Private m_signalReference As SignalReference

        Public Sub New(ByVal measurement As IMeasurement, ByVal signalReference As SignalReference)

            m_measurement = measurement
            m_signalReference = signalReference

        End Sub

        Public ReadOnly Property SignalReference() As SignalReference
            Get
                Return m_signalReference
            End Get
        End Property

        Public ReadOnly Property UnwrappedMeasurement() As IMeasurement
            Get
                Return m_measurement
            End Get
        End Property

#Region " IMeasurement Wrapper "

        Public ReadOnly Property This() As IMeasurement Implements IMeasurement.This
            Get
                Return Me
            End Get
        End Property

        Public Property ID() As Integer Implements IMeasurement.ID
            Get
                Return m_measurement.ID
            End Get
            Set(ByVal value As Integer)
                m_measurement.ID = value
            End Set
        End Property

        Public Property Source() As String Implements IMeasurement.Source
            Get
                Return m_measurement.Source
            End Get
            Set(ByVal value As String)
                m_measurement.Source = value
            End Set
        End Property

        Public ReadOnly Property Key() As MeasurementKey Implements IMeasurement.Key
            Get
                Return m_measurement.Key
            End Get
        End Property

        Public Property TagName() As String Implements IMeasurement.TagName
            Get
                Return m_measurement.TagName
            End Get
            Set(ByVal value As String)
                m_measurement.TagName = value
            End Set
        End Property

        Public Property Ticks() As Long Implements IMeasurement.Ticks
            Get
                Return m_measurement.Ticks
            End Get
            Set(ByVal value As Long)
                m_measurement.Ticks = value
            End Set
        End Property

        Public ReadOnly Property Timestamp() As Date Implements IMeasurement.Timestamp
            Get
                Return m_measurement.Timestamp
            End Get
        End Property

        Public Property Value() As Double Implements IMeasurement.Value
            Get
                Return m_measurement.Value
            End Get
            Set(ByVal value As Double)
                m_measurement.Value = value
            End Set
        End Property

        Public Property Adder() As Double Implements IMeasurement.Adder
            Get
                Return m_measurement.Adder
            End Get
            Set(ByVal value As Double)
                m_measurement.Adder = value
            End Set
        End Property

        Public Property Multiplier() As Double Implements IMeasurement.Multiplier
            Get
                Return m_measurement.Multiplier
            End Get
            Set(ByVal value As Double)
                m_measurement.Multiplier = value
            End Set
        End Property

        Public ReadOnly Property AdjustedValue() As Double Implements IMeasurement.AdjustedValue
            Get
                Return m_measurement.AdjustedValue
            End Get
        End Property

        Public Property TimestampQualityIsGood() As Boolean Implements IMeasurement.TimestampQualityIsGood
            Get
                Return m_measurement.TimestampQualityIsGood
            End Get
            Set(ByVal value As Boolean)
                m_measurement.TimestampQualityIsGood = value
            End Set
        End Property

        Public Property ValueQualityIsGood() As Boolean Implements IMeasurement.ValueQualityIsGood
            Get
                Return m_measurement.ValueQualityIsGood
            End Get
            Set(ByVal value As Boolean)
                m_measurement.ValueQualityIsGood = value
            End Set
        End Property

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            Return CompareTo(DirectCast(obj, IMeasurement))

        End Function

        Public Function CompareTo(ByVal other As IMeasurement) As Integer Implements System.IComparable(Of IMeasurement).CompareTo

            Return DirectCast(m_measurement, System.IComparable(Of IMeasurement)).CompareTo(other)

        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Return Equals(DirectCast(obj, IMeasurement))

        End Function

        Public Overloads Function Equals(ByVal other As IMeasurement) As Boolean Implements System.IEquatable(Of IMeasurement).Equals

            Return m_measurement.Equals(other)

        End Function

#End Region

    End Class

    Protected Sub New( _
        ByVal communicationServer As ICommunicationServer, _
        ByVal name As String, _
        ByVal framesPerSecond As Integer, _
        ByVal lagTime As Double, _
        ByVal leadTime As Double, _
        ByVal exceptionLogger As GlobalExceptionLogger)

        MyBase.New(framesPerSecond, lagTime, leadTime)

        m_signalReferences = New Dictionary(Of MeasurementKey, SignalReference)
        m_communicationServer = communicationServer
        m_exceptionLogger = exceptionLogger
        m_publishDescriptor = True

        If String.IsNullOrEmpty(name) Then
            m_name = Me.GetType().Name
        Else
            m_name = name
        End If

    End Sub

    Public Overrides Sub Start()

        ' Start communications server
        If m_communicationServer IsNot Nothing Then m_communicationServer.Start()

        ' Start concentrator
        MyBase.Start()

    End Sub

    Public Overrides Sub [Stop]()

        ' Stop concentrator
        MyBase.[Stop]()

        ' Stop communications server
        If m_communicationServer IsNot Nothing Then m_communicationServer.Stop()

    End Sub

    Public Sub Initialize( _
        ByVal connection As OleDbConnection, _
        ByVal pmuFilterSql As String, _
        ByVal nominalFrequencyValue As Byte, _
        ByVal idCode As UInt16)

        Dim signal As SignalReference
        Dim idLabelCellIndex As New Dictionary(Of String, Integer)
        Dim nominalFrequency As LineFrequency
        Dim keys As Dictionary(Of String, String)
        Dim virtualDevice As Boolean
        Dim virtualSetting As String

        ' Attempt to parse line frequency enumeration
        Try
            nominalFrequency = CType(nominalFrequencyValue, LineFrequency)
        Catch
            ' We'll default to 60Hz if there are issues
            nominalFrequency = LineFrequency.Hz60
            UpdateStatus(String.Format("Failed to parse standard line frequency from value ""{0}"", defaulting t0 60Hz", nominalFrequencyValue))
        End Try

        ' Define protocol independent configuration frame based on PMU filter expression
        Dim configurationFrame As New ConfigurationFrame(idCode, DateTime.UtcNow.Ticks, Convert.ToInt16(FramesPerSecond))
        Dim cell As ConfigurationCell

        ' We'll give a full config frame of validated PMU's when debugging...
        'If String.IsNullOrEmpty(pmuFilterSql) Then pmuFilterSql = "SELECT * FROM Pmu WHERE Interconnection = 'Eastern' AND Validated <> 0"

        If String.IsNullOrEmpty(pmuFilterSql) Then pmuFilterSql = "SELECT * FROM Pmu WHERE Active <> 0"

        ' TODO: Will need to allow a way to define digitals and analogs in the ouput stream at some point
        With RetrieveData(pmuFilterSql, connection).Rows
            For x As Integer = 0 To .Count - 1
                With .Item(x)
                    ' Parse additional connection info for special parameters
                    keys = ParseKeyValuePairs(.Item("AdditionalConnectionInfo").ToString())

                    ' See if this is a virtual device
                    virtualDevice = False

                    If keys.TryGetValue("virtual", virtualSetting) Then
                        ' Virtual devices consist entirely of composed points, hence there will be
                        ' no incoming status measurement - so these cell's need to be handled special
                        virtualDevice = ParseBoolean(virtualSetting)
                    End If

                    ' Create a new configuration cell
                    cell = New ConfigurationCell(configurationFrame, Convert.ToUInt16(.Item("ID")), nominalFrequency, virtualDevice)

                    ' To allow rectangular phasors and/or scaled values - make adjustments here...
                    cell.PhasorDataFormat = DataFormat.FloatingPoint
                    cell.PhasorCoordinateFormat = CoordinateFormat.Polar
                    cell.FrequencyDataFormat = DataFormat.FloatingPoint
                    cell.AnalogDataFormat = DataFormat.FloatingPoint

                    cell.IDLabel = .Item("Acronym").ToString().Trim()
                    cell.StationName = TruncateRight(.Item("Name").ToString(), cell.MaximumStationNameLength).Trim()

                    ' Load all phasors as defined in the database
                    With RetrieveData(String.Format("SELECT * FROM Phasor WHERE PmuID={0} ORDER BY IOIndex", Convert.ToInt32(.Item("ID"))), connection).Rows
                        For y As Integer = 0 To .Count - 1
                            With .Item(y)
                                cell.PhasorDefinitions.Add( _
                                    New PhasorDefinition( _
                                        cell, y, GeneratePhasorLabel( _
                                        .Item("Label"), .Item("PhaseType"), .Item("Type"), _
                                        cell.MaximumStationNameLength), _
                                        1, 0.0F, IIf(.Item("Type").ToString().StartsWith( _
                                        "V", StringComparison.OrdinalIgnoreCase), _
                                        PhasorType.Voltage, PhasorType.Current), Nothing))
                            End With
                        Next
                    End With

                    ' Add frequency definition
                    cell.FrequencyDefinition = New FrequencyDefinition( _
                        cell, String.Format("{0} Freq", TruncateRight(cell.IDLabel, 11)).Trim(), _
                        Convert.ToInt32(.Item("FrequencyScale")), _
                        Convert.ToSingle(.Item("FrequencyOffset")), _
                        Convert.ToInt32(.Item("DfDtScale")), _
                        Convert.ToSingle(.Item("DfDtOffset")))

                    configurationFrame.Cells.Add(cell)
                End With
            Next
        End With

        ' Define protocol specific configuration frame
        BaseConfigurationFrameCreated(configurationFrame)

        ' Cache configuration frame for reference
        UpdateStatus(String.Format("Caching new {0} [{1}] configuration frame...", Name, m_configurationFrame.IDCode))

        Try
            Dim cachePath As String = String.Format("{0}ConfigurationCache\", GetApplicationPath())
            If Not Directory.Exists(cachePath) Then Directory.CreateDirectory(cachePath)
            Dim configFile As FileStream = File.Create(String.Format("{0}{1}.{2}.configuration.xml", cachePath, RemoveWhiteSpace(Name), m_configurationFrame.IDCode))

            With New SoapFormatter
                .AssemblyFormat = FormatterAssemblyStyle.Simple
                .TypeFormat = FormatterTypeStyle.TypesWhenNeeded
                .Serialize(configFile, m_configurationFrame)
            End With

            configFile.Close()
        Catch ex As Exception
            UpdateStatus(String.Format("Failed to serialize {0} [{1}] configuration frame: {3}", Name, m_configurationFrame.IDCode, ex.Message))
            m_exceptionLogger.Log(ex)
        End Try

        ' Define measurement to signal cross reference dictionary
        ' Initialize measurement list for each pmu keyed on the signal reference field
        With RetrieveData("SELECT * FROM ActiveDeviceMeasurements WHERE SignalType IN ('FREQ', 'DFDT', 'STAT', 'VPHA', 'VPHM', 'IPHA', 'IPHM')", connection).Rows
            For x As Integer = 0 To .Count - 1
                With .Item(x)
                    signal = New SignalReference(.Item("SignalReference").ToString())

                    ' Lookup cell index by acronym - doing this work upfront will save a huge amount
                    ' of work during primary measurement sorting
                    If Not idLabelCellIndex.TryGetValue(signal.Acronym, signal.CellIndex) Then
                        ' We cache these indicies locally to speed up initialization - we'll be
                        ' requesting these indexes for the same PMU's over and over
                        signal.CellIndex = m_configurationFrame.Cells.IndexOfIDLabel(signal.Acronym)
                        idLabelCellIndex.Add(signal.Acronym, signal.CellIndex)
                    End If

                    m_signalReferences.Add(New MeasurementKey(Convert.ToInt32(.Item("PointID")), .Item("Historian").ToString()), signal)
                End With
            Next
        End With

    End Sub

    Private Function GeneratePhasorLabel(ByVal phasorlabel As Object, ByVal phaseType As Object, ByVal type As Object, ByVal maxLength As Integer) As String

        With New StringBuilder
            If phasorlabel IsNot Nothing AndAlso Not TypeOf phasorlabel Is DBNull Then
                .Append(TruncateRight(RemoveDuplicateWhiteSpace(phasorlabel.ToString()).Trim(), maxLength - 4))

                Select Case phaseType.ToString().Trim().ToUpper().Chars(0)
                    Case "+"c ' Positive Sequence
                        .Append(" +S")
                    Case "-"c ' Negative Sequence
                        .Append(" -S")
                    Case "0"c ' Zero Sequence
                        .Append(" 0S")
                    Case "A"c ' A Phase
                        .Append(" AP")
                    Case "B"c ' B Phase
                        .Append(" BP")
                    Case "C"c ' C Phase
                        .Append(" CP")
                End Select

                .Append(type.ToString().Trim().ToUpper().Chars(0))
            End If

            Return .ToString().Trim()
        End With

    End Function

    Public ReadOnly Property BaseConfigurationFrame() As IConfigurationFrame
        Get
            Return m_configurationFrame
        End Get
    End Property

    Public Overridable ReadOnly Property Name() As String Implements TVA.Services.IServiceComponent.Name
        Get
            Return m_name
        End Get
    End Property

    Public Overrides ReadOnly Property Status() As String Implements TVA.Services.IServiceComponent.Status
        Get
            With New StringBuilder
                .Append("Operational Status for ")
                .Append(Name)
                .Append(":"c)
                .AppendLine()
                If m_communicationServer IsNot Nothing Then
                    .Append(m_communicationServer.Status)
                End If
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    Public Overridable Property PublishDescriptor() As Boolean
        Get
            Return m_publishDescriptor
        End Get
        Set(ByVal value As Boolean)
            m_publishDescriptor = value
        End Set
    End Property

    Protected ReadOnly Property CommunicationServer() As ICommunicationServer
        Get
            Return m_communicationServer
        End Get
    End Property

    Protected ReadOnly Property ExceptionLogger() As GlobalExceptionLogger
        Get
            Return m_exceptionLogger
        End Get
    End Property

    Protected Overridable Sub UpdateStatus(ByVal status As String)

        RaiseEvent StatusMessage(String.Format("[{0}]: {1}", Name, status))

    End Sub

    Protected Overridable Sub BaseConfigurationFrameCreated(ByVal baseConfiguration As IConfigurationFrame)

        ' This is optionally overridden to create a protocol specific configuration frame if needed

    End Sub

    ' We filter sorted incoming measurements to just those that are needed in the concentrated output stream
    Public Overrides Sub SortMeasurement(ByVal measurement As IMeasurement)

        Dim signalRef As SignalReference

        ' We assign signal reference to measurement in advance since we are using this as a filter
        ' anyway, this will save a lookup later during measurement assignment to frame...
        If m_signalReferences.TryGetValue(measurement.Key, signalRef) Then
            ' No need to sort this measurement unless it has a destination PMU
            If signalRef.CellIndex > -1 Then MyBase.SortMeasurement(New SignalReferenceMeasurement(measurement, signalRef))
        End If

    End Sub

    Public Overrides Sub SortMeasurements(ByVal measurements As ICollection(Of IMeasurement))

        Dim inputMeasurements As New List(Of IMeasurement)
        Dim signalRef As SignalReference

        For Each measurement As IMeasurement In measurements
            ' We assign signal reference to measurement in advance since we are using this as a filter
            ' anyway, this will save a lookup later during measurement assignment to frame...
            If m_signalReferences.TryGetValue(measurement.Key, signalRef) Then
                ' No need to sort this measurement unless it has a destination PMU
                If signalRef.CellIndex > -1 Then inputMeasurements.Add(New SignalReferenceMeasurement(measurement, signalRef))
            End If
        Next

        If inputMeasurements.Count > 0 Then MyBase.SortMeasurements(inputMeasurements)

    End Sub

    Protected Overrides Function AssignMeasurementToFrame(ByVal frame As IFrame, ByVal measurement As IMeasurement) As Boolean

        ' Make sure the measurement is a "SignalReferenceMeasurement" wrapper (it should be)
        Dim wrappedMeasurement As SignalReferenceMeasurement = TryCast(measurement, SignalReferenceMeasurement)

        If wrappedMeasurement IsNot Nothing Then
            ' Get associated data cell
            Dim signalRef As SignalReference = wrappedMeasurement.SignalReference
            Dim dataCell As IDataCell = DirectCast(frame, IDataFrame).Cells(signalRef.CellIndex)
            Dim signalIndex As Integer = signalRef.Index

            ' Assign value to appropriate cell property based on signal type
            Select Case signalRef.Type
                Case SignalType.Angle
                    ' Assign "phase angle" measurement to data cell
                    Dim phasorValues As PhasorValueCollection = dataCell.PhasorValues
                    If phasorValues.Count >= signalIndex Then phasorValues(signalIndex - 1).Angle = Convert.ToSingle(measurement.AdjustedValue)
                Case SignalType.Magnitude
                    ' Assign "phase magnitude" measurement to data cell
                    Dim phasorValues As PhasorValueCollection = dataCell.PhasorValues
                    If phasorValues.Count >= signalIndex Then phasorValues(signalIndex - 1).Magnitude = Convert.ToSingle(measurement.AdjustedValue)
                Case SignalType.Frequency
                    ' Assign "frequency" measurement to data cell
                    dataCell.FrequencyValue.Frequency = Convert.ToSingle(measurement.AdjustedValue)
                Case SignalType.dFdt
                    ' Assign "df/dt" measurement to data cell
                    dataCell.FrequencyValue.DfDt = Convert.ToSingle(measurement.AdjustedValue)
                Case SignalType.Status
                    ' Assign "common status flags" measurement to data cell
                    dataCell.CommonStatusFlags = Convert.ToInt32(measurement.AdjustedValue)
                Case SignalType.Digital
                    ' Assign "digital" measurement to data cell
                    Dim digitalValues As DigitalValueCollection = dataCell.DigitalValues
                    If digitalValues.Count >= signalIndex Then digitalValues(signalIndex - 1).Value = Convert.ToInt16(measurement.AdjustedValue)
                Case SignalType.Analog
                    ' Assign "analog" measurement to data cell
                    Dim analogValues As AnalogValueCollection = dataCell.AnalogValues
                    If analogValues.Count >= signalIndex Then analogValues(signalIndex - 1).Value = Convert.ToSingle(measurement.AdjustedValue)
            End Select

            ' Track total measurements sorted for frame - this will become total measurements published
            frame.PublishedMeasurements += 1

            Return True
        Else
            ' I don't expect this to occur - but just in case
            Throw New InvalidCastException(String.Format("Attempt was made to assign an invalid measurement to phasor data concentration frame, expected a ""SignalReferenceMeasurement"" but got a ""{0}""", TypeName(measurement)))
        End If

    End Function

    Protected Overrides Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        Dim dataFrame As IDataFrame = TryCast(frame, IDataFrame)

        If dataFrame IsNot Nothing Then
            ' Send a descriptor packet at the top of each minute...
            If index = 0 AndAlso m_publishDescriptor AndAlso dataFrame.Timestamp.Second = 0 Then
                ' Publish binary image over specified communication layer
                m_configurationFrame.Ticks = dataFrame.Ticks
                m_communicationServer.Multicast(m_configurationFrame.BinaryImage())
            End If

            ' JRC: Removed this step - this only helps when there were missing points during sorting, however
            ' this doesn't happen very often and is further complicated by the fact that unused measurements
            ' are intentionally disabled and hence not sorted - so this step is skipped for now...
            '' Prior to publication, set data validity bits based on reception of all data values
            'For Each dataCell As IDataCell In dataFrame.Cells
            '    ' As a missing data marker, we set all status bits - this is how the BPA PDC traditionally handled this...
            '    If Not dataCell.AllValuesAssigned Then dataCell.StatusFlags = -1
            '    'If Not dataCell.AllValuesAssigned Then dataCell.CommonStatusFlags = dataCell.CommonStatusFlags Or CommonStatusFlags.DataIsValid
            'Next

            ' Publish binary image over specified communication layer
            m_communicationServer.Multicast(dataFrame.BinaryImage())
        End If

    End Sub

    Protected Overridable Sub HandleIncomingData(ByVal commandBuffer As Byte())

        ' This is optionally overridden to handle incoming data - such as IEEE commands

    End Sub

    Private Sub PhasorDataConcentrator_ProcessException(ByVal ex As System.Exception) Handles Me.ProcessException

        UpdateStatus(String.Format("Processing exception: {0}", ex.Message))
        m_exceptionLogger.Log(ex)

    End Sub

    Private Sub PhasorDataConcentrator_UnpublishedSamples(ByVal total As Integer) Handles Me.UnpublishedSamples

        If total > 2 * LagTime Then UpdateStatus(String.Format("WARNING: There are {0} unpublished samples in the concentration queue...", total))

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

    Protected Overridable Sub ProcessStateChanged(ByVal processName As String, ByVal newState As TVA.Services.ProcessState) Implements TVA.Services.IServiceComponent.ProcessStateChanged

        ' We don't normally handle changes in process state - but dervived classes may choose to...

    End Sub

    Protected Overridable Sub ServiceStateChanged(ByVal newState As TVA.Services.ServiceState) Implements TVA.Services.IServiceComponent.ServiceStateChanged

        Select Case newState
            Case ServiceState.Paused
                ' Pause concentrator
                Me.Enabled = False
                UpdateStatus("Data concentration paused at the request of service manager...")
            Case ServiceState.Resumed
                ' Resume concentrator
                Me.Enabled = True
                UpdateStatus("Data concentration resumed...")
        End Select

    End Sub

End Class
