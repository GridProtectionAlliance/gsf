Imports TVA.Shared.Crypto
Imports System.Configuration

Public Class GenPasswordKey

    Inherits System.Windows.Forms.Form

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
    Friend WithEvents txtTypeName As System.Windows.Forms.TextBox
    Friend WithEvents lblTypeName As System.Windows.Forms.Label
    Friend WithEvents btnGenerate As System.Windows.Forms.Button
    Friend WithEvents lblGenKeyLabel As System.Windows.Forms.Label
    Friend WithEvents lblPasswordKey As System.Windows.Forms.Label
    Friend WithEvents lblKeyCopied As System.Windows.Forms.Label
    Private configurationAppSettings As System.Configuration.AppSettingsReader = New System.Configuration.AppSettingsReader
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(GenPasswordKey))
        Me.txtTypeName = New System.Windows.Forms.TextBox
        Me.lblTypeName = New System.Windows.Forms.Label
        Me.btnGenerate = New System.Windows.Forms.Button
        Me.lblGenKeyLabel = New System.Windows.Forms.Label
        Me.lblPasswordKey = New System.Windows.Forms.Label
        Me.lblKeyCopied = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'txtTypeName
        '
        Me.txtTypeName.AccessibleName = ""
        Me.txtTypeName.Location = New System.Drawing.Point(8, 24)
        Me.txtTypeName.Name = "txtTypeName"
        Me.txtTypeName.Size = New System.Drawing.Size(184, 20)
        Me.txtTypeName.TabIndex = 0
        Me.txtTypeName.Text = ""
        '
        'lblTypeName
        '
        Me.lblTypeName.Location = New System.Drawing.Point(8, 8)
        Me.lblTypeName.Name = "lblTypeName"
        Me.lblTypeName.Size = New System.Drawing.Size(184, 16)
        Me.lblTypeName.TabIndex = 1
        Me.lblTypeName.Text = "Enter password:"
        '
        'btnGenerate
        '
        Me.btnGenerate.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnGenerate.Location = New System.Drawing.Point(200, 24)
        Me.btnGenerate.Name = "btnGenerate"
        Me.btnGenerate.Size = New System.Drawing.Size(64, 24)
        Me.btnGenerate.TabIndex = 2
        Me.btnGenerate.Text = "&Generate"
        '
        'lblGenKeyLabel
        '
        Me.lblGenKeyLabel.Location = New System.Drawing.Point(8, 64)
        Me.lblGenKeyLabel.Name = "lblGenKeyLabel"
        Me.lblGenKeyLabel.Size = New System.Drawing.Size(256, 16)
        Me.lblGenKeyLabel.TabIndex = 3
        Me.lblGenKeyLabel.Text = "Generated password key:"
        '
        'lblPasswordKey
        '
        Me.lblPasswordKey.Font = New System.Drawing.Font("Verdana", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPasswordKey.Location = New System.Drawing.Point(8, 88)
        Me.lblPasswordKey.Name = "lblPasswordKey"
        Me.lblPasswordKey.Size = New System.Drawing.Size(256, 16)
        Me.lblPasswordKey.TabIndex = 4
        '
        'lblKeyCopied
        '
        Me.lblKeyCopied.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblKeyCopied.Location = New System.Drawing.Point(8, 112)
        Me.lblKeyCopied.Name = "lblKeyCopied"
        Me.lblKeyCopied.Size = New System.Drawing.Size(256, 16)
        Me.lblKeyCopied.TabIndex = 5
        Me.lblKeyCopied.Text = "Key copied onto clipboard..."
        Me.lblKeyCopied.TextAlign = System.Drawing.ContentAlignment.TopRight
        Me.lblKeyCopied.Visible = False
        '
        'GenPasswordKey
        '
        Me.AcceptButton = Me.btnGenerate
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(272, 127)
        Me.Controls.Add(Me.lblKeyCopied)
        Me.Controls.Add(Me.lblPasswordKey)
        Me.Controls.Add(Me.lblGenKeyLabel)
        Me.Controls.Add(Me.btnGenerate)
        Me.Controls.Add(Me.lblTypeName)
        Me.Controls.Add(Me.txtTypeName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "GenPasswordKey"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = " DatAWare PDC Key Generator"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Const PasswordKey As String = "B1864405-59C0-4157-AB38-0417AFDBD395"

    Private Sub btnGenerate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGenerate.Click

        ' Show encrypted password
        lblPasswordKey.Text = Encrypt(txtTypeName.Text, PasswordKey, EncryptLevel.Level4)

        ' Copy encrypted password onto clipboard
        Clipboard.SetDataObject(lblPasswordKey.Text, True)
        lblKeyCopied.Visible = True

    End Sub

End Class