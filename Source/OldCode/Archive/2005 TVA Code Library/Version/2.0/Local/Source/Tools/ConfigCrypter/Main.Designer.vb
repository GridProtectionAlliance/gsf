<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Main
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
        Me.GroupBoxDirection = New System.Windows.Forms.GroupBox
        Me.RadioButtonDecrypt = New System.Windows.Forms.RadioButton
        Me.RadioButtonEncrypt = New System.Windows.Forms.RadioButton
        Me.GroupBoxConfiguration = New System.Windows.Forms.GroupBox
        Me.LinkLabelCopy = New System.Windows.Forms.LinkLabel
        Me.TextBoxOutput = New System.Windows.Forms.TextBox
        Me.LabelOutput = New System.Windows.Forms.Label
        Me.TextBoxInput = New System.Windows.Forms.TextBox
        Me.LabelInput = New System.Windows.Forms.Label
        Me.GroupBoxDirection.SuspendLayout()
        Me.GroupBoxConfiguration.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBoxDirection
        '
        Me.GroupBoxDirection.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBoxDirection.Controls.Add(Me.RadioButtonDecrypt)
        Me.GroupBoxDirection.Controls.Add(Me.RadioButtonEncrypt)
        Me.GroupBoxDirection.Location = New System.Drawing.Point(12, 12)
        Me.GroupBoxDirection.Name = "GroupBoxDirection"
        Me.GroupBoxDirection.Size = New System.Drawing.Size(268, 49)
        Me.GroupBoxDirection.TabIndex = 0
        Me.GroupBoxDirection.TabStop = False
        Me.GroupBoxDirection.Text = "Direction"
        '
        'RadioButtonDecrypt
        '
        Me.RadioButtonDecrypt.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.RadioButtonDecrypt.AutoSize = True
        Me.RadioButtonDecrypt.Location = New System.Drawing.Point(136, 20)
        Me.RadioButtonDecrypt.Name = "RadioButtonDecrypt"
        Me.RadioButtonDecrypt.Size = New System.Drawing.Size(126, 17)
        Me.RadioButtonDecrypt.TabIndex = 1
        Me.RadioButtonDecrypt.Text = "Decrypt Config Value"
        Me.RadioButtonDecrypt.UseVisualStyleBackColor = True
        '
        'RadioButtonEncrypt
        '
        Me.RadioButtonEncrypt.AutoSize = True
        Me.RadioButtonEncrypt.Checked = True
        Me.RadioButtonEncrypt.Location = New System.Drawing.Point(6, 20)
        Me.RadioButtonEncrypt.Name = "RadioButtonEncrypt"
        Me.RadioButtonEncrypt.Size = New System.Drawing.Size(125, 17)
        Me.RadioButtonEncrypt.TabIndex = 0
        Me.RadioButtonEncrypt.TabStop = True
        Me.RadioButtonEncrypt.Text = "Encrypt Config Value"
        Me.RadioButtonEncrypt.UseVisualStyleBackColor = True
        '
        'GroupBoxConfiguration
        '
        Me.GroupBoxConfiguration.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBoxConfiguration.Controls.Add(Me.LinkLabelCopy)
        Me.GroupBoxConfiguration.Controls.Add(Me.TextBoxOutput)
        Me.GroupBoxConfiguration.Controls.Add(Me.LabelOutput)
        Me.GroupBoxConfiguration.Controls.Add(Me.TextBoxInput)
        Me.GroupBoxConfiguration.Controls.Add(Me.LabelInput)
        Me.GroupBoxConfiguration.Location = New System.Drawing.Point(12, 67)
        Me.GroupBoxConfiguration.Name = "GroupBoxConfiguration"
        Me.GroupBoxConfiguration.Size = New System.Drawing.Size(268, 157)
        Me.GroupBoxConfiguration.TabIndex = 1
        Me.GroupBoxConfiguration.TabStop = False
        Me.GroupBoxConfiguration.Text = "Config Value"
        '
        'LinkLabelCopy
        '
        Me.LinkLabelCopy.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LinkLabelCopy.AutoSize = True
        Me.LinkLabelCopy.Location = New System.Drawing.Point(169, 66)
        Me.LinkLabelCopy.Name = "LinkLabelCopy"
        Me.LinkLabelCopy.Size = New System.Drawing.Size(93, 13)
        Me.LinkLabelCopy.TabIndex = 4
        Me.LinkLabelCopy.TabStop = True
        Me.LinkLabelCopy.Text = "Copy to Clipboard"
        '
        'TextBoxOutput
        '
        Me.TextBoxOutput.Location = New System.Drawing.Point(9, 82)
        Me.TextBoxOutput.Multiline = True
        Me.TextBoxOutput.Name = "TextBoxOutput"
        Me.TextBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TextBoxOutput.Size = New System.Drawing.Size(253, 51)
        Me.TextBoxOutput.TabIndex = 3
        '
        'LabelOutput
        '
        Me.LabelOutput.AutoSize = True
        Me.LabelOutput.Location = New System.Drawing.Point(6, 66)
        Me.LabelOutput.Name = "LabelOutput"
        Me.LabelOutput.Size = New System.Drawing.Size(45, 13)
        Me.LabelOutput.TabIndex = 2
        Me.LabelOutput.Text = "Output:"
        '
        'TextBoxInput
        '
        Me.TextBoxInput.Location = New System.Drawing.Point(9, 33)
        Me.TextBoxInput.Name = "TextBoxInput"
        Me.TextBoxInput.Size = New System.Drawing.Size(253, 21)
        Me.TextBoxInput.TabIndex = 1
        '
        'LabelInput
        '
        Me.LabelInput.AutoSize = True
        Me.LabelInput.Location = New System.Drawing.Point(6, 17)
        Me.LabelInput.Name = "LabelInput"
        Me.LabelInput.Size = New System.Drawing.Size(37, 13)
        Me.LabelInput.TabIndex = 0
        Me.LabelInput.Text = "Input:"
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(292, 233)
        Me.Controls.Add(Me.GroupBoxConfiguration)
        Me.Controls.Add(Me.GroupBoxDirection)
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "Main"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "{0} v{1}"
        Me.GroupBoxDirection.ResumeLayout(False)
        Me.GroupBoxDirection.PerformLayout()
        Me.GroupBoxConfiguration.ResumeLayout(False)
        Me.GroupBoxConfiguration.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBoxDirection As System.Windows.Forms.GroupBox
    Friend WithEvents RadioButtonDecrypt As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButtonEncrypt As System.Windows.Forms.RadioButton
    Friend WithEvents GroupBoxConfiguration As System.Windows.Forms.GroupBox
    Friend WithEvents TextBoxOutput As System.Windows.Forms.TextBox
    Friend WithEvents LabelOutput As System.Windows.Forms.Label
    Friend WithEvents TextBoxInput As System.Windows.Forms.TextBox
    Friend WithEvents LabelInput As System.Windows.Forms.Label
    Friend WithEvents LinkLabelCopy As System.Windows.Forms.LinkLabel
End Class
