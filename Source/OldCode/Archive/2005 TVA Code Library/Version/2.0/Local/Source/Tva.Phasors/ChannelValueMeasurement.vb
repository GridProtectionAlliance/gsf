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

' This class represents the protocol independent representation of any kind of data value as an abstract measurement.
<CLSCompliant(False)> _
Friend Class ChannelValueMeasurement(Of T As IChannelDefinition)

    Implements IMeasurement

    Private m_parent As IChannelValue(Of T)
    Private m_id As Integer
    Private m_valueIndex As Integer

    Public Sub New(ByVal parent As IChannelValue(Of T), ByVal valueIndex As Integer)

        m_parent = parent
        m_valueIndex = valueIndex
        m_id = -1

    End Sub

    Public ReadOnly Property This() As Measurements.IMeasurement Implements Measurements.IMeasurement.This
        Get
            Return Me
        End Get
    End Property

    Public Property ID() As Integer Implements Measurements.IMeasurement.ID
        Get
            Return m_id
        End Get
        Set(ByVal value As Integer)
            m_id = value
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

    Public Property Value() As Double Implements Measurements.IMeasurement.Value
        Get
            Return m_parent.Values(m_valueIndex)
        End Get
        Set(ByVal value As Double)
            m_parent.Values(m_valueIndex) = value
        End Set
    End Property

    Public Property TimestampQualityIsGood() As Boolean Implements Measurements.IMeasurement.TimestampQualityIsGood
        Get
            Return m_parent.Parent.SynchronizationIsValid
        End Get
        Private Set(ByVal value As Boolean)
            Throw New NotImplementedException("Timestamp quality for " & m_parent.InheritedType.Name & " is derived from parent data cell and is hence read-only for channel value measurements")
        End Set
    End Property

    Public Property ValueQualityIsGood() As Boolean Implements Measurements.IMeasurement.ValueQualityIsGood
        Get
            Return m_parent.Parent.DataIsValid
        End Get
        Set(ByVal value As Boolean)
            Throw New NotImplementedException("Value quality for " & m_parent.InheritedType.Name & " is derived from parent data cell and is hence read-only for channel value measurements")
        End Set
    End Property

    Public Property Ticks() As Long Implements Measurements.IMeasurement.Ticks
        Get
            Return m_parent.Parent.Parent.Ticks
        End Get
        Private Set(ByVal value As Long)
            Throw New NotImplementedException("Ticks for " & m_parent.InheritedType.Name & " are derived from parent frame and are hence read-only for channel value measurements")
        End Set
    End Property

    Public ReadOnly Property Timestamp() As Date Implements Measurements.IMeasurement.Timestamp
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
