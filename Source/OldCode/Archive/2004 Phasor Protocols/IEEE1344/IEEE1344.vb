Imports System.Text
Imports System.IO
Imports System.Reflection
Imports TVA.Forms.Common
Imports TVA.Config.Common
Imports TVA.Shared.Bit
Imports TVA.Shared.FilePath
Imports TVA.Compression.Common
Imports TVA.EE.Phasor
Imports VB = Microsoft.VisualBasic
Imports Dundas.Charting.WinControl

Public Class IEEE1344Listener

    Inherits System.Windows.Forms.Form

    Private WithEvents m_parser As IEEE1344.FrameParser
    Private m_configFile As IEEE1344.ConfigurationFile
    Private m_initializingTitle As Dundas.Charting.WinControl.Title
    Private m_frequencyTitle As String
    Private m_phasorTitle As String
    Private m_frameQueue As ArrayList
    Private m_frequencyRange As DataPoints
    Private m_frameCount As Long
    Private m_firstFrame As Long
    Private m_startTime As Long
    Private m_total As Long
    
    Const FrequencyPoints As Integer = 50
    Const PhasorPoints As Integer = 20

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
    Friend WithEvents SampleCount As System.Windows.Forms.Label
    Friend WithEvents SampleRate As System.Windows.Forms.Label
    Friend WithEvents SamplesLabel As System.Windows.Forms.Label
    Friend WithEvents SampleRateLabel As System.Windows.Forms.Label
    Friend WithEvents SamplesPerSecLabel As System.Windows.Forms.Label
    Friend WithEvents ListenOnPortLabel As System.Windows.Forms.Label
    Friend WithEvents Listen As System.Windows.Forms.Button
    Friend WithEvents ConnectToIPLabel As System.Windows.Forms.Label
    Friend WithEvents IDLabel As System.Windows.Forms.Label
    Friend WithEvents DataChart As Dundas.Charting.WinControl.Chart
    Friend WithEvents TextErrorListener As TVA.ErrorManagement.TextErrorListener
    Friend WithEvents DataFrame As System.Windows.Forms.Label
    Friend WithEvents HeaderFrame As System.Windows.Forms.TextBox
    Friend WithEvents PMUHeaderLabel As System.Windows.Forms.Label
    Friend WithEvents DataFrameLabel As System.Windows.Forms.Label
    Friend WithEvents CopyrightLabel As System.Windows.Forms.Label
    Friend WithEvents FrequencyTrigger As System.Windows.Forms.CheckBox
    Friend WithEvents TriggerGroupBox As System.Windows.Forms.GroupBox
    Friend WithEvents DfDtTrigger As System.Windows.Forms.CheckBox
    Friend WithEvents AngleTrigger As System.Windows.Forms.CheckBox
    Friend WithEvents OverCurrentTrigger As System.Windows.Forms.CheckBox
    Friend WithEvents UnderVoltageTrigger As System.Windows.Forms.CheckBox
    Friend WithEvents RateTrigger As System.Windows.Forms.CheckBox
    Friend WithEvents CustomTrigger As System.Windows.Forms.CheckBox
    Friend WithEvents FrameProcessor As System.Timers.Timer
    Friend WithEvents GraphFrameLagLabel As System.Windows.Forms.Label
    Friend WithEvents GraphFrameLag As System.Windows.Forms.Label
    Friend WithEvents PMUPort As System.Windows.Forms.TextBox
    Friend WithEvents PMUIP As System.Windows.Forms.ComboBox
    Friend WithEvents PMUID As System.Windows.Forms.ComboBox
    Friend WithEvents PhasorSelectionGroupBox As System.Windows.Forms.GroupBox
    Friend WithEvents VoltagePhasorLabel As System.Windows.Forms.Label
    Friend WithEvents VoltagePhasor As System.Windows.Forms.ComboBox
    Friend WithEvents CurrentPhasor As System.Windows.Forms.ComboBox
    Friend WithEvents CurrentPhasorLabel As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim ChartArea1 As Dundas.Charting.WinControl.ChartArea = New Dundas.Charting.WinControl.ChartArea
        Dim ChartArea2 As Dundas.Charting.WinControl.ChartArea = New Dundas.Charting.WinControl.ChartArea
        Dim Legend1 As Dundas.Charting.WinControl.Legend = New Dundas.Charting.WinControl.Legend
        Dim Legend2 As Dundas.Charting.WinControl.Legend = New Dundas.Charting.WinControl.Legend
        Dim Series1 As Dundas.Charting.WinControl.Series = New Dundas.Charting.WinControl.Series
        Dim Series2 As Dundas.Charting.WinControl.Series = New Dundas.Charting.WinControl.Series
        Dim Title1 As Dundas.Charting.WinControl.Title = New Dundas.Charting.WinControl.Title
        Dim Title2 As Dundas.Charting.WinControl.Title = New Dundas.Charting.WinControl.Title
        Dim Title3 As Dundas.Charting.WinControl.Title = New Dundas.Charting.WinControl.Title
        Dim Title4 As Dundas.Charting.WinControl.Title = New Dundas.Charting.WinControl.Title
        Dim Title5 As Dundas.Charting.WinControl.Title = New Dundas.Charting.WinControl.Title
        Dim Title6 As Dundas.Charting.WinControl.Title = New Dundas.Charting.WinControl.Title
        Dim configurationAppSettings As System.Configuration.AppSettingsReader = New System.Configuration.AppSettingsReader
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(IEEE1344Listener))
        Me.DataFrame = New System.Windows.Forms.Label
        Me.SamplesLabel = New System.Windows.Forms.Label
        Me.SampleCount = New System.Windows.Forms.Label
        Me.SampleRate = New System.Windows.Forms.Label
        Me.SampleRateLabel = New System.Windows.Forms.Label
        Me.SamplesPerSecLabel = New System.Windows.Forms.Label
        Me.ListenOnPortLabel = New System.Windows.Forms.Label
        Me.Listen = New System.Windows.Forms.Button
        Me.ConnectToIPLabel = New System.Windows.Forms.Label
        Me.IDLabel = New System.Windows.Forms.Label
        Me.DataChart = New Dundas.Charting.WinControl.Chart
        Me.TextErrorListener = New TVA.ErrorManagement.TextErrorListener
        Me.HeaderFrame = New System.Windows.Forms.TextBox
        Me.PMUHeaderLabel = New System.Windows.Forms.Label
        Me.DataFrameLabel = New System.Windows.Forms.Label
        Me.CopyrightLabel = New System.Windows.Forms.Label
        Me.TriggerGroupBox = New System.Windows.Forms.GroupBox
        Me.CustomTrigger = New System.Windows.Forms.CheckBox
        Me.RateTrigger = New System.Windows.Forms.CheckBox
        Me.UnderVoltageTrigger = New System.Windows.Forms.CheckBox
        Me.OverCurrentTrigger = New System.Windows.Forms.CheckBox
        Me.AngleTrigger = New System.Windows.Forms.CheckBox
        Me.DfDtTrigger = New System.Windows.Forms.CheckBox
        Me.FrequencyTrigger = New System.Windows.Forms.CheckBox
        Me.FrameProcessor = New System.Timers.Timer
        Me.GraphFrameLagLabel = New System.Windows.Forms.Label
        Me.GraphFrameLag = New System.Windows.Forms.Label
        Me.PMUPort = New System.Windows.Forms.TextBox
        Me.PMUIP = New System.Windows.Forms.ComboBox
        Me.PMUID = New System.Windows.Forms.ComboBox
        Me.PhasorSelectionGroupBox = New System.Windows.Forms.GroupBox
        Me.CurrentPhasor = New System.Windows.Forms.ComboBox
        Me.CurrentPhasorLabel = New System.Windows.Forms.Label
        Me.VoltagePhasor = New System.Windows.Forms.ComboBox
        Me.VoltagePhasorLabel = New System.Windows.Forms.Label
        CType(Me.DataChart, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TriggerGroupBox.SuspendLayout()
        CType(Me.FrameProcessor, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PhasorSelectionGroupBox.SuspendLayout()
        Me.SuspendLayout()
        '
        'DataFrame
        '
        Me.DataFrame.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.DataFrame.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DataFrame.Location = New System.Drawing.Point(8, 400)
        Me.DataFrame.Name = "DataFrame"
        Me.DataFrame.Size = New System.Drawing.Size(318, 104)
        Me.DataFrame.TabIndex = 18
        '
        'SamplesLabel
        '
        Me.SamplesLabel.Location = New System.Drawing.Point(0, 40)
        Me.SamplesLabel.Name = "SamplesLabel"
        Me.SamplesLabel.Size = New System.Drawing.Size(56, 16)
        Me.SamplesLabel.TabIndex = 8
        Me.SamplesLabel.Text = "Samples:"
        Me.SamplesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'SampleCount
        '
        Me.SampleCount.Location = New System.Drawing.Point(56, 40)
        Me.SampleCount.Name = "SampleCount"
        Me.SampleCount.Size = New System.Drawing.Size(72, 16)
        Me.SampleCount.TabIndex = 9
        Me.SampleCount.Text = "0"
        Me.SampleCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'SampleRate
        '
        Me.SampleRate.Location = New System.Drawing.Point(200, 40)
        Me.SampleRate.Name = "SampleRate"
        Me.SampleRate.Size = New System.Drawing.Size(56, 16)
        Me.SampleRate.TabIndex = 11
        Me.SampleRate.Text = "0"
        Me.SampleRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'SampleRateLabel
        '
        Me.SampleRateLabel.Location = New System.Drawing.Point(120, 40)
        Me.SampleRateLabel.Name = "SampleRateLabel"
        Me.SampleRateLabel.Size = New System.Drawing.Size(80, 16)
        Me.SampleRateLabel.TabIndex = 10
        Me.SampleRateLabel.Text = "Sample Rate:"
        Me.SampleRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'SamplesPerSecLabel
        '
        Me.SamplesPerSecLabel.Location = New System.Drawing.Point(256, 40)
        Me.SamplesPerSecLabel.Name = "SamplesPerSecLabel"
        Me.SamplesPerSecLabel.Size = New System.Drawing.Size(88, 16)
        Me.SamplesPerSecLabel.TabIndex = 12
        Me.SamplesPerSecLabel.Text = "samples/second"
        Me.SamplesPerSecLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ListenOnPortLabel
        '
        Me.ListenOnPortLabel.Location = New System.Drawing.Point(352, 12)
        Me.ListenOnPortLabel.Name = "ListenOnPortLabel"
        Me.ListenOnPortLabel.Size = New System.Drawing.Size(80, 16)
        Me.ListenOnPortLabel.TabIndex = 4
        Me.ListenOnPortLabel.Text = "Listen on &port:"
        Me.ListenOnPortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Listen
        '
        Me.Listen.Location = New System.Drawing.Point(504, 8)
        Me.Listen.Name = "Listen"
        Me.Listen.TabIndex = 6
        Me.Listen.Text = "&Listen"
        '
        'ConnectToIPLabel
        '
        Me.ConnectToIPLabel.Location = New System.Drawing.Point(0, 12)
        Me.ConnectToIPLabel.Name = "ConnectToIPLabel"
        Me.ConnectToIPLabel.Size = New System.Drawing.Size(80, 16)
        Me.ConnectToIPLabel.TabIndex = 0
        Me.ConnectToIPLabel.Text = "Connect to &IP:"
        Me.ConnectToIPLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'IDLabel
        '
        Me.IDLabel.Location = New System.Drawing.Point(184, 12)
        Me.IDLabel.Name = "IDLabel"
        Me.IDLabel.Size = New System.Drawing.Size(32, 16)
        Me.IDLabel.TabIndex = 2
        Me.IDLabel.Text = "I&D:"
        Me.IDLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'DataChart
        '
        Me.DataChart.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.DataChart.BorderLineWidth = 0
        Me.DataChart.BorderSkin.FrameBorderWidth = 0
        ChartArea1.AxisX.LabelStyle.Enabled = False
        ChartArea1.AxisX.LabelStyle.Interval = 0
        ChartArea1.AxisX.LabelStyle.IntervalOffset = 0
        ChartArea1.AxisX.LabelStyle.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX.LabelStyle.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX.LineWidth = 0
        ChartArea1.AxisX.MajorGrid.Enabled = False
        ChartArea1.AxisX.MajorGrid.Interval = 0
        ChartArea1.AxisX.MajorGrid.IntervalOffset = 0
        ChartArea1.AxisX.MajorGrid.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX.MajorGrid.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX.MajorTickMark.Enabled = False
        ChartArea1.AxisX.MajorTickMark.Interval = 0
        ChartArea1.AxisX.MajorTickMark.IntervalOffset = 0
        ChartArea1.AxisX.MajorTickMark.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX.MajorTickMark.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX.MinorGrid.Enabled = False
        ChartArea1.AxisX.MinorTickMark.Enabled = False
        ChartArea1.AxisX2.LabelStyle.Interval = 0
        ChartArea1.AxisX2.LabelStyle.IntervalOffset = 0
        ChartArea1.AxisX2.LabelStyle.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX2.LabelStyle.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX2.MajorGrid.Interval = 0
        ChartArea1.AxisX2.MajorGrid.IntervalOffset = 0
        ChartArea1.AxisX2.MajorGrid.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX2.MajorGrid.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX2.MajorTickMark.Interval = 0
        ChartArea1.AxisX2.MajorTickMark.IntervalOffset = 0
        ChartArea1.AxisX2.MajorTickMark.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisX2.MajorTickMark.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY.LabelStyle.Enabled = False
        ChartArea1.AxisY.LabelStyle.Interval = 0
        ChartArea1.AxisY.LabelStyle.IntervalOffset = 0
        ChartArea1.AxisY.LabelStyle.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY.LabelStyle.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY.LineWidth = 0
        ChartArea1.AxisY.MajorGrid.Enabled = False
        ChartArea1.AxisY.MajorGrid.Interval = 0
        ChartArea1.AxisY.MajorGrid.IntervalOffset = 0
        ChartArea1.AxisY.MajorGrid.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY.MajorGrid.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY.MajorTickMark.Enabled = False
        ChartArea1.AxisY.MajorTickMark.Interval = 0
        ChartArea1.AxisY.MajorTickMark.IntervalOffset = 0
        ChartArea1.AxisY.MajorTickMark.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY.MajorTickMark.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY.MinorGrid.Enabled = False
        ChartArea1.AxisY.MinorTickMark.Enabled = False
        ChartArea1.AxisY2.LabelStyle.Interval = 0
        ChartArea1.AxisY2.LabelStyle.IntervalOffset = 0
        ChartArea1.AxisY2.LabelStyle.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY2.LabelStyle.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY2.MajorGrid.Enabled = False
        ChartArea1.AxisY2.MajorGrid.Interval = 0
        ChartArea1.AxisY2.MajorGrid.IntervalOffset = 0
        ChartArea1.AxisY2.MajorGrid.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY2.MajorGrid.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY2.MajorTickMark.Enabled = False
        ChartArea1.AxisY2.MajorTickMark.Interval = 0
        ChartArea1.AxisY2.MajorTickMark.IntervalOffset = 0
        ChartArea1.AxisY2.MajorTickMark.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY2.MajorTickMark.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea1.AxisY2.MinorGrid.Enabled = False
        ChartArea1.AxisY2.MinorTickMark.Enabled = False
        ChartArea1.BorderWidth = 0
        ChartArea1.Name = "Frequency"
        ChartArea1.Position.Auto = False
        ChartArea1.Position.Height = 30.0!
        ChartArea1.Position.Width = 90.0!
        ChartArea1.Position.X = 3.5!
        ChartArea1.Position.Y = 10.0!
        ChartArea2.AxisX.Enabled = Dundas.Charting.WinControl.AxisEnabled.True
        ChartArea2.AxisX.LabelStyle.Enabled = False
        ChartArea2.AxisX.LabelStyle.Interval = 0
        ChartArea2.AxisX.LabelStyle.IntervalOffset = 0
        ChartArea2.AxisX.LabelStyle.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX.LabelStyle.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX.LineWidth = 0
        ChartArea2.AxisX.MajorGrid.Enabled = False
        ChartArea2.AxisX.MajorGrid.Interval = 0
        ChartArea2.AxisX.MajorGrid.IntervalOffset = 0
        ChartArea2.AxisX.MajorGrid.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX.MajorGrid.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX.MajorTickMark.Enabled = False
        ChartArea2.AxisX.MajorTickMark.Interval = 0
        ChartArea2.AxisX.MajorTickMark.IntervalOffset = 0
        ChartArea2.AxisX.MajorTickMark.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX.MajorTickMark.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX.MarksNextToAxis = False
        ChartArea2.AxisX.MinorGrid.Enabled = False
        ChartArea2.AxisX.MinorTickMark.Enabled = False
        ChartArea2.AxisX2.LabelStyle.Interval = 0
        ChartArea2.AxisX2.LabelStyle.IntervalOffset = 0
        ChartArea2.AxisX2.LabelStyle.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX2.LabelStyle.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX2.MajorGrid.Interval = 0
        ChartArea2.AxisX2.MajorGrid.IntervalOffset = 0
        ChartArea2.AxisX2.MajorGrid.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX2.MajorGrid.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX2.MajorTickMark.Interval = 0
        ChartArea2.AxisX2.MajorTickMark.IntervalOffset = 0
        ChartArea2.AxisX2.MajorTickMark.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisX2.MajorTickMark.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY.Enabled = Dundas.Charting.WinControl.AxisEnabled.False
        ChartArea2.AxisY.LabelStyle.Format = "000"
        ChartArea2.AxisY.LabelStyle.Interval = 0
        ChartArea2.AxisY.LabelStyle.IntervalOffset = 0
        ChartArea2.AxisY.LabelStyle.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY.LabelStyle.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY.LineWidth = 0
        ChartArea2.AxisY.MajorGrid.Enabled = False
        ChartArea2.AxisY.MajorGrid.Interval = 0
        ChartArea2.AxisY.MajorGrid.IntervalOffset = 0
        ChartArea2.AxisY.MajorGrid.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY.MajorGrid.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY.MajorTickMark.Enabled = False
        ChartArea2.AxisY.MajorTickMark.Interval = 0
        ChartArea2.AxisY.MajorTickMark.IntervalOffset = 0
        ChartArea2.AxisY.MajorTickMark.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY.MajorTickMark.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY.MinorGrid.Enabled = False
        ChartArea2.AxisY.MinorTickMark.Enabled = False
        ChartArea2.AxisY2.Enabled = Dundas.Charting.WinControl.AxisEnabled.True
        ChartArea2.AxisY2.LabelsAutoFit = False
        ChartArea2.AxisY2.LabelStyle.Font = New System.Drawing.Font("Verdana", 7.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        ChartArea2.AxisY2.LabelStyle.Format = "0°"
        ChartArea2.AxisY2.LabelStyle.Interval = 0
        ChartArea2.AxisY2.LabelStyle.IntervalOffset = 0
        ChartArea2.AxisY2.LabelStyle.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY2.LabelStyle.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY2.LineWidth = 0
        ChartArea2.AxisY2.MajorGrid.Enabled = False
        ChartArea2.AxisY2.MajorGrid.Interval = 0
        ChartArea2.AxisY2.MajorGrid.IntervalOffset = 0
        ChartArea2.AxisY2.MajorGrid.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY2.MajorGrid.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY2.MajorTickMark.Enabled = False
        ChartArea2.AxisY2.MajorTickMark.Interval = 0
        ChartArea2.AxisY2.MajorTickMark.IntervalOffset = 0
        ChartArea2.AxisY2.MajorTickMark.IntervalOffsetType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY2.MajorTickMark.IntervalType = Dundas.Charting.WinControl.DateTimeIntervalType.Auto
        ChartArea2.AxisY2.MarksNextToAxis = False
        ChartArea2.AxisY2.MinorGrid.Enabled = False
        ChartArea2.AxisY2.MinorTickMark.Enabled = False
        ChartArea2.BorderWidth = 0
        ChartArea2.Name = "Phasors"
        ChartArea2.Position.Auto = False
        ChartArea2.Position.Height = 60.0!
        ChartArea2.Position.Width = 65.0!
        ChartArea2.Position.X = 4.0!
        ChartArea2.Position.Y = 40.0!
        Me.DataChart.ChartAreas.Add(ChartArea1)
        Me.DataChart.ChartAreas.Add(ChartArea2)
        Legend1.AutoFitText = False
        Legend1.Enabled = False
        Legend1.LegendStyle = Dundas.Charting.WinControl.LegendStyle.Table
        Legend1.Name = "Default"
        Legend2.AutoFitText = False
        Legend2.BorderWidth = 0
        Legend2.DockInsideChartArea = False
        Legend2.DockToChartArea = "Phasors"
        Legend2.EquallySpacedItems = True
        Legend2.Font = New System.Drawing.Font("Lucida Console", 7.25!)
        Legend2.Name = "PhasorLegend"
        Me.DataChart.Legends.Add(Legend1)
        Me.DataChart.Legends.Add(Legend2)
        Me.DataChart.Location = New System.Drawing.Point(8, 64)
        Me.DataChart.Name = "DataChart"
        Me.DataChart.Palette = Dundas.Charting.WinControl.ChartColorPalette.Pastel
        Series1.BorderWidth = 3
        Series1.ChartArea = "Frequency"
        Series1.ChartType = "Spline"
        Series1.LegendText = "Frequency"
        Series1.Name = "Frequency"
        Series2.ChartArea = "Phasors"
        Series2.ChartType = "Spline"
        Series2.Name = "PhasorReference"
        Me.DataChart.Series.Add(Series1)
        Me.DataChart.Series.Add(Series2)
        Me.DataChart.Size = New System.Drawing.Size(538, 312)
        Me.DataChart.SoftShadows = False
        Me.DataChart.TabIndex = 16
        Me.DataChart.TabStop = False
        Title1.DockOffset = -1
        Title1.Font = New System.Drawing.Font("Verdana", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Title1.Text = "PMU ID"
        Title2.Docking = Dundas.Charting.WinControl.Docking.Left
        Title2.DockOffset = -4
        Title2.DockToChartArea = "Frequency"
        Title2.Font = New System.Drawing.Font("Verdana", 8.0!)
        Title2.Text = "Freq"
        Title3.Docking = Dundas.Charting.WinControl.Docking.Left
        Title3.DockOffset = -5
        Title3.DockToChartArea = "Phasors"
        Title3.Font = New System.Drawing.Font("Verdana", 8.0!)
        Title3.Text = "Phase Angles"
        Title4.Alignment = System.Drawing.ContentAlignment.BottomRight
        Title4.Docking = Dundas.Charting.WinControl.Docking.Bottom
        Title4.DockInsideChartArea = False
        Title4.DockOffset = 2
        Title4.Font = New System.Drawing.Font("Verdana", 7.25!)
        Title4.Text = "Vars: unavailable"
        Title5.Alignment = System.Drawing.ContentAlignment.BottomRight
        Title5.Docking = Dundas.Charting.WinControl.Docking.Bottom
        Title5.DockInsideChartArea = False
        Title5.DockOffset = 4
        Title5.Font = New System.Drawing.Font("Verdana", 7.25!)
        Title5.Text = "Power: unavailable"
        Title6.Docking = Dundas.Charting.WinControl.Docking.Bottom
        Title6.DockOffset = -50
        Title6.Font = New System.Drawing.Font("Verdana", 9.75!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Title6.Text = "Initializing..."
        Me.DataChart.Titles.Add(Title1)
        Me.DataChart.Titles.Add(Title2)
        Me.DataChart.Titles.Add(Title3)
        Me.DataChart.Titles.Add(Title4)
        Me.DataChart.Titles.Add(Title5)
        Me.DataChart.Titles.Add(Title6)
        '
        'TextErrorListener
        '
        Me.TextErrorListener.FileName = CType(configurationAppSettings.GetValue("TextErrorListener.FileName", GetType(System.String)), String)
        Me.TextErrorListener.WasErrorReceived = False
        '
        'HeaderFrame
        '
        Me.HeaderFrame.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.HeaderFrame.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.HeaderFrame.Location = New System.Drawing.Point(334, 400)
        Me.HeaderFrame.Multiline = True
        Me.HeaderFrame.Name = "HeaderFrame"
        Me.HeaderFrame.ReadOnly = True
        Me.HeaderFrame.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.HeaderFrame.Size = New System.Drawing.Size(338, 112)
        Me.HeaderFrame.TabIndex = 20
        Me.HeaderFrame.Text = ""
        '
        'PMUHeaderLabel
        '
        Me.PMUHeaderLabel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.PMUHeaderLabel.Location = New System.Drawing.Point(333, 384)
        Me.PMUHeaderLabel.Name = "PMUHeaderLabel"
        Me.PMUHeaderLabel.Size = New System.Drawing.Size(136, 23)
        Me.PMUHeaderLabel.TabIndex = 19
        Me.PMUHeaderLabel.Text = "PMU &Header:"
        '
        'DataFrameLabel
        '
        Me.DataFrameLabel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.DataFrameLabel.Location = New System.Drawing.Point(8, 384)
        Me.DataFrameLabel.Name = "DataFrameLabel"
        Me.DataFrameLabel.Size = New System.Drawing.Size(136, 23)
        Me.DataFrameLabel.TabIndex = 17
        Me.DataFrameLabel.Text = "Data Frame:"
        '
        'CopyrightLabel
        '
        Me.CopyrightLabel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.CopyrightLabel.Font = New System.Drawing.Font("Verdana", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CopyrightLabel.Location = New System.Drawing.Point(8, 503)
        Me.CopyrightLabel.Name = "CopyrightLabel"
        Me.CopyrightLabel.Size = New System.Drawing.Size(336, 16)
        Me.CopyrightLabel.TabIndex = 21
        Me.CopyrightLabel.Text = "Copyright © 2005, TVA  Version 1.0.0.1, James R Carroll"
        Me.CopyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'TriggerGroupBox
        '
        Me.TriggerGroupBox.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TriggerGroupBox.Controls.Add(Me.CustomTrigger)
        Me.TriggerGroupBox.Controls.Add(Me.RateTrigger)
        Me.TriggerGroupBox.Controls.Add(Me.UnderVoltageTrigger)
        Me.TriggerGroupBox.Controls.Add(Me.OverCurrentTrigger)
        Me.TriggerGroupBox.Controls.Add(Me.AngleTrigger)
        Me.TriggerGroupBox.Controls.Add(Me.DfDtTrigger)
        Me.TriggerGroupBox.Controls.Add(Me.FrequencyTrigger)
        Me.TriggerGroupBox.Location = New System.Drawing.Point(554, 58)
        Me.TriggerGroupBox.Name = "TriggerGroupBox"
        Me.TriggerGroupBox.Size = New System.Drawing.Size(120, 190)
        Me.TriggerGroupBox.TabIndex = 15
        Me.TriggerGroupBox.TabStop = False
        Me.TriggerGroupBox.Text = "Triggers:"
        '
        'CustomTrigger
        '
        Me.CustomTrigger.Location = New System.Drawing.Point(8, 160)
        Me.CustomTrigger.Name = "CustomTrigger"
        Me.CustomTrigger.TabIndex = 6
        Me.CustomTrigger.TabStop = False
        Me.CustomTrigger.Text = "Custom"
        '
        'RateTrigger
        '
        Me.RateTrigger.Location = New System.Drawing.Point(8, 136)
        Me.RateTrigger.Name = "RateTrigger"
        Me.RateTrigger.TabIndex = 5
        Me.RateTrigger.TabStop = False
        Me.RateTrigger.Text = "Rate"
        '
        'UnderVoltageTrigger
        '
        Me.UnderVoltageTrigger.Location = New System.Drawing.Point(8, 112)
        Me.UnderVoltageTrigger.Name = "UnderVoltageTrigger"
        Me.UnderVoltageTrigger.TabIndex = 4
        Me.UnderVoltageTrigger.TabStop = False
        Me.UnderVoltageTrigger.Text = "Under Voltage"
        '
        'OverCurrentTrigger
        '
        Me.OverCurrentTrigger.Location = New System.Drawing.Point(8, 88)
        Me.OverCurrentTrigger.Name = "OverCurrentTrigger"
        Me.OverCurrentTrigger.TabIndex = 3
        Me.OverCurrentTrigger.TabStop = False
        Me.OverCurrentTrigger.Text = "Over Current"
        '
        'AngleTrigger
        '
        Me.AngleTrigger.Location = New System.Drawing.Point(8, 64)
        Me.AngleTrigger.Name = "AngleTrigger"
        Me.AngleTrigger.TabIndex = 2
        Me.AngleTrigger.TabStop = False
        Me.AngleTrigger.Text = "Angle"
        '
        'DfDtTrigger
        '
        Me.DfDtTrigger.Location = New System.Drawing.Point(8, 40)
        Me.DfDtTrigger.Name = "DfDtTrigger"
        Me.DfDtTrigger.TabIndex = 1
        Me.DfDtTrigger.TabStop = False
        Me.DfDtTrigger.Text = "df/dt"
        '
        'FrequencyTrigger
        '
        Me.FrequencyTrigger.Location = New System.Drawing.Point(8, 16)
        Me.FrequencyTrigger.Name = "FrequencyTrigger"
        Me.FrequencyTrigger.TabIndex = 0
        Me.FrequencyTrigger.TabStop = False
        Me.FrequencyTrigger.Text = "Frequency"
        '
        'FrameProcessor
        '
        Me.FrameProcessor.AutoReset = False
        Me.FrameProcessor.Interval = 1
        Me.FrameProcessor.SynchronizingObject = Me
        '
        'GraphFrameLagLabel
        '
        Me.GraphFrameLagLabel.Location = New System.Drawing.Point(352, 40)
        Me.GraphFrameLagLabel.Name = "GraphFrameLagLabel"
        Me.GraphFrameLagLabel.Size = New System.Drawing.Size(96, 16)
        Me.GraphFrameLagLabel.TabIndex = 13
        Me.GraphFrameLagLabel.Text = "Graph frame lag:"
        Me.GraphFrameLagLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'GraphFrameLag
        '
        Me.GraphFrameLag.Location = New System.Drawing.Point(448, 40)
        Me.GraphFrameLag.Name = "GraphFrameLag"
        Me.GraphFrameLag.Size = New System.Drawing.Size(64, 16)
        Me.GraphFrameLag.TabIndex = 14
        Me.GraphFrameLag.Text = "0"
        Me.GraphFrameLag.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'PMUPort
        '
        Me.PMUPort.Location = New System.Drawing.Point(440, 8)
        Me.PMUPort.Name = "PMUPort"
        Me.PMUPort.Size = New System.Drawing.Size(48, 20)
        Me.PMUPort.TabIndex = 5
        Me.PMUPort.Text = "4711"
        '
        'PMUIP
        '
        Me.PMUIP.Location = New System.Drawing.Point(88, 8)
        Me.PMUIP.Name = "PMUIP"
        Me.PMUIP.Size = New System.Drawing.Size(104, 21)
        Me.PMUIP.TabIndex = 1
        '
        'PMUID
        '
        Me.PMUID.Location = New System.Drawing.Point(224, 8)
        Me.PMUID.Name = "PMUID"
        Me.PMUID.Size = New System.Drawing.Size(128, 21)
        Me.PMUID.Sorted = True
        Me.PMUID.TabIndex = 3
        '
        'PhasorSelectionGroupBox
        '
        Me.PhasorSelectionGroupBox.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PhasorSelectionGroupBox.Controls.Add(Me.CurrentPhasor)
        Me.PhasorSelectionGroupBox.Controls.Add(Me.CurrentPhasorLabel)
        Me.PhasorSelectionGroupBox.Controls.Add(Me.VoltagePhasor)
        Me.PhasorSelectionGroupBox.Controls.Add(Me.VoltagePhasorLabel)
        Me.PhasorSelectionGroupBox.Location = New System.Drawing.Point(552, 256)
        Me.PhasorSelectionGroupBox.Name = "PhasorSelectionGroupBox"
        Me.PhasorSelectionGroupBox.Size = New System.Drawing.Size(120, 120)
        Me.PhasorSelectionGroupBox.TabIndex = 7
        Me.PhasorSelectionGroupBox.TabStop = False
        Me.PhasorSelectionGroupBox.Text = "Phasor Selection:"
        '
        'CurrentPhasor
        '
        Me.CurrentPhasor.Location = New System.Drawing.Point(8, 88)
        Me.CurrentPhasor.Name = "CurrentPhasor"
        Me.CurrentPhasor.Size = New System.Drawing.Size(104, 21)
        Me.CurrentPhasor.Sorted = True
        Me.CurrentPhasor.TabIndex = 3
        '
        'CurrentPhasorLabel
        '
        Me.CurrentPhasorLabel.Location = New System.Drawing.Point(8, 72)
        Me.CurrentPhasorLabel.Name = "CurrentPhasorLabel"
        Me.CurrentPhasorLabel.Size = New System.Drawing.Size(88, 16)
        Me.CurrentPhasorLabel.TabIndex = 2
        Me.CurrentPhasorLabel.Text = "&Current Phasor:"
        '
        'VoltagePhasor
        '
        Me.VoltagePhasor.Location = New System.Drawing.Point(8, 40)
        Me.VoltagePhasor.Name = "VoltagePhasor"
        Me.VoltagePhasor.Size = New System.Drawing.Size(104, 21)
        Me.VoltagePhasor.Sorted = True
        Me.VoltagePhasor.TabIndex = 1
        '
        'VoltagePhasorLabel
        '
        Me.VoltagePhasorLabel.Location = New System.Drawing.Point(8, 24)
        Me.VoltagePhasorLabel.Name = "VoltagePhasorLabel"
        Me.VoltagePhasorLabel.Size = New System.Drawing.Size(88, 16)
        Me.VoltagePhasorLabel.TabIndex = 0
        Me.VoltagePhasorLabel.Text = "&Voltage Phasor:"
        '
        'IEEE1344Listener
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(682, 517)
        Me.Controls.Add(Me.PhasorSelectionGroupBox)
        Me.Controls.Add(Me.PMUID)
        Me.Controls.Add(Me.PMUIP)
        Me.Controls.Add(Me.PMUPort)
        Me.Controls.Add(Me.HeaderFrame)
        Me.Controls.Add(Me.SampleCount)
        Me.Controls.Add(Me.GraphFrameLag)
        Me.Controls.Add(Me.GraphFrameLagLabel)
        Me.Controls.Add(Me.TriggerGroupBox)
        Me.Controls.Add(Me.CopyrightLabel)
        Me.Controls.Add(Me.DataFrame)
        Me.Controls.Add(Me.DataFrameLabel)
        Me.Controls.Add(Me.DataChart)
        Me.Controls.Add(Me.IDLabel)
        Me.Controls.Add(Me.ConnectToIPLabel)
        Me.Controls.Add(Me.Listen)
        Me.Controls.Add(Me.ListenOnPortLabel)
        Me.Controls.Add(Me.SamplesPerSecLabel)
        Me.Controls.Add(Me.SampleRate)
        Me.Controls.Add(Me.SampleRateLabel)
        Me.Controls.Add(Me.SamplesLabel)
        Me.Controls.Add(Me.PMUHeaderLabel)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(690, 544)
        Me.Name = "IEEE1344Listener"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "IEEE Std 1344-1995 PMU Connection Test and Sample Rate Calculator"
        CType(Me.DataChart, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TriggerGroupBox.ResumeLayout(False)
        CType(Me.FrameProcessor, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PhasorSelectionGroupBox.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub IEEE1344Listener_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Variables.Create("PMU.IP", "152.86.30.15", VariableType.Text)
        Variables.Create("PMU.ID", 1, VariableType.Int)
        Variables.Create("PMU.Port", 4711, VariableType.Int)
        Variables.Create("PMU.MRU.IP.Count", 0, VariableType.Int)
        Variables.Create("PMU.MRU.ID.Count", 0, VariableType.Int)

        PMUIP.Text = Variables("PMU.IP")
        PMUID.Text = Variables("PMU.ID")
        PMUPort.Text = Variables("PMU.Port")

        With FileVersionInfo.GetVersionInfo(Reflection.Assembly.GetExecutingAssembly.Location)
            CopyrightLabel.Text = "Copyright © 2005, TVA - Version " & .FileMajorPart & "." & .FileMinorPart & "." & .FileBuildPart & ",  James R Carroll"
        End With

        RestoreWindowLocation(Me)

        ' Initialize chart title states
        With DataChart
            m_frequencyTitle = .Titles(1).Text
            m_phasorTitle = .Titles(2).Text
            m_initializingTitle = .Titles(5)
            .Titles.RemoveAt(5)
        End With

        Variables.Save()

        m_frameQueue = New ArrayList
        m_frequencyRange = New DataPoints

        LoadMRUList(PMUIP, "PMU.MRU.IP")
        LoadMRUList(PMUID, "PMU.MRU.ID")

        ' We auto-launch with specified command line parameters, if provided
        If Len(VB.Command) > 0 Then
            With VB.Command.Split("/"c)
                If .Length >= 3 Then
                    PMUIP.Text = .GetValue(0)
                    PMUID.Text = .GetValue(1)
                    PMUPort.Text = .GetValue(2)
                    Listen_Click(Me, Nothing)
                End If
            End With
        End If

    End Sub

    Private Sub LoadMRUList(ByVal comboBox As System.Windows.Forms.ComboBox, ByVal setting As String)

        With comboBox
            For x As Integer = 0 To Variables(setting & ".Count") - 1
                .Items.Add(Variables(setting & ".Item" & x))
            Next

            If Not .Items.Contains(.Text) Then .Items.Add(.Text)
        End With

    End Sub

    Private Sub IEEE1344Listener_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing

        If Not m_parser Is Nothing Then
            m_parser.DisableRealTimeData()
            m_parser.Disconnect()
        End If

        SaveWindowLocation(Me)

        Variables("PMU.IP") = PMUIP.Text
        Variables("PMU.ID") = PMUID.Text
        Variables("PMU.Port") = PMUPort.Text

        SaveMRUList(PMUIP, "PMU.MRU.IP")
        SaveMRUList(PMUID, "PMU.MRU.ID")

    End Sub

    Private Sub SaveMRUList(ByVal comboBox As System.Windows.Forms.ComboBox, ByVal setting As String)

        Variables(setting & ".Count") = comboBox.Items.Count

        For x As Integer = 0 To Variables(setting & ".Count") - 1
            Variables(setting & ".Item" & x) = comboBox.Items(x)
        Next

    End Sub

    Private Sub Listen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Listen.Click

        If m_parser Is Nothing Then
            ' Start frame parser
            Try
                Listen.Text = "&Stop"
                m_parser = New IEEE1344.FrameParser(PMUID.Text, PMUIP.Text, PMUPort.Text, IEEE1344.PhasorFormat.Rectangular)

                m_parser.Connect()
                m_parser.EnableRealTimeData()
                m_parser.RetrieveHeaderFile()

                ' If we successfully connect to a PMU, we'll add the IP and ID to the pick lists for future reference
                With PMUIP
                    If Not .Items.Contains(.Text) Then .Items.Add(.Text)
                End With

                With PMUID
                    If Not .Items.Contains(.Text) Then .Items.Add(.Text)
                End With
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.OKOnly Or MsgBoxStyle.Exclamation)
                Listen.Text = "&Listen"
                m_parser = Nothing
            End Try
        Else
            ' Stop frame parser and reinitialize
            Try
                If Not m_parser Is Nothing Then
                    m_parser.DisableRealTimeData()
                    m_parser.Disconnect()
                End If

                With DataChart
                    .Titles(0).Text = "PMU ID"
                    .Titles(1).Text = m_frequencyTitle
                    .Titles(2).Text = m_phasorTitle
                    .Titles(3).Text = "Vars: unavailable"
                    .Titles(4).Text = "Power: unavailable"

                    ' Clear all existing data
                    For x As Integer = .Series.Count - 1 To 0 Step -1
                        If x > 1 Then
                            .Series.RemoveAt(x)
                        Else
                            .Series(x).Points.Clear()
                        End If
                    Next
                End With

                CurrentPhasor.Text = ""
                CurrentPhasor.Items.Clear()

                VoltagePhasor.Text = ""
                VoltagePhasor.Items.Clear()

                m_frameQueue.Clear()
                m_frequencyRange.Clear()
                m_firstFrame = 0
                m_frameCount = 0
                m_startTime = 0
                m_total = 0

                FrameProcessor.Enabled = False

                DataFrame.Text = ""
                HeaderFrame.Text = ""
                SampleCount.Text = "0"
                SampleRate.Text = "0"
                GraphFrameLag.Text = "0"

                FrequencyTrigger.Checked = False
                DfDtTrigger.Checked = False
                AngleTrigger.Checked = False
                OverCurrentTrigger.Checked = False
                UnderVoltageTrigger.Checked = False
                RateTrigger.Checked = False
                CustomTrigger.Checked = False
            Catch ex As Exception
                LogException(ex.Message)
            Finally
                Listen.Text = "&Listen"
                m_parser = Nothing
            End Try
        End If

    End Sub

    Private Sub parser_DataStreamException(ByVal ex As System.Exception) Handles m_parser.DataStreamException

        LogException("Data Stream Exception: " & ex.Message)

    End Sub

    Private Sub parser_ReceivedConfigFile2(ByVal configFile As IEEE1344.ConfigurationFile) Handles m_parser.ReceivedConfigFile2

        m_configFile = configFile

        ' Configure chart series
        With DataChart
            .Titles(0).Text = configFile.StationName & _
                " - Nominal Frequency " & IIf(configFile.LineFrequency = IEEE1344.PMULineFrequency._50Hz, "50Hz", "60Hz") & _
                ", " & configFile.Period / 100 & " cycles / data transmission"

            If Not m_initializingTitle Is Nothing Then .Titles.Add(m_initializingTitle)

            For x As Integer = 0 To configFile.PhasorDefinitions.Count - 1
                .Series.Add("Phasors" & x)
                With .Series("Phasors" & x)
                    .ChartArea = "Phasors"
                    .ChartType = "Spline"
                    .BorderWidth = 2
                    .Legend = "PhasorLegend"
                    .LegendText = configFile.PhasorDefinitions(x).Label
                End With

                ' Load voltage and current phasors into appropriate picklists used for power/var calculations
                Select Case configFile.PhasorDefinitions(x).Type
                    Case IEEE1344.PhasorType.Voltage
                        VoltagePhasor.Items.Add(configFile.PhasorDefinitions(x).Label)
                    Case IEEE1344.PhasorType.Current
                        CurrentPhasor.Items.Add(configFile.PhasorDefinitions(x).Label)
                End Select
            Next

            If VoltagePhasor.SelectedIndex = -1 And VoltagePhasor.Items.Count > 0 Then VoltagePhasor.SelectedIndex = 0
            If CurrentPhasor.SelectedIndex = -1 And CurrentPhasor.Items.Count > 0 Then CurrentPhasor.SelectedIndex = 0
        End With

    End Sub

    Private Sub parser_ReceivedDataFrame(ByVal frame As IEEE1344.DataFrame) Handles m_parser.ReceivedDataFrame

        If m_startTime = 0 Then m_startTime = DateTime.Now.Ticks

        ' We queue up this data frame for post processing (i.e., graphing etc.) so that we don't slow down the real-time data stream
        SyncLock m_frameQueue.SyncRoot
            m_frameQueue.Add(frame)
        End SyncLock

        FrameProcessor.Enabled = True

        ' We calculate sample rate here to get an accurate sample rate
        m_total += 1

        SampleCount.Text = m_total.ToString

        If m_total Mod 30 = 0 Then
            SampleRate.Text = (m_total / ((DateTime.Now.Ticks - m_startTime) / 10000000L)).ToString("0.0000")

            SyncLock m_frameQueue.SyncRoot
                GraphFrameLag.Text = m_frameQueue.Count
            End SyncLock
        End If

    End Sub

    Private Sub parser_ReceivedHeaderFile(ByVal headerFile As IEEE1344.HeaderFile) Handles m_parser.ReceivedHeaderFile

        HeaderFrame.Text = headerFile.Data

    End Sub

    Private Sub parser_ReceivedUnknownFrame(ByVal frame As IEEE1344.BaseFrame) Handles m_parser.ReceivedUnknownFrame

        LogException("Received unknown frame type: " & frame.FrameLength & " bytes")

    End Sub

    Private Sub LogException(ByVal errorMessage As String)

        Try
            With File.AppendText(AddPathSuffix(Application.StartupPath) & TextErrorListener.FileName)
                .WriteLine(errorMessage)
                .Close()
            End With
        Catch
        End Try

    End Sub

    Private Sub FrameProcessor_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles FrameProcessor.Elapsed

        Dim frame As IEEE1344.DataFrame

        SyncLock m_frameQueue.SyncRoot
            If m_frameQueue.Count > 0 Then
                frame = m_frameQueue(0)
                m_frameQueue.RemoveAt(0)
            End If
        End SyncLock

        If Not frame Is Nothing Then
            Dim image As Byte() = frame.BinaryImage()
            Dim maxTicks, minTicks As Long
            Dim frequency As Double
            Dim x, phasors As Integer

            If m_firstFrame = 0 Then m_firstFrame = frame.Timestamp.Ticks
            maxTicks = (frame.Timestamp.Ticks - m_firstFrame) / 10000L
            m_frameCount += 1

            With DataChart
                With .Series("Frequency")
                    frequency = frame.Frequency
                    m_frequencyRange.Add(frequency)
                    .Points.AddXY(maxTicks, frequency)
                    Do While .Points.Count > FrequencyPoints
                        m_frequencyRange.Remove(.Points(0).YValues(0))
                        .Points.RemoveAt(0)
                        minTicks = .Points(0).XValue
                    Loop
                End With

                If (m_frameCount - 1) Mod 120 = 0 Then
                    Dim minPhasorTicks As Long

                    phasors = frame.PhasorValues.Count

                    For x = 0 To phasors - 1
                        With .Series("Phasors" & x)
                            .Points.AddXY(maxTicks, frame.PhasorValues(x).Angle)

                            Do While .Points.Count > PhasorPoints
                                .Points.RemoveAt(0)
                            Loop

                            If x = 0 Then minPhasorTicks = .Points(0).XValue
                        End With
                    Next

                    .Titles(2).Text = m_phasorTitle & " / " & (maxTicks - minPhasorTicks + 1) \ 1000L & "s"

                    ' Calculate and display power and vars using selected phasors
                    If CurrentPhasor.SelectedIndex > -1 And VoltagePhasor.SelectedIndex > -1 Then
                        Dim voltage As IEEE1344.PhasorValue = frame.PhasorValues(VoltagePhasor.SelectedIndex)
                        Dim current As IEEE1344.PhasorValue = frame.PhasorValues(CurrentPhasor.SelectedIndex)
                        .Titles(3).Text = "Vars: " & (IEEE1344.PhasorValue.CalculateVars(voltage, current) / 1000000).ToString("0.0") & " MVars"
                        .Titles(4).Text = "Power: " & (IEEE1344.PhasorValue.CalculatePower(voltage, current) / 1000000).ToString("0.0") & " MW"
                    Else
                        .Titles(3).Text = "Vars: unavailable"
                        .Titles(4).Text = "Power: unavailable"
                    End If

                    For x = 0 To phasors - 1
                        With .Series("Phasors" & x)
                            .LegendText = _
                                VB.Left(m_configFile.PhasorDefinitions(x).Label, 12).PadRight(13) & _
                                IIf(m_configFile.PhasorDefinitions(x).Type = IEEE1344.PhasorType.Voltage, _
                                    Convert.ToInt32(frame.PhasorValues(x).Magnitude / 1000) & "KV", _
                                    Convert.ToInt32(frame.PhasorValues(x).Magnitude) & "A")
                        End With
                    Next

                    With .ChartAreas("Phasors")
                        .AxisX.Maximum = maxTicks
                        .AxisX.Minimum = minPhasorTicks
                        .AxisY2.Minimum = -180
                        .AxisY2.Maximum = 360
                    End With
                End If

                If minTicks > 0 And m_frameCount Mod 4 = 0 Then
                    ' Remove "Initializing" title...                    
                    If .Titles.Count > 5 Then .Titles.RemoveAt(5)
                    .Titles(1).Text = m_frequencyTitle & " / " & ((maxTicks - minTicks + 1) / 1000L).ToString("0.0") & "s"

                    With .ChartAreas("Frequency")
                        .AxisX.Maximum = maxTicks
                        .AxisX.Minimum = minTicks
                        .AxisY.Minimum = m_frequencyRange.Minimum(-0.0001)
                        .AxisY.Maximum = m_frequencyRange.Maximum(0.0001)
                    End With

                    .Invalidate()
                End If
            End With

            With New StringBuilder
                .Append(frame.Timestamp.ToString("dd-MMM-yyyy HH:mm:ss.fff - "))
                .Append(frequency.ToString("#.0000"))
                .Append(" Hz")
                .Append(vbCrLf)
                .Append(vbCrLf)

                For i As Integer = 0 To image.Length - 1
                    .Append(Hex(image(i)).PadLeft(2, "0"c))
                    .Append(" "c)
                Next

                DataFrame.Text = .ToString
            End With

            ' Set triggers if any are fired
            Select Case frame.TriggerStatus
                Case IEEE1344.PMUTriggerStatus.FrequencyTrigger
                    FrequencyTrigger.Checked = True
                Case IEEE1344.PMUTriggerStatus.DfDtTrigger
                    DfDtTrigger.Checked = True
                Case IEEE1344.PMUTriggerStatus.AngleTrigger
                    AngleTrigger.Checked = True
                Case IEEE1344.PMUTriggerStatus.OverCurrentTrigger
                    OverCurrentTrigger.Checked = True
                Case IEEE1344.PMUTriggerStatus.UnderVoltageTrigger
                    UnderVoltageTrigger.Checked = True
                Case IEEE1344.PMUTriggerStatus.RateTrigger
                    RateTrigger.Checked = True
                Case IEEE1344.PMUTriggerStatus.UserDefined
                    CustomTrigger.Checked = True
            End Select
        End If

        SyncLock m_frameQueue.SyncRoot
            FrameProcessor.Enabled = (m_frameQueue.Count > 0)
        End SyncLock

    End Sub

    Private Sub PMUPort_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles PMUPort.GotFocus

        With PMUPort
            .SelectionStart = 0
            .SelectionLength = Len(.Text)
        End With

    End Sub

    Private Sub CurrentPhasor_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CurrentPhasor.SelectedIndexChanged

        DataChart.Titles(3).Text = "Vars: calculating"
        DataChart.Titles(4).Text = "Power: calculating"

    End Sub

    Private Sub VoltagePhasor_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles VoltagePhasor.SelectedIndexChanged

        DataChart.Titles(3).Text = "Vars: calculating"
        DataChart.Titles(4).Text = "Power: calculating"

    End Sub

End Class
