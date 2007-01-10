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
Imports System.Data.SqlClient
Imports Tva.Measurements
Imports Tva.Configuration.Common
Imports Tva.IO.FilePath
Imports Tva.Data.Common
Imports InterfaceAdapters

Public Class MeasurementExporter

    Inherits CalculatedMeasurementAdapterBase

    Private Const ConfigSection As String = "ICCPDataExportModule"

    Private m_measurementTags As Dictionary(Of MeasurementKey, String)

    Public Sub New()

        ' Make sure needed configuration variables exist - since configuration variables will
        ' be added to config file of parent process we add them to a new configuration category
        With CategorizedSettings(ConfigSection)
            .Add("ExportInterval", "5", "Data export interval, in seconds")
            .Add("ExportShare", "\\152.85.98.6\pmu\", "UNC path (\\server\share) name for export file")
            .Add("ExportShare.Domain", "SOCOPPMU", "Domain used for authentication to UNC path (computer name for local accounts", False)
            .Add("ExportShare.UserName", "shukri", "User name used for authentication to UNC path")
            .Add("ExportShare.Password", "shukri", "Encrypted password used for authentication to UNC path", True)
            .Add("ExportShare.FileName", "PMU.txt", "File name of ICCP data export")

            ' Save updates to config file, if any
            SaveSettings()
        End With

        m_measurementTags = New Dictionary(Of MeasurementKey, String)

    End Sub

    Public Overrides Sub Initialize(ByVal outputMeasurements() As Tva.Measurements.IMeasurement, ByVal inputMeasurementKeys() As Tva.Measurements.MeasurementKey, ByVal minimumMeasurementsToUse As Integer, ByVal expectedMeasurementsPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        MyBase.Initialize(outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime)

        ' Attempt connection to external network share - this only needs to be done once
        With CategorizedSettings(ConfigSection)
            ConnectToNetworkShare( _
                .Item("ExportShare").Value, _
                .Item("ExportShare.UserName").Value, _
                .Item("ExportShare.Password").Value, _
                .Item("ExportShare.Domain").Value)
        End With

        ' Data connection string defined in config file of parent process
        Dim connection As New SqlConnection(StringSetting("PMUDatabase"))

        connection.Open()

        For x As Integer = 0 To inputMeasurementKeys.Length - 1
            ' HACK: When you get around to putting implementing both archive source AND id in the measurements table - this MUST be changed :)
            m_measurementTags.Add(inputMeasurementKeys(x), ExecuteScalar("SELECT PointTag FROM Measurements WHERE ID=" & inputMeasurementKeys(x).ID, connection).ToString().Replace("-"c, "_"c).Replace(":"c, "_"c))
        Next

        connection.Close()

    End Sub

    Public Overrides Sub Dispose()

        MyBase.Dispose()

        ' We'll be nice and disconnect network share when class is garbage collected...
        DisconnectFromNetworkShare(CategorizedStringSetting(ConfigSection, "ExportShare"))

        GC.SuppressFinalize(Me)

    End Sub

    Protected Overrides Sub Finalize()

        Dispose()

    End Sub

    Public Overrides ReadOnly Property Name() As String
        Get
            Return "ICCP Data Export Module"
        End Get
    End Property

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append(Name & " Status:")
                .Append(Environment.NewLine)
                '.Append(" Last calculated frequency: ")
                '.Append(m_averageFrequency)
                .Append(Environment.NewLine)
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
    Protected Overrides Sub PerformCalculation(ByVal frame As IFrame, ByVal index As Integer)


        ' calculate Mod interval based .Add("ExportInterval") in seconds

        With frame.Measurements
            ' We need to get at least one frequency for this calculation...
            If .Count > 0 Then

                ' Provide calculated measurement for external consumption
                'PublishNewCalculatedMeasurement(Measurement.Clone(OutputMeasurements(0), m_averageFrequency, frame.Ticks))
            Else
                ' TODO: Raise warning when minimum set of frequency's are not available for calculation - but not 30 times per second :)
                'RaiseCalculationException(
            End If
        End With

    End Sub

End Class
