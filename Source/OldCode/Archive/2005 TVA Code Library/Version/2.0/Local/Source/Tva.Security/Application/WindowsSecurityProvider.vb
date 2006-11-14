' 10-03-06

Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Data.SqlClient
Imports Tva.Data.Common

Namespace Application

    <ToolboxBitmap(GetType(WindowsSecurityProvider))> _
    Public Class WindowsSecurityProvider

#Region " Member Declaration "

        Private WithEvents m_parent As System.Windows.Forms.Form

#End Region

#Region " Public Code "

        <Category("Configuration")> _
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

        Public Overrides Sub LogoutUser()

            ' In windows environment, the user never logs in, so logging out is not supported.
            Throw New NotSupportedException()

        End Sub

#End Region

#Region " Protected Code "

        Protected Overrides Sub CacheUserData()

            ' We don't need to implement this method is windows environment.

        End Sub

        Protected Overrides Sub RetrieveUserData()

            ' We don't need to implement this method is windows environment.

        End Sub

        Protected Overrides Sub HandleLoginFailure()

            If m_parent IsNot Nothing Then
                With New AccessDenied()
                    Dim result As DialogResult = .ShowDialog(m_parent)
                    Select Case result
                        Case DialogResult.Yes
                            ' The user wants to submit a request for access to the application.
                            Dim connection As SqlConnection = Nothing
                            Try
                                ' Establish connection with the database.
                                connection = New SqlConnection(MyBase.ConnectionString)
                                connection.Open()

                                ' Submit the access request.
                                ExecuteNonQuery("dbo.SubmitAccessRequest", connection, MyBase.User.Username, MyBase.ApplicationName)

                                ' Show success message.
                                MessageBox.Show("Your access request has been submitted and the application will now exit.", _
                                    "Request Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information)
                            Catch ex As SqlException
                                If ex.Number = 50000 Then
                                    ' A pending access request for the user already exists.
                                    MessageBox.Show("A pending access request already exists. The application will now exist.", _
                                        "Request Pending", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                Else
                                    ' An unknown exception was encountered.
                                    With New StringBuilder()
                                        .Append("Access request could not be submitted due to an exception.")
                                        .Append(Environment.NewLine)
                                        .Append(Environment.NewLine)
                                        .Append(ex.ToString())
                                        MessageBox.Show(.ToString(), "Access Request", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    End With
                                End If
                            Catch ex As Exception
                                With New StringBuilder()
                                    .Append("Access request could not be submitted due to an exception.")
                                    .Append(Environment.NewLine)
                                    .Append(Environment.NewLine)
                                    .Append(ex.ToString())
                                    MessageBox.Show(.ToString(), "Access Request", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                End With
                            Finally
                                If connection IsNot Nothing Then
                                    ' Close connection with the database.
                                    connection.Close()
                                    connection.Dispose()
                                End If
                            End Try
                        Case DialogResult.No
                            ' The user wants to exit the application.
                            MessageBox.Show("The application will now exit.", _
                                "Application Exiting", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End Select

                    ' This worked out to be the best way to exit the application.
                    Process.GetCurrentProcess().Kill()
                End With
            Else
                Throw New InvalidOperationException("Parent must be set.")
            End If

        End Sub

        Protected Overrides Function GetUsername() As String

            Return ""   ' This function will never be called by the base class in windows environment.

        End Function

        Protected Overrides Function GetPassword() As String

            Return ""   ' This function will never be called by the base class in windows environment.

        End Function

#End Region

#Region " Private Methods "

        Private Sub m_parent_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_parent.Load

            If MyBase.User Is Nothing Then
                LoginUser()
            End If

        End Sub

#End Region

    End Class

End Namespace