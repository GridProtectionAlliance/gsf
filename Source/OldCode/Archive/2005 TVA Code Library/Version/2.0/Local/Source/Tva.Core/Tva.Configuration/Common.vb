'*******************************************************************************************************
'  Tva.Configuration.Common.vb - Common Configuration Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/13/2006 - Pinal C Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Namespace Configuration

    ''' <summary>
    ''' Defines common shared configuration file related functions.
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class Common

        Private Sub New()

        End Sub

        Private Shared m_defaultConfigFile As ConfigurationFile

        ''' <summary>
        ''' Gets or sets the Tva.Configuration.ConfigurationFile object that represents the configuration
        ''' file of the currently executing windows or web application.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' The Tva.Configuration.ConfigurationFile object that represents the configuration file of the 
        ''' currently executing windows or web application.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this property to access the settings saved under the "appSettings" and "connectionStrings"
        ''' sections provided by the .Net framework in addition to the "categorizedSettings" sections.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <code>
        ''' With DefaultConfigFile()
        '''     ' Adds setting to the "appSettings" section of the config file.
        '''     .AppSettings.Settings.Add("SaveSettingsOnExit", "1")
        ''' 
        '''     ' Adds setting to the "connectionStrings" section of the config file.
        '''     .ConnectionStrings.ConnectionStrings.Add(New ConnectionStringSettings("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True"))
        ''' 
        '''     ' Adds settings to the "categorizedSettings" section of the config file.
        '''     .CategorizedSettings.General.Add("SaveSettingsOnExit", "1")
        '''     .CategorizedSettings("Development").Add("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True", True)
        '''     .CategorizedSettings("Production").Add("DbConnectString", "Server=OPDATSQL;Database=DB;Trusted_Connection=True", True)
        ''' 
        '''     ' Saves modified settings to the config file.
        '''     .Save()
        ''' End With
        ''' </code>
        ''' </para>
        ''' </remarks>
        Public Shared Property DefaultConfigFile() As ConfigurationFile
            Get
                If m_defaultConfigFile Is Nothing Then m_defaultConfigFile = New ConfigurationFile()
                Return m_defaultConfigFile
            End Get
            Set(ByVal value As ConfigurationFile)
                m_defaultConfigFile = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the Tva.Configuration.ConfigurationFile object that represent the specified configuration
        ''' file that belongs a windows or web application.
        ''' </summary>
        ''' <param name="filePath">Path of the configuration file that belongs to a windows or web application.</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' <para>
        ''' Use this property for accessing the configuration files of other windows or web applications.
        ''' This will also give access to settings saved under the "appSettings" and "connectionStrings"
        ''' sections provided by the .Net framework in addition to the "categorizedSettings" sections.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <code>
        ''' ' Manupulating the configuration file of another windows application.
        ''' With CustomConfigFile("C:\Projects\WindowsApplication1\bin\Debug\WindowsApplication1.exe.config")
        '''     ' Adds setting to the "appSettings" section of the config file.
        '''     .AppSettings.Settings.Add("SaveSettingsOnExit", "1")
        ''' 
        '''     ' Adds setting to the "connectionStrings" section of the config file.
        '''     .ConnectionStrings.ConnectionStrings.Add(New ConnectionStringSettings("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True"))
        ''' 
        '''     ' Adds settings to the "categorizedSettings" section of the config file.
        '''     .CategorizedSettings.General.Add("SaveSettingsOnExit", "1")
        '''     .CategorizedSettings("Development").Add("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True", True)
        '''     .CategorizedSettings("Production").Add("DbConnectString", "Server=OPDATSQL;Database=DB;Trusted_Connection=True", True)
        ''' 
        '''     ' Saves modified settings to the config file.
        '''     .Save()
        ''' End With
        ''' 
        ''' ' Manupulating the configuration file of another web application.
        ''' With CustomConfigFile("/WebApplication1/web.config")
        '''     ' Adds setting to the "appSettings" section of the config file.
        '''     .AppSettings.Settings.Add("SaveSettingsOnExit", "1")
        ''' 
        '''     ' Adds setting to the "connectionStrings" section of the config file.
        '''     .ConnectionStrings.ConnectionStrings.Add(New ConnectionStringSettings("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True"))
        ''' 
        '''     ' Adds settings to the "categorizedSettings" section of the config file.
        '''     .CategorizedSettings.General.Add("SaveSettingsOnExit", "1")
        '''     .CategorizedSettings("Development").Add("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True", True)
        '''     .CategorizedSettings("Production").Add("DbConnectString", "Server=OPDATSQL;Database=DB;Trusted_Connection=True", True)
        ''' 
        '''     ' Saves modified settings to the config file.
        '''     .Save()
        ''' End With
        ''' </code>
        ''' </para>
        ''' </remarks>
        Public Shared ReadOnly Property CustomConfigFile(ByVal filePath As String) As ConfigurationFile
            Get
                Return New ConfigurationFile(filePath)
            End Get
        End Property

#Region " Config Shortcuts "

        ''' <summary>
        ''' Gets the Tva.Configuration.CategorizedSettingsCollection representing the settings under "general"
        ''' category of the "categorizedSettings" section within the default configuration file.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' The Tva.Configuration.CategorizedSettingsCollection representing the settings under "general"
        ''' category of the "categorizedSettings" section.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This property is meant to be a shortcut for accessing the settings under the "general" category of 
        ''' "categorizedSettings" section within the configuration file. In order to access the settings under 
        ''' "appSettings" and "connectionStrings" sections, use either DefaultConfigFile() or CustomConfigFile() 
        ''' property.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <code>
        ''' Settings.Add("SaveSettingsOnExit", "1") ' Add a new setting to the "general" category.
        ''' SaveSettings() ' Propogate the changes to the config file.
        ''' 
        ''' Settings("SaveSettingsOnExit").Value() ' Read an existing setting from the "general" category.
        ''' </code>
        ''' </para>
        ''' </remarks>
        Public Shared ReadOnly Property Settings() As CategorizedSettingsCollection
            Get
                Return DefaultConfigFile.CategorizedSettings.General
            End Get
        End Property

        ''' <summary>
        ''' Gets the Tva.Configuration.CategorizedSettingsCollection representing the settings under the specified
        ''' category of the "categorizedSettings" section within the default configuration file.
        ''' </summary>
        ''' <param name="category">The name of the category whose settings are to be retreived.</param>
        ''' <value></value>
        ''' <returns>
        ''' The Tva.Configuration.CategorizedSettingsCollection representing the settings under the specified
        ''' category of the "categorizedSettings" section.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This property is meant to be a shortcut for accessing the settings under the various categories,
        ''' including "general", of "categorizedSettings" section within the configuration file. In order to access 
        ''' the settings under "appSettings" and "connectionStrings" sections, use either DefaultConfigFile() or 
        ''' CustomConfigFile() property.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <code>
        ''' CategorizedSettings("Development").Add("DbConnectString", "Server=RGOCSQLD;Database=DB;Trusted_Connection=True", True) ' Add a new setting to the "development" category.
        ''' SaveSettings() ' Propogate the changes to the config file.
        ''' 
        ''' CategorizedSettings("Development")("DbConnectString").Value() ' Read an existing setting from the "development" category.
        ''' </code>
        ''' </para>
        ''' </remarks>
        Public Shared ReadOnly Property CategorizedSettings(ByVal category As String) As CategorizedSettingsCollection
            Get
                Return DefaultConfigFile.CategorizedSettings(category)
            End Get
        End Property

        ''' <summary>
        ''' Writes the modified configuration settings to the default configuration file.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub SaveSettings()
            DefaultConfigFile.Save()
        End Sub

#End Region

    End Class

End Namespace
