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
        Private m_stateFile As StateFile
        Private m_intercomFile As IntercomFile
        Private m_fat As ArchiveFileAllocationTable
        Private m_fileStream As FileStream
        Private m_activeDataBlocks As Dictionary(Of Integer, ArchiveDataBlock)
        Private m_dataBlockRequestCount As Integer
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

        Public Property StateFile() As StateFile
            Get
                Return m_stateFile
            End Get
            Set(ByVal value As StateFile)
                m_stateFile = value
            End Set
        End Property

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
                RaiseEvent FileOpening(Me, EventArgs.Empty)

                m_name = AbsolutePath(m_name)
                If File.Exists(m_name) Then
                    ' File has been created already, so we just need to read it.
                    m_fileStream = New FileStream(m_name, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)
                    m_fat = New ArchiveFileAllocationTable(m_fileStream)
                Else
                    ' File does not exist, so we have to create it and initialize it.
                    m_fileStream = New FileStream(m_name, FileMode.Create, FileAccess.ReadWrite, FileShare.Read)
                    m_fat = New ArchiveFileAllocationTable(m_fileStream, m_blockSize, MaximumDataBlocks(m_size, m_blockSize))
                    m_fat.Persist()
                End If
                m_activeDataBlocks = New Dictionary(Of Integer, ArchiveDataBlock)()

                RaiseEvent FileOpened(Me, EventArgs.Empty)
            End If

        End Sub

        Public Sub Close()

            If IsOpen Then
                RaiseEvent FileClosing(Me, EventArgs.Empty)

                If m_saveOnClose Then Save()

                m_fat = Nothing
                m_fileStream.Close()
                m_fileStream = Nothing
                m_activeDataBlocks.Clear()
                m_activeDataBlocks = Nothing
                m_dataBlockRequestCount = 0
                m_rolloverPreparationThread.Abort()

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

                Close()
                WaitForWriteLock(m_name)    ' We must wait for the server to release the file.
                File.Move(m_name, historyFile)
                File.Move(standbyFile, m_name)
                Open()
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
                        If ToBeArchived(pointData) Then
                            ' Archive the data
                            Dim dataBlock As ArchiveDataBlock = Nothing
                            m_activeDataBlocks.TryGetValue(pointData.Definition.ID, dataBlock)
                            If dataBlock Is Nothing OrElse (dataBlock IsNot Nothing AndAlso dataBlock.SlotsAvailable <= 0) Then
                                ' We either don't have a active data block where we can archive the point data or we have a 
                                ' active data block but it is full, so we have to request a new data block from the FAT.
                                m_dataBlockRequestCount += 1
                                m_activeDataBlocks.Remove(pointData.Definition.ID)
                                dataBlock = m_fat.RequestDataBlock(pointData.Definition.ID, pointData.TimeTag)
                                m_activeDataBlocks.Add(pointData.Definition.ID, dataBlock)

                                If m_dataBlockRequestCount >= m_fat.DataBlockCount * (m_rolloverPreparationThreshold / 100) AndAlso _
                                        Not m_rolloverPreparationDone AndAlso Not m_rolloverPreparationThread.IsAlive Then
                                    ' We've requested the specified percent of the total number of data blocks in the file, 
                                    ' so we must now prepare for the rollver process since has not been done yet and it is 
                                    ' not already in progress.
                                    m_rolloverPreparationThread = New Thread(AddressOf PrepareForRollover)
                                    m_rolloverPreparationThread.Priority = ThreadPriority.Lowest
                                    m_rolloverPreparationThread.Start()
                                End If
                            End If

                            If dataBlock IsNot Nothing Then
                                ' We were able to obtain a data block for writing data.
                                dataBlock.Write(pointData)

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
                RaiseEvent RolloverPreparationStart(Me, EventArgs.Empty)

                With New ArchiveFile()
                    .Name = GetStandbyArchiveFileName()
                    .Size = m_size
                    .BlockSize = m_blockSize
                    .Open()
                    .Close()
                End With

                m_rolloverPreparationDone = True

                RaiseEvent RolloverPreparationComplete(Me, EventArgs.Empty)
            Catch ex As ThreadAbortException
                ' We can safely ignore this exception.
            Catch ex As Exception
                RaiseEvent RolloverPreparationException(Me, New ExceptionEventArgs(ex))
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

        Private Function ToBeArchived(ByVal pointDate As StandardPointData) As Boolean

            ' TODO: Perform compression check here.
            Return True

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