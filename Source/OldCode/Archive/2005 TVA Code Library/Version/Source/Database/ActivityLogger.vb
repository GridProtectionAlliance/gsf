Imports System.ComponentModel
Imports System.Data.OleDb
Imports System.Text
Imports System.Drawing
Imports TVA.Shared.String
Imports TVA.Shared.DateTime
Imports TVA.Database
Imports TVA.Database.Common
Imports TVA.Services

Namespace Database

    ' This class queues up status log entries for delayed writes on a timer thread so that status postings
    ' can be logged as much as needed without causing delays in general processing
    <ToolboxBitmap(GetType(ActivityLogger), "ActivityLogger.bmp"), DefaultProperty("ConnectString")> _
    Public Class ActivityLogger

        Inherits Component
        Implements IServiceComponent

        Public Class LogEntry

            Public Status As String
            Public Logged As Boolean
            Public EntryType As String
            Public UserData As Object

        End Class

        Public Class LogEntries

            Private colEntries As ArrayList

            Friend Sub New(Optional ByVal BaseList As ArrayList = Nothing)

                If BaseList Is Nothing Then
                    colEntries = New ArrayList()
                Else
                    colEntries = BaseList
                End If

            End Sub

            Default Public ReadOnly Property Item(ByVal Index As Integer) As LogEntry
                Get
                    If Index < 0 Or Index >= colEntries.Count Then
                        Throw New IndexOutOfRangeException()
                    Else
                        Return DirectCast(colEntries(Index), LogEntry)
                    End If
                End Get
            End Property

            Public Sub Add(ByVal Entry As LogEntry)

                colEntries.Add(Entry)

            End Sub

            Public ReadOnly Property Count() As Integer
                Get
                    Return colEntries.Count
                End Get
            End Property

            Public ReadOnly Property SyncRoot() As Object
                Get
                    Return colEntries.SyncRoot
                End Get
            End Property

            Public Function GetEnumerator() As IEnumerator

                Return colEntries.GetEnumerator()

            End Function

            Friend Sub RemoveAt(ByVal Index As Integer)

                colEntries.RemoveAt(Index)

            End Sub

            Friend Function Clone() As LogEntries

                Return New LogEntries(colEntries.Clone())

            End Function

            Friend Sub Clear()

                colEntries.Clear()

            End Sub

        End Class

        <Browsable(False)> Public TotalSuccessfulLogs As Long
        <Browsable(False)> Public TotalExceptions As Long
        <Browsable(False)> Public LogEntryQueue As LogEntries

        Protected WithEvents QueueTimer As System.Timers.Timer
        Protected QueueEnabled As Boolean
        Protected LoggingThreshold As Integer   ' If this number of log entries is exceeded, we go ahead and flush the log...
        Protected ExceptionThreshold As Integer ' If this number of log entry exceptions is exceeded, we dump the entries...

        Private strConnectString As String
        Private strLogName As String
        Private strInsertSql As String
        Private strCountSql As String
        Private strDeleteSql As String
        Private strStatusSql As String
        Private intMaxLogEntries As Integer

        <Description("This event will be raised any time there is an error during the logging of activities.")> _
        Public Event LogException(ByVal ex As Exception, ByVal TotalExceptions As Long)

        <Description("This event will be raised any time there have been a number of exceptions in quick succession and there is a backlog of log entries.")> _
        Public Event LogExceptionDump(ByVal UnloggedItems As LogEntries)

        Public Sub New()

            ' We allow user to optionally assign a name to the activity logger in case they are creating unique multiple instances
            strConnectString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\ActivityLog.mdb"
            strLogName = Me.GetType.Name
            strInsertSql = "INSERT INTO StatusLog(Status, Logged, EntryType) VALUES ('%Status%', %Logged%, '%EntryType%')"
            strCountSql = "SELECT COUNT(*) AS Total FROM StatusLog"
            strDeleteSql = "DELETE * FROM StatusLog WHERE ID IN (SELECT TOP 1 ID FROM StatusLog ORDER BY ID)"

            LogEntryQueue = New LogEntries()
            QueueTimer = New System.Timers.Timer()

            QueueTimer.AutoReset = False
            QueueTimer.Interval = 2000      ' We setup a two second log timer
            QueueTimer.Enabled = False
            QueueEnabled = True

            LoggingThreshold = 10
            ExceptionThreshold = 10

        End Sub

        Protected Overrides Sub Finalize()

            Dispose(True)

        End Sub

        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)

            Shutdown()

        End Sub

        Public Sub Shutdown() Implements IServiceComponent.Dispose

            GC.SuppressFinalize(Me)
            QueueEnabled = False
            Flush()

        End Sub

        Public Sub Flush()

            Dim flgOrgState As Boolean = QueueEnabled

            Enabled = False
            If QueueCount > 0 Then QueueTimer_Elapsed(Nothing, Nothing)
            Enabled = flgOrgState

        End Sub

        <Browsable(True), Category("Configuration"), Description("OLEDB connection string to activity log.")> _
        Public Property ConnectString() As String
            Get
                Return strConnectString
            End Get
            Set(ByVal Value As String)
                strConnectString = Value
            End Set
        End Property

        <Browsable(True), Category("Service Related"), Description("When used within a service, you can optionally assign a unique name to the activity logger in case you will be creating multiple instances that need to report status to a service."), DefaultValue("ActivityLogger")> _
        Public Property LogName() As String
            Get
                Return strLogName
            End Get
            Set(ByVal Value As String)
                strLogName = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Defines the insert Sql the activity logger will use.  The following replaceable tokens are available: %Status%, %Logged%, %EntryType%, and %UserData%")> _
        Public Property InsertSql() As String
            Get
                Return strInsertSql
            End Get
            Set(ByVal Value As String)
                strInsertSql = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Defines the scalar count Sql expression the activity logger will use.  This is only used when a MaxLogEntries setting is specified.")> _
        Public Property CountSql() As String
            Get
                Return strCountSql
            End Get
            Set(ByVal Value As String)
                strCountSql = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Defines the delete Sql the activity logger will use.  This is only used when a MaxLogEntries setting is specified.")> _
        Public Property DeleteSql() As String
            Get
                Return strDeleteSql
            End Get
            Set(ByVal Value As String)
                strDeleteSql = Value
            End Set
        End Property

        <Browsable(True), Category("Service Related"), Description("When used within a service, you can optionally specify a Sql statement the activity logger will use to return the most recent log entries to return to a service status request.  Only the first field specified in the Sql expression will be used for status display, the field name is not important.  Make sure your Sql statement includes a row restriction clause, e.g., SELECT TOP 5 * FROM StatusLog"), DefaultValue("")> _
        Public Property StatusSql() As String
            Get
                Return strStatusSql
            End Get
            Set(ByVal Value As String)
                strStatusSql = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Defines the maximum number of log entries to maintain - set to 0 for no maximum."), DefaultValue(0)> _
        Public Property MaxLogEntries() As Integer
            Get
                Return intMaxLogEntries
            End Get
            Set(ByVal Value As Integer)
                intMaxLogEntries = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Determines if activity logger is enabled."), DefaultValue(True)> _
        Public Property Enabled() As Boolean
            Get
                Return QueueEnabled
            End Get
            Set(ByVal Value As Boolean)
                QueueEnabled = Value
                If Not DesignMode Then
                    If Value Then
                        QueueEnabled = (QueueCount > 0)
                    Else
                        QueueTimer.Enabled = False
                    End If
                End If
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("If this number of activity entries is exceeded, we go ahead and flush the collection."), DefaultValue(10)> _
        Public Property EventLogThreshold() As Integer
            Get
                Return LoggingThreshold
            End Get
            Set(ByVal Value As Integer)
                LoggingThreshold = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("If this number of logging exceptions is exceeded in quick succession (less than 30 seconds), we dump the log entries through the LogExceptionDump event so user can handle unloggable items."), DefaultValue(10)> _
        Public Property ExceptionDumpThreshold() As Integer
            Get
                Return ExceptionThreshold
            End Get
            Set(ByVal Value As Integer)
                ExceptionThreshold = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("This value determines how often to log buffered activities."), DefaultValue(2)> _
        Public Overridable Property EventLogInterval() As Integer
            Get
                Return QueueTimer.Interval \ 1000
            End Get
            Set(ByVal Value As Integer)
                QueueTimer.Interval = Value * 1000
            End Set
        End Property

        Public Overridable Sub AddLogEntry(ByVal Status As String, ByVal Logged As Boolean, ByVal EntryType As String, Optional ByVal UserData As Object = Nothing)

            Dim leItem As New LogEntry

            With leItem
                .Status = SqlEncode(Status)
                .Logged = Logged
                .EntryType = SqlEncode(EntryType)
                .UserData = UserData
            End With

            SyncLock LogEntryQueue.SyncRoot
                ' As long as we are within the log entry threshold, we'll keep resetting the log timer
                If LogEntryQueue.Count <= LoggingThreshold Then QueueTimer.Enabled = False
                LogEntryQueue.Add(leItem)
            End SyncLock

            QueueTimer.Enabled = (QueueEnabled And QueueCount > 0)

        End Sub

        <Browsable(False)> _
        Public Overridable ReadOnly Property QueueCount() As Integer
            Get
                Dim intCount As Integer

                SyncLock LogEntryQueue.SyncRoot
                    intCount = LogEntryQueue.Count
                End SyncLock

                Return intCount
            End Get
        End Property

        Private Sub QueueTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles QueueTimer.Elapsed

            Dim cnn As New OleDbConnection(ConnectString)
            Dim colLogEntries As LogEntries
            Dim leItem As LogEntry

            Try
                ' We create a synchronized copy of the items that we are about to log so that
                ' new items can be added without delay and not interfere with current log process
                SyncLock LogEntryQueue.SyncRoot
                    colLogEntries = LogEntryQueue.Clone()
                    LogEntryQueue.Clear()
                End SyncLock

                If colLogEntries.Count > 0 Then
                    Dim strSql As String

                    cnn.Open()

                    ' Process all queued log entry items, removing items as they are successfully logged
                    While True
                        leItem = DirectCast(colLogEntries(0), LogEntry)

                        With leItem
                            ' Replace Sql tokens with actual values
                            strSql = Replace(InsertSql, "%Status%", .Status, 1, -1, CompareMethod.Text)
                            strSql = Replace(strSql, "%Logged%", Math.Abs(CInt(.Logged)), 1, -1, CompareMethod.Text)
                            strSql = Replace(strSql, "%EntryType%", .EntryType, 1, -1, CompareMethod.Text)
                            If Not .UserData Is Nothing Then strSql = Replace(strSql, "%UserData%", CStr(.UserData), 1, -1, CompareMethod.Text)
                        End With

                        ExecuteNonQuery(strSql, cnn)
                        TotalSuccessfulLogs += 1

                        colLogEntries.RemoveAt(0)
                        If colLogEntries.Count = 0 Then Exit While
                    End While

                    ' Maintain log file size if requested
                    If MaxLogEntries > 0 Then
                        While ExecuteScalar(CountSql, cnn) > MaxLogEntries
                            ExecuteNonQuery(DeleteSql, cnn)
                        End While
                    End If
                End If
            Catch ex As Exception
                Static dblLastExceptionTime As Double
                Static intQuickExceptions As Integer

                TotalExceptions += 1
                RaiseEvent LogException(ex, TotalExceptions)

                If Timer - dblLastExceptionTime < 30 Then
                    intQuickExceptions += 1
                Else
                    intQuickExceptions = 1
                End If
                dblLastExceptionTime = Timer

                If intQuickExceptions > ExceptionThreshold And colLogEntries.Count > 1 Then
                    ' If there have been a number of exceptions in quick succession and the
                    ' queue is backlogged, we are just going to dump the log entries - we can't
                    ' keep accumulating these items and consuming excessive amounts of RAM.
                    ' We'll let the user do something with these unloggable items if they desire...
                    RaiseEvent LogExceptionDump(colLogEntries)
                Else
                    ' Restore unlogged items to log entry queue
                    If Not colLogEntries Is Nothing Then
                        SyncLock LogEntryQueue.SyncRoot
                            ' Add any log items that may have been added
                            ' during current log processing...
                            For Each leItem In LogEntryQueue
                                colLogEntries.Add(leItem)
                            Next

                            ' Recreate log entry queue
                            LogEntryQueue.Clear()

                            For Each leItem In colLogEntries
                                LogEntryQueue.Add(leItem)
                            Next
                        End SyncLock
                    End If
                End If
            Finally
                If Not cnn Is Nothing Then cnn.Close()
            End Try

            ' Keep the log timer alive so long as there are more log entries in the queue
            QueueTimer.Enabled = (QueueEnabled And QueueCount > 0)

        End Sub

        <Browsable(False)> _
        Public Overridable ReadOnly Property Name() As String Implements IServiceComponent.Name
            Get
                Return LogName
            End Get
        End Property

        Public Overridable Sub ProcessStateChanged(ByVal NewState As ProcessState) Implements IServiceComponent.ProcessStateChanged

            ' Logger doesn't need to respond changes in process state

        End Sub

        Public Overridable Sub ServiceStateChanged(ByVal NewState As ServiceState) Implements IServiceComponent.ServiceStateChanged

            ' When used as a service component, we should respectfully respond to service pause and resume requests
            Select Case NewState
                Case ServiceState.Paused
                    Enabled = False
                Case ServiceState.Resumed
                    Enabled = True
            End Select

        End Sub

        <Browsable(False)> _
        Public Overridable ReadOnly Property Status() As String Implements IServiceComponent.Status
            Get
                Dim strStatus As New StringBuilder

                ' Respond to the Freudrian client's service request to "tell me about yourself"...
                strStatus.Append("Logging is currently " & IIf(Enabled, "enabled", "disabled") & ":" & vbCrLf)
                strStatus.Append("   Total logged entries : " & TotalSuccessfulLogs & vbCrLf)
                strStatus.Append("       Total exceptions : " & TotalExceptions & vbCrLf)
                strStatus.Append("    Log entry threshold : " & LoggingThreshold & vbCrLf)
                strStatus.Append("   Current log interval : " & SecondsToText(EventLogInterval).ToLower() & vbCrLf)
                strStatus.Append("    Maximum log entries : " & MaxLogEntries & vbCrLf)
                strStatus.Append("     Queued log entries : " & QueueCount & vbCrLf)

                If Len(strStatusSql) > 0 Then
                    strStatus.Append("  Recent logged entries :" & vbCrLf & vbCrLf)
                    Try
                        Dim cnn As New OleDbConnection(ConnectString)
                        Dim x As Integer

                        With RetrieveData(strStatusSql, cnn).Rows
                            For x = 0 To .Count - 1
                                strStatus.Append(NotNull(.Item(x)(0)) & vbCrLf)
                            Next
                        End With
                    Catch ex As Exception
                        strStatus.Append("Error while retrieving recent status log entries: " & ex.Message & vbCrLf)
                    End Try
                End If

                Return strStatus.ToString()
            End Get
        End Property

    End Class

End Namespace