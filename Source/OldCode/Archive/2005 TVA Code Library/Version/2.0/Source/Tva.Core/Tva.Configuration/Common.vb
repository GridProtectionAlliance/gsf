Namespace Configuration

    Public NotInheritable Class Common

        Private Sub New()

        End Sub

        Private Shared m_defaultConfigFile As ConfigurationFile

        Public Shared Property DefaultConfigFile() As ConfigurationFile
            Get
                If m_defaultConfigFile Is Nothing Then m_defaultConfigFile = New ConfigurationFile()
                Return m_defaultConfigFile
            End Get
            Set(ByVal value As ConfigurationFile)
                m_defaultConfigFile = value
            End Set
        End Property

        Public Shared ReadOnly Property CustomConfigFile(ByVal filePath As String) As ConfigurationFile
            Get
                Return New ConfigurationFile(filePath)
            End Get
        End Property

#Region "Config Shortcuts"
        Public Shared ReadOnly Property Settings() As CategorizedSettingsCollection
            Get
                Return DefaultConfigFile.CategorizedSettings.General
            End Get
        End Property

        Public Shared ReadOnly Property CategorizedSettings(ByVal category As String) As CategorizedSettingsCollection
            Get
                Return DefaultConfigFile.CategorizedSettings(category)
            End Get
        End Property

        Public Sub SaveSettings()
            DefaultConfigFile.Save()
        End Sub
#End Region

    End Class

End Namespace
