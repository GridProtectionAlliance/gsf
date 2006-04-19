' 04-11-06

Imports System.Xml
Imports System.Configuration

Namespace Configuration

    ''' <summary>
    ''' Represents a section in the configuration with one or more Tva.Configuration.CategorizedSettingsCollection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CategorizedSettingsSection
        Inherits ConfigurationSection

        ''' <summary>
        ''' Gets the Tva.Configuration.CategorizedSettingsCollection for the specified category name.
        ''' </summary>
        ''' <param name="name">The name of the Tva.Configuration.CategorizedSettingsCollection to return.</param>
        ''' <value></value>
        ''' <returns>The Tva.Configuration.CategorizedSettingsCollection with the specified name; otherwise null.</returns>
        ''' <remarks></remarks>
        Default Public ReadOnly Property Category(ByVal name As String) As CategorizedSettingsCollection
            Get
                ' We'll add the requested category to the default properties collection so that when 
                ' the settings are saved off to the config file, all of the categories under which 
                ' settings may be saved are processed and saved to the config file. This is essentially 
                ' doing what marking a property with <ConfigurationProperty()> attribute does.
                ' Make the first letter of category name all lower case if not already.
                Dim nameChars() As Char = name.ToCharArray()
                nameChars(0) = Char.ToLower(nameChars(0))
                name = Convert.ToString(nameChars)
                Dim configProperty As New ConfigurationProperty(name, GetType(CategorizedSettingsCollection))

                MyBase.Properties.Add(configProperty)
                Dim settingsCategory As CategorizedSettingsCollection = Nothing
                If MyBase.Item(configProperty) IsNot Nothing Then
                    settingsCategory = DirectCast(MyBase.Item(configProperty), CategorizedSettingsCollection)
                End If
                Return settingsCategory
            End Get
        End Property

        ''' <summary>
        ''' Gets the "general" category under the "categorizedSettings" of the configuration file.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Tva.Configuration.CategorizedSettingsCollection of the "general" category.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property General() As CategorizedSettingsCollection
            Get
                Return Category("general")
            End Get
        End Property

        Protected Overrides Sub DeserializeSection(ByVal reader As System.Xml.XmlReader)

            Dim configSectionStream As New System.IO.MemoryStream()
            Dim configSection As New XmlDocument()
            configSection.Load(reader)
            configSection.Save(configSectionStream)
            ' Add all the categories that are under the categorizedSettings section of the configuration file
            ' to the property collection. Again, this is essentially doing what marking a property with 
            ' <ConfigurationProperty()> attribute does. If this is not done then exception will be raised
            ' when the category elements are being deserialized.
            For Each category As XmlNode In configSection.DocumentElement.SelectNodes("*")
                MyBase.Properties.Add(New ConfigurationProperty(category.Name(), GetType(CategorizedSettingsCollection)))
            Next
            configSectionStream.Seek(0, System.IO.SeekOrigin.Begin)

            MyBase.DeserializeSection(XmlReader.Create(configSectionStream))

        End Sub

    End Class

End Namespace
