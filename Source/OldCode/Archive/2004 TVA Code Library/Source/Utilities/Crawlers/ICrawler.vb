' James Ritchie Carroll - 2004
Option Explicit On 

Namespace Utilities.Crawlers

    ' Define an interface of common elements for a document system crawler, currently implemented by:
    '
    '   WebCrawler
    '   FTPCrawler
    '   FileSystemCrawler

    Public Interface ICrawler

        Event IndexDocument(ByVal Document As Object)   ' Example doc types: WebCrawler.Page, FTP.File or a file name (String)
        Event IndexingComplete()
        Event CrawlerError(ByVal DocumentName As String, ByVal [Step] As String, ByVal [Error] As String)

        Property StartPath() As String
        Property Enabled() As Boolean

        ReadOnly Property TotalIndexedDocuments() As Long
        ReadOnly Property RunTime() As Double
        ReadOnly Property IsActive() As Boolean
        ReadOnly Property Name() As String
        ReadOnly Property Status() As String

        Sub StartIndexing()
        Sub StopIndexing()
        Sub Shutdown()

    End Interface

End Namespace
