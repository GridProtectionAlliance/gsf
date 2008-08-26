'*******************************************************************************************************
'  TVA.Security.Radius.RadiusPacket.vb - RADIUS authentication packet
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

Imports System.Text
Imports System.Security.Cryptography
Imports TVA.Common
Imports TVA.Interop
Imports TVA.Parsing
Imports TVA.Math.Common
Imports TVA.Security.Cryptography.Common

Namespace Radius

    Public Class RadiusPacket
        Implements IBinaryDataProvider, IBinaryDataConsumer

        ' 0                   1                   2                   3
        ' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        '+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        '|     Code      |  Identifier   |            Length             |
        '+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        '|                                                               |
        '|                         Authenticator                         |
        '|                                                               |
        '|                                                               |
        '+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        '|  Attributes ...
        '+-+-+-+-+-+-+-+-+-+-+-+-+-

#Region " Member Declaration "

        Private m_type As PacketType
        Private m_identifier As Byte
        Private m_authenticator As Byte()
        Private m_attributes As List(Of RadiusPacketAttribute)

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Encoding format for encoding text.
        ''' </summary>
        Public Shared Encoding As Encoding = Encoding.UTF8

        ''' <summary>
        ''' Creates a default instance of RADIUS packet.
        ''' </summary>
        Public Sub New()

            m_identifier = CByte(RandomBetween(0, 255))
            m_authenticator = CreateArray(Of Byte)(16)
            m_attributes = New List(Of RadiusPacketAttribute)()

        End Sub

        ''' <summary>
        ''' Creates an instance of RADIUS packet.
        ''' </summary>
        ''' <param name="type">Type of the packet.</param>
        Public Sub New(ByVal type As PacketType)

            MyClass.New()
            m_type = type

        End Sub

        ''' <summary>
        ''' Creates an instance of RADIUS packet.
        ''' </summary>
        ''' <param name="binaryImage">A byte array.</param>
        ''' <param name="startIndex">Starting point in the byte array.</param>
        Public Sub New(ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            MyClass.New()
            Initialize(binaryImage, startIndex)

        End Sub

        ''' <summary>
        ''' Gets or sets the type of the packet.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Type of the packet.</returns>
        Public Property Type() As PacketType
            Get
                Return m_type
            End Get
            Set(ByVal value As PacketType)
                m_type = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the packet identifier.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Identifier of the packet.</returns>
        Public Property Identifier() As Byte
            Get
                Return m_identifier
            End Get
            Set(ByVal value As Byte)
                m_identifier = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the packet authenticator.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Authenticator of the packet.</returns>
        Public Property Authenticator() As Byte()
            Get
                Return m_authenticator
            End Get
            Set(ByVal value As Byte())
                If value IsNot Nothing Then
                    If value.Length = 16 Then
                        m_authenticator = value
                    Else
                        Throw New ArgumentException("Authenticator must 16-byte long.")
                    End If
                Else
                    Throw New ArgumentNullException("Authenticator")
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets a list of packet attributes.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Attributes of the packet.</returns>
        Public ReadOnly Property Attributes() As List(Of RadiusPacketAttribute)
            Get
                Return m_attributes
            End Get
        End Property

        ''' <summary>
        ''' Gets the value of the specified attribute if it is present in the packet.
        ''' </summary>
        ''' <param name="type">Type of the attribute whose value is to be retrieved.</param>
        ''' <returns>Attribute value as a byte array if attribute is present; otherwise Nothing.</returns>
        Public Function GetAttributeValue(ByVal type As AttributeType) As Byte()

            For Each attrib As RadiusPacketAttribute In m_attributes
                ' Attribute found, return its value.
                If attrib.Type = type Then Return attrib.Value
            Next

            Return Nothing  ' Attribute is not present in the packet.

        End Function

#Region " Interface Implementations "

#Region " IBinaryDataProvider "

        ''' <summary>
        ''' Gets the binary lenght of the packet.
        ''' </summary>
        ''' <value></value>
        ''' <returns>32-bit signed integer value.</returns>
        Public ReadOnly Property BinaryLength() As Integer Implements IBinaryDataProvider.BinaryLength
            Get
                ' 20 bytes are fixed + length of all attributes combined
                Dim length As Integer = 20
                For Each attribute As RadiusPacketAttribute In m_attributes
                    length += attribute.BinaryLength
                Next

                Return length
            End Get
        End Property

        ''' <summary>
        ''' Gets the binary image of the packet.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A byte array.</returns>
        Public ReadOnly Property BinaryImage() As Byte() Implements IBinaryDataProvider.BinaryImage
            Get
                Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)
                image(0) = CByte(m_type)
                image(1) = m_identifier
                Array.Copy(GetBytes(CUShort(BinaryLength)), 0, image, 2, 2)
                Array.Copy(m_authenticator, 0, image, 4, m_authenticator.Length)
                Dim cursor As Integer = 20
                For Each attribute As RadiusPacketAttribute In m_attributes
                    Array.Copy(attribute.BinaryImage, 0, image, cursor, attribute.BinaryLength)
                    cursor += attribute.BinaryLength
                Next

                Return image
            End Get
        End Property

#End Region

#Region " IBinaryDataConsumer "

        ''' <summary>
        ''' Initializes the packet from the specified binary image.
        ''' </summary>
        ''' <param name="binaryImage">A byte array.</param>
        ''' <param name="startIndex">Starting point in the byte array.</param>
        ''' <returns>Number of bytes used to initialize the packet.</returns>
        Public Function Initialize(ByVal binaryImage() As Byte, ByVal startIndex As Integer) As Integer Implements IBinaryDataConsumer.Initialize

            If binaryImage IsNot Nothing AndAlso binaryImage.Length >= 20 Then
                ' We have a valid buffer to work with.
                Dim length As UShort
                m_type = CType(binaryImage(startIndex), PacketType)
                m_identifier = binaryImage(startIndex + 1)
                length = ToUInt16(binaryImage, startIndex + 2)
                Array.Copy(binaryImage, 4, m_authenticator, 0, m_authenticator.Length)
                ' Parse any attributes in the packet.
                Dim cursor As Integer = 20
                Do While cursor < length
                    Dim attribute As New RadiusPacketAttribute(binaryImage, startIndex + cursor)
                    m_attributes.Add(attribute)
                    cursor += attribute.BinaryLength
                Loop
                
                Return BinaryLength
            Else
                Throw New ArgumentException("Buffer is not valid.")
            End If

        End Function

#End Region

#End Region

#Region " Shared "

        ''' <summary>
        ''' Gets bytes for the specified text.
        ''' </summary>
        ''' <param name="value">Text blob.</param>
        ''' <returns>A byte array.</returns>
        Public Shared Function GetBytes(ByVal value As String) As Byte()

            Return Encoding.GetBytes(value)

        End Function

        ''' <summary>
        ''' Gets bytes for the specified 16-bit signed integer value.
        ''' </summary>
        ''' <param name="value">16-bit signed integer value.</param>
        ''' <returns>A byte array.</returns>
        Public Shared Function GetBytes(ByVal value As Short) As Byte()

            ' Integer values are in Big-endian (most significant byte first) format.
            Return EndianOrder.BigEndian.GetBytes(value)

        End Function

        ''' <summary>
        ''' Gets bytes for the specified 16-bit unsigned integer value.
        ''' </summary>
        ''' <param name="value">16-bit unsigned integer value.</param>
        ''' <returns>A byte array.</returns>
        <CLSCompliant(False)> _
        Public Shared Function GetBytes(ByVal value As UShort) As Byte()

            ' Integer values are in Big-endian (most significant byte first) format.
            Return EndianOrder.BigEndian.GetBytes(value)

        End Function

        ''' <summary>
        ''' Gets bytes for the specified 32-bit signed integer value.
        ''' </summary>
        ''' <param name="value">32-bit signed integer value.</param>
        ''' <returns>A byte array.</returns>
        Public Shared Function GetBytes(ByVal value As Integer) As Byte()

            ' Integer values are in Big-endian (most significant byte first) format.
            Return EndianOrder.BigEndian.GetBytes(value)

        End Function

        ''' <summary>
        ''' Gets bytes for the specified 32-bit unsigned integer value.
        ''' </summary>
        ''' <param name="value">32-bit unsigned integer value.</param>
        ''' <returns>A byte array.</returns>
        <CLSCompliant(False)> _
        Public Shared Function GetBytes(ByVal value As UInteger) As Byte()

            ' Integer values are in Big-endian (most significant byte first) format.
            Return EndianOrder.BigEndian.GetBytes(value)

        End Function

        ''' <summary>
        ''' Converts the specified byte array to text.
        ''' </summary>
        ''' <param name="buffer">A byte array.</param>
        ''' <param name="index">Starting point in the byte array.</param>
        ''' <param name="length">Number of bytes to be converted.</param>
        ''' <returns>A text blob.</returns>
        Public Shared Function ToText(ByVal buffer As Byte(), ByVal index As Integer, ByVal length As Integer) As String

            Return Encoding.GetString(buffer, index, length)

        End Function

        ''' <summary>
        ''' Converts the specified byte array to a signed 16-bit integer value.
        ''' </summary>
        ''' <param name="buffer">A byte array.</param>
        ''' <param name="index">Starting point in the byte array.</param>
        ''' <returns>A 16-bit signed integer value.</returns>
        Public Shared Function ToInt16(ByVal buffer As Byte(), ByVal index As Integer) As Short

            ' Integer values are in Big-endian (most significant byte first) format.
            Return EndianOrder.BigEndian.ToInt16(buffer, index)

        End Function

        ''' <summary>
        ''' Converts the specified byte array to an unsigned 16-bit integer value.
        ''' </summary>
        ''' <param name="buffer">A byte array.</param>
        ''' <param name="index">Starting point in the byte array.</param>
        ''' <returns>A 16-bit unsigned integer value.</returns>
        <CLSCompliant(False)> _
        Public Shared Function ToUInt16(ByVal buffer As Byte(), ByVal index As Integer) As UShort

            ' Integer values are in Big-endian (most significant byte first) format.
            Return EndianOrder.BigEndian.ToUInt16(buffer, index)

        End Function

        ''' <summary>
        ''' Converts the specified byte array to a signed 32-bit integer value.
        ''' </summary>
        ''' <param name="buffer">A byte array.</param>
        ''' <param name="index">Starting point in the byte array.</param>
        ''' <returns>A 32-bit signed integer value.</returns>
        Public Shared Function ToInt32(ByVal buffer As Byte(), ByVal index As Integer) As Integer

            ' Integer values are in Big-endian (most significant byte first) format.
            Return EndianOrder.BigEndian.ToInt32(buffer, index)

        End Function

        ''' <summary>
        ''' Converts the specified byte array to an unsigned 32-bit integer value.
        ''' </summary>
        ''' <param name="buffer">A byte array.</param>
        ''' <param name="index">Starting point in the byte array.</param>
        ''' <returns>A 32-bit unsigned integer value.</returns>
        <CLSCompliant(False)> _
        Public Shared Function ToUInt32(ByVal buffer As Byte(), ByVal index As Integer) As UInteger

            ' Integer values are in Big-endian (most significant byte first) format.
            Return EndianOrder.BigEndian.ToUInt32(buffer, index)

        End Function

        ''' <summary>
        ''' Generates an "Authenticator" value used in a RADIUS request packet sent by the client to server.
        ''' </summary>
        ''' <param name="sharedSecret">The shared secret to be used in generating the output.</param>
        ''' <returns>A byte array.</returns>
        Public Shared Function CreateRequestAuthenticator(ByVal sharedSecret As String) As Byte()

            ' We create a input buffer that'll be used to create a 16-byte value using the RSA MD5 algorithm.
            ' Since the output value (The Authenticator) has to be unique over the life of the shared secret,
            ' we prepend a randomly generated "salt" text to ensure the uniqueness of the output value.
            Dim inputString As String = GenerateKey() & sharedSecret
            Dim inputBuffer As Byte() = RadiusPacket.GetBytes(inputString)

            Return New MD5CryptoServiceProvider().ComputeHash(inputBuffer)

        End Function

        ''' <summary>
        ''' Generates an "Authenticator" value used in a RADIUS response packet sent by the server to client.
        ''' </summary>
        ''' <param name="sharedSecret">The shared secret key.</param>
        ''' <param name="requestPacket">RADIUS packet sent from client to server.</param>
        ''' <param name="responsePacket">RADIUS packet sent from server to client.</param>
        ''' <returns>A byte array.</returns>
        Public Shared Function CreateResponseAuthenticator(ByVal sharedSecret As String, _
                                                           ByVal requestPacket As RadiusPacket, _
                                                           ByVal responsePacket As RadiusPacket) As Byte()

            Dim requestBytes As Byte() = requestPacket.BinaryImage
            Dim responseBytes As Byte() = responsePacket.BinaryImage
            Dim sharedSecretBytes As Byte() = RadiusPacket.GetBytes(sharedSecret)
            Dim inputBuffer As Byte() = CreateArray(Of Byte)(responseBytes.Length + sharedSecretBytes.Length)

            ' Response authenticator is generated as follows:
            ' MD5(Code + Identifier + Length + Request Authenticator + Attributes + Shared Secret)
            '   where:
            '   Code, Identifier, Length & Attributes are from the response RADIUS packet
            '   Request Authenticator if from the request RADIUS packet
            '   Shared Secret is the shared secret ket

            Array.Copy(responseBytes, 0, inputBuffer, 0, responseBytes.Length)
            Array.Copy(requestBytes, 4, inputBuffer, 4, 16)
            Array.Copy(sharedSecretBytes, 0, inputBuffer, responseBytes.Length, sharedSecretBytes.Length)

            Return New MD5CryptoServiceProvider().ComputeHash(inputBuffer)

        End Function

        ''' <summary>
        ''' Generates an encrypted password using the RADIUS protocol specification (RFC 2285).
        ''' </summary>
        ''' <param name="password">User's password.</param>
        ''' <param name="sharedSecret">Shared secret key.</param>
        ''' <param name="requestAuthenticator">Request authenticator byte array.</param>
        ''' <returns>A byte array.</returns>
        Public Shared Function EncryptPassword(ByVal password As String, _
                                               ByVal sharedSecret As String, _
                                               ByVal requestAuthenticator As Byte()) As Byte()

            ' Max length of the password can be 130 according to RFC 2865. Since 128 is the closest multiple
            ' of 16 (password segment length), we allow the password to be no longer than 128 characters.
            If password.Length <= 128 Then
                Dim result As Byte()
                Dim xorBytes As Byte() = Nothing
                Dim passwordBytes As Byte() = RadiusPacket.GetBytes(password)
                Dim sharedSecretBytes As Byte() = RadiusPacket.GetBytes(sharedSecret)
                Dim md5HashInputBytes As Byte() = CreateArray(Of Byte)(sharedSecretBytes.Length + 16)
                Dim md5Provider As New MD5CryptoServiceProvider()
                If passwordBytes.Length Mod 16 = 0 Then
                    ' Length of password is a multiple of 16.
                    result = CreateArray(Of Byte)(passwordBytes.Length)
                Else
                    ' Length of password is not a multiple of 16, so we'll take the multiple of 16 that's next 
                    ' closest to the password's length and leave the empty space at the end as padding.
                    result = CreateArray(Of Byte)(((passwordBytes.Length \ 16) * 16) + 16)
                End If

                ' Copy the password to the result buffer where it'll be XORed.
                Array.Copy(passwordBytes, 0, result, 0, passwordBytes.Length)
                ' For the first 16-byte segment of the password, password characters are to be XORed with the
                ' MD5 hash value that's computed as follows:
                '   MD5(Shared secret key + Request authenticator)
                Array.Copy(sharedSecretBytes, 0, md5HashInputBytes, 0, sharedSecretBytes.Length)
                Array.Copy(requestAuthenticator, 0, md5HashInputBytes, sharedSecretBytes.Length, requestAuthenticator.Length)
                For i As Integer = 0 To result.Length - 1 Step 16
                    ' Perform XOR-based encryption of the password in 16-byte segments.
                    If i > 0 Then
                        ' For passwords that are more than 16 characters in length, each consecutive 16-byte 
                        ' segment of the password is XORed with MD5 hash value that's computed as follows:
                        '   MD5(Shared secret key + XOR bytes used in the previous segment)
                        Array.Copy(xorBytes, 0, md5HashInputBytes, sharedSecretBytes.Length, xorBytes.Length)
                    End If
                    xorBytes = md5Provider.ComputeHash(md5HashInputBytes)

                    ' XOR the password bytes in the current segment with the XOR bytes.
                    For j As Integer = i To (i + 16) - 1
                        result(j) = result(j) Xor xorBytes(j)
                    Next
                Next

                Return result
            Else
                Throw New ArgumentException("Password can be a maximum of 128 characters in length.")
            End If

        End Function

#End Region

#End Region

    End Class

End Namespace