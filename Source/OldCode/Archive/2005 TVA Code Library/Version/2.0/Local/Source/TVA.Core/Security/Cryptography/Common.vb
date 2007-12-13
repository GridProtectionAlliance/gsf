'*******************************************************************************************************
'  TVA.Security.Cryptography.Common.vb - Handy Cryptography Functions
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
'       Generated original version of source code.
'  01/04/2006 - J. Ritchie Carroll
'       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Crypto).
'  02/28/2007 - J. Ritchie Carroll
'       Changed string-based encrypt and decrypt functions to return null if
'       input string to be encrypted or decrypted was null or empty.
'  10/11/2007 - J. Ritchie Carroll
'       Added Obfuscate and Deobfuscate functions that perform data obfuscation
'       based upon simple bit-rotation algorithms.
'  12/13/2007 - Darrell Zuercher
'       Edited code comments.
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports TVA.Common
Imports TVA.Math.Common
Imports TVA.IO.Common
Imports TVA.Interop.Bit

Namespace Security.Cryptography

    ''' <summary>Performs common cryptographic functions.</summary>
    Public NotInheritable Class Common

        Public Delegate Sub ProgressEventHandler(ByVal bytesCompleted As Long, ByVal bytesTotal As Long)

        ' IMPORTANT! Never change the following constants, or you will break cross-application crypto operability:
        Private Const StandardKey As String = "{&-<%=($#/T.V:A!\,@[20O3]*^_j`|?)>+~}"
        Private Const BufferSize As Integer = 262144    ' 256K

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated.

        End Sub

        ''' <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        ''' the given parameter, using the standard encryption key and encryption level 1,</summary>
        Public Shared Function Encrypt(ByVal str As String) As String

            Return Encrypt(str, Nothing, EncryptLevel.Level1)

        End Function

        ''' <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        ''' the given parameters using standard encryption.</summary>
        Public Shared Function Encrypt(ByVal str As String, ByVal strength As EncryptLevel) As String

            Return Encrypt(str, Nothing, strength)

        End Function

        ''' <summary>Returns a Base64 encoded string of the returned binary array of the encrypted data, generated with
        ''' the given parameters.</summary>
        Public Shared Function Encrypt(ByVal str As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel) As String

            If String.IsNullOrEmpty(str) Then Return Nothing
            If String.IsNullOrEmpty(encryptionKey) Then encryptionKey = StandardKey

            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

            Return Convert.ToBase64String(Encrypt(Encoding.Unicode.GetBytes(str), rgbKey, rgbIV, strength))

        End Function

        ''' <summary>Returns a binary array of encrypted data for the given parameters.</summary>
        Public Shared Function Encrypt(ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Byte()

            If strength = EncryptLevel.None Then Return data

            ' Performs requested levels of encryption.
            data = Crypt(data, key)
            If strength >= EncryptLevel.Level2 Then
                data = Encrypt(New TripleDESCryptoServiceProvider, data, key, IV)
                If strength >= EncryptLevel.Level3 Then
                    data = Encrypt(New RC2CryptoServiceProvider, data, key, IV)
                    If strength >= EncryptLevel.Level4 Then
                        data = Encrypt(New RijndaelManaged, data, key, IV)
                        If strength >= EncryptLevel.Level5 Then
                            data = Obfuscate(data, key)
                        End If
                    End If
                End If
            End If

            Return data

        End Function

        ''' <summary>Returns a binary array of encrypted data for the given parameters.</summary>
        Public Shared Function Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte()) As Byte()

            Return DirectCast(Encrypt(algorithm, New MemoryStream(data), key, IV), MemoryStream).ToArray()

        End Function

        ''' <summary>Returns a stream of encrypted data for the given parameters.</summary>
        Public Shared Function Encrypt(ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Stream

            If strength = EncryptLevel.None Then Return inStream

            ' Performs requested levels of encryption.
            inStream = Crypt(inStream, key)
            If strength >= EncryptLevel.Level2 Then
                inStream = Encrypt(New TripleDESCryptoServiceProvider, inStream, key, IV)
                If strength >= EncryptLevel.Level3 Then
                    inStream = Encrypt(New RC2CryptoServiceProvider, inStream, key, IV)
                    If strength >= EncryptLevel.Level4 Then
                        inStream = Encrypt(New RijndaelManaged, inStream, key, IV)
                        If strength >= EncryptLevel.Level5 Then
                            inStream = Obfuscate(inStream, key)
                        End If
                    End If
                End If
            End If

            Return inStream

        End Function

        ''' <summary>Returns a stream of encrypted data for the given parameters.</summary>
        Public Shared Function Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte()) As Stream

            Dim outStream As New MemoryStream

            Encrypt(algorithm, inStream, outStream, key, IV)
            outStream.Position = 0

            Return outStream

        End Function

        ''' <summary>Encrypts input stream onto output stream for the given parameters.</summary>
        Public Shared Sub Encrypt(ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

            Dim inBuffer As Byte() = CreateArray(Of Byte)(BufferSize)
            Dim outBuffer As Byte()
            Dim lengthBuffer As Byte()
            Dim read As Integer
            Dim total As Long
            Dim length As Long = -1

            ' Sends initial progress event.
            If Not progressHandler Is Nothing Then
                Try
                    If inStream.CanSeek Then length = inStream.Length
                Catch
                    length = -1
                End Try

                progressHandler(0, length)
            End If

            ' Reads initial buffer.
            read = inStream.Read(inBuffer, 0, BufferSize)

            While read > 0
                ' Encrypts buffer.
                outBuffer = Encrypt(CopyBuffer(inBuffer, 0, read), key, IV, strength)

                ' The destination encryption stream length does not have to be same as the input stream length, so we
                ' prepend the final size of each encrypted buffer onto the destination ouput stream so that we can
                ' safely decrypt the stream in a "chunked" fashion later.
                lengthBuffer = BitConverter.GetBytes(outBuffer.Length)
                outStream.Write(lengthBuffer, 0, lengthBuffer.Length)
                outStream.Write(outBuffer, 0, outBuffer.Length)

                ' Updates encryption progress.
                If Not progressHandler Is Nothing Then
                    total += read
                    progressHandler(total, length)
                End If

                ' Reads next buffer.
                read = inStream.Read(inBuffer, 0, BufferSize)
            End While

        End Sub

        ''' <summary>Encrypts input stream onto output stream for the given parameters.</summary>
        Public Shared Sub Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte())

            ' This is the root encryption function. Eventually, all the encryption functions perform their actual
            ' encryption here.
            Dim rgbKey As Byte() = GetLegalKey(algorithm, key)
            Dim rgbIV As Byte() = GetLegalIV(algorithm, IV)
            Dim encodeStream As New CryptoStream(outStream, algorithm.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write)
            Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
            Dim read As Integer

            ' Encrypts data onto output stream.
            read = inStream.Read(buffer, 0, BufferSize)

            While read > 0
                encodeStream.Write(buffer, 0, read)
                read = inStream.Read(buffer, 0, BufferSize)
            End While

            encodeStream.FlushFinalBlock()

        End Sub

        ''' <summary>Creates an encrypted file from source file data.</summary>
        Public Shared Sub EncryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal strength As EncryptLevel)

            EncryptFile(sourceFileName, destFileName, Nothing, strength, Nothing)

        End Sub

        ''' <summary>Creates an encrypted file from source file data.</summary>
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

        ''' <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        ''' parameter using the standard encryption key and encryption level 1.</summary>
        Public Shared Function Decrypt(ByVal str As String) As String

            Return Decrypt(str, Nothing, EncryptLevel.Level1)

        End Function

        ''' <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        ''' parameters using the standard encryption key.</summary>
        Public Shared Function Decrypt(ByVal str As String, ByVal strength As EncryptLevel) As String

            Return Decrypt(str, Nothing, strength)

        End Function

        ''' <summary>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given
        ''' parameters.</summary>
        Public Shared Function Decrypt(ByVal str As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel) As String

            If String.IsNullOrEmpty(str) Then Return Nothing
            If String.IsNullOrEmpty(encryptionKey) Then encryptionKey = StandardKey

            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

            Return Encoding.Unicode.GetString(Decrypt(Convert.FromBase64String(str), rgbKey, rgbIV, strength))

        End Function

        ''' <summary>Returns a binary array of decrypted data for the given parameters.</summary>
        Public Shared Function Decrypt(ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Byte()

            If strength = EncryptLevel.None Then Return data

            ' Performs requested levels of decryption.
            If strength >= EncryptLevel.Level5 Then data = Deobfuscate(data, key)
            If strength >= EncryptLevel.Level4 Then data = Decrypt(New RijndaelManaged, data, key, IV)
            If strength >= EncryptLevel.Level3 Then data = Decrypt(New RC2CryptoServiceProvider, data, key, IV)
            If strength >= EncryptLevel.Level2 Then data = Decrypt(New TripleDESCryptoServiceProvider, data, key, IV)

            Return Crypt(data, key)

        End Function

        ''' <summary>Returns a binary array of decrypted data for the given parameters.</summary>
        Public Shared Function Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte()) As Byte()

            Return DirectCast(Decrypt(algorithm, New MemoryStream(data), key, IV), MemoryStream).ToArray()

        End Function

        ''' <summary>Returns a stream of decrypted data for the given parameters.</summary>
        Public Shared Function Decrypt(ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Stream

            If strength = EncryptLevel.None Then Return inStream

            ' Performs requested levels of decryption.
            If strength >= EncryptLevel.Level5 Then inStream = Deobfuscate(inStream, key)
            If strength >= EncryptLevel.Level4 Then inStream = Decrypt(New RijndaelManaged, inStream, key, IV)
            If strength >= EncryptLevel.Level3 Then inStream = Decrypt(New RC2CryptoServiceProvider, inStream, key, IV)
            If strength >= EncryptLevel.Level2 Then inStream = Decrypt(New TripleDESCryptoServiceProvider, inStream, key, IV)

            Return Crypt(inStream, key)

        End Function

        ''' <summary>Returns a stream of decrypted data for the given parameters.</summary>
        Public Shared Function Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte()) As Stream

            Dim outStream As New MemoryStream

            Decrypt(algorithm, inStream, outStream, key, IV)
            outStream.Position = 0

            Return outStream

        End Function

        ''' <summary>Decrypts input stream onto output stream for the given parameters.</summary>
        Public Shared Sub Decrypt(ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

            Dim inBuffer As Byte()
            Dim outBuffer As Byte()
            Dim lengthBuffer As Byte() = BitConverter.GetBytes(CType(0, Int32))
            Dim size, read As Integer
            Dim total As Long
            Dim length As Long = -1

            ' Sends initial progress event.
            If Not progressHandler Is Nothing Then
                Try
                    If inStream.CanSeek Then length = inStream.Length
                Catch
                    length = -1
                End Try

                progressHandler(0, length)
            End If

            ' When the source stream was encrypted, it was known that the encrypted stream length did not have to be same as
            ' the input stream length. We prepended the final size of the each encrypted buffer onto the destination
            ' ouput stream (now the input stream to this function), so that we could safely decrypt the stream in a
            ' "chunked" fashion, hence the following:

            ' Reads the size of the next buffer from the stream.
            read = inStream.Read(lengthBuffer, 0, lengthBuffer.Length)

            While read > 0
                ' Converts the byte array containing the buffer size into an integer.
                size = BitConverter.ToInt32(lengthBuffer, 0)

                If size > 0 Then
                    ' Creates and reads the next buffer.
                    inBuffer = CreateArray(Of Byte)(size)
                    read = inStream.Read(inBuffer, 0, size)

                    If read > 0 Then
                        ' Decrypts buffer.
                        outBuffer = Decrypt(inBuffer, key, IV, strength)
                        outStream.Write(outBuffer, 0, outBuffer.Length)

                        ' Updates decryption progress.
                        If Not progressHandler Is Nothing Then
                            total += (read + lengthBuffer.Length)
                            progressHandler(total, length)
                        End If
                    End If
                End If

                ' Reads the size of the next buffer from the stream.
                read = inStream.Read(lengthBuffer, 0, lengthBuffer.Length)
            End While

        End Sub

        ''' <summary>Decrypts input stream onto output stream for the given parameters.</summary>
        Public Shared Sub Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte())

            ' This is the root decryption function. Eventually, all the decryption functions perform their actual 
            ' decryption here.
            Dim rgbKey As Byte() = GetLegalKey(algorithm, key)
            Dim rgbIV As Byte() = GetLegalIV(algorithm, IV)
            Dim decodeStream As New CryptoStream(outStream, algorithm.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write)
            Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
            Dim read As Integer

            ' Decrypts data onto output stream.
            read = inStream.Read(buffer, 0, BufferSize)

            While read > 0
                decodeStream.Write(buffer, 0, read)
                read = inStream.Read(buffer, 0, BufferSize)
            End While

            decodeStream.FlushFinalBlock()

        End Sub

        ''' <summary>Creates a decrypted file from source file data.</summary>
        Public Shared Sub DecryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal strength As EncryptLevel)

            DecryptFile(sourceFileName, destFileName, Nothing, strength, Nothing)

        End Sub

        ''' <summary>Creates a decrypted file from source file data.</summary>
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

        ''' <summary>Coerces key to maximum legal bit length for given encryption algorithm.</summary>
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

        ''' <summary>Coerces initialization vector to legal block size for given encryption algorithm.</summary>
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

        ''' <summary>Encrypts or decrypts input stream onto output stream using XOR based algorithms. Call once to 
        ''' encrypt, call again with same key to decrypt.</summary>
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
                ' For streams that can not be positioned (i.e., cannot obtain length), we copy all
                ' data onto a memory stream and try again.
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

        ''' <summary>Encrypts or decrypts data using XOR based algorithms. Call once to encrypt; call again with same 
        ''' key to decrypt.</summary>
        Public Shared Function Crypt(ByVal data As Byte(), ByVal encryptionKey As Byte()) As Byte()

            ' The longer the encryption key, the better the encryption.
            ' Repeated encryption sequences do not occur for (3 * encryptionKey.Length) unique bytes.

            Dim cryptData As Byte() = CreateArray(Of Byte)(data.Length)
            Dim keyIndex As Long
            Dim algorithm As Integer
            Dim cryptChar As Integer

            ' Re-seeds random number generator.
            Rnd(-1)
            Randomize(encryptionKey(0))

            For x As Integer = 0 To data.Length - 1
                cryptData(x) = data(x)
                If cryptData(x) > 0 Then
                    Select Case algorithm
                        Case 0
                            cryptChar = encryptionKey(keyIndex)
                            If cryptData(x) <> cryptChar Then cryptData(x) = (cryptData(x) Xor cryptChar)
                        Case 1
                            cryptChar = Int(Rnd() * (encryptionKey(keyIndex) + 1))
                            If cryptData(x) <> cryptChar Then cryptData(x) = (cryptData(x) Xor cryptChar)
                        Case 2
                            cryptChar = Int(Rnd() * 256)
                            If cryptData(x) <> cryptChar Then cryptData(x) = (cryptData(x) Xor cryptChar)
                    End Select
                End If

                ' Selects next encryption algorithm.
                algorithm += 1
                If algorithm = 3 Then
                    algorithm = 0

                    ' Selects next encryption key.
                    keyIndex += 1
                    If keyIndex > encryptionKey.Length - 1 Then keyIndex = 0
                End If
            Next

            Return cryptData

        End Function

        ''' <summary>Obfuscates input stream onto output stream using bit-rotation algorithms.</summary>
        Public Shared Function Obfuscate(ByVal inStream As Stream, ByVal encryptionKey As Byte()) As Stream

            If inStream.CanSeek Then
                Dim outStream As New MemoryStream
                Dim buffer As Byte() = CreateArray(Of Byte)(inStream.Length)

                inStream.Read(buffer, 0, buffer.Length)
                buffer = Obfuscate(buffer, encryptionKey)
                outStream.Write(buffer, 0, buffer.Length)

                outStream.Position = 0
                Return outStream
            Else
                ' For streams that cannot be positioned (i.e., cannot obtain length), we copy all
                ' data onto a memory stream and try again.
                Dim outStream As New MemoryStream
                Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
                Dim read As Integer

                read = inStream.Read(buffer, 0, BufferSize)

                While read > 0
                    outStream.Write(buffer, 0, read)
                    read = inStream.Read(buffer, 0, BufferSize)
                End While

                outStream.Position = 0
                Return Obfuscate(outStream, encryptionKey)
            End If

        End Function

        ''' <summary>Obfuscates data using bit-rotation algorithms.</summary>
        Public Shared Function Obfuscate(ByVal data As Byte(), ByVal encryptionKey As Byte()) As Byte()

            Dim key As Byte
            Dim keyIndex As Long = encryptionKey.Length - 1
            Dim cryptData As Byte() = CreateArray(Of Byte)(data.Length)

            ' Starts bit rotation cycle.
            For x As Integer = 0 To cryptData.Length - 1
                ' Gets current key value.
                key = encryptionKey(keyIndex)

                If key Mod 2 = 0 Then
                    cryptData(x) = BitRotL(data(x), key)
                Else
                    cryptData(x) = BitRotR(data(x), key)
                End If

                ' Selects next encryption key index.
                keyIndex -= 1
                If keyIndex < 0 Then keyIndex = encryptionKey.Length - 1
            Next

            Return cryptData

        End Function

        ''' <summary>Deobfuscates input stream onto output stream using bit-rotation algorithms.</summary>
        Public Shared Function Deobfuscate(ByVal inStream As Stream, ByVal encryptionKey As Byte()) As Stream

            If inStream.CanSeek Then
                Dim outStream As New MemoryStream
                Dim buffer As Byte() = CreateArray(Of Byte)(inStream.Length)

                inStream.Read(buffer, 0, buffer.Length)
                buffer = Deobfuscate(buffer, encryptionKey)
                outStream.Write(buffer, 0, buffer.Length)

                outStream.Position = 0
                Return outStream
            Else
                ' For streams that cannot be positioned (i.e., cannot obtain length), we copy all
                ' data onto a memory stream and try again.
                Dim outStream As New MemoryStream
                Dim buffer As Byte() = CreateArray(Of Byte)(BufferSize)
                Dim read As Integer

                read = inStream.Read(buffer, 0, BufferSize)

                While read > 0
                    outStream.Write(buffer, 0, read)
                    read = inStream.Read(buffer, 0, BufferSize)
                End While

                outStream.Position = 0
                Return Deobfuscate(outStream, encryptionKey)
            End If

        End Function

        ''' <summary>Deobfuscates data using bit-rotation algorithms.</summary>
        Public Shared Function Deobfuscate(ByVal data As Byte(), ByVal encryptionKey As Byte()) As Byte()

            Dim key As Byte
            Dim keyIndex As Long = encryptionKey.Length - 1
            Dim cryptData As Byte() = CreateArray(Of Byte)(data.Length)

            ' Starts bit rotation cycle.
            For x As Integer = 0 To cryptData.Length - 1
                ' Gets current key value.
                key = encryptionKey(keyIndex)

                If key Mod 2 = 0 Then
                    cryptData(x) = BitRotR(data(x), key)
                Else
                    cryptData(x) = BitRotL(data(x), key)
                End If

                ' Selects next encryption key index.
                keyIndex -= 1
                If keyIndex < 0 Then keyIndex = encryptionKey.Length - 1
            Next

            Return cryptData

        End Function

        ''' <summary>Generates a random key useful for cryptographic functions.</summary>
        Public Shared Function GenerateKey() As String

            Dim keyChars As Char()
            Dim keyChar As Char
            Dim x, y As Integer

            ' Generates a character array of unique values.
            With New StringBuilder
                .Append(StandardKey)
                .Append(Guid.NewGuid.ToString.ToLower.Replace("-", "©ª¦"))
                .Append(System.DateTime.UtcNow.Ticks)
                For x = 1 To 50
                    .Append(Convert.ToChar(RandomInt32Between(33, 255)))
                Next
                .Append(Environment.MachineName)
                .Append(GetKeyFromSeed(Microsoft.VisualBasic.Timer))
                .Append(Environment.UserDomainName)
                .Append(Environment.UserName)
                .Append(Microsoft.VisualBasic.Timer)
                .Append(System.DateTime.Now.ToString.Replace("/", "¡¤¥").Replace(" ", "°"))
                .Append(Guid.NewGuid.ToString.ToUpper.Replace("-", "£§"))
                keyChars = .ToString().ToCharArray()
            End With

            ' Swaps values around in array at random.
            For x = 0 To keyChars.Length - 1
                y = RandomInt32Between(1, keyChars.Length) - 1
                If x <> y Then
                    keyChar = keyChars(x)
                    keyChars(x) = keyChars(y)
                    keyChars(y) = keyChar
                End If
            Next

            Return New String(keyChars)

        End Function

        ''' <summary>Returns a simple encoded string representing a number which can later be decoded
        ''' with <see cref="GetSeedFromKey" />.</summary>
        ''' <remarks>This function was designed for 24-bit values.</remarks>
        Public Shared Function GetKeyFromSeed(ByVal seed As Int24) As String

            ' This is a handy algorithm for encoding an integer value, use GetSeedFromKey to decode.
            Dim seedValue As Int32 = CType(seed, Int32)
            Dim seedBytes(3) As Byte
            Dim alphaIndex As Integer
            Dim asciiA As Integer = Asc("A"c)

            If seedValue < 0 Then Throw New ArgumentException("Cannot calculate key from negative seed")

            ' Breaks seed into its component bytes.
            seedBytes(0) = LoByte(LoWord(seedValue))
            seedBytes(1) = HiByte(LoWord(seedValue))
            seedBytes(2) = LoByte(HiWord(seedValue))

            ' Creates alpha-numeric key string.
            With New StringBuilder
                For x As Integer = 0 To 2
                    alphaIndex = RandomInt32Between(0, 25)
                    If x > 0 Then .Append("-"c)
                    .Append(Convert.ToChar(asciiA + (25 - alphaIndex)))
                    .Append(seedBytes(x) + alphaIndex)
                Next

                Return .ToString
            End With

        End Function

        ''' <summary>Returns the number from a string encoded with <see cref="GetKeyFromSeed" />.</summary>
        Public Shared Function GetSeedFromKey(ByVal key As String) As Int24

            If String.IsNullOrEmpty(key) Then Throw New ArgumentNullException("key", "key cannot be null")

            Dim seedBytes As Byte() = CreateArray(Of Byte)(3)
            Dim delimeter1 As Integer
            Dim delimeter2 As Integer
            Dim code As String = ""
            Dim value As Integer

            ' Removes all white space from specified parameter.
            key = key.Trim().ToUpper()

            If key.Length > 5 And key.Length < 15 Then
                ' Gets Delimiter positions.
                delimeter1 = InStr(key, "-")
                delimeter2 = InStr(delimeter1 + 1, key, "-", 0)

                If delimeter1 > 0 And delimeter2 > 0 Then
                    For x As Integer = 0 To 2
                        ' Extracts encoded byte.
                        Select Case x
                            Case 0
                                code = Left(key, delimeter1 - 1)
                            Case 1
                                code = Mid(key, delimeter1 + 1, delimeter2 - delimeter1 - 1)
                            Case 2
                                code = Right(key, Len(key) - delimeter2)
                        End Select

                        ' Calculates byte.
                        value = Val(Right(code, Len(code) - 1)) - (25 - (Asc(Left(code, 1)) - Asc("A")))

                        ' Validates calculation.
                        If value >= 0 And value <= 255 Then
                            seedBytes(x) = CByte(value)
                        Else
                            ' Determines the key is invalid and exits with -1.
                            Return -1
                        End If
                    Next

                    ' Creates seed from its component bytes.
                    Return MakeDWord(MakeWord(0, seedBytes(2)), MakeWord(seedBytes(1), seedBytes(0)))
                End If
            End If

            ' Determines the key is invalid and exits with -1.
            Return -1

        End Function

    End Class

End Namespace
