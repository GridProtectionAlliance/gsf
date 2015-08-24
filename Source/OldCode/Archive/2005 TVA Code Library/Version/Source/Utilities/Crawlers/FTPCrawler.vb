' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.Text
Imports System.Text.RegularExpressions
Imports System.ComponentModel
Imports System.Drawing
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Services
Imports TVA.Threading

Namespace Utilities.Crawlers

    <ToolboxBitmap(GetType(FTPCrawler), "FTPCrawler.bmp"), DefaultProperty("StartPath"), DefaultEvent("IndexFile")> _
    Public Class FTPCrawler

        Inherits Component
        Implements IServiceComponent
        Implements ICrawler

        Public Event IndexFile(ByVal File As FTP.File)
        Public Event IndexDirectory(ByVal Directory As FTP.Directory)
        Public Event IndexingComplete() Implements ICrawler.IndexingComplete
        Public Event CrawlerError(ByVal FileName As String, ByVal [Step] As String, ByVal [Error] As String) Implements ICrawler.CrawlerError

        Public Event CrawlerStatus(ByVal StatusText As String)
        Public Event InternalSessionCommand(ByVal Command As String)
        Public Event InternalSessionResponse(ByVal Response As String)

        ' This event is implemented on behalf of the ICrawler interface, but is hidden from the editor
        ' to prevent end user confusion with the IndexFile event which perfroms the same function...
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Event IndexDocument(ByVal Document As Object) Implements ICrawler.IndexDocument

        Private WithEvents FTPSession As FTP.Session                ' FTP session
        Private WithEvents ThreadState As RunThread                 ' Indexing thread state
        Private IndexedFiles As Long                                ' Total indexed file count
        Private StartTime As Double                                 ' Start timer value of indexing run
        Private StopTime As Double                                  ' Stop timer value of indexing run
        Private Shared CachedFilePatterns As New Hashtable          ' Cached file match patterns
        Private WithEvents RestartTimer As Timers.Timer             ' Connection cycle timer

        ' Exposed FTPCrawler property values
        Private CrawlerStartPath As String
        Private CrawlerFileInclusion As String
        Private CrawlerFileExclusion As String
        Private CrawlerDirectoryRecurse As Boolean
        Private CrawlerDirectoryInclusion As String
        Private CrawlerDirectoryExclusion As String
        Private CrawlerEnabled As Boolean
        Private CrawlerUserName As String
        Private CrawlerPassword As String

        Public Sub New()

            FTPSession = New FTP.Session(False)
            CrawlerStartPath = ""
            CrawlerFileInclusion = "*"
            CrawlerFileExclusion = ""
            CrawlerDirectoryRecurse = True
            CrawlerDirectoryInclusion = "*"
            CrawlerDirectoryExclusion = ""
            CrawlerEnabled = True

            ' Define a timer for FTP connection in case of availability failures
            RestartTimer = New Timers.Timer
            With RestartTimer
                .AutoReset = False
                .Interval = 10000
                .Enabled = False
            End With

        End Sub

        Public Sub New(ByVal CaseInsensitive As Boolean)

            MyClass.New()
            FTPSession.CaseInsensitive = CaseInsensitive

        End Sub

        Protected Overrides Sub Finalize()

            Shutdown()

        End Sub

        Public Sub Shutdown() Implements Services.IServiceComponent.Dispose, ICrawler.Shutdown

            MyBase.Dispose()
            StopIndexing()
            GC.SuppressFinalize(Me)

        End Sub

        <Browsable(True), Category("Configuration"), Description("Start directory for FTP crawler.  Leave blank to start crawl from initial FTP session directory."), DefaultValue("")> _
        Public Property StartPath() As String Implements ICrawler.StartPath
            Get
                Return CrawlerStartPath
            End Get
            Set(ByVal Value As String)
                CrawlerStartPath = JustPath(Value)
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Specify FTP server name (do not prefix with ftp://).")> _
        Public Overridable Property Server() As String
            Get
                Return FTPSession.Server
            End Get
            Set(ByVal Value As String)
                FTPSession.Server = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set to True to not be case sensitive with FTP file names."), DefaultValue(False)> _
        Public Property CaseInsensitive() As Boolean
            Get
                Return FTPSession.CaseInsensitive
            End Get
            Set(ByVal Value As Boolean)
                FTPSession.CaseInsensitive = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Specify FTP server login user name"), DefaultValue("")> _
        Public Overridable Property UserName() As String
            Get
                Return CrawlerUserName
            End Get
            Set(ByVal Value As String)
                CrawlerUserName = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Specify FTP server login password"), DefaultValue("")> _
        Public Overridable Property Password() As String
            Get
                Return CrawlerPassword
            End Get
            Set(ByVal Value As String)
                CrawlerPassword = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Determines if the FTP crawler is enabled."), DefaultValue(True)> _
        Public Property Enabled() As Boolean Implements ICrawler.Enabled
            Get
                Return CrawlerEnabled
            End Get
            Set(ByVal Value As Boolean)
                CrawlerEnabled = Value

                If Not DesignMode Then
                    ' Pause or resume crawling, if needed
                    If Not ThreadState Is Nothing Then
                        If Value Then
                            ThreadState.Thread.Resume()
                        Else
                            ThreadState.Thread.Suspend()
                        End If
                    End If
                End If
            End Set
        End Property

        <Browsable(True), Category("Crawler Directives"), Description("Files to include in crawl."), DefaultValue("*")> _
        Public Property FileInclusion() As String
            Get
                Return CrawlerFileInclusion
            End Get
            Set(ByVal Value As String)
                CrawlerFileInclusion = Value
            End Set
        End Property

        <Browsable(True), Category("Crawler Directives"), Description("Files to exclude from crawl."), DefaultValue("")> _
        Public Property FileExclusion() As String
            Get
                Return CrawlerFileExclusion
            End Get
            Set(ByVal Value As String)
                CrawlerFileExclusion = Value
            End Set
        End Property

        <Browsable(True), Category("Crawler Directives"), Description("Set to False to not recurse into directories."), DefaultValue(True)> _
        Public Property DirectoryRecurse() As Boolean
            Get
                Return CrawlerDirectoryRecurse
            End Get
            Set(ByVal Value As Boolean)
                CrawlerDirectoryRecurse = Value
            End Set
        End Property

        <Browsable(True), Category("Crawler Directives"), Description("Directories to include in crawl."), DefaultValue("*")> _
        Public Property DirectoryInclusion() As String
            Get
                Return CrawlerDirectoryInclusion
            End Get
            Set(ByVal Value As String)
                CrawlerDirectoryInclusion = Value
            End Set
        End Property

        <Browsable(True), Category("Crawler Directives"), Description("Directories to exclude from crawl."), DefaultValue("")> _
        Public Property DirectoryExclusion() As String
            Get
                Return CrawlerDirectoryExclusion
            End Get
            Set(ByVal Value As String)
                CrawlerDirectoryExclusion = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property TotalIndexedFiles() As Long Implements ICrawler.TotalIndexedDocuments
            Get
                Return IndexedFiles
            End Get
        End Property

        Private Sub RaiseCrawlerError(ByVal FileName As String, ByVal [Step] As String, ByVal [Error] As String)

            Try
                RaiseEvent CrawlerError(FileName, [Step], [Error])
            Catch
                ' Not stopping for end-user event handling code error's when we're already reporting an exception of our own
            End Try

        End Sub

        Private Sub RaiseIndexFile(ByVal File As FTP.File)

            Try
                RaiseEvent IndexFile(File)
                RaiseEvent IndexDocument(File)
            Catch ex As Exception
                ' Not stopping for end-user event handling code errors while we're in the middle of indexing,
                ' but we'll let them know something's broke...
                RaiseCrawlerError(File.FullPath, "End User ""IndexFile"" Event Handler", ex.Message)
            End Try

        End Sub

        Private Sub RaiseIndexDirectory(ByVal Directory As FTP.Directory)

            Try
                RaiseEvent IndexDirectory(Directory)
            Catch ex As Exception
                ' Not stopping for end-user event handling code errors while we're in the middle of indexing,
                ' but we'll let them know something's broke...
                RaiseCrawlerError(Directory.FullPath, "End User ""IndexDirectory"" Event Handler", ex.Message)
            End Try

        End Sub

        Public Sub StartIndexing() Implements ICrawler.StartIndexing

            If Len(CrawlerStartPath) = 0 Then Throw New NullReferenceException("Cannot start indexing - no starting path was specified for crawler.  Check ""StartPath"" property.")
            If Len(CrawlerUserName) = 0 Then Throw New NullReferenceException("Cannot start indexing - no FTP server login user name specified.  Check ""UserName"" property.")
            If Not CrawlerEnabled Then Throw New InvalidOperationException("Cannot start indexing - FTP crawler is not enabled.  Check ""Enabled"" property.")
            If IsActive Then Throw New InvalidOperationException("Indexing already in progress.")

            ' Attempt to connect to FTP session
            ConnectSession(UserName, Password)

            IndexedFiles = 0
            StopTime = 0
            StartTime = Timer
            ThreadState = RunThread.ExecuteNonPublicMethod(Me, "CrawlFTP")

        End Sub

        Private Sub ConnectSession(ByVal UserName As String, ByVal Password As String)

            FTPSession.Connect(UserName, Password)
            RaiseEvent CrawlerStatus("[" & Now() & "] FTP crawler connected to ""ftp://" & UserName & "@" & FTPSession.Server & """")

            With FTPSession
                .SetCurrentDirectory(CrawlerStartPath)

                If Len(CrawlerStartPath) > 0 Then
                    RaiseEvent CrawlerStatus("[" & Now() & "] FTP crawler starting directory """ & CrawlerStartPath & """")
                Else
                    RaiseEvent CrawlerStatus("[" & Now() & "] No FTP crawler starting directory specified - crawling initial folder")
                End If
            End With

        End Sub

        Private Sub CloseSession()

            Try
                FTPSession.Close()
            Catch
            End Try

        End Sub

        Public Sub StopIndexing() Implements ICrawler.StopIndexing

            If Not ThreadState Is Nothing Then
                If ThreadState.Thread.IsAlive Then ThreadState.Abort()
            End If
            ThreadState = Nothing
            StopTime = Timer
            RaiseEvent IndexingComplete()
            CloseSession()

        End Sub

        Private Sub CrawlFTP()

            ' Get paths into string arrays
            Dim IncludedFiles As String() = GetPaths(FileInclusion)
            Dim ExcludedFiles As String() = GetPaths(FileExclusion)
            Dim IncludedDirs As String() = GetPaths(DirectoryInclusion)
            Dim ExcludedDirs As String() = GetPaths(DirectoryExclusion)

            If IncludedFiles.Length = 0 Then IncludedFiles = GetPaths("*")
            If IncludedDirs.Length = 0 Then IncludedDirs = GetPaths("*")

            ' Start FTP crawl
            RaiseIndexDirectory(FTPSession.CurrentDirectory)
            IndexFiles(CrawlerStartPath, IncludedFiles, ExcludedFiles, IncludedDirs, ExcludedDirs)

        End Sub

        Private Sub ThreadState_ThreadComplete() Handles ThreadState.ThreadComplete

            StopIndexing()

        End Sub

        Private Sub ThreadState_ThreadExecError(ByVal ex As System.Exception) Handles ThreadState.ThreadExecError

            RaiseCrawlerError(CrawlerStartPath, "FTP Crawl", "Failed during FTP crawl started from path """ & CrawlerStartPath & """ due to exception: " & ex.Message)

        End Sub

        Public ReadOnly Property RunTime() As Double Implements ICrawler.RunTime
            Get
                If StopTime > StartTime Then
                    Return StopTime - StartTime
                ElseIf StartTime > 0 Then
                    Return Timer - StartTime
                Else
                    Return 0
                End If
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsActive() As Boolean Implements ICrawler.IsActive
            Get
                Return (CrawlerEnabled And Not ThreadState Is Nothing)
            End Get
        End Property

        <Browsable(False)> _
        Public Overridable ReadOnly Property IsConnected() As Boolean
            Get
                Return FTPSession.IsConnected
            End Get
        End Property

        Private Function GetPaths(ByVal Directive As String) As String()

            Dim PathList As New ArrayList
            Dim Path As String

            For Each Path In Directive.Split(";")
                Path = Trim(Path)
                If Len(Path) > 0 Then PathList.Add(Path)
            Next

            Return PathList.ToArray(GetType(String))

        End Function

        ' Since the FTP file server is finite in scope, we can just use recursion to go through and crawl the FTP
        Private Sub IndexFiles(ByVal Path As String, ByVal IncludedFiles As String(), ByVal ExcludedFiles As String(), ByVal IncludedDirs As String(), ByVal ExcludedDirs As String())

            If FTPSession.IsConnected Then
                Try
                    ' Attempt to access specified directory
                    FTPSession.SetCurrentDirectory(Path)

                    Try
                        Dim File As FTP.File
                        Dim Files As IDictionaryEnumerator
                        Dim Directory As FTP.Directory
                        Dim Directories As IDictionaryEnumerator
                        Dim FilePath As String

                        ' Index files in the folder
                        Files = FTPSession.CurrentDirectory.Files.GetEnumerator()
                        While Files.MoveNext()
                            File = CType(Files.Value, FTP.File)
                            FilePath = JustFileName(File.FullPath)
                            If IsFilePatternMatch(IncludedFiles, FilePath) And Not IsFilePatternMatch(ExcludedFiles, FilePath) Then
                                IndexedFiles += 1
                                RaiseIndexFile(File)
                            End If
                        End While

                        ' Recurse into any sub-directories of this folder and index those files
                        If DirectoryRecurse Then
                            Directories = FTPSession.CurrentDirectory.SubDirectories.GetEnumerator()
                            While Directories.MoveNext()
                                Directory = CType(Files.Value, FTP.Directory)
                                FilePath = LastDirectoryName(Directory.FullPath)
                                If IsFilePatternMatch(IncludedDirs, FilePath) And Not IsFilePatternMatch(ExcludedDirs, FilePath) Then
                                    RaiseIndexDirectory(Directory)
                                    IndexFiles(Directory.FullPath, IncludedFiles, ExcludedFiles, IncludedDirs, ExcludedDirs)
                                End If
                            End While
                        End If
                    Catch ex As Exception
                        RaiseCrawlerError(Path, "Indexing FTP Directory", ex.Message)
                    End Try
                Catch ex As Exception
                    RaiseCrawlerError(Path, "Changing FTP Directory", ex.Message)
                End Try
            Else
                RaiseCrawlerError(Path, "Indexing FTP Directory", "FTP crawler is no longer connected to server """ & FTPSession.Server & """.")
            End If

        End Sub

        Private Shared Function IsFilePatternMatch(ByVal FileSpecs As String(), ByVal FileName As String) As Boolean

            Dim Found As Boolean

            For Each FileSpec As String In FileSpecs
                If IsFilePatternMatch(FileSpec, FileName) Then
                    Found = True
                    Exit For
                End If
            Next

            Return Found

        End Function

        Private Shared Function IsFilePatternMatch(ByVal FileSpec As String, ByVal FileName As String) As Boolean

            Dim FilePattern As Regex
            FileSpec = Trim(FileSpec)

            ' We statically cache file pattern regular expressions since they may be used frequently
            FilePattern = CachedFilePatterns(FileSpec)

            If FilePattern Is Nothing Then
                FilePattern = New Regex(GetFilePatternRegularExpression(FileSpec), RegexOptions.IgnoreCase)
                CachedFilePatterns.Add(FileSpec, FilePattern)
            End If

            Return FilePattern.IsMatch(JustFileName(FileName))

        End Function

        <Browsable(False)> _
        Public ReadOnly Property Name() As String Implements TVA.Services.IServiceComponent.Name, ICrawler.Name
            Get
                Return Me.GetType.Name
            End Get
        End Property

        Public Sub ProcessStateChanged(ByVal NewState As TVA.Services.ProcessState) Implements TVA.Services.IServiceComponent.ProcessStateChanged

            ' FTPCrawler, when used as a service component, doesn't need to respond to changes in process state

        End Sub

        Public Sub ServiceStateChanged(ByVal NewState As TVA.Services.ServiceState) Implements TVA.Services.IServiceComponent.ServiceStateChanged

            ' When used as a service component, we should respectfully respond to service pause and resume requests
            Select Case NewState
                Case ServiceState.Paused
                    Enabled = False
                Case ServiceState.Resumed
                    Enabled = True
            End Select

        End Sub

        <Browsable(False)> _
        Public ReadOnly Property Status() As String Implements TVA.Services.IServiceComponent.Status, ICrawler.Status
            Get
                Dim StatusText As New StringBuilder

                StatusText.Append("Current crawler state: " & IIf(IsActive, "Indexing", "Idle") & vbCrLf)
                StatusText.Append("   Crawler start path: " & CrawlerStartPath & vbCrLf)
                StatusText.Append("  Crawler FTP session: " & IIf(IsConnected, "Connected", "Not Connected") & vbCrLf)
                StatusText.Append("  Total indexed files: " & IndexedFiles & vbCrLf)
                StatusText.Append("     Total crawl time: " & SecondsToText(RunTime).ToLower() & vbCrLf)

                Return StatusText.ToString()
            End Get
        End Property

    End Class

End Namespace