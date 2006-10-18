' 10-16-06

Public Class AccessDenied

    Private Sub AccessDenied_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If Me.Owner IsNot Nothing Then
            Me.Font = Me.Owner.Font
            Me.Text = Windows.Forms.Application.ProductName & " - " & Me.Text
        End If

    End Sub

    Private Sub ButtonExitApplication_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonExitApplication.Click

        Me.DialogResult = Windows.Forms.DialogResult.No
        Me.Close()

    End Sub

    Private Sub ButtonRequestAccess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonRequestAccess.Click

        Me.DialogResult = Windows.Forms.DialogResult.Yes
        Me.Close()

    End Sub

End Class