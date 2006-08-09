' 08-01-06

Option Strict On

Imports System.Text.RegularExpressions
Imports Tva.Common

Public Class Schedule

    Private m_minute As Element
    Private m_hour As Element
    Private m_day As Element
    Private m_month As Element
    Private m_dayOfWeek As Element
    Private m_previousRuntime As Date
    Private m_nextRuntime As Date

    Public Sub New(ByVal schedule As String)
        MyBase.New()
        schedule = Tva.Text.Common.RemoveDuplicateWhiteSpace(schedule)
        Dim scheduleElements As String() = schedule.Split(" "c)
        If scheduleElements.Length = 5 Then
            MyClass.Minute = scheduleElements(0)
            MyClass.Hour = scheduleElements(1)
            MyClass.Day = scheduleElements(2)
            MyClass.Month = scheduleElements(3)
            MyClass.DayOfWeek = scheduleElements(4)
        Else
            Throw New ArgumentException()
        End If
    End Sub

    Public Property Minute() As String
        Get
            Return m_minute.Text
        End Get
        Set(ByVal value As String)
            m_minute = New Element(value, 0, 59)
        End Set
    End Property

    Public Property Hour() As String
        Get
            Return m_hour.Text
        End Get
        Set(ByVal value As String)
            m_hour = New Element(value, 0, 23)
        End Set
    End Property

    Public Property Day() As String
        Get
            Return m_day.Text
        End Get
        Set(ByVal value As String)
            m_day = New Element(value, 1, 31)
        End Set
    End Property

    Public Property Month() As String
        Get
            Return m_month.Text
        End Get
        Set(ByVal value As String)
            m_month = New Element(value, 1, 12)
        End Set
    End Property

    Public Property DayOfWeek() As String
        Get
            Return m_dayOfWeek.Text
        End Get
        Set(ByVal value As String)
            m_dayOfWeek = New Element(value, 0, 6)
        End Set
    End Property

    Public Function IsDue() As Boolean


    End Function

    Public Overrides Function ToString() As String

        Return m_minute.Text & " " & m_hour.Text & " " & m_day.Text & " " & m_month.Text & " " & m_dayOfWeek.Text

    End Function

    Private Class Element

        Private m_text As String
        Private m_elementType As ElementType
        Private m_valueType As ValueType
        Private m_values As Integer()
        Private m_minValue As Integer
        Private m_maxValue As Integer

        Public Enum ElementType As Integer
            Minute
            Hour
            Day
            Month
            DayOfWeek
        End Enum

        Public Enum ValueType As Integer
            ''' <summary>
            ''' *
            ''' </summary>
            Any
            ''' <summary>
            ''' */[any digit]
            ''' </summary>
            EveryN
            ''' <summary>
            ''' [any digit]-[any digit]
            ''' </summary>
            Range
            ''' <summary>
            ''' [any digit] AND [any digit], ..., [any digit]
            ''' </summary>
            Specific
        End Enum

        Public Sub New(ByVal text As String, ByVal minValue As Integer, ByVal maxValue As Integer)
            If IsValidElement(text, minValue, maxValue) Then
                m_text = text
                m_minValue = minValue
                m_maxValue = maxValue
            Else
                Throw New ArgumentException()
            End If
        End Sub

        Public ReadOnly Property Text() As String
            Get
                Return m_text
            End Get
        End Property

        Public ReadOnly Property Values() As Integer()
            Get
                Return m_values
            End Get
        End Property

        'Public ReadOnly Property Type() As ValueType
        '    Get
        '        Return m_type
        '    End Get
        'End Property

        Public Function IsDue() As Boolean

            Dim currentTime As System.DateTime = System.DateTime.Now

        End Function

        Private Function IsValidElement(ByVal element As String, ByVal minValue As Integer, ByVal maxValue As Integer) As Boolean

            If Regex.Match(element, "^(\*){1}$").Success Then
                ' ^(\*){1}$             Matches: *
                m_valueType = ValueType.Any
                PopulateValues(minValue, maxValue, 1)

                Return True
            ElseIf Regex.Match(element, "^(\*/\d+){1}$").Success Then
                ' ^(\*/\d+){1}$         Matches: */[any digit]
                m_valueType = ValueType.EveryN

                Dim interval As Integer = Convert.ToInt32(element.Split("/"c)(1))
                If interval > 0 AndAlso interval >= minValue AndAlso interval <= maxValue Then
                    PopulateValues(minValue, maxValue, interval)
                    Return True
                End If

                Return False
            ElseIf Regex.Match(element, "^(\d+\-\d+){1}$").Success Then
                ' ^(\d+\-\d+){1}$       Matches: [any digit]-[any digit]
                m_valueType = ValueType.Range

                Dim range As String() = element.Split("-"c)
                Dim lowRange As Integer = Convert.ToInt32(range(0))
                Dim highRange As Integer = Convert.ToInt32(range(1))
                If lowRange < highRange AndAlso lowRange >= minValue AndAlso highRange <= maxValue Then
                    PopulateValues(lowRange, highRange, 1)
                    Return True
                End If

                Return False
            ElseIf Regex.Match(element, "^((\d+,?)+){1}$").Success Then
                ' ^((\d+,?)+){1}$     Matches: [any digit] AND [any digit], ..., [any digit]
                m_valueType = ValueType.Specific

                Dim values As New List(Of Integer)()
                For Each value As Integer In element.Split(","c)
                    If Not (value >= minValue AndAlso value <= maxValue) Then
                        Return False
                    Else
                        If Not values.Contains(value) Then values.Add(value)
                    End If
                Next
                m_values = values.ToArray()

                Return True
            End If

        End Function

        Private Sub PopulateValues(ByVal fromValue As Integer, ByVal toValue As Integer, ByVal stepValue As Integer)

            m_values = CreateArray(Of Integer)(Convert.ToInt32(System.Math.Ceiling(((toValue - fromValue) + 1) / stepValue)))
            Dim index As Integer = 0
            For i As Integer = fromValue To toValue Step stepValue
                m_values(index) = i
                index += 1
            Next

        End Sub

    End Class

End Class
