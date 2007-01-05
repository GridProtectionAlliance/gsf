' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Net.Sockets

Namespace Ftp

    Public Class File

        Implements IFile
        Implements IComparable

        Private m_parent As Directory
        Private m_name As String
        Private m_size As Long
        Private m_permission As String
        Private m_timestamp As Date

        Friend Sub New(ByVal parent As Directory, ByVal info As ItemInfo)

            m_parent = parent
            m_name = info.Name
            m_size = info.Size
            m_permission = info.Permission
            m_timestamp = info.TimeStamp.Value

        End Sub

        Friend Sub New(ByVal parent As Directory, ByVal name As String)

            m_parent = parent
            m_name = name

        End Sub

        Public ReadOnly Property Name() As String Implements IFile.Name
            Get
                Return m_name
            End Get
        End Property

        Public ReadOnly Property FullPath() As String Implements IFile.FullPath
            Get
                Return m_parent.FullPath & m_name
            End Get
        End Property

        Public ReadOnly Property IsFile() As Boolean Implements IFile.IsFile
            Get
                Return True
            End Get
        End Property

        Public ReadOnly Property IsDirectory() As Boolean Implements IFile.IsDirectory
            Get
                Return False
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

        Public Property TimeStamp() As Date Implements IFile.TimeStamp
            Get
                Return m_timestamp
            End Get
            Set(ByVal Value As Date)
                m_timestamp = Value
            End Set
        End Property

        Public ReadOnly Property Parent() As Directory Implements IFile.Parent
            Get
                m_parent.CheckSessionCurrentDirectory()
                Return m_parent
            End Get
        End Property

        Public Overloads Function GetInputStream() As InputDataStream

            Return CType(GetStream(0, TransferDirection.Download), InputDataStream)

        End Function

        Public Overloads Function GetOutputStream() As OutputDataStream

            Return CType(GetStream(0, TransferDirection.Upload), OutputDataStream)

        End Function

        Public Overloads Function GetInputStream(ByVal offset As Long) As InputDataStream

            Return CType(GetStream(offset, TransferDirection.Download), InputDataStream)

        End Function

        Public Overloads Function GetOutputStream(ByVal offset As Long) As OutputDataStream

            Return CType(GetStream(offset, TransferDirection.Upload), OutputDataStream)

        End Function

        Private Function GetStream(ByVal offset As Long, ByVal dir As TransferDirection) As DataStream

            m_parent.CheckSessionCurrentDirectory()

            Dim Session As SessionConnected = m_parent.Session

            If offset <> 0 Then
                Session.ControlChannel.REST(offset)
            End If

            Dim stream As DataStream = Session.ControlChannel.GetPassiveDataStream(dir)

            Try
                If dir = TransferDirection.Download Then
                    Session.ControlChannel.RETR(m_name)
                Else
                    Session.ControlChannel.STOR(m_name)
                End If
            Catch
                stream.Close()
                Throw
            End Try

            Return stream

        End Function

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            ' Files are sorted by name
            If TypeOf obj Is IFile Then
                Return StrComp(m_name, DirectCast(obj, IFile).Name, IIf(m_parent.CaseInsensitive, CompareMethod.Text, CompareMethod.Binary))
            Else
                Throw New ArgumentException("File can only be compared to other Files or Directories")
            End If

        End Function

    End Class

End Namespace