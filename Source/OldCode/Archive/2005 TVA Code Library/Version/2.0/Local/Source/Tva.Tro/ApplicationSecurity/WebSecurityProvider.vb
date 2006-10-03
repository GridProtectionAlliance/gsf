' 09-26-06

Imports System.IO
Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.Assembly
Imports Tva.IO.Common
Imports Tva.IO.Compression
Imports Tva.Security.Cryptography.Common

Namespace ApplicationSecurity

    <ToolboxBitmap(GetType(WebSecurityProvider))> _
    Public Class WebSecurityProvider

        Private WithEvents m_parent As System.Web.UI.Page

        <Category("Configuration")> _
        Public Property Parent() As System.Web.UI.Page
            Get
                Return m_parent
            End Get
            Set(ByVal value As System.Web.UI.Page)
                If value IsNot Nothing Then
                    m_parent = value
                Else
                    Throw New ArgumentException("Parent cannot be null.")
                End If
            End Set
        End Property

        Protected Overrides Function GetUsername() As String

            If m_parent IsNot Nothing Then
                If m_parent.Request("AS.Username") IsNot Nothing Then
                    m_parent.Session.Add("AS.Username", Decrypt(m_parent.Request("AS.Username").ToString(), Security.Cryptography.EncryptLevel.Level4))
                End If

                If m_parent.Session("AS.Username") IsNot Nothing Then
                    Return m_parent.Session("AS.Username").ToString()
                Else
                    Return ""
                End If
            Else
                Throw New InvalidOperationException("Parent must be set in order to retrieve the username.")
            End If

        End Function

        Protected Overrides Function GetPassword() As String

            If m_parent IsNot Nothing Then
                If m_parent.Request("AS.Password") IsNot Nothing Then
                    m_parent.Session.Add("AS.Password", Decrypt(m_parent.Request("AS.Password").ToString(), Security.Cryptography.EncryptLevel.Level4))
                End If

                If m_parent.Session("AS.Password") IsNot Nothing Then
                    Return m_parent.Session("AS.Password").ToString()
                Else
                    Return ""
                End If
            Else
                Throw New InvalidOperationException("Parent must be set in order to retrieve the password.")
            End If

        End Function

        Protected Overrides Sub ShowLoginScreen()

            If m_parent IsNot Nothing Then
                ExtractWebFiles()   ' Make sure that the required web file exist in the application bin directory.

                Try
                    m_parent.Application.Add("AS.ConnectionString", MyBase.ConnectionString)
                    m_parent.Application.Add("AS.ApplicationName", MyBase.ApplicationName)
                    m_parent.Response.Redirect("Login.aspx")
                Catch ex As Exception
                    With New StringBuilder()
                        .Append(GetWebSiteUrl())
                        .Append("Login.aspx?AS.ConnectionString=")
                        .Append(Encrypt(MyBase.ConnectionString, Security.Cryptography.EncryptLevel.Level4))
                        .Append("&AS.ApplicationName=")
                        .Append(Encrypt(MyBase.ApplicationName, Security.Cryptography.EncryptLevel.Level4))

                        m_parent.Response.Redirect(.ToString())
                    End With
                End Try
            Else
                Throw New InvalidOperationException("Parent must be set in order to login the user.")
            End If

        End Sub

        Protected Overrides Sub HandleLoginFailure()

            If m_parent IsNot Nothing Then
                ExtractWebFiles()   ' Make sure that the required web file exist in the application bin directory.

                Try
                    ' We'll try to redirect to the "Access Denied" web page extacted from the embedded zip file.
                    ' However, this will fail when the web site has not been published and the developer is 
                    ' debugging it from Visual Studio.
                    m_parent.Response.Redirect("AccessDenied.aspx")
                Catch ex As Exception
                    m_parent.Response.Redirect(GetWebSiteUrl() & "AccessDenied.aspx")
                End Try
            Else
                Throw New InvalidOperationException("Parent must be set in order to proceed.")
            End If

        End Sub

        Private Sub ExtractWebFiles()

            If m_parent.Application("AP.WebFilesExtracted") Is Nothing Then
                ' Extract the embedded web files to the the web site's bin directory.
                Try
                    Dim webFiles As ZipFile = Nothing
                    Dim zipFilePath As String = m_parent.Server.MapPath("~/bin/")
                    Dim zipFileName As String = zipFilePath & "WebFiles.dat"
                    File.WriteAllBytes(zipFileName, ReadStream(CallingAssembly.GetEmbeddedResource("WebFiles.dat")))
                    webFiles = ZipFile.Open(zipFileName)
                    webFiles.Extract("*.*", zipFilePath, UpdateOption.ZipFileIsNewer)
                    webFiles.Close()
                    File.Delete(zipFileName)
                    m_parent.Application.Add("AP.WebFilesExtracted", True)
                Catch ex As Exception
                    ' We most likely encountered some sort of an access violation exception.
                    Throw New AccessViolationException("Failed to extract the required web files.", ex)
                End Try
            End If

        End Sub

        Private Function GetWebSiteUrl() As String

            With New StringBuilder()
                .Append("http://")
                Select Case MyBase.Server
                    Case SecurityServer.Development
                        .Append("chadesoweb.cha.tva.gov")
                    Case SecurityServer.Acceptance
                        .Append("chaaesoweb.cha.tva.gov")
                    Case SecurityServer.Production
                        .Append("troweb.cha.tva.gov")
                End Select
                .Append("/troapplicationsecurity/")

                Return .ToString()
            End With

        End Function

        Private Sub m_parent_PreLoad(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_parent.PreLoad

            If MyBase.User Is Nothing Then
                ' EndInit() method of the ISupportInitialize interface was not called which in-turn calls the
                ' LoginUser() method, so we'll call LoginUser() over here implicitly before the web page loads.
                LoginUser()
            End If

        End Sub

    End Class

End Namespace