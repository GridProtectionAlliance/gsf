<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DebugUI
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DebugUI))
        Me.LabelDebugUI = New System.Windows.Forms.Label
        Me.PictureBoxDebugUI = New System.Windows.Forms.PictureBox
        Me.SPDCService = New TVASPDC.Service
        CType(Me.PictureBoxDebugUI, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LabelDebugUI
        '
        Me.LabelDebugUI.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LabelDebugUI.Location = New System.Drawing.Point(68, 12)
        Me.LabelDebugUI.Name = "LabelDebugUI"
        Me.LabelDebugUI.Size = New System.Drawing.Size(289, 51)
        Me.LabelDebugUI.TabIndex = 0
        Me.LabelDebugUI.Text = "TVA SPDC is running in debug mode. Note that this window acts as the primary thre" & _
            "ad while debugging, as a result closing this window will terminate the applicati" & _
            "on."
        Me.LabelDebugUI.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'PictureBoxDebugUI
        '
        Me.PictureBoxDebugUI.Image = CType(resources.GetObject("PictureBoxDebugUI.Image"), System.Drawing.Image)
        Me.PictureBoxDebugUI.Location = New System.Drawing.Point(12, 12)
        Me.PictureBoxDebugUI.Name = "PictureBoxDebugUI"
        Me.PictureBoxDebugUI.Size = New System.Drawing.Size(50, 50)
        Me.PictureBoxDebugUI.TabIndex = 1
        Me.PictureBoxDebugUI.TabStop = False
        '
        'SPDCService
        '
        Me.SPDCService.CanPauseAndContinue = True
        Me.SPDCService.CanShutdown = True
        Me.SPDCService.ExitCode = 0
        Me.SPDCService.ServiceName = "TVASPDC"
        '
        'DebugUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(369, 72)
        Me.Controls.Add(Me.PictureBoxDebugUI)
        Me.Controls.Add(Me.LabelDebugUI)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "DebugUI"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "TVA SPDC (Debug Mode)"
        CType(Me.PictureBoxDebugUI, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents LabelDebugUI As System.Windows.Forms.Label
    Friend WithEvents PictureBoxDebugUI As System.Windows.Forms.PictureBox
    Friend WithEvents SPDCService As TVASPDC.Service
End Class
