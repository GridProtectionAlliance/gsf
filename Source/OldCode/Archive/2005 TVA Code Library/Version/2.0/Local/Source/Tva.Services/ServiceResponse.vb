' 08-29-06

Imports Tva.Text.Common

<Serializable()> _
Public Class ServiceResponse

    Private m_type As String
    Private m_message As String
    Private m_attachments As List(Of Object)

    Public Sub New()
        MyClass.New("UNDETERMINED")
    End Sub

    Public Sub New(ByVal type As String)
        MyClass.New(type, "")
    End Sub

    Public Sub New(ByVal type As String, ByVal message As String)
        m_type = type
        m_message = message
        m_attachments = New List(Of Object)
    End Sub

    ' Standard Response Type
    '    Undetermined
    '    UnicastMessage
    '    MulticastMessage
    '    ServiceStateChanged
    '    ProcessStateChanged
    '    ProcessProgressUpdate
    Public Property Type() As String
        Get
            Return m_type
        End Get
        Set(ByVal value As String)
            m_type = value
        End Set
    End Property

    Public Property Message() As String
        Get
            Return m_message
        End Get
        Set(ByVal value As String)
            m_message = value
        End Set
    End Property

    Public ReadOnly Property Attachments() As List(Of Object)
        Get
            Return m_attachments
        End Get
    End Property

End Class