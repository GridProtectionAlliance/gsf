' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Xml
Imports System.IO
Imports System.Text
Imports System.Reflection
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath

Namespace Config

    Public Interface IApplicationConfig
        Default Property Value(ByVal Name As String) As Object          ' Get or set config value
        Property ConfigFile() As String                                 ' Get or set config file name
        Sub Create(ByVal [Name] As String, ByVal [Value] As Object)     ' Create config value if it doesn't exist
        Sub Refresh()                                                   ' Reload config values
        Sub Save()                                                      ' Save config values
    End Interface

    Public Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Note that the names of these enum values must match that of an available type in the variable evaulator
        Public Enum VariableType
            [Eval]          ' JScript.NET expression value
            [Bool]          ' Boolean value
            [Int]           ' Int32 value
            [Float]         ' Single value
            [Date]          ' DateTime value
            [Text]          ' String value
            [Database]      ' Database scalar expression value
            [Undetermined]  ' Undetermined type - evaluated as String
        End Enum

        ' We automatically create a shared instance of this class that will load the default application
        ' configuration settings file, should it exist
        Private Shared appVariables As ApplicationVariables

        Public Shared Property Variables() As ApplicationVariables
            Get
                If appVariables Is Nothing Then appVariables = New ApplicationVariables(SharedSourceDoc, True)
                Return appVariables
            End Get
            Set(ByVal Value As ApplicationVariables)
                appVariables = Value
            End Set
        End Property

        ' We automatically create a shared instance of this class that will load the default application configuration
        ' settings file, should it exist.  Note that by default the shared instances of the Variables and Settings config
        ' classes use the same XML document instance since they use the same config file.  This ensures that there will
        ' be no data loss and also conserves memory.
        Private Shared appSettings As ApplicationSettings

        Public Shared Property Settings() As ApplicationSettings
            Get
                If appSettings Is Nothing Then appSettings = New ApplicationSettings(SharedSourceDoc, True)
                Return appSettings
            End Get
            Set(ByVal Value As ApplicationSettings)
                appSettings = Value
            End Set
        End Property

        Friend Shared ReadOnly Property SharedSourceDoc() As XmlDocument
            Get
                Static xmlDoc As XmlDocument
                If xmlDoc Is Nothing Then xmlDoc = New XmlDocument
                Return xmlDoc
            End Get
        End Property

        ' Using this property users can override the default config file name used by an application - do note
        ' this only affects this code - it doesn't make the .NET ApplicationSettingsReader use this...
        ' To use this, set this property before any calls to the shared Variables or Settings objects, e.g.:
        '           SharedConfigFileName = JustPath(SharedConfigFileName) & "CustomSettings.config"
        Private Shared strSharedConfigFileName As String

        Public Shared Property SharedConfigFileName() As String
            Get
                If Len(strSharedConfigFileName) = 0 Then strSharedConfigFileName = GetConfigFileName()
                Return strSharedConfigFileName
            End Get
            Set(ByVal Value As String)
                strSharedConfigFileName = Value
            End Set
        End Property

        ' This function always returns a running app's proper app.config file name
        Public Shared Function GetConfigFileName() As String

            Dim strConfigFileName As String

            If Not System.Reflection.Assembly.GetEntryAssembly Is Nothing Then
                ' Get config file name for desktop applications
                strConfigFileName = System.Reflection.Assembly.GetEntryAssembly.Location & ".config"
            ElseIf Not System.Reflection.Assembly.GetExecutingAssembly Is Nothing Then
                ' Get config file name for web applications
                strConfigFileName = System.Reflection.Assembly.GetExecutingAssembly.CodeBase

                ' Remove "file" prefix for web page locations
                If Left(strConfigFileName, 8) = "file:///" Then strConfigFileName = Mid(strConfigFileName, 9)

                ' Remove file name from location (code base will be source DLL)
                strConfigFileName = JustPath(strConfigFileName)

                ' Make sure config file ends up in source directory
                If Right(strConfigFileName, 5) = "\bin\" Then strConfigFileName = Left(strConfigFileName, Len(strConfigFileName) - 4)

                strConfigFileName &= "Web.config"
            Else
                Throw New NotSupportedException("Unable to determine configuration file name for this application.")
            End If

            ' Make this a file path instead of a URL path
            Return Replace(strConfigFileName, "/", "\")

        End Function

        Public Shared Sub CreateConfigFile(Optional ByVal ConfigFileName As String = "", Optional ByVal OverwriteExisting As Boolean = False)

            If Len(ConfigFileName) = 0 Then ConfigFileName = GetConfigFileName()
            Dim strConfigFile As String = ConfigFileName
            Dim flgIsDirty As Boolean
            Dim xmlDoc As New XmlDocument
            Dim node As XmlNode

            If OverwriteExisting Or Not File.Exists(strConfigFile) Then
                Dim sw As StreamWriter = File.CreateText(strConfigFile)
                sw.WriteLine("<?xml version=""1.0"" encoding=""Windows-1252""?>")
                sw.WriteLine("<configuration>")
                sw.WriteLine("</configuration>")
                sw.Close()
            End If

            xmlDoc.Load(strConfigFile)

            ' Make sure appSettings section exists
            With xmlDoc.DocumentElement
                If .SelectNodes("appSettings").Count = 0 Then
                    .AppendChild(xmlDoc.CreateElement("appSettings"))
                    InitializeAppSettings(xmlDoc)
                    flgIsDirty = True
                ElseIf .SelectNodes("appSettings/add[@key='appSettings.Initialized']").Count = 0 Then
                    InitializeAppSettings(xmlDoc)
                    flgIsDirty = True
                End If
            End With

            ' Make sure variables section exists
            node = CreateCustomConfigSection("variables", VariablesSectionComments(), xmlDoc, flgIsDirty)
            If Len(Attribute(node, "connectString")) = 0 Then
                Attribute(node, "connectString") = "unspecified"
                flgIsDirty = True
            End If

            If flgIsDirty Then xmlDoc.Save(strConfigFile)

        End Sub

        Public Shared Function CreateCustomConfigSection(ByVal SectionName As String, Optional ByVal SectionComments As String = "", Optional ByVal xmlDoc As XmlDocument = Nothing, Optional ByRef IsDirty As Boolean = False) As XmlNode

            Dim strConfigFileName As String
            Dim flgFoundSection As Boolean
            Dim node As XmlNode

            If xmlDoc Is Nothing Then
                strConfigFileName = GetConfigFileName()
                xmlDoc = New XmlDocument
                CreateConfigFile(strConfigFileName)
                xmlDoc.Load(strConfigFileName)
            End If

            With xmlDoc.DocumentElement
                If .SelectNodes("configSections").Count = 0 Then
                    node = xmlDoc.CreateElement("configSections")
                    .InsertBefore(node, .FirstChild)
                    IsDirty = True
                End If
                With .SelectSingleNode("configSections")
                    If .SelectNodes("section[@name = '" & SectionName & "']").Count = 0 Then
                        node = xmlDoc.CreateElement("section")
                        Attribute(node, "name") = SectionName
                        Attribute(node, "type") = "System.Configuration.IgnoreSectionHandler, System"
                        .AppendChild(node)
                        IsDirty = True
                    End If
                End With
            End With

            With xmlDoc.DocumentElement
                If .SelectNodes(SectionName).Count = 0 Then
                    node = xmlDoc.CreateElement(SectionName)
                    Attribute(node, "created") = "[" & Now() & "]"
                    If Len(SectionComments) > 0 Then node.AppendChild(xmlDoc.CreateComment(SectionComments))
                    .AppendChild(node)
                    IsDirty = True
                Else
                    node = .SelectSingleNode(SectionName)
                End If
            End With

            If IsDirty And Len(strConfigFileName) > 0 Then xmlDoc.Save(strConfigFileName)

            Return node

        End Function

        Friend Shared Sub InitializeAppSettings(ByVal xmlDoc As XmlDocument)

            Dim node As XmlNode
            Dim comment As XmlComment

            With xmlDoc.DocumentElement.SelectSingleNode("appSettings")
                comment = xmlDoc.CreateComment(AppSettingsSectionComments())
                .InsertBefore(comment, .FirstChild)
                node = xmlDoc.CreateElement("add")
                Attribute(node, "key") = "appSettings.Initialized"
                Attribute(node, "value") = "[" & Now() & "] - do not delete this key."
                .InsertAfter(node, comment)
            End With

        End Sub

        Friend Shared Function AppSettingsSectionComments() As String

            Dim strComments As New StringBuilder

            strComments.Append(vbCrLf)
            strComments.Append("      Dynamic component property settings for this application are defined here." & vbCrLf)
            strComments.Append("      These settings can be accessed using the global ""Settings"" object when you import" & vbCrLf)
            strComments.Append("      the ApplicationSettings assembly.  This section is designed to create and manage" & vbCrLf)
            strComments.Append("      dynamic component and control property settings and is distinct from the variables" & vbCrLf)
            strComments.Append("      section used to manage ""typed"" dynamic application variables.  This section is" & vbCrLf)
            strComments.Append("      managed by .NET." & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("      Example:" & vbCrLf)
            strComments.Append("        Imports TVA.Config.Common" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("        Sub Main()" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("            ' Assign value to ""Form1.Caption"" property setting, add it if it doesn't exist" & vbCrLf)
            strComments.Append("            Settings(""Form1.Caption"") = ""My ESO App""" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("        End Sub" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("      Example Settings Defintion:" & vbCrLf)
            strComments.Append("        <add key=""settingName"" value=""settingValue""/>" & vbCrLf)
            strComments.Append("    ")

            Return strComments.ToString()

        End Function

        Friend Shared Function VariablesSectionComments() As String

            Dim strComments As New StringBuilder

            strComments.Append(vbCrLf)
            strComments.Append("      Typed application variables and settings for this application are defined here." & vbCrLf)
            strComments.Append("      These settings can be accessed using the global ""Variables"" object when you import" & vbCrLf)
            strComments.Append("      the ApplicationVariables assembly.  This section is designed to create and manage" & vbCrLf)
            strComments.Append("      ""typed"" dynamic application variables and is distinct from the appSettings section" & vbCrLf)
            strComments.Append("      used to manage dynamic property settings for components and controls.  This section" & vbCrLf)
            strComments.Append("      is not managed by .NET." & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("      Example:" & vbCrLf)
            strComments.Append("        Imports TVA.Config.Common" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("        Sub Main()" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("            ' Make sure ""TitleEnabled"" boolean variable exists, default value to True" & vbCrLf)
            strComments.Append("            Variables.Create(""TitleEnabled"", True, VariableType.Bool)" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("            ' Assign value to ""Title"" variable, create it if it doesn't exist" & vbCrLf)
            strComments.Append("            Variables(""Title"") = ""My ESO App""" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("            DefineTitle()" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("        End Sub" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("        Sub DefineTitle()" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("            If Variables(""TitleEnabled"") Then" & vbCrLf)
            strComments.Append("                Me.Caption = Variables(""Title"")" & vbCrLf)
            strComments.Append("            End If" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("        End Sub" & vbCrLf)
            strComments.Append(vbCrLf)
            strComments.Append("      Example Variable Definitions:" & vbCrLf)
            strComments.Append("        <variable name=""TitleEnabled"" value=""True"" type=""Bool"" description=""This enables the application title"" />" & vbCrLf)
            strComments.Append("        <variable name=""Title"" value=""My ESO App"" priority=""1"" />" & vbCrLf)
            strComments.Append("        <variable name=""StartTime"" value=""DateTime(new Date())"" type=""Eval"" priority=""2"" />" & vbCrLf)
            strComments.Append("        <variable name=""StatusBar"" value=""Variables('Title') + ' started at ' + Variables('StartTime')"" type=""Eval"" priority=""3"" />" & vbCrLf)
            strComments.Append("    ")

            Return strComments.ToString()

        End Function

    End Class

End Namespace