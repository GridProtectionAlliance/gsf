' 02/21/2007

Imports System.Text
Imports TVA.Text.Common

Public Class PointDefinitionDigitalFields
    Implements IBinaryDataProvider

    ' *******************************************************************************
    ' *                             Binary Info Structure                           *
    ' *******************************************************************************
    ' * # Of Bytes  Byte Index  Data Type   Description                             *
    ' * ----------  ----------  ----------  ----------------------------------------*
    ' * 13          0-12        Char(13)    Text describing the input in "1" state  *
    ' * 13          13-25       Char(13)    Text describing the input in "0" state  *
    ' * 2           26-27       Int16       0 or 1 which corresponds to alarm state *
    ' *******************************************************************************

#Region " Member Declaration "

    Private m_setDescription As String
    Private m_clearDescription As String
    Private m_alarmState As Short
    Private m_textEncoding As Encoding

#End Region

#Region " Public Code "

    Public Const Size As Integer = 28

    Public Sub New(ByVal binaryInfo As Byte())

        MyClass.New(binaryInfo, Nothing)

    End Sub

    Public Sub New(ByVal binaryInfo As Byte(), ByVal textEncoding As Encoding)

        MyBase.New()

        If textEncoding IsNot Nothing Then m_textEncoding = textEncoding Else m_textEncoding = Encoding.Default

        Update(binaryInfo)

    End Sub

    Public Property SetDescription() As String
        Get
            Return m_setDescription
        End Get
        Set(ByVal value As String)
            m_setDescription = TruncateRight(value, 13)
        End Set
    End Property

    Public Property ClearDescription() As String
        Get
            Return m_clearDescription
        End Get
        Set(ByVal value As String)
            m_clearDescription = TruncateRight(value, 13)
        End Set
    End Property

    Public Property AlarmState() As Short
        Get
            Return m_alarmState
        End Get
        Set(ByVal value As Short)
            m_alarmState = value
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

    Public Sub Update(ByVal binaryInfo As Byte())

        If binaryInfo IsNot Nothing Then
            If binaryInfo.Length >= Size Then
                m_setDescription = m_textEncoding.GetString(binaryInfo, 0, 13).Trim()
                m_clearDescription = m_textEncoding.GetString(binaryInfo, 13, 13).Trim()
                m_alarmState = BitConverter.ToInt16(binaryInfo, 26)
            Else
                Throw New ArgumentException("Binary info size is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryInfo")
        End If

    End Sub

#Region " IBinaryDataProvider Implementation "

    Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
        Get
            Dim image As Byte() = CreateArray(Of Byte)(Size)

            Array.Copy(m_textEncoding.GetBytes(m_setDescription.PadRight(13)), 0, image, 0, 13)
            Array.Copy(m_textEncoding.GetBytes(m_clearDescription.PadRight(13)), 0, image, 13, 13)
            Array.Copy(BitConverter.GetBytes(m_alarmState), 0, image, 26, 2)

            Return image
        End Get
    End Property

    Public ReadOnly Property BinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
        Get
            Return Size
        End Get
    End Property

#End Region

#End Region

End Class
