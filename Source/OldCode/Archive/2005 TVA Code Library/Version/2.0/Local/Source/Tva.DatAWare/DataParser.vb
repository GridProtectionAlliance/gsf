Imports System.IO
Imports System.Drawing
Imports System.Reflection
Imports System.ComponentModel
Imports Tva.IO
Imports Tva.Collections
Imports Tva.DatAWare.Packets

<ToolboxBitmap(GetType(DataParser)), DefaultEvent("DataParsed")> _
Public Class DataParser

#Region " Member Declaration "

    Private m_processed As Integer
    Private m_packetTypes As Dictionary(Of Short, PacketTypeInfo)
    Private WithEvents m_dataQueue As ProcessQueue(Of IdentifiableItem(Of Byte()))

#End Region

#Region " Event Declaration "

    Public Event DataParsed As EventHandler(Of DataParsedEventArgs)
    Public Event DataDiscarded As EventHandler(Of IdentifiableItemEventArgs(Of Byte()))

#End Region

#Region " Public Code "

    Public ReadOnly Property ParsedDataCount() As Long
        Get
            Return m_dataQueue.TotalProcessedItems
        End Get
    End Property

    Public ReadOnly Property UnparsedDataCount() As Integer
        Get
            Return m_dataQueue.Count
        End Get
    End Property

    Public Sub Start()

        ' We'll scan all of the assemblies and keep a cached list of all available packet types.
        Dim binDirectory As String = FilePath.JustPath(Tva.Assembly.EntryAssembly.Location)
        For Each dll As String In Directory.GetFiles(binDirectory, "*.dll")
            Try
                Dim asm As Reflection.Assembly = Reflection.Assembly.LoadFrom(dll)
                For Each asmType As Type In asm.GetTypes()
                    If Not asmType.IsAbstract AndAlso _
                            asmType.GetInterface("Tva.DatAWare.Packets.IPacket", True) IsNot Nothing Then
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
                            ' to the method that creates packets out of binary data.
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

        m_dataQueue.Start()

    End Sub

    Public Sub [Stop]()

        m_dataQueue.Stop()      ' Stop processing of queued data.
        m_packetTypes.Clear()   ' Clear the cached packet type available.

    End Sub

    Public Sub Finish()

        m_dataQueue.Flush()     ' Make sure all of the data is parsed.

    End Sub

    Public Sub Add(ByVal source As Guid, ByVal data As Byte())

        m_dataQueue.Add(New IdentifiableItem(Of Byte())(source, data))

    End Sub

#End Region

#Region " Private Code"

    Private Sub ParseData(ByVal data As IdentifiableItem(Of Byte()))

        If data.Item IsNot Nothing AndAlso data.Item.Length >= 1 Then
            ' We have data that can now be parsed.
            Dim typeID As Short = BitConverter.ToInt16(data.Item, 0)
            Dim packetType As PacketTypeInfo = Nothing

            If m_packetTypes.TryGetValue(typeID, packetType) Then
                '  The target packet type implements a TryParse function that could be called to parse a binary
                ' image to a list of packets. 
                Dim packets As List(Of IPacket) = Nothing

                If packetType.TryParse(data.Item, packets) Then
                    ' Data could be parsed and was converted to packets.
                    RaiseEvent DataParsed(Me, New DataParsedEventArgs(data.Source, packets))
                Else
                    ' Data could not be parsed for some reason.
                    RaiseEvent DataDiscarded(Me, New IdentifiableItemEventArgs(Of Byte())(data.Source, data.Item))
                End If
            Else
                RaiseEvent DataDiscarded(Me, New IdentifiableItemEventArgs(Of Byte())(data.Source, data.Item))
            End If
        End If

    End Sub

#Region " PacketTypeInfo Class "

    Private Delegate Function TryParseFunctionSignature(ByVal binaryImage As Byte(), ByRef packets As List(Of IPacket)) As Boolean

    Private Class PacketTypeInfo

        Public Sub New()

            Me.TypeID = -1

        End Sub

        Public TypeID As Short

        Public TryParse As TryParseFunctionSignature

    End Class

#End Region

#End Region

End Class
