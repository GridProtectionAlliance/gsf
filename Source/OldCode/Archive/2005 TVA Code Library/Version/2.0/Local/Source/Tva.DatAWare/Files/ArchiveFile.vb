' 02/18/2007

Imports System.IO
Imports System.Drawing
Imports System.Threading
Imports System.ComponentModel
Imports Tva.IO.FilePath

Namespace Files

    <ToolboxBitmap(GetType(ArchiveFile))> _
    Public Class ArchiveFile

#Region " Member Declaration "

        Private m_name As String
        Private m_size As Double
        Private m_blockSize As Integer
        Private m_saveOnClose As Boolean
        Private m_rolloverOnFull As Boolean
        Private m_rolloverPreparationThreshold As Short
        Private m_offloadPath As String
        Private m_offloadCount As Integer
        Private m_offloadThreshold As Short
        Private m_stateFile As StateFile
        Private m_intercomFile As IntercomFile
        Private m_fat As ArchiveFileAllocationTable
        Private m_fileStream As FileStream
        Private m_rolloverPreparationDone As Boolean
        Private m_rolloverPreparationThread As Thread

#End Region

#Region " Event Declaration "

        Public Event FileFull As EventHandler
        Public Event FileOpening As EventHandler
        Public Event FileOpened As EventHandler
        Public Event FileClosing As EventHandler
        Public Event FileClosed As EventHandler
        Public Event RolloverStart As EventHandler
        Public Event RolloverComplete As EventHandler
        Public Event RolloverPreparationStart As EventHandler
        Public Event RolloverPreparationComplete As EventHandler
        Public Event RolloverPreparationException As EventHandler(Of ExceptionEventArgs)
        Public Event OffloadStart As EventHandler
        Public Event OffloadComplete As EventHandler
        Public Event OffloadException As EventHandler(Of ExceptionEventArgs)

        'Public Event DataReceived As EventHandler
        'Public Event DataArchived As EventHandler
        'Public Event DataDiscarded As EventHandler

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

                    ' Make sure that the necessary files are available and ready for use.
                    If Not m_stateFile.IsOpen Then m_stateFile.Open()
                    If Not m_intercomFile.IsOpen Then m_intercomFile.Open()

                    RaiseEvent FileOpened(Me, EventArgs.Empty)
                Else
                    Throw New InvalidOperationException("StateFile and IntercomFile properties must be set.")
                End If
            End If

        End Sub

        Public Sub Close()

            Close(True)

        End Sub

        Public Sub Close(ByVal releaseAllFileLocks As Boolean)

            If IsOpen Then
                RaiseEvent FileClosing(Me, EventArgs.Empty)

                If m_saveOnClose Then Save()

                m_fat = Nothing
                m_fileStream.Dispose()
                m_fileStream = Nothing
                m_rolloverPreparationThread.Abort()
                If releaseAllFileLocks AndAlso m_stateFile.IsOpen Then
                    For i As Integer = 0 To m_stateFile.Records.Count - 1
                        ' We'll release all the data blocks that were being used by the file.
                        If m_stateFile.Records(i).ActiveDataBlock IsNot Nothing Then
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

            If m_rolloverPreparationDone Then
                RaiseEvent RolloverStart(Me, EventArgs.Empty)

                Dim standbyFile As String = GetStandbyArchiveFileName()
                Dim historyFile As String = GetHistoryArchiveFileName()

                ' Signal the server that we're are performing rollover so it must let go of this file.
                m_intercomFile.Records(0).FileWrap = True
                m_intercomFile.Save()
                Close()

                WaitForWriteLock(m_name)        ' Wait for the server to release the file.

                File.Move(m_name, historyFile)  ' Make the active archive file, historic archive file.
                File.Move(standbyFile, m_name)  ' Make the standby archive file, active archive file.

                ' We're now done with the rollover process, so we must inform the server of this.
                Open()
                m_intercomFile.Records(0).FileWrap = False
                m_intercomFile.Save()

                m_rolloverPreparationDone = False

                RaiseEvent RolloverComplete(Me, EventArgs.Empty)
            End If

        End Sub

        Public Function Read(ByVal pointID As Integer) As List(Of StandardPointData)

            Return Read(pointID, TimeTag.MinValue)

        End Function

        Public Function Read(ByVal pointID As Integer, ByVal startTime As TimeTag) As List(Of StandardPointData)

            Return Read(pointID, startTime, TimeTag.MaxValue)

        End Function

        Public Function Read(ByVal pointID As Integer, ByVal startTime As TimeTag, ByVal endTime As TimeTag) As List(Of StandardPointData)

            If IsOpen Then
                Dim data As New List(Of StandardPointData)()
                Dim foundBlocks As List(Of ArchiveDataBlock) = m_fat.FindDataBlocks(pointID, startTime, endTime)
                For i As Integer = 0 To foundBlocks.Count - 1
                    data.AddRange(foundBlocks(i).Read())
                    foundBlocks(i).Dispose()
                Next

                Return data
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Function

        Public Sub Write(ByVal pointData As StandardPointData)

            If IsOpen Then
                If pointData.Definition IsNot Nothing Then
                    m_fat.EventsReceived += 1

                    If pointData.TimeTag.CompareTo(m_fat.FileStartTime) >= 0 Then
                        ' The data to be written has a timetag that is the same as newer than the file's start time.
                        Dim pointState As PointState = m_stateFile.Read(pointData.Definition.ID)
                        If pointData.TimeTag.CompareTo(pointState.LastArchivedValue.TimeTag) >= 0 Then
                            If ToBeArchived(pointState, pointData) Then
                                ' Archive the data
                                If pointState.ActiveDataBlock Is Nothing OrElse _
                                        (pointState.ActiveDataBlock IsNot Nothing AndAlso pointState.ActiveDataBlock.SlotsAvailable <= 0) Then
                                    ' We either don't have a active data block where we can archive the point data or we have a 
                                    ' active data block but it is full, so we have to request a new data block from the FAT.
                                    If pointState.ActiveDataBlock IsNot Nothing Then pointState.ActiveDataBlock.Dispose()
                                    pointState.ActiveDataBlock = m_fat.RequestDataBlock(pointData.Definition.ID, pointData.TimeTag)

                                    If m_fat.DataBlocksAvailable < m_fat.DataBlockCount * (1 - (m_rolloverPreparationThreshold / 100)) AndAlso _
                                            Not m_rolloverPreparationDone AndAlso Not m_rolloverPreparationThread.IsAlive Then
                                        ' We've requested the specified percent of the total number of data blocks in the file, 
                                        ' so we must now prepare for the rollver process since has not been done yet and it is 
                                        ' not already in progress.
                                        m_rolloverPreparationThread = New Thread(AddressOf PrepareForRollover)
                                        m_rolloverPreparationThread.Priority = ThreadPriority.Lowest
                                        m_rolloverPreparationThread.Start()
                                    End If
                                End If

                                If pointState.ActiveDataBlock IsNot Nothing Then
                                    ' We were able to obtain a data block for writing data.
                                    pointState.ActiveDataBlock.Write(pointData)

                                    m_fat.EventsArchived += 1
                                    m_fat.FileEndTime = pointData.TimeTag
                                    If m_fat.FileStartTime.CompareTo(TimeTag.MinValue) = 0 Then m_fat.FileStartTime = pointData.TimeTag
                                Else
                                    ' We were unable to obtain a data block for writing data to because all data block are in use.
                                    RaiseEvent FileFull(Me, EventArgs.Empty)
                                End If
                            Else
                                ' Discard the data
                            End If
                        Else
                            ' Insert the data into the current file.
                            InsertInCurrentArchiveFile()
                        End If
                    Else
                        ' The data to be written has a timetag that is older than the file's start time, so the data
                        ' does not belong in this file but in a historic archive file instead.
                        WriteToHistoricArchiveFile()    ' <- This is just a stub for now.
                    End If
                Else
                    Throw New ArgumentException("Definition property for point data is not set.")
                End If
            Else
                Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
            End If

        End Sub

        Public Shared Function MaximumDataBlocks(ByVal fileSize As Double, ByVal blockSize As Integer) As Integer

            Return Convert.ToInt32((fileSize * 1024) / blockSize)

        End Function

#End Region

#Region " Private Code "

        Private Sub PrepareForRollover()

            Try
                Dim archiveDrive As New DriveInfo(Path.GetPathRoot(m_name))
                If archiveDrive.AvailableFreeSpace < archiveDrive.TotalSize * (1 - (m_offloadThreshold / 100)) Then
                    ' We'll start offloading historic files if we've reached the offload threshold.
                    OffloadHistoricFiles()
                End If

                RaiseEvent RolloverPreparationStart(Me, EventArgs.Empty)

                With New ArchiveFile()
                    .Name = GetStandbyArchiveFileName()
                    .Size = m_size
                    .BlockSize = m_blockSize
                    .StateFile = m_stateFile
                    .IntercomFile = m_intercomFile
                    .Open()
                    .Close(False)
                End With

                m_rolloverPreparationDone = True

                RaiseEvent RolloverPreparationComplete(Me, EventArgs.Empty)
            Catch ex As ThreadAbortException
                ' We can safely ignore this exception.
            Catch ex As Exception
                RaiseEvent RolloverPreparationException(Me, New ExceptionEventArgs(ex))
            End Try

        End Sub

        Public Sub OffloadHistoricFiles()

            Try
                RaiseEvent OffloadStart(Me, EventArgs.Empty)

                If Directory.Exists(m_offloadPath) Then
                    ' The offload path that is specified is a valid one so we'll gather a list of all historic
                    ' files in the directory where the current (active) archive file is located.
                    Dim historicFilePattern As String = NoFileExtension(m_name) & "_*" & Extension
                    Dim historicFiles As String() = Directory.GetFiles(JustPath(m_name), historicFilePattern)

                    ' Sorting the list will sort the historic files from oldest to newest.
                    Array.Sort(historicFiles)

                    ' We'll offload the specified number of oldest historic files to the offload location if the 
                    ' number of historic files is more than the offload count or all of the historic files if the 
                    ' offload count is smaller the available number of historic files.
                    For i As Integer = 0 To IIf(historicFiles.Length < m_offloadCount, historicFiles.Length, m_offloadCount) - 1
                        File.Move(historicFiles(i), AddPathSuffix(m_offloadPath) & JustFileName(historicFiles(i)))
                    Next
                Else
                    Throw New ArgumentException(String.Format("The offload path ""{0}"" is invalid.", m_offloadPath))
                End If

                RaiseEvent RolloverComplete(Me, EventArgs.Empty)
            Catch ex As Exception
                RaiseEvent OffloadException(Me, New ExceptionEventArgs(ex))
            End Try

        End Sub

        Public Sub WriteToHistoricArchiveFile()

        End Sub

        Public Sub InsertInCurrentArchiveFile()

        End Sub

        Private Function GetStandbyArchiveFileName() As String

            Return JustPath(m_name) & NoFileExtension(m_name) & "_standby" & Extension

        End Function

        Private Function GetHistoryArchiveFileName() As String

            Return JustPath(m_name) & (NoFileExtension(m_name) & "_" & m_fat.FileStartTime.ToString() & "_to_" & m_fat.FileEndTime.ToString() & Extension).Replace(":"c, "!"c)

        End Function

        Private Function ToBeArchived(ByVal pointState As PointState, ByRef pointData As StandardPointData) As Boolean

            pointState.PreviousValue = pointState.CurrentValue
            pointState.CurrentValue = pointData.ToExtended()

            If Not pointState.PreviousValue.IsNull Then
                ' TODO: Perform compression check here.

                pointData = pointState.PreviousValue.ToStandard()
                pointState.LastArchivedValue = pointState.PreviousValue

                Return True
            End If

        End Function

#Region " ArchiveFile Events "

        Private Sub ArchiveFile_FileFull(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.FileFull

            Rollover()

        End Sub

#End Region

        Private Class HistoricPointData

            Public ArchiveFile As HistoricArchiveFile

            Public PointData As List(Of StandardPointData)

        End Class

        Public Class HistoricArchiveFile

            Public FileName As String

            Public StartTimeTag As TimeTag

            Public EndTimeTag As TimeTag

        End Class

#End Region

    End Class

End Namespace