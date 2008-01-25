' 11/24/20006

Public Class GenericEventArgs(Of T)
    Inherits EventArgs

    Private m_argument As T

    Public Sub New(ByVal argument As T)

        MyBase.New()
        m_argument = argument

    End Sub

    Public Property Argument() As T
        Get
            Return m_argument
        End Get
        Set(ByVal value As T)
            m_argument = value
        End Set
    End Property

End Class
