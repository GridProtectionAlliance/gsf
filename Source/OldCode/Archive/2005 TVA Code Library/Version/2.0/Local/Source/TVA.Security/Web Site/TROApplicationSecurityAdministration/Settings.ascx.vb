Imports SecurityTableAdapters

Partial Class Settings
    Inherits System.Web.UI.UserControl

    Private settingsAdapter As New SettingsTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            BindToGrid()
            ViewState("Mode") = "Add"
            ViewState("Setting") = ""
        End If
    End Sub

    Private Sub BindToGrid()
        With Me.GridViewSettings
            .DataSource = settingsAdapter.GetSettings
            .DataBind()
        End With
    End Sub

    Private Sub ClearForm()
        Me.TextBoxSettingDescription.Text = ""
        Me.TextBoxSettingName.Text = ""
        Me.TextBoxSettingValue.Text = ""
        ViewState("Mode") = "Add"
        BindToGrid()
    End Sub

    Protected Sub HandleUserClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridViewSettings.RowCommand
        If e.CommandName = "PopulateSettingInfo" Then
            PopulateSettingInfo(e.CommandArgument.ToString)
            ViewState("Mode") = "Edit"
            ViewState("Setting") = e.CommandArgument.ToString
        End If
    End Sub

    Protected Sub PopulateSettingInfo(ByVal settingName As String)

        Dim settings As Security.SettingsDataTable = settingsAdapter.GetSettingsBySettingName(settingName)
        Dim setting As Security.SettingsRow = settings.Rows(0)

        Me.TextBoxSettingName.Text = setting.SettingName
        Me.TextBoxSettingValue.Text = setting.SettingValue
        Me.TextBoxSettingDescription.Text = setting.SettingDescription

    End Sub

    Protected Sub ButtonCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        ClearForm()
    End Sub

    Protected Sub ButtonSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSave.Click
        Dim settingName As String = Me.TextBoxSettingName.Text.Replace("'", "''")
        Dim settingValue As String = Me.TextBoxSettingValue.Text.Replace("'", "''")
        Dim settingDescription As String = Me.TextBoxSettingDescription.Text.Replace("'", "''")

        If ViewState("Mode") = "Add" Then
            settingsAdapter.InsertSettings(settingName, settingName, settingDescription)
        Else
            If Not ViewState("Setting") = "" Then   'if viewstate information is set then use it
                settingsAdapter.UpdateSettings(settingName, settingValue, settingDescription, ViewState("Setting"))
            End If
        End If
        ClearForm()
    End Sub
End Class
