'*******************************************************************************************************
'  EventDetectTrigger.vb - Detect the events if the frequency change rate exceed the 
'                          pre-set threshold
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Jian Zuo(Ryan), Operations Data Analysis [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2S-118
'       Phone: 423/751-2570
'       Email: jrzuo@tva.gov; ryanzuo@vt.edu
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/15/2006 - Jian Zuo (Ryan)
'       Initial version of source generated
'
'*******************************************************************************************************
Imports System.Text
Imports System.Math
Imports TVA.Common
Imports TVA.Math.Common
Imports TVA.IO
Imports TVA.Measurements
Imports InterfaceAdapters


Public Class EventDetectTrigger
    Inherits CalculatedMeasurementAdapterBase

    Public Event EventDetected(ByVal eventTime As Long)

#Region "Private members"
    'The frequency change rate threshold (Hz/Sec)
    Private m_threshold As Double
    'Time tick for most recent detected event
    Private m_eventTime As Long
    'Event type
    Private m_eventType As eventType
    'Calculation window size (second)
    Private m_windowSize As Integer
    'The unCheck time interval between current time and the most recent event time (second)
    Private m_unCheckInterval As Integer
    Private m_freqElement As freqElement
    Private m_freqBuffer As ArrayList = New ArrayList()
    'm_freqBuffer max capacity
    Private m_maxCapacity As Integer
    'PMU or FDR data Rate points/second
    Private m_dataRate As Integer
    'MW/HZ ratio 
    Private m_MWHZRatio As Double
    'Generator or load trip amount
    Private m_tripAmount As Double

#End Region

#Region "Type and Structure define"
    Enum eventType
        genTrip
        loadTrip
    End Enum


    Structure freqElement
        Dim freq As Double
        Dim timeTick As Long

    End Structure

#End Region

#Region "Public Methods"

    Public Overrides Sub Initialize(ByVal calculationName As String, ByVal configurationSection As String, ByVal outputMeasurements As IMeasurement(), ByVal inputMeasurementKeys As MeasurementKey(), ByVal minimumMeasurementsToUse As Integer, ByVal expectedMeasurementsPerSecond As Integer, ByVal lagTime As Double, ByVal leadTime As Double)

        MyBase.Initialize(calculationName, configurationSection, outputMeasurements, inputMeasurementKeys, minimumMeasurementsToUse, expectedMeasurementsPerSecond, lagTime, leadTime)
        MyClass.MinimumMeasurementsToUse = minimumMeasurementsToUse
        m_windowSize = 4 ' Default calculation window size
        m_unCheckInterval = 10 ' Default time interval
        m_maxCapacity = 300
        m_dataRate = 30        ' Points/Second
        m_threshold = 0.004    ' Hz/Second
        m_MWHZRatio = 29000    ' Default parameter for Eastern interconnection

    End Sub
    ''' <summary>
    ''' Calculates the frequency change rate and compared them with the pre-set threshold.
    ''' </summary>
    ''' <param name="frame">Single frame of measurement data within a one second sample</param>
    ''' <param name="index">Index of frame within the one second sample</param>
    ''' <remarks>
    ''' The frame.Measurements property references a dictionary, keyed on each measurement's MeasurementKey, containing
    ''' all available measurements as defined by the InputMeasurementKeys property that arrived within the specified
    ''' LagTime.  Note that this function will be called with a frequency specified by the ExpectedMeasurementsPerSecond
    ''' property, so make sure all work to be done is executed as efficiently as possible.
    ''' </remarks>

    Protected Overrides Function PublishFrame(ByVal frame As TVA.Measurements.IFrame, ByVal index As Integer) As Integer
        Dim averageFreq As Double

        With frame.Measurements
            'There may be several monitoring units in the same interconnection
            If .Count > 0 Then
                Dim freqTotal As Double
                For Each measurements As IMeasurement In .Values
                    freqTotal += measurements.AdjustedValue
                Next
                averageFreq = freqTotal / .Count
                Dim count As Integer = m_freqBuffer.Count
                If count > 10 Then
                    Dim pos As Integer
                    For pos = count - 1 To count - 9
                        averageFreq += DirectCast(m_freqBuffer.Item(pos), freqElement).freq
                    Next
                    averageFreq = averageFreq / 10
                End If
                m_freqElement.freq = averageFreq
                m_freqElement.timeTick = frame.Ticks
                m_freqBuffer.Add(m_freqElement)
                'Prevent multi trigger the same event
                If New DateTime(frame.Ticks - m_eventTime).Second > m_unCheckInterval Then
                    If checkEvent() Then
                        RaiseEvent EventDetected(m_eventTime)
                        'Prevent mutiple trigger the same event
                        m_freqBuffer.Clear()

                    End If
                End If

                If m_freqBuffer.Count > m_maxCapacity Then
                    m_freqBuffer.RemoveAt(0)
                End If

                ' Return count of measurements handled by calculation
                Return .Count
            End If

        End With

    End Function

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append(Name & " Status:")
                .Append(Environment.NewLine)
                .Append(MyBase.Status)
                Return .ToString()
            End With
        End Get
    End Property

    Private Function checkEvent() As Boolean
        Dim element1, element2 As freqElement
        Dim DfDt1, DfDt2 As Double
        Dim count As Integer
        count = m_freqBuffer.Count
        'Data in buffer should be at least 5 second
        If count < 5 * m_dataRate Then
            Return False
        Else
            element1 = CType(m_freqBuffer.Item(count - 5 * m_dataRate), freqElement)
            element2 = CType(m_freqBuffer.Item(count - 1 * m_dataRate), freqElement)
            DfDt1 = (element1.freq - element2.freq) / ((element1.timeTick - element2.timeTick) / 10000000)
            element1 = CType(m_freqBuffer.Item(count - 4 * m_dataRate - 1), freqElement)
            element2 = CType(m_freqBuffer.Item(count - 1), freqElement)
            DfDt2 = (element1.freq - element2.freq) / ((element1.timeTick - element2.timeTick) / 10000000)
            If DfDt1 > m_threshold AndAlso DfDt2 > m_threshold Then
                m_eventType = eventType.genTrip
                m_eventTime = element2.timeTick
                m_tripAmount = System.Math.Abs((element1.freq - element2.freq) * m_MWHZRatio)
                Return True
            ElseIf DfDt1 < -m_threshold AndAlso DfDt2 < -m_threshold Then
                m_eventType = eventType.loadTrip
                m_eventTime = element2.timeTick
                m_tripAmount = System.Math.Abs((element1.freq - element2.freq) * m_MWHZRatio)
                Return True
            Else
                Return False
            End If
        End If
    End Function
#End Region

#Region "Property"
    ''' <summary>
    ''' Property of the frequency changing rate threshold
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property threshold() As Double
        Get
            Return m_threshold

        End Get
        Set(ByVal value As Double)
            m_threshold = value
        End Set
    End Property
    ''' <summary>
    ''' Property of detected event type
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property eventTypes() As eventType
        Get
            Return m_eventType
        End Get
    End Property
    ''' <summary>
    ''' Property for detected event time
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property eventTime() As Long
        Get
            Return m_eventTime
        End Get
    End Property
    ''' <summary>
    ''' Property for the calculation window size
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Public Property windowSize() As Integer
        Get
            Return m_windowSize
        End Get
        Set(ByVal value As Integer)
            m_windowSize = value
        End Set
    End Property
    ''' <summary>
    ''' Property for the time interval between the current time and most recent event time
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property unCheckInterval() As Integer
        Get
            Return m_unCheckInterval
        End Get
        Set(ByVal value As Integer)
            m_unCheckInterval = value
        End Set
    End Property
    ''' <summary>
    ''' Property for the max capacity of m_freqBuffer
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Public Property maxCapacity() As Integer
        Get
            Return m_maxCapacity
        End Get
        Set(ByVal value As Integer)
            m_maxCapacity = value

        End Set
    End Property
    ''' <summary>
    ''' Property for PMU or FDR data Rate
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Public Property dataRate() As Integer
        Get
            Return m_dataRate
        End Get
        Set(ByVal value As Integer)
            m_dataRate = value
        End Set
    End Property
    ''' <summary>
    ''' Property for MW HZ Ratio
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MWHZRatio() As Double
        Get
            Return m_MWHZRatio
        End Get
        Set(ByVal value As Double)
            m_MWHZRatio = value
        End Set
    End Property
    Public ReadOnly Property tripAmount() As Double
        Get
            Return m_tripAmount
        End Get
    End Property
#End Region


End Class
