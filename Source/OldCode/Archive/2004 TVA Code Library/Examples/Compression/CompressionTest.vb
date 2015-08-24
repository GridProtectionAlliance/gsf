Imports System.Text
Imports System.IO
Imports TVA.Compression
Imports TVA.Compression.Common
Imports TVA.Compression.ZipFile
Imports TVA.Shared.String
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports VB = Microsoft.VisualBasic

Public Class CompressionTest
    Inherits System.Windows.Forms.Form

    Private Const MaxStatusLength As Integer = 524288

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
    Friend WithEvents StatusText As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CompressionTestButton As System.Windows.Forms.Button
    Friend WithEvents ZipTestButton As System.Windows.Forms.Button
    Friend WithEvents ZipFile As TVA.Compression.ZipFile
    Friend WithEvents ProgressBar As System.Windows.Forms.ProgressBar
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.StatusText = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.CompressionTestButton = New System.Windows.Forms.Button
        Me.ZipTestButton = New System.Windows.Forms.Button
        Me.ZipFile = New TVA.Compression.ZipFile
        Me.ProgressBar = New System.Windows.Forms.ProgressBar
        Me.SuspendLayout()
        '
        'StatusText
        '
        Me.StatusText.BackColor = System.Drawing.Color.Black
        Me.StatusText.Font = New System.Drawing.Font("Lucida Console", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StatusText.ForeColor = System.Drawing.Color.White
        Me.StatusText.Location = New System.Drawing.Point(8, 24)
        Me.StatusText.MaxLength = 0
        Me.StatusText.Multiline = True
        Me.StatusText.Name = "StatusText"
        Me.StatusText.ReadOnly = True
        Me.StatusText.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.StatusText.Size = New System.Drawing.Size(568, 296)
        Me.StatusText.TabIndex = 0
        Me.StatusText.Text = ""
        Me.StatusText.WordWrap = False
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(8, 8)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(560, 16)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Compression Test Results:"
        '
        'CompressionTestButton
        '
        Me.CompressionTestButton.Location = New System.Drawing.Point(584, 24)
        Me.CompressionTestButton.Name = "CompressionTestButton"
        Me.CompressionTestButton.Size = New System.Drawing.Size(112, 23)
        Me.CompressionTestButton.TabIndex = 2
        Me.CompressionTestButton.Text = "Test &Compression"
        '
        'ZipTestButton
        '
        Me.ZipTestButton.Location = New System.Drawing.Point(584, 56)
        Me.ZipTestButton.Name = "ZipTestButton"
        Me.ZipTestButton.Size = New System.Drawing.Size(112, 23)
        Me.ZipTestButton.TabIndex = 3
        Me.ZipTestButton.Text = "Test &Zip"
        '
        'ZipFile
        '
        Me.ZipFile.AutoRefresh = True
        Me.ZipFile.CaseSensitive = False
        Me.ZipFile.Strength = TVA.Compression.CompressLevel.DefaultCompression
        '
        'ProgressBar
        '
        Me.ProgressBar.Location = New System.Drawing.Point(8, 328)
        Me.ProgressBar.Name = "ProgressBar"
        Me.ProgressBar.Size = New System.Drawing.Size(568, 23)
        Me.ProgressBar.TabIndex = 4
        '
        'CompressionTest
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(702, 355)
        Me.Controls.Add(Me.ProgressBar)
        Me.Controls.Add(Me.ZipTestButton)
        Me.Controls.Add(Me.CompressionTestButton)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.StatusText)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.MaximizeBox = False
        Me.Name = "CompressionTest"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Compression Testing"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ClearStatus()

    End Sub

    Private Sub ClearStatus()

        StatusText.Text = ""
        UpdateStatus("ZLib Version: " & ZLibVersion() & vbCrLf)

    End Sub

    Private Sub UpdateStatus(ByVal Status As String)

        StatusText.Text += Status & vbCrLf
        StatusText.SelectionStart = Len(StatusText.Text)
        StatusText.ScrollToCaret()

    End Sub

    Private Sub CompressionTestButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CompressionTestButton.Click

        ClearStatus()
        UpdateStatus(vbCrLf & vbCrLf & "Starting compression functions tests...")
        TestAllDataCompressionLevels()
        TestAllFileCompressionLevels()

    End Sub

    Private Sub ZipTestButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ZipTestButton.Click

        ClearStatus()
        UpdateStatus(vbCrLf & vbCrLf & "Starting Zip component tests..." & vbCrLf)

        UpdateStatus(vbCrLf & "Testing ZipFile.Add(""Binary.*""), recursive, all compression levels" & vbCrLf & "   without a zip file password..." & vbCrLf)
        TestAllZipCompressionLevels("NoPasswordTest.zip", Nothing)
        UpdateStatus("   without a zip file password..." & vbCrLf)

        UpdateStatus(vbCrLf & "Testing ZipFile.Add(""Binary.*""), recursive, all compression levels" & vbCrLf & "   with a zip file password..." & vbCrLf)
        TestAllZipCompressionLevels("PasswordTest.zip", "password")
        UpdateStatus("   with a zip file password..." & vbCrLf)

        UpdateStatus(vbCrLf & "Testing zip file compressed file collection:" & vbCrLf)
        Dim cmpFile As ZipFile.CompressedFile
        Dim x As Integer

        ZipFile.Close()
        ZipFile.FileName = "NoPasswordTest.zip"
        ZipFile.Password = ""
        ZipFile.Open()

        For Each cmpFile In ZipFile.Files
            x = x + 1
            With cmpFile
                UpdateStatus("Compressed File (" & x & "):")
                UpdateStatus("                     File Name: " & .FileName)
                UpdateStatus("               Compressed Size: " & .CompressedSize)
                UpdateStatus("             Uncompressed Size: " & .UncompressedSize)
                UpdateStatus("                 File DateTime: " & .FileDateTime)
                UpdateStatus("                 DOS Date Time: " & .DOSDateTime)
                UpdateStatus("              Extra Data Bytes: " & .ExtraData.ToString())
                UpdateStatus("                  File Comment: " & .FileComment)
                UpdateStatus("            Compression Method: " & .CompressionMethod)
                UpdateStatus("      External File Attributes: " & .ExternalFileAttributes)
                UpdateStatus("      Internal File Attributes: " & .InternalFileAttributes)
                UpdateStatus("        Zip Version Created By: " & .ZipVersionCreatedBy)
                UpdateStatus(" Zip Version Needed To Extract: " & .ZipVersionNeededToExtract)
                UpdateStatus("             Disk Number Start: " & .DiskNumberStart)
                UpdateStatus("                           CRC: " & .CRC)
                UpdateStatus("                          Flag: " & .Flag)
            End With
        Next

    End Sub

    Private Sub TestAllDataCompressionLevels()

        Dim flgStringPassed As Boolean
        Dim flgBinaryPassed As Boolean

        Dim strBuffer As Byte()
        Dim binBuffer As Byte()

        Dim builder As New StringBuilder()
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

        If flgStringPassed Then
            UpdateStatus("All String Tests Passed!")
        Else
            UpdateStatus("*** STRING TEST FAILED ***")
        End If

        If flgBinaryPassed Then
            UpdateStatus("All Binary Tests Passed!")
        Else
            UpdateStatus("*** BINARY TEST FAILED ***")
        End If

    End Sub

    Private Sub TestAllZipCompressionLevels(ByVal FileName As String, ByVal Password As String)

        Dim flgAllLevelsPassed As Boolean

        'Dim zip As ZipFile = ZipFile.Open(FileName, Password)
        ZipFile.FileName = FileName
        ZipFile.Password = Password

        If TestZipFileCompressionLevel(CompressLevel.NoCompression) Then
            If TestZipFileCompressionLevel(CompressLevel.DefaultCompression) Then
                If TestZipFileCompressionLevel(CompressLevel.BestSpeed) Then
                    If TestZipFileCompressionLevel(CompressLevel.BestCompression) Then
                        flgAllLevelsPassed = True
                    End If
                End If
            End If
        End If

        If flgAllLevelsPassed Then
            UpdateStatus(vbCrLf & "All Zip File Compression Level Tests Passed!")
        Else
            UpdateStatus(vbCrLf & "*** ZIP FILE TEST FAILED ***")
        End If

    End Sub

    Private Function TestZipFileCompressionLevel(ByVal Strength As CompressLevel) As Boolean

        Dim intCompare As Integer
        Dim str1, str2 As String

        If ZipFile.IsOpen Then ZipFile.Close()
        If File.Exists(ZipFile.FileName) Then File.Delete(ZipFile.FileName)

        RemoveAnyExistingUnzippedFiles()

        ZipFile.Open()
        ZipFile.Strength = Strength

        UpdateStatus(vbCrLf & [Enum].GetName(GetType(CompressLevel), Strength) & " ""Recursive Add"" Zip File Compression Test:" & vbCrLf)
        ZipFile.Add(Application.StartupPath & "\BinaryTest.*", True, PathInclusion.RelativePath)
        ZipFile.Add(Application.StartupPath & "\ASCIITest.txt")

        UpdateStatus(vbCrLf & """Extract"" Zip File Compression Test:" & vbCrLf)
        ZipFile.Extract("BinaryTest.*", Application.StartupPath & "\Unzip\", UpdateOption.Always, PathInclusion.RelativePath)

        ZipFile.Close()

        UpdateStatus(vbCrLf & """Update"" Zip File Compression Test:" & vbCrLf)
        ZipFile.Update(Application.StartupPath & "\BinaryTest.*", UpdateOption.Always, False, True, PathInclusion.RelativePath)

        UpdateStatus(vbCrLf & """Remove"" Zip File Compression Test:" & vbCrLf)
        ZipFile.Remove("*.txt")

        ZipFile.Close()

        UpdateStatus(vbCrLf & "Validating extractions to source..." & vbCrLf)
        For Each str1 In Directory.GetFiles(Application.StartupPath & "\", "BinaryTest.*")
            str2 = JustPath(str1) & "\Unzip\" & JustFileName(str1)
            intCompare = CompareBinaryFile(str1, str2)
            If intCompare <> 0 Then Exit For
        Next

        If intCompare = 0 Then
            For Each str1 In Directory.GetFiles(Application.StartupPath & "\SubFolderTest\", "BinaryTest.*")
                str2 = Application.StartupPath & "\Unzip\SubFolderTest\" & JustFileName(str1)
                intCompare = CompareBinaryFile(str1, str2)
                If intCompare <> 0 Then Exit For
            Next
        End If

        UpdateStatus("End Compare = " & intCompare)

        Return (intCompare = 0)

    End Function

    Private Sub RemoveAnyExistingUnzippedFiles()

        On Error Resume Next
        Kill(Application.StartupPath & "\Unzip\SubFolderTest\*.*")
        Kill(Application.StartupPath & "\Unzip\*.*")
        Directory.Delete(Application.StartupPath & "\Unzip\SubFolderTest\")
        Directory.Delete(Application.StartupPath & "\Unzip\")

    End Sub

    Private Sub TestAllFileCompressionLevels()

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

        If flgStringPassed Then
            UpdateStatus("All ASCII File Tests Passed!")
        Else
            UpdateStatus("*** ASCII FILE TEST FAILED ***")
        End If

        If flgBinaryPassed Then
            UpdateStatus("All Binary File Tests Passed!")
        Else
            UpdateStatus("*** BINARY FILE TEST FAILED ***")
        End If

    End Sub

    Private Function StringTestLevel(ByVal Data As Byte(), ByVal Strength As CompressLevel) As Boolean

        Return TestDataLevel("String", Data, Strength)

    End Function

    Private Function BinaryTestLevel(ByVal Data As Byte(), ByVal Strength As CompressLevel) As Boolean

        Return TestDataLevel("Binary", Data, Strength)

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
        Dim str1 As String = "ASCIITest.txt"
        Dim str2 As String = "ASCIITest.cmp"
        Dim str3 As String = "ASCIITest.unc"
        Dim intCompare, intSLen, intDLen As Integer

        UpdateStatus(vbCrLf & [Enum].GetName(GetType(CompressLevel), Strength) & " ASCII File Compression Test:" & vbCrLf)

        startTime = VB.Timer
        CompressFile(str1, str2, Strength, AddressOf ZipFile_FileProgress)
        UncompressFile(str2, str3, AddressOf ZipFile_FileProgress)
        stopTime = VB.Timer

        intCompare = CompareASCIIFile(str1, str3)

        intSLen = GetFileLength(str1)
        intDLen = GetFileLength(str2)
        UpdateStatus("Compression: " & intSLen & " to " & intDLen & " (" & Format((intSLen - intDLen) / intSLen, "#.00%") & ")")
        UpdateStatus(" Total time: " & SecondsToText(stopTime - startTime, 2))
        UpdateStatus("End Compare = " & intCompare)

        Return (intCompare = 0)

    End Function

    Private Function BinaryFileTestLevel(ByVal Strength As CompressLevel) As Boolean

        Dim startTime As Double
        Dim stopTime As Double
        Dim str1 As String = "BinaryTest.bin"
        Dim str2 As String = "BinaryTest.cmp"
        Dim str3 As String = "BinaryTest.unc"
        Dim intCompare, intSLen, intDLen As Integer

        If Not File.Exists(str1) Then CreateBinaryTestFile(str1)
        If GetFileLength(str1) = 0 Then CreateBinaryTestFile(str1)

        UpdateStatus(vbCrLf & [Enum].GetName(GetType(CompressLevel), Strength) & " Binary File Compression Test:" & vbCrLf)

        startTime = VB.Timer
        CompressFile(str1, str2, Strength, AddressOf ZipFile_FileProgress)
        UncompressFile(str2, str3, AddressOf ZipFile_FileProgress)
        stopTime = VB.Timer

        intCompare = CompareBinaryFile(str1, str3)

        intSLen = GetFileLength(str1)
        intDLen = GetFileLength(str2)
        UpdateStatus("Compression: " & intSLen & " to " & intDLen & " (" & Format((intSLen - intDLen) / intSLen, "#.00%") & ")")
        UpdateStatus(" Total time: " & SecondsToText(stopTime - startTime, 2))
        UpdateStatus("End Compare = " & intCompare)

        Return (intCompare = 0)

    End Function

    ' Slightly compressable binary data generator...
    Private Function GenSlightlyCompressableBinaryData(ByVal TotalBytes As Integer) As Byte()

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

    Private Function GenRandomBinaryData(ByVal TotalBytes As Integer) As Byte()

        Dim x As Integer
        Dim Data(TotalBytes - 1) As Byte

        Rnd(-1)
        Randomize()

        For x = 0 To TotalBytes - 1
            Data(x) = CByte(Int(Rnd() * 256))
        Next

        Return Data

    End Function

    Private Function CoinToss() As Boolean

        Return CInt(Int(2 * Rnd() + 1)) = 1

    End Function

    Private Sub CreateBinaryTestFile(ByVal FileName As String)

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
            UpdateStatus("WARNING: data size mismatch!!!")
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
                    UpdateStatus("Error encountered during binary file comparision: file stream corruption!")
                    Stop
                End If

                intCompare = CompareData(fs1Buffer, fs2Buffer)
                If intCompare <> 0 Then Exit While

                intF1Read = fs1.Read(fs1Buffer, 0, 2048)
                intF2Read = fs2.Read(fs2Buffer, 0, 2048)
            End While

            fs1.Close()
            fs2.Close()
        End If

        Return intCompare

    End Function

    Private Sub ZipFile_CurrentFile(ByVal FullFileName As String, ByVal RelativeFileName As String) Handles ZipFile.CurrentFile

        UpdateStatus("Current Zip File: " & RelativeFileName)

    End Sub

    Private Sub ZipFile_FileProgress(ByVal BytesCompleted As Long, ByVal BytesTotal As Long) Handles ZipFile.FileProgress

        With ProgressBar
            .Minimum = 0
            .Maximum = 100
            .Value = BytesCompleted / BytesTotal * 100
        End With
        Application.DoEvents()

    End Sub

End Class