' 02/24/2007

Imports System.IO

Public Class ArchiveDataBlock

    Private m_fileStream As FileStream
    Private m_location As Long
    Private m_size As Integer
    Private m_dataCount As Integer

    Public Sub New(ByVal archiveFileStream As FileStream, ByVal location As Long, ByVal size As Integer)

        MyBase.New()
        m_fileStream = archiveFileStream
        m_location = location
        m_size = size

        Dim ttData As Byte() = CreateArray(Of Byte)(4)
        m_fileStream.Seek(m_location, SeekOrigin.Begin)
        For i As Integer = 1 To Me.Capacity
            m_fileStream.Read(ttData, 0, 4)
            If BitConverter.ToInt32(ttData, 0) = 0 Then
                m_fileStream.Seek(-4, SeekOrigin.Current)
                Exit For
            Else
                m_dataCount = i
                m_fileStream.Seek(6, SeekOrigin.Current)
            End If
        Next

    End Sub

    Public ReadOnly Property Capacity() As Integer
        Get
            Return m_size * 1024 \ StandardPointData.BinaryLength
        End Get
    End Property

    Public ReadOnly Property IsFull() As Boolean
        Get
            Return Me.Capacity - m_dataCount = 0
        End Get
    End Property

    Public Function Read() As List(Of StandardPointData)

        Return Nothing

    End Function

    Public Sub Write(ByVal pointData As StandardPointData)

    End Sub

End Class
