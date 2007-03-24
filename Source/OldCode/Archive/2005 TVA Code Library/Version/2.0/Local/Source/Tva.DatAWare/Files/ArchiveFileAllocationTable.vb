' 02/18/2007

Imports System.IO
Imports Tva.Interop

Namespace Files

    Public Class ArchiveFileAllocationTable
        Implements IDisposable, IBinaryDataProvider

        ' *******************************************************
        ' *                     FAT structure                   *
        ' *******************************************************

#Region " Member Declaration "

        Private m_fileStartTime As TimeTag
        Private m_fileEndTime As TimeTag
        Private m_eventsReceived As Integer
        Private m_eventsArchived As Integer
        Private m_dataBlockSize As Integer
        Private m_dataBlockCount As Integer
        Private m_dataBlockPointers As List(Of ArchiveDataBlockPointer)
        Private m_fileStream As FileStream
        Private m_searchPointID As Integer      ' <=|
        Private m_searchStartTime As TimeTag    ' <=| Used for finding data block pointer in m_dataBlockPointers 
        Private m_searchEndTime As TimeTag      ' <=|

        Private WithEvents m_fatUpdateTimer As System.Timers.Timer

        Private Const FatUpdateInterval As Integer = 3000
        Private Const ArrayDescriptorLength As Integer = 10

#End Region

        Private Sub New()

            MyBase.New()
            m_fileStartTime = TimeTag.MinValue
            m_fileEndTime = TimeTag.MinValue
            m_dataBlockPointers = New List(Of ArchiveDataBlockPointer)()
            m_fatUpdateTimer = New System.Timers.Timer(FatUpdateInterval)

        End Sub

#Region " Public Code "

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="archiveFileStream"></param>
        ''' <remarks>Used when reading existing archive file.</remarks>
        Public Sub New(ByVal archiveFileStream As FileStream)

            MyClass.New()
            If archiveFileStream IsNot Nothing Then
                m_fileStream = archiveFileStream
                Dim fixedFatData As Byte() = CreateArray(Of Byte)(FixedBinaryLength)
                m_fileStream.Seek(-fixedFatData.Length, SeekOrigin.End)
                m_fileStream.Read(fixedFatData, 0, fixedFatData.Length)
                m_fileStartTime = New TimeTag(BitConverter.ToDouble(fixedFatData, 0))
                m_fileEndTime = New TimeTag(BitConverter.ToDouble(fixedFatData, 8))
                m_eventsReceived = BitConverter.ToInt32(fixedFatData, 16)
                m_eventsArchived = BitConverter.ToInt32(fixedFatData, 20)
                m_dataBlockSize = BitConverter.ToInt32(fixedFatData, 24)
                m_dataBlockCount = BitConverter.ToInt32(fixedFatData, 28)

                Dim variableFatData As Byte() = CreateArray(Of Byte)(m_dataBlockCount * ArchiveDataBlockPointer.Size)
                m_fileStream.Seek(-(variableFatData.Length + FixedBinaryLength), SeekOrigin.End)
                m_fileStream.Read(variableFatData, 0, variableFatData.Length)
                For i As Integer = 0 To variableFatData.Length - 1 Step ArchiveDataBlockPointer.Size
                    m_dataBlockPointers.Add(New ArchiveDataBlockPointer(variableFatData, i))
                Next

                m_fatUpdateTimer.Start()
            Else
                Throw New ArgumentNullException("archiveFileStream")
            End If

        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="archiveFileStream"></param>
        ''' <param name="blockSize"></param>
        ''' <param name="blockCount"></param>
        ''' <remarks>Used when creating new archive file.</remarks>
        Public Sub New(ByVal archiveFileStream As FileStream, ByVal blockSize As Integer, ByVal blockCount As Integer)

            MyClass.New()
            m_fileStream = archiveFileStream
            m_dataBlockSize = blockSize
            m_dataBlockCount = blockCount
            For i As Integer = 1 To m_dataBlockCount
                m_dataBlockPointers.Add(New ArchiveDataBlockPointer())
            Next

            m_fatUpdateTimer.Start()

        End Sub

        Public Property FileStartTime() As TimeTag
            Get
                Return m_fileStartTime
            End Get
            Set(ByVal value As TimeTag)
                m_fileStartTime = value
            End Set
        End Property

        Public Property FileEndTime() As TimeTag
            Get
                Return m_fileEndTime
            End Get
            Set(ByVal value As TimeTag)
                m_fileEndTime = value
            End Set
        End Property

        Public Property EventsReceived() As Integer
            Get
                Return m_eventsReceived
            End Get
            Set(ByVal value As Integer)
                m_eventsReceived = value
            End Set
        End Property

        Public Property EventsArchived() As Integer
            Get
                Return m_eventsArchived
            End Get
            Set(ByVal value As Integer)
                m_eventsArchived = value
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Size in KB.</remarks>
        Public ReadOnly Property DataBlockSize() As Integer
            Get
                Return m_dataBlockSize
            End Get
        End Property

        Public ReadOnly Property DataBlockCount() As Integer
            Get
                Return m_dataBlockCount
            End Get
        End Property

        Public ReadOnly Property DataBlocksUsed() As Integer
            Get
                Return m_dataBlockCount - DataBlocksAvailable
            End Get
        End Property

        Public ReadOnly Property DataBlocksAvailable() As Integer
            Get
                Dim unusedPointerIndex As Integer = m_dataBlockPointers.IndexOf(New ArchiveDataBlockPointer())
                If unusedPointerIndex >= 0 Then
                    Return m_dataBlockCount - unusedPointerIndex
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property DataBlockPointers() As List(Of ArchiveDataBlockPointer)
            Get
                Return m_dataBlockPointers
            End Get
        End Property

        Public Sub Persist()

            ' Leave space for data blocks.
            SyncLock m_fileStream
                m_fileStream.Seek(DataBinaryLength, SeekOrigin.Begin)
                m_fileStream.Write(BinaryImage, 0, BinaryLength)
                m_fileStream.Flush()
            End SyncLock

        End Sub

        Public Sub Extend()

            Extend(1)

        End Sub

        Public Sub Extend(ByVal dataBlocksToAdd As Integer)

            m_dataBlockCount += dataBlocksToAdd
            m_dataBlockPointers.Add(New ArchiveDataBlockPointer())
            Persist()

        End Sub

        Public Function RequestDataBlock(ByVal pointID As Integer, ByVal startTime As TimeTag) As ArchiveDataBlock

            ' Get the index of the first available data block's pointer.
            Dim unusedPointerIndex As Integer = m_dataBlockPointers.IndexOf(New ArchiveDataBlockPointer())
            If unusedPointerIndex >= 0 Then
                ' Assign the data block to the specified point index.
                m_dataBlockPointers(unusedPointerIndex).PointID = pointID
                m_dataBlockPointers(unusedPointerIndex).StartTime = startTime

                SyncLock m_fileStream
                    ' We'll write information about the just allocateddata block to the file.
                    m_fileStream.Seek(DataBinaryLength + ArrayDescriptorLength + (unusedPointerIndex * ArchiveDataBlockPointer.Size), SeekOrigin.Begin)
                    m_fileStream.Write(m_dataBlockPointers(unusedPointerIndex).BinaryImage, 0, ArchiveDataBlockPointer.Size)
                End SyncLock

                ' Get the data block that corresponds to data block pointer.
                Return GetDataBlock(m_dataBlockPointers(unusedPointerIndex))
            Else
                Return Nothing
            End If

        End Function

        Public Function FindDataBlocks(ByVal pointID As Integer) As List(Of ArchiveDataBlock)

            Return FindDataBlocks(pointID, TimeTag.MinValue)

        End Function

        Public Function FindDataBlocks(ByVal pointID As Integer, ByVal startTime As TimeTag) As List(Of ArchiveDataBlock)

            Return FindDataBlocks(pointID, startTime, TimeTag.MaxValue)

        End Function

        Public Function FindDataBlocks(ByVal pointID As Integer, ByVal startTime As TimeTag, ByVal endTime As TimeTag) As List(Of ArchiveDataBlock)

            ' Setup the search criteria that will be used for finding data block pointers.
            m_searchPointID = pointID
            m_searchStartTime = IIf(startTime IsNot Nothing, startTime, TimeTag.MinValue)
            m_searchEndTime = IIf(endTime IsNot Nothing, endTime, TimeTag.MaxValue)

            Dim blocks As List(Of ArchiveDataBlock) = New List(Of ArchiveDataBlock)()
            Dim pointers As List(Of ArchiveDataBlockPointer) = m_dataBlockPointers.FindAll(AddressOf FindDataBlockPointer)

            ' Build a list of data blocks that correspond to the found data block pointers.
            For i As Integer = 0 To pointers.Count - 1
                blocks.Add(GetDataBlock(pointers(i)))
            Next

            Return blocks

        End Function

#Region " Interface Implementations "

#Region " IDisposable "

        Private m_disposedValue As Boolean = False        ' To detect redundant calls

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)

            If Not m_disposedValue Then
                If disposing Then
                    m_fatUpdateTimer.Dispose()
                End If
            End If
            m_disposedValue = True

        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

#End Region

#Region " IBinaryDataProvider "

        Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
            Get
                Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)

                Array.Copy(VariableBinaryImage, 0, image, 0, VariableBinaryLength)
                Array.Copy(FixedBinaryImage, 0, image, VariableBinaryLength, FixedBinaryLength)

                Return image
            End Get
        End Property

        Public ReadOnly Property BinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
            Get
                ' We add 10 bytes for the array descriptor that required for reading the file from VB.
                Return VariableBinaryLength + FixedBinaryLength
            End Get
        End Property

#End Region

#End Region

#End Region

#Region " Private Code "

        Private ReadOnly Property DataBinaryLength() As Long
            Get
                Return (m_dataBlockCount * (m_dataBlockSize * 1024L))
            End Get
        End Property

        Private ReadOnly Property FixedBinaryLength() As Integer
            Get
                Return 32
            End Get
        End Property

        Private ReadOnly Property VariableBinaryLength() As Integer
            Get
                ' We add the extra bytes for the array descriptor that required for reading the file from VB.
                Return (ArrayDescriptorLength + (m_dataBlockCount * ArchiveDataBlockPointer.Size))
            End Get
        End Property

        Private ReadOnly Property FixedBinaryImage() As Byte()
            Get
                Dim fixedImage As Byte() = CreateArray(Of Byte)(FixedBinaryLength)

                Array.Copy(BitConverter.GetBytes(m_fileStartTime.Value), 0, fixedImage, 0, 8)
                Array.Copy(BitConverter.GetBytes(m_fileEndTime.Value), 0, fixedImage, 8, 8)
                Array.Copy(BitConverter.GetBytes(m_eventsReceived), 0, fixedImage, 16, 4)
                Array.Copy(BitConverter.GetBytes(m_eventsArchived), 0, fixedImage, 20, 4)
                Array.Copy(BitConverter.GetBytes(m_dataBlockSize), 0, fixedImage, 24, 4)
                Array.Copy(BitConverter.GetBytes(m_dataBlockCount), 0, fixedImage, 28, 4)

                Return fixedImage
            End Get
        End Property

        Private ReadOnly Property VariableBinaryImage() As Byte()
            Get
                Dim variableImage As Byte() = CreateArray(Of Byte)(VariableBinaryLength)
                Dim arrayDescriptor As VBArrayDescriptor = VBArrayDescriptor.OneBasedOneDimensionalArray(m_dataBlockCount)

                Array.Copy(arrayDescriptor.BinaryImage, 0, variableImage, 0, arrayDescriptor.BinaryLength)
                For i As Integer = 0 To m_dataBlockPointers.Count - 1
                    Array.Copy(m_dataBlockPointers(i).BinaryImage, 0, variableImage, _
                        (i * ArchiveDataBlockPointer.Size) + arrayDescriptor.BinaryLength, ArchiveDataBlockPointer.Size)
                Next

                Return variableImage
            End Get
        End Property

        Private Function GetDataBlock(ByVal blockPointer As ArchiveDataBlockPointer) As ArchiveDataBlock

            ' First, get the location of the data block corresponding to the specified data block pointer.
            Dim location As Long = GetDataBlockLocation(blockPointer)
            If location >= 0 Then
                ' We have a valid location, so we'll create a data block instance using this information.
                Return New ArchiveDataBlock(m_fileStream, location, m_dataBlockSize)
            Else
                ' We don't a valid location, so the specified data block pointer must not be valid.
                Return Nothing
            End If

        End Function

        Private Function GetDataBlockLocation(ByVal dataBlockPointer As ArchiveDataBlockPointer) As Long

            ' First, we'll get the index of the specified data block pointer.
            Dim pointerIndex As Integer = m_dataBlockPointers.IndexOf(dataBlockPointer)
            If pointerIndex >= 0 Then
                ' We calculate the data block's location based on the data block pointer's index.
                Return pointerIndex * (m_dataBlockSize * 1024L)
            Else
                Return -1
            End If

        End Function

        ''' <summary>
        ''' Finds data block pointer that match the search criteria that is determined by member variables.
        ''' </summary>
        ''' <param name="dataBlockPointer"></param>
        ''' <returns></returns>
        Private Function FindDataBlockPointer(ByVal dataBlockPointer As ArchiveDataBlockPointer) As Boolean

            Return (dataBlockPointer.PointID = m_searchPointID AndAlso _
                    (m_searchStartTime.CompareTo(TimeTag.MinValue) = 0 OrElse _
                        dataBlockPointer.StartTime.CompareTo(m_searchStartTime) >= 0) AndAlso _
                    (m_searchEndTime.CompareTo(TimeTag.MaxValue) = 0 OrElse _
                        dataBlockPointer.StartTime.CompareTo(m_searchEndTime) <= 0))

        End Function

#Region " Event Handlers "

#Region " m_fatUpdateTimer "

        Private Sub m_fatUpdateTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_fatUpdateTimer.Elapsed

            ' We'll periodically write the fixed part of the FAT to the file.
            If m_fileStream IsNot Nothing Then
                SyncLock m_fileStream
                    m_fileStream.Seek(DataBinaryLength + VariableBinaryLength, SeekOrigin.Begin)
                    m_fileStream.Write(FixedBinaryImage, 0, FixedBinaryLength)
                End SyncLock
            End If

        End Sub

#End Region

#End Region

#End Region

    End Class

End Namespace