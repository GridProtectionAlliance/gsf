' 08-29-06

Partial Class ServiceHelper
    Inherits System.ComponentModel.Component

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New(ByVal Container As System.ComponentModel.IContainer)
        MyClass.New()

        'Required for Windows.Forms Class Composition Designer support
        Container.Add(Me)
        m_startedEventHandlerList = New List(Of EventHandler)
        m_stoppedEventHandlerList = New List(Of EventHandler)
        m_serviceComponents = New List(Of IServiceComponent)
        m_serviceComponents.Add(SHTcpServer)
        m_serviceComponents.Add(SHScheduleManager)
        m_serviceComponents.Add(SHSsamLogger)

    End Sub

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ServiceHelper))
        Me.SHTcpServer = New Tva.Communication.TcpServer(Me.components)
        Me.SHScheduleManager = New Tva.ScheduleManager(Me.components)
        Me.SHSsamLogger = New Tva.Tro.Ssam.SsamLogger(Me.components)
        '
        'SHTcpServer
        '
        Me.SHTcpServer.ConfigurationString = "Port=6500"
        Me.SHTcpServer.Encryption = Tva.Security.Cryptography.EncryptLevel.Level4
        Me.SHTcpServer.PayloadAware = True
        Me.SHTcpServer.SecureSession = True
        Me.SHTcpServer.TextEncoding = CType(resources.GetObject("SHTcpServer.TextEncoding"), System.Text.Encoding)
        '
        'SHScheduleManager
        '
        Me.SHScheduleManager.ConfigurationElement = "ScheduleManager"
        Me.SHScheduleManager.Enabled = True
        Me.SHScheduleManager.PersistSchedules = True
        '
        'SHSsamLogger
        '
        Me.SHSsamLogger.SsamApi.ConnectionString = "Server=RGOCSQLD;Database=Ssam;Trusted_Connection=True;"

    End Sub
    Friend WithEvents SHTcpServer As Tva.Communication.TcpServer
    Friend WithEvents SHScheduleManager As Tva.ScheduleManager
    Friend WithEvents SHSsamLogger As Tva.Tro.Ssam.SsamLogger

End Class