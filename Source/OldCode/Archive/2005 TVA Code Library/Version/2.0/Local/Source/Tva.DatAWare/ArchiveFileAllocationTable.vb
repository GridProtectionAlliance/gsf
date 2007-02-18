' 02/18/2007

Imports System.IO

Public Class ArchiveFileAllocationTable

    Public Sub New(ByVal archiveFileStream As Stream)

    End Sub

    Public ReadOnly Property BlockPointers() As List(Of ArchiveDataBlockPointer)
        Get

        End Get
    End Property

    Public Property FileStartTime() As TimeTag
        Get

        End Get
        Set(ByVal value As TimeTag)

        End Set
    End Property

    Public Property FileEndTime() As TimeTag
        Get

        End Get
        Set(ByVal value As TimeTag)

        End Set
    End Property

    Public Property EventsReceived() As Integer
        Get

        End Get
        Set(ByVal value As Integer)

        End Set
    End Property

    Public Property EventsArchived() As Integer
        Get

        End Get
        Set(ByVal value As Integer)

        End Set
    End Property

    Public Property DataBlockSize() As Integer
        Get

        End Get
        Set(ByVal value As Integer)

        End Set
    End Property

    Public Property DataBlockCount() As Integer
        Get

        End Get
        Set(ByVal value As Integer)

        End Set
    End Property

End Class
