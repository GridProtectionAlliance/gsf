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

Imports Tva.Measurements

''' <summary>This class represents the protocol independent representation of any kind of data value as an abstract measurement.</summary>
<CLSCompliant(False)> _
Friend Class ChannelValueMeasurement(Of T As IChannelDefinition)

    Implements IMeasurement

    Private m_parent As IChannelValue(Of T)
    Private m_id As Integer
    Private m_tag As String
    Private m_source As String
    Private m_valueIndex As Integer
    Private m_adder As Double
    Private m_multiplier As Double

    Protected Sub New()
    End Sub

    Public Sub New(ByVal parent As IChannelValue(Of T), ByVal valueIndex As Integer)

        m_parent = parent
        m_valueIndex = valueIndex
        m_id = -1

    End Sub

    Public ReadOnly Property This() As IMeasurement Implements IMeasurement.This
        Get
            Return Me
        End Get
    End Property

    Public Property ID() As Integer Implements IMeasurement.ID
        Get
            Return m_id
        End Get
        Set(ByVal value As Integer)
            m_id = value
        End Set
    End Property

    Public Property Source() As String Implements IMeasurement.Source
        Get
            Return m_source
        End Get
        Set(ByVal value As String)
            m_source = value
        End Set
    End Property

    Public ReadOnly Property Key() As MeasurementKey Implements IMeasurement.Key
        Get
            Return New MeasurementKey(m_id, m_source)
        End Get
    End Property

    Public Property Tag() As String Implements IMeasurement.Tag
        Get
            Return m_tag
        End Get
        Set(ByVal value As String)
            m_tag = value
        End Set
    End Property

    Public Property ValueIndex() As Integer
        Get
            Return m_valueIndex
        End Get
        Set(ByVal value As Integer)
            m_valueIndex = value
        End Set
    End Property

    Public Property Value() As Double Implements IMeasurement.Value
        Get
            Return m_parent(m_valueIndex)
        End Get
        Set(ByVal value As Double)
            m_parent(m_valueIndex) = value
        End Set
    End Property

    Public ReadOnly Property AdjustedValue() As Double Implements IMeasurement.AdjustedValue
        Get
            Return m_parent(m_valueIndex) * m_multiplier + m_adder
        End Get
    End Property

    Public Property Adder() As Double Implements IMeasurement.Adder
        Get
            Return m_adder
        End Get
        Set(ByVal value As Double)
            m_adder = value
        End Set
    End Property

    Public Property Multiplier() As Double Implements IMeasurement.Multiplier
        Get
            Return m_multiplier
        End Get
        Set(ByVal value As Double)
            m_multiplier = value
        End Set
    End Property

    Public Property TimestampQualityIsGood() As Boolean Implements IMeasurement.TimestampQualityIsGood
        Get
            Return m_parent.Parent.SynchronizationIsValid
        End Get
        Private Set(ByVal value As Boolean)
            Throw New NotImplementedException("Timestamp quality for " & m_parent.InheritedType.Name & " is derived from parent data cell and is hence read-only for channel value measurements")
        End Set
    End Property

    Public Property ValueQualityIsGood() As Boolean Implements IMeasurement.ValueQualityIsGood
        Get
            Return m_parent.Parent.DataIsValid
        End Get
        Private Set(ByVal value As Boolean)
            Throw New NotImplementedException("Value quality for " & m_parent.InheritedType.Name & " is derived from parent data cell and is hence read-only for channel value measurements")
        End Set
    End Property

    Public Property Ticks() As Long Implements IMeasurement.Ticks
        Get
            Return m_parent.Parent.Parent.Ticks
        End Get
        Private Set(ByVal value As Long)
            Throw New NotImplementedException("Ticks for " & m_parent.InheritedType.Name & " are derived from parent frame and are hence read-only for channel value measurements")
        End Set
    End Property

    Public ReadOnly Property Timestamp() As Date Implements IMeasurement.Timestamp
        Get
            Return New Date(Ticks)
        End Get
    End Property

    ''' <summary>This implementation of a basic measurement compares itself by value</summary>
    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        If TypeOf obj Is IMeasurement Then
            Return Value.CompareTo(DirectCast(obj, IMeasurement).Value)
        Else
            Throw New ArgumentException(m_parent.InheritedType.Name & " measurement can only be compared with other IMeasurements...")
        End If

    End Function

End Class
