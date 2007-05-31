Imports System.Text
Imports TVA.Measurements

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

        Public Sub New(ByVal measurement As IMeasurement)

            MyClass.New()
            m_pointID = measurement.ID
            m_timeTag = New TimeTag(measurement.Timestamp)
            m_value = Convert.ToSingle(measurement.AdjustedValue)
            m_quality = IIf(measurement.TimestampQualityIsGood And measurement.ValueQualityIsGood, Quality.Good, Quality.SuspectData)

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

#Region " Overrides "

        Public Overrides ReadOnly Property BinaryImage() As Byte()
            Get
                Dim image As Byte() = CreateArray(Of Byte)(Size)

                Array.Copy(BitConverter.GetBytes(TypeID), 0, image, 0, 2)
                Array.Copy(BitConverter.GetBytes(m_pointID), 0, image, 2, 4)
                Array.Copy(BitConverter.GetBytes(m_timeTag.Value), 0, image, 6, 8)
                Array.Copy(BitConverter.GetBytes(m_quality), 0, image, 14, 4)
                Array.Copy(BitConverter.GetBytes(m_value), 0, image, 18, 4)

                Return image
            End Get
        End Property

        Public Overrides ReadOnly Property BinaryLength() As Integer
            Get
                Return Size
            End Get
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

                    ' We'll send an "ACK" to the sender if this is the last packet in the transmission.
                    If startIndex + Size = binaryImage.Length Then ActionType = PacketActionType.SaveAndReply

                    Return Size
                Else
                    Throw New ArgumentException(String.Format("Unexpected packet type ID {0}. Expected packet type ID {1}.", packetTypeID, TypeID))
                End If
            Else
                Throw New ArgumentException(String.Format("Binary image smaller than expected. Expected binary image size {0}.", Size))
            End If

        End Function

#End Region

    End Class

End Namespace