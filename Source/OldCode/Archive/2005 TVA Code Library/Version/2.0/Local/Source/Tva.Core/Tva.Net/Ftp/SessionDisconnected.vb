' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Text

Namespace Net.Ftp

    Friend Class SessionDisconnected

        Implements ISessionState

        Private m_host As Session
        Private m_server As String
        Private m_port As Integer
        Private m_caseInsensitive As Boolean

        Friend Sub New(ByVal h As Session, ByVal CaseInsensitive As Boolean)

            m_port = 21
            m_host = h
            m_caseInsensitive = CaseInsensitive

        End Sub

        Public Property Server() As String Implements ISessionState.Server
            Get
                Return m_server
            End Get
            Set(ByVal Value As String)
                m_server = Value
            End Set
        End Property

        Public Property Port() As Integer Implements ISessionState.Port
            Get
                Return m_port
            End Get
            Set(ByVal Value As Integer)
                m_port = Value
            End Set
        End Property

        Public Sub Connect(ByVal UserName As String, ByVal Password As String) Implements ISessionState.Connect

            Dim ctrl As New ControlChannel(m_host)

            ctrl.Server = m_server
            ctrl.Port = m_port
            ctrl.Connect()

            Try
                ctrl.Command("USER " & UserName)

                If ctrl.LastResponse.Code = Response.UserAcceptedWaitingPass Then
                    ctrl.Command("PASS " & Password)
                End If

                If ctrl.LastResponse.Code <> Response.UserLoggedIn Then
                    Throw New AuthenticationException("Failed to login.", ctrl.LastResponse)
                End If

                m_host.State = New SessionConnected(m_host, ctrl, m_caseInsensitive)
                CType(m_host.State, SessionConnected).InitRootDirectory()
            Catch
                ctrl.Close()
                Throw
            End Try

        End Sub

        Public Property CurrentDirectory() As Directory Implements ISessionState.CurrentDirectory
            Get
                Throw New InvalidOperationException
            End Get
            Set(ByVal Value As Directory)
                Throw New InvalidOperationException
            End Set
        End Property

        Public ReadOnly Property ControlChannel() As ControlChannel Implements ISessionState.ControlChannel
            Get
                Throw New InvalidOperationException
            End Get
        End Property

        Public ReadOnly Property IsBusy() As Boolean Implements ISessionState.IsBusy
            Get
                Throw New InvalidOperationException
            End Get
        End Property

        Public ReadOnly Property RootDirectory() As Directory Implements ISessionState.RootDirectory
            Get
                Throw New InvalidOperationException
            End Get
        End Property

        Public Sub AbortTransfer() Implements ISessionState.AbortTransfer

            Throw New InvalidOperationException

        End Sub

        Public Sub Close() Implements ISessionState.Close

            ' Nothing to do - don't want to throw an error...

        End Sub

    End Class

End Namespace