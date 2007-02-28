Namespace Packets

    Public MustInherit Class PacketBase
        Implements IPacket

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
        Public Const BinaryLength As Integer = -1

        Public Sub New(ByVal actionType As PacketActionType)

            MyBase.New()
            m_actionType = actionType

        End Sub

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

        Public ReadOnly Property ActionType() As PacketActionType Implements IPacket.ActionType
            Get
                Return m_actionType
            End Get
        End Property

        Public MustOverride ReadOnly Property ReplyData() As Byte() Implements IPacket.ReplyData

        Public MustOverride Sub SaveData() Implements IPacket.SaveData

        Public Shared Function TryParse(ByVal binaryImage As Byte(), ByRef packets As List(Of IPacket)) As Boolean

            Throw New NotImplementedException("This static method has not been implemented by the derived class.")

        End Function

#End Region

    End Class

End Namespace