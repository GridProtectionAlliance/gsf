' 02/15/2007

Imports System.IO
Imports System.Drawing
Imports Tva.IO.FilePath

<ToolboxBitmap(GetType(MetadataFile))> _
Public Class MetadataFile

#Region " Member Declaration "

    Private m_file As FileStream
    Private m_name As String
    Private m_keepOpen As Boolean
    Private m_pointCursor As Dictionary(Of String, Integer)

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
                    Throw New ArgumentException(String.Format("Name of {0} must have extension of {1}.", Me.GetType().Name, MetadataFile.FileExtension))
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

    Public ReadOnly Property IsOpen() As Boolean
        Get
            Return (m_file IsNot Nothing)
        End Get
    End Property

    Public Sub Open()

        m_file = New FileStream(m_name, FileMode.OpenOrCreate)
        If m_file.Length Mod PointDefinition.BinaryLength = 0 Then
            For i As Long = 0 To m_file.Length - 1 Step PointDefinition.BinaryLength

            Next
        Else
            Throw New InvalidOperationException(String.Format("File """"{0}"""" is corrupt.", m_name))
        End If

    End Sub

    Public Sub Close()

        m_file.Close()
        m_file.Dispose()
        m_file = Nothing

    End Sub

    Public Sub Insert(ByVal point As PointDefinition)

    End Sub

    Public Sub Update(ByVal point As PointDefinition)

    End Sub

    Public Sub Delete(ByVal pointID As String)

    End Sub

    Public Sub Delete(ByVal point As PointDefinition)

    End Sub

    'Public Sub Write(ByVal pointDefinition As PointDefinition)

    '    If Not Me.IsOpen Then Open()


    '    If Not m_keepOpen Then Close()

    'End Sub

    'Public Function Read(ByVal pointID As String) As PointDefinition

    'End Function

    'Public Function ReadAll() As List(Of PointDefinition)

    'End Function

End Class
