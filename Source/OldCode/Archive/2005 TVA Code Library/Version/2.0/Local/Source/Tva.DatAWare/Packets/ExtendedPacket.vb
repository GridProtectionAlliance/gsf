Imports System.Text

Namespace Packets

    Public Class ExtendedPacket
        Inherits PacketBase

        ' *******************************************************
        ' *                 Packet structure                    *
        ' *******************************************************
        ' * # Of Bytes  Byte Index  Data Type   Description     *
        ' * ----------  ----------  ----------  -----------     *
        ' * 2           0-1         Int16       Packet ID       *
        ' * 4           2-5         Int32       Database index  *
        ' * 2           6-7         Int16       Year            *
        ' * 1           8           Byte        Month           *
        ' * 1           9           Byte        Day             *
        ' * 1           10          Byte        Hour            *
        ' * 1           11          Byte        Minute          *
        ' * 1           12          Byte        Second          *
        ' * 1           13          Byte        Quality         *
        ' * 2           14-15       Int16       Msec            *
        ' * 2           16-17       Int16       GMT Offset      *
        ' * 4           18-21       Int32       Value           *
        ' *******************************************************

        Public Shadows Const TypeID As Short = 2

        Public Shadows Const BinaryLength As Integer = 22

        Public Sub New(ByVal binaryImage As Byte())

            MyClass.New(binaryImage, 0)

        End Sub

        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New()

            If (binaryImage.Length - startIndex) >= BinaryLength Then
                ' We have a binary image of valid size.
                Dim packetTypeID As Short = BitConverter.ToInt16(binaryImage, startIndex)
                If packetTypeID = TypeID Then
                    ' We have a binary image with the correct type ID.
                    MyBase.ActionType = PacketActionType.SaveAndReply
                    MyBase.SaveLocation = PacketSaveLocation.ArchiveFile
                    With MyBase.Items
                        .Add("TypeID", packetTypeID)
                        .Add("Index", BitConverter.ToInt32(binaryImage, startIndex + 2))
                        .Add("Year", BitConverter.ToInt16(binaryImage, startIndex + 6))
                        .Add("Month", binaryImage(startIndex + 8))
                        .Add("Day", binaryImage(startIndex + 9))
                        .Add("Hour", binaryImage(startIndex + 10))
                        .Add("Minute", binaryImage(startIndex + 11))
                        .Add("Second", binaryImage(startIndex + 12))
                        .Add("Quality", binaryImage(startIndex + 13))
                        .Add("Millisecond", BitConverter.ToInt16(binaryImage, startIndex + 14))
                        .Add("GMTOffset", BitConverter.ToInt16(binaryImage, startIndex + 16))
                        .Add("Value", BitConverter.ToSingle(binaryImage, startIndex + 18))
                    End With
                Else
                    Throw New ArgumentException(String.Format("Unexpected packet type ID {0}. Expected packet type ID {1}", packetTypeID, TypeID))
                End If
            Else
                Throw New ArgumentException(String.Format("Binary image smaller than expected. Expected binary image size {0}", BinaryLength))
            End If

        End Sub

        Public Sub New(ByVal index As Integer, ByVal year As Short, ByVal month As Byte, ByVal day As Byte, _
                ByVal hour As Byte, ByVal minute As Byte, ByVal second As Byte, ByVal quality As Byte, _
                ByVal millisecond As Short, ByVal gmtOffset As Short, ByVal value As Single)

            MyBase.New()
            With MyBase.Items
                .Add("TypeID", ExtendedPacket.TypeID)
                .Add("Index", index)
                .Add("Year", year)
                .Add("Month", month)
                .Add("Day", day)
                .Add("Hour", hour)
                .Add("Minute", minute)
                .Add("Second", second)
                .Add("Quality", quality)
                .Add("Millisecond", millisecond)
                .Add("GMTOffset", gmtOffset)
                .Add("Value", value)
            End With

        End Sub

        Public Overrides Function GetReplyData() As Byte()

            Return Encoding.ASCII.GetBytes("ACK")

        End Function

        Public Overrides Function GetSaveData() As Byte()

            ' HACK: Check with Ritchie
            Dim timestamp As New System.DateTime(GetItemValue(Of Integer)("Year"), _
                GetItemValue(Of Integer)("Month"), GetItemValue(Of Integer)("Day"), _
                (GetItemValue(Of Integer)("Hour") + GetItemValue(Of Integer)("GMTOffset")), _
                GetItemValue(Of Integer)("Minute"), GetItemValue(Of Integer)("Second"), _
                GetItemValue(Of Integer)("Millisecond"), DateTimeKind.Utc)

            Return New DataPoint(timestamp, GetItemValue(Of Single)("Value"), GetItemValue(Of Integer)("Quality")).BinaryImage

        End Function

        Public Shared Shadows Function TryParse(ByVal binaryImage As Byte(), ByRef packets As List(Of IPacket)) As Boolean

            If binaryImage IsNot Nothing Then
                If binaryImage.Length >= BinaryLength Then
                    packets = New List(Of IPacket)()
                    For i As Integer = 0 To binaryImage.Length - 1 Step BinaryLength
                        Try
                            packets.Add(New StandardPacket(binaryImage, i))
                        Catch ex As Exception
                            ' Its likely that we encounter an exception here if the entire buffer is not well formed.
                        End Try
                    Next
                    Return True
                End If
            End If

            Return False

        End Function

    End Class

End Namespace