' 02/24/2007

Imports System.IO

Public Class ArchiveDataBlock

    Private m_location As Long
    Private m_size As Integer
    Private m_fileStream As FileStream
    Private m_writeCursor As Long
    'Private m_pointDataCount As Integer

    Public Sub New(ByVal archiveFileStream As FileStream, ByVal location As Long, ByVal size As Integer)

        MyBase.New()
        m_fileStream = archiveFileStream
        m_location = location
        m_size = size
        m_writeCursor = location    ' This is where we'll start writing data to begin with.

        ' This is useful information but VERY VERY time consuming...
        'Dim ttData As Byte() = CreateArray(Of Byte)(4)
        'm_fileStream.Seek(m_location, SeekOrigin.Begin)
        'For i As Integer = 1 To Me.Capacity
        '    m_fileStream.Read(ttData, 0, 4)
        '    If BitConverter.ToInt32(ttData, 0) = 0 Then
        '        m_fileStream.Seek(-4, SeekOrigin.Current)
        '        Exit For
        '    Else
        '        m_pointDataCount = i
        '        m_fileStream.Seek(6, SeekOrigin.Current)
        '    End If
        'Next

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
            Return m_size * 1024 \ StandardPointData.BinaryLength
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
        m_fileStream.Seek(m_writeCursor, SeekOrigin.Begin)

        Return data

    End Function

    Public Sub Write(ByVal pointData As StandardPointData)

        If m_writeCursor + StandardPointData.BinaryLength <= m_size * 1024 Then
            ' We have enough space to write the provided point data to the data block.
            m_fileStream.Seek(m_writeCursor, SeekOrigin.Begin)
            m_fileStream.Write(pointData.BinaryImage, 0, StandardPointData.BinaryLength)
            m_writeCursor = m_fileStream.Position
        Else
            Throw New ApplicationException("")
        End If

    End Sub

End Class
