'*******************************************************************************************************
'  AlternateCommandChannel.vb
'  Copyright © 2009 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: James R Carroll
'      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
'       Phone: 423/751-4165
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/15/2009 - James R Carroll
'       Generated original version of source code.
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.Windows.Forms.DialogResult
Imports TVA
Imports TVA.Communication
Imports TVA.Common

Public Class AlternateCommandChannel

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Initialize serial port selection lists
        For Each port As String In Ports.SerialPort.GetPortNames()
            ComboBoxSerialPorts.Items.Add(port)
        Next

        For Each parity As String In [Enum].GetNames(GetType(Ports.Parity))
            ComboBoxSerialParities.Items.Add(parity)
        Next

        For Each stopbit As String In [Enum].GetNames(GetType(Ports.StopBits))
            ComboBoxSerialStopBits.Items.Add(stopbit)
        Next

    End Sub

    Private Sub CheckBoxUndefined_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxUndefined.CheckedChanged

        ' Enable or disable command channel settings tab
        TabControlCommunications.Enabled = Not CheckBoxUndefined.Checked

    End Sub

    Private Sub ButtonBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonBrowse.Click

        With PMUConnectionTester.OpenFileDialog
            .Title = "Open Capture File"
            .Filter = "Captured Files (*.PmuCapture)|*.PmuCapture|All Files|*.*"
            .FileName = ""
            If .ShowDialog(Me) = OK Then
                TextBoxFileCaptureName.Text = .FileName()
            End If
        End With

    End Sub

    Public ReadOnly Property IsDefined() As Boolean
        Get
            Return Not CheckBoxUndefined.Checked
        End Get
    End Property

    Public Property ConnectionString() As String
        Get
            If CheckBoxUndefined.Checked Then
                Return ""
            Else
                Select Case TabControlCommunications.SelectedTab.Index
                    Case TransportProtocol.Tcp
                        Return _
                            "; commandchannel={protocol=Tcp" & _
                            "; server=" & TextBoxTcpHostIP.Text & _
                            "; port=" & TextBoxTcpPort.Text & "}"
                    Case TransportProtocol.Serial - 1 ' UDP removed from tab set...
                        Return _
                            "; commandchannel={protocol=Serial" & _
                            "; port=" & ComboBoxSerialPorts.Text & _
                            "; baudrate=" & ComboBoxSerialBaudRates.Text & _
                            "; parity=" & ComboBoxSerialParities.Text & _
                            "; stopbits=" & ComboBoxSerialStopBits.Text & _
                            "; databits=" & TextBoxSerialDataBits.Text & _
                            "; dtrenable=" & CheckBoxSerialDTR.Checked.ToString() & _
                            "; rtsenable=" & CheckBoxSerialRTS.Checked.ToString() & "}"
                    Case TransportProtocol.File - 1 ' UDP removed from tab set...
                        Return _
                            "; commandchannel={protocol=File" & _
                            "; file=" & TextBoxFileCaptureName.Text & "}"
                    Case Else
                        Return ""
                End Select
            End If
        End Get
        Set(ByVal value As String)
            Dim connectionData As Dictionary(Of String, String) = value.ParseKeyValuePairs()

            If connectionData.ContainsKey("commandchannel") Then
                Dim protocol As TransportProtocol

                CheckBoxUndefined.Checked = False
                connectionData = connectionData("commandchannel").ParseKeyValuePairs()
                protocol = DirectCast([Enum].Parse(GetType(TransportProtocol), connectionData("protocol")), TransportProtocol)

                ' Load remaining connection settings
                TabControlCommunications.Tabs(IIf(protocol > TransportProtocol.Tcp, protocol - 1, protocol)).Selected = True

                Select Case protocol
                    Case TransportProtocol.Tcp
                        TextBoxTcpPort.Text = connectionData("port")
                        TextBoxTcpHostIP.Text = connectionData("server")
                    Case TransportProtocol.Serial
                        ComboBoxSerialPorts.Text = connectionData("port")
                        ComboBoxSerialBaudRates.Text = connectionData("baudrate")
                        ComboBoxSerialParities.Text = connectionData("parity")
                        ComboBoxSerialStopBits.Text = connectionData("stopbits")
                        TextBoxSerialDataBits.Text = connectionData("databits")
                        CheckBoxSerialDTR.Checked = connectionData("dtrenable").ParseBoolean()
                        CheckBoxSerialRTS.Checked = connectionData("rtsenable").ParseBoolean()
                    Case TransportProtocol.File
                        TextBoxFileCaptureName.Text = connectionData("file")
                End Select
            Else
                CheckBoxUndefined.Checked = True
            End If
        End Set
    End Property

End Class