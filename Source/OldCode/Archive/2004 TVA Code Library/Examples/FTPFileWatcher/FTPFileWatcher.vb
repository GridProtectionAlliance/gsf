Imports System.IO
Imports System.Text.RegularExpressions

Public Class FTPFileWatcher
    Inherits System.Windows.Forms.Form

    Const MaxStatusLength As Long = 262144
    'Private WithEvents FTPDirectory As FTP.Directory

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
    Friend WithEvents FileWatcher As TVA.FTP.FileWatcher
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.StatusText = New System.Windows.Forms.TextBox
        Me.FileWatcher = New TVA.FTP.FileWatcher
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
        Me.StatusText.Size = New System.Drawing.Size(840, 384)
        Me.StatusText.TabIndex = 2
        Me.StatusText.Text = ""
        '
        'FileWatcher
        '
        Me.FileWatcher.Directory = Nothing
        Me.FileWatcher.Server = "opsun"
        '
        'FTPFileWatcher
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(856, 397)
        Me.Controls.Add(Me.StatusText)
        Me.Name = "FTPFileWatcher"
        Me.Text = "FTP File Watcher Test"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub FTPFileWatcher_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        With FileWatcher
            .Directory = "/apps/cluster/esocss/test/"
            .Connect("esocss", "telegyr")

            'UpdateStatus("Known files in directory from separate session:")
            'FTPDirectory = .NewDirectorySession().CurrentDirectory
            'FTPDirectory.Refresh()
        End With

    End Sub

    Private Sub FileWatcher_FileAdded(ByVal FileReference As FTP.File) Handles FileWatcher.FileAdded

        With FileReference
            UpdateStatus("Added new file: " & .Name & " - " & .Size & " bytes, last modified: " & .TimeStamp)
        End With

    End Sub

    Private Sub FileWatcher_FileDeleted(ByVal FileReference As FTP.File) Handles FileWatcher.FileDeleted

        UpdateStatus("Deleted file: " & FileReference.Name)

    End Sub

    Private Sub FileWatcher_Status(ByVal StatusText As String) Handles FileWatcher.Status

        UpdateStatus(StatusText)

    End Sub

    Private Sub UpdateStatus(Optional ByRef Status As String = "", Optional ByVal NewLine As Boolean = True)

        Dim strStatusText As String = StatusText.Text

        If NewLine Then
            strStatusText &= Status & vbCrLf & vbCrLf
        Else
            strStatusText &= Status
        End If

        strStatusText = Microsoft.VisualBasic.Right(strStatusText, MaxStatusLength)
        StatusText.Text = strStatusText
        StatusText.SelectionStart = Len(strStatusText)
        StatusText.ScrollToCaret()

    End Sub

    'Private Sub FileWatcher_InternalSessionCommand(ByVal Command As String) Handles FileWatcher.InternalSessionCommand

    '    UpdateStatus(" Command: " & Command & vbCrLf, False)

    'End Sub

    'Private Sub FileWatcher_InternalSessionResponse(ByVal Response As String) Handles FileWatcher.InternalSessionResponse

    '    UpdateStatus("Response: " & Response & vbCrLf, False)

    'End Sub

    'Private Sub FTPDirectory_DirectoryLineScan(ByVal Line As String) Handles FTPDirectory.DirectoryListLineScan

    '    UpdateStatus("Line Queue Entry: " & Line)

    'End Sub

End Class
