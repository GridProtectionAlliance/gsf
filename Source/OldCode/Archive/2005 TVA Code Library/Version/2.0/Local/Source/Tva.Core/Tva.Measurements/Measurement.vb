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
        Private m_value As Double
        Private m_adder As Double
        Private m_multiplier As Double
        Private m_ticks As Long
        Private m_valueQualityIsGood As Boolean
        Private m_timestampQualityIsGood As Boolean

        Public Sub New()

            MyClass.New(-1, Double.NaN, 0)

        End Sub

        Public Sub New(ByVal id As Integer, ByVal value As Double, ByVal timestamp As Date)

            MyClass.New(id, value, timestamp.Ticks)

        End Sub

        Public Sub New(ByVal id As Integer, ByVal value As Double, ByVal ticks As Long)

            MyClass.New(id, value, 0.0R, 1.0R, ticks)

        End Sub

        Public Sub New(ByVal id As Integer, ByVal value As Double, ByVal adder As Double, ByVal multiplier As Double, ByVal ticks As Long)

            m_id = id
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

        ''' <summary>Gets or sets index or ID of this measurement</summary>
        Public Overridable Property ID() As Integer Implements IMeasurement.ID
            Get
                Return m_id
            End Get
            Set(ByVal value As Integer)
                m_id = value
            End Set
        End Property

        ''' <summary>Gets or sets numeric value of this measurement</summary>
        ''' <returns>Returns value offset by adder and multipler (i.e., RawValue * Multiplier + Adder)</returns>
        Public Overridable Property Value() As Double Implements IMeasurement.Value
            Get
                Return m_value * m_multiplier + m_adder
            End Get
            Set(ByVal value As Double)
                m_value = value
            End Set
        End Property

        ''' <summary>Raw measurement value that is not offset by adder and multiplier</summary>
        ''' <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier)</returns>
        Public ReadOnly Property RawValue() As Double Implements IMeasurement.RawValue
            Get
                Return m_value
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

        ''' <summary>This implementation of a basic measurement compares itself by value</summary>
        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            If TypeOf obj Is IMeasurement Then
                Return m_value.CompareTo(DirectCast(obj, IMeasurement).Value)
            Else
                Throw New ArgumentException("Measurement can only be compared with other IMeasurements...")
            End If

        End Function

    End Class

End Namespace
