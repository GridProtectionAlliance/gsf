' 03/28/2007

Option Strict On

Imports System.IO
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.ComponentModel
Imports TVA.Collections
Imports TVA.IO.FilePath

Namespace Components

    Public MustInherit Class BinaryDataParserBase(Of T As IBinaryDataConsumer)

#Region " Member Declaration "

        Private m_idFieldName As String
        Private m_idValueLocation As Integer
        Private m_optimizeParsing As Boolean
        Private m_parserTypes As Dictionary(Of Short, ParserTypeInfo)

        Private Delegate Function DefaultConstructor() As T

        Private WithEvents m_dataQueue As ProcessQueue(Of IdentifiableItem(Of Byte()))

#End Region

#Region " Event Declaration "

        Public Event DataParsed As EventHandler(Of IdentifiableItemEventArgs(Of List(Of T)))
        Public Event DataDiscarded As EventHandler(Of IdentifiableItemEventArgs(Of Byte()))

#End Region

#Region " Code Scope: Public "

        Public Const InterfaceName As String = "TVA.IBinaryDataConsumer"

        Public Property IDFieldName() As String
            Get
                Return m_idFieldName
            End Get
            Set(ByVal value As String)
                m_idFieldName = value
            End Set
        End Property

        Public Property IDValueLocation() As Integer
            Get
                Return m_idValueLocation
            End Get
            Set(ByVal value As Integer)
                m_idValueLocation = value
            End Set
        End Property

        Public Property OptimizeParsing() As Boolean
            Get
                Return m_optimizeParsing
            End Get
            Set(ByVal value As Boolean)
                m_optimizeParsing = value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ParsedDataCount() As Long
            Get
                Return m_dataQueue.TotalProcessedItems
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property UnparsedDataCount() As Integer
            Get
                Return m_dataQueue.Count
            End Get
        End Property

        Public Sub Start()

            Dim asm As Reflection.Assembly = Nothing
            Dim idField As FieldInfo = Nothing
            Dim typeCtor As ConstructorInfo = Nothing
            Dim asmBuilder As AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(New AssemblyName("InMemory"), AssemblyBuilderAccess.Run)
            Dim modBuilder As ModuleBuilder = asmBuilder.DefineDynamicModule("Helper")
            Dim typeBuilder As TypeBuilder = modBuilder.DefineType("ClassFactory")
            For Each dll As String In Directory.GetFiles(JustPath(TVA.Assembly.EntryAssembly.Location), "*.dll")
                asm = Reflection.Assembly.LoadFrom(dll)
                For Each asmType As Type In asm.GetExportedTypes()
                    idField = asmType.GetField(m_idFieldName)
                    typeCtor = asmType.GetConstructor(Type.EmptyTypes)
                    If idField IsNot Nothing AndAlso typeCtor IsNot Nothing AndAlso _
                            Not asmType.IsAbstract AndAlso TVA.Common.GetRootType(asmType) Is GetType(T) Then

                        Dim parserType As New ParserTypeInfo()
                        parserType.RuntimeType = asmType
                        parserType.ID = Convert.ToInt16(idField.GetValue(Nothing))

                        If m_optimizeParsing Then
                            Dim dynamicTypeCtor As MethodBuilder = typeBuilder.DefineMethod(asmType.Name, MethodAttributes.Public Or MethodAttributes.Static, asmType, Type.EmptyTypes)
                            With dynamicTypeCtor.GetILGenerator()
                                .Emit(Emit.OpCodes.Nop)
                                .Emit(Emit.OpCodes.Newobj, typeCtor)
                                .Emit(Emit.OpCodes.Ret)
                            End With
                        Else
                            Dim dynamicTypeCtor As New DynamicMethod("DefaultConstructor", asmType, Type.EmptyTypes, asmType.Module, True)
                            With dynamicTypeCtor.GetILGenerator()
                                .Emit(Emit.OpCodes.Nop)
                                .Emit(Emit.OpCodes.Newobj, typeCtor)
                                .Emit(Emit.OpCodes.Ret)
                            End With

                            parserType.CreateNew = CType(dynamicTypeCtor.CreateDelegate(GetType(DefaultConstructor)), DefaultConstructor)
                        End If

                        If Not m_parserTypes.ContainsKey(parserType.ID) Then
                            m_parserTypes.Add(parserType.ID, parserType)
                        End If
                    End If
                Next
            Next

            Dim bakedType As Type = typeBuilder.CreateType()
            For Each parserTypeID As Short In m_parserTypes.Keys
                With m_parserTypes(parserTypeID)
                    .CreateNew = CType(System.Delegate.CreateDelegate(GetType(DefaultConstructor), bakedType.GetMethod(.RuntimeType.Name)), DefaultConstructor)
                End With
            Next

            m_dataQueue.Start()

        End Sub

        Public Sub [Stop]()

            m_dataQueue.Stop()      ' Stop processing of queued data.
            m_parserTypes.Clear()   ' Clear the cached packet type available.

        End Sub

        Public Sub Finish()

            m_dataQueue.Flush()     ' Make sure all of the data is parsed.

        End Sub

        Public Sub Add(ByVal data As Byte())

            Add(Guid.Empty, data)

        End Sub

        Public Sub Add(ByVal source As Guid, ByVal data As Byte())

            m_dataQueue.Add(New IdentifiableItem(Of Byte())(source, data))

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Sub ParseData(ByVal item As IdentifiableItem(Of Byte())())

            For i As Integer = 0 To item.Length - 1
                If item(i).Item IsNot Nothing AndAlso item(i).Item.Length > 0 Then
                    Dim typeID As Short = BitConverter.ToInt16(item(i).Item, m_idValueLocation)
                    Dim parserType As ParserTypeInfo = Nothing

                    If m_parserTypes.TryGetValue(typeID, parserType) Then
                        Dim parsedData As New List(Of T)()
                        Dim newData As T = Nothing

                        Dim j As Integer = 0
                        Do While j < item(i).Item.Length
                            Try
                                newData = parserType.CreateNew()
                                j += newData.Initialize(item(i).Item, j)
                                parsedData.Add(newData)
                            Catch ex As Exception
                                ' TODO: Raise event...
                            End Try
                        Loop

                        RaiseEvent DataParsed(Me, New IdentifiableItemEventArgs(Of List(Of T))(item(i).Source, parsedData))
                    Else
                        RaiseEvent DataDiscarded(Me, New IdentifiableItemEventArgs(Of Byte())(item(i).Source, item(i).Item))
                    End If
                End If
            Next

        End Sub

#Region " ParserTypeInfo Class "

        Private Class ParserTypeInfo

            Public ID As Short

            Public CreateNew As DefaultConstructor

            Public RuntimeType As Type

        End Class

#End Region

#End Region

    End Class

End Namespace