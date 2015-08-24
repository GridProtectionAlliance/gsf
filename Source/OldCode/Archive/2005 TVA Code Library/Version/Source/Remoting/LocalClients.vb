' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Text

Namespace Remoting

    ' This class represents a locally cached collection of LocalClient instances used by an IServer
    ' Threading is a real concern for this class - make sure to synclock base collection as necessary...
    <Serializable()> _
    Public Class LocalClients

        Inherits MarshalByRefObject

        Private Clients As ArrayList                ' List of locally cached clients
        Private WithEvents Sweeper As Timers.Timer  ' Periodic non-responsive local client remover

        Friend Sub New()

            Clients = New ArrayList
            Sweeper = New Timers.Timer

            With Sweeper
                .AutoReset = False
                .Interval = 5000
                .Enabled = False
            End With

        End Sub

        Friend Sub Add(ByVal Value As LocalClient)

            Dim intIndex As Integer

            SyncLock Clients.SyncRoot
                intIndex = Clients.BinarySearch(Value)
                If intIndex >= 0 Then Clients.RemoveAt(intIndex)
                Clients.Add(Value)
                Clients.Sort()
                Sweeper.Enabled = True
            End SyncLock

        End Sub

        Friend Sub Remove(ByVal Value As LocalClient)

            SyncLock Clients.SyncRoot
                Dim intIndex As Integer = Clients.BinarySearch(Value)
                If intIndex >= 0 Then Clients.RemoveAt(intIndex)
            End SyncLock

        End Sub

        Friend ReadOnly Property SyncRoot() As Object
            Get
                Return Clients.SyncRoot
            End Get
        End Property

        Friend Function Clone() As ArrayList

            Return Clients.Clone()

        End Function

        Friend Function GetEnumerator() As Collections.IEnumerator

            Return Clients.GetEnumerator()

        End Function

        Default Public ReadOnly Property Item(ByVal Index As Integer) As LocalClient
            Get
                Dim lccClient As LocalClient

                SyncLock Clients.SyncRoot
                    If Index > -1 And Index < Clients.Count Then
                        lccClient = DirectCast(Clients(Index), LocalClient)
                    End If
                End SyncLock

                Return lccClient
            End Get
        End Property

        Public Function Find(ByVal ID As Guid) As LocalClient

            Dim intIndex As Integer

            SyncLock Clients.SyncRoot
                intIndex = Clients.BinarySearch(New LocalClient(ID, Nothing))
            End SyncLock

            If intIndex < 0 Then
                Return Nothing
            Else
                Return Me(intIndex)
            End If

        End Function

        Public ReadOnly Property Count() As Integer
            Get
                Dim intCount As Integer

                SyncLock Clients.SyncRoot
                    intCount = Clients.Count
                End SyncLock

                Return intCount
            End Get
        End Property

        Friend ReadOnly Property InternalCount() As Integer
            Get
                Dim intCount As Integer
                Dim currClients As ArrayList
                Dim lccClient As LocalClient

                ' Because we may have to wait for non-responsive clients to timeout, we create a
                ' copy of the client list so that we can keep synclock time down to a minimum...
                SyncLock Clients.SyncRoot
                    currClients = Clients.Clone()
                End SyncLock

                For Each lccClient In currClients
                    If lccClient.IsInternalClient Then intCount += 1
                Next

                Return intCount
            End Get
        End Property

        ' Send a "broadcast message" to all clients
        Friend Sub SendNotification(ByVal sender As Object, ByVal e As EventArgs)

            Dim lccClient As LocalClient

            SyncLock Clients.SyncRoot
                For Each lccClient In Clients
                    ' Add notification to recipient's personal synchronized notification queue
                    lccClient.SendNotification(sender, e)
                Next
            End SyncLock

        End Sub

        ' Send a "private message" to a specific client
        Friend Sub SendPrivateNotification(ByVal ID As Guid, ByVal sender As Object, ByVal e As EventArgs)

            Dim lccClient As LocalClient = Find(ID)

            If Not lccClient Is Nothing Then lccClient.SendNotification(sender, e)

        End Sub

        Friend Sub FlushNotifications()

            Dim currClients As ArrayList
            Dim lccClient As LocalClient

            ' Because we may have to wait for non-responsive clients to timeout when
            ' flushing notifications, we create a copy of the client list so that
            ' we can keep synclock time down to a minimum...
            SyncLock Clients.SyncRoot
                currClients = Clients.Clone()
            End SyncLock

            For Each lccClient In currClients
                ' Flush recipient's personal synchronized notification queue
                lccClient.FlushNotifications()
            Next

        End Sub

        ' Return a string that lists all of the connected clients
        Public Function GetClientList() As String

            Dim currClients As ArrayList
            Dim lccClient As LocalClient
            Dim strClient As String
            Dim strClients As New StringBuilder
            Dim intClients As Integer
            Dim x As Integer

            ' Because we may have to wait for non-responsive clients to timeout when
            ' requesting a description, we create a copy of the client list so that
            ' we can keep synclock time down to a minimum...
            SyncLock Clients.SyncRoot
                currClients = Clients.Clone()
            End SyncLock

            For Each lccClient In currClients
                strClient = lccClient.Description
                If Len(strClient) > 0 Then
                    intClients += 1
                    strClients.Append(vbCrLf & "Client Connection " & intClients & ":" & vbCrLf & vbCrLf & strClient & vbCrLf)
                End If
            Next

            ' We manually call sweeper to get an accurate connected client count
            SweepNonResponsiveClients(Nothing, Nothing)

            intClients = Count()

            If intClients > 0 Then
                If intClients = 1 Then
                    strClients.Insert(0, vbCrLf & "1 Active Client:" & vbCrLf)
                Else
                    strClients.Insert(0, vbCrLf & intClients & " Active Clients:" & vbCrLf)
                End If
            Else
                strClients.Insert(0, vbCrLf & "There are no active clients..." & vbCrLf)
            End If

            Return strClients.ToString()

        End Function

        Private Sub SweepNonResponsiveClients(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles Sweeper.Elapsed

            SyncLock Clients.SyncRoot
                Dim colNonResponsiveClients As New ArrayList
                Dim x As Integer

                For x = 0 To Clients.Count - 1
                    If DirectCast(Clients(x), LocalClient).NonResponsive Then colNonResponsiveClients.Add(x)
                Next

                If colNonResponsiveClients.Count > 0 Then
                    colNonResponsiveClients.Sort()

                    ' Remove non responsive clients in reverse order to maintain index integrity
                    For x = colNonResponsiveClients.Count - 1 To 0 Step -1
                        Clients.RemoveAt(colNonResponsiveClients(x))
                    Next
                End If

                ' Keep sweeper alive so long as there are active clients
                Sweeper.Enabled = (Clients.Count > 0)
            End SyncLock

        End Sub

    End Class

End Namespace