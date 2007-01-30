'*******************************************************************************************************
'  Tva.Measurements.Measurement.vb - Basic measurement implementation
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
'  12/8/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    ''' <summary>Implementation of a basic measured value</summary>
    Public Class Measurement

        Implements IMeasurement

        Private m_id As Integer
        Private m_source As String
        Private m_tag As String
        Private m_value As Double
        Private m_adder As Double
        Private m_multiplier As Double
        Private m_ticks As Long
        Private m_valueQualityIsGood As Boolean
        Private m_timestampQualityIsGood As Boolean

        Public Sub New()

            MyClass.New(-1, Nothing, Double.NaN, 0)

        End Sub

        Public Sub New(ByVal id As Integer, ByVal source As String)

            MyClass.New(id, source, Double.NaN, 0.0R, 1.0R, 0)

        End Sub


        Public Sub New(ByVal id As Integer, ByVal source As String, ByVal value As Double, ByVal timestamp As Date)

            MyClass.New(id, source, value, timestamp.Ticks)

        End Sub

        Public Sub New(ByVal id As Integer, ByVal source As String, ByVal value As Double, ByVal ticks As Long)

            MyClass.New(id, source, value, 0.0R, 1.0R, ticks)

        End Sub

        Public Sub New(ByVal id As Integer, ByVal source As String, ByVal tag As String, ByVal adder As Double, ByVal multiplier As Double)

            MyClass.New(id, source, Double.NaN, adder, multiplier, 0)
            m_tag = tag

        End Sub

        Public Sub New(ByVal id As Integer, ByVal source As String, ByVal value As Double, ByVal adder As Double, ByVal multiplier As Double, ByVal ticks As Long)

            m_id = id
            m_source = source
            m_value = value
            m_adder = adder
            m_multiplier = multiplier
            m_ticks = ticks
            m_valueQualityIsGood = True
            m_timestampQualityIsGood = True

        End Sub

        ''' <summary>Handy instance reference to self</summary>
        Public Overridable ReadOnly Property This() As IMeasurement Implements IMeasurement.This
            Get
                Return Me
            End Get
        End Property

        ''' <summary>Creates a copy of the specified measurement</summary>
        Public Shared Function Clone(ByVal measurementToClone As IMeasurement) As Measurement

            With measurementToClone
                Return New Measurement(.ID, .Source, .Value, .Adder, .Multiplier, .Ticks)
            End With

        End Function

        ''' <summary>Creates a copy of the specified measurement using a new timestamp</summary>
        Public Shared Function Clone(ByVal measurementToClone As IMeasurement, ByVal ticks As Long) As Measurement

            With measurementToClone
                Return New Measurement(.ID, .Source, .Value, .Adder, .Multiplier, ticks)
            End With

        End Function

        ''' <summary>Creates a copy of the specified measurement using a new value and timestamp</summary>
        Public Shared Function Clone(ByVal measurementToClone As IMeasurement, ByVal value As Double, ByVal ticks As Long) As Measurement

            With measurementToClone
                Return New Measurement(.ID, .Source, value, .Adder, .Multiplier, ticks)
            End With

        End Function

        ''' <summary>Gets or sets the numeric ID of this measurement</summary>
        ''' <remarks>
        ''' <para>In most implementations, this will be a required field</para>
        ''' <para>Note that this field, in addition to Source, typically creates the primary key for a measurement</para>
        ''' </remarks>
        Public Overridable Property ID() As Integer Implements IMeasurement.ID
            Get
                Return m_id
            End Get
            Set(ByVal value As Integer)
                m_id = value
            End Set
        End Property

        ''' <summary>Gets or sets the source of this measurement</summary>
        ''' <remarks>
        ''' <para>In most implementations, this will be a required field</para>
        ''' <para>Note that this field, in addition to ID, typically creates the primary key for a measurement</para>
        ''' <para>This value is typically used to track the archive name in which measurement is stored</para>
        ''' </remarks>
        Public Overridable Property Source() As String Implements IMeasurement.Source
            Get
                Return m_source
            End Get
            Set(ByVal value As String)
                m_source = value
            End Set
        End Property

        ''' <summary>Returns the primary key of this measurement</summary>
        Public ReadOnly Property Key() As MeasurementKey Implements IMeasurement.Key
            Get
                Return New MeasurementKey(m_id, m_source)
            End Get
        End Property

        ''' <summary>Gets or sets the text based ID of this measurement</summary>
        Public Overridable Property Tag() As String Implements IMeasurement.Tag
            Get
                Return m_tag
            End Get
            Set(ByVal value As String)
                m_tag = value
            End Set
        End Property

        ''' <summary>Gets or sets the raw measurement value that is not offset by adder and multiplier</summary>
        ''' <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier)</returns>
        Public Property Value() As Double Implements IMeasurement.Value
            Get
                Return m_value
            End Get
            Set(ByVal value As Double)
                m_value = value
            End Set
        End Property

        ''' <summary>Returns the adjusted numeric value of this measurement</summary>
        ''' <remarks>Note that returned value will be offset by adder and multiplier</remarks>
        ''' <returns>Value offset by adder and multipler (i.e., Value * Multiplier + Adder)</returns>
        Public Overridable ReadOnly Property AdjustedValue() As Double Implements IMeasurement.AdjustedValue
            Get
                Return m_value * m_multiplier + m_adder
            End Get
        End Property

        ''' <summary>Defines an offset to add to the measurement value - defaults to zero</summary>
        Public Property Adder() As Double Implements IMeasurement.Adder
            Get
                Return m_adder
            End Get
            Set(ByVal value As Double)
                m_adder = value
            End Set
        End Property

        ''' <summary>Defines a mulplicative offset to add to the measurement value - defaults to one</summary>
        Public Property Multiplier() As Double Implements IMeasurement.Multiplier
            Get
                Return m_multiplier
            End Get
            Set(ByVal value As Double)
                m_multiplier = value
            End Set
        End Property

        ''' <summary>Gets or sets exact timestamp of the data represented by this measurement</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Public Overridable Property Ticks() As Long Implements IMeasurement.Ticks
            Get
                Return m_ticks
            End Get
            Set(ByVal value As Long)
                m_ticks = value
            End Set
        End Property

        ''' <summary>Date representation of ticks of this measurement</summary>
        Public Overridable ReadOnly Property Timestamp() As Date Implements IMeasurement.Timestamp
            Get
                Return New Date(m_ticks)
            End Get
        End Property

        ''' <summary>Determines if the quality of the numeric value of this measurement is good</summary>
        Public Overridable Property ValueQualityIsGood() As Boolean Implements IMeasurement.ValueQualityIsGood
            Get
                Return m_valueQualityIsGood
            End Get
            Set(ByVal value As Boolean)
                m_valueQualityIsGood = value
            End Set
        End Property

        ''' <summary>Determines if the quality of the timestamp of this measurement is good</summary>
        Public Overridable Property TimestampQualityIsGood() As Boolean Implements IMeasurement.TimestampQualityIsGood
            Get
                Return m_timestampQualityIsGood
            End Get
            Set(ByVal value As Boolean)
                m_timestampQualityIsGood = value
            End Set
        End Property

        Public Overrides Function ToString() As String

            Return Key.ToString()

        End Function

        ''' <summary>This implementation of a basic measurement compares itself by value</summary>
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is IMeasurement Then Return CompareTo(DirectCast(obj, IMeasurement))
            Throw New ArgumentException("Measurement can only be compared with other IMeasurements...")

        End Function

        ''' <summary>This implementation of a basic measurement compares itself by value</summary>
        Public Function CompareTo(ByVal other As IMeasurement) As Integer Implements System.IComparable(Of IMeasurement).CompareTo

            Return m_value.CompareTo(other.Value)

        End Function

        ''' <summary>Returns True if the value of this measurement equals the value of the specified other measurement</summary>
        Public Overloads Function Equals(ByVal other As IMeasurement) As Boolean Implements System.IEquatable(Of IMeasurement).Equals

            Return (CompareTo(other) = 0)

        End Function

    End Class

End Namespace
