' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports TVA.Shared.Common
Imports TVA.Shared.String
Imports TVA.Shared.Bit

Namespace [Shared]
    ''' <summary>
    ''' <para>Common Crypto Functions</para>
    ''' </summary>

    Public Class Crypto

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Encryption algorithms are cumulative, the levels represent tradeoffs on speed vs. cipher strength - level 1
        ' will have the fastest encryption speed with the simplest encryption strength - level 4 will have the
        ' strongest cumulative encryption strength with the slowest encryption speed
        ' See ms-help://MS.VSCC.2003/MS.MSDNQTR.2003JUL.1033/cpguide/html/cpconcryptographyoverview.htm
        ' for information on the symmetric encryption algorithms
        Public Enum EncryptLevel
            [Level1]    ' Adds simple multi-alogorithm XOR based encryption
            [Level2]    ' Adds TripleDES based encryption
            [Level3]    ' Adds RC2 based encryption
            [Level4]    ' Adds RijndaelManaged based enryption
        End Enum

        Public Delegate Sub ProgressEventHandler(ByVal BytesCompleted As Long, ByVal BytesTotal As Long)

        ' IMPORTANT! Never change the following constants or you will break cross-application crypto operability:
        Private Const StandardKey As String = "{&-<%=($#/T.V:A!\,@[20O3]*^_j`|?)>+~}"
        Private Const BufferSize As Integer = 262144    ' 256K
        ''' <summary>
        ''' <para> Returns a Base64 encoded string of the returned binary array of the encrypted data generated with the given parameters</para>
        ''' </summary>
        '''  <param name="FileName"> Required. Any file. </param>
        ' Returns a Base64 encoded string of the returned binary array of the encrypted data generated with the given parameters
        Public Shared Function Encrypt(ByVal Str As System.String, Optional ByVal EncryptionKey As System.String = "", Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1) As String

            If Len(EncryptionKey) = 0 Then EncryptionKey = StandardKey

            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(EncryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(EncryptionKey)

            Return Base64Encode(Encrypt(Encoding.Unicode.GetBytes(Str), rgbKey, rgbIV, Strength))

        End Function
        ''' <summary>
        ''' <para> Returns a binary array of encrypted data for the given parameters</para>
        ''' </summary>
        ' Returns a binary array of encrypted data for the given parameters
        Public Shared Function Encrypt(ByVal Data As Byte(), ByVal Key As Byte(), ByVal IV As Byte(), Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1) As Byte()

            ' Perform requested levels of encryption
            Data = Crypt(Data, Key)
            If Strength >= EncryptLevel.Level2 Then
                Data = Encrypt(New TripleDESCryptoServiceProvider, Data, Key, IV)
                If Strength >= EncryptLevel.Level3 Then
                    Data = Encrypt(New RC2CryptoServiceProvider, Data, Key, IV)
                    If Strength >= EncryptLevel.Level4 Then
                        Data = Encrypt(New RijndaelManaged, Data, Key, IV)
                    End If
                End If
            End If

            Return Data

        End Function
        ''' <summary>
        ''' <para> Returns a binary array of encrypted data for the given parameters</para>
        ''' </summary>
        ' Returns a binary array of encrypted data for the given parameters
        Public Shared Function Encrypt(ByVal Algorithm As SymmetricAlgorithm, ByVal Data As Byte(), ByVal Key As Byte(), ByVal IV As Byte()) As Byte()

            Return DirectCast(Encrypt(Algorithm, New MemoryStream(Data), Key, IV), MemoryStream).ToArray()

        End Function
        ''' <summary>
        ''' <para> Returns a stream of encrypted data for the given parameters</para>
        ''' </summary>
        ' Returns a stream of encrypted data for the given parameters
        Public Shared Function Encrypt(ByVal InStream As Stream, ByVal Key As Byte(), ByVal IV As Byte(), Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1) As Stream

            ' Perform requested levels of encryption
            InStream = Crypt(InStream, Key)
            If Strength >= EncryptLevel.Level2 Then
                InStream = Encrypt(New TripleDESCryptoServiceProvider, InStream, Key, IV)
                If Strength >= EncryptLevel.Level3 Then
                    InStream = Encrypt(New RC2CryptoServiceProvider, InStream, Key, IV)
                    If Strength >= EncryptLevel.Level4 Then
                        InStream = Encrypt(New RijndaelManaged, InStream, Key, IV)
                    End If
                End If
            End If

            Return InStream

        End Function
        ''' <summary>
        ''' <para>Returns a stream of encrypted data for the given parameters</para>
        ''' </summary>
        ' Returns a stream of encrypted data for the given parameters
        Public Shared Function Encrypt(ByVal Algorithm As SymmetricAlgorithm, ByVal InStream As Stream, ByVal Key As Byte(), ByVal IV As Byte()) As Stream

            Dim outStream As New MemoryStream

            Encrypt(Algorithm, InStream, outStream, Key, IV)
            outStream.Position = 0

            Return outStream

        End Function
        ''' <summary>
        ''' <para>Encrypts input stream onto output stream for the given parameters</para>
        ''' </summary>
        ' Encrypts input stream onto output stream for the given parameters
        Public Shared Sub Encrypt(ByVal InStream As Stream, ByVal OutStream As Stream, ByVal Key As Byte(), ByVal IV As Byte(), Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1, Optional ByVal ProgressHandler As ProgressEventHandler = Nothing)

            Dim inBuffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim outBuffer As Byte()
            Dim bufferLen As Byte()
            Dim intRead As Integer
            Dim lngTotal As Long
            Dim lngLen As Long = -1

            ' Send initial progress event
            If Not ProgressHandler Is Nothing Then
                Try
                    If InStream.CanSeek Then lngLen = InStream.Length
                Catch
                    lngLen = -1
                End Try

                ProgressHandler(0, lngLen)
            End If

            ' Read initial buffer
            intRead = InStream.Read(inBuffer, 0, BufferSize)

            While intRead > 0
                ' Encrypt buffer
                outBuffer = Encrypt(TruncateBuffer(inBuffer, intRead), Key, IV, Strength)

                ' The destination encryption stream length doesn't have to be same as the input stream length, so we
                ' prepend the final size of each encrypted buffer onto the destination ouput stream so that we can
                ' safely decrypt the stream in a "chunked" fashion later...
                bufferLen = BitConverter.GetBytes(CType(outBuffer.Length, Int32))
                OutStream.Write(bufferLen, 0, bufferLen.Length)
                OutStream.Write(outBuffer, 0, outBuffer.Length)

                ' Update encryption progress
                If Not ProgressHandler Is Nothing Then
                    lngTotal += intRead
                    ProgressHandler(lngTotal, lngLen)
                End If

                ' Read next buffer
                intRead = InStream.Read(inBuffer, 0, BufferSize)
            End While

        End Sub
        ''' <summary>
        ''' <para>Encrypts input stream onto output stream for the given parameters</para>
        ''' </summary>
        ' Encrypts input stream onto output stream for the given parameters
        Public Shared Sub Encrypt(ByVal Algorithm As SymmetricAlgorithm, ByVal InStream As Stream, ByVal OutStream As Stream, ByVal Key As Byte(), ByVal IV As Byte())

            ' This is the root encryption function - eventually, all the encryption functions perform their actual encryption here...
            Dim rgbKey As Byte() = GetLegalKey(Algorithm, Key)
            Dim rgbIV As Byte() = GetLegalIV(Algorithm, IV)
            Dim encStream As New CryptoStream(OutStream, Algorithm.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write)
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim intRead As Integer

            ' Encrypt data onto output stream
            intRead = InStream.Read(buffer, 0, BufferSize)

            While intRead > 0
                encStream.Write(buffer, 0, intRead)
                intRead = InStream.Read(buffer, 0, BufferSize)
            End While

            encStream.FlushFinalBlock()

        End Sub
        ''' <summary>
        ''' <para>Creates an encrypted file from source file data</para>
        ''' </summary>
        ' Creates an encrypted file from source file data
        Public Shared Sub EncryptFile(ByVal SourceFileName As System.String, ByVal DestFileName As System.String, Optional ByVal EncryptionKey As System.String = "", Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1, Optional ByVal ProgressHandler As ProgressEventHandler = Nothing)

            If Len(EncryptionKey) = 0 Then EncryptionKey = StandardKey

            Dim sourceFileStream As FileStream = File.Open(SourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim destFileStream As FileStream = File.Create(DestFileName)
            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(EncryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(EncryptionKey)

            Encrypt(sourceFileStream, destFileStream, rgbKey, rgbIV, Strength, ProgressHandler)

            destFileStream.Flush()
            destFileStream.Close()
            sourceFileStream.Close()

        End Sub
        ''' <summary>
        ''' <para>Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given parameters</para>
        ''' </summary>

        ' Returns a decrypted string from a Base64 encoded string of binary encrypted data from the given parameters
        Public Shared Function Decrypt(ByVal Str As System.String, Optional ByVal EncryptionKey As System.String = "", Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1) As String

            If Len(EncryptionKey) = 0 Then EncryptionKey = StandardKey

            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(EncryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(EncryptionKey)

            Return Encoding.Unicode.GetString(Decrypt(Base64Decode(Str), rgbKey, rgbIV, Strength))

        End Function
        ''' <summary>
        ''' <para>Returns a binary array of decrypted data for the given parameters</para>
        ''' </summary>
        ' Returns a binary array of decrypted data for the given parameters
        Public Shared Function Decrypt(ByVal Data As Byte(), ByVal Key As Byte(), ByVal IV As Byte(), Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1) As Byte()

            ' Perform requested levels of decryption
            If Strength >= EncryptLevel.Level4 Then Data = Decrypt(New RijndaelManaged, Data, Key, IV)
            If Strength >= EncryptLevel.Level3 Then Data = Decrypt(New RC2CryptoServiceProvider, Data, Key, IV)
            If Strength >= EncryptLevel.Level2 Then Data = Decrypt(New TripleDESCryptoServiceProvider, Data, Key, IV)

            Return Crypt(Data, Key)

        End Function
        ''' <summary>
        ''' <para>Returns a binary array of decrypted data for the given parameters</para>
        ''' </summary>
        ' Returns a binary array of decrypted data for the given parameters
        Public Shared Function Decrypt(ByVal Algorithm As SymmetricAlgorithm, ByVal Data As Byte(), ByVal Key As Byte(), ByVal IV As Byte()) As Byte()

            Return DirectCast(Decrypt(Algorithm, New MemoryStream(Data), Key, IV), MemoryStream).ToArray()

        End Function
        ''' <summary>
        ''' <para>Returns a stream of decrypted data for the given parameters</para>
        ''' </summary>
        ' Returns a stream of decrypted data for the given parameters
        Public Shared Function Decrypt(ByVal InStream As Stream, ByVal Key As Byte(), ByVal IV As Byte(), Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1) As Stream

            If Strength >= EncryptLevel.Level4 Then InStream = Decrypt(New RijndaelManaged, InStream, Key, IV)
            If Strength >= EncryptLevel.Level3 Then InStream = Decrypt(New RC2CryptoServiceProvider, InStream, Key, IV)
            If Strength >= EncryptLevel.Level2 Then InStream = Decrypt(New TripleDESCryptoServiceProvider, InStream, Key, IV)

            Return Crypt(InStream, Key)

        End Function
        ''' <summary>
        ''' <para> Returns a stream of decrypted data for the given parameters</para>
        ''' </summary>
        ' Returns a stream of decrypted data for the given parameters
        Public Shared Function Decrypt(ByVal Algorithm As SymmetricAlgorithm, ByVal InStream As Stream, ByVal Key As Byte(), ByVal IV As Byte()) As Stream

            Dim outStream As New MemoryStream

            Decrypt(Algorithm, InStream, outStream, Key, IV)
            outStream.Position = 0

            Return outStream

        End Function
        ''' <summary>
        ''' <para> Decrypts input stream onto output stream for the given parameters</para>
        ''' </summary>
        ' Decrypts input stream onto output stream for the given parameters
        Public Shared Sub Decrypt(ByVal InStream As Stream, ByVal OutStream As Stream, ByVal Key As Byte(), ByVal IV As Byte(), Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1, Optional ByVal ProgressHandler As ProgressEventHandler = Nothing)

            Dim inBuffer As Byte()
            Dim outBuffer As Byte()
            Dim bufferLen As Byte() = BitConverter.GetBytes(CType(0, Int32))
            Dim intSize, intRead As Integer
            Dim lngTotal As Long
            Dim lngLen As Long = -1

            ' Send initial progress event
            If Not ProgressHandler Is Nothing Then
                Try
                    If InStream.CanSeek Then lngLen = InStream.Length
                Catch
                    lngLen = -1
                End Try

                ProgressHandler(0, lngLen)
            End If

            ' When the source stream was encrypted, it was known that the encrypted stream length didn't have to be same as
            ' the input stream length, so we prepended the final size of the each encrypted buffer onto the destination
            ' ouput stream (now the input stream to this function) so that we could safely decrypt the stream in a
            ' "chunked" fashion, hence the following:

            ' Read the size of the next buffer from the stream
            intRead = InStream.Read(bufferLen, 0, bufferLen.Length)

            While intRead > 0
                ' Convert the byte array containing the buffer size into an integer
                intSize = BitConverter.ToInt32(bufferLen, 0)

                If intSize > 0 Then
                    ' Create and read the next buffer
                    inBuffer = Array.CreateInstance(GetType(Byte), intSize)
                    intRead = InStream.Read(inBuffer, 0, intSize)

                    If intRead > 0 Then
                        ' Decrypt buffer
                        outBuffer = Decrypt(inBuffer, Key, IV, Strength)
                        OutStream.Write(outBuffer, 0, outBuffer.Length)

                        ' Update decryption progress
                        If Not ProgressHandler Is Nothing Then
                            lngTotal += (intRead + bufferLen.Length)
                            ProgressHandler(lngTotal, lngLen)
                        End If
                    End If
                End If

                ' Read the size of the next buffer from the stream
                intRead = InStream.Read(bufferLen, 0, bufferLen.Length)
            End While

        End Sub
        ''' <summary>
        ''' <para> Decrypts input stream onto output stream for the given parameters</para>
        ''' </summary>
        ' Decrypts input stream onto output stream for the given parameters
        Public Shared Sub Decrypt(ByVal Algorithm As SymmetricAlgorithm, ByVal InStream As Stream, ByVal OutStream As Stream, ByVal Key As Byte(), ByVal IV As Byte())

            ' This is the root decryption function - eventually, all the decryption functions perform their actual decryption here...
            Dim rgbKey As Byte() = GetLegalKey(Algorithm, Key)
            Dim rgbIV As Byte() = GetLegalIV(Algorithm, IV)
            Dim decStream As New CryptoStream(OutStream, Algorithm.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write)
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim intRead As Integer

            ' Decrypt data onto output stream
            intRead = InStream.Read(buffer, 0, BufferSize)

            While intRead > 0
                decStream.Write(buffer, 0, intRead)
                intRead = InStream.Read(buffer, 0, BufferSize)
            End While

            decStream.FlushFinalBlock()

        End Sub
        ''' <summary>
        ''' <para> Creates a decrypted file from source file data</para>
        ''' </summary>
        ' Creates a decrypted file from source file data
        Public Shared Sub DecryptFile(ByVal SourceFileName As System.String, ByVal DestFileName As System.String, Optional ByVal EncryptionKey As System.String = "", Optional ByVal Strength As EncryptLevel = EncryptLevel.Level1, Optional ByVal ProgressHandler As ProgressEventHandler = Nothing)

            If Len(EncryptionKey) = 0 Then EncryptionKey = StandardKey

            Dim sourceFileStream As FileStream = File.Open(SourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim destFileStream As FileStream = File.Create(DestFileName)
            Dim rgbKey As Byte() = Encoding.ASCII.GetBytes(EncryptionKey)
            Dim rgbIV As Byte() = Encoding.ASCII.GetBytes(EncryptionKey)

            Decrypt(sourceFileStream, destFileStream, rgbKey, rgbIV, Strength, ProgressHandler)

            destFileStream.Flush()
            destFileStream.Close()
            sourceFileStream.Close()

        End Sub

        Public Shared Function GetLegalKey(ByVal Algorithm As SymmetricAlgorithm, ByVal Key() As Byte) As Byte()

            Dim rgbKey As Byte()
            Dim x As Integer

            ' Coerce key to maximum legal bit length for given encryption algorithm
            ReDim rgbKey(Algorithm.LegalKeySizes(0).MaxSize / 8 - 1)

            For x = 0 To rgbKey.Length - 1
                If x < Key.Length Then
                    rgbKey(x) = Key(x)
                Else
                    rgbKey(x) = Asc(Mid(StandardKey, x Mod Len(StandardKey) + 1, 1))
                End If
            Next

            Return rgbKey

        End Function

        Public Shared Function GetLegalIV(ByVal Algorithm As SymmetricAlgorithm, ByVal IV() As Byte) As Byte()

            Dim rgbIV As Byte()
            Dim x As Integer

            ' Coerce initialization vector to legal block size for given encryption algorithm
            ReDim rgbIV(Algorithm.LegalBlockSizes(0).MinSize / 8 - 1)

            For x = 0 To rgbIV.Length - 1
                If x < IV.Length Then
                    rgbIV(x) = IV(IV.Length - 1 - x)
                Else
                    rgbIV(x) = Asc(Mid(StandardKey, x Mod Len(StandardKey) + 1, 1))
                End If
            Next

            Return rgbIV

        End Function
        ''' <summary>
        ''' <para> Encrypts/decrypts input stream onto output stream, call once to encrypt, call again to decrypt</para>
        ''' </summary>
        ' Encrypts/decrypts input stream onto output stream, call once to encrypt, call again to decrypt
        Public Shared Function Crypt(ByVal InStream As Stream, ByVal EncryptionKey As Byte()) As Stream

            If InStream.CanSeek Then
                Dim outStream As New MemoryStream
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), InStream.Length)

                InStream.Read(buffer, 0, buffer.Length)
                buffer = Crypt(buffer, EncryptionKey)
                outStream.Write(buffer, 0, buffer.Length)

                outStream.Position = 0
                Return outStream
            Else
                Dim outStream As New MemoryStream
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
                Dim intRead As Integer

                intRead = InStream.Read(buffer, 0, BufferSize)

                While intRead > 0
                    outStream.Write(buffer, 0, intRead)
                    intRead = InStream.Read(buffer, 0, BufferSize)
                End While

                outStream.Position = 0
                Return Crypt(outStream, EncryptionKey)
            End If

        End Function
        ''' <summary>
        ''' <para> Encrypts/decrypts a string using XOR based algorithms, call once to encrypt, call again with same key to decrypt.</para>
        ''' </summary>
        ' Encrypts/decrypts a string using XOR based algorithms, call once to encrypt, call again with same key to decrypt
        Public Shared Function Crypt(ByVal Str As System.String, ByVal EncryptionKey As System.String) As String

            Return Encoding.Unicode.GetString(Crypt(Encoding.Unicode.GetBytes(Str), Encoding.Unicode.GetBytes(EncryptionKey)))

        End Function
        ''' <summary>
        ''' <para> Encrypts/decrypts data, call once to encrypt, call again with same key to decrypt.</para>
        ''' </summary>
        ' Encrypts/decrypts data, call once to encrypt, call again with same key to decrypt
        Public Shared Function Crypt(ByVal Data As Byte(), ByVal EncryptionKey As Byte()) As Byte()

            ' The longer the encryption key the better the encryption
            ' Repeated encryption sequences do not occur for (3 * EncryptionKey.Length) unique bytes

            Dim cryptData As Byte() = Array.CreateInstance(GetType(Byte), Data.Length)
            Dim lngKeyPos As Long
            Dim intAlgorithm As Integer
            Dim intChar As Integer
            Dim x As Long

            ' Re-seed random number generator
            Rnd(-1)
            Randomize(EncryptionKey(0))

            lngKeyPos = 0
            intAlgorithm = 0

            For x = 0 To Data.Length - 1
                cryptData(x) = Data(x)
                If cryptData(x) > 0 Then
                    Select Case intAlgorithm
                        Case 0
                            intChar = EncryptionKey(lngKeyPos)
                            If cryptData(x) <> intChar Then cryptData(x) = (cryptData(x) Xor intChar)
                        Case 1
                            intChar = Int(Rnd() * (EncryptionKey(lngKeyPos) + 1))
                            If cryptData(x) <> intChar Then cryptData(x) = (cryptData(x) Xor intChar)
                        Case 2
                            intChar = Int(Rnd() * 256)
                            If cryptData(x) <> intChar Then cryptData(x) = (cryptData(x) Xor intChar)
                    End Select
                End If

                ' Select next encryption algorithm
                intAlgorithm = intAlgorithm + 1
                If intAlgorithm = 3 Then
                    intAlgorithm = 0

                    ' Select next encryption key
                    lngKeyPos = lngKeyPos + 1
                    If lngKeyPos > EncryptionKey.Length - 1 Then lngKeyPos = 0
                End If
            Next

            Return cryptData

        End Function

        Public Shared Function GenerateKey() As String

            Dim strKey As String = StandardKey & Replace(Replace(Replace(Replace(Guid.NewGuid.ToString() & GetShortAssemblyName(System.Reflection.Assembly.GetExecutingAssembly) & GetKeyFromSeed(Microsoft.VisualBasic.Timer) & Now() & Guid.NewGuid.ToString(), " ", "©"), "-", "~"), "/", "%"), ":", "§")
            Dim strChar As String
            Dim intKeyLen As Integer
            Dim x As Integer
            Dim y As Integer

            Randomize()

            intKeyLen = Len(strKey)

            For x = 1 To intKeyLen
                y = CInt(Int(Rnd() * intKeyLen) + 1)
                If x <> y Then
                    strChar = Mid(strKey, x, 1)
                    Mid(strKey, x, 1) = Mid(strKey, y, 1)
                    Mid(strKey, y, 1) = strChar
                End If
            Next

            Return strKey

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
        Public Shared Function GetSeedFromKey(ByVal Key As System.String) As Integer

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

End Namespace