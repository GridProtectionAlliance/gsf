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
    Friend WithEvents findPDCPointsFileButton As System.Windows.Forms.Button
    Friend WithEvents pdcPointsFile As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(Configuration))
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
        Me.pdcPointsFile = New System.Windows.Forms.TextBox
        Me.openFileDialog = New System.Windows.Forms.OpenFileDialog
        Me.findPDCPointsFileButton = New System.Windows.Forms.Button
        Me.Label1 = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'pointListFileLabel
        '
        Me.pointListFileLabel.Location = New System.Drawing.Point(0, 8)
        Me.pointListFileLabel.Name = "pointListFileLabel"
        Me.pointListFileLabel.Size = New System.Drawing.Size(88, 23)
        Me.pointListFileLabel.TabIndex = 0
        Me.pointListFileLabel.Text = "&All Points List:"
        Me.pointListFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'pdcConfigFileLabel
        '
        Me.pdcConfigFileLabel.Location = New System.Drawing.Point(0, 72)
        Me.pdcConfigFileLabel.Name = "pdcConfigFileLabel"
        Me.pdcConfigFileLabel.Size = New System.Drawing.Size(88, 23)
        Me.pdcConfigFileLabel.TabIndex = 6
        Me.pdcConfigFileLabel.Text = "PDC &Config File:"
        Me.pdcConfigFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'pdcDataPortLabel
        '
        Me.pdcDataPortLabel.Location = New System.Drawing.Point(0, 104)
        Me.pdcDataPortLabel.Name = "pdcDataPortLabel"
        Me.pdcDataPortLabel.Size = New System.Drawing.Size(88, 23)
        Me.pdcDataPortLabel.TabIndex = 9
        Me.pdcDataPortLabel.Text = "PDC &Data Port:"
        Me.pdcDataPortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'pointListFile
        '
        Me.pointListFile.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower
        Me.errorProvider.SetIconAlignment(Me.pointListFile, System.Windows.Forms.ErrorIconAlignment.MiddleLeft)
        Me.errorProvider.SetIconPadding(Me.pointListFile, 3)
        Me.pointListFile.Location = New System.Drawing.Point(96, 8)
        Me.pointListFile.Name = "pointListFile"
        Me.pointListFile.Size = New System.Drawing.Size(170, 20)
        Me.pointListFile.TabIndex = 1
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
        Me.pdcConfigFile.TabIndex = 7
        Me.pdcConfigFile.Text = "c:\"
        '
        'pdcDataPort
        '
        Me.errorProvider.SetIconAlignment(Me.pdcDataPort, System.Windows.Forms.ErrorIconAlignment.MiddleLeft)
        Me.errorProvider.SetIconPadding(Me.pdcDataPort, 3)
        Me.pdcDataPort.Location = New System.Drawing.Point(96, 104)
        Me.pdcDataPort.Name = "pdcDataPort"
        Me.pdcDataPort.Size = New System.Drawing.Size(40, 20)
        Me.pdcDataPort.TabIndex = 10
        Me.pdcDataPort.Text = "65535"
        '
        'findPointListFileButton
        '
        Me.findPointListFileButton.Font = New System.Drawing.Font("Verdana", 11.0!, System.Drawing.FontStyle.Bold)
        Me.findPointListFileButton.Location = New System.Drawing.Point(264, 8)
        Me.findPointListFileButton.Name = "findPointListFileButton"
        Me.findPointListFileButton.Size = New System.Drawing.Size(30, 23)
        Me.findPointListFileButton.TabIndex = 2
        Me.findPointListFileButton.Text = "..."
        '
        'findPDCConfigFileButton
        '
        Me.findPDCConfigFileButton.Font = New System.Drawing.Font("Verdana", 11.0!, System.Drawing.FontStyle.Bold)
        Me.findPDCConfigFileButton.Location = New System.Drawing.Point(264, 72)
        Me.findPDCConfigFileButton.Name = "findPDCConfigFileButton"
        Me.findPDCConfigFileButton.Size = New System.Drawing.Size(30, 23)
        Me.findPDCConfigFileButton.TabIndex = 8
        Me.findPDCConfigFileButton.Text = "..."
        '
        'saveButton
        '
        Me.saveButton.Location = New System.Drawing.Point(160, 104)
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Size = New System.Drawing.Size(64, 24)
        Me.saveButton.TabIndex = 11
        Me.saveButton.Text = "&Save"
        '
        'escapeButton
        '
        Me.escapeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.escapeButton.Location = New System.Drawing.Point(232, 104)
        Me.escapeButton.Name = "escapeButton"
        Me.escapeButton.Size = New System.Drawing.Size(64, 24)
        Me.escapeButton.TabIndex = 12
        Me.escapeButton.Text = "&Cancel"
        '
        'errorProvider
        '
        Me.errorProvider.ContainerControl = Me
        '
        'pdcPointsFile
        '
        Me.pdcPointsFile.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower
        Me.errorProvider.SetIconAlignment(Me.pdcPointsFile, System.Windows.Forms.ErrorIconAlignment.MiddleLeft)
        Me.errorProvider.SetIconPadding(Me.pdcPointsFile, 3)
        Me.pdcPointsFile.Location = New System.Drawing.Point(96, 40)
        Me.pdcPointsFile.Name = "pdcPointsFile"
        Me.pdcPointsFile.Size = New System.Drawing.Size(170, 20)
        Me.pdcPointsFile.TabIndex = 4
        Me.pdcPointsFile.Text = "c:\"
        '
        'findPDCPointsFileButton
        '
        Me.findPDCPointsFileButton.Font = New System.Drawing.Font("Verdana", 11.0!, System.Drawing.FontStyle.Bold)
        Me.findPDCPointsFileButton.Location = New System.Drawing.Point(264, 40)
        Me.findPDCPointsFileButton.Name = "findPDCPointsFileButton"
        Me.findPDCPointsFileButton.Size = New System.Drawing.Size(30, 23)
        Me.findPDCPointsFileButton.TabIndex = 5
        Me.findPDCPointsFileButton.Text = "..."
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(0, 40)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(88, 23)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "PDC &Points File:"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Configuration
        '
        Me.AcceptButton = Me.saveButton
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.CancelButton = Me.escapeButton
        Me.ClientSize = New System.Drawing.Size(306, 135)
        Me.Controls.Add(Me.findPDCPointsFileButton)
        Me.Controls.Add(Me.pdcPointsFile)
        Me.Controls.Add(Me.Label1)
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

    Private m_instance As Integer

    Private Sub Configuration_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.VisibleChanged

        If Me.Visible Then
            Variables.Refresh()
            pointListFile.Text = Variables("DatAWare.PointListFile")
            pdcPointsFile.Text = Variables("PDCDataReader.PointList" & m_instance)
            pdcConfigFile.Text = Variables("PDCDataReader.ConfigFile" & m_instance)
            pdcDataPort.Text = Variables("PDCDataReader.ListenPort" & m_instance)
        End If

    End Sub

    Public Property Instance() As Integer
        Get
            Return m_instance
        End Get
        Set(ByVal Value As Integer)
            m_instance = Value
            Me.Text &= " - Instance " & m_instance
        End Set
    End Property

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
        ElseIf Not File.Exists(pdcPointsFile.Text) Then
            pdcPointsFile.Select(0, Len(pdcPointsFile.Text))
            errorProvider.SetError(pdcPointsFile, "Specified file does not exist!")
        Else
            MsgBox("You will need to unload and restart this DAQ plug-in for these changes to take effect.", MsgBoxStyle.Information Or MsgBoxStyle.OKOnly)

            Variables("DatAWare.PointListFile") = pointListFile.Text
            Variables("PDCDataReader.PointList" & m_instance) = pdcPointsFile.Text
            Variables("PDCDataReader.ConfigFile" & m_instance) = pdcConfigFile.Text
            Variables("PDCDataReader.ListenPort" & m_instance) = pdcDataPort.Text
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

    Private Sub pdcPointsFile_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles pdcPointsFile.Validated

        If File.Exists(pdcPointsFile.Text) Then errorProvider.SetError(pdcPointsFile, "")

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

    Private Sub findPDCPointsFileButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles findPDCPointsFileButton.Click

        With openFileDialog
            .Title = "Select PDC Point List File (csv)"
            If Len(pdcPointsFile.Text) > 0 Then
                .InitialDirectory = JustPath(pdcPointsFile.Text)
                .FileName = JustFileName(pdcPointsFile.Text)
            Else
                .FileName = ""
            End If
            .Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"

            If .ShowDialog() = DialogResult.OK Then
                pdcPointsFile.Text = .FileName
                errorProvider.SetError(pdcPointsFile, "")
            End If
        End With

    End Sub

End Class
