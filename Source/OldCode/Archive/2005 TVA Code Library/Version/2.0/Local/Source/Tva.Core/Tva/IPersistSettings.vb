' 03/21/2007

Public Interface IPersistsSettings

    Property PersistSettings() As Boolean

    Property ConfigurationCategory() As String

    Sub SaveSettings()

    Sub LoadSettings()

End Interface
