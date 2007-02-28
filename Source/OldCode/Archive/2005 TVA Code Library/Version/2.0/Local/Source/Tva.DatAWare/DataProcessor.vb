Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports Tva.IO
Imports Tva.Collections
Imports Tva.DatAWare.Packets

' TODO: This code is to be moved to the DatAWare Archiver windows service.

Public Class DataProcessor

#Region " Member Declaration "

    Private m_saveQueue As KeyedProcessQueue(Of Guid, IPacket)
    Private m_replyQueue As KeyedProcessQueue(Of Guid, IPacket)

#End Region

#Region " Event Declaration "

    ' TODO: Make the events more useful.
    Public Event PacketProcessed()
    Public Event PacketDiscarded()

#End Region

#Region " Public Code "

    Public Sub Process(ByVal senderID As Guid, ByVal binaryImage As Byte())

        DataParser.Add(senderID, binaryImage)

    End Sub

#End Region

#Region " Private Code "

    Private Sub DataParser_DataParsed(ByVal sender As Object, ByVal e As DataParsedEventArgs) Handles DataParser.DataParsed

        For i As Integer = 0 To e.Packets.Count - 1
            ' Process all of the packets.
            e.Packets(i).ArchiveFile = Nothing  ' <- Set the ArchiveFile
            e.Packets(i).MetadataFile = Nothing ' <- Set the MetadataFile

            Select Case e.Packets(i).ActionType
                Case PacketActionType.SaveOnly, PacketActionType.SaveAndReply
                    m_saveQueue.Add(e.Source, e.Packets(i))
                Case PacketActionType.ReplyOnly
                    m_replyQueue.Add(e.Source, e.Packets(i))
            End Select
        Next

    End Sub

#Region " ProcessQueue Delegates "

    Private Sub SaveToFile(ByVal senderID As Guid, ByVal packet As IPacket)

        ' Call the packet's SaveData() method...
        packet.SaveData()

        If packet.ActionType = PacketActionType.SaveAndReply Then
            m_replyQueue.Add(senderID, packet)
        End If

    End Sub

    Private Sub ReplyToSender(ByVal senderID As Guid, ByVal packet As IPacket)

        ' Send the reply data to the sender...
        ' TcpServer.Send(senderID, packet.ReplyData)

    End Sub

#End Region

#End Region

End Class
