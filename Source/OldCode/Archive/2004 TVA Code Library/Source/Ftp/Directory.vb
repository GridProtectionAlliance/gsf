' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.Net.Sockets
Imports System.Text.RegularExpressions

Namespace Ftp

    Public Interface IFile

        ReadOnly Property Parent() As Directory
        ReadOnly Property Name() As String
        ReadOnly Property FullPath() As String
        ReadOnly Property IsFile() As Boolean
        ReadOnly Property IsDirectory() As Boolean
        Property Size() As Long
        Property Permission() As String
        Property TimeStamp() As DateTime

    End Interface

    Friend Class TimeStampParser

        Public Enum RawDataStyle
            UnixDate
            UnixDateTime
            DosDateTime
            Undetermined
        End Enum

        Public RawValue As String
        Public Style As RawDataStyle

        Public Sub New()

            Style = RawDataStyle.Undetermined

        End Sub

        Public Sub New(ByVal RawValue As String, ByVal Style As RawDataStyle)

            Me.RawValue = RawValue
            Me.Style = Style

        End Sub

        Public ReadOnly Property Value() As DateTime
            Get
                If Len(RawValue) > 0 Then
                    Try
                        Select Case Style
                            Case RawDataStyle.UnixDate
                                Return CDate(RawValue)
                            Case RawDataStyle.UnixDateTime
                                Dim sa As String() = RawValue.Split(" "c)
                                Return CDate(sa(0) & " " & sa(1) & " " & Year(Now()) & " " & sa(2))
                            Case RawDataStyle.DosDateTime
                                Return CDate(RawValue)
                            Case Else
                                Return CDate(RawValue)
                        End Select
                    Catch
                        Return DateTime.MinValue
                    End Try
                Else
                    Return DateTime.MinValue
                End If
            End Get
        End Property

    End Class

    Friend Class ItemInfo

        Public Name As String
        Public FullPath As String
        Public Permission As String
        Public IsDirectory As Boolean
        Public Size As Long
        Public TimeStamp As New TimeStampParser()

    End Class

    Public Class Directory

        Implements IFile
        Implements IComparable

        Private m_session As SessionConnected
        Private m_parent As Directory
        Private m_name As String
        Private m_fullPath As String
        Private m_subDirectories As Hashtable
        Private m_files As Hashtable
        Private m_caseInsensitive As Boolean
        Private m_size As Long
        Private m_permission As String
        Private m_timestamp As DateTime

        Private Shared m_UnixListLineStyle1 As New Regex("(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)")
        Private Shared m_UnixListLineStyle2 As New Regex("(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)")
        Private Shared m_UnixListLineStyle3 As New Regex("(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d+:\d+)\s+(?<name>.+)")
        Private Shared m_DosListLineStyle1 As New Regex("(?<timestamp>\d{2}\-\d{2}\-\d{2}\s+\d{2}:\d{2}[Aa|Pp][mM])\s+(?<dir>\<\w+\>){0,1}(?<size>\d+){0,1}\s+(?<name>.+)")                     ' IIS FTP Service
        Private Shared m_DosListLineStyle2 As New Regex("(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d+:\d+)\s+(?<name>.+)") ' IIS FTP Service in Unix Mode

        Public Event DirectoryListLineScan(ByVal Line As String)

        Friend Sub New(ByVal s As SessionConnected, ByVal CaseInsensitive As Boolean, ByVal fullPath As String)

            m_session = s
            m_parent = Nothing
            m_caseInsensitive = CaseInsensitive

            If Right(fullPath, 1) = "/" Then fullPath = Left(fullPath, Len(fullPath) - 1)

            If Len(fullPath) = 0 Then
                m_name = m_fullPath = "/"
            Else
                Dim directories As String() = fullPath.Split("/"c)
                m_name = directories(directories.Length - 1)
                m_fullPath = fullPath & "/"
            End If

        End Sub

        Friend Sub New(ByVal s As SessionConnected, ByVal parent As Directory, ByVal CaseInsensitive As Boolean, ByVal info As ItemInfo)

            m_session = s
            m_parent = parent
            m_caseInsensitive = CaseInsensitive

            If Len(info.Name) > 0 Then
                m_name = info.Name
                If parent Is Nothing Then
                    m_fullPath = m_name & "/"
                Else
                    m_fullPath = parent.FullPath & m_name & "/"
                End If
            Else
                m_name = m_fullPath = "/"
            End If

            m_size = info.Size
            m_permission = info.Permission
            m_timestamp = info.TimeStamp.Value

        End Sub

        Public Property CaseInsensitive() As Boolean
            Get
                Return m_caseInsensitive
            End Get
            Set(ByVal Value As Boolean)
                m_caseInsensitive = Value
                Refresh()
            End Set
        End Property

        Public ReadOnly Property Name() As String Implements IFile.Name
            Get
                Return m_name
            End Get
        End Property

        Public ReadOnly Property FullPath() As String Implements IFile.FullPath
            Get
                Return m_fullPath
            End Get
        End Property

        Public ReadOnly Property IsFile() As Boolean Implements IFile.IsFile
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property IsDirectory() As Boolean Implements IFile.IsDirectory
            Get
                Return True
            End Get
        End Property

        Public Property Size() As Long Implements IFile.Size
            Get
                Return m_size
            End Get
            Set(ByVal Value As Long)
                m_size = Value
            End Set
        End Property

        Public Property Permission() As String Implements IFile.Permission
            Get
                Return m_permission
            End Get
            Set(ByVal Value As String)
                m_permission = Value
            End Set
        End Property

        Public Property TimeStamp() As DateTime Implements IFile.TimeStamp
            Get
                Return m_timestamp
            End Get
            Set(ByVal Value As DateTime)
                m_timestamp = Value
            End Set
        End Property

        Public ReadOnly Property Parent() As Directory Implements IFile.Parent
            Get
                If StrComp(m_fullPath, m_session.RootDirectory.m_fullPath, IIf(m_caseInsensitive, CompareMethod.Text, CompareMethod.Binary)) = 0 Then
                    Return Nothing
                End If

                ' If we don't have a reference to parent directory, we try to derive it...
                If m_parent Is Nothing Then
                    CheckSessionCurrentDirectory()

                    Dim parentPath As New StringBuilder
                    Dim fullPath = m_session.ControlChannel.PWD()

                    If Right(fullPath, 1) <> "/" Then fullPath &= "/"

                    Dim paths As String() = fullPath.Split("/")
                    Dim i As Integer

                    For i = 0 To paths.Length - 2
                        If Len(paths(i)) = 0 Then
                            parentPath.Append("/")
                        Else
                            parentPath.Append(paths(i))
                            parentPath.Append("/")
                        End If
                    Next i

                    Dim parentDir As New Directory(m_session, m_caseInsensitive, parentPath.ToString())

                    If StrComp(parentDir.m_fullPath, m_session.RootDirectory.m_fullPath, IIf(m_caseInsensitive, CompareMethod.Text, CompareMethod.Binary)) = 0 Then
                        m_parent = m_session.RootDirectory
                    Else
                        m_parent = parentDir
                    End If
                End If

                Return m_parent
            End Get
        End Property

        Public ReadOnly Property SubDirectories() As ICollection
            Get
                InitHashtable()
                Return m_subDirectories.Values
            End Get
        End Property

        Public ReadOnly Property Files() As ICollection
            Get
                InitHashtable()
                Return m_files.Values
            End Get
        End Property

        Public Function FindFile(ByVal fileName As String) As File

            InitHashtable()

            Return CType(m_files(fileName), File)

        End Function

        Public Overloads Function FindSubDirectory(ByVal dirName As String) As Directory

            InitHashtable()

            Return CType(m_subDirectories(dirName), Directory)

        End Function

        Public Overloads Sub PutFile(ByVal localFile As String)

            PutFile(localFile, Nothing)

        End Sub

        Public Overloads Sub PutFile(ByVal localFile As String, ByVal remoteFile As String)

            CheckSessionCurrentDirectory()

            Dim fi As New FileInfo(localFile)

            If remoteFile Is Nothing Then
                remoteFile = fi.Name
            End If

            Dim transfer As New FileTransferer(Me, localFile, remoteFile, fi.Length, TransferDirection.Upload)

            transfer.StartTransfer()

        End Sub

        Public Overloads Sub GetFile(ByVal remoteFile As String)

            GetFile(remoteFile, remoteFile)

        End Sub

        Public Overloads Sub GetFile(ByVal localFile As String, ByVal remoteFile As String)

            InitHashtable()

            Dim file As File = CType(m_files(remoteFile), File)

            If file Is Nothing Then
                Throw New FileNotFoundException(remoteFile)
            End If

            Dim transfer As New FileTransferer(Me, localFile, remoteFile, file.Size, TransferDirection.Download)

            transfer.StartTransfer()

        End Sub

        Public Overloads Sub BeginPutFile(ByVal localFile As String)

            BeginPutFile(localFile, Nothing)

        End Sub

        Public Overloads Sub BeginPutFile(ByVal localFile As String, ByVal remoteFile As String)

            CheckSessionCurrentDirectory()

            Dim fi As New FileInfo(localFile)

            If remoteFile Is Nothing Then
                remoteFile = fi.Name
            End If

            Dim transfer As New FileTransferer(Me, localFile, remoteFile, fi.Length, TransferDirection.Upload)

            transfer.StartAsyncTransfer()

        End Sub

        Public Overloads Sub BeginGetFile(ByVal remoteFile As String)

            BeginGetFile(remoteFile, remoteFile)

        End Sub

        Public Overloads Sub BeginGetFile(ByVal localFile As String, ByVal remoteFile As String)

            InitHashtable()

            Dim file As File = CType(m_files(remoteFile), File)

            If file Is Nothing Then
                Throw New FileNotFoundException(remoteFile)
            End If

            Dim transfer As New FileTransferer(Me, localFile, remoteFile, file.Size, TransferDirection.Download)

            transfer.StartAsyncTransfer()

        End Sub

        Public Sub RemoveFile(ByVal fileName As String)

            CheckSessionCurrentDirectory()

            m_session.ControlChannel.DELE(fileName)

            m_files.Remove(fileName)

        End Sub

        Public Sub RemoveSubDir(ByVal dirName As String)

            CheckSessionCurrentDirectory()

            m_session.ControlChannel.RMD(dirName)

            m_subDirectories.Remove(dirName)

        End Sub

        Public Function CreateFile(ByVal newFileName As String) As File

            Dim stream As DataStream = CreateFileStream(newFileName)

            stream.Close()

            Return CType(m_files(newFileName), File)

        End Function

        Public Function CreateFileStream(ByVal newFileName As String) As OutputDataStream

            InitHashtable()

            Dim stream As DataStream = m_session.ControlChannel.GetPassiveDataStream(TransferDirection.Upload)

            Try
                m_session.ControlChannel.STOR(newFileName)

                Dim newFile As New File(Me, newFileName)

                m_files(newFileName) = newFile

                Return CType(stream, OutputDataStream)
            Catch
                stream.Close()
                Throw
            End Try

        End Function

        Public Sub Refresh()

            ClearItems()
            InitHashtable()

        End Sub

        Friend Sub ClearItems()

            m_subDirectories = Nothing
            m_files = Nothing

        End Sub

        Friend ReadOnly Property Session() As SessionConnected
            Get
                Return m_session
            End Get
        End Property

        Friend Sub CheckSessionCurrentDirectory()

            If m_session.CurrentDirectory.m_fullPath <> m_fullPath Then
                Throw New InvalidOperationException(m_fullPath & " is not current directory.")
            End If

        End Sub

        Private Sub LoadDirectoryItems()

            If Not m_session.CurrentDirectory Is Me Then
                Throw New InvalidOperationException(m_name & " is not current active directory")
            End If

            Dim lineQueue As Queue = m_session.ControlChannel.List(False)
            Dim info As ItemInfo
            Dim line As String

            For Each line In lineQueue
                ' We allow users to inspect FTP lineQueue if desired...
                RaiseEvent DirectoryListLineScan(line)

                info = New ItemInfo
                If ParseListLine(line, info) Then
                    If info.IsDirectory Then
                        m_subDirectories.Add(info.Name, New Directory(m_session, Me, m_caseInsensitive, info))
                    Else
                        m_files.Add(info.Name, New File(Me, info))
                    End If
                End If
            Next

        End Sub

        Private Sub InitHashtable()

            CheckSessionCurrentDirectory()

            If Not m_subDirectories Is Nothing AndAlso Not m_files Is Nothing Then
                Return
            End If

            If m_subDirectories Is Nothing Then
                If m_caseInsensitive Then
                    m_subDirectories = New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
                Else
                    m_subDirectories = New Hashtable
                End If
            End If

            If m_files Is Nothing Then
                If m_caseInsensitive Then
                    m_files = New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
                Else
                    m_files = New Hashtable
                End If
            End If

            LoadDirectoryItems()

        End Sub

        Private Function ParseListLine(ByVal line As String, ByVal info As ItemInfo) As Boolean

            Dim m As Match = MatchingListLine(line, info.TimeStamp.Style)

            If m Is Nothing Then
                Return False
            End If

            info.Name = m.Groups("name").Value
            info.FullPath = m_fullPath & info.Name

            Dim dir As String = m.Groups("dir").Value

            If Len(dir) > 0 AndAlso dir <> "-" Then
                info.IsDirectory = True
                info.FullPath &= "/"
            Else
                info.Size = Long.Parse(m.Groups("size").Value)
            End If

            info.Permission = m.Groups("permission").Value
            info.TimeStamp.RawValue = m.Groups("timestamp").Value

            Return True

        End Function

        Private Function MatchingListLine(ByVal line As String, ByRef tsStyle As TimeStampParser.RawDataStyle) As Match

            Dim m As Match = m_UnixListLineStyle1.Match(line)

            If m.Success Then
                tsStyle = TimeStampParser.RawDataStyle.UnixDate
                Return m
            End If

            m = m_UnixListLineStyle3.Match(line)

            If m.Success Then
                tsStyle = TimeStampParser.RawDataStyle.UnixDateTime
                Return m
            End If

            m = m_UnixListLineStyle2.Match(line)

            If m.Success Then
                tsStyle = TimeStampParser.RawDataStyle.UnixDate
                Return m
            End If

            m = m_DosListLineStyle1.Match(line)

            If m.Success Then
                tsStyle = TimeStampParser.RawDataStyle.DosDateTime
                Return m
            End If

            m = m_DosListLineStyle2.Match(line)

            If m.Success Then
                tsStyle = TimeStampParser.RawDataStyle.UnixDateTime
                Return m
            End If

            tsStyle = TimeStampParser.RawDataStyle.Undetermined
            Return Nothing

        End Function

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            ' Directories are sorted by name
            If TypeOf obj Is IFile Then
                Return StrComp(m_name, DirectCast(obj, IFile).Name, IIf(m_caseInsensitive, CompareMethod.Text, CompareMethod.Binary))
            Else
                Throw New ArgumentException("Directory can only be compared to other Directories or Files")
            End If

        End Function

    End Class

End Namespace