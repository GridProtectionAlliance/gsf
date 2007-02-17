Namespace Packets

    Public Interface IPacket

        Property ActionType() As PacketActionType

        Property SaveLocation() As PacketSaveLocation

        Function GetSaveData() As Byte()

        Function GetReplyData() As Byte()

    End Interface

End Namespace