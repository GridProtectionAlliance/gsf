'***********************************************************************
'  DatAWare PDC.vb - TVA Service Template
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/5/2004 - James R Carroll
'       Initial version of source generated for new Windows service
'       project "DatAWare PDC".
'
'***********************************************************************

Imports System.IO
Imports System.Net
Imports System.ServiceProcess
Imports TVA.Config.Common
Imports TVA.Shared.Crypto
Imports TVA.Shared.FilePath
Imports TVA.Database.Common
Imports TVA.ESO.Ssam
Imports TVA.ESO.Ssam.SsamEntityTypeID
Imports TVA.ESO.Ssam.SsamEventTypeID
Imports TVA.Services

Public Class DatAWarePDC

    Inherits System.ServiceProcess.ServiceBase

    Public WithEvents Concentrator As PDCstream.Concentrator
    Public EventQueue As DatAWare.EventQueue
    Public Listeners As DatAWare.Listener()
    Public Aggregator As DatAWare.Aggregator

    Friend Const PasswordKey As String = "B1864405-59C0-4157-AB38-0417AFDBD395"

#Region " Common TVA Service Code "

    Protected Overrides Sub OnStart(ByVal args() As String)

        StatusLog.AddLogEntry(ServiceName & " started", True, "ServiceEvent")
        ServiceHelper.OnStartHandler(args)

    End Sub

    Protected Overrides Sub OnContinue()

        StatusLog.AddLogEntry(ServiceName & " resumed", True, "ServiceEvent")
        ServiceHelper.OnContinueHandler()

    End Sub

    Protected Overrides Sub OnPause()

        ServiceHelper.OnPauseHandler()
        StatusLog.AddLogEntry(ServiceName & " paused", True, "ServiceEvent")

    End Sub

    Protected Overrides Sub OnShutdown()

        StatusLog.AddLogEntry(ServiceName & " received shutdown request", True, "ServiceEvent")
        ServiceHelper.OnShutdownHandler()

    End Sub

    Protected Overrides Sub OnStop()

        ServiceHelper.OnStopHandler()
        StatusLog.AddLogEntry(ServiceName & " stopped", True, "ServiceEvent")

    End Sub

    Public Sub UpdateStatus(ByVal Status As String, Optional ByVal LogStatusToEventLog As Boolean = False, Optional ByVal EntryType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information)

        ' We log all status entries locally for posterity
        If Not StatusLog Is Nothing Then StatusLog.AddLogEntry(SqlEncode(Status), LogStatusToEventLog, [Enum].GetName(GetType(System.Diagnostics.EventLogEntryType), EntryType))

        ' Post status messages to any remote clients
        ServiceHelper.SendServiceMessage(Status & vbCrLf, LogStatusToEventLog, EntryType)

    End Sub

    Public Sub LogSSAMEvent(ByVal EventType As SsamEventTypeID, ByVal EntityType As SsamEntityTypeID, ByVal EntityID As String, Optional ByVal Message As String = Nothing, Optional ByVal Description As String = Nothing)

        Try
            ' We send the SSAM event to the buffered SSAM logger for asynchronous logging
            SsamLogger.LogEvent(EventType, EntityType, EntityID, Message, Description)
        Catch ex As Exception
            UpdateStatus("ERROR: Failed to log SSAM event for """ & EntityID & """ due to exception """ & ex.Message & """", False)
        End Try

    End Sub

    Private Sub HeartbeatTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles HeartbeatTimer.Elapsed

        LogSSAMEvent(ssamSuccessEvent, ssamProcessEntity, Variables("Ssam.DatAWarePDCHeartbeat"), "DatAWare PDC Heartbeat")

    End Sub

    Private Sub StatusLog_LogException(ByVal ex As System.Exception, ByVal TotalExceptions As Long) Handles StatusLog.LogException

        Try
            ServiceHelper.SendServiceMessage("NOTICE: Retrying log entry due to exception encountered while logging status information to """ & Variables("StatusLog.ConnectString") & """: " & ex.Message, False)

            ' If we've exceeded the threshold for logging errors, we'll recreate the log file
            If TotalExceptions > Variables("StatusLog.ExceptionThreshold") Then
                Dim originalState As Boolean = StatusLog.Enabled
                StatusLog.Enabled = False
                CreateNewStatusLog()
                StatusLog.TotalExceptions = 0
                StatusLog.Enabled = originalState
            End If
        Catch
        End Try

    End Sub

    Private Sub CreateNewStatusLog()

        Dim statusLog As String = GetStatusLogFileName()
        Dim filePath As String = ServiceHelper.AppPath & "Backups\"
        Dim fileName As String = NoFileExtension(statusLog)
        Dim fileExtension As String = JustFileExtension(statusLog)
        Dim oldBackup, newBackup As String

        ' We make backups of old status logs
        Directory.CreateDirectory(filePath)

        For x As Integer = Variables("StatusLog.BackupsToKeep") To 2 Step -1
            oldBackup = filePath & fileName & "-B" & x & fileExtension
            newBackup = filePath & fileName & "-B" & x - 1 & fileExtension
            If File.Exists(oldBackup) Then File.Delete(oldBackup)
            If File.Exists(newBackup) Then File.Move(newBackup, oldBackup)
        Next

        oldBackup = filePath & fileName & "-B1" & fileExtension
        If File.Exists(oldBackup) Then File.Delete(oldBackup)

        If File.Exists(statusLog) Then WaitForWriteLock(statusLog, 30)

        If File.Exists(Variables("StatusLog.Template")) Then
            If File.Exists(statusLog) Then File.Move(statusLog, oldBackup)
            File.Copy(Variables("StatusLog.Template"), statusLog)
        Else
            ' Can't delete existing status log if we can't find a template!
            If File.Exists(statusLog) Then File.Copy(statusLog, oldBackup)
            ServiceHelper.SendServiceMessage("WARNING: Could not create a new status log file: status log template """ & Variables("StatusLog.Template") & """ does not exist.", True, EventLogEntryType.Warning)
        End If

    End Sub

    Private Function GetStatusLogFileName() As String

        Dim statusLog As String

        ' Attempt to parse the status log file name out of the status log connection string
        For Each attr As String In Variables("StatusLog.ConnectString").ToString().Split(";")
            Dim key() As String = attr.Split("=")
            If key.Length = 2 Then
                If StrComp(Trim(key(0)), "Data Source", CompareMethod.Text) = 0 Then
                    statusLog = Trim(key(1))
                    Exit For
                End If
            End If
        Next

        ' Create a default file name if one wasn't found in connect string
        If Len(statusLog) = 0 Then statusLog = ServiceHelper.AppPath & "StatusLog.mdb"

        ' Make sure log file name includes a path
        If StrComp(statusLog, JustFileName(statusLog), CompareMethod.Text) = 0 Then
            statusLog = ServiceHelper.AppPath & statusLog
        End If

        Return statusLog

    End Function

#End Region

#Region " Component Designer generated code "

    ' Class auto-generated using TVA service template at Fri Nov 5 09:43:23 EST 2004
    Public Sub New()
        MyBase.New()

        ' This call is required by the Component Designer.
        InitializeComponent()

        ' Make sure status log variables exist for this service
        Variables.Create("DatAWare.PDC.ConfigFile", ServiceHelper.AppPath & "TVA_PDC.ini", VariableType.Text, "PDC Configuration File used by the DatAWare Phasor Data Concentrator")
        Variables.Create("DatAWare.PDC.LagTime", 3.0#, VariableType.Float, "Maximum time deviation, in seconds, tolerated before data packet is published (i.e., how long to wait for data before broadcast)")
        Variables.Create("DatAWare.PDC.IntervalAdjustment", 3.0#, VariableType.Float, "Time span, in milliseconds, used to adjust broadcast timer interval as needed to maintain the sample rate")
        Variables.Create("DatAWare.PDC.HighSampleCount", 6, VariableType.Int, "High warning limit for the concentrator sample count, warning raised when unpublished sample count exceeds this value.  Too many queued samples means concentrator is falling behind.")
        Variables.Create("DatAWare.PDC.BroadcastPoints.Total", 1, VariableType.Int, "Total number of PDCstream UDP broadcast points")
        Variables.Create("DatAWare.PDC.BroadcastPoint0.IP", "152.85.255.255", VariableType.Text, "IP used for this broadcast point - can be single IP or you can use 255.255 for sub-net suffix for broadcast (e.g., 152.85.255.255)")
        Variables.Create("DatAWare.PDC.BroadcastPoint0.Port", 3060, VariableType.Int, "Port used for this broadcast point")
        Variables.Create("DatAWare.PDC.AggregatedArchive.Server", "pmudw", VariableType.Text, "DatAWare server name used to archive permanent aggregated PMU data")
        Variables.Create("DatAWare.PDC.AggregatedArchive.PlantCode", "T2", VariableType.Text, "Plant code of DatAWare server used to archive permanent aggregated PMU data")
        Variables.Create("DatAWare.PDC.AggregatedArchive.TimeZone", "Central Standard Time", VariableType.Text, "Timezone of DatAWare server used to archive permanent aggregated PMU data")
        Variables.Create("DatAWare.Listeners.Total", 1, VariableType.Int, "Total number of DatAWare listeners to establish")
        Variables.Create("DatAWare.Listener0.Port", 8500, VariableType.Int, "Port to establish this listener on")
        Variables.Create("DatAWare.Listener0.Server", "152.85.70.76", VariableType.Text, "DatAWare server name to associate this listener with")
        Variables.Create("DatAWare.Listener0.PlantCode", "PM", VariableType.Text, "Plant code of DatAWare server this listener is associated with")
        Variables.Create("DatAWare.Listener0.TimeZone", "Eastern Standard Time", VariableType.Text, "Timezone of DatAWare server this listener is associated with")
        Variables.Create("DatAWare.Listener0.UserName", "DatAWarePDC", VariableType.Text, "Username used to connect to DatAWare server this listener is associated with")
        Variables.Create("DatAWare.Listener0.Password", "4kqd4WHPevelrySArtKLlp4nV5ykh90Xe3EuJotBg1Y=", VariableType.Text, "Encrypted password used to connect to DatAWare server this listener is associated with - use GenPassword to create password")
        Variables.Create("StatusLog.Template", ServiceHelper.AppPath & "StatusLog.template", VariableType.Text, "Define the status log template database", 1)
        Variables.Create("StatusLog.ConnectString", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & ServiceHelper.AppPath & "StatusLog.mdb", VariableType.Text, "Define the connection string for the local status log database", 2)
        Variables.Create("StatusLog.MaxEntries", 15000, VariableType.Int, "Define the maximum number of entries for the status log database", 3)
        Variables.Create("StatusLog.BackupsToKeep", 5, VariableType.Int, "Define the maximum number of status log database backups to keep", 4)
        Variables.Create("StatusLog.ExceptionThreshold", 10, VariableType.Int, "Maximum number of exceptions to tolerate before recreating the status log database", 5)
        Variables.Create("Ssam.DatAWarePDCHeartbeat", "PR_DATAWAREPDC_HEARTBEAT", VariableType.Text, "SSAM Entity ID: SSAM DatAWare PDC Heartbeat Process - Heartbeat sent every 1 minute")

        CreateNewStatusLog()
        StatusLog.ConnectString = Variables("StatusLog.ConnectString")
        StatusLog.MaxLogEntries = Variables("StatusLog.MaxEntries")

        ' If any variables have been added to the config file, go ahead and flush them to disk
        Variables.Save()

    End Sub

    'UserService overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' The main entry point for the process
    <MTAThread()> _
    Shared Sub Main()
        Dim ServicesToRun() As System.ServiceProcess.ServiceBase

        ' More than one NT Service may run within the same process. To add
        ' another service to this process, change the following line to
        ' create a second service object. For example,
        '
        '   ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}
        '
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New DatAWarePDC}

        System.ServiceProcess.ServiceBase.Run(ServicesToRun)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  
    ' Do not modify it using the code editor.
    Friend WithEvents ServiceHelper As TVA.Services.ServiceHelper
    Friend WithEvents SecureServer As TVA.Remoting.SecureServer
    Friend WithEvents StatusLog As TVA.Database.ActivityLogger
    Friend WithEvents SsamLogger As TVA.ESO.Ssam.SsamLogger
    Friend WithEvents HeartbeatTimer As System.Timers.Timer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim configurationAppSettings As System.Configuration.AppSettingsReader = New System.Configuration.AppSettingsReader
        Me.ServiceHelper = New TVA.Services.ServiceHelper
        Me.SecureServer = New TVA.Remoting.SecureServer
        Me.StatusLog = New TVA.Database.ActivityLogger
        Me.SsamLogger = New TVA.ESO.Ssam.SsamLogger
        Me.HeartbeatTimer = New System.Timers.Timer
        CType(Me.ServiceHelper, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.SsamLogger, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.HeartbeatTimer, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'ServiceHelper
        '
        Me.ServiceHelper.AboveNormalThreadPriority = CType(configurationAppSettings.GetValue("ServiceHelper.AboveNormalThreadPriority", GetType(System.Boolean)), Boolean)
        Me.ServiceHelper.AutoLaunchMonitorApplication = CType(configurationAppSettings.GetValue("ServiceHelper.AutoLaunchMonitorApplication", GetType(System.Boolean)), Boolean)
        Me.ServiceHelper.CommandHistoryLimit = CType(configurationAppSettings.GetValue("ServiceHelper.CommandHistoryLimit", GetType(System.Int32)), Integer)
        Me.ServiceHelper.MonitorApplication = CType(configurationAppSettings.GetValue("ServiceHelper.MonitorApplication", GetType(System.String)), String)
        Me.ServiceHelper.ParentService = Me
        Me.ServiceHelper.RemotingServer = Me.SecureServer
        Me.ServiceHelper.ShowClientConnects = CType(configurationAppSettings.GetValue("ServiceHelper.ShowClientConnects", GetType(System.Boolean)), Boolean)
        Me.ServiceHelper.ShowClientDisconnects = CType(configurationAppSettings.GetValue("ServiceHelper.ShowClientDisconnects", GetType(System.Boolean)), Boolean)
        '
        'SecureServer
        '
        Me.SecureServer.HostID = CType(configurationAppSettings.GetValue("SecureServer.HostID", GetType(System.String)), String)
        Me.SecureServer.MaximumClients = CType(configurationAppSettings.GetValue("SecureServer.MaximumClients", GetType(System.Int32)), Integer)
        Me.SecureServer.MaximumServers = CType(configurationAppSettings.GetValue("SecureServer.MaximumServers", GetType(System.Int32)), Integer)
        Me.SecureServer.NoActivityLimit = CType(configurationAppSettings.GetValue("SecureServer.NoActivityLimit", GetType(System.Int32)), Integer)
        Me.SecureServer.PrivateKey = "498BF815-2613-4BFD-82BB-138A1633D862"
        Me.SecureServer.QueueInterval = CType(configurationAppSettings.GetValue("SecureServer.QueueInterval", GetType(System.Int32)), Integer)
        Me.SecureServer.ResponseTimeout = CType(configurationAppSettings.GetValue("SecureServer.ResponseTimeout", GetType(System.Int32)), Integer)
        Me.SecureServer.ShutdownTimeout = CType(configurationAppSettings.GetValue("SecureServer.ShutdownTimeout", GetType(System.Int32)), Integer)
        Me.SecureServer.TCPPort = CType(configurationAppSettings.GetValue("SecureServer.TCPPort", GetType(System.Int32)), Integer)
        Me.SecureServer.URI = CType(configurationAppSettings.GetValue("SecureServer.URI", GetType(System.String)), String)
        '
        'StatusLog
        '
        Me.StatusLog.ConnectString = ""
        Me.StatusLog.CountSQL = "SELECT COUNT(*) AS Total FROM StatusLog"
        Me.StatusLog.DeleteSQL = "DELETE * FROM StatusLog WHERE ID IN (SELECT TOP 1 ID FROM StatusLog ORDER BY ID)"
        Me.StatusLog.InsertSQL = "INSERT INTO StatusLog(Status, Logged, EntryType) VALUES ('%Status%', %Logged%, '%" & _
        "EntryType%')"
        Me.StatusLog.LogName = "StatusLog"
        Me.StatusLog.StatusSQL = "SELECT TOP 5 Status & "" ["" & EntryType & ""] Logged at: "" & TimeStamp AS ServiceSt" & _
        "atus FROM StatusLog ORDER BY TimeStamp DESC"
        '
        'SsamLogger
        '
        Me.SsamLogger.Connection = TVA.ESO.Ssam.SsamDatabase.ssamProductionDB
        Me.SsamLogger.Enabled = CType(configurationAppSettings.GetValue("SsamLogger.Enabled", GetType(System.Boolean)), Boolean)
        Me.SsamLogger.EventLogInterval = CType(configurationAppSettings.GetValue("SsamLogger.EventLogInterval", GetType(System.Int32)), Integer)
        Me.SsamLogger.EventLogThreshold = CType(configurationAppSettings.GetValue("SsamLogger.EventLogThreshold", GetType(System.Int32)), Integer)
        '
        'HeartbeatTimer
        '
        Me.HeartbeatTimer.Enabled = True
        Me.HeartbeatTimer.Interval = CType(configurationAppSettings.GetValue("HeartbeatTimer.Interval", GetType(System.Double)), Double)
        '
        'DatAWarePDC
        '
        Me.CanPauseAndContinue = True
        Me.CanShutdown = True
        Me.ServiceName = "DatAWarePDC"
        CType(Me.ServiceHelper, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.SsamLogger, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.HeartbeatTimer, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

    ' Designer in VS.NET will lose the settings below sure as the world! So here they are, backed-up, in case you are so lucky ;)

    'Dim configurationAppSettings As System.Configuration.AppSettingsReader = New System.Configuration.AppSettingsReader
    'Me.ServiceHelper = New TVA.Services.ServiceHelper
    'Me.SecureServer = New TVA.Remoting.SecureServer
    'Me.StatusLog = New TVA.Database.ActivityLogger
    'Me.SsamLogger = New TVA.ESO.Ssam.SsamLogger
    'Me.HeartbeatTimer = New System.Timers.Timer
    'CType(Me.ServiceHelper, System.ComponentModel.ISupportInitialize).BeginInit()
    'CType(Me.SsamLogger, System.ComponentModel.ISupportInitialize).BeginInit()
    'CType(Me.HeartbeatTimer, System.ComponentModel.ISupportInitialize).BeginInit()
    ''
    ''ServiceHelper
    ''
    'Me.ServiceHelper.AboveNormalThreadPriority = CType(configurationAppSettings.GetValue("ServiceHelper.AboveNormalThreadPriority", GetType(System.Boolean)), Boolean)
    'Me.ServiceHelper.AutoLaunchMonitorApplication = CType(configurationAppSettings.GetValue("ServiceHelper.AutoLaunchMonitorApplication", GetType(System.Boolean)), Boolean)
    'Me.ServiceHelper.CommandHistoryLimit = CType(configurationAppSettings.GetValue("ServiceHelper.CommandHistoryLimit", GetType(System.Int32)), Integer)
    'Me.ServiceHelper.MonitorApplication = CType(configurationAppSettings.GetValue("ServiceHelper.MonitorApplication", GetType(System.String)), String)
    'Me.ServiceHelper.ParentService = Me
    'Me.ServiceHelper.RemotingServer = Me.SecureServer
    'Me.ServiceHelper.ShowClientConnects = CType(configurationAppSettings.GetValue("ServiceHelper.ShowClientConnects", GetType(System.Boolean)), Boolean)
    'Me.ServiceHelper.ShowClientDisconnects = CType(configurationAppSettings.GetValue("ServiceHelper.ShowClientDisconnects", GetType(System.Boolean)), Boolean)
    ''
    ''SecureServer
    ''
    'Me.SecureServer.HostID = CType(configurationAppSettings.GetValue("SecureServer.HostID", GetType(System.String)), String)
    'Me.SecureServer.MaximumClients = CType(configurationAppSettings.GetValue("SecureServer.MaximumClients", GetType(System.Int32)), Integer)
    'Me.SecureServer.MaximumServers = CType(configurationAppSettings.GetValue("SecureServer.MaximumServers", GetType(System.Int32)), Integer)
    'Me.SecureServer.NoActivityLimit = CType(configurationAppSettings.GetValue("SecureServer.NoActivityLimit", GetType(System.Int32)), Integer)
    'Me.SecureServer.PrivateKey = "498BF815-2613-4BFD-82BB-138A1633D862"
    'Me.SecureServer.QueueInterval = CType(configurationAppSettings.GetValue("SecureServer.QueueInterval", GetType(System.Int32)), Integer)
    'Me.SecureServer.ResponseTimeout = CType(configurationAppSettings.GetValue("SecureServer.ResponseTimeout", GetType(System.Int32)), Integer)
    'Me.SecureServer.ShutdownTimeout = CType(configurationAppSettings.GetValue("SecureServer.ShutdownTimeout", GetType(System.Int32)), Integer)
    'Me.SecureServer.TCPPort = CType(configurationAppSettings.GetValue("SecureServer.TCPPort", GetType(System.Int32)), Integer)
    'Me.SecureServer.URI = CType(configurationAppSettings.GetValue("SecureServer.URI", GetType(System.String)), String)
    ''
    ''StatusLog
    ''
    'Me.StatusLog.ConnectString = ""
    'Me.StatusLog.CountSQL = "SELECT COUNT(*) AS Total FROM StatusLog"
    'Me.StatusLog.DeleteSQL = "DELETE * FROM StatusLog WHERE ID IN (SELECT TOP 1 ID FROM StatusLog ORDER BY ID)"
    'Me.StatusLog.InsertSQL = "INSERT INTO StatusLog(Status, Logged, EntryType) VALUES ('%Status%', %Logged%, '%" & _
    '"EntryType%')"
    'Me.StatusLog.LogName = "StatusLog"
    'Me.StatusLog.StatusSQL = "SELECT TOP 5 Status & "" ["" & EntryType & ""] Logged at: "" & TimeStamp AS ServiceSt" & _
    '"atus FROM StatusLog ORDER BY TimeStamp DESC"
    ''
    ''SsamLogger
    ''
    'Me.SsamLogger.Connection = TVA.ESO.Ssam.SsamDatabase.ssamProductionDB
    'Me.SsamLogger.Enabled = CType(configurationAppSettings.GetValue("SsamLogger.Enabled", GetType(System.Boolean)), Boolean)
    'Me.SsamLogger.EventLogInterval = CType(configurationAppSettings.GetValue("SsamLogger.EventLogInterval", GetType(System.Int32)), Integer)
    'Me.SsamLogger.EventLogThreshold = CType(configurationAppSettings.GetValue("SsamLogger.EventLogThreshold", GetType(System.Int32)), Integer)
    ''
    ''HeartbeatTimer
    ''
    'Me.HeartbeatTimer.Enabled = True
    'Me.HeartbeatTimer.Interval = CType(configurationAppSettings.GetValue("HeartbeatTimer.Interval", GetType(System.Double)), Double)
    ''
    ''DatAWarePDC
    ''
    'Me.CanPauseAndContinue = True
    'Me.CanShutdown = True
    'Me.ServiceName = "DatAWarePDC"
    'CType(Me.ServiceHelper, System.ComponentModel.ISupportInitialize).EndInit()
    'CType(Me.SsamLogger, System.ComponentModel.ISupportInitialize).EndInit()
    'CType(Me.HeartbeatTimer, System.ComponentModel.ISupportInitialize).EndInit()

#End Region

#Region " DatAWare PDC Service Event Code "

    Private Sub ServiceHelper_OnStart(ByVal args() As String) Handles ServiceHelper.OnStart

        ' Define UDP broadcast end-points
        Dim broadcastIPs As IPEndPoint() = Array.CreateInstance(GetType(IPEndPoint), Variables("DatAWare.PDC.BroadcastPoints.Total"))

        For x As Integer = 0 To broadcastIPs.Length - 1
            broadcastIPs(x) = New IPEndPoint(IPAddress.Parse(Variables("DatAWare.PDC.BroadcastPoint" & x & ".IP")), Variables("DatAWare.PDC.BroadcastPoint" & x & ".Port"))
        Next

        ' Create an instance of the PDC concentrator
        Concentrator = New PDCstream.Concentrator(Me, Variables("DatAWare.PDC.ConfigFile"), Variables("DatAWare.PDC.LagTime"), Variables("DatAWare.PDC.IntervalAdjustment"), broadcastIPs)

        ' Create an instance of the DatAWare event queue
        EventQueue = New DatAWare.EventQueue(Me)

        ' Create a new PMU data aggregator
        Aggregator = New DatAWare.Aggregator(Me)

        ' Register these service components
        With ServiceHelper.Components
            .Add(StatusLog)
            .Add(SsamLogger)
            .Add(Concentrator)
            .Add(EventQueue)
            .Add(Aggregator)
        End With

        ' Create the DatAware network packet listeners
        Listeners = Array.CreateInstance(GetType(DatAWare.Listener), Variables("DatAWare.Listeners.Total"))

        For x As Integer = 0 To Listeners.Length - 1
            ' Define the listener on the specified port associated with the specified DatAWare server
            Listeners(x) = New DatAWare.Listener(Me, _
                Variables("DatAWare.Listener" & x & ".Port"), _
                Variables("DatAWare.Listener" & x & ".Server"), _
                Variables("DatAWare.Listener" & x & ".PlantCode"), _
                Variables("DatAWare.Listener" & x & ".TimeZone"))

            ' Register the listener component
            ServiceHelper.Components.Add(Listeners(x))

            ' We'll try three times to connect to DatAWare and retrieve points before giving up...
            For y As Integer = 1 To 3
                ' Load all the point definitions from the associated DatAWare server (this takes a second)
                Try
                    ' Note that this step is mission critical - we can't start broadcasting and archiving if we don't
                    ' get a point list from the DatAWare server...
                    EventQueue.DefinePointList(Listeners(x).Connection, Variables("DatAWare.Listener" & x & ".UserName"), _
                        Decrypt(Variables("DatAWare.Listener" & x & ".Password"), PasswordKey, EncryptLevel.Level4))

                    ' If load was successful, we'll exit retry loop
                    Exit For
                Catch ex As Exception
                    If y = 3 Then
                        ' Log this exception to the event log, the local status log and any remote clients
                        UpdateStatus("Failed to load point list from DatAWare server """ & Listeners(x).Connection.Server & " (" & _
                            Listeners(x).Connection.PlantCode & ")"" due to exception: " & ex.Message, True, EventLogEntryType.Error)
                        Exit Sub
                    End If
                End Try
            Next
        Next

        ' Start the DatAWare listeners...
        For x As Integer = 0 To Listeners.Length - 1
            Listeners(x).Start()
        Next

    End Sub

    Private Sub ServiceHelper_OnStop() Handles ServiceHelper.OnStop

        ' Stop the DatAWare listeners...
        For x As Integer = 0 To Listeners.Length - 1
            Listeners(x).Stop()
        Next

    End Sub

    Private Sub Concentrator_SamplePublished(ByVal sample As PDCstream.DataSample) Handles Concentrator.SamplePublished

        ' TODO: Once a sample is published, we should pass it along to the aggregator for aggregation and archival

        'With New Text.StringBuilder
        '    Dim row As PDCstream.DescriptorPacket

        '    .Append("Sample @ " & sample.Timestamp & " rows:" & vbCrLf)
        '    For x As Integer = 0 To sample.Rows.Length - 1
        '        .Append("  " & x.ToString.PadLeft(2, "0"c) & ": ")
        '        For y As Integer = 0 To sample.Rows(x).Cells.Length - 1
        '            If y > 0 Then .Append(", ")
        '            .Append(sample.Rows(x).Cells(y).FrequencyValue.ScaledFrequency.ToString("00.0000"))
        '        Next
        '        .Append(vbCrLf)
        '    Next

        '    UpdateStatus(.ToString)
        'End With

    End Sub

    Private Sub Concentrator_UnpublishedSamples(ByVal total As Integer) Handles Concentrator.UnpublishedSamples

        Static highSampleCount As Integer = Variables("DatAWare.PDC.HighSampleCount")
        Static lastWarning As Long
        Static warningState As Boolean

        ' You should never have very many unpublished samples queued up - each sample represents one second of data,
        ' so the total number of unpublished samples equals the number of seconds the PDC is behind real-time...
        If total > highSampleCount Then
            ' Don't send any warnings more than every 10 seconds
            If (DateTime.Now.Ticks - lastWarning) / 10000000L > 10 Then
                UpdateStatus("WARNING: There are " & total & " unpublished samples in the queue, real-time stream could be falling behind...")
                lastWarning = DateTime.Now.Ticks
            End If
            warningState = True
        Else
            If warningState Then
                UpdateStatus("INFO: Warning state terminated, there are now only " & total & " unpublished samples in the queue...")
            End If
            warningState = False
        End If

    End Sub

    'Private Sub ServiceHelper_ExecuteServiceProcess(ByVal ProcessSchedule As TVA.Services.ScheduleType, ByVal ScheduledTime As System.TimeSpan, ByVal Client As TVA.Remoting.LocalClient, ByVal UserData As Object) Handles ServiceHelper.ExecuteServiceProcess

    '    ' Execute primary service process
    '    'primaryProcess.ExecuteProcess(UserData)

    'End Sub

    'Private Sub ServiceHelper_ClientNotification(ByVal Client As TVA.Remoting.LocalClient, ByVal sender As Object, ByVal e As System.EventArgs) Handles ServiceHelper.ClientNotification

    '    ' You can handle any custom notifications coming in from remote clients here...
    '    If Not e Is Nothing Then
    '        If TypeOf e Is MyCustomEventArgs Then
    '            With DirectCast(e, MyCustomEventArgs)
    '                Select Case .Notification
    '                    Case CustomNotification.ReloadSettings
    '                        PrimaryServiceProcess.ReloadSettings()
    '                End Select
    '            End With
    '        End If
    '    End If

    'End Sub

#End Region

End Class
