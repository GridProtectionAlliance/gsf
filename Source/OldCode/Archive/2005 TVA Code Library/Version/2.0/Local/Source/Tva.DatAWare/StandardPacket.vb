Public Class StandardPacket
    Inherits PacketBase

    ' ***********************************************
    ' *             Packet structure                *
    ' ***********************************************
    ' * # Of Bytes    Byte Index  Description       *
    ' * ---------     ----------  -----------       *
    ' * 2             0-1         Packet ID         *
    ' * 4             2-5         Database index    *
    ' * 16            6-21        Data point        *
    ' ***********************************************

    Public Shadows Const BinaryImageLength As Integer = 22

    Public Sub New(ByVal binaryImage As Byte())

        MyClass.New(binaryImage, 0)

    End Sub

    Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyBase.New()
        MyBase.TypeID = 1
        MyBase.ActionType = PacketActionType.SaveAndReply
        MyBase.SaveLocation = PacketSaveLocation.ArchiveFile

        Dim packetTypeID As Short = BitConverter.ToInt16(binaryImage, startIndex)
        If packetTypeID = MyBase.TypeID Then
            If binaryImage.Length - startIndex >= BinaryImageLength Then
                ' We have a binary image of a valid size.
                With Me.Items
                    .Add("Index", BitConverter.ToInt32(binaryImage, startIndex + 2))
                    .Add("DataPoint", New DataPoint(binaryImage, startIndex + 6))
                End With
            Else
                Throw New ArgumentException(String.Format("Binary image smaller than expected. Expected binary image size {0}", BinaryImageLength))
            End If
        Else
            Throw New ArgumentException(String.Format("Unexpected packet type ID {0}. Expected packet type ID {1}", packetTypeID, MyBase.TypeID))
        End If

    End Sub

    Public Overrides Function GetReplyData() As Byte()

        Throw New NotImplementedException()

    End Function

    Public Overrides Function GetSaveData() As Byte()

        Throw New NotImplementedException()

    End Function

End Class