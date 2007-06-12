' 03/28/2007

Option Strict On

Imports System.IO
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.ComponentModel
Imports TVA.Collections
Imports TVA.IO.FilePath

<DefaultEvent("DataParsed")> _
Public MustInherit Class BinaryDataParserBase(Of TIdentifier, TResult As IBinaryDataConsumer)
    Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

    Private m_idFieldName As String
    Private m_optimizeParsing As Boolean
    Private m_persistSettings As Boolean
    Private m_settingsCategoryName As String
    Private m_parserTypes As Dictionary(Of TIdentifier, ParserTypeInfo)

    Private Delegate Function DefaultConstructor() As TResult

    Private WithEvents m_dataQueue As ProcessQueue(Of IdentifiableItem(Of Guid, Byte()))

#End Region

#Region " Event Declaration "

    Public Event DataParsed As EventHandler(Of GenericEventArgs(Of IdentifiableItem(Of Guid, List(Of TResult))))
    Public Event DataDiscarded As EventHandler(Of GenericEventArgs(Of IdentifiableItem(Of Guid, Byte())))

#End Region

#Region " Code Scope: Public "

    Public Property IDFieldName() As String
        Get
            Return m_idFieldName
        End Get
        Set(ByVal value As String)
            m_idFieldName = value
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
    Public ReadOnly Property DataQueue() As ProcessQueue(Of IdentifiableItem(Of Guid, Byte()))
        Get
            Return m_dataQueue
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
                        Not asmType.IsAbstract AndAlso TVA.Common.GetRootType(asmType) Is GetType(TResult) Then

                    Dim parserType As New ParserTypeInfo()
                    parserType.RuntimeType = asmType
                    parserType.ID = CType(idField.GetValue(Nothing), TIdentifier)

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
        For Each parserTypeID As TIdentifier In m_parserTypes.Keys
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

    Public MustOverride Function GetID(ByVal binaryImage As Byte()) As TIdentifier

#Region " Interface Implementation "

#Region " IPersistSettings "

    Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
        Get
            Return m_persistSettings
        End Get
        Set(ByVal value As Boolean)
            m_persistSettings = value
        End Set
    End Property

    Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
        Get
            Return m_settingsCategoryName
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_settingsCategoryName = value
            Else
                Throw New ArgumentNullException("SettingsCategoryName")
            End If
        End Set
    End Property

    Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

        Try
            With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                If .Count > 0 Then
                    IDFieldName = .Item("IDFieldName").GetTypedValue(m_idFieldName)
                    OptimizeParsing = .Item("OptimizeParsing").GetTypedValue(m_optimizeParsing)
                End If
            End With
        Catch ex As Exception
            ' We'll encounter exceptions if the settings are not present in the config file.
        End Try

    End Sub

    Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

        If m_persistSettings Then
            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    .Clear()
                    With .Item("IDFieldName", True)
                        .Value = m_idFieldName
                        .Description = ""
                    End With
                    With .Item("OptimizeParsing", True)
                        .Value = m_optimizeParsing.ToString()
                        .Description = ""
                    End With
                End With
                TVA.Configuration.Common.SaveSettings()
            Catch ex As Exception
                ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
            End Try
        End If

    End Sub

#End Region

#Region " ISupportInitialize "

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        ' We don't need to do anything before the component is initialized.

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If Not DesignMode Then
            LoadSettings()  ' Load settings from the config file.
        End If

    End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

    Private Sub ParseData(ByVal item As IdentifiableItem(Of Guid, Byte())())

        For i As Integer = 0 To item.Length - 1
            If item(i).Item IsNot Nothing AndAlso item(i).Item.Length > 0 Then
                Dim typeID As TIdentifier = GetID(item(i).Item) 'BitConverter.ToInt16(item(i).Item, m_idValueLocation)
                Dim parserType As ParserTypeInfo = Nothing

                If m_parserTypes.TryGetValue(typeID, parserType) Then
                    Dim parsedData As New List(Of TResult)()
                    Dim newData As TResult = Nothing

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

                    RaiseEvent DataParsed(Me, New GenericEventArgs(Of IdentifiableItem(Of Guid, List(Of TResult)))(New IdentifiableItem(Of Guid, List(Of TResult))(item(i).Source, parsedData)))
                Else
                    RaiseEvent DataDiscarded(Me, New GenericEventArgs(Of IdentifiableItem(Of Guid, Byte()))(New IdentifiableItem(Of Guid, Byte())(item(i).Source, item(i).Item)))
                End If
            End If
        Next

    End Sub

#Region " ParserTypeInfo Class "

    Private Class ParserTypeInfo

        Public ID As TIdentifier

        Public CreateNew As DefaultConstructor

        Public RuntimeType As Type

    End Class

#End Region

#End Region

End Class