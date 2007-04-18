Option Strict On

Imports System.Drawing

Namespace ErrorManagement

    Public Class GelDialog

        Private Const Spacing As Integer = 10

        Private Sub GelDialog_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

            Me.TopMost = True
            Me.TopMost = False

            '-- More >> has to be expanded
            RichTextBoxMoreInfo.Anchor = System.Windows.Forms.AnchorStyles.None
            RichTextBoxMoreInfo.Visible = False

            '-- size the labels' height to accommodate the amount of text in them
            SizeBox(RichTextBoxScope)
            SizeBox(RichTextBoxAction)
            SizeBox(RichTextBoxError)

            '-- now shift everything up
            LabelScope.Top = RichTextBoxError.Top + RichTextBoxError.Height + Spacing
            RichTextBoxScope.Top = LabelScope.Top + LabelScope.Height + Spacing

            LabelAction.Top = RichTextBoxScope.Top + RichTextBoxScope.Height + Spacing
            RichTextBoxAction.Top = LabelAction.Top + LabelAction.Height + Spacing

            LabelMoreInfo.Top = RichTextBoxAction.Top + RichTextBoxAction.Height + Spacing
            ButtonMore.Top = LabelMoreInfo.Top - 3

            Me.Height = ButtonMore.Top + ButtonMore.Height + Spacing + 45

            Me.CenterToScreen()

        End Sub

        Private Sub ButtonMore_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMore.Click

            If ButtonMore.Text = ">>" Then
                Me.Height = Me.Height + 300
                With RichTextBoxMoreInfo
                    .Location = New System.Drawing.Point(LabelMoreInfo.Left, LabelMoreInfo.Top + LabelMoreInfo.Height + Spacing)
                    .Height = Me.ClientSize.Height - RichTextBoxMoreInfo.Top - 45
                    .Width = Me.ClientSize.Width - 2 * Spacing
                    .Anchor = Windows.Forms.AnchorStyles.Top Or Windows.Forms.AnchorStyles.Bottom _
                                Or Windows.Forms.AnchorStyles.Left Or Windows.Forms.AnchorStyles.Right
                    .Visible = True
                End With
                ButtonOK.Focus()
                ButtonMore.Text = "<<"
            Else
                Me.SuspendLayout()
                ButtonMore.Text = ">>"
                Me.Height = ButtonMore.Top + ButtonMore.Height + Spacing + 45
                RichTextBoxMoreInfo.Visible = False
                RichTextBoxMoreInfo.Anchor = Windows.Forms.AnchorStyles.None
                Me.ResumeLayout()
            End If

        End Sub

        Private Sub SizeBox(ByVal ctl As System.Windows.Forms.RichTextBox)

            Dim g As Graphics
            Try
                '-- note that the height is taken as MAXIMUM, so size the label for maximum desired height!
                g = Graphics.FromHwnd(ctl.Handle)
                Dim objSizeF As SizeF = g.MeasureString(ctl.Text, ctl.Font, New SizeF(ctl.Width, ctl.Height))
                g.Dispose()
                ctl.Height = Convert.ToInt32(objSizeF.Height) + 5
            Catch ex As System.Security.SecurityException
                '-- do nothing; we can't set control sizes without full trust
            Finally
                If Not g Is Nothing Then g.Dispose()
            End Try

        End Sub

        Private Sub ButtonOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOK.Click

            Me.Close()
            Me.DialogResult = Windows.Forms.DialogResult.OK

        End Sub

    End Class

End Namespace