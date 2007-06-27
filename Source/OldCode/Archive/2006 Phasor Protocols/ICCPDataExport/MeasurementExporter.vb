'*******************************************************************************************************
'  MeasurementExporter.vb - ICCP data export module
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
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
Imports TVA.Measurements
Imports TVA.Configuration.Common
Imports TVA.IO.FilePath
Imports TVA.Data.Common
Imports TVA.DateTime.Common
Imports InterfaceAdapters

Public Class MeasurementExporter

    Inherits CalculatedMeasurementAdapterBase

    Private Const DefaultConfigSection As String = "ICCPDataExportModule"

    Private m_measurementTags As Dictionary(Of MeasurementKey, String)
    Private m_signalTypes As Dictionary(Of MeasurementKey, String)
    Private m_referenceAngleKey As MeasurementKey
    Private m_exportInterval As Integer
    Private m_exportFileName As String
    Private m_exportCount As Long
    Private m_statusDisplayed As Boolean
    Private m_sqrtOf3 As Double

    Public Sub New()
    End Sub

    Public Overrides Sub Initialize(ByVal calculationName As String, ByVal configurationSection As String, ByVal outputMeasurements As IMeasurement(), ByVal inputMeasurementKeys As MeasurementKey(), ByVal minimumMeasurementsToUse As Integer, ByVal expectedMeasurementsPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        MyBase.Initialize(calculationName, configurationSection, outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime)
        If String.IsNullOrEmpty(configurationSection) Then MyBase.ConfigurationSection = DefaultConfigSection

        With CategorizedSettings(MyBase.ConfigurationSection)
            ' Make sure needed configuration variables exist - since configuration variables will
            ' be added to config file of parent process we add them to a new configuration category
            .Add("ExportInterval", "5", "Data export interval, in seconds")
            .Add("ExportShare", "\\152.85.98.6\pmu", "UNC path (\\server\share) name for export file")
            .Add("ExportShare.Domain", "SOCOPPMU", "Domain used for authentication to UNC path (computer name for local accounts", False)
            .Add("ExportShare.UserName", "shukri", "User name used for authentication to UNC path")
            .Add("ExportShare.Password", "shukri", "Encrypted password used for authentication to UNC path", True)
            .Add("ExportShare.FileName", "\PMU.txt", "Path and file name of ICCP data export - must be prefixed with directory separator")

            ' Save updates to config file, if any
            SaveSettings()

            ' Load needed settings
            m_exportInterval = Convert.ToInt32(.Item("ExportInterval").Value)
            m_exportFileName = .Item("ExportShare").Value.ToString() & .Item("ExportShare.FileName").Value.ToString()

            ' Attempt connection to external network share
            ConnectToNetworkShare( _
                .Item("ExportShare").Value, _
                .Item("ExportShare.UserName").Value, _
                .Item("ExportShare.Password").Value, _
                .Item("ExportShare.Domain").Value)
        End With

        ' Create new measurement dictionaries
        m_measurementTags = New Dictionary(Of MeasurementKey, String)
        m_signalTypes = New Dictionary(Of MeasurementKey, String)

        ' Calculate the square root of 3
        m_sqrtOf3 = Math.Sqrt(3.0R)

        ' Need to open database connection to load measurement tags and signal types.
        ' Note that data connection string defined in config file of parent process.
        Dim connection As New OleDbConnection(StringSetting("PMUDatabase"))

        connection.Open()

        ' NOTE: When you get around to implementing both archive source and ID in the measurements table
        ' these queries will need to be changed :)

        ' Populate measurement tag and signal type dictionaries
        For x As Integer = 0 To inputMeasurementKeys.Length - 1
            ' Load measurement tag name
            m_measurementTags.Add(inputMeasurementKeys(x), _
                ExecuteScalar("SELECT PointTag FROM Measurements WHERE ID=" & _
                inputMeasurementKeys(x).ID, connection).ToString().Replace("-"c, "_"c).Replace(":"c, "_"c))

            ' Load measurement signal type
            m_signalTypes.Add(inputMeasurementKeys(x), _
                ExecuteScalar("SELECT SignalID FROM Measurements WHERE ID=" & _
                inputMeasurementKeys(x).ID, connection).ToString())
        Next

        ' Lastly, we also need to determine which angle is the reference angle
        With RetrieveRow("SELECT * FROM OutputReferenceAngleMeasurement", connection)
            m_referenceAngleKey = New MeasurementKey( _
                Convert.ToInt32(.Item("MeasurementID")), _
                .Item("ArchiveSource").ToString())
        End With

        connection.Close()

    End Sub

    Public Overrides Sub Dispose()

        MyBase.Dispose()

        GC.SuppressFinalize(Me)

        ' We'll be nice and disconnect network share when this class is disposed...
        DisconnectFromNetworkShare(CategorizedStringSetting(ConfigurationSection, "ExportShare"))

    End Sub

    Protected Overrides Sub Finalize()

        Dispose()

    End Sub

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append(MyBase.Status)
                .Append("   Total ICCP measurements: ")
                .Append(m_measurementTags.Count)
                .Append(Environment.NewLine)
                .Append("   Defined export interval: ")
                .Append(m_exportInterval)
                .Append(" seconds")
                .Append(Environment.NewLine)
                .Append("     ICCP export file name: ")
                .Append(m_exportFileName)
                .Append(Environment.NewLine)
                .Append("      Total exports so far: ")
                .Append(m_exportCount)
                .Append(Environment.NewLine)
                Return .ToString()
            End With
        End Get
    End Property

    ' To optimize performance, we only sort the exact measurements that will be needed
    Public Overrides Sub QueueMeasurementForCalculation(ByVal measurement As IMeasurement)

        If IsTimeToExport(measurement.Ticks) Then MyBase.QueueMeasurementForCalculation(measurement)

    End Sub

    Public Overrides Sub QueueMeasurementsForCalculation(ByVal measurements As IList(Of IMeasurement))

        If measurements IsNot Nothing Then
            For x As Integer = 0 To measurements.Count - 1
                QueueMeasurementForCalculation(measurements(x))
            Next
        End If

    End Sub

    Public Overrides Sub QueueMeasurementsForCalculation(ByVal measurements As IDictionary(Of MeasurementKey, IMeasurement))

        If measurements IsNot Nothing Then
            For Each measurement As IMeasurement In measurements.Values
                QueueMeasurementForCalculation(measurement)
            Next
        End If

    End Sub

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

        ' We only export data at the specified interval
        If IsTimeToExport(frame.Ticks) Then
            ' Note: since this code only gets executed every few seconds we go ahead and publish any error messages as needed
            With frame.Measurements
                ' Make sure there are measurements to export
                If .Count > 0 Then
                    Dim referenceAngle As IMeasurement

                    ' We need to get calculated reference angle value in order to export relative phase angles
                    ' If the value is not here, we don't export
                    If .TryGetValue(m_referenceAngleKey, referenceAngle) Then
                        ' Create a new export file
                        Dim fileStream As StreamWriter = File.CreateText(m_exportFileName)
                        Dim measurementTag, signalType As String

                        ' Write all measurements in this frame to the export file
                        For Each measurement As IMeasurement In .Values
                            ' Look up measurement's tag name
                            If m_measurementTags.TryGetValue(measurement.Key, measurementTag) Then
                                With fileStream
                                    ' Export tag name field
                                    .Write(measurementTag)
                                    .Write(","c)

                                    ' If we are exporting an angle measurement, we need to make sure it is relative to the reference angle
                                    If m_signalTypes.TryGetValue(measurement.Key, signalType) Then
                                        If String.Compare(signalType, "VPHA", True) = 0 OrElse String.Compare(signalType, "IPHA", True) = 0 Then
                                            ' This is a phase angle measurement, export the value relative to the reference angle
                                            .Write(referenceAngle.AdjustedValue - measurement.AdjustedValue)
                                        ElseIf String.Compare(signalType, "VPHM", True) = 0 Then
                                            ' Voltage from PMU's is line-to-neutral volts, we convert this to line-to-line kilovolts
                                            .Write(referenceAngle.AdjustedValue * m_sqrtOf3 / 1000.0R)
                                        Else
                                            ' All other measurements are exported using their raw value
                                            .Write(measurement.AdjustedValue)
                                        End If
                                    Else
                                        ' We were unable to find signal type for this key - this is unexpected
                                        RaiseCalculationException(New InvalidOperationException("Failed to find signal type for measurement " & measurement.Key.ToString()))
                                    End If

                                    ' This export field reserved for quality?  Need to ask Tanya
                                    .Write(","c)
                                    .WriteLine(",0,")
                                End With
                            Else
                                ' We were unable to find measurement tag for this key - this is unexpected
                                RaiseCalculationException(New InvalidOperationException("Failed to find measurement tag for measurement " & measurement.Key.ToString()))
                            End If
                        Next

                        fileStream.Close()
                        m_exportCount += 1

                        ' We display export status every other minute
                        If frame.Timestamp.Minute Mod 2 = 0 Then
                            ' Make sure message is only displayed once during the minute :)
                            If Not m_statusDisplayed Then
                                UpdateStatus(m_exportCount & " ICCP data exports have been successfully completed...")
                                m_statusDisplayed = True
                            End If
                        Else
                            m_statusDisplayed = False
                        End If
                    Else
                        ' We were unable to find reference angle in this data concentration pass, lag time too small?
                        RaiseCalculationException(New InvalidOperationException("Calculated reference angle was not found in this frame, possible reasons: system is initializing, receiving no data or lag time is too small.  File creation was skipped."))
                    End If
                Else
                    ' No data was available in the frame, lag time set too tight?
                    RaiseCalculationException(New InvalidOperationException("No measurements were available for ICCP data export, possible reasons: system is initializing, receiving no data or lag time is too small.  File creation was skipped."))
                End If
            End With
        End If

    End Sub

    Private Function IsTimeToExport(ByVal ticks As Long) As Boolean

        Return ((New Date(ticks)).Second Mod m_exportInterval = 0 AndAlso TicksBeyondSecond(ticks) = 0)

    End Function

End Class
