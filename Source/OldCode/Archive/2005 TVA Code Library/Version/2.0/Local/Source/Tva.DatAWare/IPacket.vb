Public Interface IPacket

    Property TypeID() As Short

    Property ActionType() As PacketActionType

    Property SaveLocation() As PacketSaveLocation

    ReadOnly Property Items() As Dictionary(Of String, Object)

    Function GetSaveData() As Byte()

    Function GetReplyData() As Byte()

End Interface
