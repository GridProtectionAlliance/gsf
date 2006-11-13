Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports Tva.IO
Imports Tva.Collections
Imports Tva.DatAWare.Packets

Public Class DataProcessor

#Region " Event Declaration "

    ' TODO: Make the events more useful.
    Public Event PacketProcessed()
    Public Event PacketDiscarded()

#End Region

#Region " Member Declaration "

    Private m_toReplySender As KeyedProcessQueue(Of Guid, IPacket)

#End Region

#Region " Public Code "

    Public Sub Process(ByVal senderID As Guid, ByVal binaryImage As Byte())

        DataParser.Parse(senderID, binaryImage)

    End Sub

#End Region

#Region " Private Code "

    Private Sub DataParser_DataParsed(ByVal source As System.Guid, ByVal packets As System.Collections.Generic.List(Of IPacket)) Handles DataParser.DataParsed

        For Each packet As IPacket In packets
            ' Process all of the packets.
            Select Case packet.ActionType
                Case PacketActionType.SaveOnly, PacketActionType.SaveAndReply
                    Select Case packet.SaveLocation
                        Case PacketSaveLocation.ArchiveFile

                        Case PacketSaveLocation.MetadataFile

                    End Select
                Case PacketActionType.ReplyOnly

            End Select
        Next

    End Sub

#Region " ProcessQueue Delegates "

    Private Sub SaveToArchiveFile(ByVal senderID As Guid, ByVal packet As IPacket)

        If packet.SaveLocation = PacketSaveLocation.ArchiveFile Then

        End If

    End Sub

    Private Sub SaveToMetadataFile(ByVal senderID As Guid, ByVal packet As IPacket)

        If packet.SaveLocation = PacketSaveLocation.MetadataFile Then

        End If

    End Sub

    Private Sub ReplyToSender(ByVal senderID As Guid, ByVal packet As IPacket)

        If packet.ActionType = PacketActionType.ReplyOnly Then

        End If

    End Sub

#End Region

#End Region

End Class
