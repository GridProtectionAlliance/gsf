' 09-12-06

Imports TVA.Assembly

<Serializable()> _
Friend Class ClientInfo

    Public Sub New()

        MyBase.New()
        UserName = String.Format("{0}\{1}", Environment.UserDomainName, Environment.UserName)
        MachineName = Environment.MachineName
        Try
            Assembly = EntryAssembly.Name & ", Version=" & EntryAssembly.Version.ToString()
            Location = EntryAssembly.Location
            Created = EntryAssembly.BuildDate
        Catch ex As Exception
            ' We will encounter System.NullReferenceException when ClientHelper is being used from ASP.Net.
            ' The exception will be caused because in ASP.Net we don't have an EntryAssembly.
        End Try

    End Sub

    Public Assembly As String
    Public Location As String
    Public Created As System.DateTime
    Public UserName As String
    Public MachineName As String

End Class
