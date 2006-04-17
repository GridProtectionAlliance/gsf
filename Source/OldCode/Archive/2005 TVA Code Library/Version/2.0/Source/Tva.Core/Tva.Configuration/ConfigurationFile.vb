' 04-12-06

Imports System.Xml
Imports System.Configuration
Imports System.Web.Configuration
Imports Tva.Xml.Common
Imports Tva.IO.FilePath

Namespace Configuration

    Public Class ConfigurationFile

        Private m_configuration As System.Configuration.Configuration

        Private Const CustomSectionName As String = "categorizedSettings"
        Private Const CustomSectionType As String = "Tva.Configuration.CategorizedSettingsSection, Tva.Core"

        Public Enum Environments As Integer
            Win
            Web
        End Enum

        Public Sub New()
            Me.New("")
        End Sub

        Public Sub New(ByVal filePath As String)
            m_configuration = GetConfiguration(filePath)
            If m_configuration.HasFile() Then
                ValidateConfigurationFile(m_configuration.FilePath())
            Else
                CreateConfigurationFile(m_configuration.FilePath())
            End If
            m_configuration = GetConfiguration(filePath)
        End Sub

        Public ReadOnly Property Environment() As Environments
            Get
                Dim currentEnvironment As Environments = Environments.Win
                If System.Web.HttpContext.Current() IsNot Nothing Then currentEnvironment = Environments.Web
                Return currentEnvironment
            End Get
        End Property

        Public ReadOnly Property CategorizedSettings() As CategorizedSettingsSection
            Get
                Return DirectCast(m_configuration.GetSection(CustomSectionName), CategorizedSettingsSection)
            End Get
        End Property

        Public ReadOnly Property AppSettings() As AppSettingsSection
            Get
                Return m_configuration.AppSettings()
            End Get
        End Property

        Public ReadOnly Property ConnectionStrings() As ConnectionStringsSection
            Get
                Return m_configuration.ConnectionStrings()
            End Get
        End Property

        Public ReadOnly Property FilePath() As String
            Get
                Return m_configuration.FilePath()
            End Get
        End Property

        Public Sub Save()

            Me.Save(ConfigurationSaveMode.Modified)

        End Sub

        Public Sub Save(ByVal saveMode As ConfigurationSaveMode)

            m_configuration.Save(saveMode)

        End Sub


        Public Sub SaveAs(ByVal fileName As String)

            m_configuration.SaveAs(fileName)

        End Sub

        Public Function GetConfiguration(ByVal filePath As String) As System.Configuration.Configuration

            Dim configuration As System.Configuration.Configuration = Nothing

            If filePath IsNot Nothing Then
                If filePath = "" OrElse JustFileExtension(filePath) = ".config" Then
                    Select Case Me.Environment()
                        Case Environments.Win
                            If filePath = "" OrElse _
                                    (filePath <> "" AndAlso JustFileExtension(NoFileExtension(filePath)) = ".exe") Then
                                configuration = ConfigurationManager.OpenExeConfiguration(filePath.TrimEnd(".config".ToCharArray()))
                            Else
                                Throw New ArgumentException()
                            End If
                        Case Environments.Web
                            configuration = WebConfigurationManager.OpenWebConfiguration(filePath)
                    End Select
                Else
                    Throw New ArgumentException()
                End If
            Else
                Throw New ArgumentNullException()
            End If

            Return configuration

        End Function

        Private Sub CreateConfigurationFile(ByVal filePath As String)

            Dim configFileWriter As New XmlTextWriter(filePath, System.Text.Encoding.UTF8)
            configFileWriter.Indentation = 4
            configFileWriter.Formatting = Formatting.Indented
            ' Populate the very basic information required in a config file.
            configFileWriter.WriteStartDocument()
            configFileWriter.WriteStartElement("configuration")
            configFileWriter.WriteEndElement()
            configFileWriter.WriteEndDocument()
            ' Close the config file.
            configFileWriter.Close()

            ValidateConfigurationFile(filePath)

        End Sub

        Private Sub ValidateConfigurationFile(ByVal filePath As String)

            Dim configFile As New XmlDocument()
            configFile.Load(filePath)

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
            configFile.Save(filePath)

        End Sub

    End Class

End Namespace