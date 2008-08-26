Imports System.Data
Imports System.Data.SqlClient
Imports TVA.Data.Common
Imports System.Net
Imports System.Net.Sockets

Namespace Speech

    Public NotInheritable Class Common

        Private Sub New()

            ' This class is not to be instantiated.

        End Sub

        ''' <summary>
        ''' Marks an event as complete so that looping service will not make any more calls for the event.
        ''' </summary>
        ''' <param name="EventId">Id of an event to be marked as complete.</param>
        ''' <param name="EnvironmentType">Environment: Development/Acceptance/Production.</param>
        ''' <remarks></remarks>
        Public Shared Sub StopCalling(ByVal EventId As Integer, ByVal EnvironmentType As Environment)
            Dim connection As New SqlConnection(GetConnectionString(EnvironmentType))
            Try
                If connection.State <> ConnectionState.Open Then
                    connection.Open()
                End If
                ExecuteScalar("Update Events Set Active = '0' Where ID = " & EventId, connection)
                ExecuteScalar("Delete From LoopingTable Where EventId = " & EventId, connection)
            Catch
                Throw
            Finally
                If connection.State <> ConnectionState.Closed Then
                    connection.Close()
                End If
                If connection IsNot Nothing Then connection.Dispose()
            End Try
        End Sub

        ''' <summary>
        ''' Return call log information from the database for an event.
        ''' </summary>
        ''' <param name="EventId">Id of an event for which call log needs to be retrieved.</param>
        ''' <param name="EnvironmentType">Environment: Development/Acceptance/Production.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetCallLog(ByVal EventId As Integer, ByVal EnvironmentType As Environment) As DataTable
            Dim callLogData As New DataTable
            Dim connection As New SqlConnection(GetConnectionString(EnvironmentType))
            Try
                If connection.State <> ConnectionState.Open Then
                    connection.Open()
                End If
                callLogData = RetrieveData("Select * From CallLogDetail Where EventId = " & EventId, connection)
            Catch
                Throw
            Finally
                If connection.State <> ConnectionState.Closed Then
                    connection.Close()
                End If
                If connection IsNot Nothing Then connection.Dispose()
            End Try

            Return callLogData
        End Function

        ''' <summary>
        ''' Return database connection string based on the environment type.
        ''' </summary>
        ''' <param name="EnvironmentType">Environment type: Development, acceptance, production</param>
        ''' <returns>Database connection string</returns>
        ''' <remarks></remarks>
        Public Shared Function GetConnectionString(ByVal EnvironmentType As Environment) As String
            Select Case EnvironmentType
                Case Environment.Development
                    Return "Server=RGOCDSQL; Database=Speech; UID=trospeech; PWD=tr0speech"
                Case Environment.Acceptance
                    Return "Server=ESOASQLGENDAT\GENDAT; Database=Speech; UID=trospeech; PWD=tr0speech"
                Case Environment.Production
                    Return "Server=ESOOPSQL1; Database=Speech; UID=trospeech; PWD=tr0speech"
                Case Else
                    Return "Server=RGOCDSQL; Database=Speech; UID=trospeech; PWD=tr0speech"
            End Select
        End Function

        Public Shared Function IsLoopingServiceRunning(ByVal EnvironmentType As Environment) As Boolean
            Select Case EnvironmentType
                Case Environment.Development
                    Return PingServer("RGOCMSSPEECH3", 6999)
                Case Environment.Acceptance
                    Return PingServer("RGOCMSSPEECH3", 6999)
                Case Environment.Production
                    Return PingServer("RGOCMSSPEECH3", 6999)
                Case Else
                    Return PingServer("RGOCMSSPEECH3", 6999)
            End Select
        End Function

        ''' <summary>
        ''' This function pings the server based on supplied name on port and returns boolean result.
        ''' </summary>
        ''' <param name="ServerName"></param>
        ''' <param name="port"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function PingServer(ByVal ServerName As String, ByVal port As Integer) As Boolean
            Dim bUP As Boolean = False
            Dim hostEntry As IPHostEntry = Nothing
            ' Get host related information.
            Try
                hostEntry = Dns.GetHostEntry(ServerName.Trim())
                Dim address As IPAddress
                For Each address In hostEntry.AddressList
                    Dim endPoint As New IPEndPoint(address, port)
                    Dim tempSocket As New Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                    tempSocket.Connect(endPoint)
                    If tempSocket.Connected Then
                        bUP = True
                        tempSocket.Close()
                        tempSocket = Nothing
                        Exit For
                    End If
                Next address

            Catch ex As Exception
                bUP = False

            End Try

            Return bUP
        End Function
    End Class

End Namespace