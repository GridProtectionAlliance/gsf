Imports System.Text
Imports TVA.Text.Common
Imports TVA.DateTime

Namespace Scheduling

    Public Class Schedule

#Region " Member declaration "

        Private m_name As String
        Private m_description As String
        Private m_minutePart As SchedulePart
        Private m_hourPart As SchedulePart
        Private m_dayPart As SchedulePart
        Private m_monthPart As SchedulePart
        Private m_dayOfWeekPart As SchedulePart
        Private m_lastDueAt As System.DateTime

#End Region

#Region " Code Scope: Public "

        Public Sub New(ByVal name As String)

            MyClass.New(name, "* * * * *")

        End Sub

        Public Sub New(ByVal name As String, ByVal rule As String)

            MyClass.New(name, rule, "")

        End Sub

        Public Sub New(ByVal name As String, ByVal rule As String, ByVal description As String)

            MyBase.New()
            Me.Name = name
            Me.Rule = rule
            Me.Description = description

        End Sub

        ''' <summary>
        ''' Gets or sets the schedule name.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The schedule name.</returns>
        Public Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_name = value
                Else
                    Throw New ArgumentNullException("Name")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the schedule rule.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The schedule rule.</returns>
        Public Property Rule() As String
            Get
                Return m_minutePart.Text & " " & m_hourPart.Text & " " & m_dayPart.Text & " " & m_monthPart.Text & " " & m_dayOfWeekPart.Text
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    With RemoveDuplicateWhiteSpace(value).Split(" "c)
                        If .Length = 5 Then
                            m_minutePart = New SchedulePart(.GetValue(0).ToString(), DateTimePart.Minute)
                            m_hourPart = New SchedulePart(.GetValue(1).ToString(), DateTimePart.Hour)
                            m_dayPart = New SchedulePart(.GetValue(2).ToString(), DateTimePart.Day)
                            m_monthPart = New SchedulePart(.GetValue(3).ToString(), DateTimePart.Month)
                            m_dayOfWeekPart = New SchedulePart(.GetValue(4).ToString(), DateTimePart.DayOfWeek)

                            ' Update the schedule description.
                            With New StringBuilder()
                                .Append(m_minutePart.Description)
                                .Append(", ")
                                .Append(m_hourPart.Description)
                                .Append(", ")
                                .Append(m_dayPart.Description)
                                .Append(", ")
                                .Append(m_monthPart.Description)
                                .Append(", ")
                                .Append(m_dayOfWeekPart.Description)

                                m_description = .ToString()
                            End With
                        Else
                            Throw New ArgumentException("Schedule rule must have exactly 5 parts (Example: * * * * *).")
                        End If
                    End With
                Else
                    Throw New ArgumentNullException("Rule")
                End If
            End Set
        End Property

        Public Property Description() As String
            Get
                Return m_description
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_description = value
                End If
            End Set
        End Property

        Public ReadOnly Property LastDueAt() As System.DateTime
            Get
                Return m_lastDueAt
            End Get
        End Property

        Public ReadOnly Property MinutePart() As SchedulePart
            Get
                Return m_minutePart
            End Get
        End Property

        Public ReadOnly Property HourPart() As SchedulePart
            Get
                Return m_hourPart
            End Get
        End Property

        Public ReadOnly Property DayPart() As SchedulePart
            Get
                Return m_dayPart
            End Get
        End Property

        Public ReadOnly Property MonthPart() As SchedulePart
            Get
                Return m_monthPart
            End Get
        End Property

        Public ReadOnly Property DaysOfWeekPart() As SchedulePart
            Get
                Return m_dayOfWeekPart
            End Get
        End Property

        ''' <summary>
        ''' Gets the current status of the schedule.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current status of the schedule.</returns>
        Public ReadOnly Property Status() As String
            Get
                With New System.Text.StringBuilder()
                    .Append("             Schedule name: ")
                    .Append(m_name)
                    .AppendLine()
                    .Append("             Schedule rule: ")
                    .Append(Rule)
                    .AppendLine()
                    .Append("             Last run time: ")
                    .Append(IIf(m_lastDueAt = System.DateTime.MinValue, "Never", m_lastDueAt))
                    .AppendLine()

                    Return .ToString()
                End With
            End Get
        End Property

        ''' <summary>
        ''' Checks whether the schedule is due at the present system time.
        ''' </summary>
        ''' <returns>True if the schedule is due at the present system time; otherwise False.</returns>
        Public Function IsDue() As Boolean

            Dim currentDateTime As System.DateTime = System.DateTime.Now
            If m_minutePart.Matches(currentDateTime) AndAlso m_hourPart.Matches(currentDateTime) AndAlso _
                    m_dayPart.Matches(currentDateTime) AndAlso m_monthPart.Matches(currentDateTime) AndAlso _
                    m_dayOfWeekPart.Matches(currentDateTime) Then
                m_lastDueAt = currentDateTime
                Return True
            Else
                Return False
            End If

        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Dim other As Schedule = TryCast(obj, Schedule)
            If other IsNot Nothing AndAlso other.Name = Me.Name AndAlso other.Rule = Me.Rule Then
                Return True
            Else
                Return False
            End If

        End Function

#End Region

    End Class

End Namespace