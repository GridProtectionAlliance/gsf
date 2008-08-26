' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Net.Sockets

Namespace Net.Ftp

    Public Class Response

        Private m_responses As Queue
        Private m_code As Integer

        Public Const InvalidCode As Integer = -1
        Public Const DataChannelOpenedTransferStart As Integer = 125
        Public Const FileOkBeginOpenDataChannel As Integer = 150
        Public Const ServiceReady As Integer = 220
        Public Const ClosingDataChannel As Integer = 226
        Public Const EnterPassiveMode As Integer = 227
        Public Const RequestFileActionComplete As Integer = 250
        Public Const UserLoggedIn As Integer = 230
        Public Const UserAcceptedWaitingPass As Integer = 331
        Public Const RequestFileActionPending As Integer = 350
        Public Const ServiceUnavailable As Integer = 421
        Public Const TransferAborted As Integer = 426

        Friend Sub New(ByVal stream As NetworkStream)

            Dim response As String

            m_responses = New Queue()

            Do
                response = GetLine(stream)

                Try
                    m_code = InvalidCode
                    m_code = Integer.Parse(response.Substring(0, 3))
                Catch
                    Throw New InvalidResponseException("Invalid response", Me)
                End Try

                m_responses.Enqueue(response)
            Loop While response.Length >= 4 And response.Chars(3) = "-"

            If m_code = ServiceUnavailable Then
                Throw New ServerDownException(Me)
            End If

        End Sub

        Public ReadOnly Property Message() As String
            Get
                Return CStr(m_responses.Peek())
            End Get
        End Property

        Public ReadOnly Property Respones() As Queue
            Get
                Return m_responses
            End Get
        End Property

        Public ReadOnly Property Code() As Integer
            Get
                Return m_code
            End Get
        End Property

        Private Function ReadAppendChar(ByVal stream As NetworkStream, ByVal toAppend As System.Text.StringBuilder) As Char

            Dim i As Integer = stream.ReadByte()

            If i > -1 Then
                Dim c As Char = Chr(i)
                toAppend.Append(c)
                Return c
            Else
                Throw New EndOfStreamException("Attempt to read past end of stream")
            End If

        End Function

        Private Function GetLine(ByVal stream As NetworkStream) As String

            Dim buff As New System.Text.StringBuilder(256)

            While True
                While ReadAppendChar(stream, buff) <> ControlChars.Cr
                End While

                While ReadAppendChar(stream, buff) = ControlChars.Cr
                End While

                If buff(buff.Length - 1) = ControlChars.Lf Then Exit While
            End While

            Return buff.ToString()

        End Function

    End Class

End Namespace