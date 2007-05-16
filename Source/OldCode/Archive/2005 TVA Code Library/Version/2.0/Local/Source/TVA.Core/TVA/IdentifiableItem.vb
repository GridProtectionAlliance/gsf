' 03/12/2007

Public Class IdentifiableItem(Of TIdentifier, TItem)

    Private m_source As TIdentifier
    Private m_item As TItem

    Public Sub New(ByVal source As TIdentifier, ByVal item As TItem)

        MyBase.New()
        m_source = source
        m_item = item

    End Sub

    Public Property Source() As TIdentifier
        Get
            Return m_source
        End Get
        Set(ByVal value As TIdentifier)
            m_source = value
        End Set
    End Property

    Public Property Item() As TItem
        Get
            Return m_item
        End Get
        Set(ByVal value As TItem)
            m_item = value
        End Set
    End Property

End Class
