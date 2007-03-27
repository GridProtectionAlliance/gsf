' 11/24/2006

Public Class ServiceResponseEventArgs
    Inherits EventArgs

    Private m_serviceResponse As ServiceResponse

    Public Sub New(ByVal serviceResponse As ServiceResponse)

        MyBase.New()
        m_serviceResponse = serviceResponse

    End Sub

    Public Property ServiceResponse() As ServiceResponse
        Get
            Return m_serviceResponse
        End Get
        Set(ByVal value As ServiceResponse)
            m_serviceResponse = value
        End Set
    End Property

End Class
