Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports Tva.IO
Imports Tva.Collections
Imports Tva.DatAWare.Packets

' TODO: This code is to be moved to the DatAWare Archiver windows service.

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

        DataParser.Add(senderID, binaryImage)

    End Sub

#End Region

#Region " Private Code "

    Private Sub DataParser_DataParsed(ByVal sender As Object, ByVal e As DataParsedEventArgs) Handles DataParser.DataParsed

        For Each packet As IPacket In e.Packets
            ' Process all of the packets.
            Select Case packet.ActionType
                Case PacketActionType.SaveOnly, PacketActionType.SaveAndReply
                    Select Case packet.SaveLocation
                        Case PacketSaveLocation.ArchiveFile
                            ' Call ArchiveFile.Write()
                        Case PacketSaveLocation.MetadataFile
                            ' Call ArchiveFile.Write()
                    End Select
                Case PacketActionType.ReplyOnly
                    ' Add to the Reply Sender queue.
            End Select
        Next

    End Sub

#Region " ProcessQueue Delegates "

    Private Sub ReplyToSender(ByVal senderID As Guid, ByVal packet As IPacket)

        If packet.ActionType = PacketActionType.ReplyOnly Then

        End If

    End Sub

#End Region

#End Region

End Class
