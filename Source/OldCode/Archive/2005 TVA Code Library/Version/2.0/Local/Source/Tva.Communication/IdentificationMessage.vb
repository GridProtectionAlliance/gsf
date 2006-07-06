' 06-09-06

<Serializable()> _
Friend Class IdentificationMessage

    Private m_id As Guid
    Private m_system As String
    Private m_ntUser As String
    Private m_assembly As String
    Private m_location As String
    Private m_created As Date

    Public Property ID() As Guid
        Get
            Return m_id
        End Get
        Set(ByVal value As Guid)
            m_id = value
        End Set
    End Property

    Public Property System() As String
        Get
            Return m_system
        End Get
        Set(ByVal value As String)
            m_system = value
        End Set
    End Property

    Public Property NTUser() As String
        Get
            Return m_ntUser
        End Get
        Set(ByVal value As String)
            m_ntUser = value
        End Set
    End Property

    Public Property Assembly() As String
        Get
            Return m_assembly
        End Get
        Set(ByVal value As String)
            m_assembly = value
        End Set
    End Property

    Public Property Location() As String
        Get
            Return m_location
        End Get
        Set(ByVal value As String)
            m_location = value
        End Set
    End Property

    Public Property Created() As Date
        Get
            Return m_created
        End Get
        Set(ByVal value As Date)
            m_created = value
        End Set
    End Property

End Class