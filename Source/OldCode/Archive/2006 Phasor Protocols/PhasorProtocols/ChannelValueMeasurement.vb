'*******************************************************************************************************
'  ChannelValueMeasurement.vb - Channel data value measurement class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  3/7/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports PhasorProtocols.Common
Imports TVA.Common
Imports TVA.Measurements

''' <summary>This class represents the protocol independent representation of any kind of data value as an abstract measurement.</summary>
<CLSCompliant(False)> _
Friend Class ChannelValueMeasurement(Of T As IChannelDefinition)

    Implements IMeasurement

    Private m_parent As IChannelValue(Of T)
    Private m_id As Integer
    Private m_source As String
    Private m_key As MeasurementKey
    Private m_tag As String
    Private m_ticks As Long
    Private m_valueIndex As Integer
    Private m_adder As Double
    Private m_multiplier As Double
    Private m_dataQualityIsGood As Integer
    Private m_timeQualityIsGood As Integer

    Protected Sub New()
    End Sub

    Public Sub New(ByVal parent As IChannelValue(Of T), ByVal valueIndex As Integer)

        m_parent = parent
        m_valueIndex = valueIndex
        m_id = -1
        m_source = "__"
        m_key = UndefinedKey
        m_ticks = parent.Parent.Parent.Ticks
        m_adder = 0.0R
        m_multiplier = 1.0R
        m_dataQualityIsGood = -1
        m_timeQualityIsGood = -1

    End Sub

    Protected Property Parent() As IChannelValue(Of T)
        Get
            Return m_parent
        End Get
        Set(ByVal value As IChannelValue(Of T))
            m_parent = value
        End Set
    End Property

    Public ReadOnly Property This() As IMeasurement Implements IMeasurement.This
        Get
            Return Me
        End Get
    End Property

    Public Overridable Property ID() As Integer Implements IMeasurement.ID
        Get
            Return m_id
        End Get
        Set(ByVal value As Integer)
            m_id = value
        End Set
    End Property

    Public Overridable Property Source() As String Implements IMeasurement.Source
        Get
            Return m_source
        End Get
        Set(ByVal value As String)
            m_source = value
        End Set
    End Property

    Public Overridable ReadOnly Property Key() As MeasurementKey Implements IMeasurement.Key
        Get
            If m_key.Equals(UndefinedKey) Then m_key = New MeasurementKey(m_id, m_source)
            Return m_key
        End Get
    End Property

    Public Overridable Property Tag() As String Implements IMeasurement.Tag
        Get
            Return m_tag
        End Get
        Set(ByVal value As String)
            m_tag = value
        End Set
    End Property

    Public Overridable Property ValueIndex() As Integer
        Get
            Return m_valueIndex
        End Get
        Set(ByVal value As Integer)
            m_valueIndex = value
        End Set
    End Property

    Public Overridable Property Value() As Double Implements IMeasurement.Value
        Get
            Return Convert.ToDouble(m_parent(m_valueIndex))
        End Get
        Set(ByVal value As Double)
            m_parent(m_valueIndex) = Convert.ToSingle(value)
        End Set
    End Property

    Public Overridable ReadOnly Property AdjustedValue() As Double Implements IMeasurement.AdjustedValue
        Get
            Return Convert.ToDouble(m_parent(m_valueIndex)) * m_multiplier + m_adder
        End Get
    End Property

    ''' <summary>Defines an offset to add to the measurement value</summary>
    Public Overridable Property Adder() As Double Implements IMeasurement.Adder
        Get
            Return m_adder
        End Get
        Set(ByVal value As Double)
            m_adder = value
        End Set
    End Property

    ''' <summary>Defines a mulplicative offset to add to the measurement value</summary>
    Public Overridable Property Multiplier() As Double Implements IMeasurement.Multiplier
        Get
            Return m_multiplier
        End Get
        Set(ByVal value As Double)
            m_multiplier = value
        End Set
    End Property

    ''' <summary>Determines if the quality of the timestamp of this measurement is good</summary>
    ''' <remarks>This value returns timestamp quality of parent data cell unless assigned an alternate value</remarks>
    Public Overridable Property TimestampQualityIsGood() As Boolean Implements IMeasurement.TimestampQualityIsGood
        Get
            If m_timeQualityIsGood = -1 Then Return m_parent.Parent.SynchronizationIsValid
            Return (m_timeQualityIsGood <> 0)
        End Get
        Set(ByVal value As Boolean)
            m_timeQualityIsGood = IIf(value, 1, 0)
        End Set
    End Property

    ''' <summary>Determines if the quality of the numeric value of this measurement is good</summary>
    ''' <remarks>This value returns data quality of parent data cell unless assigned an alternate value</remarks>
    Public Overridable Property ValueQualityIsGood() As Boolean Implements IMeasurement.ValueQualityIsGood
        Get
            If m_dataQualityIsGood = -1 Then Return m_parent.Parent.DataIsValid
            Return (m_dataQualityIsGood <> 0)
        End Get
        Set(ByVal value As Boolean)
            m_dataQualityIsGood = IIf(value, 1, 0)
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

    Public Overridable ReadOnly Property Timestamp() As Date Implements IMeasurement.Timestamp
        Get
            Return New Date(Ticks)
        End Get
    End Property

    ''' <summary>This implementation of a basic measurement compares itself by value</summary>
    Public Overridable Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is IMeasurement Then Return CompareTo(DirectCast(obj, IMeasurement))
        Throw New ArgumentException(m_parent.DerivedType.Name & " measurement can only be compared with other IMeasurements...")

    End Function

    ''' <summary>This implementation of a basic measurement compares itself by value</summary>
    Public Overridable Function CompareTo(ByVal other As Measurements.IMeasurement) As Integer Implements System.IComparable(Of Measurements.IMeasurement).CompareTo

        Return Value.CompareTo(other.Value)

    End Function

    ''' <summary>Returns True if the value of this measurement equals the value of the specified other measurement</summary>
    Public Overridable Overloads Function Equals(ByVal other As Measurements.IMeasurement) As Boolean Implements System.IEquatable(Of Measurements.IMeasurement).Equals

        Return (CompareTo(other) = 0)

    End Function

End Class