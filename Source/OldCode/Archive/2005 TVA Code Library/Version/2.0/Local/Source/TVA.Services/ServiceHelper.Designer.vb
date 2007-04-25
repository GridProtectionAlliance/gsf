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
        m_encryption = Security.Cryptography.EncryptLevel.Level1
        m_secureSession = True
        m_configurationString = "Protocol=Tcp; Port=6500"
        m_processes = New Dictionary(Of String, ServiceProcess)(StringComparer.CurrentCultureIgnoreCase)
        m_clientInfo = New Dictionary(Of Guid, ClientInfo)()
        m_requestHistory = New List(Of RequestInfo)()
        m_serviceComponents = New List(Of IServiceComponent)()
        m_startedEventHandlerList = New List(Of StartedEventHandler)()
        m_stoppedEventHandlerList = New List(Of EventHandler)()

    End Sub

    'Component overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.TcpServer = New TVA.Communication.TcpServer(Me.components)
        Me.UdpServer = New TVA.Communication.UdpServer(Me.components)
        Me.ScheduleManager = New TVA.Scheduling.ScheduleManager(Me.components)
        Me.LogFile = New TVA.IO.LogFile(Me.components)
        Me.GlobalExceptionLogger = New TVA.ErrorManagement.GlobalExceptionLogger(Me.components)
        CType(Me.TcpServer, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.UdpServer, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ScheduleManager, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LogFile, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.GlobalExceptionLogger, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'TcpServer
        '
        Me.TcpServer.ConfigurationString = "Port=8888"
        Me.TcpServer.HandshakePassphrase = Nothing
        Me.TcpServer.PayloadAware = True
        Me.TcpServer.PersistSettings = True
        Me.TcpServer.SettingsCategoryName = "ServiceHelper.TcpServer"
        '
        'UdpServer
        '
        Me.UdpServer.ConfigurationString = "Port=8888; Clients=255.255.255.255:8888"
        Me.UdpServer.Enabled = False
        Me.UdpServer.HandshakePassphrase = Nothing
        Me.UdpServer.PayloadAware = True
        Me.UdpServer.PersistSettings = True
        Me.UdpServer.ReceiveBufferSize = 32768
        Me.UdpServer.SettingsCategoryName = "ServiceHelper.UdpServer"
        '
        'ScheduleManager
        '
        Me.ScheduleManager.PersistSettings = True
        Me.ScheduleManager.SettingsCategoryName = "ServiceHelper.ScheduleManager"
        '
        'LogFile
        '
        Me.LogFile.Name = "StatusLog.txt"
        Me.LogFile.PersistSettings = True
        Me.LogFile.SettingsCategoryName = "ServiceHelper.LogFile"
        '
        'GlobalExceptionLogger
        '
        Me.GlobalExceptionLogger.AutoRegister = False
        Me.GlobalExceptionLogger.ContactPersonName = Nothing
        Me.GlobalExceptionLogger.ContactPersonPhone = Nothing
        Me.GlobalExceptionLogger.EmailRecipients = Nothing
        Me.GlobalExceptionLogger.EmailServer = "mailhost.cha.tva.gov"
        Me.GlobalExceptionLogger.ExitOnUnhandledException = False
        Me.GlobalExceptionLogger.LogToEmail = False
        Me.GlobalExceptionLogger.LogToEventLog = False
        Me.GlobalExceptionLogger.LogToFile = False
        Me.GlobalExceptionLogger.LogToScreenshot = False
        Me.GlobalExceptionLogger.LogToUI = False
        Me.GlobalExceptionLogger.PersistSettings = True
        Me.GlobalExceptionLogger.SettingsCategoryName = "ServiceHelper.GlobalExceptionLogger"
        CType(Me.TcpServer, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.UdpServer, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ScheduleManager, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LogFile, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.GlobalExceptionLogger, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Friend WithEvents ScheduleManager As TVA.Scheduling.ScheduleManager
    Friend WithEvents LogFile As TVA.IO.LogFile
    Friend WithEvents GlobalExceptionLogger As TVA.ErrorManagement.GlobalExceptionLogger
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    Public WithEvents TcpServer As TVA.Communication.TcpServer
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    Public WithEvents UdpServer As TVA.Communication.UdpServer

End Class