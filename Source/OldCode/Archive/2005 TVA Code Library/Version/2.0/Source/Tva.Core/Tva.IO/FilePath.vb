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

        Private Shared m_fileNameCharPattern As String

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

            Dim found As Boolean

            For Each fileSpec As String In matchPatterns
                If IsFilePatternMatch(fileSpec, fileName, True) Then
                    found = True
                    Exit For
                End If
            Next

            Return found

        End Function

        ''''<summary>
        ''''  <para>Returns a regular expression that simulates wildcard matching for filenames (wildcards are defined as '*' or '?' characters)</para>
        ''''</summary>
        ''''<param name="FileSpec"> Required. File spec . </param>
        Public Shared Function GetFilePatternRegularExpression(ByVal fileSpec As String) As String

            Dim filePattern As String

            If m_fileNameCharPattern Is Nothing Then
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
                    m_fileNameCharPattern = .ToString()
                End With
            End If

            filePattern = Replace(fileSpec, "\", "\u005C")      ' Backslash in Regex means special sequence, here we really want a backslash
            filePattern = Replace(filePattern, ".", "\u002E")   ' Dot in Regex means any character, here we really want a dot
            filePattern = Replace(filePattern, "?", m_fileNameCharPattern)

            Return "^" & Replace(filePattern, "*", "(" & m_fileNameCharPattern & ")*") & "$"

        End Function

        ''' <summary>
        ''' Get the size of the specified file.
        ''' </summary>
        ''' <param name="fileName">Name of file whose size is to be returned.</param>
        ''' <returns>The size of the specified file.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetFileLength(ByVal fileName As System.String) As Long

            Try
                With New FileInfo(fileName)
                    Return .Length
                End With
            Catch
                Return -1
            End Try

        End Function

        ''' <summary>
        ''' Gets a unique temporary file name with path.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetTempFile() As String

            Return GetTempFile(True, True, "tmp")

        End Function

        ''' <summary>
        ''' Gets a unique temporary file name with path - if UseTempPath is False, application path is used for temp file.
        ''' </summary>
        ''' <param name="useTempPath">To be provided.</param>
        ''' <param name="createZeroLengthFile">To be provided.</param>
        ''' <param name="fileExtension">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetTempFile(ByVal useTempPath As Boolean, ByVal createZeroLengthFile As Boolean, ByVal fileExtension As String) As String

            If useTempPath And createZeroLengthFile Then
                Dim tempFile As String = GetTempFilePath() & GetTempFileName(fileExtension)
                With File.Create(tempFile)
                    .Close()
                End With
                Return tempFile
            ElseIf useTempPath Then
                Return GetTempFilePath() & GetTempFileName(fileExtension)
            ElseIf createZeroLengthFile Then
                Dim tempFile As String = GetApplicationPath() & GetTempFileName(fileExtension)
                With File.Create(tempFile)
                    .Close()
                End With
                Return tempFile
            Else
                Return GetApplicationPath() & GetTempFileName(fileExtension)
            End If

        End Function

        ''' <summary>
        ''' Gets a file name (with .tmp extension) guaranteed to be unique with no path.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetTempFileName() As String

            Return GetTempFileName("tmp")

        End Function

        ''' <summary>
        ''' Gets a file name guaranteed to be unique with no path.
        ''' </summary>
        ''' <param name="fileExtension">The extension of the temporary file.</param>
        ''' <returns>The file name guaranteed to be unique with no path.</returns>
        ''' <remarks>Use GetTempFile to return unique file name with path.</remarks>
        Public Shared Function GetTempFileName(ByVal fileExtension As String) As String

            If fileExtension.Substring(0, 1) = "." Then fileExtension = fileExtension.Substring(1)
            Return Guid.NewGuid.ToString() & "." & fileExtension

        End Function

        ' TODO: Note for Pinal - I am OK with these being functions instead of readonly properties
        ' and using the "Get" prefix, see .NET example below that uses "GetTempPath" for the .NET
        ' "Path" class.

        ''' <summary>
        ''' Gets the temporary file path - path will be suffixed with standard directory separator.
        ''' </summary>
        ''' <returns>The temporary file path.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetTempFilePath() As String

            Return AddPathSuffix(Path.GetTempPath())

        End Function

        ''' <summary>
        ''' Gets the path of the executing assembly - path will be suffixed with standard directory separator.
        ''' </summary>
        ''' <returns>The path of the executing assembly.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetApplicationPath() As String

            Return JustPath(Trim(System.Reflection.Assembly.GetEntryAssembly.Location()))

        End Function

        ''' <summary>
        ''' Returns just the drive letter (or UNC \\server\share\) from a path") - path will be suffixed with standard directory separator.
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function JustDrive(ByVal filePath As String) As String

            If filePath IsNot Nothing AndAlso filePath.Length > 0 Then
                Return AddPathSuffix(Path.GetPathRoot(filePath))
            Else
                Return Path.DirectorySeparatorChar
            End If

        End Function

        ''' <summary>
        ''' Returns just the file name from a path.
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function JustFileName(ByVal filePath As String) As String

            If filePath IsNot Nothing AndAlso filePath.Length > 0 Then
                Return Path.GetFileName(filePath)
            Else
                Return ""
            End If

        End Function

        ''' <summary>
        ''' Returns last directory name from a path (e.g., would return sub2 from c:\windows\sub2\filename.ext).
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function LastDirectoryName(ByVal filePath As String) As String

            Dim dirChars As Char() = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
            Dim dirVolChars As Char() = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar}

            filePath = JustPath(filePath)

            Do While Right(filePath, 1).IndexOfAny(dirChars) > -1
                filePath = Left(filePath, Len(filePath) - 1)
            Loop

            Do While filePath.IndexOfAny(dirVolChars) > -1
                filePath = Right(filePath, Len(filePath) - 1)
            Loop

            Return filePath

        End Function

        ''' <summary>
        ''' Returns just the path without a filename from a path - path will be suffixed with standard directory separator.
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function JustPath(ByVal filePath As String) As String

            If filePath IsNot Nothing AndAlso filePath.Length > 0 Then
                Return Path.GetDirectoryName(filePath) & Path.DirectorySeparatorChar
            Else
                Return Path.DirectorySeparatorChar
            End If

        End Function

        ''' <summary>
        ''' Returns just the file extension from a path - keeps extension "dot".
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function JustFileExtension(ByVal filePath As String) As String

            If filePath IsNot Nothing AndAlso filePath.Length > 0 Then
                Return Path.GetExtension(filePath)
            Else
                Return ""
            End If

        End Function

        ''' <summary>
        ''' Returns just the file name with no extension from a path.
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function NoFileExtension(ByVal filePath As String) As String

            If filePath IsNot Nothing AndAlso filePath.Length > 0 Then
                Return Path.GetFileNameWithoutExtension(filePath)
            Else
                Return ""
            End If

        End Function

        ''' <summary>
        ''' Returns True if given path exists.
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function PathExists(ByVal filePath As String) As Boolean

            Return Directory.Exists(filePath)

        End Function

        ''' <summary>
        ''' Makes sure path is suffixed with standard directory separator.
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function AddPathSuffix(ByVal filePath As String) As String

            If filePath IsNot Nothing AndAlso filePath.Length > 0 Then
                Dim suffixChar As Char = filePath.Chars(filePath.Length - 1)
                If suffixChar <> Path.DirectorySeparatorChar And suffixChar <> Path.AltDirectorySeparatorChar Then
                    filePath &= Path.DirectorySeparatorChar
                End If
            Else
                filePath = Path.DirectorySeparatorChar
            End If

            Return filePath

        End Function

        ''' <summary>
        ''' Makes sure path is not suffixed with any directory separator.
        ''' </summary>
        ''' <param name="filePath">To be provided.</param>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public Shared Function RemovePathSuffix(ByVal filePath As String) As String

            If filePath IsNot Nothing AndAlso filePath.Length > 0 Then
                Dim suffixChar As Char = filePath.Chars(filePath.Length - 1)
                While (suffixChar = Path.DirectorySeparatorChar Or suffixChar = Path.AltDirectorySeparatorChar) And filePath.Length > 0
                    filePath = filePath.Substring(0, filePath.Length - 1)
                    If filePath.Length > 0 Then suffixChar = filePath.Chars(filePath.Length - 1)
                End While
            End If

            Return filePath

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
            ElseIf fileName.Length > length Then
                Dim justName As String = JustFileName(fileName)

                If justName.Length = fileName.Length Then
                    ' This is just a file name, make sure extension shows...
                    Dim justExtension As String = JustFileExtension(fileName)
                    Dim trimName As String = NoFileExtension(fileName)

                    If trimName.Length > 8 Then
                        If justExtension.Length > length - 8 Then justExtension = justExtension.Substring(0, length - 8)
                        Dim offset As Single = (length - justExtension.Length() - 3) / 2
                        Return trimName.Substring(0, CInt(Ceiling(offset))) & "..." & trimName.Substring(Len(trimName) - CInt(Floor(offset)) + 1) & justExtension
                    Else
                        ' We can't trim file names less than 8 with a "...", so we truncate long extension
                        Return trimName & justExtension.Substring(0, length - trimName.Length)
                    End If
                ElseIf justName.Length > length Then
                    ' Just file name part exeeds length, recurse into function without path
                    Return TrimFileName(justName, length)
                Else
                    ' File name contains path, trim path before file name...
                    Dim justFilePath As String = JustPath(fileName)
                    Dim offset As Integer = length - justName.Length - 4

                    If Len(justFilePath) > offset And offset > 0 Then
                        Return justFilePath.Substring(0, offset) & "...\" & justName
                    Else
                        ' Can't fit path, just trim file name
                        Return TrimFileName(justName, length)
                    End If
                End If
            Else
                ' Full file name fits within requested length...
                Return fileName
            End If

        End Function

        ''' <summary>
        ''' Gets a list of files for the given path and wildcard pattern (e.g., "c:\*.*").
        ''' </summary>
        ''' <param name="path">The path for which a list of files is to be returned.</param>
        ''' <returns>A list of files for the given path.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetFileList(ByVal path As String) As String()

            Return Directory.GetFiles(JustPath(path), JustFileName(path))

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
