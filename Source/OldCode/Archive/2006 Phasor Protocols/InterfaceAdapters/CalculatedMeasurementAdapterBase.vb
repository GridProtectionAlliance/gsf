'*******************************************************************************************************
'  CalculatedMeasurementAdapterBase.vb - Calculated measurement adpater base class
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  07/29/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports TVA.Common
Imports TVA.Measurements
Imports TVA.Collections.Common

Public MustInherit Class CalculatedMeasurementAdapterBase

    Inherits ConcentratorBase
    Implements ICalculatedMeasurementAdapter

    Public Event StatusMessage(ByVal status As String) Implements IAdapter.StatusMessage

    Public Event NewCalculatedMeasurements(ByVal measurements As IList(Of IMeasurement)) Implements ICalculatedMeasurementAdapter.NewCalculatedMeasurements

    Public Event CalculationException(ByVal source As String, ByVal ex As Exception) Implements ICalculatedMeasurementAdapter.CalculationException

    ' We need to time align incoming measurements before attempting to calculate new outgoing measurement
    Private m_calculationName As String
    Private m_configurationSection As String
    Private m_outputMeasurements As IMeasurement()
    Private m_inputMeasurementKeys As MeasurementKey()
    Private m_inputMeasurementKeysHash As List(Of MeasurementKey)
    Private m_minimumMeasurementsToUse As Integer

    Public Sub New()

        MyBase.New(30, 1, 1)

    End Sub

    Public Overridable Sub Initialize( _
        ByVal calculationName As String, _
        ByVal configurationSection As String, _
        ByVal outputMeasurements As IMeasurement(), _
        ByVal inputMeasurementKeys As MeasurementKey(), _
        ByVal minimumMeasurementsToUse As Integer, _
        ByVal expectedMeasurementsPerSecond As Integer, _
        ByVal lagTime As Double, _
        ByVal leadTime As Double) Implements ICalculatedMeasurementAdapter.Initialize

        m_calculationName = calculationName
        m_configurationSection = configurationSection
        m_outputMeasurements = outputMeasurements
        m_inputMeasurementKeys = inputMeasurementKeys

        ' We create a sorted list of the input keys for quick lookup - we must keep this separate because
        ' the order of the input measurements may be relevant to the calculation
        m_inputMeasurementKeysHash = New List(Of MeasurementKey)(inputMeasurementKeys)
        m_inputMeasurementKeysHash.Sort()

        ' Default to all measurements if minimum is not specified
        If minimumMeasurementsToUse < 1 Then
            m_minimumMeasurementsToUse = inputMeasurementKeys.Length
        Else
            m_minimumMeasurementsToUse = minimumMeasurementsToUse
        End If

        Me.FramesPerSecond = expectedMeasurementsPerSecond
        Me.LagTime = lagTime
        Me.LeadTime = leadTime

    End Sub

    Public Overrides Sub Start() Implements ICalculatedMeasurementAdapter.Start

        ' Start measurement concentration
        MyBase.Start()

    End Sub

    Public Overrides Sub [Stop]() Implements ICalculatedMeasurementAdapter.Stop

        ' Stop measurement concentration
        MyBase.Stop()

    End Sub

    Public Overridable Sub QueueMeasurementForCalculation(ByVal measurement As IMeasurement) Implements ICalculatedMeasurementAdapter.QueueMeasurementForCalculation

        ' If this is an input measurement to this calculation, sort it!
        If IsInputMeasurement(measurement.Key) Then SortMeasurement(measurement)

    End Sub

    Public Overridable Sub QueueMeasurementsForCalculation(ByVal measurements As ICollection(Of IMeasurement)) Implements ICalculatedMeasurementAdapter.QueueMeasurementsForCalculation

        Dim inputMeasurements As New List(Of IMeasurement)

        For Each measurement As IMeasurement In measurements
            If IsInputMeasurement(measurement.Key) Then inputMeasurements.Add(measurement)
        Next

        If inputMeasurements.Count > 0 Then SortMeasurements(inputMeasurements)

    End Sub

    Public Overridable Property OutputMeasurements() As IMeasurement() Implements ICalculatedMeasurementAdapter.OutputMeasurements
        Get
            Return m_outputMeasurements
        End Get
        Set(ByVal value As IMeasurement())
            m_outputMeasurements = value
        End Set
    End Property

    Public Overridable Property InputMeasurementKeys() As MeasurementKey() Implements ICalculatedMeasurementAdapter.InputMeasurementKeys
        Get
            Return m_inputMeasurementKeys
        End Get
        Set(ByVal value As MeasurementKey())
            m_inputMeasurementKeys = value

            ' Update input key lookup hash table
            m_inputMeasurementKeysHash = New List(Of MeasurementKey)(value)
            m_inputMeasurementKeysHash.Sort()
        End Set
    End Property

    Public Overridable ReadOnly Property IsInputMeasurement(ByVal item As MeasurementKey) As Boolean
        Get
            Return (m_inputMeasurementKeysHash.BinarySearch(item) >= 0)
        End Get
    End Property

    Public Overridable Property MinimumMeasurementsToUse() As Integer Implements ICalculatedMeasurementAdapter.MinimumMeasurementsToUse
        Get
            Return m_minimumMeasurementsToUse
        End Get
        Set(ByVal value As Integer)
            m_minimumMeasurementsToUse = value
        End Set
    End Property

    Public Overridable Property ConfigurationSection() As String Implements ICalculatedMeasurementAdapter.ConfigurationSection
        Get
            Return m_configurationSection
        End Get
        Set(ByVal value As String)
            m_configurationSection = value
        End Set
    End Property

    Public ReadOnly Property Name() As String Implements IAdapter.Name
        Get
            Return m_calculationName
        End Get
    End Property

    Public Overrides ReadOnly Property Status() As String Implements IAdapter.Status
        Get
            Const MaxMeasurementsToShow As Integer = 6

            With New StringBuilder
                .Append("   Output measurement ID's: ")
                If m_outputMeasurements.Length <= MaxMeasurementsToShow Then
                    .Append(ListToString(m_outputMeasurements, ","c))
                Else
                    Dim outputMeasurements As IMeasurement() = CreateArray(Of IMeasurement)(MaxMeasurementsToShow)
                    Array.Copy(m_outputMeasurements, 0, outputMeasurements, 0, MaxMeasurementsToShow)
                    .Append(ListToString(outputMeasurements, ","c))
                    .Append(",...")
                End If
                .Append(Environment.NewLine)
                .Append("    Input measurement ID's: ")
                If m_inputMeasurementKeys.Length <= MaxMeasurementsToShow Then
                    .Append(ListToString(m_inputMeasurementKeys, ","c))
                Else
                    Dim inputMeasurements As MeasurementKey() = CreateArray(Of MeasurementKey)(MaxMeasurementsToShow)
                    Array.Copy(m_inputMeasurementKeys, 0, inputMeasurements, 0, MaxMeasurementsToShow)
                    .Append(ListToString(inputMeasurements, ","c))
                    .Append(",...")
                End If
                .Append(Environment.NewLine)
                .Append(" Minimum measurements used: ")
                .Append(m_minimumMeasurementsToUse)
                .Append(Environment.NewLine)
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    Protected Overridable Sub PublishNewCalculatedMeasurement(ByVal measurement As IMeasurement)

        PublishNewCalculatedMeasurements(New IMeasurement() {measurement})

    End Sub

    Protected Overridable Sub PublishNewCalculatedMeasurements(ByVal measurements As IList(Of IMeasurement))

        RaiseEvent NewCalculatedMeasurements(measurements)

    End Sub

    ''' <summary>
    ''' Attempts to retrieve the minimum needed number of measurements from the frame (as specified by MinimumMeasurementsToUse)
    ''' </summary>
    ''' <param name="frame">Source frame for the measurements</param>
    ''' <param name="measurements">Return array of measurements</param>
    ''' <returns>True if minimum needed number of measurements were returned in measurement array</returns>
    ''' <remarks>
    ''' <para>
    ''' Remember this function will *only* return the minimum needed number of measurements, no more.  If you want to use
    ''' all available measurements in your calculation you should just use Frame.Measurements.Values directly.
    ''' </para>
    ''' <para>
    ''' Note that the measurements array parameter will be created if the reference is null, otherwise if caller creates
    ''' array it must be sized to MinimumMeasurementsToUse
    ''' </para>
    ''' </remarks>
    Protected Overridable Function TryGetMinimumNeededMeasurements(ByVal frame As IFrame, ByRef measurements As IMeasurement()) As Boolean

        Dim index As Integer
        Dim measurement As IMeasurement = Nothing

        If measurements Is Nothing Then measurements = CreateArray(Of IMeasurement)(m_minimumMeasurementsToUse)

        ' Loop through all input measurements to see if they exist in this frame
        For x As Integer = 0 To m_inputMeasurementKeys.Length - 1
            If frame.Measurements.TryGetValue(m_inputMeasurementKeys(x), measurement) Then
                measurements(index) = measurement
                index += 1
                If index = m_minimumMeasurementsToUse Then Exit For
            End If
        Next

        Return (index = m_minimumMeasurementsToUse)

    End Function

    ' Bubble any exceptions out to the consumer
    Protected Sub RaiseCalculationException(ByVal ex As Exception) Handles Me.ProcessException

        RaiseEvent CalculationException(Name, ex)

    End Sub

    Protected Overridable Sub UpdateStatus(ByVal message As String)

        RaiseEvent StatusMessage(message)

    End Sub

    Private Sub CalculatedMeasurementAdapterBase_UnpublishedSamples(ByVal total As Integer) Handles Me.UnpublishedSamples

        If total > 2 * Convert.ToInt32(Math.Ceiling(LagTime)) Then UpdateStatus(String.Format("WARNING: There are {0} unpublished samples in the calculated measurement """ & Name & """ concentration queue...", total))

    End Sub

End Class
