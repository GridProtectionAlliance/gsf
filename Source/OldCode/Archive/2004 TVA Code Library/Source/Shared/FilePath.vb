' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Threading
Imports System.Math
Imports System.Text
Imports System.Text.RegularExpressions
Imports TVA.Shared.Common
Imports VB = Microsoft.VisualBasic

Namespace [Shared]

    ' Common File/Path Functions
    Public Class [FilePath]

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Returns True if specified file name matches given file spec (wildcards are defined as '*' or '?' characters)
        Public Shared Function IsFilePatternMatch(ByVal FileSpec As String, ByVal FileName As String, Optional ByVal IgnoreCase As Boolean = True) As Boolean

            Return (New Regex(GetFilePatternRegularExpression(FileSpec), IIf(IgnoreCase, RegexOptions.IgnoreCase, RegexOptions.None))).IsMatch(FileName)

        End Function

        ' Returns True if specified file name matches any of the given file specs (wildcards are defined as '*' or '?' characters)
        Public Shared Function IsFilePatternMatch(ByVal FileSpecs As String(), ByVal FileName As String) As Boolean

            Dim Found As Boolean

            For Each FileSpec As String In FileSpecs
                If IsFilePatternMatch(FileSpec, FileName) Then
                    Found = True
                    Exit For
                End If
            Next

            Return Found

        End Function

        ' Returns a regular expression that simulates wildcard matching for filenames (wildcards are defined as '*' or '?' characters)
        Public Shared Function GetFilePatternRegularExpression(ByVal FileSpec As String) As String

            Static FileNameCharPattern As String
            Dim FilePattern As String

            If Len(FileNameCharPattern) = 0 Then
                With New StringBuilder
                    ' Define a regular expression pattern for a valid file name character, we do this by
                    ' allowing any characters except those that would not be valid as part of a filename,
                    ' this essentially builds the "?" wildcard pattern match
                    .Append("[^")
                    .Append(GetRegexUnicodeChar(Path.DirectorySeparatorChar))
                    .Append(GetRegexUnicodeChar(Path.AltDirectorySeparatorChar))
                    .Append(GetRegexUnicodeChar(Path.PathSeparator))
                    .Append(GetRegexUnicodeChar(Path.VolumeSeparatorChar))

                    For Each c As Char In Path.InvalidPathChars
                        .Append(GetRegexUnicodeChar(c))
                    Next

                    .Append("]")
                    FileNameCharPattern = .ToString()
                End With
            End If

            FilePattern = Replace(FileSpec, "\", "\u005C")      ' Backslash in Regex means special sequence, here we really want a backslash
            FilePattern = Replace(FilePattern, ".", "\u002E")   ' Dot in Regex means any character, here we really want a dot
            FilePattern = Replace(FilePattern, "?", FileNameCharPattern)

            Return "^" & Replace(FilePattern, "*", "(" & FileNameCharPattern & ")*") & "$"

        End Function

        ' Get the length of the specified file in bytes
        Public Shared Function GetFileLength(ByVal FileName As String) As Long

            Try
                With New FileInfo(FileName)
                    Return .Length
                End With
            Catch
                Return -1
            End Try

        End Function

        ' Gets a unique temporary file name with path - if UseTempPath is False, application path is used for temp file
        Public Shared Function GetTempFile(Optional ByVal UseTempPath As Boolean = True, Optional ByVal CreateZeroLengthFile As Boolean = True, Optional ByVal FileExtension As String = "tmp") As String

            If UseTempPath And CreateZeroLengthFile Then
                Dim strTempFile As String = GetTempFilePath() & GetTempFileName(FileExtension)
                With File.Create(strTempFile)
                    .Close()
                End With
                Return strTempFile
            ElseIf UseTempPath Then
                Return GetTempFilePath() & GetTempFileName(FileExtension)
            ElseIf CreateZeroLengthFile Then
                Dim strTempFile As String = GetApplicationPath() & GetTempFileName(FileExtension)
                With File.Create(strTempFile)
                    .Close()
                End With
                Return strTempFile
            Else
                Return GetApplicationPath() & GetTempFileName(FileExtension)
            End If

        End Function

        ' Gets a file name guaranteed to be unique with no path - use GetTempFile to return unique file name with path
        Public Shared Function GetTempFileName(Optional ByVal FileExtension As String = "tmp") As String

            If Left(FileExtension, 1) = "." Then FileExtension = FileExtension.Substring(1)
            Return Guid.NewGuid.ToString() & "." & FileExtension

        End Function

        ' Gets the temporary file path - path will be suffixed with standard directory separator
        Public Shared Function GetTempFilePath() As String

            Return AddPathSuffix(Path.GetTempPath())

        End Function

        ' Gets the path of the executing assembly - path will be suffixed with standard directory separator
        Public Shared Function GetApplicationPath() As String

            Return JustPath(Trim(System.Reflection.Assembly.GetExecutingAssembly.Location))

        End Function

        ' Returns just the drive letter (or UNC \\server\share\) from a path") - path will be suffixed with standard directory separator
        Public Shared Function JustDrive(ByVal FilePath As String) As String

            'Dim lngParamPos As Long

            'If Len(FilePath) > 0 Then
            '    lngParamPos = InStr(FilePath, "\\")
            '    If lngParamPos > 0 Then
            '        lngParamPos = InStr(lngParamPos + 2, FilePath, "\", 0)
            '        If lngParamPos > 0 Then
            '            lngParamPos = InStr(lngParamPos + 1, FilePath, "\", 0)
            '            If lngParamPos > 0 Then FilePath = Left$(FilePath, lngParamPos - 1)
            '        End If
            '    Else
            '        lngParamPos = InStr(FilePath, ":")
            '        If lngParamPos > 0 Then
            '            FilePath = Left(FilePath, lngParamPos)
            '        Else
            '            FilePath = ""
            '        End If
            '    End If
            'End If

            'Return FilePath

            If Len(FilePath) > 0 Then
                Return AddPathSuffix(Path.GetPathRoot(FilePath))
            Else
                Return Path.DirectorySeparatorChar
            End If

        End Function

        ' Returns just the file name from a path
        Public Shared Function JustFileName(ByVal FilePath As String) As String

            '' Remove path from FilePath
            'Do While InStr(FilePath, "\") <> 0 Or InStr(FilePath, "/") <> 0 Or InStr(FilePath, ":") <> 0
            '    FilePath = Right(FilePath, Len(FilePath) - 1)
            'Loop

            'Return FilePath

            If Len(FilePath) > 0 Then
                Return Path.GetFileName(FilePath)
            Else
                Return ""
            End If

        End Function

        ' Returns last directory name from a path (e.g., would return sub2 from c:\windows\sub2\filename.ext)
        Public Shared Function LastDirectoryName(ByVal FilePath As String) As String

            Dim DirChars As Char() = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
            Dim DirVolChars As Char() = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar}

            FilePath = JustPath(FilePath)

            Do While Right(FilePath, 1).IndexOfAny(DirChars) > -1
                FilePath = Left(FilePath, Len(FilePath) - 1)
            Loop

            Do While FilePath.IndexOfAny(DirVolChars) > -1
                FilePath = Right(FilePath, Len(FilePath) - 1)
            Loop

            Return FilePath

        End Function

        ' Returns just the path without a filename from a path - path will be suffixed with standard directory separator
        Public Shared Function JustPath(ByVal FilePath As String) As String

            '' Find the file portion of the path and return what's left
            'If Len(FilePath) > 0 Then
            '    Return Left(FilePath, Len(FilePath) - Len(JustFilePath(FilePath)))
            'Else
            '    Return ""
            'End If

            If Len(FilePath) > 0 Then
                Return Path.GetDirectoryName(FilePath) & Path.DirectorySeparatorChar
            Else
                Return Path.DirectorySeparatorChar
            End If

        End Function

        ' Returns just the file extension from a path - keeps extension "dot"
        Public Shared Function JustFileExtension(ByVal FilePath As String) As String

            'Dim intDotPos As Long

            '' Remove any path from filename
            'FilePath = JustFilePath(FilePath)

            'intDotPos = InStrRev(FilePath, ".")

            'If intDotPos > 0 Then
            '    Return Mid(FilePath, intDotPos)
            'Else
            '    Return ""
            'End If

            If Len(FilePath) > 0 Then
                Return Path.GetExtension(FilePath)
            Else
                Return ""
            End If

        End Function

        ' Returns just the file name with no extension from a path
        Public Shared Function NoFileExtension(ByVal FilePath As String) As String

            'Dim intDotPos As Long

            '' Remove any path from filename
            'FilePath = JustFilePath(FilePath)

            'intDotPos = InStrRev(FilePath, ".")

            'If intDotPos > 0 Then
            '    Return Left(FilePath, intDotPos - 1)
            'Else
            '    Return FilePath
            'End If

            If Len(FilePath) > 0 Then
                Return Path.GetFileNameWithoutExtension(FilePath)
            Else
                Return ""
            End If

        End Function

        ' Returns True if given path exists
        Public Shared Function PathExists(ByVal FilePath As String) As Boolean

            'Dim flgExists As Boolean

            'If Len(FilePath) > 0 Then
            '    Try
            '        flgExists = (Len(Dir(FilePath, vbDirectory)) > 0)
            '    Catch ex As Exception
            '        flgExists = False
            '    End Try
            'Else
            '    flgExists = False
            'End If

            'Return flgExists

            Return Directory.Exists(FilePath)

        End Function

        ' Makes sure path is suffixed with standard directory separator
        Public Shared Function AddPathSuffix(ByVal FilePath As String) As String

            If Len(FilePath) > 0 Then
                Dim SuffixChar As Char = FilePath.Chars(FilePath.Length - 1)
                If SuffixChar <> Path.DirectorySeparatorChar And SuffixChar <> Path.AltDirectorySeparatorChar Then
                    FilePath &= Path.DirectorySeparatorChar
                End If
            Else
                FilePath = Path.DirectorySeparatorChar
            End If

            Return FilePath

        End Function

        ' Makes sure path is not suffixed with any directory separator
        Public Shared Function RemovePathSuffix(ByVal FilePath As String) As String

            If Len(FilePath) > 0 Then
                Dim SuffixChar As Char = FilePath.Chars(FilePath.Length - 1)
                While (SuffixChar = Path.DirectorySeparatorChar Or SuffixChar = Path.AltDirectorySeparatorChar) And FilePath.Length > 0
                    FilePath = FilePath.Substring(0, FilePath.Length - 1)
                    If FilePath.Length > 0 Then SuffixChar = FilePath.Chars(FilePath.Length - 1)
                End While
            End If

            Return FilePath

        End Function

        ' Returns a file name for display purposes of the specified length using "..." to indicate larger name
        Public Shared Function TrimFileName(ByVal FileName As String, ByVal Length As Integer) As String

            FileName = Trim(FileName)

            If Length < 12 Then
                Throw New InvalidOperationException("Cannot trim file names to less than 12 characters...")
            ElseIf Len(FileName) > Length Then
                Dim strJustFileName As String = JustFileName(FileName)

                If Len(strJustFileName) = Len(FileName) Then
                    ' This is just a file name, make sure extension shows...
                    Dim strJustExt As String = JustFileExtension(FileName)
                    Dim strTrimName As String = NoFileExtension(FileName)

                    If Len(strTrimName) > 8 Then
                        If Len(strJustExt) > Length - 8 Then strJustExt = Left(strJustExt, Length - 8)
                        Dim sngOffset As Single = (Length - Len(strJustExt) - 3) / 2
                        Return Left(strTrimName, CInt(Ceiling(sngOffset))) & "..." & Mid(strTrimName, Len(strTrimName) - CInt(Floor(sngOffset)) + 1) & strJustExt
                    Else
                        ' We can't trim file names less than 8 with a "...", so we truncate long extension
                        Return strTrimName & Left(strJustExt, Length - Len(strTrimName))
                    End If
                ElseIf Len(strJustFileName) > Length Then
                    ' Just file name part exeeds length, recurse into function without path
                    Return TrimFileName(strJustFileName, Length)
                Else
                    ' File name contains path, trim path before file name...
                    Dim strJustPath As String = JustPath(FileName)
                    Dim intOffset As Integer = Length - Len(strJustFileName) - 4

                    If Len(strJustPath) > intOffset And intOffset > 0 Then
                        Return Left(strJustPath, intOffset) & "...\" & strJustFileName
                    Else
                        ' Can't fit path, just trim file name
                        Return TrimFileName(strJustFileName, Length)
                    End If
                End If
            Else
                ' Full file name fits within requested length...
                Return FileName
            End If

        End Function

        ' Gets a list of files for the given path and wildcard pattern (e.g., "c:\*.*")
        Public Shared Function GetFileList(ByVal Selection As String) As String()

            'Dim colFiles As New ArrayList()
            'Dim strFile As String

            'strFile = Dir(Selection, vbNormal)

            'Do Until Len(strFile) = 0
            '    colFiles.Add(strFile)
            '    strFile = Dir()
            'Loop

            'Return colFiles

            Return Directory.GetFiles(JustPath(Selection), JustFileName(Selection))

        End Function

        ' Waits specified number of seconds for read access on a file, set SecondsToWait to zero to wait infinitely
        Public Shared Sub WaitForReadLock(ByVal FileName As String, Optional ByVal SecondsToWait As Integer = 5)

            If Not File.Exists(FileName) Then
                Throw New FileNotFoundException("Could not test file lock for """ & FileName & """, file does not exist", FileName)
                Exit Sub
            End If

            ' We use this function to keep trying for a file lock...
            Dim fs As FileStream
            Dim dblStart As Double = VB.Timer

            While True
                Try
                    fs = File.OpenRead(FileName)
                    fs.Close()
                    Exit While
                Catch
                    ' We'll keep trying till we can open the file...
                End Try

                If Not fs Is Nothing Then
                    Try
                        fs.Close()
                    Catch
                    End Try
                    fs = Nothing
                End If

                If SecondsToWait > 0 Then
                    If VB.Timer > dblStart + CDbl(SecondsToWait) Then
                        Throw New IOException("Could not open """ & FileName & """ for read access, tried for " & SecondsToWait & " seconds")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(0)
            End While

        End Sub

        ' Waits specified number of seconds for write access on a file, set SecondsToWait to zero to wait infinitely
        Public Shared Sub WaitForWriteLock(ByVal FileName As String, Optional ByVal SecondsToWait As Integer = 5)

            If Not File.Exists(FileName) Then
                Throw New FileNotFoundException("Could not test file lock for """ & FileName & """, file does not exist", FileName)
                Exit Sub
            End If

            ' We use this function to keep trying for a file lock...
            Dim fs As FileStream
            Dim dblStart As Double = VB.Timer

            While True
                Try
                    fs = File.OpenWrite(FileName)
                    fs.Close()
                    Exit While
                Catch
                    ' We'll keep trying till we can open the file...
                End Try

                If Not fs Is Nothing Then
                    Try
                        fs.Close()
                    Catch
                    End Try
                    fs = Nothing
                End If

                If SecondsToWait > 0 Then
                    If VB.Timer > dblStart + CDbl(SecondsToWait) Then
                        Throw New IOException("Could not open """ & FileName & """ for write access, tried for " & SecondsToWait & " seconds")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(0)
            End While

        End Sub

        ' Waits specified number of seconds for a file to exist, set SecondsToWait to zero to wait infinitely
        Public Shared Sub WaitTillExists(ByVal FileName As String, Optional ByVal SecondsToWait As Integer = 5)

            ' We use this function to keep waiting for a file to be created...
            Dim dblStart As Double = VB.Timer

            While Not File.Exists(FileName)
                If SecondsToWait > 0 Then
                    If VB.Timer > dblStart + CDbl(SecondsToWait) Then
                        Throw New IOException("Waited for """ & FileName & """ to exist for " & SecondsToWait & " seconds, but it was never created")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(0)
            End While

        End Sub

    End Class

End Namespace