' 02/15/2007

Imports System.IO
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.IO.FilePath

<ToolboxBitmap(GetType(MetadataFile))> _
Public Class MetadataFile
    Implements ISupportInitialize

#Region " Member Declaration "

    Private m_file As FileStream
    Private m_name As String
    Private m_keepOpen As Boolean
    Private m_initialRecordCount As Integer
    Private m_pointDefinitions As List(Of PointDefinition)

#End Region

    Public Const FileExtension As String = ".dat"

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                If String.Compare(JustFileExtension(value), MetadataFile.FileExtension) = 0 Then
                    m_name = value
                Else
                    Throw New ArgumentException(String.Format("Name of {0} must have an extension of {1}.", Me.GetType().Name, MetadataFile.FileExtension))
                End If
            Else
                Throw New ArgumentNullException("Name")
            End If
        End Set
    End Property

    Public Property KeepOpen() As Boolean
        Get
            Return m_keepOpen
        End Get
        Set(ByVal value As Boolean)
            m_keepOpen = value
        End Set
    End Property

    <Browsable(False)> _
    Public ReadOnly Property IsOpen() As Boolean
        Get
            Return (m_file IsNot Nothing)
        End Get
    End Property

    <Browsable(False)> _
    Public ReadOnly Property PointDefinitions() As List(Of PointDefinition)
        Get
            Return m_pointDefinitions
        End Get
    End Property

    Public Sub Open()

        If m_file Is Nothing Then
            m_file = New FileStream(m_name, FileMode.OpenOrCreate)
        End If

    End Sub

    Public Sub Close()

        If m_file IsNot Nothing Then
            m_file.Close()
            m_file.Dispose()
            m_file = Nothing
        End If

    End Sub

    Public Sub ReadPointDefinitions()

        If Not Me.IsOpen Then Open()

        If m_file.Length Mod PointDefinition.BinaryLength = 0 Then
            If m_pointDefinitions.Count = 0 Then
                ' We'll read the file, since we've not read the file yet.
                m_file.Seek(0, SeekOrigin.Begin)    ' Set the cursor to BOF before we start reading the file.
                Dim binaryImage As Byte() = CreateArray(Of Byte)(PointDefinition.BinaryLength)
                For i As Long = 0 To m_file.Length - 1 Step pointDefinition.BinaryLength
                    m_file.Read(binaryImage, 0, binaryImage.Length)
                    Dim pointDefinition As New PointDefinition(m_pointDefinitions.Count + 1, binaryImage, 0)
                    m_pointDefinitions.Add(pointDefinition)
                Next
                m_file.Seek(0, SeekOrigin.Begin)    ' Set the cursor to BOF after we're done reading the file.
                m_initialRecordCount = m_pointDefinitions.Count
            End If
        Else
            Throw New InvalidOperationException(String.Format("File """"{0}"""" is corrupt.", m_name))
        End If

        If Not m_keepOpen Then Close()

    End Sub

    Public Sub WritePointDefinitions()

        AnalyzePointDefinitions()

        If m_pointDefinitions.Count >= m_initialRecordCount Then
            ' We have at least (if not more) the number of points we read in from the file to begin with.
            If Not Me.IsOpen Then Open()

            m_file.Seek(0, SeekOrigin.Begin)    ' Set the cursor to BOF before we start writing to the file.
            For Each pointDefinition As PointDefinition In m_pointDefinitions
                m_file.Write(pointDefinition.BinaryImage, 0, pointDefinition.BinaryLength)
            Next
            m_file.Flush()
            m_file.Seek(0, SeekOrigin.Begin)    ' Set the cursor to BOF after we're done writing to the file.

            If Not m_keepOpen Then Close()
        Else
            Throw New InvalidOperationException(String.Format("Number of point definition records for file ""{0}"" must be at least {1}.", m_name, m_initialRecordCount))
        End If

    End Sub

    Public Sub AnalyzePointDefinitions()

        ' First, we'll sort our point definition list to ensure that all the point definitions are sorted by index.
        Dim nonAlignedPointDefinitions As New List(Of PointDefinition)(m_pointDefinitions)
        nonAlignedPointDefinitions.Sort()
        m_pointDefinitions.Clear()
        For Each pointDefinition As PointDefinition In nonAlignedPointDefinitions
            AddPointDefinition(pointDefinition)
        Next

    End Sub

    Public Sub AddPointDefinition(ByVal pointDefinition As PointDefinition)

        ' Insert to the in-memory list of point definitions.
        If Not m_pointDefinitions.Contains(pointDefinition) Then
            If pointDefinition.Index > m_pointDefinitions.Count + 1 Then
                ' We must add blank definitions as place holders before inserting the specified point definition.
                For i As Integer = m_pointDefinitions.Count + 1 To pointDefinition.Index - 1
                    m_pointDefinitions.Add(New PointDefinition(i))
                Next
            End If

            m_pointDefinitions.Add(pointDefinition)
        Else
            Throw New InvalidOperationException(String.Format("Point definition already exists at index {0}.", pointDefinition.Index))
        End If

    End Sub

    Public Function GetPointDefinition(ByVal pointIndex As Integer) As PointDefinition

        For Each pointDefinition As PointDefinition In m_pointDefinitions
            If pointDefinition.Index = pointIndex Then
                Return pointDefinition
            End If
        Next

        Return Nothing

    End Function

    Public Function GetPointDefinition(ByVal pointName As String) As PointDefinition

        For Each pointDefinition As PointDefinition In m_pointDefinitions
            If String.Compare(pointName, pointDefinition.Name) = 0 OrElse _
                    String.Compare(pointName, pointDefinition.Synonym1) = 0 OrElse _
                    String.Compare(pointName, pointDefinition.Synonym2) = 0 Then
                Return pointDefinition
            End If
        Next

        Return Nothing

    End Function

#Region " ISupportInitialize Implementation "

    Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        ' We don't need to do anything before the component is initialized.

    End Sub

    Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

        If Not DesignMode Then
            Open()
            ReadPointDefinitions()
        End If

    End Sub

#End Region

End Class
