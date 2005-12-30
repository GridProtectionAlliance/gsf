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

        ''' <summary>
        ''' Get the size of the specified file.
        ''' </summary>
        ''' <param name="fileName">Name of file whose size is to be returned.</param>
        ''' <returns>The size of the specified file.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetFileLength(ByVal fileName As System.String) As Long

            Try
                With New FileInfo(FileName)
                    Return .Length()
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

        ''' <summary>
        ''' Returns a file name for display purposes of the specified length using "..." to indicate larger name.
        ''' </summary>
        ''' <param name="fileName">To be provided.</param>
        ''' <param name="length">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function TrimFileName(ByVal fileName As String, ByVal length As Integer) As String

            fileName = Trim(fileName)

            If length < 12 Then
                Throw New InvalidOperationException("Cannot trim file names to less than 12 characters...")
            ElseIf Len(fileName) > length Then
                Dim strJustFileName As String = JustFileName(fileName)

                If Len(strJustFileName) = Len(fileName) Then
                    ' This is just a file name, make sure extension shows...
                    Dim strJustExt As String = JustFileExtension(fileName)
                    Dim strTrimName As String = NoFileExtension(fileName)

                    If Len(strTrimName) > 8 Then
                        If Len(strJustExt) > length - 8 Then strJustExt = Left(strJustExt, length - 8)
                        Dim sngOffset As Single = (length - Len(strJustExt) - 3) / 2
                        Return Left(strTrimName, CInt(Ceiling(sngOffset))) & "..." & Mid(strTrimName, Len(strTrimName) - CInt(Floor(sngOffset)) + 1) & strJustExt
                    Else
                        ' We can't trim file names less than 8 with a "...", so we truncate long extension
                        Return strTrimName & Left(strJustExt, length - Len(strTrimName))
                    End If
                ElseIf Len(strJustFileName) > length Then
                    ' Just file name part exeeds length, recurse into function without path
                    Return TrimFileName(strJustFileName, length)
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

        ''' <summary>
        ''' Gets a list of files for the given path and wildcard pattern (e.g., "c:\*.*").
        ''' </summary>
        ''' <param name="selectionPath">To be provided.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetFileList(ByVal selectionPath As String) As String()

            Return Directory.GetFiles(JustPath(selectionPath), JustFileName(selectionPath))

        End Function

        ''' <summary>
        ''' Waits for the default duration (5 seconds) for read access on a file.
        ''' </summary>
        ''' <param name="fileName">The name of the file to wait for to obtain read access.</param>
        ''' <remarks></remarks>
        Public Shared Sub WaitForReadLock(ByVal fileName As String)

            WaitForReadLock(fileName, 5)

        End Sub

        ''' <summary>
        ''' Waits for read access on a file for the specified number of seconds.
        ''' </summary>
        ''' <param name="fileName">The name of the file to wait for to obtain read access.</param>
        ''' <param name="secondsToWait">The time to wait for in seconds to obtain read access on a file.</param>
        ''' <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
        Public Shared Sub WaitForReadLock(ByVal fileName As String, ByVal secondsToWait As Integer)

            If Not File.Exists(fileName) Then
                Throw New FileNotFoundException("Could not test file lock for """ & fileName & """, file does not exist", fileName)
                Exit Sub
            End If

            ' We use this function to keep trying for a file lock...
            Dim targetFile As FileStream = Nothing
            Dim startTime As Double = VB.Timer

            While True
                Try
                    targetFile = File.OpenRead(fileName)
                    targetFile.Close()
                    Exit While
                Catch
                    ' We'll keep trying till we can open the file...
                End Try

                If Not targetFile Is Nothing Then
                    Try
                        targetFile.Close()
                    Catch
                    End Try
                    targetFile = Nothing
                End If

                If secondsToWait > 0 Then
                    If VB.Timer > startTime + CDbl(secondsToWait) Then
                        Throw New IOException("Could not open """ & fileName & """ for read access, tried for " & secondsToWait & " seconds")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(0)
            End While

        End Sub

        ''' <summary>
        ''' Waits for the default duration (5 seconds) for write access on a file.
        ''' </summary>
        ''' <param name="fileName">The name of the file to wait for to obtain write access.</param>
        ''' <remarks></remarks>
        Public Shared Sub WaitForWriteLock(ByVal fileName As String)

            WaitForWriteLock(fileName, 5)

        End Sub

        ''' <summary>
        ''' Waits for write access on a file for the specified number of seconds.
        ''' </summary>
        ''' <param name="fileName">The name of the file to wait for to obtain write access.</param>
        ''' <param name="secondsToWait">The time to wait for in seconds to obtain write access on a file.</param>
        ''' <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
        Public Shared Sub WaitForWriteLock(ByVal fileName As String, ByVal secondsToWait As Integer)

            If Not File.Exists(fileName) Then
                Throw New FileNotFoundException("Could not test file lock for """ & fileName & """, file does not exist", fileName)
                Exit Sub
            End If

            ' We use this function to keep trying for a file lock...
            Dim targetFile As FileStream = Nothing
            Dim startTime As Double = VB.Timer

            While True
                Try
                    targetFile = File.OpenWrite(fileName)
                    targetFile.Close()
                    Exit While
                Catch
                    ' We'll keep trying till we can open the file...
                End Try

                If Not targetFile Is Nothing Then
                    Try
                        targetFile.Close()
                    Catch
                    End Try
                    targetFile = Nothing
                End If

                If secondsToWait > 0 Then
                    If VB.Timer > startTime + CDbl(secondsToWait) Then
                        Throw New IOException("Could not open """ & fileName & """ for write access, tried for " & secondsToWait & " seconds")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(0)
            End While

        End Sub

        ''' <summary>
        ''' Waits for the default duration (5 seconds) for a file to exist.
        ''' </summary>
        ''' <param name="fileName">The name of the file to wait for until it is created.</param>
        ''' <remarks></remarks>
        Public Shared Sub WaitTillExists(ByVal fileName As String)

            WaitTillExists(fileName, 5)

        End Sub

        ''' <summary>
        ''' Waits for a file to exist for the specified number of seconds.
        ''' </summary>
        ''' <param name="fileName">The name of the file to wait for until it is created.</param>
        ''' <param name="secondsToWait">The time to wait for in seconds for the file to be created.</param>
        ''' <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
        Public Shared Sub WaitTillExists(ByVal fileName As String, ByVal secondsToWait As Integer)

            ' We use this function to keep waiting for a file to be created...
            Dim startTime As Double = VB.Timer

            While Not File.Exists(fileName)
                If secondsToWait > 0 Then
                    If VB.Timer > startTime + CDbl(secondsToWait) Then
                        Throw New IOException("Waited for """ & fileName & """ to exist for " & secondsToWait & " seconds, but it was never created")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(0)
            End While

        End Sub

    End Class

End Namespace
