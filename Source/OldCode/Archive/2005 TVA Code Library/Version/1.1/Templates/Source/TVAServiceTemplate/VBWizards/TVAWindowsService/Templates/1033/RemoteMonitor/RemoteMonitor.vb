'***********************************************************************
'  RemoteMonitor.vb - TVA Service Template
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
Imports System.Text
Imports System.Reflection
Imports System.Reflection.Assembly
Imports System.Security.Principal
Imports TVA.Services
Imports TVA.Remoting
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath
Imports TVA.Config.Common
Imports TVA.Forms.Common
Imports VB = Microsoft.VisualBasic

' This project exists just to display status information about the parent service in a simple
' GUI application and allows remote users to monitor and control the service.  This is helpful
' for isolating any issues that may occur with the service as all status, progress, warnings
' and errors will be posted here.
'
' Class auto-generated using TVA service template at [!output GEN_TIME]
Public Class RemoteMonitor

    Inherits System.Windows.Forms.Form

    Private Const AuthenticationKey As String = "[!output GUID_HOSTAUTHKEY]"
    Private Const HostName As String = "[!output PROJECT_NAME]"
    Private Const HostURI As String = "tcp://localhost:6500/[!output PROJECT_ID]"
    Private Const MaximumStatusLength As Integer = 65536

#Region " Remote Service Monitor Code "

    ' Create an instance of the remote service monitor class
    Private WithEvents RemoteMonitor As RemoteServiceMonitor

    Private Class RemoteServiceMonitor

        Inherits ServiceMonitor

        Private WithEvents SecureRemotingClient As SecureClient

        Public Event SecureServerConnection()
        Public Event SecureServerAuthentication()

        Public Sub New(ByVal RemotingClient As ClientBase)

            MyBase.New(RemotingClient)

            ' If service is implementing a secure channel, we pick up some extra functionality...
            If TypeOf RemotingClient Is SecureClient Then SecureRemotingClient = DirectCast(RemotingClient, SecureClient)

        End Sub

        ' We create a derived service monitor to provide additional information about
        ' the "type" of client we are creating by overriding the description property
        Public Overrides ReadOnly Property Description() As String
            Get
                Return MonitorInformation & vbCrLf & MyBase.Description
            End Get
        End Property

        Public Shared ReadOnly Property MonitorInformation() As String
            Get
                With New StringBuilder
                    .Append(HostName)
                    .Append(" Remote Service Monitor:")
                    .Append(vbCrLf)
                    .Append("   Assembly: ")
                    .Append(GetShortAssemblyName(GetExecutingAssembly))
                    .Append(vbCrLf)
                    .Append("   Location: ")
                    .Append(GetExecutingAssembly.Location)
                    .Append(vbCrLf)
                    .Append("    Created: ")
                    .Append(File.GetLastWriteTime(GetExecutingAssembly.Location))
                    .Append(vbCrLf)
                    .Append("    NT User: ")
                    .Append(WindowsIdentity.GetCurrent.Name)

                    Return .ToString()
                End With
            End Get
        End Property

        Public Function ConnectionIsSecure() As Boolean

            Return Not SecureRemotingClient Is Nothing

        End Function

        Public Sub Reauthenticate()

            If ConnectionIsSecure() Then SecureRemotingClient.Reauthenticate()

        End Sub

        ' We bubble events up from the SecureClient if it is being used...
        Private Sub SecureRemotingClient_SecureServerConnection() Handles SecureRemotingClient.SecureServerConnection

            Try
                RaiseEvent SecureServerConnection()
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("RemoteServiceMonitor::SecureServerConnection", ex)
            End Try

        End Sub

        Private Sub SecureRemotingClient_SecureServerAuthentication() Handles SecureRemotingClient.SecureServerAuthentication

            Try
                RaiseEvent SecureServerAuthentication()
            Catch ex As Exception
                RaiseEvent_UserEventHandlerException("RemoteServiceMonitor::SecureServerAuthentication", ex)
            End Try

        End Sub

    End Class

    Private Sub RemoteMonitor_ServiceProgress(ByVal BytesCompleted As Long, ByVal BytesTotal As Long) Handles RemoteMonitor.ServiceProgress

        Static LastProgress As Integer

        If BytesTotal > 0 Then
            Dim CurrProgress As Integer = CInt(BytesCompleted / BytesTotal * 100)

            If CurrProgress > LastProgress Or Math.Abs(CurrProgress - LastProgress) > 5 Then
                ' This progress information is bubbled up directly from service process
                ProgressBar.Value = CurrProgress
                LastProgress = CurrProgress
            End If
        End If

    End Sub

    Private Sub RemoteMonitor_ServiceMessage(ByVal Message As String, ByVal LogMessage As Boolean) Handles RemoteMonitor.ServiceMessage

        UpdateStatus(Message)

    End Sub

    Private Sub RemoteMonitor_ServiceStateChanged(ByVal NewState As ServiceState) Handles RemoteMonitor.ServiceStateChanged

        Select Case NewState
            Case ServiceState.Started
                UpdateStatus(vbCrLf & "Received service started notification [" & Now() & "]" & vbCrLf)
            Case ServiceState.Stopped
                UpdateStatus(vbCrLf & "Received service stopped notification [" & Now() & "]" & vbCrLf)
            Case ServiceState.Paused
                UpdateStatus(vbCrLf & "Received service paused notification [" & Now() & "]" & vbCrLf)
            Case ServiceState.Resumed
                UpdateStatus(vbCrLf & "Received service resumed notification [" & Now() & "]" & vbCrLf)
            Case ServiceState.ShutDown
                UpdateStatus(vbCrLf & "Received system shutdown notification from service [" & Now() & "]" & vbCrLf)
        End Select

    End Sub

    Private Sub RemoteMonitor_ServiceProcessStateChanged(ByVal NewState As ServiceMonitorNotification) Handles RemoteMonitor.ServiceProcessStateChanged

        Select Case NewState
            Case ServiceMonitorNotification.ProcessStarted
                UpdateStatus(vbCrLf & "Received process started notification [" & Now() & "]" & vbCrLf)
            Case ServiceMonitorNotification.ProcessCompleted
                UpdateStatus(vbCrLf & "Received process completed notification [" & Now() & "]" & vbCrLf)
            Case ServiceMonitorNotification.ProcessCanceled
                UpdateStatus(vbCrLf & "Received process canceled notification [" & Now() & "]" & vbCrLf)
        End Select

    End Sub

    Private Sub RemoteMonitor_StatusMessage(ByVal Text As String, ByVal NewLine As Boolean) Handles RemoteMonitor.StatusMessage

        UpdateStatus(Text, NewLine)

    End Sub

    Private Sub RemoteMonitor_ConnectionEstablished() Handles RemoteMonitor.ConnectionEstablished

        If StrComp(RemoteMonitor.ServiceName, HostName, CompareMethod.Text) = 0 Then
            UpdateStatus(vbCrLf & "Successfully connected to " & HostName & " [" & Now() & "]" & vbCrLf)
        Else
            ' We let the user know if we didn't connect to the service we were expecting...
            UpdateStatus(vbCrLf & "WARNING! Connected to foreign host: " & RemoteMonitor.ServiceName & " [" & Now() & "]" & vbCrLf)
            UpdateStatus("Results of this connection could be unpredictable - suggest terminating connection." & vbCrLf)
        End If

    End Sub

    Private Sub RemoteMonitor_ConnectionTerminated() Handles RemoteMonitor.ConnectionTerminated

        UpdateStatus(vbCrLf & "Disconnected from " & HostName & " [" & Now() & "]" & vbCrLf)

    End Sub

    Private Sub RemoteMonitor_SecureServerAuthentication() Handles RemoteMonitor.SecureServerAuthentication

        UpdateStatus(vbCrLf & "[" & Now() & "] Secure client authenticated with server." & vbCrLf)

    End Sub

    Private Sub RemoteMonitor_SecureServerConnection() Handles RemoteMonitor.SecureServerConnection

        UpdateStatus(vbCrLf & "[" & Now() & "] Secure client connected to server." & vbCrLf)

    End Sub

    Private Sub RemoteMonitor_UserEventHandlerException(ByVal EventName As String, ByVal ex As System.Exception) Handles RemoteMonitor.UserEventHandlerException

        UpdateStatus(vbCrLf & "An exception occured in an event handler [" & EventName & "]: " & ex.Message & vbCrLf)

    End Sub

    Private Sub RemoteMonitor_AttemptingConnection() Handles RemoteMonitor.AttemptingConnection

        If Variables("RemoteMonitor.DetailedErrors") Then
            UpdateStatus(vbCrLf & "Attempting connection to """ & Variables("RemoteMonitor.HostURI") & """..." & vbCrLf)
        Else
            UpdateStatus(".", False)
        End If

    End Sub

    Private Sub RemoteMonitor_ConnectionAttemptFailed(ByVal ex As System.Exception) Handles RemoteMonitor.ConnectionAttemptFailed

        If Variables("RemoteMonitor.DetailedErrors") Then
            UpdateStatus(vbCrLf & "Connection attempt failed: " & ex.Message & vbCrLf)
        Else
            UpdateStatus(".", False)
        End If

    End Sub

#End Region

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents ProgressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents ServiceStatusLabel As System.Windows.Forms.Label
    Friend WithEvents StatusText As System.Windows.Forms.TextBox
    Friend WithEvents ButtonPause As System.Windows.Forms.Button
    Friend WithEvents ButtonResume As System.Windows.Forms.Button
    Friend WithEvents ButtonStop As System.Windows.Forms.Button
    Friend WithEvents ButtonRestart As System.Windows.Forms.Button
    Friend WithEvents ButtonQuit As System.Windows.Forms.Button
    Friend WithEvents ButtonPing As System.Windows.Forms.Button
    Friend WithEvents ButtonPingAll As System.Windows.Forms.Button
    Friend WithEvents ButtonShowSchedule As System.Windows.Forms.Button
    Friend WithEvents ButtonReloadSchedule As System.Windows.Forms.Button
    Friend WithEvents ButtonGetClientList As System.Windows.Forms.Button
    Friend WithEvents ServiceProgressLabel As System.Windows.Forms.Label
    Friend WithEvents ButtonShowStatus As System.Windows.Forms.Button
    Friend WithEvents ButtonStartProcess As System.Windows.Forms.Button
    Friend WithEvents VersionLabel As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(RemoteMonitor))
        Me.ProgressBar = New System.Windows.Forms.ProgressBar
        Me.ServiceProgressLabel = New System.Windows.Forms.Label
        Me.ServiceStatusLabel = New System.Windows.Forms.Label
        Me.StatusText = New System.Windows.Forms.TextBox
        Me.ButtonPause = New System.Windows.Forms.Button
        Me.ButtonResume = New System.Windows.Forms.Button
        Me.ButtonStop = New System.Windows.Forms.Button
        Me.ButtonRestart = New System.Windows.Forms.Button
        Me.ButtonShowStatus = New System.Windows.Forms.Button
        Me.ButtonQuit = New System.Windows.Forms.Button
        Me.ButtonPing = New System.Windows.Forms.Button
        Me.ButtonPingAll = New System.Windows.Forms.Button
        Me.ButtonShowSchedule = New System.Windows.Forms.Button
        Me.ButtonReloadSchedule = New System.Windows.Forms.Button
        Me.ButtonGetClientList = New System.Windows.Forms.Button
        Me.ButtonStartProcess = New System.Windows.Forms.Button
        Me.VersionLabel = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'ProgressBar
        '
        Me.ProgressBar.Location = New System.Drawing.Point(8, 360)
        Me.ProgressBar.Name = "ProgressBar"
        Me.ProgressBar.Size = New System.Drawing.Size(568, 24)
        Me.ProgressBar.TabIndex = 3
        '
        'ServiceProgressLabel
        '
        Me.ServiceProgressLabel.Location = New System.Drawing.Point(8, 344)
        Me.ServiceProgressLabel.Name = "ServiceProgressLabel"
        Me.ServiceProgressLabel.Size = New System.Drawing.Size(568, 16)
        Me.ServiceProgressLabel.TabIndex = 2
        Me.ServiceProgressLabel.Text = "Current Process Progress:"
        '
        'ServiceStatusLabel
        '
        Me.ServiceStatusLabel.Location = New System.Drawing.Point(8, 8)
        Me.ServiceStatusLabel.Name = "ServiceStatusLabel"
        Me.ServiceStatusLabel.Size = New System.Drawing.Size(136, 16)
        Me.ServiceStatusLabel.TabIndex = 0
        Me.ServiceStatusLabel.Text = "Service Status:"
        '
        'StatusText
        '
        Me.StatusText.BackColor = System.Drawing.Color.Black
        Me.StatusText.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StatusText.ForeColor = System.Drawing.Color.White
        Me.StatusText.Location = New System.Drawing.Point(8, 24)
        Me.StatusText.MaxLength = 0
        Me.StatusText.Multiline = True
        Me.StatusText.Name = "StatusText"
        Me.StatusText.ReadOnly = True
        Me.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.StatusText.Size = New System.Drawing.Size(568, 312)
        Me.StatusText.TabIndex = 1
        Me.StatusText.TabStop = False
        Me.StatusText.Text = ""
        '
        'ButtonPause
        '
        Me.ButtonPause.Location = New System.Drawing.Point(584, 8)
        Me.ButtonPause.Name = "ButtonPause"
        Me.ButtonPause.Size = New System.Drawing.Size(112, 23)
        Me.ButtonPause.TabIndex = 4
        Me.ButtonPause.Text = "Pause Service"
        '
        'ButtonResume
        '
        Me.ButtonResume.Location = New System.Drawing.Point(584, 40)
        Me.ButtonResume.Name = "ButtonResume"
        Me.ButtonResume.Size = New System.Drawing.Size(112, 23)
        Me.ButtonResume.TabIndex = 5
        Me.ButtonResume.Text = "Resume Service"
        '
        'ButtonStop
        '
        Me.ButtonStop.Location = New System.Drawing.Point(584, 72)
        Me.ButtonStop.Name = "ButtonStop"
        Me.ButtonStop.Size = New System.Drawing.Size(112, 23)
        Me.ButtonStop.TabIndex = 6
        Me.ButtonStop.Text = "Stop Service"
        '
        'ButtonRestart
        '
        Me.ButtonRestart.Location = New System.Drawing.Point(584, 104)
        Me.ButtonRestart.Name = "ButtonRestart"
        Me.ButtonRestart.Size = New System.Drawing.Size(112, 23)
        Me.ButtonRestart.TabIndex = 7
        Me.ButtonRestart.Text = "Restart Service"
        '
        'ButtonShowStatus
        '
        Me.ButtonShowStatus.Location = New System.Drawing.Point(584, 296)
        Me.ButtonShowStatus.Name = "ButtonShowStatus"
        Me.ButtonShowStatus.Size = New System.Drawing.Size(112, 23)
        Me.ButtonShowStatus.TabIndex = 13
        Me.ButtonShowStatus.Text = "Show Status"
        '
        'ButtonQuit
        '
        Me.ButtonQuit.Location = New System.Drawing.Point(584, 360)
        Me.ButtonQuit.Name = "ButtonQuit"
        Me.ButtonQuit.Size = New System.Drawing.Size(112, 23)
        Me.ButtonQuit.TabIndex = 15
        Me.ButtonQuit.Text = "Quit"
        '
        'ButtonPing
        '
        Me.ButtonPing.Location = New System.Drawing.Point(584, 136)
        Me.ButtonPing.Name = "ButtonPing"
        Me.ButtonPing.Size = New System.Drawing.Size(112, 23)
        Me.ButtonPing.TabIndex = 8
        Me.ButtonPing.Text = "Ping Service"
        '
        'ButtonPingAll
        '
        Me.ButtonPingAll.Location = New System.Drawing.Point(584, 168)
        Me.ButtonPingAll.Name = "ButtonPingAll"
        Me.ButtonPingAll.Size = New System.Drawing.Size(112, 23)
        Me.ButtonPingAll.TabIndex = 9
        Me.ButtonPingAll.Text = "Ping All Clients"
        '
        'ButtonShowSchedule
        '
        Me.ButtonShowSchedule.Location = New System.Drawing.Point(584, 232)
        Me.ButtonShowSchedule.Name = "ButtonShowSchedule"
        Me.ButtonShowSchedule.Size = New System.Drawing.Size(112, 23)
        Me.ButtonShowSchedule.TabIndex = 11
        Me.ButtonShowSchedule.Text = "Show Schedule"
        '
        'ButtonReloadSchedule
        '
        Me.ButtonReloadSchedule.Location = New System.Drawing.Point(584, 264)
        Me.ButtonReloadSchedule.Name = "ButtonReloadSchedule"
        Me.ButtonReloadSchedule.Size = New System.Drawing.Size(112, 23)
        Me.ButtonReloadSchedule.TabIndex = 12
        Me.ButtonReloadSchedule.Text = "Reload Schedule"
        '
        'ButtonGetClientList
        '
        Me.ButtonGetClientList.Location = New System.Drawing.Point(584, 200)
        Me.ButtonGetClientList.Name = "ButtonGetClientList"
        Me.ButtonGetClientList.Size = New System.Drawing.Size(112, 23)
        Me.ButtonGetClientList.TabIndex = 10
        Me.ButtonGetClientList.Text = "Get Client List"
        '
        'ButtonStartProcess
        '
        Me.ButtonStartProcess.Location = New System.Drawing.Point(584, 328)
        Me.ButtonStartProcess.Name = "ButtonStartProcess"
        Me.ButtonStartProcess.Size = New System.Drawing.Size(112, 23)
        Me.ButtonStartProcess.TabIndex = 14
        Me.ButtonStartProcess.Text = "Start Process"
        '
        'VersionLabel
        '
        Me.VersionLabel.Location = New System.Drawing.Point(336, 8)
        Me.VersionLabel.Name = "VersionLabel"
        Me.VersionLabel.Size = New System.Drawing.Size(248, 16)
        Me.VersionLabel.TabIndex = 16
        Me.VersionLabel.Text = "Version: 1.0.0.0"
        Me.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'RemoteMonitor
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(702, 387)
        Me.Controls.Add(Me.VersionLabel)
        Me.Controls.Add(Me.ButtonStartProcess)
        Me.Controls.Add(Me.ButtonGetClientList)
        Me.Controls.Add(Me.ButtonReloadSchedule)
        Me.Controls.Add(Me.ButtonShowSchedule)
        Me.Controls.Add(Me.ButtonPingAll)
        Me.Controls.Add(Me.ButtonPing)
        Me.Controls.Add(Me.ButtonQuit)
        Me.Controls.Add(Me.ButtonShowStatus)
        Me.Controls.Add(Me.ButtonRestart)
        Me.Controls.Add(Me.ButtonStop)
        Me.Controls.Add(Me.ButtonResume)
        Me.Controls.Add(Me.ButtonPause)
        Me.Controls.Add(Me.StatusText)
        Me.Controls.Add(Me.ServiceStatusLabel)
        Me.Controls.Add(Me.ServiceProgressLabel)
        Me.Controls.Add(Me.ProgressBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "RemoteMonitor"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Remote Service Monitor"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub RemoteMonitor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Load form title from assembly information
        Me.Text = DirectCast(GetExecutingAssembly.GetCustomAttributes(GetType(AssemblyTitleAttribute), False)(0), AssemblyTitleAttribute).Title

        ' Display compiled assembly version
        With FileVersionInfo.GetVersionInfo(GetExecutingAssembly.Location)
            VersionLabel.Text = "Version: " & .FileMajorPart & "." & .FileMinorPart & "." & .FileBuildPart & "." & .FilePrivatePart
        End With

        UpdateStatus(RemoteServiceMonitor.MonitorInformation)
        UpdateStatus("    Started: " & Now() & vbCrLf)

        RestoreWindowSettings(Me)

        ' Make sure remote service monitor variables exist
        Variables.Create("RemoteMonitor.HostURI", HostURI, VariableType.Text)
        Variables.Create("RemoteMonitor.DetailedErrors", False, VariableType.Bool)
        Variables.Save()

        If Len(Variables("RemoteMonitor.HostURI").ToString()) > 0 Then
            RemoteMonitor = New RemoteServiceMonitor(New SecureClient(Variables("RemoteMonitor.HostURI"), AuthenticationKey))
            RemoteMonitor.Connect()
        Else
            UpdateStatus(vbCrLf & "No host URI information was available." & vbCrLf & vbCrLf & "No service connection could be established." & vbCrLf)
        End If

    End Sub

    Private Sub RemoteMonitor_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed

        SaveWindowSettings(Me)

    End Sub

    Private Sub UpdateStatus(Optional ByVal Status As String = "", Optional ByVal NewLine As Boolean = True)

        If NewLine Then
            Status = StatusText.Text & Status & vbCrLf
        Else
            Status = StatusText.Text & Status
        End If

        Status = VB.Right(Status, MaximumStatusLength)
        StatusText.Text = Status
        StatusText.SelectionStart = Len(Status)
        StatusText.ScrollToCaret()

    End Sub

    Private Sub SendNotification(ByVal NotificationType As ServiceNotification, Optional ByVal ItemName As String = "", Optional ByVal ItemData As Object = Nothing)

        Dim NotificationArgs As New ServiceNotificationEventArgs(NotificationType)

        With NotificationArgs
            .EventItemName = ItemName
            .EventItemData = ItemData

            If .Notification <> ServiceNotification.Undetermined Then
                If RemoteMonitor.SendSafeNotification(Nothing, NotificationArgs) Then
                    UpdateStatus(vbCrLf & .Notification.GetName(GetType(ServiceNotification), .Notification) & " notification successfully sent..." & vbCrLf)
                Else
                    UpdateStatus(vbCrLf & "Unable to send " & .Notification.GetName(GetType(ServiceNotification), .Notification) & " notification..." & vbCrLf)
                End If
            End If
        End With

    End Sub

    Private Sub ButtonPause_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPause.Click

        SendNotification(ServiceNotification.PauseService)

    End Sub

    Private Sub ButtonResume_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonResume.Click

        SendNotification(ServiceNotification.ResumeService)

    End Sub

    Private Sub ButtonStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonStop.Click

        If MsgBox("WARNING: Are you sure you want to stop the service?" & vbCrLf & "You will not be able to ""start"" the service remotely once it is stopped - you may want to use ""Restart Service"" instead.", MsgBoxStyle.YesNo + MsgBoxStyle.Question, Me.Text) = MsgBoxResult.Yes Then
            SendNotification(ServiceNotification.StopService)
        End If

    End Sub

    Private Sub ButtonRestart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonRestart.Click

        SendNotification(ServiceNotification.RestartService)

    End Sub

    Private Sub ButtonPing_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPing.Click

        SendNotification(ServiceNotification.PingService)

    End Sub

    Private Sub ButtonShowStatus_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonShowStatus.Click

        SendNotification(ServiceNotification.GetServiceStatus)

    End Sub

    Private Sub ButtonStartProcess_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonStartProcess.Click

        ' Note that the on demand process request can accept an optional parameter
        SendNotification(ServiceNotification.OnDemandProcess)

    End Sub

    Private Sub ButtonPingAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPingAll.Click

        SendNotification(ServiceNotification.PingAllClients)

    End Sub

    Private Sub ButtonShowSchedule_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonShowSchedule.Click

        ' Note that the process schedule display is customizable, so you can alternately pass in additional data for this request
        SendNotification(ServiceNotification.GetProcessSchedule)

    End Sub

    Private Sub ButtonReloadSchedule_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonReloadSchedule.Click

        SendNotification(ServiceNotification.ReloadProcessSchedule)

    End Sub

    Private Sub ButtonGetClientList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGetClientList.Click

        SendNotification(ServiceNotification.GetClientList)

    End Sub

    Private Sub ButtonQuit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonQuit.Click

        Me.Close()

    End Sub

End Class
