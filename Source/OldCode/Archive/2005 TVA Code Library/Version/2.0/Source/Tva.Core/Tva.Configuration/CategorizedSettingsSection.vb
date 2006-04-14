' 04-11-06

Imports System.Xml
Imports System.Configuration

Namespace Configuration

    Public Class CategorizedSettingsSection
        Inherits ConfigurationSection

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
                Return DirectCast(MyBase.Item(configProperty), CategorizedSettingsCollection)
            End Get
        End Property

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
            For Each category As XmlNode In configSection.DocumentElement.SelectNodes("*")
                MyBase.Properties.Add(New ConfigurationProperty(category.Name(), GetType(CategorizedSettingsCollection)))
            Next
            configSectionStream.Seek(0, System.IO.SeekOrigin.Begin)

            MyBase.DeserializeSection(XmlReader.Create(configSectionStream))

        End Sub

    End Class

End Namespace
