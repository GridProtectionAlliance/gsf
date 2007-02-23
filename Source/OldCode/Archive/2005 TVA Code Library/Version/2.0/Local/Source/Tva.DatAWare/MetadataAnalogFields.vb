' 02/21/2007

Imports System.Text
Imports Tva.Text.Common

Public Class MetadataAnalogFields

    ' *******************************************************************************
    ' *                             Binary Info Structure                           *
    ' *******************************************************************************
    ' * # Of Bytes  Byte Index  Data Type   Description                             *
    ' * ----------  ----------  ----------  ----------------------------------------*
    ' * 8           0-7         Char(8)     Engineering units of the entry          *
    ' * 4           8-11        Single      High alarm value of point               *
    ' * 4           12-15       Single      Low alarm value of point                *
    ' * 4           16-19       Single      High range value of point               *
    ' * 4           20-23       Single      Low range of value point                *
    ' * 4           24-27       Single      High warning value of point             *
    ' * 4           28-31       Single      Low warning value of point              *
    ' * 4           32-35       Single      Exception limit in engineering units    *
    ' * 4           36-39       Single      Compression limit in engineering units  *
    ' * 4           40-43       Int32       Decimal places displyed in the client   *
    ' *******************************************************************************

#Region " Member Declaration "

    Private m_engineeringUnits As String
    Private m_highAlarm As Single
    Private m_lowAlarm As Single
    Private m_highRange As Single
    Private m_lowRange As Single
    Private m_highWarning As Single
    Private m_lowWarning As Single
    Private m_exceptionLimit As Single
    Private m_compressionLimit As Single
    Private m_displayedDigits As Integer
    Private m_textEncoding As Encoding

#End Region

    Public Const BinaryLength As Integer = 44

    Public Sub New(ByVal binaryInfo As Byte())

        MyClass.New(binaryInfo, Nothing)

    End Sub

    Public Sub New(ByVal binaryInfo As Byte(), ByVal textEncoding As Encoding)

        MyBase.New()

        If textEncoding IsNot Nothing Then m_textEncoding = textEncoding Else m_textEncoding = Encoding.Default

        Update(binaryInfo)

    End Sub

    Public Property EngineeringUnits() As String
        Get
            Return m_engineeringUnits
        End Get
        Set(ByVal value As String)
            m_engineeringUnits = TruncateString(value, 8)
        End Set
    End Property

    Public Property HighAlarm() As Single
        Get
            Return m_highAlarm
        End Get
        Set(ByVal value As Single)
            m_highAlarm = value
        End Set
    End Property

    Public Property LowAlarm() As Single
        Get
            Return m_lowAlarm
        End Get
        Set(ByVal value As Single)
            m_lowAlarm = value
        End Set
    End Property

    Public Property HighRange() As Single
        Get
            Return m_highRange
        End Get
        Set(ByVal value As Single)
            m_highRange = value
        End Set
    End Property

    Public Property LowRange() As Single
        Get
            Return m_lowRange
        End Get
        Set(ByVal value As Single)
            m_lowRange = value
        End Set
    End Property

    Public Property HighWarning() As Single
        Get
            Return m_highWarning
        End Get
        Set(ByVal value As Single)
            m_highWarning = value
        End Set
    End Property

    Public Property LowWarning() As Single
        Get
            Return m_lowWarning
        End Get
        Set(ByVal value As Single)
            m_lowWarning = value
        End Set
    End Property

    Public Property ExceptionLimit() As Single
        Get
            Return m_exceptionLimit
        End Get
        Set(ByVal value As Single)
            m_exceptionLimit = value
        End Set
    End Property

    Public Property CompressionLimit() As Single
        Get
            Return m_compressionLimit
        End Get
        Set(ByVal value As Single)
            m_compressionLimit = value
        End Set
    End Property

    Public Property DisplayedDigits() As Integer
        Get
            Return m_displayedDigits
        End Get
        Set(ByVal value As Integer)
            m_displayedDigits = value
        End Set
    End Property

    Public Property TextEncoding() As Encoding
        Get
            Return m_textEncoding
        End Get
        Set(ByVal value As Encoding)
            m_textEncoding = value
        End Set
    End Property

    Public ReadOnly Property BinaryImage() As Byte()
        Get
            Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)

            Array.Copy(m_textEncoding.GetBytes(m_engineeringUnits.PadRight(8)), 0, image, 0, 8)
            Array.Copy(BitConverter.GetBytes(m_highAlarm), 0, image, 8, 4)
            Array.Copy(BitConverter.GetBytes(m_lowAlarm), 0, image, 12, 4)
            Array.Copy(BitConverter.GetBytes(m_highRange), 0, image, 16, 4)
            Array.Copy(BitConverter.GetBytes(m_lowRange), 0, image, 20, 4)
            Array.Copy(BitConverter.GetBytes(m_highWarning), 0, image, 24, 4)
            Array.Copy(BitConverter.GetBytes(m_lowWarning), 0, image, 28, 4)
            Array.Copy(BitConverter.GetBytes(m_exceptionLimit), 0, image, 32, 4)
            Array.Copy(BitConverter.GetBytes(m_compressionLimit), 0, image, 36, 4)
            Array.Copy(BitConverter.GetBytes(m_displayedDigits), 0, image, 40, 4)

            Return image
        End Get
    End Property

    Public Sub Update(ByVal binaryInfo As Byte())

        If binaryInfo IsNot Nothing Then
            If binaryInfo.Length >= BinaryLength Then
                m_engineeringUnits = m_textEncoding.GetString(binaryInfo, 0, 8).Trim()
                m_highAlarm = BitConverter.ToSingle(binaryInfo, 8)
                m_lowAlarm = BitConverter.ToSingle(binaryInfo, 12)
                m_highRange = BitConverter.ToSingle(binaryInfo, 16)
                m_lowRange = BitConverter.ToSingle(binaryInfo, 20)
                m_highWarning = BitConverter.ToSingle(binaryInfo, 24)
                m_lowWarning = BitConverter.ToSingle(binaryInfo, 28)
                m_exceptionLimit = BitConverter.ToSingle(binaryInfo, 32)
                m_compressionLimit = BitConverter.ToSingle(binaryInfo, 36)
                m_displayedDigits = BitConverter.ToInt32(binaryInfo, 40)
            Else
                Throw New ArgumentException("Binary info size is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryInfo")
        End If

    End Sub

End Class
