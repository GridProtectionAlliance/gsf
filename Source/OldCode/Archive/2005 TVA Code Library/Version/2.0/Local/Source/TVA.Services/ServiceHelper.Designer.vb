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

        m_logStatusUpdates = True
        m_requestHistoryLimit = 50
        m_queryableSettingsCategories = "SH.General, SH.LogFile, SH.GlobalExceptionLogger"
        m_processes = New List(Of ServiceProcess)()
        m_settingsCategoryName = Me.GetType().Name
        m_connectedClients = New List(Of ClientInfo)()
        m_clientRequestHistory = New List(Of ClientRequestInfo)()
        m_serviceComponents = New List(Of IServiceComponent)()
        'm_serviceStartingEventHandlerList = New List(Of ServiceStartingEventHandler)()
        'm_serviceStoppingEventHandlerList = New List(Of EventHandler)()
        m_clientRequestHandlers = New List(Of ClientRequestHandlerInfo)()

    End Sub

    'Component overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            SaveSettings()  ' Saves settings to the config file.
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
        Me.components = New System.ComponentModel.Container
        Me.LogFile = New TVA.IO.LogFile(Me.components)
        Me.ScheduleManager = New TVA.Scheduling.ScheduleManager(Me.components)
        Me.GlobalExceptionLogger = New TVA.ErrorManagement.GlobalExceptionLogger(Me.components)
        CType(Me.LogFile, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ScheduleManager, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.GlobalExceptionLogger, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'LogFile
        '
        Me.LogFile.Name = "StatusLog.txt"
        Me.LogFile.PersistSettings = True
        Me.LogFile.SettingsCategoryName = "SH.LogFile"
        '
        'ScheduleManager
        '
        Me.ScheduleManager.PersistSettings = True
        Me.ScheduleManager.SettingsCategoryName = "SH.ScheduleManager"
        '
        'GlobalExceptionLogger
        '
        Me.GlobalExceptionLogger.AutoRegister = True
        Me.GlobalExceptionLogger.ContactEmail = Nothing
        Me.GlobalExceptionLogger.ContactName = Nothing
        Me.GlobalExceptionLogger.ContactPhone = Nothing
        Me.GlobalExceptionLogger.ExitOnUnhandledException = True
        Me.GlobalExceptionLogger.LogToEmail = False
        Me.GlobalExceptionLogger.LogToEventLog = True
        Me.GlobalExceptionLogger.LogToFile = True
        Me.GlobalExceptionLogger.LogToScreenshot = False
        Me.GlobalExceptionLogger.LogToUI = False
        Me.GlobalExceptionLogger.PersistSettings = True
        Me.GlobalExceptionLogger.SettingsCategoryName = "SH.GlobalExceptionLogger"
        Me.GlobalExceptionLogger.SmtpServer = "mailhost.cha.tva.gov"
        CType(Me.LogFile, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ScheduleManager, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.GlobalExceptionLogger, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public WithEvents LogFile As TVA.IO.LogFile
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public WithEvents ScheduleManager As TVA.Scheduling.ScheduleManager
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public WithEvents GlobalExceptionLogger As TVA.ErrorManagement.GlobalExceptionLogger

End Class