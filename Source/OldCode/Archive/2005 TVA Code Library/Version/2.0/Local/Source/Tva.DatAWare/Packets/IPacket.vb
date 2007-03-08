Namespace Packets

    Public Interface IPacket

        Property ArchiveFile() As Files.ArchiveFile

        Property MetadataFile() As Files.MetadataFile

        ReadOnly Property ActionType() As PacketActionType

        ReadOnly Property ReplyData() As Byte()

        Sub SaveData()

    End Interface

End Namespace