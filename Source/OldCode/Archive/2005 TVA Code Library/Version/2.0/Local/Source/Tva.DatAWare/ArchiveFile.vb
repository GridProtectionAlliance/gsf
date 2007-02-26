' 02/18/2007

Imports System.IO
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.IO.FilePath

<ToolboxBitmap(GetType(ArchiveFile))> _
Public Class ArchiveFile

#Region " Member Declaration "

    Private m_name As String
    Private m_size As Double
    Private m_blockSize As Integer
    Private m_saveOnClose As Boolean
    Private m_fat As ArchiveFileAllocationTable
    Private m_fileStream As FileStream
    Private m_activeDataBlocks As Dictionary(Of Integer, ArchiveDataBlock)

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

        If Not Me.IsOpen Then
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
            m_size = m_fileStream.Length / (1024 * 1024)
            m_blockSize = m_fat.DataBlockSize
        End If
        
    End Sub

    Public Sub Close()

        If Me.IsOpen Then
            If m_saveOnClose Then Save()

            m_fileStream.Close()
            m_fileStream = Nothing
        End If

    End Sub

    Public Sub Save()

        ' The only thing that we need to write back to the file is the FAT.
        m_fat.Persist()

    End Sub

    Public Function Read(ByVal pointIndex As Integer) As List(Of StandardPointData)

        Return Nothing

    End Function

    Public Function Read(ByVal pointIndex As Integer, ByVal startTime As System.DateTime) As List(Of StandardPointData)

        Return Nothing

    End Function

    Public Function Read(ByVal pointIndex As Integer, ByVal startTime As System.DateTime, ByVal endTime As System.DateTime) As List(Of StandardPointData)

        ' Use m_fat.FindDataBlocks(...) function here.
        Return Nothing

    End Function

    Public Sub Write(ByVal pointData As StandardPointData)

        If pointData.Definition IsNot Nothing Then
            ' TODO: Perform compression here.
            Dim dataBlock As ArchiveDataBlock = m_activeDataBlocks(pointData.Definition.Index)
            'If (dataBlock IsNot Nothing AndAlso dataBlock.IsFull) OrElse dataBlock Is Nothing Then
            '    ' We either don't have a active data block where we can archive the point data or we have a active
            '    ' data block but it is full, so we have to request a new data block from the FAT.
            '    dataBlock = m_fat.RequestDataBlock(pointData.Definition.Index, pointData.TTag)
            '    m_activeDataBlocks(pointData.Definition.Index) = dataBlock
            'End If
        Else
            Throw New ArgumentException("Definition property for point data is not set.")
        End If

    End Sub

    Public Shared Function MaximumDataBlocks(ByVal fileSize As Double, ByVal blockSize As Integer) As Integer

        Return Convert.ToInt32((fileSize * 1024) / blockSize)

    End Function

#End Region

End Class
