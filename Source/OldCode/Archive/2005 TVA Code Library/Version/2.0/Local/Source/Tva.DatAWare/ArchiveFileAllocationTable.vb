' 02/18/2007

Imports System.IO

Public Class ArchiveFileAllocationTable

    ' *******************************************************
    ' *                     FAT structure                   *
    ' *******************************************************

    Private m_dataBlockPointers As List(Of ArchiveDataBlockPointer)
    Private m_fileStartTime As TimeTag
    Private m_fileEndTime As TimeTag
    Private m_eventsReceived As Integer
    Private m_eventsArchived As Integer
    Private m_dataBlockSize As Integer
    Private m_dataBlockCount As Integer

    Private Sub New()

        MyBase.New()
        m_dataBlockPointers = New List(Of ArchiveDataBlockPointer)()

    End Sub

    Public Sub New(ByVal archiveFileStream As Stream)

        MyClass.New()
        If archiveFileStream IsNot Nothing Then
            Dim cursorPosition As Long = archiveFileStream.Position
            Dim fixedFatData As Byte() = CreateArray(Of Byte)(32)
            archiveFileStream.Seek(-fixedFatData.Length, SeekOrigin.End)
            archiveFileStream.Read(fixedFatData, 0, fixedFatData.Length)
            m_fileStartTime = New TimeTag(BitConverter.ToDouble(fixedFatData, 0))
            m_fileEndTime = New TimeTag(BitConverter.ToDouble(fixedFatData, 8))
            m_eventsReceived = BitConverter.ToInt32(fixedFatData, 16)
            m_eventsArchived = BitConverter.ToInt32(fixedFatData, 20)
            m_dataBlockSize = BitConverter.ToInt32(fixedFatData, 24)
            m_dataBlockCount = BitConverter.ToInt32(fixedFatData, 28)

            Dim variableFatData As Byte() = CreateArray(Of Byte)(m_dataBlockCount * ArchiveDataBlockPointer.BinaryLength)
            archiveFileStream.Seek(-(variableFatData.Length + fixedFatData.Length), SeekOrigin.End)
            archiveFileStream.Read(variableFatData, 0, variableFatData.Length)
            For i As Integer = 0 To variableFatData.Length - 1 Step ArchiveDataBlockPointer.BinaryLength
                m_dataBlockPointers.Add(New ArchiveDataBlockPointer(variableFatData, i))
            Next

            archiveFileStream.Seek(cursorPosition, SeekOrigin.Begin)
        Else
            Throw New ArgumentNullException("archiveFileStream")
        End If

    End Sub

    Public ReadOnly Property DataBlockPointers() As List(Of ArchiveDataBlockPointer)
        Get
            Return m_dataBlockPointers
        End Get
    End Property

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

    Public ReadOnly Property BinaryImage() As Byte()
        Get

        End Get
    End Property

    Public Function GetDataBlockLocation(ByVal pointID As Integer, ByVal startTime As TimeTag) As Long

    End Function

    Public Function GetDataBlockLocation(ByVal dataBlockPointer As ArchiveDataBlockPointer) As Long

        Dim pointerIndex As Integer = m_dataBlockPointers.IndexOf(dataBlockPointer)
        If pointerIndex >= 0 Then
            Return pointerIndex * (m_dataBlockSize * 1024)
        Else
            Return -1
        End If

    End Function

End Class
