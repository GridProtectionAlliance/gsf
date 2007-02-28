'*******************************************************************************************************
'  Tva.Security.Cryptography.Common.vb - Handy Cryptography Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2003 - J. Ritchie Carroll
'       Original version of source code generated
'  01/04/2006 - J. Ritchie Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Crypto)
'  02/28/2007 - J. Ritchie Carroll
'       Change string based encrypt and decrypt functions to return null if
'       input string to be encrypted or decrypted was null or empty
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports Tva.Common
Imports Tva.Math.Common
Imports Tva.IO.Common
Imports Tva.Interop.Bit

Namespace Security.Cryptography

    ''' <summary>Common Cryptographic Functions</summary>
    Public NotInheritable Class Common

        Public Delegate Sub ProgressEventHandler(ByVal bytesCompleted As Long, ByVal bytesTotal As Long)

        ' IMPORTANT! Never change the following constants or you will break cross-application crypto operability:
        Private Const StandardKey As String = "{&-<%=($#/T.V:A!\,@[20O3]*^_j`|?)>+~}"
        Private Const BufferSize As Integer = 262144    ' 256K

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data generated with the given parameter using the standard encryption key and encryption level 1</summary>
        Public Shared Function Encrypt(ByVal str As String) As String

            Return Encrypt(str, Nothing, EncryptLevel.Level1)

        End Function

        ''' <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data generated with the given parameters using standard encryption</summary>
        Public Shared Function Encrypt(ByVal str As String, ByVal strength As EncryptLevel) As String

            Return Encrypt(str, Nothing, strength)

        End Function

        ''' <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data generated with the given parameters</summary>
        Public Shared Function Encrypt(ByVal str As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel) As String

            If String.IsNullOrEmpty(str) Then Return Nothing
            If String.IsNullOrEmpty(encryptionKey) Then encryptionKey = StandardKey

            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

            Return Convert.ToBase64String(Encrypt(Encoding.Unicode.GetBytes(str), rgbKey, rgbIV, strength))

        End Function

        ''' <summary>Returns a binary array of encrypted data for the given parameters</summary>
        Public Shared Function Encrypt(ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Byte()

            If strength = EncryptLevel.None Then Return data

            ' Perform requested levels of encryption
            data = Crypt(data, key)
            If strength >= EncryptLevel.Level2 Then
                data = Encrypt(New TripleDESCryptoServiceProvider, data, key, IV)
                If strength >= EncryptLevel.Level3 Then
                    data = Encrypt(New RC2CryptoServiceProvider, data, key, IV)
                    If strength >= EncryptLevel.Level4 Then
                        data = Encrypt(New RijndaelManaged, data, key, IV)
                    End If
                End If
            End If

            Return data

        End Function

        ''' <summary>Returns a binary array of encrypted data for the given parameters</summary>
        Public Shared Function Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte()) As Byte()

            Return DirectCast(Encrypt(algorithm, New MemoryStream(data), key, IV), MemoryStream).ToArray()

        End Function

        ''' <summary>Returns a stream of encrypted data for the given parameters</summary>
        Public Shared Function Encrypt(ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Stream

            If strength = EncryptLevel.None Then Return inStream

            ' Perform requested levels of encryption
            inStream = Crypt(inStream, key)
            If strength >= EncryptLevel.Level2 Then
                inStream = Encrypt(New TripleDESCryptoServiceProvider, inStream, key, IV)
                If strength >= EncryptLevel.Level3 Then
                    inStream = Encrypt(New RC2CryptoServiceProvider, inStream, key, IV)
                    If strength >= EncryptLevel.Level4 Then
                        inStream = Encrypt(New RijndaelManaged, inStream, key, IV)
                    End If
                End If
            End If

            Return inStream

        End Function

        ''' <summary>Returns a stream of encrypted data for the given parameters</summary>
        Public Shared Function Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte()) As Stream

            Dim outStream As New MemoryStream

            Encrypt(algorithm, inStream, outStream, key, IV)
            outStream.Position = 0

            Return outStream

        End Function

        ''' <summary>Encrypts input stream onto output stream for the given parameters</summary>
        Public Shared Sub Encrypt(ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

            Dim inBuffer As Byte() = CreateArray(Of Byte)(BufferSize)
            Dim outBuffer As Byte()
            Dim lengthBuffer As Byte()
            Dim read As Integer
            Dim total As Long
            Dim length As Long = -1

            ' Send initial progress event
            If Not progressHandler Is Nothing Then
                Try
                    If inStream.CanSeek Then length = inStream.Length
                Catch
                    length = -1
                End Try

                progressHandler(0, length)
            End If

            ' Read initial buffer
            read = inStream.Read(inBuffer, 0, BufferSize)

            While read > 0
                ' Encrypt buffer
                outBuffer = Encrypt(CopyBuffer(inBuffer, 0, read), key, IV, strength)

                ' The destination encryption stream length doesn't have to be same as the input stream length, so we
                ' prepend the final size of each encrypted buffer onto the destination ouput stream so that we can
                ' safely decrypt the stream in a "chunked" fashion later...
                lengthBuffer = BitConverter.GetBytes(outBuffer.Length)
                outStream.Write(lengthBuffer, 0, lengthBuffer.Length)
                outStream.Write(outBuffer, 0, outBuffer.Length)

                ' Update encryption progress
                If Not progressHandler Is Nothing Then
                    total += read
                    progressHandler(total, length)
                End If

                ' Read next buffer
                read = inStream.Read(inBuffer, 0, BufferSize)
            End While

        End Sub

        ''' <summary>Encrypts input stream onto output stream for the given parameters</summary>
        Public Shared Sub Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte())

            ' This is the root encryption function - eventually, all the encryption functions perform their actual encryption here...
            Dim rgbKey As Byte() = GetLegalKey(algorithm, key)
            Dim rgbIV As Byte() = GetLegalIV(algorithm, IV)
            Dim encodeStream As New CryptoStream(outStream, algorithm.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write)
            Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
            Dim read As Integer

            ' Encrypt data onto output stream
            read = inStream.Read(buffer, 0, BufferSize)

            While read > 0
                encodeStream.Write(buffer, 0, read)
                read = inStream.Read(buffer, 0, BufferSize)
            End While

            encodeStream.FlushFinalBlock()

        End Sub

        ''' <summary>Creates an encrypted file from source file data</summary>
        Public Shared Sub EncryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal strength As EncryptLevel)

            EncryptFile(sourceFileName, destFileName, Nothing, strength, Nothing)

        End Sub

        ''' <summary>Creates an encrypted file from source file data</summary>
        Public Shared Sub EncryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

            If String.IsNullOrEmpty(encryptionKey) Then encryptionKey = StandardKey

            Dim sourceFileStream As FileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim destFileStream As FileStream = File.Create(destFileName)
            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

            Encrypt(sourceFileStream, destFileStream, rgbKey, rgbIV, strength, progressHandler)

            destFileStream.Flush()
            destFileStream.Close()
            sourceFileStream.Close()

        End Sub

        ''' <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given parameter using the standard encryption key and encryption level 1</summary>
        Public Shared Function Decrypt(ByVal str As String) As String

            Return Decrypt(str, Nothing, EncryptLevel.Level1)

        End Function

        ''' <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given parameters using the standard encryption key</summary>
        Public Shared Function Decrypt(ByVal str As String, ByVal strength As EncryptLevel) As String

            Return Decrypt(str, Nothing, strength)

        End Function

        ''' <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given parameters</summary>
        Public Shared Function Decrypt(ByVal str As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel) As String

            If String.IsNullOrEmpty(str) Then Return Nothing
            If String.IsNullOrEmpty(encryptionKey) Then encryptionKey = StandardKey

            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

            Return Encoding.Unicode.GetString(Decrypt(Convert.FromBase64String(str), rgbKey, rgbIV, strength))

        End Function

        ''' <summary>Returns a binary array of decrypted data for the given parameters</summary>
        Public Shared Function Decrypt(ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Byte()

            If strength = EncryptLevel.None Then Return data

            ' Perform requested levels of decryption
            If strength >= EncryptLevel.Level4 Then data = Decrypt(New RijndaelManaged, data, key, IV)
            If strength >= EncryptLevel.Level3 Then data = Decrypt(New RC2CryptoServiceProvider, data, key, IV)
            If strength >= EncryptLevel.Level2 Then data = Decrypt(New TripleDESCryptoServiceProvider, data, key, IV)

            Return Crypt(data, key)

        End Function

        ''' <summary>Returns a binary array of decrypted data for the given parameters</summary>
        Public Shared Function Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte()) As Byte()

            Return DirectCast(Decrypt(algorithm, New MemoryStream(data), key, IV), MemoryStream).ToArray()

        End Function

        ''' <summary>Returns a stream of decrypted data for the given parameters</summary>
        Public Shared Function Decrypt(ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Stream

            If strength = EncryptLevel.None Then Return inStream

            If strength >= EncryptLevel.Level4 Then inStream = Decrypt(New RijndaelManaged, inStream, key, IV)
            If strength >= EncryptLevel.Level3 Then inStream = Decrypt(New RC2CryptoServiceProvider, inStream, key, IV)
            If strength >= EncryptLevel.Level2 Then inStream = Decrypt(New TripleDESCryptoServiceProvider, inStream, key, IV)

            Return Crypt(inStream, key)

        End Function

        ''' <summary>Returns a stream of decrypted data for the given parameters</summary>
        Public Shared Function Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte()) As Stream

            Dim outStream As New MemoryStream

            Decrypt(algorithm, inStream, outStream, key, IV)
            outStream.Position = 0

            Return outStream

        End Function

        ''' <summary>Decrypts input stream onto output stream for the given parameters</summary>
        Public Shared Sub Decrypt(ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

            Dim inBuffer As Byte()
            Dim outBuffer As Byte()
            Dim lengthBuffer As Byte() = BitConverter.GetBytes(CType(0, Int32))
            Dim size, read As Integer
            Dim total As Long
            Dim length As Long = -1

            ' Send initial progress event
            If Not progressHandler Is Nothing Then
                Try
                    If inStream.CanSeek Then length = inStream.Length
                Catch
                    length = -1
                End Try

                progressHandler(0, length)
            End If

            ' When the source stream was encrypted, it was known that the encrypted stream length didn't have to be same as
            ' the input stream length, so we prepended the final size of the each encrypted buffer onto the destination
            ' ouput stream (now the input stream to this function) so that we could safely decrypt the stream in a
            ' "chunked" fashion, hence the following:

            ' Read the size of the next buffer from the stream
            read = inStream.Read(lengthBuffer, 0, lengthBuffer.Length)

            While read > 0
                ' Convert the byte array containing the buffer size into an integer
                size = BitConverter.ToInt32(lengthBuffer, 0)

                If size > 0 Then
                    ' Create and read the next buffer
                    inBuffer = CreateArray(Of Byte)(size)
                    read = inStream.Read(inBuffer, 0, size)

                    If read > 0 Then
                        ' Decrypt buffer
                        outBuffer = Decrypt(inBuffer, key, IV, strength)
                        outStream.Write(outBuffer, 0, outBuffer.Length)

                        ' Update decryption progress
                        If Not progressHandler Is Nothing Then
                            total += (read + lengthBuffer.Length)
                            progressHandler(total, length)
                        End If
                    End If
                End If

                ' Read the size of the next buffer from the stream
                read = inStream.Read(lengthBuffer, 0, lengthBuffer.Length)
            End While

        End Sub

        ''' <summary>Decrypts input stream onto output stream for the given parameters</summary>
        Public Shared Sub Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte())

            ' This is the root decryption function - eventually, all the decryption functions perform their actual decryption here...
            Dim rgbKey As Byte() = GetLegalKey(algorithm, key)
            Dim rgbIV As Byte() = GetLegalIV(algorithm, IV)
            Dim decodeStream As New CryptoStream(outStream, algorithm.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write)
            Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
            Dim read As Integer

            ' Decrypt data onto output stream
            read = inStream.Read(buffer, 0, BufferSize)

            While read > 0
                decodeStream.Write(buffer, 0, read)
                read = inStream.Read(buffer, 0, BufferSize)
            End While

            decodeStream.FlushFinalBlock()

        End Sub

        ''' <summary>Creates a decrypted file from source file data</summary>
        Public Shared Sub DecryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal strength As EncryptLevel)

            DecryptFile(sourceFileName, destFileName, Nothing, strength, Nothing)

        End Sub

        ''' <summary>Creates a decrypted file from source file data</summary>
        Public Shared Sub DecryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

            If String.IsNullOrEmpty(encryptionKey) Then encryptionKey = StandardKey

            Dim sourceFileStream As FileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim destFileStream As FileStream = File.Create(destFileName)
            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

            Decrypt(sourceFileStream, destFileStream, rgbKey, rgbIV, strength, progressHandler)

            destFileStream.Flush()
            destFileStream.Close()
            sourceFileStream.Close()

        End Sub

        ''' <summary>Coerces key to maximum legal bit length for given encryption algorithm</summary>
        Public Shared Function GetLegalKey(ByVal algorithm As SymmetricAlgorithm, ByVal key As Byte()) As Byte()

            Dim rgbKey As Byte() = CreateArray(Of Byte)(algorithm.LegalKeySizes(0).MaxSize \ 8)

            For x As Integer = 0 To rgbKey.Length - 1
                If x < key.Length Then
                    rgbKey(x) = key(x)
                Else
                    rgbKey(x) = Encoding.ASCII.GetBytes(StandardKey.Substring(x Mod StandardKey.Length, 1))(0)
                End If
            Next

            Return rgbKey

        End Function

        ''' <summary>Coerces initialization vector to legal block size for given encryption algorithm</summary>
        Public Shared Function GetLegalIV(ByVal algorithm As SymmetricAlgorithm, ByVal IV As Byte()) As Byte()

            Dim rgbIV As Byte() = CreateArray(Of Byte)(algorithm.LegalBlockSizes(0).MinSize \ 8)

            For x As Integer = 0 To rgbIV.Length - 1
                If x < IV.Length Then
                    rgbIV(x) = IV(IV.Length - 1 - x)
                Else
                    rgbIV(x) = Encoding.ASCII.GetBytes(StandardKey.Substring(x Mod StandardKey.Length, 1))(0)
                End If
            Next

            Return rgbIV

        End Function

        ''' <summary>Encrypts or decrypts input stream onto output stream using XOR based algorithms.  Call once to encrypt, call again with same key to decrypt.</summary>
        Public Shared Function Crypt(ByVal inStream As Stream, ByVal encryptionKey As Byte()) As Stream

            If inStream.CanSeek Then
                Dim outStream As New MemoryStream
                Dim buffer As Byte() = CreateArray(Of Byte)(inStream.Length)

                inStream.Read(buffer, 0, buffer.Length)
                buffer = Crypt(buffer, encryptionKey)
                outStream.Write(buffer, 0, buffer.Length)

                outStream.Position = 0
                Return outStream
            Else
                ' For streams that can't be positioned, we copy all data onto a memory stream and try again
                Dim outStream As New MemoryStream
                Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
                Dim read As Integer

                read = inStream.Read(buffer, 0, BufferSize)

                While read > 0
                    outStream.Write(buffer, 0, read)
                    read = inStream.Read(buffer, 0, BufferSize)
                End While

                outStream.Position = 0
                Return Crypt(outStream, encryptionKey)
            End If

        End Function

        ''' <summary>Encrypts or decrypts a string using XOR based algorithms.  Call once to encrypt, call again with same key to decrypt.</summary>
        Public Shared Function Crypt(ByVal str As String, ByVal encryptionKey As String) As String

            Return Encoding.Unicode.GetString(Crypt(Encoding.Unicode.GetBytes(str), Encoding.Unicode.GetBytes(encryptionKey)))

        End Function

        ''' <summary>Encrypts or decrypts data using XOR based algorithms.  Call once to encrypt, call again with same key to decrypt.</summary>
        Public Shared Function Crypt(ByVal data As Byte(), ByVal encryptionKey As Byte()) As Byte()

            ' The longer the encryption key the better the encryption
            ' Repeated encryption sequences do not occur for (3 * encryptionKey.Length) unique bytes

            Dim cryptData As Byte() = CreateArray(Of Byte)(data.Length)
            Dim keyPos As Long
            Dim algorithm As Integer
            Dim cryptChar As Integer

            ' Re-seed random number generator
            Rnd(-1)
            Randomize(encryptionKey(0))

            keyPos = 0
            algorithm = 0

            For x As Integer = 0 To data.Length - 1
                cryptData(x) = data(x)
                If cryptData(x) > 0 Then
                    Select Case algorithm
                        Case 0
                            cryptChar = encryptionKey(keyPos)
                            If cryptData(x) <> cryptChar Then cryptData(x) = (cryptData(x) Xor cryptChar)
                        Case 1
                            cryptChar = Int(Rnd() * (encryptionKey(keyPos) + 1))
                            If cryptData(x) <> cryptChar Then cryptData(x) = (cryptData(x) Xor cryptChar)
                        Case 2
                            cryptChar = Int(Rnd() * 256)
                            If cryptData(x) <> cryptChar Then cryptData(x) = (cryptData(x) Xor cryptChar)
                    End Select
                End If

                ' Select next encryption algorithm
                algorithm += 1
                If algorithm = 3 Then
                    algorithm = 0

                    ' Select next encryption key
                    keyPos = keyPos + 1
                    If keyPos > encryptionKey.Length - 1 Then keyPos = 0
                End If
            Next

            Return cryptData

        End Function

        ''' <summary>Generates a random key useful for cryptographic functions</summary>
        Public Shared Function GenerateKey() As String

            Dim keyChars As Char()
            Dim keyChar As Char
            Dim y As Integer

            ' Generate a character array of unique values
            With New StringBuilder
                .Append(StandardKey)
                .Append(Guid.NewGuid.ToString.ToLower)
                .Append(System.DateTime.UtcNow.Ticks)
                .Append(Environment.MachineName)
                .Append(GetSeedFromKey(Microsoft.VisualBasic.Timer))
                .Append(Environment.UserDomainName)
                .Append(Environment.UserName)
                .Append(Microsoft.VisualBasic.Timer)
                .Append(System.DateTime.Now.ToString)
                .Append(Guid.NewGuid.ToString.ToUpper)
                keyChars = .ToString.Replace(" "c, "©"c).Replace("-"c, "~"c).Replace("/"c, "%"c).ToCharArray
            End With

            ' Swap values around in array at random
            For x As Integer = 0 To keyChars.Length - 1
                y = RandomInt32Between(1, keyChars.Length) - 1
                If x <> y Then
                    keyChar = keyChars(x)
                    keyChars(x) = keyChars(y)
                    keyChars(y) = keyChar
                End If
            Next

            Return New String(keyChars)

        End Function

        ''' <summary>Returns a coded string representing a number which can later be decoded with <see cref="GetSeedFromKey" />.</summary>
        ''' <remarks>
        ''' <para>This function was designed for Microsoft.VisualBasic.Timer values.</para>
        ''' </remarks>
        Public Shared Function GetKeyFromSeed(ByVal seed As Integer) As String

            ' This is a handy algorithm for encoding a timer value
            ' Use GetSeedFromKey to decode

            Dim alphaIndex As Integer
            Dim seedBytes(3) As Byte

            If seed < 0 Then Throw New ArgumentException("Cannot calculate key from negative seed")
            If LoByte(LoWord(seed)) > 0 Then Throw New ArgumentException("Seed is too large (function was designed for Microsoft.VisualBasic.Timer values)")

            ' Break seed into its component bytes
            seedBytes(0) = HiByte(HiWord(seed))
            seedBytes(1) = LoByte(HiWord(seed))
            seedBytes(2) = HiByte(LoWord(seed))

            ' Create alpha-numeric key string
            With New StringBuilder
                For x As Integer = 0 To 2
                    alphaIndex = RandomInt32Between(0, 25)
                    If x > 0 Then .Append("-"c)
                    .Append(Convert.ToChar(Asc("A"c) + (25 - alphaIndex)))
                    .Append(seedBytes(x) + alphaIndex)
                Next

                Return .ToString
            End With

        End Function

        ''' <summary>Returns the number from a string coded with <see cref="GetKeyFromSeed" />.</summary>
        Public Shared Function GetSeedFromKey(ByVal key As String) As Integer

            Dim seedBytes As Byte() = CreateArray(Of Byte)(3)
            Dim delimeter1 As Integer
            Dim delimeter2 As Integer
            Dim code As String = ""
            Dim value As Integer

            ' Remove all white space from specified parameter
            key = key.Trim.ToUpper

            If Len(key) > 5 And Len(key) < 15 Then
                ' Get Delimiter positions
                delimeter1 = InStr(key, "-")
                delimeter2 = InStr(delimeter1 + 1, key, "-", 0)

                If delimeter1 > 0 And delimeter2 > 0 Then
                    For x As Integer = 0 To 2
                        ' Extract encoded byte
                        Select Case x
                            Case 0
                                code = Left(key, delimeter1 - 1)
                            Case 1
                                code = Mid(key, delimeter1 + 1, delimeter2 - delimeter1 - 1)
                            Case 2
                                code = Right(key, Len(key) - delimeter2)
                        End Select

                        ' Calculate byte
                        value = Val(Right(code, Len(code) - 1)) - (25 - (Asc(Left(code, 1)) - Asc("A")))

                        ' Validate calculation
                        If value >= 0 And value <= 255 Then
                            seedBytes(x) = CByte(value)
                        Else
                            ' Invalid key, exit with -1
                            Return -1
                        End If
                    Next

                    ' Create seed from its component bytes
                    Return MakeDWord(MakeWord(seedBytes(0), seedBytes(1)), MakeWord(seedBytes(2), 0))
                End If
            End If

            ' Invalid key, exit with -1
            Return -1

        End Function

    End Class

End Namespace
