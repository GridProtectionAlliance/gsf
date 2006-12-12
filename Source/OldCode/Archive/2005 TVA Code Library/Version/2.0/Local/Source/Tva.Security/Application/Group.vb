' 10-19-06

Namespace Application

    <Serializable()> _
    Public Class Group

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

            Dim other As Group = TryCast(obj, Group)
            If other IsNot Nothing Then
                If m_name = other.Name AndAlso m_description = other.Description Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If

        End Function

    End Class

End Namespace