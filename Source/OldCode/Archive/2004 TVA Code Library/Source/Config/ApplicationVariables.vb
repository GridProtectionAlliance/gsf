' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.ComponentModel
Imports System.Drawing
Imports System.Xml
Imports TVA.Config.Common
Imports TVA.Shared.Common
Imports TVA.Shared.String
Imports TVA.Shared.FilePath

Namespace Config

    <ToolboxBitmap(GetType(ApplicationVariables), "ApplicationVariables.bmp"), DefaultProperty("ConfigFile")> _
    Public Class ApplicationVariables

        Inherits Component
        Implements IApplicationConfig

        Public LogFileName As String

        <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
        Public varEval As VariableEvaluator

        <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
        Public xmlDoc As XmlDocument
        Private colVariables As VariableTable
        Private strFileName As String
        Private flgWriteLog As Boolean

        Public Class Variable

            Implements IComparable

            Private parent As VariableTable
            Private varEval As VariableEvaluator
            Private xmlDoc As XmlDocument
            Private objValue As Object
            Friend strName As String
            Friend varType As VariableType
            Friend strData As String
            Friend intPriority As Integer
            Friend strDescription As String
            Friend strCategory As String
            Friend strScope As String
            Friend dataChanged As Boolean
            Friend logFile As StreamWriter

            Friend Sub New(ByVal parent As VariableTable, ByVal varEval As VariableEvaluator, ByVal xmlDoc As XmlDocument)

                ' End users should create new variables using the "CreateVariable" function of the variable table
                Me.parent = parent
                Me.varEval = varEval
                Me.xmlDoc = xmlDoc
                varType = VariableType.Undetermined

            End Sub

            ' Name of the variable
            Public Property [Name]() As String
                Get
                    Return strName
                End Get
                Set(ByVal Value As String)
                    dataChanged = True
                    strName = Value
                    Update()
                End Set
            End Property

            ' Type of the variable
            Public Property [Type]() As VariableType
                Get
                    Return varType
                End Get
                Set(ByVal Value As VariableType)
                    dataChanged = True
                    varType = Value
                    Update()
                End Set
            End Property

            ' Raw data value of the variable
            Public Property [Data]() As String
                Get
                    Return strData
                End Get
                Set(ByVal Value As String)
                    dataChanged = True
                    strData = Value
                    EvaluateValue()
                    Update()
                End Set
            End Property

            ' Evaluated data value of the variable
            Public ReadOnly Property [Value]() As Object
                Get
                    Return objValue
                End Get
            End Property

            ' The load priority for this variable
            Public Property [Priority]() As Integer
                Get
                    Return intPriority
                End Get
                Set(ByVal Value As Integer)
                    dataChanged = True
                    intPriority = Value
                    Update()
                End Set
            End Property

            ' Any description defined for the variable
            Public Property [Description]() As String
                Get
                    Return strDescription
                End Get
                Set(ByVal Value As String)
                    dataChanged = True
                    strDescription = Value
                    Update()
                End Set
            End Property

            ' The category for this variable
            Public Property [Category]() As String
                Get
                    Return strCategory
                End Get
                Set(ByVal Value As String)
                    dataChanged = True
                    strCategory = Value
                    Update()
                End Set
            End Property

            ' Any possible scope defined for the variable (e.g., "Application" or "Session" for web variables)
            Public Property [Scope]() As String
                Get
                    Return strScope
                End Get
                Set(ByVal Value As String)
                    dataChanged = True
                    strScope = Value
                    Update()
                End Set
            End Property

            Friend Sub EvaluateValue()

                ' Corece raw data into variable value based on specified data type
                Try
                    objValue = varEval.Evaluate(strName, [Enum].GetName(GetType(VariableType), varType), strData, logFile)
                Catch
                    ' We will still provide a blank value for the given data type if the value couldn't be evaluated
                    Select Case varType
                        Case VariableType.[Bool]
                            objValue = False
                        Case VariableType.[Int]
                            objValue = 0
                        Case VariableType.[Float]
                            objValue = CSng(0)
                        Case VariableType.[Date]
                            objValue = DateTime.MinValue
                        Case VariableType.[Text]
                            objValue = ""
                        Case Else
                            objValue = Nothing
                    End Select
                End Try

            End Sub

            Friend Sub Update()

                ' Update XML host file
                Dim node As XmlNode
                Dim flgFound As Boolean = False

                ' Lookup and update node
                For Each node In GetXmlNode(xmlDoc, "variables").SelectNodes("variable")
                    If StrComp(node.Attributes("name").Value, strName, CompareMethod.Text) = 0 Then
                        UpdateAttributes(node)
                        flgFound = True
                        Exit For
                    End If
                Next

                ' If variable was not found, add it
                If Not flgFound Then
                    node = xmlDoc.CreateElement("variable")
                    Attribute(node, "name") = strName
                    UpdateAttributes(node)
                    GetXmlNode(xmlDoc, "variables").AppendChild(node)
                End If

                ' Data changed, make sure class is registered for finalize
                parent.parent.IsDirty = True

            End Sub

            Private Sub UpdateAttributes(ByVal node As XmlNode)

                Attribute(node, "value") = strData
                If varType <> VariableType.Undetermined Then Attribute(node, "type") = [Enum].GetName(GetType(VariableType), varType)
                If intPriority <> 0 Then Attribute(node, "priority") = intPriority
                If Len(strDescription) > 0 Then Attribute(node, "description") = strDescription
                If Len(strCategory) > 0 Then Attribute(node, "category") = strCategory
                If Len(strScope) > 0 Then Attribute(node, "scope") = strScope

            End Sub

            Public Shared Function TranslateType(ByVal TypeName As String) As VariableType

                Select Case Trim(TypeName).ToLower()
                    Case "eval", "evaluate", "expression", "jscript", "javascript"
                        Return VariableType.Eval
                    Case "bool", "boolean", "bit"
                        Return VariableType.Bool
                    Case "int", "int32", "integer", "long"
                        Return VariableType.Int
                    Case "float", "single", "double", "real"
                        Return VariableType.Float
                    Case "date", "datetime", "time"
                        Return VariableType.Date
                    Case "text", "string", "data"
                        Return VariableType.Text
                    Case "database", "db", "oledb"
                        Return VariableType.Database
                    Case Else
                        Return VariableType.Undetermined
                End Select

            End Function

            Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

                ' Variables are sorted in priority order
                If TypeOf obj Is Variable Then
                    Return intPriority.CompareTo(DirectCast(obj, Variable).intPriority)
                Else
                    Throw New ArgumentException("Variable can only be compared to other Variables")
                End If

            End Function

        End Class

        Public Class VariableTable

            Friend parent As ApplicationVariables
            Private tblVariables As New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
            Private varEval As VariableEvaluator
            Private xmlDoc As XmlDocument
            Friend dataChanged As Boolean

            Friend Sub New(ByVal parent As ApplicationVariables, ByVal varEval As VariableEvaluator, ByVal xmlDoc As XmlDocument)

                Me.parent = parent
                Me.varEval = varEval
                Me.xmlDoc = xmlDoc

            End Sub

            Public Function Create(ByVal [Name] As String, ByVal [Value] As String, Optional ByVal [Type] As VariableType = VariableType.Undetermined) As Variable

                Dim var As New Variable(Me, varEval, xmlDoc)

                var.strName = [Name]
                var.varType = [Type]
                var.strData = [Value]

                Return var

            End Function

            Public Function Create(ByVal [Name] As String, ByVal [Value] As String, ByVal [Type] As VariableType, ByVal [Description] As String, Optional ByVal [Priority] As Integer = 0) As Variable

                Dim var As New Variable(Me, varEval, xmlDoc)

                var.strName = [Name]
                var.varType = [Type]
                var.intPriority = [Priority]
                var.strDescription = [Description]
                var.strData = [Value]

                Return var

            End Function

            Public Sub Add(ByVal NewVar As Variable)

                Me(NewVar.strName) = NewVar

            End Sub

            Default Public Property Item(ByVal Index As String) As Variable
                Get
                    Return tblVariables(Index)
                End Get
                Set(ByVal Value As Variable)
                    ' Make sure variable changes get propagated to XML file
                    Value.Update()
                    Value.EvaluateValue()

                    dataChanged = True

                    ' Data changed, make sure class is registered for finalize
                    parent.IsDirty = True

                    If tblVariables(Index) Is Nothing Then
                        ' If we didn't find the variable, we'll add a new one
                        tblVariables.Add(Value.strName, Value)
                    Else
                        ' Otherwise, we'll just update the variable
                        tblVariables(Index) = Value
                    End If
                End Set
            End Property

            Public Function GetEnumerator() As IDictionaryEnumerator

                Return tblVariables.GetEnumerator()

            End Function

            Public ReadOnly Property Count() As Integer
                Get
                    Return tblVariables.Count
                End Get
            End Property

            Friend Sub Clear()

                tblVariables.Clear()

            End Sub

        End Class

        Friend Sub New(ByVal SourceDoc As XmlDocument, ByVal AutoLoad As Boolean)

            xmlDoc = SourceDoc
            varEval = New VariableEvaluator()
            colVariables = New VariableTable(Me, varEval, xmlDoc)
            strFileName = SharedConfigFileName
            LogFileName = JustPath(strFileName) & "VarInit.log"

            ' Give the variable evaluator an instance of this class so variables can reference other variable values
            varEval.Variables = Me

            Try
                ' We'll try to auto create the config file if it doesn't exist
                CreateConfigFile(strFileName)
                If AutoLoad Then Refresh()
            Catch
                ' We're not going throw an exception on library load just because
                ' we couldn't create a default variable configuration file
            End Try

        End Sub

        Public Sub New()

            MyClass.New("app.config", False, "", False)

        End Sub

        Public Sub New(ByVal ConfigFileName As String, Optional ByVal WriteLog As Boolean = False, Optional ByVal LogFileName As String = "", Optional ByVal AutoLoad As Boolean = True)

            MyClass.New(New XmlDocument(), False)
            flgWriteLog = WriteLog
            If Len(LogFileName) > 0 Then Me.LogFileName = LogFileName
            If Len(ConfigFileName) > 0 Then strFileName = ConfigFileName
            If AutoLoad Then Refresh()

        End Sub

        Protected Overrides Sub Finalize()

            Save(True)

        End Sub

        <Browsable(True), Category("Configuration"), Description("Set this value to app.config to use default config file."), DefaultValue("app.config")> _
        Public Property ConfigFile() As String Implements IApplicationConfig.ConfigFile
            Get
                Return strFileName
            End Get
            Set(ByVal Value As String)
                strFileName = Value
                If Not DesignMode Then Refresh()
            End Set
        End Property

        ' Reload the config file
        Public Sub Refresh() Implements IApplicationConfig.Refresh

            ' If user left default design property as "app.config", then we substitute this with proper config file name...
            If StrComp(strFileName, "app.config", CompareMethod.Text) = 0 Then
                strFileName = SharedConfigFileName

                ' We also use the shared Xml document so all changes get registered to the same object
                xmlDoc = SharedSourceDoc
            End If

            If Not File.Exists(strFileName) Then
                Throw New FileNotFoundException("Specified configuration file """ & strFileName & """ does not exist.")
                Exit Sub
            End If

            ' Evaluate and validate all of the variable values based on their types
            Dim strConnectString As String
            Dim node As XmlNode
            Dim var As Variable
            Dim vars As New ArrayList
            Dim sw As StreamWriter
            Dim x As Integer

            If WriteLog Then
                ' Open log file
                sw = File.AppendText(LogFileName)
                sw.WriteLine("Variable Initialization Log - " & Now())
                sw.WriteLine("Provided Config File Name: " & strFileName)
                sw.WriteLine("")
            End If

            ' Clear any existing loaded variables
            colVariables.Clear()

            ' Load the XML based application config file
            xmlDoc.Load(strFileName)

            ' Initialize OleDb connection for "database" variables if connect string was specified
            Try
                strConnectString = xmlDoc.SelectSingleNode("/configuration/variables/@connectString").Value
            Catch
                strConnectString = ""
            End Try

            If Len(strConnectString) > 0 Then
                If StrComp(strConnectString, "unspecified", CompareMethod.Text) <> 0 Then
                    varEval.Connection = New Data.OleDb.OleDbConnection(strConnectString)
                    varEval.Connection.Open()
                End If
            End If

            ' Load all non-disabled variables into a sortable array - but don't evaluate values yet
            For Each node In GetXmlNode(xmlDoc, "variables").SelectNodes("variable")
                ' If the user has disabled this variable, we skip it
                If Not CBool(NotEmpty(Attribute(node, "disabled"), False)) Then
                    var = New Variable(colVariables, varEval, xmlDoc)

                    With var
                        .strName = Attribute(node, "name")
                        .varType = Variable.TranslateType(Attribute(node, "type"))
                        .intPriority = CInt(NotEmpty(Attribute(node, "priority"), 0))
                        .strDescription = Attribute(node, "description")
                        .strCategory = Attribute(node, "category")
                        .strScope = Attribute(node, "scope")
                        .strData = Attribute(node, "value")
                        .logFile = sw
                    End With

                    vars.Add(var)
                End If
            Next

            ' Sort all of our new variables by priority order - this way any variables that
            ' depend on other variables will be loaded and evaluated in the proper order
            vars.Sort()

            ' Now validate and evaluate our variable values and add them to the variable table
            For x = 0 To vars.Count - 1
                var = DirectCast(vars(x), Variable)
                var.EvaluateValue()
                var.dataChanged = False
                var.logFile = Nothing       ' We don't log variable updates outside this function

                ' Add this validated, evaluated variable to the main variable table
                colVariables.Add(var)
            Next

            colVariables.dataChanged = False

            If Not varEval.Connection Is Nothing Then
                varEval.Connection.Close()
                varEval.Connection = Nothing
            End If

            If WriteLog Then
                ' Close log file
                sw.WriteLine("-----------------------------------------------------------------------")
                sw.Close()
            End If

        End Sub

        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)

            Save()

        End Sub

        ' Saves changes to app.config
        Public Sub Save() Implements IApplicationConfig.Save

            IsDirty = False
            Save(False)

        End Sub

        ' To finalize, or not to finalize - that is the question
        Friend WriteOnly Property IsDirty() As Boolean
            Set(ByVal Value As Boolean)
                If Value Then
                    GC.ReRegisterForFinalize(Me)
                Else
                    GC.SuppressFinalize(Me)
                End If
            End Set
        End Property

        Private Sub Save(ByVal SkipReset As Boolean)

            If Not xmlDoc Is Nothing Then
                If Not colVariables Is Nothing Then
                    Dim x As Integer

                    ' Check for variable table changes
                    If colVariables.dataChanged Then
                        xmlDoc.Save(strFileName)
                    Else
                        Dim dataChanged As Boolean
                        Dim de As DictionaryEntry

                        ' Check for variable attribute changes
                        For Each de In colVariables
                            If Not de.Value Is Nothing Then
                                If DirectCast(de.Value, Variable).dataChanged Then
                                    dataChanged = True
                                    Exit For
                                End If
                            End If
                        Next

                        If dataChanged Then xmlDoc.Save(strFileName)
                    End If

                    If Not SkipReset Then
                        colVariables.dataChanged = False

                        For x = 0 To colVariables.Count - 1
                            If Not colVariables(x) Is Nothing Then colVariables(x).dataChanged = False
                        Next
                    End If
                End If
            End If

        End Sub

        Default Property Value(ByVal Name As String) As Object Implements IApplicationConfig.Value
            Get
                Dim var As Variable = colVariables(Name)

                ' See if this variable exists
                If var Is Nothing Then
                    ' If not, return an empty string
                    Return ""
                Else
                    ' Otherwise return the value
                    Return colVariables(Name).Value
                End If
            End Get
            Set(ByVal Value As Object)
                Dim var As Variable = colVariables(Name)

                ' See if this variable exists
                If var Is Nothing Then
                    ' If not, create a new variable and add it to the variable collection
                    colVariables(Name) = colVariables.Create(Name, CStr(Value))
                Else
                    ' Otherwise just update the value
                    var.Data = CStr(Value)
                End If
            End Set
        End Property

        Public ReadOnly Property Table() As VariableTable
            Get
                Return colVariables
            End Get
        End Property

        Public Function Exists(ByVal Name As String) As Boolean

            Return (Not colVariables(Name) Is Nothing)

        End Function

        Public Sub Create(ByVal [Name] As String, ByVal [Value] As Object) Implements IApplicationConfig.Create

            If Not Exists([Name]) Then colVariables.Add(colVariables.Create([Name], CStr([Value])))

        End Sub

        Public Function Create(ByVal [Name] As String, ByVal [Value] As Object, ByVal [Type] As VariableType, Optional ByVal [Description] As String = "", Optional ByVal [Priority] As Integer = 0, Optional ByVal AddToTable As Boolean = True, Optional ByVal OverwriteExisting As Boolean = False) As Variable

            Dim NewVar As Variable = colVariables.Create([Name], CStr([Value]), [Type], [Description], [Priority])

            If AddToTable Then
                If OverwriteExisting Or Not Exists([Name]) Then
                    colVariables.Add(NewVar)
                End If
            End If

            Return NewVar

        End Function

        Public Sub Add(ByVal NewVar As Variable)

            colVariables.Add(NewVar)

        End Sub

        Public Property WriteLog() As Boolean
            Get
                Return flgWriteLog
            End Get
            Set(ByVal Value As Boolean)
                flgWriteLog = Value
                Save()
                Refresh()
            End Set
        End Property

        ' This function exists for backwards compatibility - but is hidden from the editor because it
        ' has been deprecated - use default Value property instead
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Function GetValue(ByVal configKey As String) As Object

            Return Me(configKey)

        End Function

        ' This function exists for backwards compatibility - but is hidden from the editor because it
        ' has been deprecated - use default Value property instead
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Function SetValue(ByVal configKey As String, ByVal value As Object) As Boolean

            Dim found As Boolean = Exists(configKey)

            Me(configKey) = value

            Return found

        End Function

    End Class

End Namespace