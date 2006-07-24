' 07-24-06

Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.ComponentModel
Imports Tva.Common
Imports Tva.IO.Common

Public Class FileClient

    Private m_receiveOnDemand As Boolean
    Private m_fileClient As StateKeeper(Of FileStream)
    Private m_connectionData As Dictionary(Of String, String)
    Private m_connectionThread As Thread
    Private m_receivingThread As Thread

    Public Sub New(ByVal connectionString As String)
        MyClass.New()
        MyBase.ConnectionString = connectionString
    End Sub

    <Category("Data"), DefaultValue(GetType(Boolean), "False")> _
    Public Property ReceiveOnDemand() As Boolean
        Get
            Return m_receiveOnDemand
        End Get
        Set(ByVal value As Boolean)
            m_receiveOnDemand = value
        End Set
    End Property

    Public Overrides Sub CancelConnect()

        If Enabled() AndAlso m_connectionThread IsNot Nothing Then
            m_connectionThread.Abort()
        End If

    End Sub

    Public Overrides Sub Connect()

        If Enabled() AndAlso Not IsConnected() AndAlso ValidConnectionString(ConnectionString()) Then
            If File.Exists(m_connectionData("file")) Then
                m_connectionThread = New Thread(AddressOf OpenFile)
                m_connectionThread.Start()
            Else
                Throw New FileNotFoundException(m_connectionData("file") & " does not exist.")
            End If
        End If

    End Sub

    Public Overrides Sub Disconnect()

        CancelConnect()

        If Enabled() AndAlso IsConnected() AndAlso m_receivingThread IsNot Nothing Then
            m_receivingThread.Abort()
            m_fileClient.Client.Close()
            OnDisconnected(EventArgs.Empty)
        End If

    End Sub

    Public Sub ReceiveData()

        If Enabled() AndAlso IsConnected() AndAlso m_receiveOnDemand AndAlso m_receivingThread Is Nothing Then
            m_receivingThread = New Thread(AddressOf ReadFile)
            m_receivingThread.Start()
        End If

    End Sub

    Protected Overrides Sub SendPreparedData(ByVal data() As Byte)

        Throw New NotSupportedException()

    End Sub

    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("file") Then
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   File=<Name of the file>")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    Private Sub OpenFile()

        Dim connectionAttempts As Integer = 0
        Do While MaximumConnectionAttempts() = -1 OrElse connectionAttempts < MaximumConnectionAttempts()
            Try
                OnConnecting(EventArgs.Empty)
                m_fileClient.Client = New FileStream(m_connectionData("file"), FileMode.Open)
                OnConnected(EventArgs.Empty)
                If Not m_receiveOnDemand Then
                    m_receivingThread = New Thread(AddressOf ReadFile)
                    m_receivingThread.Start()
                End If

                m_connectionThread = Nothing
                Exit Do
            Catch ex As ThreadAbortException
                Exit Do
                OnConnectingCancelled(EventArgs.Empty)
            Catch ex As Exception
                connectionAttempts += 1
                OnConnectingException(ex)
            End Try
        Loop

    End Sub

    Private Sub ReadFile()

        Try
            With m_fileClient
                Do While .Client.Position() < .Client.Length()
                    .DataBuffer = CreateArray(Of Byte)(ReceiveBufferSize())
                    .BytesReceived = .Client.Read(.DataBuffer(), 0, .DataBuffer.Length())
                    .DataBuffer = CopyBuffer(.DataBuffer(), 0, .BytesReceived())

                    OnReceivedData(.DataBuffer())

                    If m_receiveOnDemand Then Exit Do
                Loop
            End With
            m_receivingThread = Nothing
        Catch ex As Exception

        End Try

    End Sub

End Class
