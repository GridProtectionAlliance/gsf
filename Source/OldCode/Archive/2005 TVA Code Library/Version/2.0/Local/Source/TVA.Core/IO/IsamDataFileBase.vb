' 03/08/2007

Option Strict On

Imports System.IO
Imports System.Threading
Imports System.ComponentModel
Imports TVA.IO.FilePath

Namespace IO

    Public MustInherit Class IsamDataFileBase(Of T As IBinaryDataProvider)
        Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

        Private m_name As String
        Private m_loadOnOpen As Boolean
        Private m_reloadOnModify As Boolean
        Private m_saveOnClose As Boolean
        Private m_autoSaveInterval As Integer
        Private m_minimumRecordCount As Integer
        Private m_persistSettings As Boolean
        Private m_settingsCategoryName As String

        Private m_fileStream As FileStream
        Private m_fileRecords As List(Of T)
        Private m_loadWaitHandle As ManualResetEvent
        Private m_saveWaitHandle As ManualResetEvent

        Private WithEvents m_autoSaveTimer As System.Timers.Timer

#End Region

#Region " Event Declaration "

        Public Event FileOpening As EventHandler
        Public Event FileOpened As EventHandler
        Public Event FileClosing As EventHandler
        Public Event FileClosed As EventHandler
        Public Event FileModified As EventHandler
        Public Event DataLoading As EventHandler
        Public Event DataLoaded As EventHandler
        Public Event DataSaving As EventHandler
        Public Event DataSaved As EventHandler
        Public Event DataReadStart As EventHandler
        Public Event DataReadComplete As EventHandler
        Public Event DataReadProgress As EventHandler(Of GenericEventArgs(Of ProcessProgress(Of Integer)))
        Public Event DataWriteStart As EventHandler
        Public Event DataWriteComplete As EventHandler
        Public Event DataWriteProgress As EventHandler(Of GenericEventArgs(Of ProcessProgress(Of Integer)))

#End Region

#Region " Code Scope: Public "

        Public Const Extension As String = ".dat"

        Public Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    If String.Compare(JustFileExtension(value), Extension) = 0 Then
                        m_name = value
                        If IsOpen() Then
                            Close()
                            Open()
                        End If
                    Else
                        Throw New ArgumentException(String.Format("Name must have an extension of {0}.", Extension))
                    End If
                Else
                    Throw New ArgumentNullException("Name")
                End If
            End Set
        End Property

        Public Property LoadOnOpen() As Boolean
            Get
                Return m_loadOnOpen
            End Get
            Set(ByVal value As Boolean)
                m_loadOnOpen = value
            End Set
        End Property

        Public Property ReloadOnModify() As Boolean
            Get
                Return m_reloadOnModify
            End Get
            Set(ByVal value As Boolean)
                m_reloadOnModify = value
            End Set
        End Property

        Public Property SaveOnClose() As Boolean
            Get
                Return m_saveOnClose
            End Get
            Set(ByVal value As Boolean)
                m_saveOnClose = value
            End Set
        End Property

        Public Property AutoSaveInterval() As Integer
            Get
                Return m_autoSaveInterval
            End Get
            Set(ByVal value As Integer)
                m_autoSaveInterval = value
            End Set
        End Property

        Public Property MinimumRecordCount() As Integer
            Get
                Return m_minimumRecordCount
            End Get
            Set(ByVal value As Integer)
                m_minimumRecordCount = value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsOpen() As Boolean
            Get
                Return m_fileStream IsNot Nothing
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsCorrupt() As Boolean
            Get
                If IsOpen Then
                    Dim fileLength As Long
                    SyncLock m_fileStream
                        fileLength = m_fileStream.Length
                    End SyncLock
                    Return (fileLength Mod RecordSize <> 0)
                Else
                    Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
                End If
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsSynchronized() As Boolean
            Get
                Return InMemoryRecordCount = PersistedRecordCount
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>In KB.</remarks>
        <Browsable(False)> _
        Public ReadOnly Property MemoryUsage() As Long
            Get
                Return InMemoryRecordCount * RecordSize \ 1024
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property InMemoryRecordCount() As Integer
            Get
                Dim recordCount As Integer = 0
                If m_fileRecords IsNot Nothing Then
                    SyncLock m_fileRecords
                        recordCount = m_fileRecords.Count
                    End SyncLock
                End If
                Return recordCount
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property PersistedRecordCount() As Integer
            Get
                If IsOpen Then
                    Dim fileLength As Long
                    SyncLock m_fileStream
                        fileLength = m_fileStream.Length
                    End SyncLock
                    Return Convert.ToInt32(fileLength \ RecordSize)
                Else
                    Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
                End If
            End Get
        End Property

        Public Sub Open()

            If Not IsOpen Then
                RaiseEvent FileOpening(Me, EventArgs.Empty)

                m_name = AbsolutePath(m_name)
                If Not Directory.Exists(JustPath(m_name)) Then Directory.CreateDirectory(JustPath(m_name))
                If File.Exists(m_name) Then
                    ' Opens existing file.
                    m_fileStream = New FileStream(m_name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
                Else
                    ' Creates file.
                    m_fileStream = New FileStream(m_name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
                End If

                If m_loadOnOpen Then Load()

                ' Makes sure that we have the minimum number of records specified.
                For i As Integer = PersistedRecordCount + 1 To m_minimumRecordCount
                    Write(i, NewRecord(i))
                Next

                If m_reloadOnModify Then
                    ' Watches for any modifications made to the file.
                    FileSystemWatcher.Path = JustPath(m_name)
                    FileSystemWatcher.Filter = JustFileName(m_name)
                    FileSystemWatcher.EnableRaisingEvents = True
                End If

                If m_autoSaveInterval > 0 Then
                    ' Starts the timer for saving data automatically.
                    m_autoSaveTimer.Interval = m_autoSaveInterval
                    m_autoSaveTimer.Start()
                End If

                RaiseEvent FileOpened(Me, EventArgs.Empty)
            End If

        End Sub

        Public Sub Close()

            If IsOpen Then
                RaiseEvent FileClosing(Me, EventArgs.Empty)

                ' Stops the timers if they are ticking.
                m_autoSaveTimer.Stop()

                ' Stops monitoring for changes to the file.
                FileSystemWatcher.EnableRaisingEvents = False

                ' Saves records back to the file if specified.
                If m_saveOnClose Then Save()

                ' Releases all of the used resources.
                If m_fileStream IsNot Nothing Then
                    SyncLock m_fileStream
                        m_fileStream.Dispose()
                    End SyncLock
                End If
                m_fileStream = Nothing
                If m_fileRecords IsNot Nothing Then
                    SyncLock m_fileRecords
                        m_fileRecords.Clear()
                    End SyncLock
                End If
                m_fileRecords = Nothing

                RaiseEvent FileClosed(Me, EventArgs.Empty)
            End If

        End Sub

        Public Sub Load()

            If IsOpen Then
                ' Waits for any pending request to save records before completing.
                m_saveWaitHandle.WaitOne()
                ' Waits for any prior request to load records before completing.
                m_loadWaitHandle.WaitOne()

                m_loadWaitHandle.Reset()
                Try
                    RaiseEvent DataLoading(Me, EventArgs.Empty)

                    If m_fileRecords Is Nothing Then
                        m_fileRecords = New List(Of T)()
                    End If

                    Dim records As New List(Of T)(ReadFromDisk())
                    SyncLock m_fileRecords
                        m_fileRecords.Clear()
                        m_fileRecords.InsertRange(0, records)
                    End SyncLock

                    RaiseEvent DataLoaded(Me, EventArgs.Empty)
                Catch ex As Exception
                    Throw
                Finally
                    m_loadWaitHandle.Set()
                End Try
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Sub Save()

            If IsOpen Then
                ' Waits for any pending request to save records before completing.
                m_saveWaitHandle.WaitOne()
                ' Waits for any prior request to load records before completing.
                m_loadWaitHandle.WaitOne()

                m_saveWaitHandle.Reset()
                Try
                    RaiseEvent DataSaving(Me, EventArgs.Empty)

                    ' Saves (persists) records to the file, if present in memory.
                    If m_fileRecords IsNot Nothing Then
                        SyncLock m_fileRecords
                            WriteToDisk(m_fileRecords)
                        End SyncLock
                        If InMemoryRecordCount < PersistedRecordCount Then
                            SyncLock m_fileStream
                                m_fileStream.SetLength(InMemoryRecordCount * RecordSize)
                            End SyncLock
                        End If
                    End If

                    RaiseEvent DataSaved(Me, EventArgs.Empty)
                Catch ex As Exception
                    Throw
                Finally
                    m_saveWaitHandle.Set()
                End Try
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Overridable Sub Write(ByVal records As List(Of T))

            If IsOpen Then
                If m_fileRecords Is Nothing Then
                    WriteToDisk(records)
                Else
                    SyncLock m_fileRecords
                        m_fileRecords.Clear()
                        m_fileRecords.InsertRange(0, records)
                    End SyncLock
                End If
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Overridable Sub Write(ByVal recordID As Integer, ByVal record As T)

            If IsOpen Then
                If record IsNot Nothing Then
                    If m_fileRecords Is Nothing Then
                        ' We're writing directly to the file.
                        WriteToDisk(recordID, record)
                    Else
                        ' We're updating the in-memory record list.
                        Dim lastRecordID As Integer = InMemoryRecordCount
                        If recordID > lastRecordID Then
                            If recordID > lastRecordID + 1 Then
                                For i As Integer = lastRecordID + 1 To recordID - 1
                                    Write(i, NewRecord(i))
                                Next
                            End If

                            SyncLock m_fileRecords
                                m_fileRecords.Add(record)
                            End SyncLock
                        Else
                            ' Updates the existing record with the new one.
                            SyncLock m_fileRecords
                                m_fileRecords(recordID - 1) = record
                            End SyncLock
                        End If
                    End If
                Else
                    Throw New ArgumentNullException("record")
                End If
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Overridable Function Read() As List(Of T)

            If IsOpen Then
                Dim records As New List(Of T)()

                If m_fileRecords Is Nothing Then
                    ' Reads persisted records if no records are in memory.
                    records.InsertRange(0, ReadFromDisk())
                Else
                    ' Reads records in memory.
                    SyncLock m_fileRecords
                        records.InsertRange(0, m_fileRecords)
                    End SyncLock
                End If

                Return records
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Function

        Public Overridable Function Read(ByVal recordID As Integer) As T

            If IsOpen Then
                Dim record As T = Nothing
                If recordID > 0 Then
                    ' ID of the requested record is valid.
                    If m_fileRecords Is Nothing AndAlso recordID <= PersistedRecordCount Then
                        ' Reads the requested record exists in the file.
                        record = ReadFromDisk(recordID)
                    ElseIf m_fileRecords IsNot Nothing AndAlso recordID <= InMemoryRecordCount Then
                        ' Uses the requested record from memory.
                        SyncLock m_fileRecords
                            record = m_fileRecords(recordID - 1)
                        End SyncLock
                    End If
                End If

                Return record
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Function

        <Browsable(False)> _
        Public MustOverride ReadOnly Property RecordSize() As Integer

        Public MustOverride Function NewRecord(ByVal id As Integer) As T

        Public MustOverride Function NewRecord(ByVal id As Integer, ByVal binaryImage As Byte()) As T

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

        Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    If .Count > 0 Then
                        Name = .Item("Name").GetTypedValue(m_name)
                        LoadOnOpen = .Item("LoadOnOpen").GetTypedValue(m_loadOnOpen)
                        ReloadOnModify = .Item("ReloadOnModify").GetTypedValue(m_reloadOnModify)
                        SaveOnClose = .Item("SaveOnClose").GetTypedValue(m_saveOnClose)
                        AutoSaveInterval = .Item("AutoSaveInterval").GetTypedValue(m_autoSaveInterval)
                        MinimumRecordCount = .Item("MinimumRecordCount").GetTypedValue(m_minimumRecordCount)
                    End If
                End With
            Catch ex As Exception
                ' Exceptions will occur if the settings are not present in the config file.
            End Try

        End Sub

        Public Sub SaveSettings() Implements IPersistSettings.SaveSettings

            If m_persistSettings Then
                Try
                    With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                        .Clear()
                        With .Item("Name", True)
                            .Value = m_name
                            .Description = "Name of the file including its path."
                        End With
                        With .Item("LoadOnOpen", True)
                            .Value = m_loadOnOpen.ToString()
                            .Description = "True if file is to be loaded when opened; otherwise False."
                        End With
                        With .Item("ReloadOnModify", True)
                            .Value = m_reloadOnModify.ToString()
                            .Description = "True if file is to be re-loaded when modified; otherwise False."
                        End With
                        With .Item("SaveOnClose", True)
                            .Value = m_saveOnClose.ToString()
                            .Description = "True if file is to be saved when closed; otherwise False."
                        End With
                        With .Item("AutoSaveInterval", True)
                            .Value = m_autoSaveInterval.ToString()
                            .Description = "Interval in milliseconds at which the file is to be saved automatically. A value of -1 indicates that automatic saving is disabled."
                        End With
                        With .Item("MinimumRecordCount", True)
                            .Value = m_minimumRecordCount.ToString()
                            .Description = "Minimum number of records that the file must have."
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
                LoadSettings()  ' Loads settings from the config file.
            End If

        End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

        Private Sub WriteToDisk(ByVal records As List(Of T))

            For i As Integer = 1 To records.Count
                WriteToDisk(i, records(i - 1))
            Next

        End Sub

        Private Sub WriteToDisk(ByVal recordID As Integer, ByVal record As T)

            SyncLock m_fileStream
                m_fileStream.Seek((recordID - 1) * record.BinaryLength, SeekOrigin.Begin)
                m_fileStream.Write(record.BinaryImage, 0, record.BinaryLength)
                m_fileStream.Flush()
            End SyncLock

        End Sub

        Private Function ReadFromDisk() As List(Of T)

            Dim records As New List(Of T)()
            Dim recordCount As Integer = PersistedRecordCount
            For i As Integer = 1 To recordCount
                records.Add(ReadFromDisk(i))
            Next
            Return records

        End Function

        Private Function ReadFromDisk(ByVal recordID As Integer) As T

            Dim binaryImage As Byte() = TVA.Common.CreateArray(Of Byte)(RecordSize)
            SyncLock m_fileStream
                m_fileStream.Seek((recordID - 1) * RecordSize, SeekOrigin.Begin)
                m_fileStream.Read(binaryImage, 0, binaryImage.Length)
            End SyncLock
            Return NewRecord(recordID, binaryImage)

        End Function

#Region " Event Handlers "

#Region " m_autoSaveTimer "

        Private Sub m_autoSaveTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_autoSaveTimer.Elapsed

            If IsOpen Then Save() ' Automatically save records to the file if the file is open.

        End Sub

#End Region

#Region " FileSystemWatcher "

        Private Sub FileSystemWatcher_Changed(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles FileSystemWatcher.Changed

            RaiseEvent FileModified(Me, EventArgs.Empty)

            ' Reload the file when it is modified externally, but only if it has been loaded once.
            If m_fileRecords IsNot Nothing AndAlso m_reloadOnModify Then Load()

        End Sub

#End Region

#End Region

#End Region

    End Class

End Namespace