'*******************************************************************************************************
'  MultipleDestinationExporter.vb - Multiple Destination File Exporter
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/13/2008 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.IO
Imports System.Threading
Imports TVA.Common
Imports TVA.Configuration.Common
Imports TVA.Text.Common
Imports TVA.IO.FilePath
Imports TVA.Collections
Imports TVA.Collections.Common
Imports TVA.Services

Namespace IO

    ''' <summary>
    ''' Handles exporting the same file to multiple destinations that are defined in the configuration file.  Includes feature for network share authentication.
    ''' </summary>
    ''' <remarks>
    ''' Currently only designed for text files. Useful for updating the same file on multiple servers (e.g., load balanced web server).
    ''' </remarks>
    Public Class MultipleDestinationExporter

        Implements IServiceComponent

        Private Const DefaultConfigSection As String = "ExportDestinations"
        Private Const DefaultExportTimeout As Integer = Timeout.Infinite

        Public Event StatusMessage(ByVal status As String)

        Private m_exportDestinations As ExportDestination()
        Private m_configSection As String
        Private m_exportTimeout As Integer
        Private m_totalExports As Long
        Private m_disposed As Boolean
        Private WithEvents m_exportQueue As ProcessQueue(Of String)

        Public Sub New()

            MyClass.New(New ExportDestination() {New ExportDestination("C:\filename.txt", False, "domain", "username", "password")})

        End Sub

        Public Sub New(ByVal defaultDestinations As ExportDestination())

            MyClass.New(DefaultConfigSection, DefaultExportTimeout, defaultDestinations)

        End Sub

        Public Sub New(ByVal configSection As String, ByVal exportTimeout As Integer, ByVal defaultDestinations As ExportDestination())

            m_configSection = configSection
            m_exportTimeout = exportTimeout

            ' So as to not delay class construction due to share authentication, we perform initialzation on another thread...
#If ThreadTracking Then
        With TVA.Threading.ManagedThreadPool.QueueUserWorkItem(AddressOf Initialize, defaultDestinations)
            .Name = "TVA.IO.MultipleDestinationExporter.Initialize()"
        End With
#Else
            ThreadPool.QueueUserWorkItem(AddressOf Initialize, defaultDestinations)
#End If

        End Sub

        Private Sub Initialize(ByVal state As Object)

            Dim defaultDestinations As ExportDestination() = DirectCast(state, ExportDestination())

            ' Set up a synchronous process queue to handle exports that will limit total export time to export interval
            m_exportQueue = ProcessQueue(Of String).CreateSynchronousQueue(AddressOf WriteExportFiles, 10, m_exportTimeout, False, False)

            With CategorizedSettings(m_configSection)
                ' Make sure the default configuration variables exist
                .Add("ExportCount", defaultDestinations.Length.ToString(), "Total number of export files to produce")

                For x As Integer = 0 To defaultDestinations.Length - 1
                    .Add(String.Format("ExportDestination{0}", x + 1), defaultDestinations(x).Share, "Root path for export destination. Use UNC path (\\server\share) with no trailing slash for network shares.")
                    .Add(String.Format("ExportDestination{0}.ConnectToShare", x + 1), defaultDestinations(x).ConnectToShare.ToString(), "Set to True to attempt authentication to network share.", False)
                    .Add(String.Format("ExportDestination{0}.Domain", x + 1), defaultDestinations(x).Domain, "Domain used for authentication to network share (computer name for local accounts).", False)
                    .Add(String.Format("ExportDestination{0}.UserName", x + 1), defaultDestinations(x).UserName, "User name used for authentication to network share.")
                    .Add(String.Format("ExportDestination{0}.Password", x + 1), defaultDestinations(x).Password, "Encrypted password used for authentication to network share.", True)
                    .Add(String.Format("ExportDestination{0}.FileName", x + 1), defaultDestinations(x).FileName, "Path and file name of data export. Prefix with directory separator when using UNC paths.")
                Next

                ' Save updates to config file, if any
                SaveSettings()

                ' Load needed settings
                Dim entryRoot As String
                Dim filename As String
                Dim destination As ExportDestination
                Dim exportCount As Integer = Integer.Parse(.Item("ExportCount").Value)

                m_exportDestinations = CreateArray(Of ExportDestination)(exportCount)

                For x As Integer = 0 To exportCount - 1
                    entryRoot = String.Format("ExportDestination{0}", x + 1)
                    filename = .Item(entryRoot).Value & .Item(String.Format("{0}.FileName", entryRoot)).Value

                    ' Load export destination from configuration entries
                    destination.DestinationFile = filename
                    destination.ConnectToShare = ParseBoolean(.Item(String.Format("{0}.ConnectToShare", entryRoot)).Value)
                    destination.Domain = .Item(String.Format("{0}.Domain", entryRoot)).Value
                    destination.UserName = .Item(String.Format("{0}.UserName", entryRoot)).Value
                    destination.Password = .Item(String.Format("{0}.Password", entryRoot)).Value

                    If destination.ConnectToShare Then
                        ' Attempt connection to external network share
                        Try
                            ConnectToNetworkShare( _
                                destination.Share, _
                                destination.UserName, _
                                destination.Password, _
                                destination.Domain)
                        Catch ex As Exception
                            ' Something unexpected happened during attempt to connect to network share - so we'll report it...
                            UpdateStatus("Network share authentication exception: " & ex.Message)
                        End Try
                    End If

                    m_exportDestinations(x) = destination
                Next
            End With

            m_exportQueue.Start()

        End Sub

        Public Sub Dispose() Implements System.IDisposable.Dispose

            If Not m_disposed Then
                m_disposed = True

                GC.SuppressFinalize(Me)

                If m_exportQueue IsNot Nothing Then m_exportQueue.Stop()

                ' We'll be nice and disconnect network shares when this class is disposed...
                For x As Integer = 0 To m_exportDestinations.Length - 1
                    If m_exportDestinations(x).ConnectToShare Then
                        DisconnectFromNetworkShare(m_exportDestinations(x).Share)
                    End If
                Next
            End If

        End Sub

        Protected Overrides Sub Finalize()

            Dispose()

        End Sub

        Public Property ConfigSection() As String
            Get
                Return m_configSection
            End Get
            Set(ByVal value As String)
                m_configSection = value
            End Set
        End Property

        ''' <summary>
        ''' Total allowed time for all exports to execute in milliseconds.
        ''' </summary>
        ''' <remarks>
        ''' Set to Timeout.Infinite (-1) for timeout.
        ''' </remarks>
        Public Property ExportTimeout() As Integer
            Get
                Return m_exportTimeout
            End Get
            Set(ByVal value As Integer)
                m_exportTimeout = value
                If m_exportQueue IsNot Nothing Then m_exportQueue.ProcessTimeout = value
            End Set
        End Property

        Public ReadOnly Property ExportDestinations() As ExportDestination()
            Get
                Return m_exportDestinations
            End Get
        End Property

        Public ReadOnly Property TotalExports() As Long
            Get
                Return m_totalExports
            End Get
        End Property

        Public ReadOnly Property Status() As String Implements Services.IServiceComponent.Status
            Get
                With New StringBuilder
                    .Append("     Configuration Section: ")
                    .Append(m_configSection)
                    .AppendLine()
                    .Append("       Export destinations: ")
                    .Append(ListToString(m_exportDestinations, ","c))
                    .AppendLine()
                    .Append(" Cumulative export timeout: ")
                    .Append(m_exportTimeout)
                    .Append(" milliseconds")
                    .AppendLine()
                    .Append("      Total exports so far: ")
                    .Append(m_totalExports)
                    .AppendLine()
                    If m_exportQueue IsNot Nothing Then .Append(m_exportQueue.Status)
                    Return .ToString()
                End With
            End Get
        End Property

        ''' <summary>
        ''' Start multiple file export.
        ''' </summary>
        ''' <param name="fileData">Data to export into each destination.</param>
        Public Sub ExportData(ByVal fileData As String)

            ' Queue data for export - multiple exports may take some time, so we do this on another thread...
            If m_exportQueue IsNot Nothing Then m_exportQueue.Add(fileData)

        End Sub

        Private Sub WriteExportFiles(ByVal fileData As String)

            ' Make sure there are measurements to export
            Dim fileName As String
            Dim fileStream As StreamWriter

            ' Loop through each defined export file
            For x As Integer = 0 To m_exportDestinations.Length - 1
                Try
                    '  Get next export file name
                    fileName = m_exportDestinations(x).DestinationFile

                    Try
                        ' We'll wait on file lock for up to one second - then give up with IO exception
                        WaitForWriteLock(fileName, 1)
                    Catch ex As ThreadAbortException
                        ' This exception is normal, we'll just rethrow this back up the try stack
                        Throw ex
                    Catch ex As FileNotFoundException
                        ' This would be an expected exception, nothing to do - even if we checked for
                        ' this before we called the wait function, another process could have deleted
                        ' the file before we attempt a file lock...
                    End Try

                    ' Create a new export file
                    fileStream = File.CreateText(fileName)

                    ' Export file data
                    fileStream.Write(fileData)

                    ' Close stream
                    fileStream.Close()

                    ' Track successful exports
                    m_totalExports += 1
                Catch ex As ThreadAbortException
                    ' This exception is normal, we'll just rethrow this back up the try stack
                    Throw ex
                Catch ex As Exception
                    ' Something unexpected happened during export - we'll report it but keep going, could be
                    ' that export destination was offline (not uncommon when system is being rebooted, etc.)
                    UpdateStatus("Export exception: " & ex.Message)
                End Try
            Next

        End Sub

        Private Sub UpdateStatus(ByVal status As String)

            RaiseEvent StatusMessage(String.Format("[{0}]: {1}", m_configSection, status))

        End Sub

        Private Sub m_exportQueue_ProcessException(ByVal ex As System.Exception) Handles m_exportQueue.ProcessException

            ' Something unexpected happened during export
            UpdateStatus("Export exception: " & ex.Message)

        End Sub

        Public ReadOnly Property Name() As String Implements Services.IServiceComponent.Name
            Get
                Return m_configSection
            End Get
        End Property

        Public Sub ProcessStateChanged(ByVal processName As String, ByVal newState As Services.ProcessState) Implements Services.IServiceComponent.ProcessStateChanged

            ' This component is not abstractly associated with any particular service process...

        End Sub

        Public Sub ServiceStateChanged(ByVal newState As Services.ServiceState) Implements Services.IServiceComponent.ServiceStateChanged

            Select Case newState
                Case ServiceState.Paused
                    If m_exportQueue IsNot Nothing Then m_exportQueue.Stop()
                Case ServiceState.Resumed
                    If m_exportQueue IsNot Nothing Then m_exportQueue.Start()
                Case ServiceState.Shutdown
                    If m_exportQueue IsNot Nothing Then m_exportQueue.Stop()
            End Select

        End Sub

    End Class

End Namespace
