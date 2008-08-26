Imports System.ServiceProcess

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Service
    Inherits System.ServiceProcess.ServiceBase

#Region " ServiceHelper Code "

    Protected Overrides Sub OnStart(ByVal args() As String)
        ServiceHelper.OnStart(args)
    End Sub

    Protected Overrides Sub OnStop()
        ServiceHelper.OnStop()
    End Sub

    Protected Overrides Sub OnPause()
        ServiceHelper.OnPause()
    End Sub

    Protected Overrides Sub OnContinue()
        ServiceHelper.OnResume()
    End Sub

    Protected Overrides Sub OnShutdown()
        ServiceHelper.OnShutdown()
    End Sub

#End Region

    'UserService overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' The main entry point for the process
    <MTAThread()> _
    <System.Diagnostics.DebuggerNonUserCode()> _
    Shared Sub Main()
        Dim ServicesToRun() As System.ServiceProcess.ServiceBase

        ' More than one NT Service may run within the same process. To add
        ' another service to this process, change the following line to
        ' create a second service object. For example,
        '
        '   ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}
        '
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New Service}

        System.ServiceProcess.ServiceBase.Run(ServicesToRun)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  
    ' Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.TcpServer = New TVA.Communication.TcpServer(Me.components)
        Me.ServiceHelper = New TVA.Services.ServiceHelper(Me.components)
        CType(Me.TcpServer, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ServiceHelper, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'TcpServer
        '
        Me.TcpServer.ConfigurationString = "Port=8500"
        Me.TcpServer.HandshakePassphrase = "TVASPDC"
        Me.TcpServer.MaximumClients = 50
        Me.TcpServer.PayloadAware = True
        Me.TcpServer.PersistSettings = True
        Me.TcpServer.SettingsCategoryName = "RemoteMonitorSocket"
        '
        'ServiceHelper
        '
        Me.ServiceHelper.RemotingServer = Me.TcpServer
        Me.ServiceHelper.MonitorServiceHealth = True
        Me.ServiceHelper.PersistSettings = True
        Me.ServiceHelper.QueryableSettingsCategories = "ServiceSettings, SH.LogFile, SH.ExceptionLogger,ScheduleManager,RemoteMonit" & _
            "orSocket"
        Me.ServiceHelper.Service = Me
        Me.ServiceHelper.SettingsCategoryName = "ServiceSettings"
        '
        'Service
        '
        Me.CanShutdown = True
        Me.ServiceName = "TVASPDC"
        CType(Me.TcpServer, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ServiceHelper, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Friend WithEvents TcpServer As TVA.Communication.TcpServer
    Friend WithEvents ServiceHelper As TVA.Services.ServiceHelper

End Class
