Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports TVA.Forms.Common
Imports TVA.Config.Common
Imports TVA.Threading

Public Class UDPListener
    Inherits System.Windows.Forms.Form

    Private listeningThread As RunThread

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents UDPFrame As System.Windows.Forms.Label
    Friend WithEvents SampleCount As System.Windows.Forms.Label
    Friend WithEvents SampleRate As System.Windows.Forms.Label
    Friend WithEvents SamplesLabel As System.Windows.Forms.Label
    Friend WithEvents SampleRateLabel As System.Windows.Forms.Label
    Friend WithEvents SamplesPerSecLabel As System.Windows.Forms.Label
    Friend WithEvents ListenOnPortLabel As System.Windows.Forms.Label
    Friend WithEvents Port As Infragistics.Win.UltraWinEditors.UltraNumericEditor
    Friend WithEvents Listen As System.Windows.Forms.Button
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(UDPListener))
        Me.UDPFrame = New System.Windows.Forms.Label
        Me.SamplesLabel = New System.Windows.Forms.Label
        Me.SampleCount = New System.Windows.Forms.Label
        Me.SampleRate = New System.Windows.Forms.Label
        Me.SampleRateLabel = New System.Windows.Forms.Label
        Me.SamplesPerSecLabel = New System.Windows.Forms.Label
        Me.ListenOnPortLabel = New System.Windows.Forms.Label
        Me.Port = New Infragistics.Win.UltraWinEditors.UltraNumericEditor
        Me.Listen = New System.Windows.Forms.Button
        CType(Me.Port, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'UDPFrame
        '
        Me.UDPFrame.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.UDPFrame.Font = New System.Drawing.Font("Courier New", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.UDPFrame.Location = New System.Drawing.Point(8, 64)
        Me.UDPFrame.Name = "UDPFrame"
        Me.UDPFrame.Size = New System.Drawing.Size(400, 152)
        Me.UDPFrame.TabIndex = 8
        Me.UDPFrame.Text = "UDPFrame"
        '
        'SamplesLabel
        '
        Me.SamplesLabel.Location = New System.Drawing.Point(8, 40)
        Me.SamplesLabel.Name = "SamplesLabel"
        Me.SamplesLabel.Size = New System.Drawing.Size(48, 16)
        Me.SamplesLabel.TabIndex = 3
        Me.SamplesLabel.Text = "Samples:"
        '
        'SampleCount
        '
        Me.SampleCount.Location = New System.Drawing.Point(56, 40)
        Me.SampleCount.Name = "SampleCount"
        Me.SampleCount.Size = New System.Drawing.Size(64, 16)
        Me.SampleCount.TabIndex = 4
        Me.SampleCount.Text = "0"
        '
        'SampleRate
        '
        Me.SampleRate.Location = New System.Drawing.Point(192, 40)
        Me.SampleRate.Name = "SampleRate"
        Me.SampleRate.Size = New System.Drawing.Size(72, 16)
        Me.SampleRate.TabIndex = 6
        Me.SampleRate.Text = "0"
        Me.SampleRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'SampleRateLabel
        '
        Me.SampleRateLabel.Location = New System.Drawing.Point(120, 40)
        Me.SampleRateLabel.Name = "SampleRateLabel"
        Me.SampleRateLabel.Size = New System.Drawing.Size(80, 16)
        Me.SampleRateLabel.TabIndex = 5
        Me.SampleRateLabel.Text = "Sample Rate:"
        '
        'SamplesPerSecLabel
        '
        Me.SamplesPerSecLabel.Location = New System.Drawing.Point(264, 40)
        Me.SamplesPerSecLabel.Name = "SamplesPerSecLabel"
        Me.SamplesPerSecLabel.Size = New System.Drawing.Size(88, 16)
        Me.SamplesPerSecLabel.TabIndex = 7
        Me.SamplesPerSecLabel.Text = "samples/second"
        '
        'ListenOnPortLabel
        '
        Me.ListenOnPortLabel.Location = New System.Drawing.Point(8, 8)
        Me.ListenOnPortLabel.Name = "ListenOnPortLabel"
        Me.ListenOnPortLabel.Size = New System.Drawing.Size(80, 16)
        Me.ListenOnPortLabel.TabIndex = 0
        Me.ListenOnPortLabel.Text = "Listen on &port:"
        '
        'Port
        '
        Me.Port.FormatString = "0"
        Me.Port.Location = New System.Drawing.Point(80, 8)
        Me.Port.MaskInput = "nnnnn"
        Me.Port.MaxValue = 32768
        Me.Port.MinValue = 80
        Me.Port.Name = "Port"
        Me.Port.NullText = "3060"
        Me.Port.PromptChar = Microsoft.VisualBasic.ChrW(32)
        Me.Port.Size = New System.Drawing.Size(48, 21)
        Me.Port.TabIndex = 1
        Me.Port.Value = 3060
        '
        'Listen
        '
        Me.Listen.Location = New System.Drawing.Point(136, 8)
        Me.Listen.Name = "Listen"
        Me.Listen.TabIndex = 2
        Me.Listen.Text = "&Listen"
        '
        'UDPListener
        '
        Me.AcceptButton = Me.Listen
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(416, 221)
        Me.Controls.Add(Me.Listen)
        Me.Controls.Add(Me.Port)
        Me.Controls.Add(Me.ListenOnPortLabel)
        Me.Controls.Add(Me.SamplesPerSecLabel)
        Me.Controls.Add(Me.SampleRate)
        Me.Controls.Add(Me.SampleRateLabel)
        Me.Controls.Add(Me.SampleCount)
        Me.Controls.Add(Me.SamplesLabel)
        Me.Controls.Add(Me.UDPFrame)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(424, 248)
        Me.Name = "UDPListener"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = " PMU Sample Rate Calculator"
        CType(Me.Port, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub SampleRateCalcs_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Variables.Create("Port", 3060, VariableType.Int)

        Port.Value = Variables("Port")
        RestoreWindowLocation(Me)

        Variables.Save()

        '' TODO: to be removed...  
        'CalcCheckSum(Application.StartupPath & "\TVA PDC Sample Descriptor.bin", "f0 ba")    ' TVA gen
        'CalcCheckSum(Application.StartupPath & "\BPA PDC Sample Descriptor.bin", "bb c0")    ' PDC gen
        'Stop

    End Sub

    '' TODO: to be removed...
    'Private Sub CalcCheckSum(ByVal filename As String, ByVal expected As String)

    '    Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), &H41)
    '    With File.OpenRead(filename)
    '        .Read(buffer, 0, buffer.Length)
    '    End With

    '    Debug.WriteLine(vbCrLf & "Test for file """ & filename & """")
    '    Debug.WriteLine(" From Stream = " & expected)

    '    'Debug.Write("C# Calculated = ")
    '    'For Each b As Byte In BitConverter.GetBytes(UnsignedOps.XorCheckSum.CheckWord(buffer, 0, buffer.Length - 2))
    '    '    Debug.Write(Hex(b).PadLeft(2, "0"c) & " ")
    '    'Next
    '    'Debug.WriteLine("")

    '    Debug.Write("VB Calculated = ")
    '    For Each b As Byte In BitConverter.GetBytes(XorCheckSum(buffer, 0, buffer.Length - 2))
    '        Debug.Write(Hex(b).PadLeft(2, "0"c) & " ")
    '    Next
    '    Debug.WriteLine("")

    'End Sub

    '' TODO: remove this later...
    'Public Shared Function XorCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As Int16

    '    Dim sum As Int16

    '    ' Word length XOR check-sum
    '    For x As Integer = 0 To length - 1 Step 2
    '        sum = sum Xor BitConverter.ToInt16(data, startIndex + x)
    '    Next

    '    Return sum

    'End Function

    'Private Class Word

    '    Public hi As Byte
    '    Public lo As Byte

    '    Public Shared Function [Xor](ByVal w1 As Word, ByVal w2 As Word)

    '        Dim w1bits As BitArray

    '    End Function

    'End Class

    Private Sub SampleRateCalcs_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing

        SaveWindowLocation(Me)
        Variables("Port") = Port.Value

        If Not listeningThread Is Nothing Then listeningThread.Abort()
        End

    End Sub

    Private Sub Listen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Listen.Click

        listeningThread = RunThread.ExecuteNonPublicMethod(Me, "UDPListen")
        Listen.Enabled = False

    End Sub

    Private Sub UDPListen()

        Dim udpSocket As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), 4096)
        Dim i, received As Integer
        Dim total As Long
        Dim remoteEP As System.Net.EndPoint = CType(New IPEndPoint(IPAddress.Any, Port.Value), System.Net.EndPoint)
        Dim startTime As Long = DateTime.Now.Ticks
        Dim logged As Boolean

        udpSocket.Bind(remoteEP)

        Do While True
            Try
                ' Blocks until a message returns on this socket from a remote host
                received = udpSocket.ReceiveFrom(buffer, remoteEP)
                total += 1

                '' TODO: Remove this debug code later...
                'If buffer(0) = &HAA And buffer(1) = &H0 Then
                '    Dim ds As FileStream = File.OpenWrite(Application.StartupPath & "\" & Guid.NewGuid.ToString & ".bin")
                '    ds.Write(buffer, 0, buffer.Length)
                '    ds.Close()
                'ElseIf Not logged Then
                '    logged = True
                '    Dim ds As FileStream = File.OpenWrite(Application.StartupPath & "\" & Guid.NewGuid.ToString & ".bin")
                '    ds.Write(buffer, 0, buffer.Length)
                '    ds.Close()
                'End If

                With New StringBuilder
                    For i = 0 To received - 1
                        .Append(Hex(buffer(i)).PadLeft(2, "0"c))
                        .Append(" "c)
                    Next
                    UDPFrame.Text = .ToString
                    SampleCount.Text = total.ToString
                    If total Mod 30 = 0 Then
                        SampleRate.Text = (total / ((DateTime.Now.Ticks - startTime) / 10000000L)).ToString("0.0000")
                    End If
                End With
            Catch ex As Threading.ThreadAbortException
                Exit Do
            Catch ex As SocketException
                Debug.WriteLine("Socket exception " & ex.ErrorCode & ": " & ex.Message)
            Catch ex As Exception
                Debug.WriteLine("Exception: " & ex.Message)
            End Try
        Loop

        udpSocket.Close()

    End Sub

    Private Sub Port_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles Port.GotFocus

        Port.SelectAll()

    End Sub

End Class
