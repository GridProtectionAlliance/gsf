Imports System.Text
Imports System.IO
Imports TVA.Compression
Imports TVA.Compression.Common
Imports TVA.Compression.ZipFile
Imports TVA.Shared.String
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Testing
Imports VB = Microsoft.VisualBasic

Public Class ZipFileComponentTest

    Inherits TestBase

    Private WithEvents ZipFile As TVA.Compression.ZipFile
    Private Const BufferSize As Integer = 262144

    Public Sub New()

        ZipFile = New TVA.Compression.ZipFile
        With ZipFile
            .AutoRefresh = True
            .CaseSensitive = False
            .Strength = CompressLevel.DefaultCompression
        End With

    End Sub

    Public Sub TestAllZipCompressionLevelsWithoutPassword()

        UpdateStatus(vbCrLf & "Testing ZipFile.Add(""Binary.*""), recursive, all compression levels" & vbCrLf & "   without a zip file password..." & vbCrLf)
        Assert(TestAllZipCompressionLevels(Logger.TestPath & "NoPasswordTest.zip", ""), "Test of all Zip file compression levels without a password")

    End Sub

    Public Sub TestAllZipCompressionLevelsWithPassword()

        UpdateStatus(vbCrLf & "Testing ZipFile.Add(""Binary.*""), recursive, all compression levels" & vbCrLf & "   with a zip file password..." & vbCrLf)
        Assert(TestAllZipCompressionLevels(Logger.TestPath & "PasswordTest.zip", "password"), "Test of all Zip file compression levels with a password")

    End Sub

    Public Sub TestCompressedFileCollectionTraversion()
        
        UpdateStatus(vbCrLf & "Testing zip file compressed file collection:" & vbCrLf)
        Dim cmpFile As ZipFile.CompressedFile
        Dim x As Integer

        Try
            ZipFile.Close()
            ZipFile.FileName = Logger.TestPath & "NoPasswordTest.zip"
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
            Assert(True, "Compressed File Collection Traversion")
        Catch ex As Exception
            Assert(False, "Compressed File Collection Traversion", ex.Message)
        Finally
            ZipFile.Close()
        End Try

    End Sub

    Private Function TestAllZipCompressionLevels(ByVal FileName As String, ByVal Password As String) As Boolean

        Dim flgAllLevelsPassed As Boolean

        ValidateTestFoldersAndFiles()

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

        Return flgAllLevelsPassed

    End Function

    Private Function TestZipFileCompressionLevel(ByVal Strength As CompressLevel) As Boolean

        Dim intCompare As Integer
        Dim str1, str2 As String
        Dim strStrength As String = [Enum].GetName(GetType(CompressLevel), Strength)

        If ZipFile.IsOpen Then ZipFile.Close()
        If File.Exists(ZipFile.FileName) Then File.Delete(ZipFile.FileName)

        RemoveAnyExistingUnzippedFiles()

        ZipFile.Open()
        ZipFile.Strength = Strength

        UpdateStatus(vbCrLf & strStrength & " ""Recursive Add"" Zip File Compression Test:" & vbCrLf)
        ZipFile.Add(Logger.TestPath & "BinaryTest.*", True, PathInclusion.RelativePath)
        ZipFile.Add(Logger.TestPath & "ASCIITest.txt")

        UpdateStatus(vbCrLf & """Extract"" Zip File Compression Test:" & vbCrLf)
        Try
            ZipFile.Extract("BinaryTest.*", Logger.TestPath & "Unzip\", UpdateOption.Always, PathInclusion.RelativePath)
            Assert(True, """Extract"" Zip file compression test at compression level: " & strStrength)
        Catch ex As Exception
            Assert(False, """Extract"" Zip file compression test at compression level: " & strStrength, ex.Message)
        End Try

        ZipFile.Close()

        UpdateStatus(vbCrLf & """Update"" Zip File Compression Test:" & vbCrLf)
        Try
            ZipFile.Update(Logger.TestPath & "BinaryTest.*", UpdateOption.Always, False, True, PathInclusion.RelativePath)
            Assert(True, """Update"" Zip file compression test at compression level: " & strStrength)
        Catch ex As Exception
            Assert(False, """Update"" Zip file compression test at compression level: " & strStrength, ex.Message)
        End Try

        UpdateStatus(vbCrLf & """Remove"" Zip File Compression Test:" & vbCrLf)
        Try
            ZipFile.Remove("*.txt")
            Assert(True, """Remove"" Zip file compression test at compression level: " & strStrength)
        Catch ex As Exception
            Assert(False, """Remove"" Zip file compression test at compression level: " & strStrength, ex.Message)
        End Try

        ZipFile.Close()

        UpdateStatus(vbCrLf & "Validating extractions to source..." & vbCrLf)
        Try
            For Each str1 In Directory.GetFiles(Logger.TestPath, "BinaryTest.*")
                str2 = JustPath(str1) & "Unzip\" & JustFileName(str1)
                UpdateStatus("Comparing """ & JustFileName(str1) & """ to """ & JustFileName(str2) & """...")
                intCompare = CompareBinaryFile(str1, str2)
                If intCompare <> 0 Then Exit For
            Next

            If intCompare = 0 Then
                For Each str1 In Directory.GetFiles(Logger.TestPath & "SubFolderTest\", "BinaryTest.*")
                    str2 = Logger.TestPath & "Unzip\SubFolderTest\" & JustFileName(str1)
                    UpdateStatus("Comparing """ & JustFileName(str1) & """ to """ & JustFileName(str2) & """...")
                    intCompare = CompareBinaryFile(str1, str2)
                    If intCompare <> 0 Then Exit For
                Next
            End If

            Assert((intCompare = 0), "Extracted files compared to source files at compression level " & Strength & " (" & strStrength & ")")
        Catch ex As Exception
            Assert(False, "Extracted files compared to source files at compression level " & Strength & " (" & strStrength & ")", ex.Message)
        End Try

        UpdateStatus("End Compare = " & intCompare)

        Return (intCompare = 0)

    End Function

    Private Sub RemoveAnyExistingUnzippedFiles()

        On Error Resume Next
        Kill(Logger.TestPath & "Unzip\SubFolderTest\*.*")
        Kill(Logger.TestPath & "Unzip\*.*")
        Directory.Delete(Logger.TestPath & "Unzip\SubFolderTest\")
        Directory.Delete(Logger.TestPath & "Unzip\")

    End Sub

    Private Sub ValidateTestFoldersAndFiles()

        If Not Directory.Exists(Logger.TestPath & "SubFolderTest\") Then Directory.CreateDirectory(Logger.TestPath & "SubFolderTest\")

        If File.Exists(Logger.TestPath & "BinaryTest.bin") Then
            For x As Integer = 1 To 3
                If Not File.Exists(Logger.TestPath & "SubFolderTest\BinaryTest.bin" & x) Then File.Copy(Logger.TestPath & "BinaryTest.bin", Logger.TestPath & "SubFolderTest\BinaryTest.bin" & x)
            Next
        End If

    End Sub

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

    Private Sub UpdateStatus(ByVal Status As String, Optional ByVal Tag As String = Nothing)

        LogEvent(EventType.Status, Tag, Status)

    End Sub

    Private Sub ZipFile_CurrentFile(ByVal FullFileName As String, ByVal RelativeFileName As String) Handles ZipFile.CurrentFile

        UpdateStatus("Current Zip File: " & RelativeFileName)

    End Sub

    Private Sub ZipFile_FileProgress(ByVal BytesCompleted As Long, ByVal BytesTotal As Long) Handles ZipFile.FileProgress

        ' Want to give up time (DoEvents) so the app responds better
        UpdateTestProgress(BytesCompleted, BytesTotal)

    End Sub

End Class
