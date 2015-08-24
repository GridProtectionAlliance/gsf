' Mickey Mazarick / James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Xml
Imports TVA.Config.Common
Imports TVA.Shared.Common

Namespace Config

    ' Initial class written by Mickey Mazarick
    <ToolboxBitmap(GetType(ApplicationSettings), "ApplicationSettings.bmp"), DefaultProperty("ConfigFile")> _
    Public Class ApplicationSettings

        Inherits Component
        Implements IApplicationConfig

        Protected xmlDoc As XmlDocument
        Protected strFileName As String
        Protected flgIsDirty As Boolean

        ' This uses the System.Reflection.Assembly.GetExecutingAssembly to get the location of the config file.
        ' A filename can be supplied for custom configs.
        Friend Sub New(ByVal SourceDoc As XmlDocument, ByVal AutoLoad As Boolean)

            xmlDoc = SourceDoc
            strFileName = SharedConfigFileName
            If AutoLoad Then Refresh()

        End Sub

        Public Sub New()

            MyClass.New("app.config", False)

        End Sub

        Public Sub New(ByVal ConfigFileName As String, Optional ByVal AutoLoad As Boolean = True)

            MyClass.New(New XmlDocument(), False)
            If Len(ConfigFileName) > 0 Then strFileName = ConfigFileName
            If AutoLoad Then Refresh()

        End Sub

        Protected Overrides Sub Finalize()

            If flgIsDirty Then xmlDoc.Save(strFileName)

        End Sub

        Default Property Value(ByVal Name As String) As Object Implements IApplicationConfig.Value
            Get
                Return GetValue(Name)
            End Get
            Set(ByVal Value As Object)
                SetValue(Name, Value)
            End Set
        End Property

        Public Sub Create(ByVal Name As String, ByVal Value As Object) Implements IApplicationConfig.Create

            Dim node As XmlNode
            Dim found As Boolean

            ' Creates value only if it doesn't already exist
            With GetXmlNode(xmlDoc, "appSettings", flgIsDirty)
                For Each node In .SelectNodes("add")
                    ' We search for config key name in a case-insensitive manner
                    If StrComp(Attribute(node, "key"), Name, CompareMethod.Text) = 0 Then
                        found = True
                        Exit For
                    End If
                Next
            End With

            If Not found Then Me(Name) = Value

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

            xmlDoc.Load(strFileName)

        End Sub

        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)

            Save()

        End Sub

        ' Saves changes to app.config
        Public Sub Save() Implements IApplicationConfig.Save

            GC.SuppressFinalize(Me)
            xmlDoc.Save(strFileName)

        End Sub

        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Function GetValue(ByVal configKey As String) As Object

            Dim node As XmlNode
            Dim result As Object

            For Each node In xmlDoc.SelectNodes("/configuration/appSettings/add")
                ' We search for config key name in a case-insensitive manner
                If StrComp(Attribute(node, "key"), configKey, CompareMethod.Text) = 0 Then
                    result = Attribute(node, "value")
                    Exit For
                End If
            Next

            ' If config attribute is not found, return an empty string
            If result Is Nothing Then result = ""

            Return result

        End Function

        'Sets a paticular value in the cached app.config file. If the key isn't found, it is added.
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Function SetValue(ByVal configKey As String, ByVal value As Object) As Boolean

            Dim node As XmlNode
            Dim found As Boolean = False

            For Each node In xmlDoc.SelectNodes("/configuration/appSettings/add")
                ' We search for config key name in a case-insensitive manner
                If StrComp(Attribute(node, "key"), configKey, CompareMethod.Text) = 0 Then
                    Attribute(node, "value") = value
                    found = True
                    Exit For
                End If
            Next

            'If config attribute is not found, add it 
            If Not found Then
                node = xmlDoc.CreateElement("add")
                Attribute(node, "key") = configKey
                Attribute(node, "value") = value
                xmlDoc.SelectSingleNode("/configuration/appSettings").AppendChild(node)
                flgIsDirty = True
            End If

            Return found

        End Function

    End Class

End Namespace