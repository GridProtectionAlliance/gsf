'*******************************************************************************************************
'  TVA.Security.Application.WebSecurityProvider.vb - Security provider for web applications
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
'  09-22-06 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports System.Security.Principal
Imports TVA.Assembly
Imports TVA.IO.Common
Imports TVA.IO.Compression
Imports TVA.Identity.Common
Imports TVA.Security.Cryptography.Common

Namespace Application

    <ToolboxBitmap(GetType(WebSecurityProvider)), DisplayName("WebSecurityProvider")> _
    Public Class WebSecurityProvider

#Region " Member Declaration "

        Private WithEvents m_parent As System.Web.UI.Page

        ''' <summary>
        ''' Key used for storing the username.
        ''' </summary>
        Private Const UNKey As String = "u"

        ''' <summary>
        ''' Key used for storing the password.
        ''' </summary>
        Private Const PWKey As String = "p"

        ''' <summary>
        ''' Key used for storing whether or not the supported web file have been extracted.
        ''' </summary>
        Private Const WEKey As String = "we"

        ''' <summary>
        ''' Key used for storing the user data.
        ''' </summary>
        Private Const UDKey As String = "ud"

        ''' <summary>
        ''' Name of the cookie that will contain the current user's credentials.
        ''' </summary>
        ''' <remarks>
        ''' This cookie is used for "single-signon" purposes.
        ''' </remarks>
        Private Const CCName As String = "Credentials"

#End Region

#Region " Public Code "

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
                ' Delete the session cookie used for "single-signon" purposes.
                Dim credentialCookie As New System.Web.HttpCookie(CCName)
                credentialCookie.Expires = System.DateTime.Now.AddDays(-1)
                m_parent.Response.Cookies.Add(credentialCookie)

                ' Remove the username and password from session variables.
                If m_parent.Session(UNKey) IsNot Nothing Then m_parent.Session.Remove(UNKey)
                If m_parent.Session(PWKey) IsNot Nothing Then m_parent.Session.Remove(PWKey)
                If m_parent.Session(UDKey) IsNot Nothing Then m_parent.Session.Remove(UDKey)

                m_parent.Response.Redirect(GetCleanUrl())
            End If

        End Sub

#End Region

#Region " Protected Code "

        Protected Overrides Sub CacheUserData()

            If m_parent IsNot Nothing AndAlso m_parent.Session(UDKey) Is Nothing Then
                ' Cache the current user's data.
                m_parent.Session.Add(UDKey, MyBase.User)
            End If

        End Sub

        Protected Overrides Sub RetrieveUserData()

            If m_parent IsNot Nothing AndAlso m_parent.Session(UDKey) IsNot Nothing Then
                ' Retrieve previously cached user data.
                UpdateUserData(TryCast(m_parent.Session(UDKey), User))
            End If

        End Sub

        Protected Overrides Sub ShowLoginScreen()

            If m_parent IsNot Nothing Then
                ExtractWebFiles(False)   ' Make sure that the required web file exist in the application bin directory.

                Dim webPage As String = "Login.aspx"
                Dim redirUrl As String = GetSafeUrl(webPage)
                With New StringBuilder()
                    .Append(redirUrl)
                    .Append("?r=")              ' Return Url
                    If redirUrl <> webPage Then
                        ' Redirection is absolute, so we'll provide absolute return Url.
                        .Append(m_parent.Server.UrlEncode(m_parent.Request.Url.AbsoluteUri))
                    Else
                        ' Redirection is relative, so we'll provide relative return Url.
                        .Append(m_parent.Server.UrlEncode(m_parent.Request.Url.PathAndQuery))
                    End If
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

        Protected Overrides Sub HandleAccessGranted()

            If m_parent IsNot Nothing Then
                If m_parent.Request(UNKey) IsNot Nothing OrElse m_parent.Request(PWKey) IsNot Nothing Then
                    ' Upon successful login, we'll remove the username and password from the querystring if present.
                    m_parent.Response.Redirect(GetCleanUrl())
                End If
            End If

        End Sub

        Protected Overrides Sub HandleAccessDenied()

            ' Upon unsuccessful login, we'll redirect the user to the *Access Denied* page.
            If m_parent IsNot Nothing Then
                ExtractWebFiles(False)   ' Make sure that the required web file exist in the application bin directory.

                Dim webPage As String = "ErrorPage.aspx"
                Dim redirUrl As String = GetSafeUrl(webPage)
                With New StringBuilder()
                    .Append(redirUrl)
                    .Append("?t=0")             ' Specify the type of error to be "Access Denied".
                    .Append("&r=")              ' Return Url
                    If redirUrl <> webPage Then
                        ' Redirection is absolute, so we'll provide absolute return Url.
                        .Append(m_parent.Server.UrlEncode(m_parent.Request.Url.AbsoluteUri))
                    Else
                        ' Redirection is relative, so we'll provide relative return Url.
                        .Append(m_parent.Server.UrlEncode(m_parent.Request.Url.PathAndQuery))
                    End If
                    .Append("&a=")              ' Application Name
                    .Append(m_parent.Server.UrlEncode(Encrypt(MyBase.ApplicationName, Security.Cryptography.EncryptLevel.Level4)))
                    .Append("&c=")              ' Connection String
                    .Append(m_parent.Server.UrlEncode(Encrypt(MyBase.ConnectionString, Security.Cryptography.EncryptLevel.Level4)))

                    m_parent.Response.Redirect(.ToString())
                End With
            End If

        End Sub

        Protected Overrides Function GetUsername() As String

            If m_parent IsNot Nothing Then
                Dim username As String = ""

                If m_parent.Request(UNKey) IsNot Nothing Then
                    ' We'll save the username present in the query string to session and cookie for later use.
                    m_parent.Session.Add(UNKey, m_parent.Request(UNKey).ToString())
                    m_parent.Response.Cookies(CCName)(UNKey) = m_parent.Request(UNKey).ToString()
                    username = Decrypt(m_parent.Request(UNKey).ToString(), Cryptography.EncryptLevel.Level4)
                Else
                    If m_parent.Session(UNKey) IsNot Nothing Then
                        ' Retrieve previously saved username from session.
                        username = Decrypt(m_parent.Session(UNKey).ToString(), Cryptography.EncryptLevel.Level4)
                    ElseIf m_parent.Request.Cookies(CCName) IsNot Nothing Then
                        ' Retrieve previously saved username from cookie.
                        username = Decrypt(m_parent.Request.Cookies(CCName)(UNKey).ToString(), Cryptography.EncryptLevel.Level4)
                    End If
                End If

                Return username
            Else
                Throw New InvalidOperationException("Parent must be set in order to retrieve the username.")
            End If

        End Function

        Protected Overrides Function GetPassword() As String

            If m_parent IsNot Nothing Then
                Dim password As String = ""

                If m_parent.Request(PWKey) IsNot Nothing Then
                    ' We'll save the password present in the query string to session and cookie for later use.
                    m_parent.Session.Add(PWKey, m_parent.Request(PWKey).ToString())
                    m_parent.Response.Cookies(CCName)(PWKey) = m_parent.Request(PWKey).ToString()
                    password = Decrypt(m_parent.Request(PWKey).ToString(), Cryptography.EncryptLevel.Level4)
                Else
                    If m_parent.Session(UNKey) IsNot Nothing Then
                        ' Retrieve previously saved username from session.
                        password = Decrypt(m_parent.Session(PWKey).ToString(), Cryptography.EncryptLevel.Level4)
                    ElseIf m_parent.Request.Cookies(CCName) IsNot Nothing Then
                        ' Retrieve previously saved username from cookie.
                        password = Decrypt(m_parent.Request.Cookies(CCName)(PWKey).ToString(), Cryptography.EncryptLevel.Level4)
                    End If
                End If

                Return password
            Else
                Throw New InvalidOperationException("Parent must be set in order to retrieve the password.")
            End If

        End Function

#End Region

#Region " Private Code "

        Private Sub ExtractWebFiles(ByVal impersonate As Boolean)

            If m_parent.Application(WEKey) Is Nothing Then
                ' Extract the embedded web files to the the web site's bin directory.
                Dim context As WindowsImpersonationContext = Nothing
                Try
                    Dim webFiles As ZipFile = Nothing
                    Dim zipFilePath As String = m_parent.Server.MapPath("~/")
                    Dim zipFileName As String = zipFilePath & "WebFiles.dat"

                    If impersonate Then
                        ' Impersonate privileged user before extracting files.
                        context = ImpersonateUser("esocss", "pwd4ctrl", "TVA")
                    End If

                    File.WriteAllBytes(zipFileName, ReadStream(CallingAssembly.GetEmbeddedResource("TVA.Security.Application.WebFiles.dat")))
                    webFiles = ZipFile.Open(zipFileName)
                    webFiles.Extract("*.*", zipFilePath, UpdateOption.ZipFileIsNewer, True)
                    webFiles.Close()
                    File.Delete(zipFileName)
                    m_parent.Application.Add(WEKey, True)
                Catch ex As UnauthorizedAccessException
                    ' We failed to extract the web files because the user under which the web site is running
                    ' under doesn't have write permission to web site directory. So, we'll try to extract the web
                    ' files after impersonating a privileged user who will have write access on the server, and
                    ' if not needs to be given access. However, we try impersonating privileged user before 
                    ' extracting only if we haven't tried that already in order to prevent an endless loop in
                    ' case the priviled user doesn't have write permission to the web site directory.
                    If Not impersonate Then
                        ' Try impersonation before extracting.
                        ExtractWebFiles(True)
                    Else
                        ' There isn't anything we can do now, so we propogate the exception.
                        Throw
                    End If
                Catch ex As Exception
                    ' Propogate the encountered exception.
                    Throw
                Finally
                    If context IsNot Nothing Then
                        EndImpersonation(context)
                    End If
                End Try
            End If

        End Sub

        Private Function GetCleanUrl() As String

            With New StringBuilder()
                ' Remove the username and password from querystring if present.
                .Append(m_parent.Request.Url.AbsolutePath)
                .Append("?")
                For Each parameter As String In m_parent.Request.Url.Query.TrimStart("?"c).Split("&"c)
                    Dim key As String = parameter.Split("="c)(0)
                    If Not (key = UNKey OrElse key = PWKey) Then
                        .Append(parameter)
                    End If
                Next

                Return .ToString()
            End With

        End Function

        Private Function GetSafeUrl(ByVal webPage As String) As String

            With New StringBuilder()
                Dim getRequest As WebRequest = Nothing
                Try
                    ' First, we'll try to access the specified web page locally by hitting the web site at the
                    ' default port 80.
                    getRequest = WebRequest.Create(GetLocalWebSiteUrl(False) & webPage)
                    getRequest.Credentials = CredentialCache.DefaultCredentials

                    getRequest.GetResponse()
                Catch ex1 As Exception
                    Try
                        ' Next, we'll try to access the specified web page locally by hitting the web site at
                        ' a port other than 80 if it is running on a different port.
                        getRequest = WebRequest.Create(GetLocalWebSiteUrl(True) & webPage)
                        getRequest.Credentials = CredentialCache.DefaultCredentials

                        getRequest.GetResponse()
                    Catch ex2 As Exception
                        ' If all fails, we use one of three remote web sites (development/acceptance/production).
                        .Append(GetRemoteWebSiteUrl())
                    End Try
                End Try
                .Append(webPage)

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

        Private Function GetLocalWebSiteUrl(ByVal addPort As Boolean) As String

            With New StringBuilder()
                .Append(m_parent.Request.Url.Scheme)
                .Append(System.Uri.SchemeDelimiter)
                .Append(m_parent.Request.Url.Host)
                If Not m_parent.Request.Url.IsDefaultPort AndAlso addPort Then
                    ' Port other than the default port 80 is used to access the web site.
                    .Append(":")
                    .Append(m_parent.Request.Url.Port)
                End If
                .Append(m_parent.Request.ApplicationPath)
                .Append("/")

                Return .ToString()
            End With

        End Function

        Private Sub WebSecurityProvider_DbConnectionException(ByVal sender As Object, ByVal e As GenericEventArgs(Of System.Exception)) Handles Me.DbConnectionException

            If m_parent IsNot Nothing Then
                With New StringBuilder()
                    .Append("<html>")
                    .AppendLine()
                    .Append("<head>")
                    .AppendLine()
                    .Append("<Title>Login Aborted</Title>")
                    .AppendLine()
                    .Append("</head>")
                    .AppendLine()
                    .Append("<body>")
                    .AppendLine()
                    .Append("<div style=""font-family: Tahoma; font-size: 8pt; font-weight: bold; text-align: center;"">")
                    .AppendLine()
                    .Append("<span style=""font-size: 22pt; color: red;"">")
                    .Append("Login Process Aborted")
                    .Append("</span><br /><br />")
                    .AppendLine()
                    .Append("[Connection could not be established with the security database]")
                    .AppendLine()
                    .Append("</div>")
                    .AppendLine()
                    .Append("</body>")
                    .AppendLine()
                    .Append("</html>")

                    m_parent.Response.Clear()
                    m_parent.Response.Write(.ToString())
                    m_parent.Response.End()
                End With
            End If

        End Sub

        Private Sub m_parent_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_parent.PreInit

            If MyBase.User Is Nothing Then
                ' EndInit() method of the ISupportInitialize interface was not called which in-turn calls the
                ' LoginUser() method, so we'll call LoginUser() over here implicitly before the web page initializes.
                ' Engaging the security before the page, or anything inside the page for that matter initializes,
                ' will prove extremely useful when security is to be implemented at a control level.
                LoginUser()
            End If

        End Sub

        Private Sub m_parent_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_parent.Unload

            Dispose()

        End Sub

#End Region

    End Class

End Namespace