' 11/22/2006

Imports TVA.DatAWare.Packets

Public Class DataParsedEventArgs
    Inherits EventArgs

    Private m_source As Guid
    Private m_packets As List(Of IPacket)

    Public Sub New()

        MyClass.New(Guid.Empty, New List(Of IPacket)())

    End Sub

    Public Sub New(ByVal packets As List(Of IPacket))

        MyClass.New(Guid.Empty, packets)

    End Sub

    Public Sub New(ByVal source As Guid, ByVal packets As List(Of Packets.IPacket))

        m_source = source
        m_packets = packets

    End Sub

    Public Property Source() As Guid
        Get
            Return m_source
        End Get
        Set(ByVal value As Guid)
            m_source = value
        End Set
    End Property

    Public ReadOnly Property Packets() As List(Of IPacket)
        Get
            Return m_packets
        End Get
    End Property

End Class
