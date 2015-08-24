' 07-06-06

<Serializable()> _
Friend Class GoodbyeMessage

    Private m_id As Guid

    Public Sub New(ByVal id As Guid)
        Me.ID = id
    End Sub

    ''' <summary>
    ''' Gets or sets the disconnecting client's ID.
    ''' </summary>
    Public Property ID() As Guid
        Get
            Return m_id
        End Get
        Set(ByVal value As Guid)
            m_id = value
        End Set
    End Property

End Class
