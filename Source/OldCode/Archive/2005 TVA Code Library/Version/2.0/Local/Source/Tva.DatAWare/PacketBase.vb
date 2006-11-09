Public MustInherit Class PacketBase
    Implements IPacket

    Private m_typeID As Short
    Private m_actiontype As PacketActionType
    Private m_saveLocation As PacketSaveLocation
    Private m_items As Dictionary(Of String, Object)

    ''' <summary>
    ''' Identifies the type of the packet.
    ''' </summary>
    Public Const TypeID As Short = -1

    ''' <summary>
    ''' This constant is required only for packet types of fixed length.
    ''' </summary>
    ''' <remarks>A value of -1 indicates that the packets can be of variable length.</remarks>
    Public Const BinaryLength As Integer = -1

    Public Sub New()

        MyBase.New()
        m_items = New Dictionary(Of String, Object)()

    End Sub

    Public Property ActionType() As PacketActionType Implements IPacket.ActionType
        Get
            Return m_actiontype
        End Get
        Set(ByVal value As PacketActionType)
            m_actiontype = value
        End Set
    End Property

    Public Property SaveLocation() As PacketSaveLocation Implements IPacket.SaveLocation
        Get
            Return m_saveLocation
        End Get
        Set(ByVal value As PacketSaveLocation)
            m_saveLocation = value
        End Set
    End Property

    Public ReadOnly Property Items() As System.Collections.Generic.Dictionary(Of String, Object) Implements IPacket.Items
        Get
            Return m_items
        End Get
    End Property

    Public MustOverride Function GetSaveData() As Byte() Implements IPacket.GetSaveData

    Public MustOverride Function GetReplyData() As Byte() Implements IPacket.GetReplyData

    Public Shared Function TryParse(ByVal binaryImage As Byte(), ByRef packets As List(Of IPacket)) As Boolean

        Throw New NotImplementedException("This static method has not been implemented by the derived class.")

    End Function

End Class
