' 02/24/2007

Imports System.IO

Public Class ArchiveDataBlock

    Private m_location As Long
    Private m_size As Integer
    Private m_fileStream As FileStream
    Private m_writeCursor As Long

    Public Sub New(ByVal archiveFileStream As FileStream, ByVal location As Long, ByVal size As Integer)

        MyBase.New()
        m_fileStream = archiveFileStream
        m_location = location
        m_size = size
        m_writeCursor = location    ' This is where we'll start writing data to begin with.

    End Sub

    Public ReadOnly Property Location() As Long
        Get
            Return m_location
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>In KB</remarks>
    Public ReadOnly Property Size() As Integer
        Get
            Return m_size
        End Get
    End Property

    Public ReadOnly Property Capacity() As Integer
        Get
            Return (m_size * 1024) \ StandardPointData.BinaryLength
        End Get
    End Property

    Public ReadOnly Property SlotsUsed() As Integer
        Get
            Return Convert.ToInt32((m_writeCursor - m_location) \ StandardPointData.BinaryLength)
        End Get
    End Property

    Public ReadOnly Property SlotsAvailable() As Integer
        Get
            Return Me.Capacity - Me.SlotsUsed
        End Get
    End Property

    Public Function Read() As List(Of StandardPointData)

        Dim data As New List(Of StandardPointData)()

        ' We'll start reading from where the data block begins.
        m_fileStream.Seek(m_location, SeekOrigin.Begin)

        Dim binaryImage As Byte() = CreateArray(Of Byte)(StandardPointData.BinaryLength)
        For i As Integer = 1 To Me.Capacity
            ' Read the binary data from the file and create StandardPointData instance form it.
            m_fileStream.Read(binaryImage, 0, binaryImage.Length)
            Dim pointData As New StandardPointData(binaryImage)
            If Not pointData.IsNull Then
                data.Add(pointData)
            Else
                ' The data we just read is blank so we'll roll-back the cursor and stop reading further.
                m_fileStream.Seek(-binaryImage.Length, SeekOrigin.Current)
                Exit For
            End If
        Next
        If m_writeCursor <> m_fileStream.Position Then m_writeCursor = m_fileStream.Position

        Return data

    End Function

    Public Sub Write(ByVal pointData As StandardPointData)

        If Me.SlotsAvailable > 0 Then
            ' We have enough space to write the provided point data to the data block.
            'm_fileStream.Seek(m_writeCursor - m_fileStream.Position, SeekOrigin.Current)   ' This is slower than
            m_fileStream.Seek(m_writeCursor, SeekOrigin.Begin)                              ' <--------------This
            m_fileStream.Write(pointData.BinaryImage, 0, StandardPointData.BinaryLength)
            m_writeCursor = m_fileStream.Position   ' Update the write cursor.
        Else
            Throw New InvalidOperationException("No slots available for writing new data.")
        End If

    End Sub

End Class
