'***********************************************************************
'  ConsoleMonitor.vb - TVA Service Template
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
Imports System.Text
Imports System.Reflection.Assembly
Imports System.Security.Principal
Imports TVA.Services
Imports TVA.Remoting
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath
Imports TVA.Config.Common

' This project exists just to display status information about the parent service in a simple
' console window and allows remote users to monitor and control the service.  This is helpful
' for isolating any issues that may occur with the service as all status, progress, warnings
' and errors will be posted here.
'
' The console window can be setup to be automatically visible if the service is configured
' to "interact with desktop" and the config file is set to auto-launch the console
' monitor application by setting "ServiceHelper.AutoLaunchMonitorApplication" to True.
'
' NOTE: Do not remove this project!  The console monitor is a required server-side service
' application used by the service as a known external process to stop and restart itself.
'
' Class auto-generated using TVA service template at Fri Oct 1 14:26:17 EDT 2004
Public Class ConsoleMonitor

    Private Const SourceConfigFile As String = "BPA PDC To DatAWare Loader.exe.config"
    Private Const HostName As String = "BPA PDC To DatAWare Loader"
    Private Const HostURI As String = "tcp://localhost:6500/BPAPDCToDatAWareLoader"
    Private Const VariablePrefix As String = "ConsoleMonitor"

#Region " Remote Service Monitor Code "

    ' Create an instance of the remote service monitor class
    Private Shared WithEvents RemoteMonitor As RemoteServiceMonitor

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
                    .Append(" Console Based Service Monitor:")
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

    Private Shared Sub RemoteMonitor_ServiceProgress(ByVal BytesCompleted As Long, ByVal BytesTotal As Long) Handles RemoteMonitor.ServiceProgress

        Static LastProgress As Integer

        If BytesTotal > 0 Then
            Dim CurrProgress As Integer = CInt(BytesCompleted / BytesTotal * 100)

            If CurrProgress > LastProgress Or Math.Abs(CurrProgress - LastProgress) > 5 Then
                ' This progress information is bubbled up directly from service process
                UpdateStatus("    " & CurrProgress & "% complete")
                LastProgress = CurrProgress
            End If
        End If

    End Sub

    Private Shared Sub RemoteMonitor_ServiceMessage(ByVal Message As String, ByVal LogMessage As Boolean) Handles RemoteMonitor.ServiceMessage

        UpdateStatus(Message)

    End Sub

    Private Shared Sub RemoteMonitor_ServiceStateChanged(ByVal NewState As ServiceState) Handles RemoteMonitor.ServiceStateChanged

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

    Private Shared Sub RemoteMonitor_ServiceProcessStateChanged(ByVal NewState As ServiceMonitorNotification) Handles RemoteMonitor.ServiceProcessStateChanged

        Select Case NewState
            Case ServiceMonitorNotification.ProcessStarted
                UpdateStatus(vbCrLf & "Received process started notification [" & Now() & "]" & vbCrLf)
            Case ServiceMonitorNotification.ProcessCompleted
                UpdateStatus(vbCrLf & "Received process completed notification [" & Now() & "]" & vbCrLf)
            Case ServiceMonitorNotification.ProcessCanceled
                UpdateStatus(vbCrLf & "Received process canceled notification [" & Now() & "]" & vbCrLf)
        End Select

    End Sub

    Private Shared Sub RemoteMonitor_StatusMessage(ByVal Text As String, ByVal NewLine As Boolean) Handles RemoteMonitor.StatusMessage

        UpdateStatus(Text, NewLine)

    End Sub

    Private Shared Sub RemoteMonitor_ConnectionEstablished() Handles RemoteMonitor.ConnectionEstablished

        If StrComp(RemoteMonitor.ServiceName, HostName, CompareMethod.Text) = 0 Then
            UpdateStatus(vbCrLf & "Successfully connected to " & HostName & " [" & Now() & "]" & vbCrLf)
        Else
            ' We let the user know if we didn't connect to the service we were expecting...
            UpdateStatus(vbCrLf & "WARNING! Connected to foreign host: " & RemoteMonitor.ServiceName & " [" & Now() & "]" & vbCrLf)
            UpdateStatus("Results of this connection could be unpredictable - suggest terminating connection." & vbCrLf)
        End If

    End Sub

    Private Shared Sub RemoteMonitor_ConnectionTerminated() Handles RemoteMonitor.ConnectionTerminated

        UpdateStatus(vbCrLf & "Disconnected from " & HostName & " [" & Now() & "]" & vbCrLf)

    End Sub

    Private Shared Sub RemoteMonitor_SecureServerAuthentication() Handles RemoteMonitor.SecureServerAuthentication

        UpdateStatus(vbCrLf & "[" & Now() & "] Secure client authenticated with server." & vbCrLf)

    End Sub

    Private Shared Sub RemoteMonitor_SecureServerConnection() Handles RemoteMonitor.SecureServerConnection

        UpdateStatus(vbCrLf & "[" & Now() & "] Secure client connected to server." & vbCrLf)

    End Sub

    Private Shared Sub RemoteMonitor_UserEventHandlerException(ByVal EventName As String, ByVal ex As System.Exception) Handles RemoteMonitor.UserEventHandlerException

        UpdateStatus(vbCrLf & "An exception occured in an event handler [" & EventName & "]: " & ex.Message & vbCrLf)

    End Sub

    Private Shared Sub RemoteMonitor_AttemptingConnection() Handles RemoteMonitor.AttemptingConnection

        If Variables(VariablePrefix & ".DetailedErrors") Then
            UpdateStatus(vbCrLf & "Attempting connection to """ & Variables(VariablePrefix & ".HostURI") & """..." & vbCrLf)
        Else
            UpdateStatus(".", False)
        End If

    End Sub

    Private Shared Sub RemoteMonitor_ConnectionAttemptFailed(ByVal ex As System.Exception) Handles RemoteMonitor.ConnectionAttemptFailed

        If Variables(VariablePrefix & ".DetailedErrors") Then
            UpdateStatus(vbCrLf & "Connection attempt failed: " & ex.Message & vbCrLf)
        Else
            UpdateStatus(".", False)
        End If

    End Sub

    Private Shared Sub UpdateStatus(Optional ByVal Status As String = "", Optional ByVal NewLine As Boolean = True)

        Console.Write(Status)
        If NewLine Then Console.WriteLine()

    End Sub

#End Region

    Public Shared Sub Main()

        ' Check for service control related command line parameters
        If ServiceMonitor.HandleServiceControlCommands(Command(), HostName) Then End

        Dim strConsoleLine As String
        Dim objNotificationArgs As New ServiceNotificationEventArgs
        Dim flgSentNotification As Boolean

        Console.WriteLine(RemoteServiceMonitor.MonitorInformation)
        Console.WriteLine("    Started: " & Now() & vbCrLf)

        ' Make sure console monitor variables exist (we share config file with parent service)
        SharedConfigFileName = JustPath(SharedConfigFileName) & SourceConfigFile
        Variables.Create(VariablePrefix & ".HostURI", HostURI, VariableType.Text)
        Variables.Create(VariablePrefix & ".DetailedErrors", False, VariableType.Bool)
        Variables.Save()

        If Len(Variables(VariablePrefix & ".HostURI").ToString()) > 0 Then
            RemoteMonitor = New RemoteServiceMonitor(New BinaryClient(Variables(VariablePrefix & ".HostURI")))
            RemoteMonitor.Connect()
        Else
            Console.WriteLine()
            Console.WriteLine("No host URI information was available." & vbCrLf & vbCrLf & "No service connection could be established.")
            Console.WriteLine()
            Console.ReadLine()
            End
        End If

        Do While True
            ' This console window stays open by continually reading in console lines
            strConsoleLine = Console.ReadLine()

            ' While doing this, we monitor for special commands...
            objNotificationArgs.TranslateNotification(strConsoleLine)

            If objNotificationArgs.Notification <> ServiceNotification.Undetermined Then
                ' If user requests to stop service, make sure they want to do this because they won't
                ' be able to request a "start" once they are disconnected
                If objNotificationArgs.Notification = ServiceNotification.StopService Then
                    Console.WriteLine(vbCrLf & "WARNING: Are you sure you want to stop the service (enter Yes or No)?" & vbCrLf & "You will not be able to ""start"" the service remotely once it is stopped - you may want to use ""Restart"" instead." & vbCrLf)
                    flgSentNotification = (UCase(Left(Trim(Console.ReadLine()), 1)) = "Y")
                    If flgSentNotification Then
                        Console.WriteLine(vbCrLf & "StopService notification accepted." & vbCrLf)
                    Else
                        Console.WriteLine(vbCrLf & "StopService notification canceled." & vbCrLf)
                    End If
                Else
                    flgSentNotification = True
                End If

                If flgSentNotification Then
                    ' Send the actual notification to the service
                    If RemoteMonitor.SendSafeNotification(Nothing, objNotificationArgs) Then
                        Console.WriteLine(vbCrLf & [Enum].GetName(GetType(ServiceNotification), objNotificationArgs.Notification) & " notification successfully sent..." & vbCrLf)
                    Else
                        Console.WriteLine(vbCrLf & "Unable to send " & [Enum].GetName(GetType(ServiceNotification), objNotificationArgs.Notification) & " notification..." & vbCrLf)
                    End If
                End If
            ElseIf StrComp(strConsoleLine, "Connect", CompareMethod.Text) = 0 Or StrComp(strConsoleLine, "Reconnect", CompareMethod.Text) = 0 Then
                RemoteMonitor.Connect()
            ElseIf StrComp(strConsoleLine, "Disconnect", CompareMethod.Text) = 0 Then
                RemoteMonitor.Disconnect()
            ElseIf StrComp(strConsoleLine, "Reauthenticate", CompareMethod.Text) = 0 Then
                RemoteMonitor.Reauthenticate()
            ElseIf StrComp(strConsoleLine, "Help", CompareMethod.Text) = 0 Or strConsoleLine = "?" Then
                Console.WriteLine()
                DisplayPossibleCommandList()
            ElseIf StrComp(strConsoleLine, "Version", CompareMethod.Text) = 0 Then
                Console.WriteLine(vbCrLf & RemoteMonitor.Description() & vbCrLf)
            ElseIf StrComp(strConsoleLine, "Exit", CompareMethod.Text) = 0 Then
                Exit Do
            Else
                Console.Write(vbCrLf & "Command unrecognized.  ")
                DisplayPossibleCommandList()
            End If
        Loop

    End Sub

    Private Shared Sub DisplayPossibleCommandList()

        Console.WriteLine("Possible commands:")
        Console.WriteLine()
        Console.WriteLine("    ""Process  ProcessID""   - Starts specified process on demand")
        Console.WriteLine("    ""Abort""                - Aborts current service process")
        Console.WriteLine("    ""Pause""                - Pauses service")
        Console.WriteLine("    ""Resume""               - Resumes paused service")
        Console.WriteLine("    ""Stop""                 - Stops service")
        Console.WriteLine("    ""Restart""              - Restarts service")
        Console.WriteLine("    ""Status""               - Returns current service status")
        Console.WriteLine("    ""History""              - Displays the command history")
        Console.WriteLine("    ""Ping""                 - Request service ping")
        Console.WriteLine("    ""PingAll""              - Request service to ping all clients")
        Console.WriteLine("    ""Clients""              - Display client connections")
        Console.WriteLine("    ""Schedule""             - Show service process schedule")
        Console.WriteLine("    ""Reload""               - Reload service process schedule")
        Console.WriteLine("    ""Dir""                  - Returns remote directory listing")
        Console.WriteLine("    ""Settings""             - Displays all remote variable settings")
        Console.WriteLine("    ""Setting name = value"" - Updates remote variable setting")
        Console.WriteLine("    ""Connect""              - Starts connection cycle")
        Console.WriteLine("    ""Disconnect""           - Terminates current connection")
        If RemoteMonitor.ConnectionIsSecure() Then
            Console.WriteLine("    ""Reauthenticate""       - Reauthenticates client with server")
        End If
        Console.WriteLine("    ""Version""              - Displays client version information")
        Console.WriteLine("    ""Help""                 - Displays this help information")
        Console.WriteLine("    ""Exit""                 - Exits this console monitor")
        Console.WriteLine()

    End Sub

End Class