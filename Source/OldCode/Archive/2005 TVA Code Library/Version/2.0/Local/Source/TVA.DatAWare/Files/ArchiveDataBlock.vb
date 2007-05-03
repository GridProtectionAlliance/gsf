' 02/24/2007

Imports System.IO

Namespace Files

    Public Class ArchiveDataBlock
        Implements IDisposable

        Private m_index As Integer
        Private m_size As Integer
        Private m_isForHistoricData As Boolean
        Private m_fileStream As FileStream
        'Private m_writeCursor As Long

        Public Sub New(ByVal index As Integer, ByVal size As Integer, ByVal archiveFileName As String)

            MyClass.New(index, size, archiveFileName, False)

        End Sub

        Public Sub New(ByVal index As Integer, ByVal size As Integer, ByVal archiveFileName As String, ByVal isForHistoricData As Boolean)

            MyBase.New()
            m_index = index
            m_size = size
            m_isForHistoricData = isForHistoricData
            m_fileStream = New FileStream(archiveFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            m_fileStream.Seek(Location, SeekOrigin.Begin)

        End Sub

        Public ReadOnly Property Index() As Integer
            Get
                Return m_index
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

        Public ReadOnly Property Location() As Long
            Get
                Return (m_index * (m_size * 1024L))
            End Get
        End Property

        Public ReadOnly Property Capacity() As Integer
            Get
                Return ((m_size * 1024) \ StandardPointData.Size)
            End Get
        End Property

        Public ReadOnly Property SlotsUsed() As Integer
            Get
                Return Convert.ToInt32((m_fileStream.Position - Location) \ StandardPointData.Size)
            End Get
        End Property

        Public ReadOnly Property SlotsAvailable() As Integer
            Get
                Return Capacity - SlotsUsed
            End Get
        End Property

        Public Property IsForHistoricData() As Boolean
            Get
                Return m_isForHistoricData
            End Get
            Set(ByVal value As Boolean)
                m_isForHistoricData = value
            End Set
        End Property

        Public Function Read() As List(Of StandardPointData)

            Dim data As New List(Of StandardPointData)()

            ' We'll start reading from where the data block begins.
            m_fileStream.Seek(Location, SeekOrigin.Begin)

            Dim binaryImage As Byte() = CreateArray(Of Byte)(StandardPointData.Size)
            For i As Integer = 1 To Capacity
                ' Read the binary data from the file and create StandardPointData instance form it.
                m_fileStream.Read(binaryImage, 0, binaryImage.Length)
                Dim pointData As New StandardPointData(binaryImage)
                If Not pointData.IsEmpty Then
                    data.Add(pointData)
                Else
                    ' The data we just read is blank so we'll roll-back the cursor and stop reading further.
                    m_fileStream.Seek(-binaryImage.Length, SeekOrigin.Current)
                    Exit For
                End If
            Next
            'If m_writeCursor <> m_fileStream.Position Then m_writeCursor = m_fileStream.Position

            Return data

        End Function

        Public Sub Write(ByVal pointData As StandardPointData)

            If SlotsAvailable > 0 Then
                ' We have enough space to write the provided point data to the data block.
                'm_fileStream.Seek(m_writeCursor - m_fileStream.Position, SeekOrigin.Current)   ' This is slower than
                'm_fileStream.Seek(m_writeCursor, SeekOrigin.Begin)                              ' <--------------This
                m_fileStream.Write(pointData.BinaryImage, 0, StandardPointData.Size)
                'm_writeCursor = m_fileStream.Position   ' Update the write cursor.
                'm_fileStream.BeginWrite(pointData.BinaryData, 0, StandardPointData.Size, Nothing, Nothing)
            Else
                Throw New InvalidOperationException("No slots available for writing new data.")
            End If

        End Sub

        Public Sub Scan()

            Read()

        End Sub

        Public Sub Initialize()

            m_fileStream.Seek(Location, SeekOrigin.Begin)
            For i As Integer = 1 To Capacity
                Write(New StandardPointData())
            Next
            m_fileStream.Seek(Location, SeekOrigin.Begin)

        End Sub

#Region " Interface Implementations "

#Region " IDisposable "

        Private m_disposed As Boolean = False        ' To detect redundant calls

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)

            If Not m_disposed Then
                ' Dispose unmanaged resources.
                If disposing Then
                    ' Dispose managed resources.
                    m_fileStream.Dispose()
                End If
            End If
            m_disposed = True

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
#End Region

    End Class

End Namespace