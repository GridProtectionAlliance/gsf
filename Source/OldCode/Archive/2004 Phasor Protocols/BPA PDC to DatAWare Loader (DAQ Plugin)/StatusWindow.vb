Imports System.IO
Imports System.Text
Imports System.Reflection
Imports System.Reflection.Assembly
Imports System.Runtime.InteropServices
Imports VB = Microsoft.VisualBasic

<ComVisible(False)> _
Public Class StatusWindow

    Inherits System.Windows.Forms.Form

    Friend ParentInterface As [Interface]
    Private Const MaximumStatusLength As Integer = 65536

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
    Friend WithEvents ServiceStatusLabel As System.Windows.Forms.Label
    Friend WithEvents VersionLabel As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(StatusWindow))
        Me.StatusText = New System.Windows.Forms.TextBox
        Me.ServiceStatusLabel = New System.Windows.Forms.Label
        Me.VersionLabel = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'StatusText
        '
        Me.StatusText.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.StatusText.BackColor = System.Drawing.Color.Black
        Me.StatusText.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StatusText.ForeColor = System.Drawing.Color.White
        Me.StatusText.Location = New System.Drawing.Point(8, 16)
        Me.StatusText.MaxLength = 0
        Me.StatusText.Multiline = True
        Me.StatusText.Name = "StatusText"
        Me.StatusText.ReadOnly = True
        Me.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.StatusText.Size = New System.Drawing.Size(722, 218)
        Me.StatusText.TabIndex = 19
        Me.StatusText.TabStop = False
        Me.StatusText.Text = ""
        '
        'ServiceStatusLabel
        '
        Me.ServiceStatusLabel.Location = New System.Drawing.Point(8, 0)
        Me.ServiceStatusLabel.Name = "ServiceStatusLabel"
        Me.ServiceStatusLabel.Size = New System.Drawing.Size(224, 16)
        Me.ServiceStatusLabel.TabIndex = 17
        Me.ServiceStatusLabel.Text = "DatAWare DAQ Service Status:"
        '
        'VersionLabel
        '
        Me.VersionLabel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.VersionLabel.Location = New System.Drawing.Point(504, 0)
        Me.VersionLabel.Name = "VersionLabel"
        Me.VersionLabel.Size = New System.Drawing.Size(224, 16)
        Me.VersionLabel.TabIndex = 18
        Me.VersionLabel.Text = "Version: 1.0.0.0"
        Me.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'StatusWindow
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(736, 237)
        Me.Controls.Add(Me.StatusText)
        Me.Controls.Add(Me.ServiceStatusLabel)
        Me.Controls.Add(Me.VersionLabel)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "StatusWindow"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "StatusWindow"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub StatusWindow_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Load form title from assembly information
        Me.Text = DirectCast(GetExecutingAssembly.GetCustomAttributes(GetType(AssemblyTitleAttribute), False)(0), AssemblyTitleAttribute).Title & " Status Window"

        ' Display compiled assembly version
        With FileVersionInfo.GetVersionInfo(GetExecutingAssembly.Location)
            VersionLabel.Text = "Version: " & .FileMajorPart & "." & .FileMinorPart & "." & .FileBuildPart & "." & .FilePrivatePart
        End With

        UpdateStatus("Status window loaded [" & Now() & "]")

    End Sub

    Public WriteOnly Property Instance() As Integer
        Set(ByVal Value As Integer)
            Me.Text = "[Instance " & Value & "] " & Me.Text
        End Set
    End Property

    Protected Overrides Sub OnClosing(ByVal e As System.ComponentModel.CancelEventArgs)

        ' We don't ever close this form, we just hide it...
        Me.Hide()
        e.Cancel = True

    End Sub

    ' We show conversion stats when user double clicks on status window...
    Private Sub StatusWindow_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.DoubleClick

        Try
            UpdateStatus(ParentInterface.converter.Status)
        Catch ex As Exception
            UpdateStatus("Failed to show stats due to exception: " & ex.Message)
        End Try

    End Sub

    Public Sub UpdateStatus(Optional ByVal Status As String = "", Optional ByVal NewLine As Boolean = True)

        If NewLine Then
            Status = StatusText.Text & Status & vbCrLf
        Else
            Status = StatusText.Text & Status
        End If

        Status = VB.Right(Status, MaximumStatusLength)
        StatusText.Text = Status
        StatusText.SelectionStart = Len(Status)
        StatusText.ScrollToCaret()

    End Sub

End Class
