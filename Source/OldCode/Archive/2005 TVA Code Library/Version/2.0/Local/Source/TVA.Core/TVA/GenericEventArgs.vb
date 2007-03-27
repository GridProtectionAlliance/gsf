' 11/24/20006

Public Class GenericEventArgs
    Inherits EventArgs

    Private m_arguments As Object()

    Public Sub New(ByVal arguments As Object())

        MyBase.New()
        m_arguments = arguments

    End Sub

    Public Property Arguments() As Object()
        Get
            Return m_arguments
        End Get
        Set(ByVal value As Object())
            m_arguments = value
        End Set
    End Property

End Class
