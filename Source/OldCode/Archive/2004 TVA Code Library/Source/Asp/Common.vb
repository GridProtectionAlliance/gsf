' James Ritchie Carroll - 2003
Option Explicit On 

Namespace Asp

    Public Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Performs JavaScript encoding on given string
        Public Shared Function JavaScriptEncode(ByVal Str As String) As String

            Str = Replace(Str, "\", "\\")
            Str = Replace(Str, "'", "\'")
            Str = Replace(Str, """", "\""")
            Str = Replace(Str, Chr(8), "\b")
            Str = Replace(Str, Chr(9), "\t")
            Str = Replace(Str, Chr(10), "\r")
            Str = Replace(Str, Chr(12), "\f")
            Str = Replace(Str, Chr(13), "\n")

            Return Str

        End Function

        ' Decodes JavaScript characters from given string
        Public Shared Function JavaScriptDecode(ByVal Str As String) As String

            Str = Replace(Str, "\\", "\")
            Str = Replace(Str, "\'", "'")
            Str = Replace(Str, "\""", """")
            Str = Replace(Str, "\b", Chr(8))
            Str = Replace(Str, "\t", Chr(9))
            Str = Replace(Str, "\r", Chr(10))
            Str = Replace(Str, "\f", Chr(12))
            Str = Replace(Str, "\n", Chr(13))

            Return Str

        End Function

        ' Ensures a string is compliant with cookie name requirements
        Public Shared Function ValidCookieName(ByVal Str As String) As String

            Str = Replace(Str, "=", "")
            Str = Replace(Str, ";", "")
            Str = Replace(Str, ",", "")
            Str = Replace(Str, Chr(9), "")
            Str = Replace(Str, Chr(10), "")
            Str = Replace(Str, Chr(13), "")

            Return Str

        End Function

        ' Ensures a string is compliant with cookie value requirements
        Public Shared Function ValidCookieValue(ByVal Str As String) As String

            Str = Replace(Str, ";", "")
            Str = Replace(Str, ",", "")

            Return Str

        End Function

    End Class

End Namespace