' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Net.Sockets

Namespace Ftp

	Public Class DataStream

		Inherits Stream

		Private m_ctrl As ControlChannel
        Private m_session As SessionConnected
        Private m_tcpClient As TcpClient
        Private m_stream As Stream
        Private m_userAbort As Boolean

        Friend Sub New(ByVal ctrl As ControlChannel, ByVal client As TcpClient)

            m_session = ctrl.Session
            m_ctrl = ctrl
            m_tcpClient = client
            m_stream = client.GetStream()
            m_session.BeginDataTransfer(Me)

        End Sub

        Public Overrides Sub Close()

            If Not IsClosed Then
                CloseConnection()
                m_ctrl.RefreshResponse()
                m_ctrl.Session.EndDataTransfer()
            End If

        End Sub

        Friend Sub Abort()

            m_userAbort = True

        End Sub

        Private Sub CloseConnection()

            m_stream.Close()
            m_tcpClient.Close()
            m_tcpClient = Nothing

        End Sub

        Friend ReadOnly Property IsClosed() As Boolean
            Get
                Return m_tcpClient Is Nothing
            End Get
        End Property

        Friend ReadOnly Property ControlChannel() As ControlChannel
            Get
                Return m_ctrl
            End Get
        End Property

        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return m_stream.CanRead
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return m_stream.CanSeek
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return m_stream.CanWrite
            End Get
        End Property

        Public Overrides ReadOnly Property Length() As Long
            Get
                Return m_stream.Length
            End Get
        End Property

        Public Overrides Property Position() As Long
            Get
                Return m_stream.Position
            End Get
            Set(ByVal Value As Long)
                m_stream.Position = Value
            End Set
        End Property

        Public Overrides Sub Flush()

            m_stream.Flush()

        End Sub

        Public Overrides Function Seek(ByVal offset As Long, ByVal origin As SeekOrigin) As Long

            Return m_stream.Seek(offset, origin)

        End Function

        Public Overrides Function Read(ByVal buffer() As Byte, ByVal offset As Integer, ByVal count As Integer) As Integer

            Try
                Return m_stream.Read(buffer, offset, count)
            Finally
                If m_userAbort Then
                    Throw New UserAbortException()
                End If
            End Try

        End Function

        Public Overrides Sub Write(ByVal buffer() As Byte, ByVal offset As Integer, ByVal count As Integer)

            Try
                m_stream.Write(buffer, offset, count)
            Finally
                If m_userAbort Then
                    Throw New UserAbortException()
                End If
            End Try

        End Sub

        Public Overrides Function ReadByte() As Integer

            Try
                Return m_stream.ReadByte()
            Finally
                If m_userAbort Then
                    Throw New UserAbortException()
                End If
            End Try

        End Function

        Public Overrides Sub WriteByte(ByVal b As Byte)

            Try
                m_stream.WriteByte(b)
            Finally
                If m_userAbort Then
                    Throw New UserAbortException()
                End If
            End Try

        End Sub

        Public Overrides Sub SetLength(ByVal len As Long)

            m_stream.SetLength(len)

        End Sub

    End Class

	Public Class InputDataStream

		Inherits DataStream

		Friend Sub New(ByVal ctrl As ControlChannel, ByVal client As TcpClient)

			MyBase.New(ctrl, client)

		End Sub

		Public Overrides Sub Write(ByVal buffer() As Byte, ByVal offset As Integer, ByVal count As Integer)

			Throw New NotSupportedException()

		End Sub

		Public Overrides Sub WriteByte(ByVal b As Byte)

			Throw New NotSupportedException()

		End Sub

		Public Overrides ReadOnly Property CanWrite() As Boolean
			Get
				Return False
			End Get
		End Property

	End Class

	Public Class OutputDataStream

		Inherits DataStream

		Friend Sub New(ByVal ctrl As ControlChannel, ByVal client As TcpClient)

			MyBase.New(ctrl, client)

		End Sub

		Public Overrides Function Read(ByVal buffer() As Byte, ByVal offset As Integer, ByVal count As Integer) As Integer

			Throw New NotSupportedException()

		End Function

		Public Overrides Function ReadByte() As Integer

			Throw New NotSupportedException()

		End Function

		Public Overrides ReadOnly Property CanRead() As Boolean
			Get
				Return False
			End Get
		End Property

	End Class

End Namespace