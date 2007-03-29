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

    Inherits AdapterBase
    Implements ICalculatedMeasurementAdapter

    Public Event NewCalculatedMeasurements(ByVal measurements As IList(Of IMeasurement)) Implements ICalculatedMeasurementAdapter.NewCalculatedMeasurements

    Public Event CalculationException(ByVal source As String, ByVal ex As Exception) Implements ICalculatedMeasurementAdapter.CalculationException

    ' We need to time align incoming measurements before attempting to calculate new outgoing measurement
    Private WithEvents m_concentrator As Concentrator
    Private m_calculationName As String
    Private m_configurationSection As String
    Private m_outputMeasurements As IMeasurement()
    Private m_inputMeasurementKeys As MeasurementKey()
    Private m_inputMeasurementKeysHash As List(Of MeasurementKey)
    Private m_minimumMeasurementsToUse As Integer

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
        ' the order of the input measurements may be relevant the calculation
        m_inputMeasurementKeysHash = New List(Of MeasurementKey)(inputMeasurementKeys)
        m_inputMeasurementKeysHash.Sort()

        ' Default to all measurements of minimum is not specified
        If minimumMeasurementsToUse < 1 Then
            m_minimumMeasurementsToUse = inputMeasurementKeys.Length
        Else
            m_minimumMeasurementsToUse = minimumMeasurementsToUse
        End If

        m_concentrator = New Concentrator(AddressOf PerformCalculation, expectedMeasurementsPerSecond, lagTime, leadTime)
        m_concentrator.Enabled = True

    End Sub

    Public Overrides Sub Dispose()

        m_concentrator.Enabled = False

    End Sub

    ' Note that since there may be many hundreds of calculated measurements and each measurement can have hundreds
    ' of inputs, consumer handling the queuing of input measurements should make sure that queue operations are
    ' handled on independent threads (as needed) so as to not slow up publishing of new measurements

    Public Overridable Sub QueueMeasurementForCalculation(ByVal measurement As IMeasurement) Implements ICalculatedMeasurementAdapter.QueueMeasurementForCalculation

        ' If this is an input measurement to this calculation, sort it!
        If IsInputMeasurement(measurement.Key) Then m_concentrator.SortMeasurement(measurement)

    End Sub

    Public Overridable Sub QueueMeasurementsForCalculation(ByVal measurements As IList(Of IMeasurement)) Implements ICalculatedMeasurementAdapter.QueueMeasurementsForCalculation

        If measurements IsNot Nothing Then
            For x As Integer = 0 To measurements.Count - 1
                QueueMeasurementForCalculation(measurements(x))
            Next
        End If

    End Sub

    Public Overridable Sub QueueMeasurementsForCalculation(ByVal measurements As IDictionary(Of MeasurementKey, IMeasurement)) Implements ICalculatedMeasurementAdapter.QueueMeasurementsForCalculation

        If measurements IsNot Nothing Then
            For Each measurement As IMeasurement In measurements.Values
                QueueMeasurementForCalculation(measurement)
            Next
        End If

    End Sub

    Public Overridable Property ExpectedMeasurementsPerSecond() As Integer Implements ICalculatedMeasurementAdapter.ExpectedMeasurementsPerSecond
        Get
            If m_concentrator Is Nothing Then
                Return -1
            Else
                Return m_concentrator.FramesPerSecond
            End If
        End Get
        Set(ByVal value As Integer)
            If m_concentrator IsNot Nothing Then
                m_concentrator.FramesPerSecond = value
            End If
        End Set
    End Property

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

    Protected ReadOnly Property MeasurementConcentrator() As Concentrator
        Get
            Return m_concentrator
        End Get
    End Property

    Public ReadOnly Property TicksPerFrame() As Decimal
        Get
            If m_concentrator Is Nothing Then
                Return -1D
            Else
                Return m_concentrator.TicksPerFrame
            End If
        End Get
    End Property

    Public Property LagTime() As Double Implements ICalculatedMeasurementAdapter.LagTime
        Get
            If m_concentrator Is Nothing Then
                Return -1.0R
            Else
                Return m_concentrator.LagTime
            End If
        End Get
        Set(ByVal value As Double)
            If m_concentrator Is Nothing Then
                Throw New InvalidOperationException("Cannot change allowed lag time until calculated measurement has been initialized")
            Else
                m_concentrator.LagTime = value
            End If
        End Set
    End Property

    Public Property LeadTime() As Double Implements ICalculatedMeasurementAdapter.LeadTime
        Get
            If m_concentrator Is Nothing Then
                Return -1.0R
            Else
                Return m_concentrator.LeadTime
            End If
        End Get
        Set(ByVal value As Double)
            If m_concentrator Is Nothing Then
                Throw New InvalidOperationException("Cannot change allowed lead time until calculated measurement has been initialized")
            Else
                m_concentrator.LeadTime = value
            End If
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

    Public Overrides ReadOnly Property Name() As String
        Get
            Return m_calculationName
        End Get
    End Property

    Public Overrides ReadOnly Property Status() As String
        Get
            Const MaxMeasurementsToShow As Integer = 6

            With New StringBuilder
                .Append(Name & " Status:")
                .Append(Environment.NewLine)
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
                If m_concentrator IsNot Nothing Then
                    .Append(m_concentrator.Status)
                End If
                Return .ToString()
            End With
        End Get
    End Property

    Protected Overridable Sub PublishNewCalculatedMeasurement(ByVal measurement As IMeasurement)

        PublishNewCalculatedMeasurements(New IMeasurement() {measurement})

    End Sub

    Protected Overridable Sub PublishNewCalculatedMeasurements(ByVal measurements As IList(Of IMeasurement))

        ' We handle publication of new measurements on a new thread since work to be done during
        ' publication can be time consuming (e.g., sorting and queuing) and we don't want to slow
        ' up calculation process which is being called at rates of, or over, 30 times per second
        ThreadPool.UnsafeQueueUserWorkItem(AddressOf PublishNewCalculatedMeasurements, measurements)

    End Sub

    Private Sub PublishNewCalculatedMeasurements(ByVal state As Object)

        RaiseEvent NewCalculatedMeasurements(DirectCast(state, IList(Of IMeasurement)))

    End Sub

    ''' <summary>
    ''' Users inheriting this class must override this method to implement the needed "calculation" algorithm.
    ''' </summary>
    ''' <param name="frame">Single frame of measurement data within a one second sample</param>
    ''' <param name="index">Index of frame within the one second sample</param>
    ''' <remarks>
    ''' The frame.Measurements property references a dictionary, keyed on each measurement's MeasurementKey, containing
    ''' all available measurements as defined by the InputMeasurementKeys property that arrived within the specified
    ''' LagTime.  Note that this function will be called with a frequency specified by the ExpectedMeasurementsPerSecond
    ''' property, so make sure all work to be done is executed as efficiently as possible.
    ''' </remarks>
    Protected MustOverride Sub PerformCalculation(ByVal frame As IFrame, ByVal index As Integer)

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
    Protected Sub RaiseCalculationException(ByVal ex As Exception) Handles m_concentrator.ProcessException

        RaiseEvent CalculationException(Name, ex)

    End Sub

End Class
