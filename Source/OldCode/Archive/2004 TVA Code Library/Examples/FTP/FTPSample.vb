Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Shared.Crypto
Imports TVA.Shared.String
Imports TVA.Config.Common
Imports VB = Microsoft.VisualBasic

Public Class FTPSample

    Inherits System.Windows.Forms.Form

    Private EncryptionKey As String = "ThisIsMyEncryptionKey:p!"

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
    Friend WithEvents ButtonRunTest As System.Windows.Forms.Button
    Friend WithEvents FtpSession As TVA.FTP.Session
    Friend WithEvents ProgressBar As System.Windows.Forms.ProgressBar
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim configurationAppSettings As System.Configuration.AppSettingsReader = New System.Configuration.AppSettingsReader
        Me.StatusText = New System.Windows.Forms.TextBox
        Me.ButtonRunTest = New System.Windows.Forms.Button
        Me.FtpSession = New TVA.FTP.Session
        Me.ProgressBar = New System.Windows.Forms.ProgressBar
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
        Me.StatusText.Size = New System.Drawing.Size(464, 240)
        Me.StatusText.TabIndex = 3
        Me.StatusText.Text = ""
        '
        'ButtonRunTest
        '
        Me.ButtonRunTest.Location = New System.Drawing.Point(480, 8)
        Me.ButtonRunTest.Name = "ButtonRunTest"
        Me.ButtonRunTest.TabIndex = 4
        Me.ButtonRunTest.Text = "&Run Test"
        '
        'FtpSession
        '
        Me.FtpSession.Port = CType(configurationAppSettings.GetValue("FtpSession.Port", GetType(System.Int32)), Integer)
        Me.FtpSession.Server = CType(configurationAppSettings.GetValue("FtpSession.Server", GetType(System.String)), String)
        '
        'ProgressBar
        '
        Me.ProgressBar.Location = New System.Drawing.Point(8, 256)
        Me.ProgressBar.Name = "ProgressBar"
        Me.ProgressBar.Size = New System.Drawing.Size(464, 23)
        Me.ProgressBar.TabIndex = 5
        '
        'FTPSample
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(560, 285)
        Me.Controls.Add(Me.ProgressBar)
        Me.Controls.Add(Me.ButtonRunTest)
        Me.Controls.Add(Me.StatusText)
        Me.Name = "FTPSample"
        Me.Text = "FTP Sample"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub FTPSample_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Create application variables used by sample if they don't already exist...
        Variables.Create("FTP.Username", "esocss", VariableType.Text, "Specifies the FTP Server Username")
        Variables.Create("FTP.Password", Encrypt("telegyr", EncryptionKey, EncryptLevel.Level4), VariableType.Text, "Specifies the FTP Server Password - this must be encrypted!")
        Variables.Create("FTP.InitialDirectory", "/apps/cluster/esocss/test/", VariableType.Text, "Specifies the desired initial FTP directory")
        Variables.Save()

    End Sub

    Private Sub ButtonRunTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonRunTest.Click

        Dim dblStart As Double
        Dim dblDuration As Double
        Dim strRemoteFileName As String = "TestFile.bin"
        Dim strLocalFileName As String = JustPath(Application.ExecutablePath) & strRemoteFileName
        Dim dblSize As Double = GetFileLength(strLocalFileName)

        Try
            ButtonRunTest.Enabled = False

            With FtpSession
                ' Connect to FTP server
                .Connect(Variables("FTP.Username"), Decrypt(Variables("FTP.Password"), EncryptionKey, EncryptLevel.Level4))
                .SetCurrentDirectory(Variables("FTP.InitialDirectory"))

                With .CurrentDirectory
                    ' Remove file from FTP server if it already exists
                    If Not .FindFile(strRemoteFileName) Is Nothing Then .RemoveFile(strRemoteFileName)
                    UpdateStatus("Uploading file to FTP server...")
                    dblStart = VB.Timer

                    ' Upload file to FTP server
                    .PutFile(strLocalFileName, strRemoteFileName)

                    dblDuration = VB.Timer - dblStart
                    UpdateStatus("Upload complete in " & SecondsToText(dblDuration, 2).ToLower())
                    UpdateStatus("Upload speed: " & ((dblSize / dblDuration * 8) / 1024 / 1024) & " Mbps")

                    ' Refresh file list on current directory
                    .Refresh()

                    UpdateStatus("Downloading file from FTP server...")
                    dblStart = VB.Timer

                    ' Download file from FTP server
                    .GetFile(strLocalFileName, strRemoteFileName)

                    dblDuration = VB.Timer - dblStart
                    UpdateStatus("Download complete in " & SecondsToText(dblDuration, 2).ToLower())
                    UpdateStatus("Download speed: " & ((dblSize / dblDuration * 8) / 1024 / 1024) & " Mbps")
                End With
            End With
        Catch ex As Exception
            UpdateStatus("Test failed fue to exception: " & ex.Message)
        Finally
            ButtonRunTest.Enabled = True
        End Try

    End Sub

    Private Sub UpdateStatus(Optional ByVal Status As String = "", Optional ByVal NewLine As Boolean = True)

        Const icMaxStatusLength As Integer = 262144
        Dim strStatusText As String = StatusText.Text

        If NewLine Then
            strStatusText &= Status & vbCrLf & vbCrLf
        Else
            strStatusText &= Status
        End If

        strStatusText = Microsoft.VisualBasic.Right(strStatusText, icMaxStatusLength)
        StatusText.Text = strStatusText
        StatusText.SelectionStart = Len(strStatusText)
        StatusText.ScrollToCaret()
        Application.DoEvents()

    End Sub

    Private Sub FtpSession_FileTransferProgress(ByVal TotalBytes As Long, ByVal TotalBytesTransfered As Long, ByVal TransferDirection As TVA.FTP.TransferDirection) Handles FtpSession.FileTransferProgress

        ProgressBar.Minimum = 0
        ProgressBar.Maximum = TotalBytes
        ProgressBar.Value = TotalBytesTransfered
        Application.DoEvents()

    End Sub

    Private Sub Session1_FileTransferProgress(ByVal TotalBytes As Long, ByVal TotalBytesTransfered As Long, ByVal TransferDirection As TVA.FTP.TransferDirection)

    End Sub
End Class