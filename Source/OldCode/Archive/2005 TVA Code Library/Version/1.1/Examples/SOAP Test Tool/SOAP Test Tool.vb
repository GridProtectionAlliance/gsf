Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Reflection
Imports System.Reflection.Assembly
Imports System.Text
Imports System.Text.Encoding
Imports System.Collections.Specialized
Imports System.Threading.Thread
Imports TVA.Forms.Common
Imports TVA.Config.Common
Imports TVA.Shared.FilePath
Imports TVA.Shared.Crypto
Imports TVA.Threading
Imports TVA.Utilities
Imports VB = Microsoft.VisualBasic

Public Class SOAPTestTool

    Inherits System.Windows.Forms.Form

    Private Const MaximumStatusLength As Integer = 65536
    Private Const BufferSize As Integer = 616448

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
    Friend WithEvents dlgOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents lblVersion As System.Windows.Forms.Label
    Friend WithEvents txtSoapText As System.Windows.Forms.TextBox
    Friend WithEvents tabOptions As System.Windows.Forms.TabControl
    Friend WithEvents tabClient As System.Windows.Forms.TabPage
    Friend WithEvents tabServer As System.Windows.Forms.TabPage
    Friend WithEvents lblSoapAction As System.Windows.Forms.Label
    Friend WithEvents txtSoapAction As System.Windows.Forms.TextBox
    Friend WithEvents txtPostingURL As System.Windows.Forms.TextBox
    Friend WithEvents lblPostingURL As System.Windows.Forms.Label
    Friend WithEvents txtSelectedFile As System.Windows.Forms.TextBox
    Friend WithEvents lblSelectFile As System.Windows.Forms.Label
    Friend WithEvents btnSelectFile As System.Windows.Forms.Button
    Friend WithEvents txtServerPort As System.Windows.Forms.TextBox
    Friend WithEvents lblSelectServerPort As System.Windows.Forms.Label
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents txtStatus As System.Windows.Forms.TextBox
    Friend WithEvents ErrorProvider As System.Windows.Forms.ErrorProvider
    Friend WithEvents btnPost As System.Windows.Forms.Button
    Friend WithEvents lblUserName As System.Windows.Forms.Label
    Friend WithEvents txtUserName As System.Windows.Forms.TextBox
    Friend WithEvents lblPassword As System.Windows.Forms.Label
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents WebServer As TVA.Utilities.Web.Server
    Friend WithEvents lblThreadsInUseText As System.Windows.Forms.Label
    Friend WithEvents lblThreadsInUse As System.Windows.Forms.Label
    Friend WithEvents ThreadUseTimer As System.Timers.Timer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog
        Me.lblVersion = New System.Windows.Forms.Label
        Me.txtSoapText = New System.Windows.Forms.TextBox
        Me.tabOptions = New System.Windows.Forms.TabControl
        Me.tabClient = New System.Windows.Forms.TabPage
        Me.lblPassword = New System.Windows.Forms.Label
        Me.txtPassword = New System.Windows.Forms.TextBox
        Me.lblUserName = New System.Windows.Forms.Label
        Me.txtUserName = New System.Windows.Forms.TextBox
        Me.txtSelectedFile = New System.Windows.Forms.TextBox
        Me.lblSelectFile = New System.Windows.Forms.Label
        Me.btnSelectFile = New System.Windows.Forms.Button
        Me.lblSoapAction = New System.Windows.Forms.Label
        Me.txtSoapAction = New System.Windows.Forms.TextBox
        Me.txtPostingURL = New System.Windows.Forms.TextBox
        Me.btnPost = New System.Windows.Forms.Button
        Me.lblPostingURL = New System.Windows.Forms.Label
        Me.tabServer = New System.Windows.Forms.TabPage
        Me.txtStatus = New System.Windows.Forms.TextBox
        Me.btnStart = New System.Windows.Forms.Button
        Me.txtServerPort = New System.Windows.Forms.TextBox
        Me.lblSelectServerPort = New System.Windows.Forms.Label
        Me.ErrorProvider = New System.Windows.Forms.ErrorProvider
        Me.WebServer = New TVA.Utilities.Web.Server
        Me.lblThreadsInUseText = New System.Windows.Forms.Label
        Me.lblThreadsInUse = New System.Windows.Forms.Label
        Me.ThreadUseTimer = New System.Timers.Timer
        Me.tabOptions.SuspendLayout()
        Me.tabClient.SuspendLayout()
        Me.tabServer.SuspendLayout()
        CType(Me.ThreadUseTimer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.Multiselect = True
        '
        'lblVersion
        '
        Me.lblVersion.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblVersion.Location = New System.Drawing.Point(472, 8)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(176, 16)
        Me.lblVersion.TabIndex = 2
        Me.lblVersion.Text = "Version: 1.0.0.0"
        Me.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'txtSoapText
        '
        Me.txtSoapText.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSoapText.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSoapText.Location = New System.Drawing.Point(8, 192)
        Me.txtSoapText.Multiline = True
        Me.txtSoapText.Name = "txtSoapText"
        Me.txtSoapText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtSoapText.Size = New System.Drawing.Size(638, 256)
        Me.txtSoapText.TabIndex = 1
        Me.txtSoapText.Text = ""
        '
        'tabOptions
        '
        Me.tabOptions.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tabOptions.Controls.Add(Me.tabClient)
        Me.tabOptions.Controls.Add(Me.tabServer)
        Me.tabOptions.Location = New System.Drawing.Point(8, 8)
        Me.tabOptions.Name = "tabOptions"
        Me.tabOptions.SelectedIndex = 0
        Me.tabOptions.Size = New System.Drawing.Size(638, 176)
        Me.tabOptions.TabIndex = 0
        '
        'tabClient
        '
        Me.tabClient.Controls.Add(Me.lblPassword)
        Me.tabClient.Controls.Add(Me.txtPassword)
        Me.tabClient.Controls.Add(Me.lblUserName)
        Me.tabClient.Controls.Add(Me.txtUserName)
        Me.tabClient.Controls.Add(Me.txtSelectedFile)
        Me.tabClient.Controls.Add(Me.lblSelectFile)
        Me.tabClient.Controls.Add(Me.btnSelectFile)
        Me.tabClient.Controls.Add(Me.lblSoapAction)
        Me.tabClient.Controls.Add(Me.txtSoapAction)
        Me.tabClient.Controls.Add(Me.txtPostingURL)
        Me.tabClient.Controls.Add(Me.btnPost)
        Me.tabClient.Controls.Add(Me.lblPostingURL)
        Me.tabClient.Location = New System.Drawing.Point(4, 22)
        Me.tabClient.Name = "tabClient"
        Me.tabClient.Size = New System.Drawing.Size(630, 150)
        Me.tabClient.TabIndex = 0
        Me.tabClient.Text = "Client"
        '
        'lblPassword
        '
        Me.lblPassword.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblPassword.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblPassword.Location = New System.Drawing.Point(416, 104)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Size = New System.Drawing.Size(136, 16)
        Me.lblPassword.TabIndex = 10
        Me.lblPassword.Text = "&Password:"
        Me.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'txtPassword
        '
        Me.txtPassword.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtPassword.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPassword.Location = New System.Drawing.Point(416, 120)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.PasswordChar = Microsoft.VisualBasic.ChrW(42)
        Me.txtPassword.Size = New System.Drawing.Size(200, 21)
        Me.txtPassword.TabIndex = 11
        Me.txtPassword.Text = ""
        '
        'lblUserName
        '
        Me.lblUserName.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblUserName.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblUserName.Location = New System.Drawing.Point(208, 104)
        Me.lblUserName.Name = "lblUserName"
        Me.lblUserName.Size = New System.Drawing.Size(136, 16)
        Me.lblUserName.TabIndex = 8
        Me.lblUserName.Text = "User &Name:"
        Me.lblUserName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'txtUserName
        '
        Me.txtUserName.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtUserName.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtUserName.Location = New System.Drawing.Point(208, 120)
        Me.txtUserName.Name = "txtUserName"
        Me.txtUserName.Size = New System.Drawing.Size(200, 21)
        Me.txtUserName.TabIndex = 9
        Me.txtUserName.Text = ""
        '
        'txtSelectedFile
        '
        Me.txtSelectedFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSelectedFile.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSelectedFile.Location = New System.Drawing.Point(8, 24)
        Me.txtSelectedFile.Name = "txtSelectedFile"
        Me.txtSelectedFile.Size = New System.Drawing.Size(512, 21)
        Me.txtSelectedFile.TabIndex = 1
        Me.txtSelectedFile.Text = ""
        '
        'lblSelectFile
        '
        Me.lblSelectFile.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblSelectFile.Location = New System.Drawing.Point(8, 8)
        Me.lblSelectFile.Name = "lblSelectFile"
        Me.lblSelectFile.Size = New System.Drawing.Size(112, 16)
        Me.lblSelectFile.TabIndex = 0
        Me.lblSelectFile.Text = "Select Test &File:"
        Me.lblSelectFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'btnSelectFile
        '
        Me.btnSelectFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSelectFile.Font = New System.Drawing.Font("Verdana", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSelectFile.Location = New System.Drawing.Point(520, 24)
        Me.btnSelectFile.Name = "btnSelectFile"
        Me.btnSelectFile.Size = New System.Drawing.Size(30, 21)
        Me.btnSelectFile.TabIndex = 2
        Me.btnSelectFile.Text = "..."
        '
        'lblSoapAction
        '
        Me.lblSoapAction.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblSoapAction.Location = New System.Drawing.Point(8, 104)
        Me.lblSoapAction.Name = "lblSoapAction"
        Me.lblSoapAction.Size = New System.Drawing.Size(136, 16)
        Me.lblSoapAction.TabIndex = 6
        Me.lblSoapAction.Text = "Select SOAP &Action:"
        Me.lblSoapAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'txtSoapAction
        '
        Me.txtSoapAction.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSoapAction.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSoapAction.Location = New System.Drawing.Point(8, 120)
        Me.txtSoapAction.Name = "txtSoapAction"
        Me.txtSoapAction.Size = New System.Drawing.Size(192, 21)
        Me.txtSoapAction.TabIndex = 7
        Me.txtSoapAction.Text = "urn:#"
        '
        'txtPostingURL
        '
        Me.txtPostingURL.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtPostingURL.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPostingURL.Location = New System.Drawing.Point(8, 72)
        Me.txtPostingURL.Name = "txtPostingURL"
        Me.txtPostingURL.Size = New System.Drawing.Size(608, 21)
        Me.txtPostingURL.TabIndex = 5
        Me.txtPostingURL.Text = ""
        '
        'btnPost
        '
        Me.btnPost.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPost.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnPost.Location = New System.Drawing.Point(560, 24)
        Me.btnPost.Name = "btnPost"
        Me.btnPost.Size = New System.Drawing.Size(56, 24)
        Me.btnPost.TabIndex = 3
        Me.btnPost.Text = "Post!"
        '
        'lblPostingURL
        '
        Me.lblPostingURL.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblPostingURL.Location = New System.Drawing.Point(8, 56)
        Me.lblPostingURL.Name = "lblPostingURL"
        Me.lblPostingURL.Size = New System.Drawing.Size(136, 16)
        Me.lblPostingURL.TabIndex = 4
        Me.lblPostingURL.Text = "Select Posting &URL:"
        Me.lblPostingURL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'tabServer
        '
        Me.tabServer.Controls.Add(Me.lblThreadsInUse)
        Me.tabServer.Controls.Add(Me.lblThreadsInUseText)
        Me.tabServer.Controls.Add(Me.txtStatus)
        Me.tabServer.Controls.Add(Me.btnStart)
        Me.tabServer.Controls.Add(Me.txtServerPort)
        Me.tabServer.Controls.Add(Me.lblSelectServerPort)
        Me.tabServer.Location = New System.Drawing.Point(4, 22)
        Me.tabServer.Name = "tabServer"
        Me.tabServer.Size = New System.Drawing.Size(630, 150)
        Me.tabServer.TabIndex = 1
        Me.tabServer.Text = "Server"
        '
        'txtStatus
        '
        Me.txtStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtStatus.BackColor = System.Drawing.Color.Black
        Me.txtStatus.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtStatus.ForeColor = System.Drawing.Color.White
        Me.txtStatus.Location = New System.Drawing.Point(8, 40)
        Me.txtStatus.MaxLength = 0
        Me.txtStatus.Multiline = True
        Me.txtStatus.Name = "txtStatus"
        Me.txtStatus.ReadOnly = True
        Me.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtStatus.Size = New System.Drawing.Size(616, 104)
        Me.txtStatus.TabIndex = 3
        Me.txtStatus.TabStop = False
        Me.txtStatus.Text = ""
        '
        'btnStart
        '
        Me.btnStart.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.btnStart.Location = New System.Drawing.Point(216, 8)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(64, 24)
        Me.btnStart.TabIndex = 2
        Me.btnStart.Text = "Start!"
        '
        'txtServerPort
        '
        Me.txtServerPort.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtServerPort.Location = New System.Drawing.Point(136, 8)
        Me.txtServerPort.Name = "txtServerPort"
        Me.txtServerPort.Size = New System.Drawing.Size(64, 21)
        Me.txtServerPort.TabIndex = 1
        Me.txtServerPort.Text = "8080"
        '
        'lblSelectServerPort
        '
        Me.lblSelectServerPort.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblSelectServerPort.Location = New System.Drawing.Point(6, 12)
        Me.lblSelectServerPort.Name = "lblSelectServerPort"
        Me.lblSelectServerPort.Size = New System.Drawing.Size(128, 16)
        Me.lblSelectServerPort.TabIndex = 0
        Me.lblSelectServerPort.Text = "Select Server &Port:"
        Me.lblSelectServerPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ErrorProvider
        '
        Me.ErrorProvider.ContainerControl = Me
        '
        'WebServer
        '
        '
        'lblThreadsInUseText
        '
        Me.lblThreadsInUseText.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblThreadsInUseText.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblThreadsInUseText.Location = New System.Drawing.Point(472, 12)
        Me.lblThreadsInUseText.Name = "lblThreadsInUseText"
        Me.lblThreadsInUseText.Size = New System.Drawing.Size(128, 16)
        Me.lblThreadsInUseText.TabIndex = 4
        Me.lblThreadsInUseText.Text = "Threads In Use:"
        Me.lblThreadsInUseText.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblThreadsInUse
        '
        Me.lblThreadsInUse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblThreadsInUse.AutoSize = True
        Me.lblThreadsInUse.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
        Me.lblThreadsInUse.Location = New System.Drawing.Point(608, 12)
        Me.lblThreadsInUse.Name = "lblThreadsInUse"
        Me.lblThreadsInUse.Size = New System.Drawing.Size(12, 18)
        Me.lblThreadsInUse.TabIndex = 5
        Me.lblThreadsInUse.Text = "0"
        Me.lblThreadsInUse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ThreadUseTimer
        '
        Me.ThreadUseTimer.Interval = 1000
        Me.ThreadUseTimer.SynchronizingObject = Me
        '
        'SOAPTestTool
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(656, 453)
        Me.Controls.Add(Me.txtSoapText)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.tabOptions)
        Me.MaximizeBox = False
        Me.MinimumSize = New System.Drawing.Size(424, 400)
        Me.Name = "SOAPTestTool"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SOAP Test Tool"
        Me.tabOptions.ResumeLayout(False)
        Me.tabClient.ResumeLayout(False)
        Me.tabServer.ResumeLayout(False)
        CType(Me.ThreadUseTimer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub SOAPTestTool_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Load form title from assembly information
        Me.Text = DirectCast(GetExecutingAssembly.GetCustomAttributes(GetType(AssemblyTitleAttribute), False)(0), AssemblyTitleAttribute).Title

        ' Display compiled assembly version
        With FileVersionInfo.GetVersionInfo(GetExecutingAssembly.Location)
            lblVersion.Text = "Version: " & .FileMajorPart & "." & .FileMinorPart & "." & .FileBuildPart '& "." & .FilePrivatePart
        End With

        Variables.Create("Last.SelectedFile", "", VariableType.Text)
        Variables.Create("Last.PostingURL", "", VariableType.Text)
        Variables.Create("Last.SoapAction", txtSoapAction.Text, VariableType.Text)
        Variables.Create("Last.ServerPort", txtServerPort.Text, VariableType.Int)
        Variables.Create("Last.UserName", "", VariableType.Text)
        Variables.Create("Last.Password", "", VariableType.Text)
        Variables.Save()

        txtSelectedFile.Text = Variables("Last.SelectedFile")
        txtPostingURL.Text = Variables("Last.PostingURL")
        txtSoapAction.Text = Variables("Last.SoapAction")
        txtServerPort.Text = Variables("Last.ServerPort")
        txtUserName.Text = Variables("Last.UserName")
        txtPassword.Text = Decrypt(Variables("Last.Password"))

        RestoreWindowSettings(Me)

    End Sub

    Private Sub SOAPTestTool_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed

        SaveWindowSettings(Me)

        Variables("Last.SelectedFile") = txtSelectedFile.Text
        Variables("Last.PostingURL") = txtPostingURL.Text
        Variables("Last.SoapAction") = txtSoapAction.Text
        Variables("Last.ServerPort") = txtServerPort.Text
        Variables("Last.UserName") = txtUserName.Text
        Variables("Last.Password") = Encrypt(txtPassword.Text)

        End

    End Sub

    Private Sub UpdateStatus(Optional ByVal Status As String = "", Optional ByVal NewLine As Boolean = True)

        If NewLine Then
            Status = txtStatus.Text & Status & vbCrLf
        Else
            Status = txtStatus.Text & Status
        End If

        Status = VB.Right(Status, MaximumStatusLength)
        txtStatus.Text = Status
        txtStatus.SelectionStart = Len(Status)
        txtStatus.ScrollToCaret()

    End Sub

    Private Sub btnSelectFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectFile.Click

        With dlgOpenFile
            .InitialDirectory = Application.StartupPath
            .Filter = "Test SOAP post file (*.xml)|*.xml|All files (*.*)|*.*"
            .FilterIndex = 1
            .RestoreDirectory = False

            If .ShowDialog() = DialogResult.OK Then txtSelectedFile.Text = .FileName
        End With

    End Sub

    Private Sub txtSelectedFile_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSelectedFile.TextChanged

        If File.Exists(txtSelectedFile.Text) Then
            Dim textBuffer As New StringBuilder
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim read As Integer

            With File.OpenRead(txtSelectedFile.Text)
                ' Read initial buffer from source file
                read = .Read(buffer, 0, BufferSize)

                While read > 0
                    ' Add buffer to string
                    textBuffer.Append(UTF8.GetChars(buffer, 0, read))

                    ' Read next buffer from source file
                    read = .Read(buffer, 0, BufferSize)
                End While

                .Close()
            End With

            txtSoapText.Text = textBuffer.ToString()
        End If

    End Sub

    Private Sub btnGO_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPost.Click

        If Len(txtSoapText.Text) = 0 Then
            MsgBox("No SOAP text to post", MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, Me.Text)
            Exit Sub
        End If

        If Len(txtPostingURL.Text) = 0 Then
            MsgBox("You must specify a posting URL", MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, Me.Text)
            Exit Sub
        End If

        If Len(txtSoapAction.Text) = 0 Then
            MsgBox("You must specify a SOAP action", MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, Me.Text)
            Exit Sub
        End If

        Try
            With New WebClient
                ' Set access credientials if specified
                If Len(txtUserName.Text) > 0 Then .Credentials = New NetworkCredential(txtUserName.Text, txtPassword.Text)

                ' Add needed headers for a SOAP post
                .Headers.Add("Content-Type", "text/xml; charset=utf-8")
                .Headers.Add("SOAPAction", txtSoapAction.Text)

                Dim postResponse As String
                Dim response As New StringBuilder

                ' Post SOAP text to URL
                response.Append("Response:")
                response.Append(vbCrLf)
                response.Append(vbCrLf)
                postResponse = UTF8.GetString(.UploadData(txtPostingURL.Text, "POST", UTF8.GetBytes(txtSoapText.Text)))

                If Not .ResponseHeaders Is Nothing Then
                    With .ResponseHeaders
                        For x As Integer = 0 To .Count - 1
                            ' Display the headers as name/value pairs
                            response.Append(.GetKey(x))
                            response.Append(": ")
                            response.Append(.Get(x))
                            response.Append(vbCrLf)
                        Next
                    End With
                End If

                response.Append(vbCrLf)
                response.Append(postResponse)

                txtSoapText.Text = response.ToString()
            End With

            MsgBox("Test file posted successfully!", MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, Me.Text)
        Catch ex As Exception
            MsgBox("Test file failed to post due to exception: " & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, Me.Text)
        End Try

    End Sub

    Private Sub btnStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStart.Click

        If WebServer.IsRunning Then
            WebServer.Stop()
            ThreadUseTimer.Enabled = False
            lblThreadsInUse.Text = 0
            btnStart.Text = "Start!"
        Else
            WebServer.Port = txtServerPort.Text
            WebServer.Start()
            ThreadUseTimer.Enabled = True
            btnStart.Text = "Stop!"
        End If

    End Sub

    Private Sub txtServerPort_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtServerPort.Validating

        If Len(txtServerPort.Text) > 0 Then
            If IsNumeric(txtServerPort.Text) Then
                ErrorProvider.SetError(txtServerPort, "")
            Else
                e.Cancel = True
                ErrorProvider.SetError(txtServerPort, "Server port is not a number!")
                txtServerPort.Select(0, txtServerPort.Text.Length)
                txtServerPort.Focus()
            End If
        Else
            e.Cancel = True
            ErrorProvider.SetError(txtServerPort, "Server port cannot be blank!")
            txtServerPort.Focus()
        End If

    End Sub

    Private Sub WebServer_ProcessRequest(ByVal request As Web.Server.HttpRequest, ByVal response As Web.Server.HttpResponse) Handles WebServer.ProcessRequest

        With New StringBuilder
            .Append("Request: ")
            .Append(request.RequestedURI)
            .Append(vbCrLf)
            .Append(vbCrLf)
            For Each header As DictionaryEntry In request.Headers
                ' Display the headers as name/value pairs
                .Append(header.Key)
                .Append(": ")
                .Append(header.Value)
                .Append(vbCrLf)
            Next
            .Append(vbCrLf)
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), request.Content.Length)            
            request.Content.Read(buffer, 0, request.Content.Length)
            .Append(WebServer.TextEncoding.GetString(buffer))
            txtSoapText.Text = .ToString()
        End With

    End Sub

    Private Sub WebServer_StatusMessage(ByVal message As String) Handles WebServer.StatusMessage

        UpdateStatus(message)

    End Sub

    Private Sub ThreadUseTimer_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ThreadUseTimer.Elapsed

        lblThreadsInUse.Text = WebServer.ThreadsInUse

    End Sub

    'Private Function DownloadTextFile(ByVal url As String, ByVal userName As String, ByVal password As String) As String

    '    Dim textBuffer As New StringBuilder
    '    Const bufferSize As Integer = 524288 ' 512Kb buffer

    '    With New WebClient
    '        ' Set access credientials if specified
    '        If Len(userName) > 0 Then .Credentials = New NetworkCredential(userName, password)

    '        Dim dataStream As Stream = .OpenRead(url)
    '        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), bufferSize)
    '        Dim read As Integer

    '        ' Load the data stream into the text buffer
    '        read = dataStream.Read(buffer, 0, bufferSize)

    '        Do While read > 0
    '            ' Assume incoming buffer is in a UTF8 format, convert it to Unicode and add it to our text buffer
    '            textBuffer.Append(UTF8.GetChars(buffer, 0, read))
    '            read = dataStream.Read(buffer, 0, bufferSize)
    '        Loop

    '        ' Close the stream
    '        dataStream.Close()
    '    End With

    '    Return textBuffer.ToString()

    'End Function

End Class
