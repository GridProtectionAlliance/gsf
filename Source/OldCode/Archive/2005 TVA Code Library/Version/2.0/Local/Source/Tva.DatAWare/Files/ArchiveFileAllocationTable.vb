' 02/18/2007

Imports System.IO
Imports Tva.Interop

Namespace Files

    Public Class ArchiveFileAllocationTable
        Implements IBinaryDataProvider

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
        Private m_searchPointIndex As Integer   ' <=|
        Private m_searchStartTime As TimeTag    ' <=| Used for finding data block pointer in m_dataBlockPointers 
        Private m_searchEndTime As TimeTag      ' <=|

        Private Const MinimumBinaryLength As Integer = 32

#End Region

        Private Sub New()

            MyBase.New()
            m_fileStartTime = TimeTag.MinValue
            m_fileEndTime = TimeTag.MinValue
            m_dataBlockPointers = New List(Of ArchiveDataBlockPointer)()

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
                Dim fixedFatData As Byte() = CreateArray(Of Byte)(MinimumBinaryLength)
                m_fileStream.Seek(-fixedFatData.Length, SeekOrigin.End)
                m_fileStream.Read(fixedFatData, 0, fixedFatData.Length)
                m_fileStartTime = New TimeTag(BitConverter.ToDouble(fixedFatData, 0))
                m_fileEndTime = New TimeTag(BitConverter.ToDouble(fixedFatData, 8))
                m_eventsReceived = BitConverter.ToInt32(fixedFatData, 16)
                m_eventsArchived = BitConverter.ToInt32(fixedFatData, 20)
                m_dataBlockSize = BitConverter.ToInt32(fixedFatData, 24)
                m_dataBlockCount = BitConverter.ToInt32(fixedFatData, 28)

                Dim variableFatData As Byte() = CreateArray(Of Byte)(m_dataBlockCount * ArchiveDataBlockPointer.Size)
                m_fileStream.Seek(-(variableFatData.Length + MinimumBinaryLength), SeekOrigin.End)
                m_fileStream.Read(variableFatData, 0, variableFatData.Length)
                For i As Integer = 0 To variableFatData.Length - 1 Step ArchiveDataBlockPointer.Size
                    m_dataBlockPointers.Add(New ArchiveDataBlockPointer(variableFatData, i))
                Next
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

        Public ReadOnly Property DataBlockPointers() As List(Of ArchiveDataBlockPointer)
            Get
                Return m_dataBlockPointers
            End Get
        End Property

        Public Sub Persist()

            ' Leave space for data blocks.
            m_fileStream.Seek(m_dataBlockCount * (m_dataBlockSize * 1024L), SeekOrigin.Begin)
            'If m_fileStream.Length > 0 Then
            '    ' Existing file...
            '    m_fileStream.Seek(-BinaryLength, SeekOrigin.End)
            'Else
            '    ' New file...
            '    m_fileStream.Seek(m_dataBlockCount * (m_dataBlockSize * 1024L), SeekOrigin.Begin)
            'End If
            m_fileStream.Write(BinaryData, 0, BinaryDataLength)
            m_fileStream.Flush()

        End Sub

        Public Sub Extend()

            Extend(1)

        End Sub

        Public Sub Extend(ByVal dataBlocksToAdd As Integer)

            m_dataBlockCount += dataBlocksToAdd
            m_dataBlockPointers.Add(New ArchiveDataBlockPointer())
            Persist()

        End Sub

        Public Function RequestDataBlock(ByVal pointIndex As Integer, ByVal startTime As TimeTag) As ArchiveDataBlock

            ' Get the index of the first available data block's pointer.
            Dim unusedPointerIndex As Integer = m_dataBlockPointers.IndexOf(New ArchiveDataBlockPointer())
            If unusedPointerIndex >= 0 Then
                ' Assign the data block to the specified point index.
                m_dataBlockPointers(unusedPointerIndex).PointIndex = pointIndex
                m_dataBlockPointers(unusedPointerIndex).StartTime = startTime
                ' Get the data block that corresponds to data block pointer.
                Return GetDataBlock(m_dataBlockPointers(unusedPointerIndex))
            Else
                Return Nothing
            End If

        End Function

        Public Function FindDataBlocks(ByVal pointIndex As Integer) As List(Of ArchiveDataBlock)

            Return FindDataBlocks(pointIndex, TimeTag.MinValue)

        End Function

        Public Function FindDataBlocks(ByVal pointIndex As Integer, ByVal startTime As TimeTag) As List(Of ArchiveDataBlock)

            Return FindDataBlocks(pointIndex, startTime, TimeTag.MaxValue)

        End Function

        Public Function FindDataBlocks(ByVal pointIndex As Integer, ByVal startTime As TimeTag, ByVal endTime As TimeTag) As List(Of ArchiveDataBlock)

            ' Setup the search criteria that will be used for finding data block pointers.
            m_searchPointIndex = pointIndex
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

#Region " IBinaryDataProvider Implementation "

        Public ReadOnly Property BinaryData() As Byte() Implements IBinaryDataProvider.BinaryData
            Get
                Dim image As Byte() = CreateArray(Of Byte)(BinaryDataLength)
                Dim arrayDescriptor As VBArrayDescriptor = VBArrayDescriptor.OneBasedOneDimensionalArray(m_dataBlockCount)

                Array.Copy(arrayDescriptor.BinaryData, 0, image, 0, arrayDescriptor.BinaryDataLength)
                For i As Integer = 0 To m_dataBlockPointers.Count - 1
                    Array.Copy(m_dataBlockPointers(i).BinaryData, 0, image, _
                        (i * ArchiveDataBlockPointer.Size) + arrayDescriptor.BinaryDataLength, ArchiveDataBlockPointer.Size)
                Next

                Dim pointersBinaryLength As Integer = BinaryDataLength - MinimumBinaryLength
                Array.Copy(BitConverter.GetBytes(m_fileStartTime.Value), 0, image, pointersBinaryLength, 8)
                Array.Copy(BitConverter.GetBytes(m_fileEndTime.Value), 0, image, pointersBinaryLength + 8, 8)
                Array.Copy(BitConverter.GetBytes(m_eventsReceived), 0, image, pointersBinaryLength + 16, 4)
                Array.Copy(BitConverter.GetBytes(m_eventsArchived), 0, image, pointersBinaryLength + 20, 4)
                Array.Copy(BitConverter.GetBytes(m_dataBlockSize), 0, image, pointersBinaryLength + 24, 4)
                Array.Copy(BitConverter.GetBytes(m_dataBlockCount), 0, image, pointersBinaryLength + 28, 4)

                Return image
            End Get
        End Property

        Public ReadOnly Property BinaryDataLength() As Integer Implements IBinaryDataProvider.BinaryDataLength
            Get
                ' We add 10 bytes for the array descriptor that required for reading the file from VB.
                Return (10 + (m_dataBlockCount * ArchiveDataBlockPointer.Size) + MinimumBinaryLength)
            End Get
        End Property

#End Region

#End Region

#Region " Private Code "

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

            Return (dataBlockPointer.PointIndex = m_searchPointIndex AndAlso _
                    (m_searchStartTime.CompareTo(TimeTag.MinValue) = 0 OrElse _
                        dataBlockPointer.StartTime.CompareTo(m_searchStartTime) >= 0) AndAlso _
                    (m_searchEndTime.CompareTo(TimeTag.MaxValue) = 0 OrElse _
                        dataBlockPointer.StartTime.CompareTo(m_searchEndTime) <= 0))

        End Function

#End Region

    End Class

End Namespace