'***********************************************************************
'  BPA PDC To DatAWare Loader.vb - TVA Service Template
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
'  10/1/2004 - James R Carroll
'       Initial version of source generated for new Windows service
'       project "BPA PDC To DatAWare Loader".
'
'***********************************************************************

Imports System.IO
Imports System.ServiceProcess
Imports TVA.Config.Common
Imports TVA.Shared.FilePath
Imports TVA.ESO.Ssam
Imports TVA.Services
Imports PDCMessages

Public Class BPAPDCToDatAWareLoader

    Inherits System.ServiceProcess.ServiceBase

    Friend converter As PDCToDatAWare.Converter
    Friend dataReader As Diagnostics.Process

#Region " Common TVA Service Code "

    Protected Overrides Sub OnStart(ByVal args() As String)

        ' Start PDC Data Reader external process on service startup...
        StartPDCDataReader()
        StatusLog.AddLogEntry(ServiceName & " started", True, "ServiceEvent")
        ServiceHelper.OnStartHandler(args)

    End Sub

    Protected Overrides Sub OnContinue()

        ' Restart PDC Data Reader external process on service resume...
        StartPDCDataReader()
        StatusLog.AddLogEntry(ServiceName & " resumed", True, "ServiceEvent")
        ServiceHelper.OnContinueHandler()

    End Sub

    Protected Overrides Sub OnPause()

        ' Stop external PDC Data Reader process on service pause...
        StopPDCDataReader()
        ServiceHelper.OnPauseHandler()
        StatusLog.AddLogEntry(ServiceName & " paused", True, "ServiceEvent")

    End Sub

    Protected Overrides Sub OnStop()

        ' Stop external PDC Data Reader process on service shutdown...
        StopPDCDataReader()
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

        LogSSAMEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamFlowEntity, Variables("Ssam.BPAPDCLoaderHeartbeat"), "Heartbeat...")

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

    ' Class auto-generated using TVA service template at Fri Oct 1 14:26:17 EDT 2004
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
        Variables.Create("Ssam.BPAPDCLoaderHeartbeat", "FL_BPAPDCLOADER_HEARTBEAT", VariableType.Text, "SSAM Entity ID: SSAM BPA PDC To DatAWare Loader Heartbeat Process - Heartbeat sent every 1 minute")
        Variables.Create("Ssam.BPAPDCLoaderPrimaryProcess", "FL_BPAPDCLOADER_PRIMARYPROCESS", VariableType.Text, "SSAM Entity ID: SSAM BPA PDC To DatAWare Loader Primary Service Process")
        Variables.Create("DatAWare.PointListFile", ServiceHelper.AppPath & "PD_DBASE.csv", VariableType.Text, "DatAWare Point List File")
        Variables.Create("DatAWare.Server", "socopb8", VariableType.Text, "DatAWare Server Name")
        Variables.Create("DatAWare.PlantCode", "B8", VariableType.Text, "DatAWare Plant Code")
        Variables.Create("DatAWare.TimeZone", "Central Standard Time", VariableType.Text, "DatAWare Server TimeZone")

        ' If any variables have been added to the config file, go ahead and flush them to disk
        Variables.Save()

        CreateNewStatusLog()
        StatusLog.ConnectString = Variables("StatusLog.ConnectString")
        StatusLog.MaxLogEntries = Variables("StatusLog.MaxEntries")

        ' Create an instance of the PDC to DatAWare conversion class
        converter = New PDCToDatAWare.Converter(Me, Variables("DatAWare.Server"), Variables("DatAWare.PlantCode"), Variables("DatAWare.TimeZone"))

        ' Register the service components
        With ServiceHelper.Components
            .Add(StatusLog)
            .Add(SsamLogger)
            .Add(converter)
        End With

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
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New BPAPDCToDatAWareLoader}

        System.ServiceProcess.ServiceBase.Run(ServicesToRun)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  
    ' Do not modify it using the code editor.
    Friend WithEvents ServiceHelper As TVA.Services.ServiceHelper
    Friend WithEvents BinaryServer As TVA.Remoting.BinaryServer
    Friend WithEvents StatusLog As TVA.Database.ActivityLogger
    Friend WithEvents SsamLogger As TVA.ESO.Ssam.SsamLogger
    Friend WithEvents HeartbeatTimer As System.Timers.Timer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(BPAPDCToDatAWareLoader))
        Dim configurationAppSettings As System.Configuration.AppSettingsReader = New System.Configuration.AppSettingsReader
        Me.ServiceHelper = New TVA.Services.ServiceHelper
        Me.BinaryServer = New TVA.Remoting.BinaryServer
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
        Me.ServiceHelper.RemotingServer = Me.BinaryServer
        Me.ServiceHelper.ShowClientConnects = CType(configurationAppSettings.GetValue("ServiceHelper.ShowClientConnects", GetType(System.Boolean)), Boolean)
        Me.ServiceHelper.ShowClientDisconnects = CType(configurationAppSettings.GetValue("ServiceHelper.ShowClientDisconnects", GetType(System.Boolean)), Boolean)
        '
        'BinaryServer
        '
        Me.BinaryServer.HostID = CType(configurationAppSettings.GetValue("BinaryServer.HostID", GetType(System.String)), String)
        Me.BinaryServer.MaximumClients = CType(configurationAppSettings.GetValue("BinaryServer.MaximumClients", GetType(System.Int32)), Integer)
        Me.BinaryServer.MaximumServers = CType(configurationAppSettings.GetValue("BinaryServer.MaximumServers", GetType(System.Int32)), Integer)
        Me.BinaryServer.QueueInterval = CType(configurationAppSettings.GetValue("BinaryServer.QueueInterval", GetType(System.Int32)), Integer)
        Me.BinaryServer.ResponseTimeout = CType(configurationAppSettings.GetValue("BinaryServer.ResponseTimeout", GetType(System.Int32)), Integer)
        Me.BinaryServer.ShutdownTimeout = CType(configurationAppSettings.GetValue("BinaryServer.ShutdownTimeout", GetType(System.Int32)), Integer)
        Me.BinaryServer.TCPPort = CType(configurationAppSettings.GetValue("BinaryServer.TCPPort", GetType(System.Int32)), Integer)
        Me.BinaryServer.URI = CType(configurationAppSettings.GetValue("BinaryServer.URI", GetType(System.String)), String)
        '
        'StatusLog
        '
        Me.StatusLog.ConnectString = ""
        Me.StatusLog.CountSql = "SELECT COUNT(*) AS Total FROM StatusLog"
        Me.StatusLog.DeleteSql = "DELETE * FROM StatusLog WHERE ID IN (SELECT TOP 1 ID FROM StatusLog ORDER BY ID)"
        Me.StatusLog.InsertSql = "INSERT INTO StatusLog(Status, Logged, EntryType) VALUES ('%Status%', %Logged%, '%" & _
        "EntryType%')"
        Me.StatusLog.LogName = "StatusLog"
        Me.StatusLog.StatusSql = "SELECT TOP 5 Status & "" ["" & EntryType & ""] Logged at: "" & TimeStamp AS ServiceSt" & _
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
        'BPAPDCToDatAWareLoader
        '
        Me.CanPauseAndContinue = True
        Me.CanShutdown = True
        Me.ServiceName = "BPAPDCToDatAWareLoader"
        CType(Me.ServiceHelper, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.SsamLogger, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.HeartbeatTimer, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

#End Region

#Region " BPA PDC To DatAWare Loader Service Event Code "

    Private Sub ServiceHelper_ClientNotification(ByVal Client As TVA.Remoting.LocalClient, ByVal sender As Object, ByVal e As System.EventArgs) Handles ServiceHelper.ClientNotification

        Static lastUpdate As Long

        ' Handle notifications from remote clients
        If Not e Is Nothing Then
            If TypeOf e Is PDCDataUnit Then
                ' Received a new PDC data unit from PDC Data Reader, so we add it to the queue...
                With DirectCast(e, PDCDataUnit)
                    ' We push data into the conversion queue as fast as we get it...
                    converter.QueueNewData(.AnalogData, .DigitalData)
                End With

                ' Every ten seconds or so we update the user on the processing status...
                If (DateTime.Now.Ticks - lastUpdate) \ 10000000L > 10 Then
                    UpdateStatus("Processed requests = " & converter.ProcessedDataUnits & ", queued requests = " & converter.QueuedDataUnits)
                    lastUpdate = DateTime.Now.Ticks
                End If
            ElseIf TypeOf e Is PDCDescriptorUnit Then
                ' Received a new PDC descriptor unit from PDC Data Reader, so we process it...
                With DirectCast(e, PDCDescriptorUnit)
                    ' Load in analog and digital point names so we can create a proper cross-reference to the same points defined in DatAWare...
                    converter.LoadDescriptorData(Variables("DatAWare.PointListFile"), .AnalogNames, .DigitalNames)
                End With

                UpdateStatus("Received PDC descriptor data packet - starting PMU data processing...")
            ElseIf TypeOf e Is PDCErrorUnit Then
                ' Received a new PDC error unit from PDC Data Reader, so we report it...
                With DirectCast(e, PDCErrorUnit)
                    UpdateStatus("PDC Data Reader Error: " & .Number & " (" & .SCode & ") from " & .Source & ": " & .Description)
                End With
            ElseIf TypeOf e Is ServiceMessageEventArgs Then
                ' Received a request to rebroadcast a message from one of the remote clients, so we send it...
                With DirectCast(e, ServiceMessageEventArgs)
                    UpdateStatus(.Message, .LogMessage)
                End With
            End If
        End If

    End Sub

    Private Sub StartPDCDataReader()

        ' Start PDC Data Reader external process...
        If Not dataReader Is Nothing Then StopPDCDataReader()
        dataReader = Process.Start(ServiceHelper.AppPath & "PDCDataReader.exe")

    End Sub

    Private Sub StopPDCDataReader()

        ' Stop external PDC Data Reader process...
        If Not dataReader Is Nothing Then
            Try
                If Not dataReader.HasExited Then dataReader.Kill()
            Catch
            End Try
        End If
        dataReader = Nothing

    End Sub

#End Region

End Class
