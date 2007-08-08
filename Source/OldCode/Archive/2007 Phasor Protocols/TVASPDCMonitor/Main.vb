'*******************************************************************************************************
'  Main.vb - Console monitor for windows service "SPDC"
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
'  05/08/2007 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Text
Imports TVA.Console
Imports TVA.Assembly

Public Class Main

#Region " Member Declaration "

    Private m_args As Arguments
    Private m_originalBgColor As ConsoleColor
    Private m_originalFgColor As ConsoleColor
    Private m_userInitiatedDisconnect As Boolean

#End Region

#Region " Code Scope: Public "

    Public Sub New(ByVal args() As String)

        MyClass.New()
        m_args = New Arguments(String.Join(" ", args))

    End Sub

    Public Sub Main_Load()

        Dim userInput As String = Nothing

        If m_args.Exists("server") AndAlso m_args.Exists("service") Then
            ' Override the default connection information with user provided input.
            Dim serverArgs As String() = m_args("server").Split(":"c)
            If serverArgs.Length = 2 Then
                ' User provided the necessary connection string data for TCP client.
                TcpClient.PersistSettings = False
                TcpClient.ConnectionString = String.Format("Server={0}; Port={1}", serverArgs(0), serverArgs(1))

                ClientHelper.PersistSettings = False
                ClientHelper.ServiceName = m_args("service")
            End If
        End If

        ' Initiate connection with the service and wait util the connection is made.
        ClientHelper.Connect()
        ClientHelper.CommunicationClient.WaitForConnection(-1)

        Do While True
            userInput = Console.ReadLine()  ' Wait for a command from the user.
            Console.WriteLine()             ' Write a blank line to the console.

            If Not String.IsNullOrEmpty(userInput) Then
                ' The user typed in a command and didn't just hit <ENTER>.
                Select Case userInput.ToUpper()
                    Case "SERVICECONNECT"
                        ' User wants to connect to the service.
                        m_userInitiatedDisconnect = False
                        ClientHelper.Connect()
                    Case "SERVICEDISCONNECT"
                        ' User wants to disconnect from the service.
                        m_userInitiatedDisconnect = True
                        ClientHelper.Disconnect()
                    Case "CLS"
                        ' User wants to clear the console window.
                        Console.Clear()
                    Case "EXIT"
                        ' User wants to exit the client session with the service.
                        m_userInitiatedDisconnect = True
                        Exit Do
                    Case Else
                        ' User wants to send a request to the service.
                        ClientHelper.SendRequest(userInput)

                        If String.Compare(userInput, "Help", True) = 0 Then
                            DisplayHelp()
                        End If
                End Select
            End If
        Loop

    End Sub

#End Region

#Region " Code Scope: Private "

    Private Sub DisplayHelp()

        ' Display a list of command that are supported by this console application.
        With New StringBuilder()
            .AppendFormat("Commands supported by {0}:", EntryAssembly.Name)
            .AppendLine()
            .AppendLine()
            .Append("Command".PadRight(20))
            .Append(" ")
            .Append("Description".PadRight(55))
            .AppendLine()
            .Append(New String("-"c, 20))
            .Append(" ")
            .Append(New String("-"c, 55))
            .AppendLine()
            .Append("ServiceConnect".PadRight(20))
            .Append(" ")
            .Append("Connects to the service".PadRight(55))
            .AppendLine()
            .Append("ServiceDisconnect".PadRight(20))
            .Append(" ")
            .Append("Disconnects from the service".PadRight(55))
            .AppendLine()
            .Append("Cls".PadRight(20))
            .Append(" ")
            .Append("Clears this console screen".PadRight(55))
            .AppendLine()
            .Append("Exit".PadRight(20))
            .Append(" ")
            .Append("Exits this console screen".PadRight(55))

            ClientHelper.UpdateStatus(.ToString(), 3)
        End With

    End Sub

#Region " Event Handlers "

#Region " ClientHelper "

    Private Sub ClientHelper_CommandSessionEstablished(ByVal sender As Object, ByVal e As System.EventArgs) Handles ClientHelper.CommandSessionEstablished

        ' Save the original color scheme of the console window.
        m_originalBgColor = Console.BackgroundColor
        m_originalFgColor = Console.ForegroundColor

        ' Change the console color scheme to indicate a remote command session.
        Console.BackgroundColor = ConsoleColor.Blue
        Console.ForegroundColor = ConsoleColor.Gray

        Console.Clear()

    End Sub

    Private Sub ClientHelper_CommandSessionTerminated(ByVal sender As Object, ByVal e As System.EventArgs) Handles ClientHelper.CommandSessionTerminated

        ' Revert to the original color scheme of the console window.
        Console.BackgroundColor = m_originalBgColor
        Console.ForegroundColor = m_originalFgColor

        Console.Clear()

    End Sub

    Private Sub ClientHelper_UpdateClientStatus(ByVal sender As Object, ByVal e As TVA.GenericEventArgs(Of String)) Handles ClientHelper.UpdateClientStatus

        ' Output status updates from the service to the console window.
        Console.Write(e.Argument)

    End Sub

#End Region

#Region " TcpClient "

    Private Sub TcpClient_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles TcpClient.Disconnected

        ' We'll retry connecting to the service if user did not initiate the disconnection.
        If Not m_userInitiatedDisconnect Then ClientHelper.Connect()

    End Sub

#End Region

#End Region

#End Region

End Class