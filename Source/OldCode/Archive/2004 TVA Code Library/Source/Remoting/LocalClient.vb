' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Threading
Imports System.Reflection
Imports System.Reflection.BindingFlags
Imports TVA.Shared.DateTime
Imports TVA.Threading

Namespace Remoting

    ' This class represents the cached reference to a remote IClient used by IServer
    <Serializable()> _
    Public Class LocalClient

        Inherits MarshalByRefObject
        Implements IComparable

        Public ID As Guid
        Public ConnectTime As DateTime = Now()
        Public UserData As Object
        Public NonResponsive As Boolean
        Private RemoteClient As IClient
        Private Notifications As NotificationQueue
        Private ClientResponseTimeout As Integer
        Private IsInternal As Integer = -1

        Private Class NotificationQueue

            Inherits CollectionBase

            Private Parent As LocalClient
            Private WithEvents QueueTimer As Timers.Timer

            Private Class Notification

                Public Sender As Object
                Public EventArgs As EventArgs

                ' Create new notification
                Public Sub New(ByVal Sender As Object, ByVal EventArgs As EventArgs)

                    Me.Sender = Sender
                    Me.EventArgs = EventArgs

                End Sub

            End Class

            ' We process queued client notifications on their own thread so we can timeout if processing is
            ' taking longer than user is willing to wait...
            Private Class NotificationThread

                Inherits ThreadBase

                Private Parent As LocalClient
                Private ProcessQueue As ArrayList

                Public Overloads Sub Start(ByVal Parent As LocalClient, ByVal ProcessQueue As ArrayList)

                    Me.Parent = Parent
                    Me.ProcessQueue = ProcessQueue
                    MyBase.Start()

                End Sub

                Protected Overrides Sub ThreadProc()

                    Dim intAttempts As Integer

                    ' If any items are in the notification process queue, we handle them
                    While ProcessQueue.Count > 0 And Not Parent.NonResponsive
                        With DirectCast(ProcessQueue(0), Notification)
                            Try
                                ' Send notification to remote client
                                Parent.RemoteReference.SendClientNotification(.Sender, .EventArgs)

                                ' Remove handled item from queue
                                ProcessQueue.RemoveAt(0)
                                intAttempts = 0
                            Catch ex As ThreadAbortException
                                Throw ex
                            Catch
                                ' Our connection may be slow, we'll try three times before assuming connection is bad
                                intAttempts += 1

                                ' Badly behaving remote clients will be dealt with accordingly...
                                If intAttempts >= 3 Then Parent.NonResponsive = True
                            End Try
                        End With
                    End While

                End Sub

            End Class

            Public Sub New(ByVal Parent As LocalClient)

                Me.Parent = Parent

                ' Define a queue timer for processing notifications
                QueueTimer = New Timers.Timer
                With QueueTimer
                    .AutoReset = False
                    .Interval = 100
                    .Enabled = False
                End With

            End Sub

            Protected Overrides Sub Finalize()

                Flush()

            End Sub

            Public Sub Flush()

                ' We disable the queue timer and process all items immediately when
                ' flush queue is requested - this doesn't prevent new items from being
                ' added to queue while processing, however this function doesn't return
                ' until all known items are processed...
                QueueTimer.Enabled = False
                ProcessNotifications()

            End Sub

            Public Property QueueInterval() As Integer
                Get
                    Return QueueTimer.Interval
                End Get
                Set(ByVal Value As Integer)
                    QueueTimer.Interval = Value
                End Set
            End Property

            ' This function queues a notification to be sent to the remote client
            Public Sub AddNotification(ByVal sender As Object, ByVal e As EventArgs)

                If Not Parent.NonResponsive Then
                    SyncLock List.SyncRoot
                        List.Add(New Notification(sender, e))
                    End SyncLock
                    QueueTimer.Enabled = True
                End If

            End Sub

            Public ReadOnly Property SyncCount() As Integer
                Get
                    Dim intCount As Integer

                    SyncLock List.SyncRoot
                        intCount = List.Count
                    End SyncLock

                    Return intCount
                End Get
            End Property

            Private Sub QueueTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles QueueTimer.Elapsed

                ProcessNotifications()
                QueueTimer.Enabled = (SyncCount > 0)

            End Sub

            Private Sub ProcessNotifications()

                Dim ProcessQueue As New ArrayList
                Dim Item As Notification

                ' We create a synchronized copy of all the items to be processed so new notifications 
                ' can be queued even while notification processing is currently occurring
                SyncLock List.SyncRoot
                    For Each Item In List
                        ProcessQueue.Add(Item)
                    Next
                    List.Clear()
                End SyncLock

                If ProcessQueue.Count > 0 Then
                    ' We process all synchronized notifications on their own thread
                    With New NotificationThread
                        Dim QueueCount As Integer = ProcessQueue.Count
                        .Start(Parent, ProcessQueue)

                        ' We block calling thread allowing each notification a user specified amount of response time
                        ' before aborting thread.  When the calling thread is the timer event, we are only blocking
                        ' another subordinate thread - so the block has no negative impact on overall performance.
                        ' When this function is executed as a result of a FlushNotifications request, then blocking
                        ' of the calling thread until complete is the desired behaviour.  Because the thread will
                        ' retry sending a notification up to three times, it is possible that the thread will
                        ' timeout before all of the notifications actually get sent, as a result any remaining
                        ' unprocessed notifications will get requeued.
                        With .Thread
                            If Not .Join(Parent.ResponseTimeout * QueueCount) Then .Abort()
                        End With
                    End With

                    ' We requeue any unprocessed notifications
                    If ProcessQueue.Count > 0 And Not Parent.NonResponsive Then
                        SyncLock List.SyncRoot
                            Dim x As Integer

                            ' Since the order in which these items came in may be relevant, we at least push them before
                            ' any currently queued items - however, this cannot guarantee proper processing order because
                            ' another queue timer event may already be processing another notification batch
                            For x = ProcessQueue.Count - 1 To 0 Step -1
                                List.Insert(0, ProcessQueue(x))
                            Next
                        End SyncLock
                    End If
                End If

            End Sub

        End Class

        ' We process getting client properties on their own thread so we can timeout if processing is
        ' taking longer than user is willing to wait...
        Private Class GetDescriptionThread

            Inherits ThreadBase

            Private Parent As LocalClient
            Private ReturnValue As String

            Public Function GetValue(ByVal Parent As LocalClient) As String

                Me.Parent = Parent
                Start()

                ' We block calling thread allowing property call a user specified amount of response time
                ' before aborting thread.
                With WorkerThread
                    If Not .Join(Parent.ResponseTimeout) Then .Abort()
                End With

                Return ReturnValue

            End Function

            Protected Overrides Sub ThreadProc()

                Dim flgCallComplete As Boolean
                Dim intAttempts As Integer

                While Not flgCallComplete And Not Parent.NonResponsive
                    Try
                        ReturnValue = Parent.RemoteClient.Description

                        ' Signal call complete
                        flgCallComplete = True
                    Catch ex As ThreadAbortException
                        Throw ex
                    Catch
                        ' Our connection may be slow, we'll try call three times before assuming connection is bad
                        intAttempts += 1

                        ' Badly behaving remote clients will be dealt with accordingly...
                        If intAttempts >= 3 Then Parent.NonResponsive = True
                    End Try
                End While

            End Sub

        End Class

        Private Class GetIsInternalClientThread

            Inherits ThreadBase

            Private Parent As LocalClient
            Private ReturnValue As Boolean

            Public Function GetValue(ByVal Parent As LocalClient) As Boolean

                Me.Parent = Parent
                Start()

                ' We block calling thread allowing property call a user specified amount of response time
                ' before aborting thread.
                With WorkerThread
                    If Not .Join(Parent.ResponseTimeout) Then .Abort()
                End With

                Return ReturnValue

            End Function

            Protected Overrides Sub ThreadProc()

                Dim flgCallComplete As Boolean
                Dim intAttempts As Integer

                While Not flgCallComplete And Not Parent.NonResponsive
                    Try
                        ReturnValue = Parent.RemoteClient.IsInternalClient

                        ' Signal call complete
                        flgCallComplete = True
                    Catch ex As ThreadAbortException
                        Throw ex
                    Catch
                        ' Our connection may be slow, we'll try call three times before assuming connection is bad
                        intAttempts += 1

                        ' Badly behaving remote clients will be dealt with accordingly...
                        If intAttempts >= 3 Then Parent.NonResponsive = True
                    End Try
                End While

            End Sub

        End Class

        Friend Sub New(ByVal ID As Guid, ByVal RemotingClient As IClient, Optional ByVal ResponseTimeout As Integer = 10000, Optional ByVal QueueInterval As Integer = 250, Optional ByVal UserData As Object = Nothing)

            Me.ID = ID
            Me.RemoteClient = RemotingClient
            Me.UserData = UserData
            Me.Notifications = New NotificationQueue(Me)
            Me.ResponseTimeout = ResponseTimeout
            Me.QueueInterval = QueueInterval

        End Sub

        ' We give the end user the option of changing the response timeout for specific clients
        Public Property ResponseTimeout() As Integer
            Get
                Return ClientResponseTimeout
            End Get
            Set(ByVal Value As Integer)
                If Value < 1000 Then
                    ClientResponseTimeout = 1000
                Else
                    ClientResponseTimeout = Value
                End If
            End Set
        End Property

        ' We give the end user the option of changing the queue processing interval for specific clients
        Public Property QueueInterval() As Integer
            Get
                Return Notifications.QueueInterval
            End Get
            Set(ByVal Value As Integer)
                If Value < 1 Then
                    Notifications.QueueInterval = 1
                Else
                    Notifications.QueueInterval = Value
                End If
            End Set
        End Property

        Public ReadOnly Property Description() As String
            Get
                Dim strDescription As String

                If Not NonResponsive Then
                    With New GetDescriptionThread
                        Try
                            strDescription = .GetValue(Me) & "  Connected: " & SecondsToText(DateDiff(DateInterval.Second, ConnectTime, Now())) & vbCrLf
                        Catch
                            strDescription = ""
                            NonResponsive = True
                        End Try
                    End With
                End If

                Return strDescription
            End Get
        End Property

        Friend ReadOnly Property IsInternalClient() As Boolean
            Get
                If IsInternal = -1 Then
                    If Not NonResponsive Then
                        With New GetIsInternalClientThread
                            Try
                                If .GetValue(Me) Then
                                    IsInternal = 1
                                Else
                                    IsInternal = 0
                                End If
                            Catch
                                NonResponsive = True
                            End Try
                        End With
                    End If
                End If

                Return IIf(IsInternal = 1, True, False)
            End Get
        End Property

        Public ReadOnly Property RemoteReference() As IClient
            Get
                Return RemoteClient
            End Get
        End Property

        Friend Sub FlushNotifications()

            ' Process any remaining notifications in queue...
            Notifications.Flush()

        End Sub

        Friend Sub SendNotification(ByVal sender As Object, ByVal e As EventArgs)

            ' Add notification to queue for processing...
            Notifications.AddNotification(sender, e)

        End Sub

        Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

            ' LocalClient's are sorted by their ID's
            If TypeOf obj Is LocalClient Then
                Return ID.CompareTo(DirectCast(obj, LocalClient).ID)
            Else
                Throw New ArgumentException("LocalClient can only be compared to other LocalClients")
            End If

        End Function

    End Class

End Namespace