' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO

Namespace Net.Ftp

    Friend Class SessionConnected

        Implements ISessionState

        Private m_host As FtpSession
        Private m_ctrlChannel As ControlChannel
        Private m_root As Directory
        Private m_current As Directory
        Private m_dataStream As DataStream
        Private m_caseInsensitive As Boolean

        Friend Sub New(ByVal h As FtpSession, ByVal ctrl As ControlChannel, ByVal CaseInsensitive As Boolean)

            m_host = h
            m_ctrlChannel = ctrl
            m_ctrlChannel.Session = Me
            m_caseInsensitive = CaseInsensitive

        End Sub

        Friend Sub InitRootDirectory()

            m_root = New Directory(Me, m_caseInsensitive, m_ctrlChannel.PWD())
            m_current = m_root

        End Sub

        Public Property Server() As String Implements ISessionState.Server
            Get
                Return m_ctrlChannel.Server
            End Get
            Set(ByVal Value As String)
                Throw New InvalidOperationException
            End Set
        End Property

        Public Property Port() As Integer Implements ISessionState.Port
            Get
                Return m_ctrlChannel.Port
            End Get
            Set(ByVal Value As Integer)
                Throw New InvalidOperationException
            End Set
        End Property

        Public ReadOnly Property RootDirectory() As Directory Implements ISessionState.RootDirectory
            Get
                Return m_root
            End Get
        End Property

        Public Property CurrentDirectory() As Directory Implements ISessionState.CurrentDirectory
            Get
                Return m_current
            End Get
            Set(ByVal Value As Directory)
                m_ctrlChannel.CWD(Value.FullPath)
                m_current = Value
                m_current.ClearItems()
            End Set
        End Property

        Public ReadOnly Property IsBusy() As Boolean Implements ISessionState.IsBusy
            Get
                Return Not (m_dataStream Is Nothing)
            End Get
        End Property

        Friend ReadOnly Property Host() As FtpSession
            Get
                Return m_host
            End Get
        End Property

        Public ReadOnly Property ControlChannel() As ControlChannel Implements ISessionState.ControlChannel
            Get
                Return m_ctrlChannel
            End Get
        End Property

        ' You can only aborting file transfer started by
        ' BeginPutFile and BeginGetFile
        Public Sub AbortTransfer() Implements ISessionState.AbortTransfer

            ' Save a copy of m_dataStream since it will be set 
            ' to null when DataStream call EndDataTransfer
            Dim tempDataStream As DataStream = m_dataStream

            If Not (tempDataStream Is Nothing) Then
                tempDataStream.Abort()
                While Not tempDataStream.IsClosed
                    System.Threading.Thread.Sleep(0)
                End While
            End If

        End Sub

        Public Sub Close() Implements ISessionState.Close

            m_host.State = New SessionDisconnected(m_host, m_caseInsensitive)
            m_host.Server = m_ctrlChannel.Server
            m_host.Port = m_ctrlChannel.Port
            Try
                m_ctrlChannel.QUIT()
            Catch
                Return
            Finally
                m_ctrlChannel.Close()
            End Try

        End Sub

        Public Sub Connect(ByVal UserName As String, ByVal Password As String) Implements ISessionState.Connect

            Throw New InvalidOperationException

        End Sub

        Friend Sub BeginDataTransfer(ByVal stream As DataStream)

            SyncLock Me
                If Not m_dataStream Is Nothing Then
                    Throw New DataTransferException
                End If

                m_dataStream = stream
            End SyncLock

        End Sub

        Friend Sub EndDataTransfer()

            SyncLock Me
                If m_dataStream Is Nothing Then
                    Throw New InvalidOperationException
                End If

                m_dataStream = Nothing
            End SyncLock

        End Sub

    End Class

End Namespace