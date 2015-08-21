Imports System.Text
Imports System.IO
Imports TVA.Compression
Imports TVA.Compression.Common
Imports TVA.Shared.String
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Testing
Imports VB = Microsoft.VisualBasic

Public Class CommonFunctionsTest

    Inherits TestBase

    Private Const BufferSize As Integer = 262144

    Public Sub TestAllDataCompressionLevels()

        Dim flgStringPassed As Boolean
        Dim flgBinaryPassed As Boolean

        Dim strBuffer As Byte()
        Dim binBuffer As Byte()

        Dim builder As New StringBuilder
        Dim sample As String = "This is test ;p ..." & vbCrLf & "    this is another line, did it come too, what about a date: [" & Now() & "]"
        Dim x As Integer

        For x = 1 To 150
            builder.Append(sample)
        Next

        strBuffer = ASCIIEncoding.ASCII.GetBytes(builder.ToString())
        binBuffer = GenSlightlyCompressableBinaryData(50 * 1024)

        If StringTestLevel(strBuffer, CompressLevel.NoCompression) Then
            If StringTestLevel(strBuffer, CompressLevel.DefaultCompression) Then
                If StringTestLevel(strBuffer, CompressLevel.BestSpeed) Then
                    If StringTestLevel(strBuffer, CompressLevel.BestCompression) Then
                        If StringTestLevel(strBuffer, CompressLevel.MultiPass) Then
                            flgStringPassed = True
                        End If
                    End If
                End If
            End If
        End If

        Assert(flgStringPassed, "Testing of all compression levels for String data")

        If BinaryTestLevel(binBuffer, CompressLevel.NoCompression) Then
            If BinaryTestLevel(binBuffer, CompressLevel.DefaultCompression) Then
                If BinaryTestLevel(binBuffer, CompressLevel.BestSpeed) Then
                    If BinaryTestLevel(binBuffer, CompressLevel.BestCompression) Then
                        If BinaryTestLevel(binBuffer, CompressLevel.MultiPass) Then
                            flgBinaryPassed = True
                        End If
                    End If
                End If
            End If
        End If

        Assert(flgBinaryPassed, "Test of all compression levels for Binary data")

    End Sub

    Public Sub TestAllFileCompressionLevels()

        Dim flgStringPassed As Boolean
        Dim flgBinaryPassed As Boolean

        If ASCIIFileTestLevel(CompressLevel.NoCompression) Then
            If ASCIIFileTestLevel(CompressLevel.DefaultCompression) Then
                If ASCIIFileTestLevel(CompressLevel.BestSpeed) Then
                    If ASCIIFileTestLevel(CompressLevel.BestCompression) Then
                        If ASCIIFileTestLevel(CompressLevel.MultiPass) Then
                            flgStringPassed = True
                        End If
                    End If
                End If
            End If
        End If

        Assert(flgStringPassed, "Test of all compression levels for ASCII data files")

        If BinaryFileTestLevel(CompressLevel.NoCompression) Then
            If BinaryFileTestLevel(CompressLevel.DefaultCompression) Then
                If BinaryFileTestLevel(CompressLevel.BestSpeed) Then
                    If BinaryFileTestLevel(CompressLevel.BestCompression) Then
                        If BinaryFileTestLevel(CompressLevel.MultiPass) Then
                            flgBinaryPassed = True
                        End If
                    End If
                End If
            End If
        End If

        Assert(flgBinaryPassed, "Test of all compression levels for Binary data files")

    End Sub

    Private Function StringTestLevel(ByVal Data As Byte(), ByVal Strength As CompressLevel) As Boolean

        Return Assert(TestDataLevel("String", Data, Strength), "String data test at compression level " & Strength)

    End Function

    Private Function BinaryTestLevel(ByVal Data As Byte(), ByVal Strength As CompressLevel) As Boolean

        Return Assert(TestDataLevel("Binary", Data, Strength), "Binary data test at compression level " & Strength)

    End Function

    Private Function TestDataLevel(ByVal Type As String, ByVal Data As Byte(), ByVal Strength As CompressLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim Data2 As Byte()
        Dim Data3 As Byte()
        Dim intCompare As Integer

        UpdateStatus(vbCrLf & [Enum].GetName(GetType(CompressLevel), Strength) & " " & Type & " Compression Test:" & vbCrLf)

        startTime = VB.Timer
        Data2 = Compress(Data, Strength)
        Data3 = Uncompress(Data2, Data.Length)
        stopTime = VB.Timer

        intCompare = CompareData(Data, Data3)

        UpdateStatus("Compression: " & Data.Length & " to " & Data2.Length & " (" & Format((Data.Length - Data2.Length) / Data.Length, "#.00%") & ")")
        UpdateStatus(" Total time: " & SecondsToText(stopTime - startTime, 2))
        UpdateStatus("Data 1: " & VB.Left(Base64Encode(Data), 100))
        UpdateStatus("Data 2: " & VB.Left(Base64Encode(Data2), 100))
        UpdateStatus("Data 3: " & VB.Left(Base64Encode(Data3), 100))
        UpdateStatus("End Compare = " & intCompare)

        Return (intCompare = 0)

    End Function

    Private Function ASCIIFileTestLevel(ByVal Strength As CompressLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = Logger.TestPath & "ASCIITest.txt"
        Dim str2 As String = Logger.TestPath & "ASCIITest.cmp"
        Dim str3 As String = Logger.TestPath & "ASCIITest.unc"
        Dim intCompare, intSLen, intDLen As Integer

        If Not File.Exists(str1) Then CreateASCIITestFile(str1)
        If GetFileLength(str1) = 0 Then CreateASCIITestFile(str1)

        UpdateStatus(vbCrLf & [Enum].GetName(GetType(CompressLevel), Strength) & " ASCII file compression test:" & vbCrLf)

        startTime = VB.Timer

        UpdateStatus("Compressing """ & JustFileName(str1) & """ to """ & JustFileName(str2) & """...")
        CompressFile(str1, str2, Strength, AddressOf ProgressHandler)

        UpdateStatus("Uncompressing """ & JustFileName(str2) & """ to """ & JustFileName(str3) & """...")
        UncompressFile(str2, str3, AddressOf ProgressHandler)

        stopTime = VB.Timer

        UpdateStatus("Comparing """ & JustFileName(str1) & """ to """ & JustFileName(str3) & """...")
        intCompare = CompareASCIIFile(str1, str3)

        intSLen = GetFileLength(str1)
        intDLen = GetFileLength(str2)
        UpdateStatus("Compression: " & intSLen & " to " & intDLen & " (" & Format((intSLen - intDLen) / intSLen, "#.00%") & ")")
        UpdateStatus(" Total time: " & SecondsToText(stopTime - startTime, 2))
        UpdateStatus("End Compare = " & intCompare)

        Return Assert((intCompare = 0), "ASCII file test at compression level " & Strength)

    End Function

    Private Function BinaryFileTestLevel(ByVal Strength As CompressLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = Logger.TestPath & "BinaryTest.bin"
        Dim str2 As String = Logger.TestPath & "BinaryTest.cmp"
        Dim str3 As String = Logger.TestPath & "BinaryTest.unc"
        Dim intCompare, intSLen, intDLen As Integer

        If Not File.Exists(str1) Then CreateBinaryTestFile(str1)
        If GetFileLength(str1) = 0 Then CreateBinaryTestFile(str1)

        UpdateStatus(vbCrLf & [Enum].GetName(GetType(CompressLevel), Strength) & " Binary file compression test:" & vbCrLf)

        startTime = VB.Timer

        UpdateStatus("Compressing """ & JustFileName(str1) & """ to """ & JustFileName(str2) & """...")
        CompressFile(str1, str2, Strength, AddressOf ProgressHandler)

        UpdateStatus("Uncompressing """ & JustFileName(str2) & """ to """ & JustFileName(str3) & """...")
        UncompressFile(str2, str3, AddressOf ProgressHandler)

        stopTime = VB.Timer

        UpdateStatus("Comparing """ & JustFileName(str1) & """ to """ & JustFileName(str3) & """...")
        intCompare = CompareBinaryFile(str1, str3)

        intSLen = GetFileLength(str1)
        intDLen = GetFileLength(str2)
        UpdateStatus("Compression: " & intSLen & " to " & intDLen & " (" & Format((intSLen - intDLen) / intSLen, "#.00%") & ")")
        UpdateStatus(" Total time: " & SecondsToText(stopTime - startTime, 2))
        UpdateStatus("End Compare = " & intCompare)

        Return Assert((intCompare = 0), "Binary file test at compression level " & Strength)

    End Function

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

    ' Slightly compressable binary data generator...
    Friend Function GenSlightlyCompressableBinaryData(ByVal TotalBytes As Integer) As Byte()

        Dim x As Integer
        Dim Data(TotalBytes - 1) As Byte

        Rnd(-1)
        Randomize()

        For x = 0 To TotalBytes - 1 Step 4
            Data(x) = CByte(Int(Rnd() * 256))
            If CoinToss() Then
                Data(x + 1) = Data(x)
                If CoinToss() Then
                    Data(x + 2) = Data(x)
                    If CoinToss() Then
                        Data(x + 3) = Data(x)
                    Else
                        Data(x + 3) = CByte(Int(Rnd() * 256))
                    End If
                Else
                    Data(x + 2) = CByte(Int(Rnd() * 256))
                End If
            Else
                Data(x + 1) = CByte(Int(Rnd() * 256))
            End If
        Next

        Return Data

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

    Friend Function CoinToss() As Boolean

        Return CInt(Int(2 * Rnd() + 1)) = 1

    End Function

    Private Sub UpdateStatus(ByVal Status As String, Optional ByVal Tag As String = Nothing)

        LogEvent(EventType.Status, Tag, Status)

    End Sub

    Private Sub ProgressHandler(ByVal BytesCompleted As Long, ByVal BytesTotal As Long)

        ' Want to give up time (DoEvents) so the app responds better
        UpdateTestProgress(BytesCompleted, BytesTotal)

    End Sub

End Class
