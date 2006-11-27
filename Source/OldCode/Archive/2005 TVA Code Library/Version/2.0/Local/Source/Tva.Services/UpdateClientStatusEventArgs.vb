Public Class UpdateClientStatusEventArgs
    Inherits EventArgs

    Private m_update As String

    Public Sub New(ByVal update As String)

        MyBase.New()
        m_update = update

    End Sub

    Public Property Update() As String
        Get
            Return m_update
        End Get
        Set(ByVal value As String)
            m_update = value
        End Set
    End Property

End Class
