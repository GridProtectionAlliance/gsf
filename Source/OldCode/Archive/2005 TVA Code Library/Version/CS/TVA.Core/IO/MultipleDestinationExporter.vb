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
'  07/29/2008 - J. Ritchie Carroll
'       Added "Reauthenticate" method to enable user to reconnect to network shares.
'       Added more descriptive status messages to provide more detailed user feedback.
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
    ''' This class is useful for updating the same file on multiple servers (e.g., load balanced web server).
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
        Private m_textEncoding As Encoding
        Private m_disposed As Boolean
        Private WithEvents m_exportQueue As ProcessQueue(Of Byte())

        Public Sub New()

            MyClass.New(New ExportDestination() {New ExportDestination("C:\filename.txt", False, "domain", "username", "password")})

        End Sub

        Public Sub New(ByVal defaultDestinations As ExportDestination())

            MyClass.New(DefaultConfigSection, DefaultExportTimeout, defaultDestinations)

        End Sub

        Public Sub New(ByVal configSection As String, ByVal exportTimeout As Integer, ByVal defaultDestinations As ExportDestination())

            m_configSection = configSection
            m_exportTimeout = exportTimeout
            m_textEncoding = Encoding.Default   ' We use default ANSI page encoding for text based exports...

            ' So as to not delay class construction due to share authentication, we perform initialization on another thread...
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

            ' In case we are reinitializing class (e.g., user requested to reauthenticate), we shutdown
            ' any prior queue operations and close any existing network connections...
            Shutdown()

            ' Set up a synchronous process queue to handle exports that will limit total export time to export interval
            m_exportQueue = ProcessQueue(Of Byte()).CreateSynchronousQueue(AddressOf WriteExportFiles, 10, m_exportTimeout, False, False)

            With CategorizedSettings(m_configSection)
                If defaultDestinations IsNot Nothing AndAlso defaultDestinations.Length > 0 Then                    
                    ' Make sure the default configuration variables exist
                    .Add("ExportCount", defaultDestinations.Length.ToString(), "Total number of export files to produce")

                    For x As Integer = 0 To defaultDestinations.Length - 1
                        .Add(String.Format("ExportDestination{0}", x + 1), defaultDestinations(x).Share, "Root path for export destination. Use UNC path (\\server\share) with no trailing slash for network shares.")
                        .Add(String.Format("ExportDestination{0}.ConnectToShare", x + 1), defaultDestinations(x).ConnectToShare.ToString(), "Set to True to attempt authentication to network share.", False)
                        .Add(String.Format("ExportDestination{0}.Domain", x + 1), defaultDestinations(x).Domain, "Domain used for authentication to network share (computer name for local accounts).", False)
                        .Add(String.Format("ExportDestination{0}.UserName", x + 1), defaultDestinations(x).UserName, "User name used for authentication to network share.")
                        .Add(String.Format("ExportDestination{0}.Password", x + 1), defaultDestinations(x).Password, "Encrypted password used for authentication to network share.", True)
                        .Add(String.Format("ExportDestination{0}.FileName", x + 1), defaultDestinations(x).FileName, "Path and file name of data export (do not include drive letter or UNC share). Prefix with slash when using UNC paths (\path\filename.txt).")
                    Next

                    ' Save updates to config file, if any
                    SaveSettings()
                End If

                ' Load needed settings
                Dim entryRoot As String
                Dim destination As ExportDestination
                Dim exportCount As Integer = Integer.Parse(.Item("ExportCount").Value)

                m_exportDestinations = CreateArray(Of ExportDestination)(exportCount)

                For x As Integer = 0 To exportCount - 1
                    entryRoot = String.Format("ExportDestination{0}", x + 1)

                    ' Load export destination from configuration entries
                    destination.DestinationFile = .Item(entryRoot).Value & .Item(String.Format("{0}.FileName", entryRoot)).Value
                    destination.ConnectToShare = ParseBoolean(.Item(String.Format("{0}.ConnectToShare", entryRoot)).Value)
                    destination.Domain = .Item(String.Format("{0}.Domain", entryRoot)).Value
                    destination.UserName = .Item(String.Format("{0}.UserName", entryRoot)).Value
                    destination.Password = .Item(String.Format("{0}.Password", entryRoot)).Value

                    If destination.ConnectToShare Then
                        ' Attempt connection to external network share
                        Try
                            UpdateStatus(String.Format("Attempting network share authentication for user {0}\{1} to {2}...", destination.Domain, destination.UserName, destination.Share))

                            ConnectToNetworkShare( _
                                destination.Share, _
                                destination.UserName, _
                                destination.Password, _
                                destination.Domain)

                            UpdateStatus(String.Format("Network share authentication to {0} succeeded.", destination.Share))
                        Catch ex As Exception
                            ' Something unexpected happened during attempt to connect to network share - so we'll report it...
                            UpdateStatus(String.Format("Network share authentication to {0} failed due to exception: {1}", destination.Share, ex.Message))
                        End Try
                    End If

                    m_exportDestinations(x) = destination
                Next
            End With

            m_exportQueue.Start()

        End Sub

        Public Sub Reauthenticate()

            Initialize(Nothing)

        End Sub

        Public Sub Reauthenticate(ByVal defaultDestinations As ExportDestination())

            Initialize(defaultDestinations)

        End Sub

        Public Sub Dispose() Implements System.IDisposable.Dispose

            Dispose(True)
            GC.SuppressFinalize(Me)

        End Sub

        Protected Sub Dispose(ByVal disposing As Boolean)

            If Not m_disposed Then
                If disposing Then
                    Shutdown()
                End If
            End If

            m_disposed = True

        End Sub

        Private Sub Shutdown()

            If m_exportQueue IsNot Nothing Then m_exportQueue.Dispose()
            m_exportQueue = Nothing

            ' We'll be nice and disconnect network shares when this class is disposed...
            If m_exportDestinations IsNot Nothing Then
                For x As Integer = 0 To m_exportDestinations.Length - 1
                    If m_exportDestinations(x).ConnectToShare Then
                        Try
                            DisconnectFromNetworkShare(m_exportDestinations(x).Share)
                        Catch ex As Exception
                            ' Something unexpected happened during attempt to disconnect from network share - so we'll report it...
                            UpdateStatus(String.Format("Network share disconnect from {0} failed due to exception: {1}", m_exportDestinations(x).Share, ex.Message))
                        End Try
                    End If
                Next
            End If

            m_exportDestinations = Nothing

        End Sub

        Protected Overrides Sub Finalize()

            Dispose(True)

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
        ''' Set to Timeout.Infinite (-1) for no timeout.
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
                    .Append(IIf(m_exportTimeout = Timeout.Infinite, "Infinite", m_exportTimeout & " milliseconds"))
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
        ''' Gets or sets the encoding to be used to encode text data being exported.
        ''' </summary>
        ''' <value>The encoding to be used to encode text data being exported.</value>
        ''' <returns>The encoding to be used to encode text data being exported.</returns>
        Public Overridable Property TextEncoding() As Encoding
            Get
                Return m_textEncoding
            End Get
            Set(ByVal value As Encoding)
                If value Is Nothing Then
                    m_textEncoding = Encoding.Default
                Else
                    m_textEncoding = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Start multiple file export.
        ''' </summary>
        ''' <param name="fileData">Text based data to export to each destination.</param>
        Public Sub ExportData(ByVal fileData As String)

            ' Queue data for export - multiple exports may take some time, so we do this on another thread...
            If m_exportQueue IsNot Nothing Then m_exportQueue.Add(m_textEncoding.GetBytes(fileData))

        End Sub

        ''' <summary>
        ''' Start multiple file export.
        ''' </summary>
        ''' <param name="fileData">Binary data to export to each destination.</param>
        Public Sub ExportData(ByVal fileData As Byte())

            ' Queue data for export - multiple exports may take some time, so we do this on another thread...
            If m_exportQueue IsNot Nothing Then m_exportQueue.Add(fileData)

        End Sub

        Private Sub WriteExportFiles(ByVal fileData As Byte())

            Dim filename As String
            Dim exportFile As FileStream

            ' Loop through each defined export file
            For x As Integer = 0 To m_exportDestinations.Length - 1
                Try
                    '  Get next export file name
                    filename = m_exportDestinations(x).DestinationFile

                    Try
                        ' We'll wait on file lock for up to one second - then give up with IO exception
                        WaitForWriteLock(filename, 1)
                    Catch ex As ThreadAbortException
                        ' This exception is normal, we'll just rethrow this back up the try stack
                        Throw ex
                    Catch ex As FileNotFoundException
                        ' This would be an expected exception, nothing to do - even if we checked for
                        ' this before we called the wait function, another process could have deleted
                        ' the file before we attempt a file lock...
                    End Try

                    ' Create a new export file
                    exportFile = File.Create(filename)

                    ' Export file data
                    exportFile.Write(fileData, 0, fileData.Length)

                    ' Close stream
                    exportFile.Close()

                    ' Track successful exports
                    m_totalExports += 1
                Catch ex As ThreadAbortException
                    ' This exception is normal, we'll just rethrow this back up the try stack
                    Throw ex
                Catch ex As Exception
                    ' Something unexpected happened during export - we'll report it but keep going, could be
                    ' that export destination was offline (not uncommon when system is being rebooted, etc.)
                    UpdateStatus(String.Format("Exception encountered during export for {0}: {1}", m_exportDestinations(x).DestinationFile, ex.Message))
                End Try
            Next

        End Sub

        Private Sub UpdateStatus(ByVal status As String)

            RaiseEvent StatusMessage(status)

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
                    Dispose()
            End Select

        End Sub

    End Class

End Namespace
