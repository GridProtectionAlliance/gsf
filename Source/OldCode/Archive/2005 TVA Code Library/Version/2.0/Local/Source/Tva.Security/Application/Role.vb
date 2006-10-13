
Namespace Application

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

    End Class

End Namespace