' 03/28/2007

Option Strict On

Imports System.IO
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.ComponentModel
Imports TVA.Common
Imports TVA.Collections
Imports TVA.IO.FilePath

<DefaultEvent("DataParsed")> _
Public MustInherit Class BinaryDataParserBase(Of TIdentifier, TOutput As IBinaryDataConsumer)
    Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

    Private m_idPropertyName As String
    Private m_optimizeParsing As Boolean
    Private m_discardUnparsedData As Boolean
    Private m_persistSettings As Boolean
    Private m_settingsCategoryName As String
    Private m_outputTypes As Dictionary(Of TIdentifier, TypeInfo)

    Private WithEvents m_dataQueue As ProcessQueue(Of IdentifiableItem(Of Guid, Byte()))

    Private Delegate Function DefaultConstructor() As TOutput

#End Region

#Region " Event Declaration "

    ''' <summary>
    ''' Occurs when a data image has been parsed.
    ''' </summary>
    Public Event DataParsed As EventHandler(Of GenericEventArgs(Of IdentifiableItem(Of Guid, List(Of TOutput))))

    ''' <summary>
    ''' Occurs when unused data during parsing is reused and not discarded.
    ''' </summary>
    Public Event DataReused As EventHandler(Of GenericEventArgs(Of TIdentifier))

    ''' <summary>
    ''' Occurs when unused data during parsing is discarded and not re-used.
    ''' </summary>
    Public Event DataDiscarded As EventHandler(Of GenericEventArgs(Of TIdentifier))

    ''' <summary>
    ''' Occurs when a matching output type is not found for parsing the data image.
    ''' </summary>
    Public Event OutputTypeNotFound As EventHandler(Of GenericEventArgs(Of TIdentifier))

#End Region

#Region " Code Scope: Public "

    ''' <summary>
    ''' Gets or sets the name of the property that identifies the output type.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Name of the property that identifies the output type.</returns>
    Public Property IDPropertyName() As String
        Get
            Return m_idPropertyName
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                m_idPropertyName = value
            Else
                Throw New ArgumentNullException("IDPropertyName")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a boolean value indicating if parsing is to be done in an optimal mode.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if parsing is to be done in an optimal mode; otherwise False.</returns>
    Public Property OptimizeParsing() As Boolean
        Get
            Return m_optimizeParsing
        End Get
        Set(ByVal value As Boolean)
            m_optimizeParsing = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a boolean value indicating if unparsed data is to be discarded and not re-used.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if unparsed data is to be discarded and not re-used; otherwise False.</returns>
    Public Property DiscardUnparsedData() As Boolean
        Get
            Return m_discardUnparsedData
        End Get
        Set(ByVal value As Boolean)
            m_discardUnparsedData = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the queue to which data to be parsed is to be added.
    ''' </summary>
    ''' <value></value>
    ''' <returns>
    ''' The TVA.Collections.ProcessQueue(Of TVA.IdentifiableItem(Of Guid, Byte())) to which data to be parsed is 
    ''' to be added.
    ''' </returns>
    <Browsable(False)> _
    Public ReadOnly Property DataQueue() As ProcessQueue(Of IdentifiableItem(Of Guid, Byte()))
        Get
            Return m_dataQueue
        End Get
    End Property

    ''' <summary>
    ''' Starts the parser.
    ''' </summary>
    Public Sub Start()

        Dim asm As Reflection.Assembly = Nothing
        Dim typeCtor As ConstructorInfo = Nothing
        Dim asmBuilder As AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(New AssemblyName("InMemory"), AssemblyBuilderAccess.Run)
        Dim modBuilder As ModuleBuilder = asmBuilder.DefineDynamicModule("Helper")
        Dim typeBuilder As TypeBuilder = modBuilder.DefineType("ClassFactory")
        Dim outputTypes As New List(Of TypeInfo)() ' Temporarily hold output types until their IDs are determined.

        ' Process all assemblies in the application's directory.
        For Each dll As String In Directory.GetFiles(JustPath(TVA.Assembly.EntryAssembly.Location), "*.dll")
            ' Load the assembly in the curent app domain.
            asm = Reflection.Assembly.LoadFrom(dll)

            ' Process all of the public types in the assembly.
            For Each asmType As Type In asm.GetExportedTypes()
                typeCtor = asmType.GetConstructor(Type.EmptyTypes)
                If typeCtor IsNot Nothing AndAlso Not asmType.IsAbstract AndAlso _
                        TVA.Common.GetRootType(asmType) Is GetType(TOutput) Then
                    ' The type meets the following criteria:
                    ' - has a default public constructor
                    ' - is not abstract and can be instantiated.
                    ' - root type is same as the type specified for the output

                    Dim outputType As New TypeInfo()
                    outputType.RuntimeType = asmType

                    ' We employ 2 of the best peforming ways of instantiating objects using reflection.
                    ' See: http://blogs.msdn.com/haibo_luo/archive/2005/11/17/494009.aspx
                    If m_optimizeParsing Then
                        ' Invokation approach: Reflection.Emit + Delegate
                        ' This is hands-down that most fastest way of instantiating objects using reflection.
                        Dim dynamicTypeCtor As MethodBuilder = typeBuilder.DefineMethod(asmType.Name, MethodAttributes.Public Or MethodAttributes.Static, asmType, Type.EmptyTypes)
                        With dynamicTypeCtor.GetILGenerator()
                            .Emit(Emit.OpCodes.Nop)
                            .Emit(Emit.OpCodes.Newobj, typeCtor)
                            .Emit(Emit.OpCodes.Ret)
                        End With
                    Else
                        ' Invokation approach: DynamicMethod + Delegate
                        ' This method is very fast compared to rest of the approaches, but not as fast as the one above.
                        Dim dynamicTypeCtor As New DynamicMethod("DefaultConstructor", asmType, Type.EmptyTypes, asmType.Module, True)
                        With dynamicTypeCtor.GetILGenerator()
                            .Emit(Emit.OpCodes.Nop)
                            .Emit(Emit.OpCodes.Newobj, typeCtor)
                            .Emit(Emit.OpCodes.Ret)
                        End With

                        ' Create a delegate to the constructor that'll be called to create a new instance of the type.
                        outputType.CreateNew = CType(dynamicTypeCtor.CreateDelegate(GetType(DefaultConstructor)), DefaultConstructor)
                    End If

                    ' We'll hold all of the matching types in this list temporarily until their IDs are determined.
                    outputTypes.Add(outputType)
                End If
            Next
        Next

        If m_optimizeParsing Then
            ' The reason we have to do this over here is because we can create a type only once. This is the type 
            ' that has all the constructors created above for the various class matching the requirements for the 
            ' output type.
            Dim bakedType As Type = typeBuilder.CreateType()    ' This can be done only once!!!
            For Each outputType As TypeInfo In outputTypes
                outputType.CreateNew = CType(System.Delegate.CreateDelegate(GetType(DefaultConstructor), bakedType.GetMethod(outputType.RuntimeType.Name)), DefaultConstructor)
            Next
        End If

        Dim idProperty As PropertyInfo
        For Each outputType As TypeInfo In outputTypes
            ' Now, we'll go though all of the output types we've found and instantiate an instance of each in order
            ' to get the identifier for each of the type. This will help lookup of the type to be used when parsing
            ' the data.
            Dim instance As TOutput = outputType.CreateNew()
            idProperty = outputType.RuntimeType.GetProperty(m_idPropertyName, GetType(TIdentifier))
            If idProperty IsNot Nothing Then
                ' The output type does expose a property with the specified name and return type.
                outputType.ID = CType(idProperty.GetValue(instance, Nothing), TIdentifier)
                If Not m_outputTypes.ContainsKey(outputType.ID) Then
                    m_outputTypes.Add(outputType.ID, outputType)
                End If
            End If
        Next

        m_dataQueue.Start()

    End Sub

    ''' <summary>
    ''' Stops the parser.
    ''' </summary>
    Public Sub [Stop]()

        m_dataQueue.Stop()      ' Stop processing of queued data.
        m_outputTypes.Clear()   ' Clear the cached packet type available.

    End Sub

#Region " MustOverride "

    Public MustOverride Function GetTypeID(ByVal binaryImage As Byte()) As TIdentifier

#End Region

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
                    IDPropertyName = .Item("IDPropertyName").GetTypedValue(m_idPropertyName)
                    OptimizeParsing = .Item("OptimizeParsing").GetTypedValue(m_optimizeParsing)
                    DiscardUnparsedData = .Item("DiscardUnparsedData").GetTypedValue(m_discardUnparsedData)
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
                    With .Item("IDPropertyName", True)
                        .Value = m_idPropertyName
                        .Description = "Name of the property that identifies the output type."
                    End With
                    With .Item("OptimizeParsing", True)
                        .Value = m_optimizeParsing.ToString()
                        .Description = "True if parsing is to be done in an optimal mode; otherwise False."
                    End With
                    With .Item("DiscardUnparsedData", True)
                        .Value = m_discardUnparsedData.ToString()
                        .Description = "True if unparsed data is to be discarded and not re-used; otherwise False."
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
            ' We'll process all of the data images in the batch.
            If item(i).Item IsNot Nothing AndAlso item(i).Item.Length > 0 Then
                Dim typeID As TIdentifier = GetTypeID(item(i).Item)
                Dim outputType As TypeInfo = Nothing

                If m_outputTypes.TryGetValue(typeID, outputType) Then
                    ' We have a type we can use to represent this data.
                    Dim output As New List(Of TOutput)()
                    Dim newInstance As TOutput = Nothing

                    Dim j As Integer = 0
                    Try
                        Do While j < item(i).Item.Length
                            ' Process the entire data image.
                            newInstance = outputType.CreateNew()
                            j += newInstance.Initialize(item(i).Item, j)
                            output.Add(newInstance)
                        Loop
                    Catch ex As Exception
                        ' We might encounter an exception if the data image being parsed is partial.
                        If Not m_discardUnparsedData Then
                            Dim insertUnusedData As Boolean = True
                            Dim unusedData As Byte() = CreateArray(Of Byte)(item(i).Item.Length - j)
                            Array.Copy(item(i).Item, j, unusedData, 0, unusedData.Length)

                            If i < (item.Length - 1) Then
                                ' This isn't the last data image in the batch.
                                For k As Integer = i + 1 To item.Length - 1
                                    ' We'll for a data image in the batch that is from the same source as this image.
                                    If item(k).Source.Equals(item(i).Source) Then
                                        Dim mergedImage As Byte() = CreateArray(Of Byte)(item(k).Item.Length + unusedData.Length)
                                        Array.Copy(item(k).Item, 0, mergedImage, 0, item(k).Item.Length)
                                        Array.Copy(unusedData, 0, mergedImage, item(k).Item.Length, unusedData.Length)

                                        item(k).Item = mergedImage

                                        insertUnusedData = False
                                        Exit For
                                    End If
                                Next
                            End If

                            If insertUnusedData Then
                                m_dataQueue.Insert(0, New IdentifiableItem(Of Guid, Byte())(item(i).Source, unusedData))
                            End If

                            RaiseEvent DataReused(Me, New GenericEventArgs(Of TIdentifier)(typeID))
                        Else
                            RaiseEvent DataDiscarded(Me, New GenericEventArgs(Of TIdentifier)(typeID))
                        End If
                    End Try

                    RaiseEvent DataParsed(Me, New GenericEventArgs(Of IdentifiableItem(Of Guid, List(Of TOutput)))(New IdentifiableItem(Of Guid, List(Of TOutput))(item(i).Source, output)))
                Else
                    ' We'll notify that we didn't find a output type we could use to represent this data image.
                    RaiseEvent OutputTypeNotFound(Me, New GenericEventArgs(Of TIdentifier)(typeID))
                End If
            End If
        Next

    End Sub

#Region " ParserTypeInfo Class "

    Private Class TypeInfo

        Public ID As TIdentifier

        Public CreateNew As DefaultConstructor

        Public RuntimeType As Type

    End Class

#End Region

#End Region

End Class