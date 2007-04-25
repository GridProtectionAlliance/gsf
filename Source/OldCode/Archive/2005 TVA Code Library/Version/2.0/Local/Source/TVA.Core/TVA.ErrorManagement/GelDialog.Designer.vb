Namespace ErrorManagement

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class GelDialog
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
            Me.PictureBoxIcon = New System.Windows.Forms.PictureBox
            Me.LabelError = New System.Windows.Forms.Label
            Me.RichTextBoxError = New System.Windows.Forms.RichTextBox
            Me.RichTextBoxScope = New System.Windows.Forms.RichTextBox
            Me.LabelScope = New System.Windows.Forms.Label
            Me.RichTextBoxAction = New System.Windows.Forms.RichTextBox
            Me.LabelAction = New System.Windows.Forms.Label
            Me.LabelMoreInfo = New System.Windows.Forms.Label
            Me.RichTextBoxMoreInfo = New System.Windows.Forms.RichTextBox
            Me.ButtonMore = New System.Windows.Forms.Button
            Me.ButtonOK = New System.Windows.Forms.Button
            CType(Me.PictureBoxIcon, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'PictureBoxIcon
            '
            Me.PictureBoxIcon.Location = New System.Drawing.Point(12, 12)
            Me.PictureBoxIcon.Name = "PictureBoxIcon"
            Me.PictureBoxIcon.Size = New System.Drawing.Size(49, 44)
            Me.PictureBoxIcon.TabIndex = 0
            Me.PictureBoxIcon.TabStop = False
            '
            'LabelError
            '
            Me.LabelError.AutoSize = True
            Me.LabelError.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.LabelError.Location = New System.Drawing.Point(67, 12)
            Me.LabelError.Name = "LabelError"
            Me.LabelError.Size = New System.Drawing.Size(97, 13)
            Me.LabelError.TabIndex = 1
            Me.LabelError.Text = "What happened"
            '
            'RichTextBoxError
            '
            Me.RichTextBoxError.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.RichTextBoxError.BackColor = System.Drawing.SystemColors.Control
            Me.RichTextBoxError.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.RichTextBoxError.Location = New System.Drawing.Point(70, 28)
            Me.RichTextBoxError.Name = "RichTextBoxError"
            Me.RichTextBoxError.ReadOnly = True
            Me.RichTextBoxError.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
            Me.RichTextBoxError.Size = New System.Drawing.Size(390, 62)
            Me.RichTextBoxError.TabIndex = 2
            Me.RichTextBoxError.Text = ""
            '
            'RichTextBoxScope
            '
            Me.RichTextBoxScope.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.RichTextBoxScope.BackColor = System.Drawing.SystemColors.Control
            Me.RichTextBoxScope.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.RichTextBoxScope.Location = New System.Drawing.Point(28, 121)
            Me.RichTextBoxScope.Name = "RichTextBoxScope"
            Me.RichTextBoxScope.ReadOnly = True
            Me.RichTextBoxScope.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
            Me.RichTextBoxScope.Size = New System.Drawing.Size(432, 62)
            Me.RichTextBoxScope.TabIndex = 4
            Me.RichTextBoxScope.Text = ""
            '
            'LabelScope
            '
            Me.LabelScope.AutoSize = True
            Me.LabelScope.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.LabelScope.Location = New System.Drawing.Point(9, 105)
            Me.LabelScope.Name = "LabelScope"
            Me.LabelScope.Size = New System.Drawing.Size(139, 13)
            Me.LabelScope.TabIndex = 3
            Me.LabelScope.Text = "How this will affect you"
            '
            'RichTextBoxAction
            '
            Me.RichTextBoxAction.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.RichTextBoxAction.BackColor = System.Drawing.SystemColors.Control
            Me.RichTextBoxAction.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.RichTextBoxAction.Location = New System.Drawing.Point(28, 212)
            Me.RichTextBoxAction.Name = "RichTextBoxAction"
            Me.RichTextBoxAction.ReadOnly = True
            Me.RichTextBoxAction.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
            Me.RichTextBoxAction.Size = New System.Drawing.Size(432, 62)
            Me.RichTextBoxAction.TabIndex = 6
            Me.RichTextBoxAction.Text = ""
            '
            'LabelAction
            '
            Me.LabelAction.AutoSize = True
            Me.LabelAction.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.LabelAction.Location = New System.Drawing.Point(9, 196)
            Me.LabelAction.Name = "LabelAction"
            Me.LabelAction.Size = New System.Drawing.Size(151, 13)
            Me.LabelAction.TabIndex = 5
            Me.LabelAction.Text = "What you can do about it"
            '
            'LabelMoreInfo
            '
            Me.LabelMoreInfo.AutoSize = True
            Me.LabelMoreInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.LabelMoreInfo.Location = New System.Drawing.Point(9, 290)
            Me.LabelMoreInfo.Name = "LabelMoreInfo"
            Me.LabelMoreInfo.Size = New System.Drawing.Size(101, 13)
            Me.LabelMoreInfo.TabIndex = 7
            Me.LabelMoreInfo.Text = "More information"
            '
            'RichTextBoxMoreInfo
            '
            Me.RichTextBoxMoreInfo.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                        Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.RichTextBoxMoreInfo.BackColor = System.Drawing.SystemColors.Control
            Me.RichTextBoxMoreInfo.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.RichTextBoxMoreInfo.Location = New System.Drawing.Point(12, 314)
            Me.RichTextBoxMoreInfo.Name = "RichTextBoxMoreInfo"
            Me.RichTextBoxMoreInfo.ReadOnly = True
            Me.RichTextBoxMoreInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
            Me.RichTextBoxMoreInfo.Size = New System.Drawing.Size(448, 212)
            Me.RichTextBoxMoreInfo.TabIndex = 8
            Me.RichTextBoxMoreInfo.Text = ""
            '
            'ButtonMore
            '
            Me.ButtonMore.Location = New System.Drawing.Point(116, 285)
            Me.ButtonMore.Name = "ButtonMore"
            Me.ButtonMore.Size = New System.Drawing.Size(32, 23)
            Me.ButtonMore.TabIndex = 9
            Me.ButtonMore.Text = ">>"
            Me.ButtonMore.UseVisualStyleBackColor = True
            '
            'ButtonOK
            '
            Me.ButtonOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.ButtonOK.Location = New System.Drawing.Point(385, 538)
            Me.ButtonOK.Name = "ButtonOK"
            Me.ButtonOK.Size = New System.Drawing.Size(75, 23)
            Me.ButtonOK.TabIndex = 10
            Me.ButtonOK.Text = "OK"
            Me.ButtonOK.UseVisualStyleBackColor = True
            '
            'GelDialog
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(472, 573)
            Me.ControlBox = False
            Me.Controls.Add(Me.ButtonOK)
            Me.Controls.Add(Me.ButtonMore)
            Me.Controls.Add(Me.RichTextBoxMoreInfo)
            Me.Controls.Add(Me.LabelMoreInfo)
            Me.Controls.Add(Me.RichTextBoxAction)
            Me.Controls.Add(Me.LabelAction)
            Me.Controls.Add(Me.RichTextBoxScope)
            Me.Controls.Add(Me.LabelScope)
            Me.Controls.Add(Me.RichTextBoxError)
            Me.Controls.Add(Me.LabelError)
            Me.Controls.Add(Me.PictureBoxIcon)
            Me.Name = "GelDialog"
            Me.ShowInTaskbar = False
            Me.Text = "{0} has encountered a problem"
            CType(Me.PictureBoxIcon, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Friend WithEvents PictureBoxIcon As System.Windows.Forms.PictureBox
        Friend WithEvents LabelError As System.Windows.Forms.Label
        Friend WithEvents RichTextBoxError As System.Windows.Forms.RichTextBox
        Friend WithEvents RichTextBoxScope As System.Windows.Forms.RichTextBox
        Friend WithEvents LabelScope As System.Windows.Forms.Label
        Friend WithEvents RichTextBoxAction As System.Windows.Forms.RichTextBox
        Friend WithEvents LabelAction As System.Windows.Forms.Label
        Friend WithEvents LabelMoreInfo As System.Windows.Forms.Label
        Friend WithEvents RichTextBoxMoreInfo As System.Windows.Forms.RichTextBox
        Friend WithEvents ButtonMore As System.Windows.Forms.Button
        Friend WithEvents ButtonOK As System.Windows.Forms.Button
    End Class

End Namespace