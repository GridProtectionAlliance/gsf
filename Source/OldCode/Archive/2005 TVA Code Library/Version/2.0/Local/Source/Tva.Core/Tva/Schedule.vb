' 08-01-06

Option Strict On

Imports System.Text.RegularExpressions
Imports Tva.Text.Common

Public Class Schedule

#Region "Private Schedule Element Class"

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

    Private m_minutes As Element
    Private m_hours As Element
    Private m_days As Element
    Private m_months As Element
    Private m_dayOfWeek As Element

    Public Sub New(ByVal schedule As String)
        MyBase.New()
        With RemoveDuplicateWhiteSpace(schedule).Split(" "c)
            If .Length = 5 Then
                MyClass.Minutes = .GetValue(0).ToString()
                MyClass.Hours = .GetValue(1).ToString()
                MyClass.Days = .GetValue(2).ToString()
                MyClass.Months = .GetValue(3).ToString()
                MyClass.DaysOfWeek = .GetValue(4).ToString()
            Else
                Throw New ArgumentException("Schedule must have exactly 5 elements (Example: * * * * *).")
            End If
        End With
    End Sub

    Public Sub New(ByVal minutes As String, ByVal hours As String, ByVal days As String, ByVal months As String, _
            ByVal daysOfWeek As String)
        MyBase.New()
        MyClass.Minutes = minutes
        MyClass.Hours = hours
        MyClass.Days = days
        MyClass.Months = months
        MyClass.DaysOfWeek = daysOfWeek
    End Sub

    Public Property Minutes() As String
        Get
            Return m_minutes.Text
        End Get
        Set(ByVal value As String)
            m_minutes = New Element(RemoveWhiteSpace(value), Element.ElementType.Minute)
        End Set
    End Property

    Public Property Hours() As String
        Get
            Return m_hours.Text
        End Get
        Set(ByVal value As String)
            m_hours = New Element(RemoveWhiteSpace(value), Element.ElementType.Hour)
        End Set
    End Property

    Public Property Days() As String
        Get
            Return m_days.Text
        End Get
        Set(ByVal value As String)
            m_days = New Element(RemoveWhiteSpace(value), Element.ElementType.Day)
        End Set
    End Property

    Public Property Months() As String
        Get
            Return m_months.Text
        End Get
        Set(ByVal value As String)
            m_months = New Element(RemoveWhiteSpace(value), Element.ElementType.Month)
        End Set
    End Property

    Public Property DaysOfWeek() As String
        Get
            Return m_dayOfWeek.Text
        End Get
        Set(ByVal value As String)
            m_dayOfWeek = New Element(RemoveWhiteSpace(value), Element.ElementType.DayOfWeek)
        End Set
    End Property

    Public Function IsDue() As Boolean

        Dim currentTime As System.DateTime = System.DateTime.Now
        Return m_minutes.Matches(currentTime) And m_hours.Matches(currentTime) And m_days.Matches(currentTime) And _
            m_months.Matches(currentTime) And m_dayOfWeek.Matches(currentTime)

    End Function

    Public Overrides Function ToString() As String

        Return m_minutes.Text & " " & m_hours.Text & " " & m_days.Text & " " & m_months.Text & " " & m_dayOfWeek.Text

    End Function

End Class
