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
Imports System.Data.SqlClient
Imports TVA.Common
Imports TVA.Assembly
Imports TVA.IO
Imports TVA.IO.FilePath
Imports TVA.Configuration.Common
Imports TVA.Text.Common
Imports TVA.Data.Common
Imports TVA.Measurements
Imports TVA.DateTime.Common
Imports TVA.Phasors
Imports InterfaceAdapters
Imports System.Reflection

Module MainModule

    Private Delegate Sub InitializationFunctionSignature(ByVal connection As SqlConnection)

    Private m_measurementReceivers As Dictionary(Of String, PhasorMeasurementReceiver)
    Private m_calculatedMeasurements As ICalculatedMeasurementAdapter()
    Private m_messageDisplayTimepan As Integer
    Private m_maximumMessagesToDisplay As Integer
    Private m_lastDisplayedMessageTime As Long
    Private m_displayedMessageCount As Long
    Private m_systemLogEnabled As Boolean
    Private m_systemLogFile As LogFile

    Public Sub Main()

        Dim consoleLine As String
        Dim receiver As PhasorMeasurementReceiver
        Dim mapper As PhasorMeasurementMapper = Nothing

        Console.WriteLine(MonitorInformation)

        ' Make sure service settings exist
        Settings.Add("PMUDatabase", "Data Source=ESOEXTSQL;Initial Catalog=PMU_SDS;Integrated Security=False;user ID=ESOPublic;pwd=4all2see", "PMU metaData database connect string")
        Settings.Add("PMUStatusInterval", "5", "Number of seconds of deviation from UTC time (according to local clock) that last PMU reporting time is allowed before considering it offline")
        Settings.Add("DataLossInterval", "35000", "Number of milliseconds to wait for incoming data before restarting connection cycle to device")
        Settings.Add("MessageDisplayTimespan", "2", "Timespan, in seconds, over which to monitor message volume")
        Settings.Add("MaximumMessagesToDisplay", "100", "Maximum number of messages to be tolerated during MessageDisplayTimespan")
        Settings.Add("EnableLogFile", "True", "Set to ""True"" to enable log file")
        SaveSettings()

        m_systemLogEnabled = BooleanSetting("EnableLogFile")

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
                Console.WriteLine()
                InitializeConfiguration(AddressOf InitializeSystem)
            ElseIf consoleLine.StartsWith("reconnectall", True, Nothing) Then
                Console.WriteLine()
                InitializeConfiguration(AddressOf ReinitializeReceivers)
            ElseIf consoleLine.StartsWith("gc", True, Nothing) Then
                ForceGarbageCollection()
            ElseIf consoleLine.StartsWith("status", True, Nothing) Then
                Console.WriteLine()
                For Each receiver In m_measurementReceivers.Values
                    Console.WriteLine(receiver.Status)
                Next
                For x As Integer = 0 To m_calculatedMeasurements.Length - 1
                    Console.WriteLine(m_calculatedMeasurements(x).Status)
                Next

                Dim totalWorkerThreads, availableWorkerThreads, totalIOThreads, availableIOThreads As Integer

                Threading.ThreadPool.GetMaxThreads(totalWorkerThreads, totalIOThreads)
                Threading.ThreadPool.GetAvailableThreads(availableWorkerThreads, availableIOThreads)

                Console.WriteLine("Worker Thread Utilization: " & ((totalWorkerThreads - availableWorkerThreads) / totalWorkerThreads * 100.0R).ToString("0.00%"))
                Console.WriteLine("    IO Thread Utilization: " & ((totalIOThreads - availableIOThreads) / totalIOThreads * 100.0R).ToString("0.00%"))
                Console.WriteLine()
            ElseIf consoleLine.StartsWith("list", True, Nothing) Then
                Console.WriteLine()
                DisplayConnectionList()
            ElseIf consoleLine.StartsWith("version", True, Nothing) Then
                Console.WriteLine()
                Console.WriteLine(MonitorInformation)
            ElseIf consoleLine.StartsWith("help", True, Nothing) OrElse consoleLine = "?" Then
                Console.WriteLine()
                DisplayCommandList()
            ElseIf consoleLine.StartsWith("exit", True, Nothing) Then
                Exit Do
            Else
                Console.WriteLine()
                Console.Write("Command unrecognized.  ")
                DisplayCommandList()
            End If
        Loop

        ' Attempt an orderly shutdown...
        For Each receiver In m_measurementReceivers.Values
            receiver.Disconnect()
        Next

        End

    End Sub

    Private Sub InitializeConfiguration(ByVal initializationFunction As InitializationFunctionSignature)

        Dim connection As SqlConnection

        Try
            connection = New SqlConnection(StringSetting("PMUDatabase"))
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
            DisplayStatusMessage("Failure during initialization: " & ex.Message)
        Finally
            If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then connection.Close()
            DisplayStatusMessage("PMU database connection closed.")
        End Try

    End Sub

    Private Sub InitializeSystem(ByVal connection As SqlConnection)

        ' If calculated measurements are already defined (i.e., we are "re-initializing" code), we need to dispose
        ' existing calculation instances such that all items can be shut-down in an orderly fashion
        If m_calculatedMeasurements IsNot Nothing Then
            For x As Integer = 0 To m_calculatedMeasurements.Length - 1
                m_calculatedMeasurements(x).Dispose()
            Next
        End If

        ' Define all of the calculated measurements
        m_calculatedMeasurements = DefineCalculatedMeasurements(connection)

        ' If the phasor measurement receivers are already defined we must be reloading - so we attempt
        ' an orderly shutdown
        If m_measurementReceivers IsNot Nothing Then
            For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                receiver.Disconnect()
            Next
        End If

        ' Load the phasor measurement receivers (one per each established archive)
        m_measurementReceivers = LoadMeasurementReceivers(connection, IntegerSetting("PMUStatusInterval"), IntegerSetting("DataLossInterval"), m_calculatedMeasurements)

    End Sub

    Private Sub ReinitializeReceivers(ByVal connection As SqlConnection)

        ' This reloads active data nodes and reconnects
        For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
            receiver.Initialize(connection)
        Next

    End Sub

    Private Function LoadMeasurementReceivers(ByVal connection As SqlConnection, ByVal pmuStatusInterval As Integer, ByVal dataLossInterval As Integer, ByVal calculatedMeasurements As ICalculatedMeasurementAdapter()) As Dictionary(Of String, PhasorMeasurementReceiver)

        Dim measurementReceivers As New Dictionary(Of String, PhasorMeasurementReceiver)
        Dim measurementReceiver As PhasorMeasurementReceiver
        Dim externalAssemblyName As String
        Dim externalAssembly As Assembly
        Dim historianAdapter As IHistorianAdapter
        Dim connectionString As String = StringSetting("PMUDatabase")
        Dim archiveSource As String

        With RetrieveData("SELECT * FROM MeasurementArchives WHERE Enabled != 0", connection)
            For x As Integer = 0 To .Rows.Count - 1
                Try
                    archiveSource = .Rows(x)("ArchiveSource").ToString()
                    externalAssemblyName = .Rows(x)("AssemblyName").ToString()

                    ' Load the external assembly for the historian adapter
                    externalAssembly = Assembly.LoadFrom(externalAssemblyName)

                    ' Create a new instance of the historian adpater
                    historianAdapter = Activator.CreateInstance(externalAssembly.GetType(.Rows(x)("TypeName").ToString()))
                    historianAdapter.Initialize(.Rows(x)("ConnectionString").ToString())

                    measurementReceiver = New PhasorMeasurementReceiver( _
                        historianAdapter, _
                        archiveSource, _
                        pmuStatusInterval, _
                        connectionString, _
                        dataLossInterval, _
                        calculatedMeasurements)

                    AddHandler measurementReceiver.StatusMessage, AddressOf DisplayStatusMessage
                    If Convert.ToBoolean(.Rows(x)("InitializeOnStartup")) Then measurementReceiver.Initialize(connection)

                    measurementReceivers.Add(archiveSource, measurementReceiver)
                Catch ex As Exception
                    DisplayStatusMessage("Failed to load measurement receiver for archive """ & archiveSource & """ from assembly """ & externalAssemblyName & """ due to exception: " & ex.Message)
                End Try
            Next
        End With

        Return measurementReceivers

    End Function

    Private Function DefineCalculatedMeasurements(ByVal connection As SqlConnection) As ICalculatedMeasurementAdapter()

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
        With RetrieveData("SELECT * FROM CalculatedMeasurements WHERE Enabled != 0", connection)
            For x As Integer = 0 To .Rows.Count - 1
                Try
                    ' Load the external assembly
                    calculatedMeasurementName = .Rows(x)("Name").ToString()
                    externalAssemblyName = .Rows(x)("AssemblyName").ToString()
                    externalAssembly = Assembly.LoadFrom(externalAssemblyName)

                    ' Query the output measurements
                    outputMeasurementsSql = .Rows(x)("OutputMeasurementsSql").ToString()
                    outputMeasurements = New List(Of IMeasurement)

                    ' Calculated measurements have the option of internally defining the output measurements
                    If Not String.IsNullOrEmpty(outputMeasurementsSql) Then
                        Try
                            With RetrieveData(outputMeasurementsSql, connection)
                                For y As Integer = 0 To .Rows.Count - 1
                                    With .Rows(y)
                                        outputMeasurements.Add( _
                                            New Measurement( _
                                                Convert.ToInt32(.Item("MeasurementID")), _
                                                .Item("ArchiveSource").ToString(), _
                                                Double.NaN, _
                                                Convert.ToDouble(.Item("Adder")), _
                                                Convert.ToDouble(.Item("Multiplier"))))
                                    End With
                                Next
                            End With
                        Catch ex As Exception
                            DisplayStatusMessage("Failed to load output measurement for """ & calculatedMeasurementName & """: " & ex.Message)
                        End Try
                    End If

                    ' Query the input measurement keys
                    inputMeasurementsSql = .Rows(x)("InputMeasurementsSql").ToString()
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
                            DisplayStatusMessage("Failed to load input measurements for """ & calculatedMeasurementName & """: " & ex.Message)
                        End Try
                    End If

                    calculatedMeasurementAdapter = Nothing

                    ' Load the specified type from the assembly
                    Try
                        ' Create a new instance of the adpater
                        calculatedMeasurementAdapter = Activator.CreateInstance(externalAssembly.GetType(.Rows(x)("TypeName").ToString()))
                    Catch ex As Exception
                        DisplayStatusMessage("Failed to load type """ & .Rows(x)("TypeName").ToString() & """ from assembly """ & externalAssemblyName & """ for """ & calculatedMeasurementName & """ due to exception: " & ex.Message)
                    End Try

                    If calculatedMeasurementAdapter IsNot Nothing Then
                        ' Intialize calculated measurement adapter
                        With .Rows(x)
                            calculatedMeasurementAdapter.Initialize( _
                                .Item("Name").ToString(), _
                                .Item("ConfigSection").ToString(), _
                                outputMeasurements.ToArray(), _
                                inputMeasurementKeys.ToArray(), _
                                Convert.ToInt32(.Item("MinimumInputMeasurements")), _
                                Convert.ToInt32(.Item("ExpectedFrameRate")), _
                                Convert.ToDouble(.Item("LagTime")), _
                                Convert.ToDouble(.Item("LeadTime")))
                        End With

                        With calculatedMeasurementAdapter
                            ' Bubble calculation module status messages out to local update status function
                            AddHandler .StatusMessage, AddressOf DisplayStatusMessage

                            ' Bubble newly calculated measurement out to functions that need the real-time data
                            AddHandler .NewCalculatedMeasurements, AddressOf NewCalculatedMeasurements

                            ' Bubble calculation exceptions out to procedure that can handle these exceptions
                            AddHandler .CalculationException, AddressOf CalculationException
                        End With

                        ' Add new adapter to the list
                        calculatedMeasurementAdapters.Add(calculatedMeasurementAdapter)

                        DisplayStatusMessage("Loaded calculated measurement """ & calculatedMeasurementName & """ from assembly """ & externalAssemblyName & """")
                    End If
                Catch ex As Exception
                    DisplayStatusMessage("Failed to load calculated measurement """ & calculatedMeasurementName & """ from assembly """ & externalAssemblyName & """ due to exception: " & ex.Message)
                End Try
            Next
        End With

        Return calculatedMeasurementAdapters.ToArray()

    End Function

    Private Sub NewCalculatedMeasurements(ByVal measurements As IList(Of IMeasurement))

        If measurements IsNot Nothing Then
            ' Provide new calculated measurements "directly" to all calculated measurement modules
            ' such that calculated measurements can be based on other calculated measurements
            If m_calculatedMeasurements IsNot Nothing Then
                For x As Integer = 0 To m_calculatedMeasurements.Length - 1
                    m_calculatedMeasurements(x).QueueMeasurementsForCalculation(measurements)
                Next
            End If

            ' Queue new measurements for archival
            For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                receiver.QueueMeasurementsForArchival(measurements)
            Next

            ' TODO: Provide real-time calculated measurements outside of receiver as needed...
            ' In an "integrated" system, this is where you would provide calculated measurements
            ' as part of the real-time stream (such as in Analog points in C37.118)
        End If

    End Sub

    Private Sub CalculationException(ByVal source As String, ByVal ex As Exception)

        DisplayStatusMessage("ERROR: """ & source & """ threw an exception: " & ex.Message)

    End Sub

    Private Function TryGetMapper(ByVal consoleLine As String, ByRef mapper As PhasorMeasurementMapper) As Boolean

        Try
            Dim pmuID As String = RemoveDuplicateWhiteSpace(consoleLine).Split(" "c)(1).ToUpper()
            Dim foundMapper As Boolean

            For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
                If receiver.Mappers.TryGetValue(pmuID, mapper) Then
                    foundMapper = True
                    Exit For
                End If
            Next

            If Not foundMapper Then Console.WriteLine("Failed to find a PMU or PDC in the connection list named """ & pmuID & """, type ""List"" to see avaialable list.")

            Return foundMapper
        Catch ex As Exception
            Console.WriteLine("Failed to lookup specified mapper due to exception: " & ex.Message)
            Return False
        End Try

    End Function

    Private Function ParseDeviceCommand(ByVal consoleLine As String) As TVA.Phasors.DeviceCommand

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

    Private Sub DisplayConnectionList()

        For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
            Console.WriteLine("Phasor Measurement Retriever for Archive """ & receiver.HistorianName & """")
            Console.WriteLine(">> PMU/PDC Connection List (" & receiver.Mappers.Count & " Total)")
            Console.WriteLine()

            Console.WriteLine("  Last Data Report Time:   PDC/PMU [PMU list]:")
            Console.WriteLine("  ------------------------ ----------------------------------------------------")
            '                    01-JAN-2006 12:12:24.000 SourceName [Pmu0, Pmu1, Pmu2, Pmu3, Pmu4]
            '                    >> No data frame has been parsed for SourceName - 00000 bytes received"

            For Each mapper As PhasorMeasurementMapper In receiver.Mappers.Values
                With mapper
                    Console.Write("  ")
                    If .LastReportTime > 0 Then
                        Console.Write((New DateTime(mapper.LastReportTime)).ToString("dd-MMM-yyyy HH:mm:ss.fff"))
                        Console.Write(" "c)
                        Console.WriteLine(mapper.Name)
                    Else
                        Console.WriteLine(">> No data frame has been parsed for " & mapper.Name & " - " & .TotalBytesReceived & " bytes received")
                    End If
                End With
            Next

            Console.WriteLine()
        Next

    End Sub

    Private Sub DisplayCommandList()

        Console.WriteLine("Possible commands:")
        Console.WriteLine()
        Console.WriteLine("  ""Connect PmuID""                 - Restarts PMU connection cycle")
        Console.WriteLine("  ""Disconnect PmuID""              - Terminates PMU connection")
        Console.WriteLine("  ""SendCommand PmuID DisableData"" - Turns off real-time data")
        Console.WriteLine("  ""SendCommand PmuID EnableData""  - Turns on real-time data")
        Console.WriteLine("  ""SendCommand PmuID GetConfig""   - Requests configuration frame")
        Console.WriteLine("  ""Reload""                        - Reloads the entire service process")
        Console.WriteLine("  ""ReconnectAll""                  - Reconnects archives and phasor devices")
        Console.WriteLine("  ""GC""                            - Force .NET Garbage Collection")
        Console.WriteLine("  ""Status""                        - Returns current service status")
        Console.WriteLine("  ""List""                          - Displays loaded PMU/PDC connections")
        Console.WriteLine("  ""Version""                       - Displays service version information")
        Console.WriteLine("  ""Help""                          - Displays this help information")
        Console.WriteLine("  ""Exit""                          - Exits this console monitor")
        Console.WriteLine()

    End Sub

    Private Sub ForceGarbageCollection()

        DisplayStatusMessage("Forcing garbage collection and waiting for pending finalizers...")

        GC.Collect()
        GC.WaitForPendingFinalizers()

        DisplayStatusMessage("Garbage collection complete.")

    End Sub

    ' Display status messages bubbled up from phasor measurement receiver and its internal components
    Private Sub DisplayStatusMessage(ByVal status As String)

        Dim displayMessage As Boolean

        ' When errors happen with data being processed at 30 samples per second you can get a hefty volume
        ' of errors very quickly, so to keep from flooding the message queue - we'll only send a handful
        ' of messages every couple of seconds
        If TicksToSeconds(DateTime.Now.Ticks - m_lastDisplayedMessageTime) < m_messageDisplayTimepan Then
            displayMessage = (m_displayedMessageCount < m_maximumMessagesToDisplay)
            m_displayedMessageCount += 1
        Else
            If m_displayedMessageCount > m_maximumMessagesToDisplay Then
                Console.WriteLine("WARNING: " & (m_displayedMessageCount - m_maximumMessagesToDisplay) & " error messages discarded to avoid flooding message queue...")
                Console.WriteLine()
            End If
            displayMessage = True
            m_displayedMessageCount = 0
            m_lastDisplayedMessageTime = DateTime.Now.Ticks
        End If

        If displayMessage Then
            Console.WriteLine(status)
            Console.WriteLine()
        End If

        If m_systemLogEnabled Then
            If m_systemLogFile Is Nothing Then m_systemLogFile = New LogFile()
            m_systemLogFile.Name = "SystemLog.txt"
            m_systemLogFile.FileFullOperation = LogFileFullOperation.Rollover
            m_systemLogFile.Open()
            m_systemLogFile.WriteTimestampedLine(status)
        End If

    End Sub

End Module
