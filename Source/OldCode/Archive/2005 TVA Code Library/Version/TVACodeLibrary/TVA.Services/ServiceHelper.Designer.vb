Imports System.ComponentModel

Partial Class ServiceHelper
    Inherits System.ComponentModel.Component

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New(ByVal Container As System.ComponentModel.IContainer)
        MyClass.New()

        'Required for Windows.Forms Class Composition Designer support
        Container.Add(Me)

    End Sub

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        m_logStatusUpdates = DefaultLogStatusUpdates
        m_monitorServiceHealth = DefaultMonitorServiceHealth
        m_requestHistoryLimit = DefaultRequestHistoryLimit
        m_queryableSettingsCategories = DefaultQueryableSettingsCategories
        m_persistSettings = DefaultPersistSettings
        m_settingsCategoryName = DefaultSettingsCategoryName
        m_pursip = "s3cur3"
        m_processes = New List(Of ServiceProcess)()
        m_connectedClients = New List(Of ClientInfo)()
        m_clientRequestHistory = New List(Of ClientRequestInfo)()
        m_serviceComponents = New List(Of IServiceComponent)()
        m_clientRequestHandlers = New List(Of ClientRequestHandlerInfo)()
        ' Components
        m_statusLog = New TVA.IO.LogFile()
        m_scheduler = New TVA.Scheduling.ScheduleManager()
        m_exceptionLogger = New TVA.ErrorManagement.GlobalExceptionLogger()
        m_statusLog.Name = "StatusLog.txt"
        m_statusLog.PersistSettings = True
        m_statusLog.SettingsCategoryName = "StatusLog"
        m_scheduler.PersistSettings = True
        m_scheduler.SettingsCategoryName = "Scheduler"
        m_exceptionLogger.ExitOnUnhandledException = True
        m_exceptionLogger.PersistSettings = True
        m_exceptionLogger.SettingsCategoryName = "ExceptionLogger"

    End Sub

    'Component overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            SaveSettings()  ' Saves settings to the config file.
            m_statusLog.Dispose()
            m_scheduler.Dispose()
            m_exceptionLogger.Dispose()
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()

    End Sub

End Class