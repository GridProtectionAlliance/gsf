' 03/21/2007

Namespace Configuration

    Public Interface IPersistSettings

        Property PersistSettings() As Boolean

        Property SettingsCategoryName() As String

        Sub SaveSettings()

        Sub LoadSettings()

    End Interface

End Namespace