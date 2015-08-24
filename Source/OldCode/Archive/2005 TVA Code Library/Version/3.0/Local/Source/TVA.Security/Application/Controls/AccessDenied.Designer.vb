Namespace Application.Controls

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AccessDenied
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AccessDenied))
            Me.ButtonRequestAccess = New System.Windows.Forms.Button
            Me.ButtonExitApplication = New System.Windows.Forms.Button
            Me.Label1 = New System.Windows.Forms.Label
            Me.SuspendLayout()
            '
            'ButtonRequestAccess
            '
            Me.ButtonRequestAccess.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.ButtonRequestAccess.Location = New System.Drawing.Point(282, 83)
            Me.ButtonRequestAccess.Name = "ButtonRequestAccess"
            Me.ButtonRequestAccess.Size = New System.Drawing.Size(100, 23)
            Me.ButtonRequestAccess.TabIndex = 1
            Me.ButtonRequestAccess.TabStop = False
            Me.ButtonRequestAccess.Text = "Request Access"
            Me.ButtonRequestAccess.UseVisualStyleBackColor = True
            '
            'ButtonExitApplication
            '
            Me.ButtonExitApplication.Location = New System.Drawing.Point(12, 83)
            Me.ButtonExitApplication.Name = "ButtonExitApplication"
            Me.ButtonExitApplication.Size = New System.Drawing.Size(100, 23)
            Me.ButtonExitApplication.TabIndex = 2
            Me.ButtonExitApplication.TabStop = False
            Me.ButtonExitApplication.Text = "Exit Application"
            Me.ButtonExitApplication.UseVisualStyleBackColor = True
            '
            'Label1
            '
            Me.Label1.Location = New System.Drawing.Point(9, 9)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(373, 58)
            Me.Label1.TabIndex = 0
            Me.Label1.Text = resources.GetString("Label1.Text")
            '
            'AccessDenied
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(394, 118)
            Me.ControlBox = False
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.ButtonExitApplication)
            Me.Controls.Add(Me.ButtonRequestAccess)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.Name = "AccessDenied"
            Me.ShowInTaskbar = False
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            Me.Text = "Access Denied"
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents ButtonRequestAccess As System.Windows.Forms.Button
        Friend WithEvents ButtonExitApplication As System.Windows.Forms.Button
        Friend WithEvents Label1 As System.Windows.Forms.Label
    End Class

End Namespace