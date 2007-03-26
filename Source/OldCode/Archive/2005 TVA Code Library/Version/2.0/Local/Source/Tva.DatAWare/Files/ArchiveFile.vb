' 02/18/2007

Imports System.IO
Imports System.Drawing
Imports System.Threading
Imports System.ComponentModel
Imports Tva.Collections
Imports Tva.IO.FilePath
Imports Tva.Configuration.Common

Namespace Files

    <ToolboxBitmap(GetType(ArchiveFile))> _
    Public Class ArchiveFile
        Implements ISupportInitialize, IPersistsSettings

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
        Private m_compressData As Boolean
        Private m_discardOoSData As Boolean
        Private m_stateFile As StateFile
        Private m_intercomFile As IntercomFile
        Private m_fat As ArchiveFileAllocationTable
        Private m_fileStream As FileStream
        Private m_historicArchiveFileList As List(Of ArchiveFileInfo)
        Private m_rolloverPreparationThread As Thread
        Private m_buildHistoricFileListThread As Thread
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
        Public Event OffloadStart As EventHandler
        Public Event OffloadComplete As EventHandler
        Public Event OffloadException As EventHandler(Of ExceptionEventArgs)
        Public Event RolloverPreparationStart As EventHandler
        Public Event RolloverPreparationComplete As EventHandler
        Public Event RolloverPreparationException As EventHandler(Of ExceptionEventArgs)
        Public Event HistoricFileListBuildStart As EventHandler
        Public Event HistoricFileListBuildComplete As EventHandler
        Public Event HistoricFileListBuildException As EventHandler(Of ExceptionEventArgs)
        Public Event HistoricFileListUpdated As EventHandler

        Public Event CurrentDataReceived As EventHandler
        Public Event CurrentDataWritten As EventHandler
        Public Event CurrentDataDiscarded As EventHandler
        Public Event HistoricDataReceived As EventHandler
        Public Event OutOfSequenceDataReceived As EventHandler

#End Region

#Region " Public Code "

        Public Const Extension As String = ".d"

        Public Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    If String.Compare(JustFileExtension(value), Extension) = 0 Then
                        m_name = value
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

        Public Property CompressData() As Boolean
            Get
                Return m_compressData
            End Get
            Set(ByVal value As Boolean)
                m_compressData = value
            End Set
        End Property

        Public Property DiscardOoSData() As Boolean
            Get
                Return m_discardOoSData
            End Get
            Set(ByVal value As Boolean)
                m_discardOoSData = value
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

                    ' We can safely mark the file as historic if it doesn't have space for data unless extended.
                    If m_fat.DataBlocksAvailable = 0 Then m_type = ArchiveFileType.Historic

                    If m_type = ArchiveFileType.Active Then
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
                            m_fat.FileStartTime = m_intercomFile.Records(0).LastestCurrentValueTimeTag
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

                m_fat.Dispose()
                m_fat = Nothing
                m_fileStream.Dispose()
                m_fileStream = Nothing
                m_rolloverPreparationThread.Abort()
                m_buildHistoricFileListThread.Abort()
                m_historicDataQueue.Stop()
                m_outOfSequenceDataQueue.Stop()

                If m_type = ArchiveFileType.Active Then
                    m_historicArchiveFileList.Clear()
                    m_historicArchiveFileList = Nothing
                    CurrentLocationFileSystemWatcher.EnableRaisingEvents = False
                    OffloadLocationFileSystemWatcher.EnableRaisingEvents = False
                End If

                If m_type <> ArchiveFileType.Standby AndAlso m_stateFile.IsOpen Then
                    ' The archive file is open multiple times (by ArchiveDataBlock) only when data is being
                    ' written to the file. In case the current file is for "standby" purpose, no data will be 
                    ' written to it and therefore, the file will not be opened multiple time in "standby" mode.
                    For i As Integer = 0 To m_stateFile.Records.Count - 1
                        ' We'll release all the data blocks that were being used by the file.
                        If m_stateFile.Records(i).ActiveDataBlock IsNot Nothing AndAlso _
                                (m_type = ArchiveFileType.Active OrElse (m_type = ArchiveFileType.Historic AndAlso m_stateFile.Records(i).ActiveDataBlock.IsForHistoricData)) Then
                            ' We'll deallocate the data block if the current file is "active" or if it is "historic" 
                            ' and the block was used to append historical data into it.
                            m_stateFile.Records(i).ActiveDataBlock.Dispose()
                            m_stateFile.Records(i).ActiveDataBlock = Nothing
                        End If
                    Next
                End If

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

            If m_type = ArchiveFileType.Active AndAlso File.Exists(StandbyArchiveFileName) Then
                RaiseEvent RolloverStart(Me, EventArgs.Empty)

                Dim historyFile As String = HistoryArchiveFileName
                Dim standbyFile As String = StandbyArchiveFileName

                ' Signal the server that we're are performing rollover so it must let go of this file.
                m_intercomFile.Records(0).RolloverInProgress = True
                m_intercomFile.Save()
                Close()

                WaitForWriteLock(m_name)        ' Wait for the server to release the file.

                File.Move(m_name, historyFile)  ' Make the active archive file, historic archive file.
                File.Move(standbyFile, m_name)  ' Make the standby archive file, active archive file.

                ' We're now done with the rollover process, so we must inform the server of this.
                Open()
                m_intercomFile.Records(0).RolloverInProgress = False
                m_intercomFile.Save()

                RaiseEvent RolloverComplete(Me, EventArgs.Empty)
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

        Public Sub Write(ByVal pointData As StandardPointData)

            If IsOpen Then
                ' We don't allow data to be written to a "standby" file.
                If m_type = ArchiveFileType.Standby Then Exit Sub

                m_fat.EventsReceived += 1

                If pointData.TimeTag.CompareTo(m_fat.FileStartTime) >= 0 Then
                    ' The data belongs to this file.
                    Dim pointState As PointState = m_stateFile.Read(pointData.Definition.PointID)
                    If pointData.TimeTag.CompareTo(pointState.LastArchivedValue.TimeTag) >= 0 Then
                        ' The data is in sequence.

                        ' Data passed compression test - don't write it.
                        If Not ToBeArchived(pointData, pointState) Then Exit Sub

                        ' Data failed compression test - write it to current file.
                        If pointState.ActiveDataBlock Is Nothing OrElse _
                                (pointState.ActiveDataBlock IsNot Nothing AndAlso pointState.ActiveDataBlock.SlotsAvailable <= 0) Then
                            ' We either don't have a active data block where we can archive the point data or   
                            ' we have a active data block but it is full. So, we have to request a new data block 
                            ' from the FAT in order to write the data.

                            If pointState.ActiveDataBlock IsNot Nothing Then
                                ' We must release the previously used data block before we request a new one.
                                pointState.ActiveDataBlock.Dispose()
                            End If

                            Select Case m_type
                                Case ArchiveFileType.Active
                                    pointState.ActiveDataBlock = m_fat.RequestDataBlock(pointData.Definition.PointID, pointData.TimeTag)

                                    If m_fat.DataBlocksAvailable < m_fat.DataBlockCount * (1 - (m_rolloverPreparationThreshold / 100)) AndAlso _
                                            Not File.Exists(StandbyArchiveFileName) AndAlso Not m_rolloverPreparationThread.IsAlive Then
                                        ' We've requested the specified percent of the total number of data 
                                        ' blocks in the file, so we must now prepare for the rollover process 
                                        ' since it has not been done yet and it is not already in progress.
                                        m_rolloverPreparationThread = New Thread(AddressOf PrepareForRollover)
                                        m_rolloverPreparationThread.Priority = ThreadPriority.Lowest
                                        m_rolloverPreparationThread.Start()
                                    End If
                                Case ArchiveFileType.Historic
                                    pointState.ActiveDataBlock = m_fat.RequestDataBlock(pointData.Definition.PointID, pointData.TimeTag, True)
                            End Select
                        End If

                        If pointState.ActiveDataBlock IsNot Nothing Then
                            ' We were able to obtain a data block for writing data.
                            If m_type = ArchiveFileType.Active OrElse _
                                    (m_type = ArchiveFileType.Historic AndAlso pointState.ActiveDataBlock.IsForHistoricData) Then
                                pointState.ActiveDataBlock.Write(pointData)
                                m_fat.EventsArchived += 1
                            End If

                            If m_type = ArchiveFileType.Active Then m_fat.FileEndTime = pointData.TimeTag
                            If m_fat.FileStartTime.CompareTo(TimeTag.MinValue) = 0 Then m_fat.FileStartTime = pointData.TimeTag
                        Else
                            ' We were unable to obtain a data block for writing data to because all data block are in use.
                            RaiseEvent FileFull(Me, EventArgs.Empty)
                        End If
                    Else
                        ' The data is in out-of-sequence.
                        If Not m_discardOoSData Then
                            ' Insert the data into the current file.
                            m_outOfSequenceDataQueue.Add(pointData)
                        End If
                    End If
                Else
                    ' The data belongs to a historic archive file.
                    If m_type = ArchiveFileType.Active Then
                        m_historicDataQueue.Add(pointData)
                    End If
                End If
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Sub Write(ByVal pointData() As StandardPointData)

            If IsOpen Then
                For i As Integer = 0 To pointData.Length - 1
                    Write(pointData(i))
                Next
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Sub HistoricWrite(ByVal pointData() As StandardPointData)



        End Sub

        Public Shared Function MaximumDataBlocks(ByVal fileSize As Double, ByVal blockSize As Integer) As Integer

            Return Convert.ToInt32((fileSize * 1024) / blockSize)

        End Function

#Region " Interface Implementations "

#Region " ISupportInitialize "

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            If Not DesignMode Then
                LoadSettings()  ' Load settings from the config file.
            End If

        End Sub

#End Region

#Region " IPersistsSettings "

        Private m_persistSettings As Boolean
        Private m_configurationCategory As String

        Public Property PersistSettings() As Boolean Implements IPersistsSettings.PersistSettings
            Get
                Return m_persistSettings
            End Get
            Set(ByVal value As Boolean)
                m_persistSettings = value
            End Set
        End Property

        Public Property ConfigurationCategory() As String Implements IPersistsSettings.ConfigurationCategory
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

        Public Sub LoadSettings() Implements IPersistsSettings.LoadSettings

            If m_persistSettings Then
                Try
                    With CategorizedSettings(m_configurationCategory)
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
                        CompressData = .Item("CompressData").GetTypedValue(m_compressData)
                        DiscardOoSData = .Item("DiscardOoSData").GetTypedValue(m_discardOoSData)
                    End With
                Catch ex As Exception
                    ' We'll encounter exceptions if the settings are not present in the config file.
                End Try
            End If

        End Sub

        Public Sub SaveSettings() Implements IPersistsSettings.SaveSettings

            If m_persistSettings Then
                Try
                    With CategorizedSettings(m_configurationCategory)
                        .Clear()
                        With .Item("Name", True)
                            .Value = m_name
                            .Description = "Name of the file including its path."
                        End With
                        With .Item("Type", True)
                            .Value = m_type.ToString()
                            .Description = "Type of the file (Active; Standby; Historic)."
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
                        With .Item("CompressData", True)
                            .Value = m_compressData.ToString()
                            .Description = "True if compression is to be performed on the data; otherwise False."
                        End With
                        With .Item("DiscardOoSData", True)
                            .Value = m_discardOoSData.ToString()
                            .Description = "True if out-of-sequence data is to be discarded; otherwise False."
                        End With
                    End With
                    Tva.Configuration.Common.SaveSettings()
                Catch ex As Exception
                    ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
                End Try
            End If

        End Sub

#End Region

#End Region

#End Region

#Region " Private Code "

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
                SyncLock m_historicDataQueue
                    ' Prevent the historic file list from being updated by the file watchers.
                    For Each historicFileName As String In Directory.GetFiles(JustPath(m_name), HistoricFilesSearchPattern)
                        m_historicArchiveFileList.Add(GetHistoricFileInfo(historicFileName))
                    Next

                    If Not String.IsNullOrEmpty(m_offloadPath) Then
                        For Each historicFileName As String In Directory.GetFiles(m_offloadPath, HistoricFilesSearchPattern)
                            m_historicArchiveFileList.Add(GetHistoricFileInfo(historicFileName))
                        Next
                    End If
                End SyncLock

                RaiseEvent HistoricFileListBuildComplete(Me, EventArgs.Empty)
            Catch ex As ThreadAbortException
                ' We can safely ignore this exception.
            Catch ex As Exception
                RaiseEvent HistoricFileListBuildException(Me, New ExceptionEventArgs(ex))
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

                ' Opening and closing a new archive file in "standby" mode will create a "standby" file.
                With New ArchiveFile()
                    .Name = m_name
                    .Type = ArchiveFileType.Standby
                    .Size = m_size
                    .BlockSize = m_blockSize
                    .StateFile = m_stateFile
                    .IntercomFile = m_intercomFile
                    .Open()
                    .Close()
                End With

                RaiseEvent RolloverPreparationComplete(Me, EventArgs.Empty)
            Catch ex As ThreadAbortException
                ' We can safely ignore this exception.
            Catch ex As Exception
                RaiseEvent RolloverPreparationException(Me, New ExceptionEventArgs(ex))
            End Try

        End Sub

        Private Sub OffloadHistoricFiles()


            RaiseEvent OffloadStart(Me, EventArgs.Empty)

            If m_buildHistoricFileListThread.IsAlive Then
                ' Wait until the historic file list has been built.
                m_buildHistoricFileListThread.Join()
            End If

            If Directory.Exists(m_offloadPath) Then
                ' The offload path that is specified is a valid one so we'll gather a list of all historic
                ' files in the directory where the current (active) archive file is located.
                Dim historicFiles As String() = Directory.GetFiles(JustPath(m_name), HistoricFilesSearchPattern)

                ' Sorting the list will sort the historic files from oldest to newest.
                Array.Sort(historicFiles)

                ' We'll offload the specified number of oldest historic files to the offload location if the 
                ' number of historic files is more than the offload count or all of the historic files if the 
                ' offload count is smaller the available number of historic files.
                For i As Integer = 0 To IIf(historicFiles.Length < m_offloadCount, historicFiles.Length, m_offloadCount) - 1
                    Try
                        Dim destinationFileName As String = AddPathSuffix(m_offloadPath) & JustFileName(historicFiles(i))
                        If File.Exists(destinationFileName) Then
                            ' Delete the destination file is it already exists.
                            File.Delete(destinationFileName)
                        End If

                        File.Move(historicFiles(i), destinationFileName)
                    Catch ex As ThreadAbortException
                        Throw
                    Catch ex As Exception
                        RaiseEvent OffloadException(Me, New ExceptionEventArgs(ex))
                    End Try
                Next
            End If

            RaiseEvent OffloadComplete(Me, EventArgs.Empty)

        End Sub

        Private Function GetHistoricFileInfo(ByVal fileName As String) As ArchiveFileInfo

            Dim fileInfo As ArchiveFileInfo = Nothing

            Try
                If File.Exists(fileName) Then
                    ' We'll open the file and get relevant information about it.
                    With New ArchiveFile()
                        .Name = fileName
                        .Type = ArchiveFileType.Historic
                        .SaveOnClose = False
                        .StateFile = m_stateFile
                        .IntercomFile = m_intercomFile
                        Try
                            .Open()
                            fileInfo = New ArchiveFileInfo()
                            fileInfo.FileName = fileName
                            fileInfo.StartTimeTag = .FileAllocationTable.FileStartTime
                            fileInfo.EndTimeTag = .FileAllocationTable.FileEndTime
                        Catch ex As Exception

                        Finally
                            .Close()
                        End Try
                    End With
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

            If pointData.Definition IsNot Nothing Then
                Dim calculateSlopes As Boolean = False
                Dim compressionLimit As Single = pointData.Definition.AnalogFields.CompressionLimit

                If pointData.Definition.GeneralFlags.PointType = PointType.Digital Then compressionLimit = 0.000000001

                ' We'll only allow archival of points with a corresponding definition.
                pointState.PreviousValue = pointState.CurrentValue  ' Promote old CurrentValue to PreviousValue.
                pointState.CurrentValue = pointData.ToExtended()    ' Promote new value received to CurrentValue.

                ' Update the environment data that is periodically written to the Intercom File.
                m_intercomFile.Records(0).LastestCurrentValueTimeTag = pointState.CurrentValue.TimeTag
                m_intercomFile.Records(0).LatestCurrentValuePointID = pointState.CurrentValue.Definition.PointID

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
                    If m_compressData Then
                        ' We have to perform compression on data, so we'll do just that.
                        If .LastArchivedValue.IsNull Then
                            ' This is the first time data is received for the point.
                            .LastArchivedValue = .CurrentValue
                            result = True
                        ElseIf .PreviousValue.IsNull Then
                            ' This is the second time data is received for the point.
                            calculateSlopes = True
                        ElseIf .CurrentValue.Definition.CompressionMinimumTime > 0 AndAlso _
                                .CurrentValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value < .CurrentValue.Definition.CompressionMinimumTime Then
                            ' the "cmpMinTime" parameter specifies (when > 0), that a point should
                            ' not be archived if it was already archived less than "cmpMinTime" seconds
                            ' ago.  Determine difference between current event time and Last Archived
                            ' Value time, and exit if less than this amount of seconds.
                            result = False
                        ElseIf .CurrentValue.Quality <> .LastArchivedValue.Quality OrElse _
                                .CurrentValue.Quality <> .PreviousValue.Quality OrElse _
                                .PreviousValue.TimeTag.Value - .LastArchivedValue.TimeTag.Value > .CurrentValue.Definition.CompressionMaximumTime Then
                            ' The "cmpMaxTime" parameter specifies (when > 0) that a point should
                            ' be archived if the last time it was archived is more than "cmpMaxTime"
                            ' seconds ago.  Determine this difference and set flag accordingly.
                            ' If quality changed, or MaxTimeExceeded, archive it,
                            ' and recalculate slopes
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
                        ' We don't need to perform compression on the data and save all of it.
                        .LastArchivedValue = .CurrentValue
                        result = True
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
                End With
            End If

            Return result

        End Function

        Private Function FindHistoricArchiveFile(ByVal historicArchiveFile As ArchiveFileInfo) As Boolean

            Return m_searchTimeTag.CompareTo(historicArchiveFile.StartTimeTag) >= 0 And _
                    m_searchTimeTag.CompareTo(historicArchiveFile.EndTimeTag) <= 0

        End Function

#Region " Queue Delegates "

        Public Sub WriteToHistoricArchiveFile(ByVal items() As StandardPointData)

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

            For Each pointID As Integer In sortedPointData.Keys
                ' We'll sort the point data for the current point ID by time.
                sortedPointData(pointID).Sort()

                Dim writeData As Boolean = False
                Dim pointData As New List(Of StandardPointData)()
                Dim historicArchiveFile As ArchiveFileInfo = Nothing
                For i As Integer = 0 To sortedPointData(pointID).Count - 1
                    If historicArchiveFile Is Nothing Then
                        ' We'll try to find a historic file when the current point data belongs.
                        m_searchTimeTag = sortedPointData(pointID)(i).TimeTag
                        SyncLock m_historicArchiveFileList
                            historicArchiveFile = m_historicArchiveFileList.Find(AddressOf FindHistoricArchiveFile)
                        End SyncLock
                    End If

                    If historicArchiveFile IsNot Nothing Then
                        If sortedPointData(pointID)(i).TimeTag.CompareTo(historicArchiveFile.StartTimeTag) >= 0 AndAlso _
                                sortedPointData(pointID)(i).TimeTag.CompareTo(historicArchiveFile.EndTimeTag) <= 0 Then
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
                            With New ArchiveFile()
                                .Name = historicArchiveFile.FileName
                                .CompressData = m_compressData
                                .DiscardOoSData = m_discardOoSData
                                .StateFile = m_stateFile
                                .IntercomFile = m_intercomFile
                                .Open()
                                .Write(pointData.ToArray())
                                .Close()
                            End With

                            writeData = False
                            pointData.Clear()
                            historicArchiveFile = Nothing
                        End If
                    End If
                Next
            Next

        End Sub

        Public Sub InsertInCurrentArchiveFile(ByVal items() As StandardPointData)

        End Sub

#End Region

#Region " Event Handlers "

#Region " ArchiveFile "

        Private Sub ArchiveFile_FileFull(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.FileFull

            Rollover()

        End Sub

#End Region

#Region " FileSystemWatcher "

        Private Sub CurrentLocationFileSystemWatcher_Created(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles CurrentLocationFileSystemWatcher.Created, OffloadLocationFileSystemWatcher.Created

            If IsOpen Then
                ' Attempt to update the historic file list only if the current file is open.
                Dim historicFileInfo As ArchiveFileInfo = GetHistoricFileInfo(e.FullPath)
                SyncLock m_historicArchiveFileList
                    If historicFileInfo IsNot Nothing AndAlso Not m_historicArchiveFileList.Contains(historicFileInfo) Then
                        m_historicArchiveFileList.Add(historicFileInfo)
                        RaiseEvent HistoricFileListUpdated(Me, EventArgs.Empty)
                    End If
                End SyncLock
            End If

        End Sub

        Private Sub CurrentLocationFileSystemWatcher_Deleted(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles CurrentLocationFileSystemWatcher.Deleted, OffloadLocationFileSystemWatcher.Deleted

            If IsOpen Then
                ' Attempt to update the historic file list only if the current file is open.
                Dim historicFileInfo As ArchiveFileInfo = GetHistoricFileInfo(e.FullPath)
                SyncLock m_historicArchiveFileList
                    If historicFileInfo IsNot Nothing AndAlso m_historicArchiveFileList.Contains(historicFileInfo) Then
                        m_historicArchiveFileList.Remove(historicFileInfo)
                        RaiseEvent HistoricFileListUpdated(Me, EventArgs.Empty)
                    End If
                End SyncLock
            End If

        End Sub

        Private Sub CurrentLocationFileSystemWatcher_Renamed(ByVal sender As Object, ByVal e As System.IO.RenamedEventArgs) Handles CurrentLocationFileSystemWatcher.Renamed, OffloadLocationFileSystemWatcher.Renamed

            If IsOpen Then
                ' Attempt to update the historic file list only if the current file is open.
                If String.Compare(JustFileExtension(e.OldFullPath), Extension, True) = 0 Then
                    Try
                        Dim oldFileInfo As ArchiveFileInfo = GetHistoricFileInfo(e.OldFullPath)
                        SyncLock m_historicArchiveFileList
                            If oldFileInfo IsNot Nothing AndAlso m_historicArchiveFileList.Contains(oldFileInfo) Then
                                m_historicArchiveFileList.Remove(oldFileInfo)
                                RaiseEvent HistoricFileListUpdated(Me, EventArgs.Empty)
                            End If
                        End SyncLock
                    Catch ex As Exception
                        ' Ignore any exception we might encounter here if an archive file being renamed to a 
                        ' historic archive file. This might happen if someone is renaming files manually.
                    End Try
                End If

                If String.Compare(JustFileExtension(e.FullPath), Extension, True) = 0 Then
                    Try
                        Dim newFileInfo As ArchiveFileInfo = GetHistoricFileInfo(e.FullPath)
                        SyncLock m_historicArchiveFileList
                            If newFileInfo IsNot Nothing AndAlso Not m_historicArchiveFileList.Contains(newFileInfo) Then
                                m_historicArchiveFileList.Add(newFileInfo)
                                RaiseEvent HistoricFileListUpdated(Me, EventArgs.Empty)
                            End If
                        End SyncLock
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

            Public FileName As String

            Public StartTimeTag As TimeTag

            Public EndTimeTag As TimeTag

            Public Overrides Function Equals(ByVal obj As Object) As Boolean

                Dim other As ArchiveFileInfo = TryCast(obj, ArchiveFileInfo)
                If other IsNot Nothing Then
                    Return StartTimeTag.Equals(other.StartTimeTag) And EndTimeTag.Equals(other.EndTimeTag) And _
                        String.Compare(JustPath(FileName), JustPath(other.FileName), True) = 0 And _
                        String.Compare(JustFileName(FileName), JustFileName(other.FileName), True) = 0
                End If

            End Function

        End Class

#End Region

#End Region

    End Class

End Namespace