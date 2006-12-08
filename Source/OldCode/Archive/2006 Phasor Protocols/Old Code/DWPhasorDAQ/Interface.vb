'***********************************************************************
'  Interface.vb - DatAWare DAQ Template
'  Copyright © 2004 - TVA, all rights reserved
'
'  COM Exposed DatAWare DAQ "Interface"
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  10/18/2004 - James R Carroll
'       Initial version of source created
'
'  IMPORTANT:
'
'   Rename this project to desired DAQ plug-in name - the project name
'   becomes the typelib name exposed through COM.  Note that DatAWare
'   expects the DLL name to be the same as the typelib name in order
'   for it to be able to create an instance of the plug-in.  You can
'   change the DLL name by changing the "Assembly name" under the
'   project properties.
'
'   Make sure you create all new GUID's when using this project
'   as a new .NET based DAQ template.  Interface class GUID's can
'   be found below in the "COM GUID's" region.  Primary typelib
'   GUID is defined in AssemblyInfo.vb.  You can run 'guidgen'
'   to create new GUID's.
'
'  TODO: Turn this code into a new .NET project template, this will
'        automatically create new GUID's and setup new project name
'
'***********************************************************************

Imports System.IO
Imports System.Text
Imports System.Data
Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Reflection.Assembly
Imports System.Windows.Forms
Imports Microsoft.Win32
Imports Tva.Common
Imports Tva.Collections.Common
Imports Tva.Configuration.Common
Imports Tva.Data.Common
Imports Tva.DatAWare
Imports Tva.Phasors
Imports Tva.Phasors.Common
Imports Tva.Measurements

<ComClass([Interface].ClassId, [Interface].InterfaceId, [Interface].EventsId)> _
Public Class [Interface]

#Region " COM GUID's "
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "A01CC472-3963-4469-B441-CE4E12A2AB4D"
    Public Const InterfaceId As String = "BD38E14A-A554-419d-ADA7-7E5E6BCECDFA"
    Public Const EventsId As String = "F746EA55-EB51-4510-95E5-68D502889335"
#End Region

    Public Const IPBufferLength As Integer = 100000
    Public Const MaximumEvents As Integer = IPBufferLength \ StandardEvent.BinaryLength

    Private m_instance As Integer
    Private m_busy As Boolean
    Private m_cfgChanged As Boolean
    Private m_statusWindow As StatusWindow
    Private m_pollEvents As Long
    Private m_measurementIDs As Dictionary(Of String, Integer)
    Private m_mappers As List(Of PhasorMeasurementMapper)
    Private m_measurementBuffer As List(Of IMeasurement)
    Private m_lastTotal As Integer
    Private m_assemblyCache As Dictionary(Of String, Assembly)
    Private m_rootNameSpace As String
    Private m_assemblyFolder As String

    Public Sub New()

        LoadAssembly("Tva.IO.Compression")
        LoadAssembly("Tva.Core")
        LoadAssembly("Tva.Phasors")
        LoadAssembly("Tva.DatAWare")

        m_measurementBuffer = New List(Of IMeasurement)
        m_statusWindow = New StatusWindow
        m_statusWindow.ParentInterface = Me

    End Sub

    Private Sub LoadAssembly(ByVal assemblyName As String)

        ' Hook into assembly resolve event for current domain so we can load assembly from an alternate path
        If m_assemblyCache Is Nothing Then
            ' Create a new assembly cache
            m_assemblyCache = New Dictionary(Of String, Assembly)

            ' DAQ Path: HKEY_LOCAL_MACHINE\SOFTWARE\DatAWare\DAQ Configuration\
            With Registry.LocalMachine.OpenSubKey("SOFTWARE\DatAWare\DAQ Configuration")
                ' We define alternate path as a subfolder of defined DAQ DLL path...
                m_assemblyFolder = .GetValue("DAQDLLPath") & "\bin\"
                .Close()
            End With

            ' Add hook into standard assembly resolution
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssemblyFromAlternatePath
        End If

        ' Load the assembly (this will invoke event that will resolve assembly from resource)
        AppDomain.CurrentDomain.Load(assemblyName)

    End Sub

    Private Function ResolveAssemblyFromAlternatePath(ByVal sender As Object, ByVal e As ResolveEventArgs) As Assembly

        Dim resourceAssembly As Assembly = Nothing
        Dim shortName As String = e.Name.Split(","c)(0)

        If Not m_assemblyCache.TryGetValue(shortName, resourceAssembly) Then
            ' Load assembly from "bin" sub-folder
            resourceAssembly = LoadFrom(m_assemblyFolder & shortName & ".dll")
            m_assemblyCache.Add(shortName, resourceAssembly)
        End If

        Return resourceAssembly

    End Function

    Protected Overrides Sub Finalize()

        If m_statusWindow IsNot Nothing Then
            m_statusWindow.Visible = False
            m_statusWindow.Dispose()
            m_statusWindow = Nothing
        End If

    End Sub

    ' Instance of DLL, set by caller
    Public Property Instance() As Integer
        Get
            Return m_instance
        End Get
        Set(ByVal value As Integer)
            m_instance = value
            If m_statusWindow IsNot Nothing Then m_statusWindow.SetInstance(value)
        End Set
    End Property

    ' Busy flag, set/cleared by .Poll method
    Public Property Busy() As Boolean
        Get
            Return m_busy
        End Get
        Set(ByVal value As Boolean)
            m_busy = value
        End Set
    End Property

    ' Flag indicating configuration changed
    Public Property cfgChanged() As Boolean
        Get
            Return m_cfgChanged
        End Get
        Set(ByVal value As Boolean)
            m_cfgChanged = value
        End Set
    End Property

    Public Sub Initialize(ByRef InfoStrings() As String)

        ' This subroutine is called immediately after the DLL is loaded by the calling program.
        ' FYI - this method does not seem to get fired - would avoid critical code here for now...

    End Sub

    Private Sub Initialize()

        UpdateStatus("[" & Now() & "] Starting DAQ interface initialization...")

        Try
            ' HACK: Had to hard code this for now :( - did not have rights to create or access config file - must fix later...
            Dim connection As New SqlConnection("Data Source=RGOCSQLD;Initial Catalog=PMU_SDS;Integrated Security=False;user ID=ESOPublic;pwd=4all2see")
            Dim row As DataRow
            Dim parser As FrameParser
            Dim source As String
            Dim pmuIDs As List(Of String)
            Dim x, y As Integer

            m_measurementIDs = New Dictionary(Of String, Integer)
            m_mappers = New List(Of PhasorMeasurementMapper)

            connection.Open()

            UpdateStatus("Database connection opened...")

            ' Initialize measurement ID list
            With RetrieveData("SELECT * FROM IEEEDataConnectionMeasurements", connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    With .Rows(x)
                        m_measurementIDs.Add(.Item("Synonym"), .Item("ID"))
                    End With
                Next
            End With

            UpdateStatus("Loaded " & m_measurementIDs.Count & " measurement ID's...")

            ' Initialize each data connection
            With RetrieveData("SELECT * FROM IEEEDataConnections", connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    row = .Rows(x)

                    parser = New FrameParser()
                    source = row("SourceID").ToString.Trim
                    pmuIDs = New List(Of String)

                    With parser
                        ' TODO: Change database entries to match Enumeration Names - then do lookup:
                        '.Protocol = [Enum].Parse(GetType(Protocol), row("DataID<name>"))
                        .Protocol = Math.Abs(5 - Convert.ToInt32(row("DataID")))
                        .TransportLayer = IIf(String.Compare(row("NTP"), "UDP", True) = 0, DataTransportLayer.Udp, DataTransportLayer.Tcp)
                        .HostIP = row("IPAddress")
                        .Port = row("IPPort")
                        .PmuID = row("AccessID")
                    End With

                    If row("IsConcentrator") = 1 Then
                        UpdateStatus("Loading excepted PMU list for """ & source & """:")

                        ' Making a connection to a concentrator - this may support multiple PMU's
                        With RetrieveData("SELECT PMUID FROM IEEEDataConnectionPMUs WHERE PDCID='" & source & "' ORDER BY PMUIndex", connection)
                            For y = 0 To .Rows.Count - 1
                                pmuIDs.Add(.Rows(y)("PMUID"))
                                UpdateStatus("   >> " & pmuIDs(y))
                            Next
                        End With
                    Else
                        ' Making a connection to a single device
                        pmuIDs.Add(source)
                    End If

                    ' UPDATE: Could compile these "special" case protocol "fixers" into indivdual DLL's
                    ' with an interface that could provide enough info to handle mapping
                    Select Case source.ToUpper
                        Case "CONED"
                            With New ConEdPhasorMeasurementMapper(parser, source, pmuIDs, m_measurementIDs)
                                AddHandler .ParsingStatus, AddressOf UpdateStatus
                                m_mappers.Add(.This)
                                .Connect()
                            End With
                        Case "AEP"
                            With New AEPPhasorMeasurementMapper(parser, source, pmuIDs, m_measurementIDs)
                                AddHandler .ParsingStatus, AddressOf UpdateStatus
                                m_mappers.Add(.This)
                                .Connect()
                            End With
                        Case Else
                            With New PhasorMeasurementMapper(parser, source, pmuIDs, m_measurementIDs)
                                AddHandler .ParsingStatus, AddressOf UpdateStatus
                                m_mappers.Add(.This)
                                .Connect()
                            End With
                    End Select
                Next
            End With

            connection.Close()

            UpdateStatus("[" & Now() & "] DAQ interface initialized successfully.")
        Catch ex As Exception
            UpdateStatus("[" & Now() & "] ERROR: DAQ interface failed to initialize: " & ex.Message)
        End Try

    End Sub

    Friend Function GetStatus() As String

        With New StringBuilder
            For Each parser As PhasorMeasurementMapper In m_mappers
                .Append(parser.Status())
                .Append(Environment.NewLine)
            Next

            Return .ToString()
        End With

    End Function

    Public Sub Terminate()

        ' This routine is called just before the DLL is unloaded.
        UpdateStatus("[" & Now() & "] DAQ interface terminated")

    End Sub

    Public Sub Poll(ByRef IntIPBuf() As Byte, ByRef nBytes As Integer, ByRef iReturn As Integer, ByRef Status As Integer)

        If m_mappers Is Nothing Then Initialize()

        Try

            If iReturn = -2 Then
                ' Intialization request poll event
                Busy = True
                m_pollEvents += 1

                nBytes = FillIPBuffer(IntIPBuf, LoadDatabase())
                iReturn = 1

                UpdateStatus("Local database intialized..." & vbCrLf)
            Else
                ' Standard poll event
                Busy = True
                m_pollEvents += 1

                If m_pollEvents Mod 500 = 0 Then UpdateStatus(m_pollEvents & " have been processed...")

                Dim events As StandardEvent() = LoadEvents()

                If events IsNot Nothing Then
                    nBytes = FillIPBuffer(IntIPBuf, events)
                End If

                If m_measurementBuffer.Count > 0 Or m_lastTotal > 0 Then UpdateStatus("    >> Uploading measurements, " & m_measurementBuffer.Count & " remaining...")
                If m_measurementBuffer.Count = 0 And m_lastTotal > 0 Then UpdateStatus(Environment.NewLine)
                m_lastTotal = m_measurementBuffer.Count

                ' We set iReturn to zero to have DatAWare call the poll event again immediately, else set to one
                ' (i.e., set to zero if you still have more items in the queue to be processed)
                iReturn = IIf(m_measurementBuffer.Count > 0, 0, 1)
            End If
        Catch ex As Exception
            UpdateStatus("Exception occured during poll event: " & ex.Message)
            nBytes = 0
            Status = 1
            iReturn = 1
        Finally
            Busy = False
        End Try

    End Sub

    Private Function LoadDatabase() As StandardEvent()

        Dim standardEvents As New List(Of StandardEvent)

        For Each measurementID As Integer In m_measurementIDs.Values
            standardEvents.Add(New StandardEvent(measurementID, New TimeTag(Date.UtcNow), 0.0!, Quality.Good))
        Next

        Return standardEvents.ToArray()

    End Function

    Private Function LoadEvents() As StandardEvent()

        Dim events As StandardEvent() = Nothing

        SyncLock m_measurementBuffer
            ' Extract all queued data frames from the data parsers
            For Each mapper As PhasorMeasurementMapper In m_mappers
                ' Get all queued frames in this parser
                For Each frame As IFrame In mapper.GetQueuedFrames()
                    ' Extract each measurement from the frame and add queue up for processing
                    For Each measurement As IMeasurement In frame.Measurements.Values
                        m_measurementBuffer.Add(measurement)
                    Next
                Next
            Next

            Dim totalEvents As Integer = Minimum(MaximumEvents, m_measurementBuffer.Count)

            If totalEvents > 0 Then
                ' Create standard DatAWare event array of all points to be processed
                events = CreateArray(Of StandardEvent)(totalEvents)

                For x As Integer = 0 To totalEvents - 1
                    events(x) = New StandardEvent(m_measurementBuffer(x))
                Next

                ' Remove measurements being processed
                m_measurementBuffer.RemoveRange(0, totalEvents)
            End If
        End SyncLock

        Return events

    End Function

    Private Function FillIPBuffer(ByVal buffer As Byte(), ByVal events As StandardEvent()) As Integer

        If events Is Nothing Then
            Return 0
        Else
            Dim eventCount As Integer = events.Length

            If events.Length > MaximumEvents Then
                eventCount = MaximumEvents
                UpdateStatus("WARNING! Total number of loaded standard events would exceed IP buffer - excess events were truncated!  Maximum number of events that can be processed per poll is " & MaximumEvents & ".")
            End If

            For x As Integer = 0 To eventCount - 1
                System.Buffer.BlockCopy(events(x).BinaryImage, 0, buffer, x * StandardEvent.BinaryLength, StandardEvent.BinaryLength)
            Next

            Return eventCount * StandardEvent.BinaryLength
        End If

    End Function

    Public Sub ShowInterface()

        ' This Public routine is used to display the device configuration dialog box

        ' Note that you should not show any window modally; this allows the
        ' calling program to continue operation while a dialog is active.

    End Sub

    Public Sub ShowStatus(ByRef ShowIt As Boolean, Optional ByRef X As Integer = 0, Optional ByRef Y As Integer = 0, Optional ByRef W As Integer = 5000)

        ' This public routine is used to show or hide a debug/status window
        With m_statusWindow
            .WindowState = FormWindowState.Normal
            If ShowIt Then
                .Left = X \ 15
                .Top = Y \ 15
                .Width = W \ 15
            End If
            .Visible = ShowIt
        End With

    End Sub

    Public Sub CloseArchive()

        UpdateStatus("[" & Now() & "] DAQ interface received notfication from DatAWare that a new archive is being created...")

    End Sub

    Private Sub UpdateStatus(ByVal status As String)

        UpdateStatus(status, True)

    End Sub

    Private Sub UpdateStatus(ByVal status As String, ByVal newLine As Boolean)

        m_statusWindow.UpdateStatus(status, newLine)

    End Sub

End Class
