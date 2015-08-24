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
'  hence the need for this forms based wrapper class.
'
'  Ideally at some point we can just write a .NET UDP listener and parse
'  the data out ourselves...
' 
'  Code Modification History:
'  ---------------------------------------------------------------------
'  10/1/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.IO
Imports System.Runtime.InteropServices
Imports TVA.Config.Common
Imports TVA.Shared.FilePath

<ComVisible(False)> _
Public Class PDCDataReader

    Inherits System.Windows.Forms.Form

    Private daqInterface As [Interface]
    Private statusLog As String

    Public Sub New(ByVal daqInterface As [Interface])

        Me.New()
        Me.daqInterface = daqInterface
        statusLog = [Interface].ApplicationPath & "PDCDataReader.log"

    End Sub

    Public WriteOnly Property Instance() As Integer
        Set(ByVal Value As Integer)
            Try
                If AxPDCDataReader.Initialize(Variables("PDCDataReader.ConfigFile" & Value), Variables("PDCDataReader.ListenPort" & Value)) Then
                    LogMessage("PDC data reader initialized OK for instance " & Value & "!")
                Else
                    LogMessage("PDC data reader failed to initialize for instance " & Value & "!")
                End If
            Catch ex As Exception
                LogMessage("PDC data reader failed to initialize for instance " & Value & " due to exception: " & ex.Message)
            End Try
        End Set
    End Property

#Region " Windows Form Designer generated code "

    Private Sub New()
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
        Me.AxPDCDataReader.Size = New System.Drawing.Size(168, 72)
        Me.AxPDCDataReader.TabIndex = 0
        '
        'PDCDataReader
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(184, 85)
        Me.Controls.Add(Me.AxPDCDataReader)
        Me.Name = "PDCDataReader"
        Me.Text = "PDCDataReader"
        CType(Me.AxPDCDataReader, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub LogMessage(ByVal message As String)

        daqInterface.UpdateStatus(message)

        Try
            With File.AppendText(statusLog)
                .WriteLine(Now() & ": " & message & vbCrLf)
                .Close()
            End With
        Catch ex As Exception
            daqInterface.UpdateStatus("Error logging status message: " & ex.Message)
        End Try

    End Sub

    Private Sub AxPDCDataReader_NewDataEvent(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AxPDCDataReader.NewDataEvent

        Static lastUpdate As Long
        Static totalEvents As Long

        totalEvents += 1

        If daqInterface.pollingStarted Then
            With daqInterface.converter
                ' We push data into the conversion queue as fast as we get it...
                .QueueNewData(AxPDCDataReader.GetAnalogData(), AxPDCDataReader.GetDigitalData())

                ' Every ten seconds or so we update the user on the processing status...
                If (DateTime.Now.Ticks - lastUpdate) \ 10000000L > 10 Then
                    daqInterface.UpdateStatus("Processed DatAWare events = " & .ProcessedEvents & vbCrLf & _
                                              "Total incoming PDC frames = " & totalEvents & vbCrLf & _
                                              "        Queued PDC frames = " & .QueuedDataUnits & vbCrLf)
                    lastUpdate = DateTime.Now.Ticks
                End If

                ' At every 500th processed event, we'll give a detailed status
                If totalEvents Mod 500 = 0 Then daqInterface.UpdateStatus(.Status)
            End With
        End If

    End Sub

    Private Sub AxPDCDataReader_NewDescriptorEvent(ByVal sender As Object, ByVal e As System.EventArgs) Handles AxPDCDataReader.NewDescriptorEvent

        LogMessage("Received new PDC descriptor")
        daqInterface.converter.LoadDescriptorData(Variables("DatAWare.PointListFile"), AxPDCDataReader.GetAnalogNames(), AxPDCDataReader.GetDigitalNames())

    End Sub

    Private Sub AxPDCDataReader_Error(ByVal sender As Object, ByVal e As AxPDCDATAREADERLib._DPDCDataReaderEvents_ErrorEvent) Handles AxPDCDataReader.Error

        LogMessage("Encountered PDC error: " & e.number & " - " & e.description)

    End Sub

End Class
