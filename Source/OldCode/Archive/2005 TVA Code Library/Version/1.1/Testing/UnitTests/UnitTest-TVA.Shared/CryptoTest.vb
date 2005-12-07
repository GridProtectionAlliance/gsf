Imports System.Text
Imports System.IO
Imports TVA.Shared.Crypto
Imports TVA.Shared.FilePath
Imports TVA.Shared.String
Imports TVA.Testing

Public Class CryptoTest

    Inherits TestBase

    Private Const BufferSize As Integer = 262144

    Public Sub TestAllEncryptionLevelsWithDefaultPassword()

        Assert(TestAllLevels(""), "Test of all encryption levels and data types with no password")

    End Sub

    Public Sub TestAllEncryptionLevelsWithGeneratedPassword()

        Assert(TestAllLevels(GenerateKey()), "Test of all encryption levels and data types with generated password")

    End Sub

    Public Sub TestAllFileEncryptionLevelsWithDefaultPassword()

        Assert(TestAllFileLevels(""), "Test of all file encryption levels and data types with no password")

    End Sub

    Public Sub TestAllFileEncryptionLevelsWithGeneratedPassword()

        Assert(TestAllFileLevels(GenerateKey()), "Test of all file encryption levels and data types with generated password")

    End Sub

    Private Function TestAllLevels(ByVal Key As String) As Boolean

        Dim flgStringPassed As Boolean
        Dim flgBinaryPassed As Boolean

        If StringTestLevel(Key, EncryptLevel.Level1) Then
            If StringTestLevel(Key, EncryptLevel.Level2) Then
                If StringTestLevel(Key, EncryptLevel.Level3) Then
                    If StringTestLevel(Key, EncryptLevel.Level4) Then
                        flgStringPassed = True
                    End If
                End If
            End If
        End If

        Assert(flgStringPassed, "Testing of all encryption levels for text data")

        If BinaryTestLevel(Key, EncryptLevel.Level1) Then
            If BinaryTestLevel(Key, EncryptLevel.Level2) Then
                If BinaryTestLevel(Key, EncryptLevel.Level3) Then
                    If BinaryTestLevel(Key, EncryptLevel.Level4) Then
                        flgBinaryPassed = True
                    End If
                End If
            End If
        End If

        Assert(flgBinaryPassed, "Test of all encryption levels for binary data")

        Return (flgStringPassed And flgBinaryPassed)

    End Function

    Private Function TestAllFileLevels(ByVal Key As String) As Boolean

        Dim flgStringPassed As Boolean
        Dim flgBinaryPassed As Boolean

        If ASCIIFileTestLevel(Key, EncryptLevel.Level1) Then
            If ASCIIFileTestLevel(Key, EncryptLevel.Level2) Then
                If ASCIIFileTestLevel(Key, EncryptLevel.Level3) Then
                    If ASCIIFileTestLevel(Key, EncryptLevel.Level4) Then
                        flgStringPassed = True
                    End If
                End If
            End If
        End If

        Assert(flgStringPassed, "Test of all encryption levels for ASCII data files")

        If BinaryFileTestLevel(Key, EncryptLevel.Level1) Then
            If BinaryFileTestLevel(Key, EncryptLevel.Level2) Then
                If BinaryFileTestLevel(Key, EncryptLevel.Level3) Then
                    If BinaryFileTestLevel(Key, EncryptLevel.Level4) Then
                        flgBinaryPassed = True
                    End If
                End If
            End If
        End If

        Assert(flgBinaryPassed, "Test of all encryption levels for binary data files")

        Return (flgStringPassed And flgBinaryPassed)

    End Function

    Private Function StringTestLevel(ByVal Key As String, ByVal Level As EncryptLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = "This is test..." & vbCrLf & "    this is another line, did it come too, what about a date: [" & Now() & "]"
        Dim str2 As String
        Dim str3 As String
        Dim intCompare As Integer

        UpdateStatus(vbCrLf & Level.GetName(GetType(EncryptLevel), Level) & " text data encryption test:" & vbCrLf)

        startTime = Timer
        str2 = Encrypt(str1, Key, Level)
        str3 = Decrypt(str2, Key, Level)
        stopTime = Timer

        intCompare = StrComp(str1, str3, CompareMethod.Binary)

        UpdateStatus("String 1: " & str1)
        UpdateStatus("String 2: " & str2)
        UpdateStatus("String 3: " & str3)
        UpdateStatus("End Compare = " & intCompare)
        UpdateStatus("Total time: " & stopTime - startTime)

        Return Assert((intCompare = 0), Level.GetName(GetType(EncryptLevel), Level) & " text data encryption test")

    End Function

    Private Function ASCIIFileTestLevel(ByVal Key As String, ByVal Level As EncryptLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = Logger.TestPath & "ASCIITest.txt"
        Dim str2 As String = Logger.TestPath & "ASCIITest.enc"
        Dim str3 As String = Logger.TestPath & "ASCIITest.dec"
        Dim intCompare As Integer

        If Not File.Exists(str1) Then CreateASCIITestFile(str1)
        If GetFileLength(str1) = 0 Then CreateASCIITestFile(str1)

        UpdateStatus(vbCrLf & Level.GetName(GetType(EncryptLevel), Level) & " ASCII file encryption test:" & vbCrLf)

        startTime = Timer

        UpdateStatus("Encrypting """ & JustFileName(str1) & """ to """ & JustFileName(str2) & """...")
        EncryptFile(str1, str2, Key, Level, AddressOf ProgressHandler)

        UpdateStatus("Decrypting """ & JustFileName(str2) & """ to """ & JustFileName(str3) & """...")
        DecryptFile(str2, str3, Key, Level, AddressOf ProgressHandler)

        stopTime = Timer

        UpdateStatus("Comparing """ & JustFileName(str1) & """ to """ & JustFileName(str3) & """...")
        intCompare = CompareASCIIFile(str1, str3)

        UpdateStatus("End Compare = " & intCompare)
        UpdateStatus("Total time: " & stopTime - startTime)

        Return Assert((intCompare = 0), Level.GetName(GetType(EncryptLevel), Level) & " ASCII file encryption test")

    End Function

    Private Function BinaryTestLevel(ByVal Key As String, ByVal Level As EncryptLevel) As Boolean

        If Key = "" Then Key = "{&-<%=($#/T.V:A!\,@[20O3]*^_j`|?)>+~}"
        Dim startTime As Double
        Dim stopTime As Double
        Dim Data1 As Byte() = GenRandomBinaryData(1024)
        Dim Data2 As Byte()
        Dim Data3 As Byte()
        Dim KeyBA As Byte() = ASCIIEncoding.ASCII.GetBytes(Key)
        Dim intCompare As Integer

        Debug.Write(vbCrLf & [Enum].GetName(GetType(EncryptLevel), Level) & " binary data encryption test:" & vbCrLf)

        startTime = Timer
        Data2 = Encrypt(Data1, KeyBA, KeyBA, Level)
        Data3 = Decrypt(Data2, KeyBA, KeyBA, Level)
        stopTime = Timer

        intCompare = CompareData(Data1, Data3)

        UpdateStatus("Data 1: " & Base64Encode(Data1))
        UpdateStatus("Data 2: " & Base64Encode(Data2))
        UpdateStatus("Data 3: " & Base64Encode(Data3))
        UpdateStatus("End Compare = " & intCompare)
        UpdateStatus("Total time: " & stopTime - startTime)

        Return Assert((intCompare = 0), Level.GetName(GetType(EncryptLevel), Level) & " binary data encryption test")

    End Function

    Private Function BinaryFileTestLevel(ByVal Key As String, ByVal Level As EncryptLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = Logger.TestPath & "BinaryTest.bin"
        Dim str2 As String = Logger.TestPath & "BinaryTest.enc"
        Dim str3 As String = Logger.TestPath & "BinaryTest.dec"
        Dim intCompare As Integer

        If Not File.Exists(str1) Then CreateBinaryTestFile(str1)
        If GetFileLength(str1) = 0 Then CreateBinaryTestFile(str1)

        UpdateStatus(vbCrLf & Level.GetName(GetType(EncryptLevel), Level) & " binary file encryption test:" & vbCrLf)

        startTime = Timer

        UpdateStatus("Encrypting """ & JustFileName(str1) & """ to """ & JustFileName(str2) & """...")
        EncryptFile(str1, str2, Key, Level, AddressOf ProgressHandler)

        UpdateStatus("Decrypting """ & JustFileName(str2) & """ to """ & JustFileName(str3) & """...")
        DecryptFile(str2, str3, Key, Level, AddressOf ProgressHandler)

        stopTime = Timer

        UpdateStatus("Comparing """ & JustFileName(str1) & """ to """ & JustFileName(str3) & """...")
        intCompare = CompareBinaryFile(str1, str3)

        UpdateStatus("End Compare = " & intCompare)
        UpdateStatus("Total time: " & stopTime - startTime)

        Return Assert((intCompare = 0), Level.GetName(GetType(EncryptLevel), Level) & " binary file encryption test")

    End Function

    Friend Function GenRandomBinaryData(ByVal TotalBytes As Integer) As Byte()

        Dim x As Integer
        Dim Data(TotalBytes - 1) As Byte

        Rnd(-1)
        Randomize()

        For x = 0 To TotalBytes - 1
            Data(x) = CByte(Int(Rnd() * 256))
        Next

        Return Data

    End Function

    Friend Sub CreateASCIITestFile(ByVal FileName As String)

        File.Copy(JustPath(Me.GetType.Assembly.GetExecutingAssembly.Location) & "ASCIITest.txt", FileName)

    End Sub

    Friend Sub CreateBinaryTestFile(ByVal FileName As String)

        Dim fs As FileStream = File.Create(FileName)
        Dim binData() As Byte
        Dim x As Integer

        For x = 1 To 10000
            binData = GenRandomBinaryData(1024)
            fs.Write(binData, 0, binData.Length)
        Next

        fs.Close()

    End Sub

    Private Function CompareData(ByVal Data1 As Byte(), ByVal Data2 As Byte()) As Integer

        If Data1.Length = Data2.Length Then
            Dim x As Integer

            For x = 0 To Data1.Length - 1
                If Data1(x) <> Data2(x) Then
                    Return IIf(Data1(x) < Data2(x), -1, 1)
                End If
            Next
        Else
            Assert(False, "", "Error in CompareData function: data size mismatch!!!")
        End If

        Return 0

    End Function

    Private Function CompareASCIIFile(ByVal File1 As String, ByVal File2 As String) As Integer

        Dim fs1, fs2 As StreamReader
        Dim intF1Len As Integer = GetFileLength(File1)
        Dim intF2Len As Integer = GetFileLength(File2)
        Dim strF1Line, strF2Line As String
        Dim intCompare As Integer = IIf(intF1Len > intF2Len, 1, IIf(intF1Len < intF2Len, -1, 0))
        Dim lngCompleted As Long
        Dim lngTotal As Long = intF1Len + intF2Len

        If intCompare = 0 Then
            fs1 = File.OpenText(File1)
            fs2 = File.OpenText(File2)

            While fs1.Peek() >= 0 And fs1.Peek() >= 0
                strF1Line = fs1.ReadLine()
                strF2Line = fs2.ReadLine()

                intCompare = StrComp(strF1Line, strF2Line, CompareMethod.Binary)
                If intCompare <> 0 Then Exit While

                lngCompleted += Len(strF1Line) + Len(strF2Line)
                UpdateTestProgress(lngCompleted, lngTotal)
            End While

            UpdateTestProgress(lngTotal, lngTotal)

            fs1.Close()
            fs2.Close()
        End If

        Return intCompare

    End Function

    Private Function CompareBinaryFile(ByVal File1 As String, ByVal File2 As String) As Integer

        Dim fs1, fs2 As FileStream
        Dim intF1Len As Integer = GetFileLength(File1)
        Dim intF2Len As Integer = GetFileLength(File2)
        Dim intCompare As Integer = IIf(intF1Len > intF2Len, 1, IIf(intF1Len < intF2Len, -1, 0))
        Dim fs1Buffer(BufferSize) As Byte
        Dim fs2Buffer(BufferSize) As Byte
        Dim intF1Read As Integer
        Dim intF2Read As Integer
        Dim lngCompleted As Long
        Dim lngTotal As Long = intF1Len + intF2Len

        If intCompare = 0 Then
            fs1 = File.Open(File1, FileMode.Open, FileAccess.Read, FileShare.Read)
            fs2 = File.Open(File2, FileMode.Open, FileAccess.Read, FileShare.Read)

            intF1Read = fs1.Read(fs1Buffer, 0, BufferSize)
            intF2Read = fs2.Read(fs2Buffer, 0, BufferSize)

            While intF1Read > 0
                If intF1Read <> intF2Read Then
                    UpdateStatus("Error encountered during binary file comparision: file stream corruption!")
                    Stop
                End If

                intCompare = CompareData(fs1Buffer, fs2Buffer)
                If intCompare <> 0 Then Exit While

                lngCompleted += intF1Read + intF2Read
                UpdateTestProgress(lngCompleted, lngTotal)

                intF1Read = fs1.Read(fs1Buffer, 0, BufferSize)
                intF2Read = fs2.Read(fs2Buffer, 0, BufferSize)
            End While

            UpdateTestProgress(lngTotal, lngTotal)

            fs1.Close()
            fs2.Close()
        End If

        Return intCompare

    End Function

    Private Sub UpdateStatus(ByVal Status As String, Optional ByVal Tag As String = Nothing)

        LogEvent(EventType.Status, Tag, Status)

    End Sub

    Private Sub ProgressHandler(ByVal BytesCompleted As Long, ByVal BytesTotal As Long)

        ' Want to give up time (DoEvents) so the app responds better
        UpdateTestProgress(BytesCompleted, BytesTotal)

    End Sub

End Class
