' 02/18/2007

Imports System.IO
Imports System.Drawing
Imports System.Threading
Imports System.ComponentModel
Imports TVA.Collections
Imports TVA.IO.FilePath

Namespace Files

    <ToolboxBitmap(GetType(ArchiveFile)), DisplayName("DwArchiveFile")> _
    Public Class ArchiveFile
        Implements IPersistSettings, ISupportInitialize

#Region " Member Declaration "

        Private m_name As String
        Private m_type As ArchiveFileType
        Private m_size As Double
        Private m_blockSize As Integer
        Private m_saveOnClose As Boolean
        Private m_rolloverOnFull As Boolean
        Private m_rolloverPreparationThreshold As Short
        Private m_offloadPath As String
        Private m_offloadCount As Integer
        Private m_offloadThreshold As Short
        Private m_compressPoints As Boolean
        Private m_discardOutOfSequencePoints As Boolean
        Private m_stateFile As StateFile
        Private m_intercomFile As IntercomFile
        Private m_fat As ArchiveFileAllocationTable
        Private m_persistSettings As Boolean
        Private m_settingsCategoryName As String
        Private m_pointsDiscarded As Integer
        Private m_pointsCompressed As Integer
        Private m_pointsHistoric As Integer
        Private m_pointsOutOfSequence As Integer

        Private m_environmentData As EnvironmentData
        Private m_fileStream As FileStream
        Private m_dataBlockList As List(Of ArchiveDataBlock)
        Private m_historicArchiveFileList As List(Of ArchiveFileInfo)
        Private m_rolloverPreparationThread As Thread
        Private m_buildHistoricFileListThread As Thread
        Private m_rolloverWaitHandle As ManualResetEvent
        Private m_searchTimeTag As TimeTag  ' Used for finding a historic archive file for historic data.

        Private WithEvents m_historicDataQueue As ProcessQueue(Of StandardPointData)
        Private WithEvents m_outOfSequenceDataQueue As ProcessQueue(Of StandardPointData)

        Private StandbyFileExtension As String = ".standby"

#End Region

#Region " Event Declaration "

        Public Event FileFull As EventHandler
        Public Event FileOpening As EventHandler
        Public Event FileOpened As EventHandler
        Public Event FileClosing As EventHandler
        Public Event FileClosed As EventHandler
        Public Event RolloverStart As EventHandler
        Public Event RolloverComplete As EventHandler
        Public Event RolloverException As EventHandler(Of GenericEventArgs(Of Exception))
        Public Event OffloadStart As EventHandler
        Public Event OffloadComplete As EventHandler
        Public Event OffloadException As EventHandler(Of GenericEventArgs(Of Exception))
        Public Event OffloadProgress As EventHandler(Of GenericEventArgs(Of ProcessProgress(Of Integer)))
        Public Event RolloverPreparationStart As EventHandler
        Public Event RolloverPreparationComplete As EventHandler
        Public Event RolloverPreparationException As EventHandler(Of GenericEventArgs(Of Exception))
        Public Event HistoricFileListBuildStart As EventHandler
        Public Event HistoricFileListBuildComplete As EventHandler
        Public Event HistoricFileListBuildException As EventHandler(Of GenericEventArgs(Of Exception))
        Public Event HistoricFileListUpdated As EventHandler
        Public Event CurrentPointReceived As EventHandler
        Public Event CurrentPointWritten As EventHandler
        Public Event CurrentPointCompressed As EventHandler
        Public Event CurrentPointDiscarded As EventHandler
        Public Event HistoricPointReceived As EventHandler(Of GenericEventArgs(Of Integer))
        Public Event HistoricPointQueued As EventHandler
        Public Event HistoricPointsWriteStart As EventHandler
        Public Event HistoricPointsWriteComplete As EventHandler
        Public Event HistoricPointsWriteException As EventHandler(Of GenericEventArgs(Of Exception))
        Public Event HistoricPointsWriteProgress As EventHandler(Of GenericEventArgs(Of ProcessProgress(Of Integer)))
        Public Event OutOfSequencePointReceived As EventHandler(Of GenericEventArgs(Of Integer))
        Public Event OutOfSequencePointDiscarded As EventHandler
        Public Event OutOfSequencePointQueued As EventHandler
        Public Event OutOfSequencePointsWriteStart As EventHandler
        Public Event OutOfSequencePointsWriteComplete As EventHandler
        Public Event OutOfSequencePointsWriteException As EventHandler(Of GenericEventArgs(Of Exception))
        Public Event OutOfSequencePointsWriteProgress As EventHandler(Of GenericEventArgs(Of ProcessProgress(Of Integer)))

#End Region

#Region " Code Scope: Public "

        Public Const Extension As String = ".d"

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
                        Throw New ArgumentException(String.Format("Name of {0} must have an extension of {1}.", Me.GetType().Name, Extension))
                    End If
                Else
                    Throw New ArgumentNullException("Name")
                End If
            End Set
        End Property

        Public Property Type() As ArchiveFileType
            Get
                Return m_type
            End Get
            Set(ByVal value As ArchiveFileType)
                m_type = value
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Size in MB.</remarks>
        Public Property Size() As Double
            Get
                Return m_size
            End Get
            Set(ByVal value As Double)
                m_size = value
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Size in KB.</remarks>
        Public Property BlockSize() As Integer
            Get
                Return m_blockSize
            End Get
            Set(ByVal value As Integer)
                m_blockSize = value
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

        Public Property RolloverOnFull() As Boolean
            Get
                Return m_rolloverOnFull
            End Get
            Set(ByVal value As Boolean)
                m_rolloverOnFull = value
            End Set
        End Property

        Public Property RolloverPreparationThreshold() As Short
            Get
                Return m_rolloverPreparationThreshold
            End Get
            Set(ByVal value As Short)
                m_rolloverPreparationThreshold = value
            End Set
        End Property

        Public Property OffloadPath() As String
            Get
                Return m_offloadPath
            End Get
            Set(ByVal value As String)
                m_offloadPath = value
            End Set
        End Property

        Property OffloadCount() As Integer
            Get
                Return m_offloadCount
            End Get
            Set(ByVal value As Integer)
                m_offloadCount = value
            End Set
        End Property

        Public Property OffloadThreshold() As Short
            Get
                Return m_offloadThreshold
            End Get
            Set(ByVal value As Short)
                m_offloadThreshold = value
            End Set
        End Property

        Public Property CompressPoints() As Boolean
            Get
                Return m_compressPoints
            End Get
            Set(ByVal value As Boolean)
                m_compressPoints = value
            End Set
        End Property

        Public Property DiscardOutOfSequencePoints() As Boolean
            Get
                Return m_discardOutOfSequencePoints
            End Get
            Set(ByVal value As Boolean)
                m_discardOutOfSequencePoints = value
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Used for backwards compatibility with old version of DatAWare Server.</remarks>
        Public Property StateFile() As StateFile
            Get
                Return m_stateFile
            End Get
            Set(ByVal value As StateFile)
                m_stateFile = value
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Used for backwards compatibility with old version of DatAWare Server.</remarks>
        Public Property IntercomFile() As IntercomFile
            Get
                Return m_intercomFile
            End Get
            Set(ByVal value As IntercomFile)
                m_intercomFile = value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property PointsDiscarded() As Integer
            Get
                Return m_pointsDiscarded
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property PointsCompressed() As Integer
            Get
                Return m_pointsCompressed
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property PointsHistoric() As Integer
            Get
                Return m_pointsHistoric
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property PointsOutOfSequence() As Integer
            Get
                Return m_pointsOutOfSequence
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property IsOpen() As Boolean
            Get
                Return m_fileStream IsNot Nothing
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property FileAllocationTable() As ArchiveFileAllocationTable
            Get
                Return m_fat
            End Get
        End Property

        Public Sub Open()

            If Not IsOpen Then
                If m_stateFile IsNot Nothing AndAlso m_intercomFile IsNot Nothing Then
                    RaiseEvent FileOpening(Me, EventArgs.Empty)

                    m_name = AbsolutePath(m_name)
                    If m_type = ArchiveFileType.Standby Then m_name = StandbyArchiveFileName

                    If Not Directory.Exists(JustPath(m_name)) Then Directory.CreateDirectory(JustPath(m_name))
                    If File.Exists(m_name) Then
                        ' File has been created already, so we just need to read it.
                        m_fileStream = New FileStream(m_name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
                        m_fat = New ArchiveFileAllocationTable(m_fileStream)
                    Else
                        ' File does not exist, so we have to create it and initialize it.
                        m_fileStream = New FileStream(m_name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
                        m_fat = New ArchiveFileAllocationTable(m_fileStream, m_blockSize, MaximumDataBlocks(m_size, m_blockSize))
                        m_fat.Persist()
                    End If

                    m_historicDataQueue.Start()
                    m_outOfSequenceDataQueue.Start()

                    ' Make sure that the necessary files are available and ready for use.
                    If Not m_stateFile.IsOpen Then m_stateFile.Open()
                    If Not m_intercomFile.IsOpen Then m_intercomFile.Open()

                    ' This intercom message will be used for communicating with DW Server.
                    m_environmentData = m_intercomFile.Read(1)

                    m_dataBlockList = New List(Of ArchiveDataBlock)(CreateArray(Of ArchiveDataBlock)(m_stateFile.InMemoryRecordCount))

                    If m_type = ArchiveFileType.Active Then
                        ' We'll make sure that the intercom file has the right information. This is done to ensure
                        ' that the server is able to read this file because the server will not read the "active"
                        ' archive file if the file is being rolled over.
                        m_environmentData.RolloverInProgress = False

                        ' Start preparing the list of historic files on a seperate thread.
                        m_buildHistoricFileListThread = New Thread(AddressOf BuildHistoricFileList)
                        m_buildHistoricFileListThread.Priority = ThreadPriority.Lowest
                        m_buildHistoricFileListThread.Start()

                        CurrentLocationFileSystemWatcher.Filter = HistoricFilesSearchPattern
                        CurrentLocationFileSystemWatcher.Path = JustPath(m_name)
                        CurrentLocationFileSystemWatcher.EnableRaisingEvents = True
                        OffloadLocationFileSystemWatcher.Filter = HistoricFilesSearchPattern
                        OffloadLocationFileSystemWatcher.Path = m_offloadPath
                        OffloadLocationFileSystemWatcher.EnableRaisingEvents = True

                        If m_fat.FileStartTime.CompareTo(TimeTag.MinValue) = 0 Then
                            ' If the current file is the "active" archive file and if its file's start time is not 
                            ' set (i.e. we're working with a brand new file with no data), we set it to the timetag 
                            ' of the latest value from the intercom file. This latest value timetag will be 0 only 
                            ' when the archiver is being run for the first time and if that's the case, the file's 
                            ' start time will be set when the very first point data is written.
                            m_fat.FileStartTime = m_environmentData.LastestCurrentValueTimeTag
                        End If
                    End If

                    RaiseEvent FileOpened(Me, EventArgs.Empty)
                Else
                    Throw New InvalidOperationException("StateFile and IntercomFile properties must be set.")
                End If
            End If

        End Sub

        Public Sub Close()

            If IsOpen Then
                RaiseEvent FileClosing(Me, EventArgs.Empty)

                If m_saveOnClose Then Save()

                ' Abort any asynchronous processing.
                m_rolloverPreparationThread.Abort()
                m_buildHistoricFileListThread.Abort()

                ' Stop the historic and out-of-sequence data queues.
                m_historicDataQueue.Stop()
                m_outOfSequenceDataQueue.Stop()

                ' Dispose the file allocation table of the file.
                If m_fat IsNot Nothing Then
                    m_fat.Dispose()
                    m_fat = Nothing
                End If

                ' Dispose the file stream used primarily by the file allocation table.
                If m_fileStream IsNot Nothing Then
                    SyncLock m_fileStream
                        m_fileStream.Dispose()
                    End SyncLock
                    m_fileStream = Nothing
                End If

                If m_dataBlockList IsNot Nothing Then
                    For Each datablock As ArchiveDataBlock In m_dataBlockList
                        If datablock IsNot Nothing Then datablock.Dispose()
                    Next
                    m_dataBlockList.Clear()
                    m_dataBlockList = Nothing
                End If

                ' Stop watching to historic archive files.
                CurrentLocationFileSystemWatcher.EnableRaisingEvents = False
                OffloadLocationFileSystemWatcher.EnableRaisingEvents = False

                ' Clear the list of historic archive files.
                If m_historicArchiveFileList IsNot Nothing Then
                    SyncLock m_historicArchiveFileList
                        m_historicArchiveFileList.Clear()
                    End SyncLock
                    m_historicArchiveFileList = Nothing
                End If

                m_pointsDiscarded = 0
                m_pointsCompressed = 0
                m_pointsHistoric = 0
                m_pointsOutOfSequence = 0
                m_environmentData = Nothing

                RaiseEvent FileClosed(Me, EventArgs.Empty)
            End If

        End Sub

        Public Sub Save()

            If IsOpen Then
                ' The only thing that we need to write back to the file is the FAT.
                m_fat.Persist()
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Sub Rollover()

            If m_type = ArchiveFileType.Active Then
                Try
                    RaiseEvent RolloverStart(Me, EventArgs.Empty)

                    ' Notify other threads that rollover is in progress.
                    m_rolloverWaitHandle.Reset()

                    Dim historyFileName As String = HistoryArchiveFileName
                    Dim standbyFileName As String = StandbyArchiveFileName

                    ' Notify DW Server that rollover is in progress.
                    m_environmentData.RolloverInProgress = True
                    m_intercomFile.Save()
                    Close()

                    ' CRITICAL: Exception can be encountered if exclusive lock to the current file cannot be obtained.
                    '           Possible if the server fails to give up the file or for some reason the current file
                    '           doesn't release all locks on the file.
                    If File.Exists(m_name) Then
                        Try
                            WaitForWriteLock(m_name, 60)        ' Wait for the server to release the file.
                            File.Move(m_name, historyFileName)  ' Make the active archive file historic.
                            If File.Exists(standbyFileName) Then
                                ' We have a "standby" archive file for us to use, so we'll use it. It is possible that
                                ' the "standby" file may not be available for use if it could not be created due to 
                                ' insufficient disk space during the "rollover preparation stage". If that's the case, 
                                ' Open() below will try to create a new archive file, but will only succeed if there 
                                ' is enough disk space.
                                File.Move(standbyFileName, m_name)  ' Make the standby archive file active.
                            End If
                        Catch ex As Exception
                            Open()
                            Throw
                        End Try
                    End If

                    ' CRITICAL: Exception can be encountered if a "standby" archive is not present for us to use and
                    '           we cannot create a new archive file probably because there isn't enough disk space.
                    Try
                        Open()
                        ' Notify DW Server that rollover is complete.
                        m_environmentData.RolloverInProgress = False
                        m_intercomFile.Save()

                        ' Notify other threads that rollover is complete.
                        m_rolloverWaitHandle.Set()

                        RaiseEvent RolloverComplete(Me, EventArgs.Empty)
                    Catch ex As Exception
                        Close() ' Close the file if we fail to open it.
                        File.Delete(m_name)
                        Throw   ' Rethrow the exception so that the exception event can be raised.
                    End Try
                Catch ex As Exception
                    RaiseEvent RolloverException(Me, New GenericEventArgs(Of Exception)(ex))
                End Try
            End If

        End Sub

        Public Sub Write(ByVal pointData As StandardPointData)

            ' Yeild to the rollover process if it is in progress.
            m_rolloverWaitHandle.WaitOne()

            If IsOpen Then
                ' We don't allow data to be written to a "standby" file.
                If m_type = ArchiveFileType.Standby Then Exit Sub

                m_fat.PointsReceived += 1

                If pointData.Definition IsNot Nothing AndAlso pointData.Definition.GeneralFlags.Archived Then
                    ' The received data has a corresponding definition and is marked for archival.
                    Dim pointID As Integer = pointData.Definition.PointID
                    Dim pointIndex As Integer = pointID - 1
                    Dim pointState As PointState = m_stateFile.Read(pointID)
                    If pointData.TimeTag.CompareTo(m_fat.FileStartTime) >= 0 Then
                        ' The data belongs to this file.
                        If pointData.TimeTag.CompareTo(pointState.LastArchivedValue.TimeTag) >= 0 Then
                            ' The data is in sequence.
                            RaiseEvent CurrentPointReceived(Me, EventArgs.Empty)

                            If ToBeArchived(pointData, pointState) Then
                                ' Data failed compression test - write it to current file.

                                If m_dataBlockList(pointIndex) Is Nothing Then
                                    ' This is the first time we're writing data for this point since this file opened,
                                    ' so we'll check if this point's data has been written in this file previously. We
                                    ' do so by requesting the last data block for this point and scanning it if one is
                                    ' found. The result of the code below can be wither one of the following:
                                    ' 1) No data block is found - Data for this point has never been written to this 
                                    '    file previously. 
                                    ' 2) Data block is found but full - Data for this point has been written to this
                                    '    file previously.
                                    ' 3) Data block is found and not full - Data for this point has been written to 
                                    '    this file previously.
                                    m_dataBlockList(pointIndex) = m_fat.FindLastDataBlock(pointID)
                                    If m_dataBlockList(pointIndex) IsNot Nothing Then
                                        m_dataBlockList(pointIndex).Scan()
                                    End If
                                End If

                                If m_dataBlockList(pointIndex) Is Nothing OrElse _
                                        (m_dataBlockList(pointIndex) IsNot Nothing AndAlso m_dataBlockList(pointIndex).SlotsAvailable <= 0) Then
                                    ' We either don't have a active data block where we can archive the point data or 
                                    ' we have a active data block but it is full. So, we have to request a new data 
                                    ' block from the FAT in order to write the data.

                                    If m_dataBlockList(pointIndex) IsNot Nothing Then
                                        ' Release the previously used data block before requesting a new one.
                                        m_dataBlockList(pointIndex).Dispose()
                                        m_dataBlockList(pointIndex) = Nothing
                                    End If

                                    Select Case m_type
                                        Case ArchiveFileType.Active
                                            m_dataBlockList(pointIndex) = m_fat.RequestDataBlock(pointID, pointData.TimeTag)

                                            If Not File.Exists(StandbyArchiveFileName) AndAlso Not m_rolloverPreparationThread.IsAlive AndAlso _
                                                    m_fat.DataBlocksAvailable < m_fat.DataBlockCount * (1 - (m_rolloverPreparationThreshold / 100)) Then
                                                ' We've requested the specified percent of the total number of data 
                                                ' blocks in the file, so we must now prepare for the rollover process 
                                                ' since it has not been done yet and it is not already in progress.
                                                m_rolloverPreparationThread = New Thread(AddressOf PrepareForRollover)
                                                m_rolloverPreparationThread.Priority = ThreadPriority.Lowest
                                                m_rolloverPreparationThread.Start()
                                            End If
                                        Case ArchiveFileType.Historic
                                            m_dataBlockList(pointIndex) = m_fat.RequestDataBlock(pointID, pointData.TimeTag, True)
                                    End Select
                                End If

                                If m_dataBlockList(pointIndex) IsNot Nothing Then
                                    ' We have a data block to which we can write the data.
                                    m_dataBlockList(pointIndex).Write(pointData)
                                    m_fat.PointsArchived += 1

                                    If m_type = ArchiveFileType.Active Then m_fat.FileEndTime = pointData.TimeTag
                                    If m_fat.FileStartTime.CompareTo(TimeTag.MinValue) = 0 Then m_fat.FileStartTime = pointData.TimeTag

                                    RaiseEvent CurrentPointWritten(Me, EventArgs.Empty)
                                Else
                                    ' We either don't have a data block for writing data or we have one but it doesn't 
                                    ' belong to this file. This is possible under the following circumstances:
                                    ' 1) 
                                    ' 2)

                                    If m_fat.DataBlocksAvailable = 0 Then
                                        ' There are no more data blocks available for writing data to.
                                        RaiseEvent FileFull(Me, EventArgs.Empty)
                                    End If
                                End If
                            Else
                                ' Data passed compression test - don't write it.
                                m_pointsCompressed += 1
                                RaiseEvent CurrentPointCompressed(Me, EventArgs.Empty)
                            End If
                        Else
                            ' The data is out-of-sequence.
                            m_pointsOutOfSequence += 1
                            RaiseEvent OutOfSequencePointReceived(Me, New GenericEventArgs(Of Integer)(pointID))
                            If Not m_discardOutOfSequencePoints Then
                                ' Insert the data into the current file.
                                m_outOfSequenceDataQueue.Add(pointData)
                                RaiseEvent OutOfSequencePointQueued(Me, EventArgs.Empty)
                            Else
                                RaiseEvent OutOfSequencePointDiscarded(Me, EventArgs.Empty)
                            End If
                        End If
                    Else
                        ' The data is historic.
                        m_pointsHistoric += 1
                        RaiseEvent HistoricPointReceived(Me, New GenericEventArgs(Of Integer)(pointID))
                        If m_type = ArchiveFileType.Active Then
                            m_historicDataQueue.Add(pointData)
                            RaiseEvent HistoricPointQueued(Me, EventArgs.Empty)
                        End If
                    End If
                Else
                    ' We'll discard the data if it doesn't has a corresponding definition or is not
                    ' marked for archival in the metadata file.
                    m_pointsDiscarded += 1
                    RaiseEvent CurrentPointDiscarded(Me, EventArgs.Empty)
                End If
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Sub Write(ByVal pointData() As StandardPointData)

            ' Yeild to the rollover process if it is in progress.
            m_rolloverWaitHandle.WaitOne()

            If IsOpen Then
                For i As Integer = 0 To pointData.Length - 1
                    Write(pointData(i))
                Next
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Function Read(ByVal pointID As Integer) As List(Of StandardPointData)

            Return Read(pointID, TimeTag.MinValue)

        End Function

        Public Function Read(ByVal pointID As Integer, ByVal startTime As String) As List(Of StandardPointData)

            Return Read(pointID, startTime, TimeTag.MinValue.ToString())

        End Function

        Public Function Read(ByVal pointID As Integer, ByVal startTime As String, ByVal endTime As String) As List(Of StandardPointData)

            Return Read(pointID, Convert.ToDateTime(startTime), Convert.ToDateTime(endTime))

        End Function

        Public Function Read(ByVal pointID As Integer, ByVal startTime As Date) As List(Of StandardPointData)

            Return Read(pointID, startTime, TimeTag.MinValue.ToDateTime())

        End Function

        Public Function Read(ByVal pointID As Integer, ByVal startTime As Date, ByVal endTime As Date) As List(Of StandardPointData)

            Return Read(pointID, New TimeTag(startTime), New TimeTag(endTime))

        End Function

        Public Function Read(ByVal pointID As Integer, ByVal startTime As TimeTag) As List(Of StandardPointData)

            Return Read(pointID, startTime, TimeTag.MaxValue)

        End Function

        Public Function Read(ByVal pointID As Integer, ByVal startTime As TimeTag, ByVal endTime As TimeTag) As List(Of StandardPointData)

            ' Yeild to the rollover process if it is in progress.
            m_rolloverWaitHandle.WaitOne()

            If IsOpen Then
                ' We don't allow data to be read from a "standby" file.
                If m_type = ArchiveFileType.Standby Then Return Nothing

                Dim data As New List(Of StandardPointData)()
                Dim foundBlocks As List(Of ArchiveDataBlock) = m_fat.FindDataBlocks(pointID, startTime, endTime)
                For i As Integer = 0 To foundBlocks.Count - 1
                    If i < foundBlocks.Count - 1 Then
                        data.AddRange(foundBlocks(i).Read())
                    Else
                        ' We have to scan the data of the last block and only add the data that have a timetag
                        ' that's less than or equal to the specified end time. If we don't do this we might
                        ' return data that is beyond the specified time range.
                        Dim blockData As List(Of StandardPointData) = foundBlocks(i).Read()
                        For j As Integer = 0 To blockData.Count - 1
                            If blockData(j).TimeTag.CompareTo(endTime) <= 0 Then
                                data.Add(blockData(j))
                            End If
                        Next
                    End If
                    foundBlocks(i).Dispose()
                Next

                Return data
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Function

        Public Shared Function MaximumDataBlocks(ByVal fileSize As Double, ByVal blockSize As Integer) As Integer

            Return Convert.ToInt32((fileSize * 1024) / blockSize)

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

        Public Property SettingsCategoryName() As String Implements IPersistSettings.SettingsCategoryName
            Get
                Return m_settingsCategoryName
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_settingsCategoryName = value
                Else
                    Throw New ArgumentNullException("ConfigurationCategory")
                End If
            End Set
        End Property

        Public Sub LoadSettings() Implements IPersistSettings.LoadSettings

            Try
                With TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName)
                    If .Count > 0 Then
                        Name = .Item("Name").GetTypedValue(m_name)
                        Type = .Item("Type").GetTypedValue(m_type)
                        Size = .Item("Size").GetTypedValue(m_size)
                        BlockSize = .Item("BlockSize").GetTypedValue(m_blockSize)
                        SaveOnClose = .Item("SaveOnClose").GetTypedValue(m_saveOnClose)
                        RolloverOnFull = .Item("RolloverOnFull").GetTypedValue(m_rolloverOnFull)
                        RolloverPreparationThreshold = .Item("RolloverPreparationThreshold").GetTypedValue(m_rolloverPreparationThreshold)
                        OffloadPath = .Item("OffloadPath").GetTypedValue(m_offloadPath)
                        OffloadCount = .Item("OffloadCount").GetTypedValue(m_offloadCount)
                        OffloadThreshold = .Item("OffloadThreshold").GetTypedValue(m_offloadThreshold)
                        CompressPoints = .Item("CompressPoints").GetTypedValue(m_compressPoints)
                        DiscardOutOfSequencePoints = .Item("DiscardOutOfSequencePoints").GetTypedValue(m_discardOutOfSequencePoints)
                    End If
                End With
            Catch ex As Exception
                ' We'll encounter exceptions if the settings are not present in the config file.
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
                        With .Item("Type", True)
                            .Value = m_type.ToString()
                            .Description = "Type (Active; Standby; Historic) of the file."
                        End With
                        With .Item("Size", True)
                            .Value = m_size.ToString()
                            .Description = "Initial size of the file in MB."
                        End With
                        With .Item("BlockSize", True)
                            .Value = m_blockSize.ToString()
                            .Description = "Size of the data blocks in the file."
                        End With
                        With .Item("SaveOnClose", True)
                            .Value = m_saveOnClose.ToString()
                            .Description = "True if file is to be saved when closed; otherwise False."
                        End With
                        With .Item("RolloverOnFull", True)
                            .Value = m_rolloverOnFull.ToString()
                            .Description = "True if rollover of the file is to be performed when it is full; otherwise False."
                        End With
                        With .Item("RolloverPreparationThreshold", True)
                            .Value = m_rolloverPreparationThreshold.ToString()
                            .Description = "Percentage file full when the rollover preparation should begin."
                        End With
                        With .Item("OffloadPath", True)
                            .Value = m_offloadPath
                            .Description = "Path to the location where historic files are to be moved when disk start getting full."
                        End With
                        With .Item("OffloadCount", True)
                            .Value = m_offloadCount.ToString()
                            .Description = "Number of files that are to be moved to the offload location when the disk starts getting full."
                        End With
                        With .Item("OffloadThreshold", True)
                            .Value = m_offloadThreshold.ToString()
                            .Description = "Percentage disk full when the historic files should be moved to the offload location."
                        End With
                        With .Item("CompressPoints", True)
                            .Value = m_compressPoints.ToString()
                            .Description = "True if compression is to be performed on the points; otherwise False."
                        End With
                        With .Item("DiscardOutOfSequencePoints", True)
                            .Value = m_discardOutOfSequencePoints.ToString()
                            .Description = "True if out-of-sequence points are to be discarded; otherwise False."
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

        Private ReadOnly Property StandbyArchiveFileName() As String
            Get
                Return Path.ChangeExtension(m_name, StandbyFileExtension)
            End Get
        End Property

        Private ReadOnly Property HistoryArchiveFileName() As String
            Get
                Return JustPath(m_name) & (NoFileExtension(m_name) & "_" & m_fat.FileStartTime.ToString() & "_to_" & m_fat.FileEndTime.ToString() & Extension).Replace(":"c, "!"c)
            End Get
        End Property

        Private ReadOnly Property HistoricFilesSearchPattern() As String
            Get
                Return NoFileExtension(m_name) & "_*_to_*" & Extension
            End Get
        End Property

        Private Sub BuildHistoricFileList()

            Try
                m_historicArchiveFileList = New List(Of ArchiveFileInfo)()

                RaiseEvent HistoricFileListBuildStart(Me, EventArgs.Empty)

                ' We can safely assume that we'll always get information about the historic file because, the
                ' the search pattern ensures that we only can a list of historic archive files and not all files.
                Dim historicFileInfo As ArchiveFileInfo = Nothing
                SyncLock m_historicArchiveFileList
                    ' Prevent the historic file list from being updated by the file watchers.
                    For Each historicFileName As String In Directory.GetFiles(JustPath(m_name), HistoricFilesSearchPattern)
                        historicFileInfo = GetHistoricFileInfo(historicFileName)
                        If historicFileInfo IsNot Nothing Then m_historicArchiveFileList.Add(historicFileInfo)
                    Next

                    If Not String.IsNullOrEmpty(m_offloadPath) Then
                        For Each historicFileName As String In Directory.GetFiles(m_offloadPath, HistoricFilesSearchPattern)
                            historicFileInfo = GetHistoricFileInfo(historicFileName)
                            If historicFileInfo IsNot Nothing Then m_historicArchiveFileList.Add(historicFileInfo)
                        Next
                    End If
                End SyncLock

                RaiseEvent HistoricFileListBuildComplete(Me, EventArgs.Empty)
            Catch ex As ThreadAbortException
                ' This thread must die now...
            Catch ex As Exception
                RaiseEvent HistoricFileListBuildException(Me, New GenericEventArgs(Of Exception)(ex))
            End Try

        End Sub

        Private Sub PrepareForRollover()

            Try
                Dim archiveDrive As New DriveInfo(Path.GetPathRoot(m_name))
                If archiveDrive.AvailableFreeSpace < archiveDrive.TotalSize * (1 - (m_offloadThreshold / 100)) Then
                    ' We'll start offloading historic files if we've reached the offload threshold.
                    OffloadHistoricFiles()
                End If

                RaiseEvent RolloverPreparationStart(Me, EventArgs.Empty)

                ' Opening and closing a new archive file in "standby" mode will create a "standby" archive file.
                Dim standbyArchiveFile As New ArchiveFile()
                standbyArchiveFile.Name = m_name
                standbyArchiveFile.Type = ArchiveFileType.Standby
                standbyArchiveFile.Size = m_size
                standbyArchiveFile.BlockSize = m_blockSize
                standbyArchiveFile.StateFile = m_stateFile
                standbyArchiveFile.IntercomFile = m_intercomFile
                Try
                    standbyArchiveFile.Open()
                    standbyArchiveFile.Close()
                    standbyArchiveFile = Nothing
                Catch ex As Exception
                    Dim standbyFileName As String = standbyArchiveFile.Name
                    standbyArchiveFile.Close()
                    standbyArchiveFile = Nothing
                    ' We didn't succeed in creating a "standby" archive file, so we'll delete it if it was created
                    ' partially (might happen if there isn't enough disk space or thread is aborted). This is to 
                    ' ensure that this preparation processes is kicked off again until a valid "standby" archive 
                    ' file is successfully created.
                    If File.Exists(standbyFileName) Then File.Delete(standbyFileName)

                    Throw ' Rethrow the exception so the appropriate action is taken.
                End Try

                RaiseEvent RolloverPreparationComplete(Me, EventArgs.Empty)
            Catch ex As ThreadAbortException
                ' This thread must die now...
            Catch ex As Exception
                RaiseEvent RolloverPreparationException(Me, New GenericEventArgs(Of Exception)(ex))
            End Try

        End Sub

        Private Sub OffloadHistoricFiles()

            If Directory.Exists(m_offloadPath) Then
                If m_buildHistoricFileListThread.IsAlive Then
                    ' Wait until the historic file list has been built.
                    m_buildHistoricFileListThread.Join()
                End If

                Try
                    RaiseEvent OffloadStart(Me, EventArgs.Empty)

                    ' The offload path that is specified is a valid one so we'll gather a list of all historic
                    ' files in the directory where the current (active) archive file is located.
                    Dim newHistoricFiles As List(Of ArchiveFileInfo) = Nothing
                    SyncLock m_historicArchiveFileList
                        newHistoricFiles = m_historicArchiveFileList.FindAll(AddressOf IsNewHistoricArchiveFile)
                    End SyncLock

                    ' Sorting the list will sort the historic files from oldest to newest.
                    newHistoricFiles.Sort()

                    ' We'll offload the specified number of oldest historic files to the offload location if the 
                    ' number of historic files is more than the offload count or all of the historic files if the 
                    ' offload count is smaller the available number of historic files.
                    Dim offloadProgress As New ProcessProgress(Of Integer)("FileOffload")
                    offloadProgress.Total = IIf(newHistoricFiles.Count < m_offloadCount, newHistoricFiles.Count, m_offloadCount)
                    For i As Integer = 0 To offloadProgress.Total - 1
                        offloadProgress.ProgressMessage = JustFileName(newHistoricFiles(i).FileName)

                        RaiseEvent OffloadProgress(Me, New GenericEventArgs(Of ProcessProgress(Of Integer))(offloadProgress))

                        Dim destinationFileName As String = AddPathSuffix(m_offloadPath) & JustFileName(newHistoricFiles(i).FileName)
                        If File.Exists(destinationFileName) Then
                            ' Delete the destination file is it already exists.
                            File.Delete(destinationFileName)
                        End If

                        File.Move(newHistoricFiles(i).FileName, destinationFileName)

                        offloadProgress.Complete += 1
                    Next

                    RaiseEvent OffloadComplete(Me, EventArgs.Empty)
                Catch ex As ThreadAbortException
                    Throw ' Bubble up the ThreadAbortException.
                Catch ex As Exception
                    RaiseEvent OffloadException(Me, New GenericEventArgs(Of Exception)(ex))
                End Try
            End If

        End Sub

        Private Function GetHistoricFileInfo(ByVal fileName As String) As ArchiveFileInfo

            Dim fileInfo As ArchiveFileInfo = Nothing

            Try
                If File.Exists(fileName) Then
                    ' We'll open the file and get relevant information about it.
                    Dim historicArchiveFile As New ArchiveFile()
                    historicArchiveFile.Name = fileName
                    historicArchiveFile.Type = ArchiveFileType.Historic
                    historicArchiveFile.SaveOnClose = False
                    historicArchiveFile.StateFile = m_stateFile
                    historicArchiveFile.IntercomFile = m_intercomFile
                    Try
                        historicArchiveFile.Open()
                        fileInfo = New ArchiveFileInfo()
                        fileInfo.FileName = fileName
                        fileInfo.StartTimeTag = historicArchiveFile.FileAllocationTable.FileStartTime
                        fileInfo.EndTimeTag = historicArchiveFile.FileAllocationTable.FileEndTime
                    Catch ex As Exception

                    Finally
                        historicArchiveFile.Close()
                        historicArchiveFile = Nothing
                    End Try
                Else
                    ' We'll resolve to getting the file information from its name only if the file no longer exists
                    ' at the location. This will be the case when file is moved to a different location. In this
                    ' case the file information we provide is only as good as the file name.
                    Dim datesString As String = NoFileExtension(fileName).Substring((NoFileExtension(m_name) & "_").Length)
                    Dim fileStartEndDates As String() = datesString.Split(New String() {"_to_"}, StringSplitOptions.None)

                    fileInfo = New ArchiveFileInfo()
                    fileInfo.FileName = fileName
                    If fileStartEndDates.Length = 2 Then
                        fileInfo.StartTimeTag = New TimeTag(Convert.ToDateTime(fileStartEndDates(0).Replace("!"c, ":"c)))
                        fileInfo.EndTimeTag = New TimeTag(Convert.ToDateTime(fileStartEndDates(1).Replace("!"c, ":"c)))
                    End If
                End If
            Catch ex As Exception

            End Try

            Return fileInfo

        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pointData"></param>
        ''' <param name="pointState"></param>
        ''' <returns>True if the point data fails compression test and is to be archived; otherwise False.</returns>
        Private Function ToBeArchived(ByRef pointData As StandardPointData, ByVal pointState As PointState) As Boolean

            ' TODO: Validate compression logic with Brian Fox

            Dim result As Boolean = False
            Dim calculateSlopes As Boolean = False
            Dim compressionLimit As Single = pointData.Definition.AnalogFields.CompressionLimit

            If pointData.Definition.GeneralFlags.PointType = PointType.Digital Then compressionLimit = 0.000000001

            If pointData.Quality = 31 Then
                ' We have to check the quality of this data since the sender didn't provide it. Here we're 
                ' checking if the Quality is 31 instead of -1 because the quality value is stored in the first
                ' 5 bits (QualityMask = 31) of Flags in the point data. Initially when the Quality is set to -1,
                ' all the bits Flags (a 32-bit integer) are set to 1. And therefore, when we get the Quality, 
                ' which is a masked value of Flags, we get 31 and not -1.
                Select Case pointData.Definition.GeneralFlags.PointType
                    Case PointType.Analog
                        Select Case pointData.Value
                            Case Is >= pointData.Definition.AnalogFields.HighRange
                                pointData.Quality = Quality.UnreasonableHigh
                            Case Is >= pointData.Definition.AnalogFields.HighAlarm
                                pointData.Quality = Quality.ValueAboveHiHiAlarm
                            Case Is >= pointData.Definition.AnalogFields.HighWarning
                                pointData.Quality = Quality.ValueAboveHiAlarm
                            Case Is >= pointData.Definition.AnalogFields.LowRange
                                pointData.Quality = Quality.UnreasonableLow
                            Case Is >= pointData.Definition.AnalogFields.LowAlarm
                                pointData.Quality = Quality.ValueBelowLoLoAlarm
                            Case Is >= pointData.Definition.AnalogFields.LowWarning
                                pointData.Quality = Quality.ValueBelowLoAlarm
                            Case Else
                                pointData.Quality = Quality.Good
                        End Select
                    Case PointType.Digital
                        Select Case pointData.Value
                            Case pointData.Definition.DigitalFields.AlarmState
                                pointData.Quality = Quality.LogicalAlarm
                            Case Else
                                pointData.Quality = Quality.Good
                        End Select
                End Select
            End If

            With pointState
                .CurrentValue = pointData.ToExtended()

                ' Update the environment data that is periodically written to the Intercom File.
                m_environmentData.LastestCurrentValueTimeTag = .CurrentValue.TimeTag
                m_environmentData.LatestCurrentValuePointID = .CurrentValue.Definition.PointID

                If .LastArchivedValue.IsEmpty Then
                    ' This is the first time data is received for the point.
                    .LastArchivedValue = .CurrentValue
                    Return True
                ElseIf .PreviousValue.IsEmpty Then
                    ' This is the second time data is received for the point.
                    calculateSlopes = True
                Else
                    ' TODO: Notify on change in quality
                    If m_compressPoints Then
                        ' Perform compression check.
                        If .CurrentValue.Definition.CompressionMinimumTime > 0 AndAlso _
                                .CurrentValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value < .CurrentValue.Definition.CompressionMinimumTime Then
                            result = False
                        ElseIf .CurrentValue.Quality <> .LastArchivedValue.Quality OrElse _
                                .CurrentValue.Quality <> .PreviousValue.Quality OrElse _
                                .PreviousValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value > .CurrentValue.Definition.CompressionMaximumTime Then
                            result = True
                            calculateSlopes = True
                        Else
                            Dim slope1 As Double
                            Dim slope2 As Double
                            Dim currentSlope As Double

                            slope1 = (.CurrentValue.Value - (.LastArchivedValue.Value + compressionLimit)) / _
                                        (.CurrentValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value)
                            slope2 = (.CurrentValue.Value - (.LastArchivedValue.Value - compressionLimit)) / _
                                        (.CurrentValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value)
                            currentSlope = (.CurrentValue.Value - .LastArchivedValue.Value) / _
                                        (.CurrentValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value)

                            If slope1 >= .Slope1 Then .Slope1 = slope1
                            If slope2 <= .Slope2 Then .Slope2 = slope2
                            If currentSlope <= .Slope1 OrElse currentSlope >= .Slope2 Then
                                result = True
                                calculateSlopes = True
                            End If
                        End If
                    Else
                        ' No compression check required.
                        result = True
                    End If
                End If

                If result Then
                    .LastArchivedValue = .PreviousValue
                    pointData = .LastArchivedValue.ToStandard()
                End If

                If calculateSlopes Then
                    If .CurrentValue.TimeTag.Value <> .LastArchivedValue.TimeTag.Value Then
                        .Slope1 = (.CurrentValue.Value - (.LastArchivedValue.Value + compressionLimit)) / _
                                    (.CurrentValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value)
                        .Slope2 = (.CurrentValue.Value - (.LastArchivedValue.Value - compressionLimit)) / _
                                    (.CurrentValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value)
                    Else
                        .Slope1 = 0
                        .Slope2 = 0
                    End If
                End If

                .PreviousValue = .CurrentValue
            End With

            Return result

        End Function

#Region " Find Predicates "

        Private Function FindHistoricArchiveFile(ByVal fileInfo As ArchiveFileInfo) As Boolean

            Return fileInfo IsNot Nothing And _
                m_searchTimeTag.CompareTo(fileInfo.StartTimeTag) >= 0 And _
                m_searchTimeTag.CompareTo(fileInfo.EndTimeTag) <= 0

        End Function

        Private Function IsNewHistoricArchiveFile(ByVal fileInfo As ArchiveFileInfo) As Boolean

            Return fileInfo IsNot Nothing And _
                String.Compare(JustPath(m_name), JustPath(fileInfo.FileName), True) = 0

        End Function

        Private Function IsOldHistoricArchiveFile(ByVal fileInfo As ArchiveFileInfo) As Boolean

            Return fileInfo IsNot Nothing And _
                Not String.IsNullOrEmpty(m_offloadPath) And _
                String.Compare(JustPath(m_offloadPath), JustPath(fileInfo.FileName), True) = 0

        End Function

#End Region

#Region " Queue Delegates "

        Public Sub WriteToHistoricArchiveFile(ByVal items As StandardPointData())

            If m_buildHistoricFileListThread.IsAlive Then
                ' Wait until the historic file list has been built.
                m_buildHistoricFileListThread.Join()
            End If

            RaiseEvent HistoricPointsWriteStart(Me, EventArgs.Empty)

            Dim sortedPointData As New Dictionary(Of Integer, List(Of StandardPointData))()
            ' First we'll seperate all point data by ID.
            For i As Integer = 0 To items.Length - 1
                If items(i).Definition IsNot Nothing Then
                    ' We only process point data that has an associated definition.
                    If Not sortedPointData.ContainsKey(items(i).Definition.PointID) Then
                        sortedPointData.Add(items(i).Definition.PointID, New List(Of StandardPointData)())
                    End If

                    sortedPointData(items(i).Definition.PointID).Add(items(i))
                End If
            Next

            Dim historicWriteProgress As New ProcessProgress(Of Integer)("HistoricWrite")
            historicWriteProgress.Total = sortedPointData.Count
            For Each pointID As Integer In sortedPointData.Keys
                ' We'll sort the point data for the current point ID by time.
                sortedPointData(pointID).Sort()

                Dim writeData As Boolean = False
                Dim pointData As New List(Of StandardPointData)()
                Dim historicFileInfo As ArchiveFileInfo = Nothing
                For i As Integer = 0 To sortedPointData(pointID).Count - 1
                    If historicFileInfo Is Nothing Then
                        ' We'll try to find a historic file when the current point data belongs.
                        m_searchTimeTag = sortedPointData(pointID)(i).TimeTag
                        SyncLock m_historicArchiveFileList
                            historicFileInfo = m_historicArchiveFileList.Find(AddressOf FindHistoricArchiveFile)
                        End SyncLock
                    End If

                    If historicFileInfo IsNot Nothing Then
                        If sortedPointData(pointID)(i).TimeTag.CompareTo(historicFileInfo.StartTimeTag) >= 0 AndAlso _
                                sortedPointData(pointID)(i).TimeTag.CompareTo(historicFileInfo.EndTimeTag) <= 0 Then
                            ' The current point data belongs to the current historic archive file.
                            pointData.Add(sortedPointData(pointID)(i))
                        Else
                            ' The current point data doesn't belong to the current historic archive file, so we have
                            ' to write all the point data we have so far for the current historic archive file to it.
                            i -= 1
                            writeData = True
                        End If

                        ' This is last point data for the current point ID, so we must write all the point data so
                        ' far to the current historic archive file.
                        If i = sortedPointData(pointID).Count - 1 Then writeData = True

                        If writeData AndAlso pointData.Count > 0 Then
                            ' Before we start writing data to a historic file, we make sure that rollover is not in
                            ' progress. Not sure if this is necessary, but trying to be safe than sorry!
                            m_rolloverWaitHandle.WaitOne()

                            Dim historicArchiveFile As New ArchiveFile()
                            historicArchiveFile.Name = historicFileInfo.FileName
                            historicArchiveFile.Type = ArchiveFileType.Historic
                            historicArchiveFile.CompressPoints = m_compressPoints
                            historicArchiveFile.DiscardOutOfSequencePoints = m_discardOutOfSequencePoints
                            historicArchiveFile.StateFile = m_stateFile
                            historicArchiveFile.IntercomFile = m_intercomFile
                            Try
                                historicWriteProgress.ProgressMessage = pointID.ToString()

                                RaiseEvent HistoricPointsWriteProgress(Me, New GenericEventArgs(Of ProcessProgress(Of Integer))(historicWriteProgress))

                                ' Write all data to the historic file in which it belongs.
                                historicArchiveFile.Open()
                                historicArchiveFile.Write(pointData.ToArray())

                                historicWriteProgress.Complete += 1
                            Catch ex As Exception

                            Finally
                                historicArchiveFile.Close()
                                historicArchiveFile = Nothing
                            End Try

                            writeData = False
                            pointData.Clear()
                            historicFileInfo = Nothing
                        End If
                    End If
                Next
            Next

            RaiseEvent HistoricPointsWriteComplete(Me, EventArgs.Empty)

        End Sub

        Public Sub InsertInCurrentArchiveFile(ByVal items As StandardPointData())

        End Sub

#End Region

#Region " Event Handlers "

#Region " m_historicDataQueue "

        Private Sub m_historicDataQueue_ProcessException(ByVal ex As System.Exception) Handles m_historicDataQueue.ProcessException

            RaiseEvent HistoricPointsWriteException(Me, New GenericEventArgs(Of Exception)(ex))

        End Sub

#End Region

#Region " m_outOfSequenceDataQueue "

        Private Sub m_outOfSequenceDataQueue_ProcessException(ByVal ex As System.Exception) Handles m_outOfSequenceDataQueue.ProcessException

            RaiseEvent OutOfSequencePointsWriteException(Me, New GenericEventArgs(Of Exception)(ex))

        End Sub

#End Region

#Region " ArchiveFile "

        Private Sub ArchiveFile_FileFull(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.FileFull

            If m_rolloverOnFull Then
                Do While True
                    Rollover()  ' Start the rollover process.
                    If m_rolloverWaitHandle.WaitOne(1, False) Then Exit Do ' Rollover is successful.
                Loop
            End If

        End Sub

#End Region

#Region " FileSystemWatcher "

        Private Sub CurrentLocationFileSystemWatcher_Created(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles CurrentLocationFileSystemWatcher.Created, OffloadLocationFileSystemWatcher.Created

            If IsOpen Then
                ' Attempt to update the historic file list only if the current file is open.
                Dim historicFileListUpdated As Boolean = False
                Dim historicFileInfo As ArchiveFileInfo = GetHistoricFileInfo(e.FullPath)
                SyncLock m_historicArchiveFileList
                    If historicFileInfo IsNot Nothing AndAlso Not m_historicArchiveFileList.Contains(historicFileInfo) Then
                        m_historicArchiveFileList.Add(historicFileInfo)
                        historicFileListUpdated = True
                    End If
                End SyncLock
                If historicFileListUpdated Then RaiseEvent HistoricFileListUpdated(Me, EventArgs.Empty)
            End If

        End Sub

        Private Sub CurrentLocationFileSystemWatcher_Deleted(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles CurrentLocationFileSystemWatcher.Deleted, OffloadLocationFileSystemWatcher.Deleted

            If IsOpen Then
                ' Attempt to update the historic file list only if the current file is open.
                Dim historicFileListUpdated As Boolean = False
                Dim historicFileInfo As ArchiveFileInfo = GetHistoricFileInfo(e.FullPath)
                SyncLock m_historicArchiveFileList
                    If historicFileInfo IsNot Nothing AndAlso m_historicArchiveFileList.Contains(historicFileInfo) Then
                        m_historicArchiveFileList.Remove(historicFileInfo)
                        historicFileListUpdated = True
                    End If
                End SyncLock
                If historicFileListUpdated Then RaiseEvent HistoricFileListUpdated(Me, EventArgs.Empty)
            End If

        End Sub

        Private Sub CurrentLocationFileSystemWatcher_Renamed(ByVal sender As Object, ByVal e As System.IO.RenamedEventArgs) Handles CurrentLocationFileSystemWatcher.Renamed, OffloadLocationFileSystemWatcher.Renamed

            If IsOpen Then
                ' Attempt to update the historic file list only if the current file is open.
                If String.Compare(JustFileExtension(e.OldFullPath), Extension, True) = 0 Then
                    Try
                        Dim historicFileListUpdated As Boolean = False
                        Dim oldFileInfo As ArchiveFileInfo = GetHistoricFileInfo(e.OldFullPath)
                        SyncLock m_historicArchiveFileList
                            If oldFileInfo IsNot Nothing AndAlso m_historicArchiveFileList.Contains(oldFileInfo) Then
                                m_historicArchiveFileList.Remove(oldFileInfo)
                                historicFileListUpdated = True
                            End If
                        End SyncLock
                        If historicFileListUpdated Then RaiseEvent HistoricFileListUpdated(Me, EventArgs.Empty)
                    Catch ex As Exception
                        ' Ignore any exception we might encounter here if an archive file being renamed to a 
                        ' historic archive file. This might happen if someone is renaming files manually.
                    End Try
                End If

                If String.Compare(JustFileExtension(e.FullPath), Extension, True) = 0 Then
                    Try
                        Dim historicFileListUpdated As Boolean = False
                        Dim newFileInfo As ArchiveFileInfo = GetHistoricFileInfo(e.FullPath)
                        SyncLock m_historicArchiveFileList
                            If newFileInfo IsNot Nothing AndAlso Not m_historicArchiveFileList.Contains(newFileInfo) Then
                                m_historicArchiveFileList.Add(newFileInfo)
                                historicFileListUpdated = True
                            End If
                        End SyncLock
                        If historicFileListUpdated Then RaiseEvent HistoricFileListUpdated(Me, EventArgs.Empty)
                    Catch ex As Exception
                        ' Ignore any exception we might encounter if a historic archive file is being renamed to 
                        ' something else. This might happen if someone is renaming files manually.
                    End Try
                End If
            End If

        End Sub

#End Region

#End Region

#Region " Classes "

        ''' <summary>
        ''' Represents information about an Archive File.
        ''' </summary>
        Public Class ArchiveFileInfo
            Implements IComparable

            Public FileName As String

            Public StartTimeTag As TimeTag

            Public EndTimeTag As TimeTag

            Public Overrides Function Equals(ByVal obj As Object) As Boolean

                Dim other As ArchiveFileInfo = TryCast(obj, ArchiveFileInfo)
                If other IsNot Nothing Then
                    ' We will only compare file name for equality because the result will be incorrent if one of 
                    ' the ArchiveFileInfo instance is created from the filename by GetHistoricFileInfo() function.
                    Return String.Compare(JustFileName(FileName), JustFileName(other.FileName), True) = 0
                End If

            End Function

            Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

                Dim other As ArchiveFileInfo = TryCast(obj, ArchiveFileInfo)
                If other IsNot Nothing Then
                    Dim startTTagCompare As Integer = StartTimeTag.CompareTo(other.StartTimeTag)
                    Return IIf(startTTagCompare = 0, EndTimeTag.CompareTo(other.EndTimeTag), startTTagCompare)
                Else
                    Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
                End If

            End Function

        End Class

#End Region

#End Region

    End Class

End Namespace