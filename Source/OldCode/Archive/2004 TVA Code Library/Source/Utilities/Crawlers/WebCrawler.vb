' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.Net
Imports System.IO
Imports System.Text
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Thread
Imports TVA.Parsers.HTMLParser
Imports TVA.Parsers.HTMLParser.HTMLAttributeExtractor
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Services
Imports TVA.Threading
Imports Microsoft.Win32
Imports VB = Microsoft.VisualBasic

Namespace Utilities.Crawlers

    ' High-level BNF grammar and semantic rules for META tag directives that control how the web-crawler handles site indexing

    ' Semantic Rules / Functional Notes:

    ' Only indexing directives appearing inside of an HTML <HEAD> tag will be considered:

    ' <HTML>
    '    <HEAD>
    '        IndexingDirectives
    '    </HEAD>
    ' ...

    ' All tag, attributes and content are case and order insensitive.

    ' Only the INDEXME and FOLLOWLINKS directives are considered “page-level” and are targeted for indexing control of the current page.

    ' All other directives are just for general control of the web-crawler.

    ' Multiple indexing directives are allowed to be specified, for example:

    '      <META NAME=”ESOINDEX” INDEXME=”TRUE”>
    '      <META NAME=”ESOINDEX” FOLLOWLINKS=”FALSE”>
    '      <META NAME=”ESOINDEX” ALSOINDEX=”http://www.yahoo.com”>
    '      <META NAME=”ESOINDEX” ALSOINDEX=”SiteIndex.asp?DisplayType=Alpha”>

    ' is the same as the following:

    '      <META NAME=”ESOINDEX”
    '               ALSOINDEX=”http://www.yahoo.com”
    '               ALSOINDEX=”SiteIndex.asp?DisplayType=Alpha”
    '               INDEXME=”TRUE” FOLLOWLINKS=”FALSE”>

    ' Only multiple occurrences of the ALSOINDEX directive will be respected.  When multiple tags (or attributes) exist that specify
    ' a previously defined directive, the first encountered directive will take precedence.

    ' File system indexing will only occur when both the FILESYSTEMROOT and FILEWEBROOT directives exist.  These directives are
    ' intended to direct the web-crawler to index files from a file system perspective when these files are already assessable from
    ' the web (this is useful for indexing files that are “available” to be served by the web server, yet may or may not have direct
    ' links to these files – like in the case of the ESO ad-hoc document library).

    ' The optional FILEINCLUSION, FILEEXCLUSION, DIRRECURSE, DIRINCLUSION, and DIREXCLUSION directives allow detailed control of a
    ' “file-system” based index.  These directives are ignored unless the FILESYSTEMROOT and FILEWEBROOT directives exist.

    ' The optional DIRINCLUSION and DIREXCLUSION directives are ignored unless the DIRRECURSE is “TRUE”.

    ' The ALSOINDEX directive allows additional pages to be indexed by the web-crawler.

    ' Site Indexing Meta Tag BNF Grammar:

    ' IndexingDirectives    :=  ( IndexingMetaTag )*

    ' IndexingMetaTag       :=  <META ( AttributeList) * >

    ' AttributeList         :=  TagNameAttr | IndexMeAttr | FollowLinksAttr | FileSystemRootAttr | FileWebRootAttr | FileInclusionAttr
    '                           | FileExclustionAttr | DirRecurseAttr | DirInclusionAttr | DirExclusionAttr | AlsoIndexAttr

    ' TagNameAttr           :=  NAME = “Indexing Directive Name”

    ' IndexMeAttr           :=  INDEXME = (“TRUE” | “FALSE”)

    ' FollowLinksAttr       :=  FOLLOWLINKS = (“TRUE” | “FALSE”)

    ' FileSystemRootAttr    :=  FILESYSTEMROOT = “Fully qualified file system path”

    ' FileWebRootAttr       :=  FILEWEBROOT = “Fully qualified URI to web root path that matches file system root”

    ' FileInclusionAttr     :=  FILEINCLUSION = “semi-colon separated file names (wild cards OK) to include in file system index”

    ' FileExclustionAttr    :=  FILEEXCLUSION = “semi-colon separated file names (wild cards OK) to exclude from file system index”

    ' DirRecurseAttr        :=  DIRRECURSE = (“TRUE” | “FALSE”)

    ' DirInclusionAttr      :=  DIRINCLUSION = “semi-colon separated directory names (wild cards OK) to include in file system index”

    ' DirExclusionAttr      :=  DIREXCLUSION = “semi-colon separated directory names (wild cards OK) to exclude from file system index”

    ' AlsoIndexAttr         :=  ALSOINDEX = “Relative or fully qualified URI of another page that should be indexed”

    <ToolboxBitmap(GetType(WebCrawler), "WebCrawler.bmp"), DefaultProperty("StartPage"), DefaultEvent("IndexPage")> _
    Public Class WebCrawler

        Inherits Component
        Implements IServiceComponent
        Implements ICrawler

        Public Class Page

            Private Parent As WebCrawler
            Private Document As HTMLDocument
            Private FoundDirective As Hashtable

            ' Exposed Page property values
            Private PageURL As String
            Private PageSourceURL As String
            Private PageSourceHTML As String
            Private PageSourceText As String
            Private PageTitle As String
            Private PageContentType As String
            Private PageContentLength As Long
            Private PageIndexMe As Boolean
            Private PageFollowLinks As Boolean
            Private PageFileSystemRoot As String
            Private PageFileWebRoot As String
            Private PageFileInclusion As String
            Private PageFileExclusion As String
            Private PageDirectoryRecurse As Boolean
            Private PageDirectoryInclusion As String
            Private PageDirectoryExclusion As String
            Private PageAlsoIndex As String()

            Public Sub New(ByVal Parent As WebCrawler, ByVal SourceURL As String, ByVal URL As String)

                Me.Parent = Parent
                PageURL = URL
                PageSourceURL = SourceURL
                PageIndexMe = Parent.IndexMeDefault
                PageFollowLinks = Parent.FollowLinksDefault
                PageFileInclusion = Parent.FileInclusionDefault
                PageFileExclusion = Parent.FileExclusionDefault
                PageDirectoryRecurse = Parent.DirectoryRecurseDefault
                PageDirectoryInclusion = Parent.DirectoryInclusionDefault
                PageDirectoryExclusion = Parent.DirectoryExclusionDefault

            End Sub

            Public Property URL() As String
                Get
                    Return PageURL
                End Get
                Set(ByVal Value As String)
                    PageURL = Value
                End Set
            End Property

            Public Property SourceURL() As String
                Get
                    Return PageSourceURL
                End Get
                Set(ByVal Value As String)
                    PageSourceURL = Value
                End Set
            End Property

            Public ReadOnly Property SourceText() As String
                Get
                    If Len(PageSourceText) = 0 Then
                        Try
                            ' Extract raw text from HTML source
                            PageSourceText = Trim(HTMLTextExtractor.RawText(Document))
                        Catch ex As Exception
                            Parent.RaiseCrawlerError(PageURL, "Source Text Extraction", "Failed to extract raw text data for """ & PageURL & """ due to exception: " & ex.Message)
                        End Try
                    End If

                    Return PageSourceText
                End Get
            End Property

            Public Property SourceHTML() As String
                Get
                    Return PageSourceHTML
                End Get
                Set(ByVal Value As String)
                    PageSourceHTML = Value
                    PageSourceText = ""

                    ' Parse HTML source
                    Try
                        Document = HTMLParser.ParseSource(PageSourceHTML)
                    Catch ex As Exception
                        Parent.RaiseCrawlerError(PageURL, "HTML Parse", "Failed to parse HTML for """ & PageURL & """ due to exception: " & ex.Message)
                        Document = HTMLParser.ParseSource("<HTML></HTML>")
                    End Try

                    ' Parse META tags
                    Try
                        ParseMetaTags()
                    Catch ex As Exception
                        Parent.RaiseCrawlerError(PageURL, "HTML META Tag Parse", "Failed while parsing HTML META Tags for """ & PageURL & """ due to exception: " & ex.Message)
                    End Try
                End Set
            End Property

            Public ReadOnly Property ParsedHTMLDocument() As HTMLDocument
                Get
                    Return Document
                End Get
            End Property

            Public Property Title() As String
                Get
                    Return PageTitle
                End Get
                Set(ByVal Value As String)
                    PageTitle = Value
                End Set
            End Property

            Public Property ContentType() As String
                Get
                    Return PageContentType
                End Get
                Set(ByVal Value As String)
                    PageContentType = Value
                End Set
            End Property

            Public Property ContentLength() As Long
                Get
                    Return PageContentLength
                End Get
                Set(ByVal Value As Long)
                    PageContentLength = Value
                End Set
            End Property

            Public Property IndexMe() As Boolean
                Get
                    Return PageIndexMe
                End Get
                Set(ByVal Value As Boolean)
                    PageIndexMe = Value
                End Set
            End Property

            Public Property FollowLinks() As Boolean
                Get
                    Return PageFollowLinks
                End Get
                Set(ByVal Value As Boolean)
                    PageFollowLinks = Value
                End Set
            End Property

            Public Property FileSystemRoot() As String
                Get
                    Return PageFileSystemRoot
                End Get
                Set(ByVal Value As String)
                    PageFileSystemRoot = Value
                End Set
            End Property

            Public Property FileWebRoot() As String
                Get
                    If Len(PageFileWebRoot) > 0 Then
                        If PageFileWebRoot.Chars(PageFileWebRoot.Length - 1) <> "/" Then
                            PageFileWebRoot = JustPath(PageFileWebRoot).Replace("\", "/")
                        End If
                    Else
                        PageFileWebRoot = "/"
                    End If

                    Return PageFileWebRoot
                End Get
                Set(ByVal Value As String)
                    PageFileWebRoot = Value
                End Set
            End Property

            Public Property FileInclusion() As String
                Get
                    Return PageFileInclusion
                End Get
                Set(ByVal Value As String)
                    PageFileInclusion = Value
                End Set
            End Property

            Public Property FileExclusion() As String
                Get
                    Return PageFileExclusion
                End Get
                Set(ByVal Value As String)
                    PageFileExclusion = Value
                End Set
            End Property

            Public Property DirectoryRecurse() As Boolean
                Get
                    Return PageDirectoryRecurse
                End Get
                Set(ByVal Value As Boolean)
                    PageDirectoryRecurse = Value
                End Set
            End Property

            Public Property DirectoryInclusion() As String
                Get
                    Return PageDirectoryInclusion
                End Get
                Set(ByVal Value As String)
                    PageDirectoryInclusion = Value
                End Set
            End Property

            Public Property DirectoryExclusion() As String
                Get
                    Return PageDirectoryExclusion
                End Get
                Set(ByVal Value As String)
                    PageDirectoryExclusion = Value
                End Set
            End Property

            Public Property AlsoIndex() As String()
                Get
                    Return PageAlsoIndex
                End Get
                Set(ByVal Value As String())
                    PageAlsoIndex = Value
                End Set
            End Property

            Private Sub ParseMetaTags()

                Dim AlsoIndex As New ArrayList

                FoundDirective = New Hashtable

                ' Parse out HTML header attributes
                With HTMLAttributeExtractor.Attributes(Document)
                    ' Get page title
                    PageTitle = .PageTitle

                    ' Look for indexing directive meta-tags
                    For Each Tag As MetaTag In .MetaTags.GetMetaTags(Parent.DirectiveName)
                        SetDirective(Tag, "INDEXME", PageIndexMe)
                        SetDirective(Tag, "FOLLOWLINKS", PageFollowLinks)
                        SetDirective(Tag, "FILESYSTEMROOT", PageFileSystemRoot)
                        SetDirective(Tag, "FILEWEBROOT", PageFileWebRoot)
                        SetDirective(Tag, "FILEINCLUSION", PageFileInclusion)
                        SetDirective(Tag, "FILEEXCLUSION", PageFileExclusion)
                        SetDirective(Tag, "DIRRECURSE", PageDirectoryRecurse)
                        SetDirective(Tag, "DIRINCLUSION", PageDirectoryInclusion)
                        SetDirective(Tag, "DIREXCLUSION", PageDirectoryExclusion)

                        ' We can have multiple occurrences of ALSO-INDEX, so we handle these separately
                        For Each Attr As HTMLDocument.Attribute In Tag.Attributes.GetAttributes("ALSOINDEX")
                            If Len(Attr.value) > 0 Then AlsoIndex.Add(Attr.value)
                        Next
                    Next
                End With

                PageAlsoIndex = AlsoIndex.ToArray(GetType(String))

                FoundDirective = Nothing

            End Sub

            Private Sub SetDirective(ByVal Tag As MetaTag, ByVal DirectiveName As String, ByRef DirectiveValue As String)

                FindDirective(Tag, DirectiveName, DirectiveValue)

            End Sub

            Private Sub SetDirective(ByVal Tag As MetaTag, ByVal DirectiveName As String, ByRef DirectiveValue As Boolean)

                Dim Value As String

                If FindDirective(Tag, DirectiveName, Value) Then
                    Try
                        DirectiveValue = CBool(Value)
                    Catch
                    End Try
                End If

            End Sub

            Private Function FindDirective(ByVal Tag As MetaTag, ByVal DirectiveName As String, ByRef DirectiveValue As String) As Boolean

                Dim Value As String
                Dim Found As Boolean

                ' Only the first encountered directive will be considered...
                If Not FoundDirective.ContainsKey(DirectiveName) Then FoundDirective.Add(DirectiveName, False)
                If Not FoundDirective(DirectiveName) Then
                    Value = Tag.AttributeValue(DirectiveName)
                    If Len(Value) > 0 Then
                        DirectiveValue = Value
                        FoundDirective(DirectiveName) = True
                        Found = True
                    End If
                End If

                Return Found

            End Function

        End Class

        ' This class indexes a file system path on an independent thread
        Private Class FileSystemIndexer

            Private Parent As WebCrawler
            Private Directives As Page
            Private WithEvents Crawler As FileSystemCrawler

            Public Event IndexingComplete()

            Public Sub New(ByVal Parent As WebCrawler, ByVal Directives As Page)

                Me.Parent = Parent
                Me.Directives = Directives
                Me.Crawler = New FileSystemCrawler

            End Sub

            Public Sub StartIndexing()

                If Not Parent.IgnorePageDirectives And Parent.FileSystemIndexingAllowed Then
                    With Directives
                        ' We only crawl file system when both the file system root and file web root directives have been specified
                        If Len(.FileSystemRoot) > 0 And Len(.FileWebRoot) > 0 Then
                            Crawler.FileInclusion = .FileInclusion
                            Crawler.FileExclusion = .FileExclusion
                            Crawler.DirectoryInclusion = .DirectoryInclusion
                            Crawler.DirectoryExclusion = .DirectoryExclusion

                            ' Start file system crawl
                            Crawler.StartPath = .FileSystemRoot
                            Crawler.StartIndexing()
                        End If
                    End With
                End If

            End Sub

            Public Property Enabled() As Boolean
                Get
                    Return Crawler.Enabled
                End Get
                Set(ByVal Value As Boolean)
                    Crawler.Enabled = Value
                End Set
            End Property

            Public Sub Abort()

                Crawler.StopIndexing()

            End Sub

            Private Sub Crawler_IndexFile(ByVal FileName As String) Handles Crawler.IndexFile

                Dim URL As String
                Dim NewPage As Page
                Dim FileExtension As String
                Dim Source As String

                ' Generate associated URL for file name
                URL = GetURLOfFileName(FileName)
                NewPage = New Page(Parent, Directives.FileWebRoot, URL)
                FileExtension = JustFileExtension(FileName)

                NewPage.ContentType = GetContentType(FileExtension)
                NewPage.ContentLength = GetFileLength(FileName)

                If Parent.HTMLFileExtensions.BinarySearch(FileExtension, CaseInsensitiveComparer.Default) < 0 Then
                    ' Other file-types are not automatically handled, but we provide file name to user
                    ' in case they have a way of converting data type to HTML...
                    Source = Parent.UserHTMLConversion(FileName)
                Else
                    Source = Parent.GetHTML(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                End If

                ' We'll at least put a little something in the source to make the parser happy
                If Len(Source) = 0 Then Source = "<HTML></HTML>"

                ' Give HTML to new page object and let it begin it's parsing
                NewPage.SourceHTML = Source

                ' Provide page to user for indexing (or whatever they're crawling pages for...)
                Parent.IndexedPages += 1
                Parent.RaiseIndexPage(NewPage)

            End Sub

            Private Sub Crawler_CrawlerError(ByVal FileName As String, ByVal [Step] As String, ByVal [Error] As String) Handles Crawler.CrawlerError

                Parent.RaiseCrawlerError(GetURLOfFileName(FileName), [Step], [Error])

            End Sub

            Private Sub Crawler_IndexingComplete() Handles Crawler.IndexingComplete

                RaiseEvent IndexingComplete()

            End Sub

            Private Function GetURLOfFileName(ByVal FileName As String) As String

                ' Generate associated URL for file name
                Try
                    Return (Directives.FileWebRoot & FileName.Substring(Directives.FileSystemRoot.Length)).Replace("\", "/")
                Catch
                    Return Directives.FileWebRoot
                End Try

            End Function

            Private Function GetContentType(ByVal FileExtension As String) As String

                Dim ContentType As String

                ' Lookup content type in the registry
                With Registry.ClassesRoot
                    Try
                        With .OpenSubKey(FileExtension)
                            ContentType = .GetValue("Content Type")
                            .Close()
                        End With
                    Catch
                        ' May be an unknown content-type
                        ContentType = ""
                    End Try
                End With

                Return ContentType

            End Function

        End Class

        ' This class manages the file system indexing queue
        Private Class FileSystemIndexingQueue

            ' We queue up requests for file system indexing and process them one-by-one until we are finished...
            ' You could spawn up several of these at once, but given everything else that's already going on, I doubt
            ' it would finish any faster and it might start clogging file system IO - so file system indexing is
            ' restricted to single thread of execution

            Private Parent As WebCrawler
            Private Directives As ArrayList
            Private WithEvents CurrentIndexer As FileSystemIndexer

            Public Sub New(ByVal Parent As WebCrawler)

                Me.Parent = Parent
                Me.Directives = New ArrayList

            End Sub

            Public Sub QueueIndexRequest(ByVal IndexingDirectives As Page)

                SyncLock Directives.SyncRoot
                    Directives.Add(IndexingDirectives)
                End SyncLock

                StartNextIndexer()

            End Sub

            Public WriteOnly Property Enabled() As Boolean
                Set(ByVal Value As Boolean)
                    If Not CurrentIndexer Is Nothing Then
                        CurrentIndexer.Enabled = Value
                    End If
                End Set
            End Property

            Public Sub Abort()

                If IsActive Then CurrentIndexer.Abort()
                CurrentIndexer = Nothing

            End Sub

            Public ReadOnly Property IsActive() As Boolean
                Get
                    Return (Not CurrentIndexer Is Nothing Or QueueCount > 0)
                End Get
            End Property

            Public ReadOnly Property QueueCount() As Integer
                Get
                    SyncLock Directives.SyncRoot
                        Return Directives.Count
                    End SyncLock
                End Get
            End Property

            Private Sub StartNextIndexer()

                If CurrentIndexer Is Nothing Then
                    If QueueCount > 0 Then
                        Dim IndexingDirectives As Page

                        ' Process first item in queue
                        SyncLock Directives.SyncRoot
                            IndexingDirectives = Directives(0)
                            Directives.RemoveAt(0)
                        End SyncLock

                        CurrentIndexer = New FileSystemIndexer(Parent, IndexingDirectives)
                        CurrentIndexer.StartIndexing()
                    End If
                End If

            End Sub

            Private Sub CurrentIndexer_IndexingComplete() Handles CurrentIndexer.IndexingComplete

                CurrentIndexer = Nothing

                If QueueCount > 0 Then
                    StartNextIndexer()
                Else
                    Parent.RaiseIndexingComplete()
                End If

            End Sub

        End Class

        Public Event IndexPage(ByVal NewPage As Page)
        Public Event IndexingComplete() Implements ICrawler.IndexingComplete
        Public Event CrawlerError(ByVal URL As String, ByVal [Step] As String, ByVal [Error] As String) Implements ICrawler.CrawlerError
        Public Event WebRequestError(ByVal SourceURL As String, ByVal OffendingURL As String, ByVal [Error] As String)

        ' This event is implemented on behalf of the ICrawler interface, but is hidden from the editor
        ' to prevent end user confusion with the IndexPage event which perfroms the same function...
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Event IndexDocument(ByVal Document As Object) Implements ICrawler.IndexDocument

        ' We allow end user to restrict URL following based on their own criteria...
        Public Delegate Function FollowURLSignature(ByVal URL As String) As Boolean

        ' We allow end user to override how we keep track of whether or not a page has been crawled...
        Public Delegate Function PageHasBeenIndexedSignature(ByVal URL As String) As Boolean

        ' Web crawler only automatically handles HTML (dynamic or static) - other document types can also be handled, but user
        ' must provide implementation.  For example, web served .DOC files would return a content-type of "application/msword",
        ' which the web crawler would not automatically parse - so the user could use the following event to save the stream to a
        ' temporary file, create a MS Word object and convert the file to HTML, returning the HTML...
        Public Delegate Function UserHTMLExtractionSignature(ByVal URL As String, ByVal ContentType As String, ByVal ResponseStream As Stream) As String

        ' When indexing files from a file-system perspective, crawler still only handles HTML (based on HTML/Text file extensions property)
        ' but again, if user recognizes file type and can convert it to HTML - crawler will index it...
        Public Delegate Function UserHTMLConversionSignature(ByVal FileName As String) As String

        Private FollowURL As FollowURLSignature                     ' User overridable "follow url" function
        Private PageHasBeenIndexed As PageHasBeenIndexedSignature   ' User overridable "page has been indexed" function
        Private UserHTMLExtraction As UserHTMLExtractionSignature   ' User definable extract html from data stream function
        Private UserHTMLConversion As UserHTMLConversionSignature   ' User definable convert file to html function
        Private IndexedURLs As ArrayList                            ' Indexed URL list (used if "page has been indexed" function is not overridden)
        Private IndexedPages As Long                                ' Total indexed page count
        Private Schemes As ArrayList                                ' Valid URI scheme list
        Private HTMLFileExtensions As ArrayList                     ' HTML file type extension list
        Private URLQueue As ArrayList                               ' Queue of URL's to be processes
        Private WithEvents QueueTimer As Timers.Timer               ' URL queue processing timer
        Private Processing As Boolean                               ' Determines if crawler is active
        Private StartTime As Long                                   ' Start timer value of indexing run
        Private StopTime As Long                                    ' Stop timer value of indexing run
        Private FileSystemIndexRequests As FileSystemIndexingQueue  ' File system indexing queue
        Private Shared CachedFilePatterns As New Hashtable          ' Cached file match patterns

        ' Exposed WebCrawler property values
        Private CrawlerDirectiveName As String
        Private CrawlerStartPage As String
        Private CrawlerValidSchemes As String
        Private CrawlerMaximumThreads As Integer
        Private CrawlerActiveThreads As Integer
        Private CrawlerPageIndexTimeout As Integer
        Private CrawlerIgnorePageDirectives As Boolean
        Private CrawlerIndexMeDefault As Boolean
        Private CrawlerFollowLinksDefault As Boolean
        Private CrawlerFileInclusionDefault As String
        Private CrawlerFileExclusionDefault As String
        Private CrawlerDirectoryRecurseDefault As Boolean
        Private CrawlerDirectoryInclusionDefault As String
        Private CrawlerDirectoryExclusionDefault As String
        Private CrawlerFileSystemIndexingAllowed As Boolean
        Private CrawlerHTMLFileTypes As String
        Private CrawlerEnabled As Boolean
        Private CrawlerEncoding As Encoding

        Private Const BufferSize As Integer = 4096 ' 4K

        Public Sub New()

            UrlIndexer.Parent = Me
            UserHTMLExtraction = AddressOf DefaultUserHTMLExtraction
            UserHTMLConversion = AddressOf DefaultUserHTMLConversion
            FollowURL = AddressOf DefaultFollowURL
            PageHasBeenIndexed = AddressOf DefaultPageHasBeenIndexed
            IndexedURLs = New ArrayList
            URLQueue = New ArrayList
            QueueTimer = New Timers.Timer
            ValidSchemes = "http;https"
            HTMLFileTypes = ".HTML;.HTM;.TXT"
            FileSystemIndexRequests = New FileSystemIndexingQueue(Me)
            CrawlerDirectiveName = "INDEX"
            CrawlerStartPage = "http://www.msn.com/"
            CrawlerMaximumThreads = 5
            CrawlerPageIndexTimeout = 10
            CrawlerIgnorePageDirectives = False
            CrawlerIndexMeDefault = True
            CrawlerFollowLinksDefault = True
            CrawlerFileInclusionDefault = "*"
            CrawlerFileExclusionDefault = "*.TMP;*.BAK;*.LCK"
            CrawlerDirectoryRecurseDefault = True
            CrawlerDirectoryInclusionDefault = "*"
            CrawlerDirectoryExclusionDefault = "_vti*"
            CrawlerFileSystemIndexingAllowed = True
            CrawlerEnabled = True
            CrawlerEncoding = Encoding.UTF8

            With QueueTimer
                .Interval = 100
                .AutoReset = False
                .Enabled = False
            End With

        End Sub

        Protected Overrides Sub Finalize()

            Shutdown()

        End Sub

        Public Sub Shutdown() Implements Services.IServiceComponent.Dispose, ICrawler.Shutdown

            MyBase.Dispose()
            StopIndexing()
            GC.SuppressFinalize(Me)

        End Sub

        <Browsable(True), Category("Configuration"), Description("Value of the ""NAME"" attribute in META tag used to define crawling directives.  For example, the directive name in the following meta tag <META NAME=""MyCrawler"" INDEXME=""False""> is ""MyCrawler""."), DefaultValue("INDEX")> _
        Public Property DirectiveName() As String
            Get
                Return CrawlerDirectiveName
            End Get
            Set(ByVal Value As String)
                CrawlerDirectiveName = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Fully qualified URL of start page for web crawler."), DefaultValue("http://www.msn.com/")> _
        Public Property StartPage() As String Implements ICrawler.StartPath
            Get
                Return CrawlerStartPage
            End Get
            Set(ByVal Value As String)
                CrawlerStartPage = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Semi-colon separated list of valid URI schemes.  Crawler only supports HTTP protocol."), DefaultValue("http;https")> _
        Public Property ValidSchemes() As String
            Get
                Return CrawlerValidSchemes
            End Get
            Set(ByVal Value As String)
                CrawlerValidSchemes = Value
                If Not DesignMode Then
                    Schemes = New ArrayList
                    For Each Str As String In Value.Split(";")
                        Str = Trim(Str)
                        If Len(Str) > 0 Then
                            Schemes.Add(Str)
                        End If
                    Next
                    Schemes.Sort()
                End If
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Define the maximum number of simultaneous indexing threads to process."), DefaultValue(5)> _
        Public Property MaximumThreads() As Integer
            Get
                Return CrawlerMaximumThreads
            End Get
            Set(ByVal Value As Integer)
                CrawlerMaximumThreads = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ActiveThreads() As Integer
            Get
                Return CrawlerActiveThreads
            End Get
        End Property

        <Browsable(True), Category("Configuration"), Description("Define the maximum number of seconds allowed for a page to be indexed before aborting."), DefaultValue(10)> _
        Public Property PageIndexTimeout() As Integer
            Get
                Return CrawlerPageIndexTimeout
            End Get
            Set(ByVal Value As Integer)
                CrawlerPageIndexTimeout = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set to True to ignore page level meta-tag directives and only use default crawler settings."), DefaultValue(False)> _
        Public Property IgnorePageDirectives() As Boolean
            Get
                Return CrawlerIgnorePageDirectives
            End Get
            Set(ByVal Value As Boolean)
                CrawlerIgnorePageDirectives = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Determines if the web crawler is enabled."), DefaultValue(True)> _
        Public Property Enabled() As Boolean Implements ICrawler.Enabled
            Get
                Return CrawlerEnabled
            End Get
            Set(ByVal Value As Boolean)
                CrawlerEnabled = Value

                If Not DesignMode Then
                    ' Resume crawling, if needed
                    QueueTimer.Enabled = (Processing And URLQueueCount > 0)
                End If
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property HTMLEncoding() As Encoding
            Get
                Return CrawlerEncoding
            End Get
            Set(ByVal Value As Encoding)
                CrawlerEncoding = Value
            End Set
        End Property

        <Browsable(True), Category("File System Indexing"), Description("Set to False to disallow any file system indexing when any file system indexing directives are encountered."), DefaultValue(True)> _
        Public Property FileSystemIndexingAllowed() As Boolean
            Get
                Return CrawlerFileSystemIndexingAllowed
            End Get
            Set(ByVal Value As Boolean)
                CrawlerFileSystemIndexingAllowed = Value
            End Set
        End Property

        <Browsable(True), Category("File System Indexing"), Description("Semi-colon separated file extensions (including dot) that are known to be HTML or plain-text."), DefaultValue(".HTML;.HTM;.TXT")> _
        Public Property HTMLFileTypes() As String
            Get
                Return CrawlerHTMLFileTypes
            End Get
            Set(ByVal Value As String)
                CrawlerHTMLFileTypes = Value
                If Not DesignMode Then
                    HTMLFileExtensions = New ArrayList
                    For Each Str As String In Value.Split(";")
                        Str = Trim(Str)
                        If Len(Str) > 0 Then
                            HTMLFileExtensions.Add(Str)
                        End If
                    Next
                    HTMLFileExtensions.Sort()
                End If
            End Set
        End Property

        <Browsable(True), Category("Directive Defaults"), Description("Default setting to use when INDEXME directive is not found."), DefaultValue(True)> _
        Public Property IndexMeDefault() As Boolean
            Get
                Return CrawlerIndexMeDefault
            End Get
            Set(ByVal Value As Boolean)
                CrawlerIndexMeDefault = Value
            End Set
        End Property

        <Browsable(True), Category("Directive Defaults"), Description("Default setting to use when FOLLOWLINKS directive is not found."), DefaultValue(True)> _
        Public Property FollowLinksDefault() As Boolean
            Get
                Return CrawlerFollowLinksDefault
            End Get
            Set(ByVal Value As Boolean)
                CrawlerFollowLinksDefault = Value
            End Set
        End Property

        <Browsable(True), Category("Directive Defaults"), Description("Default setting to use when FILEINCLUSION directive is not found."), DefaultValue("*")> _
        Public Property FileInclusionDefault() As String
            Get
                Return CrawlerFileInclusionDefault
            End Get
            Set(ByVal Value As String)
                CrawlerFileInclusionDefault = Value
            End Set
        End Property

        <Browsable(True), Category("Directive Defaults"), Description("Default setting to use when FILEEXCLUSION directive is not found."), DefaultValue("*.TMP;*.BAK;*.LCK")> _
        Public Property FileExclusionDefault() As String
            Get
                Return CrawlerFileExclusionDefault
            End Get
            Set(ByVal Value As String)
                CrawlerFileExclusionDefault = Value
            End Set
        End Property

        <Browsable(True), Category("Directive Defaults"), Description("Default setting to use when DIRRECURSE directive is not found."), DefaultValue(True)> _
        Public Property DirectoryRecurseDefault() As Boolean
            Get
                Return CrawlerDirectoryRecurseDefault
            End Get
            Set(ByVal Value As Boolean)
                CrawlerDirectoryRecurseDefault = Value
            End Set
        End Property

        <Browsable(True), Category("Directive Defaults"), Description("Default setting to use when DIRINCLUSION directive is not found."), DefaultValue("*")> _
        Public Property DirectoryInclusionDefault() As String
            Get
                Return CrawlerDirectoryInclusionDefault
            End Get
            Set(ByVal Value As String)
                CrawlerDirectoryInclusionDefault = Value
            End Set
        End Property

        <Browsable(True), Category("Directive Defaults"), Description("Default setting to use when DIREXCLUSION directive is not found."), DefaultValue("_vti*")> _
        Public Property DirectoryExclusionDefault() As String
            Get
                Return CrawlerDirectoryExclusionDefault
            End Get
            Set(ByVal Value As String)
                CrawlerDirectoryExclusionDefault = Value
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property FollowURLFunction() As FollowURLSignature
            Get
                Return FollowURL
            End Get
            Set(ByVal Value As FollowURLSignature)
                If Value Is Nothing Then
                    ' This function should never be null...
                    FollowURL = AddressOf DefaultFollowURL
                Else
                    FollowURL = Value
                End If
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property PageHasBeenIndexedFunction() As PageHasBeenIndexedSignature
            Get
                Return PageHasBeenIndexed
            End Get
            Set(ByVal Value As PageHasBeenIndexedSignature)
                If Value Is Nothing Then
                    ' This function should never be null
                    PageHasBeenIndexed = AddressOf DefaultPageHasBeenIndexed
                    IndexedURLs = New ArrayList
                Else
                    PageHasBeenIndexed = Value
                    IndexedURLs = Nothing
                End If
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property UserHTMLExtractionFunction() As UserHTMLExtractionSignature
            Get
                Return UserHTMLExtraction
            End Get
            Set(ByVal Value As UserHTMLExtractionSignature)
                If Value Is Nothing Then
                    ' This function should never be null
                    UserHTMLExtraction = AddressOf DefaultUserHTMLExtraction
                Else
                    UserHTMLExtraction = Value
                End If
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property UserHTMLConversionFunction() As UserHTMLConversionSignature
            Get
                Return UserHTMLConversion
            End Get
            Set(ByVal Value As UserHTMLConversionSignature)
                If Value Is Nothing Then
                    ' This function should never be null
                    UserHTMLConversion = AddressOf DefaultUserHTMLConversion
                Else
                    UserHTMLConversion = Value
                End If
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property TotalIndexedPages() As Long Implements ICrawler.TotalIndexedDocuments
            Get
                Return IndexedPages
            End Get
        End Property

        Private Class URLIndexer

            Inherits ThreadBase

            Public Shared Parent As WebCrawler
            Public SourceURL As String
            Public URL As String

            Public Sub New(ByVal SourceURL As String, ByVal URL As String)

                Me.SourceURL = SourceURL
                Me.URL = Parent.ScrubbedURL(SourceURL, URL)

            End Sub

            Protected Overrides Sub ThreadProc()
                Try
                    If Len(URL) > 0 Then
                        ' Index hyper-link if hasn't been done already
                        If Not Parent.PageHasBeenIndexed(URL) Then
                            ' Get parsed HTML document and attributes from URL
                            Dim NewPage As Page = Parent.GetResponse(SourceURL, URL)

                            If IIf(Parent.IgnorePageDirectives, True, NewPage.IndexMe) Then
                                ' If end-user didn't override "PageHasBeenIndexed" functionality, then we just use an array list
                                ' to keep up with whether or not a page has been indexed.  It should be well noted that this
                                ' is *only* OK for "small" crawls - indexing large sites requires a database
                                If Not Parent.IndexedURLs Is Nothing Then
                                    ' Add this URL to the "indexed" list - this is fast, but not scalable - end-user should
                                    ' override this functionality for large unrestricted crawls...
                                    With Parent.IndexedURLs
                                        SyncLock .SyncRoot
                                            .Add(URL)
                                            .Sort()
                                        End SyncLock
                                    End With
                                End If

                                ' Provide page to user for indexing (or whatever they're crawling pages for...)
                                ' Note: end-user has to keep track of whether or not page has been indexed if they have
                                ' overridden default "PageHasBeenIndexed" function
                                Parent.IndexedPages += 1
                                Parent.RaiseIndexPage(NewPage)

                                ' Handle indexing of any "AlsoIndex" page directives
                                For Each alsoIndexURL As String In NewPage.AlsoIndex
                                    If Len(alsoIndexURL) > 0 Then Parent.AddURLToIndex(URL, alsoIndexURL)
                                Next

                                ' Check to see if we need to crawl file system based on any found page directives...
                                If Not Parent.IgnorePageDirectives And Parent.FileSystemIndexingAllowed Then
                                    ' We only crawl file system when both the file system root and file web root directives have been specified
                                    If Len(NewPage.FileSystemRoot) > 0 And Len(NewPage.FileWebRoot) > 0 Then
                                        Parent.FileSystemIndexRequests.QueueIndexRequest(NewPage)
                                    End If
                                End If

                                If IIf(Parent.IgnorePageDirectives, Parent.FollowLinksDefault, NewPage.FollowLinks) Then
                                    ' Recurse through any frames on this page
                                    For Each frameSourceURL As String In HTMLFrameSourceExtractor.FrameSourceList(NewPage.ParsedHTMLDocument)
                                        If Len(frameSourceURL) > 0 Then Parent.AddURLToIndex(URL, frameSourceURL)
                                    Next

                                    ' Recurse through any links on this page
                                    For Each anchorURL As String In HTMLAnchorExtractor.AnchorList(NewPage.ParsedHTMLDocument)
                                        If Len(anchorURL) > 0 Then Parent.AddURLToIndex(URL, anchorURL)
                                    Next
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception
                    Parent.RaiseCrawlerError(URL, "Indexing URL", "Failed to index URL due to exception: " & ex.Message)
                End Try
            End Sub

        End Class

        Private Sub RaiseCrawlerError(ByVal URL As String, ByVal [Step] As String, ByVal [Error] As String)

            Try
                RaiseEvent CrawlerError(URL, [Step], [Error])
            Catch
                ' Not stopping for end-user event handling code error's when we're already reporting an exception of our own
            End Try

        End Sub

        Private Sub RaiseIndexingComplete()

            If Not IsActive Then
                StopIndexing()
                RaiseEvent IndexingComplete()
            End If

        End Sub

        Private Sub RaiseIndexPage(ByVal NewPage As Page)

            Try
                RaiseEvent IndexPage(NewPage)
                RaiseEvent IndexDocument(NewPage)
            Catch ex As Exception
                ' Not stopping for end-user event handling code errors, but we'll let them know something's broke...
                RaiseCrawlerError(NewPage.URL, "End User ""IndexPage"" Event Handler", ex.Message)
            End Try

        End Sub

        Public Sub AddURLToIndex(ByVal URL As String)

            SyncLock URLQueue.SyncRoot
                URLQueue.Add(New URLIndexer("", URL))
            End SyncLock
            QueueTimer.Enabled = True

        End Sub

        Private Sub AddURLToIndex(ByVal SourceURL As String, ByVal URL As String)

            SyncLock URLQueue.SyncRoot
                URLQueue.Add(New URLIndexer(SourceURL, URL))
            End SyncLock
            QueueTimer.Enabled = True

        End Sub

        Private ReadOnly Property URLQueueCount() As Integer
            Get
                SyncLock URLQueue.SyncRoot
                    Return URLQueue.Count
                End SyncLock
            End Get
        End Property

        Public Sub StartIndexing() Implements ICrawler.StartIndexing

            If Len(CrawlerStartPage) = 0 And URLQueueCount = 0 Then Throw New NullReferenceException("Cannot start indexing - no start page was specified for crawler.  Check ""StartPage"" property.")
            If Not CrawlerEnabled Then Throw New InvalidOperationException("Cannot start indexing - web crawler is not enabled.  Check ""Enabled"" property.")
            If IsActive Then Throw New InvalidOperationException("Indexing already in progress.")

            AddURLToIndex(CrawlerStartPage)

            IndexedPages = 0
            Processing = True
            StopTime = 0
            StartTime = Now.Ticks

        End Sub

        Public Sub StopIndexing() Implements ICrawler.StopIndexing

            Processing = False
            StopTime = Now.Ticks

        End Sub

        Private Sub QueueTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles QueueTimer.Elapsed

            Dim indexer As URLIndexer

            If CrawlerActiveThreads < CrawlerMaximumThreads Then
                Try
                    Interlocked.Increment(CrawlerActiveThreads)

                    SyncLock URLQueue.SyncRoot
                        If URLQueue.Count > 0 Then
                            indexer = URLQueue(0)
                            URLQueue.RemoveAt(0)
                        End If
                    End SyncLock

                    If Not indexer Is Nothing Then
                        With indexer
                            .Start()
                            If Not .Thread.Join(CrawlerPageIndexTimeout * 1000) Then .Abort()
                        End With
                    End If
                Catch
                    Throw
                Finally
                    Interlocked.Decrement(CrawlerActiveThreads)
                End Try
            End If

            If URLQueueCount > 0 Then
                ' Keep queue timer alive so long as there are URL's to index...
                QueueTimer.Enabled = (Enabled And Processing And URLQueueCount > 0)
            Else
                ' We've finished crawling if no URL's are in the queue and no threads are active
                If CrawlerActiveThreads <= 0 Then
                    RaiseIndexingComplete()
                Else
                    QueueTimer.Enabled = (Enabled And Processing)
                End If
            End If

        End Sub

        Public ReadOnly Property RunTime() As Double Implements ICrawler.RunTime
            Get
                Dim ProcessingTime As Long

                If StartTime > 0 Then
                    If StopTime > 0 Then
                        ProcessingTime = StopTime - StartTime
                    Else
                        ProcessingTime = Now.Ticks - StartTime
                    End If
                End If

                If ProcessingTime < 0 Then ProcessingTime = 0

                Return ProcessingTime / 10000000L
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsActive() As Boolean Implements ICrawler.IsActive
            Get
                Return (CrawlerEnabled And (Processing Or FileSystemIndexRequests.IsActive))
            End Get
        End Property

        ' Return parsed HTML document and attributes based on content-type of web response
        Private Function GetResponse(ByVal SourceURL As String, ByVal URL As String) As Page

            Dim NewPage As New Page(Me, SourceURL, URL)
            Dim Source As String

            Try
                ' Create new web request and append return entire response to a string
                With CType(WebRequest.Create(URL), HttpWebRequest)
                    ' Use current login credentials for web page access...
                    .Credentials = CredentialCache.DefaultCredentials

                    With .GetResponse
                        NewPage.ContentType = .ContentType.Split(";"c)(0).ToLower.Trim
                        NewPage.ContentLength = .ContentLength

                        Select Case NewPage.ContentType
                            Case "text/html"
                                Source = GetHTML(.GetResponseStream)
                            Case Else
                                ' Other content-types are not automatically handled, but we provide data to user
                                ' if they have a way of converting data type to HTML...
                                Source = UserHTMLExtraction(URL, NewPage.ContentType, .GetResponseStream)
                        End Select

                        .Close()
                    End With
                End With
            Catch ex As Exception
                RaiseEvent WebRequestError(SourceURL, URL, ex.Message)
            End Try

            ' We'll at least put a little something in the source to make the parser happy
            If Len(Source) = 0 Then Source = "<HTML></HTML>"

            ' Give HTML to new page object and let it begin it's parsing
            NewPage.SourceHTML = Source

            Return NewPage

        End Function

        ' Read HTML from stream
        Private Function GetHTML(ByVal Source As IO.Stream) As String

            Dim Response As New StringBuilder
            Dim Buffer As Byte() = System.Array.CreateInstance(GetType(Byte), BufferSize)
            Dim Html As Char()
            Dim BytesRead As Integer

            With Source
                BytesRead = .Read(Buffer, 0, BufferSize)
                Do While BytesRead > 0
                    Html = CrawlerEncoding.GetChars(Buffer, 0, BytesRead)
                    Response.Append(Html, 0, Html.Length)
                    BytesRead = .Read(Buffer, 0, BufferSize)
                Loop
            End With

            Return Response.ToString()

        End Function

        ' URL scrubber for those nasty URL's
        Private Function ScrubbedURL(ByVal SourceURL As String, ByVal URL As String) As String

            Try
                URL = Trim(URL)

                If Len(URL) > 0 Then
                    Dim chrQuote As Char

                    ' Remove any out-of-place prefixing or suffixing quotes
                    chrQuote = URL.Chars(0)

                    Do While chrQuote = "'"c Or chrQuote = """"c
                        URL = Mid(URL, 2)
                        chrQuote = URL.Chars(0)
                    Loop

                    chrQuote = URL.Chars(URL.Length - 1)

                    Do While chrQuote = "'"c Or chrQuote = """"c
                        URL = Left(URL, URL.Length - 1)
                        chrQuote = URL.Chars(URL.Length - 1)
                    Loop

                    Try
                        If Len(URL) > 0 Then
                            ' Make sure URL is a supported scheme (i.e., ignore mailto:, javascript:, etc...)
                            Dim Colon As Integer = InStr(URL, ":")

                            If Colon > 0 Then
                                If Schemes.BinarySearch(Left(URL, Colon - 1), CaseInsensitiveComparer.Default) < 0 Then
                                    URL = ""
                                End If
                            End If

                            If Len(URL) > 0 Then
                                ' If path is not fully qualified, we must assume it's a relative path...
                                If URL.IndexOf("://") < 0 And Len(SourceURL) > 0 Then
                                    Dim SourceURI As New Uri(SourceURL)

                                    If URL.StartsWith("/") Then
                                        ' Relative server only
                                        URL = GetBaseURI(SourceURI) & URL
                                    Else
                                        ' Relative server and path
                                        URL = GetBaseURI(SourceURI) & GetSourcePath(SourceURI) & URL
                                    End If
                                End If

                                ' Finally we make sure to get a standard DNS host name for all of our URL's:
                                With New Uri(URL)
                                    URL = .Scheme & "://" & Dns.Resolve(.Host).HostName & IIf(.IsDefaultPort, "", ":" & .Port) & .PathAndQuery
                                End With
                            End If
                        End If
                    Catch
                        ' If we failed to adjust URL or resolve DNS host name, we stick with what we have
                    End Try
                End If
            Catch ex As Exception
                ' If we're having trouble with the URL, we'll skip it...
                URL = ""
            End Try

            ' We allow end user to restrict URL following based on their own criteria...
            If Len(URL) > 0 AndAlso Not FollowURL(URL) Then URL = ""

            Return URL

        End Function

        Private Function GetBaseURI(ByVal SourceURI As Uri) As String

            With SourceURI
                Return .Scheme & "://" & .Host & IIf(.IsDefaultPort, "", ":" & .Port)
            End With

        End Function

        Private Function GetSourcePath(ByVal SourceURI As Uri) As String

            With SourceURI
                Return JustPath(.AbsolutePath).Replace("\", "/")
            End With

        End Function

        Private Function DefaultFollowURL(ByVal URL As String) As Boolean

            Return CrawlerFollowLinksDefault

        End Function

        Private Function DefaultPageHasBeenIndexed(ByVal URL As String) As Boolean

            If IndexedURLs Is Nothing Then
                Return True
            Else
                SyncLock IndexedURLs.SyncRoot
                    Return IndexedURLs.BinarySearch(URL, CaseInsensitiveComparer.Default) < 0
                End SyncLock
            End If

        End Function

        Private Function DefaultUserHTMLExtraction(ByVal URL As String, ByVal ContentType As String, ByVal ResponseStream As Stream) As String

            Return ""

        End Function

        Private Function DefaultUserHTMLConversion(ByVal FileName As String) As String

            Return ""

        End Function

        <Browsable(False)> _
        Public ReadOnly Property Name() As String Implements TVA.Services.IServiceComponent.Name, ICrawler.Name
            Get
                Return Me.GetType.Name
            End Get
        End Property

        Public Sub ProcessStateChanged(ByVal NewState As TVA.Services.ProcessState) Implements TVA.Services.IServiceComponent.ProcessStateChanged

            ' WebCrawler, when used as a service component, doesn't need to respond to changes in process state

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
                Dim strStatus As New StringBuilder
                Dim dblRunTime As Double

                strStatus.Append("Current crawler state: " & IIf(IsActive, "Indexing", "Idle") & vbCrLf)
                strStatus.Append("  Total indexed pages: " & IndexedPages & vbCrLf)
                strStatus.Append("     Total crawl time: " & SecondsToText(RunTime).ToLower() & vbCrLf)
                strStatus.Append("      Maximum threads: " & CrawlerMaximumThreads & vbCrLf)
                strStatus.Append("       Active threads: " & CrawlerActiveThreads & vbCrLf)
                strStatus.Append("      URL queue count: " & URLQueueCount & vbCrLf)
                strStatus.Append(" File system indexing: " & IIf(FileSystemIndexRequests.IsActive, "Indexing: " & FileSystemIndexRequests.QueueCount & " queued", "Idle") & vbCrLf)

                Return strStatus.ToString()
            End Get
        End Property

    End Class

End Namespace