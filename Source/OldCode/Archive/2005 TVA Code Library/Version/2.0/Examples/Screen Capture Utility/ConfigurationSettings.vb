Imports System.Configuration

Public Class ConfigurationSettings

    Private Shared m_configFile As Configuration

    Public Shared ReadOnly Property ConfigFile() As Configuration
        Get
            If m_configFile Is Nothing Then m_configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
            Return m_configFile
        End Get
    End Property

    Public Shared Sub AddSetting(ByVal name As String, ByVal value As String)

        If ConfigFile.AppSettings.Settings(name) Is Nothing Then ConfigFile.AppSettings.Settings.Add(name, value)

    End Sub

    Public Shared Property BooleanSettings(ByVal name As String) As Boolean
        Get
            Return Convert.ToBoolean(Settings(name))
        End Get
        Set(ByVal value As Boolean)
            Settings(name) = value
        End Set
    End Property

    Public Shared Property IntegerSettings(ByVal name As String) As Integer
        Get
            Return Convert.ToInt32(Settings(name))
        End Get
        Set(ByVal value As Integer)
            Settings(name) = value
        End Set
    End Property

    Public Shared Property Settings(ByVal name As String) As String
        Get
            Return ConfigFile.AppSettings.Settings(name).Value
            'Return ConfigurationManager.AppSettings(name)
        End Get
        Set(ByVal value As String)
            ConfigFile.AppSettings.Settings(name).Value = value
            'ConfigurationManager.AppSettings(name) = value
        End Set
    End Property

    Public Shared Sub SaveSettings()

        ' Save the configuration file
        ConfigFile.Save(ConfigurationSaveMode.Full)

        ' Force a reload of a changed section
        ConfigurationManager.RefreshSection("appSettings")

    End Sub

End Class
