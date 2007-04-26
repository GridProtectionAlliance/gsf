' 02/08/2007

Imports System.Collections.Specialized
Imports System.Text.RegularExpressions

Namespace Console

    Public Class Arguments
        Implements IEnumerable

        Public OrderedArgID As String
        Private m_orderedArg As Integer
        Private m_parameters As StringDictionary

        Public Sub New(ByVal CommandLine As String, Optional ByVal OrderedArgID As String = "OrderedArg")

            Dim spliter As New Regex("^-{1,2}|^/|=|:", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
            Dim remover As New Regex("^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
            Dim parameter As String
            Dim parts As String()

            m_parameters = New StringDictionary
            OrderedArgID = OrderedArgID
            m_orderedArg = 0

            ' Valid parameters forms:
            '   {-,/,--}param{=,:}((",')value(",'))
            ' Examples: 
            '   -param1=value1 --param2 /param3:"Test-:-work" 
            '   /param4=happy -param5 '--=nice=--'
            For Each arg As String In Common.ParseCommand(CommandLine)
                If Not String.IsNullOrEmpty(arg) Then
                    ' If this argument begins with a quote, we treat it as a stand-alone argument
                    If arg.Chars(0) = """"c Or arg.Chars(0) = "'"c Then
                        ' Handle stand alone ordered arguments
                        m_orderedArg += 1
                        parameter = OrderedArgID & m_orderedArg

                        ' Remove possible enclosing characters (",')
                        If Not m_parameters.ContainsKey(parameter) Then
                            arg = remover.Replace(arg, "$1")
                            m_parameters.Add(parameter, arg)
                        End If

                        parameter = Nothing
                    Else
                        ' Look for new parameters (-,/ or --) and a
                        ' possible enclosed value (=,:)
                        parts = spliter.Split(arg, 3)

                        Select Case parts.Length
                            Case 1
                                ' Found just a parameter
                                ' The last parameter is still waiting. 
                                ' With no value, set it to true.
                                If Not parameter Is Nothing Then
                                    If Not m_parameters.ContainsKey(parameter) Then m_parameters.Add(parameter, "True")
                                End If

                                parameter = Nothing

                                ' Handle stand alone ordered arguments
                                m_orderedArg += 1
                                parameter = OrderedArgID & m_orderedArg

                                ' Remove possible enclosing characters (",')
                                If Not m_parameters.ContainsKey(parameter) Then
                                    arg = remover.Replace(arg, "$1")
                                    m_parameters.Add(parameter, arg)
                                End If

                                parameter = Nothing
                            Case 2
                                ' Found just a parameter
                                ' The last parameter is still waiting. 
                                ' With no value, set it to true.
                                If Not parameter Is Nothing Then
                                    If Not m_parameters.ContainsKey(parameter) Then m_parameters.Add(parameter, "True")
                                End If

                                parameter = parts(1)
                            Case 3
                                ' Parameter with enclosed value
                                ' The last parameter is still waiting. 
                                ' With no value, set it to true.
                                If Not parameter Is Nothing Then
                                    If Not m_parameters.ContainsKey(parameter) Then m_parameters.Add(parameter, "True")
                                End If

                                parameter = parts(1)

                                ' Remove possible enclosing characters (",')
                                If Not m_parameters.ContainsKey(parameter) Then
                                    parts(2) = remover.Replace(parts(2), "$1")
                                    m_parameters.Add(parameter, parts(2))
                                End If

                                parameter = Nothing
                        End Select
                    End If
                End If
            Next

            ' In case a parameter is still waiting
            If Not parameter Is Nothing Then
                If Not m_parameters.ContainsKey(parameter) Then m_parameters.Add(parameter, "True")
            End If

        End Sub

        ' Retrieve a parameter value if it exists 
        Default Public ReadOnly Property Item(ByVal param As String) As String
            Get
                Return m_parameters(param)
            End Get
        End Property

        Public ReadOnly Property Exists(ByVal param As String) As Boolean
            Get
                Return (Len(Trim(Me(param))) > 0)
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return m_parameters.Count
            End Get
        End Property

        Public ReadOnly Property OrderedArgCount() As Integer
            Get
                Return m_orderedArg
            End Get
        End Property

        Public ReadOnly Property ContainHelpRequest() As Boolean
            Get
                Return (m_parameters.ContainsKey("?") Or m_parameters.ContainsKey("Help"))
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

            Return m_parameters.GetEnumerator()

        End Function

    End Class

End Namespace