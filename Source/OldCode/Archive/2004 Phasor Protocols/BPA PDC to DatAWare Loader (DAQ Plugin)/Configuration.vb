Imports System.IO
Imports System.Runtime.InteropServices
Imports TVA.Config.Common
Imports TVA.Shared.DateTime
Imports TVA.Shared.FilePath

<ComVisible(False)> _
Public Class Configuration
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
    Friend WithEvents timeZone As System.Windows.Forms.ComboBox
    Friend WithEvents timeZoneLabel As System.Windows.Forms.Label
    Friend WithEvents pointListFileLabel As System.Windows.Forms.Label
    Friend WithEvents pdcConfigFileLabel As System.Windows.Forms.Label
    Friend WithEvents pdcDataPortLabel As System.Windows.Forms.Label
    Friend WithEvents pointListFile As System.Windows.Forms.TextBox
    Friend WithEvents pdcConfigFile As System.Windows.Forms.TextBox
    Friend WithEvents pdcDataPort As System.Windows.Forms.TextBox
    Friend WithEvents findPointListFileButton As System.Windows.Forms.Button
    Friend WithEvents findPDCConfigFileButton As System.Windows.Forms.Button
    Friend WithEvents errorProvider As System.Windows.Forms.ErrorProvider
    Friend WithEvents saveButton As System.Windows.Forms.Button
    Friend WithEvents escapeButton As System.Windows.Forms.Button
    Friend WithEvents openFileDialog As System.Windows.Forms.OpenFileDialog
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(Configuration))
        Me.timeZone = New System.Windows.Forms.ComboBox
        Me.timeZoneLabel = New System.Windows.Forms.Label
        Me.pointListFileLabel = New System.Windows.Forms.Label
        Me.pdcConfigFileLabel = New System.Windows.Forms.Label
        Me.pdcDataPortLabel = New System.Windows.Forms.Label
        Me.pointListFile = New System.Windows.Forms.TextBox
        Me.pdcConfigFile = New System.Windows.Forms.TextBox
        Me.pdcDataPort = New System.Windows.Forms.TextBox
        Me.findPointListFileButton = New System.Windows.Forms.Button
        Me.findPDCConfigFileButton = New System.Windows.Forms.Button
        Me.saveButton = New System.Windows.Forms.Button
        Me.escapeButton = New System.Windows.Forms.Button
        Me.errorProvider = New System.Windows.Forms.ErrorProvider
        Me.openFileDialog = New System.Windows.Forms.OpenFileDialog
        Me.SuspendLayout()
        '
        'timeZone
        '
        Me.timeZone.Location = New System.Drawing.Point(96, 8)
        Me.timeZone.Name = "timeZone"
        Me.timeZone.Size = New System.Drawing.Size(200, 21)
        Me.timeZone.TabIndex = 1
        Me.timeZone.Text = "TimeZone"
        '
        'timeZoneLabel
        '
        Me.timeZoneLabel.Location = New System.Drawing.Point(0, 8)
        Me.timeZoneLabel.Name = "timeZoneLabel"
        Me.timeZoneLabel.Size = New System.Drawing.Size(88, 23)
        Me.timeZoneLabel.TabIndex = 0
        Me.timeZoneLabel.Text = "&Time Zone:"
        Me.timeZoneLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'pointListFileLabel
        '
        Me.pointListFileLabel.Location = New System.Drawing.Point(0, 40)
        Me.pointListFileLabel.Name = "pointListFileLabel"
        Me.pointListFileLabel.Size = New System.Drawing.Size(88, 23)
        Me.pointListFileLabel.TabIndex = 2
        Me.pointListFileLabel.Text = "&Point List File:"
        Me.pointListFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'pdcConfigFileLabel
        '
        Me.pdcConfigFileLabel.Location = New System.Drawing.Point(0, 72)
        Me.pdcConfigFileLabel.Name = "pdcConfigFileLabel"
        Me.pdcConfigFileLabel.Size = New System.Drawing.Size(88, 23)
        Me.pdcConfigFileLabel.TabIndex = 5
        Me.pdcConfigFileLabel.Text = "PDC C&onfig File:"
        Me.pdcConfigFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'pdcDataPortLabel
        '
        Me.pdcDataPortLabel.Location = New System.Drawing.Point(0, 104)
        Me.pdcDataPortLabel.Name = "pdcDataPortLabel"
        Me.pdcDataPortLabel.Size = New System.Drawing.Size(88, 23)
        Me.pdcDataPortLabel.TabIndex = 8
        Me.pdcDataPortLabel.Text = "PDC &Data Port:"
        Me.pdcDataPortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'pointListFile
        '
        Me.pointListFile.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower
        Me.errorProvider.SetIconAlignment(Me.pointListFile, System.Windows.Forms.ErrorIconAlignment.MiddleLeft)
        Me.errorProvider.SetIconPadding(Me.pointListFile, 3)
        Me.pointListFile.Location = New System.Drawing.Point(96, 40)
        Me.pointListFile.Name = "pointListFile"
        Me.pointListFile.Size = New System.Drawing.Size(170, 20)
        Me.pointListFile.TabIndex = 3
        Me.pointListFile.Text = "c:\"
        '
        'pdcConfigFile
        '
        Me.pdcConfigFile.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower
        Me.errorProvider.SetIconAlignment(Me.pdcConfigFile, System.Windows.Forms.ErrorIconAlignment.MiddleLeft)
        Me.errorProvider.SetIconPadding(Me.pdcConfigFile, 3)
        Me.pdcConfigFile.Location = New System.Drawing.Point(96, 72)
        Me.pdcConfigFile.Name = "pdcConfigFile"
        Me.pdcConfigFile.Size = New System.Drawing.Size(170, 20)
        Me.pdcConfigFile.TabIndex = 6
        Me.pdcConfigFile.Text = "c:\"
        '
        'pdcDataPort
        '
        Me.errorProvider.SetIconAlignment(Me.pdcDataPort, System.Windows.Forms.ErrorIconAlignment.MiddleLeft)
        Me.errorProvider.SetIconPadding(Me.pdcDataPort, 3)
        Me.pdcDataPort.Location = New System.Drawing.Point(96, 104)
        Me.pdcDataPort.Name = "pdcDataPort"
        Me.pdcDataPort.Size = New System.Drawing.Size(40, 20)
        Me.pdcDataPort.TabIndex = 9
        Me.pdcDataPort.Text = "65535"
        '
        'findPointListFileButton
        '
        Me.findPointListFileButton.Font = New System.Drawing.Font("Verdana", 11.0!, System.Drawing.FontStyle.Bold)
        Me.findPointListFileButton.Location = New System.Drawing.Point(266, 40)
        Me.findPointListFileButton.Name = "findPointListFileButton"
        Me.findPointListFileButton.Size = New System.Drawing.Size(30, 23)
        Me.findPointListFileButton.TabIndex = 4
        Me.findPointListFileButton.Text = "..."
        '
        'findPDCConfigFileButton
        '
        Me.findPDCConfigFileButton.Font = New System.Drawing.Font("Verdana", 11.0!, System.Drawing.FontStyle.Bold)
        Me.findPDCConfigFileButton.Location = New System.Drawing.Point(266, 72)
        Me.findPDCConfigFileButton.Name = "findPDCConfigFileButton"
        Me.findPDCConfigFileButton.Size = New System.Drawing.Size(30, 23)
        Me.findPDCConfigFileButton.TabIndex = 7
        Me.findPDCConfigFileButton.Text = "..."
        '
        'saveButton
        '
        Me.saveButton.Location = New System.Drawing.Point(160, 104)
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Size = New System.Drawing.Size(64, 24)
        Me.saveButton.TabIndex = 10
        Me.saveButton.Text = "&Save"
        '
        'escapeButton
        '
        Me.escapeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.escapeButton.Location = New System.Drawing.Point(232, 104)
        Me.escapeButton.Name = "escapeButton"
        Me.escapeButton.Size = New System.Drawing.Size(64, 24)
        Me.escapeButton.TabIndex = 11
        Me.escapeButton.Text = "&Cancel"
        '
        'errorProvider
        '
        Me.errorProvider.ContainerControl = Me
        '
        'Configuration
        '
        Me.AcceptButton = Me.saveButton
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.CancelButton = Me.escapeButton
        Me.ClientSize = New System.Drawing.Size(306, 135)
        Me.Controls.Add(Me.escapeButton)
        Me.Controls.Add(Me.saveButton)
        Me.Controls.Add(Me.findPDCConfigFileButton)
        Me.Controls.Add(Me.findPointListFileButton)
        Me.Controls.Add(Me.pdcDataPort)
        Me.Controls.Add(Me.pdcConfigFile)
        Me.Controls.Add(Me.pointListFile)
        Me.Controls.Add(Me.pdcDataPortLabel)
        Me.Controls.Add(Me.pdcConfigFileLabel)
        Me.Controls.Add(Me.pointListFileLabel)
        Me.Controls.Add(Me.timeZoneLabel)
        Me.Controls.Add(Me.timeZone)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Configuration"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "BPA PDC DAQ Configuration"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub Configuration_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        For Each tz As Win32TimeZone In TimeZones.GetTimeZones()
            timeZone.Items.Add(tz.StandardName)
        Next

        timeZone.SelectedItem = Variables("DatAWare.TimeZone")
        pointListFile.Text = Variables("DatAWare.PointListFile")
        pdcConfigFile.Text = Variables("PDCDataReader.ConfigFile")
        pdcDataPort.Text = Variables("PDCDataReader.ListenPort")

    End Sub

    Protected Overrides Sub OnClosing(ByVal e As System.ComponentModel.CancelEventArgs)

        ' We don't ever close this form, we just hide it...
        Me.Hide()
        e.Cancel = True

    End Sub

    Private Sub saveButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles saveButton.Click

        If Not File.Exists(pointListFile.Text) Then
            pointListFile.Select(0, Len(pointListFile.Text))
            errorProvider.SetError(pointListFile, "Specified file does not exist!")
        ElseIf Not File.Exists(pdcConfigFile.Text) Then
            pdcConfigFile.Select(0, Len(pdcConfigFile.Text))
            errorProvider.SetError(pdcConfigFile, "Specified file does not exist!")
        Else
            MsgBox("You will need to unload and restart this DAQ plug-in for these changes to take effect.", MsgBoxStyle.Information Or MsgBoxStyle.OKOnly)

            Variables("DatAWare.TimeZone") = timeZone.SelectedItem
            Variables("DatAWare.PointListFile") = pointListFile.Text
            Variables("PDCDataReader.ConfigFile") = pdcConfigFile.Text
            Variables("PDCDataReader.ListenPort") = pdcDataPort.Text
            Variables.Save()

            Me.Hide()
        End If

    End Sub

    Private Sub escapeButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles escapeButton.Click

        Me.Hide()

    End Sub

    Private Sub pointListFile_Validated(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pointListFile.Validated

        If File.Exists(pointListFile.Text) Then errorProvider.SetError(pointListFile, "")

    End Sub

    Private Sub pdcConfigFile_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles pdcConfigFile.Validated

        If File.Exists(pdcConfigFile.Text) Then errorProvider.SetError(pdcConfigFile, "")

    End Sub

    Private Sub pdcDataPort_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles pdcDataPort.Validating

        Dim errorMessage As String

        If Not IsNumeric(pdcDataPort.Text) Then
            errorMessage = "PDC data port must be a number!"
        ElseIf CInt(pdcDataPort.Text) < 1 Or CInt(pdcDataPort.Text) > 65535 Then
            errorMessage = "Invalid PDC data port number!"
        End If

        If Len(errorMessage) > 0 Then
            e.Cancel = True
            pdcConfigFile.Select(0, Len(pdcConfigFile.Text))
            errorProvider.SetError(pdcConfigFile, errorMessage)
        End If

    End Sub

    Private Sub pdcDataPort_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles pdcDataPort.Validated

        errorProvider.SetError(pdcDataPort, "")

    End Sub

    Private Sub findPointListFileButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles findPointListFileButton.Click

        With openFileDialog
            .Title = "Select DatAWare Point List Configuration File (csv)"
            If Len(pointListFile.Text) > 0 Then
                .InitialDirectory = JustPath(pointListFile.Text)
                .FileName = JustFileName(pointListFile.Text)
            Else
                .FileName = ""
            End If
            .Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"

            If .ShowDialog() = DialogResult.OK Then
                pointListFile.Text = .FileName
                errorProvider.SetError(pointListFile, "")
            End If
        End With

    End Sub

    Private Sub findPDCConfigFileButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles findPDCConfigFileButton.Click

        With openFileDialog
            .Title = "Select PDC Configuration File (ini)"
            If Len(pdcConfigFile.Text) > 0 Then
                .InitialDirectory = JustPath(pdcConfigFile.Text)
                .FileName = JustFileName(pdcConfigFile.Text)
            Else
                .FileName = ""
            End If
            .Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*"

            If .ShowDialog() = DialogResult.OK Then
                pdcConfigFile.Text = .FileName
                errorProvider.SetError(pdcConfigFile, "")
            End If
        End With

    End Sub

End Class
