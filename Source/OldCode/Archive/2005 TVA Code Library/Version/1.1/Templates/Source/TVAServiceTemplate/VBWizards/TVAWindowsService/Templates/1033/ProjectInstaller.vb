'***********************************************************************
'  ProjectInstaller.vb - TVA Service Template
'  Copyright © [!output CURR_YEAR] - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: [!output DEV_NAME]
'      Office: [!output DEV_OFFICE]
'       Phone: [!output DEV_PHONE]
'       Email: [!output DEV_EMAIL]
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  [!output CURR_DATE] - [!output USER_NAME]
'       Initial version of source generated for new Windows service
'       project "[!output PROJECT_NAME]".
'
'***********************************************************************

Imports System.ComponentModel
Imports System.Configuration.Install

<RunInstaller(True)> Public Class ProjectInstaller
    Inherits System.Configuration.Install.Installer

#Region " Component Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Installer overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents ServiceProcessInstaller As System.ServiceProcess.ServiceProcessInstaller
    Friend WithEvents ServiceInstaller As System.ServiceProcess.ServiceInstaller
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.ServiceProcessInstaller = New System.ServiceProcess.ServiceProcessInstaller()
        Me.ServiceInstaller = New System.ServiceProcess.ServiceInstaller()
        '
        'ServiceProcessInstaller
        '
        Me.ServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem
        Me.ServiceProcessInstaller.Password = Nothing
        Me.ServiceProcessInstaller.Username = Nothing
        '
        'ServiceInstaller
        '
        Me.ServiceInstaller.DisplayName = "[!output PROJECT_NAME]"
        Me.ServiceInstaller.ServiceName = "[!output PROJECT_ID]"
        Me.ServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic
        '
        'ProjectInstaller
        '
        Me.Installers.AddRange(New System.Configuration.Install.Installer() {Me.ServiceProcessInstaller, Me.ServiceInstaller})

    End Sub

#End Region

End Class
