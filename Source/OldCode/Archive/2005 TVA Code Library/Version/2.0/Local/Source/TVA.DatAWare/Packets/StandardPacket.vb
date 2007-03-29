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

#Region " Member Declaration "

        Private m_pointID As Integer
        Private m_timeTag As TimeTag
        Private m_quality As Quality
        Private m_value As Single

#End Region

        Public Shadows Const TypeID As Short = 1

        Public Shadows Const Size As Integer = 22

        Public Sub New()

            MyBase.New(PacketActionType.SaveOnly)

        End Sub

        Public Sub New(ByVal binaryImage As Byte())

            MyClass.New(binaryImage, 0)

        End Sub

        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyClass.New()

            If binaryImage.Length - startIndex >= Size Then
                ' We have a binary image of valid size.
                Dim packetTypeID As Short = BitConverter.ToInt16(binaryImage, startIndex)
                If packetTypeID = TypeID Then
                    ' We have a binary image with the correct type ID.
                    m_pointID = BitConverter.ToInt32(binaryImage, startIndex + 2)
                    m_timeTag = New TimeTag(BitConverter.ToDouble(binaryImage, startIndex + 6))
                    m_quality = CType(BitConverter.ToInt32(binaryImage, startIndex + 14), Quality)
                    m_value = BitConverter.ToSingle(binaryImage, startIndex + 18)
                Else
                    Throw New ArgumentException(String.Format("Unexpected packet type ID {0}. Expected packet type ID {1}.", packetTypeID, TypeID))
                End If
            Else
                Throw New ArgumentException(String.Format("Binary image smaller than expected. Expected binary image size {0}.", Size))
            End If

        End Sub

        Public Property PointID() As Integer
            Get
                Return m_pointID
            End Get
            Set(ByVal value As Integer)
                m_pointID = value
            End Set
        End Property

        Public Property TimeTag() As TimeTag
            Get
                Return m_timeTag
            End Get
            Set(ByVal value As TimeTag)
                m_timeTag = value
            End Set
        End Property

        Public Property Quality() As Quality
            Get
                Return m_quality
            End Get
            Set(ByVal value As Quality)
                m_quality = value
            End Set
        End Property

        Public Property Value() As Single
            Get
                Return m_value
            End Get
            Set(ByVal value As Single)
                m_value = value
            End Set
        End Property

        Public Overrides ReadOnly Property ReplyData() As Byte()
            Get
                Return Encoding.Default.GetBytes("ACK")
            End Get
        End Property

        Public Overrides Sub SaveData()

            If ArchiveFile IsNot Nothing AndAlso MetadataFile IsNot Nothing Then
                Dim pointData As New StandardPointData(m_timeTag, m_value, m_quality)
                pointData.Definition = MetadataFile.Read(m_pointID)

                ArchiveFile.Write(pointData)
            End If

        End Sub

        Public Shared Shadows Function TryParse(ByVal binaryImage As Byte(), ByRef packets As List(Of IPacket)) As Boolean

            If binaryImage IsNot Nothing Then
                If binaryImage.Length >= Size Then
                    packets = New List(Of IPacket)()
                    For i As Integer = 0 To binaryImage.Length - 1 Step Size
                        Try
                            packets.Add(New StandardPacket(binaryImage, i))
                        Catch ex As Exception
                            ' Its likely that we encounter an exception here if the entire buffer is not well formed.
                        End Try
                    Next
                    packets(packets.Count - 1).ActionType = PacketActionType.SaveAndReply

                    Return True
                End If
            End If

            Return False

        End Function

        Public Overloads Overrides Function Initialize(ByVal binaryImage() As Byte, ByVal startIndex As Integer) As Integer

            If binaryImage.Length - startIndex >= Size Then
                ' We have a binary image of valid size.
                Dim packetTypeID As Short = BitConverter.ToInt16(binaryImage, startIndex)
                If packetTypeID = TypeID Then
                    ' We have a binary image with the correct type ID.
                    m_pointID = BitConverter.ToInt32(binaryImage, startIndex + 2)
                    m_timeTag = New TimeTag(BitConverter.ToDouble(binaryImage, startIndex + 6))
                    m_quality = CType(BitConverter.ToInt32(binaryImage, startIndex + 14), Quality)
                    m_value = BitConverter.ToSingle(binaryImage, startIndex + 18)

                    If startIndex + Size = binaryImage.Length Then ActionType = PacketActionType.SaveAndReply

                    Return Size
                Else
                    Throw New ArgumentException(String.Format("Unexpected packet type ID {0}. Expected packet type ID {1}.", packetTypeID, TypeID))
                End If
            Else
                Throw New ArgumentException(String.Format("Binary image smaller than expected. Expected binary image size {0}.", Size))
            End If

        End Function

    End Class

End Namespace