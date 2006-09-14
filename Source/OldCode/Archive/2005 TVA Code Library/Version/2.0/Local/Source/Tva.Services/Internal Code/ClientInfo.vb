' 09-12-06

Imports Tva.Assembly

<Serializable()> _
Friend Class ClientInfo

    Public Sub New()
        Assembly = EntryAssembly.FullName
        Location = EntryAssembly.Location
        Created = EntryAssembly.BuildDate
        NTUser = My.User.Name
    End Sub

    Public Assembly As String
    Public Location As String
    Public Created As System.DateTime
    Public NTUser As String

End Class
