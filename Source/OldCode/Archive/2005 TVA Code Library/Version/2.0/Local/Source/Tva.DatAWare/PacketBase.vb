Public MustInherit Class PacketBase
    Implements IPacket

    Private m_typeID As Short
    Private m_actiontype As PacketActionType
    Private m_saveLocation As PacketSaveLocation
    Private m_items As Dictionary(Of String, Object)

    ''' <summary>
    ''' This constant is required only for packet types of fixed length.
    ''' </summary>
    ''' <remarks>A value of -1 indicates that the packets can be of variable length.</remarks>
    Public Const BinaryImageLength As Integer = -1

    Public Sub New()

        MyBase.New()
        m_items = New Dictionary(Of String, Object)()

    End Sub

    Public Property TypeID() As Short Implements IPacket.TypeID
        Get
            Return m_typeID
        End Get
        Set(ByVal value As Short)
            If value >= 0 Then
                m_typeID = value
            Else
                Throw New ArgumentOutOfRangeException("TypeID")
            End If
        End Set
    End Property

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

End Class
