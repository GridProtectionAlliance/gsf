' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Diagnostics
Imports System.Drawing
Imports System.ComponentModel
Imports System.Collections
Imports System.Xml
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Shared.String
Imports TVA.Remoting
Imports TVA.Remoting.ServerBase
Imports TVA.Threading
Imports TVA.Config.Common
Imports TVA.Config.ApplicationVariables
Imports VB = Microsoft.VisualBasic

Namespace Services

    ' Defines an interface for user created components used by the service so that components
    ' can inform service of current status and automatically react to service events
    Public Interface IServiceComponent

        ReadOnly Property Name() As String
        ReadOnly Property Status() As String
        Sub ServiceStateChanged(ByVal NewState As ServiceState)
        Sub ProcessStateChanged(ByVal NewState As ProcessState)
        Sub Dispose()

    End Interface

    Public Enum ScheduleType As Integer
        [Unscheduled]   ' Run with no schedule (on-demand)
        [Daily]         ' Run every day at specified time
        [Weekly]        ' Run every week on specified week days at specified time
        [Monthly]       ' Run every month on specified days at specified time
        [Yearly]        ' Run every year on specified months and days at specified time
        [Intervaled]    ' Run on a specific interval
    End Enum

    ' Define a custom service component collection
    Public Class ServiceComponents

        Implements IEnumerable

        Private ServiceComponents As ArrayList

        Friend Sub New()

            ServiceComponents = New ArrayList

        End Sub

        Friend Sub New(ByVal BaseList As ArrayList)

            ServiceComponents = BaseList

        End Sub

        Public Sub Add(ByVal Component As IServiceComponent)

            SyncLock ServiceComponents.SyncRoot
                ServiceComponents.Add(Component)
            End SyncLock

        End Sub

        Friend Sub Clear()

            ServiceComponents.Clear()

        End Sub

        Public Sub Remove(ByVal Index As Integer)

            SyncLock ServiceComponents.SyncRoot
                If Index >= 0 And Index < ServiceComponents.Count Then
                    ServiceComponents.RemoveAt(Index)
                End If
            End SyncLock

        End Sub

        Public Sub Remove(ByVal Component As IServiceComponent)

            Dim x As Integer

            SyncLock ServiceComponents.SyncRoot
                For x = 0 To ServiceComponents.Count - 1
                    If Component Is Me(x) Then
                        Remove(x)
                        Exit For
                    End If
                Next
            End SyncLock

        End Sub

        Default Public ReadOnly Property Item(ByVal Index As Integer) As IServiceComponent

            Get
                Dim scItem As IServiceComponent

                SyncLock ServiceComponents.SyncRoot
                    If Index >= 0 And Index < ServiceComponents.Count Then
                        scItem = DirectCast(ServiceComponents(Index), IServiceComponent)
                    End If
                End SyncLock

                Return scItem
            End Get

        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Dim intCount As Integer

                SyncLock ServiceComponents.SyncRoot
                    intCount = ServiceComponents.Count
                End SyncLock

                Return intCount
            End Get
        End Property

        Public Function Clone() As ServiceComponents

            Dim sccItem As ServiceComponents

            SyncLock ServiceComponents.SyncRoot
                sccItem = New ServiceComponents(ServiceComponents.Clone())
            End SyncLock

            Return sccItem

        End Function

        Public ReadOnly Property SyncRoot() As Object
            Get
                Return ServiceComponents.SyncRoot
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

            Return ServiceComponents.GetEnumerator()

        End Function

        Friend ReadOnly Property This() As ServiceComponents
            Get
                Return Me
            End Get
        End Property

    End Class

    ' Define a service "process" that encompasses scheduling and threading of service code
    Public Class ServiceProcess

        Private Parent As ServiceHelper                     ' The parent service process

        ' Service Process Properties
        Private ServiceProcessSchedule As ScheduleType      ' Type of process schedule
        Private ServiceProcessState As ProcessState         ' Current run state of this process
        Private ServiceProcessMonths As Integer()           ' Months to run process
        Private ServiceProcessDays As Integer()             ' Days of month or days of week to run process
        Private ServiceProcessThread As Thread              ' Thread used by this process
        Private ServiceProcessClient As LocalClient         ' Remote client that executed an on-demand process
        Private ServiceProcessTime As TimeSpan              ' Time to run this process
        Private ServiceProcessUserData As Object            ' User data for on-demand process calls

        Public Sub New(ByVal Parent As ServiceHelper, ByVal Intervaled As Boolean, ByVal Interval As Integer)

            ' Create a new "on demand" or "intervaled" process
            Me.Parent = Parent

            ServiceProcessTime = New TimeSpan(Interval * 10000L)
            ServiceProcessState = ProcessState.Unprocessed
            ServiceProcessSchedule = IIf(Intervaled, ScheduleType.Intervaled, ScheduleType.Unscheduled)

        End Sub

        Public Sub New(ByVal Parent As ServiceHelper, ByVal ScheduledTime As TimeSpan)

            ' Create a new scheduled daily process
            Me.Parent = Parent

            ServiceProcessTime = ScheduledTime
            ServiceProcessState = ProcessState.Unprocessed
            ServiceProcessSchedule = ScheduleType.Daily

        End Sub

        Public Sub New(ByVal Parent As ServiceHelper, ByVal DaysOfWeek As DayOfWeek(), ByVal ScheduledTime As TimeSpan)

            ' Create a new scheduled weekly process
            Me.Parent = Parent

            ServiceProcessDays = Array.CreateInstance(GetType(Integer), DaysOfWeek.Length)
            DaysOfWeek.CopyTo(ServiceProcessDays, 0)
            Array.Sort(ServiceProcessDays)

            ServiceProcessTime = ScheduledTime

            ServiceProcessState = ProcessState.Unprocessed
            ServiceProcessSchedule = ScheduleType.Weekly

        End Sub

        Public Sub New(ByVal Parent As ServiceHelper, ByVal DaysOfMonth As Integer(), ByVal ScheduledTime As TimeSpan)

            ' Create a new scheduled monthly process
            Me.Parent = Parent

            ServiceProcessDays = DaysOfMonth
            Array.Sort(ServiceProcessDays)

            ServiceProcessTime = ScheduledTime

            ServiceProcessState = ProcessState.Unprocessed
            ServiceProcessSchedule = ScheduleType.Monthly

        End Sub

        Public Sub New(ByVal Parent As ServiceHelper, ByVal Months As Integer(), ByVal DaysOfMonth As Integer(), ByVal ScheduledTime As TimeSpan)

            ' Create a new scheduled yearly process
            Me.Parent = Parent

            ServiceProcessMonths = Months
            Array.Sort(ServiceProcessMonths)

            ServiceProcessDays = DaysOfMonth
            Array.Sort(ServiceProcessDays)

            ServiceProcessTime = ScheduledTime

            ServiceProcessState = ProcessState.Unprocessed
            ServiceProcessSchedule = ScheduleType.Yearly

        End Sub

        Public ReadOnly Property Scheduled() As Boolean
            Get
                Return (ServiceProcessSchedule <> ScheduleType.Unscheduled And ServiceProcessSchedule <> ScheduleType.Intervaled)
            End Get
        End Property

        ' Current run state of this process
        Public Property ProcessState() As ProcessState
            Get
                Return ServiceProcessState
            End Get
            Set(ByVal Value As ProcessState)
                ServiceProcessState = Value

                ' Notify any server components of change in process state
                Parent.NotifyComponentsOfProcessStateChange(ServiceProcessState)

                ' Notify any remote clients of change in process state.  This is very useful for any applications
                ' that may use data created by this service as the apps can automatically put themselves into a
                ' "maintenance mode" thereby disallowing access until the processing is complete.
                Parent.SendServiceNotification(Nothing, New ServiceMonitorNotificationEventArgs(Parent.GetTranslatedProcessingState(ServiceProcessState)))
            End Set
        End Property

        Public ReadOnly Property ProcessTime() As TimeSpan
            Get
                Return ServiceProcessTime
            End Get
        End Property

        Public ReadOnly Property ProcessDays() As Integer()
            Get
                Return ServiceProcessDays
            End Get
        End Property

        Public ReadOnly Property ProcessMonths() As Integer()
            Get
                Return ServiceProcessMonths
            End Get
        End Property

        Public ReadOnly Property ProcessSchedule() As ScheduleType
            Get
                Return ServiceProcessSchedule
            End Get
        End Property

        Public ReadOnly Property ProcessThread() As Thread
            Get
                Return ServiceProcessThread
            End Get
        End Property

        Public Property UserData() As Object
            Get
                Return ServiceProcessUserData
            End Get
            Set(ByVal Value As Object)
                ServiceProcessUserData = Value
            End Set
        End Property

        Public Property Client() As LocalClient
            Get
                Return ServiceProcessClient
            End Get
            Set(ByVal Value As LocalClient)
                ServiceProcessClient = Value
            End Set
        End Property

        ' Define general thread manipulation methods
        Friend Sub StartThread(ByVal ThreadProc As ThreadStart, ByVal ThreadID As Long)

            ServiceProcessThread = New Thread(ThreadProc)
            If Parent.AboveNormalThreadPriority Then
                ServiceProcessThread.Priority = ThreadPriority.AboveNormal
            Else
                ServiceProcessThread.Priority = ThreadPriority.Normal
            End If
            ServiceProcessThread.Name = ThreadID
            ServiceProcessThread.Start()

        End Sub

        Friend Sub EndThread()

            ServiceProcessThread = Nothing

        End Sub

        Friend Sub AbortThread()

            If Not ServiceProcessThread Is Nothing Then
                ServiceProcessThread.Abort()
            End If

            ServiceProcessThread = Nothing

        End Sub

        Friend Sub SuspendThread()

            If Not ServiceProcessThread Is Nothing Then
                ServiceProcessThread.Suspend()
            End If

        End Sub

        Friend Sub ResumeThread()

            If Not ServiceProcessThread Is Nothing Then
                ServiceProcessThread.Resume()
            End If

        End Sub

        Friend Sub Sleep(ByVal Seconds As Integer)

            If Not ServiceProcessThread Is Nothing Then
                ServiceProcessThread.Sleep(Seconds * 1000)
            End If

        End Sub

        Public ReadOnly Property ThreadID() As Integer
            Get
                If ServiceProcessThread Is Nothing Then
                    ThreadID = -1
                Else
                    ThreadID = CInt(ServiceProcessThread.Name)
                End If
            End Get
        End Property

    End Class

    ' Define a process info collection
    Public Class ServiceProcesses

        Inherits CollectionBase

        Friend Sub New()

            ' This is only intended for internal creation

        End Sub

        Public Sub Add(ByVal Process As ServiceProcess)

            SyncLock List.SyncRoot
                List.Add(Process)
            End SyncLock

        End Sub

        Public Sub Remove(ByVal Index As Integer)

            SyncLock List.SyncRoot
                If Index >= 0 And Index < Count Then
                    List.RemoveAt(Index)
                End If
            End SyncLock

        End Sub

        Public Sub Remove(ByVal Process As ServiceProcess)

            Dim x As Integer

            SyncLock List.SyncRoot
                For x = 0 To Count - 1
                    If Process.ThreadID = Me(x).ThreadID Then
                        Remove(x)
                        Exit For
                    End If
                Next
            End SyncLock

        End Sub

        Default Public ReadOnly Property Item(ByVal Index As Integer) As ServiceProcess

            Get
                Dim ptItem As ServiceProcess

                SyncLock List.SyncRoot
                    If Index >= 0 And Index < List.Count Then
                        ptItem = DirectCast(List.Item(Index), ServiceProcess)
                    End If
                End SyncLock

                Return ptItem
            End Get

        End Property

        Public ReadOnly Property SyncRoot() As Object
            Get
                Return List.SyncRoot
            End Get
        End Property

    End Class

    <ToolboxBitmap(GetType(ServiceHelper), "ServiceHelper.bmp"), DefaultEvent("ServiceProcess"), DefaultProperty("RemotingServer")> _
    Public Class ServiceHelper

        Inherits Component
        Implements ISupportInitialize

        ' Define a delgate to allow end users the option of overriding service status reporting functionality
        Public Delegate Function CustomServiceStatusSignature(ByVal StatusParameter As String) As String

        ' Define events to notify service when the service state has changed
        Public Event OnStart(ByVal args() As String)
        Public Event OnContinue()
        Public Event OnPause()
        Public Event OnShutdown()
        Public Event OnStop()

        ' Define an event to do the actual service work that gets called from a scheduled process thread
        Public Event ExecuteServiceProcess(ByVal ProcessSchedule As ScheduleType, ByVal ScheduledTime As TimeSpan, ByVal Client As LocalClient, ByVal UserData As Object)

        ' Define an event to allow derived class to pick up client notifications
        Public Event ClientNotification(ByVal Client As LocalClient, ByVal sender As Object, ByVal e As EventArgs)

        ' Instance variables
        <Browsable(False)> Public AppPath As String                         ' Service application path
        Protected ServiceController As ServiceControlHelper                 ' Our instance of the service control helper
        Protected ScheduledRestarts As ArrayList                            ' Scheduled service restart times
        Protected CommandHistory As CommandHistoryCollection                ' Service notification command history

        Private ServiceProcesses As ServiceProcesses                        ' Scheduled process collection
        Private ServiceComponents As ServiceComponents                      ' Defines a user definable collection of service components
        Private WithEvents ScheduledProcessTimer As Timers.Timer            ' Scheduled process run timer
        Private WithEvents IntervalProcessTimer As Timers.Timer             ' Interval process run timer
        Private WithEvents ServiceEventTimer As Timers.Timer                ' Service event notification timer
        Private ServiceStarted As Date                                      ' Service start time
        Private UIProcessID As Integer                                      ' Process ID of launched associated UI (if any)
        Private CurrentEventState As ServiceState                           ' Current service state
        Private StartArgs() As String                                       ' OnStart service arguments
        Private ServiceHasIntervalProcess As Boolean                        ' Flag to indicate that service has intervaled process
        Private CustomServiceStatus As CustomServiceStatusSignature         ' User overridable service status function

        ' Exposed ServiceHelper property values
        Private HelperParentService As System.ServiceProcess.ServiceBase    ' The parent service process
        Private WithEvents HelperRemotingServer As IServer                  ' Remoting server instance to allow remote monitoring
        Private HelperShowClientConnects As Boolean                         ' Set to True to report all client connections to remote monitors
        Private HelperShowClientDisconnects As Boolean                      ' Set to True to report all client disconnections to remote monitors
        Private HelperMissedEventThreshold As Integer                       ' Specifies the maximum tolerance for processing missed timer events
        Private HelperVariablePrefix As String                              ' Application variable prefix
        Private HelperMonitorApplication As String                          ' Associated UI to launch if desired
        Private HelperAutoLaunchMonitorApplication As Boolean               ' Specifies whether or not to auto-launch monitor application at startup
        Private HelperCommandHistoryLimit As Integer                        ' Set the maximum number of command history items
        Private HelperAboveNormalThreadPriority As Boolean                  ' Specifies whether or not use a higher than normal thread priority
        Private HelperIncludeRestartSchedule As Boolean                     ' Flag to determine if service should include a restart schedule

        Protected Class CommandHistoryCollection

            Public HistoryLimit As Integer
            Private Parent As ServiceHelper
            Private CommandHistory As ArrayList

            Public Class Command

                Public Type As ServiceNotification
                Public ID As Guid
                Public Time As Date = Now()

                Public Sub New(ByVal Type As ServiceNotification, ByVal ID As Guid)

                    Me.Type = Type
                    Me.ID = ID

                End Sub

            End Class

            Friend Sub New(ByVal Parent As ServiceHelper, ByVal HistoryLimit As Integer)

                Me.Parent = Parent
                Me.HistoryLimit = HistoryLimit
                Me.CommandHistory = New ArrayList

            End Sub

            Public Sub Add(ByVal Type As ServiceNotification, ByVal ID As Guid)

                If HistoryLimit > 0 Then
                    SyncLock CommandHistory.SyncRoot
                        CommandHistory.Add(New Command(Type, ID))

                        While CommandHistory.Count > HistoryLimit
                            CommandHistory.RemoveAt(0)
                        End While
                    End SyncLock
                End If

            End Sub

            Public Function GetHistoryList() As String

                Dim strHistory As New StringBuilder
                Dim cmdItem As Command

                strHistory.Append(vbCrLf & Parent.HelperParentService.ServiceName & " Command History:" & vbCrLf & vbCrLf)
                strHistory.Append("Command".PadRight(22) & " " & "Sent".PadRight(19) & " " & "By".PadRight(36) & vbCrLf)
                strHistory.Append(New String("-"c, 22) & " " & New String("-"c, 19) & " " & New String("-"c, 36) & vbCrLf)

                SyncLock CommandHistory.SyncRoot
                    For Each cmdItem In CommandHistory
                        With cmdItem
                            strHistory.Append([Enum].GetName(GetType(ServiceNotification), .Type).PadRight(22) & " " & .Time.ToString("MM/dd/yyyy HH:mm:ss").PadRight(19) & " " & .ID.ToString() & vbCrLf)
                        End With
                    Next
                End SyncLock

                Return strHistory.ToString()

            End Function

        End Class

        ' We use this class to queue up service stops and restarts so that the service will have time to
        ' finish responding to any current events (e.g., RPC reponses) before starting service shutdown...
        Protected Class ServiceControlHelper

            Private Parent As ServiceHelper
            Private LocalController As System.ServiceProcess.ServiceController

            Friend Sub New(ByVal Parent As ServiceHelper)

                Me.Parent = Parent
                LocalController = New System.ServiceProcess.ServiceController(Parent.HelperParentService.ServiceName)

            End Sub

            ' We immediately respond to pause/resume requests
            Public Function PauseService() As Boolean

                Dim flgSuccess As Boolean = True

                Try
                    LocalController.Pause()
                Catch
                    flgSuccess = False
                End Try

                Return flgSuccess

            End Function

            Public Function ResumeService() As Boolean

                Dim flgSuccess As Boolean = True

                Try
                    LocalController.Continue()
                Catch
                    flgSuccess = False
                End Try

                Return flgSuccess

            End Function

            ' We use an external process to handle stops and restarts
            Public Function StopService() As Boolean

                Return (Parent.LaunchConsoleMonitor(" /STOP", AppWinStyle.Hide) <> -1)

            End Function

            Public Function RestartService() As Boolean

                Return (Parent.LaunchConsoleMonitor(" /RESTART", AppWinStyle.Hide) <> -1)

            End Function

        End Class

        Public Sub New()

            HelperVariablePrefix = "Service"
            HelperMonitorApplication = "ConsoleMonitor.exe"
            HelperAutoLaunchMonitorApplication = False
            HelperAboveNormalThreadPriority = False
            HelperCommandHistoryLimit = 50
            HelperShowClientConnects = True
            HelperShowClientDisconnects = False
            HelperMissedEventThreshold = 5
            HelperIncludeRestartSchedule = True
            CustomServiceStatus = AddressOf GetServiceStatus

            ' OnStart service method will be called immediately, so we must be ready for it...
            ServiceEventTimer = New Timers.Timer

            With ServiceEventTimer
                .AutoReset = False
                .Interval = 10
                .Enabled = False
            End With

        End Sub

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

            If Not DesignMode Then
                ScheduledProcessTimer = New Timers.Timer
                IntervalProcessTimer = New Timers.Timer

                With ScheduledProcessTimer
                    .AutoReset = True
                    .Interval = 60000
                    .Enabled = True
                End With

                With IntervalProcessTimer
                    .AutoReset = True
                    .Interval = 600000
                    .Enabled = False
                End With

                ' Get application path if path wasn't passed in as command line parameter
                Dim strAppPath As String = Trim(Command())

                If Len(strAppPath) = 0 Then
                    strAppPath = GetApplicationPath()
                Else
                    strAppPath = JustPath(strAppPath)
                End If

                AppPath = strAppPath
            End If

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            If Not DesignMode Then
                If HelperParentService Is Nothing Then Throw New InvalidOperationException("Parent service must be specified before ServiceHelper can be initialized - please check ""ParentService"" property!")

                ' Define service configuration variables that can be manipulated post compile
                Variables.Create(HelperVariablePrefix & ".ShutdownTimestamp", Today(), VariableType.Date, "Timestamp of when " & HelperParentService.ServiceName & " " & HelperVariablePrefix & " was shutdown")

                ServiceController = New ServiceControlHelper(Me)
                ServiceComponents = New ServiceComponents
                CommandHistory = New CommandHistoryCollection(Me, HelperCommandHistoryLimit)

                If Not HelperRemotingServer Is Nothing Then
                    Dim intRestartDelay As Integer

                    intRestartDelay = 31 - DateDiff(DateInterval.Second, Variables(HelperVariablePrefix & ".ShutdownTimestamp"), Now())
                    If intRestartDelay < 1 Or intRestartDelay > 30 Then intRestartDelay = 0

                    ' Establish remoting server if provided
                    HelperRemotingServer.Start(intRestartDelay)
                End If
            End If

        End Sub

        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)

            If Not ServiceComponents Is Nothing Then
                With ServiceComponents
                    SyncLock .SyncRoot
                        Dim scItem As IServiceComponent

                        For Each scItem In ServiceComponents
                            Try
                                scItem.Dispose()
                            Catch ex As Exception
                                SendServiceMessage(vbCrLf & "[ERROR] An exception occurred in service component [" & scItem.Name & "] ""Dispose"" implementation: " & ex.Message & vbCrLf & vbCrLf, False)
                            End Try
                        Next

                        .Clear()
                    End SyncLock
                End With
            End If

            HelperRemotingServer = Nothing
            ServiceController = Nothing
            ServiceComponents = Nothing
            CommandHistory = Nothing
            ScheduledProcessTimer = Nothing
            IntervalProcessTimer = Nothing
            ServiceEventTimer = Nothing

        End Sub

        ' Instance variables available to component property browser
        <Browsable(True), Category("Service Helper"), Description("Hosting service process.")> _
        Public Property ParentService() As System.ServiceProcess.ServiceBase
            Get
                Return HelperParentService
            End Get
            Set(ByVal Value As System.ServiceProcess.ServiceBase)
                HelperParentService = Value

                If DesignMode Then
                    If Not HelperParentService Is Nothing And Not HelperRemotingServer Is Nothing Then
                        HelperRemotingServer.URI = HelperParentService.ServiceName
                    End If
                End If
            End Set
        End Property

        <Browsable(True), Category("Service Helper"), Description("Override this value if more than one service will be sharing the same config file."), DefaultValue("Service")> _
        Public Property ApplicationVariablePrefix() As String
            Get
                Return HelperVariablePrefix
            End Get
            Set(ByVal Value As String)
                If Len(Value) = 0 Then Value = "Service"
                HelperVariablePrefix = Value
            End Set
        End Property

        <Browsable(True), Category("Service Helper"), Description("Define the associated monitoring application for the service.  If no path is specified, the app will be launched from the same folder as the service.  This application must be capable of handling service stop and restart requests."), DefaultValue("ConsoleMonitor.exe")> _
        Public Property MonitorApplication() As String
            Get
                Return HelperMonitorApplication
            End Get
            Set(ByVal Value As String)
                HelperMonitorApplication = Value
            End Set
        End Property

        <Browsable(True), Category("Service Helper"), Description("Set to True to automatically launch associated monitoring application when service starts.  Note that unless the service has been configured to interact with the desktop, the application launched from the service won't be visible."), DefaultValue(False)> _
        Public Property AutoLaunchMonitorApplication() As Boolean
            Get
                Return HelperAutoLaunchMonitorApplication
            End Get
            Set(ByVal Value As Boolean)
                HelperAutoLaunchMonitorApplication = Value
            End Set
        End Property

        <Browsable(True), Category("Service Helper"), Description("Set to True to use an above-normal thread priority when executing service processes."), DefaultValue(False)> _
        Public Property AboveNormalThreadPriority() As Boolean
            Get
                Return HelperAboveNormalThreadPriority
            End Get
            Set(ByVal Value As Boolean)
                HelperAboveNormalThreadPriority = Value
            End Set
        End Property

        <Browsable(True), Category("Service Helper"), Description("Set to False to not include a restart schedule in the config file for this service."), DefaultValue(True)> _
        Public Property IncludeServiceRestartSchedule() As Boolean
            Get
                Return HelperIncludeRestartSchedule
            End Get
            Set(ByVal Value As Boolean)
                HelperIncludeRestartSchedule = Value
            End Set
        End Property

        <Browsable(True), Category("Service Helper"), Description("Maxmimum number of items to keep in the service command history."), DefaultValue(50)> _
        Public Property CommandHistoryLimit() As Integer
            Get
                Return HelperCommandHistoryLimit
            End Get
            Set(ByVal Value As Integer)
                HelperCommandHistoryLimit = Value
            End Set
        End Property

        <Browsable(True), Category("Service Helper"), Description("Maxmimum number of minutes to consider for execution of unprocessed process that missed scheduled run time due to service pause or excessive system load."), DefaultValue(5)> _
        Public Property MissedEventThreshold() As Integer
            Get
                Return HelperMissedEventThreshold
            End Get
            Set(ByVal Value As Integer)
                If Value < 0 Then Value = 0
                If Value > 60 Then Value = 60
                HelperMissedEventThreshold = Value
            End Set
        End Property

        <Browsable(True), Category("Remote Monitoring"), Description("Specifies the remoting server to use.")> _
        Public Property RemotingServer() As IServer
            Get
                Return HelperRemotingServer
            End Get
            Set(ByVal Value As IServer)
                HelperRemotingServer = Value

                If DesignMode Then
                    If Not HelperParentService Is Nothing And Not HelperRemotingServer Is Nothing Then
                        HelperRemotingServer.URI = HelperParentService.ServiceName
                    End If
                End If
            End Set
        End Property

        <Browsable(True), Category("Remote Monitoring"), Description("Specifies whether or not to dispatch client connect messages to monitoring clients."), DefaultValue(True)> _
        Public Property ShowClientConnects() As Boolean
            Get
                Return HelperShowClientConnects
            End Get
            Set(ByVal Value As Boolean)
                HelperShowClientConnects = Value
            End Set
        End Property

        <Browsable(True), Category("Remote Monitoring"), Description("Specifies whether or not to dispatch client disconnect messages to monitoring clients."), DefaultValue(False)> _
        Public Property ShowClientDisconnects() As Boolean
            Get
                Return HelperShowClientDisconnects
            End Get
            Set(ByVal Value As Boolean)
                HelperShowClientDisconnects = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property Components() As ServiceComponents
            Get
                Return ServiceComponents
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property Processes() As ServiceProcesses
            Get
                Return ServiceProcesses
            End Get
        End Property

        <Browsable(False)> _
        Public Property CustomServiceStatusFunction() As CustomServiceStatusSignature
            Get
                Return CustomServiceStatus
            End Get
            Set(ByVal Value As CustomServiceStatusSignature)
                If Value Is Nothing Then
                    ' This function should always exist
                    CustomServiceStatus = AddressOf GetServiceStatus
                Else
                    CustomServiceStatus = Value
                End If
            End Set
        End Property

        Public Function GetProcessingState(Optional ByVal ProcessIndex As Integer = 0) As ServiceMonitorNotification

            Dim pt As ServiceProcess = ServiceProcesses(ProcessIndex)

            If pt Is Nothing Then
                Return ServiceMonitorNotification.Undetermined
            Else
                Return GetTranslatedProcessingState(pt.ProcessState)
            End If

        End Function

        Friend Function GetTranslatedProcessingState(ByVal CurrentState As ProcessState) As ServiceMonitorNotification

            ' Translate current processing state into proper client notification
            Select Case CurrentState
                Case ProcessState.Processing
                    Return ServiceMonitorNotification.ProcessStarted
                Case ProcessState.Processed
                    Return ServiceMonitorNotification.ProcessCompleted
                Case ProcessState.Aborted
                    Return ServiceMonitorNotification.ProcessCanceled
                Case Else
                    Return ServiceMonitorNotification.Undetermined
            End Select

        End Function

        ' You have to exit these service event functions very quickly or the service control manager
        ' will think your app is not responding (and hence your service will stop responding), so
        ' you can't put any real amount of code in these functions. Instead we just start an associated
        ' timer that will handle the actual event and can take as long as it needs to run.
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub OnStartHandler(ByVal args() As String)

            ServiceStarted = Now()
            CurrentEventState = ServiceState.Started
            StartArgs = args
            ServiceEventTimer.Enabled = True

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub OnPauseHandler()

            CurrentEventState = ServiceState.Paused
            ServiceEventTimer.Enabled = True

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub OnContinueHandler()

            CurrentEventState = ServiceState.Resumed
            ServiceEventTimer.Enabled = True

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub OnStopHandler()

            ' No need to overload RPC by showing all client disconnects on service shutdown...
            HelperShowClientDisconnects = False
            CurrentEventState = ServiceState.Stopped

            ' We handle OnStop as a special case as we want to to make sure any remaining
            ' client messages get flushed before we leave.  So instead, we spawn a new
            ' thread to handle orderly service shutdown just so we can force the calling
            ' thread to wait for this code to finish...
            With RunThread.ExecuteNonPublicMethod(Me, "ServiceEventProc", Nothing, Nothing)
                ' Join new thread to block calling thread until OnStop code is finished...
                With .Thread
                    .Join()
                End With
            End With

            Variables(HelperVariablePrefix & ".ShutdownTimestamp") = Now()
            Variables.Save()

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overridable Sub OnShutdownHandler()

            CurrentEventState = ServiceState.ShutDown
            ServiceEventTimer.Enabled = True

        End Sub

        ' All service state event code is here to make sure service responds to requests in a "timely" manner
        Private Sub ServiceEventProc(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ServiceEventTimer.Elapsed

            Dim x As Integer

            ' We don't want to start working before the service helper component collection has had a chance
            ' to initialize - the end init function creates the service components collection - so as soon
            ' as it's been created we'll go!
            While ServiceComponents Is Nothing
                System.Threading.Thread.CurrentThread.Sleep(100)
            End While

            ' Notify any server components of service state change
            NotifyComponentsOfServiceStateChange(CurrentEventState)

            ' Notify any remote clients of service state change
            If Not HelperRemotingServer Is Nothing Then HelperRemotingServer.SendNotification(Nothing, New ServiceStateChangedEventArgs(CurrentEventState))

            Select Case CurrentEventState
                Case ServiceState.Started
                    ' Load all schedule files
                    LoadProcessSchedule()
                    LoadRestartSchedule()

                    ' If it is defined, we'll auto-launch a service monitor
                    If HelperAutoLaunchMonitorApplication And Len(HelperMonitorApplication) > 0 Then UIProcessID = LaunchConsoleMonitor()

                    ' Handle OnStart() Event
                    Try
                        RaiseEvent OnStart(StartArgs)
                    Catch ex As Exception
                        SendServiceMessage("Exception occurred in end user OnStart event: " & ex.Message, True, EventLogEntryType.Error)
                    End Try

                    SendServiceMessage(HelperParentService.ServiceName & " has started [" & ServiceStarted & "]." & vbCrLf & vbCrLf & _
                        "Loaded " & ServiceProcesses.Count & " process run time(s).")
                Case ServiceState.Paused
                    ' Handle OnPause() Event
                    Try
                        RaiseEvent OnPause()
                    Catch ex As Exception
                        SendServiceMessage("Exception occurred in end user OnPause event: " & ex.Message, True, EventLogEntryType.Error)
                    End Try

                    ScheduledProcessTimer.Enabled = False
                    IntervalProcessTimer.Enabled = False

                    ' Suspend all threads when service is paused
                    If Not ServiceProcesses Is Nothing Then
                        For x = 0 To ServiceProcesses.Count - 1
                            ' Get current process information
                            ServiceProcesses(x).SuspendThread()
                        Next
                    End If
                Case ServiceState.Resumed
                    ' Handle OnContinue() Event
                    Try
                        RaiseEvent OnContinue()
                    Catch ex As Exception
                        SendServiceMessage("Exception occurred in end user OnContinue event: " & ex.Message, True, EventLogEntryType.Error)
                    End Try

                    ' Resume all threads when service is continued
                    If Not ServiceProcesses Is Nothing Then
                        For x = 0 To ServiceProcesses.Count - 1
                            ' Get current process information
                            ServiceProcesses(x).ResumeThread()
                        Next
                    End If

                    ' Resume scheduled events
                    ScheduledProcessTimer.Enabled = True
                    IntervalProcessTimer.Enabled = ServiceHasIntervalProcess
                Case ServiceState.Stopped
                    ' Handle OnStop() Event
                    Try
                        RaiseEvent OnStop()
                    Catch ex As Exception
                        SendServiceMessage("Exception occurred in end user OnStop event: " & ex.Message, True, EventLogEntryType.Error)
                    End Try

                    ' Make sure to abort all process threads when service is stopped
                    AbortProcess()

                    SendServiceMessage(HelperParentService.ServiceName & " has stopped [" & Now() & "]." & vbCrLf & vbCrLf & _
                        "Total service run time: " & SecondsToText(DateDiff(DateInterval.Second, ServiceStarted, Now())) & vbCrLf)

                    If UIProcessID > 0 Then
                        Try
                            ' Terminate any associated UI process
                            Process.GetProcessById(UIProcessID).Kill()
                        Catch ex As Exception
                            ' We're not going to crash service terminatation just because we couldn't stop UI
                        End Try
                    End If

                    ' Shutdown remoting server - this will flush any remaining notifications out of the message queue
                    If Not HelperRemotingServer Is Nothing Then
                        HelperRemotingServer.Shutdown()
                        HelperRemotingServer = Nothing
                    End If
                Case ServiceState.ShutDown
                    ' Handle OnShutDown() Event
                    Try
                        RaiseEvent OnShutdown()
                    Catch ex As Exception
                        SendServiceMessage("Exception occurred in end user OnShutdown event: " & ex.Message, True, EventLogEntryType.Error)
                    End Try

                    SendServiceMessage(HelperParentService.ServiceName & " stop requested due to system shutdown.")
            End Select

        End Sub

        Private Sub CheckForScheduledProcess(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ScheduledProcessTimer.Elapsed

            Static LastCheck As Date
            Dim CurrentTime As DateTime = TimeOfDay()
            Dim CheckScheduleTime As Boolean
            Dim x As Integer

            ' If it's a new day, reset all the scheduled process states
            If LastCheck.Date.CompareTo(Today()) <> 0 Then
                If Not ServiceProcesses Is Nothing Then
                    For Each Process As ServiceProcess In ServiceProcesses
                        With Process
                            If .Scheduled Then .ProcessState = ProcessState.Unprocessed
                        End With
                    Next
                End If
            End If

            LastCheck = Now()

            ' Loop through service process collection to see if we should execute our process now
            If Not ServiceProcesses Is Nothing Then
                For x = 0 To ServiceProcesses.Count - 1
                    With ServiceProcesses(x)
                        Select Case .ProcessSchedule
                            Case ScheduleType.Daily
                                CheckScheduleTime = True
                            Case ScheduleType.Weekly
                                CheckScheduleTime = (Array.BinarySearch(.ProcessDays, Today.DayOfWeek) >= 0)
                            Case ScheduleType.Monthly
                                CheckScheduleTime = (Array.BinarySearch(.ProcessDays, Today.Day) >= 0)
                            Case ScheduleType.Yearly
                                CheckScheduleTime = (Array.BinarySearch(.ProcessMonths, Today.Month) >= 0 AndAlso Array.BinarySearch(.ProcessDays, Today.Day) >= 0)
                            Case Else
                                CheckScheduleTime = False
                        End Select

                        If CheckScheduleTime Then
                            ' Get current process information
                            If CurrentTime.Hour = .ProcessTime.Hours AndAlso CurrentTime.Minute = .ProcessTime.Minutes Then
                                ' Only process this if it's not already been processed
                                If .ProcessState = ProcessState.Unprocessed Then
                                    .StartThread(New ThreadStart(AddressOf MainThreadProc), x)
                                End If
                            ElseIf .ProcessState = ProcessState.Unprocessed AndAlso (CurrentTime.Hour = .ProcessTime.Hours AndAlso CurrentTime.Minute > .ProcessTime.Minutes AndAlso CurrentTime.Minute - .ProcessTime.Minutes <= HelperMissedEventThreshold) Then
                                ' Also pick up possible delayed events that timer may have missed (but only within specified threshold)
                                .StartThread(New ThreadStart(AddressOf MainThreadProc), x)
                            End If
                        End If
                    End With
                Next
            End If

            ' Loop through scheduled restart collection to see if we should restart the service now
            If Not ScheduledRestarts Is Nothing Then
                For Each ts As TimeSpan In ScheduledRestarts
                    ' Make sure we didn't just already restart the service...
                    If Not (ServiceStarted.Hour = ts.Hours AndAlso ServiceStarted.Minute = ts.Minutes AndAlso ServiceStarted.Date.CompareTo(Today()) = 0) Then
                        If CurrentTime.Hour = ts.Hours AndAlso CurrentTime.Minute = ts.Minutes Then
                            ScheduledProcessTimer.Enabled = False
                            RestartService()
                            Exit Sub
                        End If
                    End If
                Next
            End If

            ' We give any UI's a little feedback to let them know the service is still alive every five
            ' minutes or so - this also ensures that the remote client leases get renewed as well if needed
            If CurrentTime.Minute Mod 5 = 0 Then SendServiceMessage(HelperVariablePrefix & " ping [" & Now() & "]" & vbCrLf & vbCrLf & "    " & GetProcessState() & vbCrLf, False)

        End Sub

        Public Overridable Sub ExecuteOnDemandProcess(ByVal Client As LocalClient, ByVal UserData As Object)

            ExecuteUnscheduledProcess(ScheduleType.Unscheduled, Client, UserData)

        End Sub

        Private Sub IntervalProcessTimer_Elapsed(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs) Handles IntervalProcessTimer.Elapsed

            ExecuteUnscheduledProcess(ScheduleType.Intervaled, Nothing, Nothing)

        End Sub

        Private Sub ExecuteUnscheduledProcess(ByVal ProcessSchedule As ScheduleType, ByVal Client As LocalClient, ByVal UserData As Object)

            Dim Process As ServiceProcess

            If ProcessSchedule = ScheduleType.Intervaled Then
                ' Create a new intervaled process
                Process = New ServiceProcess(Me, True, IntervalProcessTimer.Interval)
            Else
                ' Create a new on demand process
                Process = New ServiceProcess(Me, False, 0)
            End If

            Process.Client = Client
            Process.UserData = UserData

            SyncLock ServiceProcesses.SyncRoot
                ServiceProcesses.Add(Process)
                Process.StartThread(New ThreadStart(AddressOf MainThreadProc), ServiceProcesses.Count - 1)
            End SyncLock

        End Sub

        Public Overridable Sub AbortProcess()

            Dim x As Integer

            If Not ServiceProcesses Is Nothing Then
                For x = 0 To ServiceProcesses.Count - 1
                    ServiceProcesses(x).AbortThread()
                Next
            End If

        End Sub

        Protected Overridable Sub MainThreadProc()

            Dim ThreadStartTime As Date = Now()
            Dim ThreadStopTime As Date
            Dim ThreadSuspended As Boolean = True
            Dim ThreadID As Integer
            Dim Process As ServiceProcess
            Dim x As Integer

            Try
                ' Get current process information
                ThreadID = CInt(Thread.CurrentThread.Name)
                Process = ServiceProcesses(ThreadID)

                ' We wait until all other threads are finished processing before allowing
                ' another to start - this could happen if a scheduled process didn't complete
                ' before another was scheduled to begin
                While ThreadSuspended
                    ThreadSuspended = False
                    For x = 0 To ServiceProcesses.Count - 1
                        If x <> ThreadID Then
                            If ServiceProcesses(x).ProcessState = ProcessState.Processing Then
                                ' We'll hang out for a minute and try again
                                ThreadSuspended = True
                                Process.Sleep(60)
                                Exit For
                            End If
                        End If
                    Next
                End While

                ' Set thread state to processing
                Process.ProcessState = ProcessState.Processing

                SendServiceMessage("Starting " & [Enum].GetName(GetType(ScheduleType), Process.ProcessSchedule) & " [" & Process.ProcessTime.ToString() & "] " & LCase(HelperVariablePrefix) & " process [" & ThreadStartTime & "]", Process.Scheduled)

                ' Service class handling this event will do "actual work" of this service
                RaiseEvent ExecuteServiceProcess(Process.ProcessSchedule, Process.ProcessTime, Process.Client, Process.UserData)

                ThreadStopTime = Now()
                SendServiceMessage("Finished " & [Enum].GetName(GetType(ScheduleType), Process.ProcessSchedule) & " [" & Process.ProcessTime.ToString() & "] " & LCase(HelperVariablePrefix) & " process [" & ThreadStopTime & "]." & vbCrLf & vbCrLf & _
                    "Total process time: " & SecondsToText(DateDiff(DateInterval.Second, ThreadStartTime, ThreadStopTime)), Process.Scheduled)

                ' Set the processed state to complete
                Process.ProcessState = ProcessState.Processed
            Catch ex As Exception
                ThreadStopTime = Now()
                SendServiceMessage("ERROR: Aborted " & [Enum].GetName(GetType(ScheduleType), Process.ProcessSchedule) & " [" & Process.ProcessTime.ToString() & "] " & LCase(HelperVariablePrefix) & " process [" & ThreadStopTime & "].  Total process time: " & SecondsToText(DateDiff(DateInterval.Second, ThreadStartTime, ThreadStopTime)) & vbCrLf & vbCrLf & _
                    "Thread aborted by exception: " & ex.Message(), True, EventLogEntryType.Error)

                ' Set the processed state to aborted
                Process.ProcessState = ProcessState.Aborted
            End Try

            ' End thread
            Process.EndThread()

            ' If this was a non-scheduled thread, we remove it from the collection
            If Not Process.Scheduled Then ServiceProcesses.Remove(Process)

        End Sub

        Public Overridable Sub LoadProcessSchedule()

            Try
                Dim FoundSchedule As Boolean
                Dim BadTimeSpan As Boolean = False
                Dim TimeSpan As String
                Dim ScheduledTime As TimeSpan
                Dim CPos As Integer

                If Not ServiceProcesses Is Nothing Then
                    ' We abort all running threads when reloading the service schedule
                    For Each Process As ServiceProcess In ServiceProcesses
                        With Process
                            If .ProcessState = ProcessState.Processing Then
                                SendServiceMessage("WARNING: The ""Reload Process Schedule"" request has forced the [" & .ProcessTime.ToString() & "] " & [Enum].GetName(GetType(ScheduleType), .ProcessSchedule) & " " & LCase(HelperVariablePrefix) & " process that was already in progress to be aborted [" & Now() & "]." & vbCrLf, True, EventLogEntryType.Warning)
                            End If
                            .AbortThread()
                        End With
                    Next
                End If

                ' Create a new collection of process times
                ServiceProcesses = New ServiceProcesses

                ' Note: we loop through each node instead of using xpath so that we find items in a case-insensitive manner...
                For Each Node As XmlNode In Variables.xmlDoc.DocumentElement.ChildNodes
                    ' We are only interested in the "ServiceProcessSchedules" node (we loop through so we can do case-insensitive comparisons)
                    If StrComp(Node.Name, HelperVariablePrefix & "ProcessSchedules", CompareMethod.Text) = 0 Then
                        FoundSchedule = True

                        ' Load any specified process interval
                        Try
                            IntervalProcessTimer.Interval = CInt(Attribute(Node, "Interval")) * 1000
                            IntervalProcessTimer.Enabled = (IntervalProcessTimer.Interval > 0)
                        Catch
                            IntervalProcessTimer.Interval = 60000
                            IntervalProcessTimer.Enabled = False
                        End Try

                        For Each NodeChild As XmlNode In Node.ChildNodes
                            ' We are only interested in the "ScheduledEvent" nodes
                            If StrComp(NodeChild.Name, "ScheduledEvent", CompareMethod.Text) = 0 Then
                                ' Attempt to parse out HH:MM formatted time span
                                TimeSpan = Attribute(NodeChild, "TimeSpan")
                                CPos = InStr(TimeSpan, ":")
                                BadTimeSpan = False

                                If CPos > 0 Then
                                    Try
                                        ScheduledTime = New TimeSpan(CInt(Trim(Left(TimeSpan, CPos - 1))), CInt(Trim(Mid(TimeSpan, CPos + 1))), 0)
                                    Catch
                                        BadTimeSpan = True
                                    End Try
                                End If

                                If Not BadTimeSpan Then
                                    ' If we have a good time span, create a new scheduled process
                                    Select Case LCase(Trim(Attribute(NodeChild, "Type")))
                                        Case "daily"
                                            ' Add new scheduled daily event
                                            ServiceProcesses.Add(New ServiceProcess(Me, ScheduledTime))
                                        Case "weekly"
                                            ' Add new scheduled weekly event
                                            Dim Days As String() = Attribute(NodeChild, "WeekDays").Split(","c, ";"c)
                                            Dim WeekDays As DayOfWeek() = Array.CreateInstance(GetType(DayOfWeek), Days.Length)

                                            For x As Integer = 0 To Days.Length - 1
                                                WeekDays(x) = CInt(Days(x))
                                            Next

                                            ServiceProcesses.Add(New ServiceProcess(Me, WeekDays, ScheduledTime))
                                        Case "monthly"
                                            ' Add new scheduled monthly event
                                            Dim Days As String() = Attribute(NodeChild, "MonthDays").Split(","c, ";"c)
                                            Dim MonthDays As Integer() = Array.CreateInstance(GetType(Integer), Days.Length)

                                            For x As Integer = 0 To Days.Length - 1
                                                MonthDays(x) = CInt(Days(x))
                                            Next

                                            ServiceProcesses.Add(New ServiceProcess(Me, MonthDays, ScheduledTime))
                                        Case "yearly"
                                            ' Add new scheduled yearly event
                                            Dim Months As String() = Attribute(NodeChild, "Months").Split(","c, ";"c)
                                            Dim YearlyMonths As Integer() = Array.CreateInstance(GetType(Integer), Months.Length)
                                            Dim Days As String() = Attribute(NodeChild, "MonthDays").Split(","c, ";"c)
                                            Dim MonthDays As Integer() = Array.CreateInstance(GetType(Integer), Days.Length)

                                            For x As Integer = 0 To Months.Length - 1
                                                YearlyMonths(x) = CInt(Months(x))
                                            Next

                                            For x As Integer = 0 To Days.Length - 1
                                                MonthDays(x) = CInt(Days(x))
                                            Next

                                            ServiceProcesses.Add(New ServiceProcess(Me, YearlyMonths, MonthDays, ScheduledTime))
                                        Case Else
                                            SendServiceMessage("WARNING: Unrecognized schedule type """ & Attribute(NodeChild, "Type") & """ in " & HelperVariablePrefix & " processing schedule ignored.", False)
                                    End Select
                                End If
                            End If
                        Next
                    End If
                Next

                If Not FoundSchedule Then
                    Dim ScheduleComment As New StringBuilder

                    ' Generate comment for scheduling
                    ScheduleComment.Append(vbCrLf & "    Define the times of day the " & LCase(HelperVariablePrefix) & " process should run below.  This information is only processed when" & vbCrLf)
                    ScheduleComment.Append("    the service starts or when a ""ReloadProcessSchedule"" notification is sent.  To run the process on a" & vbCrLf)
                    ScheduleComment.Append("    specific interval, add an ""Interval"" attribute to the """ & HelperVariablePrefix & "ProcessSchedules"" node.  For example, the" & vbCrLf)
                    ScheduleComment.Append("    following would run every 10 minutes: <" & HelperVariablePrefix & "ProcessSchedules Interval=""600"" />  Currently, only one" & vbCrLf)
                    ScheduleComment.Append("    interval per service will be processed." & vbCrLf & vbCrLf)
                    ScheduleComment.Append("            Scheduling Example:" & vbCrLf & vbCrLf)
                    ScheduleComment.Append("                <" & HelperVariablePrefix & "ProcessSchedules Interval=""0"">" & vbCrLf)
                    ScheduleComment.Append("                  <ScheduledEvent Type=""Daily"" TimeSpan=""03:00""> Runs daily at 3:00 AM </ScheduledEvent>" & vbCrLf)
                    ScheduleComment.Append("                  <ScheduledEvent Type=""Weekly"" WeekDays=""0,2,4"" TimeSpan=""5:00""> Runs weekly on Sunday, Tuesday, and Thrusday at 5:00 AM </ScheduledEvent>" & vbCrLf)
                    ScheduleComment.Append("                  <ScheduledEvent Type=""Monthly"" MonthDays=""1,15"" TimeSpan=""15:00""> Runs monthly on the 1st and the 15th at 3:00 PM </ScheduledEvent>" & vbCrLf)
                    ScheduleComment.Append("                  <ScheduledEvent Type=""Yearly"" Months=""3,8,12"" MonthDays=""1,2"" TimeSpan=""22:00""> Runs yearly during March, August and December on the 1st and 2nd day of the month at 10:00 PM </ScheduledEvent>" & vbCrLf)
                    ScheduleComment.Append("                </" & HelperVariablePrefix & "ProcessSchedules>" & vbCrLf & "    ")

                    ' Add default service process schedule node
                    Attribute(CreateCustomConfigSection(HelperVariablePrefix & "ProcessSchedules", ScheduleComment.ToString(), Variables.xmlDoc), "Interval") = "0"
                    Variables.Save()

                    ' Log warning
                    SendServiceMessage("WARNING: """ & HelperVariablePrefix & "ProcessSchedules"" node did not exist in config file.  A default entry was created.", True, EventLogEntryType.Warning)
                End If

                ServiceHasIntervalProcess = IntervalProcessTimer.Enabled
            Catch ex As Exception
                SendServiceMessage("ERROR: Failed while loading process schedule from """ & HelperVariablePrefix & "ProcessSchedules"" node in config file due to exception: " & ex.Message, True, EventLogEntryType.Error)
            End Try

        End Sub

        Public Overridable Sub LoadRestartSchedule()

            ' Create a new collection of scheduled restart times (we create the collection whether it is used or not)
            ScheduledRestarts = New ArrayList

            ' Load scheduled restart times from the application config file
            If HelperIncludeRestartSchedule Then
                Try
                    Dim FoundSchedule As Boolean
                    Dim BadTimeSpan As Boolean = False
                    Dim TimeSpan As String
                    Dim ScheduledTime As TimeSpan
                    Dim CPos As Integer

                    ' Note: we loop through each node instead of using xpath so that we find items in a case-insensitive manner...
                    For Each Node As XmlNode In Variables.xmlDoc.DocumentElement.ChildNodes
                        ' We are only interested in the "ServiceRestartSchedules" node (we loop through so we can do case-insensitive comparisons)
                        If StrComp(Node.Name, HelperVariablePrefix & "RestartSchedules", CompareMethod.Text) = 0 Then
                            FoundSchedule = True
                            For Each NodeChild As XmlNode In Node.ChildNodes
                                ' We are only interested in the "ScheduledEvent" nodes
                                If StrComp(NodeChild.Name, "ScheduledEvent", CompareMethod.Text) = 0 Then
                                    ' Add new scheduled daily event
                                    If StrComp(Attribute(NodeChild, "Type"), "Daily", CompareMethod.Text) = 0 Then
                                        TimeSpan = Attribute(NodeChild, "TimeSpan")
                                        CPos = InStr(TimeSpan, ":")
                                        BadTimeSpan = False

                                        If CPos > 0 Then
                                            Try
                                                ' Attempt to parse out HH:MM formatted time span
                                                ScheduledTime = New TimeSpan(CInt(Trim(Left(TimeSpan, CPos - 1))), CInt(Trim(Mid(TimeSpan, CPos + 1))), 0)
                                            Catch
                                                BadTimeSpan = True
                                            End Try

                                            If Not BadTimeSpan Then
                                                ' If we have a good time span, create a new scheduled restart
                                                ScheduledRestarts.Add(ScheduledTime)
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Next

                    If Not FoundSchedule Then
                        Dim ScheduleComment As New StringBuilder

                        ' Generate comment for scheduling
                        ScheduleComment.Append(vbCrLf & "    Define the times of day the service should restart itself below, if any.  This information is only processed" & vbCrLf)
                        ScheduleComment.Append("    when the service starts or when a ""ReloadProcessSchedule"" notification is sent." & vbCrLf)
                        ScheduleComment.Append("    Note that only the ""Daily"" schedule type is supported for restart schedules." & vbCrLf & vbCrLf)
                        ScheduleComment.Append("            Scheduling Example:" & vbCrLf & vbCrLf)
                        ScheduleComment.Append("                <" & HelperVariablePrefix & "RestartSchedules>" & vbCrLf)
                        ScheduleComment.Append("                  <ScheduledEvent Type=""Daily"" TimeSpan=""00:00""> Restart service every night at midnight </ScheduledEvent>" & vbCrLf)
                        ScheduleComment.Append("                </" & HelperVariablePrefix & "RestartSchedules>" & vbCrLf & "    ")

                        ' Add default service restart schedule node
                        CreateCustomConfigSection(HelperVariablePrefix & "RestartSchedules", ScheduleComment.ToString(), Variables.xmlDoc)
                        Variables.Save()

                        ' Log warning
                        SendServiceMessage("WARNING: """ & HelperVariablePrefix & "RestartSchedules"" node did not exist in config file.  A default entry was created.", True, EventLogEntryType.Warning)
                    End If
                Catch ex As Exception
                    SendServiceMessage("ERROR: Failed while loading restart schedule from """ & HelperVariablePrefix & "RestartSchedules"" node in config file due to exception: " & ex.Message, True, EventLogEntryType.Error)
                End Try
            End If

        End Sub

        Public Overridable Function PauseService() As Boolean

            Return ServiceController.PauseService()

        End Function

        Public Overridable Function ResumeService() As Boolean

            Return ServiceController.ResumeService()

        End Function

        Public Overridable Function StopService() As Boolean

            Return ServiceController.StopService()

        End Function

        Public Overridable Function RestartService() As Boolean

            Return ServiceController.RestartService()

        End Function

        ' The service can use this function to log information to the event log and end status information to its monitoring clients
        Public Overridable Sub SendServiceMessage(ByVal Message As String, Optional ByVal LogMessage As Boolean = True, Optional ByVal EntryType As EventLogEntryType = EventLogEntryType.Information)

            If Not HelperRemotingServer Is Nothing Then HelperRemotingServer.SendNotification(Nothing, New ServiceMessageEventArgs(Message, LogMessage))
            If LogMessage Then HelperParentService.EventLog.WriteEntry(Message, EntryType)

        End Sub

        ' The service can use this function to notify any clients of work progress
        Public Overridable Sub SendServiceProgress(ByVal BytesCompleted As Long, ByVal BytesTotal As Long)

            If Not HelperRemotingServer Is Nothing Then HelperRemotingServer.SendNotification(Nothing, New ServiceProgressEventArgs(BytesCompleted, BytesTotal))

        End Sub

        ' The service can use this function to send a custom notification to its clients
        Public Overridable Sub SendServiceNotification(ByVal sender As Object, ByVal e As EventArgs)

            If Not HelperRemotingServer Is Nothing Then HelperRemotingServer.SendNotification(sender, e)

        End Sub

        Public Overridable Sub SendPrivateServiceMessage(ByVal ID As Guid, ByVal Message As String)

            If Not HelperRemotingServer Is Nothing Then HelperRemotingServer.SendPrivateNotification(ID, Nothing, New ServiceMessageEventArgs(Message, False))

        End Sub

        Public Overridable Sub SendPrivateServiceNotification(ByVal ID As Guid, ByVal sender As Object, ByVal e As EventArgs)

            If Not HelperRemotingServer Is Nothing Then HelperRemotingServer.SendPrivateNotification(ID, sender, e)

        End Sub

        ' Return a string that lists all of the connected clients
        Public Overridable Function GetClientList() As String

            If HelperRemotingServer Is Nothing Then
                Return "Client list unavailable."
            Else
                Return HelperRemotingServer.GetFullClientList()
            End If

        End Function

        Public Overridable Function GetCommandHistory() As String

            Return CommandHistory.GetHistoryList()

        End Function

        Public Overridable Function GetServiceStatus(ByVal StatusParameter As String) As String

            Dim ServiceStatus As New StringBuilder

            ServiceStatus.Append(vbCrLf & HelperParentService.ServiceName & " " & HelperVariablePrefix & " Status:" & vbCrLf & vbCrLf)
            ServiceStatus.Append("Service Components: " & ServiceComponents.Count & vbCrLf)
            ServiceStatus.Append("     Service State: " & [Enum].GetName(GetType(ServiceState), CurrentEventState) & vbCrLf)
            ServiceStatus.Append("          Run Time: " & SecondsToText(DateDiff(DateInterval.Second, ServiceStarted, Now())) & vbCrLf)
            ServiceStatus.Append("          Assembly: " & GetShortAssemblyName(System.Reflection.Assembly.GetExecutingAssembly) & vbCrLf)
            ServiceStatus.Append("          EXE Name: " & System.Reflection.Assembly.GetExecutingAssembly.Location & vbCrLf)
            ServiceStatus.Append("           Created: " & File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly.Location) & vbCrLf)
            ServiceStatus.Append("     Process State: " & GetProcessState())
            ServiceStatus.Append(" Process Schedules: " & vbCrLf & vbCrLf & GetProcessSchedule() & vbCrLf)

            ' Because we can't depend on end user code to return quickly, we work from a cloned
            ' service component list to keep total synclock time down to a minimum
            With ServiceComponents.Clone()
                For Each Component As IServiceComponent In .This
                    With Component
                        Try
                            ServiceStatus.Append(vbCrLf & .Name & " Component Status:" & vbCrLf & vbCrLf)
                            ServiceStatus.Append(.Status & vbCrLf)
                        Catch ex As Exception
                            SendServiceMessage(vbCrLf & "[ERROR] An exception occurred in service component [" & .Name & "] ""Status"" implementation: " & ex.Message & vbCrLf & vbCrLf, False)
                        End Try
                    End With
                Next
            End With

            Return ServiceStatus.ToString()

        End Function

        Protected Overridable Sub NotifyComponentsOfServiceStateChange(ByVal NewState As ServiceState)

            ' Because we can't depend on end user code to return quickly, we work from a cloned
            ' service component list to keep total synclock time down to a minimum
            With ServiceComponents.Clone()
                For Each Component As IServiceComponent In .This
                    Try
                        Component.ServiceStateChanged(NewState)
                    Catch ex As Exception
                        SendServiceMessage(vbCrLf & "[ERROR] An exception occurred in service component [" & Component.Name & "] ""ServiceStateChanged"" implementation: " & ex.Message & vbCrLf & vbCrLf, False)
                    End Try
                Next
            End With

        End Sub

        Protected Friend Overridable Sub NotifyComponentsOfProcessStateChange(ByVal NewState As ProcessState)

            ' Because we can't depend on end user code to return quickly, we work from a cloned
            ' service component list to keep total synclock time down to a minimum
            With ServiceComponents.Clone()
                For Each Component As IServiceComponent In .This
                    Try
                        Component.ProcessStateChanged(NewState)
                    Catch ex As Exception
                        SendServiceMessage(vbCrLf & "[ERROR] An exception occurred in service component [" & Component.Name & "] ""ProcessStateChanged"" implementation: " & ex.Message & vbCrLf & vbCrLf, False)
                    End Try
                Next
            End With

        End Sub

        Public Overridable Function GetPingResponse() As String

            Return vbCrLf & "Reply from """ & HelperParentService.ServiceName & " " & HelperVariablePrefix & """: Pong [" & Now() & "]" & vbCrLf

        End Function

        Public Overridable Function GetProcessState() As String

            Dim Remaining As Integer = 0
            Dim Scheduled As Integer = 0
            Dim x As Integer

            For x = 0 To ServiceProcesses.Count - 1
                With ServiceProcesses(x)
                    If .Scheduled Then
                        Scheduled += 1
                        If .ProcessState = ProcessState.Unprocessed Then Remaining += 1
                    End If
                End With
            Next

            Return Remaining & " of " & Scheduled & " scheduled processes remain unprocessed." & vbCrLf

        End Function

        Public Overridable Function GetProcessSchedule() As String

            Dim ProcessSchedule As New StringBuilder
            Dim i As Integer

            If IntervalProcessTimer.Enabled Then
                ProcessSchedule.Append(vbCrLf & "Processing occurs on an interval every " & SecondsToText(IntervalProcessTimer.Interval \ 1000) & vbCrLf)
            End If

            If ServiceProcesses.Count > 0 Then
                If ProcessSchedule.Length = 0 Then
                    ProcessSchedule.Append(vbCrLf & "Processing occurs on the following schedule:" & vbCrLf & vbCrLf)
                Else
                    ProcessSchedule.Append(vbCrLf & "Additionally, processing occurs on the following schedule:" & vbCrLf & vbCrLf)
                End If

                For Each Process As ServiceProcess In ServiceProcesses
                    With Process
                        Select Case .ProcessSchedule
                            Case ScheduleType.Daily
                                ProcessSchedule.Append("    Daily at ")
                            Case ScheduleType.Weekly
                                ProcessSchedule.Append("    Weekly on ")
                                i = 0
                                For Each WeekDay As DayOfWeek In .ProcessDays
                                    If i > 0 Then ProcessSchedule.Append(", ")
                                    ProcessSchedule.Append(Left([Enum].GetName(GetType(DayOfWeek), WeekDay), 3))
                                    i += 1
                                Next
                                ProcessSchedule.Append(" at ")
                            Case ScheduleType.Monthly
                                ProcessSchedule.Append("    Monthly on day" & IIf(.ProcessDays.Length = 1, " ", "s "))
                                i = 0
                                For Each MonthDay As Integer In .ProcessDays
                                    If i > 0 Then ProcessSchedule.Append(", ")
                                    ProcessSchedule.Append(MonthDay)
                                    i += 1
                                Next
                                ProcessSchedule.Append(" at ")
                            Case ScheduleType.Yearly
                                ProcessSchedule.Append("    Yearly during month" & IIf(.ProcessMonths.Length = 1, " ", "s "))
                                i = 0
                                For Each Month As Integer In .ProcessMonths
                                    If i > 0 Then ProcessSchedule.Append(", ")
                                    ProcessSchedule.Append(GetShortMonth(Month))
                                    i += 1
                                Next
                                ProcessSchedule.Append(" on day" & IIf(.ProcessDays.Length = 1, " ", "s "))
                                i = 0
                                For Each MonthDay As Integer In .ProcessDays
                                    If i > 0 Then ProcessSchedule.Append(", ")
                                    ProcessSchedule.Append(MonthDay)
                                    i += 1
                                Next
                                ProcessSchedule.Append(" at ")
                        End Select
                        ProcessSchedule.Append(PadLeft(.ProcessTime.Hours, 2, "0"c) & ":" & PadLeft(.ProcessTime.Minutes, 2, "0"c) & " [" & [Enum].GetName(GetType(ProcessState), .ProcessState) & "]" & vbCrLf)
                    End With
                Next
                ProcessSchedule.Append(vbCrLf)
            End If

            If ProcessSchedule.Length = 0 Then ProcessSchedule.Append("No " & LCase(HelperVariablePrefix) & " process schedule was loaded.  If desired, add a scheduled process event to the config file.")

            If HelperIncludeRestartSchedule Then
                ProcessSchedule.Append(vbCrLf & vbCrLf)

                If ScheduledRestarts.Count > 0 Then
                    ProcessSchedule.Append(HelperVariablePrefix & " is scheduled to auto-restart on the following daily schedule:" & vbCrLf & vbCrLf)

                    For Each ts As TimeSpan In ScheduledRestarts
                        ProcessSchedule.Append("    " & PadLeft(ts.Hours, 2, "0"c) & ":" & PadLeft(ts.Minutes, 2, "0"c))
                    Next
                Else
                    ProcessSchedule.Append(HelperVariablePrefix & " is not scheduled to perform a daily auto-restart.  If desired, add a scheduled restart event to the config file.")
                End If
            End If

            ProcessSchedule.Append(vbCrLf)

            Return ProcessSchedule.ToString()

        End Function

        Private Sub HelperRemotingServer_ClientNotification(ByVal Client As LocalClient, ByVal sender As Object, ByVal e As EventArgs) Handles HelperRemotingServer.ClientNotification

            ' Here we handle any standard notifications sent from our client monitors.  Note that clients can send any kind of 
            ' custom notfications they desire to the actual service, we only "automatically" respond to "ServiceNotfications"
            If Not e Is Nothing Then
                If TypeOf e Is ServiceNotificationEventArgs Then
                    With DirectCast(e, ServiceNotificationEventArgs)
                        ' Track service notification command history...
                        CommandHistory.Add(.Notification, Client.ID)

                        Select Case .Notification
                            Case ServiceNotification.OnDemandProcess
                                ExecuteOnDemandProcess(Client, .EventItemData)
                            Case ServiceNotification.AbortProcess
                                AbortProcess()
                            Case ServiceNotification.PauseService
                                PauseService()
                            Case ServiceNotification.ResumeService
                                ResumeService()
                            Case ServiceNotification.StopService
                                StopService()
                            Case ServiceNotification.RestartService
                                RestartService()
                            Case ServiceNotification.PingService
                                SendPrivateServiceMessage(Client.ID, GetPingResponse())
                            Case ServiceNotification.PingAllClients
                                SendServiceMessage(vbCrLf & "*** Ping All Clients Notification Received ***" & vbCrLf & GetPingResponse(), False)
                            Case ServiceNotification.GetServiceStatus
                                SendPrivateServiceMessage(Client.ID, CustomServiceStatus(.EventItemName) & vbCrLf)
                            Case ServiceNotification.GetCommandHistory
                                SendPrivateServiceMessage(Client.ID, GetCommandHistory() & vbCrLf)
                            Case ServiceNotification.GetClientList
                                SendPrivateServiceMessage(Client.ID, GetClientList() & vbCrLf)
                            Case ServiceNotification.GetProcessSchedule
                                SendPrivateServiceMessage(Client.ID, GetProcessSchedule() & vbCrLf)
                            Case ServiceNotification.ReloadProcessSchedule
                                Variables.Refresh()
                                LoadProcessSchedule()
                                LoadRestartSchedule()
                                SendServiceMessage("Loaded " & ServiceProcesses.Count & " scheduled " & LCase(HelperVariablePrefix) & " process run time(s)." & vbCrLf & vbCrLf & _
                                    IIf(HelperIncludeRestartSchedule, "Loaded " & ScheduledRestarts.Count & " scheduled " & LCase(HelperVariablePrefix) & " restart time(s)." & vbCrLf & vbCrLf, "") & _
                                    GetProcessSchedule() & vbCrLf, False)
                            Case ServiceNotification.RequestProcessState
                                SendPrivateServiceNotification(Client.ID, Nothing, New ServiceMonitorNotificationEventArgs(GetProcessingState(.EventItemData)))
                            Case ServiceNotification.DirectoryListing
                                Dim strFiles As New StringBuilder
                                Dim strFile As String

                                strFiles.Append("Directory Listing for """)
                                strFiles.Append(AppPath)
                                strFiles.Append(""": ")
                                strFiles.Append(vbCrLf)
                                strFiles.Append(vbCrLf)

                                For Each strFile In GetFileList(AppPath & "*.*")
                                    strFiles.Append(PadRight(TrimFileName(JustFileName(strFile), 37), 37))
                                    strFiles.Append(" ")
                                    strFiles.Append(File.GetLastWriteTime(strFile).ToString("MM/dd/yyyy hh:mm tt"))
                                    strFiles.Append(" ")
                                    strFiles.Append(PadLeft(GetFileLength(strFile), 15))
                                    strFiles.Append(" bytes")
                                    strFiles.Append(vbCrLf)
                                Next

                                SendPrivateServiceMessage(Client.ID, strFiles.ToString())
                            Case ServiceNotification.GetAllVariables
                                Dim de As DictionaryEntry
                                Dim var As Variable
                                Dim strVars As String

                                For Each de In Variables.Table
                                    var = DirectCast(de.Value, Variable)
                                    strVars &= "[" & var.Name & "] = """ & var.Value & """" & vbCrLf
                                Next

                                SendPrivateServiceMessage(Client.ID, "Configuration Variables for """ & HelperParentService.ServiceName & " " & HelperVariablePrefix & """: " & vbCrLf & vbCrLf & strVars)
                            Case ServiceNotification.GetVariable
                                Dim var As Variable = Variables.Table(.EventItemName)
                                SendPrivateServiceMessage(Client.ID, "Configuration Variable [" & var.Name & "] = """ & var.Value & """" & vbCrLf)
                            Case ServiceNotification.SetVariable
                                Dim var As Variable = Variables.Table(.EventItemName)
                                Dim strMessage As String = "Changed Configuration Variable [" & var.Name & "] From """ & var.Value & """ To """ & .EventItemData & """" & vbCrLf
                                Variables(.EventItemName) = .EventItemData
                                Variables.Save()
                                SendServiceMessage(strMessage, False)
                            Case ServiceNotification.KillProcess
                                Try
                                    Process.GetProcessById(CInt(.EventItemName)).Kill()
                                    SendPrivateServiceMessage(Client.ID, "Process terminated successfully." & vbCrLf)
                                Catch ex As Exception
                                    SendPrivateServiceMessage(Client.ID, "Failed to terminate process: " & ex.Message & vbCrLf)
                                End Try
                        End Select
                    End With
                End If
            End If

            RaiseEvent ClientNotification(Client, sender, e)

        End Sub

        Private Sub HelperRemotingServer_ServerEstablished(ByVal ServerID As System.Guid, ByVal TCPPort As Integer) Handles HelperRemotingServer.ServerEstablished

            SendServiceMessage(vbCrLf & "A new remoting server [" & ServerID.ToString() & "] of type [" & TypeName(HelperRemotingServer) & "] has been established to handle remote client connections on port [" & TCPPort & "]" & vbCrLf & vbCrLf, False)

        End Sub

        Private Sub HelperRemotingServer_ClientConnected(ByVal Client As LocalClient) Handles HelperRemotingServer.ClientConnected

            If HelperShowClientConnects Then SendServiceMessage(vbCrLf & "Client connected [" & Now() & "]:" & vbCrLf & Client.Description & vbCrLf & vbCrLf, False)

        End Sub

        Private Sub HelperRemotingServer_ClientDisconnected(ByVal Client As LocalClient) Handles HelperRemotingServer.ClientDisconnected

            If HelperShowClientDisconnects Then SendServiceMessage(vbCrLf & "Client disconnected [" & Now() & "]:" & vbCrLf & Client.Description & vbCrLf & vbCrLf, False)

        End Sub

        Private Sub HelperRemotingServer_UserEventHandlerException(ByVal EventName As String, ByVal ex As System.Exception) Handles HelperRemotingServer.UserEventHandlerException

            SendServiceMessage(vbCrLf & "[ERROR] An exception occurred in user event handler [" & EventName & "]:" & vbCrLf & ex.Message & vbCrLf & vbCrLf, False)

        End Sub

        Protected Overridable Function LaunchConsoleMonitor(Optional ByVal CommandLineParams As String = "", Optional ByVal WindowStyle As VB.AppWinStyle = AppWinStyle.NormalFocus) As Integer

            Dim ProcessID As Integer = -1
            Dim UIMonitorCommandLine As String

            Try
                ' If defined, launch UI host application (this could be a console app for example)
                If Len(HelperMonitorApplication) > 0 Then
                    If Len(JustFileName(HelperMonitorApplication)) = Len(HelperMonitorApplication) Then
                        ' Execute UI client application providing service path
                        UIMonitorCommandLine = AppPath & HelperMonitorApplication & CommandLineParams
                    Else
                        ' Execute UI client application as specified
                        UIMonitorCommandLine = HelperMonitorApplication & CommandLineParams
                    End If

                    ProcessID = Shell(UIMonitorCommandLine, WindowStyle, False)
                End If
            Catch ex As Exception
                ' We're not going to throw an exception just because we couldn't launch a monitor, but we will notifiy user...
                SendServiceMessage("Failed to launch console monitor """ & UIMonitorCommandLine & """ due to exception: " & ex.Message, False)
            End Try

            Return ProcessID

        End Function

    End Class

End Namespace