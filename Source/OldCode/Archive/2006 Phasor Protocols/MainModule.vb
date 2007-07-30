'*******************************************************************************************************
'  MainModule.vb - Main module for phasor measurement receiver service
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
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
'
'*******************************************************************************************************

Imports System.Text
Imports System.Security.Principal
Imports System.Data.OleDb
Imports System.Threading
Imports System.Reflection
Imports TVA.IO
Imports TVA.Assembly
Imports TVA.Communication
Imports TVA.Communication.Common
Imports TVA.Configuration.Common
Imports TVA.Text.Common
Imports TVA.Data.Common
Imports TVA.Measurements
Imports TVA.DateTime.Common
Imports TVA.Collections
Imports TVA.ErrorManagement
Imports InterfaceAdapters
Imports PhasorProtocols

Module MainModule

    Private Delegate Sub InitializationFunctionSignature(ByVal connection As OleDbConnection)

    Private m_measurementReceivers As Dictionary(Of String, PhasorMeasurementReceiver)
    Private m_measurementConcentrators As PhasorDataConcentratorBase()
    Private m_calculatedMeasurements As ICalculatedMeasurementAdapter()
    Private m_messageDisplayTimepan As Integer
    Private m_maximumMessagesToDisplay As Integer
    Private m_lastDisplayedMessageTime As Long
    Private m_displayedMessageCount As Long
    Private m_systemLogEnabled As Boolean
    Private m_systemLogFile As LogFile
    Private m_statusMessageQueue As ProcessQueue(Of String)
    Private m_exceptionLogger As GlobalExceptionLogger

    Public Sub Main()

        'Debug.WriteLine(TVA.ByteEncoding.LittleEndianBinary.GetString(BitConverter.GetBytes(Single.MinValue)))
        'Debug.WriteLine(TVA.ByteEncoding.LittleEndianBinary.GetString(BitConverter.GetBytes(Single.MaxValue)))
        'Debug.WriteLine(TVA.ByteEncoding.LittleEndianBinary.GetString(BitConverter.GetBytes(Single.NegativeInfinity)))
        'Debug.WriteLine(TVA.ByteEncoding.LittleEndianBinary.GetString(BitConverter.GetBytes(Single.PositiveInfinity)))
        'Debug.WriteLine(TVA.ByteEncoding.LittleEndianBinary.GetString(BitConverter.GetBytes(Single.NaN)))
        'Debug.WriteLine(TVA.ByteEncoding.LittleEndianBinary.GetString(BitConverter.GetBytes(UInt16.MaxValue)))
        'Debug.WriteLine(TVA.ByteEncoding.LittleEndianBinary.GetString(BitConverter.GetBytes(Convert.ToInt32(-1))))
        'Debug.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(-1)), 0))
        'Debug.WriteLine(Single.IsNaN(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(-1)), 0)))
        'Debug.WriteLine(Single.IsInfinity(BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(-1)), 0)))
        'Debug.WriteLine(BitConverter.ToSingle(, 0))
        'Stop

        Dim consoleLine As String
        Dim mapper As PhasorMeasurementMapper

        Console.WriteLine(MonitorInformation)

        ' Make sure service settings exist
        'Settings.Add("PMUDatabase", "Data Source=ESOEXTSQL;Initial Catalog=PMU_SDS;Integrated Security=False;user ID=ESOPublic;pwd=4all2see", "PMU metaData database connect string")
        'Settings.Add("PMUDatabase", "Provider=SQLOLEDB;Data Source=esoextsql;Initial Catalog=PMU_SDS;User ID=ESOPublic;pwd=4all2see", "PMU metaData database connect string")
        Settings.Add("PMUDatabase", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Databases\PMU_SDS.mdb", "PMU metaData database connect string")
        Settings.Add("PMUStatusInterval", "5", "Number of seconds of deviation from UTC time (according to local clock) that last PMU reporting time is allowed before considering it offline")
        Settings.Add("DataLossInterval", "35000", "Number of milliseconds to wait for incoming data before restarting connection cycle to device")
        Settings.Add("MessageDisplayTimespan", "2", "Timespan, in seconds, over which to monitor message volume")
        Settings.Add("MaximumMessagesToDisplay", "100", "Maximum number of messages to be tolerated during MessageDisplayTimespan")
        Settings.Add("EnableLogFile", "True", "Set to ""True"" to enable log file")
        SaveSettings()

        m_systemLogEnabled = BooleanSetting("EnableLogFile")

        m_exceptionLogger = New GlobalExceptionLogger()
        m_exceptionLogger.LogToFile = True
        m_exceptionLogger.PersistSettings = True
        m_exceptionLogger.AutoRegister = True
        m_exceptionLogger.EndInit()

        m_statusMessageQueue = ProcessQueue(Of String).CreateSynchronousQueue(AddressOf DisplayStatusMessages, 50, Timeout.Infinite, False, False)
        m_statusMessageQueue.Start()

        InitializeConfiguration(AddressOf InitializeSystem)

        Do While True
            ' This console window stays open by continually reading in console lines
            consoleLine = Console.ReadLine()

            ' While doing this, we monitor for special commands...
            If consoleLine.StartsWith("connect", True, Nothing) Then
                If TryGetMapper(consoleLine, mapper) Then
                    mapper.Connect()
                End If
            ElseIf consoleLine.StartsWith("disconnect", True, Nothing) Then
                If TryGetMapper(consoleLine, mapper) Then
                    mapper.Disconnect()
                End If
            ElseIf consoleLine.StartsWith("sendcommand", True, Nothing) Then
                If TryGetMapper(consoleLine, mapper) Then
                    mapper.SendDeviceCommand(ParseDeviceCommand(consoleLine))
                End If
            ElseIf consoleLine.StartsWith("reload", True, Nothing) Then
                InitializeConfiguration(AddressOf InitializeSystem)
            ElseIf consoleLine.StartsWith("reconnectall", True, Nothing) Then
                InitializeConfiguration(AddressOf ReinitializeReceivers)
            ElseIf consoleLine.StartsWith("gc", True, Nothing) Then
                ForceGarbageCollection()
            ElseIf consoleLine.StartsWith("status", True, Nothing) Then
                DisplayStatusMessage(SystemStatus)
            ElseIf consoleLine.StartsWith("list", True, Nothing) Then
                DisplayStatusMessage(ConnectionList)
            ElseIf consoleLine.StartsWith("version", True, Nothing) Then
                DisplayStatusMessage(MonitorInformation)
            ElseIf consoleLine.StartsWith("help", True, Nothing) OrElse consoleLine.StartsWith("?") Then
                DisplayStatusMessage(CommandList)
            ElseIf consoleLine.StartsWith("exit", True, Nothing) Then
                Exit Do
            Else
                With New StringBuilder
                    .Append(Environment.NewLine)
                    .Append("Command unrecognized.  ")
                    .Append(CommandList)

                    DisplayStatusMessage(.ToString())
                End With
            End If
        Loop

        ' Attempt an orderly shutdown...

        ' Stop data concentrators
        For Each concentrator As PhasorDataConcentratorBase In m_measurementConcentrators
            concentrator.Dispose()
        Next

        ' Stop data measurement receivers
        For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
            receiver.Disconnect()
        Next

        End

    End Sub

    Private Sub InitializeConfiguration(ByVal initializationFunction As InitializationFunctionSignature)

        Dim connection As OleDbConnection

        Try
            connection = New OleDbConnection(StringSetting("PMUDatabase"))
            connection.Open()

            ' To make sure any initialization messages don't get missed, we increase message
            ' tolerance to maximum during the initialization process
            m_messageDisplayTimepan = 10
            m_maximumMessagesToDisplay = Integer.MaxValue
            m_displayedMessageCount = 0

            DisplayStatusMessage("PMU database connection opened...")

            ' Call user initialization function
            initializationFunction(connection)

            ' Restore normal message volume tolerances
            m_messageDisplayTimepan = IntegerSetting("MessageDisplayTimespan")
            m_maximumMessagesToDisplay = IntegerSetting("MaximumMessagesToDisplay")
            m_displayedMessageCount = 0
        Catch ex As Exception
            DisplayStatusMessage(String.Format("Failure during initialization: {0}", ex.Message))
            m_exceptionLogger.Log(ex)
        Finally
            If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then connection.Close()
            DisplayStatusMessage("PMU database connection closed.")
        End Try

    End Sub

    Private Sub InitializeSystem(ByVal connection As OleDbConnection)

        DisplayStatusMessage(String.Format("*** System Initializing [UTC: {0}] ***", Date.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")))

        ' If calculated measurements are already defined (i.e., we are "re-initializing" code), we need to dispose
        ' existing calculation instances such that all items can be shut-down in an orderly fashion
        If m_calculatedMeasurements IsNot Nothing Then
            For x As Integer = 0 To m_calculatedMeasurements.Length - 1
                m_calculatedMeasurements(x).Dispose()
            Next
        End If

        ' Define all of the calculated measurements
        m_calculatedMeasurements = LoadCalculatedMeasurements(connection)

        ' If the phasor measurement receivers are already defined we must be reloading - so we attempt
        ' an orderly shutdown
        If m_measurementReceivers IsNot Nothing Then
            For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                receiver.Disconnect()
            Next
        End If

        ' Load the phasor measurement receivers (one per each established archive)
        m_measurementReceivers = LoadMeasurementReceivers(connection, IntegerSetting("PMUStatusInterval"), IntegerSetting("DataLossInterval"), m_calculatedMeasurements)

        ' If the phasor measurement concentrators are already defined we must be reloading - so we attempt
        ' an orderly shutdown
        If m_measurementConcentrators IsNot Nothing Then
            For Each concentrator As PhasorDataConcentratorBase In m_measurementConcentrators
                concentrator.Dispose()
            Next
        End If

        ' Load the phasor measurement concentrators
        m_measurementConcentrators = LoadMeasurementConcentrators(connection)

    End Sub

    Private Sub ReinitializeReceivers(ByVal connection As OleDbConnection)

        ' This reloads active data nodes and reconnects
        For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
            receiver.Initialize(connection)
        Next

    End Sub

    Private Function LoadMeasurementReceivers(ByVal connection As OleDbConnection, ByVal pmuStatusInterval As Integer, ByVal dataLossInterval As Integer, ByVal calculatedMeasurements As ICalculatedMeasurementAdapter()) As Dictionary(Of String, PhasorMeasurementReceiver)

        Dim measurementReceivers As New Dictionary(Of String, PhasorMeasurementReceiver)
        Dim measurementReceiver As PhasorMeasurementReceiver
        Dim externalAssemblyName As String
        Dim externalAssembly As Assembly
        Dim historianAdapter As IHistorianAdapter
        Dim connectionString As String = StringSetting("PMUDatabase")
        Dim archiveSource As String

        'With RetrieveData("SELECT * FROM Historian WHERE Enabled <> 0", connection)
        With RetrieveData("SELECT * FROM MeasurementArchives WHERE Enabled <> 0", connection).Rows
            For x As Integer = 0 To .Count - 1
                Try
                    With .Item(x)
                        'archiveSource = .Item("Acronym").ToString()
                        archiveSource = .Item("ArchiveSource").ToString()
                        externalAssemblyName = .Item("AssemblyName").ToString()

                        ' Load the external assembly for the historian adapter
                        externalAssembly = Assembly.LoadFrom(externalAssemblyName)

                        ' Create a new instance of the historian adpater
                        historianAdapter = Activator.CreateInstance(externalAssembly.GetType(.Item("TypeName").ToString()))
                        historianAdapter.Initialize(.Item("ConnectionString").ToString())

                        measurementReceiver = New PhasorMeasurementReceiver( _
                            historianAdapter, _
                            archiveSource, _
                            pmuStatusInterval, _
                            connectionString, _
                            dataLossInterval, _
                            m_exceptionLogger)

                        AddHandler measurementReceiver.StatusMessage, AddressOf DisplayStatusMessage
                        AddHandler measurementReceiver.NewMeasurements, AddressOf NewParsedMeasurements
                        If .Item("InitializeOnStartup") <> 0 Then measurementReceiver.Initialize(connection)

                        measurementReceivers.Add(archiveSource, measurementReceiver)
                    End With
                Catch ex As Exception
                    DisplayStatusMessage(String.Format("Failed to load measurement receiver for archive ""{0}"" from assembly ""{1}"" due to exception: {2}", archiveSource, externalAssemblyName, ex.Message))
                    m_exceptionLogger.Log(ex)
                End Try
            Next
        End With

        Return measurementReceivers

    End Function

    Private Function LoadMeasurementConcentrators(ByVal connection As OleDbConnection) As PhasorDataConcentratorBase()

        Dim measurementConcentrators As New List(Of PhasorDataConcentratorBase)
        Dim measurementConcentrator As PhasorDataConcentratorBase
        Dim communicationServer As ICommunicationServer

        With RetrieveData("SELECT * FROM Concentrator WHERE Enabled <> 0", connection).Rows
            For x As Integer = 0 To .Count - 1
                With .Item(x)
                    Try
                        ' Create communications server for concentrator
                        communicationServer = CreateCommunicationServer(.Item("ConnectionString").ToString())
                        communicationServer.Handshake = False

                        ' Create new data concentrator
                        Select Case .Item("Type").ToString().Trim.ToUpper()
                            Case "IEEEC37.118"
                                ' TODO: Load time base and version properties from generic connection parameters
                                Dim timeBase As Integer = 16777215
                                Dim version As Byte = 1

                                measurementConcentrator = New IeeeC37_118Concentrator( _
                                        communicationServer, _
                                        .Item("Name").ToString(), _
                                        Convert.ToInt32(.Item("FrameRate")), _
                                        Convert.ToDouble(.Item("LagTime")), _
                                        Convert.ToDouble(.Item("LeadTime")), _
                                        timeBase, _
                                        version, _
                                        m_exceptionLogger)
                            Case "BPAPDC"
                                ' TODO: Load INI file name property from generic connection parameters
                                Dim iniFileName As String = ""

                                measurementConcentrator = New BpaPdcConcentrator( _
                                    communicationServer, _
                                    .Item("Name").ToString(), _
                                    Convert.ToInt32(.Item("FrameRate")), _
                                    Convert.ToDouble(.Item("LagTime")), _
                                    Convert.ToDouble(.Item("LeadTime")), _
                                    iniFileName, _
                                    m_exceptionLogger)
                        End Select

                        ' Expose status messages from measurement concentrator 
                        AddHandler measurementConcentrator.StatusMessage, AddressOf DisplayStatusMessage

                        ' Initialize measurement concentrator (sets up signal references and configuration frames)
                        measurementConcentrator.Initialize( _
                            connection, _
                            .Item("PmuFilterSql").ToString(), _
                            Convert.ToInt32(.Item("NominalFrequency")), _
                            Convert.ToUInt16(.Item("IDCode")))

                        ' Start measurement concentrator
                        measurementConcentrator.Start()

                        measurementConcentrators.Add(measurementConcentrator)
                    Catch ex As Exception
                        DisplayStatusMessage(String.Format("Failed to load measurement concentrator ""{0}"" due to exception: {1}", .Item("Name").ToString(), ex.Message))
                        m_exceptionLogger.Log(ex)
                    End Try
                End With
            Next
        End With

        Return measurementConcentrators.ToArray()

    End Function

    Private Function LoadCalculatedMeasurements(ByVal connection As OleDbConnection) As ICalculatedMeasurementAdapter()

        Dim calculatedMeasurementAdapters As New List(Of ICalculatedMeasurementAdapter)
        Dim calculatedMeasurementAdapter As ICalculatedMeasurementAdapter
        Dim calculatedMeasurementName As String
        Dim externalAssemblyName As String
        Dim externalAssembly As Assembly
        Dim outputMeasurementsSql As String
        Dim outputMeasurements As List(Of IMeasurement)
        Dim inputMeasurementsSql As String
        Dim inputMeasurementKeys As List(Of MeasurementKey)

        ' CalculatedMeasurements Fields:
        '   ID                          AutoInc
        '   Name                        String
        '   TypeName                    String
        '   AssemblyName                String
        '   ConfigSection               String
        '   OuputMeasurementsSql        String      Expects one or more rows, with four fields named "MeasurementID", "ArchiveSource", "Adder" and "Multipler"
        '   InputMeasurementsSql        String      Expects one or more rows, with two fields each named "MeasurementID" and "ArchiveSource"
        '   MinimumInputMeasurements    Integer     Defaults to -1 (use all)
        '   ExpectedFrameRate           Integer
        '   LagTime                     Double
        '   LeadTime                    Double

        ' Load all the unique calculated measurement assemlies into the current application domain
        'With RetrieveData("SELECT * FROM CalculatedMeasurement WHERE Enabled <> 0", connection)
        With RetrieveData("SELECT * FROM CalculatedMeasurements WHERE Enabled <> 0", connection).Rows
            For x As Integer = 0 To .Count - 1
                Try
                    With .Item(x)
                        ' Load the external assembly
                        calculatedMeasurementName = .Item("Name").ToString()
                        externalAssemblyName = .Item("AssemblyName").ToString()
                        externalAssembly = Assembly.LoadFrom(externalAssemblyName)

                        ' Query the output measurements
                        outputMeasurementsSql = .Item("OutputMeasurementsSql").ToString()
                        outputMeasurements = New List(Of IMeasurement)

                        ' Calculated measurements have the option of internally defining the output measurements
                        If Not String.IsNullOrEmpty(outputMeasurementsSql) Then
                            Try
                                With RetrieveData(outputMeasurementsSql, connection).Rows
                                    For y As Integer = 0 To .Count - 1
                                        With .Item(y)
                                            outputMeasurements.Add( _
                                                New Measurement( _
                                                    Convert.ToInt32(.Item("MeasurementID")), _
                                                    .Item("ArchiveSource").ToString(), _
                                                    String.Empty, _
                                                    Convert.ToDouble(.Item("Adder")), _
                                                    Convert.ToDouble(.Item("Multiplier"))))
                                        End With
                                    Next
                                End With
                            Catch ex As Exception
                                DisplayStatusMessage(String.Format("Failed to load output measurement for ""{0}"": {1}", calculatedMeasurementName, ex.Message))
                                m_exceptionLogger.Log(ex)
                            End Try
                        End If

                        ' Query the input measurement keys
                        inputMeasurementsSql = .Item("InputMeasurementsSql").ToString()
                        inputMeasurementKeys = New List(Of MeasurementKey)

                        ' Calculated measurements have the option of internally defining the input measurements
                        If Not String.IsNullOrEmpty(inputMeasurementsSql) Then
                            Try
                                With RetrieveData(inputMeasurementsSql, connection)
                                    For y As Integer = 0 To .Rows.Count - 1
                                        With .Rows(y)
                                            inputMeasurementKeys.Add( _
                                                New MeasurementKey( _
                                                    Convert.ToInt32(.Item("MeasurementID")), _
                                                    .Item("ArchiveSource").ToString()))
                                        End With
                                    Next
                                End With
                            Catch ex As Exception
                                DisplayStatusMessage(String.Format("Failed to load input measurements for ""{0}"": {1}", calculatedMeasurementName, ex.Message))
                                m_exceptionLogger.Log(ex)
                            End Try
                        End If

                        calculatedMeasurementAdapter = Nothing

                        ' Load the specified type from the assembly
                        Try
                            ' Create a new instance of the adpater
                            calculatedMeasurementAdapter = Activator.CreateInstance(externalAssembly.GetType(.Item("TypeName").ToString()))
                        Catch ex As Exception
                            DisplayStatusMessage(String.Format("Failed to load type ""{0}"" from assembly ""{1}"" for ""{2}"" due to exception: {3}", .Item("TypeName").ToString(), externalAssemblyName, calculatedMeasurementName, ex.Message))
                            m_exceptionLogger.Log(ex)
                        End Try

                        If calculatedMeasurementAdapter IsNot Nothing Then
                            ' Bubble calculation module status messages out to local update status function
                            AddHandler calculatedMeasurementAdapter.StatusMessage, AddressOf DisplayStatusMessage

                            ' Bubble newly calculated measurement out to functions that need the real-time data
                            AddHandler calculatedMeasurementAdapter.NewCalculatedMeasurements, AddressOf NewCalculatedMeasurements

                            ' Bubble calculation exceptions out to procedure that can handle these exceptions
                            AddHandler calculatedMeasurementAdapter.CalculationException, AddressOf CalculationException

                            ' Intialize calculated measurement adapter - we do this on a separate thread in case this task takes some time
                            CalculatedMeasurementInitialization.Execute( _
                                    calculatedMeasurementAdapter, _
                                    .Item("Name").ToString(), _
                                    .Item("ConfigSection").ToString(), _
                                    outputMeasurements.ToArray(), _
                                    inputMeasurementKeys.ToArray(), _
                                    Convert.ToInt32(.Item("MinimumInputMeasurements")), _
                                    Convert.ToInt32(.Item("ExpectedFrameRate")), _
                                    Convert.ToDouble(.Item("LagTime")), _
                                    Convert.ToDouble(.Item("LeadTime")))

                            ' Add new adapter to the list
                            calculatedMeasurementAdapters.Add(calculatedMeasurementAdapter)

                            DisplayStatusMessage(String.Format("Loaded calculated measurement ""{0}"" from assembly ""{1}""", calculatedMeasurementName, externalAssemblyName))
                        End If
                    End With
                Catch ex As Exception
                    DisplayStatusMessage(String.Format("Failed to load calculated measurement ""{0}"" from assembly ""{1}"" due to exception: {2}", calculatedMeasurementName, externalAssemblyName, ex.Message))
                    m_exceptionLogger.Log(ex)
                End Try
            Next
        End With

        Return calculatedMeasurementAdapters.ToArray()

    End Function

    Private Class CalculatedMeasurementInitialization

        Public CalculatedMeasurementAdapter As ICalculatedMeasurementAdapter
        Public CalculationName As String
        Public ConfigurationSection As String
        Public OutputMeasurements As IMeasurement()
        Public InputMeasurementKeys As MeasurementKey()
        Public MinimumMeasurementsToUse As Integer
        Public ExpectedMeasurementsPerSecond As Integer
        Public LagTime As Double
        Public LeadTime As Double

        Public Shared Sub Execute( _
            ByVal calculatedMeasurementAdapter As ICalculatedMeasurementAdapter, _
            ByVal calculationName As String, _
            ByVal configurationSection As String, _
            ByVal outputMeasurements As IMeasurement(), _
            ByVal inputMeasurementKeys As MeasurementKey(), _
            ByVal minimumMeasurementsToUse As Integer, _
            ByVal expectedMeasurementsPerSecond As Integer, _
            ByVal lagTime As Double, _
            ByVal leadTime As Double)

            With New CalculatedMeasurementInitialization
                .CalculatedMeasurementAdapter = calculatedMeasurementAdapter
                .CalculationName = calculationName
                .ConfigurationSection = configurationSection
                .OutputMeasurements = outputMeasurements
                .InputMeasurementKeys = inputMeasurementKeys
                .MinimumMeasurementsToUse = minimumMeasurementsToUse
                .ExpectedMeasurementsPerSecond = expectedMeasurementsPerSecond
                .LagTime = lagTime
                .LeadTime = leadTime

                ThreadPool.QueueUserWorkItem(AddressOf Initialize, .This)
            End With

        End Sub

        Public ReadOnly Property This() As CalculatedMeasurementInitialization
            Get
                Return Me
            End Get
        End Property

        Private Shared Sub Initialize(ByVal state As Object)

            ' Initialize calculated measurement - this is end user code and we're executing the initilization on a separate thread, so 
            ' we'll need to manually report any errors...
            With DirectCast(state, CalculatedMeasurementInitialization)
                Try
                    .CalculatedMeasurementAdapter.Initialize( _
                        .CalculationName, _
                        .ConfigurationSection, _
                        .OutputMeasurements, _
                        .InputMeasurementKeys, _
                        .MinimumMeasurementsToUse, _
                        .ExpectedMeasurementsPerSecond, _
                        .LagTime, _
                        .LeadTime)

                    ' We start the measurement concentration only after a successful initialization
                    .CalculatedMeasurementAdapter.Start()
                Catch ex As Exception
                    DisplayStatusMessage(String.Format("Exception during calculated measurement ""{0}"" initialization: {1}", .CalculatedMeasurementAdapter.Name, ex.Message))
                    m_exceptionLogger.Log(ex)
                End Try
            End With

        End Sub

    End Class

    Private Sub NewCalculatedMeasurements(ByVal measurements As IList(Of IMeasurement))

        If measurements IsNot Nothing Then
            Dim x As Integer

            ' Provide new calculated measurements to all calculated measurement modules such
            ' that calculated measurements can be based on other calculated measurements
            If m_calculatedMeasurements IsNot Nothing Then
                For x = 0 To m_calculatedMeasurements.Length - 1
                    m_calculatedMeasurements(x).QueueMeasurementsForCalculation(measurements)
                Next
            End If

            ' Queue new calculated measurements for archival
            If m_measurementReceivers IsNot Nothing Then
                For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                    receiver.QueueMeasurementsForArchival(measurements)
                Next
            End If

            ' Provide calculated measurements along to data concentrators
            If m_measurementConcentrators IsNot Nothing Then
                For x = 0 To m_measurementConcentrators.Length - 1
                    m_measurementConcentrators(x).SortMeasurements(measurements)
                Next
            End If
        End If

    End Sub

    Private Sub NewParsedMeasurements(ByVal measurements As ICollection(Of IMeasurement))

        ' Note that this data comes from a phasor measurement receiver which will have already
        ' queued the measurements for archival...

        If measurements IsNot Nothing Then
            ' Provide newly parsed measurements to all calculated measurement modules
            If m_calculatedMeasurements IsNot Nothing Then
                For x As Integer = 0 To m_calculatedMeasurements.Length - 1
                    m_calculatedMeasurements(x).QueueMeasurementsForCalculation(measurements)
                Next
            End If

            ' Provide newly parsed measurements to all data concentrators
            If m_measurementConcentrators IsNot Nothing Then
                For x As Integer = 0 To m_measurementConcentrators.Length - 1
                    m_measurementConcentrators(x).SortMeasurements(measurements)
                Next
            End If
        End If

    End Sub

    Private Sub CalculationException(ByVal source As String, ByVal ex As Exception)

        DisplayStatusMessage(String.Format("ERROR: ""{0}"" threw an exception: {1}", source, ex.Message))
        m_exceptionLogger.Log(ex)

    End Sub

    Private Function TryGetMapper(ByVal deviceAcronym As String, ByRef mapper As PhasorMeasurementMapper) As Boolean

        Try
            Dim pmuID As String = RemoveDuplicateWhiteSpace(deviceAcronym).Split(" "c)(1).ToUpper()
            Dim foundMapper As Boolean

            For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                If receiver.Mappers.TryGetValue(pmuID, mapper) Then
                    foundMapper = True
                    Exit For
                End If
            Next

            If Not foundMapper Then DisplayStatusMessage(String.Format("Failed to find a PMU or PDC in the connection list named ""{0}"", type ""List"" to see available list.", pmuID))

            Return foundMapper
        Catch ex As Exception
            DisplayStatusMessage(String.Format("Failed to lookup specified mapper due to exception: {0}", ex.Message))
            m_exceptionLogger.Log(ex)
            Return False
        End Try

    End Function

    Private Function ParseDeviceCommand(ByVal consoleLine As String) As PhasorProtocols.DeviceCommand

        Select Case RemoveDuplicateWhiteSpace(consoleLine).Split(" "c)(2).ToLower()
            Case "disabledata"
                Return DeviceCommand.DisableRealTimeData
            Case "enabledata"
                Return DeviceCommand.EnableRealTimeData
            Case "getconfig"
                Return DeviceCommand.SendConfigurationFrame2
            Case Else
                Return DeviceCommand.SendHeaderFrame
        End Select

    End Function

    Private ReadOnly Property MonitorInformation() As String
        Get
            With New StringBuilder
                .Append(EntryAssembly.Title)
                .Append(Environment.NewLine)
                .Append("   Assembly: ")
                .Append(EntryAssembly.Name)
                .Append(Environment.NewLine)
                .Append("    Version: ")
                .Append(EntryAssembly.Version)
                .Append(Environment.NewLine)
                .Append("   Location: ")
                .Append(EntryAssembly.Location)
                .Append(Environment.NewLine)
                .Append("    Created: ")
                .Append(EntryAssembly.BuildDate)
                .Append(Environment.NewLine)
                .Append("    NT User: ")
                .Append(WindowsIdentity.GetCurrent.Name)
                .Append(Environment.NewLine)
                .Append("    Runtime: ")
                .Append(EntryAssembly.ImageRuntimeVersion)
                .Append(Environment.NewLine)
                .Append("    Started: ")
                .Append(DateTime.Now.ToString())
                .Append(Environment.NewLine)

                Return .ToString()
            End With
        End Get
    End Property

    Private ReadOnly Property SystemStatus() As String
        Get
            With New StringBuilder
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append("*** Phasor Measurement Receivers (one per historian) ***")
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)

                For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                    .Append(receiver.Status)
                Next

                .Append(Environment.NewLine)
                .Append("*** Calculated Measurement Adapters ***")
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)

                For Each calculation As ICalculatedMeasurementAdapter In m_calculatedMeasurements
                    .Append(calculation.Status)
                Next

                .Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append("*** Phasor Data Concentrators ***")
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)

                For Each concentrator As PhasorDataConcentratorBase In m_measurementConcentrators
                    .Append(concentrator.Status)
                Next

                .Append(Environment.NewLine)

                Return .ToString()
            End With
        End Get
    End Property

    Private ReadOnly Property ConnectionList() As String
        Get
            With New StringBuilder
                .Append(Environment.NewLine)

                For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                    .Append("Receiver for historian ")
                    .Append(receiver.HistorianName)
                    .Append(Environment.NewLine)
                    .Append(">> PMU/PDC Connection List (")
                    .Append(receiver.Mappers.Count)
                    .Append(" Total)")
                    .Append(Environment.NewLine)
                    .Append(Environment.NewLine)

                    .Append("  Last Data Report Time:   PDC/PMU [PMU list]:")
                    .Append(Environment.NewLine)
                    .Append("  ------------------------ ----------------------------------------------------")
                    .Append(Environment.NewLine)
                    '          01-JAN-2006 12:12:24.000 SourceName [Pmu0, Pmu1, Pmu2, Pmu3, Pmu4]
                    '          >> No data has been parsed for SourceName - 00000 bytes received"

                    For Each mapper As PhasorMeasurementMapper In receiver.Mappers.Values
                        .Append("  ")
                        If mapper.LastReportTime > 0 Then
                            .Append((New DateTime(mapper.LastReportTime)).ToString("dd-MMM-yyyy HH:mm:ss.fff"))
                            .Append(" "c)
                            .Append(mapper.Name(6))
                            .Append(Environment.NewLine)
                        Else
                            .Append(">> No data has been parsed for ")
                            .Append(mapper.Name)
                            .Append(" - ")
                            .Append(mapper.TotalBytesReceived)
                            .Append(" bytes received")
                            .Append(Environment.NewLine)
                        End If
                    Next

                    .Append(Environment.NewLine)
                Next

                Return .ToString()
            End With
        End Get
    End Property

    Private ReadOnly Property CommandList() As String
        Get
            With New StringBuilder
                .Append("Possible commands:")
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append("  ""Connect PmuID""                 - Restarts PMU connection cycle")
                .Append(Environment.NewLine)
                .Append("  ""Disconnect PmuID""              - Terminates PMU connection")
                .Append(Environment.NewLine)
                .Append("  ""SendCommand PmuID DisableData"" - Turns off real-time data")
                .Append(Environment.NewLine)
                .Append("  ""SendCommand PmuID EnableData""  - Turns on real-time data")
                .Append(Environment.NewLine)
                .Append("  ""SendCommand PmuID GetConfig""   - Requests configuration frame")
                .Append(Environment.NewLine)
                .Append("  ""Reload""                        - Reloads the entire service process")
                .Append(Environment.NewLine)
                .Append("  ""ReconnectAll""                  - Reconnects archives and phasor devices")
                .Append(Environment.NewLine)
                .Append("  ""GC""                            - Force .NET Garbage Collection")
                .Append(Environment.NewLine)
                .Append("  ""Status""                        - Returns current service status")
                .Append(Environment.NewLine)
                .Append("  ""List""                          - Displays loaded PMU/PDC connections")
                .Append(Environment.NewLine)
                .Append("  ""Version""                       - Displays service version information")
                .Append(Environment.NewLine)
                .Append("  ""Help""                          - Displays this help information")
                .Append(Environment.NewLine)
                .Append("  ""Exit""                          - Exits this console monitor")
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)

                Return .ToString()
            End With
        End Get
    End Property

    Private Sub ForceGarbageCollection()

        DisplayStatusMessage("Forcing garbage collection and waiting for pending finalizers...")

        GC.Collect()
        GC.WaitForPendingFinalizers()

        DisplayStatusMessage("Garbage collection complete.")

    End Sub

    ' Display status messages bubbled up from phasor measurement receiver and its internal components
    Private Sub DisplayStatusMessage(ByVal status As String)

        ' We queue up status messages for display on a separate thread so we don't slow any important activity
        m_statusMessageQueue.Add(status)

    End Sub

    Private Sub DisplayStatusMessages(ByVal messages As String())

        Dim displayMessage As Boolean

        For x As Integer = 0 To messages.Length - 1
            ' When errors happen with data being processed at 30 samples per second you can get a hefty volume
            ' of errors very quickly, so to keep from flooding the message queue - we'll only send a handful
            ' of messages every couple of seconds
            If TicksToSeconds(Date.Now.Ticks - m_lastDisplayedMessageTime) < m_messageDisplayTimepan Then
                displayMessage = (m_displayedMessageCount < m_maximumMessagesToDisplay)
                m_displayedMessageCount += 1
            Else
                If m_displayedMessageCount > m_maximumMessagesToDisplay Then
                    Console.WriteLine("WARNING: {0} error messages discarded to avoid flooding message queue, check log for full detail", (m_displayedMessageCount - m_maximumMessagesToDisplay))
                    Console.WriteLine()
                End If
                displayMessage = True
                m_displayedMessageCount = 0
                m_lastDisplayedMessageTime = Date.Now.Ticks
            End If

            If displayMessage Then                
                Console.WriteLine(messages(x))
                Console.WriteLine()
            End If

            If m_systemLogEnabled Then
                ' Initialize system log if this is first call
                If m_systemLogFile Is Nothing Then
                    m_systemLogFile = New LogFile()
                    m_systemLogFile.Name = "SystemLog.txt"
                    m_systemLogFile.FileFullOperation = LogFileFullOperation.Rollover
                    m_systemLogFile.Open()
                End If

                m_systemLogFile.WriteTimestampedLine(messages(x))
            End If
        Next

    End Sub

End Module
