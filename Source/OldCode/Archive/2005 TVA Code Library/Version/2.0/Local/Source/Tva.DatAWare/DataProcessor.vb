Imports System.IO
Imports System.Reflection
Imports System.ComponentModel
Imports Tva.IO
Imports Tva.Collections

Public Class DataProcessor
    Implements ISupportInitialize

#Region " Event Declaration "

    ' TODO: Make the events more useful.
    Public Event PacketProcessed()
    Public Event PacketDiscarded()

#End Region

#Region " Member Declaration "

    Private m_packetTypes As Dictionary(Of Short, PacketTypeInfo)
    Private m_toProcess As KeyedProcessQueue(Of Guid, Byte())
    Private m_toArchiveFile As KeyedProcessQueue(Of Guid, IPacket)
    Private m_toMetadataFile As KeyedProcessQueue(Of Guid, IPacket)
    Private m_toReplySender As KeyedProcessQueue(Of Guid, IPacket)

#End Region

#Region " Public Code "

    Public Sub Initialize()

        ' We'll scan all of the assemblies and keep a cached list of all available packet types.
        Dim binDirectory As String = FilePath.JustPath(Tva.Assembly.EntryAssembly.Location)
        For Each dll As String In Directory.GetFiles(binDirectory, "*.dll")
            Try
                Dim asm As Reflection.Assembly = Reflection.Assembly.LoadFrom(dll)
                For Each asmType As Type In asm.GetTypes()
                    If Not asmType.IsAbstract AndAlso _
                            asmType.GetInterface("Tva.DatAWare.IPacket", True) IsNot Nothing Then
                        ' We need to cache this type since it implements the IPacket interface.
                        Dim typeInfo As New PacketTypeInfo()

                        Dim typeID As FieldInfo = asmType.GetField("TypeID")
                        If typeID IsNot Nothing Then
                            ' This is just a safety check just in case the contant is renamed.
                            typeInfo.TypeID = Convert.ToInt16(typeID.GetValue(Nothing))
                        End If

                        Dim tryParse As MethodInfo = asmType.GetMethod("TryParse")
                        If tryParse IsNot Nothing Then
                            ' We create a delegate for the TryParse method that is to be called for parsing
                            ' raw binary data. This way we'll be making early bound calls for speed purposes
                            ' to the method that creates packets out of binary data .
                            typeInfo.TryParse = DirectCast(System.Delegate.CreateDelegate(GetType(TryParseFunctionSignature), tryParse), TryParseFunctionSignature)
                        End If

                        If Not m_packetTypes.ContainsKey(typeInfo.TypeID) Then
                            m_packetTypes.Add(typeInfo.TypeID, typeInfo)
                        End If
                    End If
                Next
            Catch ex As Exception
                ' We'll ignore exceptions encountered while processing the DLLs.
            End Try
        Next

        m_toProcess.Start()
        m_toArchiveFile.Start()
        m_toMetadataFile.Start()
        m_toReplySender.Start()

    End Sub

    Public Sub Process(ByVal senderID As Guid, ByVal binaryImage As Byte())

        m_toProcess.Add(senderID, binaryImage)

    End Sub

#End Region

#Region " Private Code "

#Region " ProcessQueue Delegates "

    Private Sub ProcessPacket(ByVal senderID As Guid, ByVal binaryImage As Byte())

        If binaryImage IsNot Nothing AndAlso binaryImage.Length >= 1 Then
            Dim typeID As Short = BitConverter.ToInt16(binaryImage, 0)
            Dim packetType As PacketTypeInfo = Nothing

            If m_packetTypes.TryGetValue(typeID, packetType) Then
                Dim packets As List(Of IPacket) = Nothing

                If packetType.TryParse(binaryImage, packets) Then
                    For Each packet As IPacket In packets
                        ' Process all the packets.
                        Select Case packet.ActionType
                            Case PacketActionType.SaveOnly, PacketActionType.SaveAndReply
                                Select Case packet.SaveLocation
                                    Case PacketSaveLocation.ArchiveFile
                                        m_toArchiveFile.Add(senderID, packet)
                                    Case PacketSaveLocation.MetadataFile
                                        m_toMetadataFile.Add(senderID, packet)
                                End Select
                            Case PacketActionType.ReplyOnly
                                m_toReplySender.Add(senderID, packet)
                        End Select
                    Next
                End If
            Else
                RaiseEvent PacketDiscarded()
            End If
        End If

    End Sub

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

#Region " PacketTypeInfo Class "

    Private Delegate Function TryParseFunctionSignature(ByVal binaryImage As Byte(), ByRef packets As List(Of IPacket)) As Boolean

    Private Class PacketTypeInfo

        Public Sub New()

            TypeID = -1

        End Sub

        Public TypeID As Short

        Public TryParse As TryParseFunctionSignature

    End Class

#End Region

#End Region

#Region " ISupportInitialize Implementation "

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If Not DesignMode Then
            Initialize()
        End If

    End Sub

#End Region

End Class
