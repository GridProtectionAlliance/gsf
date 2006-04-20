' 04-12-06

Imports System.Xml
Imports System.Configuration
Imports System.Web.Configuration
Imports Tva.Xml.Common
Imports Tva.IO.FilePath

Namespace Configuration

    ''' <summary>
    ''' Represents a configuration file of a windows or web application.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ConfigurationFile

        Private m_configuration As System.Configuration.Configuration

        Private Const CustomSectionName As String = "categorizedSettings"
        Private Const CustomSectionType As String = "Tva.Configuration.CategorizedSettingsSection, Tva.Core"

        ''' <summary>
        ''' Specifies the application environment.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum ApplicationEnvironment As Integer
            Win
            Web
        End Enum

        ''' <summary>
        ''' Initializes a default instance of Tva.Configuration.ConfigurationFile.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            Me.New("")
        End Sub

        ''' <summary>
        ''' Initializes a instance of Tva.Configuration.ConfigurationFile for the specified configuration file
        ''' that belongs to a windows or web application.
        ''' </summary>
        ''' <param name="configFilePath">Path of the configuration file that belongs to a windows or web application.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal configFilePath As String)
            m_configuration = GetConfiguration(configFilePath)
            If m_configuration.HasFile() Then
                ValidateConfigurationFile(m_configuration.FilePath())
            Else
                CreateConfigurationFile(m_configuration.FilePath())
            End If
            m_configuration = GetConfiguration(configFilePath)
        End Sub

        ''' <summary>
        ''' Gets the environment of the application to which the current configuration file belongs.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The environment of the application to which the current configuration file belongs.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Environment() As ApplicationEnvironment
            Get
                Dim currentEnvironment As ApplicationEnvironment = ApplicationEnvironment.Win
                If System.Web.HttpContext.Current() IsNot Nothing Then currentEnvironment = ApplicationEnvironment.Web
                Return currentEnvironment
            End Get
        End Property

        ''' <summary>
        ''' Gets the Tva.Configuration.CategorizedSettingsSection representing the "categorizedSettings" section of the configuration file.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Tva.Configuration.CategorizedSettingsSection representing the "categorizedSettings" section of the configuration file.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CategorizedSettings() As CategorizedSettingsSection
            Get
                Return DirectCast(m_configuration.GetSection(CustomSectionName), CategorizedSettingsSection)
            End Get
        End Property

        ''' <summary>
        ''' Gets the System.Configuration.AppSettingsSection representing the "appSettings" section of the configuration file.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The System.Configuration.AppSettingsSection representing the "appSettings" section of the configuration file.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AppSettings() As AppSettingsSection
            Get
                Return m_configuration.AppSettings()
            End Get
        End Property

        ''' <summary>
        ''' Gets the System.Configuration.ConnectionStringsSection representing the "connectionStrings" section of the configuration file.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The System.Configuration.ConnectionStringsSection representing the "connectionStrings" section of the configuration file.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ConnectionStrings() As ConnectionStringsSection
            Get
                Return m_configuration.ConnectionStrings()
            End Get
        End Property

        ''' <summary>
        ''' Gets the physical path to the configuration file represented by this Tva.Configuration.Configuration object.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The physical path to the configuration file represented by this Tva.Configuration.ConfigurationFile object.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FilePath() As String
            Get
                Return m_configuration.FilePath()
            End Get
        End Property

        ''' <summary>
        ''' Writes the configuration settings contained within this Tva.Configuration.ConfigurationFile object 
        ''' to the configuration file that it represents.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Save()

            Me.Save(ConfigurationSaveMode.Modified)

        End Sub

        ''' <summary>
        ''' Writes the configuration settings contained within this Tva.Configuration.ConfigurationFile object 
        ''' to the configuration file that it represents.
        ''' </summary>
        ''' <param name="saveMode">A System.Configuration.ConfigurationSaveMode value that determines which property values to save.</param>
        ''' <remarks></remarks>
        Public Sub Save(ByVal saveMode As ConfigurationSaveMode)

            m_configuration.Save(saveMode)

        End Sub

        ''' <summary>
        ''' Writes the configuration settings contained within this Tva.Configuration.ConfigurationFile object 
        ''' to the specified configuration file.
        ''' </summary>
        ''' <param name="fileName">The path and file name to save the configuration file to.</param>
        ''' <remarks></remarks>
        Public Sub SaveAs(ByVal fileName As String)

            m_configuration.SaveAs(fileName)

        End Sub

        Private Function GetConfiguration(ByVal configFilePath As String) As System.Configuration.Configuration

            Dim configuration As System.Configuration.Configuration = Nothing

            If configFilePath IsNot Nothing Then
                If configFilePath = "" OrElse JustFileExtension(configFilePath) = ".config" Then
                    Select Case Me.Environment()
                        Case ApplicationEnvironment.Win
                            If configFilePath = "" OrElse _
                                    (configFilePath <> "" AndAlso configFilePath.EndsWith(".exe.config")) Then
                                ' Path to the exe is to be provided in order to open the configuration file
                                ' associated with the exe.
                                configuration = ConfigurationManager.OpenExeConfiguration(configFilePath.TrimEnd(".config".ToCharArray()))
                            Else
                                Throw New ArgumentException("Path of configuration file for windows application must end in '.exe.config'", "configFilePath")
                            End If
                        Case ApplicationEnvironment.Web
                            If configFilePath = "" Then configFilePath = System.Web.HttpContext.Current.Request.ApplicationPath()
                            configuration = WebConfigurationManager.OpenWebConfiguration(configFilePath.TrimEnd("web.config".ToCharArray()))
                    End Select
                Else
                    Throw New ArgumentException("Path of configuration file must be either empty or end in '.config'")
                End If
            Else
                Throw New ArgumentNullException("configFilePath", "Path of configuration file path cannot be null")
            End If

            Return configuration

        End Function

        Private Sub CreateConfigurationFile(ByVal configFilePath As String)

            If Not String.IsNullOrEmpty(configFilePath) Then
                Dim configFileWriter As New XmlTextWriter(configFilePath, System.Text.Encoding.UTF8)
                configFileWriter.Indentation = 4
                configFileWriter.Formatting = Formatting.Indented
                ' Populate the very basic information required in a config file.
                configFileWriter.WriteStartDocument()
                configFileWriter.WriteStartElement("configuration")
                configFileWriter.WriteEndElement()
                configFileWriter.WriteEndDocument()
                ' Close the config file.
                configFileWriter.Close()

                ValidateConfigurationFile(configFilePath)
            Else
                Throw New ArgumentNullException("configFilePath", "Path of configuration file path cannot be null")
            End If

        End Sub

        Private Sub ValidateConfigurationFile(ByVal configFilePath As String)

            If Not String.IsNullOrEmpty(configFilePath) Then
                Dim configFile As New XmlDocument()
                configFile.Load(configFilePath)

                If configFile.DocumentElement.SelectNodes("configSections").Count() = 0 Then
                    configFile.DocumentElement.InsertBefore(configFile.CreateElement("configSections"), _
                        configFile.DocumentElement.FirstChild())
                End If
                Dim configSectionsNode As XmlNode = configFile.DocumentElement.SelectSingleNode("configSections")
                If configSectionsNode.SelectNodes("section[@name = '" & CustomSectionName & "']").Count() = 0 Then
                    Dim node As XmlNode = configFile.CreateElement("section")
                    Attribute(node, "name") = CustomSectionName
                    Attribute(node, "type") = CustomSectionType
                    configSectionsNode.AppendChild(node)
                End If
                configFile.Save(configFilePath)
            Else
                Throw New ArgumentNullException("configFilePath", "Path of configuration file path cannot be null")
            End If

        End Sub

    End Class

End Namespace