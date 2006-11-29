' 09-12-06

Imports Tva.Assembly

<Serializable()> _
Friend Class ClientInfo

    Public Sub New()

        MyBase.New()
        NTUser = System.Threading.Thread.CurrentPrincipal.Identity.Name
        Try
            Assembly = EntryAssembly.FullName
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
    Public NTUser As String

End Class
