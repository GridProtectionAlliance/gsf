
Namespace Application

    <Serializable()> _
    Public Class Role

        Private m_name As String
        Private m_description As String
        Private m_application As Application

        Public Sub New(ByVal name As String, ByVal description As String, ByVal application As Application)

            m_name = name
            m_description = description
            m_application = application

        End Sub

        Public ReadOnly Property Application() As Application
            Get
                Return m_application
            End Get
        End Property

        Public ReadOnly Property Name() As String
            Get
                Return m_name
            End Get
        End Property

        Public ReadOnly Property Description() As String
            Get
                Return m_description
            End Get
        End Property

        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Dim other As Role = TryCast(obj, Role)
            If other IsNot Nothing Then
                Return (m_name = other.Name AndAlso m_description = other.Description AndAlso m_application.Equals(other.Application))
            Else
                Return False
            End If

        End Function

    End Class

End Namespace