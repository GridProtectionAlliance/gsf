'*******************************************************************************************************
'  MeasurementExporter.vb - ICCP data export module
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
'  01/10/2007 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.IO
Imports System.Data.OleDb
Imports System.Threading
Imports TVA.Common
Imports TVA.Measurements
Imports TVA.Configuration.Common
Imports TVA.IO.FilePath
Imports TVA.Text.Common
Imports TVA.Collections
Imports TVA.Collections.Common
Imports TVA.Data.Common
Imports TVA.DateTime.Common
Imports InterfaceAdapters

Public Class MeasurementExporter

    Inherits CalculatedMeasurementAdapterBase

    Private Const DefaultConfigSection As String = "ICCPDataExportModule"

    ' Initialization might take some time due to network share logon - so we postpone any frame publication until we're finished
    Private m_initialized As Boolean
    Private m_measurementTags As Dictionary(Of MeasurementKey, String)
    Private m_signalTypes As Dictionary(Of MeasurementKey, String)
    Private m_useReferenceAngle As Boolean
    Private m_referenceAngleKey As MeasurementKey
    Private m_exportInterval As Integer
    Private m_exportCount As Integer
    Private m_exportFileName As String()
    Private m_totalExports As Long
    Private m_statusDisplayed As Boolean
    Private m_sqrtOf3 As Double
    Private m_disposed As Boolean
    Private WithEvents m_exportQueue As ProcessQueue(Of IFrame)

    Private Enum ICCPDataQuality
        Good = 0
        Suspect = 20
        Bad = 32
    End Enum

    Public Overrides Sub Initialize(ByVal calculationName As String, ByVal configurationSection As String, ByVal outputMeasurements As IMeasurement(), ByVal inputMeasurementKeys As MeasurementKey(), ByVal minimumMeasurementsToUse As Integer, ByVal expectedMeasurementsPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        MyBase.Initialize(calculationName, configurationSection, outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime)
        If String.IsNullOrEmpty(configurationSection) Then MyBase.ConfigurationSection = DefaultConfigSection

        With CategorizedSettings(MyBase.ConfigurationSection)
            ' Make sure needed configuration variables exist - since configuration variables will
            ' be added to config file of parent process we add them to a new configuration category
            .Add("ExportInterval", "5", "Data export interval, in seconds")
            .Add("UseReferenceAngle", "True", "Require existance of reference angle to export relative angles")
            .Add("ExportCount", "1", "Total number of export files to produce")
            .Add("ExportShare1", "\\152.85.98.6\pmu", "UNC path (\\server\share) name for export file")
            .Add("ExportShare1.Domain", "SOCOPPMU", "Domain used for authentication to UNC path (computer name for local accounts)", False)
            .Add("ExportShare1.UserName", "shukri", "User name used for authentication to UNC path")
            .Add("ExportShare1.Password", "shukri", "Encrypted password used for authentication to UNC path", True)
            .Add("ExportShare1.FileName", "\PMU.txt", "Path and file name of ICCP data export - must be prefixed with directory separator")

            ' Save updates to config file, if any
            SaveSettings()

            ' Load needed settings
            m_exportInterval = Convert.ToInt32(.Item("ExportInterval").Value)
            m_useReferenceAngle = ParseBoolean(.Item("UseReferenceAngle").Value)
            m_exportCount = Convert.ToInt32(.Item("ExportCount").Value)
            m_exportFileName = CreateArray(Of String)(m_exportCount)

            Dim exportShare As String

            For x As Integer = 0 To m_exportCount - 1
                exportShare = String.Format("ExportShare{0}", x + 1)
                m_exportFileName(x) = .Item(exportShare).Value & .Item(String.Format("{0}.FileName", exportShare)).Value

                ' Attempt connection to external network share
                Try
                    ConnectToNetworkShare( _
                        .Item(exportShare).Value, _
                        .Item(String.Format("{0}.UserName", exportShare)).Value, _
                        .Item(String.Format("{0}.Password", exportShare)).Value, _
                        .Item(String.Format("{0}.Domain", exportShare)).Value)
                Catch ex As Exception
                    ' Something unexpected happened during attempt to connect to network share - so we'll report it...
                    RaiseCalculationException(ex)
                End Try
            Next
        End With

        ' Create new measurement dictionaries
        m_measurementTags = New Dictionary(Of MeasurementKey, String)
        m_signalTypes = New Dictionary(Of MeasurementKey, String)

        ' Calculate the square root of 3
        m_sqrtOf3 = Math.Sqrt(3.0R)

        ' Need to open database connection to load measurement tags and signal types.
        ' Note that data connection string defined in config file of parent process.
        Dim connection As New OleDbConnection(StringSetting("PMUDatabase"))
        Dim inputMeasurementKey As MeasurementKey

        connection.Open()

        ' Populate measurement tag and signal type dictionaries
        For x As Integer = 0 To inputMeasurementKeys.Length - 1
            ' Get input measurement identification key
            inputMeasurementKey = inputMeasurementKeys(x)

            ' Get point tag and signal type information for each input measurement
            With RetrieveRow(String.Format( _
                    "SELECT PointTag, SignalAcronym FROM MeasurementDetail WHERE HistorianAcronym='{0}' AND PointID={1}", _
                        inputMeasurementKey.Source, _
                        inputMeasurementKey.ID), _
                    connection)

                ' Load measurement tag name (remove any dashes or colons)
                m_measurementTags.Add(inputMeasurementKey, .Item("PointTag").ToString().Replace("-"c, "_"c).Replace(":"c, "_"c))

                ' Load measurement signal type
                m_signalTypes.Add(inputMeasurementKey, .Item("SignalAcronym").ToString())
            End With
        Next

        If m_useReferenceAngle Then
            ' Lastly, we also need to determine which angle is the reference angle
            With RetrieveRow("SELECT * FROM CalcOutputReferenceAngleMeasurement", connection)
                m_referenceAngleKey = New MeasurementKey( _
                    Convert.ToInt32(.Item("MeasurementID")), _
                    .Item("ArchiveSource").ToString())
            End With
        End If

        connection.Close()

        ' We track latest measurements so we can use these values when points are missing
        TrackLatestMeasurements = True

        ' Set up a synchronous process queue to handle exports that will limit total export time to export interval - queue should
        ' never have more than one item queued at once...
        m_exportQueue = ProcessQueue(Of IFrame).CreateSynchronousQueue(AddressOf ExportMeasurements, 10, m_exportInterval * 1000, False, False)

    End Sub

    Public Overrides Sub Start()

        MyBase.Start()
        If m_exportQueue IsNot Nothing Then m_exportQueue.Start()

        ' Mark initialization as complete
        m_initialized = True

    End Sub

    Public Overrides Sub [Stop]()

        MyBase.[Stop]()
        If m_exportQueue IsNot Nothing Then m_exportQueue.Stop()

    End Sub

    Public Overrides Sub Dispose()

        If Not m_disposed Then
            m_disposed = True

            GC.SuppressFinalize(Me)

            MyBase.Dispose()

            ' We'll be nice and disconnect network shares when this class is disposed...
            For x As Integer = 1 To m_exportCount
                DisconnectFromNetworkShare(CategorizedStringSetting(ConfigurationSection, String.Format("ExportShare{0}", x)))
            Next
        End If

    End Sub

    Protected Overrides Sub Finalize()

        Dispose()

    End Sub

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("   Total ICCP measurements: ")
                .Append(m_measurementTags.Count)
                .AppendLine()
                .Append("     Using reference angle: ")
                .Append(m_useReferenceAngle)
                .AppendLine()
                .Append("     Reference angle point: ")
                .Append(m_referenceAngleKey.ToString())
                .AppendLine()
                .Append("   Defined export interval: ")
                .Append(m_exportInterval)
                .Append(" seconds")
                .AppendLine()
                .Append("    ICCP export file names: ")
                .Append(ListToString(m_exportFileName, ","c))
                .AppendLine()
                .Append("      Total exports so far: ")
                .Append(m_totalExports)
                .AppendLine()
                If m_exportQueue IsNot Nothing Then .Append(m_exportQueue.Status)
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    ''' <summary>
    ''' Export PMU data to ICCP using an intermediate file
    ''' </summary>
    ''' <param name="frame">Single frame of measurement data within a one second sample</param>
    ''' <param name="index">Index of frame within the one second sample</param>
    ''' <remarks>
    ''' The frame.Measurements property references a dictionary, keyed on each measurement's MeasurementKey, containing
    ''' all available measurements as defined by the InputMeasurementKeys property that arrived within the specified
    ''' LagTime.  Note that this function will be called with a frequency specified by the ExpectedMeasurementsPerSecond
    ''' property, so make sure all work to be done is executed as efficiently as possible.
    ''' </remarks>
    Protected Overrides Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        Dim ticks As Long = frame.Ticks

        ' We only export data at the specified interval
        If m_initialized AndAlso (New Date(ticks)).Second Mod m_exportInterval = 0 AndAlso TicksBeyondSecond(ticks) = 0 Then
            ' Measurement export to a file may take more than 1/30 of a second - so we do this work asyncrhonously
            m_exportQueue.Add(frame)
        End If

    End Sub

    Private Sub ExportMeasurements(ByVal frame As IFrame)

        Dim measurements As IDictionary(Of MeasurementKey, IMeasurement) = frame.Measurements

        ' Make sure there are measurements to export
        If measurements.Count > 0 Then
            Dim exportData As New StringBuilder
            Dim measurement, referenceAngle As IMeasurement
            Dim measurementTag, signalType As String
            Dim measurementValue As Double
            Dim measurementQuality As ICCPDataQuality
            Dim fileStream As StreamWriter

            ' We need to get calculated reference angle value in order to export relative phase angles
            ' If the value is not here, we don't export
            If m_useReferenceAngle AndAlso Not measurements.TryGetValue(m_referenceAngleKey, referenceAngle) Then
                ' We were unable to find reference angle in this data concentration pass, lag time too small?
                RaiseCalculationException(New InvalidOperationException("Calculated reference angle was not found in this frame, possible reasons: system is initializing, receiving no data or lag time is too small.  File creation was skipped."))
                Exit Sub
            End If

            ' Export all defined ICCP measurements
            For Each inputMeasurementKey As MeasurementKey In InputMeasurementKeys
                ' Look up measurement's tag name
                If m_measurementTags.TryGetValue(inputMeasurementKey, measurementTag) Then
                    ' See if measurement exists in this frame
                    If measurements.TryGetValue(inputMeasurementKey, measurement) Then
                        ' Get measurement's adjusted value
                        measurementValue = measurement.AdjustedValue

                        ' Interpret data quality flags
                        measurementQuality = IIf(measurement.ValueQualityIsGood, IIf(measurement.TimestampQualityIsGood, ICCPDataQuality.Good, ICCPDataQuality.Suspect), ICCPDataQuality.Bad)
                    Else
                        ' Didn't find measurement in this frame, try using most recent adjusted value
                        measurementValue = LatestMeasurements(inputMeasurementKey)

                        ' Interpret data quality flags
                        measurementQuality = IIf(Double.IsNaN(measurementValue), ICCPDataQuality.Bad, ICCPDataQuality.Suspect)

                        ' We'll export zero instead of NaN for bad data
                        If measurementQuality = ICCPDataQuality.Bad Then measurementValue = 0.0R
                    End If

                    ' Export tag name field
                    exportData.Append(measurementTag)
                    exportData.Append(","c)

                    ' Export measurement value, making any needed adjustments
                    If m_signalTypes.TryGetValue(inputMeasurementKey, signalType) Then
                        If String.Compare(signalType, "VPHA") = 0 OrElse String.Compare(signalType, "IPHA") = 0 Then
                            ' This is a phase angle measurement, export the value relative to the reference angle (if available)
                            If referenceAngle Is Nothing Then
                                exportData.Append(measurementValue)
                            Else
                                exportData.Append(referenceAngle.AdjustedValue - measurementValue)
                            End If
                        ElseIf String.Compare(signalType, "VPHM") = 0 Then
                            ' Voltage from PMU's is line-to-neutral volts, we convert this to line-to-line kilovolts
                            exportData.Append(measurementValue * m_sqrtOf3 / 1000.0R)
                        Else
                            ' All other measurements are exported using their raw value
                            exportData.Append(measurementValue)
                        End If
                    Else
                        ' We were unable to find signal type for this key - this is unexpected
                        RaiseCalculationException(New InvalidOperationException(String.Format("Failed to find signal type for measurement {0}", inputMeasurementKey)))
                    End If

                    ' Export measurement quality
                    exportData.Append(","c)
                    exportData.Append(measurementQuality)

                    ' Terminate line
                    exportData.AppendLine(",,")
                Else
                    ' We were unable to find measurement tag for this key - this is unexpected
                    RaiseCalculationException(New InvalidOperationException(String.Format("Failed to find measurement tag for measurement {0}", inputMeasurementKey)))
                End If
            Next

            ' Loop through each defined export file
            For x As Integer = 0 To m_exportCount - 1
                Try
                    Try
                        ' We'll wait on file lock for up to one second - then give up with IO exception
                        WaitForWriteLock(m_exportFileName(x), 1)
                    Catch ex As ThreadAbortException
                        ' This exception is normal, we'll just rethrow this back up the try stack
                        Throw ex
                    Catch ex As FileNotFoundException
                        ' This would be an expected exception, nothing to do - even if we checked for
                        ' this before we called the wait function, another process could have deleted
                        ' the file before we attempt a file lock (this was happening with AREVA tool)...
                    End Try

                    ' Create a new export file
                    fileStream = File.CreateText(m_exportFileName(x))

                    ' Export file data
                    fileStream.Write(exportData.ToString())

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
                    RaiseCalculationException(ex)
                End Try
            Next

            ' We display export status every other minute
            If frame.Timestamp.Minute Mod 2 = 0 Then
                ' Make sure message is only displayed once during the minute :)
                If Not m_statusDisplayed Then
                    UpdateStatus(String.Format("{0} successful ICCP exports...", m_totalExports))
                    m_statusDisplayed = True
                End If
            Else
                m_statusDisplayed = False
            End If
        Else
            ' No data was available in the frame, lag time set too tight?
            RaiseCalculationException(New InvalidOperationException("No measurements were available for ICCP data export, possible reasons: system is initializing, receiving no data or lag time is too small.  File creation was skipped."))
        End If

    End Sub

    Protected Overrides Sub UpdateStatus(ByVal message As String)

        MyBase.UpdateStatus(String.Format("[{0}]: {1}", MyBase.ConfigurationSection, message))

    End Sub

    Private Sub m_exportQueue_ProcessException(ByVal ex As System.Exception) Handles m_exportQueue.ProcessException

        ' Something unexpected happened during export - so we'll report it. Since this code only gets executed every few seconds
        ' we go ahead and publish any error messages as needed (no volume control applied)
        RaiseCalculationException(ex)

    End Sub

End Class
