' 02/21/2007

Imports System.Text
Imports Tva.Text.Common

Public Class MetadataComposedFields

    ' *******************************************************************************
    ' *                             Binary Info Structure                           *
    ' *******************************************************************************
    ' * # Of Bytes  Byte Index  Data Type   Description                             *
    ' * ----------  ----------  ----------  ----------------------------------------*
    ' * 4           0-3         Single      High alarm value of point               *
    ' * 4           4-7         Single      Low alarm value of point                *
    ' * 4           8-11        Single      High range value of point               *
    ' * 4           12-15       Single      Low range of value point                *
    ' * 4           16-19       Int32       Decimal places displyed in the client   *
    ' * 48          20-67       Int32(12)   Inputs to the equation                  *
    ' * 8           68-75       String(8)   Engineering units of the entry          *
    ' * 128         76-203      String(128) Mathematical equation defining the point*
    ' * 4           204-207     Single      Low warning value of point              *
    ' * 4           208-211     Single      High warning value of point             *
    ' *******************************************************************************

#Region " Member Declaration "

    Private m_highAlarm As Single
    Private m_lowAlarm As Single
    Private m_highRange As Single
    Private m_lowRange As Single
    Private m_displayedDigits As Integer
    Private m_inputPointers As List(Of Integer)
    Private m_engineeringUnits As String
    Private m_equation As String
    Private m_lowWarning As Single
    Private m_highWarning As Single
    Private m_textEncoding As Encoding

#End Region

    Public Const BinaryLength As Integer = 212

    Public Sub New(ByVal binaryInfo As Byte())

        MyClass.New(binaryInfo, Nothing)

    End Sub

    Public Sub New(ByVal binaryInfo As Byte(), ByVal textEncoding As Encoding)

        MyBase.New()

        If textEncoding IsNot Nothing Then m_textEncoding = textEncoding Else m_textEncoding = Encoding.Default

        If binaryInfo IsNot Nothing Then
            If binaryInfo.Length >= BinaryLength Then
                m_inputPointers = New List(Of Integer)(12)
                m_highAlarm = BitConverter.ToSingle(binaryInfo, 0)
                m_lowAlarm = BitConverter.ToSingle(binaryInfo, 4)
                m_highRange = BitConverter.ToSingle(binaryInfo, 8)
                m_lowRange = BitConverter.ToSingle(binaryInfo, 12)
                m_displayedDigits = BitConverter.ToInt32(binaryInfo, 16)
                For i As Integer = 0 To m_inputPointers.Capacity - 1
                    m_inputPointers.Add(BitConverter.ToInt32(binaryInfo, (20 + (i * 4))))
                Next
                m_engineeringUnits = m_textEncoding.GetString(binaryInfo, 68, 8).Trim()
                m_equation = m_textEncoding.GetString(binaryInfo, 76, 128).Trim()
                m_lowWarning = BitConverter.ToSingle(binaryInfo, 204)
                m_highWarning = BitConverter.ToSingle(binaryInfo, 208)
            Else
                Throw New ArgumentException("Binary info size is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryInfo")
        End If

    End Sub

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

    Public Property DisplayedDigits() As Integer
        Get
            Return m_displayedDigits
        End Get
        Set(ByVal value As Integer)
            m_displayedDigits = value
        End Set
    End Property

    Public ReadOnly Property InputPointers() As List(Of Integer)
        Get
            Return m_inputPointers
        End Get
    End Property

    Public Property EngineeringUnits() As String
        Get
            Return m_engineeringUnits
        End Get
        Set(ByVal value As String)
            m_engineeringUnits = TrimString(value, 8)
        End Set
    End Property

    Public Property Equation() As String
        Get
            Return m_equation
        End Get
        Set(ByVal value As String)
            m_equation = TrimString(value, 128)
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

    Public Property HighWarning() As Single
        Get
            Return m_highWarning
        End Get
        Set(ByVal value As Single)
            m_highWarning = value
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

            Array.Copy(BitConverter.GetBytes(m_highAlarm), 0, image, 0, 4)
            Array.Copy(BitConverter.GetBytes(m_lowAlarm), 0, image, 4, 4)
            Array.Copy(BitConverter.GetBytes(m_highRange), 0, image, 8, 4)
            Array.Copy(BitConverter.GetBytes(m_lowRange), 0, image, 12, 4)
            Array.Copy(BitConverter.GetBytes(m_displayedDigits), 0, image, 16, 4)
            For i As Integer = 0 To m_inputPointers.Count - 1
                Array.Copy(BitConverter.GetBytes(m_inputPointers(i)), 0, image, (20 + (i * 4)), 4)
            Next
            Array.Copy(m_textEncoding.GetBytes(m_engineeringUnits.PadRight(8)), 0, image, 68, 8)
            Array.Copy(m_textEncoding.GetBytes(m_equation.PadRight(128)), 0, image, 76, 128)
            Array.Copy(BitConverter.GetBytes(m_lowWarning), 0, image, 28, 4)
            Array.Copy(BitConverter.GetBytes(m_highWarning), 0, image, 24, 4)

            Return image
        End Get
    End Property

End Class
