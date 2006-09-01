'*******************************************************************************************************
'  Tva.Configuration.Common.vb - Common Configuration Functions
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
'  04/13/2006 - Pinal C. Patel
'       Original version of source code generated
'  05/19/2006 - J. Ritchie Carroll
'       Added type-safe "native type" settings shortcut functions
'  09/01/2006 - J. Ritchie Carroll
'       Added set property to shortcut functions that didn't take default value parameter
'
'*******************************************************************************************************

Namespace Configuration

    ''' <summary>
    ''' Defines common shared configuration file related functions.
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

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

#Region " General Settings Shortcuts "

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

#Region " Coerced Native Type Setting Access Shortcuts "

        Public Shared Property BooleanSetting(ByVal name As String) As Boolean
            Get
                Return BooleanSetting(name, False)
            End Get
            Set(ByVal value As Boolean)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property BooleanSetting(ByVal name As String, ByVal defaultValue As Boolean) As Boolean
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property ByteSetting(ByVal name As String) As Byte
            Get
                Return ByteSetting(name, Byte.MinValue)
            End Get
            Set(ByVal value As Byte)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property ByteSetting(ByVal name As String, ByVal defaultValue As Byte) As Byte
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property SByteSetting(ByVal name As String) As SByte
            Get
                Return SByteSetting(name, SByte.MinValue)
            End Get
            Set(ByVal value As SByte)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property SByteSetting(ByVal name As String, ByVal defaultValue As SByte) As SByte
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CharSetting(ByVal name As String) As Char
            Get
                Return CharSetting(name, Char.MinValue)
            End Get
            Set(ByVal value As Char)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CharSetting(ByVal name As String, ByVal defaultValue As Char) As Char
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property IntegerSetting(ByVal name As String) As Integer
            Get
                Return IntegerSetting(name, Integer.MinValue)
            End Get
            Set(ByVal value As Integer)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property IntegerSetting(ByVal name As String, ByVal defaultValue As Integer) As Integer
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property UIntegerSetting(ByVal name As String) As UInteger
            Get
                Return UIntegerSetting(name, UInteger.MinValue)
            End Get
            Set(ByVal value As UInteger)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property UIntegerSetting(ByVal name As String, ByVal defaultValue As UInteger) As UInteger
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property LongSetting(ByVal name As String) As Long
            Get
                Return LongSetting(name, Long.MinValue)
            End Get
            Set(ByVal value As Long)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property LongSetting(ByVal name As String, ByVal defaultValue As Long) As Long
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property ULongSetting(ByVal name As String) As ULong
            Get
                Return ULongSetting(name, ULong.MinValue)
            End Get
            Set(ByVal value As ULong)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property ULongSetting(ByVal name As String, ByVal defaultValue As ULong) As ULong
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property Int16Setting(ByVal name As String) As Int16
            Get
                Return Int16Setting(name, Int16.MinValue)
            End Get
            Set(ByVal value As Int16)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property Int16Setting(ByVal name As String, ByVal defaultValue As Int16) As Int16
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property Int32Setting(ByVal name As String) As Int32
            Get
                Return Int32Setting(name, Int32.MinValue)
            End Get
            Set(ByVal value As Int32)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property Int32Setting(ByVal name As String, ByVal defaultValue As Int32) As Int32
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property Int64Setting(ByVal name As String) As Int64
            Get
                Return Int64Setting(name, Int64.MinValue)
            End Get
            Set(ByVal value As Int64)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property Int64Setting(ByVal name As String, ByVal defaultValue As Int64) As Int64
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property UInt16Setting(ByVal name As String) As UInt16
            Get
                Return UInt16Setting(name, UInt16.MinValue)
            End Get
            Set(ByVal value As UInt16)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property UInt16Setting(ByVal name As String, ByVal defaultValue As UInt16) As UInt16
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property UInt32Setting(ByVal name As String) As UInt32
            Get
                Return UInt32Setting(name, UInt32.MinValue)
            End Get
            Set(ByVal value As UInt32)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property UInt32Setting(ByVal name As String, ByVal defaultValue As UInt32) As UInt32
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property UInt64Setting(ByVal name As String) As UInt64
            Get
                Return UInt64Setting(name, UInt64.MinValue)
            End Get
            Set(ByVal value As UInt64)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property UInt64Setting(ByVal name As String, ByVal defaultValue As UInt64) As UInt64
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property SingleSetting(ByVal name As String) As Single
            Get
                Return SingleSetting(name, Single.MinValue)
            End Get
            Set(ByVal value As Single)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property SingleSetting(ByVal name As String, ByVal defaultValue As Single) As Single
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property DoubleSetting(ByVal name As String) As Double
            Get
                Return DoubleSetting(name, Double.MinValue)
            End Get
            Set(ByVal value As Double)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property DoubleSetting(ByVal name As String, ByVal defaultValue As Double) As Double
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property DecimalSetting(ByVal name As String) As Decimal
            Get
                Return DecimalSetting(name, Decimal.MinValue)
            End Get
            Set(ByVal value As Decimal)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property DecimalSetting(ByVal name As String, ByVal defaultValue As Decimal) As Decimal
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property DateTimeSetting(ByVal name As String) As Date
            Get
                Return DateTimeSetting(name, Date.MinValue)
            End Get
            Set(ByVal value As Date)
                Settings(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property DateTimeSetting(ByVal name As String, ByVal defaultValue As Date) As Date
            Get
                Return Settings(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property StringSetting(ByVal name As String) As String
            Get
                Return StringSetting(name, "")
            End Get
            Set(ByVal value As String)
                Settings(name).Value = value
            End Set
        End Property

        Public Shared ReadOnly Property StringSetting(ByVal name As String, ByVal defaultValue As String) As String
            Get
                Dim setting As String = Settings(name).Value

                If String.IsNullOrEmpty(setting) Then
                    Return defaultValue
                Else
                    Return setting
                End If
            End Get
        End Property

        Public Shared Property CategorizedBooleanSetting(ByVal category As String, ByVal name As String) As Boolean
            Get
                Return CategorizedBooleanSetting(category, name, False)
            End Get
            Set(ByVal value As Boolean)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedBooleanSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Boolean) As Boolean
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedByteSetting(ByVal category As String, ByVal name As String) As Byte
            Get
                Return CategorizedByteSetting(category, name, Byte.MinValue)
            End Get
            Set(ByVal value As Byte)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedByteSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Byte) As Byte
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property CategorizedSByteSetting(ByVal category As String, ByVal name As String) As SByte
            Get
                Return CategorizedSByteSetting(category, name, SByte.MinValue)
            End Get
            Set(ByVal value As SByte)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property CategorizedSByteSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As SByte) As SByte
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedCharSetting(ByVal category As String, ByVal name As String) As Char
            Get
                Return CategorizedCharSetting(category, name, Char.MinValue)
            End Get
            Set(ByVal value As Char)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedCharSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Char) As Char
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedIntegerSetting(ByVal category As String, ByVal name As String) As Integer
            Get
                Return CategorizedIntegerSetting(category, name, Integer.MinValue)
            End Get
            Set(ByVal value As Integer)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedIntegerSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Integer) As Integer
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property CategorizedUIntegerSetting(ByVal category As String, ByVal name As String) As UInteger
            Get
                Return CategorizedUIntegerSetting(category, name, UInteger.MinValue)
            End Get
            Set(ByVal value As UInteger)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property CategorizedUIntegerSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As UInteger) As UInteger
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedLongSetting(ByVal category As String, ByVal name As String) As Long
            Get
                Return CategorizedLongSetting(category, name, Long.MinValue)
            End Get
            Set(ByVal value As Long)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedLongSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Long) As Long
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property CategorizedULongSetting(ByVal category As String, ByVal name As String) As ULong
            Get
                Return CategorizedULongSetting(category, name, ULong.MinValue)
            End Get
            Set(ByVal value As ULong)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property CategorizedULongSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As ULong) As ULong
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedInt16Setting(ByVal category As String, ByVal name As String) As Int16
            Get
                Return CategorizedInt16Setting(category, name, Int16.MinValue)
            End Get
            Set(ByVal value As Int16)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedInt16Setting(ByVal category As String, ByVal name As String, ByVal defaultValue As Int16) As Int16
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedInt32Setting(ByVal category As String, ByVal name As String) As Int32
            Get
                Return CategorizedInt32Setting(category, name, Int32.MinValue)
            End Get
            Set(ByVal value As Int32)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedInt32Setting(ByVal category As String, ByVal name As String, ByVal defaultValue As Int32) As Int32
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedInt64Setting(ByVal category As String, ByVal name As String) As Int64
            Get
                Return CategorizedInt64Setting(category, name, Int64.MinValue)
            End Get
            Set(ByVal value As Int64)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedInt64Setting(ByVal category As String, ByVal name As String, ByVal defaultValue As Int64) As Int64
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property CategorizedUInt16Setting(ByVal category As String, ByVal name As String) As UInt16
            Get
                Return CategorizedUInt16Setting(category, name, UInt16.MinValue)
            End Get
            Set(ByVal value As UInt16)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property CategorizedUInt16Setting(ByVal category As String, ByVal name As String, ByVal defaultValue As UInt16) As UInt16
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property CategorizedUInt32Setting(ByVal category As String, ByVal name As String) As UInt32
            Get
                Return CategorizedUInt32Setting(category, name, UInt32.MinValue)
            End Get
            Set(ByVal value As UInt32)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property CategorizedUInt32Setting(ByVal category As String, ByVal name As String, ByVal defaultValue As UInt32) As UInt32
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        <CLSCompliant(False)> _
        Public Shared Property CategorizedUInt64Setting(ByVal category As String, ByVal name As String) As UInt64
            Get
                Return CategorizedUInt64Setting(category, name, UInt64.MinValue)
            End Get
            Set(ByVal value As UInt64)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        <CLSCompliant(False)> _
        Public Shared ReadOnly Property CategorizedUInt64Setting(ByVal category As String, ByVal name As String, ByVal defaultValue As UInt64) As UInt64
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedSingleSetting(ByVal category As String, ByVal name As String) As Single
            Get
                Return CategorizedSingleSetting(category, name, Single.MinValue)
            End Get
            Set(ByVal value As Single)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedSingleSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Single) As Single
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedDoubleSetting(ByVal category As String, ByVal name As String) As Double
            Get
                Return CategorizedDoubleSetting(category, name, Double.MinValue)
            End Get
            Set(ByVal value As Double)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedDoubleSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Double) As Double
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedDecimalSetting(ByVal category As String, ByVal name As String) As Decimal
            Get
                Return CategorizedDecimalSetting(category, name, Decimal.MinValue)
            End Get
            Set(ByVal value As Decimal)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedDecimalSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Decimal) As Decimal
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedDateTimeSetting(ByVal category As String, ByVal name As String) As Date
            Get
                Return CategorizedDateTimeSetting(category, name, Date.MinValue)
            End Get
            Set(ByVal value As Date)
                CategorizedSettings(category)(name).Value = value.ToString()
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedDateTimeSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As Date) As Date
            Get
                Return CategorizedSettings(category)(name).GetTypedValue(defaultValue)
            End Get
        End Property

        Public Shared Property CategorizedStringSetting(ByVal category As String, ByVal name As String) As String
            Get
                Return CategorizedStringSetting(category, name, "")
            End Get
            Set(ByVal value As String)
                CategorizedSettings(category)(name).Value = value
            End Set
        End Property

        Public Shared ReadOnly Property CategorizedStringSetting(ByVal category As String, ByVal name As String, ByVal defaultValue As String) As String
            Get
                Dim setting As String = CategorizedSettings(category)(name).Value

                If String.IsNullOrEmpty(setting) Then
                    Return defaultValue
                Else
                    Return setting
                End If
            End Get
        End Property

#End Region

    End Class

End Namespace
