' James Ritchie Carroll - 2003
Option Explicit On 

Namespace [Shared]

    ' Bit Manipulation Functions
    Public Class Bit

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Returns the high byte (Int8) from a word (Int16)
        Public Shared Function HiByte(ByVal w As Integer) As Integer

            If w And &H8000 Then
                Return &H80 Or ((w And &H7FFF) \ 256)
            Else
                Return w \ 256
            End If

        End Function

        ' Returns the high word (Int16) from a double word (Int32)
        Public Shared Function HiWord(ByVal dw As Integer) As Integer

            If dw And &H80000000 Then
                Return &H8000 Or ((dw And &H7FFF0000) \ 65536)
            Else
                Return dw \ 65536
            End If

        End Function

        ' Returns the low byte (Int8) from a word (Int16)
        Public Shared Function LoByte(ByVal w As Integer) As Integer

            Return w And &HFF

        End Function

        ' Returns the low word (Int16) from a double word (Int32)
        Public Shared Function LoWord(ByVal dw As Integer) As Integer

            If dw And &H8000& Then
                Return &H8000 Or (dw And &H7FFF&)
            Else
                Return dw And &HFFFF&
            End If

        End Function

        ' Bits shifts word (Int16) value to the left "n" times
        Public Shared Function LShiftWord(ByVal w As Integer, ByVal n As Integer) As Integer

            Dim dw As Integer

            dw = w * (2 ^ n)

            If dw And &H8000& Then
                Return CInt(dw And &H7FFF&) Or &H8000
            Else
                Return dw And &HFFFF&
            End If

        End Function

        ' Bits shifts word (Int16) value to the right "n" times
        Public Shared Function RShiftWord(ByVal w As Integer, ByVal n As Integer) As Integer

            Dim dw As Integer

            If n = 0 Then
                Return w
            Else
                dw = w And &HFFFF&
                dw = dw \ (2 ^ n)
                Return dw And &HFFFF&
            End If

        End Function

        ' Makes a word (Int16) from two bytes (Int8)
        Public Shared Function MakeWord(ByVal bHi As Integer, ByVal bLo As Integer) As Integer

            If bHi And &H80 Then
                Return (((bHi And &H7F) * 256) + bLo) Or &H8000
            Else
                Return (bHi * 256) + bLo
            End If

        End Function

        ' Makes a double word (Int32) from two words (Int16)
        Public Shared Function MakeDWord(ByVal wHi As Integer, ByVal wLo As Integer) As Integer

            If wHi And &H8000& Then
                Return (((wHi And &H7FFF&) * 65536) Or (wLo And &HFFFF&)) Or &H80000000
            Else
                Return (wHi * 65536) + (wLo And &HFFFF&)
            End If

        End Function

    End Class

End Namespace