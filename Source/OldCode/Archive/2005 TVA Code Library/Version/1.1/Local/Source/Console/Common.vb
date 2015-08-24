' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.Text

Namespace Console

    Public Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' VB doesn't provide you with an array of tokenized command line arguments - they're all in one string.  So this function
        ' creates the desired tokenized argument array from the VB command line...
        ' This function will always return at least one argument, even if it's an empty-string
        Public Shared ReadOnly Property CommandLineArgs(ByVal CommandLine As String) As String()
            Get
                If Len(CommandLine) > 0 Then
                    Dim strArgs As String()
                    Dim strQuotedArg As String = ""
                    Dim lstArgs As New ArrayList
                    Dim strEncodedQuote = Guid.NewGuid.ToString()
                    Dim strEncodedSpace = Guid.NewGuid.ToString()
                    Dim strEncodedCommand As New StringBuilder
                    Dim flgInQuote As Boolean
                    Dim chrCurr As Char

                    ' Encode embedded quotes - we allow embedded/nested quotes encoded as \"
                    CommandLine = Replace(CommandLine, "\""", strEncodedQuote)

                    ' Combine any quoted strings into a single arg by encoding embedded spaces
                    For x As Integer = 0 To CommandLine.Length - 1
                        chrCurr = CommandLine.Chars(x)

                        If chrCurr = """"c Then
                            If flgInQuote Then
                                flgInQuote = False
                            Else
                                flgInQuote = True
                            End If
                        End If

                        If flgInQuote Then
                            If chrCurr = " "c Then
                                strEncodedCommand.Append(strEncodedSpace)
                            Else
                                strEncodedCommand.Append(chrCurr)
                            End If
                        Else
                            strEncodedCommand.Append(chrCurr)
                        End If
                    Next

                    CommandLine = strEncodedCommand.ToString()

                    ' Parse every argument out by space and combine any quoted strings into a single arg
                    For Each strArg As String In CommandLine.Split(" "c)
                        ' Add tokenized argument making sure to unencode any embedded quotes or spaces
                        strArg = Trim(Replace(Replace(strArg, strEncodedQuote, """"), strEncodedSpace, " "))
                        If Len(strArg) > 0 Then lstArgs.Add(strArg)
                    Next

                    strArgs = Array.CreateInstance(GetType(String), lstArgs.Count)
                    lstArgs.CopyTo(strArgs)
                    Return strArgs
                Else
                    Return New String() {""}
                End If
            End Get
        End Property

    End Class

End Namespace
