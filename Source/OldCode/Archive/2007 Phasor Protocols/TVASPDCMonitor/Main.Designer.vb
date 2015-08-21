Partial Class Main
    Inherits System.ComponentModel.Component

#Region " Console Code "

    Private Shared m_consoleWindow As Main

    Shared Sub Main(ByVal args() As String)

        TVA.Console.Common.EnableRaisingEvents()

        m_consoleWindow = New Main(args)
        m_consoleWindow.Main_Load()
        m_consoleWindow.Dispose()

    End Sub

#End Region

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
        Me.ClientHelper = New TVA.Services.ClientHelper(Me.components)
        Me.TcpClient = New TVA.Communication.TcpClient(Me.components)
        CType(Me.ClientHelper, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.TcpClient, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'ClientHelper
        '
        Me.ClientHelper.RemotingClient = Me.TcpClient
        Me.ClientHelper.PersistSettings = True
        Me.ClientHelper.ServiceName = "TVASPDC"
        Me.ClientHelper.SettingsCategoryName = "RemoteMonitorSettings"
        '
        'TcpClient
        '
        Me.TcpClient.ConnectionString = "Server=localhost; Port=8500"
        Me.TcpClient.HandshakePassphrase = "TVASPDC"
        Me.TcpClient.PayloadAware = True
        Me.TcpClient.PersistSettings = True
        Me.TcpClient.SettingsCategoryName = "RemoteMonitorSocket"
        CType(Me.ClientHelper, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.TcpClient, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Friend WithEvents ClientHelper As TVA.Services.ClientHelper
    Friend WithEvents TcpClient As TVA.Communication.TcpClient

End Class
