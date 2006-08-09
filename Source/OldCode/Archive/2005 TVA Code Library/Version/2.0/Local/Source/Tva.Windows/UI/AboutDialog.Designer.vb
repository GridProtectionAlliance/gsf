Namespace UI

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class AboutDialog
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.TabControlInformation = New System.Windows.Forms.TabControl
            Me.TabPageDisclaimer = New System.Windows.Forms.TabPage
            Me.RichTextBoxDisclaimer = New System.Windows.Forms.RichTextBox
            Me.TabPageApplication = New System.Windows.Forms.TabPage
            Me.ListViewApplicationInfo = New System.Windows.Forms.ListView
            Me.ColumnHeaderApplicationKey = New System.Windows.Forms.ColumnHeader
            Me.ColumnHeaderApplicationValue = New System.Windows.Forms.ColumnHeader
            Me.TabPageAssemblies = New System.Windows.Forms.TabPage
            Me.ListViewAssemblyInfo = New System.Windows.Forms.ListView
            Me.ColumnHeaderAssemblyKey = New System.Windows.Forms.ColumnHeader
            Me.ColumnHeaderAssemblyValue = New System.Windows.Forms.ColumnHeader
            Me.ComboBoxAssemblies = New System.Windows.Forms.ComboBox
            Me.ButtonOK = New System.Windows.Forms.Button
            Me.PictureBoxLogo = New System.Windows.Forms.PictureBox
            Me.TabControlInformation.SuspendLayout()
            Me.TabPageDisclaimer.SuspendLayout()
            Me.TabPageApplication.SuspendLayout()
            Me.TabPageAssemblies.SuspendLayout()
            CType(Me.PictureBoxLogo, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'TabControlInformation
            '
            Me.TabControlInformation.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                        Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.TabControlInformation.Controls.Add(Me.TabPageDisclaimer)
            Me.TabControlInformation.Controls.Add(Me.TabPageApplication)
            Me.TabControlInformation.Controls.Add(Me.TabPageAssemblies)
            Me.TabControlInformation.Location = New System.Drawing.Point(12, 68)
            Me.TabControlInformation.Name = "TabControlInformation"
            Me.TabControlInformation.SelectedIndex = 0
            Me.TabControlInformation.Size = New System.Drawing.Size(370, 203)
            Me.TabControlInformation.TabIndex = 2
            '
            'TabPageDisclaimer
            '
            Me.TabPageDisclaimer.Controls.Add(Me.RichTextBoxDisclaimer)
            Me.TabPageDisclaimer.Location = New System.Drawing.Point(4, 22)
            Me.TabPageDisclaimer.Name = "TabPageDisclaimer"
            Me.TabPageDisclaimer.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPageDisclaimer.Size = New System.Drawing.Size(362, 177)
            Me.TabPageDisclaimer.TabIndex = 0
            Me.TabPageDisclaimer.Text = "Disclaimer"
            Me.TabPageDisclaimer.UseVisualStyleBackColor = True
            '
            'RichTextBoxDisclaimer
            '
            Me.RichTextBoxDisclaimer.Dock = System.Windows.Forms.DockStyle.Fill
            Me.RichTextBoxDisclaimer.Location = New System.Drawing.Point(3, 3)
            Me.RichTextBoxDisclaimer.Name = "RichTextBoxDisclaimer"
            Me.RichTextBoxDisclaimer.ReadOnly = True
            Me.RichTextBoxDisclaimer.Size = New System.Drawing.Size(356, 171)
            Me.RichTextBoxDisclaimer.TabIndex = 0
            Me.RichTextBoxDisclaimer.Text = ""
            '
            'TabPageApplication
            '
            Me.TabPageApplication.Controls.Add(Me.ListViewApplicationInfo)
            Me.TabPageApplication.Location = New System.Drawing.Point(4, 22)
            Me.TabPageApplication.Name = "TabPageApplication"
            Me.TabPageApplication.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPageApplication.Size = New System.Drawing.Size(362, 177)
            Me.TabPageApplication.TabIndex = 1
            Me.TabPageApplication.Text = "Application"
            Me.TabPageApplication.UseVisualStyleBackColor = True
            '
            'ListViewApplicationInfo
            '
            Me.ListViewApplicationInfo.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeaderApplicationKey, Me.ColumnHeaderApplicationValue})
            Me.ListViewApplicationInfo.Dock = System.Windows.Forms.DockStyle.Fill
            Me.ListViewApplicationInfo.FullRowSelect = True
            Me.ListViewApplicationInfo.Location = New System.Drawing.Point(3, 3)
            Me.ListViewApplicationInfo.Name = "ListViewApplicationInfo"
            Me.ListViewApplicationInfo.Size = New System.Drawing.Size(356, 171)
            Me.ListViewApplicationInfo.TabIndex = 0
            Me.ListViewApplicationInfo.UseCompatibleStateImageBehavior = False
            Me.ListViewApplicationInfo.View = System.Windows.Forms.View.Details
            '
            'ColumnHeaderApplicationKey
            '
            Me.ColumnHeaderApplicationKey.Text = "Key"
            Me.ColumnHeaderApplicationKey.Width = 102
            '
            'ColumnHeaderApplicationValue
            '
            Me.ColumnHeaderApplicationValue.Text = "Value"
            Me.ColumnHeaderApplicationValue.Width = 227
            '
            'TabPageAssemblies
            '
            Me.TabPageAssemblies.Controls.Add(Me.ListViewAssemblyInfo)
            Me.TabPageAssemblies.Controls.Add(Me.ComboBoxAssemblies)
            Me.TabPageAssemblies.Location = New System.Drawing.Point(4, 22)
            Me.TabPageAssemblies.Name = "TabPageAssemblies"
            Me.TabPageAssemblies.Size = New System.Drawing.Size(362, 177)
            Me.TabPageAssemblies.TabIndex = 2
            Me.TabPageAssemblies.Text = "Assemblies"
            Me.TabPageAssemblies.UseVisualStyleBackColor = True
            '
            'ListViewAssemblyInfo
            '
            Me.ListViewAssemblyInfo.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeaderAssemblyKey, Me.ColumnHeaderAssemblyValue})
            Me.ListViewAssemblyInfo.Dock = System.Windows.Forms.DockStyle.Fill
            Me.ListViewAssemblyInfo.FullRowSelect = True
            Me.ListViewAssemblyInfo.Location = New System.Drawing.Point(0, 21)
            Me.ListViewAssemblyInfo.Name = "ListViewAssemblyInfo"
            Me.ListViewAssemblyInfo.Size = New System.Drawing.Size(362, 156)
            Me.ListViewAssemblyInfo.TabIndex = 1
            Me.ListViewAssemblyInfo.UseCompatibleStateImageBehavior = False
            Me.ListViewAssemblyInfo.View = System.Windows.Forms.View.Details
            '
            'ColumnHeaderAssemblyKey
            '
            Me.ColumnHeaderAssemblyKey.Text = "Key"
            Me.ColumnHeaderAssemblyKey.Width = 100
            '
            'ColumnHeaderAssemblyValue
            '
            Me.ColumnHeaderAssemblyValue.Text = "Value"
            Me.ColumnHeaderAssemblyValue.Width = 226
            '
            'ComboBoxAssemblies
            '
            Me.ComboBoxAssemblies.Dock = System.Windows.Forms.DockStyle.Top
            Me.ComboBoxAssemblies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
            Me.ComboBoxAssemblies.FormattingEnabled = True
            Me.ComboBoxAssemblies.Location = New System.Drawing.Point(0, 0)
            Me.ComboBoxAssemblies.Name = "ComboBoxAssemblies"
            Me.ComboBoxAssemblies.Size = New System.Drawing.Size(362, 21)
            Me.ComboBoxAssemblies.TabIndex = 0
            '
            'ButtonOK
            '
            Me.ButtonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom
            Me.ButtonOK.Location = New System.Drawing.Point(161, 283)
            Me.ButtonOK.Name = "ButtonOK"
            Me.ButtonOK.Size = New System.Drawing.Size(75, 23)
            Me.ButtonOK.TabIndex = 3
            Me.ButtonOK.Text = "OK"
            Me.ButtonOK.UseVisualStyleBackColor = True
            '
            'PictureBoxLogo
            '
            Me.PictureBoxLogo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.PictureBoxLogo.BackColor = System.Drawing.Color.Transparent
            Me.PictureBoxLogo.Cursor = System.Windows.Forms.Cursors.Hand
            Me.PictureBoxLogo.Location = New System.Drawing.Point(12, 12)
            Me.PictureBoxLogo.Name = "PictureBoxLogo"
            Me.PictureBoxLogo.Size = New System.Drawing.Size(370, 50)
            Me.PictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
            Me.PictureBoxLogo.TabIndex = 4
            Me.PictureBoxLogo.TabStop = False
            '
            'AboutDialog
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(394, 318)
            Me.Controls.Add(Me.PictureBoxLogo)
            Me.Controls.Add(Me.ButtonOK)
            Me.Controls.Add(Me.TabControlInformation)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "AboutDialog"
            Me.ShowInTaskbar = False
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            Me.Text = "About {0}"
            Me.TabControlInformation.ResumeLayout(False)
            Me.TabPageDisclaimer.ResumeLayout(False)
            Me.TabPageApplication.ResumeLayout(False)
            Me.TabPageAssemblies.ResumeLayout(False)
            CType(Me.PictureBoxLogo, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents TabControlInformation As System.Windows.Forms.TabControl
        Friend WithEvents TabPageDisclaimer As System.Windows.Forms.TabPage
        Friend WithEvents TabPageApplication As System.Windows.Forms.TabPage
        Friend WithEvents TabPageAssemblies As System.Windows.Forms.TabPage
        Friend WithEvents ButtonOK As System.Windows.Forms.Button
        Friend WithEvents RichTextBoxDisclaimer As System.Windows.Forms.RichTextBox
        Friend WithEvents ListViewApplicationInfo As System.Windows.Forms.ListView
        Friend WithEvents ColumnHeaderApplicationKey As System.Windows.Forms.ColumnHeader
        Friend WithEvents ColumnHeaderApplicationValue As System.Windows.Forms.ColumnHeader
        Friend WithEvents ListViewAssemblyInfo As System.Windows.Forms.ListView
        Friend WithEvents ColumnHeaderAssemblyKey As System.Windows.Forms.ColumnHeader
        Friend WithEvents ColumnHeaderAssemblyValue As System.Windows.Forms.ColumnHeader
        Friend WithEvents ComboBoxAssemblies As System.Windows.Forms.ComboBox
        Friend WithEvents PictureBoxLogo As System.Windows.Forms.PictureBox
    End Class

End Namespace
