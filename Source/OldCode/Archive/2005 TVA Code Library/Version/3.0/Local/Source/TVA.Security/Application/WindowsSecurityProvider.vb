'*******************************************************************************************************
'  TVA.Security.Application.WindowsSecurityProvider.vb - Security provider for windows applications
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  10/03/2006 - Pinal C. Patel
'       Original version of source code generated.
'  05/06/2008 - Pinal C. Patel
'       Removed previous implementation to be implemented when needed.
'
'*******************************************************************************************************

Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Data.SqlClient
Imports TVA.Data.Common
Imports TVA.Security.Application.Controls

Namespace Application

    <ToolboxBitmap(GetType(WindowsSecurityProvider)), DisplayName("WindowsSecurityProvider")> _
    Public Class WindowsSecurityProvider

#Region " Member Declaration "

        Private WithEvents m_parent As System.Windows.Forms.Form

#End Region

#Region " Code Scope: Public Code "

        <Category("Configuration")> _
        Public Property Parent() As Form
            Get
                Return m_parent
            End Get
            Set(ByVal value As Form)
                m_parent = value
            End Set
        End Property

        Public Overrides Sub LogoutUser()

            ' In windows environment, the user never logs in, so logging out is not supported.
            Throw New NotSupportedException()

        End Sub

#End Region

#Region " Code Scope: Protected Code "

        Protected Overrides Sub ShowLoginPrompt()

            Throw New NotImplementedException()

            'If m_parent IsNot Nothing Then
            '    With New AccessDenied()
            '        Dim result As DialogResult = .ShowDialog(m_parent)
            '        Select Case result
            '            Case DialogResult.Yes
            '                ' The user wants to submit a request for access to the application.
            '                Dim connection As SqlConnection = Nothing
            '                Try
            '                    ' Establish connection with the database.
            '                    connection = New SqlConnection(ConnectionString)
            '                    connection.Open()

            '                    ' Submit the access request.
            '                    ExecuteNonQuery("dbo.SubmitAccessRequest", connection, User.Username, ApplicationName)

            '                    ' Show success message.
            '                    MessageBox.Show("Your access request has been submitted and the application will now exit.", _
            '                        "Request Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information)
            '                Catch ex As SqlException
            '                    If ex.Number = 50000 Then
            '                        ' A pending access request for the user already exists.
            '                        MessageBox.Show("A pending access request already exists. The application will now exist.", _
            '                            "Request Pending", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            '                    Else
            '                        ' An unknown exception was encountered.
            '                        With New StringBuilder()
            '                            .Append("Access request could not be submitted due to an exception.")
            '                            .AppendLine()
            '                            .AppendLine()
            '                            .Append(ex.ToString())
            '                            MessageBox.Show(.ToString(), "Access Request", MessageBoxButtons.OK, MessageBoxIcon.Error)
            '                        End With
            '                    End If
            '                Catch ex As Exception
            '                    With New StringBuilder()
            '                        .Append("Access request could not be submitted due to an exception.")
            '                        .AppendLine()
            '                        .AppendLine()
            '                        .Append(ex.ToString())
            '                        MessageBox.Show(.ToString(), "Access Request", MessageBoxButtons.OK, MessageBoxIcon.Error)
            '                    End With
            '                Finally
            '                    If connection IsNot Nothing Then
            '                        ' Close connection with the database.
            '                        connection.Close()
            '                        connection.Dispose()
            '                    End If
            '                End Try
            '            Case DialogResult.No
            '                ' The user wants to exit the application.
            '                MessageBox.Show("The application will now exit.", _
            '                    "Application Exiting", MessageBoxButtons.OK, MessageBoxIcon.Information)
            '        End Select

            '        ' This worked out to be the best way to exit the application.
            '        Process.GetCurrentProcess().Kill()
            '    End With
            'Else
            '    Throw New InvalidOperationException("Parent must be set.")
            'End If

        End Sub

        Protected Overrides Sub HandleAccessGranted()

            Throw New NotImplementedException()

        End Sub

        Protected Overrides Sub HandleAccessDenied()

            Throw New NotImplementedException()

        End Sub

        Protected Overrides Function GetUsername() As String

            Return ""   ' This function will never be called by the base class in windows environment.

        End Function

        Protected Overrides Function GetPassword() As String

            Return ""   ' This function will never be called by the base class in windows environment.

        End Function

#End Region

#Region " Code Scope: Private Methods "

        Private Sub m_parent_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_parent.Load

            If User Is Nothing Then
                LoginUser()
            End If

        End Sub

#End Region

    End Class

End Namespace