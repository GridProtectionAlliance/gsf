' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.Collections.Specialized
Imports System.Text.RegularExpressions
Imports TVA.Console.Common

Namespace Console

    ' This class based on a similar C# class found on "The Code Project" web site written by
    ' Richard Lopes of Paris, France.  It was translated into VB, put into the TVA.Console
    ' namspace and was modified to handle ordered arguments - JRC
    ' Please note the following deviation from the original behavior: the "param value"
    ' (i.e., param "space" value) named parameter form was dropped so that this class would
    ' be able to handle stand-alone ordered parameters.
    Public Class Arguments

        Implements IEnumerable

        Public OrderedArgID As String
        Private OrderedArg As Integer
        Private Parameters As StringDictionary

        Public Sub New(ByVal CommandLine As String, Optional ByVal OrderedArgID As String = "OrderedArg")

            Dim Spliter As New Regex("^-{1,2}|^/|=|:", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
            Dim Remover As New Regex("^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
            Dim Parameter As String
            Dim Parts As String()

            Me.Parameters = New StringDictionary
            Me.OrderedArgID = OrderedArgID
            Me.OrderedArg = 0

            ' Valid parameters forms:
            '   {-,/,--}param{=,:}((",')value(",'))
            ' Examples: 
            '   -param1=value1 --param2 /param3:"Test-:-work" 
            '   /param4=happy -param5 '--=nice=--'
            For Each Arg As String In CommandLineArgs(CommandLine)
                If Len(Arg) > 0 Then
                    ' If this argument begins with a quote, we treat it as a stand-alone argument
                    If Arg.Chars(0) = """"c Or Arg.Chars(0) = "'"c Then
                        ' Handle stand alone ordered arguments
                        OrderedArg += 1
                        Parameter = OrderedArgID & OrderedArg

                        ' Remove possible enclosing characters (",')
                        If Not Parameters.ContainsKey(Parameter) Then
                            Arg = Remover.Replace(Arg, "$1")
                            Parameters.Add(Parameter, Arg)
                        End If

                        Parameter = Nothing
                    Else
                        ' Look for new parameters (-,/ or --) and a
                        ' possible enclosed value (=,:)
                        Parts = Spliter.Split(Arg, 3)

                        Select Case Parts.Length
                            Case 1
                                ' Found just a parameter
                                ' The last parameter is still waiting. 
                                ' With no value, set it to true.
                                If Not Parameter Is Nothing Then
                                    If Not Parameters.ContainsKey(Parameter) Then Parameters.Add(Parameter, "True")
                                End If

                                Parameter = Nothing

                                ' Handle stand alone ordered arguments
                                OrderedArg += 1
                                Parameter = OrderedArgID & OrderedArg

                                ' Remove possible enclosing characters (",')
                                If Not Parameters.ContainsKey(Parameter) Then
                                    Arg = Remover.Replace(Arg, "$1")
                                    Parameters.Add(Parameter, Arg)
                                End If

                                Parameter = Nothing
                            Case 2
                                ' Found just a parameter
                                ' The last parameter is still waiting. 
                                ' With no value, set it to true.
                                If Not Parameter Is Nothing Then
                                    If Not Parameters.ContainsKey(Parameter) Then Parameters.Add(Parameter, "True")
                                End If

                                Parameter = Parts(1)
                            Case 3
                                ' Parameter with enclosed value
                                ' The last parameter is still waiting. 
                                ' With no value, set it to true.
                                If Not Parameter Is Nothing Then
                                    If Not Parameters.ContainsKey(Parameter) Then Parameters.Add(Parameter, "True")
                                End If

                                Parameter = Parts(1)

                                ' Remove possible enclosing characters (",')
                                If Not Parameters.ContainsKey(Parameter) Then
                                    Parts(2) = Remover.Replace(Parts(2), "$1")
                                    Parameters.Add(Parameter, Parts(2))
                                End If

                                Parameter = Nothing
                        End Select
                    End If
                End If
            Next

            ' In case a parameter is still waiting
            If Not Parameter Is Nothing Then
                If Not Parameters.ContainsKey(Parameter) Then Parameters.Add(Parameter, "True")
            End If

        End Sub

        ' Retrieve a parameter value if it exists 
        Default Public ReadOnly Property Item(ByVal Param As String) As String
            Get
                Return Parameters(Param)
            End Get
        End Property

        Public ReadOnly Property Exists(ByVal Param As String) As Boolean
            Get
                Return (Len(Trim(Me(Param))) > 0)
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return Parameters.Count
            End Get
        End Property

        Public ReadOnly Property OrderedArgCount() As Integer
            Get
                Return OrderedArg
            End Get
        End Property

        Public ReadOnly Property ContainHelpRequest() As Boolean
            Get
                Return (Parameters.ContainsKey("?") Or Parameters.ContainsKey("Help"))
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

            Return Parameters.GetEnumerator()

        End Function

    End Class

End Namespace