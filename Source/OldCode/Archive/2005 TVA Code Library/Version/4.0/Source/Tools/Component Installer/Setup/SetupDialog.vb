Imports System.Threading
Imports Microsoft.Win32

Public NotInheritable Class SetupDialog

    Private Sub SetupDialog_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If MsgBox("This setup program installs a Visual Studio package to deploy TVA Code Library components.  Please make sure Visual Studio is not running before you continue.", MsgBoxStyle.Question Or MsgBoxStyle.OkCancel, "TVA Code Library Component Installer") = MsgBoxResult.Ok Then
            InstallationStep.Text = "Installing needed prerequisite package components..."
            Me.Show()

            ' Run project aggregator2
            RunInstallation("ProjectAggregator2.msi", "/quiet")

            InstallationStep.Text = "Installing TVA Visual Studio package component..."

            ' Run package setup
            RunInstallation("PackageSetup.msi", "/quiet")

            InstallationStep.Text = "Registering package component into Visual Studio..."

            ' Get install location of VS 2005
            Dim installDir As String = CType(Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\8.0", "InstallDir", ""), String)

            ' Run devenv.exe /setup
            RunInstallation(installDir & "devenv.exe", "/setup")

            Me.Hide()
            MsgBox("Setup complete.", MsgBoxStyle.Information Or MsgBoxStyle.OkOnly, "TVA Code Library Component Installer")
        End If

        Close()

    End Sub

    Private Sub SetupDialog_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        Global.System.Windows.Forms.Application.Exit()
        End

    End Sub

    Private m_processComplete As Boolean

    Private Sub RunInstallation(ByVal fileName As String)

        m_processComplete = False
        ThreadPool.QueueUserWorkItem(AddressOf RunInstallation, New String() {fileName})

        Do Until m_processComplete
            Application.DoEvents()
        Loop

    End Sub

    Private Sub RunInstallation(ByVal fileName As String, ByVal parameters As String)

        m_processComplete = False
        ThreadPool.QueueUserWorkItem(AddressOf RunInstallation, New String() {fileName, parameters})

        Do Until m_processComplete
            Application.DoEvents()
        Loop

    End Sub

    Private Sub RunInstallation(ByVal state As Object)

        Try
            Dim params As String() = TryCast(state, String())

            If params IsNot Nothing Then
                If params.Length > 1 Then
                    With Process.Start(params(0), params(1))
                        .WaitForExit()
                        Debug.WriteLine(String.Format("{0} exit code = ", params(0), .ExitCode))
                    End With
                ElseIf params.Length > 0 Then
                    With Process.Start(params(0))
                        .WaitForExit()
                        Debug.WriteLine(String.Format("{0} exit code = ", params(0), .ExitCode))
                    End With
                End If
            End If
        Catch
            ' Just rethrow any exceptions - only catching so that we can reset processing
            ' flag in finally clause
            Throw
        Finally
            m_processComplete = True
        End Try

    End Sub

End Class