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
Imports Tva.Common
Imports Tva.Assembly
Imports Tva.IO.FilePath
Imports Tva.Configuration.Common
Imports Tva.Text.Common
Imports Tva.Data.Common
Imports Tva.Measurements
Imports InterfaceAdapters
Imports System.Reflection

Module MainModule

    Private Delegate Sub InitializationFunctionSignature(ByVal connection As SqlConnection)

    Private m_measurementReceivers As Dictionary(Of String, PhasorMeasurementReceiver)
    Private m_calculatedMeasurements As ICalculatedMeasurementAdapter()

    Public Sub Main()

        Dim consoleLine As String
        Dim receiver As PhasorMeasurementReceiver
        Dim mapper As PhasorMeasurementMapper = Nothing

        Console.WriteLine(MonitorInformation)

        ' Make sure service settings exist
        Settings.Add("PMUDatabase", "Data Source=RGOCSQLD;Initial Catalog=PMU_SDS;Integrated Security=False;user ID=ESOPublic;pwd=4all2see", "PMU metaData database connect string", True)
        Settings.Add("PMUStatusInterval", "5", "Number of seconds of deviation from UTC time (according to local clock) that last PMU reporting time is allowed before considering it offline")
        SaveSettings()

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
            ElseIf consoleLine.StartsWith("reload", True, Nothing) Then
                Console.WriteLine()
                InitializeConfiguration(AddressOf ReinitializeReceivers)
            ElseIf consoleLine.StartsWith("status", True, Nothing) Then
                Console.WriteLine()
                For Each receiver In m_measurementReceivers.Values
                    Console.WriteLine(receiver.Status)
                Next
                For x As Integer = 0 To m_calculatedMeasurements.Length - 1
                    Console.WriteLine(m_calculatedMeasurements(x).Status)
                Next
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
            receiver.DisconnectAll()
        Next

        End

    End Sub

    Private Sub InitializeConfiguration(ByVal initializationFunction As InitializationFunctionSignature)

        Dim connection As SqlConnection

        Try
            connection = New SqlConnection(StringSetting("PMUDatabase"))

            connection.Open()

            DisplayStatusMessage("PMU database connection opened...")

            ' Call user initialization function
            initializationFunction(connection)
        Catch ex As Exception
            DisplayStatusMessage("Failure during initialization: " & ex.Message)
        Finally
            If connection IsNot Nothing AndAlso connection.State = ConnectionState.Open Then connection.Close()
            DisplayStatusMessage("PMU database connection close.")
        End Try

    End Sub

    Private Sub InitializeSystem(ByVal connection As SqlConnection)

        ' Define all of the calculated measurements
        m_calculatedMeasurements = DefineCalculatedMeasurements(connection)

        ' Load the phasor measurement receivers (one per each established archive)
        m_measurementReceivers = LoadMeasurementReceivers(connection, IntegerSetting("PMUStatusInterval"), m_calculatedMeasurements)

    End Sub

    Private Sub ReinitializeReceivers(ByVal connection As SqlConnection)

        For Each receiver As PhasorMeasurementReceiver In m_measurementReceivers.Values
            receiver.Initialize(connection)
        Next

    End Sub

    Private Function LoadMeasurementReceivers(ByVal connection As SqlConnection, ByVal pmuStatusInterval As Integer, ByVal calculatedMeasurements As ICalculatedMeasurementAdapter()) As Dictionary(Of String, PhasorMeasurementReceiver)

        Dim measurementReceivers As New Dictionary(Of String, PhasorMeasurementReceiver)
        Dim measurementReceiver As PhasorMeasurementReceiver
        Dim connectionString As String = StringSetting("PMUDatabase")
        Dim archiveSource As String

        With RetrieveData("SELECT * FROM MeasurementArchives WHERE Enabled != 0", connection)
            For x As Integer = 0 To .Rows.Count - 1
                Try
                    archiveSource = .Rows(x)("ArchiveSource").ToString()

                    measurementReceiver = New PhasorMeasurementReceiver( _
                        archiveSource, _
                        .Rows(x)("ArchiveIP").ToString(), _
                        pmuStatusInterval, _
                        connectionString, _
                        calculatedMeasurements)

                    AddHandler measurementReceiver.StatusMessage, AddressOf DisplayStatusMessage
                    If Convert.ToBoolean(.Rows(x)("InitializeOnStartup")) Then measurementReceiver.Initialize(connection)

                    measurementReceivers.Add(archiveSource, measurementReceiver)
                Catch ex As Exception
                    DisplayStatusMessage("Failed to load measurement receiver for archive """ & archiveSource & """ due to exception: " & ex.Message)
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
        Dim adapterType As Type
        Dim outputMeasurement As Measurement
        Dim inputMeasurements As List(Of Measurement)

        ' CalculatedMeasurements Fields:
        '   ID                          AutoInc
        '   Name                        String
        '   TypeName                    String
        '   AssemblyName                String
        '   OuputMeasurementSql         String      Expects one row, with four fields named "MeasurementID", "ArchiveSource", "Adder" and "Multipler"
        '   InputMeasurementsSql        String      Expects one or more rows, with four fields named "MeasurementID", "ArchiveSource", "Adder" and "Multipler"
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

                    ' Query ouput measurement
                    With RetrieveRow(.Rows(x)("OuputMeasurementSql").ToString(), connection)
                        outputMeasurement = New Measurement( _
                            Convert.ToInt32(.Item("MeasurementID")), _
                            .Item("ArchiveSource").ToString(), _
                            Double.NaN, _
                            Convert.ToDouble(.Item("Adder")), _
                            Convert.ToDouble(.Item("Multiplier")))
                    End With

                    ' Query input measurements
                    inputMeasurements = New List(Of Measurement)

                    With RetrieveData(.Rows(x)("InputMeasurementsSql").ToString(), connection)
                        For y As Integer = 0 To .Rows.Count - 1
                            With .Rows(y)
                                inputMeasurements.Add( _
                                    New Measurement( _
                                        Convert.ToInt32(.Item("MeasurementID")), _
                                        .Item("ArchiveSource").ToString(), _
                                        Double.NaN, _
                                        Convert.ToDouble(.Item("Adder")), _
                                        Convert.ToDouble(.Item("Multiplier"))))
                            End With
                        Next
                    End With

                    ' Load the specified type from the assembly
                    adapterType = externalAssembly.GetType(.Rows(x)("TypeName").ToString())

                    ' Create a new instance of the adpater
                    calculatedMeasurementAdapter = Activator.CreateInstance(adapterType)

                    ' Intialize calculated measurement adapter
                    With .Rows(x)
                        calculatedMeasurementAdapter.Initialize( _
                            outputMeasurement, _
                            inputMeasurements.ToArray(), _
                            Convert.ToInt32(.Item("MinimumInputMeasurements")), _
                            Convert.ToInt32(.Item("ExpectedFrameRate")), _
                            Convert.ToDouble(.Item("LagTime")), _
                            Convert.ToDouble(.Item("LeadTime")))
                    End With

                    With calculatedMeasurementAdapter
                        ' Bubble calculation module status messages out to local update status function
                        AddHandler .StatusMessage, AddressOf DisplayStatusMessage

                        ' Bubble newly calculated measurement out to functions that need the real-time data
                        AddHandler .NewCalculatedMeasurement, AddressOf NewCalculatedMeasurement
                    End With

                    ' Add new adapter to the list
                    calculatedMeasurementAdapters.Add(calculatedMeasurementAdapter)

                    DisplayStatusMessage("Loaded calculated measurement """ & calculatedMeasurementName & """ from assembly """ & externalAssemblyName & """")
                Catch ex As Exception
                    DisplayStatusMessage("Failed to load calculated measurement """ & calculatedMeasurementName & """ from assembly """ & externalAssemblyName & """ due to exception: " & ex.Message)
                End Try
            Next
        End With

        connection.Close()

        Return calculatedMeasurementAdapters.ToArray()

    End Function

    Private Sub NewCalculatedMeasurement(ByVal measurement As IMeasurement)

        Dim measurementReceiver As PhasorMeasurementReceiver

        ' Make sure new calculated measurement gets sent to correct archive...
        If m_measurementReceivers.TryGetValue(measurement.Source, measurementReceiver) Then
            measurementReceiver.QueueMeasurementForArchival(measurement)
        End If

        ' TODO: Provide real-time calculated measurements outside of receiver as needed...

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
            Console.WriteLine("Phasor Measurement Retriever for Archive """ & receiver.ArchiverName & """")
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
        Console.WriteLine("    ""Connect PmuID""        - Restarts PMU connection cycle")
        Console.WriteLine("    ""Disconnect PmuID""     - Terminates PMU connection")
        Console.WriteLine("    ""Reload""               - Reloads the entire service process")
        Console.WriteLine("    ""Status""               - Returns current service status")
        Console.WriteLine("    ""List""                 - Displays loaded PMU/PDC connections")
        Console.WriteLine("    ""Version""              - Displays service version information")
        Console.WriteLine("    ""Help""                 - Displays this help information")
        Console.WriteLine("    ""Exit""                 - Exits this console monitor")
        Console.WriteLine()

    End Sub

    ' Display status messages bubbled up from phasor measurement receiver and its internal components
    Private Sub DisplayStatusMessage(ByVal status As String)

        Console.WriteLine(status)
        Console.WriteLine()

    End Sub

End Module
