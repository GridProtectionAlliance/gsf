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
    Private m_rolloverOnFull As Boolean
    Private m_fat As ArchiveFileAllocationTable
    Private m_fileStream As FileStream
    Private m_activeDataBlocks As Dictionary(Of Integer, ArchiveDataBlock)

#End Region

#Region " Event Declaration "

    Public Event DataReceived As EventHandler
    Public Event DataArchived As EventHandler
    Public Event DataDiscarded As EventHandler
    Public Event FileFull As EventHandler

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

    Public Property RolloverOnFull() As Boolean
        Get
            Return m_rolloverOnFull
        End Get
        Set(ByVal value As Boolean)
            m_rolloverOnFull = value
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
            Dim fileName As String = AbsolutePath(m_name)
            If File.Exists(fileName) Then
                ' File has been created already, so we just need to read it.
                m_fileStream = New FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)
                m_fat = New ArchiveFileAllocationTable(m_fileStream)
            Else
                ' File does not exist, so we have to create it and initialize it.
                m_fileStream = New FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read)
                m_fat = New ArchiveFileAllocationTable(m_fileStream, m_blockSize, MaximumDataBlocks(m_size, m_blockSize))
                m_fat.Persist()
            End If
            m_activeDataBlocks = New Dictionary(Of Integer, ArchiveDataBlock)()
        End If
        
    End Sub

    Public Sub Close()

        If Me.IsOpen Then
            If m_saveOnClose Then Save()

            m_fat = Nothing
            m_activeDataBlocks.Clear()
            m_activeDataBlocks = Nothing
            m_fileStream.Close()
            m_fileStream = Nothing
        End If

    End Sub

    Public Sub Save()

        ' The only thing that we need to write back to the file is the FAT.
        m_fat.Persist()

    End Sub

    Public Sub Rollover()

        ' Update FAT's start & end time
        ' Close()
        ' Rename the File
        ' Open()

    End Sub

    Public Function Read(ByVal pointIndex As Integer) As List(Of StandardPointData)

        Return Read(pointIndex, TimeTag.MinValue)

    End Function

    Public Function Read(ByVal pointIndex As Integer, ByVal startTime As TimeTag) As List(Of StandardPointData)

        Return Read(pointIndex, startTime, TimeTag.MaxValue)

    End Function

    Public Function Read(ByVal pointIndex As Integer, ByVal startTime As TimeTag, ByVal endTime As TimeTag) As List(Of StandardPointData)

        Dim data As New List(Of StandardPointData)()
        Dim foundBlocks As List(Of ArchiveDataBlock) = m_fat.FindDataBlocks(pointIndex, startTime, endTime)
        For i As Integer = 0 To foundBlocks.Count - 1
            data.AddRange(foundBlocks(i).Read())
        Next

        Return data

    End Function

    Public Sub Write(ByVal pointData As StandardPointData)

        If pointData.Definition IsNot Nothing Then
            m_fat.EventsReceived += 1

            If ToBeArchived(pointData) Then
                Dim dataBlock As ArchiveDataBlock = Nothing
                m_activeDataBlocks.TryGetValue(pointData.Definition.Index, dataBlock)
                If dataBlock Is Nothing OrElse (dataBlock IsNot Nothing AndAlso dataBlock.SlotsAvailable = 0) Then
                    ' We either don't have a active data block where we can archive the point data or we have a 
                    ' active data block but it is full, so we have to request a new data block from the FAT.
                    m_activeDataBlocks.Remove(pointData.Definition.Index)
                    dataBlock = m_fat.RequestDataBlock(pointData.Definition.Index, pointData.TimeTag)
                    m_activeDataBlocks.Add(pointData.Definition.Index, dataBlock)
                End If

                If dataBlock IsNot Nothing Then
                    ' We were able to obtain a data block for writing data.
                    dataBlock.Write(pointData)

                    m_fat.EventsArchived += 1
                    m_fat.FileEndTime = pointData.TimeTag
                    If m_fat.FileStartTime.CompareTo(TimeTag.MinValue) = 0 Then m_fat.FileStartTime = pointData.TimeTag
                Else
                    ' We were unable to obtain a data block for writing data to because all data block are in use.
                    RaiseEvent FileFull(Me, EventArgs.Empty)
                End If
            Else

            End If
        Else
            Throw New ArgumentException("Definition property for point data is not set.")
        End If

    End Sub

    Public Shared Function MaximumDataBlocks(ByVal fileSize As Double, ByVal blockSize As Integer) As Integer

        Return Convert.ToInt32((fileSize * 1024) / blockSize)

    End Function

#End Region

#Region " Private Code "

    Private Function ToBeArchived(ByVal pointDate As StandardPointData) As Boolean

        ' TODO: Perform compression check here.
        Return True

    End Function

#End Region

End Class
