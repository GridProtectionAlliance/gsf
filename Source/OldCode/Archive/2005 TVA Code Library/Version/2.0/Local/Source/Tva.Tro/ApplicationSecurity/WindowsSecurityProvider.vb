' 10-03-06

Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms

Namespace ApplicationSecurity

    <ToolboxBitmap(GetType(WindowsSecurityProvider))> _
    Public Class WindowsSecurityProvider

        Private WithEvents m_parent As System.Windows.Forms.Form

        Public Property Parent() As System.Windows.Forms.Form
            Get
                Return m_parent
            End Get
            Set(ByVal value As System.Windows.Forms.Form)
                If value IsNot Nothing Then
                    m_parent = value
                Else
                    Throw New ArgumentException("Parent cannot be null.")
                End If
            End Set
        End Property

        Protected Overrides Function GetUsername() As String

            Return ""

        End Function

        Protected Overrides Function GetPassword() As String

            Return ""

        End Function

        Protected Overrides Sub ShowLoginScreen()

            HandleLoginFailure()

        End Sub

        Protected Overrides Sub HandleLoginFailure()

            'With New StringBuilder()
            '    .Append("Access to the form is denied.")

            '    MessageBox.Show(.ToString(), "Windows Security Provider", MessageBoxButtons.OK, MessageBoxIcon.Error)
            'End With

            'If m_parent IsNot Nothing Then
            '    m_parent.Close()
            'End If

        End Sub

        Private Sub m_parent_(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles m_parent.Paint

            If MyBase.User Is Nothing Then
                LoginUser()
            End If

        End Sub

    End Class

End Namespace