'*******************************************************************************************************
'  TVA.Configuration.ConfigurationFile.vb - Application Configuration File
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/12/2006 - Pinal C. Patel
'       Generated original version of source code
'  11/14/2006 - Pinal C. Patel
'       Modified the ValidateConfigurationFile to save the config file only it was modified.
'  12/12/2006 - Pinal C. Patel
'       Wrote the TrimEnd function that is being used for initialized the Configuration object.
'
'*******************************************************************************************************

Imports System.Xml
Imports System.Configuration
Imports System.Web.Configuration
Imports TVA.Xml.Common
Imports TVA.IO.FilePath

Namespace Configuration

    ''' <summary>
    ''' Represents a configuration file of a Windows or Web application.
    ''' </summary>
    Public Class ConfigurationFile

        Private m_configuration As System.Configuration.Configuration

        Private Const CustomSectionName As String = "categorizedSettings"
        Private Const CustomSectionType As String = "TVA.Configuration.CategorizedSettingsSection, TVA.Core"

        ''' <summary>
        ''' Initializes a default instance of TVA.Configuration.ConfigurationFile.
        ''' </summary>
        Public Sub New()
            MyClass.New("")
        End Sub

        ''' <summary>
        ''' Initializes an instance of TVA.Configuration.ConfigurationFile for the specified configuration file
        ''' that belongs to a Windows or Web application.
        ''' </summary>
        ''' <param name="configFilePath">Path of the configuration file that belongs to a Windows or Web application.</param>
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
        ''' Gets the TVA.Configuration.CategorizedSettingsSection representing the "categorizedSettings" section of the configuration file.
        ''' </summary>
        ''' <returns>The TVA.Configuration.CategorizedSettingsSection representing the "categorizedSettings" section of the configuration file.</returns>
        Public ReadOnly Property CategorizedSettings() As CategorizedSettingsSection
            Get
                Return DirectCast(m_configuration.GetSection(CustomSectionName), CategorizedSettingsSection)
            End Get
        End Property

        ''' <summary>
        ''' Gets the System.Configuration.AppSettingsSection representing the "appSettings" section of the configuration file.
        ''' </summary>
        ''' <returns>The System.Configuration.AppSettingsSection representing the "appSettings" section of the configuration file.</returns>
        Public ReadOnly Property AppSettings() As AppSettingsSection
            Get
                Return m_configuration.AppSettings()
            End Get
        End Property

        ''' <summary>
        ''' Gets the System.Configuration.ConnectionStringsSection representing the "connectionStrings" section of the configuration file.
        ''' </summary>
        ''' <returns>The System.Configuration.ConnectionStringsSection representing the "connectionStrings" section of the configuration file.</returns>
        Public ReadOnly Property ConnectionStrings() As ConnectionStringsSection
            Get
                Return m_configuration.ConnectionStrings()
            End Get
        End Property

        ''' <summary>
        ''' Gets the physical path to the configuration file represented by this TVA.Configuration.Configuration object.
        ''' </summary>
        ''' <returns>The physical path to the configuration file represented by this TVA.Configuration.ConfigurationFile object.</returns>
        Public ReadOnly Property FilePath() As String
            Get
                Return m_configuration.FilePath()
            End Get
        End Property

        ''' <summary>
        ''' Writes the configuration settings contained within this TVA.Configuration.ConfigurationFile object 
        ''' to the configuration file that it represents.
        ''' </summary>
        Public Sub Save()

            Save(ConfigurationSaveMode.Modified)

        End Sub

        ''' <summary>
        ''' Writes the configuration settings contained within this TVA.Configuration.ConfigurationFile object 
        ''' to the configuration file that it represents.
        ''' </summary>
        ''' <param name="saveMode">A System.Configuration.ConfigurationSaveMode value that determines which property values to save.</param>
        Public Sub Save(ByVal saveMode As ConfigurationSaveMode)

            m_configuration.Save(saveMode)

        End Sub

        ''' <summary>
        ''' Writes the configuration settings contained within this TVA.Configuration.ConfigurationFile object 
        ''' to the specified configuration file.
        ''' </summary>
        ''' <param name="fileName">The path and file name to save the configuration file to.</param>
        Public Sub SaveAs(ByVal fileName As String)

            m_configuration.SaveAs(fileName)

        End Sub

        Private Function GetConfiguration(ByVal configFilePath As String) As System.Configuration.Configuration

            Dim configuration As System.Configuration.Configuration = Nothing

            If configFilePath IsNot Nothing Then
                If configFilePath = "" OrElse JustFileExtension(configFilePath) = ".config" Then
                    ' PCP - 12/12/2006: Using the TrimEnd function to get the correct value that needs to be passed
                    ' to the method call for getting the Configuration object. The previous method (String.TrimEnd()) 
                    ' yielded incorrect output resulting in the Configuration object not being initialized correctly.
                    Select Case TVA.Common.GetApplicationType()
                        Case ApplicationType.WindowsCui, ApplicationType.WindowsGui
                            configuration = ConfigurationManager.OpenExeConfiguration(TrimEnd(configFilePath, ".config"))
                        Case ApplicationType.Web
                            If configFilePath = "" Then configFilePath = System.Web.HttpContext.Current.Request.ApplicationPath()
                            configuration = WebConfigurationManager.OpenWebConfiguration(TrimEnd(configFilePath, "web.config"))
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
                ' Populates the very basic information required in a config file.
                configFileWriter.WriteStartDocument()
                configFileWriter.WriteStartElement("configuration")
                configFileWriter.WriteEndElement()
                configFileWriter.WriteEndDocument()
                ' Closes the config file.
                configFileWriter.Close()

                ValidateConfigurationFile(configFilePath)
            Else
                Throw New ArgumentNullException("configFilePath", "Path of configuration file path cannot be null")
            End If

        End Sub

        Private Sub ValidateConfigurationFile(ByVal configFilePath As String)

            If Not String.IsNullOrEmpty(configFilePath) Then
                Dim configFile As New XmlDocument()
                Dim configFileModified As Boolean = False
                configFile.Load(configFilePath)

                ' Make sure that the config file has the necessary section information under <customSections />
                ' that is required by the .Net configuration API to process our custom <categorizedSettings />
                ' section. The configuration API will raise an exception if it doesn't find this section.
                If configFile.DocumentElement.SelectNodes("configSections").Count() = 0 Then
                    ' Adds a <configSections> node, if one is not present.
                    configFile.DocumentElement.InsertBefore(configFile.CreateElement("configSections"), _
                        configFile.DocumentElement.FirstChild())

                    configFileModified = True
                End If
                Dim configSectionsNode As XmlNode = configFile.DocumentElement.SelectSingleNode("configSections")
                If configSectionsNode.SelectNodes("section[@name = '" & CustomSectionName & "']").Count() = 0 Then
                    ' Adds the <section> node that specifies the DLL that handles the <categorizedSettings> node in
                    ' the config file, if one is not present.
                    Dim node As XmlNode = configFile.CreateElement("section")
                    Attribute(node, "name") = CustomSectionName
                    Attribute(node, "type") = CustomSectionType
                    configSectionsNode.AppendChild(node)

                    configFileModified = True
                End If

                ' 11/14/2006 - PCP: We'll save the config file only it was modified. This will prevent ASP.Net
                ' web sites from restarting every time a configuration element is accessed.
                If configFileModified Then configFile.Save(configFilePath)
            Else
                Throw New ArgumentNullException("configFilePath", "Path of configuration file path cannot be null")
            End If

        End Sub

        Private Function TrimEnd(ByVal stringToTrim As String, ByVal textToTrim As String) As String

            Dim trimEndIndex As Integer = stringToTrim.LastIndexOf(textToTrim)
            If trimEndIndex = -1 Then trimEndIndex = stringToTrim.Length

            Return stringToTrim.Substring(0, trimEndIndex)

        End Function

    End Class

End Namespace