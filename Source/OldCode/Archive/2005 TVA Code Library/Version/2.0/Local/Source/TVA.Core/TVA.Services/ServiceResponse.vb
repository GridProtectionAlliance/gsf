' 08-29-06

Namespace Services

    <Serializable()> _
    Public Class ServiceResponse

        Private m_type As ServiceResponseType
        Private m_parameters As String()

        Public Sub New(ByVal type As String)

        End Sub

        Public Sub New(ByVal type As ServiceResponseType)

        End Sub

        Public Sub New(ByVal type As ServiceResponseType, ByVal parameters As String())
            m_type = type
            m_parameters = parameters
        End Sub

        Public ReadOnly Property Type() As ServiceResponseType
            Get
                Return m_type
            End Get
        End Property

        Public ReadOnly Property Parameters() As String()
            Get
                Return m_parameters
            End Get
        End Property

        Private Function GetReponseType(ByVal type As String) As ServiceResponseType

        End Function

    End Class

End Namespace
