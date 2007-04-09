' PCP: 04-09-2007

Imports System.IO
Imports System.Text
Imports System.Drawing
Imports TVA.Collections
Imports TVA.IO.FilePath

Namespace IO

    <ToolboxBitmap(GetType(LogFile))> _
    Public Class LogFile

#Region " Member Declaration "

        Private m_name As String
        Private m_maximumSize As Integer
        Private m_rolloverOnFull As Boolean
        Private m_fileStream As FileStream

        Private WithEvents m_logEntryQueue As ProcessQueue(Of String)

#End Region

#Region " Event Declaration "


#End Region

#Region " Code Scope: Public "

        Public Const DefaultExtension As String = ".txt"

        Public Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_name = value
                Else
                    Throw New ArgumentNullException("Name")
                End If
            End Set
        End Property

        Public Property MaximumSize() As Integer
            Get
                Return m_maximumSize
            End Get
            Set(ByVal value As Integer)
                m_maximumSize = value
            End Set
        End Property

        Public Property RolloverOnFull() As Boolean
            Get
                Return m_rolloverOnFull
            End Get
            Set(ByVal value As Boolean)
                m_rolloverOnFull = value
            End Set
        End Property

        Public ReadOnly Property IsOpen() As Boolean
            Get
                Return m_fileStream IsNot Nothing
            End Get
        End Property

        Public Sub Open()

            ' Get the absolute file path if a relative path is specified.
            m_name = AbsolutePath(m_name)
            ' Create the folder in which the log file will reside it if it doesn't exist.
            If Not Directory.Exists(JustPath(m_name)) Then Directory.CreateDirectory(JustPath(m_name))
            ' Open the log file is it exists, or create it if it doesn't.
            m_fileStream = New FileStream(m_name, FileMode.OpenOrCreate)
            ' Start the queue to which log entries are going to be added.
            m_logEntryQueue.Start()

        End Sub

        Public Sub Close()

            ' Finish writing all queued entries.
            m_logEntryQueue.Flush()
            m_logEntryQueue.Stop()
            ' Close the log file.
            m_fileStream.Dispose()
            m_fileStream = Nothing

        End Sub

        Public Sub Rollover()

            Dim fileStartTime As Date = File.GetCreationTime(m_name)
            Dim fileEndTime As Date = File.GetLastWriteTime(m_name)
            Dim historyFileName As String = JustPath(m_name) & NoFileExtension(m_name)

        End Sub

        Public Sub Write(ByVal text As String)

            ' Open the file if closed.
            If Not IsOpen Then Open()

            ' Queue the text for writting to the log file.
            m_logEntryQueue.Add(text)

        End Sub

        Public Sub WriteLine(ByVal text As String)

            Write(text & Environment.NewLine)

        End Sub

        Public Sub WriteTimestampedLine(ByVal text As String)

            Write("[" & System.DateTime.Now.ToString() & "] " & text & Environment.NewLine)

        End Sub

        Public Function Read() As String

            If Not IsOpen Then Open()

            Dim text As Byte() = Nothing
            SyncLock m_fileStream
                text = TVA.Common.CreateArray(Of Byte)(m_fileStream.Length)
                m_fileStream.Seek(0, SeekOrigin.Begin)
                m_fileStream.Read(text, 0, text.Length)
            End SyncLock

            Return Encoding.Default.GetString(text)

        End Function

#End Region

#Region " Code Scope: Private "

        Private Sub WriteLogEntries(ByVal items As String())

            SyncLock m_fileStream
                For i As Integer = 0 To items.Length - 1
                    Dim text As Byte() = Encoding.Default.GetBytes(items(i))
                    m_fileStream.Write(text, 0, text.Length)
                Next
                m_fileStream.Flush()
            End SyncLock

        End Sub

#End Region

    End Class

End Namespace