Public Class EnvironmentData
    Implements IBinaryDataProvider

    ' *******************************************************************************
    ' *                         Environment Data Structure                          *
    ' *******************************************************************************
    ' * # Of Bytes  Byte Index  Data Type   Description                             *
    ' * ----------  ----------  ----------  ----------------------------------------*
    ' * 4           0-3         Int32       m_blockMap                              *
    ' * 4           4-7         Boolean     m_fileWrap                              *
    ' * 8           8-15        Double      m_lastCVTTimeTag                        *
    ' * 4           16-19       Int32       m_lastCVTIndex                          *
    ' * 160         20-179      Double(20)  m_sourceIDs                             *
    ' *******************************************************************************

    Private m_id As Integer
    Private m_blockMap As Integer
    Private m_fileWrap As Boolean
    Private m_lastCVTTimeTag As Double
    Private m_lastCVTIndex As Integer
    Private m_sourceIDs As List(Of Double)

    Public Const Size As Integer = 180

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
                m_blockMap = BitConverter.ToInt32(binaryImage, 0)
                m_fileWrap = BitConverter.ToBoolean(binaryImage, 4)
                m_lastCVTTimeTag = BitConverter.ToDouble(binaryImage, 8)
                m_lastCVTIndex = BitConverter.ToInt32(binaryImage, 16)
                For i As Integer = 0 To m_sourceIDs.Capacity - 1
                    m_sourceIDs.Add(BitConverter.ToDouble(binaryImage, (20 + (i * 8))))
                Next
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
