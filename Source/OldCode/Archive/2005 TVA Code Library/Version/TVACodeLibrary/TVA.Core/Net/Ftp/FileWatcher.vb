' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Net.Ftp

    <ToolboxBitmap(GetType(FileWatcher)), DefaultProperty("Server"), DefaultEvent("FileAdded"), Description("Monitors for file changes over an FTP session")> _
    Public Class FileWatcher

        Inherits Component

        Protected WithEvents FtpSession As Session
        Protected FtpUserName As String
        Protected FtpPassword As String
        Protected WatchDirectory As String
        Protected WithEvents WatchTimer As New Timers.Timer
        Protected WithEvents RestartTimer As New Timers.Timer
        Protected DirFiles As New ArrayList
        Protected NewFiles As New ArrayList
        Private flgEnabled As Boolean           ' Determines if file watching is enabled
        Private flgNotifyOnComplete As Boolean  ' Sets flag for notification time: set to True to only notify when a file is finished uploading, set to False to get an immediate notification when a new file is detected

        Public Event FileAdded(ByVal FileReference As File)
        Public Event FileDeleted(ByVal FileReference As File)
        Public Event Status(ByVal StatusText As String)

        Public Event InternalSessionCommand(ByVal Command As String)
        Public Event InternalSessionResponse(ByVal Response As String)

        Public Sub New()

            flgEnabled = True
            flgNotifyOnComplete = True
            FtpSession = New Session(False)

            ' Define a timer to watch for new files
            WatchTimer.AutoReset = False
            WatchTimer.Interval = 5000
            WatchTimer.Enabled = False

            ' Define a timer for FTP connection in case of availability failures
            RestartTimer.AutoReset = False
            RestartTimer.Interval = 10000
            RestartTimer.Enabled = False

        End Sub

        Public Sub New(ByVal CaseInsensitive As Boolean, Optional ByVal NotifyOnComplete As Boolean = True)

            MyClass.New()
            FtpSession.CaseInsensitive = CaseInsensitive
            flgNotifyOnComplete = NotifyOnComplete

        End Sub

        Protected Overrides Sub Finalize()

            Dispose(True)

        End Sub

        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)

            Close()

        End Sub

        Public Overridable Sub Close()

            DirFiles.Clear()
            NewFiles.Clear()
            WatchTimer.Enabled = False
            RestartTimer.Enabled = False
            CloseSession()
            GC.SuppressFinalize(Me)

        End Sub

        <Browsable(True), Category("Configuration"), Description("Specify FTP server name (do not prefix with ftp://).")> _
        Public Overridable Property Server() As String
            Get
                Return FtpSession.Server
            End Get
            Set(ByVal Value As String)
                FtpSession.Server = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set to True to not be case sensitive with FTP file names."), DefaultValue(False)> _
        Public Property CaseInsensitive() As Boolean
            Get
                Return FtpSession.CaseInsensitive
            End Get
            Set(ByVal Value As Boolean)
                FtpSession.CaseInsensitive = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Specify interval in seconds to poll FTP directory for file changes."), DefaultValue(5)> _
        Public Overridable Property WatchInterval() As Integer
            Get
                Return WatchTimer.Interval \ 1000
            End Get
            Set(ByVal Value As Integer)
                WatchTimer.Enabled = False
                WatchTimer.Interval = Value * 1000
                WatchTimer.Enabled = flgEnabled
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Specify FTP directory to monitor.  Leave blank to monitor initial FTP session directory."), DefaultValue("")> _
        Public Overridable Property Directory() As String
            Get
                Return WatchDirectory
            End Get
            Set(ByVal Value As String)
                WatchDirectory = Value
                ConnectToWatchDirectory()
                Reset()
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set to True to only be notified of new FTP files when upload is complete.  This monitors file size changes at each WatchInterval."), DefaultValue(True)> _
        Public Overridable Property NotifyOnComplete() As Boolean
            Get
                Return flgNotifyOnComplete
            End Get
            Set(ByVal Value As Boolean)
                flgNotifyOnComplete = Value
                Reset()
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Determines if FTP file watcher is enabled."), DefaultValue(True)> _
        Public Overridable Property Enabled() As Boolean
            Get
                Return flgEnabled
            End Get
            Set(ByVal Value As Boolean)
                flgEnabled = Value
                Reset()
            End Set
        End Property

        <Browsable(False)> _
        Public Overridable ReadOnly Property IsConnected() As Boolean
            Get
                Return FtpSession.IsConnected
            End Get
        End Property

        Public Overridable Sub Connect(ByVal UserName As String, ByVal Password As String)

            If Len(UserName) > 0 Then FtpUserName = UserName
            If Len(Password) > 0 Then FtpPassword = Password

            Try
                ' Attempt to connect to FTP server
                FtpSession.Connect(FtpUserName, FtpPassword)
                RaiseEvent Status("[" & Now() & "] FTP file watcher connected to ""ftp://" & FtpUserName & "@" & FtpSession.Server & """")
                ConnectToWatchDirectory()
                WatchTimer.Enabled = flgEnabled

                ' FTP servers can be fickle creatues, so after a successful connection we setup the
                ' restart timer to reconnect every thirty minutes whether we need to or not :)
                RestartTimer.Interval = 1800000
                RestartTimer.Enabled = True
            Catch ex As Exception
                ' If this fails, we'll try again in a moment.  The FTP server may be down...
                RaiseEvent Status("[" & Now() & "] FTP file watcher failed to connect to ""ftp://" & FtpUserName & "@" & FtpSession.Server & """ - trying again in 10 seconds..." & vbCrLf & vbTab & "Exception: " & ex.Message)
                RestartConnectCycle()
            End Try

        End Sub

        Public Overridable Function NewDirectorySession() As Session

            ' This method is just for convenience.  We can't allow the end user to use the
            ' actual internal directory for sending files or other work because it is
            ' constantly being refreshed/used etc., so we instead create a new FTP Session
            ' based on the current internal session and watch directory information
            Dim DirectorySession As New Session(FtpSession.CaseInsensitive)

            With DirectorySession
                .Server = FtpSession.Server
                .Connect(FtpUserName, FtpPassword)
                .SetCurrentDirectory(WatchDirectory)
            End With

            Return DirectorySession

        End Function

        Public Overridable Sub Reset()

            RestartTimer.Enabled = False
            WatchTimer.Enabled = False
            DirFiles.Clear()
            NewFiles.Clear()
            WatchTimer.Enabled = flgEnabled
            If Not FtpSession.IsConnected Then RestartConnectCycle()

        End Sub

        Private Sub ConnectToWatchDirectory()

            If FtpSession.IsConnected Then
                With FtpSession
                    .SetCurrentDirectory(WatchDirectory)

                    If Len(WatchDirectory) > 0 Then
                        RaiseEvent Status("[" & Now() & "] FTP file watcher monitoring directory """ & WatchDirectory & """")
                    Else
                        RaiseEvent Status("[" & Now() & "] No FTP file watcher directory specified - monitoring initial folder")
                    End If
                End With
            End If

        End Sub

        ' This method is synchronized in case user sets watch interval too tight...
        <MethodImpl(MethodImplOptions.Synchronized)> _
        Private Sub WatchTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles WatchTimer.Elapsed

            ' We attempt to access the FTP Session and refresh the current directory, if this fails
            ' we are going to restart the connect cycle
            Try
                ' Refresh the file listing for the current directory
                FtpSession.CurrentDirectory.Refresh()
            Catch ex As Exception
                RestartConnectCycle()
                RaiseEvent Status("[" & Now() & "] FTP file watcher is no longer connected to server """ & FtpSession.Server & """ - restarting connect cycle." & vbCrLf & vbTab & "Exception: " & ex.Message)
            End Try

            If Not FtpSession Is Nothing Then
                If FtpSession.IsConnected Then
                    Dim File, NewFile As File
                    Dim Files As Dictionary(Of String, File).ValueCollection.Enumerator = FtpSession.CurrentDirectory.Files.GetEnumerator()
                    Dim RemovedFiles As New ArrayList
                    Dim intIndex As Integer
                    Dim x As Integer

                    ' Check for new files
                    While Files.MoveNext()
                        File = Files.Current

                        If flgNotifyOnComplete Then
                            ' See if any new files are finished downloading
                            intIndex = NewFiles.BinarySearch(File)

                            If intIndex >= 0 Then
                                NewFile = DirectCast(NewFiles(intIndex), File)
                                If NewFile.Size = File.Size Then
                                    ' File size has not changed since last directory refresh, so we will
                                    ' notify user of new file...
                                    DirFiles.Add(File)
                                    DirFiles.Sort()
                                    NewFiles.RemoveAt(intIndex)
                                    RaiseEvent FileAdded(File)
                                Else
                                    NewFile.Size = File.Size
                                End If
                            ElseIf DirFiles.BinarySearch(File) < 0 Then
                                NewFiles.Add(File)
                                NewFiles.Sort()
                            End If
                        ElseIf DirFiles.BinarySearch(File) < 0 Then
                            ' If user wants an immediate notification of new files, we'll give it to them...
                            DirFiles.Add(File)
                            DirFiles.Sort()
                            RaiseEvent FileAdded(File)
                        End If
                    End While

                    ' Check for removed files
                    For x = 0 To DirFiles.Count - 1
                        File = DirectCast(DirFiles(x), File)
                        If FtpSession.CurrentDirectory.FindFile(File.Name) Is Nothing Then
                            RemovedFiles.Add(x)
                            RaiseEvent FileDeleted(File)
                        End If
                    Next

                    ' Remove files that have been deleted
                    If RemovedFiles.Count > 0 Then
                        RemovedFiles.Sort()

                        ' We remove items in desc order to maintain index integrity
                        For x = RemovedFiles.Count - 1 To 0 Step -1
                            DirFiles.RemoveAt(RemovedFiles(x))
                        Next

                        RemovedFiles.Clear()
                    End If

                    WatchTimer.Enabled = flgEnabled
                Else
                    RestartConnectCycle()
                    RaiseEvent Status("[" & Now() & "] FTP file watcher is no longer connected to server """ & FtpSession.Server & """ - restarting connect cycle.")
                End If
            End If

        End Sub

        Private Sub CloseSession()

            Try
                FtpSession.Close()
            Catch
            End Try

        End Sub

        Private Sub RestartConnectCycle()

            RestartTimer.Enabled = False
            RestartTimer.Interval = 10000
            RestartTimer.Enabled = True

        End Sub

        Private Sub RestartTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles RestartTimer.Elapsed

            ' Attempt to close the FTP Session if it is open...
            CloseSession()

            ' Try to reestablish connection
            WatchTimer.Enabled = False
            Connect("", "")

        End Sub

        Private Sub Session_CommandSent(ByVal Command As String) Handles FtpSession.CommandSent

            RaiseEvent InternalSessionCommand(Command)

        End Sub

        Private Sub Session_ResponseReceived(ByVal Response As String) Handles FtpSession.ResponseReceived

            RaiseEvent InternalSessionResponse(Response)

        End Sub

    End Class

End Namespace