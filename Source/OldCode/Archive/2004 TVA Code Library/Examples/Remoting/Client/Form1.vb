Imports TVA.Remoting

Public Class Form1

    Inherits System.Windows.Forms.Form

    Friend Const AuthenticationKey As String = "96B86EA1-4BFF-4068-BAEB-6F84E46C570D"
    Friend Const AuthenticationKey2 As String = "96B86EA1-4BFF-4068-BAEB-6F84E46C570D"
    'Friend Const AuthenticationKey2 As String = "85B86EA1-3AFF-3968-CBEB-5D84E46C570D"

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
        Me.StatusText.TabIndex = 1
        Me.StatusText.Text = ""
        '
        'Form1
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(632, 429)
        Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.StatusText})
        Me.Name = "Form1"
        Me.Text = "Proxy Client Test"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ProxyClient = New SecureClient("tcp://localhost:19000/Test", AuthenticationKey)
        ProxyClient2 = New SecureClient("tcp://localhost:19001/Test2", AuthenticationKey2)

        ProxyClient.Connect()
        ProxyClient2.Connect()

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

    Private Sub ProxyClient2_ConnectionEstablished() Handles ProxyClient2.ConnectionEstablished

        UpdateStatus("2: Connection established: " & Now())

    End Sub

    Private Sub ProxyClient2_HostNotification(ByVal sender As Object, ByVal e As System.EventArgs) Handles ProxyClient2.ServerNotification

        UpdateStatus("2: Host sent notification...")

    End Sub

    Private Sub ProxyClient2_Notification(ByVal Text As String, ByVal NewLine As Boolean) Handles ProxyClient2.StatusMessage

        UpdateStatus("2: Notfication: " & Text)

    End Sub

    Private Sub UpdateStatus(Optional ByVal Status As String = "", Optional ByVal NewLine As Boolean = True)

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

    Private Sub ProxyClient_SecureServerAuthentication() Handles ProxyClient.SecureServerAuthentication

        UpdateStatus("1: Client channel sink authenticated")

    End Sub

    Private Sub ProxyClient_SecureServerConnection() Handles ProxyClient.SecureServerConnection

        UpdateStatus("1: Client channel sink connected")

    End Sub

    Private Sub ProxyClient_ConnectionTerminated() Handles ProxyClient.ConnectionTerminated

        UpdateStatus("1: Client connection terminated")

    End Sub

End Class
