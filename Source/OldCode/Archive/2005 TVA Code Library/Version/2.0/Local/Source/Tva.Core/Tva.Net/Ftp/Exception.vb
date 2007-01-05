' James Ritchie Carroll - 2003
Option Explicit On 

Namespace Net.Ftp

    Public Class Exception

        Inherits System.Exception

        Private m_ftpResponse As Response = Nothing

        Friend Sub New(ByVal message As String)

            MyBase.New(message)

        End Sub

        Friend Sub New(ByVal message As String, ByVal inner As Exception)

            MyBase.New(message, inner)

        End Sub

        Friend Sub New(ByVal message As String, ByVal ftpResponse As Response)

            MyBase.New(message)
            m_ftpResponse = ftpResponse

        End Sub

        Public ReadOnly Property ResponseMessage() As String
            Get
                If Not (m_ftpResponse Is Nothing) Then
                    Return m_ftpResponse.Message
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Message() As String
            Get
                Return MyBase.Message
            End Get
        End Property

    End Class

    Public Class InvalidResponseException

        Inherits Exception

        Friend Sub New(ByVal message As String, ByVal ftpResponse As Response)

            MyBase.New(message, ftpResponse)

        End Sub

    End Class

    Public Class AuthenticationException

        Inherits Exception

        Friend Sub New(ByVal message As String, ByVal ftpResponse As Response)

            MyBase.New(message, ftpResponse)

        End Sub

    End Class

    Public Class FileNotFoundException

        Inherits Exception

        Friend Sub New(ByVal remoteFile As String)

            MyBase.New("Remote file (" & remoteFile & ") not found.  Try refreshing the directory.")

        End Sub

    End Class

    Public Class ServerDownException

        Inherits Exception

        Friend Sub New(ByVal ftpResponse As Response)

            Me.New("FTP service was down.", ftpResponse)

        End Sub

        Friend Sub New(ByVal message As String, ByVal ftpResponse As Response)

            MyBase.New(message, ftpResponse)

        End Sub

    End Class

    Public Class CommandException

        Inherits Exception

        Friend Sub New(ByVal message As String, ByVal ftpResponse As Response)

            MyBase.New(message, ftpResponse)

        End Sub

    End Class

    Public Class DataTransferException

        Inherits Exception

        Friend Sub New()

            MyBase.New("Data transfer error: previous transfer not finished.")

        End Sub

        Friend Sub New(ByVal message As String, ByVal ftpResponse As Response)

            MyBase.New(message, ftpResponse)

        End Sub

    End Class

    Public Class UserAbortException

        Inherits Exception

        Friend Sub New()

            MyBase.New("File Transfer aborted by user.")

        End Sub

    End Class

    Public Class ResumeNotSupportedException

        Inherits Exception

        Friend Sub New(ByVal ftpResponse As Response)

            MyBase.New("Data transfer error: server does not support resuming.", ftpResponse)

        End Sub

    End Class

End Namespace