' PCP: 04-09-2007

Option Strict On

Imports System.IO
Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports TVA.Collections
Imports TVA.IO.FilePath

Namespace IO

    Public Enum LogFileFullAction As Integer
        Scroll
        Rollover
    End Enum

    <ToolboxBitmap(GetType(LogFile))> _
    Public Class LogFile

#Region " Member Declaration "

        Private m_name As String
        Private m_size As Integer
        Private m_fileFullAction As LogFileFullAction
        Private m_fileStream As FileStream

        Private WithEvents m_logEntryQueue As ProcessQueue(Of String)

#End Region

#Region " Event Declaration "

        Public Event FileFull As EventHandler
        Public Event LogException As EventHandler(Of ExceptionEventArgs)

#End Region

#Region " Code Scope: Public "

        Public Const MinimumFileSize As Integer = 1
        Public Const MaximumFileSize As Integer = 5
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

        Public Property Size() As Integer
            Get
                Return m_size
            End Get
            Set(ByVal value As Integer)
                If value >= MinimumFileSize AndAlso value <= MaximumFileSize Then
                    m_size = value
                Else
                    Throw New ArgumentOutOfRangeException("Size", String.Format("Value must be between {0} and {1}", MinimumFileSize, MaximumFileSize))
                End If
            End Set
        End Property

        Public Property FileFullAction() As LogFileFullAction
            Get
                Return m_fileFullAction
            End Get
            Set(ByVal value As LogFileFullAction)
                m_fileFullAction = value
            End Set
        End Property

        <Browsable(False)> _
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
            m_logEntryQueue.Stop()
            ' Close the log file.
            m_fileStream.Dispose()
            m_fileStream = Nothing

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

        Public Function Read() As List(Of String)

            If Not IsOpen Then Open()

            Dim buffer As Byte() = Nothing
            SyncLock m_fileStream
                buffer = TVA.Common.CreateArray(Of Byte)(Convert.ToInt32(m_fileStream.Length))
                m_fileStream.Seek(0, SeekOrigin.Begin)
                m_fileStream.Read(buffer, 0, buffer.Length)
            End SyncLock

            Return New List(Of String)(Encoding.Default.GetString(buffer).Split(New String() {Environment.NewLine}, StringSplitOptions.None))

        End Function

#End Region

#Region " Code Scope: Private "

        Private Sub WriteLogEntries(ByVal items As String())

            Dim currentFileSize As Long = 0
            Dim maximumFileSize As Long = Convert.ToInt64(m_size * (1024 ^ 2))
            SyncLock m_fileStream
                currentFileSize = m_fileStream.Length
            End SyncLock

            For i As Integer = 0 To items.Length - 1
                If Not String.IsNullOrEmpty(items(i)) Then
                    Dim buffer As Byte() = Encoding.Default.GetBytes(items(i))

                    If currentFileSize + buffer.Length > maximumFileSize Then
                        ' The log file has reached the maximum size, so we'll have to either scroll the file or rollover.
                        RaiseEvent FileFull(Me, EventArgs.Empty)

                        For j As Integer = items.Length - 1 To i Step -1
                            m_logEntryQueue.Insert(0, items(j))
                        Next

                        Select Case m_fileFullAction
                            Case LogFileFullAction.Scroll
                                Dim entries As List(Of String) = Read()
                                For k As Integer = entries.Count - 1 To entries.Count - items.Length - 1 Step -1
                                    m_logEntryQueue.Insert(0, entries(k) & Environment.NewLine)
                                Next

                                Close()
                                File.Delete(m_name)
                                Open()
                            Case LogFileFullAction.Rollover
                                Dim historyFileName As String = JustPath(m_name) & NoFileExtension(m_name) & "_" & _
                                                                File.GetCreationTime(m_name).ToString("yyyy-mm-dd hh!MM!ss") & "_to_" & _
                                                                File.GetLastWriteTime(m_name).ToString("yyyy-mm-dd hh!MM!ss") & JustFileExtension(m_name)

                                Close()
                                File.Move(m_name, historyFileName)
                                Open()
                        End Select

                        Exit For
                    End If

                    SyncLock m_fileStream
                        m_fileStream.Write(buffer, 0, buffer.Length)
                    End SyncLock
                    currentFileSize += buffer.Length
                End If
            Next

        End Sub

#End Region

    End Class

End Namespace