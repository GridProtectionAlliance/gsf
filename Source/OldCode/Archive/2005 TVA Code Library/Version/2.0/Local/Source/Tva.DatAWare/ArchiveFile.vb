' 02/18/2007

Imports System.IO
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.IO.FilePath

<ToolboxBitmap(GetType(ArchiveFile))> _
Public Class ArchiveFile
    Implements ISupportInitialize

#Region " Member Declaration "

    Private m_name As String
    Private m_size As Double
    Private m_blockSize As Integer
    Private m_saveOnClose As Boolean
    Private m_fileStream As FileStream
    Private m_fat As ArchiveFileAllocationTable

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
    Public ReadOnly Property FAT() As ArchiveFileAllocationTable
        Get
            Return m_fat
        End Get
    End Property

    Public Sub Open()

        If File.Exists(m_name) Then
            ' File has been created already, so we just need to read it.
            m_fileStream = New FileStream(m_name, FileMode.Open)
            m_fat = New ArchiveFileAllocationTable(m_fileStream)
            m_size = m_fileStream.Length / (1024 * 1024)
            m_blockSize = m_fat.DataBlockSize
        Else
            ' File does not exist, so we have to create it and initialize it.
            m_fileStream = New FileStream(m_name, FileMode.Create)
            m_fat = New ArchiveFileAllocationTable(m_blockSize, ArchiveFile.MaximumDataBlocks(m_size, m_blockSize))
            ' Leave space for data blocks.
            m_fileStream.Seek(m_fat.DataBlockCount * m_fat.DataBlockSize * 1024, SeekOrigin.Begin)
            m_fileStream.Write(m_fat.BinaryImage, 0, m_fat.BinaryLength)
        End If

    End Sub

    Public Sub Close()

        If m_saveOnClose Then Save()
        If m_fileStream IsNot Nothing Then
            m_fileStream.Close()
            m_fileStream = Nothing
        End If

    End Sub

    Public Sub Save()

    End Sub

    Public Shared Function MaximumDataBlocks(ByVal fileSize As Double, ByVal blockSize As Integer) As Integer

        Return Convert.ToInt32((fileSize * 1024) / blockSize)

    End Function

#Region " ISupportInitialize Implementation "

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        ' We don't need to do anything before the component is initialized.

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If Not DesignMode Then
            Open()
        End If

    End Sub

#End Region

#End Region

End Class
