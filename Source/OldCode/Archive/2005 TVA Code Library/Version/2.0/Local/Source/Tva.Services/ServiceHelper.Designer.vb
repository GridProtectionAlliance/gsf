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
        Me.TcpServer = New Tva.Communication.TcpServer(Me.components)
        Me.ScheduleManager = New Tva.ScheduleManager(Me.components)
        Me.SsamLogger = New Tva.Tro.Ssam.SsamLogger(Me.components)
        CType(Me.ScheduleManager, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.SsamLogger, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'TcpServer
        '
        Me.TcpServer.ConfigurationString = "Port=8888"
        Me.TcpServer.TextEncoding = CType(resources.GetObject("TcpServer.TextEncoding"), System.Text.Encoding)
        '
        'ScheduleManager
        '
        Me.ScheduleManager.ConfigurationElement = "ScheduleManager"
        Me.ScheduleManager.Enabled = True
        Me.ScheduleManager.PersistSchedules = True
        '
        'SsamLogger
        '
        Me.SsamLogger.SsamApi.ConnectionString = "Server=RGOCSQLD;Database=Ssam;Trusted_Connection=True;"
        CType(Me.ScheduleManager, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.SsamLogger, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Friend WithEvents TcpServer As Tva.Communication.TcpServer
    Friend WithEvents ScheduleManager As Tva.ScheduleManager
    Friend WithEvents SsamLogger As Tva.Tro.Ssam.SsamLogger

End Class