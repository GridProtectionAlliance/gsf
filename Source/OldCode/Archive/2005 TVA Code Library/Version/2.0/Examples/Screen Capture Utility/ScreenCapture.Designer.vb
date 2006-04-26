<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ScreenCapture
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
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ScreenCapture))
        Me.LabelSourceScreen = New System.Windows.Forms.Label
        Me.ComboBoxSourceScreen = New System.Windows.Forms.ComboBox
        Me.ComboBoxImageFormat = New System.Windows.Forms.ComboBox
        Me.LabelImageFormat = New System.Windows.Forms.Label
        Me.LabelCaptureInterval = New System.Windows.Forms.Label
        Me.TextBoxCaptureSeconds = New System.Windows.Forms.TextBox
        Me.LabelSeconds = New System.Windows.Forms.Label
        Me.TextBoxCaptureFilename = New System.Windows.Forms.TextBox
        Me.LabelCaptureFilename = New System.Windows.Forms.Label
        Me.ButtonSelectCaptureFilename = New System.Windows.Forms.Button
        Me.GroupBoxCaptureSizes = New System.Windows.Forms.GroupBox
        Me.TextBoxCustomPercent = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.CheckBoxCustomPercent = New System.Windows.Forms.CheckBox
        Me.CheckBox25Percent = New System.Windows.Forms.CheckBox
        Me.CheckBox50Percent = New System.Windows.Forms.CheckBox
        Me.CheckBox75Percent = New System.Windows.Forms.CheckBox
        Me.CheckBox100Percent = New System.Windows.Forms.CheckBox
        Me.GroupBoxCaptureArea = New System.Windows.Forms.GroupBox
        Me.TextBoxCaptureCoordinates = New System.Windows.Forms.TextBox
        Me.LabelCustomAreaFormat = New System.Windows.Forms.Label
        Me.RadioButtonCustomArea = New System.Windows.Forms.RadioButton
        Me.RadioButtonFullScreen = New System.Windows.Forms.RadioButton
        Me.SaveFileDialog = New System.Windows.Forms.SaveFileDialog
        Me.CaptureImageTimer = New System.Windows.Forms.Timer(Me.components)
        Me.CheckBoxAutoStart = New System.Windows.Forms.CheckBox
        Me.CheckBoxStartMinimized = New System.Windows.Forms.CheckBox
        Me.ButtonStartCapture = New System.Windows.Forms.Button
        Me.ButtonExit = New System.Windows.Forms.Button
        Me.GroupBoxCaptureSizes.SuspendLayout()
        Me.GroupBoxCaptureArea.SuspendLayout()
        Me.SuspendLayout()
        '
        'LabelSourceScreen
        '
        Me.LabelSourceScreen.AutoSize = True
        Me.LabelSourceScreen.Location = New System.Drawing.Point(19, 12)
        Me.LabelSourceScreen.Name = "LabelSourceScreen"
        Me.LabelSourceScreen.Size = New System.Drawing.Size(81, 13)
        Me.LabelSourceScreen.TabIndex = 0
        Me.LabelSourceScreen.Text = "&Source Screen:"
        Me.LabelSourceScreen.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ComboBoxSourceScreen
        '
        Me.ComboBoxSourceScreen.FormattingEnabled = True
        Me.ComboBoxSourceScreen.Location = New System.Drawing.Point(99, 9)
        Me.ComboBoxSourceScreen.Name = "ComboBoxSourceScreen"
        Me.ComboBoxSourceScreen.Size = New System.Drawing.Size(128, 21)
        Me.ComboBoxSourceScreen.TabIndex = 1
        '
        'ComboBoxImageFormat
        '
        Me.ComboBoxImageFormat.FormattingEnabled = True
        Me.ComboBoxImageFormat.Items.AddRange(New Object() {"Bmp", "Emf", "Exif", "Gif", "Icon", "Jpeg", "Png", "Tiff", "Wmf"})
        Me.ComboBoxImageFormat.Location = New System.Drawing.Point(99, 36)
        Me.ComboBoxImageFormat.Name = "ComboBoxImageFormat"
        Me.ComboBoxImageFormat.Size = New System.Drawing.Size(72, 21)
        Me.ComboBoxImageFormat.TabIndex = 3
        '
        'LabelImageFormat
        '
        Me.LabelImageFormat.AutoSize = True
        Me.LabelImageFormat.Location = New System.Drawing.Point(26, 39)
        Me.LabelImageFormat.Name = "LabelImageFormat"
        Me.LabelImageFormat.Size = New System.Drawing.Size(74, 13)
        Me.LabelImageFormat.TabIndex = 2
        Me.LabelImageFormat.Text = "&Image Format:"
        Me.LabelImageFormat.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'LabelCaptureInterval
        '
        Me.LabelCaptureInterval.AutoSize = True
        Me.LabelCaptureInterval.Location = New System.Drawing.Point(12, 66)
        Me.LabelCaptureInterval.Name = "LabelCaptureInterval"
        Me.LabelCaptureInterval.Size = New System.Drawing.Size(85, 13)
        Me.LabelCaptureInterval.TabIndex = 4
        Me.LabelCaptureInterval.Text = "Capture &Interval:"
        Me.LabelCaptureInterval.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'TextBoxCaptureSeconds
        '
        Me.TextBoxCaptureSeconds.Location = New System.Drawing.Point(99, 63)
        Me.TextBoxCaptureSeconds.Name = "TextBoxCaptureSeconds"
        Me.TextBoxCaptureSeconds.Size = New System.Drawing.Size(49, 20)
        Me.TextBoxCaptureSeconds.TabIndex = 5
        Me.TextBoxCaptureSeconds.Text = "60"
        '
        'LabelSeconds
        '
        Me.LabelSeconds.AutoSize = True
        Me.LabelSeconds.Location = New System.Drawing.Point(144, 66)
        Me.LabelSeconds.Name = "LabelSeconds"
        Me.LabelSeconds.Size = New System.Drawing.Size(50, 13)
        Me.LabelSeconds.TabIndex = 6
        Me.LabelSeconds.Text = " seconds"
        Me.LabelSeconds.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'TextBoxCaptureFilename
        '
        Me.TextBoxCaptureFilename.Location = New System.Drawing.Point(99, 87)
        Me.TextBoxCaptureFilename.Name = "TextBoxCaptureFilename"
        Me.TextBoxCaptureFilename.Size = New System.Drawing.Size(277, 20)
        Me.TextBoxCaptureFilename.TabIndex = 8
        '
        'LabelCaptureFilename
        '
        Me.LabelCaptureFilename.AutoSize = True
        Me.LabelCaptureFilename.Location = New System.Drawing.Point(8, 90)
        Me.LabelCaptureFilename.Name = "LabelCaptureFilename"
        Me.LabelCaptureFilename.Size = New System.Drawing.Size(92, 13)
        Me.LabelCaptureFilename.TabIndex = 7
        Me.LabelCaptureFilename.Text = "Capture &Filename:"
        Me.LabelCaptureFilename.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ButtonSelectCaptureFilename
        '
        Me.ButtonSelectCaptureFilename.Font = New System.Drawing.Font("Verdana", 9.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonSelectCaptureFilename.Location = New System.Drawing.Point(373, 87)
        Me.ButtonSelectCaptureFilename.Name = "ButtonSelectCaptureFilename"
        Me.ButtonSelectCaptureFilename.Size = New System.Drawing.Size(35, 23)
        Me.ButtonSelectCaptureFilename.TabIndex = 9
        Me.ButtonSelectCaptureFilename.Text = "..."
        Me.ButtonSelectCaptureFilename.UseVisualStyleBackColor = True
        '
        'GroupBoxCaptureSizes
        '
        Me.GroupBoxCaptureSizes.Controls.Add(Me.TextBoxCustomPercent)
        Me.GroupBoxCaptureSizes.Controls.Add(Me.Label1)
        Me.GroupBoxCaptureSizes.Controls.Add(Me.CheckBoxCustomPercent)
        Me.GroupBoxCaptureSizes.Controls.Add(Me.CheckBox25Percent)
        Me.GroupBoxCaptureSizes.Controls.Add(Me.CheckBox50Percent)
        Me.GroupBoxCaptureSizes.Controls.Add(Me.CheckBox75Percent)
        Me.GroupBoxCaptureSizes.Controls.Add(Me.CheckBox100Percent)
        Me.GroupBoxCaptureSizes.Location = New System.Drawing.Point(15, 113)
        Me.GroupBoxCaptureSizes.Name = "GroupBoxCaptureSizes"
        Me.GroupBoxCaptureSizes.Size = New System.Drawing.Size(393, 56)
        Me.GroupBoxCaptureSizes.TabIndex = 17
        Me.GroupBoxCaptureSizes.TabStop = False
        Me.GroupBoxCaptureSizes.Text = "Capture Sizes"
        '
        'TextBoxCustomPercent
        '
        Me.TextBoxCustomPercent.Location = New System.Drawing.Point(312, 21)
        Me.TextBoxCustomPercent.Name = "TextBoxCustomPercent"
        Me.TextBoxCustomPercent.Size = New System.Drawing.Size(33, 20)
        Me.TextBoxCustomPercent.TabIndex = 22
        Me.TextBoxCustomPercent.Text = "200"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(342, 25)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(18, 13)
        Me.Label1.TabIndex = 23
        Me.Label1.Text = " %"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'CheckBoxCustomPercent
        '
        Me.CheckBoxCustomPercent.AutoSize = True
        Me.CheckBoxCustomPercent.Location = New System.Drawing.Point(252, 24)
        Me.CheckBoxCustomPercent.Name = "CheckBoxCustomPercent"
        Me.CheckBoxCustomPercent.Size = New System.Drawing.Size(64, 17)
        Me.CheckBoxCustomPercent.TabIndex = 21
        Me.CheckBoxCustomPercent.Text = "Custom:"
        Me.CheckBoxCustomPercent.UseVisualStyleBackColor = True
        '
        'CheckBox25Percent
        '
        Me.CheckBox25Percent.AutoSize = True
        Me.CheckBox25Percent.Location = New System.Drawing.Point(200, 24)
        Me.CheckBox25Percent.Name = "CheckBox25Percent"
        Me.CheckBox25Percent.Size = New System.Drawing.Size(46, 17)
        Me.CheckBox25Percent.TabIndex = 20
        Me.CheckBox25Percent.Text = "25%"
        Me.CheckBox25Percent.UseVisualStyleBackColor = True
        '
        'CheckBox50Percent
        '
        Me.CheckBox50Percent.AutoSize = True
        Me.CheckBox50Percent.Location = New System.Drawing.Point(148, 24)
        Me.CheckBox50Percent.Name = "CheckBox50Percent"
        Me.CheckBox50Percent.Size = New System.Drawing.Size(46, 17)
        Me.CheckBox50Percent.TabIndex = 19
        Me.CheckBox50Percent.Text = "50%"
        Me.CheckBox50Percent.UseVisualStyleBackColor = True
        '
        'CheckBox75Percent
        '
        Me.CheckBox75Percent.AutoSize = True
        Me.CheckBox75Percent.Location = New System.Drawing.Point(96, 24)
        Me.CheckBox75Percent.Name = "CheckBox75Percent"
        Me.CheckBox75Percent.Size = New System.Drawing.Size(46, 17)
        Me.CheckBox75Percent.TabIndex = 18
        Me.CheckBox75Percent.Text = "75%"
        Me.CheckBox75Percent.UseVisualStyleBackColor = True
        '
        'CheckBox100Percent
        '
        Me.CheckBox100Percent.AutoSize = True
        Me.CheckBox100Percent.Checked = True
        Me.CheckBox100Percent.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBox100Percent.Location = New System.Drawing.Point(38, 24)
        Me.CheckBox100Percent.Name = "CheckBox100Percent"
        Me.CheckBox100Percent.Size = New System.Drawing.Size(52, 17)
        Me.CheckBox100Percent.TabIndex = 17
        Me.CheckBox100Percent.Text = "100%"
        Me.CheckBox100Percent.UseVisualStyleBackColor = True
        '
        'GroupBoxCaptureArea
        '
        Me.GroupBoxCaptureArea.Controls.Add(Me.TextBoxCaptureCoordinates)
        Me.GroupBoxCaptureArea.Controls.Add(Me.LabelCustomAreaFormat)
        Me.GroupBoxCaptureArea.Controls.Add(Me.RadioButtonCustomArea)
        Me.GroupBoxCaptureArea.Controls.Add(Me.RadioButtonFullScreen)
        Me.GroupBoxCaptureArea.Location = New System.Drawing.Point(15, 175)
        Me.GroupBoxCaptureArea.Name = "GroupBoxCaptureArea"
        Me.GroupBoxCaptureArea.Size = New System.Drawing.Size(393, 56)
        Me.GroupBoxCaptureArea.TabIndex = 18
        Me.GroupBoxCaptureArea.TabStop = False
        Me.GroupBoxCaptureArea.Text = "Capture Area"
        '
        'TextBoxCaptureCoordinates
        '
        Me.TextBoxCaptureCoordinates.Location = New System.Drawing.Point(182, 22)
        Me.TextBoxCaptureCoordinates.Name = "TextBoxCaptureCoordinates"
        Me.TextBoxCaptureCoordinates.Size = New System.Drawing.Size(111, 20)
        Me.TextBoxCaptureCoordinates.TabIndex = 7
        Me.TextBoxCaptureCoordinates.Text = "0,0,1024,768"
        '
        'LabelCustomAreaFormat
        '
        Me.LabelCustomAreaFormat.AutoSize = True
        Me.LabelCustomAreaFormat.Location = New System.Drawing.Point(294, 28)
        Me.LabelCustomAreaFormat.Name = "LabelCustomAreaFormat"
        Me.LabelCustomAreaFormat.Size = New System.Drawing.Size(89, 13)
        Me.LabelCustomAreaFormat.TabIndex = 8
        Me.LabelCustomAreaFormat.Text = "x, y, width, height"
        Me.LabelCustomAreaFormat.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        'RadioButtonCustomArea
        '
        Me.RadioButtonCustomArea.AutoSize = True
        Me.RadioButtonCustomArea.Location = New System.Drawing.Point(94, 23)
        Me.RadioButtonCustomArea.Name = "RadioButtonCustomArea"
        Me.RadioButtonCustomArea.Size = New System.Drawing.Size(88, 17)
        Me.RadioButtonCustomArea.TabIndex = 1
        Me.RadioButtonCustomArea.Text = "Custom Area:"
        Me.RadioButtonCustomArea.UseVisualStyleBackColor = True
        '
        'RadioButtonFullScreen
        '
        Me.RadioButtonFullScreen.AutoSize = True
        Me.RadioButtonFullScreen.Checked = True
        Me.RadioButtonFullScreen.Location = New System.Drawing.Point(10, 23)
        Me.RadioButtonFullScreen.Name = "RadioButtonFullScreen"
        Me.RadioButtonFullScreen.Size = New System.Drawing.Size(78, 17)
        Me.RadioButtonFullScreen.TabIndex = 0
        Me.RadioButtonFullScreen.TabStop = True
        Me.RadioButtonFullScreen.Text = "Full Screen"
        Me.RadioButtonFullScreen.UseVisualStyleBackColor = True
        '
        'SaveFileDialog
        '
        Me.SaveFileDialog.Filter = "All Image Files|*.jpeg;*.jpg;*.gif;*.bmp;*.ico;*.emf;*.exif;*.png;*.tiff;*.wmf|Al" & _
            "l Files|*.*"
        '
        'CheckBoxAutoStart
        '
        Me.CheckBoxAutoStart.AutoSize = True
        Me.CheckBoxAutoStart.CheckAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.CheckBoxAutoStart.Location = New System.Drawing.Point(266, 62)
        Me.CheckBoxAutoStart.Name = "CheckBoxAutoStart"
        Me.CheckBoxAutoStart.Size = New System.Drawing.Size(142, 17)
        Me.CheckBoxAutoStart.TabIndex = 19
        Me.CheckBoxAutoStart.Text = "&Begin capture on startup"
        Me.CheckBoxAutoStart.UseVisualStyleBackColor = True
        '
        'CheckBoxStartMinimized
        '
        Me.CheckBoxStartMinimized.AutoSize = True
        Me.CheckBoxStartMinimized.CheckAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.CheckBoxStartMinimized.Location = New System.Drawing.Point(300, 40)
        Me.CheckBoxStartMinimized.Name = "CheckBoxStartMinimized"
        Me.CheckBoxStartMinimized.Size = New System.Drawing.Size(108, 17)
        Me.CheckBoxStartMinimized.TabIndex = 20
        Me.CheckBoxStartMinimized.Text = "Startup &minimized"
        Me.CheckBoxStartMinimized.UseVisualStyleBackColor = True
        '
        'ButtonStartCapture
        '
        Me.ButtonStartCapture.Location = New System.Drawing.Point(238, 7)
        Me.ButtonStartCapture.Name = "ButtonStartCapture"
        Me.ButtonStartCapture.Size = New System.Drawing.Size(82, 23)
        Me.ButtonStartCapture.TabIndex = 21
        Me.ButtonStartCapture.Text = "Start &Capture"
        Me.ButtonStartCapture.UseVisualStyleBackColor = True
        '
        'ButtonExit
        '
        Me.ButtonExit.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButtonExit.Location = New System.Drawing.Point(326, 7)
        Me.ButtonExit.Name = "ButtonExit"
        Me.ButtonExit.Size = New System.Drawing.Size(82, 23)
        Me.ButtonExit.TabIndex = 22
        Me.ButtonExit.Text = "E&xit"
        Me.ButtonExit.UseVisualStyleBackColor = True
        '
        'ScreenCapture
        '
        Me.AcceptButton = Me.ButtonStartCapture
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButtonExit
        Me.ClientSize = New System.Drawing.Size(420, 246)
        Me.Controls.Add(Me.ButtonExit)
        Me.Controls.Add(Me.ButtonStartCapture)
        Me.Controls.Add(Me.CheckBoxStartMinimized)
        Me.Controls.Add(Me.CheckBoxAutoStart)
        Me.Controls.Add(Me.GroupBoxCaptureArea)
        Me.Controls.Add(Me.GroupBoxCaptureSizes)
        Me.Controls.Add(Me.ButtonSelectCaptureFilename)
        Me.Controls.Add(Me.TextBoxCaptureFilename)
        Me.Controls.Add(Me.LabelCaptureFilename)
        Me.Controls.Add(Me.TextBoxCaptureSeconds)
        Me.Controls.Add(Me.LabelCaptureInterval)
        Me.Controls.Add(Me.ComboBoxImageFormat)
        Me.Controls.Add(Me.LabelImageFormat)
        Me.Controls.Add(Me.ComboBoxSourceScreen)
        Me.Controls.Add(Me.LabelSourceScreen)
        Me.Controls.Add(Me.LabelSeconds)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "ScreenCapture"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = " Screen Capture Utility"
        Me.GroupBoxCaptureSizes.ResumeLayout(False)
        Me.GroupBoxCaptureSizes.PerformLayout()
        Me.GroupBoxCaptureArea.ResumeLayout(False)
        Me.GroupBoxCaptureArea.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents LabelSourceScreen As System.Windows.Forms.Label
    Friend WithEvents ComboBoxSourceScreen As System.Windows.Forms.ComboBox
    Friend WithEvents ComboBoxImageFormat As System.Windows.Forms.ComboBox
    Friend WithEvents LabelImageFormat As System.Windows.Forms.Label
    Friend WithEvents LabelCaptureInterval As System.Windows.Forms.Label
    Friend WithEvents TextBoxCaptureSeconds As System.Windows.Forms.TextBox
    Friend WithEvents LabelSeconds As System.Windows.Forms.Label
    Friend WithEvents TextBoxCaptureFilename As System.Windows.Forms.TextBox
    Friend WithEvents LabelCaptureFilename As System.Windows.Forms.Label
    Friend WithEvents ButtonSelectCaptureFilename As System.Windows.Forms.Button
    Friend WithEvents GroupBoxCaptureSizes As System.Windows.Forms.GroupBox
    Friend WithEvents TextBoxCustomPercent As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CheckBoxCustomPercent As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox25Percent As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox50Percent As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox75Percent As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox100Percent As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBoxCaptureArea As System.Windows.Forms.GroupBox
    Friend WithEvents SaveFileDialog As System.Windows.Forms.SaveFileDialog
    Friend WithEvents CaptureImageTimer As System.Windows.Forms.Timer
    Friend WithEvents CheckBoxAutoStart As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBoxStartMinimized As System.Windows.Forms.CheckBox
    Friend WithEvents ButtonStartCapture As System.Windows.Forms.Button
    Friend WithEvents ButtonExit As System.Windows.Forms.Button
    Friend WithEvents RadioButtonCustomArea As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButtonFullScreen As System.Windows.Forms.RadioButton
    Friend WithEvents TextBoxCaptureCoordinates As System.Windows.Forms.TextBox
    Friend WithEvents LabelCustomAreaFormat As System.Windows.Forms.Label

End Class
