'*******************************************************************************************************
'  Tva.IO.FilePath.vb - File/Path Manipulation Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
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
Imports System.Runtime.InteropServices
Imports System.Reflection.Assembly
Imports Tva.DateTime.Common
Imports Tva.Text.Encoding
Imports Tva.Interop.WindowsApi

Namespace IO

    Public NotInheritable Class FilePath

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
        Private Structure NETRESOURCE
            Public dwScope As Integer
            Public dwType As Integer
            Public dwDisplayType As Integer
            Public dwUsage As Integer
            Public lpLocalName As String
            Public lpRemoteName As String
            Public lpComment As String
            Public lpProvider As String
        End Structure

        Private Declare Auto Function WNetAddConnection2 Lib "mpr.dll" Alias "WNetAddConnection2W" (ByRef lpNetResource As NETRESOURCE, ByVal lpPassword As String, ByVal lpUsername As String, ByVal dwFlags As Integer) As Integer
        Private Declare Auto Function WNetCancelConnection2 Lib "mpr.dll" Alias "WNetCancelConnection2W" (ByVal lpName As String, ByVal dwFlags As Integer, ByVal fForce As Boolean) As Integer

        Private Const RESOURCETYPE_DISK As Integer = &H1

        Private Shared m_fileNameCharPattern As String

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Connects to a network share with the specified user's credentials.</summary>
        ''' <param name="sharename">UNC share name to connect to</param>
        ''' <param name="username">Username to use for connection</param>
        ''' <param name="password">Password to use for connection</param>
        ''' <param name="domain">Domain name to use for connetion - specify computer name for local system accounts</param>
        Public Shared Sub ConnectToNetworkShare(ByVal sharename As String, ByVal username As String, ByVal password As String, ByVal domain As String)

            Dim resource As NETRESOURCE = Nothing
            Dim result As Integer

            With resource
                .dwType = RESOURCETYPE_DISK
                .lpRemoteName = sharename
            End With

            If Len(domain) > 0 Then username = domain & "\" & username

            result = WNetAddConnection2(resource, password, username, 0)
            If result <> 0 Then Throw New InvalidOperationException("Failed to connect to network share """ & sharename & """ as user " & username & ".  " & GetErrorMessage(result))

        End Sub

        ''' <summary>Disconnects the specified network share.</summary>
        ''' <param name="sharename">UNC share name to disconnect from</param>
        Public Shared Sub DisconnectFromNetworkShare(ByVal sharename As String)

            DisconnectFromNetworkShare(sharename, True)

        End Sub

        ''' <summary>Disconnects the specified network share.</summary>
        ''' <param name="sharename">UNC share name to disconnect from</param>
        ''' <param name="force">Set to True force disconnect</param>
        Public Shared Sub DisconnectFromNetworkShare(ByVal sharename As String, ByVal force As Boolean)

            Dim result As Integer = WNetCancelConnection2(sharename, 0, force)
            If result <> 0 Then Throw New InvalidOperationException("Failed to disconnect from network share """ & sharename & """.  " & GetErrorMessage(result))

        End Sub

        ''' <summary>Returns True if specified file name matches any of the given file specs (wildcards are defined as '*' or '?' characters).</summary>
        Public Shared Function IsFilePatternMatch(ByVal fileSpecs As String(), ByVal fileName As String, ByVal ignoreCase As Boolean) As Boolean

            Dim found As Boolean

            For Each fileSpec As String In fileSpecs
                If IsFilePatternMatch(fileSpec, fileName, ignoreCase) Then
                    found = True
                    Exit For
                End If
            Next

            Return found

        End Function

        ''' <summary>Returns True if specified file name matches given file spec (wildcards are defined as '*' or '?' characters).</summary>
        Public Shared Function IsFilePatternMatch(ByVal fileSpec As String, ByVal fileName As String, ByVal ignoreCase As Boolean) As Boolean

            Return (New Regex(GetFilePatternRegularExpression(fileSpec), IIf(ignoreCase, RegexOptions.IgnoreCase, RegexOptions.None))).IsMatch(fileName)

        End Function

        ''' <summary>Returns a regular expression that simulates wildcard matching for filenames (wildcards are defined as '*' or '?' characters)</summary>
        Public Shared Function GetFilePatternRegularExpression(ByVal fileSpec As String) As String

            If m_fileNameCharPattern Is Nothing Then
                With New StringBuilder
                    ' Define a regular expression pattern for a valid file name character, we do this by
                    ' allowing any characters except those that would not be valid as part of a filename,
                    ' this essentially builds the "?" wildcard pattern match
                    .Append("[^")
                    .Append(EncodeRegexChar(Path.DirectorySeparatorChar))
                    .Append(EncodeRegexChar(Path.AltDirectorySeparatorChar))
                    .Append(EncodeRegexChar(Path.PathSeparator))
                    .Append(EncodeRegexChar(Path.VolumeSeparatorChar))

                    For Each c As Char In Path.GetInvalidPathChars()
                        .Append(EncodeRegexChar(c))
                    Next

                    .Append("]")
                    m_fileNameCharPattern = .ToString()
                End With
            End If

            ' Replace wildcard file patterns with their equivalent regular expression
            fileSpec = fileSpec.Replace("\", "\u005C")  ' Backslash in Regex means special sequence, here we really want a backslash
            fileSpec = fileSpec.Replace(".", "\u002E")  ' Dot in Regex means any character, here we really want a dot
            fileSpec = fileSpec.Replace("?", m_fileNameCharPattern)
            fileSpec = fileSpec.Replace("*", "(" & m_fileNameCharPattern & ")*")

            Return "^" & fileSpec & "$"

        End Function

        ''' <summary>Get the size of the specified file.</summary>
        ''' <param name="fileName">Name of file whose size is to be returned.</param>
        ''' <returns>The size of the specified file.</returns>
        Public Shared Function GetFileLength(ByVal fileName As String) As Long

            Try
                With New FileInfo(fileName)
                    Return .Length
                End With
            Catch
                Return -1
            End Try

        End Function

        ''' <summary>Gets a unique temporary file name with path.</summary>
        Public Shared Function GetTempFile() As String

            Return GetTempFile(True, True, "tmp")

        End Function

        ''' <summary>Gets a unique temporary file name with path - if UseTempPath is False, application path is used for temp file.</summary>
        Public Shared Function GetTempFile(ByVal useTempPath As Boolean, ByVal createZeroLengthFile As Boolean, ByVal fileExtension As String) As String

            If useTempPath AndAlso createZeroLengthFile Then
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

        ''' <summary>Gets a file name (with .tmp extension) guaranteed to be unique with no path.</summary>
        Public Shared Function GetTempFileName() As String

            Return GetTempFileName("tmp")

        End Function

        ''' <summary>Gets a file name guaranteed to be unique with no path.</summary>
        ''' <param name="fileExtension">The extension of the temporary file.</param>
        ''' <returns>The file name guaranteed to be unique with no path.</returns>
        ''' <remarks>Use GetTempFile to return unique file name with path.</remarks>
        Public Shared Function GetTempFileName(ByVal fileExtension As String) As String

            If fileExtension.Substring(0, 1) = "." Then fileExtension = fileExtension.Substring(1)
            Return Guid.NewGuid.ToString() & "." & fileExtension

        End Function

        ''' <summary>Gets the temporary file path - path will be suffixed with standard directory separator.</summary>
        ''' <returns>The temporary file path.</returns>
        Public Shared Function GetTempFilePath() As String

            Return AddPathSuffix(Path.GetTempPath())

        End Function

        ''' <summary>Gets the path of the executing assembly - path will be suffixed with standard directory separator.</summary>
        ''' <returns>The path of the executing assembly.</returns>
        Public Shared Function GetApplicationPath() As String

            Return JustPath(Trim(GetEntryAssembly.Location()))

        End Function

        ''' <summary>Returns just the drive letter (or UNC \\server\share\) from a path") - path will be suffixed with standard directory separator.</summary>
        Public Shared Function JustDrive(ByVal filePath As String) As String

            If String.IsNullOrEmpty(filePath) Then
                Return Path.DirectorySeparatorChar
            Else
                Return AddPathSuffix(Path.GetPathRoot(filePath))
            End If

        End Function

        ''' <summary>Returns just the file name from a path.</summary>
        Public Shared Function JustFileName(ByVal filePath As String) As String

            If String.IsNullOrEmpty(filePath) Then
                Return ""
            Else
                Return Path.GetFileName(filePath)
            End If

        End Function

        ''' <summary>Returns last directory name from a path (e.g., would return sub2 from c:\windows\sub2\filename.ext).</summary>
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

        ''' <summary>Returns just the path without a filename from a path - path will be suffixed with standard directory separator.</summary>
        Public Shared Function JustPath(ByVal filePath As String) As String

            If String.IsNullOrEmpty(filePath) Then
                Return Path.DirectorySeparatorChar
            Else
                Return Path.GetDirectoryName(filePath) & Path.DirectorySeparatorChar
            End If

        End Function

        ''' <summary>Returns just the file extension from a path - keeps extension "dot".</summary>
        Public Shared Function JustFileExtension(ByVal filePath As String) As String

            If String.IsNullOrEmpty(filePath) Then
                Return ""
            Else
                Return Path.GetExtension(filePath)
            End If

        End Function

        ''' <summary>Returns just the file name with no extension from a path.</summary>
        Public Shared Function NoFileExtension(ByVal filePath As String) As String

            If String.IsNullOrEmpty(filePath) Then
                Return ""
            Else
                Return Path.GetFileNameWithoutExtension(filePath)
            End If

        End Function

        ''' <summary>Makes sure path is suffixed with standard directory separator.</summary>
        Public Shared Function AddPathSuffix(ByVal filePath As String) As String

            If String.IsNullOrEmpty(filePath) Then
                filePath = Path.DirectorySeparatorChar
            Else
                Dim suffixChar As Char = filePath.Chars(filePath.Length - 1)
                If suffixChar <> Path.DirectorySeparatorChar AndAlso suffixChar <> Path.AltDirectorySeparatorChar Then
                    filePath &= Path.DirectorySeparatorChar
                End If
            End If

            Return filePath

        End Function

        ''' <summary>Makes sure path is not suffixed with any directory separator.</summary>
        Public Shared Function RemovePathSuffix(ByVal filePath As String) As String

            If String.IsNullOrEmpty(filePath) Then
                filePath = ""
            Else
                Dim suffixChar As Char = filePath.Chars(filePath.Length - 1)
                While (suffixChar = Path.DirectorySeparatorChar OrElse suffixChar = Path.AltDirectorySeparatorChar) AndAlso filePath.Length > 0
                    filePath = filePath.Substring(0, filePath.Length - 1)
                    If filePath.Length > 0 Then suffixChar = filePath.Chars(filePath.Length - 1)
                End While
            End If

            Return filePath

        End Function

        ''' <summary>Returns a file name for display purposes of the specified length using "..." to indicate a longer name</summary>
        ''' <remarks>
        ''' <para>Minimum value for the <paramref name="length" /> parameter is 12.</para>
        ''' <para>12 will be used for any value specified less than 12.</para>
        ''' </remarks>
        Public Shared Function TrimFileName(ByVal fileName As String, ByVal length As Integer) As String

            If String.IsNullOrEmpty(fileName) Then
                fileName = ""
            Else
                fileName = fileName.Trim()
            End If

            If length < 12 Then length = 12

            If fileName.Length > length Then
                Dim justName As String = JustFileName(fileName)

                If justName.Length = fileName.Length Then
                    ' This is just a file name, make sure extension shows...
                    Dim justExtension As String = JustFileExtension(fileName)
                    Dim trimName As String = NoFileExtension(fileName)

                    If trimName.Length > 8 Then
                        If justExtension.Length > length - 8 Then justExtension = justExtension.Substring(0, length - 8)
                        Dim offset As Single = (length - justExtension.Length() - 3) / 2
                        Return trimName.Substring(0, Ceiling(offset)) & "..." & trimName.Substring(Len(trimName) - Floor(offset) + 1) & justExtension
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

        ''' <summary>Gets a list of files for the given path and wildcard pattern (e.g., "c:\*.*").</summary>
        ''' <param name="path">The path for which a list of files is to be returned.</param>
        ''' <returns>A list of files for the given path.</returns>
        Public Shared Function GetFileList(ByVal path As String) As String()

            Return Directory.GetFiles(JustPath(path), JustFileName(path))

        End Function

        ''' <summary>Waits for the default duration (5 seconds) for read access on a file.</summary>
        ''' <param name="fileName">The name of the file to wait for to obtain read access.</param>
        Public Shared Sub WaitForReadLock(ByVal fileName As String)

            WaitForReadLock(fileName, 5)

        End Sub

        ''' <summary>Waits for read access on a file for the specified number of seconds.</summary>
        ''' <param name="fileName">The name of the file to wait for to obtain read access.</param>
        ''' <param name="secondsToWait">The time to wait for in seconds to obtain read access on a file.</param>
        ''' <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
        Public Shared Sub WaitForReadLock(ByVal fileName As String, ByVal secondsToWait As Double)

            If Not File.Exists(fileName) Then
                Throw New FileNotFoundException("Could not test file lock for """ & fileName & """, file does not exist", fileName)
                Exit Sub
            End If

            ' We use this function to keep trying for a file lock...
            Dim targetFile As FileStream = Nothing
            Dim startTime As Double = SystemTimer

            While True
                Try
                    targetFile = File.OpenRead(fileName)
                    targetFile.Close()
                    Exit While
                Catch
                    ' We'll keep trying till we can open the file...
                End Try

                If targetFile IsNot Nothing Then
                    Try
                        targetFile.Close()
                    Catch
                    End Try
                    targetFile = Nothing
                End If

                If secondsToWait > 0 Then
                    If SystemTimer > startTime + secondsToWait Then
                        Throw New IOException("Could not open """ & fileName & """ for read access, tried for " & secondsToWait & " seconds")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(1)
            End While

        End Sub

        ''' <summary>Waits for the default duration (5 seconds) for write access on a file.</summary>
        ''' <param name="fileName">The name of the file to wait for to obtain write access.</param>
        Public Shared Sub WaitForWriteLock(ByVal fileName As String)

            WaitForWriteLock(fileName, 5)

        End Sub

        ''' <summary>Waits for write access on a file for the specified number of seconds.</summary>
        ''' <param name="fileName">The name of the file to wait for to obtain write access.</param>
        ''' <param name="secondsToWait">The time to wait for in seconds to obtain write access on a file.</param>
        ''' <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
        Public Shared Sub WaitForWriteLock(ByVal fileName As String, ByVal secondsToWait As Double)

            If Not File.Exists(fileName) Then
                Throw New FileNotFoundException("Could not test file lock for """ & fileName & """, file does not exist", fileName)
                Exit Sub
            End If

            ' We use this function to keep trying for a file lock...
            Dim targetFile As FileStream = Nothing
            Dim startTime As Double = SystemTimer

            While True
                Try
                    targetFile = File.OpenWrite(fileName)
                    targetFile.Close()
                    Exit While
                Catch
                    ' We'll keep trying till we can open the file...
                End Try

                If targetFile IsNot Nothing Then
                    Try
                        targetFile.Close()
                    Catch
                    End Try
                    targetFile = Nothing
                End If

                If secondsToWait > 0 Then
                    If SystemTimer > startTime + secondsToWait Then
                        Throw New IOException("Could not open """ & fileName & """ for write access, tried for " & secondsToWait & " seconds")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(1)
            End While

        End Sub

        ''' <summary>Waits for the default duration (5 seconds) for a file to exist.</summary>
        ''' <param name="fileName">The name of the file to wait for until it is created.</param>
        Public Shared Sub WaitTillExists(ByVal fileName As String)

            WaitTillExists(fileName, 5)

        End Sub

        ''' <summary>Waits for a file to exist for the specified number of seconds.</summary>
        ''' <param name="fileName">The name of the file to wait for until it is created.</param>
        ''' <param name="secondsToWait">The time to wait for in seconds for the file to be created.</param>
        ''' <remarks>Set secondsToWait to zero to wait infinitely.</remarks>
        Public Shared Sub WaitTillExists(ByVal fileName As String, ByVal secondsToWait As Double)

            ' We use this function to keep waiting for a file to be created...
            Dim startTime As Double = SystemTimer

            While Not File.Exists(fileName)
                If secondsToWait > 0 Then
                    If SystemTimer > startTime + secondsToWait Then
                        Throw New IOException("Waited for """ & fileName & """ to exist for " & secondsToWait & " seconds, but it was never created")
                        Exit While
                    End If
                End If

                ' Yield to all other system threads...
                Thread.Sleep(1)
            End While

        End Sub

    End Class

End Namespace
