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

    Private m_id As Integer
    Private m_lastArchivedValue As ExtendedPointData
    Private m_previousValue As ExtendedPointData
    Private m_currentValue As ExtendedPointData
    Private m_activeDataBlockIndex As Integer
    Private m_activeDataBlockSlotNumber As Integer
    Private m_slope1 As Double
    Private m_slope2 As Double

#End Region

#Region " Public Code "

    Public Const Size As Integer = 72

    Public Sub New(ByVal id As Integer)

        MyBase.New()
        m_id = id
        m_lastArchivedValue = New ExtendedPointData()
        m_previousValue = New ExtendedPointData()
        m_currentValue = New ExtendedPointData()

    End Sub

    Public Sub New(ByVal id As Integer, ByVal binaryImage As Byte())

        MyClass.New(id, binaryImage, 0)

    End Sub

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

    Public Sub New(ByVal id As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(id)

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

#End Region

#Region " IBinaryDataProvider Implementation "

    Public ReadOnly Property BinaryData() As Byte() Implements IBinaryDataProvider.BinaryData
        Get
            Dim data As Byte() = CreateArray(Of Byte)(Size)

            Array.Copy(m_lastArchivedValue.BinaryData, 0, data, 0, ExtendedPointData.Size)
            Array.Copy(m_previousValue.BinaryData, 0, data, 16, ExtendedPointData.Size)
            Array.Copy(m_currentValue.BinaryData, 0, data, 32, ExtendedPointData.Size)
            Array.Copy(BitConverter.GetBytes(m_activeDataBlockIndex), 0, data, 48, 4)
            Array.Copy(BitConverter.GetBytes(m_activeDataBlockSlotNumber), 0, data, 52, 4)
            Array.Copy(BitConverter.GetBytes(m_slope1), 0, data, 56, 8)
            Array.Copy(BitConverter.GetBytes(m_slope2), 0, data, 64, 8)

            Return data
        End Get
    End Property

    Public ReadOnly Property BinaryDataLength() As Integer Implements IBinaryDataProvider.BinaryDataLength
        Get
            Return Size
        End Get
    End Property

#End Region

End Class
