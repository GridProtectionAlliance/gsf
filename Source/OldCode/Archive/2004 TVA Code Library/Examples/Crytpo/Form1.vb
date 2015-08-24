Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports TVA.Shared.Crypto
Imports TVA.Shared.String
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath
Imports VB = Microsoft.VisualBasic

Public Class Form1
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        '
        'Form1
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(424, 325)
        Me.Name = "Form1"
        Me.Text = "Form1"

    End Sub

#End Region

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'DecryptFile("SDBDump.bin", "SDBDump.mdb", "TVATelegyrDataExchangeService2003", 4)

        'Dim x As Integer

        'Dim c As Char
        'For Each c In Path.InvalidPathChars
        '    Debug.Write("\u" & AscW(c).ToString("x"c).PadLeft(4, "0"))
        'Next

        'Debug.WriteLine(IsFilePatternMatch("*.bin*", "c:\hello.binary"))
        'Debug.WriteLine(JustDrive(GetApplicationPath()))
        'Debug.WriteLine(JustPath("file:///c:/inetpub/wwwroot/Hello/bin/test.dll"))
        'Debug.WriteLine(TrimFileName("c:\inetpub\wwwroot\This is a very long file name.longext", 45))

        TestAllLevels("")
        TestAllLevels(GenerateKey())

        TestAllFileLevels("")
        TestAllFileLevels(GenerateKey())

        'Debug.WriteLine("Result Comparision: " & StrComp(str1, str2, CompareMethod.Binary))

        '' Re-seed random number generator
        'Dim strOrgKey As String
        'Dim chrSequence As Integer()
        'Dim intKeyLen As Integer = Len(strKey)
        'Dim chrCurr As Char
        'Dim x As Integer
        'Dim y As Integer
        'Rnd(-1)
        'Randomize(Asc(12))
        'ReDim chrSequence(intKeyLen)

        'For x = 0 To intKeyLen - 1
        '    chrSequence(x) = CInt(Int(Rnd() * intKeyLen) + 1)
        'Next

        'Debug.WriteLine("PreCycle : " & strKey)
        'strOrgKey = strKey

        'For x = 1 To intKeyLen
        '    y = chrSequence(x - 1)
        '    If x <> y Then
        '        chrCurr = Mid(strKey, x, 1)
        '        Mid(strKey, x, 1) = Mid(strKey, y, 1)
        '        Mid(strKey, y, 1) = chrCurr
        '    End If
        'Next

        'Debug.WriteLine("PostCycle: " & strKey)

        'For x = intKeyLen To 1 Step -1
        '    y = chrSequence(x - 1)
        '    If x <> y Then
        '        chrCurr = Mid(strKey, x, 1)
        '        Mid(strKey, x, 1) = Mid(strKey, y, 1)
        '        Mid(strKey, y, 1) = chrCurr
        '    End If
        'Next

        'Debug.WriteLine("Restored : " & strKey)
        'Debug.WriteLine("Comparison Result = " & StrComp(strOrgKey, strKey, CompareMethod.Binary))

        'Dim strFile As String = "abc.thisisalongextension"

        'Debug.WriteLine(TrimFileName(strFile, 12))

        Stop

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

        If BinaryTestLevel(Key, EncryptLevel.Level1) Then
            If BinaryTestLevel(Key, EncryptLevel.Level2) Then
                If BinaryTestLevel(Key, EncryptLevel.Level3) Then
                    If BinaryTestLevel(Key, EncryptLevel.Level4) Then
                        flgBinaryPassed = True
                    End If
                End If
            End If
        End If

        If flgStringPassed Then
            Debug.WriteLine("All String Tests Passed!")
        Else
            Debug.WriteLine("*** STRING TEST FAILED ***")
        End If

        If flgBinaryPassed Then
            Debug.WriteLine("All Binary Tests Passed!")
        Else
            Debug.WriteLine("*** BINARY TEST FAILED ***")
        End If

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

        If BinaryFileTestLevel(Key, EncryptLevel.Level1) Then
            If BinaryFileTestLevel(Key, EncryptLevel.Level2) Then
                If BinaryFileTestLevel(Key, EncryptLevel.Level3) Then
                    If BinaryFileTestLevel(Key, EncryptLevel.Level4) Then
                        flgBinaryPassed = True
                    End If
                End If
            End If
        End If

        If flgStringPassed Then
            Debug.WriteLine("All ASCII File Tests Passed!")
        Else
            Debug.WriteLine("*** ASCII FILE TEST FAILED ***")
        End If

        If flgBinaryPassed Then
            Debug.WriteLine("All Binary File Tests Passed!")
        Else
            Debug.WriteLine("*** BINARY FILE TEST FAILED ***")
        End If

    End Function

    Private Function StringTestLevel(ByVal Key As String, ByVal Level As EncryptLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = "This is test..." & vbCrLf & "    this is another line, did it come too, what about a date: [" & Now() & "]"
        Dim str2 As String
        Dim str3 As String
        Dim intCompare As Integer

        Debug.WriteLine(vbCrLf & Level.GetName(GetType(EncryptLevel), Level) & " Encryption Test:" & vbCrLf)

        startTime = VB.Timer
        str2 = Encrypt(str1, Key, Level)
        str3 = Decrypt(str2, Key, Level)
        stopTime = VB.Timer

        intCompare = StrComp(str1, str3, CompareMethod.Binary)

        Debug.WriteLine("String 1: " & str1)
        Debug.WriteLine("String 2: " & str2)
        Debug.WriteLine("String 3: " & str3)
        Debug.WriteLine("End Compare = " & intCompare)
        Debug.WriteLine("Total time: " & stopTime - startTime)

        Return (intCompare = 0)

    End Function

    Private Function ASCIIFileTestLevel(ByVal Key As String, ByVal Level As EncryptLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = "ASCIITest.txt"
        Dim str2 As String = "ASCIITest.enc"
        Dim str3 As String = "ASCIITest.dec"
        Dim intCompare As Integer

        Debug.WriteLine(vbCrLf & Level.GetName(GetType(EncryptLevel), Level) & " ASCII File Encryption Test:" & vbCrLf)

        startTime = VB.Timer
        EncryptFile(str1, str2, Key, Level)
        DecryptFile(str2, str3, Key, Level)
        stopTime = VB.Timer

        intCompare = CompareASCIIFile(str1, str3)

        Debug.WriteLine("End Compare = " & intCompare)
        Debug.WriteLine("Total time: " & stopTime - startTime)

        Return (intCompare = 0)

    End Function

    Private Function BinaryTestLevel(ByVal Key As String, ByVal Level As EncryptLevel) As Boolean

        If Key = "" Then Key = "{&-<%=($#/T.V:A!\,@[20O3]*^_j`|?)>+~}"
        Dim startTime As Double
        Dim stopTime As Double
        Dim Data1 As Byte() = GenRandomBinaryData()
        Dim Data2 As Byte()
        Dim Data3 As Byte()
        Dim KeyBA As Byte() = ASCIIEncoding.ASCII.GetBytes(Key)
        Dim intCompare As Integer

        Debug.Write(vbCrLf & [Enum].GetName(GetType(EncryptLevel), Level) & " Binary Encryption Test:" & vbCrLf)

        startTime = VB.Timer
        Data2 = Encrypt(Data1, KeyBA, KeyBA, Level)
        Data3 = Decrypt(Data2, KeyBA, KeyBA, Level)
        stopTime = VB.Timer

        intCompare = CompareBinaryData(Data1, Data3)

        Debug.WriteLine("Data 1: " & Base64Encode(Data1))
        Debug.WriteLine("Data 2: " & Base64Encode(Data2))
        Debug.WriteLine("Data 3: " & Base64Encode(Data3))
        Debug.WriteLine("End Compare = " & intCompare)
        Debug.WriteLine("Total time: " & stopTime - startTime)

        Return (intCompare = 0)

    End Function

    Private Function BinaryFileTestLevel(ByVal Key As String, ByVal Level As EncryptLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = "BinaryTest.bin"
        Dim str2 As String = "BinaryTest.enc"
        Dim str3 As String = "BinaryTest.dec"
        Dim intCompare As Integer

        If Not File.Exists(str1) Then CreateBinaryTestFile(str1)
        If GetFileLength(str1) = 0 Then CreateBinaryTestFile(str1)

        Debug.WriteLine(vbCrLf & Level.GetName(GetType(EncryptLevel), Level) & " Binary File Encryption Test:" & vbCrLf)

        startTime = VB.Timer
        EncryptFile(str1, str2, Key, Level)
        DecryptFile(str2, str3, Key, Level)
        stopTime = VB.Timer

        intCompare = CompareBinaryFile(str1, str3)

        Debug.WriteLine("End Compare = " & intCompare)
        Debug.WriteLine("Total time: " & stopTime - startTime)

        Return (intCompare = 0)

    End Function

    Private Function GenRandomBinaryData() As Byte()

        Const TotalBytes As Integer = 1024
        Dim x As Integer
        Dim Data(TotalBytes - 1) As Byte

        Rnd(-1)
        Randomize()

        For x = 0 To TotalBytes - 1
            Data(x) = CByte(Int(Rnd() * 256))
        Next

        Return Data

    End Function

    Private Sub CreateBinaryTestFile(ByVal FileName As String)

        Dim fs As FileStream = File.Create(FileName)
        Dim binData() As Byte
        Dim x As Integer

        For x = 1 To 100
            binData = GenRandomBinaryData()
            fs.Write(binData, 0, binData.Length)
        Next

        fs.Close()

    End Sub

    Private Function CompareBinaryData(ByVal Data1 As Byte(), ByVal Data2 As Byte()) As Integer

        If Data1.Length = Data2.Length Then
            Dim x As Integer

            For x = 0 To Data1.Length - 1
                If Data1(x) <> Data2(x) Then
                    Return IIf(Data1(x) < Data2(x), -1, 1)
                End If
            Next
        Else
            Debug.WriteLine("WARNING: Binary data size mismatch!!!")
            Stop
        End If

        Return 0

    End Function

    Private Function CompareASCIIFile(ByVal File1 As String, ByVal File2 As String) As Integer

        Dim fs1, fs2 As StreamReader
        Dim intF1Len As Integer = GetFileLength(File1)
        Dim intF2Len As Integer = GetFileLength(File2)
        Dim intCompare As Integer = IIf(intF1Len > intF2Len, 1, IIf(intF1Len < intF2Len, -1, 0))

        If intCompare = 0 Then
            fs1 = File.OpenText(File1)
            fs2 = File.OpenText(File2)

            While fs1.Peek() >= 0 And fs1.Peek() >= 0
                intCompare = StrComp(fs1.ReadLine(), fs2.ReadLine(), CompareMethod.Binary)
                If intCompare <> 0 Then Exit While
            End While

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
        Dim fs1Buffer(2047) As Byte
        Dim fs2Buffer(2047) As Byte
        Dim intF1Read As Integer
        Dim intF2Read As Integer

        If intCompare = 0 Then
            fs1 = File.Open(File1, FileMode.Open, FileAccess.Read, FileShare.Read)
            fs2 = File.Open(File2, FileMode.Open, FileAccess.Read, FileShare.Read)

            intF1Read = fs1.Read(fs1Buffer, 0, 2048)
            intF2Read = fs2.Read(fs2Buffer, 0, 2048)

            While intF1Read > 0
                If intF1Read <> intF2Read Then
                    Debug.WriteLine("Error encountered during binary file comparision: file stream corruption!")
                    Stop
                End If

                intCompare = CompareBinaryData(fs1Buffer, fs2Buffer)
                If intCompare <> 0 Then Exit While

                intF1Read = fs1.Read(fs1Buffer, 0, 2048)
                intF2Read = fs2.Read(fs2Buffer, 0, 2048)
            End While

            fs1.Close()
            fs2.Close()
        End If

        Return intCompare

    End Function


End Class
