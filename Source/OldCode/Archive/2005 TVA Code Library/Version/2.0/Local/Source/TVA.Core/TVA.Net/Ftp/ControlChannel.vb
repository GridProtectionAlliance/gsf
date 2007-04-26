' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Net.Sockets
Imports System.Text.RegularExpressions

Namespace Net.Ftp

    Public Enum TransferMode
        Ascii
        Binary
        Unknown
    End Enum

    Public Class ControlChannel

        Private m_sessionHost As FtpSession
        Private m_session As SessionConnected
        Private m_connection As TcpClient
        Private m_server As String
        Private m_port As Integer
        Private m_currentTransferMode As TransferMode
        Private m_lastResponse As Response

        Private Shared m_regularExpression As New Regex("(\()(.*)(\))")
        Private Shared m_pwdExpression As New Regex("("")(.*)("")")

        Friend Sub New(ByVal host As FtpSession)

            m_connection = New TcpClient
            m_server = "localhost"
            m_port = 21
            m_sessionHost = host
            m_currentTransferMode = TransferMode.Unknown

        End Sub

        Friend Property Server() As String
            Get
                Return m_server
            End Get
            Set(ByVal Value As String)
                If Len(Value) = 0 Then
                    Throw New ArgumentNullException("Server", "Server property must not be blank.")
                End If
                m_server = Value
            End Set
        End Property

        Friend Property Port() As Integer
            Get
                Return m_port
            End Get
            Set(ByVal Value As Integer)
                m_port = Value
            End Set
        End Property

        Public ReadOnly Property LastResponse() As Response
            Get
                Return m_lastResponse
            End Get
        End Property

        Friend Sub Connect()

            m_connection.Connect(m_server, m_port)

            Try
                m_lastResponse = New Response(m_connection.GetStream())
                If m_lastResponse.Code <> Response.ServiceReady Then
                    Throw New ServerDownException("FTP service unavailable.", m_lastResponse)
                End If
            Catch ex As Exception
                Close()
                Throw
            End Try

        End Sub

        Friend Sub Close()

            Try
                m_connection.Close()
            Catch ex As Exception
                ' We keep going even if we can't close the connection
            End Try

            m_connection = Nothing

        End Sub

        Protected Overloads Overrides Sub Finalize()

            If Not m_connection Is Nothing Then m_connection.Close()
            MyBase.Finalize()

        End Sub

        Public Sub Command(ByVal cmd As String)

            Dim buff As Byte() = System.Text.Encoding.Default.GetBytes(cmd & ControlChars.Cr & ControlChars.Lf)
            Dim stream As NetworkStream = m_connection.GetStream()

            m_sessionHost.RaiseCommand(cmd)
            stream.Write(buff, 0, buff.Length)
            RefreshResponse()

        End Sub

        Public Sub RefreshResponse()

            Dim s As String

            SyncLock Me
                m_lastResponse = New Response(m_connection.GetStream())
            End SyncLock

            For Each s In m_lastResponse.Respones
                m_sessionHost.RaiseResponse(s)
            Next

        End Sub

        Friend Property Session() As SessionConnected
            Get
                Return m_session
            End Get
            Set(ByVal Value As SessionConnected)
                m_session = Value
            End Set
        End Property

        Friend Sub REST(ByVal offset As Long)

            Command("REST " & offset)
            If m_lastResponse.Code <> Response.RequestFileActionPending Then
                Throw New ResumeNotSupportedException(m_lastResponse)
            End If

        End Sub

        Friend Sub STOR(ByVal name As String)

            Type(TransferMode.Binary)
            Command("STOR " & name)
            If m_lastResponse.Code <> Response.DataChannelOpenedTransferStart AndAlso m_lastResponse.Code <> Response.FileOkBeginOpenDataChannel Then
                Throw New CommandException("Failed to send file " & name & ".", m_lastResponse)
            End If

        End Sub

        Friend Sub RETR(ByVal name As String)

            Type(TransferMode.Binary)
            Command("RETR " & name)
            If m_lastResponse.Code <> Response.DataChannelOpenedTransferStart AndAlso m_lastResponse.Code <> Response.FileOkBeginOpenDataChannel Then
                Throw New CommandException("Failed to retrieve file " & name & ".", m_lastResponse)
            End If

        End Sub

        Friend Sub DELE(ByVal fileName As String)

            Command("DELE " & fileName)
            If m_lastResponse.Code <> Response.RequestFileActionComplete Then    ' 250)
                Throw New CommandException("Failed to delete file " & fileName & ".", m_lastResponse)
            End If

        End Sub

        Friend Sub RMD(ByVal dirName As String)

            Command("RMD " & dirName)
            If m_lastResponse.Code <> Response.RequestFileActionComplete Then    ' 250)
                Throw New CommandException("Failed to remove subdirectory " & dirName & ".", m_lastResponse)
            End If

        End Sub

        Friend Function PWD() As String

            Command("PWD")
            If m_lastResponse.Code <> 257 Then
                Throw New CommandException("Cannot get current directory.", m_lastResponse)
            End If

            Dim m As Match = m_pwdExpression.Match(m_lastResponse.Message)
            Return m.Groups(2).Value

        End Function

        Friend Sub CDUP()

            Command("CDUP")
            If m_lastResponse.Code <> Response.RequestFileActionComplete Then
                Throw New CommandException("Cannot move to parent directory (CDUP).", m_lastResponse)
            End If

        End Sub

        Friend Sub CWD(ByVal path As String)

            Command("CWD " & path)
            If m_lastResponse.Code <> Response.RequestFileActionComplete AndAlso m_lastResponse.Code <> Response.ClosingDataChannel Then
                Throw New CommandException("Cannot change directory to " & path & ".", m_lastResponse)
            End If

        End Sub

        Friend Sub QUIT()

            Command("QUIT")

        End Sub

        Friend Sub Type(ByVal mode As TransferMode)

            If mode = TransferMode.Unknown Then Return

            If mode = TransferMode.Ascii AndAlso m_currentTransferMode <> TransferMode.Ascii Then
                Command("TYPE A")
            ElseIf mode = TransferMode.Binary AndAlso m_currentTransferMode <> TransferMode.Binary Then
                Command("TYPE I")
            End If

            m_currentTransferMode = mode

        End Sub

        Friend Sub Rename(ByVal oldName As String, ByVal newName As String)

            Command("RNFR " & oldName)
            If LastResponse.Code <> Response.RequestFileActionPending Then
                Throw New CommandException("Failed to rename file from " & oldName & " to " & newName & ".", m_lastResponse)
            End If

            Command("RNTO " & newName)
            If LastResponse.Code <> Response.RequestFileActionComplete Then
                Throw New CommandException("Failed to rename file from " & oldName & " to " & newName & ".", m_lastResponse)
            End If

        End Sub

        Friend Function List(ByVal passive As Boolean) As Queue

            Const errorMsgListing As String = "Error when listing server directory."

            Try
                Type(TransferMode.Ascii)
                Dim dataStream As DataStream = GetPassiveDataStream()
                Dim lineQueue As New Queue

                Command("LIST")

                If m_lastResponse.Code <> Response.DataChannelOpenedTransferStart AndAlso m_lastResponse.Code <> Response.FileOkBeginOpenDataChannel Then
                    Throw New CommandException(errorMsgListing, m_lastResponse)
                End If

                Dim lineReader As New StreamReader(dataStream, System.Text.Encoding.Default)
                Dim line As String = lineReader.ReadLine()

                While Not line Is Nothing
                    lineQueue.Enqueue(line)
                    line = lineReader.ReadLine()
                End While

                lineReader.Close()

                If m_lastResponse.Code <> Response.ClosingDataChannel Then
                    Throw New CommandException(errorMsgListing, m_lastResponse)
                End If

                Return lineQueue
            Catch ie As IOException
                Throw New System.Exception(errorMsgListing, ie)
            Catch se As SocketException
                Throw New System.Exception(errorMsgListing, se)
            End Try

        End Function

        Friend Overloads Function GetPassiveDataStream() As DataStream

            Return GetPassiveDataStream(TransferDirection.Download)

        End Function

        Friend Overloads Function GetPassiveDataStream(ByVal direction As TransferDirection) As DataStream

            Dim client As New TcpClient
            Dim port As Integer

            Try
                port = GetPassivePort()
                client.Connect(m_server, port)
                If direction = TransferDirection.Download Then
                    Return New InputDataStream(Me, client)
                Else
                    Return New OutputDataStream(Me, client)
                End If
            Catch ie As IOException
                Throw New System.Exception("Failed to open passive port (" & port & ") data connection due to IO exception: " & ie.Message & ".", ie)
            Catch se As SocketException
                Throw New System.Exception("Failed to open passive port (" & port & ") data connection due to socket exception: " & se.Message & ".", se)
            End Try

        End Function

        Private Function GetPassivePort() As Integer

            Command("PASV")

            If m_lastResponse.Code = Response.EnterPassiveMode Then
                Dim numbers As String() = m_regularExpression.Match(m_lastResponse.Message).Groups(2).Value.Split(","c)
                Return Integer.Parse(numbers(4)) * 256 + Integer.Parse(numbers(5))
            Else
                Throw New CommandException("Failed to enter passive mode.", m_lastResponse)
            End If

        End Function

    End Class

End Namespace