' 08-29-06

Imports Tva.Text.Common

<Serializable()> _
Public Class ServiceResponse

    Private m_type As ServiceResponseType
    Private m_parameters As String()

    Public Sub New()
        MyClass.New(ServiceResponseType.Undetermined)
    End Sub

    Public Sub New(ByVal type As ServiceResponseType)
        MyClass.New(type, New String() {"Undetermined"})
    End Sub

    Public Sub New(ByVal type As ServiceResponseType, ByVal parameters As String())
        m_type = type
        m_parameters = parameters
    End Sub

    Public Property Type() As ServiceResponseType
        Get
            Return m_type
        End Get
        Set(ByVal value As ServiceResponseType)
            m_type = value
        End Set
    End Property

    Public Property Parameters() As String()
        Get
            Return m_parameters
        End Get
        Set(ByVal value As String())
            m_parameters = value
        End Set
    End Property

End Class