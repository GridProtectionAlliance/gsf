Imports System.Configuration
Imports System.IO

Public NotInheritable Class SplashScreen

    Private Sub SplashScreen_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        ' Set up the dialog text at runtime according to the application's assembly information
        ApplicationTitle.Text = My.Application.Info.Title

        With My.Application.Info.Version
            Version.Text = "Version " & .Major & "." & .Minor & "." & .Build & "." & .Revision
        End With

        Copyright.Text = My.Application.Info.Copyright

    End Sub

End Class