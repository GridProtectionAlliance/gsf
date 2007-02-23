' 02/21/2007

Imports System.Text

Public Class MetadataConstantFields


    ' *******************************************************************************
    ' *                             Binary Info Structure                           *
    ' *******************************************************************************
    ' * # Of Bytes  Byte Index  Data Type   Description                             *
    ' * ----------  ----------  ----------  ----------------------------------------*
    ' * 4           0-3         Single      Value of the constant                   *
    ' * 4           4-7         Int32       Decimal places displyed in the client   *
    ' *******************************************************************************

#Region " Member Declaration "

    Private m_value As Single
    Private m_displayedDigits As Integer

#End Region

    Public Const BinaryLength As Integer = 8

    Public Sub New(ByVal binaryInfo As Byte())

        MyBase.New()

        Update(binaryInfo)

    End Sub

    Public Property Value() As Single
        Get
            Return m_value
        End Get
        Set(ByVal value As Single)
            m_value = value
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

    Public ReadOnly Property BinaryImage() As Byte()
        Get
            Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)

            Array.Copy(BitConverter.GetBytes(m_value), 0, image, 0, 4)
            Array.Copy(BitConverter.GetBytes(m_displayedDigits), 0, image, 4, 4)

            Return image
        End Get
    End Property

    Public Sub Update(ByVal binaryInfo As Byte())

        If binaryInfo IsNot Nothing Then
            If binaryInfo.Length >= BinaryLength Then
                m_value = BitConverter.ToSingle(binaryInfo, 0)
                m_displayedDigits = BitConverter.ToInt32(binaryInfo, 4)
            Else
                Throw New ArgumentException("Binary info size is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryInfo")
        End If

    End Sub

End Class
