<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AlternateCommandChannel
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim Appearance1 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance2 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance3 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance4 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance5 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance6 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance7 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance8 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance9 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim Appearance10 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AlternateCommandChannel))
        Dim Appearance11 As Infragistics.Win.Appearance = New Infragistics.Win.Appearance
        Dim UltraTab1 As Infragistics.Win.UltraWinTabControl.UltraTab = New Infragistics.Win.UltraWinTabControl.UltraTab
        Dim UltraTab2 As Infragistics.Win.UltraWinTabControl.UltraTab = New Infragistics.Win.UltraWinTabControl.UltraTab
        Dim UltraTab3 As Infragistics.Win.UltraWinTabControl.UltraTab = New Infragistics.Win.UltraWinTabControl.UltraTab
        Me.TabPageControl1 = New Infragistics.Win.UltraWinTabControl.UltraTabPageControl
        Me.LabelTcpHostIP = New Infragistics.Win.Misc.UltraLabel
        Me.TextBoxTcpHostIP = New Infragistics.Win.UltraWinMaskedEdit.UltraMaskedEdit
        Me.LabelTcpPort = New Infragistics.Win.Misc.UltraLabel
        Me.TextBoxTcpPort = New Infragistics.Win.UltraWinMaskedEdit.UltraMaskedEdit
        Me.TabPageControl2 = New Infragistics.Win.UltraWinTabControl.UltraTabPageControl
        Me.CheckBoxSerialRTS = New Infragistics.Win.UltraWinEditors.UltraCheckEditor
        Me.CheckBoxSerialDTR = New Infragistics.Win.UltraWinEditors.UltraCheckEditor
        Me.TextBoxSerialDataBits = New Infragistics.Win.UltraWinMaskedEdit.UltraMaskedEdit
        Me.ComboBoxSerialStopBits = New System.Windows.Forms.ComboBox
        Me.ComboBoxSerialParities = New System.Windows.Forms.ComboBox
        Me.LabelSerialParity = New Infragistics.Win.Misc.UltraLabel
        Me.ComboBoxSerialBaudRates = New System.Windows.Forms.ComboBox
        Me.LabelSerialBaudRate = New Infragistics.Win.Misc.UltraLabel
        Me.ComboBoxSerialPorts = New System.Windows.Forms.ComboBox
        Me.LabelSerialPort = New Infragistics.Win.Misc.UltraLabel
        Me.LabelSerialStopBits = New Infragistics.Win.Misc.UltraLabel
        Me.LabelSerialDataBits = New Infragistics.Win.Misc.UltraLabel
        Me.TabPageControl3 = New Infragistics.Win.UltraWinTabControl.UltraTabPageControl
        Me.ButtonBrowse = New System.Windows.Forms.Button
        Me.TextBoxFileCaptureName = New System.Windows.Forms.TextBox
        Me.LabelCaptureFile = New Infragistics.Win.Misc.UltraLabel
        Me.LabelReplayCapturedFile = New Infragistics.Win.Misc.UltraLabel
        Me.ButtonSave = New System.Windows.Forms.Button
        Me.ButtonCancel = New System.Windows.Forms.Button
        Me.PictureBoxIcon = New System.Windows.Forms.PictureBox
        Me.TabControlCommunications = New Infragistics.Win.UltraWinTabControl.UltraTabControl
        Me.TabSharedControlsPageCommunications = New Infragistics.Win.UltraWinTabControl.UltraTabSharedControlsPage
        Me.CheckBoxUndefined = New Infragistics.Win.UltraWinEditors.UltraCheckEditor
        Me.TabPageControl1.SuspendLayout()
        Me.TabPageControl2.SuspendLayout()
        Me.TabPageControl3.SuspendLayout()
        CType(Me.PictureBoxIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.TabControlCommunications, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabControlCommunications.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabPageControl1
        '
        Me.TabPageControl1.Controls.Add(Me.LabelTcpHostIP)
        Me.TabPageControl1.Controls.Add(Me.TextBoxTcpHostIP)
        Me.TabPageControl1.Controls.Add(Me.LabelTcpPort)
        Me.TabPageControl1.Controls.Add(Me.TextBoxTcpPort)
        Me.TabPageControl1.Location = New System.Drawing.Point(1, 20)
        Me.TabPageControl1.Name = "TabPageControl1"
        Me.TabPageControl1.Size = New System.Drawing.Size(304, 96)
        '
        'LabelTcpHostIP
        '
        Appearance1.TextHAlignAsString = "Right"
        Me.LabelTcpHostIP.Appearance = Appearance1
        Me.LabelTcpHostIP.Location = New System.Drawing.Point(15, 25)
        Me.LabelTcpHostIP.Name = "LabelTcpHostIP"
        Me.LabelTcpHostIP.Size = New System.Drawing.Size(45, 23)
        Me.LabelTcpHostIP.TabIndex = 0
        Me.LabelTcpHostIP.Text = "Host &IP:"
        '
        'TextBoxTcpHostIP
        '
        Appearance2.TextHAlignAsString = "Center"
        Me.TextBoxTcpHostIP.Appearance = Appearance2
        Me.TextBoxTcpHostIP.AutoSize = False
        Me.TextBoxTcpHostIP.EditAs = Infragistics.Win.UltraWinMaskedEdit.EditAsType.UseSpecifiedMask
        Me.TextBoxTcpHostIP.InputMask = "nnn\.nnn\.nnn\.nnn"
        Me.TextBoxTcpHostIP.Location = New System.Drawing.Point(66, 22)
        Me.TextBoxTcpHostIP.Name = "TextBoxTcpHostIP"
        Me.TextBoxTcpHostIP.PromptChar = Global.Microsoft.VisualBasic.ChrW(32)
        Me.TextBoxTcpHostIP.Size = New System.Drawing.Size(92, 20)
        Me.TextBoxTcpHostIP.TabIndex = 1
        Me.TextBoxTcpHostIP.Text = "127.0.0.1"
        '
        'LabelTcpPort
        '
        Appearance3.TextHAlignAsString = "Right"
        Me.LabelTcpPort.Appearance = Appearance3
        Me.LabelTcpPort.Location = New System.Drawing.Point(15, 51)
        Me.LabelTcpPort.Name = "LabelTcpPort"
        Me.LabelTcpPort.Size = New System.Drawing.Size(45, 23)
        Me.LabelTcpPort.TabIndex = 2
        Me.LabelTcpPort.Text = "P&ort:"
        '
        'TextBoxTcpPort
        '
        Me.TextBoxTcpPort.AutoSize = False
        Me.TextBoxTcpPort.EditAs = Infragistics.Win.UltraWinMaskedEdit.EditAsType.UseSpecifiedMask
        Me.TextBoxTcpPort.InputMask = "nnnnn"
        Me.TextBoxTcpPort.Location = New System.Drawing.Point(66, 48)
        Me.TextBoxTcpPort.Name = "TextBoxTcpPort"
        Me.TextBoxTcpPort.PromptChar = Global.Microsoft.VisualBasic.ChrW(32)
        Me.TextBoxTcpPort.Size = New System.Drawing.Size(40, 20)
        Me.TextBoxTcpPort.TabIndex = 3
        Me.TextBoxTcpPort.Text = "4712"
        '
        'TabPageControl2
        '
        Me.TabPageControl2.Controls.Add(Me.CheckBoxSerialRTS)
        Me.TabPageControl2.Controls.Add(Me.CheckBoxSerialDTR)
        Me.TabPageControl2.Controls.Add(Me.TextBoxSerialDataBits)
        Me.TabPageControl2.Controls.Add(Me.ComboBoxSerialStopBits)
        Me.TabPageControl2.Controls.Add(Me.ComboBoxSerialParities)
        Me.TabPageControl2.Controls.Add(Me.LabelSerialParity)
        Me.TabPageControl2.Controls.Add(Me.ComboBoxSerialBaudRates)
        Me.TabPageControl2.Controls.Add(Me.LabelSerialBaudRate)
        Me.TabPageControl2.Controls.Add(Me.ComboBoxSerialPorts)
        Me.TabPageControl2.Controls.Add(Me.LabelSerialPort)
        Me.TabPageControl2.Controls.Add(Me.LabelSerialStopBits)
        Me.TabPageControl2.Controls.Add(Me.LabelSerialDataBits)
        Me.TabPageControl2.Location = New System.Drawing.Point(-10000, -10000)
        Me.TabPageControl2.Name = "TabPageControl2"
        Me.TabPageControl2.Size = New System.Drawing.Size(304, 96)
        '
        'CheckBoxSerialRTS
        '
        Me.CheckBoxSerialRTS.Location = New System.Drawing.Point(225, 62)
        Me.CheckBoxSerialRTS.Name = "CheckBoxSerialRTS"
        Me.CheckBoxSerialRTS.Size = New System.Drawing.Size(50, 26)
        Me.CheckBoxSerialRTS.TabIndex = 11
        Me.CheckBoxSerialRTS.Text = "RTS"
        '
        'CheckBoxSerialDTR
        '
        Me.CheckBoxSerialDTR.Location = New System.Drawing.Point(169, 62)
        Me.CheckBoxSerialDTR.Name = "CheckBoxSerialDTR"
        Me.CheckBoxSerialDTR.Size = New System.Drawing.Size(50, 26)
        Me.CheckBoxSerialDTR.TabIndex = 10
        Me.CheckBoxSerialDTR.Text = "DTR"
        '
        'TextBoxSerialDataBits
        '
        Me.TextBoxSerialDataBits.AutoSize = False
        Me.TextBoxSerialDataBits.EditAs = Infragistics.Win.UltraWinMaskedEdit.EditAsType.UseSpecifiedMask
        Me.TextBoxSerialDataBits.InputMask = "nnn"
        Me.TextBoxSerialDataBits.Location = New System.Drawing.Point(211, 37)
        Me.TextBoxSerialDataBits.Name = "TextBoxSerialDataBits"
        Me.TextBoxSerialDataBits.PromptChar = Global.Microsoft.VisualBasic.ChrW(32)
        Me.TextBoxSerialDataBits.Size = New System.Drawing.Size(27, 20)
        Me.TextBoxSerialDataBits.TabIndex = 9
        Me.TextBoxSerialDataBits.Text = "8"
        '
        'ComboBoxSerialStopBits
        '
        Me.ComboBoxSerialStopBits.Location = New System.Drawing.Point(211, 10)
        Me.ComboBoxSerialStopBits.Name = "ComboBoxSerialStopBits"
        Me.ComboBoxSerialStopBits.Size = New System.Drawing.Size(85, 21)
        Me.ComboBoxSerialStopBits.TabIndex = 7
        '
        'ComboBoxSerialParities
        '
        Me.ComboBoxSerialParities.Location = New System.Drawing.Point(66, 65)
        Me.ComboBoxSerialParities.Name = "ComboBoxSerialParities"
        Me.ComboBoxSerialParities.Size = New System.Drawing.Size(85, 21)
        Me.ComboBoxSerialParities.TabIndex = 5
        '
        'LabelSerialParity
        '
        Appearance4.TextHAlignAsString = "Right"
        Me.LabelSerialParity.Appearance = Appearance4
        Me.LabelSerialParity.Location = New System.Drawing.Point(-4, 68)
        Me.LabelSerialParity.Name = "LabelSerialParity"
        Me.LabelSerialParity.Size = New System.Drawing.Size(64, 23)
        Me.LabelSerialParity.TabIndex = 4
        Me.LabelSerialParity.Text = "Par&ity:"
        '
        'ComboBoxSerialBaudRates
        '
        Me.ComboBoxSerialBaudRates.Items.AddRange(New Object() {"115200", "57600", "38400", "19200", "9600", "4800", "2400", "1200"})
        Me.ComboBoxSerialBaudRates.Location = New System.Drawing.Point(66, 37)
        Me.ComboBoxSerialBaudRates.Name = "ComboBoxSerialBaudRates"
        Me.ComboBoxSerialBaudRates.Size = New System.Drawing.Size(85, 21)
        Me.ComboBoxSerialBaudRates.TabIndex = 3
        '
        'LabelSerialBaudRate
        '
        Appearance5.TextHAlignAsString = "Right"
        Me.LabelSerialBaudRate.Appearance = Appearance5
        Me.LabelSerialBaudRate.Location = New System.Drawing.Point(-4, 40)
        Me.LabelSerialBaudRate.Name = "LabelSerialBaudRate"
        Me.LabelSerialBaudRate.Size = New System.Drawing.Size(64, 23)
        Me.LabelSerialBaudRate.TabIndex = 2
        Me.LabelSerialBaudRate.Text = "&Baud Rate:"
        '
        'ComboBoxSerialPorts
        '
        Me.ComboBoxSerialPorts.Location = New System.Drawing.Point(66, 10)
        Me.ComboBoxSerialPorts.Name = "ComboBoxSerialPorts"
        Me.ComboBoxSerialPorts.Size = New System.Drawing.Size(85, 21)
        Me.ComboBoxSerialPorts.TabIndex = 1
        '
        'LabelSerialPort
        '
        Appearance6.TextHAlignAsString = "Right"
        Me.LabelSerialPort.Appearance = Appearance6
        Me.LabelSerialPort.Location = New System.Drawing.Point(-4, 13)
        Me.LabelSerialPort.Name = "LabelSerialPort"
        Me.LabelSerialPort.Size = New System.Drawing.Size(64, 23)
        Me.LabelSerialPort.TabIndex = 0
        Me.LabelSerialPort.Text = "P&ort:"
        '
        'LabelSerialStopBits
        '
        Appearance7.TextHAlignAsString = "Right"
        Me.LabelSerialStopBits.Appearance = Appearance7
        Me.LabelSerialStopBits.Location = New System.Drawing.Point(150, 13)
        Me.LabelSerialStopBits.Name = "LabelSerialStopBits"
        Me.LabelSerialStopBits.Size = New System.Drawing.Size(57, 23)
        Me.LabelSerialStopBits.TabIndex = 6
        Me.LabelSerialStopBits.Text = "Stop Bits:"
        '
        'LabelSerialDataBits
        '
        Appearance8.TextHAlignAsString = "Right"
        Me.LabelSerialDataBits.Appearance = Appearance8
        Me.LabelSerialDataBits.Location = New System.Drawing.Point(148, 40)
        Me.LabelSerialDataBits.Name = "LabelSerialDataBits"
        Me.LabelSerialDataBits.Size = New System.Drawing.Size(59, 23)
        Me.LabelSerialDataBits.TabIndex = 8
        Me.LabelSerialDataBits.Text = "Data Bits:"
        '
        'TabPageControl3
        '
        Me.TabPageControl3.Controls.Add(Me.ButtonBrowse)
        Me.TabPageControl3.Controls.Add(Me.TextBoxFileCaptureName)
        Me.TabPageControl3.Controls.Add(Me.LabelCaptureFile)
        Me.TabPageControl3.Controls.Add(Me.LabelReplayCapturedFile)
        Me.TabPageControl3.Location = New System.Drawing.Point(-10000, -10000)
        Me.TabPageControl3.Name = "TabPageControl3"
        Me.TabPageControl3.Size = New System.Drawing.Size(304, 96)
        '
        'ButtonBrowse
        '
        Me.ButtonBrowse.Location = New System.Drawing.Point(213, 37)
        Me.ButtonBrowse.Name = "ButtonBrowse"
        Me.ButtonBrowse.Size = New System.Drawing.Size(75, 23)
        Me.ButtonBrowse.TabIndex = 2
        Me.ButtonBrowse.Text = "&Browse..."
        Me.ButtonBrowse.UseVisualStyleBackColor = True
        '
        'TextBoxFileCaptureName
        '
        Me.TextBoxFileCaptureName.BackColor = System.Drawing.Color.White
        Me.TextBoxFileCaptureName.Location = New System.Drawing.Point(71, 39)
        Me.TextBoxFileCaptureName.Name = "TextBoxFileCaptureName"
        Me.TextBoxFileCaptureName.Size = New System.Drawing.Size(136, 20)
        Me.TextBoxFileCaptureName.TabIndex = 1
        '
        'LabelCaptureFile
        '
        Appearance9.TextHAlignAsString = "Right"
        Me.LabelCaptureFile.Appearance = Appearance9
        Me.LabelCaptureFile.Location = New System.Drawing.Point(11, 42)
        Me.LabelCaptureFile.Name = "LabelCaptureFile"
        Me.LabelCaptureFile.Size = New System.Drawing.Size(54, 23)
        Me.LabelCaptureFile.TabIndex = 0
        Me.LabelCaptureFile.Text = "F&ilename:"
        '
        'LabelReplayCapturedFile
        '
        Appearance10.FontData.ItalicAsString = "True"
        Me.LabelReplayCapturedFile.Appearance = Appearance10
        Me.LabelReplayCapturedFile.Location = New System.Drawing.Point(191, 22)
        Me.LabelReplayCapturedFile.Name = "LabelReplayCapturedFile"
        Me.LabelReplayCapturedFile.Size = New System.Drawing.Size(120, 17)
        Me.LabelReplayCapturedFile.TabIndex = 6
        Me.LabelReplayCapturedFile.Text = "Replay captured file..."
        '
        'ButtonSave
        '
        Me.ButtonSave.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ButtonSave.Location = New System.Drawing.Point(324, 12)
        Me.ButtonSave.Name = "ButtonSave"
        Me.ButtonSave.Size = New System.Drawing.Size(75, 23)
        Me.ButtonSave.TabIndex = 0
        Me.ButtonSave.Text = "S&ave"
        Me.ButtonSave.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButtonCancel.Location = New System.Drawing.Point(324, 41)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButtonCancel.TabIndex = 1
        Me.ButtonCancel.Text = "&Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'PictureBoxIcon
        '
        Me.PictureBoxIcon.Image = CType(resources.GetObject("PictureBoxIcon.Image"), System.Drawing.Image)
        Me.PictureBoxIcon.Location = New System.Drawing.Point(339, 89)
        Me.PictureBoxIcon.Name = "PictureBoxIcon"
        Me.PictureBoxIcon.Size = New System.Drawing.Size(48, 48)
        Me.PictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.PictureBoxIcon.TabIndex = 2
        Me.PictureBoxIcon.TabStop = False
        '
        'TabControlCommunications
        '
        Appearance11.ForeColor = System.Drawing.Color.Black
        Me.TabControlCommunications.Appearance = Appearance11
        Me.TabControlCommunications.Controls.Add(Me.TabSharedControlsPageCommunications)
        Me.TabControlCommunications.Controls.Add(Me.TabPageControl1)
        Me.TabControlCommunications.Controls.Add(Me.TabPageControl2)
        Me.TabControlCommunications.Controls.Add(Me.TabPageControl3)
        Me.TabControlCommunications.Enabled = False
        Me.TabControlCommunications.Location = New System.Drawing.Point(12, 12)
        Me.TabControlCommunications.Name = "TabControlCommunications"
        Me.TabControlCommunications.SharedControlsPage = Me.TabSharedControlsPageCommunications
        Me.TabControlCommunications.Size = New System.Drawing.Size(306, 117)
        Me.TabControlCommunications.Style = Infragistics.Win.UltraWinTabControl.UltraTabControlStyle.VisualStudio2005
        Me.TabControlCommunications.TabIndex = 3
        Me.TabControlCommunications.TabLayoutStyle = Infragistics.Win.UltraWinTabs.TabLayoutStyle.SingleRowFixed
        Me.TabControlCommunications.TabOrientation = Infragistics.Win.UltraWinTabs.TabOrientation.TopLeft
        UltraTab1.TabPage = Me.TabPageControl1
        UltraTab1.Text = "&Tcp"
        UltraTab2.TabPage = Me.TabPageControl2
        UltraTab2.Text = "&Serial"
        UltraTab3.TabPage = Me.TabPageControl3
        UltraTab3.Text = "Fi&le"
        Me.TabControlCommunications.Tabs.AddRange(New Infragistics.Win.UltraWinTabControl.UltraTab() {UltraTab1, UltraTab2, UltraTab3})
        '
        'TabSharedControlsPageCommunications
        '
        Me.TabSharedControlsPageCommunications.Location = New System.Drawing.Point(-10000, -10000)
        Me.TabSharedControlsPageCommunications.Name = "TabSharedControlsPageCommunications"
        Me.TabSharedControlsPageCommunications.Size = New System.Drawing.Size(304, 96)
        '
        'CheckBoxUndefined
        '
        Me.CheckBoxUndefined.Checked = True
        Me.CheckBoxUndefined.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBoxUndefined.Location = New System.Drawing.Point(324, 64)
        Me.CheckBoxUndefined.Name = "CheckBoxUndefined"
        Me.CheckBoxUndefined.Size = New System.Drawing.Size(82, 26)
        Me.CheckBoxUndefined.TabIndex = 11
        Me.CheckBoxUndefined.Text = "Not defined"
        '
        'AlternateCommandChannel
        '
        Me.AcceptButton = Me.ButtonSave
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButtonCancel
        Me.ClientSize = New System.Drawing.Size(406, 142)
        Me.ControlBox = False
        Me.Controls.Add(Me.PictureBoxIcon)
        Me.Controls.Add(Me.CheckBoxUndefined)
        Me.Controls.Add(Me.TabControlCommunications)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonSave)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "AlternateCommandChannel"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Alternate Command Channel Configuration"
        Me.TabPageControl1.ResumeLayout(False)
        Me.TabPageControl2.ResumeLayout(False)
        Me.TabPageControl3.ResumeLayout(False)
        Me.TabPageControl3.PerformLayout()
        CType(Me.PictureBoxIcon, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.TabControlCommunications, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabControlCommunications.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ButtonSave As System.Windows.Forms.Button
    Friend WithEvents ButtonCancel As System.Windows.Forms.Button
    Friend WithEvents PictureBoxIcon As System.Windows.Forms.PictureBox
    Friend WithEvents TabControlCommunications As Infragistics.Win.UltraWinTabControl.UltraTabControl
    Friend WithEvents TabSharedControlsPageCommunications As Infragistics.Win.UltraWinTabControl.UltraTabSharedControlsPage
    Friend WithEvents TabPageControl1 As Infragistics.Win.UltraWinTabControl.UltraTabPageControl
    Friend WithEvents LabelTcpHostIP As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents TextBoxTcpHostIP As Infragistics.Win.UltraWinMaskedEdit.UltraMaskedEdit
    Friend WithEvents LabelTcpPort As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents TextBoxTcpPort As Infragistics.Win.UltraWinMaskedEdit.UltraMaskedEdit
    Friend WithEvents TabPageControl2 As Infragistics.Win.UltraWinTabControl.UltraTabPageControl
    Friend WithEvents CheckBoxSerialRTS As Infragistics.Win.UltraWinEditors.UltraCheckEditor
    Friend WithEvents CheckBoxSerialDTR As Infragistics.Win.UltraWinEditors.UltraCheckEditor
    Friend WithEvents TextBoxSerialDataBits As Infragistics.Win.UltraWinMaskedEdit.UltraMaskedEdit
    Friend WithEvents ComboBoxSerialStopBits As System.Windows.Forms.ComboBox
    Friend WithEvents ComboBoxSerialParities As System.Windows.Forms.ComboBox
    Friend WithEvents LabelSerialParity As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents ComboBoxSerialBaudRates As System.Windows.Forms.ComboBox
    Friend WithEvents LabelSerialBaudRate As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents ComboBoxSerialPorts As System.Windows.Forms.ComboBox
    Friend WithEvents LabelSerialPort As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents LabelSerialStopBits As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents LabelSerialDataBits As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents TabPageControl3 As Infragistics.Win.UltraWinTabControl.UltraTabPageControl
    Friend WithEvents ButtonBrowse As System.Windows.Forms.Button
    Friend WithEvents TextBoxFileCaptureName As System.Windows.Forms.TextBox
    Friend WithEvents LabelCaptureFile As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents LabelReplayCapturedFile As Infragistics.Win.Misc.UltraLabel
    Friend WithEvents CheckBoxUndefined As Infragistics.Win.UltraWinEditors.UltraCheckEditor
End Class
