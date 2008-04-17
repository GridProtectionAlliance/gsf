'*******************************************************************************************************
'  TVA.Security.Radius.RadiusPacketAttribute.vb - RADIUS authentication packet attribute
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/11/2008 - Pinal C. Patel
'       Original version of source code generated.
'
'*******************************************************************************************************

Imports System.Net
Imports TVA.Common
Imports TVA.Parsing

Namespace Radius

    Public Class RadiusPacketAttribute
        Implements IBinaryDataProvider, IBinaryDataConsumer

        ' 0                   1                   2
        ' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0
        '+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
        '|     Type      |    Length     |  Value ...
        '+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

#Region " Member Declaration "

        Private m_type As AttributeType
        Private m_value As Byte()

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Creates a default instance of RADIUS packet attribute.
        ''' </summary>
        Public Sub New()

            ' No initialization required.

        End Sub

        ''' <summary>
        ''' Creates an instance of RADIUS packet attribute.
        ''' </summary>
        ''' <param name="type">Type of the attribute.</param>
        ''' <param name="value">Text value of the attribute.</param>
        Public Sub New(ByVal type As AttributeType, ByVal value As String)

            MyClass.New(type, RadiusPacket.GetBytes(value))

        End Sub

        ''' <summary>
        ''' Creates an instance of RADIUS packet attribute.
        ''' </summary>
        ''' <param name="type">Type of the attribute.</param>
        ''' <param name="value">32-bit unsigned integer value of the attribute.</param>
        Public Sub New(ByVal type As AttributeType, ByVal value As UInteger)

            MyClass.New(type, RadiusPacket.GetBytes(value))

        End Sub

        ''' <summary>
        ''' Creates an instance of RADIUS packet attribute.
        ''' </summary>
        ''' <param name="type">Type of the attribute.</param>
        ''' <param name="value">IP address value of the attribute.</param>
        Public Sub New(ByVal type As AttributeType, ByVal value As IPAddress)

            MyClass.New(type, value.GetAddressBytes())

        End Sub

        ''' <summary>
        ''' Creates an instance of RADIUS packet attribute.
        ''' </summary>
        ''' <param name="type">Type of the attribute.</param>
        ''' <param name="value">Byte array value of the attribute.</param>
        Public Sub New(ByVal type As AttributeType, ByVal value As Byte())

            Me.Type = type
            Me.Value = value

        End Sub

        ''' <summary>
        ''' Creates an instance of RADIUS packet attribute.
        ''' </summary>
        ''' <param name="binaryImage">A byte array.</param>
        ''' <param name="startIndex">Starting point in the byte array.</param>
        Public Sub New(ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            Initialize(binaryImage, startIndex)

        End Sub

        ''' <summary>
        ''' Gets or sets the type of the attribute.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Type of the attribute.</returns>
        Public Property Type() As AttributeType
            Get
                Return m_type
            End Get
            Set(ByVal value As AttributeType)
                m_type = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the value of the attribute.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Value of the attribute.</returns>
        Public Property Value() As Byte()
            Get
                Return m_value
            End Get
            Set(ByVal value As Byte())
                If value IsNot Nothing AndAlso value.Length > 0 Then
                    ' By definition, attribute value cannot be null or zero-length.
                    m_value = value
                Else
                    Throw New ArgumentNullException("Value")
                End If
            End Set
        End Property

#Region " Interface Implementations "

#Region " IBinaryDataProvider "

        ''' <summary>
        ''' Gets the binary lenght of the attribute.
        ''' </summary>
        ''' <value></value>
        ''' <returns>32-bit signed integer value.</returns>
        Public ReadOnly Property BinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
            Get
                ' 2 bytes are fixed + length of the value
                If m_value Is Nothing Then
                    Return 2
                Else
                    Return 2 + m_value.Length
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets the binary image of the attribute.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A byte array.</returns>
        Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
            Get
                Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)
                image(0) = CByte(m_type)
                image(1) = CByte(BinaryLength)
                If m_value IsNot Nothing AndAlso m_value.Length > 0 Then
                    Array.Copy(m_value, 0, image, 2, m_value.Length)
                Else
                    Throw New ArgumentNullException("Value", "Attribute value cannot be null or zero-length.")
                End If

                Return image
            End Get
        End Property

#End Region

#Region " IBinaryDataConsumer "

        ''' <summary>
        ''' Initializes the attribute from the specified binary image.
        ''' </summary>
        ''' <param name="binaryImage">A byte array.</param>
        ''' <param name="startIndex">Starting point in the byte array.</param>
        ''' <returns>Number of bytes used to initialize the attribute.</returns>
        Public Function Initialize(ByVal binaryImage() As Byte, ByVal startIndex As Integer) As Integer Implements IBinaryDataConsumer.Initialize

            If binaryImage IsNot Nothing AndAlso binaryImage.Length >= 2 Then
                ' We have a valid buffer to work with.
                m_type = CType(binaryImage(startIndex), AttributeType)
                m_value = CreateArray(Of Byte)(CShort(binaryImage(startIndex + 1) - 2))
                Array.Copy(binaryImage, startIndex + 2, m_value, 0, m_value.Length)

                Return BinaryLength
            Else
                Throw New ArgumentException("Buffer is not valid.")
            End If

        End Function

#End Region

#End Region

#End Region

    End Class

End Namespace