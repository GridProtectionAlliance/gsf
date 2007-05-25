'*******************************************************************************************************
'  TVA.Communication.SerialClient.vb - Serial port communication client
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  07/24/2006 - Pinal C. Patel
'       Original version of source code generated
'  09/06/2006 - J. Ritchie Carroll
'       Added bypass optimizations for high-speed serial port data access
'
'*******************************************************************************************************

Imports System.Text
Imports System.IO.Ports
Imports System.Threading
Imports System.ComponentModel
Imports TVA.Common
Imports TVA.IO.Common
Imports TVA.ErrorManagement

''' <summary>
''' Represents a serial port communication client.
''' </summary>
<DisplayName("Serial Client")> _
Public Class SerialClient

#Region " Member Declaration "

    Private m_connectionThread As Thread
    Private m_connectionData As Dictionary(Of String, String)
    Private WithEvents m_serialClient As SerialPort

#End Region

#Region " Code Scope: Public "

    ''' <summary>
    ''' Initializes a instance of TVA.Communication.SerialClient with the specified data.
    ''' </summary>
    ''' <param name="connectionString">The data that is required by the client to initialize.</param>
    Public Sub New(ByVal connectionString As String)

        MyClass.New()
        MyBase.ConnectionString = connectionString

    End Sub

    ''' <summary>
    ''' Cancels any active attempts of connecting to the serial port.
    ''' </summary>
    Public Overrides Sub CancelConnect()

        If MyBase.Enabled AndAlso m_connectionThread.IsAlive Then m_connectionThread.Abort()

    End Sub

    ''' <summary>
    ''' Connects to the serial port asynchronously.
    ''' </summary>
    Public Overrides Sub Connect()

        If MyBase.Enabled AndAlso Not MyBase.IsConnected AndAlso ValidConnectionString(ConnectionString()) Then
            With m_serialClient
                .PortName = m_connectionData("port")
                .BaudRate = Convert.ToInt32(m_connectionData("baudrate"))
                .DataBits = Convert.ToInt32(m_connectionData("databits"))
                .Parity = CType(System.Enum.Parse(GetType(Parity), m_connectionData("parity")), Parity)
                .StopBits = CType(System.Enum.Parse(GetType(StopBits), m_connectionData("stopbits")), StopBits)
                If m_connectionData.ContainsKey("dtrenable") Then .DtrEnable = Convert.ToBoolean(m_connectionData("dtrenable"))
                If m_connectionData.ContainsKey("rtsenable") Then .RtsEnable = Convert.ToBoolean(m_connectionData("rtsenable"))
            End With

            m_connectionThread = New Thread(AddressOf ConnectToPort)
            m_connectionThread.Start()
        End If

    End Sub

    ''' <summary>
    ''' Disconnects from serial port.
    ''' </summary>
    Public Overrides Sub Disconnect()

        CancelConnect()

        If MyBase.Enabled AndAlso MyBase.IsConnected Then
            m_serialClient.Close()
            OnDisconnected(EventArgs.Empty)
        End If

    End Sub

#End Region

#Region " Code Scope: Protected "

    ''' <summary>
    ''' Sends prepared data to the server.
    ''' </summary>
    ''' <param name="data">The prepared data that is to be sent to the server.</param>
    Protected Overrides Sub SendPreparedData(ByVal data As Byte())

        If MyBase.Enabled And MyBase.IsConnected Then
            OnSendDataBegin(New IdentifiableItem(Of Guid, Byte())(ClientID, data))
            m_serialClient.Write(data, 0, data.Length())
            OnSendDataComplete(New IdentifiableItem(Of Guid, Byte())(ClientID, data))
        End If

    End Sub

    ''' <summary>
    ''' Determines whether specified connection string required for connecting to the serial port is valid.
    ''' </summary>
    ''' <param name="connectionString">The connection string to be validated.</param>
    ''' <returns>True is the connection string is valid; otherwise False.</returns>
    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = TVA.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("port") AndAlso m_connectionData.ContainsKey("baudrate") AndAlso _
                    m_connectionData.ContainsKey("parity") AndAlso m_connectionData.ContainsKey("stopbits") AndAlso _
                    m_connectionData.ContainsKey("databits") Then
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine)
                    .Append("   Port=[Name of the COM port]; BaudRate=[9600|4800|2400|1200]; Parity=[None|Odd|Even|Mark|Space]; StopBits=[None|One|Two|OnePointFive]; DataBits=[Number of data bits per byte]")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

#End Region

#Region " Code Scope: Private "

    ''' <summary>
    ''' Connects to the serial port.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ConnectToPort()

        Dim connectionAttempts As Integer = 0

        Do While MaximumConnectionAttempts = -1 OrElse connectionAttempts < MaximumConnectionAttempts
            Try
                OnConnecting(EventArgs.Empty)
                m_serialClient.Open()
                OnConnected(EventArgs.Empty)

                Exit Do
            Catch ex As ThreadAbortException
                OnConnectingCancelled(EventArgs.Empty)
                Exit Do
            Catch ex As Exception
                connectionAttempts += 1
                OnConnectingException(ex)
            End Try
        Loop

    End Sub

    ''' <summary>
    ''' Receive data from the serial port (.NET serial port class raises this event when data is available)
    ''' </summary>
    Private Sub m_serialClient_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles m_serialClient.DataReceived

        Dim received As Integer

        ' JRC: modified this code to make sure all available data on the serial port buffer is read, regardless of size of communication buffer
        For x As Integer = 1 To Convert.ToInt32(System.Math.Ceiling(m_serialClient.BytesToRead / m_buffer.Length))
            ' Retrieve data from the serial port
            received = m_serialClient.Read(m_buffer, 0, m_buffer.Length)

            ' Post raw data to real-time function delegate if defined - this bypasses all other activity
            If m_receiveRawDataFunction IsNot Nothing Then
                m_receiveRawDataFunction(m_buffer, 0, received)
                m_totalBytesReceived += received
            Else
                ' Unpack data and make available via event
                OnReceivedData(New IdentifiableItem(Of Guid, Byte())(ServerID, CopyBuffer(m_buffer, 0, received)))
            End If
        Next

    End Sub

#End Region

End Class
