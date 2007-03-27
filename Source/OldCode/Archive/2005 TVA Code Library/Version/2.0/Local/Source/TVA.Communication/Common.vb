'*******************************************************************************************************
'  TVA.Communication.Common.vb - Common global communications functions
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
'  08/04/2006 - Pinal C. Patel
'       Original version of source code generated
'  01/05/2007 - J. Ritchie Carroll
'       Moved raw data function signature into common (more logical location)
'
'*******************************************************************************************************

Imports TVA.Text.Common

Public Class Common

    ''' <summary>
    ''' This function signature gets implemented as needed to allow consumers to "intercept" data before it goes through normal processing
    ''' </summary>
    Public Delegate Sub ReceiveRawDataFunctionSignature(ByVal data As Byte(), ByVal offset As Integer, ByVal length As Integer)

    ''' <summary>
    ''' Create a communications server
    ''' </summary>
    ''' <remarks>
    ''' Note that typical configuration string should be prefixed with a "protocol=tcp" or a "protocol=udp"
    ''' </remarks>
    Public Shared Function CreateCommunicationServer(ByVal configurationString As String) As ICommunicationServer

        Dim server As ICommunicationServer = Nothing
        Dim protocol As String = ""
        Dim configurationData As Dictionary(Of String, String) = ParseKeyValuePairs(configurationString)

        If configurationData.TryGetValue("protocol", protocol) Then
            configurationData.Remove("protocol")
            With New System.Text.StringBuilder()
                For Each key As String In configurationData.Keys
                    .Append(key)
                    .Append("=")
                    .Append(configurationData(key))
                    .Append(";")
                Next
                Select Case protocol.ToLower()
                    Case "tcp"
                        server = New TcpServer(.ToString())
                    Case "udp"
                        server = New UdpServer(.ToString())
                    Case Else
                        Throw New ArgumentException("Transport protocol '" & protocol & "' is not valid.")
                End Select
            End With
        Else
            Throw New ArgumentException("Transport protocol must be specified.")
        End If

        Return server

    End Function

    ''' <summary>
    ''' Create a communications client
    ''' </summary>
    ''' <remarks>
    ''' Note that typical connection string should be prefixed with a "protocol=tcp", "protocol=udp", "protocol=serial" or "protocol=file"
    ''' </remarks>
    Public Shared Function CreateCommunicationClient(ByVal connectionString As String) As ICommunicationClient

        Dim client As ICommunicationClient = Nothing
        Dim protocol As String = ""
        Dim connectionData As Dictionary(Of String, String) = ParseKeyValuePairs(connectionString)

        If connectionData.TryGetValue("protocol", protocol) Then
            connectionData.Remove("protocol")
            With New System.Text.StringBuilder()
                For Each key As String In connectionData.Keys
                    .Append(key)
                    .Append("=")
                    .Append(connectionData(key))
                    .Append(";")
                Next
                Select Case protocol.ToLower()
                    Case "tcp"
                        client = New TcpClient(.ToString())
                    Case "udp"
                        client = New UdpClient(.ToString())
                    Case "serial"
                        client = New SerialClient(.ToString())
                    Case "file"
                        client = New FileClient(.ToString())
                    Case Else
                        Throw New ArgumentException("Transport protocol '" & protocol & "' is not valid.")
                End Select
            End With
        Else
            Throw New ArgumentException("Transport protocol must be specified.")
        End If

        Return client

    End Function

End Class
