'*******************************************************************************************************
'  MainModule.vb - Main module for phasor measurement receiver service
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/19/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Security.Principal
Imports Tva.Assembly
Imports Tva.IO.FilePath
Imports Tva.Configuration.Common
Imports Tva.Text.Common

Module MainModule

    Private m_receiver As PhasorMeasurementReceiver

    Public Sub Main()

        Dim consoleLine As String
        Dim mapper As PhasorMeasurementMapper = Nothing

        Console.WriteLine(MonitorInformation)

        ' Make sure service settings exist
        Settings.Add("ArchiverIP", "127.0.0.1", "DatAWare Archiver IP")
        Settings.Add("InitializeOnStartup", "True", "Set to True to intialize phasor measurement mapper at startup")
        Settings.Add("PMUDatabase", "Data Source=RGOCSQLD;Initial Catalog=PMU_SDS;Integrated Security=False;user ID=ESOPublic;pwd=4all2see", "PMU metaData database connect string", True)
        SaveSettings()

        m_receiver = New PhasorMeasurementReceiver(StringSetting("ArchiverIP"), StringSetting("PMUDatabase"))
        If BooleanSetting("InitializeOnStartup") Then m_receiver.Initialize()

        Do While True
            ' This console window stays open by continually reading in console lines
            consoleLine = Console.ReadLine()

            ' While doing this, we monitor for special commands...
            If consoleLine.StartsWith("connect", True, Nothing) Then
                If TryGetMapper(consoleLine, mapper) Then
                    mapper.Connect()
                End If
            ElseIf consoleLine.StartsWith("disconnect", True, Nothing) Then
                If TryGetMapper(consoleLine, mapper) Then
                    mapper.Disconnect()
                End If
            ElseIf consoleLine.StartsWith("reload", True, Nothing) Then
                Console.WriteLine()
                m_receiver.Initialize()
            ElseIf consoleLine.StartsWith("status", True, Nothing) Then
                Console.WriteLine()
                Console.WriteLine(m_receiver.Status)
            ElseIf consoleLine.StartsWith("list", True, Nothing) Then
                Console.WriteLine()
                DisplayConnectionList()
            ElseIf consoleLine.StartsWith("version", True, Nothing) Then
                Console.WriteLine()
                Console.WriteLine(MonitorInformation)
            ElseIf consoleLine.StartsWith("help", True, Nothing) OrElse consoleLine = "?" Then
                Console.WriteLine()
                DisplayCommandList()
            ElseIf consoleLine.StartsWith("exit", True, Nothing) Then
                Exit Do
            Else
                Console.WriteLine()
                Console.Write("Command unrecognized.  ")
                DisplayCommandList()
            End If
        Loop

        ' Attempt an orderly shutdown...
        Try
            ' Disconnect from PMU devices...
            For Each mapper In m_receiver.Mappers.Values
                mapper.Disconnect()
            Next

            ' Disconnect from DatAWare archiver...
            m_receiver.Disconnect()
        Catch ex As Exception
            Console.WriteLine()
            Console.WriteLine("Exception during shutdown: " & ex.Message)
            Console.WriteLine()
            Console.WriteLine("Press any key to continue...")
            Console.ReadKey()
        End Try

        End

    End Sub

    Private Function TryGetMapper(ByVal consoleLine As String, ByRef mapper As PhasorMeasurementMapper) As Boolean

        Dim pmuID As String = RemoveDuplicateWhiteSpace(consoleLine).Split(" "c)(1)

        If m_receiver.Mappers.TryGetValue(pmuID, mapper) Then
            Return True
        Else
            Console.WriteLine("Failed to find a PMU or PDC in the connection list named """ & pmuID & """, type ""List"" to see avaialable list.")
        End If

    End Function

    Private ReadOnly Property MonitorInformation() As String
        Get
            With New StringBuilder
                .Append(ExecutingAssembly.Title)
                .Append(Environment.NewLine)
                .Append("   Assembly: ")
                .Append(ExecutingAssembly.Name)
                .Append(Environment.NewLine)
                .Append("    Version: ")
                .Append(ExecutingAssembly.Version)
                .Append(Environment.NewLine)
                .Append("   Location: ")
                .Append(ExecutingAssembly.Location)
                .Append(Environment.NewLine)
                .Append("    Created: ")
                .Append(ExecutingAssembly.BuildDate)
                .Append(Environment.NewLine)
                .Append("    NT User: ")
                .Append(WindowsIdentity.GetCurrent.Name)
                .Append(Environment.NewLine)
                .Append("    Runtime: ")
                .Append(ExecutingAssembly.ImageRuntimeVersion)
                .Append(Environment.NewLine)
                .Append("    Started: ")
                .Append(DateTime.Now.ToString())
                .Append(Environment.NewLine)

                Return .ToString()
            End With
        End Get
    End Property

    Private Sub DisplayConnectionList()

        Console.WriteLine("PMU/PDC Connection List (" & m_receiver.Mappers.Count & " Total)")
        Console.WriteLine()

        For Each pmuID As String In m_receiver.Mappers.Keys
            Console.WriteLine("    " & pmuID)
        Next

        Console.WriteLine()

    End Sub

    Private Sub DisplayCommandList()

        Console.WriteLine("Possible commands:")
        Console.WriteLine()
        Console.WriteLine("    ""Connect PmuID""        - Restarts PMU connection cycle")
        Console.WriteLine("    ""Disconnect PmuID""     - Terminates PMU connection")
        Console.WriteLine("    ""Reload""               - Reloads the entire service process")
        Console.WriteLine("    ""Status""               - Returns current service status")
        Console.WriteLine("    ""List""                 - Displays loaded PMU/PDC connections")
        Console.WriteLine("    ""Version""              - Displays service version information")
        Console.WriteLine("    ""Help""                 - Displays this help information")
        Console.WriteLine("    ""Exit""                 - Exits this console monitor")
        Console.WriteLine()

    End Sub

End Module
