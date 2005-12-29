'*******************************************************************************************************
'  Tva.IO.FilePath.vb - File/Path Manipulation Functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  ??/??/2003 - James R Carroll
'       Original version of source code generated
'  12/29/2005 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.FilePath)
'
'*******************************************************************************************************

Imports System.IO
Imports System.Threading
Imports System.Math
Imports System.Text
Imports System.Text.RegularExpressions
Imports VB = Microsoft.VisualBasic

Namespace IO

    Public Class FilePath

        Private Sub New()
            ' This class contains only global functions and is not meant to be instantiated
        End Sub

        ''' <summary>
        ''' Returns True if specified file name matches given file spec (wildcards are defined as '*' or '?' characters).
        ''' </summary>
        ''' <param name="matchPattern">To be provided.</param>
        ''' <param name="fileName">To be provided.</param>
        ''' <param name="ignoreCase">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function IsFilePatternMatch(ByVal matchPattern As String, ByVal fileName As String, ByVal ignoreCase As Boolean) As Boolean

            Return (New Regex(GetFilePatternRegularExpression(matchPattern), IIf(ignoreCase, RegexOptions.IgnoreCase, RegexOptions.None))).IsMatch(fileName)

        End Function

        ''' <summary>
        ''' Returns True if specified file name matches any of the given file specs (wildcards are defined as '*' or '?' characters).
        ''' </summary>
        ''' <param name="matchPatterns">To be provided.</param>
        ''' <param name="fileName">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function IsFilePatternMatch(ByVal matchPatterns As String(), ByVal fileName As String) As Boolean

            Dim Found As Boolean

            For Each FileSpec As String In matchPatterns
                If IsFilePatternMatch(FileSpec, fileName, True) Then
                    Found = True
                    Exit For
                End If
            Next

            Return Found

        End Function

        ''''<summary>
        ''''  <para>Returns a regular expression that simulates wildcard matching for filenames (wildcards are defined as '*' or '?' characters)</para>
        ''''</summary>
        ''''<param name="FileSpec"> Required. File spec . </param>
        Public Shared Function GetFilePatternRegularExpression(ByVal FileSpec As String) As String

            Static FileNameCharPattern As System.String
            Dim FilePattern As System.String

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

                    For Each c As Char In Path.GetInvalidPathChars()
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

        ''''<summary>
        ''''  <para>Get the length of the specified file in bytes</para>
        ''''</summary>
        ''''<param name="FileSpec"> Required. File Name . </param>
        Public Shared Function GetFileLength(ByVal FileName As System.String) As Long

            Try
                With New FileInfo(FileName)
                    Return .Length
                End With
            Catch
                Return -1
            End Try

        End Function

        ''''<summary>
        ''''  <para> Gets a unique temporary file name with path - if UseTempPath is False, application path is used for temp file</para>
        ''''</summary>
        ''''<param name="UseTempPath"> Optional.default boolean value set to true</param>
        '''' <param name="CreateZeroLengthFile"> Optional.default boolean value set to true</param>
        '''' <param name="FileExtension"> Optional.File Extension, default value set to tmp</param>
        Public Shared Function GetTempFile(Optional ByVal UseTempPath As Boolean = True, Optional ByVal CreateZeroLengthFile As Boolean = True, Optional ByVal FileExtension As System.String = "tmp") As String

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

        ''''<summary>
        ''''  <para>Gets a file name guaranteed to be unique with no path - use GetTempFile to return unique file name with path</para>
        ''''</summary>
        '''' <param name="FileExtension"> Optional.File Extension, default value set to tmp</param>
        Public Shared Function GetTempFileName(Optional ByVal FileExtension As System.String = "tmp") As String

            If Left(FileExtension, 1) = "." Then FileExtension = FileExtension.Substring(1)
            Return Guid.NewGuid.ToString() & "." & FileExtension

        End Function

        ''''<summary>
        ''''  <para>Gets the temporary file path - path will be suffixed with standard directory separator</para>
        ''''</summary>
        Public Shared Function GetTempFilePath() As String

            Return AddPathSuffix(Path.GetTempPath())

        End Function

        ''''<summary>
        ''''  <para> Gets the path of the executing assembly - path will be suffixed with standard directory separator</para>
        ''''</summary>
        Public Shared Function GetApplicationPath() As String

            Return JustPath(Trim(System.Reflection.Assembly.GetExecutingAssembly.Location))

        End Function

        ''''<summary>
        ''''  <para>  Returns just the drive letter (or UNC \\server\share\) from a path") - path will be suffixed with standard directory separator</para>
        ''''</summary>
        '''' <param name="FilePath">File Path</param>
        Public Shared Function JustDrive(ByVal FilePath As System.String) As String

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

        ''''<summary>
        ''''  <para> Returns just the file name from a path</para>
        ''''</summary>
        '''' <param name="FilePath"> File Path</param>
        Public Shared Function JustFileName(ByVal FilePath As System.String) As String

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

        ''''<summary>
        ''''  <para> Returns last directory name from a path (e.g., would return sub2 from c:\windows\sub2\filename.ext)</para>
        ''''</summary>
        '''' <param name="FilePath"> File Path</param>
        Public Shared Function LastDirectoryName(ByVal FilePath As System.String) As String

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

        ''''<summary>
        ''''  <para> Returns just the path without a filename from a path - path will be suffixed with standard directory separator</para>
        ''''</summary>
        '''' <param name="FilePath"> File Path</param>
        Public Shared Function JustPath(ByVal FilePath As System.String) As String

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

        ''''<summary>
        ''''  <para> Returns just the file extension from a path - keeps extension "dot"</para>
        ''''</summary>
        '''' <param name="FilePath"> File Path</param>
        Public Shared Function JustFileExtension(ByVal FilePath As System.String) As String

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

        ''''<summary>
        ''''  <para>  Returns just the file name with no extension from a path</para>
        ''''</summary>
        '''' <param name="FilePath"> File Path</param>
        Public Shared Function NoFileExtension(ByVal FilePath As System.String) As String

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

        ''''<summary>
        ''''  <para>  Returns True if given path exists</para>
        ''''</summary>
        '''' <param name="FilePath"> File Path</param>
        Public Shared Function PathExists(ByVal FilePath As System.String) As Boolean

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

        ''''<summary>
        ''''  <para>  Makes sure path is suffixed with standard directory separator</para>
        ''''</summary>
        '''' <param name="FilePath"> File Path</param>
        Public Shared Function AddPathSuffix(ByVal FilePath As System.String) As String

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

        ''''<summary>
        ''''  <para>  Makes sure path is not suffixed with any directory separator</para>
        ''''</summary>
        '''' <param name="FilePath"> File Path</param>
        Public Shared Function RemovePathSuffix(ByVal FilePath As System.String) As String

            If Len(FilePath) > 0 Then
                Dim SuffixChar As Char = FilePath.Chars(FilePath.Length - 1)
                While (SuffixChar = Path.DirectorySeparatorChar Or SuffixChar = Path.AltDirectorySeparatorChar) And FilePath.Length > 0
                    FilePath = FilePath.Substring(0, FilePath.Length - 1)
                    If FilePath.Length > 0 Then SuffixChar = FilePath.Chars(FilePath.Length - 1)
                End While
            End If

            Return FilePath

        End Function

        ''''<summary>
        ''''  <para> Returns a file name for display purposes of the specified length using "..." to indicate larger name</para>
        ''''</summary>
        '''' <param name="FileName"> File </param>
        '''' <param name="Length"> Filelength </param>
        Public Shared Function TrimFileName(ByVal FileName As System.String, ByVal Length As Integer) As String

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

        ''''<summary>
        ''''  <para>  Gets a list of files for the given path and wildcard pattern (e.g., "c:\*.*")</para>
        ''''</summary>
        '''' <param name="Selection"> Files </param>
        Public Shared Function GetFileList(ByVal Selection As System.String) As String()

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

        ''''<summary>
        ''''  <para>  Waits specified number of seconds for read access on a file, set SecondsToWait to zero to wait infinitely.</para>
        ''''</summary>
        '''' <param name="FileName"> File </param>
        '''' <param name="SecondsToWait">Optional. Waiting time </param>
        Public Shared Sub WaitForReadLock(ByVal FileName As System.String, Optional ByVal SecondsToWait As Integer = 5)

            If Not File.Exists(FileName) Then
                Throw New FileNotFoundException("Could not test file lock for """ & FileName & """, file does not exist", FileName)
                Exit Sub
            End If

            ' We use this function to keep trying for a file lock...
            Dim fs As FileStream = Nothing
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

        ''''<summary>
        ''''  <para> Waits specified number of seconds for write access on a file, set SecondsToWait to zero to wait infinitely.</para>
        ''''</summary>
        '''' <param name="FileName"> File </param>
        '''' <param name="SecondsToWait">Optional. Waiting time </param>
        Public Shared Sub WaitForWriteLock(ByVal FileName As System.String, Optional ByVal SecondsToWait As Integer = 5)

            If Not File.Exists(FileName) Then
                Throw New FileNotFoundException("Could not test file lock for """ & FileName & """, file does not exist", FileName)
                Exit Sub
            End If

            ' We use this function to keep trying for a file lock...
            Dim fs As FileStream = Nothing
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

        ''''<summary>
        ''''  <para>  Waits specified number of seconds for a file to exist, set SecondsToWait to zero to wait infinitely.</para>
        ''''</summary>
        '''' <param name="FileName"> File </param>
        '''' <param name="SecondsToWait">Optional. Waiting time </param>
        Public Shared Sub WaitTillExists(ByVal FileName As System.String, Optional ByVal SecondsToWait As Integer = 5)

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
