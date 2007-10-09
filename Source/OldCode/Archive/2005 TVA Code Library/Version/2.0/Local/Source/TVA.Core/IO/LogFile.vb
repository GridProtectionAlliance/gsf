' PCP: 04-09-2007

Option Strict On

Imports System.IO
Imports System.Text
Imports System.Drawing
Imports System.ComponentModel
Imports System.Threading
Imports TVA.Collections
Imports TVA.IO.FilePath
Imports TVA.Configuration

Namespace IO

    <ToolboxBitmap(GetType(LogFile)), DisplayName("LogFile")> _
    Public Class LogFile
        Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

        Private m_name As String
        Private m_size As Integer
        Private m_autoOpen As Boolean
        Private m_fileFullOperation As LogFileFullOperation
        Private m_persistSettings As Boolean
        Private m_settingsCategoryName As String
        Private m_fileStream As FileStream
        Private m_operationWaitHandle As ManualResetEvent

        Private WithEvents m_logEntryQueue As ProcessQueue(Of String)

#End Region

#Region " Event Declaration "

        ''' <summary>
        ''' Occurs when the log file is being opened.
        ''' </summary>
        <Description("Occurs when the log file is being opened.")> _
        Public Event FileOpening As EventHandler

        ''' <summary>
        ''' Occurs when the log file has been opened.
        ''' </summary>
        <Description("Occurs when the log file has been opened.")> _
        Public Event FileOpened As EventHandler

        ''' <summary>
        ''' Occurs when the log file is being closed.
        ''' </summary>
        <Description("Occurs when the log file is being closed.")> _
        Public Event FileClosing As EventHandler

        ''' <summary>
        ''' Occurs when the log file has been closed.
        ''' </summary>
        <Description("Occurs when the log file has been closed.")> _
        Public Event FileClosed As EventHandler

        ''' <summary>
        ''' Occurs when the log file is full.
        ''' </summary>
        <Description("Occurs when the log file is full.")> _
        Public Event FileFull As EventHandler

        ''' <summary>
        ''' Occurs when an exception is encountered while writing entries to the log file.
        ''' </summary>
        <Description("Occurs when an exception is encountered while writing entries to the log file.")> _
        Public Event LogException As EventHandler(Of GenericEventArgs(Of Exception))

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' The minimum size for a log file.
        ''' </summary>
        Public Const MinimumFileSize As Integer = 1

        ''' <summary>
        ''' The maximum size for a log file.
        ''' </summary>
        Public Const MaximumFileSize As Integer = 10

        ''' <summary>
        ''' The default extension for a log file.
        ''' </summary>
        Public Const DefaultExtension As String = ".txt"

        ''' <summary>
        ''' Gets or sets the name of the log file, including the file extension.
        ''' </summary>
        ''' <returns>The name of the log file, including the file extension.</returns>
        <Description("The name of the log file, including the file extension.")> _
        Public Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_name = value
                    If IsOpen() Then
                        Close()
                        Open()
                    End If
                Else
                    Throw New ArgumentNullException("Name")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the size of the log file in MB.
        ''' </summary>
        ''' <returns>The size of the log file in MB.</returns>
        <Description("The size of the log file in MB."), DefaultValue(GetType(Integer), "3")> _
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

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the log file is to be opened automatically after the 
        ''' component has finished initializing.
        ''' </summary>
        ''' <returns>True, if the log file is to be opened after the component has finished initializing; otherwise, 
        ''' false.</returns>
        <Description("Indicates whether the log file is to be opened automatically after the component has finished initializing."), DefaultValue(GetType(Boolean), "False")> _
        Public Property AutoOpen() As Boolean
            Get
                Return m_autoOpen
            End Get
            Set(ByVal value As Boolean)
                m_autoOpen = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the type of operation to be performed when the log file is full.
        ''' </summary>
        ''' <returns>One of the TVA.IO.LogFileFullOperation values.</returns>
        <Description("The type of operation to be performed when the log file is full."), DefaultValue(GetType(LogFileFullOperation), "Truncate")> _
        Public Property FileFullOperation() As LogFileFullOperation
            Get
                Return m_fileFullOperation
            End Get
            Set(ByVal value As LogFileFullOperation)
                m_fileFullOperation = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a boolean value indicating whether the log file is open.
        ''' </summary>
        ''' <returns>True, if the log file is open; otherwise, false.</returns>
        <Browsable(False)> _
        Public ReadOnly Property IsOpen() As Boolean
            Get
                Return m_fileStream IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Opens the log file if it is closed.
        ''' </summary>
        Public Sub Open()

            If Not IsOpen Then
                RaiseEvent FileOpening(Me, EventArgs.Empty)

                ' Gets the absolute file path if a relative path is specified.
                m_name = AbsolutePath(m_name)
                ' Creates the folder in which the log file will reside it, if it does not exist.
                If Not Directory.Exists(JustPath(m_name)) Then Directory.CreateDirectory(JustPath(m_name))
                ' Opens the log file (if it exists) or creates it (if it does not exist).
                m_fileStream = New FileStream(m_name, FileMode.OpenOrCreate)
                ' Scrolls to the end of the file so that existing data is not overwritten.
                m_fileStream.Seek(0, SeekOrigin.End)

                ' Starts the queue to which log entries are going to be added.
                m_logEntryQueue.Start()

                RaiseEvent FileOpened(Me, EventArgs.Empty)
            End If

        End Sub

        ''' <summary>
        ''' Closes the log file if it is open.
        ''' </summary>
        Public Sub Close()

            Close(True)

        End Sub

        ''' <summary>
        ''' Closes the log file if it is open.
        ''' </summary>
        ''' <param name="flushQueuedEntries">True, if queued log entries are to be written to the log file; otherwise, 
        ''' false.</param>
        Public Sub Close(ByVal flushQueuedEntries As Boolean)

            If IsOpen Then
                RaiseEvent FileClosing(Me, EventArgs.Empty)

                If flushQueuedEntries Then
                    ' Writes all queued log entries to the file.
                    m_logEntryQueue.Flush()
                Else
                    ' Stops processing the queued log entries.
                    m_logEntryQueue.Stop()
                End If

                If m_fileStream IsNot Nothing Then
                    ' Closes the log file.
                    m_fileStream.Dispose()
                    m_fileStream = Nothing
                End If

                RaiseEvent FileClosed(Me, EventArgs.Empty)
            End If

        End Sub

        ''' <summary>
        ''' Queues the text for writing to the log file.
        ''' </summary>
        ''' <param name="text">The text to be written to the log file.</param>
        Public Sub Write(ByVal text As String)

            ' Yields to the "file full operation" to complete, if in progress.
            m_operationWaitHandle.WaitOne()

            If IsOpen Then
                ' Queues the text for writing to the log file.
                m_logEntryQueue.Add(text)
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        ''' <summary>
        ''' Queues the text for writing to the log file.
        ''' </summary>
        ''' <param name="text">The text to be written to the log file.</param>
        ''' <remarks>A "newline" character will automatically be appended to the text.</remarks>
        Public Sub WriteLine(ByVal text As String)

            Write(text & Environment.NewLine)

        End Sub

        ''' <summary>
        ''' Queues the text for writing to the log file.
        ''' </summary>
        ''' <param name="text">The text to be written to the log file.</param>
        ''' <remarks>
        ''' <para>A timestamp will automatically be preprended to the text.</para>
        ''' <para>A "newline" character will automatically be appended to the text.</para>
        ''' </remarks>
        Public Sub WriteTimestampedLine(ByVal text As String)

            Write("[" & System.DateTime.Now.ToString() & "] " & text & Environment.NewLine)

        End Sub

        ''' <summary>
        ''' Reads and returns the text from the log file.
        ''' </summary>
        ''' <returns>The text read from the log file.</returns>
        Public Function ReadText() As String

            ' Yields to the "file full operation" to complete, if in progress.
            m_operationWaitHandle.WaitOne()

            If IsOpen Then
                Dim buffer As Byte() = Nothing
                SyncLock m_fileStream
                    buffer = TVA.Common.CreateArray(Of Byte)(Convert.ToInt32(m_fileStream.Length))
                    m_fileStream.Seek(0, SeekOrigin.Begin)
                    m_fileStream.Read(buffer, 0, buffer.Length)
                End SyncLock

                Return Encoding.Default.GetString(buffer)
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Function

        ''' <summary>
        ''' Reads text from the log file and returns a list of lines created by seperating the text by the "newline" 
        ''' characters if and where present.
        ''' </summary>
        ''' <returns>A list of lines from the text read from the log file.</returns>
        Public Function ReadLines() As List(Of String)

            Return New List(Of String)(ReadText.Split(New String() {Environment.NewLine}, StringSplitOptions.None))

        End Function

#Region " Interface Implementation "

#Region " IPersistSettings "

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the component settings are to be persisted to the config 
        ''' file.
        ''' </summary>
        ''' <returns>True, if the component settings are to be persisted to the config file; otherwise, false.</returns>
        <Description("Indicates whether the component settings are to be persisted to the config file."), DefaultValue(GetType(Boolean), "False")> _
        Public Property PersistSettings() As Boolean Implements IPersistSettings.PersistSettings
            Get
                Return m_persistSettings
            End Get
            Set(ByVal value As Boolean)
                m_persistSettings = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the category name under which the component settings are to be saved in the config file.
        ''' </summary>
        ''' <returns>The category name under which the component settings are to be saved in the config file.</returns>
        <Description("The category name under which the component settings are to be saved in the config file."), DefaultValue(GetType(String), "LogFile")> _
        Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
            Get
                Return m_settingsCategoryName
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_settingsCategoryName = value
                Else
                    Throw New ArgumentNullException("SettingsCategoryName")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Loads the component settings from the config file, if present.
        ''' </summary>
        Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    If .Count > 0 Then
                        Name = .Item("Name").GetTypedValue(m_name)
                        Size = .Item("Size").GetTypedValue(m_size)
                        AutoOpen = .Item("AutoOpen").GetTypedValue(m_autoOpen)
                        FileFullOperation = .Item("FileFullOperation").GetTypedValue(m_fileFullOperation)
                    End If
                End With
            Catch ex As Exception
                ' Exceptions will occur if the settings are not present in the config file.
            End Try

        End Sub

        ''' <summary>
        ''' Saves the component settings to the config file.
        ''' </summary>
        Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
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
                            .Description = "True if the log file is to be open automatically after initialization is complete; otherwise False."
                        End With
                        With .Item("FileFullOperation", True)
                            .Value = m_fileFullOperation.ToString()
                            .Description = "Operation (Truncate; Rollover) that is to be performed on the file when it is full."
                        End With
                    End With
                    TVA.Configuration.Common.SaveSettings()
                Catch ex As Exception
                    ' Exceptions may occur if the settings cannot be saved to the config file.
                End Try
            End If

        End Sub

#End Region

#Region " ISupportInitialize "

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

            ' No prerequisites before the component is initialized.

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            If Not DesignMode Then
                LoadSettings()            ' Loads settings from the config file.
                If m_autoOpen Then Open() ' Opens the file automatically, if specified.
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
                    ' Write entries with text.
                    Dim buffer As Byte() = Encoding.Default.GetBytes(items(i))

                    If currentFileSize + buffer.Length <= maximumFileSize Then
                        ' Writes the entry.
                        SyncLock m_fileStream
                            m_fileStream.Write(buffer, 0, buffer.Length)
                            m_fileStream.Flush()
                        End SyncLock
                        currentFileSize += buffer.Length
                    Else
                        ' Either truncates the file or rolls over to a new file because the current file is full. 
                        ' Prior to acting, it requeues the entries that have not been written to the file.
                        For j As Integer = items.Length - 1 To i Step -1
                            m_logEntryQueue.Insert(0, items(j))
                        Next

                        ' Truncates file or roll over to new file.
                        RaiseEvent FileFull(Me, EventArgs.Empty)

                        Exit Sub
                    End If
                End If
            Next

        End Sub

#Region " Event Handlers "

#Region " m_logEntryQueue "

        Private Sub m_logEntryQueue_ProcessException(ByVal ex As System.Exception) Handles m_logEntryQueue.ProcessException

            RaiseEvent LogException(Me, New GenericEventArgs(Of Exception)(ex))

        End Sub

#End Region

#Region " LogFile "

        Private Sub LogFile_FileFull(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.FileFull

            ' Signals that the "file full operation" is in progress.
            m_operationWaitHandle.Reset()

            Select Case m_fileFullOperation
                Case LogFileFullOperation.Truncate
                    ' Deletes the existing log entries, and makes way from new ones.
                    Try
                        Close(False)
                        File.Delete(m_name)
                    Catch ex As Exception
                        Throw
                    Finally
                        Open()
                    End Try
                Case LogFileFullOperation.Rollover
                    Dim historyFileName As String = JustPath(m_name) & NoFileExtension(m_name) & "_" & _
                                                    File.GetCreationTime(m_name).ToString("yyyy-MM-dd hh!mm!ss") & "_to_" & _
                                                    File.GetLastWriteTime(m_name).ToString("yyyy-MM-dd hh!mm!ss") & JustFileExtension(m_name)

                    ' Rolls over to a new log file, and keeps the current file for history.
                    Try
                        Close(False)
                        File.Move(m_name, historyFileName)
                    Catch ex As Exception
                        Throw
                    Finally
                        Open()
                    End Try
            End Select

            ' Signals that the "file full operation" is complete.
            m_operationWaitHandle.Set()

        End Sub

#End Region

#End Region

#End Region

    End Class

End Namespace