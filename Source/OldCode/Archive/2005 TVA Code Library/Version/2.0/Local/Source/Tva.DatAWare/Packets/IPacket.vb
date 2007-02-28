Namespace Packets

    Public Interface IPacket

        Property ArchiveFile() As ArchiveFile

        Property MetadataFile() As MetadataFile

        ReadOnly Property ActionType() As PacketActionType

        ReadOnly Property ReplyData() As Byte()

        Sub SaveData()

    End Interface

End Namespace