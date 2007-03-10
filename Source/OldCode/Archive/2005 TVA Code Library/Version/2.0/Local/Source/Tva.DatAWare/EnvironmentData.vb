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
    Private m_lastCVTTimeTag As TimeTag
    Private m_lastCVTIndex As Integer
    Private m_sourceIDs As List(Of TimeTag)

    Public Const Size As Integer = 180

    Public Sub New(ByVal id As Integer)

        MyBase.New()
        m_id = id
        m_lastCVTTimeTag = TimeTag.MinValue
        m_sourceIDs = New List(Of TimeTag)(CreateArray(Of TimeTag)(20, TimeTag.MinValue))

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
                m_lastCVTTimeTag = New TimeTag(BitConverter.ToDouble(binaryImage, 8))
                m_lastCVTIndex = BitConverter.ToInt32(binaryImage, 16)
                For i As Integer = 0 To m_sourceIDs.Count - 1
                    m_sourceIDs(i) = New TimeTag(BitConverter.ToDouble(binaryImage, (20 + (i * 8))))
                Next
            Else
                Throw New ArgumentException("Binary image size from startIndex is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryImage")
        End If

    End Sub

    Public Property BlockMap() As Integer
        Get
            Return m_blockMap
        End Get
        Set(ByVal value As Integer)
            m_blockMap = value
        End Set
    End Property

    Public Property FileWrap() As Boolean
        Get
            Return m_fileWrap
        End Get
        Set(ByVal value As Boolean)
            m_fileWrap = value
        End Set
    End Property

    Public Property LastCVTTimeTag() As TimeTag
        Get
            Return m_lastCVTTimeTag
        End Get
        Set(ByVal value As TimeTag)
            m_lastCVTTimeTag = value
        End Set
    End Property

    Public Property LastCVTIndex() As Integer
        Get
            Return m_lastCVTIndex
        End Get
        Set(ByVal value As Integer)
            m_lastCVTIndex = value
        End Set
    End Property

    Public ReadOnly Property SourceIDs() As List(Of TimeTag)
        Get
            Return m_sourceIDs
        End Get
    End Property

    Public ReadOnly Property BinaryData() As Byte() Implements IBinaryDataProvider.BinaryData
        Get
            Dim data As Byte() = CreateArray(Of Byte)(Size)

            Array.Copy(BitConverter.GetBytes(m_blockMap), 0, data, 0, 4)
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32(m_fileWrap)), 0, data, 4, 4)
            Array.Copy(BitConverter.GetBytes(m_lastCVTTimeTag.Value), 0, data, 8, 8)
            Array.Copy(BitConverter.GetBytes(m_lastCVTIndex), 0, data, 16, 4)
            For i As Integer = 0 To m_sourceIDs.Count - 1
                Array.Copy(BitConverter.GetBytes(m_sourceIDs(i).Value), 0, data, (20 + (i * 8)), 8)
            Next

            Return data
        End Get
    End Property

    Public ReadOnly Property BinaryDataLength() As Integer Implements IBinaryDataProvider.BinaryDataLength
        Get
            Return Size
        End Get
    End Property

End Class
