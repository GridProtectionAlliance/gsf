' 08-29-06

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
        Me.LogFile = New TVA.IO.LogFile(Me.components)
        Me.ScheduleManager = New TVA.Scheduling.ScheduleManager(Me.components)
        CType(Me.LogFile, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'LogFile
        '
        Me.LogFile.Name = "StatusLog.txt"
        '
        'ScheduleManager
        '
        CType(Me.LogFile, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents ScheduleManager As TVA.Scheduling.ScheduleManager
    Public WithEvents LogFile As TVA.IO.LogFile

End Class