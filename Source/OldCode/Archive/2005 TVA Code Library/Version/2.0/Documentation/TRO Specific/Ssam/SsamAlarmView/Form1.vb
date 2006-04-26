Public Class Form1
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
	Friend WithEvents Schedule1 As Janus.Windows.Schedule.Schedule
	Friend WithEvents SqlConnection1 As System.Data.SqlClient.SqlConnection
	Friend WithEvents SqlDataAdapter1 As System.Data.SqlClient.SqlDataAdapter
	Friend WithEvents DsAlarm1 As SsamAlarmView.dsAlarm
	Friend WithEvents SqlSelectCommand1 As System.Data.SqlClient.SqlCommand
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
		Dim ScheduleLayout1 As Janus.Windows.Schedule.ScheduleLayout = New Janus.Windows.Schedule.ScheduleLayout()
		Me.DsAlarm1 = New SsamAlarmView.dsAlarm()
		Me.Schedule1 = New Janus.Windows.Schedule.Schedule()
		Me.SqlConnection1 = New System.Data.SqlClient.SqlConnection()
		Me.SqlDataAdapter1 = New System.Data.SqlClient.SqlDataAdapter()
		Me.SqlSelectCommand1 = New System.Data.SqlClient.SqlCommand()
		CType(Me.DsAlarm1, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.Schedule1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'DsAlarm1
		'
		Me.DsAlarm1.DataSetName = "dsAlarm"
		Me.DsAlarm1.Locale = New System.Globalization.CultureInfo("en-US")
		Me.DsAlarm1.Namespace = "http://www.tempuri.org/dsAlarm.xsd"
		'
		'Schedule1
		'
		Me.Schedule1.DataMember = "Alarm"
		Me.Schedule1.DataSource = Me.DsAlarm1
		Me.Schedule1.Date = New Date(2003, 6, 2, 0, 0, 0, 0)
		Me.Schedule1.Dates.Add(New Date(2003, 6, 2, 0, 0, 0, 0))
		Me.Schedule1.Dates.Add(New Date(2003, 6, 3, 0, 0, 0, 0))
		Me.Schedule1.Dates.Add(New Date(2003, 6, 4, 0, 0, 0, 0))
		Me.Schedule1.Dates.Add(New Date(2003, 6, 5, 0, 0, 0, 0))
		Me.Schedule1.Dates.Add(New Date(2003, 6, 6, 0, 0, 0, 0))
		Me.Schedule1.Dates.Add(New Date(2003, 6, 7, 0, 0, 0, 0))
		Me.Schedule1.Dates.Add(New Date(2003, 6, 8, 0, 0, 0, 0))
		Me.Schedule1.DayColumns = 7
		Me.Schedule1.DayNavigationButtons = True
		ScheduleLayout1.DataMember = "Alarm"
		ScheduleLayout1.DataSource = Me.DsAlarm1
		ScheduleLayout1.LayoutString = "<ScheduleLayoutData><Fields Collection=""true""><Field0 ID=""ID""><DataMember>ID</Dat" & _
		"aMember><Key>ID</Key><TypeCode>Int32</TypeCode></Field0><Field1 ID=""FlowName""><D" & _
		"ataMember>FlowName</DataMember><Key>FlowName</Key></Field1><Field2 ID=""startDT"">" & _
		"<DataMember>startDT</DataMember><Key>startDT</Key><TypeCode>DateTime</TypeCode><" & _
		"/Field2><Field3 ID=""endDT""><DataMember>endDT</DataMember><Key>endDT</Key><TypeCo" & _
		"de>DateTime</TypeCode></Field3><Field4 ID=""Descr""><DataMember>Descr</DataMember>" & _
		"<Key>Descr</Key></Field4><Field5 ID=""Expr1""><DataMember>Expr1</DataMember><Key>E" & _
		"xpr1</Key><TypeCode>Int32</TypeCode></Field5></Fields><StartTimeMember>startDT</" & _
		"StartTimeMember><EndTimeMember>endDT</EndTimeMember><TextMember>Descr</TextMembe" & _
		"r></ScheduleLayoutData>"
		Me.Schedule1.DesignTimeLayout = ScheduleLayout1
		Me.Schedule1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.Schedule1.EndTimeMember = "endDT"
		Me.Schedule1.FirstDayOfWeek = Janus.Windows.Schedule.ScheduleDayOfWeek.Monday
		Me.Schedule1.MaxAllDayRows = -1
		Me.Schedule1.Name = "Schedule1"
		Me.Schedule1.Size = New System.Drawing.Size(784, 550)
		Me.Schedule1.StartTimeMember = "startDT"
		Me.Schedule1.TabIndex = 0
		Me.Schedule1.TextMember = "Descr"
		Me.Schedule1.TimeFormat = Janus.Windows.Schedule.TimeFormat.TwentyFourHours
		Me.Schedule1.View = Janus.Windows.Schedule.ScheduleView.WorkWeek
		Me.Schedule1.WorkWeek = ((((((Janus.Windows.Schedule.ScheduleDayOfWeek.Sunday Or Janus.Windows.Schedule.ScheduleDayOfWeek.Monday) _
					Or Janus.Windows.Schedule.ScheduleDayOfWeek.Tuesday) _
					Or Janus.Windows.Schedule.ScheduleDayOfWeek.Wednesday) _
					Or Janus.Windows.Schedule.ScheduleDayOfWeek.Thursday) _
					Or Janus.Windows.Schedule.ScheduleDayOfWeek.Friday) _
					Or Janus.Windows.Schedule.ScheduleDayOfWeek.Saturday)
		'
		'SqlConnection1
		'
		Me.SqlConnection1.ConnectionString = "data source=OPDATSQL;initial catalog=SSAM;persist security info=True;user id=ESOP" & _
		"ublic;password=4all2see;workstation id=5W42P01;packet size=4096"
		'
		'SqlDataAdapter1
		'
		Me.SqlDataAdapter1.SelectCommand = Me.SqlSelectCommand1
		Me.SqlDataAdapter1.TableMappings.AddRange(New System.Data.Common.DataTableMapping() {New System.Data.Common.DataTableMapping("Table", "Alarm", New System.Data.Common.DataColumnMapping() {New System.Data.Common.DataColumnMapping("ID", "ID"), New System.Data.Common.DataColumnMapping("FlowName", "FlowName"), New System.Data.Common.DataColumnMapping("startDT", "startDT"), New System.Data.Common.DataColumnMapping("endDT", "endDT"), New System.Data.Common.DataColumnMapping("Descr", "Descr")})})
		'
		'SqlSelectCommand1
		'
		Me.SqlSelectCommand1.CommandText = "SELECT dbo.MonitoredObject.ID AS ID, dbo.MonitoredObject.keyFieldValue AS FlowNam" & _
		"e, dbo.Alarm.utcDatetime AS startDT, dbo.Alarm.closedGmtDatetime AS endDT, dbo.M" & _
		"onitoredObject.keyFieldValue + ': ' + LTRIM(STR(DATEDIFF(s, dbo.Alarm.utcDatetim" & _
		"e, dbo.Alarm.closedGmtDatetime))) + ' sec downtime' AS Descr, dbo.Alarm.ID AS Ex" & _
		"pr1 FROM dbo.Alarm INNER JOIN dbo.MonitoredObject ON dbo.Alarm.entityID = dbo.Mo" & _
		"nitoredObject.ID WHERE (dbo.MonitoredObject.entityType = 1)"
		Me.SqlSelectCommand1.Connection = Me.SqlConnection1
		'
		'Form1
		'
		Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
		Me.ClientSize = New System.Drawing.Size(784, 550)
		Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.Schedule1})
		Me.Name = "Form1"
		Me.Text = "SSAM Alarms"
		CType(Me.DsAlarm1, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.Schedule1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)

	End Sub

#End Region

	Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Schedule1.DayColumns = 7		  '7-day week display
		Schedule1.Date = DateAdd(DateInterval.Day, -7, Now())

		SqlDataAdapter1.Fill(DsAlarm1, "Alarm")
	End Sub
End Class
