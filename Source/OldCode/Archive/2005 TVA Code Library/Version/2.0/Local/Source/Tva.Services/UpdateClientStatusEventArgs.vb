Public Class UpdateClientStatusEventArgs
    Inherits EventArgs

    Private m_message As String

    Public Sub New(ByVal message As String)

        MyBase.New()
        m_message = message

    End Sub

    Public Property Message() As String
        Get
            Return m_message
        End Get
        Set(ByVal value As String)
            m_message = value
        End Set
    End Property

End Class
