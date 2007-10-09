' 03/21/2007

Public Interface IPersistSettings

    Property PersistSettings() As Boolean

    Property SettingsCategoryName() As String

    Sub SaveSettings()

    Sub LoadSettings()

End Interface
