' 02/21/2007

Imports System.Text

Public Class PointDefinitionConstantFields
    Implements IBinaryDataProvider

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

#Region " Public Code "

    Public Const Size As Integer = 8

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

    Public Sub Update(ByVal binaryInfo As Byte())

        If binaryInfo IsNot Nothing Then
            If binaryInfo.Length >= Size Then
                m_value = BitConverter.ToSingle(binaryInfo, 0)
                m_displayedDigits = BitConverter.ToInt32(binaryInfo, 4)
            Else
                Throw New ArgumentException("Binary info size is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryInfo")
        End If

    End Sub

#Region " IBinaryDataProvider Implementation "

    Public ReadOnly Property BinaryData() As Byte() Implements IBinaryDataProvider.BinaryData
        Get
            Dim data As Byte() = CreateArray(Of Byte)(Size)

            Array.Copy(BitConverter.GetBytes(m_value), 0, data, 0, 4)
            Array.Copy(BitConverter.GetBytes(m_displayedDigits), 0, data, 4, 4)

            Return data
        End Get
    End Property

    Public ReadOnly Property BinaryDataLength() As Integer Implements IBinaryDataProvider.BinaryDataLength
        Get
            Return Size
        End Get
    End Property

#End Region

#End Region

End Class
