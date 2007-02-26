' 02/18/2007

Imports System.IO

Public Class ArchiveFileAllocationTable

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
    Private m_dataBlocksScanned As List(Of Integer) ' ???
    Private m_searchPointIndex As Integer   ' <=|
    Private m_searchStartTime As TimeTag    ' <=| Used for finding data block pointer in m_dataBlockPointers 
    Private m_searchEndTime As TimeTag      ' <=|

    Private Const MinimumBinaryLength As Integer = 32

#End Region

    Private Sub New()

        MyBase.New()
        m_fileStartTime = New TimeTag(System.DateTime.Now)
        m_fileEndTime = New TimeTag(0D)
        m_dataBlockPointers = New List(Of ArchiveDataBlockPointer)()
        m_dataBlocksScanned = New List(Of Integer)()

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

            Dim variableFatData As Byte() = CreateArray(Of Byte)(m_dataBlockCount * ArchiveDataBlockPointer.BinaryLength)
            m_fileStream.Seek(-(variableFatData.Length + fixedFatData.Length), SeekOrigin.End)
            m_fileStream.Read(variableFatData, 0, variableFatData.Length)
            For i As Integer = 0 To variableFatData.Length - 1 Step ArchiveDataBlockPointer.BinaryLength
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
    Public Property DataBlockSize() As Integer
        Get
            Return m_dataBlockSize
        End Get
        Set(ByVal value As Integer)
            m_dataBlockSize = value
        End Set
    End Property

    Public Property DataBlockCount() As Integer
        Get
            Return m_dataBlockCount
        End Get
        Set(ByVal value As Integer)
            m_dataBlockCount = value
        End Set
    End Property

    Public ReadOnly Property DataBlockPointers() As List(Of ArchiveDataBlockPointer)
        Get
            Return m_dataBlockPointers
        End Get
    End Property

    Public Sub Persist()

        ' Leave space for data blocks.
        m_fileStream.Seek(m_dataBlockCount * m_dataBlockSize * 1024, SeekOrigin.Begin)
        m_fileStream.Write(BinaryImage, 0, BinaryLength)
        m_fileStream.Flush()

    End Sub

    Public Function RequestDataBlock(ByVal pointIndex As Integer, ByVal startTime As TimeTag) As ArchiveDataBlock

        ' TODO: Check allocated data blocks for empty space first.
        If Not m_dataBlocksScanned.Contains(pointIndex) Then

        End If

        Dim unusedPointerIndex As Integer = m_dataBlockPointers.IndexOf(New ArchiveDataBlockPointer())
        m_dataBlockPointers(unusedPointerIndex).PointIndex = pointIndex
        m_dataBlockPointers(unusedPointerIndex).StartTime = startTime
        Return GetDataBlock(m_dataBlockPointers(unusedPointerIndex))

    End Function

    Public Function GetDataBlock(ByVal pointID As Integer, ByVal startTime As TimeTag) As ArchiveDataBlock

        Return GetDataBlock(New ArchiveDataBlockPointer(pointID, startTime))

    End Function

    Public Function GetDataBlock(ByVal blockPointer As ArchiveDataBlockPointer) As ArchiveDataBlock

        Return New ArchiveDataBlock(m_fileStream, GetDataBlockLocation(blockPointer), m_dataBlockSize)

    End Function

    Public Function GetDataBlocks(ByVal pointIndex As Integer) As List(Of ArchiveDataBlock)

        Return GetDataBlocks(pointIndex, TimeTag.MinValue, TimeTag.MaxValue)

    End Function

    Public Function GetDataBlocks(ByVal pointIndex As Integer, ByVal startTime As TimeTag, ByVal endTime As TimeTag) As List(Of ArchiveDataBlock)

        Dim matchingBlocks As List(Of ArchiveDataBlock) = Nothing
        Dim matchingPointers As List(Of ArchiveDataBlockPointer) = Nothing

        m_searchPointIndex = pointIndex
        m_searchStartTime = IIf(startTime IsNot Nothing, startTime, TimeTag.MinValue)
        m_searchEndTime = IIf(endTime IsNot Nothing, endTime, TimeTag.MaxValue)
        matchingPointers = m_dataBlockPointers.FindAll(AddressOf FindDataBlockPointer)

        Return Nothing

    End Function

#End Region

#Region " Private Code "

    Private ReadOnly Property BinaryLength() As Integer
        Get
            Return (m_dataBlockPointers.Count * ArchiveDataBlockPointer.BinaryLength) + MinimumBinaryLength
        End Get
    End Property

    Private ReadOnly Property BinaryImage() As Byte()
        Get
            Dim pointersBinaryLength As Integer = Me.BinaryLength - MinimumBinaryLength
            Dim image As Byte() = CreateArray(Of Byte)(pointersBinaryLength + MinimumBinaryLength)

            For i As Integer = 0 To m_dataBlockPointers.Count - 1
                Array.Copy(m_dataBlockPointers(i).BinaryImage, 0, image, i * ArchiveDataBlockPointer.BinaryLength, ArchiveDataBlockPointer.BinaryLength)
            Next
            Array.Copy(BitConverter.GetBytes(m_fileStartTime.Value), 0, image, pointersBinaryLength, 8)
            Array.Copy(BitConverter.GetBytes(m_fileEndTime.Value), 0, image, pointersBinaryLength + 8, 8)
            Array.Copy(BitConverter.GetBytes(m_eventsReceived), 0, image, pointersBinaryLength + 16, 4)
            Array.Copy(BitConverter.GetBytes(m_eventsArchived), 0, image, pointersBinaryLength + 20, 4)
            Array.Copy(BitConverter.GetBytes(m_dataBlockSize), 0, image, pointersBinaryLength + 24, 4)
            Array.Copy(BitConverter.GetBytes(m_dataBlockCount), 0, image, pointersBinaryLength + 28, 4)

            Return image
        End Get
    End Property

    Private Function GetDataBlockLocation(ByVal pointID As Integer, ByVal startTime As TimeTag) As Long

        Return GetDataBlockLocation(New ArchiveDataBlockPointer(pointID, startTime))

    End Function

    Private Function GetDataBlockLocation(ByVal dataBlockPointer As ArchiveDataBlockPointer) As Long

        Dim pointerIndex As Integer = m_dataBlockPointers.IndexOf(dataBlockPointer)
        If pointerIndex >= 0 Then
            Return pointerIndex * (m_dataBlockSize * 1024)
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
