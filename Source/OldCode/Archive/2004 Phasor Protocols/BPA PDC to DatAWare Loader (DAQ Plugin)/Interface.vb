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
' After installation, you can delete dependent DLL's from installation
' folder since they are embedded into primary assembly - this way they
' won't interfere with DatAWare DAQ loader...
'
'***********************************************************************

Imports System.IO
Imports System.Windows.Forms
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports TVA.Config.Common
Imports TVA.Shared.FilePath
Imports BpaPdcLoader.DatAWare

<ComClass([Interface].ClassId, [Interface].InterfaceId, [Interface].EventsId)> _
Public Class [Interface]

#Region " COM GUID's "
    ' These  GUIDs provide the COM identity for this class 
    ' and its COM interfaces. If you change them, existing 
    ' clients will no longer be able to access the class.
    Public Const ClassId As String = "36306D7A-8EA0-4ad5-A790-FB16B79EC32C"
    Public Const InterfaceId As String = "8BC176DC-F37F-4da2-BC77-17808FE4BE93"
    Public Const EventsId As String = "714E0615-8094-443a-B05E-5F4BBE641C2B"
#End Region

    Public Const IPBufferLength As Integer = 100000
    Public Const MaximumEvents As Integer = IPBufferLength \ StandardEvent.BinaryLength

    Private m_instance As Integer
    Private m_busy As Boolean
    Private m_cfgChanged As Boolean
    Private m_pollEvents As Long
    Private m_converter As PDCToDatAWare.Converter
    Private m_pdcDataReader As BpaPdcLoader.PDCDataReader
    Private m_configWindow As BpaPdcLoader.Configuration
    Private m_statusWindow As BpaPdcLoader.StatusWindow

    Shared Sub New()

        ' Load embedded assemblies...
        LoadAssembly("Interop.PDCDATAREADERLib")
        LoadAssembly("AxInterop.PDCDATAREADERLib")
        LoadAssembly("TVA.Shared")
        LoadAssembly("TVA.Threading")
        LoadAssembly("TVA.VarEval")
        LoadAssembly("TVA.Config")
        LoadAssembly("TVA.Remoting")
        LoadAssembly("TVA.Services")
        LoadAssembly("TVA.Database")

    End Sub

    Private Shared Sub LoadAssembly(ByVal assemblyName As String)

        Static addedResolver As Boolean

        ' Hook into assembly resolve event for current domain so we can load assembly from embedded resource
        If Not addedResolver Then
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssemblyFromResource
            addedResolver = True
        End If

        ' Load the assembly (this will invoke event that will resolve assembly from resource)
        AppDomain.CurrentDomain.Load(assemblyName)

    End Sub

    Private Shared Function ResolveAssemblyFromResource(ByVal sender As Object, ByVal args As ResolveEventArgs) As [Assembly]

        Static rootNameSpace As String
        Dim resourceAssembly As [Assembly]
        Dim shortName As String = args.Name.Split(","c)(0)
        Dim buffer As Byte()

        ' Get root namespace of executing assembly since all embedded resources will be prefixed with this
        If rootNameSpace Is Nothing Then
            rootNameSpace = GetType([Interface]).AssemblyQualifiedName
            rootNameSpace = rootNameSpace.Substring(0, rootNameSpace.IndexOf("."c))
        End If

        ' Loop through all of the resources in the executing assembly
        For Each name As String In [Assembly].GetExecutingAssembly.GetManifestResourceNames()
            ' See if the embedded resource name matches assembly we are trying to load
            If String.Compare(Path.GetFileNameWithoutExtension(name), rootNameSpace & "." & shortName, True) = 0 Then
                ' If so, load embedded resource assembly into a binary buffer
                With [Assembly].GetExecutingAssembly.GetManifestResourceStream(name)
                    buffer = Array.CreateInstance(GetType(Byte), .Length)
                    .Read(buffer, 0, .Length)
                    .Close()
                End With

                ' Load assembly from binary buffer
                resourceAssembly = [Assembly].Load(buffer)

                Exit For
            End If
        Next

        Return resourceAssembly

    End Function

    Public Sub New()

        MyBase.New()

        SharedConfigFileName = [Assembly].GetExecutingAssembly.Location & ".config"
        Variables.Create("DatAWare.TimeZone", "GMT Standard Time", VariableType.Text, "DatAWare Server TimeZone")
        Variables.Create("DatAWare.PointListFile", ApplicationPath & "PM_DBASE.csv", VariableType.Text, "DatAWare Point List File")
        Variables.Create("PDCDataReader.ConfigFile", ApplicationPath & "TVA_PDC.ini", VariableType.Text, "BPA PDC Configuration File")
        Variables.Create("PDCDataReader.ListenPort", 3050, VariableType.Int, "BPA PDC UDP Port to Listen On")
        Variables.Save()

        ' Create an instance of the PDC to DatAWare conversion class
        m_converter = New PDCToDatAWare.Converter(Variables("DatAWare.TimeZone"))

        ' Create a new instance of the PDC data reader
        m_pdcDataReader = New PDCDataReader(Me)

    End Sub

    <ComVisible(False)> _
    Public Shared ReadOnly Property ApplicationPath() As String
        Get
            Return JustPath([Assembly].GetExecutingAssembly.Location)
        End Get
    End Property

    ' Instance of DLL, set by caller
    Public Property Instance() As Integer
        Get
            Return m_instance
        End Get
        Set(ByVal Value As Integer)
            m_instance = Value
        End Set
    End Property

    ' Busy flag, set/cleared by .Poll method
    Public Property Busy() As Boolean
        Get
            Return m_busy
        End Get
        Set(ByVal Value As Boolean)
            m_busy = Value
        End Set
    End Property

    ' Flag indicating configuration changed
    Public Property cfgChanged() As Boolean
        Get
            Return m_cfgChanged
        End Get
        Set(ByVal Value As Boolean)
            m_cfgChanged = Value
        End Set
    End Property

    Public Sub Initialize(ByRef InfoStrings() As String)

        ' This subroutine is called immediately after the DLL is loaded by the calling program.
        ' NOTE: This never seems to be getting called from DatAWare, so I don't recommend using this function...
        UpdateStatus("[" & Now() & "] DAQ interface initialized")
        m_pollEvents = 0

    End Sub

    Public Sub Terminate()

        ' This routine is called just before the DLL is unloaded.
        UpdateStatus("[" & Now() & "] DAQ interface terminated")
        m_pollEvents = 0

    End Sub

    Public Sub Poll(ByRef IntIPBuf() As Byte, ByRef nBytes As Integer, ByRef iReturn As Integer, ByRef Status As Integer)

        Try
            Dim queueIsEmpty As Boolean

            Busy = True
            m_pollEvents += 1

            nBytes = FillIPBuffer(IntIPBuf, m_converter.GetEventData(queueIsEmpty))

            ' Set iReturn to zero to have DatAWare call the poll event again immediately, else set to one
            ' (i.e., set to zero if you still have more items in the queue to be processed)
            iReturn = IIf(queueIsEmpty, 1, 0)

            If m_pollEvents Mod 300 = 0 Then UpdateStatus("Poll events processed = " & m_pollEvents & vbCrLf)
        Catch ex As Exception
            UpdateStatus("Exception occured during poll event: " & ex.Message)
            nBytes = 0
            Status = 1
            iReturn = 1
        Finally
            Busy = False
        End Try

    End Sub

    Private Function FillIPBuffer(ByVal buffer As Byte(), ByVal events As StandardEvent()) As Integer

        If events Is Nothing Then
            Return 0
        Else
            Dim byteCount As Integer

            For x As Integer = 0 To events.Length - 1
                ' We only archive events that have a valid timestamp...
                If events(x).TTag.Value > 0 Then
                    Array.Copy(events(x).BinaryValue, 0, buffer, byteCount, StandardEvent.BinaryLength)
                    byteCount += StandardEvent.BinaryLength
                End If
            Next

            Return byteCount
        End If

    End Function

    Public Sub ShowInterface()

        ' This Public routine is used to display the device configuration dialog box
        ' Note that you should not show any window modally; this allows the
        ' calling program to continue operation while a dialog is active.
        configWindow.Show()

    End Sub

    Public Sub ShowStatus(ByRef ShowIt As Boolean, Optional ByRef X As Integer = 0, Optional ByRef Y As Integer = 0, Optional ByRef W As Integer = 5000)

        ' This public routine is used to show or hide a debug/status window
        With statusWindow
            .WindowState = FormWindowState.Normal
            'If ShowIt Then
            '    .Left = X \ 15
            '    .Top = Y \ 15
            '    .Width = W \ 15
            'End If
            .Visible = ShowIt
        End With

    End Sub

    Public Sub CloseArchive()

        UpdateStatus("[" & Now() & "] DAQ interface received notfication from DatAWare that a new archive is being created...")

    End Sub

    <ComVisible(False)> _
    Public Sub UpdateStatus(Optional ByVal Status As String = "", Optional ByVal NewLine As Boolean = True)

        statusWindow.UpdateStatus(Status, NewLine)

    End Sub

    Friend ReadOnly Property pollingStarted() As Boolean
        Get
            Return (m_pollEvents > 0)
        End Get
    End Property

    Friend ReadOnly Property converter() As PDCToDatAWare.Converter
        Get
            Return m_converter
        End Get
    End Property

    Friend ReadOnly Property statusWindow() As BpaPdcLoader.StatusWindow
        Get
            ' Create a new config window if needed
            If m_statusWindow Is Nothing Then
                m_statusWindow = New BpaPdcLoader.StatusWindow
                m_statusWindow.ParentInterface = Me
            ElseIf m_statusWindow.IsDisposed Then
                m_statusWindow = New BpaPdcLoader.StatusWindow
                m_statusWindow.ParentInterface = Me
            End If

            Return m_statusWindow
        End Get
    End Property

    Friend ReadOnly Property configWindow() As BpaPdcLoader.Configuration
        Get
            ' Create a new config window if needed
            If m_configWindow Is Nothing Then
                m_configWindow = New BpaPdcLoader.Configuration
            ElseIf m_configWindow.IsDisposed Then
                m_configWindow = New BpaPdcLoader.Configuration
            End If

            Return m_configWindow
        End Get
    End Property

End Class
