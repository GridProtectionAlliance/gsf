'*******************************************************************************************************
'  Service.vb - Code for windows service project "TVASPDC"
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
Imports TVA.IO.FilePath
Imports TVA.Measurements
Imports TVA.DateTime.Common
Imports TVA.Collections
Imports TVA.ErrorManagement
Imports TVA.Services
Imports TVA.Threading
Imports InterfaceAdapters
Imports PhasorProtocols

Public Class Service

#Region " Member Declarations "

    Private Delegate Sub InitializationFunctionSignature(ByVal connection As OleDbConnection)

    Private m_measurementReceivers As Dictionary(Of String, PhasorMeasurementReceiver)
    Private m_measurementConcentrators As PhasorDataConcentratorBase()
    Private m_calculatedMeasurements As ICalculatedMeasurementAdapter()
    Private m_messageDisplayTimepan As Integer
    Private m_maximumMessagesToDisplay As Integer
    Private m_lastDisplayedMessageTime As Long
    Private m_displayedMessageCount As Long
    Private m_useLocalClockAsRealTime As Boolean
    Private m_allowSortsByArrival As Boolean
    Private WithEvents m_statusMessageQueue As ProcessQueue(Of String)
    Private WithEvents m_healthExporter As MultipleDestinationExporter
    Private WithEvents m_statusExporter As MultipleDestinationExporter

#End Region

#Region " Service Event Handlers "

    Private Sub ServiceHelper_ServiceStarting(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of Object())) Handles ServiceHelper.ServiceStarting

        Dim _forceBuildNumInc As Integer = 2

        ' Make sure default service settings exist
        Settings.Add("PMUDatabase", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Databases\PhasorMeasurementData.mdb", "PMU metaData database connect string")
        'Settings.Add("PMUDatabase", "Provider=SQLOLEDB;Data Source=esoextsql;Initial Catalog=PMU_SDS;User ID=ESOPublic;pwd=4all2see", "PMU metaData database connect string")
        'Settings.Add("PMUDatabase", "Provider=SQLOLEDB;Data Source=esoasqlgendat\gendat;Initial Catalog=PhasorMeasurementData;User ID=NaspiApp;pwd=pw4site", "PMU metaData database connect string")
        'Settings.Add("PMUDatabase", "Provider=SQLOLEDB;Data Source=ESOOPSQL1;Initial Catalog=PhasorMeasurementData;User ID=NaspiApp;pwd=pw4site", "PMU metaData database connect string")
        Settings.Add("ReportingTolerance", "5", "Number of seconds of deviation from UTC time (according to local clock) that last PMU reporting time is allowed before considering it offline")
        Settings.Add("StatusReportingInterval", "10", "How often to update PMU reporting status in database in seconds - this should match time required by update trigger which calculates uptime")
        Settings.Add("DataLossInterval", "35000", "Number of milliseconds to wait for incoming data before restarting connection cycle to device")
        Settings.Add("MeasurementWarningThreshold", "100000", "Number of unarchived measurements allowed in a historian queue before displaying a warning message")
        Settings.Add("MeasurementDumpingThreshold", "500000", "Number of unarchived measurements allowed in a historian queue before taking evasive action and dumping data")
        Settings.Add("MessageDisplayTimespan", "2", "Timespan, in seconds, over which to monitor message volume")
        Settings.Add("MaximumMessagesToDisplay", "100", "Maximum number of messages to be tolerated during MessageDisplayTimespan")
        Settings.Add("EnableLogFile", "True", "Set to True to enable log file")
        Settings.Add("UseLocalClockAsRealTime", "True", "Set to True if local clock is very close to GPS, other set to False to use most recently received timestamp")
        Settings.Add("AllowSortsByArrival", "False", "Set to True to allow sorting of measurements by arrival for bad timestamps")
        SaveSettings()

        ' Create message display queue
        m_statusMessageQueue = ProcessQueue(Of String).CreateSynchronousQueue(AddressOf DisplayStatusMessages, 50, Timeout.Infinite, False, False)
        m_statusMessageQueue.Start()

        ' Create health and status exporters
        m_healthExporter = New MultipleDestinationExporter("HealthExporter", Timeout.Infinite, New ExportDestination() {New ExportDestination("\\pmuweb\NASPI\Health.txt", True, "TVA", "esocss", "pwd4ctrl")})
        ServiceHelper.ServiceComponents.Add(m_healthExporter)

        m_statusExporter = New MultipleDestinationExporter("StatusExporter", Timeout.Infinite, New ExportDestination() {New ExportDestination("\\pmuweb\NASPI\Status.txt", True, "TVA", "esocss", "pwd4ctrl")})
        ServiceHelper.ServiceComponents.Add(m_statusExporter)

        ' Determine if local system is configured with a real-time clock
        m_useLocalClockAsRealTime = BooleanSetting("UseLocalClockAsRealTime")

        ' Determine whether or not to allow measurement sorting by arrival for measurements with bad time quality
        m_allowSortsByArrival = BooleanSetting("AllowSortsByArrival")

        ServiceHelper.LogStatusUpdates = BooleanSetting("EnableLogFile")

        With ServiceHelper.GlobalExceptionLogger
            .LogToEventLog = False
            .LogToFile = True
            .PersistSettings = True
            With .LogFile
                .FileFullOperation = LogFileFullOperation.Rollover
                .Name = "SystemLog.txt"
                .PersistSettings = True
            End With
        End With

        ServiceHelper.ClientRequestHandlers.Add(New ClientRequestHandlerInfo("List", "Displays current PMU/PDC connections", AddressOf ShowCurrentConnections))
        ServiceHelper.ClientRequestHandlers.Add(New ClientRequestHandlerInfo("Connect", "Starts connection cycle to specified device", AddressOf ConnectDevice))
        ServiceHelper.ClientRequestHandlers.Add(New ClientRequestHandlerInfo("Disconnect", "Disconnects specified device", AddressOf DisconnectDevice))
        ServiceHelper.ClientRequestHandlers.Add(New ClientRequestHandlerInfo("SendCommand", "Sends command to specified device", AddressOf SendDeviceCommand))
        ServiceHelper.ClientRequestHandlers.Add(New ClientRequestHandlerInfo("ThreadList", "Displays details for managed threads", AddressOf ActiveThreadList))
        ServiceHelper.ClientRequestHandlers.Add(New ClientRequestHandlerInfo("SysInit", "Starts a controlled system initialization", AddressOf ControlledSystemInitialization, False))
        ServiceHelper.ClientRequestHandlers.Add(New ClientRequestHandlerInfo("GC", "Forces a .NET garbage collection", AddressOf ForceGarbageCollection, False))

        DisplayStatusMessage(String.Format("*** System Initializing - [UTC: {0}] ***", Date.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")))

    End Sub

    Private Sub ServiceHelper_ServiceStarted(ByVal sender As Object, ByVal e As System.EventArgs) Handles ServiceHelper.ServiceStarted

        Try
#If Not Debug Then
            ' Start system initialization on an independent thread so that service responds in a timely fashion...
            ThreadPool.UnsafeQueueUserWorkItem(AddressOf InitializeSystem, Nothing)
#End If

            ' We add a scheduled process to automatically request health status every minute - user can change schedule in config file
            ServiceHelper.AddScheduledProcess(AddressOf HealthMonitorProcess, "HealthMonitor", "* * * * *")

            ' We add a scheduled process to automatically export status information every 30 minutes - user can change schedule in config file
            ServiceHelper.AddScheduledProcess(AddressOf StatusExportProcess, "StatusExport", "*/30 * * * *")
        Catch ex As Exception
            ServiceHelper.GlobalExceptionLogger.Log(ex)
            ServiceHelper.Service.Stop()
        End Try

    End Sub

#End Region

#Region " System Initialization "

    Private Sub InitializeSystem(ByVal state As Object)

        Dim connection As OleDbConnection
        Dim x As Integer

        Try
            connection = New OleDbConnection(StringSetting("PMUDatabase"))
            connection.Open()

            ' To make sure any initialization messages don't get missed, we increase message
            ' tolerance to maximum during the initialization process
            m_messageDisplayTimepan = 10
            m_maximumMessagesToDisplay = Integer.MaxValue
            m_displayedMessageCount = 0

            DisplayStatusMessage("PMU database connection opened...")

            ' If calculated measurements are already defined (i.e., we are "re-initializing" code), we need to stop
            ' existing calculation instances such that all items can be shut-down in an orderly fashion
            If m_calculatedMeasurements IsNot Nothing Then
                For x = 0 To m_calculatedMeasurements.Length - 1
                    ServiceHelper.ServiceComponents.Remove(m_calculatedMeasurements(x))
                    m_calculatedMeasurements(x).Stop()
                Next
            End If

            ' Define all of the calculated measurements
            m_calculatedMeasurements = LoadCalculatedMeasurements(connection)

            ' If the phasor measurement receivers are already defined we must be reloading - so we attempt
            ' an orderly shutdown
            If m_measurementReceivers IsNot Nothing Then
                For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                    ServiceHelper.ServiceComponents.Remove(receiver)
                    receiver.Dispose()
                Next
            End If

            ' Load the phasor measurement receivers (one per each established archive)
            m_measurementReceivers = LoadMeasurementReceivers(connection, IntegerSetting("ReportingTolerance"), IntegerSetting("StatusReportingInterval"), IntegerSetting("DataLossInterval"))

            ' If the phasor measurement concentrators are already defined we must be reloading - so we attempt
            ' an orderly shutdown
            If m_measurementConcentrators IsNot Nothing Then
                For Each concentrator As PhasorDataConcentratorBase In m_measurementConcentrators
                    ServiceHelper.ServiceComponents.Remove(concentrator)
                    concentrator.Stop()
                Next
            End If

            ' Load the phasor measurement concentrators
            m_measurementConcentrators = LoadMeasurementConcentrators(connection)

            ' Register all these items as service components
            For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                ServiceHelper.ServiceComponents.Add(receiver)
            Next

            For x = 0 To m_calculatedMeasurements.Length - 1
                ServiceHelper.ServiceComponents.Add(m_calculatedMeasurements(x))
            Next

            For Each concentrator As PhasorDataConcentratorBase In m_measurementConcentrators
                ServiceHelper.ServiceComponents.Add(concentrator)
            Next

            ' Restore normal message volume tolerances
            m_messageDisplayTimepan = IntegerSetting("MessageDisplayTimespan")
            m_maximumMessagesToDisplay = IntegerSetting("MaximumMessagesToDisplay")
            m_displayedMessageCount = 0
        Catch ex As Exception
            DisplayStatusMessage(String.Format("Failure during initialization: {0}", ex.Message))
            ServiceHelper.GlobalExceptionLogger.Log(ex)
        Finally
            If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then connection.Close()
            DisplayStatusMessage("PMU database connection closed.")
        End Try

    End Sub

    Private Function LoadMeasurementReceivers(ByVal connection As OleDbConnection, ByVal reportingTolerance As Integer, ByVal statusReportingInterval As Integer, ByVal dataLossInterval As Integer) As Dictionary(Of String, PhasorMeasurementReceiver)

        Dim measurementReceivers As New Dictionary(Of String, PhasorMeasurementReceiver)
        Dim measurementReceiver As PhasorMeasurementReceiver
        Dim externalAssemblyName As String
        Dim externalAssembly As Assembly
        Dim historianAdapter As IHistorianAdapter
        Dim connectionString As String = StringSetting("PMUDatabase")
        Dim warningThreshold As Integer = IntegerSetting("MeasurementWarningThreshold")
        Dim dumpingThreshold As Integer = IntegerSetting("MeasurementDumpingThreshold")
        Dim archiveSource As String

        With RetrieveData("SELECT * FROM Historian WHERE Enabled <> 0", connection).Rows
            For x As Integer = 0 To .Count - 1
                Try
                    With .Item(x)
                        archiveSource = .Item("Acronym").ToString()
                        externalAssemblyName = .Item("AssemblyName").ToString()

                        ' Load the external assembly for the historian adapter
                        externalAssembly = Assembly.LoadFrom(PrefixLocalPath(externalAssemblyName))

                        ' Create a new instance of the historian adpater
                        historianAdapter = DirectCast(Activator.CreateInstance(externalAssembly.GetType(.Item("TypeName").ToString())), IHistorianAdapter)
                        historianAdapter.Initialize(.Item("ConnectionString").ToString())

                        measurementReceiver = New PhasorMeasurementReceiver( _
                            historianAdapter, _
                            archiveSource, _
                            connectionString, _
                            reportingTolerance, _
                            statusReportingInterval, _
                            dataLossInterval, _
                            warningThreshold, _
                            dumpingThreshold, _
                            ServiceHelper.GlobalExceptionLogger)

                        AddHandler measurementReceiver.StatusMessage, AddressOf DisplayStatusMessage
                        AddHandler measurementReceiver.NewMeasurements, AddressOf NewParsedMeasurements
                        measurementReceiver.Initialize(connection)

                        measurementReceivers.Add(archiveSource, measurementReceiver)
                    End With
                Catch ex As Exception
                    DisplayStatusMessage(String.Format("Failed to load measurement receiver for archive ""{0}"" from assembly ""{1}"" due to exception: {2}", archiveSource, externalAssemblyName, ex.Message))
                    ServiceHelper.GlobalExceptionLogger.Log(ex)
                End Try
            Next
        End With

        Return measurementReceivers

    End Function

    Private Function LoadMeasurementConcentrators(ByVal connection As OleDbConnection) As PhasorDataConcentratorBase()

        Dim measurementConcentrators As New List(Of PhasorDataConcentratorBase)
        Dim measurementConcentrator As PhasorDataConcentratorBase
        Dim communicationServer As ICommunicationServer
        Dim connectionString As String
        Dim keys As Dictionary(Of String, String)
        Dim value As String

        With RetrieveData("SELECT * FROM Concentrator WHERE Enabled <> 0", connection).Rows
            For x As Integer = 0 To .Count - 1
                With .Item(x)
                    Try
                        ' Create communications server for concentrator
                        connectionString = .Item("ConnectionString").ToString()
                        communicationServer = CreateCommunicationServer(connectionString)
                        communicationServer.Handshake = False
                        measurementConcentrator = Nothing

                        ' Load protocol specific key/value pairs
                        keys = ParseKeyValuePairs(connectionString)

                        ' Create new data concentrator
                        Select Case .Item("Type").ToString().Trim.ToUpper()
                            Case "IEEEC37.118"
                                Dim timeBase As Integer = 16777215
                                Dim version As Byte = 1

                                If keys.TryGetValue("timebase", value) Then timeBase = Convert.ToInt32(value)
                                If keys.TryGetValue("version", value) Then version = Convert.ToByte(value)

                                measurementConcentrator = New IeeeC37_118Concentrator( _
                                        communicationServer, _
                                        .Item("Name").ToString(), _
                                        Convert.ToInt32(.Item("FrameRate")), _
                                        Convert.ToDouble(.Item("LagTime")), _
                                        Convert.ToDouble(.Item("LeadTime")), _
                                        timeBase, _
                                        version, _
                                        ServiceHelper.GlobalExceptionLogger)
                            Case "BPAPDCSTREAM"
                                Dim iniFileName As String

                                If Not keys.TryGetValue("inifilename", iniFileName) Then
                                    Throw New InvalidOperationException("Did not find key/valur pair for ""INIFileName"" in BPA PDCstream concentrator connection string.  BPA PDCstream concentrator cannot start without a valid INI based configuration file.")
                                End If

                                iniFileName = String.Concat(FilePath.GetApplicationPath(), iniFileName)

                                measurementConcentrator = New BpaPdcConcentrator( _
                                    communicationServer, _
                                    .Item("Name").ToString(), _
                                    Convert.ToInt32(.Item("FrameRate")), _
                                    Convert.ToDouble(.Item("LagTime")), _
                                    Convert.ToDouble(.Item("LeadTime")), _
                                    iniFileName, _
                                    ServiceHelper.GlobalExceptionLogger)
                        End Select

                        If measurementConcentrator IsNot Nothing Then
                            ' Expose status messages from measurement concentrator 
                            AddHandler measurementConcentrator.StatusMessage, AddressOf DisplayStatusMessage

                            ' Initialize measurement concentrator (sets up signal references and configuration frames)
                            measurementConcentrator.Initialize( _
                                connection, _
                                .Item("PmuFilterSql").ToString(), _
                                Convert.ToByte(.Item("NominalFrequency")), _
                                Convert.ToUInt16(.Item("IDCode")))

                            ' It's optimal to use local clock as real-time if local clock can be trusted...
                            measurementConcentrator.UseLocalClockAsRealTime = m_useLocalClockAsRealTime

                            ' Determine whether or not to allow sorts by arrival for bad timestamps...
                            measurementConcentrator.AllowSortsByArrival = m_allowSortsByArrival

                            ' Start measurement concentrator
                            measurementConcentrator.Start()

                            measurementConcentrators.Add(measurementConcentrator)
                        End If
                    Catch ex As Exception
                        DisplayStatusMessage(String.Format("Failed to load measurement concentrator ""{0}"" due to exception: {1}", .Item("Name").ToString(), ex.Message))
                        ServiceHelper.GlobalExceptionLogger.Log(ex)
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
        With RetrieveData("SELECT * FROM CalculatedMeasurement WHERE Enabled <> 0", connection).Rows
            For x As Integer = 0 To .Count - 1
                Try
                    With .Item(x)
                        ' Load the external assembly
                        calculatedMeasurementName = .Item("Name").ToString()
                        externalAssemblyName = .Item("AssemblyName").ToString()
                        externalAssembly = Assembly.LoadFrom(PrefixLocalPath(externalAssemblyName))

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
                                ServiceHelper.GlobalExceptionLogger.Log(ex)
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
                                ServiceHelper.GlobalExceptionLogger.Log(ex)
                            End Try
                        End If

                        calculatedMeasurementAdapter = Nothing

                        ' Load the specified type from the assembly
                        Try
                            ' Create a new instance of the adpater
                            calculatedMeasurementAdapter = DirectCast(Activator.CreateInstance(externalAssembly.GetType(.Item("TypeName").ToString())), ICalculatedMeasurementAdapter)
                        Catch ex As Exception
                            DisplayStatusMessage(String.Format("Failed to load type ""{0}"" from assembly ""{1}"" for ""{2}"" due to exception: {3}", .Item("TypeName").ToString(), externalAssemblyName, calculatedMeasurementName, ex.Message))
                            ServiceHelper.GlobalExceptionLogger.Log(ex)
                        End Try

                        If calculatedMeasurementAdapter IsNot Nothing Then
                            ' For calculated measurements that use a concentrator as their base class, we adjust UseLocalClockAsRealTime
                            ' property as needed - it's optimal to use local clock as real-time if local clock can be trusted...
                            Dim concentrator As ConcentratorBase = TryCast(calculatedMeasurementAdapter, ConcentratorBase)
                            If concentrator IsNot Nothing Then
                                concentrator.UseLocalClockAsRealTime = m_useLocalClockAsRealTime
                                concentrator.AllowSortsByArrival = m_allowSortsByArrival
                            End If

                            ' Bubble calculation module status messages out to local update status function
                            AddHandler calculatedMeasurementAdapter.StatusMessage, AddressOf DisplayStatusMessage

                            ' Bubble newly calculated measurement out to functions that need the real-time data
                            AddHandler calculatedMeasurementAdapter.NewCalculatedMeasurements, AddressOf NewCalculatedMeasurements

                            ' Bubble calculation exceptions out to procedure that can handle these exceptions
                            AddHandler calculatedMeasurementAdapter.CalculationException, AddressOf CalculationException

                            ' Intialize calculated measurement adapter - we do this on a separate thread in case this task takes some time
                            CalculatedMeasurementInitialization.Execute(Me, _
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
                    ServiceHelper.GlobalExceptionLogger.Log(ex)
                End Try
            Next
        End With

        Return calculatedMeasurementAdapters.ToArray()

    End Function

    Private Class CalculatedMeasurementInitialization

        Public Parent As Service
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
            ByVal parent As Service, _
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
                .Parent = parent
                .CalculatedMeasurementAdapter = calculatedMeasurementAdapter
                .CalculationName = calculationName
                .ConfigurationSection = configurationSection
                .OutputMeasurements = outputMeasurements
                .InputMeasurementKeys = inputMeasurementKeys
                .MinimumMeasurementsToUse = minimumMeasurementsToUse
                .ExpectedMeasurementsPerSecond = expectedMeasurementsPerSecond
                .LagTime = lagTime
                .LeadTime = leadTime

#If ThreadTracking Then
                With TVA.Threading.ManagedThreadPool.QueueUserWorkItem(AddressOf Initialize, .This)
                    .Name = "TVASPDC.Service.CalculatedMeasurementInitialization.Initialize()"
                End With
#Else
                ThreadPool.QueueUserWorkItem(AddressOf Initialize, .This)
#End If
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
                    .CalculatedMeasurementAdapter.Initialized = True
                Catch ex As Exception
                    ' Calculation failed to initialize
                    .CalculatedMeasurementAdapter.Initialized = False
                    .Parent.DisplayStatusMessage(String.Format("Exception during calculated measurement ""{0}"" initialization: {1}", DirectCast(.CalculatedMeasurementAdapter, IAdapter).Name, ex.Message))
                    .Parent.ServiceHelper.GlobalExceptionLogger.Log(ex)

                    ' Unregister calculation events...
                    RemoveHandler .CalculatedMeasurementAdapter.StatusMessage, AddressOf .Parent.DisplayStatusMessage
                    RemoveHandler .CalculatedMeasurementAdapter.NewCalculatedMeasurements, AddressOf .Parent.NewCalculatedMeasurements
                    RemoveHandler .CalculatedMeasurementAdapter.CalculationException, AddressOf .Parent.CalculationException
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
                    If m_calculatedMeasurements(x).Initialized Then m_calculatedMeasurements(x).QueueMeasurementsForCalculation(measurements)
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
            Dim x As Integer

            ' Provide newly parsed measurements to all calculated measurement modules
            If m_calculatedMeasurements IsNot Nothing Then
                For x = 0 To m_calculatedMeasurements.Length - 1
                    If m_calculatedMeasurements(x).Initialized Then m_calculatedMeasurements(x).QueueMeasurementsForCalculation(measurements)
                Next
            End If

            ' Provide newly parsed measurements to all data concentrators
            If m_measurementConcentrators IsNot Nothing Then
                For x = 0 To m_measurementConcentrators.Length - 1
                    m_measurementConcentrators(x).SortMeasurements(measurements)
                Next
            End If
        End If

    End Sub

    Private Sub CalculationException(ByVal source As String, ByVal ex As Exception)

        DisplayStatusMessage(String.Format("ERROR: ""{0}"" threw an exception: {1}", source, ex.Message))
        ServiceHelper.GlobalExceptionLogger.Log(ex)

    End Sub

    Private Function PrefixLocalPath(ByVal assemblyPath As String) As String

        If String.Compare(JustPath(assemblyPath), "\") = 0 Then
            Return GetApplicationPath() & assemblyPath
        Else
            Return assemblyPath
        End If

    End Function

    Private Sub HealthMonitorProcess(ByVal name As String, ByVal parameters As Object())

        ' We pretend to be a client and send a "Health" command to ourselves...
        Dim healthRequest As ClientRequest = ClientRequest.Parse("Health")
        ServiceHelper.ClientRequestHandlers(healthRequest.Command).HandlerMethod(New ClientRequestInfo(New ClientInfo(System.Guid.Empty), healthRequest))

        ' We also export the health information
        m_healthExporter.ExportData(ServiceHelper.PerformanceMonitor.Status)

    End Sub

    Private Sub StatusExportProcess(ByVal name As String, ByVal parameters As Object())

        With New StringBuilder()
            .Append(String.Format("Status of components used by {0}:", ServiceName))
            .AppendLine()
            For Each serviceComponent As IServiceComponent In ServiceHelper.ServiceComponents
                If serviceComponent IsNot Nothing Then
                    .AppendLine()
                    .Append(String.Format("Status of {0}:", serviceComponent.Name))
                    .AppendLine()
                    .Append(serviceComponent.Status)
                End If
            Next

            m_statusExporter.ExportData(.ToString())
        End With

    End Sub

#End Region

#Region " Request Handlers "

    Private Sub ShowCurrentConnections(ByVal requestInfo As ClientRequestInfo)

        Dim framesPerSecond As String
        Dim maximumLength As Integer

        If requestInfo.Request.Arguments.ContainsHelpRequest Then
            With New StringBuilder()
                .Append("Displays current PMU/PDC connections.")
                .AppendLine()
                .AppendLine()
                .Append("   Usage:")
                .AppendLine()
                .Append("       List options")
                .AppendLine()
                .AppendLine()
                .Append("   Options:")
                .AppendLine()
                .Append("       -?".PadRight(20))
                .Append("Displays this help message")

                ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
            End With
        Else
            With New StringBuilder
                .AppendFormat("System Uptime: {0}", SecondsToText(ServiceHelper.CommunicationServer.RunTime))
                .AppendLine()
                .AppendLine()

                For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                    .Append("Receiver for historian ")
                    .Append(receiver.HistorianName)
                    .AppendLine()
                    .Append(">> Device Connection List (")
                    .Append(receiver.Mappers.Count)
                    .Append(" Total)")
                    .AppendLine()
                    .AppendLine()

                    .Append("  Last Data Report Time: Device Acronym:                            Frames/Sec:")
                    .AppendLine()
                    .Append("  ---------------------- ------------------------------------------------------")
                    '                                 123456789012345678901234567890123456789012345678901234567890
                    '                                          1         2         3         4         5         6
                    .AppendLine()
                    '          01Jan2006 12:12:24.000 SourceName [PDC: n devices]                      30.01
                    '          >> SourceName awaiting config frame - 1000 bytes received
                    '          ** SourceName is not connected

                    For Each mapper As PhasorMeasurementMapper In receiver.Mappers.Values
                        .Append("  ")

                        If mapper.IsConnected Then
                            If mapper.LastReportTime > 0 Then
                                framesPerSecond = " " & Convert.ToInt32(mapper.CalculatedFrameRate).ToString()
                                maximumLength = 54 - framesPerSecond.Length
                                .Append((New DateTime(mapper.LastReportTime)).ToString("ddMMMyyyy HH:mm:ss.fff "))
                                .Append(TruncateRight(mapper.Name, maximumLength).PadRight(maximumLength))
                                .Append(framesPerSecond)
                            Else
                                .Append(">> ")
                                .Append(mapper.Name)
                                .Append(" awaiting config frame - ")
                                .Append(mapper.TotalBytesReceived)
                                .Append(" bytes received")
                            End If
                        Else
                            .Append("** ")
                            .Append(mapper.Name)
                            .Append(" is not connected")
                        End If

                        .AppendLine()
                    Next

                    .AppendLine()
                Next

                ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
            End With
        End If

    End Sub

    Private Sub ControlledSystemInitialization(ByVal requestInfo As ClientRequestInfo)

        If requestInfo.Request.Arguments.ContainsHelpRequest Then
            With New StringBuilder()
                .Append("Starts a controlled system initialization.")
                .AppendLine()
                .AppendLine()
                .Append("   Usage:")
                .AppendLine()
                .Append("       SysInit options")
                .AppendLine()
                .AppendLine()
                .Append("   Options:")
                .AppendLine()
                .Append("       -?".PadRight(20))
                .Append("Displays this help message")

                ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
            End With
        Else
            DisplayStatusMessage("Starting controlled system initialization...")

            InitializeSystem(Nothing)

            DisplayStatusMessage("Controlled system initialization complete.")
        End If

    End Sub

    Private Sub ForceGarbageCollection(ByVal requestInfo As ClientRequestInfo)

        If requestInfo.Request.Arguments.ContainsHelpRequest Then
            With New StringBuilder()
                .Append("Forces a .NET garbage collection.")
                .AppendLine()
                .AppendLine()
                .Append("   Usage:")
                .AppendLine()
                .Append("       GC options")
                .AppendLine()
                .AppendLine()
                .Append("   Options:")
                .AppendLine()
                .Append("       -?".PadRight(20))
                .Append("Displays this help message")

                ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
            End With
        Else
            DisplayStatusMessage("Forcing garbage collection and waiting for pending finalizers...")

            GC.Collect()
            GC.WaitForPendingFinalizers()

            DisplayStatusMessage("Garbage collection complete.")
        End If

    End Sub

    Private Sub ConnectDevice(ByVal requestInfo As ClientRequestInfo)

        With requestInfo.Request.Arguments
            If .ContainsHelpRequest Then
                With New StringBuilder()
                    .Append("Starts connection cycle to specified device.")
                    .AppendLine()
                    .AppendLine()
                    .Append("   Usage:")
                    .AppendLine()
                    .Append("       Connect options")
                    .AppendLine()
                    .AppendLine()
                    .Append("   Options:")
                    .AppendLine()
                    .Append("       -?".PadRight(20))
                    .Append("Displays this help message")
                    .AppendLine()
                    .Append("       deviceID".PadRight(20))
                    .Append("Starts connection for specified device")

                    ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
                End With
            Else
                If .OrderedArgCount > 0 Then
                    Dim mapper As PhasorMeasurementMapper

                    If TryGetMapper(.Item("OrderedArg1"), requestInfo.Sender.ClientID, mapper) Then
                        mapper.Connect()
                    End If
                End If
            End If
        End With

    End Sub

    Private Sub DisconnectDevice(ByVal requestInfo As ClientRequestInfo)

        With requestInfo.Request.Arguments
            If .ContainsHelpRequest Then
                With New StringBuilder()
                    .Append("Disconnects specified device.")
                    .AppendLine()
                    .AppendLine()
                    .Append("   Usage:")
                    .AppendLine()
                    .Append("       Disconnect options")
                    .AppendLine()
                    .AppendLine()
                    .Append("   Options:")
                    .AppendLine()
                    .Append("       -?".PadRight(20))
                    .Append("Displays this help message")
                    .AppendLine()
                    .Append("       deviceID".PadRight(20))
                    .Append("Disconnects from specified device")

                    ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
                End With
            Else
                If .OrderedArgCount > 0 Then
                    Dim mapper As PhasorMeasurementMapper

                    If TryGetMapper(.Item("OrderedArg1"), requestInfo.Sender.ClientID, mapper) Then
                        mapper.Disconnect()
                    End If
                End If
            End If
        End With

    End Sub

    Private Sub SendDeviceCommand(ByVal requestInfo As ClientRequestInfo)

        With requestInfo.Request.Arguments
            If .ContainsHelpRequest Then
                With New StringBuilder()
                    .Append("Sends command to specified device.")
                    .AppendLine()
                    .AppendLine()
                    .Append("   Usage:")
                    .AppendLine()
                    .Append("       SendCommand options")
                    .AppendLine()
                    .AppendLine()
                    .Append("   Options:")
                    .AppendLine()
                    .Append("       -?".PadRight(30))
                    .Append("Displays this help message")
                    .AppendLine()
                    '        123456789012345678901234567890
                    .Append("       deviceID DisableData".PadRight(30))
                    .Append("Turns off real-time data")
                    .AppendLine()
                    .Append("       deviceID EnableData".PadRight(30))
                    .Append("Turns on real-time data")
                    .AppendLine()
                    .Append("       deviceID GetConfig".PadRight(30))
                    .Append("Requests configuration frame")

                    ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
                End With
            Else
                If .OrderedArgCount > 1 Then
                    Dim mapper As PhasorMeasurementMapper

                    If TryGetMapper(.Item("OrderedArg1"), requestInfo.Sender.ClientID, mapper) Then
                        Select Case .Item("OrderedArg2").ToLower()
                            Case "disabledata"
                                mapper.SendDeviceCommand(DeviceCommand.DisableRealTimeData)
                            Case "enabledata"
                                mapper.SendDeviceCommand(DeviceCommand.EnableRealTimeData)
                            Case "getconfig"
                                mapper.SendDeviceCommand(DeviceCommand.SendConfigurationFrame2)
                            Case Else
                                ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, String.Format("Unrecognized command ""{0}""", .Item("OrderedArg2")), ServiceHelper.UpdateCrlfCount)
                        End Select
                    End If
                End If
            End If
        End With

    End Sub

    Private Sub ActiveThreadList(ByVal requestInfo As ClientRequestInfo)

        With requestInfo.Request.Arguments
            If .ContainsHelpRequest Then
                With New StringBuilder()
                    .Append("Displays detail for all active managed threads.")
                    .AppendLine()
                    .AppendLine()
                    .Append("   Usage:")
                    .AppendLine()
                    .Append("       ThreadList options")
                    .AppendLine()
                    .AppendLine()
                    .Append("   Options:")
                    .AppendLine()
                    .Append("       -?".PadRight(30))
                    .Append("Displays this help message")
                    .AppendLine()

                    ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
                End With
            Else
                With New StringBuilder
#If ThreadTracking Then
                    .Append("This build has thread tracking enabled.")
#Else
                    .Append("This build does not have thread tracking enabled.")
#End If
                    .AppendLine()
                    .AppendLine()

                    .Append(ManagedThreads.ActiveThreadStatus)

                    ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
                End With
            End If
        End With

    End Sub

    Private Function TryGetMapper(ByVal deviceAcronym As String, ByVal clientID As Guid, ByRef mapper As PhasorMeasurementMapper) As Boolean

        Try
            Dim foundMapper As Boolean

            For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                If receiver.Mappers.TryGetValue(deviceAcronym, mapper) Then
                    foundMapper = True
                    Exit For
                End If
            Next

            If Not foundMapper Then ServiceHelper.UpdateStatus(clientID, String.Format("Failed to find a PMU or PDC in the connection list named ""{0}"", type ""List"" to see available list.", deviceAcronym), ServiceHelper.UpdateCrlfCount)

            Return foundMapper
        Catch ex As Exception
            ServiceHelper.UpdateStatus(clientID, String.Format("Failed to lookup specified mapper due to exception: {0}", ex.Message), ServiceHelper.UpdateCrlfCount)
            ServiceHelper.GlobalExceptionLogger.Log(ex)
            Return False
        End Try

    End Function

#End Region

#Region " Broadcast Message Handling "

    ' Display status messages bubbled up from phasor measurement receiver and its internal components
    Public Sub DisplayStatusMessage(ByVal status As String)

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
                    ServiceHelper.UpdateStatus(String.Format("WARNING: {0} error messages discarded to avoid flooding message queue, check log for full detail", (m_displayedMessageCount - m_maximumMessagesToDisplay)))
                End If
                displayMessage = True
                m_displayedMessageCount = 0
                m_lastDisplayedMessageTime = Date.Now.Ticks
            End If

            If displayMessage Then
                ' We curtail output messages as needed
                ServiceHelper.UpdateStatus(messages(x), 2)
            ElseIf ServiceHelper.LogStatusUpdates Then
                ' But we always log messages
                ServiceHelper.LogFile.WriteTimestampedLine(messages(x))
            End If
        Next

    End Sub

    Private Sub m_statusMessageQueue_ProcessException(ByVal ex As System.Exception) Handles m_statusMessageQueue.ProcessException

        ServiceHelper.GlobalExceptionLogger.Log(ex)

    End Sub

#End Region

#Region " Old Code "

    'ServiceHelper.ClientRequestHandlers.Add(New ClientRequestHandlerInfo("Status", "Displays detailed status for all system components", AddressOf ShowStatus))

    'Private Sub ShowStatus(ByVal requestInfo As ClientRequestInfo)

    '    If requestInfo.Request.Arguments.ContainsHelpRequest Then
    '        With New StringBuilder()
    '            .Append("Displays detailed status for all system components.")
    '            .AppendLine()
    '            .AppendLine()
    '            .Append("   Usage:")
    '            .AppendLine()
    '            .Append("       Status options")
    '            .AppendLine()
    '            .AppendLine()
    '            .Append("   Options:")
    '            .AppendLine()
    '            .Append("       -?".PadRight(20))
    '            .Append("Displays this help message")

    '            ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
    '        End With
    '    Else
    '        With New StringBuilder
    '            .AppendLine()
    '            .AppendLine()
    '            .Append("*** Phasor Measurement Receivers (one per historian) ***")
    '            .AppendLine()
    '            .AppendLine()

    '            For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
    '                .Append(receiver.Status)
    '            Next

    '            .AppendLine()
    '            .Append("*** Calculated Measurement Adapters ***")
    '            .AppendLine()
    '            .AppendLine()

    '            For Each calculation As ICalculatedMeasurementAdapter In m_calculatedMeasurements
    '                .Append(calculation.Status)
    '            Next

    '            .AppendLine()
    '            .AppendLine()
    '            .Append("*** Phasor Data Concentrators ***")
    '            .AppendLine()
    '            .AppendLine()

    '            For Each concentrator As PhasorDataConcentratorBase In m_measurementConcentrators
    '                .Append(concentrator.Status)
    '            Next

    '            .AppendLine()

    '            ServiceHelper.UpdateStatus(requestInfo.Sender.ClientID, .ToString(), ServiceHelper.UpdateCrlfCount)
    '        End With
    '    End If

    'End Sub

#End Region

End Class
