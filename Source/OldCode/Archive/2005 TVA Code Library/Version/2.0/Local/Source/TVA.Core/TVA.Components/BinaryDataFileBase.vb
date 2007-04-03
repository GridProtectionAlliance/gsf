' 03/08/2007

Imports System.IO
Imports System.ComponentModel
Imports TVA.IO.FilePath

Namespace Components

    Public MustInherit Class BinaryDataFileBase(Of T As IBinaryDataProvider)
        Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

        Private m_name As String
        Private m_loadOnOpen As Boolean
        Private m_reloadOnModify As Boolean
        Private m_saveOnClose As Boolean
        Private m_alignOnSave As Boolean
        Private m_autoSaveInterval As Integer
        Private m_autoAlignInterval As Integer
        Private m_minimumRecordCount As Integer
        Private m_fileRecords As List(Of T)
        Private m_persistSettings As Boolean
        Private m_configurationCategory As String

        Private m_fileStream As FileStream

        Private WithEvents m_autoSaveTimer As System.Timers.Timer
        Private WithEvents m_autoAnalyzeTimer As System.Timers.Timer

#End Region

#Region " Event Declaration "

        Public Event FileOpening As EventHandler
        Public Event FileOpened As EventHandler
        Public Event FileClosing As EventHandler
        Public Event FileClosed As EventHandler
        Public Event FileModified As EventHandler
        Public Event DataLoadStart As EventHandler
        Public Event DataLoadComplete As EventHandler
        Public Event DataLoadProgress As EventHandler(Of ProgressEventArgs(Of Integer))
        Public Event DataSaveStart As EventHandler
        Public Event DataSaveComplete As EventHandler
        Public Event DataSaveProgress As EventHandler(Of ProgressEventArgs(Of Integer))
        Public Event DataAlignStart As EventHandler
        Public Event DataAlignComplete As EventHandler
        Public Event DataAlignProgress As EventHandler(Of ProgressEventArgs(Of Integer))

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

        Public Property AlignOnSave() As Boolean
            Get
                Return m_alignOnSave
            End Get
            Set(ByVal value As Boolean)
                m_alignOnSave = value
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

        Public Property AutoAlignInterval() As Integer
            Get
                Return m_autoAlignInterval
            End Get
            Set(ByVal value As Integer)
                m_autoAlignInterval = value
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
        Public ReadOnly Property Records() As List(Of T)
            Get
                Return m_fileRecords
            End Get
        End Property

        Public Sub Open()

            If Not IsOpen Then
                RaiseEvent FileOpening(Me, EventArgs.Empty)

                ' Initialize the list that will hold the file records in memory.
                m_fileRecords = New List(Of T)()

                m_name = AbsolutePath(m_name)
                If File.Exists(m_name) Then
                    ' File exists, so we'll open it.
                    m_fileStream = New FileStream(m_name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)

                    ' Once we have the file open, we'll process the file data.
                    If m_fileStream.Length Mod RecordSize = 0 Then
                        ' The file we're working with is a valid one.
                        If m_loadOnOpen Then Load()
                    Else
                        Close(False)
                        Throw New InvalidOperationException(String.Format("File """"{0}"""" is corrupt.", m_name))
                    End If
                Else
                    ' File doesn't exist, so we'll create it.
                    m_fileStream = New FileStream(m_name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)

                    ' Since we're working with a new file, we'll populate the in-memory list of records with the default
                    ' number of records as per the settings. These points will be witten back to the file when Save() is 
                    ' called or Close() is called and SaveOnClose is set to True.
                    For i As Integer = 1 To m_minimumRecordCount
                        m_fileRecords.Add(NewRecord(i))
                    Next
                End If

                If m_reloadOnModify Then
                    FileSystemWatcher.Path = JustPath(m_name)
                    FileSystemWatcher.Filter = JustFileName(m_name)
                    FileSystemWatcher.EnableRaisingEvents = True
                End If
                If m_autoSaveInterval > 0 Then
                    m_autoSaveTimer.Interval = m_autoSaveInterval
                    m_autoSaveTimer.Start()
                End If
                If m_autoAlignInterval > 0 Then
                    m_autoAnalyzeTimer.Interval = m_autoAlignInterval
                    m_autoAnalyzeTimer.Start()
                End If

                RaiseEvent FileOpened(Me, EventArgs.Empty)
            End If

        End Sub

        Public Sub Close()

            Close(m_saveOnClose)

        End Sub

        Public Sub Close(ByVal saveFile As Boolean)

            If IsOpen Then
                RaiseEvent FileClosing(Me, EventArgs.Empty)

                ' Stop the timers if they are ticking.
                m_autoSaveTimer.Stop()
                m_autoAnalyzeTimer.Stop()

                ' Save records back to the file if specified.
                If saveFile Then Save()

                ' Release all of the used resources.
                m_fileStream.Dispose()
                m_fileStream = Nothing
                m_fileRecords.Clear()
                m_fileRecords = Nothing
                FileSystemWatcher.EnableRaisingEvents = False

                RaiseEvent FileClosed(Me, EventArgs.Empty)
            End If

        End Sub

        Public Sub Load()

            If IsOpen Then
                RaiseEvent DataLoadStart(Me, EventArgs.Empty)

                Dim binaryImage As Byte() = TVA.Common.CreateArray(Of Byte)(RecordSize)
                Dim recordCount As Integer = Convert.ToInt32(m_fileStream.Length \ binaryImage.Length)

                m_fileRecords.Clear()
                SyncLock m_fileStream
                    ' Create records from the data in the file.
                    For i As Integer = 1 To recordCount
                        m_fileStream.Read(binaryImage, 0, binaryImage.Length)
                        m_fileRecords.Add(NewRecord(i, binaryImage))

                        RaiseEvent DataLoadProgress(Me, New ProgressEventArgs(Of Integer)(recordCount, i))
                    Next
                End SyncLock

                ' Make sure that we have the minimum number of records specified.
                For i As Integer = m_fileRecords.Count + 1 To m_minimumRecordCount
                    m_fileRecords.Add(NewRecord(i))
                Next

                RaiseEvent DataLoadComplete(Me, EventArgs.Empty)
            End If

        End Sub

        Public Sub Save()

            If IsOpen Then
                ' Align the records before writing them to the file if specified.
                If m_alignOnSave Then Align()

                RaiseEvent DataSaveStart(Me, EventArgs.Empty)

                SyncLock m_fileStream
                    ' Set the cursor to BOF before we start writing to the file.
                    m_fileStream.Seek(0, SeekOrigin.Begin)
                    ' Write all of the records to the file.
                    For i As Integer = 0 To m_fileRecords.Count - 1
                        m_fileStream.Write(m_fileRecords(i).BinaryImage, 0, RecordSize)

                        RaiseEvent DataSaveProgress(Me, New ProgressEventArgs(Of Integer)(m_fileRecords.Count, i + 1))
                    Next
                    m_fileStream.Flush()    ' Ensure that the data is written to the file.
                End SyncLock

                RaiseEvent DataSaveComplete(Me, EventArgs.Empty)
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Sub Align()

            If m_fileRecords IsNot Nothing AndAlso m_fileRecords.Count > 0 Then
                ' We can proceed with aligning the records since they have been initialized.
                ' First, we'll make a working copy of the records.

                RaiseEvent DataAlignStart(Me, EventArgs.Empty)

                Dim nonAlignedRecords As New List(Of T)(m_fileRecords)
                ' Sorting will ensure that the records are in proper order.
                nonAlignedRecords.Sort()
                ' Clear the actual record list.
                m_fileRecords.Clear()
                For i As Integer = 0 To nonAlignedRecords.Count - 1
                    ' We'll use the Write() method for adding records, so that we make use of any special record 
                    ' alignment that may be performed in deriving classes.
                    Write(nonAlignedRecords(i))

                    RaiseEvent DataAlignProgress(Me, New ProgressEventArgs(Of Integer)(nonAlignedRecords.Count, i + 1))
                Next

                RaiseEvent DataAlignComplete(Me, EventArgs.Empty)
            End If

        End Sub

        Public Overridable Function Read() As List(Of T)

            If IsOpen Then
                ' We'll load records from the file if they have not been loaded already.
                If Not m_loadOnOpen AndAlso m_fileRecords.Count = 0 Then Load()

                Return m_fileRecords
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Function

        Public Overridable Function Read(ByVal id As Integer) As T

            If IsOpen Then
                ' We'll load records from the file if they have not been loaded already.
                If Not m_loadOnOpen AndAlso m_fileRecords.Count = 0 Then Load()

                If id <= m_fileRecords.Count Then
                    Return m_fileRecords(id - 1)
                Else
                    Throw New ArgumentException(String.Format("The ID ""{0}"" is invalid. No record exists for this ID.", id))
                End If
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Function

        Public Overridable Sub Write(ByVal record As T)

            If IsOpen Then
                If record IsNot Nothing Then
                    ' Insert/Update the record in the in-memory list of records.
                    Dim recordIndex As Integer = m_fileRecords.IndexOf(record)
                    If recordIndex < 0 Then
                        ' We have to add the record since it doesn't exist.
                        m_fileRecords.Add(record)
                    Else
                        ' We have to update the record since one already exists.
                        m_fileRecords(recordIndex) = record
                    End If
                Else
                    Throw New ArgumentNullException("record")
                End If
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

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
                        LoadOnOpen = .Item("LoadOnOpen").GetTypedValue(m_loadOnOpen)
                        ReloadOnModify = .Item("ReloadOnModify").GetTypedValue(m_reloadOnModify)
                        SaveOnClose = .Item("SaveOnClose").GetTypedValue(m_saveOnClose)
                        AlignOnSave = .Item("AlignOnSave").GetTypedValue(m_alignOnSave)
                        AutoSaveInterval = .Item("AutoSaveInterval").GetTypedValue(m_autoSaveInterval)
                        AutoAlignInterval = .Item("AutoAlignInterval").GetTypedValue(m_autoAlignInterval)
                        MinimumRecordCount = .Item("MinimumRecordCount").GetTypedValue(m_minimumRecordCount)
                    End With
                Catch ex As Exception
                    ' We'll encounter exceptions if the settings are not present in the config file.
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
                            .Description = "Name of the file including its path."
                        End With
                        With .Item("LoadOnOpen", True)
                            .Value = m_loadOnOpen.ToString()
                            .Description = "True if file is to be loaded when opened; otherwise False."
                        End With
                        With .Item("ReloadOnModify", True)
                            .Value = m_reloadOnModify.ToString()
                            .Description = "True if file is to be re-loaded when modified externally; otherwise False."
                        End With
                        With .Item("SaveOnClose", True)
                            .Value = m_saveOnClose.ToString()
                            .Description = "True if file is to be saved when closed; otherwise False."
                        End With
                        With .Item("AlignOnSave", True)
                            .Value = m_alignOnSave.ToString()
                            .Description = "True if alignment of file data is to be performed before saving; otherwise False."
                        End With
                        With .Item("AutoSaveInterval", True)
                            .Value = m_autoSaveInterval.ToString()
                            .Description = "Interval in milliseconds at which the file is to be saved automatically. A value of -1 indicates that automatic saving is disabled."
                        End With
                        With .Item("AutoAlignInterval", True)
                            .Value = m_autoAlignInterval.ToString()
                            .Description = "Interval in milliseconds at which the file data is to be aligned automatically. A value of -1 indicates that automatic alignment is disabled."
                        End With
                        With .Item("MinimumRecordCount", True)
                            .Value = m_minimumRecordCount.ToString()
                            .Description = "Minimum number of records that the file must have."
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
                LoadSettings()  ' Load settings from the config file.
            End If

        End Sub

#End Region

#End Region

#End Region

#Region " Code Scope: Private "

#Region " Event Handlers "

#Region " m_autoSaveTimer "

        Private Sub m_autoSaveTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_autoSaveTimer.Elapsed

            If IsOpen Then Save() ' Automatically save records to the file if the file is open.

        End Sub

#End Region

#Region " m_autoAnalyzeTimer "

        Private Sub m_autoAnalyzeTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_autoAnalyzeTimer.Elapsed

            Align()   ' Automatically align the records in the list.

        End Sub

#End Region

#Region " FileSystemWatcher "

        Private Sub FileSystemWatcher_Changed(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles FileSystemWatcher.Changed

            RaiseEvent FileModified(Me, EventArgs.Empty)

            ' Reload the file when it is modified externally, but only if it has been loaded once.
            If m_fileRecords.Count > 0 Then Load()

        End Sub

#End Region
        
#End Region

#End Region

    End Class

End Namespace