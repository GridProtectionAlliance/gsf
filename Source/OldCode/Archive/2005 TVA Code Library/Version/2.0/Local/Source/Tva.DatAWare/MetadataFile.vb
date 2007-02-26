' 02/15/2007

Imports System.IO
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.IO.FilePath

<ToolboxBitmap(GetType(MetadataFile))> _
Public Class MetadataFile

#Region " Member Declaration "

    Private m_name As String
    Private m_initialRecordCount As Integer
    Private m_saveOnClose As Boolean
    Private m_analyzeOnSave As Boolean
    Private m_autoSaveInterval As Integer
    Private m_autoAnalyzeInterval As Integer
    Private m_pointDefinitions As List(Of PointDefinition)
    Private m_fileStream As FileStream

    Private WithEvents m_autoSaveTimer As System.Timers.Timer
    Private WithEvents m_autoAnalyzeTimer As System.Timers.Timer

#End Region

#Region " Event Declaration "

    Public Event Opening As EventHandler
    Public Event Opened As EventHandler
    Public Event Closing As EventHandler
    Public Event Closed As EventHandler
    Public Event Loading As EventHandler(Of ProgressEventArgs(Of Integer))
    Public Event Saving As EventHandler(Of ProgressEventArgs(Of Integer))
    Public Event Analyzing As EventHandler(Of ProgressEventArgs(Of Integer))

#End Region

#Region " Public Code "

    Public Const Extension As String = ".dat"

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                If String.Compare(JustFileExtension(value), Extension) = 0 Then
                    m_name = value
                Else
                    Throw New ArgumentException(String.Format("Name must have an extension of {0}.", Extension))
                End If
            Else
                Throw New ArgumentNullException("Name")
            End If
        End Set
    End Property

    Public Property InitialRecordCount() As Integer
        Get
            Return m_initialRecordCount
        End Get
        Set(ByVal value As Integer)
            m_initialRecordCount = value
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

    Public Property AnalyzeOnSave() As Boolean
        Get
            Return m_analyzeOnSave
        End Get
        Set(ByVal value As Boolean)
            m_analyzeOnSave = value
        End Set
    End Property

    Public Property AutoSaveInterval() As Integer
        Get
            Return m_autoSaveInterval
        End Get
        Set(ByVal value As Integer)
            m_autoSaveInterval = value
        End Set
    End Property

    Public Property AutoAnalyzeInterval() As Integer
        Get
            Return m_autoAnalyzeInterval
        End Get
        Set(ByVal value As Integer)
            m_autoAnalyzeInterval = value
        End Set
    End Property

    <Browsable(False)> _
    Public ReadOnly Property IsOpen() As Boolean
        Get
            Return m_fileStream IsNot Nothing
        End Get
    End Property

    <Browsable(False)> _
    Public ReadOnly Property PointDefinitions() As List(Of PointDefinition)
        Get
            Return m_pointDefinitions
        End Get
    End Property

    Public Sub Open()

        If Not Me.IsOpen Then
            RaiseEvent Opening(Me, EventArgs.Empty)

            ' Initialize the point definition list.
            m_pointDefinitions = New List(Of PointDefinition)()

            If File.Exists(m_name) Then
                ' File exists, so we'll open it.
                m_fileStream = New FileStream(m_name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)

                ' Once we have the file open, we'll process the file data.
                If m_fileStream.Length Mod PointDefinition.BinaryLength = 0 Then
                    ' The file we're working with is a valid one.
                    Dim binaryImage As Byte() = CreateArray(Of Byte)(PointDefinition.BinaryLength)
                    Dim pointDefinitionCount As Integer = Convert.ToInt32(m_fileStream.Length \ binaryImage.Length)
                    For i As Integer = 1 To pointDefinitionCount
                        m_fileStream.Read(binaryImage, 0, binaryImage.Length)
                        m_pointDefinitions.Add(New PointDefinition(i, binaryImage))
                        RaiseEvent Loading(Me, New ProgressEventArgs(Of Integer)(pointDefinitionCount, i))
                    Next
                Else
                    Close(False)
                    Throw New InvalidOperationException(String.Format("File """"{0}"""" is corrupt.", m_fileStream.Name))
                End If
            Else
                ' File doesn't exist, so we'll create it.
                m_fileStream = New FileStream(m_name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)

                ' Since we're working with a new file, we'll populate the point definition list with the default
                ' number of point definitions. These points will be witten back to the file when Save() is called
                ' or Close() is called and SaveOnClose is set to True.
                For i As Integer = 1 To m_initialRecordCount
                    m_pointDefinitions.Add(New PointDefinition(i))
                Next
            End If

            If m_autoSaveInterval > 0 Then
                m_autoSaveTimer.Interval = m_autoSaveInterval
                m_autoSaveTimer.Start()
            End If
            If m_autoAnalyzeInterval > 0 Then
                m_autoAnalyzeTimer.Interval = m_autoAnalyzeInterval
                m_autoAnalyzeTimer.Start()
            End If

            RaiseEvent Opened(Me, EventArgs.Empty)
        End If

    End Sub

    Public Sub Close()

        Close(m_saveOnClose)

    End Sub

    Public Sub Close(ByVal saveFile As Boolean)

        If Me.IsOpen Then
            RaiseEvent Closing(Me, EventArgs.Empty)

            ' Stop the timers if they are ticking.
            m_autoSaveTimer.Stop()
            m_autoAnalyzeTimer.Stop()

            ' Save point definitions back to the file if specified.
            If saveFile Then Save()

            ' Release all of the used resources.
            m_pointDefinitions.Clear()
            m_pointDefinitions = Nothing
            m_fileStream.Close()
            m_fileStream = Nothing

            RaiseEvent Closed(Me, EventArgs.Empty)
        End If

    End Sub

    Public Sub Save()

        If Me.IsOpen Then
            ' Analyze point definitions before writing them to the file if specified.
            If m_analyzeOnSave Then Analyze()

            ' Set the cursor to BOF before we start writing to the file.
            m_fileStream.Seek(0, SeekOrigin.Begin)
            ' Write all of the point definitions to the file.
            For i As Integer = 0 To m_pointDefinitions.Count - 1
                m_fileStream.Write(m_pointDefinitions(i).BinaryImage, 0, PointDefinition.BinaryLength)
                RaiseEvent Saving(Me, New ProgressEventArgs(Of Integer)(i + 1, m_pointDefinitions.Count))
            Next
            m_fileStream.Flush()    ' Ensure that the data is written to the file.
        Else
            Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
        End If

    End Sub

    Public Sub Analyze()

        If m_pointDefinitions IsNot Nothing AndAlso m_pointDefinitions.Count > 0 Then
            ' We can proceed with analyzing point definitions since they have been initialized.
            ' First, we'll make a working copy of the point definition list.
            Dim nonAlignedPointDefinitions As New List(Of PointDefinition)(m_pointDefinitions)
            ' When we sort the point definition list, it will be sorted by the Index of point definition.
            nonAlignedPointDefinitions.Sort()
            ' Clear the actual point definition list.
            m_pointDefinitions.Clear()
            For i As Integer = 0 To nonAlignedPointDefinitions.Count - 1
                ' We'll use the Write() method for adding point definitions to the actual point definition list.
                Write(nonAlignedPointDefinitions(i))
                RaiseEvent Analyzing(Me, New ProgressEventArgs(Of Integer)(nonAlignedPointDefinitions.Count, i + 1))
            Next
        End If

    End Sub

    Public Sub Write(ByVal pointDefinition As PointDefinition)

        If Me.IsOpen Then
            ' Insert/Update point definition to the in-memory point definition list.
            If Not m_pointDefinitions.Contains(pointDefinition) Then
                ' We have to add the point definition since it doesn't exist.
                If pointDefinition.Index > m_pointDefinitions.Count + 1 Then
                    ' We must add blank definitions as place holders before inserting the point definition.
                    For i As Integer = m_pointDefinitions.Count + 1 To pointDefinition.Index - 1
                        m_pointDefinitions.Add(New PointDefinition(i))
                    Next
                End If

                m_pointDefinitions.Add(pointDefinition)
            Else
                ' We have to update the point definition since one already exists.
                m_pointDefinitions(pointDefinition.Index - 1) = pointDefinition
            End If
        Else
            Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
        End If

    End Sub

    Public Function Read(ByVal pointIndex As Integer) As PointDefinition

        If Me.IsOpen Then
            For Each pointDefinition As PointDefinition In m_pointDefinitions
                If pointDefinition.Index = pointIndex Then
                    Return pointDefinition
                End If
            Next

            Return Nothing
        Else
            Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
        End If

    End Function

    Public Function Read(ByVal pointName As String) As PointDefinition

        If Me.IsOpen Then
            For Each pointDefinition As PointDefinition In m_pointDefinitions
                If String.Compare(pointName, pointDefinition.Name) = 0 OrElse _
                        String.Compare(pointName, pointDefinition.Synonym1) = 0 OrElse _
                        String.Compare(pointName, pointDefinition.Synonym2) = 0 Then
                    Return pointDefinition
                End If
            Next

            Return Nothing
        Else
            Throw New InvalidOperationException(String.Format("{0} ""{1}"" is not open.", Me.GetType().Name, m_name))
        End If

    End Function

#End Region

#Region " Private Code "

#Region " m_autoSaveTimer Events "

    Private Sub m_autoSaveTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_autoSaveTimer.Elapsed

        If Me.IsOpen Then Save() ' Automatically save point definitions to the file is the file is open.

    End Sub

#End Region

#Region " m_autoAnalyzeTimer Events "

    Private Sub m_autoAnalyzeTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_autoAnalyzeTimer.Elapsed

        Analyze()   ' Automatically analyze the current point definition list.

    End Sub

#End Region

#End Region

End Class
