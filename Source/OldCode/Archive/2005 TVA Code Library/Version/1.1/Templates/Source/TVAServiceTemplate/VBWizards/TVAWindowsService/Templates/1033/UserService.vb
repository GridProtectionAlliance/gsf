'***********************************************************************
'  [!output PROJECT_NAME].vb - TVA Service Template
'  Copyright © [!output CURR_YEAR] - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: [!output DEV_NAME]
'      Office: [!output DEV_OFFICE]
'       Phone: [!output DEV_PHONE]
'       Email: [!output DEV_EMAIL]
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  [!output CURR_DATE] - [!output USER_NAME]
'       Initial version of source generated for new Windows service
'       project "[!output PROJECT_NAME]".
'
'***********************************************************************

Imports System.IO
Imports System.ServiceProcess
Imports TVA.Config.Common
Imports TVA.Shared.FilePath
Imports TVA.ESO.Ssam
Imports TVA.ESO.Ssam.SsamEntityTypeID
Imports TVA.ESO.Ssam.SsamEventTypeID
Imports TVA.Services

Public Class [!output PROJECT_ID]

    Inherits System.ServiceProcess.ServiceBase

    Private primaryProcess As PrimaryServiceProcess

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
        If Not StatusLog Is Nothing Then StatusLog.AddLogEntry(Status, LogStatusToEventLog, [Enum].GetName(GetType(System.Diagnostics.EventLogEntryType), EntryType))

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

        LogSSAMEvent(ssamSuccessEvent, ssamProcessEntity, Variables("Ssam.[!output PROJECT_ID]Heartbeat"), "[!output PROJECT_NAME] Heartbeat")

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

    ' Class auto-generated using TVA service template at [!output GEN_TIME]
    Public Sub New()
        MyBase.New()

        ' This call is required by the Component Designer.
        InitializeComponent()

        ' Make sure status log variables exist for this service
        Variables.Create("StatusLog.Template", ServiceHelper.AppPath & "StatusLog.template", VariableType.Text, "Define the status log template database", 1)
        Variables.Create("StatusLog.ConnectString", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & ServiceHelper.AppPath & "StatusLog.mdb", VariableType.Text, "Define the connection string for the local status log database", 2)
        Variables.Create("StatusLog.MaxEntries", 15000, VariableType.Int, "Define the maximum number of entries for the status log database", 3)
        Variables.Create("StatusLog.BackupsToKeep", 5, VariableType.Int, "Define the maximum number of status log database backups to keep", 4)
        Variables.Create("StatusLog.ExceptionThreshold", 10, VariableType.Int, "Maximum number of exceptions to tolerate before recreating the status log database", 5)
        Variables.Create("Ssam.[!output PROJECT_ID]Heartbeat", "PR_[!output CAP_PROJECT_ID]_HEARTBEAT", VariableType.Text, "SSAM Entity ID: SSAM [!output PROJECT_NAME] Heartbeat Process - Heartbeat sent every 1 minute")

        CreateNewStatusLog()
        StatusLog.ConnectString = Variables("StatusLog.ConnectString")
        StatusLog.MaxLogEntries = Variables("StatusLog.MaxEntries")

        ' Create an instance of the primary service process class
        primaryProcess = New PrimaryServiceProcess(Me)

        ' Register the service components
        With ServiceHelper.Components
            .Add(StatusLog)
            .Add(SsamLogger)
            .Add(primaryProcess)
        End With

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
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New [!output PROJECT_ID]}

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
        Me.SecureServer.PrivateKey = "[!output GUID_HOSTAUTHKEY]"
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
        '[!output PROJECT_ID]
        '
        Me.CanPauseAndContinue = True
        Me.CanShutdown = True
        Me.ServiceName = "[!output PROJECT_ID]"
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
    'Me.SecureServer.PrivateKey = "[!output GUID_HOSTAUTHKEY]"
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
    ''[!output PROJECT_ID]
    ''
    'Me.CanPauseAndContinue = True
    'Me.CanShutdown = True
    'Me.ServiceName = "[!output PROJECT_ID]"
    'CType(Me.ServiceHelper, System.ComponentModel.ISupportInitialize).EndInit()
    'CType(Me.SsamLogger, System.ComponentModel.ISupportInitialize).EndInit()
    'CType(Me.HeartbeatTimer, System.ComponentModel.ISupportInitialize).EndInit()

#End Region

#Region " [!output PROJECT_NAME] Service Event Code "

    Private Sub ServiceHelper_OnStart(ByVal args() As String) Handles ServiceHelper.OnStart

        ' TODO: Add any code you want executed on service startup here...

    End Sub

    Private Sub ServiceHelper_OnStop() Handles ServiceHelper.OnStop

        ' TODO: Add any code you want executed on service shutdown here...

    End Sub

    Private Sub ServiceHelper_ExecuteServiceProcess(ByVal ProcessSchedule As TVA.Services.ScheduleType, ByVal ScheduledTime As System.TimeSpan, ByVal Client As TVA.Remoting.LocalClient, ByVal UserData As Object) Handles ServiceHelper.ExecuteServiceProcess

        ' Execute primary service process
        primaryProcess.ExecuteProcess(UserData)

    End Sub

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
