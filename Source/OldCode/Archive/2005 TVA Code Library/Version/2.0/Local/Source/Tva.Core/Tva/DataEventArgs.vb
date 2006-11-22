' 11/22/2006

Public Class DataEventArgs
    Inherits EventArgs

    Private m_source As Guid
    Private m_data As Byte()

    ''' <summary>
    ''' Initializes a new instance of the Tva.DataEventArgs class.
    ''' </summary>
    ''' <param name="data">The data that is being transferred.</param>
    Public Sub New(ByVal data As Byte())

        MyClass.New(Guid.Empty, data)

    End Sub

    ''' <summary>
    ''' Initializes a new instance of the Tva.DataEventArgs class.
    ''' </summary>
    ''' <param name="source">The source of the data.</param>
    ''' <param name="data">The data that is being transferred.</param>
    Public Sub New(ByVal source As Guid, ByVal data As Byte())

        MyBase.New()
        m_data = data
        m_source = source

    End Sub

    ''' <summary>
    ''' Gets or sets the source of the data.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The source of the data.</returns>
    Public Property Source() As Guid
        Get
            Return m_source
        End Get
        Set(ByVal value As Guid)
            m_source = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the data that is being transferred.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The data that is being transferred.</returns>
    Public Property Data() As Byte()
        Get
            Return m_data
        End Get
        Set(ByVal value As Byte())
            m_data = value
        End Set
    End Property

End Class
