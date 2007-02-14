Imports System.Text

Namespace Packets

    Public Class StandardPacket
        Inherits PacketBase

        ' *******************************************************
        ' *                 Packet structure                    *
        ' *******************************************************
        ' * # Of Bytes  Byte Index  Data Type   Description     *
        ' * ----------  ----------  ----------  -----------     *
        ' * 2           0-1         Int16       Packet ID       *
        ' * 4           2-5         Int32       Database index  *
        ' * 8           6-13        Double      Time-tag        *
        ' * 4           14-17       Int32       Quality         *
        ' * 4           18-21       Single      Value           *
        ' *******************************************************

        Public Shadows Const TypeID As Short = 1

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
                        .Add("TimeTag", BitConverter.ToDouble(binaryImage, startIndex + 6))
                        .Add("Quality", BitConverter.ToInt32(binaryImage, startIndex + 14))
                        .Add("Value", BitConverter.ToSingle(binaryImage, startIndex + 18))
                    End With
                Else
                    Throw New ArgumentException(String.Format("Unexpected packet type ID {0}. Expected packet type ID {1}", packetTypeID, TypeID))
                End If
            Else
                Throw New ArgumentException(String.Format("Binary image smaller than expected. Expected binary image size {0}", BinaryLength))
            End If

        End Sub

        Public Sub New(ByVal index As Integer, ByVal timeTag As Double, ByVal quality As Integer, _
                ByVal value As Single)

            MyBase.New()
            With MyBase.Items
                .Add("TypeID", StandardPacket.TypeID)
                .Add("Index", index)
                .Add("TimeTag", timeTag)
                .Add("Quality", quality)
                .Add("Value", value)
            End With

        End Sub

        Public Overrides Function GetReplyData() As Byte()

            Return Encoding.ASCII.GetBytes("ACK")

        End Function

        Public Overrides Function GetSaveData() As Byte()

            Return New PointData(GetItemValue(Of Double)("TimeTag"), GetItemValue(Of Single)("Value"), GetItemValue(Of Integer)("Quality")).BinaryImage

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