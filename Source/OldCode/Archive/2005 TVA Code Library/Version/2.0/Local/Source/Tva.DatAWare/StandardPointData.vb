' 02/23/2007

Imports Tva.Interop.Bit

''' <summary>
''' 
''' </summary>
''' <remarks>This is what goes into the archive file.</remarks>
Public Class StandardPointData
    Inherits PointDataBase

#Region " Public Code "

    Public Shadows Const BinaryLength As Integer = 10

    Public Sub New(ByVal binaryImage As Byte())

        MyClass.New(binaryImage, 0)

    End Sub

    Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyBase.new()
        If binaryImage IsNot Nothing Then
            If binaryImage.Length - startIndex >= BinaryLength Then
                MyBase.Value = BitConverter.ToSingle(binaryImage, 6)
                MyBase.Flags = BitConverter.ToInt16(binaryImage, 4)
                MyBase.TimeTag = New TimeTag(BitConverter.ToInt32(binaryImage, 0) + Me.Millisecond / 1000)
            Else
                Throw New ArgumentException("Binary image size from startIndex is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryImage")
        End If

    End Sub

    Public Overrides ReadOnly Property BinaryImage() As Byte()
        Get
            Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)
            Dim timeTag As Integer = Convert.ToInt32(System.Math.Truncate(MyBase.TimeTag.Value))
            Dim milliseconds As Integer = Convert.ToInt32((MyBase.TimeTag.Value - timeTag) * 1000)
            Me.Millisecond = milliseconds

            Array.Copy(BitConverter.GetBytes(timeTag), 0, image, 0, 4)
            Array.Copy(BitConverter.GetBytes(MyBase.Flags), 0, image, 4, 2)
            Array.Copy(BitConverter.GetBytes(MyBase.Value), 0, image, 6, 4)

            Return image
        End Get
    End Property

#End Region

#Region " Private Code "

    Private MillisecondMask As Int32 = Bit5 Or Bit6 Or Bit7 Or Bit8 Or Bit9 Or Bit10 Or Bit11 Or Bit12 Or Bit13 Or Bit14 Or Bit15

    Private Property Millisecond() As Integer
        Get
            Return (MyBase.Flags And MillisecondMask) \ 32 ' 32 because the 1st 5 bits are quality???
        End Get
        Set(ByVal value As Integer)
            MyBase.Flags = (MyBase.Flags And Not MillisecondMask Or value * 32)
        End Set
    End Property

#End Region

End Class

#Region " Old Code "

'' 02/23/2007

'Imports Tva.Interop.Bit

'Public Class StandardPointData

'#Region " Member Declaration "

'    Private m_tTag As TimeTag
'    Private m_flags As Int16
'    Private m_value As Single

'    Private Const QualityMask As Int16 = Bit0 Or Bit1 Or Bit2 Or Bit3 Or Bit4
'    Private Const MillisecondMask As Int16 = Bit5 Or Bit6 Or Bit7 Or Bit8 Or Bit9 Or Bit10 Or Bit11 Or Bit12 Or Bit13 Or Bit14 Or Bit15

'#End Region

'#Region " Public Code "

'    Public Const BinaryLength As Integer = 10

'    Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

'        If binaryImage IsNot Nothing Then
'            If binaryImage.Length - startIndex >= BinaryLength Then
'                m_value = BitConverter.ToSingle(binaryImage, 6)
'                m_flags = BitConverter.ToInt16(binaryImage, 4)
'                m_tTag = New TimeTag(BitConverter.ToInt32(binaryImage, 0) + Me.Millisecond / 1000)
'            Else
'                Throw New ArgumentException("Binary image size from startIndex is too small.")
'            End If
'        Else
'            Throw New ArgumentNullException("binaryImage")
'        End If

'    End Sub

'    Public Property TTag() As TimeTag
'        Get
'            Return m_tTag
'        End Get
'        Set(ByVal value As TimeTag)
'            m_tTag = value
'        End Set
'    End Property

'    Public Property Value() As Single
'        Get
'            Return m_value
'        End Get
'        Set(ByVal value As Single)
'            m_value = value
'        End Set
'    End Property

'    Public Property Quality() As Quality
'        Get
'            Return CType((m_flags And QualityMask), Quality)
'        End Get
'        Set(ByVal value As Quality)
'            m_flags = (m_flags And Not QualityMask Or Convert.ToInt16(value))
'        End Set
'    End Property

'    Public Property Millisecond() As Short
'        Get
'            Return Convert.ToInt16((m_flags And MillisecondMask) \ 32) ' 32 because the 1st 5 bits are quality???
'        End Get
'        Set(ByVal value As Short)
'            m_flags = (m_flags And Not MillisecondMask Or Convert.ToInt16(value * 32))
'        End Set
'    End Property

'    Public ReadOnly Property BinaryImage() As Byte()
'        Get
'            Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)


'            Return image
'        End Get
'    End Property

'#End Region

'End Class

#End Region
