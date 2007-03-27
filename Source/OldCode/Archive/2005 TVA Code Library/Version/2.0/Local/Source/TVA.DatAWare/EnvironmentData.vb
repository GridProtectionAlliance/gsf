Public Class EnvironmentData
    Implements IBinaryDataProvider

    ' *******************************************************************************
    ' *                         Environment Data Structure                          *
    ' *******************************************************************************
    ' * # Of Bytes  Byte Index  Data Type   Description                             *
    ' * ----------  ----------  ----------  ----------------------------------------*
    ' * 4           0-3         Int32       Number of used DataBlocks               *
    ' * 4           4-7         Boolean     Indicated if rollover is in progress    *
    ' * 8           8-15        Double      TimeTag of the latest current value     *
    ' * 4           16-19       Int32       Point ID of the latest current value    *
    ' * 160         20-179      Double(20)  ???                                     *
    ' *******************************************************************************

    Private m_id As Integer
    Private m_dataBlocksUsed As Integer
    Private m_rolloverInProgress As Boolean
    Private m_latestCurrentValueTimeTag As TimeTag
    Private m_latestCurrentValuePointID As Integer
    Private m_sourceIDs As List(Of TimeTag)

    Public Const Size As Integer = 180

    Public Sub New(ByVal id As Integer)

        MyBase.New()
        m_id = id
        m_latestCurrentValueTimeTag = TimeTag.MinValue
        m_sourceIDs = New List(Of TimeTag)(CreateArray(Of TimeTag)(20, TimeTag.MinValue))

    End Sub

    Public Sub New(ByVal id As Integer, ByVal binaryImage As Byte())

        MyClass.New(id, binaryImage, 0)

    End Sub

    Public Sub New(ByVal id As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(id)

        If binaryImage IsNot Nothing Then
            If binaryImage.Length - startIndex >= Size Then
                m_dataBlocksUsed = BitConverter.ToInt32(binaryImage, 0)
                m_rolloverInProgress = BitConverter.ToBoolean(binaryImage, 4)
                m_latestCurrentValueTimeTag = New TimeTag(BitConverter.ToDouble(binaryImage, 8))
                m_latestCurrentValuePointID = BitConverter.ToInt32(binaryImage, 16)
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

    Public Property DataBlocksUsed() As Integer
        Get
            Return m_dataBlocksUsed
        End Get
        Set(ByVal value As Integer)
            m_dataBlocksUsed = value
        End Set
    End Property

    Public Property RolloverInProgress() As Boolean
        Get
            Return m_rolloverInProgress
        End Get
        Set(ByVal value As Boolean)
            m_rolloverInProgress = value
        End Set
    End Property

    Public Property LastestCurrentValueTimeTag() As TimeTag
        Get
            Return m_latestCurrentValueTimeTag
        End Get
        Set(ByVal value As TimeTag)
            m_latestCurrentValueTimeTag = value
        End Set
    End Property

    Public Property LatestCurrentValuePointID() As Integer
        Get
            Return m_latestCurrentValuePointID
        End Get
        Set(ByVal value As Integer)
            m_latestCurrentValuePointID = value
        End Set
    End Property

    Public ReadOnly Property SourceIDs() As List(Of TimeTag)
        Get
            Return m_sourceIDs
        End Get
    End Property

    Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
        Get
            Dim image As Byte() = CreateArray(Of Byte)(Size)

            Array.Copy(BitConverter.GetBytes(m_dataBlocksUsed), 0, image, 0, 4)
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32(m_rolloverInProgress)), 0, image, 4, 4)
            Array.Copy(BitConverter.GetBytes(m_latestCurrentValueTimeTag.Value), 0, image, 8, 8)
            Array.Copy(BitConverter.GetBytes(m_latestCurrentValuePointID), 0, image, 16, 4)
            For i As Integer = 0 To m_sourceIDs.Count - 1
                Array.Copy(BitConverter.GetBytes(m_sourceIDs(i).Value), 0, image, (20 + (i * 8)), 8)
            Next

            Return image
        End Get
    End Property

    Public ReadOnly Property BinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
        Get
            Return Size
        End Get
    End Property

End Class
