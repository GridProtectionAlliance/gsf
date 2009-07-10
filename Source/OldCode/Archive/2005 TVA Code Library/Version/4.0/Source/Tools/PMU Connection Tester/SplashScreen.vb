Imports System.Configuration
Imports System.IO
Imports TVA.Reflection.AssemblyInfo

Public NotInheritable Class SplashScreen

    Private Sub SplashScreen_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        With EntryAssembly
            ' Load splash screen image
            MainLayoutPanel.BackgroundImage = New System.Drawing.Bitmap(.GetEmbeddedResource(Me.GetType.Namespace & ".SplashScreen.png"))

            ' Set up the dialog text at runtime according to the application's assembly information
            ApplicationTitle.Text = .Title

            With .Version
                Version.Text = "Version " & .Major & "." & .Minor & "." & .Build & "." & .Revision
            End With

            Copyright.Text = .Copyright
        End With

    End Sub

End Class