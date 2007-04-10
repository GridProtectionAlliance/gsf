' PCP: 04-09-2007

Option Strict On

Imports System.IO
Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports TVA.Collections
Imports TVA.IO.FilePath

Namespace IO

    Public Enum LogFileFullOperation As Integer
        Scroll
        Rollover
    End Enum

    <ToolboxBitmap(GetType(LogFile))> _
    Public Class LogFile
        Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

        Private m_name As String
        Private m_size As Integer
        Private m_autoOpen As Boolean
        Private m_fileFullOperation As LogFileFullOperation
        Private m_persistSettings As Boolean
        Private m_configurationCategory As String
        Private m_fileStream As FileStream

        Private WithEvents m_logEntryQueue As ProcessQueue(Of String)

#End Region

#Region " Event Declaration "

        Public Event FileOpening As EventHandler
        Public Event FileOpened As EventHandler
        Public Event FileClosing As EventHandler
        Public Event FileClosed As EventHandler
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

        Public Property AutoOpen() As Boolean
            Get
                Return m_autoOpen
            End Get
            Set(ByVal value As Boolean)
                m_autoOpen = value
            End Set
        End Property

        Public Property FileFullOperation() As LogFileFullOperation
            Get
                Return m_fileFullOperation
            End Get
            Set(ByVal value As LogFileFullOperation)
                m_fileFullOperation = value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsOpen() As Boolean
            Get
                Return m_fileStream IsNot Nothing
            End Get
        End Property

        Public Sub Open()

            If Not IsOpen Then
                RaiseEvent FileOpening(Me, EventArgs.Empty)

                ' Get the absolute file path if a relative path is specified.
                m_name = AbsolutePath(m_name)
                ' Create the folder in which the log file will reside it if it doesn't exist.
                If Not Directory.Exists(JustPath(m_name)) Then Directory.CreateDirectory(JustPath(m_name))
                ' Open the log file if it exists, or create it if it doesn't.
                m_fileStream = New FileStream(m_name, FileMode.OpenOrCreate)

                ' Start the queue to which log entries are going to be added.
                m_logEntryQueue.Start()

                RaiseEvent FileClosed(Me, EventArgs.Empty)
            End If

        End Sub

        Public Sub Close()

            Close(True)

        End Sub

        Public Sub Close(ByVal flushQueuedEntries As Boolean)

            If IsOpen Then
                RaiseEvent FileClosing(Me, EventArgs.Empty)

                If flushQueuedEntries Then
                    ' Write all queued log entries to the file.
                    m_logEntryQueue.Flush()
                Else
                    ' Stop processing the queued log entries.
                    m_logEntryQueue.Stop()
                End If

                ' Close the log file.
                m_fileStream.Dispose()
                m_fileStream = Nothing
            End If

        End Sub

        Public Sub Write(ByVal text As String)

            If IsOpen Then
                ' Queue the text for writting to the log file.
                m_logEntryQueue.Add(text)
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Sub WriteLine(ByVal text As String)

            Write(text & Environment.NewLine)

        End Sub

        Public Sub WriteTimestampedLine(ByVal text As String)

            Write("[" & System.DateTime.Now.ToString() & "] " & text & Environment.NewLine)

        End Sub

        Public Function Read() As List(Of String)

            If IsOpen Then
                Dim buffer As Byte() = Nothing
                SyncLock m_fileStream
                    buffer = TVA.Common.CreateArray(Of Byte)(Convert.ToInt32(m_fileStream.Length))
                    m_fileStream.Seek(0, SeekOrigin.Begin)
                    m_fileStream.Read(buffer, 0, buffer.Length)
                End SyncLock

                Return New List(Of String)(Encoding.Default.GetString(buffer).Split(New String() {Environment.NewLine}, StringSplitOptions.None))
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Function

#Region " Interface Implementation "

#Region " IPersistSettings "

        Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
            Get
                Return m_persistSettings
            End Get
            Set(ByVal value As Boolean)
                m_persistSettings = value
            End Set
        End Property

        Public Property ConfigurationCategory() As String Implements IPersistSettings.ConfigurationCategory
            Get
                Return m_configurationCategory
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_configurationCategory = value
                Else
                    Throw New ArgumentNullException("ConfigurationCategory")
                End If
            End Set
        End Property

        Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_configurationCategory)
                        Name = .Item("Name").GetTypedValue(m_name)
                        Size = .Item("Size").GetTypedValue(m_size)
                        AutoOpen = .Item("AutoOpen").GetTypedValue(m_autoOpen)
                        FileFullOperation = .Item("FileFullOperation").GetTypedValue(m_fileFullOperation)
                    End With
                Catch ex As Exception
                    ' Most likely we'll never encounter an exception here.
                End Try
            End If

        End Sub

        Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_configurationCategory)
                        .Clear()
                        With .Item("Name", True)
                            .Value = m_name
                            .Description = "Name of the log file including its path."
                        End With
                        With .Item("Size", True)
                            .Value = m_size.ToString()
                            .Description = "Maximum size of the log file in MB."
                        End With
                        With .Item("AutoOpen", True)
                            .Value = m_autoOpen.ToString()
                            .Description = "True if the log file is to be open automatically when the component is initialize; otherwise False."
                        End With
                        With .Item("FileFullOperation", True)
                            .Value = m_fileFullOperation.ToString()
                            .Description = "Operation (Scroll; Rollover) that is to be performed on the file when it is full."
                        End With
                    End With
                    TVA.Configuration.Common.SaveSettings()
                Catch ex As Exception
                    ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
                End Try
            End If

        End Sub

#End Region

#Region " ISupportInitialize "

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

            ' We don't need to do anything before the component is initialized.

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            If Not DesignMode Then
                LoadSettings()            ' Load settings from the config file.
                If m_autoOpen Then Open() ' Open the file automatically if specified.
            End If

        End Sub

#End Region

#End Region

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

                        Select Case m_fileFullOperation
                            Case LogFileFullOperation.Scroll
                                Dim entries As List(Of String) = Read()
                                For k As Integer = entries.Count - 1 To entries.Count - items.Length - 1 Step -1
                                    m_logEntryQueue.Insert(0, entries(k) & Environment.NewLine)
                                Next

                                Close(False)
                                File.Delete(m_name)
                                Open()
                            Case LogFileFullOperation.Rollover
                                Dim historyFileName As String = JustPath(m_name) & NoFileExtension(m_name) & "_" & _
                                                                File.GetCreationTime(m_name).ToString("yyyy-MM-dd hh!mm!ss") & "_to_" & _
                                                                File.GetLastWriteTime(m_name).ToString("yyyy-MM-dd hh!mm!ss") & JustFileExtension(m_name)

                                Close(False)
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