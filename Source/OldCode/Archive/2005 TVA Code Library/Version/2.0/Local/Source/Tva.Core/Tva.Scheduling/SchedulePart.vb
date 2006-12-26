Imports System.Text.RegularExpressions

Namespace Scheduling

    Public Class SchedulePart

#Region " Member Declaration "

        Private m_text As String
        Private m_dateTimePart As Tva.DateTime.DateTimePart
        Private m_textSyntax As SchedulePartTextSyntax
        Private m_values As List(Of Integer)

#End Region

#Region " Public Code "

        Public Sub New(ByVal text As String, ByVal dateTimePart As Tva.DateTime.DateTimePart)

            MyBase.New()
            If ValidateAndPopulate(text, dateTimePart) Then
                ' The text provided for populating the values is valid according to the specified date-time part.
                m_text = text
                m_dateTimePart = dateTimePart
            Else
                Throw New ArgumentException("Text is not valid for " & dateTimePart.ToString() & " schedule part.")
            End If

        End Sub

        ''' <summary>
        ''' Gets the text used for populating the values of the schedule part.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The text used for populating the values of the schedule part.</returns>
        Public ReadOnly Property Text() As String
            Get
                Return m_text
            End Get
        End Property

        ''' <summary>
        ''' Gets the date-time part that the schedule part represents in a Tva.Scheduling.Schedule.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The date-time part that the schedule part represents in a Tva.Scheduling.Schedule.</returns>
        Public ReadOnly Property DateTimePart() As Tva.DateTime.DateTimePart
            Get
                Return m_dateTimePart
            End Get
        End Property

        ''' <summary>
        ''' Gets the syntax used in the text specified for populating the values of the schedule part.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The syntax used in the text specified for populating the values of the schedule part.</returns>
        Public ReadOnly Property TextSyntax() As SchedulePartTextSyntax
            Get
                Return m_textSyntax
            End Get
        End Property

        ''' <summary>
        ''' Gets a meaningful description of the schedule part.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A meaningful description of the schedule part.</returns>
        Public ReadOnly Property Description() As String
            Get
                Select Case m_textSyntax
                    Case SchedulePartTextSyntax.Any
                        Return "Every " & m_dateTimePart.ToString()
                    Case SchedulePartTextSyntax.EveryN
                        Return "Every " & m_text.Split("/"c)(1) & " " & m_dateTimePart.ToString()
                    Case SchedulePartTextSyntax.Range
                        Dim range As String() = m_text.Split("-"c)
                        Return "Every " & m_dateTimePart.ToString() & " from " & range(0) & " to " & range(1)
                    Case SchedulePartTextSyntax.Specific
                        Return "Every " & m_text & " " & m_dateTimePart.ToString()
                    Case Else
                        Return ""
                End Select
            End Get
        End Property

        ''' <summary>
        ''' Gets a list of values that were populated from based on the specified text and date-time part that the 
        ''' schedule part represents.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' A list of values that were populated from based on the specified text and date-time part that the 
        ''' schedule part represents.</returns>
        Public ReadOnly Property Values() As List(Of Integer)
            Get
                Return m_values
            End Get
        End Property

        Public Function Matches(ByVal dateAndTime As System.DateTime) As Boolean

            Select Case m_dateTimePart
                Case DateTimePart.Minute
                    Return m_values.Contains(dateAndTime.Minute)
                Case DateTimePart.Hour
                    Return m_values.Contains(dateAndTime.Hour)
                Case DateTimePart.Day
                    Return m_values.Contains(dateAndTime.Day)
                Case DateTimePart.Month
                    Return m_values.Contains(dateAndTime.Month)
                Case DateTimePart.DayOfWeek
                    Return m_values.Contains(Convert.ToInt32(dateAndTime.DayOfWeek))
            End Select

        End Function

#End Region

#Region " Private Code"

        Private Function ValidateAndPopulate(ByVal schedulePart As String, ByVal dateTimePart As Tva.DateTime.DateTimePart) As Boolean

            Dim minValue As Integer = 0
            Dim maxValue As Integer = 0
            Select Case DateTimePart
                Case DateTime.DateTimePart.Minute
                    maxValue = 59
                Case DateTime.DateTimePart.Hour
                    maxValue = 23
                Case DateTime.DateTimePart.Day
                    minValue = 1
                    maxValue = 31
                Case DateTime.DateTimePart.Month
                    minValue = 1
                    maxValue = 12
                Case DateTime.DateTimePart.DayOfWeek
                    maxValue = 6
            End Select

            m_values = New List(Of Integer)()
            If Regex.Match(schedulePart, "^(\*){1}$").Success Then
                ' ^(\*){1}$             Matches: *
                m_textSyntax = SchedulePartTextSyntax.Any
                PopulateValues(minValue, maxValue, 1)

                Return True
            ElseIf Regex.Match(schedulePart, "^(\*/\d+){1}$").Success Then
                ' ^(\*/\d+){1}$         Matches: */[any digit]
                Dim interval As Integer = Convert.ToInt32(schedulePart.Split("/"c)(1))
                If interval > 0 AndAlso interval >= minValue AndAlso interval <= maxValue Then
                    m_textSyntax = SchedulePartTextSyntax.EveryN
                    PopulateValues(minValue, maxValue, interval)

                    Return True
                End If
            ElseIf Regex.Match(schedulePart, "^(\d+\-\d+){1}$").Success Then
                ' ^(\d+\-\d+){1}$       Matches: [any digit]-[any digit]
                Dim range As String() = schedulePart.Split("-"c)
                Dim lowRange As Integer = Convert.ToInt32(range(0))
                Dim highRange As Integer = Convert.ToInt32(range(1))
                If lowRange < highRange AndAlso lowRange >= minValue AndAlso highRange <= maxValue Then
                    m_textSyntax = SchedulePartTextSyntax.Range
                    PopulateValues(lowRange, highRange, 1)

                    Return True
                End If
            ElseIf Regex.Match(schedulePart, "^((\d+,?)+){1}$").Success Then
                ' ^((\d+,?)+){1}$       Matches: [any digit] AND [any digit], ..., [any digit]
                m_textSyntax = SchedulePartTextSyntax.Specific
                For Each value As Integer In schedulePart.Split(","c)
                    If Not (value >= minValue AndAlso value <= maxValue) Then
                        Return False
                    Else
                        If Not m_values.Contains(value) Then m_values.Add(value)
                    End If
                Next

                Return True
            End If

            Return False

        End Function

        Private Sub PopulateValues(ByVal fromValue As Integer, ByVal toValue As Integer, ByVal stepValue As Integer)

            For i As Integer = fromValue To toValue Step stepValue
                m_values.Add(i)
            Next

        End Sub

#End Region

    End Class

End Namespace