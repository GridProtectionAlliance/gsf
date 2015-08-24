Imports TVA.Remoting

Public Class Form1

    Inherits System.Windows.Forms.Form

    Friend Const AuthenticationKey As String = "96B86EA1-4BFF-4068-BAEB-6F84E46C570D"
    Friend Const AuthenticationKey2 As String = "96B86EA1-4BFF-4068-BAEB-6F84E46C570D"
    'Friend Const AuthenticationKey2 As String = "85B86EA1-3AFF-3968-CBEB-5D84E46C570D"

    Private WithEvents ProxyServer As SecureServer
    Private WithEvents ProxyServer2 As SecureServer

    ' We also create some internal clients (these will automtically use interprocess serialization for speed)
    Private WithEvents ProxyClient As SecureClient
    Private WithEvents ProxyClient2 As SecureClient

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents StatusText As System.Windows.Forms.TextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.StatusText = New System.Windows.Forms.TextBox
        Me.SuspendLayout()
        '
        'StatusText
        '
        Me.StatusText.BackColor = System.Drawing.Color.Black
        Me.StatusText.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StatusText.ForeColor = System.Drawing.Color.White
        Me.StatusText.Location = New System.Drawing.Point(8, 8)
        Me.StatusText.MaxLength = 0
        Me.StatusText.Multiline = True
        Me.StatusText.Name = "StatusText"
        Me.StatusText.ReadOnly = True
        Me.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.StatusText.Size = New System.Drawing.Size(616, 416)
        Me.StatusText.TabIndex = 0
        Me.StatusText.Text = ""
        '
        'Form1
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(632, 437)
        Me.Controls.Add(Me.StatusText)
        Me.Name = "Form1"
        Me.Text = "Proxy Server Test"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ProxyServer = New SecureServer("ServerTest", "Test", 19000, AuthenticationKey, 30)
        ProxyServer2 = New SecureServer("ServerTest2", "Test2", 19001, AuthenticationKey2, 30)

        ProxyServer.Start()
        ProxyServer2.Start()

    End Sub

    Private Sub ProxyServer_ServerEstablished(ByVal ServerID As System.Guid, ByVal TCPPort As Integer) Handles ProxyServer.ServerEstablished

        UpdateStatus("1: Secure server " & ServerID.ToString() & " established on port " & TCPPort)

        ProxyClient = New SecureClient("tcp://localhost:19000/Test", AuthenticationKey)
        ProxyClient.Connect()

    End Sub

    Private Sub ProxyServer2_ServerEstablished(ByVal ServerID As System.Guid, ByVal TCPPort As Integer) Handles ProxyServer2.ServerEstablished

        UpdateStatus("2: Secure server " & ServerID.ToString() & " established on port " & TCPPort)

        ProxyClient2 = New SecureClient("tcp://localhost:19001/Test2", AuthenticationKey2)
        ProxyClient2.Connect()

    End Sub

    Private Sub ProxyServer_ClientConnected(ByVal ClientInstance As LocalClient) Handles ProxyServer.ClientConnected

        UpdateStatus("1: Client connected: " & ClientInstance.Description)

    End Sub

    Private Sub ProxyServer_ClientDisconnected(ByVal ClientInstance As LocalClient) Handles ProxyServer.ClientDisconnected

        UpdateStatus("1: Client disconnected: " & ClientInstance.Description)

    End Sub

    Private Sub ProxyServer_ClientNotification(ByVal Client As TVA.Remoting.LocalClient, ByVal sender As Object, ByVal e As System.EventArgs) Handles ProxyServer.ClientNotification

        UpdateStatus("1: Client Notification: " & TypeName(e))

    End Sub

    Private Sub ProxyServer2_ClientConnected(ByVal ClientInstance As LocalClient) Handles ProxyServer2.ClientConnected

        UpdateStatus("2: Client connected: " & ClientInstance.Description)

    End Sub

    Private Sub ProxyServer2_ClientDisconnected(ByVal ClientInstance As LocalClient) Handles ProxyServer2.ClientDisconnected

        UpdateStatus("2: Client disconnected: " & ClientInstance.Description)

    End Sub

    Private Sub ProxyServer2_ClientNotification(ByVal Client As TVA.Remoting.LocalClient, ByVal sender As Object, ByVal e As System.EventArgs) Handles ProxyServer2.ClientNotification

        UpdateStatus("2: Client Notification: " & TypeName(e))

    End Sub

    Private Sub UpdateStatus(Optional ByRef Status As String = "", Optional ByVal NewLine As Boolean = True)

        Dim strStatusText As String = StatusText.Text

        If NewLine Then
            strStatusText &= Status & vbCrLf & vbCrLf
        Else
            strStatusText &= Status
        End If

        'strStatusText = Microsoft.VisualBasic.Right(strStatusText, MaxStatusLength)
        StatusText.Text = strStatusText
        StatusText.SelectionStart = Len(strStatusText)
        StatusText.ScrollToCaret()

    End Sub

    Private Sub ProxyServer_SecureClientAuthenticated(ByVal ID As System.Guid) Handles ProxyServer.SecureClientAuthenticated

        UpdateStatus("1: Secure client authenticated: " & ID.ToString())

    End Sub

    Private Sub ProxyServer_SecureClientConnected(ByVal ID As System.Guid) Handles ProxyServer.SecureClientConnected

        UpdateStatus("1: Secure client connected: " & ID.ToString())

    End Sub

    Private Sub ProxyServer2_SecureClientAuthenticated(ByVal ID As System.Guid) Handles ProxyServer2.SecureClientAuthenticated

        UpdateStatus("2: Secure client authenticated: " & ID.ToString())

    End Sub

    Private Sub ProxyServer2_SecureClientConnected(ByVal ID As System.Guid) Handles ProxyServer2.SecureClientConnected

        UpdateStatus("2: Secure client connected: " & ID.ToString())

    End Sub

    Private Sub ProxyClient_ConnectionEstablished() Handles ProxyClient.ConnectionEstablished

        UpdateStatus("1: Connection established: " & Now())

    End Sub

    Private Sub ProxyClient_HostNotification(ByVal sender As Object, ByVal e As System.EventArgs) Handles ProxyClient.ServerNotification

        UpdateStatus("1: Host sent notification...")

    End Sub

    Private Sub ProxyClient_Notification(ByVal Text As String, ByVal NewLine As Boolean) Handles ProxyClient.StatusMessage

        UpdateStatus("1: Notfication: " & Text)

    End Sub

    Private Sub ProxyClient_ClientChannelSinkAuthenticated() Handles ProxyClient.SecureServerAuthentication

        UpdateStatus("1: Client channel sink authenticated")

    End Sub

    Private Sub ProxyClient_ClientChannelSinkConnected() Handles ProxyClient.SecureServerConnection

        UpdateStatus("1: Client channel sink connected")

    End Sub

    Private Sub ProxyClient_ConnectionTerminated() Handles ProxyClient.ConnectionTerminated

        UpdateStatus("1: Client connection terminated")

    End Sub

    Private Sub ProxyClient_AttemptingConnection() Handles ProxyClient.AttemptingConnection

        UpdateStatus("1: Client attempting connection...")

    End Sub

    Private Sub ProxyClient2_ConnectionEstablished() Handles ProxyClient2.ConnectionEstablished

        UpdateStatus("2: Connection established: " & Now())

    End Sub

    Private Sub ProxyClient2_HostNotification(ByVal sender As Object, ByVal e As System.EventArgs) Handles ProxyClient2.ServerNotification

        UpdateStatus("2: Host sent notification...")

    End Sub

    Private Sub ProxyClient2_Notification(ByVal Text As String, ByVal NewLine As Boolean) Handles ProxyClient2.StatusMessage

        UpdateStatus("2: Notfication: " & Text)

    End Sub

    Private Sub ProxyClient2_ClientChannelSinkAuthenticated() Handles ProxyClient2.SecureServerAuthentication

        UpdateStatus("2: Client channel sink authenticated")

    End Sub

    Private Sub ProxyClient2_ClientChannelSinkConnected() Handles ProxyClient2.SecureServerConnection

        UpdateStatus("2: Client channel sink connected")

    End Sub

    Private Sub ProxyClient2_ConnectionTerminated() Handles ProxyClient2.ConnectionTerminated

        UpdateStatus("2: Client connection terminated")

    End Sub

    Private Sub StatusText_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles StatusText.DoubleClick

        UpdateStatus("1: " & ProxyServer.Clients.GetClientList())
        UpdateStatus("2: " & ProxyServer2.Clients.GetClientList())

    End Sub

    Private Sub ProxyClient_ConnectionAttemptFailed(ByVal ex As System.Exception) Handles ProxyClient.ConnectionAttemptFailed

        UpdateStatus("1: Connection attempt failed: " & ex.Message)

    End Sub

    Private Sub ProxyClient2_ConnectionAttemptFailed(ByVal ex As System.Exception) Handles ProxyClient2.ConnectionAttemptFailed

        UpdateStatus("2: Connection attempt failed: " & ex.Message)

    End Sub

    Private Sub ProxyClient2_AttemptingConnection() Handles ProxyClient2.AttemptingConnection

        UpdateStatus("2: Client attempting connection...")

    End Sub

End Class