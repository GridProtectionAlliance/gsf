Imports TVA.DatAWare.Files

Namespace Packets

    Public MustInherit Class PacketBase
        Implements IPacket, IBinaryDataConsumer

#Region " Member Declaration "

        Private m_actionType As PacketActionType
        Private m_archiveFile As ArchiveFile
        Private m_metadataFile As MetadataFile

#End Region

#Region " Public Code "

        ''' <summary>
        ''' Identifies the type of the packet.
        ''' </summary>
        Public Const TypeID As Short = -1

        ''' <summary>
        ''' This constant is required only for packet types of fixed length.
        ''' </summary>
        ''' <remarks>A value of -1 indicates that the packets is of variable length.</remarks>
        Public Const Size As Integer = -1

        Public Sub New(ByVal actionType As PacketActionType)

            MyBase.New()
            m_actionType = actionType

        End Sub

#Region " Interface Implementation "

#Region " IPacket "

        Public Property ArchiveFile() As ArchiveFile Implements IPacket.ArchiveFile
            Get
                Return m_archiveFile
            End Get
            Set(ByVal value As ArchiveFile)
                m_archiveFile = value
            End Set
        End Property

        Public Property MetadataFile() As MetadataFile Implements IPacket.MetadataFile
            Get
                Return m_metadataFile
            End Get
            Set(ByVal value As MetadataFile)
                m_metadataFile = value
            End Set
        End Property

        Public Property ActionType() As PacketActionType Implements IPacket.ActionType
            Get
                Return m_actionType
            End Get
            Set(ByVal value As PacketActionType)
                m_actionType = value
            End Set
        End Property

        Public MustOverride ReadOnly Property ReplyData() As Byte() Implements IPacket.ReplyData

        Public MustOverride Sub SaveData() Implements IPacket.SaveData

#End Region

#Region " IBinaryDataConsumer "

        Public Function Initialize(ByVal binaryImage() As Byte) As Integer Implements IBinaryDataConsumer.Initialize

            Return Initialize(binaryImage, 0)

        End Function

        Public MustOverride Function Initialize(ByVal binaryImage() As Byte, ByVal startIndex As Integer) As Integer Implements IBinaryDataConsumer.Initialize

#End Region

#End Region

#End Region

    End Class

End Namespace