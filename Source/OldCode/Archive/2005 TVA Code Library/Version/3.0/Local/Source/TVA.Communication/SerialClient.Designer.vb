' 07-27-06

Partial Class SerialClient
    Inherits TVA.Communication.CommunicationClientBase

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New(ByVal Container As System.ComponentModel.IContainer)
        MyClass.New()

        'Required for Windows.Forms Class Composition Designer support
        Container.Add(Me)

    End Sub

    <System.Diagnostics.DebuggerNonUserCode()> _
    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

#If ThreadTracking Then
        m_connectionThread = New TVA.Threading.ManagedThread(AddressOf ConnectToPort)
        m_connectionThread.Name = "TVA.Communication.SerialClient.ConnectToPort()"
#Else
        m_connectionThread = New System.Threading.Thread(AddressOf ConnectToPort)
#End If
        m_serialClient = New System.IO.Ports.SerialPort()
        MyBase.ConnectionString = "Port=COM1; BaudRate=9600; Parity=None; StopBits=One; DataBits=8; DtrEnable=False; RtsEnable=False"
        MyBase.Protocol = TransportProtocol.Serial

    End Sub

    'Component overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
    End Sub

End Class
