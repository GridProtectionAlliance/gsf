' 09-12-06

Imports TVA.Assembly

<Serializable()> _
Public Class ClientInfo

    Public Sub New(ByVal clientID As Guid)

        MyBase.New()
        Me.ClientID = clientID
        ClientType = TVA.Common.GetApplicationType()
        UserName = System.Threading.Thread.CurrentPrincipal.Identity.Name
        If String.IsNullOrEmpty(UserName) Then
            UserName = Environment.UserDomainName & "\" & Environment.UserName
        End If
        MachineName = Environment.MachineName
        Select Case ClientType
            Case ApplicationType.WindowsCui, ApplicationType.WindowsGui
                ClientName = AppDomain.CurrentDomain.FriendlyName
            Case ApplicationType.Web
                ClientName = System.Web.HttpContext.Current.Request.ApplicationPath
        End Select

    End Sub

    Public ClientID As Guid
    Public ClientType As ApplicationType
    Public ClientName As String
    Public UserName As String
    Public MachineName As String
    Public ConnectedAt As Date

End Class
