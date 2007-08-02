'*******************************************************************************************************
'  TVA.Measurements.Frame.vb - Basic frame implementation
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents a basic measured value
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  6/22/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    ''' <summary>Implementation of a basic frame</summary>
    Public Class Frame

        Implements IFrame

        Private m_ticks As Long
        Private m_published As Boolean
        Private m_publishedMeasurements As Integer
        Private m_measurements As Dictionary(Of MeasurementKey, IMeasurement)
        Private m_startSortTime As Long
        Private m_lastSortTime As Long
        Private m_lastSortedMeasurement As IMeasurement

        Public Sub New(ByVal ticks As Long)

            m_ticks = ticks
            m_measurements = New Dictionary(Of MeasurementKey, IMeasurement)
            m_publishedMeasurements = -1

        End Sub

        Public Sub New(ByVal ticks As Long, ByVal measurements As Dictionary(Of MeasurementKey, IMeasurement), ByVal startSortTime As Long, ByVal lastSortTime As Long)

            m_ticks = ticks
            m_measurements = New Dictionary(Of MeasurementKey, IMeasurement)(measurements)
            m_startSortTime = startSortTime
            m_lastSortTime = lastSortTime
            m_publishedMeasurements = -1

        End Sub

        ''' <summary>Handy instance reference to self</summary>
        Public ReadOnly Property This() As IFrame Implements IFrame.This
            Get
                Return Me
            End Get
        End Property

        ''' <summary>Create a copy of this frame and its measurements</summary>
        ''' <remarks>This frame's measurement dictionary is synclocked during copy</remarks>
        Public Function Clone() As IFrame Implements IFrame.Clone

            SyncLock m_measurements
                Return New Frame(m_ticks, m_measurements, m_startSortTime, m_lastSortTime)
            End SyncLock

        End Function

        ''' <summary>Keyed measurements in this frame</summary>
        Public ReadOnly Property Measurements() As IDictionary(Of MeasurementKey, IMeasurement) Implements IFrame.Measurements
            Get
                Return m_measurements
            End Get
        End Property

        ''' <summary>Gets or sets published state of this frame</summary>
        Public Property Published() As Boolean Implements IFrame.Published
            Get
                Return m_published
            End Get
            Set(ByVal value As Boolean)
                m_published = value
            End Set
        End Property

        ''' <summary>Gets or sets total number of measurements that have been published for this frame</summary>
        ''' <remarks>If this property has not been assigned a value, the property will return measurement count</remarks>
        Public Property PublishedMeasurements() As Integer Implements IFrame.PublishedMeasurements
            Get
                If m_publishedMeasurements = -1 Then Return m_measurements.Count
                Return m_publishedMeasurements
            End Get
            Set(ByVal value As Integer)
                m_publishedMeasurements = value
            End Set
        End Property

        ''' <summary>Exact timestamp of the data represented in this frame</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Public Property Ticks() As Long Implements IFrame.Ticks
            Get
                Return m_ticks
            End Get
            Set(ByVal value As Long)
                m_ticks = value
            End Set
        End Property

        ''' <summary>Date representation of ticks of this frame</summary>
        Public ReadOnly Property Timestamp() As Date Implements IFrame.Timestamp
            Get
                Return New Date(m_ticks)
            End Get
        End Property

        ''' <summary>Ticks of when first measurement was sorted into this frame</summary>
        Public Property StartSortTime() As Long Implements IFrame.StartSortTime
            Get
                Return m_startSortTime
            End Get
            Set(ByVal value As Long)
                m_startSortTime = value
            End Set
        End Property

        ''' <summary>Ticks of when last measurement was sorted into this frame</summary>
        Public Property LastSortTime() As Long Implements IFrame.LastSortTime
            Get
                Return m_lastSortTime
            End Get
            Set(ByVal value As Long)
                m_lastSortTime = value
            End Set
        End Property

        ''' <summary>Last measurement that was sorted into this frame</summary>
        Public Property LastSortedMeasurement() As IMeasurement Implements IFrame.LastSortedMeasurement
            Get
                Return m_lastSortedMeasurement
            End Get
            Set(ByVal value As IMeasurement)
                m_lastSortedMeasurement = value
            End Set
        End Property

        ''' <summary>Returns True if the timestamp of this frame equals the timestamp of the specified other frame</summary>
        Public Overloads Function Equals(ByVal other As IFrame) As Boolean Implements System.IEquatable(Of IFrame).Equals

            Return (CompareTo(other) = 0)

        End Function

        ''' <summary>Returns True if the timestamp of this frame equals the timestamp of the specified other frame</summary>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean

            Dim other As IFrame = TryCast(obj, IFrame)
            If other IsNot Nothing Then Return Equals(other)
            Throw New ArgumentException("Object is not an IFrame")

        End Function

        ''' <summary>This implementation of a basic measurement compares itself by timestamp</summary>
        Public Function CompareTo(ByVal other As IFrame) As Integer Implements System.IComparable(Of IFrame).CompareTo

            Return m_ticks.CompareTo(other.Ticks)

        End Function

        ''' <summary>This implementation of a basic frame compares itself by timestamp</summary>
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            Dim other As IFrame = TryCast(obj, IFrame)
            If other IsNot Nothing Then Return CompareTo(other)
            Throw New ArgumentException("Frame can only be compared with other IFrames...")

        End Function

#Region " Frame Operators "

        Public Shared Operator =(ByVal frame1 As Frame, ByVal frame2 As Frame) As Boolean

            Return frame1.Equals(frame2)

        End Operator

        Public Shared Operator <>(ByVal frame1 As Frame, ByVal frame2 As Frame) As Boolean

            Return Not frame1.Equals(frame2)

        End Operator

        Public Shared Operator >(ByVal frame1 As Frame, ByVal frame2 As Frame) As Boolean

            Return frame1.CompareTo(frame2) > 0

        End Operator

        Public Shared Operator >=(ByVal frame1 As Frame, ByVal frame2 As Frame) As Boolean

            Return frame1.CompareTo(frame2) >= 0

        End Operator

        Public Shared Operator <(ByVal frame1 As Frame, ByVal frame2 As Frame) As Boolean

            Return frame1.CompareTo(frame2) < 0

        End Operator

        Public Shared Operator <=(ByVal frame1 As Frame, ByVal frame2 As Frame) As Boolean

            Return frame1.CompareTo(frame2) <= 0

        End Operator

#End Region

    End Class

End Namespace