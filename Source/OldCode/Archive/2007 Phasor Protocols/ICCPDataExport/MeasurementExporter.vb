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
Imports TVA.IO
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

    Private m_measurementTags As Dictionary(Of MeasurementKey, String)
    Private m_signalTypes As Dictionary(Of MeasurementKey, String)
    Private m_useReferenceAngle As Boolean
    Private m_referenceAngleKey As MeasurementKey
    Private m_exportInterval As Integer
    Private m_statusDisplayed As Boolean
    Private m_sqrtOf3 As Double
    Private WithEvents m_dataExporter As MultipleDestinationExporter

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

            ' Save updates to config file, if any
            SaveSettings()

            ' Load needed settings
            m_exportInterval = Convert.ToInt32(.Item("ExportInterval").Value)
            m_useReferenceAngle = ParseBoolean(.Item("UseReferenceAngle").Value)
        End With

        ' Define default export location - user can override in config file later...
        Dim defaultDestinations As ExportDestination() = New ExportDestination() _
            {New ExportDestination("\\152.85.98.24\pmu\PMU.txt", True, "TVA", "esocss", "pwd4ctrl")}

        m_dataExporter = New MultipleDestinationExporter(MyBase.ConfigurationSection, m_exportInterval * 1000, defaultDestinations)

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
                If m_dataExporter IsNot Nothing Then .Append(m_dataExporter.Status)
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
        If (New Date(ticks)).Second Mod m_exportInterval = 0 AndAlso TicksBeyondSecond(ticks) = 0 Then
            Dim measurements As IDictionary(Of MeasurementKey, IMeasurement) = frame.Measurements

            If measurements.Count > 0 Then
                Dim fileData As New StringBuilder
                Dim measurement, referenceAngle As IMeasurement
                Dim measurementTag, signalType As String
                Dim measurementValue As Double
                Dim measurementQuality As ICCPDataQuality

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
                        fileData.Append(measurementTag)
                        fileData.Append(","c)

                        ' Export measurement value, making any needed adjustments
                        If m_signalTypes.TryGetValue(inputMeasurementKey, signalType) Then
                            If String.Compare(signalType, "VPHA") = 0 OrElse String.Compare(signalType, "IPHA") = 0 Then
                                ' This is a phase angle measurement, export the value relative to the reference angle (if available)
                                If referenceAngle Is Nothing Then
                                    fileData.Append(measurementValue)
                                Else
                                    fileData.Append(referenceAngle.AdjustedValue - measurementValue)
                                End If
                            ElseIf String.Compare(signalType, "VPHM") = 0 Then
                                ' Voltage from PMU's is line-to-neutral volts, we convert this to line-to-line kilovolts
                                fileData.Append(measurementValue * m_sqrtOf3 / 1000.0R)
                            Else
                                ' All other measurements are exported using their raw value
                                fileData.Append(measurementValue)
                            End If
                        Else
                            ' We were unable to find signal type for this key - this is unexpected
                            RaiseCalculationException(New InvalidOperationException(String.Format("Failed to find signal type for measurement {0}", inputMeasurementKey)))
                        End If

                        ' Export measurement quality
                        fileData.Append(","c)
                        fileData.Append(measurementQuality)

                        ' Terminate line - AREVA ICCP file link expects these two terminating commas, weird...
                        fileData.AppendLine(",,")
                    Else
                        ' We were unable to find measurement tag for this key - this is unexpected
                        RaiseCalculationException(New InvalidOperationException(String.Format("Failed to find measurement tag for measurement {0}", inputMeasurementKey)))
                    End If
                Next

                ' Measurement export to a file may take more than 1/30 of a second - so we queue this work up...
                m_dataExporter.ExportData(fileData.ToString())

                ' We display export status every other minute
                If frame.Timestamp.Minute Mod 2 = 0 Then
                    ' Make sure message is only displayed once during the minute :)
                    If Not m_statusDisplayed Then
                        UpdateStatus(String.Format("{0} successful ICCP exports...", m_dataExporter.TotalExports))
                        m_statusDisplayed = True
                    End If
                Else
                    m_statusDisplayed = False
                End If
            Else
                ' No data was available in the frame, lag time set too tight?
                RaiseCalculationException(New InvalidOperationException("No measurements were available for ICCP data export, possible reasons: system is initializing, receiving no data or lag time is too small.  File creation was skipped."))
            End If
        End If

    End Sub

    Protected Overrides Sub UpdateStatus(ByVal message As String) Handles m_dataExporter.StatusMessage

        MyBase.UpdateStatus(String.Format("[{0}]: {1}", MyBase.ConfigurationSection, message))

    End Sub

End Class
