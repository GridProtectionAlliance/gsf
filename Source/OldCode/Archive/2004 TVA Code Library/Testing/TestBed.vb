Imports System.IO
Imports System.Text
Imports System.Reflection
Imports TVA.Testing
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Shared.String
Imports TVA.Shared.Common
Imports TVA.Config.Common
Imports TVA.Forms.Common
Imports VB = Microsoft.VisualBasic

Public Class TestBed

    Inherits System.Windows.Forms.Form
    Implements ILogger

#Region " Private Classes "

    ' This class compares two "System.Type" class instances by full name (used for sorting)
    Private Class TypeComparer

        Implements IComparer

        Private Shared DefaultComparer As TypeComparer

        Public Shared ReadOnly Property [Default]() As TypeComparer
            Get
                If DefaultComparer Is Nothing Then DefaultComparer = New TypeComparer
                Return DefaultComparer
            End Get
        End Property

        Public Function Compare(ByVal TypeA As Type, ByVal TypeB As Type) As Integer

            Return String.Compare(TypeA.FullName, TypeB.FullName, True)

        End Function

        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare

            If TypeOf x Is Type And TypeOf y Is Type Then
                Return Compare(DirectCast(x, Type), DirectCast(y, Type))
            Else
                Throw New ArgumentException("TypeComparer can only compare instances of ""System.Type""")
            End If

        End Function

    End Class

#End Region

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
    Friend WithEvents txtNumberOfTests As System.Windows.Forms.TextBox
    Friend WithEvents txtNumberOfSuccesses As System.Windows.Forms.TextBox
    Friend WithEvents txtNumberOfFailures As System.Windows.Forms.TextBox
    Friend WithEvents txtPercentSuccessful As System.Windows.Forms.TextBox
    Friend WithEvents txtTimeElapsed As System.Windows.Forms.TextBox
    Friend WithEvents btnStartTests As System.Windows.Forms.Button
    Friend WithEvents tvLog As System.Windows.Forms.TreeView
    Friend WithEvents txtTestClassName As System.Windows.Forms.TextBox
    Friend WithEvents txtStatus As System.Windows.Forms.TextBox
    Friend WithEvents pbrTaskProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents lblSelectTestFiles As System.Windows.Forms.Label
    Friend WithEvents lstSelectedAssemblies As System.Windows.Forms.CheckedListBox
    Friend WithEvents txtSelectedFiles As System.Windows.Forms.TextBox
    Friend WithEvents lblNumberOfTests As System.Windows.Forms.Label
    Friend WithEvents lblNumberOfSuccesses As System.Windows.Forms.Label
    Friend WithEvents lblNumberOfFailures As System.Windows.Forms.Label
    Friend WithEvents lblPercentSuccessful As System.Windows.Forms.Label
    Friend WithEvents lblTimeElapsed As System.Windows.Forms.Label
    Friend WithEvents grpTestStats As System.Windows.Forms.GroupBox
    Friend WithEvents lblTest As System.Windows.Forms.Label
    Friend WithEvents btnSelectFiles As System.Windows.Forms.Button
    Friend WithEvents OpenFileDialog As System.Windows.Forms.OpenFileDialog
    Friend WithEvents lblWildcardsOK As System.Windows.Forms.Label
    Friend WithEvents txtTestPath As System.Windows.Forms.TextBox
    Friend WithEvents lblTestPath As System.Windows.Forms.Label
    Friend WithEvents lblLocalPathNote As System.Windows.Forms.Label
    Friend WithEvents chkRandomTestOrder As System.Windows.Forms.CheckBox
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
		Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(TestBed))
		Me.btnStartTests = New System.Windows.Forms.Button
		Me.lblNumberOfTests = New System.Windows.Forms.Label
		Me.lblNumberOfSuccesses = New System.Windows.Forms.Label
		Me.lblNumberOfFailures = New System.Windows.Forms.Label
		Me.lblPercentSuccessful = New System.Windows.Forms.Label
		Me.txtNumberOfTests = New System.Windows.Forms.TextBox
		Me.txtNumberOfSuccesses = New System.Windows.Forms.TextBox
		Me.txtNumberOfFailures = New System.Windows.Forms.TextBox
		Me.txtPercentSuccessful = New System.Windows.Forms.TextBox
		Me.lblTimeElapsed = New System.Windows.Forms.Label
		Me.txtTimeElapsed = New System.Windows.Forms.TextBox
		Me.tvLog = New System.Windows.Forms.TreeView
		Me.grpTestStats = New System.Windows.Forms.GroupBox
		Me.lblTestPath = New System.Windows.Forms.Label
		Me.txtTestPath = New System.Windows.Forms.TextBox
		Me.txtTestClassName = New System.Windows.Forms.TextBox
		Me.lblTest = New System.Windows.Forms.Label
		Me.lblLocalPathNote = New System.Windows.Forms.Label
		Me.txtStatus = New System.Windows.Forms.TextBox
		Me.pbrTaskProgress = New System.Windows.Forms.ProgressBar
		Me.lstSelectedAssemblies = New System.Windows.Forms.CheckedListBox
		Me.txtSelectedFiles = New System.Windows.Forms.TextBox
		Me.lblSelectTestFiles = New System.Windows.Forms.Label
		Me.btnSelectFiles = New System.Windows.Forms.Button
		Me.OpenFileDialog = New System.Windows.Forms.OpenFileDialog
		Me.lblWildcardsOK = New System.Windows.Forms.Label
		Me.chkRandomTestOrder = New System.Windows.Forms.CheckBox
		Me.grpTestStats.SuspendLayout()
		Me.SuspendLayout()
		'
		'btnStartTests
		'
		Me.btnStartTests.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnStartTests.Font = New System.Drawing.Font("Arial", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnStartTests.Location = New System.Drawing.Point(840, 8)
		Me.btnStartTests.Name = "btnStartTests"
		Me.btnStartTests.Size = New System.Drawing.Size(112, 24)
		Me.btnStartTests.TabIndex = 4
		Me.btnStartTests.Text = "Start Tests"
		'
		'lblNumberOfTests
		'
		Me.lblNumberOfTests.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblNumberOfTests.Location = New System.Drawing.Point(8, 56)
		Me.lblNumberOfTests.Name = "lblNumberOfTests"
		Me.lblNumberOfTests.Size = New System.Drawing.Size(152, 23)
		Me.lblNumberOfTests.TabIndex = 2
		Me.lblNumberOfTests.Text = "Number of Tests:"
		Me.lblNumberOfTests.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'lblNumberOfSuccesses
		'
		Me.lblNumberOfSuccesses.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblNumberOfSuccesses.Location = New System.Drawing.Point(8, 88)
		Me.lblNumberOfSuccesses.Name = "lblNumberOfSuccesses"
		Me.lblNumberOfSuccesses.Size = New System.Drawing.Size(152, 23)
		Me.lblNumberOfSuccesses.TabIndex = 6
		Me.lblNumberOfSuccesses.Text = "Number of Successes:"
		Me.lblNumberOfSuccesses.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'lblNumberOfFailures
		'
		Me.lblNumberOfFailures.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblNumberOfFailures.Location = New System.Drawing.Point(312, 88)
		Me.lblNumberOfFailures.Name = "lblNumberOfFailures"
		Me.lblNumberOfFailures.Size = New System.Drawing.Size(136, 23)
		Me.lblNumberOfFailures.TabIndex = 8
		Me.lblNumberOfFailures.Text = "Number of Failures:"
		Me.lblNumberOfFailures.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'lblPercentSuccessful
		'
		Me.lblPercentSuccessful.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblPercentSuccessful.Location = New System.Drawing.Point(312, 56)
		Me.lblPercentSuccessful.Name = "lblPercentSuccessful"
		Me.lblPercentSuccessful.Size = New System.Drawing.Size(136, 23)
		Me.lblPercentSuccessful.TabIndex = 4
		Me.lblPercentSuccessful.Text = "Percent Successful:"
		Me.lblPercentSuccessful.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'txtNumberOfTests
		'
		Me.txtNumberOfTests.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtNumberOfTests.Location = New System.Drawing.Point(168, 56)
		Me.txtNumberOfTests.Name = "txtNumberOfTests"
		Me.txtNumberOfTests.ReadOnly = True
		Me.txtNumberOfTests.Size = New System.Drawing.Size(92, 21)
		Me.txtNumberOfTests.TabIndex = 3
		Me.txtNumberOfTests.TabStop = False
		Me.txtNumberOfTests.Text = ""
		'
		'txtNumberOfSuccesses
		'
		Me.txtNumberOfSuccesses.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtNumberOfSuccesses.Location = New System.Drawing.Point(168, 88)
		Me.txtNumberOfSuccesses.Name = "txtNumberOfSuccesses"
		Me.txtNumberOfSuccesses.ReadOnly = True
		Me.txtNumberOfSuccesses.Size = New System.Drawing.Size(92, 21)
		Me.txtNumberOfSuccesses.TabIndex = 7
		Me.txtNumberOfSuccesses.TabStop = False
		Me.txtNumberOfSuccesses.Text = ""
		'
		'txtNumberOfFailures
		'
		Me.txtNumberOfFailures.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtNumberOfFailures.Location = New System.Drawing.Point(452, 88)
		Me.txtNumberOfFailures.Name = "txtNumberOfFailures"
		Me.txtNumberOfFailures.ReadOnly = True
		Me.txtNumberOfFailures.Size = New System.Drawing.Size(92, 21)
		Me.txtNumberOfFailures.TabIndex = 9
		Me.txtNumberOfFailures.TabStop = False
		Me.txtNumberOfFailures.Text = ""
		'
		'txtPercentSuccessful
		'
		Me.txtPercentSuccessful.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtPercentSuccessful.Location = New System.Drawing.Point(452, 56)
		Me.txtPercentSuccessful.Name = "txtPercentSuccessful"
		Me.txtPercentSuccessful.ReadOnly = True
		Me.txtPercentSuccessful.Size = New System.Drawing.Size(92, 21)
		Me.txtPercentSuccessful.TabIndex = 5
		Me.txtPercentSuccessful.TabStop = False
		Me.txtPercentSuccessful.Text = ""
		'
		'lblTimeElapsed
		'
		Me.lblTimeElapsed.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblTimeElapsed.Location = New System.Drawing.Point(8, 120)
		Me.lblTimeElapsed.Name = "lblTimeElapsed"
		Me.lblTimeElapsed.Size = New System.Drawing.Size(152, 23)
		Me.lblTimeElapsed.TabIndex = 10
		Me.lblTimeElapsed.Text = "Time Elapsed:"
		Me.lblTimeElapsed.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'txtTimeElapsed
		'
		Me.txtTimeElapsed.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtTimeElapsed.Location = New System.Drawing.Point(168, 120)
		Me.txtTimeElapsed.Name = "txtTimeElapsed"
		Me.txtTimeElapsed.ReadOnly = True
		Me.txtTimeElapsed.Size = New System.Drawing.Size(376, 21)
		Me.txtTimeElapsed.TabIndex = 11
		Me.txtTimeElapsed.TabStop = False
		Me.txtTimeElapsed.Text = ""
		'
		'tvLog
		'
		Me.tvLog.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
					Or System.Windows.Forms.AnchorStyles.Left) _
					Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.tvLog.Font = New System.Drawing.Font("Arial", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.tvLog.ImageIndex = -1
		Me.tvLog.Location = New System.Drawing.Point(8, 208)
		Me.tvLog.Name = "tvLog"
		Me.tvLog.SelectedImageIndex = -1
		Me.tvLog.Size = New System.Drawing.Size(504, 328)
		Me.tvLog.TabIndex = 8
		Me.tvLog.TabStop = False
		'
		'grpTestStats
		'
		Me.grpTestStats.Controls.Add(Me.lblTestPath)
		Me.grpTestStats.Controls.Add(Me.txtTestPath)
		Me.grpTestStats.Controls.Add(Me.txtTestClassName)
		Me.grpTestStats.Controls.Add(Me.lblTest)
		Me.grpTestStats.Controls.Add(Me.lblNumberOfSuccesses)
		Me.grpTestStats.Controls.Add(Me.txtNumberOfTests)
		Me.grpTestStats.Controls.Add(Me.txtTimeElapsed)
		Me.grpTestStats.Controls.Add(Me.lblNumberOfTests)
		Me.grpTestStats.Controls.Add(Me.txtNumberOfSuccesses)
		Me.grpTestStats.Controls.Add(Me.txtNumberOfFailures)
		Me.grpTestStats.Controls.Add(Me.lblNumberOfFailures)
		Me.grpTestStats.Controls.Add(Me.txtPercentSuccessful)
		Me.grpTestStats.Controls.Add(Me.lblPercentSuccessful)
		Me.grpTestStats.Controls.Add(Me.lblTimeElapsed)
		Me.grpTestStats.Controls.Add(Me.lblLocalPathNote)
		Me.grpTestStats.Font = New System.Drawing.Font("Arial", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.grpTestStats.Location = New System.Drawing.Point(8, 8)
		Me.grpTestStats.Name = "grpTestStats"
		Me.grpTestStats.Size = New System.Drawing.Size(560, 192)
		Me.grpTestStats.TabIndex = 7
		Me.grpTestStats.TabStop = False
		Me.grpTestStats.Text = "Testing Statistics"
		'
		'lblTestPath
		'
		Me.lblTestPath.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblTestPath.Location = New System.Drawing.Point(8, 152)
		Me.lblTestPath.Name = "lblTestPath"
		Me.lblTestPath.Size = New System.Drawing.Size(152, 23)
		Me.lblTestPath.TabIndex = 13
		Me.lblTestPath.Text = "Test Path:"
		Me.lblTestPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'txtTestPath
		'
		Me.txtTestPath.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtTestPath.Location = New System.Drawing.Point(168, 152)
		Me.txtTestPath.Name = "txtTestPath"
		Me.txtTestPath.Size = New System.Drawing.Size(376, 21)
		Me.txtTestPath.TabIndex = 12
		Me.txtTestPath.Text = ""
		'
		'txtTestClassName
		'
		Me.txtTestClassName.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtTestClassName.Location = New System.Drawing.Point(168, 24)
		Me.txtTestClassName.Name = "txtTestClassName"
		Me.txtTestClassName.ReadOnly = True
		Me.txtTestClassName.Size = New System.Drawing.Size(376, 21)
		Me.txtTestClassName.TabIndex = 1
		Me.txtTestClassName.TabStop = False
		Me.txtTestClassName.Text = ""
		'
		'lblTest
		'
		Me.lblTest.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblTest.Location = New System.Drawing.Point(8, 24)
		Me.lblTest.Name = "lblTest"
		Me.lblTest.Size = New System.Drawing.Size(152, 23)
		Me.lblTest.TabIndex = 0
		Me.lblTest.Text = "Test:"
		Me.lblTest.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'lblLocalPathNote
		'
		Me.lblLocalPathNote.Font = New System.Drawing.Font("Arial", 8.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.lblLocalPathNote.Location = New System.Drawing.Point(168, 174)
		Me.lblLocalPathNote.Name = "lblLocalPathNote"
		Me.lblLocalPathNote.Size = New System.Drawing.Size(376, 16)
		Me.lblLocalPathNote.TabIndex = 14
		Me.lblLocalPathNote.Text = "Note: Using a local test path will increase test performance"
		'
		'txtStatus
		'
		Me.txtStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
					Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.txtStatus.BackColor = System.Drawing.Color.Black
		Me.txtStatus.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtStatus.ForeColor = System.Drawing.Color.White
		Me.txtStatus.Location = New System.Drawing.Point(520, 208)
		Me.txtStatus.MaxLength = 0
		Me.txtStatus.Multiline = True
		Me.txtStatus.Name = "txtStatus"
		Me.txtStatus.ReadOnly = True
		Me.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both
		Me.txtStatus.Size = New System.Drawing.Size(432, 296)
		Me.txtStatus.TabIndex = 9
		Me.txtStatus.TabStop = False
		Me.txtStatus.Text = ""
		Me.txtStatus.WordWrap = False
		'
		'pbrTaskProgress
		'
		Me.pbrTaskProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.pbrTaskProgress.Location = New System.Drawing.Point(520, 512)
		Me.pbrTaskProgress.Name = "pbrTaskProgress"
		Me.pbrTaskProgress.Size = New System.Drawing.Size(432, 23)
		Me.pbrTaskProgress.TabIndex = 10
		'
		'lstSelectedAssemblies
		'
		Me.lstSelectedAssemblies.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
					Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.lstSelectedAssemblies.CheckOnClick = True
		Me.lstSelectedAssemblies.Font = New System.Drawing.Font("Courier New", 9.0!)
		Me.lstSelectedAssemblies.Location = New System.Drawing.Point(576, 48)
		Me.lstSelectedAssemblies.Name = "lstSelectedAssemblies"
		Me.lstSelectedAssemblies.Size = New System.Drawing.Size(248, 148)
		Me.lstSelectedAssemblies.TabIndex = 3
		Me.lstSelectedAssemblies.ThreeDCheckBoxes = True
		'
		'txtSelectedFiles
		'
		Me.txtSelectedFiles.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
					Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.txtSelectedFiles.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.txtSelectedFiles.Location = New System.Drawing.Point(576, 24)
		Me.txtSelectedFiles.Name = "txtSelectedFiles"
		Me.txtSelectedFiles.Size = New System.Drawing.Size(220, 21)
		Me.txtSelectedFiles.TabIndex = 1
		Me.txtSelectedFiles.Text = "UnitTests\UnitTest*.dll"
		'
		'lblSelectTestFiles
		'
		Me.lblSelectTestFiles.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold)
		Me.lblSelectTestFiles.Location = New System.Drawing.Point(576, 8)
		Me.lblSelectTestFiles.Name = "lblSelectTestFiles"
		Me.lblSelectTestFiles.Size = New System.Drawing.Size(120, 16)
		Me.lblSelectTestFiles.TabIndex = 0
		Me.lblSelectTestFiles.Text = "&Select Test Files:"
		Me.lblSelectTestFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		'
		'btnSelectFiles
		'
		Me.btnSelectFiles.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnSelectFiles.Font = New System.Drawing.Font("Verdana", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.btnSelectFiles.Location = New System.Drawing.Point(794, 24)
		Me.btnSelectFiles.Name = "btnSelectFiles"
		Me.btnSelectFiles.Size = New System.Drawing.Size(30, 21)
		Me.btnSelectFiles.TabIndex = 2
		Me.btnSelectFiles.Text = "..."
		'
		'OpenFileDialog
		'
		Me.OpenFileDialog.Multiselect = True
		'
		'lblWildcardsOK
		'
		Me.lblWildcardsOK.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
					Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.lblWildcardsOK.Font = New System.Drawing.Font("Arial", 8.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle))
		Me.lblWildcardsOK.Location = New System.Drawing.Point(704, 8)
		Me.lblWildcardsOK.Name = "lblWildcardsOK"
		Me.lblWildcardsOK.Size = New System.Drawing.Size(120, 16)
		Me.lblWildcardsOK.TabIndex = 6
		Me.lblWildcardsOK.Text = "(Wildcards OK)"
		Me.lblWildcardsOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight
		'
		'chkRandomTestOrder
		'
		Me.chkRandomTestOrder.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.chkRandomTestOrder.Font = New System.Drawing.Font("Arial", 8.75!, System.Drawing.FontStyle.Bold)
		Me.chkRandomTestOrder.Location = New System.Drawing.Point(848, 40)
		Me.chkRandomTestOrder.Name = "chkRandomTestOrder"
		Me.chkRandomTestOrder.Size = New System.Drawing.Size(104, 32)
		Me.chkRandomTestOrder.TabIndex = 5
		Me.chkRandomTestOrder.Text = "Randomize Test Order"
		Me.chkRandomTestOrder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'TestBed
		'
		Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
		Me.ClientSize = New System.Drawing.Size(960, 541)
		Me.Controls.Add(Me.chkRandomTestOrder)
		Me.Controls.Add(Me.lblWildcardsOK)
		Me.Controls.Add(Me.btnSelectFiles)
		Me.Controls.Add(Me.lblSelectTestFiles)
		Me.Controls.Add(Me.txtSelectedFiles)
		Me.Controls.Add(Me.txtStatus)
		Me.Controls.Add(Me.lstSelectedAssemblies)
		Me.Controls.Add(Me.pbrTaskProgress)
		Me.Controls.Add(Me.grpTestStats)
		Me.Controls.Add(Me.tvLog)
		Me.Controls.Add(Me.btnStartTests)
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MinimumSize = New System.Drawing.Size(968, 568)
		Me.Name = "TestBed"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "TVA .NET Shared Code Library TestBed"
		Me.grpTestStats.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub

#End Region

#Region " Instance Variables "

    Private Const MaxStatusLength As Integer = 4194304 ' 4K buffer size for status window...

    Private Shared clrGood As System.Drawing.Color = System.Drawing.Color.LawnGreen
    Private Shared clrBad As System.Drawing.Color = System.Drawing.Color.Red
    Private Shared clrDefault As System.Drawing.Color 'set on form load

    Private appPath As String
    Private exePath As String
    Private startTime As Date
    Private startTimeTicks As Long
    Private lstAssemblies As New ArrayList
    Private arrAssembly As String() 'assembly names

#End Region

#Region " Main Form Events "

    Private Sub TestBed_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        exePath = AddPathSuffix(Application.StartupPath)
        appPath = exePath
        If appPath.EndsWith("bin\") Then appPath = VB.Left(appPath, Len(appPath) - 4)

        RestoreWindowSettings(Me)
        Variables.Create("TestBed.SelectedFiles", "UnitTests\UnitTest*.dll", VariableType.Text, "Last set of selected files")
        Variables.Create("TestBed.SelectedForTesting", "", VariableType.Text, "Indicies of selected files checked for testing")
        Variables.Create("TestBed.TestPath", exePath, VariableType.Text, "Test path provided for tests involving files")
        Variables.Create("TestBed.RandomTestOrder", False, VariableType.Bool, "Randomize test order setting")

        ' Restore previously selected files
        txtSelectedFiles.Text = Variables("TestBed.SelectedFiles")
        RefreshSelectedFileList()

        ' Restore previously checked items
        ResizeSelectedFileList(StringToArray(Variables("TestBed.SelectedForTesting"), GetType(Integer)))

        ' Restore last used test path
        txtTestPath.Text = Variables("TestBed.TestPath")
        ValidateTestPath()

        ' Restore randomized test order setting
        chkRandomTestOrder.Checked = Variables("TestBed.RandomTestOrder")

        clrDefault = txtPercentSuccessful.BackColor    ' remember default background color

        Variables.Save() ' We flush config file in case any changes have been made (create any initial vars)

    End Sub

    Private Sub TestBed_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed

        Variables("TestBed.SelectedFiles") = txtSelectedFiles.Text
        Variables("TestBed.SelectedForTesting") = ArrayToString(GetCheckedAssemblies())
        Variables("TestBed.TestPath") = txtTestPath.Text
        Variables("TestBed.RandomTestOrder") = chkRandomTestOrder.Checked
        SaveWindowSettings(Me)

    End Sub

    Private Sub TestBed_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize

        ResizeSelectedFileList()

    End Sub

    Private Sub txtTestPath_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtTestPath.LostFocus

        ValidateTestPath()

    End Sub

    Private Sub ValidateTestPath()

        ' Validate test path
        If Not Directory.Exists(txtTestPath.Text) Then txtTestPath.Text = exePath
        txtTestPath.Text = AddPathSuffix(txtTestPath.Text)

    End Sub

#End Region

#Region " Status Area & Progress Control Code "

    Private Sub ClearStatus()

        txtStatus.Text = ""

    End Sub

    Private Sub UpdateStatus(ByVal Status As String)

        txtStatus.Text = VB.Right(txtStatus.Text & Status & vbCrLf, MaxStatusLength)
        txtStatus.SelectionStart = Len(txtStatus.Text)
        txtStatus.ScrollToCaret()

    End Sub

    Public Sub TestProgress(ByVal Completed As Long, ByVal Total As Long) Implements Testing.ILogger.TestProgress

        With pbrTaskProgress
            .Value = Minimum(Completed / Total * (.Maximum - .Minimum), .Maximum)
        End With

        txtTimeElapsed.Text = SecondsToText(ElapsedTime, 4)
        Application.DoEvents()

    End Sub

#End Region

#Region " Test Assembly Selection Code "

    'popus up a file dialog for the user to select one or more assemblies containing test classes,
    'which are typically named starting with 'test'
    Private Function SelectTestAssemblies() As String()

        OpenFileDialog.InitialDirectory = appPath
        OpenFileDialog.Filter = "Test Class Assembly files (*.dll)|*.dll|UnitTest Class Assembly files (UnitTest*.dll)|UnitTest*.dll"
        OpenFileDialog.FilterIndex = 1
        OpenFileDialog.RestoreDirectory = False

        If OpenFileDialog.ShowDialog() = DialogResult.OK Then
            Return OpenFileDialog.FileNames()
        Else
            Return New String() {""}
        End If

    End Function

    Private Sub txtSelectedFiles_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSelectedFiles.TextChanged

        RefreshSelectedFileList()

    End Sub

    Private Sub btnSelectFiles_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectFiles.Click

        Dim selectedFiles As New StringBuilder

        For Each selectedFile As String In SelectTestAssemblies()
            If selectedFiles.Length > 0 Then selectedFiles.Append(", ")
            selectedFiles.Append(selectedFile)
        Next

        If selectedFiles.Length > 0 Then txtSelectedFiles.Text = selectedFiles.ToString()

    End Sub

    Private Sub btnStartTests_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStartTests.Click

        ' Get selected unit test assemblies
        With lstSelectedAssemblies
            If .CheckedIndices.Count > 0 Then
                Dim intIndex As Integer

                arrAssembly = Array.CreateInstance(GetType(String), .CheckedIndices.Count)

                For Each intFullPathIndex As Integer In .CheckedIndices
                    arrAssembly(intIndex) = lstAssemblies(intFullPathIndex)
                    intIndex += 1
                Next
            Else
                arrAssembly = Nothing
            End If
        End With

        If Not arrAssembly Is Nothing Then RunTests()

    End Sub

    Private Sub RefreshSelectedFileList()

        SyncLock lstAssemblies.SyncRoot
            lstAssemblies.Clear()               ' We keep the full path in this list
            lstSelectedAssemblies.Items.Clear() ' We keep the trimmed display path and checked state in this control

            For Each selectedFile As String In txtSelectedFiles.Text.Split(","c)
                selectedFile = Trim(selectedFile)

                If Len(selectedFile) > 0 Then
                    ' Add root application path to file name if none was specified
                    If Len(JustFileName(selectedFile)) = Len(selectedFile) Then selectedFile = appPath & selectedFile
                    If Not Directory.Exists(JustPath(selectedFile)) Then selectedFile = appPath & selectedFile

                    If Directory.Exists(JustPath(selectedFile)) Then
                        ' Loop through each matched file (takes wildcards into account if any specified)
                        For Each matchedFile As String In Directory.GetFiles(JustPath(selectedFile), JustFileName(selectedFile))
                            lstAssemblies.Add(matchedFile)
                            FitTextToControl(matchedFile)
                            lstSelectedAssemblies.Items.Add(matchedFile, True)
                        Next
                    End If
                End If
            Next
        End SyncLock

    End Sub

    Private Sub ResizeSelectedFileList(Optional ByVal intStates As Integer() = Nothing)

        SyncLock lstAssemblies.SyncRoot
            Dim strDisplayText As String

            If intStates Is Nothing Then
                intStates = GetCheckedAssemblies()
            Else
                If intStates.Length = 0 Then intStates = GetCheckedAssemblies()
            End If

            ' Repopulate checked list box control with "resized" strDisplayText elements
            lstSelectedAssemblies.Items.Clear()

            For x As Integer = 0 To lstAssemblies.Count - 1
                strDisplayText = lstAssemblies(x)
                FitTextToControl(strDisplayText)
                lstSelectedAssemblies.Items.Add(strDisplayText, (Array.BinarySearch(intStates, x) >= 0))
            Next
        End SyncLock

    End Sub

    Private Function GetCheckedAssemblies() As Integer()

        Dim intStates As Integer()

        ' Creates sorted array of indexes that are checked so we can restore item "checked" state
        With lstSelectedAssemblies.CheckedIndices
            intStates = Array.CreateInstance(GetType(Integer), .Count)
            .CopyTo(intStates, 0)
            Array.Sort(intStates)
        End With

        Return intStates

    End Function

    Private Sub FitTextToControl(ByRef Text As String)

        Const EstimatedCheckBoxSize As Integer = 30

        With lstSelectedAssemblies
            Dim strText As String = Text
            Dim intWidth As Integer = Len(Text)
            Dim objGraphics As Drawing.Graphics = .CreateGraphics()

            Do While objGraphics.MeasureString(strText, .Font).Width > (.ClientSize.Width - EstimatedCheckBoxSize) And intWidth > 12
                intWidth -= 1
                strText = TrimFileName(Text, intWidth)
            Loop

            Text = strText
        End With

    End Sub

#End Region

#Region " Tests Execution Code "

    ' We use the application path as the test path for any tests that need to test with files
    Public ReadOnly Property TestPath() As String Implements Testing.ILogger.TestPath
        Get
            Return txtTestPath.Text
        End Get
    End Property

    'return elapsed time as decimal seconds based on 100-nanosecond Ticks properties
    Private Function ElapsedTime() As Double
        Dim tNow As Date = Now()
        Dim eTicks As Long = tNow.Ticks() - startTimeTicks    'a tick is 100 nanoseconds
        Return eTicks / 10000000    '100 nanoseconds is one ten-millionth of a second
    End Function

    Private Sub ShowTestStatistics(ByVal className As String, _
      ByVal iNumFailed As Integer, _
      ByVal iNumSuccessful As Integer, _
      ByVal iNumTests As Integer)
        Dim dblElapsedTime As Double = ElapsedTime()
        Dim pct As Double = 0
        If iNumTests > 0 Then
            pct = CDbl(iNumSuccessful) / CDbl(iNumTests)
        End If

        txtTestClassName.Text = className
        txtNumberOfFailures.Text = iNumFailed
        txtNumberOfSuccesses.Text = iNumSuccessful
        txtNumberOfTests.Text = iNumTests
        txtPercentSuccessful.Text = Format(pct, "0.0000%")
        If pct < 1.0 Then
            txtPercentSuccessful.BackColor = clrBad
        Else
            txtPercentSuccessful.BackColor = clrGood
        End If
        txtTimeElapsed.Text = SecondsToText(dblElapsedTime, 4)
        Application.DoEvents()
    End Sub

    Private Sub ClearTestStatistics()
        tvLog.Nodes.Clear()
        txtTestClassName.Text = ""
        txtNumberOfFailures.Text = ""
        txtNumberOfSuccesses.Text = ""
        txtNumberOfTests.Text = ""
        txtPercentSuccessful.Text = ""
        txtPercentSuccessful.BackColor = clrDefault
        txtTimeElapsed.Text = ""
    End Sub

    Private Sub RunTests()

        btnStartTests.Enabled = False    'start button off

        ClearTestStatistics()    'clears treelist nodes

        Dim assemblyNode As TreeNode
        Dim classNode As TreeNode

        Dim overallStartTime As Date = Now()
        Dim overallStartTimeTicks As Long = overallStartTime.Ticks()

        Dim oTester As TestBase

        Dim totalFailedTests As Integer
        Dim totalSuccessfulTests As Integer
        Dim totalTests As Integer
        Dim testCount As Integer

        Dim allTestsNode As TreeNode = New TreeNode("Starting all test assemblies at " & overallStartTime)
        allTestsNode.Tag = False    'we use tag to record if any child failed
        tvLog.Nodes.Add(allTestsNode)
        tvLog.SelectedNode = allTestsNode

        ' If requested, randomize order of test assemblies
        If chkRandomTestOrder.Checked Then
            Randomize()
            ScrambleArray(arrAssembly)
        End If

        'run all test classes in given assemblies
        For Each elem As String In arrAssembly
            Dim assemblyFailedTests As Integer = 0
            Dim assemblySuccessfulTests As Integer = 0
            Dim assemblyTests As Integer = 0
            Dim assemblyTypes As New ArrayList

            Dim assemblyStartTime As Date = Now()
            Dim assemblyStartTimeTicks As Long = assemblyStartTime.Ticks()

            assemblyNode = New TreeNode("Starting tests in assembly " & JustFileName(elem) & " at " & assemblyStartTime)
            assemblyNode.Tag = False    'we use tag to record if any child failed
            tvLog.SelectedNode.Nodes.Add(assemblyNode)
            tvLog.SelectedNode = assemblyNode

            'load assembly given name
            Dim oAssembly As [Assembly]
            oAssembly = [Assembly].LoadFrom(elem)
            If oAssembly Is Nothing Then
                assemblyNode.Text = "Could not load assembly " & JustFileName(elem) & ", no tests run!"
            Else
                Dim oType As Type
                Dim oTypes As Type()
                oTypes = oAssembly.GetTypes()
                If oTypes Is Nothing Then
                    assemblyNode.Text = "Assembly " & JustFileName(elem) & " had no types, no tests run!"
                Else
                    'scan assembly for test classes
                    For Each oType In oTypes
                        ' JRC - changed such that any class derived from TestBase in assembly will be tested, regardless of name...
                        ' I tried making this use TypeOf or Type.Equals but couldn't get it to work from dynamically loaded assembly,
                        ' so I ended up just comparing it by full name :p
                        If oType.IsClass() AndAlso String.Compare(oType.BaseType.FullName, GetType(TestBase).FullName) = 0 Then
                            ' JRC - changed to load all test types into a sorted list such that we can execute the test classes in the desired order
                            assemblyTypes.Add(oType)
                        End If
                    Next

                    ' If requested, randomize order of test classes
                    If chkRandomTestOrder.Checked Then
                        ScrambleList(assemblyTypes)
                    Else
                        assemblyTypes.Sort(TypeComparer.Default)
                    End If

                    For x As Integer = 0 To assemblyTypes.Count - 1
                        oType = assemblyTypes(x)
                        startTime = Now()
                        startTimeTicks = startTime.Ticks()

                        Dim pct As Double = 0
                        txtTestClassName.Text = oType.Name
                        classNode = New TreeNode("Starting tests in class " & oType.Name & " at " & startTime)
                        classNode.Tag = False       'we use tag to record if any child failed
                        tvLog.SelectedNode.Nodes.Add(classNode)
                        tvLog.SelectedNode = classNode

                        Try
                            'make instance of test class
                            oTester = CType(Activator.CreateInstance(oType), TestBase)
                            oTester.Logger = Me          'this form logs events from testing via ILogger interface
                            txtTestClassName.Text = oType.Name

                            'execute test class instance
                            pct = oTester.RunTests(chkRandomTestOrder.Checked)

                            'accumulate results for assembly
                            assemblyFailedTests += oTester.FailedTests
                            assemblySuccessfulTests += oTester.SuccessfulTests
                            assemblyTests += oTester.TestCount

                            'show results
                            ShowTestStatistics(oType.Name, oTester.FailedTests, oTester.SuccessfulTests, oTester.TestCount)
                            If pct < 1.0 Then
                                classNode.BackColor = clrBad
                                classNode.Tag = True          'some child must have failed
                            Else
                                txtPercentSuccessful.BackColor = clrGood
                                classNode.BackColor = clrGood
                            End If
                            classNode.Text = "Tests in class " & oType.Name & " completed: " & _
                               txtPercentSuccessful.Text.Trim() & " successful, " & _
                               oTester.TestCount & " tests, " & txtTimeElapsed.Text.Trim()
						Catch ex As Exception
							If Not ex.InnerException Is Nothing Then
								classNode.Text = "Error in test class " & oType.Name & ": " & ex.InnerException.Message()
							Else
								classNode.Text = "Error in test class " & oType.Name & ": " & ex.Message()
							End If
							classNode.BackColor = clrBad
						End Try

						tvLog.SelectedNode = assemblyNode
					Next

                    'update stats for assembly
                    startTime = assemblyStartTime
                    startTimeTicks = assemblyStartTimeTicks
                    ShowTestStatistics("All Tests in Assembly " & JustFileName(elem), assemblyFailedTests, assemblySuccessfulTests, assemblyTests)

                    ' JRC - sometimes was getting here with a null oTester, hence the following...
                    If oTester Is Nothing Then
                        testCount = 0
                    Else
                        testCount = oTester.TestCount
                    End If

                    assemblyNode.Text = "Tests in assembly " & JustFileName(elem) & " completed: " & _
                       txtPercentSuccessful.Text.Trim() & " successful, " & _
                       testCount & " tests, " & txtTimeElapsed.Text.Trim()

                    If assemblySuccessfulTests < assemblyTests Then       'at least one test failed
                        assemblyNode.BackColor = clrBad
                        assemblyNode.Tag = True       'some child must have failed
                    Else
                        txtPercentSuccessful.BackColor = clrGood
                        assemblyNode.BackColor = clrGood
                    End If

                    'accumulate results for all test assemblies
                    totalFailedTests += assemblyFailedTests
                    totalSuccessfulTests += assemblySuccessfulTests
                    totalTests += assemblyTests

                End If
            End If
            tvLog.SelectedNode = allTestsNode
        Next

        'show overall results at end
        startTime = overallStartTime
        startTimeTicks = overallStartTimeTicks
        ShowTestStatistics("All Tests in all Assemblies", totalFailedTests, totalSuccessfulTests, totalTests)

        ' JRC - sometimes was getting here with a null oTester, hence the following...
        If oTester Is Nothing Then
            testCount = 0
        Else
            testCount = oTester.TestCount
        End If

        allTestsNode.Text = "All Tests in all Assemblies completed: " & _
          txtPercentSuccessful.Text.Trim() & " successful, " & _
          testCount & " tests, " & txtTimeElapsed.Text.Trim()

        If totalSuccessfulTests < totalTests Then    'at least one test failed
            allTestsNode.BackColor = clrBad
            allTestsNode.Tag = True    'some child must have failed
        Else
            txtPercentSuccessful.BackColor = clrGood
            allTestsNode.BackColor = clrGood
        End If
        tvLog.SelectedNode = allTestsNode

        btnStartTests.Enabled = True    'start button on
    End Sub

    Public Sub LogEvent(ByVal sTestName As String, ByVal type As EventType, _
       ByVal iNumTests As Integer, ByVal iNumFailures As Integer, ByVal errMsg As String) Implements ILogger.LogEvent
        Dim str As String = sTestName
        Dim newNode As TreeNode = New TreeNode
        Select Case type
            Case EventType.Starting
                str &= " starting..."
                newNode.Text = str
                newNode.Tag = False    'we use tag to record if any child failed
                tvLog.SelectedNode.Nodes.Add(newNode)
                tvLog.SelectedNode = newNode
            Case EventType.Status
                UpdateStatus(errMsg)
            Case EventType.Success
                str &= " was successful."
                newNode.Text = str
                newNode.BackColor = clrGood
                tvLog.SelectedNode.Nodes.Add(newNode)
            Case EventType.Failure
                str &= " failed!"
                If Not errMsg Is Nothing Then
                    newNode.Text = str & ": " & errMsg       'add error message on failure
                Else
                    newNode.Text = str
                End If
                newNode.BackColor = clrBad
                tvLog.SelectedNode.Tag = True       'we use tag to record if any child failed
                tvLog.SelectedNode.Nodes.Add(newNode)
                newNode.Expand()
            Case EventType.Ending
                tvLog.SelectedNode.Text = Replace(tvLog.SelectedNode.Text, " starting...", " completed.")
                If CBool(tvLog.SelectedNode.Tag) Then
                    tvLog.SelectedNode.BackColor = clrBad
                    tvLog.SelectedNode.Parent.Tag = True       'we use tag to record if any child failed
                    tvLog.SelectedNode.Expand()
                Else
                    tvLog.SelectedNode.BackColor = clrGood
                    tvLog.SelectedNode.Collapse()
                End If
                tvLog.SelectedNode = tvLog.SelectedNode.Parent
            Case Else
                'do nothing
        End Select

        'update test statistics for user feedback
        ShowTestStatistics(txtTestClassName.Text, iNumFailures, iNumTests - iNumFailures, iNumTests)
    End Sub

#End Region

End Class
