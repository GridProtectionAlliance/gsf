' 02/15/2007

Imports System.Drawing

<ToolboxBitmap(GetType(MetadataFile))> _
Public Class MetadataFile

    Public Const FileExtension As String = ".???"

    Public Property Name() As String
        Get

        End Get
        Set(ByVal value As String)

        End Set
    End Property

    Public Property KeepFileOpen() As Boolean
        Get

        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Sub Open()

    End Sub

    Public Sub Close()

    End Sub

    Public Sub Write(ByVal pointDefinition As PointDefinition)

    End Sub

    Public Function Read(ByVal pointID As String) As PointDefinition

    End Function

    Public Function ReadAll() As List(Of PointDefinition)

    End Function

End Class
