Public Class DebugUI

    Private Sub DebugUI_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        ' Emulate service OnStart call...
        SPDCService.ServiceHelper.OnStart(Environment.CommandLine.Split(" "c))
        SPDCService.DisplayStatusMessage(">> Started TVA SPDC in forms based Debug Mode")

    End Sub

    Private Sub DebugUI_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        ' Emulate service OnStop call...
        SPDCService.ServiceHelper.OnStop()

        Global.System.Windows.Forms.Application.Exit()
        End

    End Sub

End Class