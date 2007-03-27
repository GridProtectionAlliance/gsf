' 03/12/2007

Public Class IdentifiableItem(Of T)

    Private m_source As Guid
    Private m_item As T

    Public Sub New(ByVal item As T)

        MyClass.New(Guid.Empty, item)

    End Sub

    Public Sub New(ByVal source As Guid, ByVal item As T)

        MyBase.New()
        m_source = source
        m_item = item

    End Sub

    Public Property Source() As Guid
        Get
            Return m_source
        End Get
        Set(ByVal value As Guid)
            m_source = value
        End Set
    End Property

    Public Property Item() As T
        Get
            Return m_item
        End Get
        Set(ByVal value As T)
            m_item = value
        End Set
    End Property

End Class
