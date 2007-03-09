Public Class EnvironmentData
    Implements IBinaryDataProvider

    Private m_id As Integer
    Private m_blockMap As Integer           ' 0     4-Bytes
    Private m_fileWrap As Integer           ' 4     4-Bytes
    Private m_lastCVTTimeTag As Double      ' 8     8-Bytes
    Private m_lastCVTIndex As Integer       ' 16    4-Bytes
    Private m_sourceIDs As List(Of Double)  ' 20    160-Bytes

    Public Const Size As Integer = 20

    Public Sub New(ByVal id As Integer)

        MyBase.New()
        m_id = id
        m_sourceIDs = New List(Of Double)(20)

    End Sub

    Public Sub New(ByVal id As Integer, ByVal binaryImage As Byte())

        MyClass.New(id, binaryImage, 0)

    End Sub

    Public Sub New(ByVal id As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(id)

        If binaryImage IsNot Nothing Then
            If binaryImage.Length - startIndex >= Size Then

            Else
                Throw New ArgumentException("Binary image size from startIndex is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryImage")
        End If

    End Sub

    Public ReadOnly Property BinaryData() As Byte() Implements IBinaryDataProvider.BinaryData
        Get
            Dim data As Byte() = CreateArray(Of Byte)(Size)

            Return data
        End Get
    End Property

    Public ReadOnly Property BinaryDataLength() As Integer Implements IBinaryDataProvider.BinaryDataLength
        Get
            Return Size
        End Get
    End Property

End Class
