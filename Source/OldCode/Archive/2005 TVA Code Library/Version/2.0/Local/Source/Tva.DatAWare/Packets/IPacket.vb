Namespace Packets

    Public Interface IPacket

        Property ActionType() As PacketActionType

        Property SaveLocation() As PacketSaveLocation

        ReadOnly Property Items() As Dictionary(Of String, Object)

        Function GetItemValue(Of T)(ByVal item As String) As T

        Function GetSaveData() As Byte()

        Function GetReplyData() As Byte()

    End Interface

End Namespace