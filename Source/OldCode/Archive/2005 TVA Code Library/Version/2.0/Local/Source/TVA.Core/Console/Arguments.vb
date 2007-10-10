'*******************************************************************************************************
'  TVA.Console.Arguments.vb - Common Configuration Functions
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/08/2007 - Pinal C. Patel
'       Generated original version of source code.
'  10/09/2007 - J. Ritchie Carroll / Pinal C. Patel
'       Fixed stand-alone argument bug at end of line and changed class to use generic Dictionary class
'
'*******************************************************************************************************

Imports System.Text.RegularExpressions

Namespace Console

    <Serializable()> _
    Public Class Arguments
        Implements IEnumerable

        Private m_commandLine As String
        Private m_orderedArgID As String
        Private m_orderedArgCount As Integer
        Private m_parameters As Dictionary(Of String, String)

        Public Sub New(ByVal commandLine As String)

            MyClass.New(commandLine, "OrderedArg")

        End Sub

        Public Sub New(ByVal commandLine As String, ByVal orderedArgID As String)

            Dim spliter As New Regex("^-{1,2}|^/|=|:", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
            Dim remover As New Regex("^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
            Dim parameter As String
            Dim parts As String()

            m_parameters = New Dictionary(Of String, String)(StringComparer.CurrentCultureIgnoreCase)
            m_orderedArgCount = 0
            m_commandLine = commandLine
            m_orderedArgID = orderedArgID

            ' Valid parameters forms:
            '   {-,/,--}param{=,:}((",')value(",'))
            ' Examples: 
            '   -param1=value1 --param2 /param3:"Test-:-work" 
            '   /param4=happy -param5 '--=nice=--'
            For Each arg As String In Common.ParseCommand(m_commandLine)
                ' Found just a parameter in last pass...
                ' The last parameter is still waiting, with no value, set it to nothing.
                If Not parameter Is Nothing Then If Not m_parameters.ContainsKey(parameter) Then m_parameters.Add(parameter, Nothing)
                parameter = Nothing

                If Not String.IsNullOrEmpty(arg) Then
                    ' If this argument begins with a quote, we treat it as a stand-alone argument
                    If arg.Chars(0) = """"c Or arg.Chars(0) = "'"c Then
                        ' Handle stand alone ordered arguments
                        m_orderedArgCount += 1
                        parameter = orderedArgID & m_orderedArgCount

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
                                ' Handle stand alone ordered arguments
                                m_orderedArgCount += 1
                                parameter = orderedArgID & m_orderedArgCount

                                ' Remove possible enclosing characters (",')
                                If Not m_parameters.ContainsKey(parameter) Then
                                    arg = remover.Replace(arg, "$1")
                                    m_parameters.Add(parameter, arg)
                                End If

                                parameter = Nothing
                            Case 2
                                ' Found just a parameter
                                parameter = parts(1)
                            Case 3
                                ' Parameter with enclosed value
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
                If Not m_parameters.ContainsKey(parameter) Then m_parameters.Add(parameter, Nothing)
            End If

        End Sub

        ' Retrieve a parameter value if it exists 
        Default Public Overridable ReadOnly Property Item(ByVal param As String) As String
            Get
                Return m_parameters(param)
            End Get
        End Property

        Public Overridable ReadOnly Property Exists(ByVal param As String) As Boolean
            Get
                Return m_parameters.ContainsKey(param)
            End Get
        End Property

        Public Overridable ReadOnly Property Count() As Integer
            Get
                Return m_parameters.Count
            End Get
        End Property

        Public Overridable ReadOnly Property OrderedArgID() As String
            Get
                Return m_orderedArgID
            End Get
        End Property

        Public Overridable ReadOnly Property OrderedArgCount() As Integer
            Get
                Return m_orderedArgCount
            End Get
        End Property

        Public Overridable ReadOnly Property ContainsHelpRequest() As Boolean
            Get
                Return (m_parameters.ContainsKey("?") Or m_parameters.ContainsKey("Help"))
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator

            Return m_parameters.GetEnumerator()

        End Function

        Public Overrides Function ToString() As String

            Return m_commandLine

        End Function

        Protected ReadOnly Property InternalDictionary() As Dictionary(Of String, String)
            Get
                Return m_parameters
            End Get
        End Property

    End Class

End Namespace