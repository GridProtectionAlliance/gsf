' James Ritchie Carroll - 2003
Option Explicit On 

Namespace Ftp

    Public Class AsyncResult

        Public Const Complete As Integer = 0
        Public Const Fail As Integer = 1
        Public Const Abort As Integer = 2

        Private m_result As BitArray
        Private m_message As String
        Private m_ftpResponse As Integer

        Friend Sub New()

            MyClass.New("Success.", CInt(Response.InvalidCode), Complete)

        End Sub

        Friend Sub New(ByVal message As String, ByVal result As Integer)

            MyClass.New(message, CInt(Response.InvalidCode), result)

        End Sub

        Friend Sub New(ByVal message As String, ByVal ftpCode As Integer, ByVal result As Integer)

            m_result = New BitArray(3)
            m_message = message
            m_ftpResponse = ftpCode
            m_result(result) = True

        End Sub

        Public ReadOnly Property IsSuccess() As Boolean
            Get
                Return m_result(Complete)
            End Get
        End Property

        Public ReadOnly Property IsFailed() As Boolean
            Get
                Return m_result(Fail)
            End Get
        End Property

        Public ReadOnly Property IsAborted() As Boolean
            Get
                Return m_result(Abort)
            End Get
        End Property

        Public ReadOnly Property ResponseCode() As Integer
            Get
                Return m_ftpResponse
            End Get
        End Property

        Public ReadOnly Property Message() As String
            Get
                Return m_message
            End Get
        End Property

    End Class

End Namespace