'***********************************************************************
'  PDCDataReader.vb - TVA Service Template
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  The PDCDataReader ActiveX control only works when hosted on a form :p,
'  hence the need for this forms based wrapper class.  Additionally, since
'  this particular ActiveX control requires apartment threading I could
'  only successfully execute the code in an independent application, as a
'  result I had to use inter-process remoting to actually get the data to
'  the service.  I talked to Jeff Johnson (writer of the PDC Data Reader -
'  his number: (509) 375-4330) about these limitations and he said he may
'  be able to compile a different version of the control that supported
'  free threading and didn't require hosting on a form - but his time is
'  limited and he wasn't in a position where he could share the source
'  code.  So in the mean time, this is the best solution I came up with.
'
'  Ideally at some point we can just write a .NET UDP listener and parse
'  the data out ourselves, because these hoops seem a little much to me.
' 
'  Code Modification History:
'  ---------------------------------------------------------------------
'  10/1/2004 - James R Carroll
'       Initial version of source generated for new Windows service
'       project "BPA PDC To DatAWare Loader".
'
'***********************************************************************

Imports System.IO
Imports System.Text
Imports System.Reflection
Imports System.Reflection.Assembly
Imports System.Security.Principal
Imports System.Threading.Thread
Imports TVA.Services
Imports TVA.Remoting
Imports TVA.Shared.Common
Imports TVA.Shared.FilePath
Imports TVA.Config.Common
Imports TVA.Forms.Common
Imports PDCMessages
Imports VB = Microsoft.VisualBasic

' Class auto-generated using TVA service template at Fri Oct 1 14:26:17 EDT 2004
Public Class PDCDataReader

    Inherits System.Windows.Forms.Form

    Private Const SourceConfigFile As String = "BPA PDC To DatAWare Loader.exe.config"
    Private Const HostName As String = "BPA PDC To DatAWare Loader"
    Private Const HostURI As String = "tcp://localhost:6500/BPAPDCToDatAWareLoader"
    Private Const MaximumStatusLength As Integer = 65536
    Private Const ConnectionTimeout As Integer = 60
    Private statusLog As String

#Region " Remote Service Monitor Code "

    ' Create an instance of the remote service monitor class
    Private WithEvents RemoteMonitor As RemoteServiceMonitor

    Private Class RemoteServiceMonitor

        Inherits ServiceMonitor

        Public Sub New(ByVal RemotingClient As ClientBase)

            MyBase.New(RemotingClient)

        End Sub

        ' We create a derived service monitor to provide additional information about
        ' the "type" of client we are creating by overriding the description property
        Public Overrides ReadOnly Property Description() As String
            Get
                Return MonitorInformation & vbCrLf & MyBase.Description
            End Get
        End Property

        Public Shared ReadOnly Property MonitorInformation() As String
            Get
                With New StringBuilder
                    .Append(HostName)
                    .Append(" PDC Data Reader:")
                    .Append(vbCrLf)
                    .Append("   Assembly: ")
                    .Append(GetShortAssemblyName(GetExecutingAssembly))
                    .Append(vbCrLf)
                    .Append("   Location: ")
                    .Append(GetExecutingAssembly.Location)
                    .Append(vbCrLf)
                    .Append("    Created: ")
                    .Append(File.GetLastWriteTime(GetExecutingAssembly.Location))
                    .Append(vbCrLf)
                    .Append("    NT User: ")
                    .Append(WindowsIdentity.GetCurrent.Name)

                    Return .ToString()
                End With
            End Get
        End Property

    End Class

    Private Sub RemoteMonitor_StatusMessage(ByVal Message As String, ByVal NewLine As Boolean) Handles RemoteMonitor.StatusMessage

        LogMessage(Message)

    End Sub

    Private Sub RemoteMonitor_ConnectionEstablished() Handles RemoteMonitor.ConnectionEstablished

        LogMessage("Connection to host service """ & Variables("PDCDataReader.HostURI") & """ established.")

    End Sub

    Private Sub RemoteMonitor_ConnectionTerminated() Handles RemoteMonitor.ConnectionTerminated

        LogMessage("Connection to host service """ & Variables("PDCDataReader.HostURI") & """ terminated.")

    End Sub

#End Region

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
    Friend WithEvents AxPDCDataReader As AxPDCDATAREADERLib.AxPDCDataReader
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(PDCDataReader))
        Me.AxPDCDataReader = New AxPDCDATAREADERLib.AxPDCDataReader
        CType(Me.AxPDCDataReader, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'AxPDCDataReader
        '
        Me.AxPDCDataReader.Enabled = True
        Me.AxPDCDataReader.Location = New System.Drawing.Point(8, 8)
        Me.AxPDCDataReader.Name = "AxPDCDataReader"
        Me.AxPDCDataReader.OcxState = CType(resources.GetObject("AxPDCDataReader.OcxState"), System.Windows.Forms.AxHost.State)
        Me.AxPDCDataReader.Size = New System.Drawing.Size(168, 96)
        Me.AxPDCDataReader.TabIndex = 0
        '
        'PDCDataReader
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(182, 107)
        Me.Controls.Add(Me.AxPDCDataReader)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "PDCDataReader"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "PDC Data Reader"
        CType(Me.AxPDCDataReader, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub RemoteMonitor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Make sure PDC data reader variables exist (we share config file with parent service)
        SharedConfigFileName = JustPath(SharedConfigFileName) & SourceConfigFile
        Variables.Create("PDCDataReader.HostURI", HostURI, VariableType.Text)
        Variables.Create("PDCDataReader.ConfigFile", AddPathSuffix(Application.StartupPath) & "TVABPA_PDC.ini", VariableType.Text, "BPA PDC Configuration File")
        Variables.Create("PDCDataReader.ListenPort", 3050, VariableType.Int, "BPA PDC UDP Port to Listen On")
        Variables.Save()

        statusLog = AddPathSuffix(Application.StartupPath) & "PDCDataReader.log"

        If Len(Variables("PDCDataReader.HostURI").ToString()) > 0 Then
            RemoteMonitor = New RemoteServiceMonitor(New BinaryClient(Variables("PDCDataReader.HostURI")))
            RemoteMonitor.Connect()
        Else
            LogMessage("No host URI information was available - check PDCDataReader.HostURI variable in config file.  No service connection could be established.")
            End
        End If

        If AxPDCDataReader.Initialize(Variables("PDCDataReader.ConfigFile"), Variables("PDCDataReader.ListenPort")) Then
            LogMessage("PDC data reader initialized OK!")
        Else
            LogMessage("PDC data reader failed to initialize!")
        End If

    End Sub

    Private Sub LogMessage(ByVal message As String)

        With File.AppendText(statusLog)
            .WriteLine(Now() & ": " & message & vbCrLf)
            .Close()
        End With

    End Sub

    Private Sub SendNotification(ByVal e As EventArgs)

        Try
            RemoteMonitor.SendNotification(Nothing, e)
        Catch ex As Exception
            LogMessage("Failed to send service notification due to exception: " & ex.Message)
        End Try

    End Sub

    ' Any messages sent from here just get rebroadcasted back out to clients...
    Private Sub UpdateStatus(ByVal Status As String, Optional ByVal LogStatusToEventLog As Boolean = False)

        Dim trys As Integer

        Do Until RemoteMonitor.Connected
            CurrentThread.Sleep(100)
            trys += 1
            If trys \ 10 >= ConnectionTimeout Then
                LogMessage("Failed to send status message to " & Variables("RemoteMonitor.HostURI") & " service.  Attempted to connect to remoting server for " & ConnectionTimeout & " seconds.")
                End
            End If
        Loop

        ' Rebroadcast this message back to all clients
        SendNotification(New ServiceMessageEventArgs(Status, LogStatusToEventLog))

    End Sub

    Private Sub AxPDCDataReader_NewDataEvent(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AxPDCDataReader.NewDataEvent

        Dim dataUnit As New PDCDataUnit

        With dataUnit
            .AnalogData = AxPDCDataReader.GetAnalogData()
            .DigitalData = AxPDCDataReader.GetDigitalData()
        End With

        SendNotification(dataUnit)

    End Sub

    Private Sub AxPDCDataReader_NewDescriptorEvent(ByVal sender As Object, ByVal e As System.EventArgs) Handles AxPDCDataReader.NewDescriptorEvent

        Dim descriptorUnit As New PDCDescriptorUnit

        With descriptorUnit
            .AnalogNames = AxPDCDataReader.GetAnalogNames()
            .DigitalNames = AxPDCDataReader.GetDigitalNames()
        End With

        SendNotification(descriptorUnit)
        LogMessage("Received new PDC descriptor")

    End Sub

    Private Sub AxPDCDataReader_Error(ByVal sender As Object, ByVal e As AxPDCDATAREADERLib._DPDCDataReaderEvents_ErrorEvent) Handles AxPDCDataReader.Error

        Dim errorUnit As New PDCErrorUnit

        With errorUnit
            .Number = e.number
            .SCode = e.scode
            .Source = e.source
            .Description = e.description
        End With

        SendNotification(errorUnit)
        LogMessage("Encountered PDC error: " & e.number & " - " & e.description)

    End Sub

End Class
