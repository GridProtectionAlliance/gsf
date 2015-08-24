' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Threading
Imports System.Net.Sockets
Imports TVA.Shared.FilePath

Namespace Ftp

	Public Enum TransferDirection
		Upload
		Download
	End Enum

	Friend Class FileTransferer

		Delegate Sub FileCommandDelegate(ByVal remoteFileName As String)
		Delegate Sub StreamCopyDelegate(ByVal remote As Stream, ByVal local As Stream)

		Private m_streamCopyRoutine As StreamCopyDelegate
		Private m_ftpFileCommandRoutine As FileCommandDelegate

		Private m_transferStarter As Directory
        Private m_session As SessionConnected
        Private m_localFile As String
        Private m_remoteFile As String
        Private m_totalBytes As Long
        Private m_totalBytesTransfered As Long
        Private m_transferedPercentage As Integer
        Private m_transferDirection As TransferDirection
        Private m_localFileOpenMode As FileMode
        Private m_transferResult As AsyncResult

        Public ReadOnly Property LocalFileName() As String
            Get
                Return m_localFile
            End Get
        End Property

        Public ReadOnly Property RemoteFileName() As String
            Get
                Return m_remoteFile
            End Get
        End Property

        Public ReadOnly Property TotalBytes() As Long
            Get
                Return m_totalBytes
            End Get
        End Property

        Public ReadOnly Property TotalBytesTransfered() As Long
            Get
                Return m_totalBytesTransfered
            End Get
        End Property

        Public ReadOnly Property TransferDirection() As TransferDirection
            Get
                Return m_transferDirection
            End Get
        End Property

        Public ReadOnly Property TransferResult() As AsyncResult
            Get
                Return m_transferResult
            End Get
        End Property

        Public ReadOnly Property TransferedPercentage() As Integer
            Get
                Return m_transferedPercentage
            End Get
        End Property

        Friend Sub New(ByVal transferStarter As Directory, ByVal localFile As String, ByVal remoteFile As String, ByVal totalBytes As Long, ByVal dir As TransferDirection)

            m_transferStarter = transferStarter
            m_transferDirection = dir
            m_session = transferStarter.Session
            m_localFile = localFile
            m_remoteFile = remoteFile
            m_totalBytes = totalBytes

            If dir = TransferDirection.Upload Then
                m_streamCopyRoutine = AddressOf LocalToRemote
                m_ftpFileCommandRoutine = AddressOf m_session.ControlChannel.STOR
                m_localFileOpenMode = FileMode.Open
            Else
                m_streamCopyRoutine = AddressOf RemoteToLocal
                m_ftpFileCommandRoutine = AddressOf m_session.ControlChannel.RETR
                m_localFileOpenMode = FileMode.Create
            End If

        End Sub

        Private Sub TransferThreadProc()

            Try
                StartTransfer()
                m_session.Host.RaiseFileTranferNotification(New AsyncResult("Success.", AsyncResult.Complete))
            Catch e As Exception
                m_session.Host.RaiseFileTranferNotification(New AsyncResult("Transfer fail: " & e.Message, AsyncResult.Fail))
            End Try

        End Sub

        Friend Sub StartTransfer()

            Dim localStream As FileStream = Nothing
            Dim remoteStream As DataStream = Nothing

            Try
                ' Files just created may still have a file lock, we'll wait a few seconds for read access if needed...
                If m_transferDirection = TransferDirection.Upload Then
                    WaitForReadLock(m_localFile, m_session.Host.WaitLockTimeout)
                End If

                m_session.Host.RaiseBeginFileTransfer(m_localFile, m_remoteFile, m_transferDirection)

                localStream = New FileStream(m_localFile, m_localFileOpenMode)
                remoteStream = m_session.ControlChannel.GetPassiveDataStream(m_transferDirection)

                m_ftpFileCommandRoutine(m_remoteFile)
                m_streamCopyRoutine(remoteStream, localStream)

                remoteStream.Close()
                TestTransferResult()

                m_session.Host.RaiseEndFileTransfer(m_localFile, m_remoteFile, m_transferDirection, m_transferResult)
            Catch ex As Exception
                If Not remoteStream Is Nothing Then remoteStream.Close()
                If Not localStream Is Nothing Then localStream.Close()
                m_session.Host.RaiseEndFileTransfer(m_localFile, m_remoteFile, m_transferDirection, m_transferResult)
                Throw
            Finally
                If Not remoteStream Is Nothing Then remoteStream.Close()
                If Not localStream Is Nothing Then localStream.Close()
            End Try

        End Sub

        Friend Sub StartAsyncTransfer()

            Dim thread As New Thread(New ThreadStart(AddressOf TransferThreadProc))

            thread.Name = "Transfer file thread: " & m_remoteFile
            thread.Start()

        End Sub

        Private Sub TestTransferResult()

            Dim responseCode As Integer = m_session.ControlChannel.LastResponse.Code

            If responseCode = Response.ClosingDataChannel Then
                Return
            End If

            If responseCode = Response.RequestFileActionComplete Then
                Return
            End If

            Throw New DataTransferException("Failed to transfer file.", m_session.ControlChannel.LastResponse)

        End Sub

        Private Sub RemoteToLocal(ByVal remote As Stream, ByVal local As Stream)

            StreamCopy(local, remote)

        End Sub

        Private Sub LocalToRemote(ByVal remote As Stream, ByVal local As Stream)

            StreamCopy(remote, local)

        End Sub

        Private Sub StreamCopy(ByVal dest As Stream, ByVal [source] As Stream)

            Dim byteRead As Integer
            Dim onePercentage, bytesReadFromLastProgressEvent As Long
            Dim buffer(4 * 1024) As Byte

            onePercentage = m_totalBytes / 100
            bytesReadFromLastProgressEvent = 0
            byteRead = [source].Read(buffer, 0, 4 * 1024)

            While byteRead <> 0
                m_totalBytesTransfered += byteRead
                bytesReadFromLastProgressEvent += byteRead

                If bytesReadFromLastProgressEvent > onePercentage Then
                    m_transferedPercentage = CInt(CSng(m_totalBytesTransfered) / CSng(m_totalBytes) * 100)
                    m_session.Host.RaiseFileTransferProgress(m_totalBytes, m_totalBytesTransfered, m_transferDirection)
                    bytesReadFromLastProgressEvent = 0
                End If

                dest.Write(buffer, 0, byteRead)
                byteRead = [source].Read(buffer, 0, 4 * 1024)
            End While

        End Sub

    End Class

End Namespace