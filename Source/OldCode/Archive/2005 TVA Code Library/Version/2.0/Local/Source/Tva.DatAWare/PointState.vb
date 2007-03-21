' 03/08/2007

Public Class PointState
    Implements IBinaryDataProvider

    ' *******************************************************************************
    ' *                             Point State Structure                           *
    ' *******************************************************************************
    ' * # Of Bytes  Byte Index  Data Type   Description                             *
    ' * ----------  ----------  ----------  ----------------------------------------*
    ' * 16          0-15        Byte(16)    Last archived value                     *
    ' * 16          16-31       Byte(16)    Previous value                          *
    ' * 16          32-47       Byte(16)    Current value                           *
    ' * 4           48-51       Int32       Index of pointer to the active block    *
    ' * 4           52-55       Int32       Next avilable slot in the active block  *
    ' * 8           56-63       Double      Slope 1                                 *
    ' * 8           64-71       Double      Slope 2                                 *
    ' *******************************************************************************

#Region " Member Declaration "

    Private m_pointID As Integer
    Private m_lastArchivedValue As ExtendedPointData
    Private m_previousValue As ExtendedPointData
    Private m_currentValue As ExtendedPointData
    Private m_activeDataBlock As Files.ArchiveDataBlock
    Private m_activeDataBlockIndex As Integer
    Private m_activeDataBlockSlotNumber As Integer
    Private m_slope1 As Double
    Private m_slope2 As Double

#End Region

#Region " Public Code "

    Public Const Size As Integer = 72

    Public Sub New(ByVal pointID As Integer)

        MyBase.New()
        m_pointID = pointID
        m_lastArchivedValue = New ExtendedPointData()
        m_previousValue = New ExtendedPointData()
        m_currentValue = New ExtendedPointData()

    End Sub

    Public Sub New(ByVal pointID As Integer, ByVal binaryImage As Byte())

        MyClass.New(pointID, binaryImage, 0)

    End Sub

    Public Sub New(ByVal pointID As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(pointID)

        If binaryImage IsNot Nothing Then
            If binaryImage.Length - startIndex >= Size Then
                m_lastArchivedValue = New ExtendedPointData(binaryImage, 0)
                m_previousValue = New ExtendedPointData(binaryImage, 16)
                m_currentValue = New ExtendedPointData(binaryImage, 32)
                m_activeDataBlockIndex = BitConverter.ToInt32(binaryImage, 48)
                m_activeDataBlockSlotNumber = BitConverter.ToInt32(binaryImage, 52)
                m_slope1 = BitConverter.ToDouble(binaryImage, 56)
                m_slope2 = BitConverter.ToDouble(binaryImage, 64)
            Else
                Throw New ArgumentException("Binary image size from startIndex is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryImage")
        End If

    End Sub

    Public ReadOnly Property PointID() As Integer
        Get
            Return m_pointID
        End Get
    End Property

    Public Property LastArchivedValue() As ExtendedPointData
        Get
            Return m_lastArchivedValue
        End Get
        Set(ByVal value As ExtendedPointData)
            m_lastArchivedValue = value
        End Set
    End Property

    Public Property PreviousValue() As ExtendedPointData
        Get
            Return m_previousValue
        End Get
        Set(ByVal value As ExtendedPointData)
            m_previousValue = value
        End Set
    End Property

    Public Property CurrentValue() As ExtendedPointData
        Get
            Return m_currentValue
        End Get
        Set(ByVal value As ExtendedPointData)
            m_currentValue = value
        End Set
    End Property

    Public Property ActiveDataBlock() As Files.ArchiveDataBlock
        Get
            Return m_activeDataBlock
        End Get
        Set(ByVal value As Files.ArchiveDataBlock)
            m_activeDataBlock = value
        End Set
    End Property

    <Obsolete("This property is here for legacy purpose only and is not to be used.")> _
    Public Property ActiveDataBlockIndex() As Integer
        Get
            Return m_activeDataBlockIndex - 1
        End Get
        Set(ByVal value As Integer)
            ' Convert the 0-based index to 1-based (for legacy purpose).
            value += 1
            m_activeDataBlockIndex = value
        End Set
    End Property

    <Obsolete("This property is here for legacy purpose only and is not to be used.")> _
    Public Property ActiveDataBlockSlotNumber() As Integer
        Get
            Return m_activeDataBlockSlotNumber
        End Get
        Set(ByVal value As Integer)
            m_activeDataBlockSlotNumber = value
        End Set
    End Property

    Public Property Slope1() As Double
        Get
            Return m_slope1
        End Get
        Set(ByVal value As Double)
            m_slope1 = value
        End Set
    End Property

    Public Property Slope2() As Double
        Get
            Return m_slope2
        End Get
        Set(ByVal value As Double)
            m_slope2 = value
        End Set
    End Property

#End Region

#Region " IBinaryDataProvider Implementation "

    Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
        Get
            Dim image As Byte() = CreateArray(Of Byte)(Size)

            Array.Copy(m_lastArchivedValue.BinaryImage, 0, image, 0, ExtendedPointData.Size)
            Array.Copy(m_previousValue.BinaryImage, 0, image, 16, ExtendedPointData.Size)
            Array.Copy(m_currentValue.BinaryImage, 0, image, 32, ExtendedPointData.Size)
            Array.Copy(BitConverter.GetBytes(m_activeDataBlockIndex), 0, image, 48, 4)
            Array.Copy(BitConverter.GetBytes(m_activeDataBlockSlotNumber), 0, image, 52, 4)
            Array.Copy(BitConverter.GetBytes(m_slope1), 0, image, 56, 8)
            Array.Copy(BitConverter.GetBytes(m_slope2), 0, image, 64, 8)

            Return image
        End Get
    End Property

    Public ReadOnly Property BinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
        Get
            Return Size
        End Get
    End Property

#End Region

End Class
