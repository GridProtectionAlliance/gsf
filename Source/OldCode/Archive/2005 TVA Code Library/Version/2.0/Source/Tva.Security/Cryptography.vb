'*******************************************************************************************************
'  Tva.Security.Cryptography.vb - Common Cryptographic Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2003 - James R Carroll
'       Original version of source code generated
'  01/04/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Crypto)
'
'*******************************************************************************************************
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports Tva.Bit

''' <summary>
''' <para>Common Cryptographic Functions</para>
''' </summary>
Public NotInheritable Class Cryptography

    ''' <summary>
    ''' <para>Cryptographic Strength Enumeration</para>
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Encryption algorithms are cumulative, the levels represent tradeoffs on speed vs. cipher strength - level 1
    ''' will have the fastest encryption speed with the simplest encryption strength - level 4 will have the
    ''' strongest cumulative encryption strength with the slowest encryption speed.
    ''' </para>
    ''' </remarks>
    Public Enum EncryptLevel
        ''' <summary>Adds simple multi-alogorithm XOR based encryption</summary>
        [Level1]
        ''' <summary>Adds TripleDES based encryption</summary>
        [Level2]
        ''' <summary>Adds RC2 based encryption</summary>
        [Level3]
        ''' <summary>Adds RijndaelManaged based enryption</summary>
        [Level4]
    End Enum

    Public Delegate Sub ProgressEventHandler(ByVal bytesCompleted As Long, ByVal bytesTotal As Long)

    ' IMPORTANT! Never change the following constants or you will break cross-application crypto operability:
    Private Const StandardKey As String = "{&-<%=($#/T.V:A!\,@[20O3]*^_j`|?)>+~}"
    Private Const BufferSize As Integer = 262144    ' 256K

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>
    ''' <para>Returns a Base64 encoded string of the returned binary array of the encrypted data generated with the given parameter using the standard encryption key and encryption level 1</para>
    ''' </summary>
    Public Shared Function Encrypt(ByVal str As String) As String

        Return Encrypt(str, Nothing, EncryptLevel.Level1)

    End Function

    ''' <summary>
    ''' <para>Returns a Base64 encoded string of the returned binary array of the encrypted data generated with the given parameters using standard encryption</para>
    ''' </summary>
    Public Shared Function Encrypt(ByVal str As String, ByVal strength As EncryptLevel) As String

        Return Encrypt(str, Nothing, strength)

    End Function

    ''' <summary>
    ''' <para>Returns a Base64 encoded string of the returned binary array of the encrypted data generated with the given parameters</para>
    ''' </summary>
    Public Shared Function Encrypt(ByVal str As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel) As String

        If encryptionKey Is Nothing OrElse encryptionKey.Length = 0 Then encryptionKey = StandardKey

        Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
        Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

        Return Base64Encode(Encrypt(Encoding.Unicode.GetBytes(str), rgbKey, rgbIV, strength))

    End Function

    ''' <summary>
    ''' <para>Returns a binary array of encrypted data for the given parameters</para>
    ''' </summary>
    Public Shared Function Encrypt(ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Byte()

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

    ''' <summary>
    ''' <para>Returns a binary array of encrypted data for the given parameters</para>
    ''' </summary>
    Public Shared Function Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte()) As Byte()

        Return DirectCast(Encrypt(algorithm, New MemoryStream(data), key, IV), MemoryStream).ToArray()

    End Function

    ''' <summary>
    ''' <para>Returns a stream of encrypted data for the given parameters</para>
    ''' </summary>
    Public Shared Function Encrypt(ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Stream

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

    ''' <summary>
    ''' <para>Returns a stream of encrypted data for the given parameters</para>
    ''' </summary>
    Public Shared Function Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte()) As Stream

        Dim outStream As New MemoryStream

        Encrypt(algorithm, inStream, outStream, key, IV)
        outStream.Position = 0

        Return outStream

    End Function

    ''' <summary>
    ''' <para>Encrypts input stream onto output stream for the given parameters</para>
    ''' </summary>
    Public Shared Sub Encrypt(ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

        Dim inBuffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
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
            outBuffer = Encrypt(TruncateBuffer(inBuffer, read), key, IV, strength)

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

    ''' <summary>
    ''' <para>Encrypts input stream onto output stream for the given parameters</para>
    ''' </summary>
    Public Shared Sub Encrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte())

        ' This is the root encryption function - eventually, all the encryption functions perform their actual encryption here...
        Dim rgbKey As Byte() = GetLegalKey(algorithm, key)
        Dim rgbIV As Byte() = GetLegalIV(algorithm, IV)
        Dim encodeStream As New CryptoStream(outStream, algorithm.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write)
        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
        Dim read As Integer

        ' Encrypt data onto output stream
        read = inStream.Read(buffer, 0, BufferSize)

        While read > 0
            encodeStream.Write(buffer, 0, read)
            read = inStream.Read(buffer, 0, BufferSize)
        End While

        encodeStream.FlushFinalBlock()

    End Sub

    ''' <summary>
    ''' <para>Creates an encrypted file from source file data</para>
    ''' </summary>
    Public Shared Sub EncryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal strength As EncryptLevel)

        EncryptFile(sourceFileName, destFileName, Nothing, strength, Nothing)

    End Sub

    ''' <summary>
    ''' <para>Creates an encrypted file from source file data</para>
    ''' </summary>
    Public Shared Sub EncryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

        If encryptionKey Is Nothing OrElse encryptionKey.Length = 0 Then encryptionKey = StandardKey

        Dim sourceFileStream As FileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
        Dim destFileStream As FileStream = File.Create(destFileName)
        Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
        Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

        Encrypt(sourceFileStream, destFileStream, rgbKey, rgbIV, strength, progressHandler)

        destFileStream.Flush()
        destFileStream.Close()
        sourceFileStream.Close()

    End Sub

    ''' <summary>
    ''' <para>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given parameter using the standard encryption key and encryption level 1</para>
    ''' </summary>
    Public Shared Function Decrypt(ByVal str As String) As String

        Return Decrypt(str, Nothing, EncryptLevel.Level1)

    End Function

    ''' <summary>
    ''' <para>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given parameters using the standard encryption key</para>
    ''' </summary>
    Public Shared Function Decrypt(ByVal str As String, ByVal strength As EncryptLevel) As String

        Return Decrypt(str, Nothing, strength)

    End Function

    ''' <summary>
    ''' <para>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given parameters</para>
    ''' </summary>
    Public Shared Function Decrypt(ByVal str As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel) As String

        If encryptionKey Is Nothing OrElse encryptionKey.Length = 0 Then encryptionKey = StandardKey

        Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
        Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

        Return Encoding.Unicode.GetString(Decrypt(Base64Decode(str), rgbKey, rgbIV, strength))

    End Function

    ''' <summary>
    ''' <para>Returns a binary array of decrypted data for the given parameters</para>
    ''' </summary>
    Public Shared Function Decrypt(ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Byte()

        ' Perform requested levels of decryption
        If strength >= EncryptLevel.Level4 Then data = Decrypt(New RijndaelManaged, data, key, IV)
        If strength >= EncryptLevel.Level3 Then data = Decrypt(New RC2CryptoServiceProvider, data, key, IV)
        If strength >= EncryptLevel.Level2 Then data = Decrypt(New TripleDESCryptoServiceProvider, data, key, IV)

        Return Crypt(data, key)

    End Function

    ''' <summary>
    ''' <para>Returns a binary array of decrypted data for the given parameters</para>
    ''' </summary>
    Public Shared Function Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal data As Byte(), ByVal key As Byte(), ByVal IV As Byte()) As Byte()

        Return DirectCast(Decrypt(algorithm, New MemoryStream(data), key, IV), MemoryStream).ToArray()

    End Function

    ''' <summary>
    ''' <para>Returns a stream of decrypted data for the given parameters</para>
    ''' </summary>
    Public Shared Function Decrypt(ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte(), ByVal strength As EncryptLevel) As Stream

        If strength >= EncryptLevel.Level4 Then inStream = Decrypt(New RijndaelManaged, inStream, key, IV)
        If strength >= EncryptLevel.Level3 Then inStream = Decrypt(New RC2CryptoServiceProvider, inStream, key, IV)
        If strength >= EncryptLevel.Level2 Then inStream = Decrypt(New TripleDESCryptoServiceProvider, inStream, key, IV)

        Return Crypt(inStream, key)

    End Function

    ''' <summary>
    ''' <para>Returns a stream of decrypted data for the given parameters</para>
    ''' </summary>
    Public Shared Function Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal key As Byte(), ByVal IV As Byte()) As Stream

        Dim outStream As New MemoryStream

        Decrypt(algorithm, inStream, outStream, key, IV)
        outStream.Position = 0

        Return outStream

    End Function

    ''' <summary>
    ''' <para>Decrypts input stream onto output stream for the given parameters</para>
    ''' </summary>
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
                inBuffer = Array.CreateInstance(GetType(Byte), size)
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

    ''' <summary>
    ''' <para>Decrypts input stream onto output stream for the given parameters</para>
    ''' </summary>
    Public Shared Sub Decrypt(ByVal algorithm As SymmetricAlgorithm, ByVal inStream As Stream, ByVal outStream As Stream, ByVal key As Byte(), ByVal IV As Byte())

        ' This is the root decryption function - eventually, all the decryption functions perform their actual decryption here...
        Dim rgbKey As Byte() = GetLegalKey(algorithm, key)
        Dim rgbIV As Byte() = GetLegalIV(algorithm, IV)
        Dim decodeStream As New CryptoStream(outStream, algorithm.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write)
        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
        Dim read As Integer

        ' Decrypt data onto output stream
        read = inStream.Read(buffer, 0, BufferSize)

        While read > 0
            decodeStream.Write(buffer, 0, read)
            read = inStream.Read(buffer, 0, BufferSize)
        End While

        decodeStream.FlushFinalBlock()

    End Sub

    ''' <summary>
    ''' <para>Creates a decrypted file from source file data</para>
    ''' </summary>
    Public Shared Sub DecryptFile(ByVal sourceFileName As String, ByVal destFileName As String, ByVal encryptionKey As String, ByVal strength As EncryptLevel, ByVal progressHandler As ProgressEventHandler)

        If encryptionKey Is Nothing OrElse encryptionKey.Length = 0 Then encryptionKey = StandardKey

        Dim sourceFileStream As FileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
        Dim destFileStream As FileStream = File.Create(destFileName)
        Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(encryptionKey)
        Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(encryptionKey)

        Decrypt(sourceFileStream, destFileStream, rgbKey, rgbIV, strength, progressHandler)

        destFileStream.Flush()
        destFileStream.Close()
        sourceFileStream.Close()

    End Sub

    ''' <summary>
    ''' <para>Coerces key to maximum legal bit length for given encryption algorithm</para>
    ''' </summary>
    Public Shared Function GetLegalKey(ByVal algorithm As SymmetricAlgorithm, ByVal key() As Byte) As Byte()

        Dim rgbKey As Byte() = Array.CreateInstance(GetType(Byte), Convert.ToInt32(algorithm.LegalKeySizes(0).MaxSize / 8 - 1))

        For x As Integer = 0 To rgbKey.Length - 1
            If x < key.Length Then
                rgbKey(x) = key(x)
            Else
                rgbKey(x) = Encoding.ASCII.GetBytes(StandardKey.Substring(x Mod StandardKey.Length, 1))(0)
            End If
        Next

        Return rgbKey

    End Function

    ''' <summary>
    ''' <para>Coerces initialization vector to legal block size for given encryption algorithm</para>
    ''' </summary>
    Public Shared Function GetLegalIV(ByVal algorithm As SymmetricAlgorithm, ByVal IV() As Byte) As Byte()

        Dim rgbIV As Byte() = Array.CreateInstance(GetType(Byte), Convert.ToInt32(algorithm.LegalBlockSizes(0).MinSize / 8 - 1))

        For x As Integer = 0 To rgbIV.Length - 1
            If x < IV.Length Then
                rgbIV(x) = IV(IV.Length - 1 - x)
            Else
                rgbIV(x) = Encoding.ASCII.GetBytes(StandardKey.Substring(x Mod StandardKey.Length, 1))(0)
            End If
        Next

        Return rgbIV

    End Function

    ''' <summary>
    ''' <para>Encrypts or decrypts input stream onto output stream using XOR based algorithms.  Call once to encrypt, call again with same key to decrypt.</para>
    ''' </summary>
    Public Shared Function Crypt(ByVal inStream As Stream, ByVal encryptionKey As Byte()) As Stream

        If inStream.CanSeek Then
            Dim outStream As New MemoryStream
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), inStream.Length)

            inStream.Read(buffer, 0, buffer.Length)
            buffer = Crypt(buffer, encryptionKey)
            outStream.Write(buffer, 0, buffer.Length)

            outStream.Position = 0
            Return outStream
        Else
            ' For streams that can't be positioned, we copy all data onto a memory stream and try again
            Dim outStream As New MemoryStream
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
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

    ''' <summary>
    ''' <para>Encrypts or decrypts a string using XOR based algorithms.  Call once to encrypt, call again with same key to decrypt.</para>
    ''' </summary>
    Public Shared Function Crypt(ByVal str As String, ByVal encryptionKey As String) As String

        Return Encoding.Unicode.GetString(Crypt(Encoding.Unicode.GetBytes(str), Encoding.Unicode.GetBytes(encryptionKey)))

    End Function

    ''' <summary>
    ''' <para>Encrypts or decrypts data using XOR based algorithms.  Call once to encrypt, call again with same key to decrypt.</para>
    ''' </summary>
    Public Shared Function Crypt(ByVal data As Byte(), ByVal encryptionKey As Byte()) As Byte()

        ' The longer the encryption key the better the encryption
        ' Repeated encryption sequences do not occur for (3 * encryptionKey.Length) unique bytes

        Dim cryptData As Byte() = Array.CreateInstance(GetType(Byte), data.Length)
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
            algorithm = algorithm + 1
            If algorithm = 3 Then
                algorithm = 0

                ' Select next encryption key
                keyPos = keyPos + 1
                If keyPos > encryptionKey.Length - 1 Then keyPos = 0
            End If
        Next

        Return cryptData

    End Function

    ''' <summary>
    ''' <para>Generates a random key useful for cryptographic functions</para>
    ''' </summary>
    Public Shared Function GenerateKey() As String

        Dim key As String
        Dim keyChar As String
        Dim keyLen As Integer
        Dim y As Integer

        With RNGCryptoServiceProvider.Create()
            .
        End With

        key = StandardKey & Replace(Replace(Replace( _
            Guid.NewGuid.ToString() & _
            System.DateTime.UtcNow.Ticks.ToString & _
            System.Environment.MachineName & _
            System.Environment.UserDomainName & _
            System.Environment.UserName & _
            Microsoft.VisualBasic.Timer.ToString & _
            System.DateTime.Now.ToString & _
            Guid.NewGuid.ToString(), _
            " ", "©"), "-", "~"), "/", "%")

        keyLen = key.Length

        For x As Integer = 1 To keyLen
            y = CInt(Int(Rnd() * keyLen) + 1)
            If x <> y Then
                keyChar = Mid(key, x, 1)
                Mid(key, x, 1) = Mid(key, y, 1)
                Mid(key, y, 1) = keyChar
            End If
        Next

        Return key

    End Function
    ''' <summary>
    ''' <para>Returns a coded string representing a number which can later be decoded with GetSeedFromKey() - function designed for system Timer values.</para>
    ''' </summary>
    ' Returns a coded string representing a number which can later be decoded with GetSeedFromKey() - function designed for system Timer values
    Public Shared Function GetKeyFromSeed(ByVal Seed As Integer) As String

        ' This is a handy algorithm for encoding a secret number
        ' Use GetSeedFromKey to decode

        Dim intAlpha As Short
        Dim bytSeed(3) As Byte
        Dim strKey As String
        Dim x As Short

        If Seed < 0 Then
            Throw New InvalidOperationException("Cannot calculate key from negative seed")
        End If

        If LoByte(LoWord(Seed)) > 0 Then
            Throw New InvalidOperationException("Seed is too large (function was designed for timer values)")
        End If

        ' Break seed into its component bytes
        bytSeed(0) = HiByte(HiWord(Seed))
        bytSeed(1) = LoByte(HiWord(Seed))
        bytSeed(2) = HiByte(LoWord(Seed))

        ' Create alpha-numeric key string
        Randomize()
        strKey = ""

        For x = 0 To 2
            intAlpha = Int(Rnd() * 26)
            strKey = strKey & IIf(x > 0, "-", "") & Chr(Asc("A") + (25 - intAlpha)) & (bytSeed(x) + intAlpha)
        Next

        Return strKey

    End Function
    ''' <summary>
    ''' <para>Returns the number from a string coded with GetKeyFromSeed().</para>
    ''' </summary>
    ' Returns the number from a string coded with GetKeyFromSeed()
    Public Shared Function GetSeedFromKey(ByVal Key As String) As Integer

        Dim bytSeed(3) As Byte
        Dim intPos1 As Integer
        Dim intPos2 As Integer
        Dim strCode As String
        Dim intCalc As Short
        Dim x As Integer

        ' Remove all white space from specified parameter
        Key = UCase(RemoveWhiteSpace(Key))

        If Len(Key) > 5 And Len(Key) < 15 Then
            ' Get Delimiter positions
            intPos1 = InStr(Key, "-")
            intPos2 = InStr(intPos1 + 1, Key, "-", 0)

            If intPos1 > 0 And intPos2 > 0 Then
                For x = 0 To 2
                    ' Extract encoded byte
                    Select Case x
                        Case 0
                            strCode = Left(Key, intPos1 - 1)
                        Case 1
                            strCode = Mid(Key, intPos1 + 1, intPos2 - intPos1 - 1)
                        Case 2
                            strCode = Right(Key, Len(Key) - intPos2)
                    End Select

                    ' Calculate byte
                    intCalc = Val(Right(strCode, Len(strCode) - 1)) - (25 - (Asc(Left(strCode, 1)) - Asc("A")))

                    ' Validate calculation
                    If intCalc >= 0 And intCalc <= 255 Then
                        bytSeed(x) = CByte(intCalc)
                    Else
                        ' Invalid key, exit with -1
                        Return -1
                    End If
                Next

                ' Create seed from its component bytes
                Return MakeDWord(MakeWord(bytSeed(0), bytSeed(1)), MakeWord(bytSeed(2), 0))
            End If
        End If

        ' Invalid key, exit with -1
        Return -1

    End Function

End Class
