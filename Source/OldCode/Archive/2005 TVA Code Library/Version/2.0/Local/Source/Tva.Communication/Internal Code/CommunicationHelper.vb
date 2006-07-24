' 06-01-06

Imports System.IO
Imports System.Net
Imports Tva.IO.Compression.Common
Imports Tva.Security.Cryptography
Imports Tva.Security.Cryptography.Common

Friend NotInheritable Class CommunicationHelper

    ''' <summary>
    ''' Gets an IP endpoint for the specified host name and port number.
    ''' </summary>
    ''' <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
    ''' <param name="port">The port number to be associated with the address.</param>
    ''' <returns>IP endpoint for the specified host name and port number.</returns>
    Public Shared Function GetIpEndPoint(ByVal hostNameOrAddress As String, ByVal port As Integer) As IPEndPoint

        ' SocketException will be thrown is the host is not found.
        Return New IPEndPoint(Dns.GetHostEntry(hostNameOrAddress).AddressList(0), port)

    End Function

    ''' <summary>
    ''' Determines whether the specified port is valid.
    ''' </summary>
    ''' <param name="port">The port number to be validated.</param>
    ''' <returns>True if the port number is valid.</returns>
    Public Shared Function ValidPortNumber(ByVal port As String) As Boolean

        Dim portNumber As Integer
        If Integer.TryParse(port, portNumber) Then
            ' The specified port is a valid integer value.
            If portNumber >= 0 AndAlso portNumber <= 65535 Then
                ' The port number is within the valid range.
                Return True
            Else
                Throw New ArgumentOutOfRangeException("Port", "Port number must be between 0 and 65535.")
            End If
        Else
            Throw New ArgumentException("Port number is not a valid number.")
        End If

    End Function

    Public Shared Function CompressData(ByVal data As Byte(), _
            ByVal compressionLevel As Tva.IO.Compression.CompressLevel) As Byte()

        Try
            If compressionLevel <> IO.Compression.CompressLevel.NoCompression Then
                Return CType(Compress(Serialization.GetStream(data), compressionLevel), MemoryStream).ToArray()
            Else
                ' No compression is required.
                Return data
            End If
        Catch ex As Exception
            ' We'll return what we received if encounter an exception during compression.
            Return data
        End Try

    End Function

    Public Shared Function UncompressData(ByVal data As Byte(), _
            ByVal compressionLevel As Tva.IO.Compression.CompressLevel) As Byte()

        Try
            If compressionLevel <> IO.Compression.CompressLevel.NoCompression Then
                Return CType(Uncompress(Serialization.GetStream(data)), MemoryStream).ToArray()
            Else
                ' No uncompression is required.
                Return data
            End If
        Catch ex As Exception
            ' We'll return what we received if encounter an exception during uncompression.
            Return data
        End Try

    End Function

    Public Shared Function EncryptData(ByVal data As Byte(), ByVal encryptionKey As String, _
            ByVal encryptionLevel As Tva.Security.Cryptography.EncryptLevel) As Byte()

        If Not String.IsNullOrEmpty(encryptionKey) AndAlso encryptionLevel <> EncryptLevel.None Then
            Dim key As Byte() = System.Text.Encoding.ASCII.GetBytes(encryptionKey)
            Dim iv As Byte() = System.Text.Encoding.ASCII.GetBytes(encryptionKey)
            Return Encrypt(data, key, iv, encryptionLevel)
        Else
            ' No encryption is required.
            Return data
        End If

    End Function

    Public Shared Function DecryptData(ByVal data As Byte(), ByVal encryptionKey As String, _
            ByVal encryptionLevel As Tva.Security.Cryptography.EncryptLevel) As Byte()

        If Not String.IsNullOrEmpty(encryptionKey) AndAlso encryptionLevel <> EncryptLevel.None Then
            Dim key As Byte() = System.Text.Encoding.ASCII.GetBytes(encryptionKey)
            Dim iv As Byte() = System.Text.Encoding.ASCII.GetBytes(encryptionKey)
            Return Decrypt(data, key, iv, encryptionLevel)
        Else
            Return data
        End If

    End Function

End Class