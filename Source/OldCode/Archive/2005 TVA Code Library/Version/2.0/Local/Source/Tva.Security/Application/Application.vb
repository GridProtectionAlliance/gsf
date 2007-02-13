
Namespace Application

    <Serializable()> _
    Public Class Application

        Private m_name As String
        Private m_description As String

        Public Sub New(ByVal name As String, ByVal description As String)

            m_name = name
            m_description = description

        End Sub

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

            Dim other As Application = TryCast(obj, Application)
            If other IsNot Nothing Then
                Return (m_name = other.Name AndAlso m_description = other.Description)
            Else
                Return False
            End If

        End Function

    End Class

End Namespace