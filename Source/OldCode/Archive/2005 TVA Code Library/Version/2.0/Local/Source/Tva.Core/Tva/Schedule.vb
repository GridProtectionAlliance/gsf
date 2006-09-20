' 08-01-06

Option Strict On

Imports System.Text.RegularExpressions
Imports Tva.Text.Common

Public Class Schedule

#Region " Private Schedule Element Class "

    Private Class Element

        Private m_text As String
        Private m_elementType As ElementType
        Private m_values As List(Of Integer)

        Public Enum ElementType As Integer
            Minute
            Hour
            Day
            Month
            DayOfWeek
        End Enum

        Public Sub New(ByVal text As String, ByVal type As ElementType)
            If IsValidElement(text, type) Then
                m_text = text
                m_elementType = type
            Else
                Throw New ArgumentException()
            End If
        End Sub

        Public ReadOnly Property Text() As String
            Get
                Return m_text
            End Get
        End Property

        Public ReadOnly Property Values() As List(Of Integer)
            Get
                Return m_values
            End Get
        End Property

        Public Function Matches(ByVal dateAndTime As System.DateTime) As Boolean

            Select Case m_elementType
                Case ElementType.Minute
                    Return m_values.Contains(dateAndTime.Minute)
                Case ElementType.Hour
                    Return m_values.Contains(dateAndTime.Hour)
                Case ElementType.Day
                    Return m_values.Contains(dateAndTime.Day)
                Case ElementType.Month
                    Return m_values.Contains(dateAndTime.Month)
                Case ElementType.DayOfWeek
                    Return m_values.Contains(Convert.ToInt32(dateAndTime.DayOfWeek))
            End Select

        End Function

        Private Function IsValidElement(ByVal element As String, ByVal elementType As ElementType) As Boolean

            Dim minValue As Integer = 0
            Dim maxvalue As Integer = 0
            Select Case elementType
                Case Schedule.Element.ElementType.Minute
                    maxvalue = 59
                Case Schedule.Element.ElementType.Hour
                    maxvalue = 23
                Case Schedule.Element.ElementType.Day
                    minValue = 1
                    maxvalue = 31
                Case Schedule.Element.ElementType.Month
                    minValue = 1
                    maxvalue = 12
                Case Schedule.Element.ElementType.DayOfWeek
                    maxvalue = 6
            End Select

            m_values = New List(Of Integer)()
            If Regex.Match(element, "^(\*){1}$").Success Then
                ' ^(\*){1}$             Matches: *
                PopulateValues(minValue, maxvalue, 1)

                Return True
            ElseIf Regex.Match(element, "^(\*/\d+){1}$").Success Then
                ' ^(\*/\d+){1}$         Matches: */[any digit]
                Dim interval As Integer = Convert.ToInt32(element.Split("/"c)(1))
                If interval > 0 AndAlso interval >= minValue AndAlso interval <= maxvalue Then
                    PopulateValues(minValue, maxvalue, interval)
                    Return True
                End If

                Return False
            ElseIf Regex.Match(element, "^(\d+\-\d+){1}$").Success Then
                ' ^(\d+\-\d+){1}$       Matches: [any digit]-[any digit]
                Dim range As String() = element.Split("-"c)
                Dim lowRange As Integer = Convert.ToInt32(range(0))
                Dim highRange As Integer = Convert.ToInt32(range(1))
                If lowRange < highRange AndAlso lowRange >= minValue AndAlso highRange <= maxvalue Then
                    PopulateValues(lowRange, highRange, 1)
                    Return True
                End If

                Return False
            ElseIf Regex.Match(element, "^((\d+,?)+){1}$").Success Then
                ' ^((\d+,?)+){1}$       Matches: [any digit] AND [any digit], ..., [any digit]
                For Each value As Integer In element.Split(","c)
                    If Not (value >= minValue AndAlso value <= maxvalue) Then
                        Return False
                    Else
                        If Not m_values.Contains(value) Then m_values.Add(value)
                    End If
                Next

                Return True
            End If

        End Function

        Private Sub PopulateValues(ByVal fromValue As Integer, ByVal toValue As Integer, ByVal stepValue As Integer)

            For i As Integer = fromValue To toValue Step stepValue
                m_values.Add(i)
            Next

        End Sub

    End Class

#End Region

    Private m_name As String
    Private m_minutes As Element
    Private m_hours As Element
    Private m_days As Element
    Private m_months As Element
    Private m_dayOfWeek As Element
    Private m_lastRunDateTime As System.DateTime

    Public Sub New(ByVal name As String)
        MyClass.New(name, "*")
    End Sub

    Public Sub New(ByVal name As String, ByVal minutes As String)
        MyClass.New(name, minutes, "*")
    End Sub

    Public Sub New(ByVal name As String, ByVal minutes As String, ByVal hours As String)
        MyClass.New(name, minutes, hours, "*")
    End Sub

    Public Sub New(ByVal name As String, ByVal minutes As String, ByVal hours As String, ByVal days As String)
        MyClass.New(name, minutes, hours, days, "*")
    End Sub

    Public Sub New(ByVal name As String, ByVal minutes As String, ByVal hours As String, ByVal days As String, _
                ByVal months As String)
        MyClass.New(name, minutes, hours, days, months, "*")
    End Sub
    Public Sub New(ByVal name As String, ByVal minutes As String, ByVal hours As String, ByVal days As String, _
            ByVal months As String, ByVal daysOfWeek As String)
        MyBase.New()
        Me.Name = name
        Me.Rule = minutes & " " & hours & " " & days & " " & months & " " & daysOfWeek
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
                Throw New ArgumentNullException("value")
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
            Return m_minutes.Text & " " & m_hours.Text & " " & m_days.Text & " " & m_months.Text & " " & m_dayOfWeek.Text
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                With RemoveDuplicateWhiteSpace(value).Split(" "c)
                    If .Length = 5 Then
                        Me.Minutes = .GetValue(0).ToString()
                        Me.Hours = .GetValue(1).ToString()
                        Me.Days = .GetValue(2).ToString()
                        Me.Months = .GetValue(3).ToString()
                        Me.DaysOfWeek = .GetValue(4).ToString()
                    Else
                        Throw New ArgumentException("Schedule rule must have exactly 5 elements (Example: * * * * *).")
                    End If
                End With
            Else
                Throw New ArgumentNullException("value")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the 1st element of the 5 element schedule rule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The 1st element of the 5 element schedule rule.</returns>
    Public Property Minutes() As String
        Get
            Return m_minutes.Text
        End Get
        Set(ByVal value As String)
            m_minutes = New Element(RemoveWhiteSpace(value), Element.ElementType.Minute)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the 2nd element of the 5 element schedule rule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The 2nd element of the 5 element schedule rule.</returns>
    Public Property Hours() As String
        Get
            Return m_hours.Text
        End Get
        Set(ByVal value As String)
            m_hours = New Element(RemoveWhiteSpace(value), Element.ElementType.Hour)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the 3rd element of the 5 element schedule rule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The 3rd element of the 5 element schedule rule.</returns>
    Public Property Days() As String
        Get
            Return m_days.Text
        End Get
        Set(ByVal value As String)
            m_days = New Element(RemoveWhiteSpace(value), Element.ElementType.Day)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the 4th element of the 5 element schedule rule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The 4th element of the 5 element schedule rule.</returns>
    Public Property Months() As String
        Get
            Return m_months.Text
        End Get
        Set(ByVal value As String)
            m_months = New Element(RemoveWhiteSpace(value), Element.ElementType.Month)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the 5th element of the 5 element schedule rule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The 5th element of the 5 element schedule rule.</returns>
    Public Property DaysOfWeek() As String
        Get
            Return m_dayOfWeek.Text
        End Get
        Set(ByVal value As String)
            m_dayOfWeek = New Element(RemoveWhiteSpace(value), Element.ElementType.DayOfWeek)
        End Set
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
                .Append(Environment.NewLine)
                .Append("             Schedule rule: ")
                .Append(Rule)
                .Append(Environment.NewLine)
                .Append("             Last run time: ")
                .Append(IIf(m_lastRunDateTime = System.DateTime.MinValue, "Never", m_lastRunDateTime))
                .Append(Environment.NewLine)

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
        If m_minutes.Matches(currentDateTime) AndAlso m_hours.Matches(currentDateTime) AndAlso _
                m_days.Matches(currentDateTime) AndAlso m_months.Matches(currentDateTime) AndAlso _
                m_dayOfWeek.Matches(currentDateTime) Then
            m_lastRunDateTime = currentDateTime
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

End Class
