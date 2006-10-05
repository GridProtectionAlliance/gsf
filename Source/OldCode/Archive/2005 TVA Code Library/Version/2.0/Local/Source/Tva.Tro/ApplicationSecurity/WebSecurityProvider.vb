' 09-26-06

Imports System.IO
Imports System.Net
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

        Public Overrides Sub LogoutUser()

            If MyBase.User IsNot Nothing AndAlso m_parent IsNot Nothing Then
                m_parent.Session.Remove("u")
                m_parent.Session.Remove("p")

                With New StringBuilder()
                    .Append(m_parent.Request.Url.AbsolutePath)
                    .Append("?")
                    For Each parameter As String In m_parent.Request.Url.Query.TrimStart("?"c).Split("&"c)
                        Dim key As String = parameter.Split("="c)(0)
                        If Not (key = "u" OrElse key = "p") Then
                            .Append(parameter)
                        End If
                    Next

                    m_parent.Response.Redirect(.ToString())
                End With
            End If

        End Sub

        Protected Overrides Function GetUsername() As String

            If m_parent IsNot Nothing Then
                If m_parent.Request("u") IsNot Nothing Then
                    m_parent.Session.Add("u", _
                        Decrypt(m_parent.Server.UrlDecode(m_parent.Request("u").ToString()), Security.Cryptography.EncryptLevel.Level4))
                End If

                If m_parent.Session("u") IsNot Nothing Then
                    Return m_parent.Session("u").ToString()
                Else
                    Return ""
                End If
            Else
                Throw New InvalidOperationException("Parent must be set in order to retrieve the username.")
            End If

        End Function

        Protected Overrides Function GetPassword() As String

            If m_parent IsNot Nothing Then
                If m_parent.Request("p") IsNot Nothing Then
                    m_parent.Session.Add("p", _
                        Decrypt(m_parent.Server.UrlDecode(m_parent.Request("p").ToString()), Security.Cryptography.EncryptLevel.Level4))
                End If

                If m_parent.Session("p") IsNot Nothing Then
                    Return m_parent.Session("p").ToString()
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

                With New StringBuilder()
                    Try
                        Dim getRequest As WebRequest = WebRequest.Create(GetLocalWebSiteUrl() & "Login.aspx")
                        getRequest.Credentials = CredentialCache.DefaultCredentials

                        If getRequest.GetResponse() Is Nothing Then
                            ' We'll redirect to the "Login Page" on predefined remote web site if we're unable
                            ' to request the page locally. This will be case when the developer is debugging the
                            ' the web site from Visual Studio, since the page we're trying to access is embedded.
                            .Append(GetRemoteWebSiteUrl())
                        End If
                    Catch ex As Exception
                        .Append(GetRemoteWebSiteUrl())
                    End Try
                    .Append("Login.aspx?r=")    ' Return Url
                    .Append(m_parent.Server.UrlEncode(m_parent.Request.Url.AbsoluteUri))
                    .Append("&a=")              ' Application Name
                    .Append(m_parent.Server.UrlEncode(Encrypt(MyBase.ApplicationName, Security.Cryptography.EncryptLevel.Level4)))
                    .Append("&c=")              ' Connection String
                    .Append(m_parent.Server.UrlEncode(Encrypt(MyBase.ConnectionString, Security.Cryptography.EncryptLevel.Level4)))

                    m_parent.Response.Redirect(.ToString())
                End With
            Else
                Throw New InvalidOperationException("Parent must be set in order to login the user.")
            End If

        End Sub

        Private Sub ExtractWebFiles()

            If m_parent.Application("AP.WebFilesExtracted") Is Nothing Then
                ' Extract the embedded web files to the the web site's bin directory.
                Try
                    Dim webFiles As ZipFile = Nothing
                    Dim zipFilePath As String = m_parent.Server.MapPath("~/")
                    Dim zipFileName As String = zipFilePath & "WebFiles.dat"
                    File.WriteAllBytes(zipFileName, ReadStream(CallingAssembly.GetEmbeddedResource("WebFiles.dat")))
                    webFiles = ZipFile.Open(zipFileName)
                    webFiles.Extract("*.*", zipFilePath, UpdateOption.ZipFileIsNewer, True)
                    webFiles.Close()
                    File.Delete(zipFileName)
                    m_parent.Application.Add("AP.WebFilesExtracted", True)
                Catch ex As Exception
                    ' We most likely encountered some sort of an access violation exception.
                    Throw New AccessViolationException("Failed to extract the required web files.", ex)
                End Try
            End If

        End Sub

        Private Function GetLocalWebSiteUrl() As String

            With New StringBuilder()
                .Append(m_parent.Request.Url.Scheme)
                .Append(System.Uri.SchemeDelimiter)
                .Append(m_parent.Request.Url.Host)
                .Append(m_parent.Request.ApplicationPath)
                .Append("/")

                Return .ToString()
            End With

        End Function

        Private Function GetRemoteWebSiteUrl() As String

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