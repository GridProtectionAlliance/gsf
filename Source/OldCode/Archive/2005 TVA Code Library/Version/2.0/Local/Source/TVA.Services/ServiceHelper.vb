' 08-29-06

Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports System.ServiceProcess
Imports TVA.IO
Imports TVA.Common
Imports TVA.Communication
Imports TVA.Serialization
Imports TVA.Scheduling
Imports TVA.Configuration.Common

<ToolboxBitmap(GetType(ServiceHelper))> _
Public Class ServiceHelper
    Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

    Private m_service As ServiceBase
    Private m_logStatusUpdates As Boolean
    Private m_requestHistoryLimit As Integer
    Private m_lastRequest As ClientRequest
    Private m_processes As Dictionary(Of String, ServiceProcess)
    Private m_serviceComponents As List(Of IServiceComponent)
    Private m_persistsettings As Boolean
    Private m_settingsCategoryName As String
    Private m_clientInfo As Dictionary(Of Guid, ClientInfo)
    Private m_requestHistory As List(Of RequestInfo)
    Private m_startedEventHandlerList As List(Of StartedEventHandler)
    Private m_stoppedEventHandlerList As List(Of EventHandler)

    Private WithEvents m_communicationServer As ICommunicationServer

#End Region

#Region " Event Declaration "

    ''' <summary>
    ''' Occurs when the service has started.
    ''' </summary>
    ''' <remarks>This is a non-blocking event.</remarks>
    Public Custom Event Started As StartedEventHandler
        AddHandler(ByVal value As StartedEventHandler)
            m_startedEventHandlerList.Add(value)
        End AddHandler

        RemoveHandler(ByVal value As StartedEventHandler)
            m_startedEventHandlerList.Remove(value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As GenericEventArgs)
            For Each handler As StartedEventHandler In m_startedEventHandlerList
                handler.BeginInvoke(sender, e, Nothing, Nothing)
            Next
        End RaiseEvent
    End Event

    ''' <summary>
    ''' Occurs when the service has stopped.
    ''' </summary>
    ''' <remarks>This is a non-blocking event.</remarks>
    Public Custom Event Stopped As EventHandler
        AddHandler(ByVal value As EventHandler)
            m_stoppedEventHandlerList.Add(value)
        End AddHandler

        RemoveHandler(ByVal value As EventHandler)
            m_stoppedEventHandlerList.Remove(value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)
            For Each handler As EventHandler In m_stoppedEventHandlerList
                handler.BeginInvoke(sender, e, Nothing, Nothing)
            Next
        End RaiseEvent
    End Event

    ''' <summary>
    ''' Occurs when the service is paused.
    ''' </summary>
    Public Event Paused As EventHandler

    ''' <summary>
    ''' Occurs when the service is resumed.
    ''' </summary>
    Public Event Resumed As EventHandler

    ''' <summary>
    ''' Occurs when the system is being shutdowm.
    ''' </summary>
    Public Event Shutdown As EventHandler

    ''' <summary>
    ''' Occurs when a request is received from a client.
    ''' </summary>
    Public Event ReceivedClientRequest(ByVal sender As Object, ByVal e As ClientRequestEventArgs)

#End Region

#Region " Code Scope: Public "

    Public Delegate Sub StartedEventHandler(ByVal sender As Object, ByVal e As GenericEventArgs)

    ''' <summary>
    ''' Gets or sets the parent service to which the service helper belongs.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The parent service to which the service helper belongs.</returns>
    Public Property Service() As ServiceBase
        Get
            Return m_service
        End Get
        Set(ByVal value As ServiceBase)
            m_service = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the instance of TCP server used for communicating with the clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>An instance of TCP server.</returns>
    Public Property CommunicationServer() As ICommunicationServer
        Get
            Return m_communicationServer
        End Get
        Set(ByVal value As ICommunicationServer)
            m_communicationServer = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether status updates are to be logged to a text file.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if status updates are to be logged to a text file; otherwise false.</returns>
    <Category("Service Helper"), DefaultValue(GetType(Boolean), "True")> _
    Public Property LogStatusUpdates() As Boolean
        Get
            Return m_logStatusUpdates
        End Get
        Set(ByVal value As Boolean)
            m_logStatusUpdates = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the number of request entries to be kept in the history.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The number of request entries to be kept in the history.</returns>
    <Category("Service Helper"), DefaultValue(GetType(Integer), "50")> _
    Public Property RequestHistoryLimit() As Integer
        Get
            Return m_requestHistoryLimit
        End Get
        Set(ByVal value As Integer)
            If value > 0 Then
                m_requestHistoryLimit = value
            Else
                Throw New ArgumentOutOfRangeException("RequestHistoryLimit", "Value must be greater that 0.")
            End If
        End Set
    End Property

    <Browsable(False)> _
    Public ReadOnly Property LastRequest() As ClientRequest
        Get
            Return m_lastRequest
        End Get
    End Property

    <Browsable(False)> _
    Public ReadOnly Property Processes() As Dictionary(Of String, ServiceProcess)
        Get
            Return m_processes
        End Get
    End Property

    ''' <summary>
    ''' Gets a list of all the components that implement the TVA.Services.IServiceComponent interface.
    ''' </summary>
    ''' <value></value>
    ''' <returns>An instance of System.Collections.Generic.List(Of TVA.Services.IServiceComponent).</returns>
    <Browsable(False)> _
    Public ReadOnly Property ServiceComponents() As List(Of IServiceComponent)
        Get
            Return m_serviceComponents
        End Get
    End Property

    ''' <summary>
    ''' To be called when the service is starts (inside the service's OnStart method).
    ''' </summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Sub OnStart(ByVal args As String())

        If m_service IsNot Nothing Then
            m_serviceComponents.Add(ScheduleManager)
            m_serviceComponents.Add(m_communicationServer)

            If m_communicationServer IsNot Nothing Then
                m_communicationServer.Handshake = True
                m_communicationServer.HandshakePassphrase = m_service.ServiceName
            End If

            For Each component As IServiceComponent In m_serviceComponents
                If component IsNot Nothing Then component.ServiceStateChanged(ServiceState.Started)
            Next

            If m_logStatusUpdates Then LogFile.Open()

            RaiseEvent Started(Me, New GenericEventArgs(args))

            SendServiceStateChangedResponse(ServiceState.Started)
        Else
            Throw New InvalidOperationException("Service cannot be started. The Service property of ServiceHelper is not set.")
        End If

    End Sub

    ''' <summary>
    ''' To be called when the service is stopped (inside the service's OnStop method).
    ''' </summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Sub OnStop()

        SendServiceStateChangedResponse(ServiceState.Stopped)

        ' Abort any processes that are currently executing.
        For Each process As ServiceProcess In m_processes.Values
            If process IsNot Nothing Then process.Abort()
        Next

        ' Notify all of the components that the service is stopping.
        For Each component As IServiceComponent In m_serviceComponents
            If component IsNot Nothing Then component.ServiceStateChanged(ServiceState.Stopped)
        Next

        If m_logStatusUpdates Then LogFile.Close()

        RaiseEvent Stopped(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' To be called when the service is paused (inside the service's OnPause method).
    ''' </summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Sub OnPause()

        For Each component As IServiceComponent In m_serviceComponents
            If component IsNot Nothing Then component.ServiceStateChanged(ServiceState.Paused)
        Next

        RaiseEvent Paused(Me, EventArgs.Empty)

        SendServiceStateChangedResponse(ServiceState.Paused)

    End Sub

    ''' <summary>
    ''' To be called when the service is resumed (inside the service's OnContinue method).
    ''' </summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Sub OnResume()

        For Each component As IServiceComponent In m_serviceComponents
            If component IsNot Nothing Then component.ServiceStateChanged(ServiceState.Resumed)
        Next

        RaiseEvent Resumed(Me, EventArgs.Empty)

        SendServiceStateChangedResponse(ServiceState.Resumed)

    End Sub

    ''' <summary>
    ''' To be when the system is shutting down (inside the service's OnShutdown method).
    ''' </summary>
    <EditorBrowsable(EditorBrowsableState.Advanced)> _
    Public Sub OnShutdown()

        SendServiceStateChangedResponse(ServiceState.Shutdown)

        ' Abort any processes that are executing.
        For Each process As ServiceProcess In m_processes.Values
            If process IsNot Nothing Then process.Abort()
        Next

        ' Stop all of the components that implement IServiceComponent interface.
        For Each component As IServiceComponent In m_serviceComponents
            If component IsNot Nothing Then component.ServiceStateChanged(ServiceState.Shutdown)
        Next

        RaiseEvent Shutdown(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' To be called when the state of a process changes.
    ''' </summary>
    ''' <param name="processName">Name of the process whose state changed.</param>
    ''' <param name="processState">New state of the process.</param>
    Public Sub ProcessStateChanged(ByVal processName As String, ByVal processState As ProcessState)

        For Each component As IServiceComponent In m_serviceComponents
            If component IsNot Nothing Then component.ProcessStateChanged(processName, processState)
        Next

        SendProcessStateChangedResponse(processName, processState)

    End Sub

    Public Sub AddProcess(ByVal processExecutionMethod As ServiceProcess.ExecutionMethodSignature, _
            ByVal processName As String)

        AddProcess(processExecutionMethod, processName, Nothing)

    End Sub

    Public Sub AddProcess(ByVal processExecutionMethod As ServiceProcess.ExecutionMethodSignature, _
            ByVal processName As String, ByVal processParameters As Object())

        processName = processName.Trim()

        If Not m_processes.ContainsKey(processName) Then
            m_processes.Add(processName, New ServiceProcess(processExecutionMethod, processName, processParameters, Me))
        Else
            UpdateStatus(String.Format("Process ""{0}"" could not be added. Process already exists.", processName))
        End If

    End Sub

    Public Sub AddScheduledProcess(ByVal processExecutionMethod As ServiceProcess.ExecutionMethodSignature, _
            ByVal processName As String, ByVal processSchedule As String)

        AddScheduledProcess(processExecutionMethod, processName, Nothing, processSchedule)

    End Sub

    Public Sub AddScheduledProcess(ByVal processExecutionMethod As ServiceProcess.ExecutionMethodSignature, _
            ByVal processName As String, ByVal processParameters As Object(), ByVal processSchedule As String)

        AddProcess(processExecutionMethod, processName, processParameters)
        ScheduleProcess(processName, processSchedule)

    End Sub

    Public Sub ScheduleProcess(ByVal processName As String, ByVal scheduleRule As String)

        processName = processName.Trim()

        If m_processes.ContainsKey(processName) Then
            ' The specified process exists, so we'll schedule it, or update its schedule if it is acheduled already.
            Dim schedule As Schedule = Nothing
            Try
                If ScheduleManager.Schedules.TryGetValue(processName, schedule) Then
                    ' Update the process schedule if it is already exists.
                    schedule.Rule = scheduleRule
                Else
                    ' Schedule the process if it is not scheduled already.
                    ScheduleManager.Schedules.Add(processName, New Schedule(processName, scheduleRule))
                End If
            Catch ex As Exception
                UpdateStatus(ex.Message)
            End Try
        Else
            UpdateStatus(String.Format("Process ""{0}"" could not be scheduled. Process does not exist.", processName))
        End If

    End Sub

    ''' <summary>
    ''' Sends the specified response to all of the connected clients.
    ''' </summary>
    ''' <param name="response">The response to be sent to the clients.</param>
    Public Sub SendResponse(ByVal response As ServiceResponse)

        m_communicationServer.Multicast(response)

    End Sub

    ''' <summary>
    ''' Sends the specified resonse to the specified client only.
    ''' </summary>
    ''' <param name="clientID">ID of the client to whom the response is to be sent.</param>
    ''' <param name="response">The response to be sent to the client.</param>
    Public Sub SendResponse(ByVal clientID As Guid, ByVal response As ServiceResponse)

        m_communicationServer.SendTo(clientID, response)

    End Sub

    Public Sub UpdateStatus(ByVal message As String)

        UpdateStatus(message, 1)

    End Sub

    Public Sub UpdateStatus(ByVal message As String, ByVal crlfCount As Integer)

        With New StringBuilder()
            .Append(message)

            For i As Integer = 0 To crlfCount - 1
                .Append(Environment.NewLine)
            Next

            ' Send the status update to all connected clients.
            SendUpdateClientStatusResponse(.ToString())

            ' Log the status update to the log file if logging is enabled.
            If m_logStatusUpdates Then Me.LogFile.WriteTimestampedLine(.ToString())
        End With

    End Sub

#Region " Interface Implementation "

#Region " IPersistSettings "

    Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
        Get
            Return m_persistSettings
        End Get
        Set(ByVal value As Boolean)
            m_persistSettings = value
        End Set
    End Property

    Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
        Get
            Return m_settingsCategoryName
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_settingsCategoryName = value
            Else
                Throw New ArgumentNullException("ConfigurationCategory")
            End If
        End Set
    End Property

    Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

        Try
            With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                If .Count > 0 Then
                    LogStatusUpdates = .Item("LogStatusUpdates").GetTypedValue(m_logStatusUpdates)
                    RequestHistoryLimit = .Item("RequestHistoryLimit").GetTypedValue(m_requestHistoryLimit)
                End If
            End With
        Catch ex As Exception
            ' We'll encounter exceptions if the settings are not present in the config file.
        End Try

    End Sub

    Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

        If m_persistsettings Then
            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    .Clear()
                    With .Item("LogStatusUpdates", True)
                        .Value = LogStatusUpdates.ToString()
                        .Description = ""
                    End With
                    With .Item("RequestHistoryLimit", True)
                        .Value = RequestHistoryLimit.ToString()
                        .Description = ""
                    End With
                End With
                TVA.Configuration.Common.SaveSettings()
            Catch ex As Exception
                ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
            End Try
        End If

    End Sub

#End Region

#Region " ISupportInitialize "

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        ' We don't need to do anything before the component is initialized.

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If Not DesignMode Then
            LoadSettings()  ' Load settings from the config file.
        End If

    End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

    Private Sub ListClients()

        If m_clientInfo.Count > 0 Then
            With New StringBuilder()
                ' Display information about all of the connected clients.
                .AppendFormat("List of client connected to {0}:", m_service.ServiceName)
                For Each clientInfo As ClientInfo In m_clientInfo.Values
                    .AppendLine()
                    .AppendLine()
                    .AppendFormat("                  Assembly: {0}", clientInfo.Assembly)
                    .AppendLine()
                    .AppendFormat("                  Location: {0}", clientInfo.Location)
                    .AppendLine()
                    .AppendFormat("                   Created: {0}", clientInfo.Created.ToString())
                    .AppendLine()
                    .AppendFormat("                 User Name: {0}", clientInfo.UserName)
                    .AppendLine()
                    .AppendFormat("              Machine Name: {0}", clientInfo.MachineName)
                Next

                UpdateStatus(.ToString())
            End With
        Else
            ' This will never be the case because at the least the client that sent the request will be connected.
            UpdateStatus(String.Format("No clients are connected to {0}.", m_service.ServiceName))
        End If

    End Sub

    Private Sub ListSettings()

        Dim categories As String() = {"ServiceHelper", "Communication"}
        With New StringBuilder()
            .AppendFormat("Settings for {0}:", m_service.ServiceName)
            .AppendLine()
            .AppendLine()
            .Append("Setting Category".PadRight(20))
            .Append(" ")
            .Append("Setting Name".PadRight(25))
            .Append(" ")
            .Append("Setting Value".PadRight(30))
            .Append(" ")
            .AppendLine()
            .Append(New String("-"c, 20))
            .Append(" ")
            .Append(New String("-"c, 25))
            .Append(" ")
            .Append(New String("-"c, 30))
            For Each category As String In categories
                For Each setting As TVA.Configuration.CategorizedSettingsElement In CategorizedSettings(category)
                    .AppendLine()
                    .Append(category.PadRight(20))
                    .Append(" ")
                    .Append(setting.Name.PadRight(25))
                    .Append(" ")
                    .Append(setting.Value.PadRight(30))
                Next
            Next

            UpdateStatus(.ToString())
        End With

    End Sub

    Private Sub ListProcesses()

        With New StringBuilder()
            If m_processes.Count > 0 Then
                ' There are processes defined in the service, so display their status.
                .AppendFormat("List of processes defined in {0}:", m_service.ServiceName)
                .AppendLine()

                For Each process As ServiceProcess In m_processes.Values
                    .AppendLine()
                    .Append(process.Status)
                    Dim processSchedule As Schedule = Nothing
                    If ScheduleManager.Schedules.TryGetValue(process.Name, processSchedule) Then
                        .Append("                 Scheduled: Yes")
                        .AppendLine()
                        .AppendFormat("             Schedule Rule: {0}", processSchedule.Rule)
                        .AppendLine()
                        .AppendFormat("      Schedule Description: {0}", processSchedule.Description)
                    Else
                        .Append("                 Scheduled: No")
                        .AppendLine()
                        .Append("             Schedule Rule: N/A")
                        .AppendLine()
                        .Append("      Schedule Description: N/A")
                    End If
                    .AppendLine()
                Next
            Else
                ' There are no processes defined in the service to be displayed.
                UpdateStatus(String.Format("No processes are defined in {0}.", m_service.ServiceName))
            End If

            UpdateStatus(.ToString())
        End With

    End Sub

    Private Sub ReloadSettings()

        ' Initialize the member variable with the values from config file.
        m_logStatusUpdates = CategorizedSettings("ServiceHelper")("LogStatusUpdates").GetTypedValue(True)
        m_requestHistoryLimit = CategorizedSettings("ServiceHelper")("RequestHistoryLimit").GetTypedValue(50)
        'm_configurationString = CategorizedSettings("Communication")("ConfigurationString").Value
        'm_encryption = CategorizedSettings("Communication")("Encryption").GetTypedValue(TVA.Security.Cryptography.EncryptLevel.Level1)
        'm_secureSession = CategorizedSettings("Communication")("SecureSession").GetTypedValue(True)

    End Sub

    Private Sub UpdateSettings()

        If m_lastRequest.Parameters.Length > 2 Then
            Dim category As String = m_lastRequest.Parameters(0)
            Dim name As String = m_lastRequest.Parameters(1)
            Dim value As String = m_lastRequest.Parameters(2)
            Dim setting As TVA.Configuration.CategorizedSettingsElement = CategorizedSettings(category)(name)
            If setting IsNot Nothing Then
                setting.Value = value
                SaveSettings()      ' Save rettings to the config file.
                ReloadSettings()    ' Initialize member variables with values from the config file.
                UpdateStatus(String.Format("Value of setting ""{0}"" under category ""{1}"" has been updated.", name, category))
            Else
                UpdateStatus(String.Format("Setting ""{0}"" does not exist under category ""{1}""", name, category))
            End If
        Else

        End If

    End Sub

    Private Sub StartProcess()

        Dim processToStart As ServiceProcess = Nothing
        If m_lastRequest.Parameters IsNot Nothing AndAlso m_lastRequest.Parameters.Length > 0 Then
            ' The user has specified the name of the process to start, so we'll see if the process exists.
            Dim processName As String = m_lastRequest.Parameters(0).Trim()
            If Not m_processes.TryGetValue(processName, processToStart) Then
                ' The specified process does not exist.
                UpdateStatus(String.Format("Process ""{0}"" cannot be started. Process does not exist.", processName))
            End If
        Else
            ' The user didn't provide the name of the process to start, so we'll see if there are any processes
            ' defined in the service and if so we'll execute the first process from the list of defined processes.
            If m_processes.Count > 0 Then
                ' There is at least 1 process defined in the service.
                For Each process As ServiceProcess In m_processes.Values
                    processToStart = process
                    Exit For
                Next
            Else
                UpdateStatus("Default process cannot be started. No processes are defined.")
            End If
        End If

        If processToStart IsNot Nothing Then
            ' We have a process that we can try to start.
            If Not processToStart.CurrentState = ProcessState.Processing Then
                ' The specified process is currently not executing, so we'll start its execution.
                UpdateStatus(String.Format("Process ""{0}"" is being started...", processToStart.Name))
                If m_lastRequest.Parameters.Length > 1 Then
                    ' We'll provide any additional parameters received to the process for consumption.
                    Dim processParameters As Object() = CreateArray(Of Object)(m_lastRequest.Parameters.Length - 1)
                    Array.Copy(m_lastRequest.Parameters, 1, processParameters, 0, processParameters.Length)
                    processToStart.Parameters = processParameters
                End If
                processToStart.Start()
            Else
                ' We cannot start execution of the specified process because it is currently executing.
                UpdateStatus(String.Format("Process ""{0}"" cannot be started. Process is executing.", processToStart.Name))
            End If
        End If

    End Sub

    Private Sub AbortProcess()

        Dim processToAbort As ServiceProcess = Nothing
        If m_lastRequest.Parameters IsNot Nothing AndAlso m_lastRequest.Parameters.Length > 0 Then
            ' The user has specified the name of the process to abort, so we'll see if the process exists.
            Dim processName As String = m_lastRequest.Parameters(0).Trim()
            If Not m_processes.TryGetValue(processName, processToAbort) Then
                ' The specified process does not exist.
                UpdateStatus(String.Format("Process ""{0}"" cannot be aborted. Process does not exist.", processName))
            End If
        Else
            ' The user didn't provide the name of the process to abort, so we'll see if there are any processes
            ' defined in the service and if so we'll try to abort the first process from the list of defined processes.
            If m_processes.Count > 0 Then
                ' There is at least 1 process defined in the service.
                For Each process As ServiceProcess In m_processes.Values
                    processToAbort = process
                    Exit For
                Next
            Else
                UpdateStatus("Default process cannot be aborted. No processes are defined.")
            End If
        End If

        If processToAbort IsNot Nothing Then
            ' We have a process that we can try to abort.
            If processToAbort.CurrentState = ProcessState.Processing Then
                ' The specified process is currently executing, so we'll abort its execution.
                UpdateStatus(String.Format("Process ""{0}"" is being aborted...", processToAbort.Name))
                processToAbort.Abort()
            Else
                ' We cannot abort execution of the specified process because it is currently not executing.
                UpdateStatus(String.Format("Process ""{0}"" cannot be aborted. Process is not executing.", processToAbort.Name))
            End If
        End If

    End Sub

    Private Sub RescheduleProcess()

        If m_lastRequest.Parameters.Length > 1 Then
            ' Parameters required for scheduling a process are provided.
            Dim processName As String = m_lastRequest.Parameters(0).Trim()
            Dim scheduleRule As String = m_lastRequest.Parameters(1).Trim(""""c)

            ' Schedule the specified process. Process will not be scheduled if process does not exist.
            ScheduleProcess(processName, scheduleRule)

            Dim processSchedule As Schedule = Nothing
            If ScheduleManager.Schedules.TryGetValue(processName, processSchedule) Then
                ' A schedule for the process exists, so the process was scheduled successfully.
                UpdateStatus(String.Format("Process ""{0}"" scheduled for {1}.", processSchedule.Name, processSchedule.Description))
            End If
        Else
            UpdateStatus("Process name and schedule are required in order to schedule a process.")
        End If

    End Sub

    Private Sub UnscheduleProcess()

        If m_lastRequest.Parameters.Length > 0 Then
            ' We have the name of the process that is to be unscheduled.
            Dim processName As String = m_lastRequest.Parameters(0).Trim()

            If ScheduleManager.Schedules.ContainsKey(processName) Then
                ' The specified process is scheduled, so we'll unschedule it.
                ScheduleManager.Schedules.Remove(processName)
                UpdateStatus(String.Format("Process ""{0}"" has been unscheduled.", processName))
            Else
                ' We cannot unschedule the specified process because it is not scheduled.
                UpdateStatus(String.Format("Process ""{0}"" could not be unscheduled. Process is not scheduled.", processName))
            End If
        Else
            UpdateStatus("Process name is required in order to unschedule a process.")
        End If

    End Sub

    Private Sub SaveSchedules()

        ScheduleManager.SaveSettings()
        UpdateStatus("Schedules saved to the configuration file successfully.")

    End Sub

    Private Sub LoadSchedules()

        ScheduleManager.LoadSettings()
        UpdateStatus("Schedules loaded from the configuration file successfully.")

    End Sub

    Private Sub GetServiceStatus()

        With New StringBuilder()
            .Append(String.Format("Status of components used by {0}:", m_service.ServiceName))
            .Append(Environment.NewLine)
            For Each serviceComponent As IServiceComponent In m_serviceComponents
                If serviceComponent IsNot Nothing Then
                    .Append(Environment.NewLine)
                    .Append(String.Format("Status of {0}:", serviceComponent.Name))
                    .Append(Environment.NewLine)
                    .Append(serviceComponent.Status)
                End If
            Next

            UpdateStatus(.ToString())
        End With

    End Sub

    Private Sub GetRequestHistory()

        With New StringBuilder()
            .Append("History of requests received from connected clients:")
            .AppendLine()
            .AppendLine()
            .Append("Request Type".PadRight(20))
            .Append(" ")
            .Append("Received At".PadRight(25))
            .Append(" ")
            .Append("Sent By".PadRight(30))
            .AppendLine()
            .Append(New String("-"c, 20))
            .Append(" ")
            .Append(New String("-"c, 25))
            .Append(" ")
            .Append(New String("-"c, 30))
            For Each request As RequestInfo In m_requestHistory
                .AppendLine()
                .Append(request.RequestType.PadRight(20))
                .Append(" ")
                .Append(request.RequestReceivedAt.ToString().PadRight(25))
                .Append(" ")
                Dim sender As ClientInfo = Nothing
                If m_clientInfo.TryGetValue(request.RequestSender, sender) Then
                    .AppendFormat("{0} from {1}".PadRight(30), sender.UserName, sender.MachineName)
                Else
                    .Append("[Client Disconnected]".PadRight(30))
                End If
            Next

            UpdateStatus(.ToString())
        End With

    End Sub

    Private Sub HandleInvalidClientRequest(ByVal request As ClientRequest)

        UpdateStatus(String.Format("Request ""{0}"" cannot be processed. Request is invalid.", request.Type))

    End Sub

    Private Sub SendUpdateClientStatusResponse(ByVal response As String)

        Dim serviceResponse As New ServiceResponse()
        serviceResponse.Type = "UPDATECLIENTSTATUS"
        serviceResponse.Message = response
        SendResponse(serviceResponse)

    End Sub

    Private Sub SendServiceStateChangedResponse(ByVal serviceState As ServiceState)

        Dim serviceResponse As New ServiceResponse()
        serviceResponse.Type = "SERVICESTATECHANGED"
        serviceResponse.Message = m_service.ServiceName & ">" & serviceState.ToString()
        SendResponse(serviceResponse)

    End Sub

    Private Sub SendProcessStateChangedResponse(ByVal processName As String, ByVal processState As ProcessState)

        Dim serviceResponse As New ServiceResponse()
        serviceResponse.Type = "PROCESSSTATECHANGED"
        serviceResponse.Message = processName & ">" & processState.ToString()
        SendResponse(serviceResponse)

    End Sub

#Region " Event Handlers "

#Region " CommunicationServer "

    Private Sub m_communicationServer_ClientConnected(ByVal sender As Object, ByVal e As IdentifiableSourceEventArgs) Handles m_communicationServer.ClientConnected

        m_clientInfo.Add(e.Source, Nothing)

    End Sub

    Private Sub m_communicationServer_ClientDisconnected(ByVal sender As Object, ByVal e As IdentifiableSourceEventArgs) Handles m_communicationServer.ClientDisconnected

        m_clientInfo.Remove(e.Source)

    End Sub

    Private Sub m_communicationServer_ReceivedClientData(ByVal sender As Object, ByVal e As IdentifiableItemEventArgs(Of Byte())) Handles m_communicationServer.ReceivedClientData

        Dim info As ClientInfo = GetObject(Of ClientInfo)(e.Item)
        Dim request As ClientRequest = GetObject(Of ClientRequest)(e.Item)

        If info IsNot Nothing Then
            ' We've received client information from a recently connected client.
            m_clientInfo(e.Source) = info
        ElseIf request IsNot Nothing Then
            Dim receivedClientRequestEvent As New ClientRequestEventArgs(e.Source, request)

            ' Log the received request.
            m_lastRequest = request
            m_requestHistory.Add(New RequestInfo(request.Type, e.Source, System.DateTime.Now))
            If m_requestHistory.Count > m_requestHistoryLimit Then
                ' We'll remove old request entries if we've exceeded the limit for request history.
                m_requestHistory.RemoveRange(0, (m_requestHistory.Count - m_requestHistoryLimit))
            End If

            ' Notify the consumer about the incoming request from client.
            RaiseEvent ReceivedClientRequest(Me, receivedClientRequestEvent)
            If receivedClientRequestEvent.Cancel Then Exit Sub

            ' We'll process the request only if the service didn't handle it.
            Select Case request.Type.ToUpper()
                Case "CLIENTS", "LISTCLIENTS"
                    ListClients()
                Case "SETTINGS", "LISTSETTINGS"
                    ListSettings()
                Case "PROCESSES", "LISTPROCESSES"
                    ListProcesses()
                Case "RELOADSETTINGS"
                    ReloadSettings()
                Case "UPDATESETTINGS"
                    UpdateSettings()
                Case "START", "STARTPROCESS"
                    StartProcess()
                Case "ABORT", "ABORTPROCESS"
                    AbortProcess()
                Case "RESCHEDULE", "RESCHEDULEPROCESS"
                    RescheduleProcess()
                Case "UNSCHEDULE", "UNSCHEDULEPROCESS"
                    UnscheduleProcess()
                Case "SAVESCHEDULES"
                    SaveSchedules()
                Case "LOADSCHEDULES"
                    LoadSchedules()
                Case "STATUS", "GETSERVICESTATUS"
                    GetServiceStatus()
                Case "HISTORY", "GETREQUESTHISTORY"
                    GetRequestHistory()
                Case Else
                    HandleInvalidClientRequest(request)
            End Select
        Else
            HandleInvalidClientRequest(request)
        End If

    End Sub

#End Region

#Region " ScheduleManager "

    Private Sub ScheduleManager_ScheduleDue(ByVal sender As Object, ByVal e As ScheduleEventArgs) Handles ScheduleManager.ScheduleDue

        Dim scheduledProcess As ServiceProcess = Nothing
        If m_processes.TryGetValue(e.Schedule.Name, scheduledProcess) Then
            scheduledProcess.Start() ' Start the process execution if it exists.
        End If

    End Sub

#End Region

#Region " LogFile "

    Private Sub LogFile_LogException(ByVal sender As Object, ByVal e As ExceptionEventArgs) Handles LogFile.LogException

        ' We'll let the connected clients know that we encountered an exception while logging the status update.
        m_logStatusUpdates = False
        UpdateStatus(String.Format("Error occurred while logging status update: {0}", e.Exception.ToString()))
        m_logStatusUpdates = True

    End Sub

#End Region

#End Region

#End Region

End Class