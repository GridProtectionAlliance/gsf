' 02/18/2007

Imports System.IO
Imports System.Drawing
Imports System.ComponentModel
Imports Tva.IO.FilePath

<ToolboxBitmap(GetType(ArchiveFile))> _
Public Class ArchiveFile
    Implements ISupportInitialize

    Private m_name As String
    Private m_size As Integer
    Private m_blockSize As Integer
    Private m_fat As ArchiveFileAllocationTable

    Public Const Extension As String = ".d"

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            If Not String.IsNullOrEmpty(value) Then
                If String.Compare(JustFileExtension(value), MetadataFile.Extension) = 0 Then
                    m_name = value
                Else
                    Throw New ArgumentException(String.Format("Name of {0} must have an extension of {1}.", Me.GetType().Name, Extension))
                End If
            Else
                Throw New ArgumentNullException("Name")
            End If
        End Set
    End Property

    Public Property Size() As Integer
        Get
            Return m_size
        End Get
        Set(ByVal value As Integer)
            m_size = value
        End Set
    End Property

    Public Property BlockSize() As Integer
        Get
            Return m_blockSize
        End Get
        Set(ByVal value As Integer)
            m_blockSize = value
        End Set
    End Property

    <Browsable(False)> _
    Public ReadOnly Property FAT() As ArchiveFileAllocationTable
        Get
            Return m_fat
        End Get
    End Property

    Public Sub Open()

        If File.Exists(m_name) Then

        Else

        End If

    End Sub

    Public Sub Close()

    End Sub

    Public Sub Close(ByVal save As Boolean)

    End Sub

    Public Sub Save()

    End Sub

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

End Class
