' 07-27-06

Imports System.Text
Imports System.IO.Ports
Imports System.Threading
Imports Tva.Common
Imports Tva.IO.Common

Public Class SerialClient

    Private m_connectionThread As Thread
    Private m_connectionData As Dictionary(Of String, String)
    Private WithEvents m_serialClient As SerialPort

    Public Sub New(ByVal connectionString As String)
        MyClass.New()
        MyBase.ConnectionString = connectionString
    End Sub

    Public Overrides Sub CancelConnect()

        If Enabled() AndAlso m_connectionThread IsNot Nothing Then
            m_connectionThread.Abort()
            OnConnectingCancelled(EventArgs.Empty)
        End If

    End Sub

    Public Overrides Sub Connect()

        If Enabled() AndAlso Not IsConnected() AndAlso ValidConnectionString(ConnectionString()) Then
            With m_serialClient
                .PortName = m_connectionData("port")
                .BaudRate = Convert.ToInt32(m_connectionData("baudrate"))
                .DataBits = Convert.ToInt32(m_connectionData("databits"))
                .Parity = CType(System.Enum.Parse(GetType(Parity), m_connectionData("parity")), Parity)
                .StopBits = CType(System.Enum.Parse(GetType(StopBits), m_connectionData("stopbits")), StopBits)
            End With

            m_connectionThread = New Thread(AddressOf ConnectToPort)
            m_connectionThread.Start()
        End If

    End Sub

    Public Overrides Sub Disconnect()

        CancelConnect()

        If Enabled() AndAlso IsConnected() Then
            m_serialClient.Close()
            OnDisconnected(EventArgs.Empty)
        End If

    End Sub

    Protected Overrides Sub SendPreparedData(ByVal data() As Byte)

        If Enabled() And IsConnected() Then
            OnSendDataBegin(data)
            m_serialClient.Write(data, 0, data.Length())
            OnSendDataComplete(data)
        End If

    End Sub

    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("port") AndAlso m_connectionData.ContainsKey("baudrate") AndAlso _
                    m_connectionData.ContainsKey("parity") AndAlso m_connectionData.ContainsKey("stopbits") AndAlso _
                    m_connectionData.ContainsKey("databits") Then
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   Port=[Name of the COM port]; BaudRate=[9600|4800|2400|1200]; Parity=[None|Odd|Even|Mark|Space]; StopBits=[None|One|Two|OnePointFive]; DataBits=[Number of data bits per byte]")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    Private Sub ConnectToPort()

        Dim connectionAttempts As Integer = 0
        Do While MaximumConnectionAttempts() = -1 OrElse connectionAttempts < MaximumConnectionAttempts()
            Try
                OnConnecting(EventArgs.Empty)
                m_serialClient.Open()
                OnConnected(EventArgs.Empty)

                Exit Do
            Catch ex As ThreadAbortException
                Exit Do
            Catch ex As Exception
                OnConnectingException(ex)
            Finally
                connectionAttempts += 1
            End Try
        Loop
        m_connectionThread = Nothing

    End Sub

    Private Sub m_serialClient_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles m_serialClient.DataReceived

        Dim data As Byte() = CreateArray(Of Byte)(ReceiveBufferSize())
        Dim dataLength As Integer = m_serialClient.Read(data, 0, data.Length())
        data = CopyBuffer(data, 0, dataLength)

        OnReceivedData(data)

    End Sub

End Class
