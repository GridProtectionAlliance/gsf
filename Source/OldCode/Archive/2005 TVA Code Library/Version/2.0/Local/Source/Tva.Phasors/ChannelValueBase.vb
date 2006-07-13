'*******************************************************************************************************
'  ChannelValueBase.vb - Channel data value base class
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

Imports System.Runtime.Serialization
Imports Tva.Measurements

''' <summary>This class represents the common implementation of the protocol independent representation of any kind of data value.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class ChannelValueBase(Of T As IChannelDefinition)

    Inherits ChannelBase
    Implements IChannelValue(Of T)

    Private m_parent As IDataCell
    Private m_definition As T
    Private m_measurements As IMeasurement()

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        ' Deserialize channel value
        m_parent = info.GetValue("parent", GetType(IDataCell))
        m_definition = info.GetValue("definition", GetType(T))

    End Sub

    Protected Sub New(ByVal parent As IDataCell)

        m_parent = parent

    End Sub

    Protected Sub New(ByVal parent As IDataCell, ByVal channelDefinition As T)

        m_parent = parent
        m_definition = channelDefinition

    End Sub

    ' Derived classes are expected to expose a Protected Sub New(ByVal channelValue As IChannelValue(Of T))
    Protected Sub New(ByVal channelValue As IChannelValue(Of T))

        MyClass.New(channelValue.Parent, channelValue.Definition)

    End Sub

    Public Overridable ReadOnly Property Parent() As IDataCell Implements IChannelValue(Of T).Parent
        Get
            Return m_parent
        End Get
    End Property

    Public Overridable Property Definition() As T Implements IChannelValue(Of T).Definition
        Get
            Return m_definition
        End Get
        Set(ByVal value As T)
            m_definition = value
        End Set
    End Property

    Public Overridable ReadOnly Property DataFormat() As DataFormat Implements IChannelValue(Of T).DataFormat
        Get
            Return m_definition.DataFormat
        End Get
    End Property

    Public MustOverride ReadOnly Property IsEmpty() As Boolean Implements IChannelValue(Of T).IsEmpty

    Default Public MustOverride Property CompositeValue(ByVal index As Integer) As Single Implements IChannelValue(Of T).CompositeValue

    Public MustOverride ReadOnly Property CompositeValueCount() As Integer Implements IChannelValue(Of T).CompositeValueCount

    Public Overridable ReadOnly Property Measurements() As IMeasurement() Implements IChannelValue(Of T).Measurements
        Get
            ' Create a measurement instance for each composite value the derived channel value exposes
            If m_measurements Is Nothing Then
                m_measurements = CreateArray(Of IMeasurement)(CompositeValueCount)

                For x As Integer = 0 To m_measurements.Length - 1
                    m_measurements(x) = New ChannelValueMeasurement(Of T)(Me, x)
                Next
            End If

            Return m_measurements
        End Get
    End Property

    Public Overridable Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData

        ' Serialize channel value
        info.AddValue("parent", m_parent, GetType(IDataCell))
        info.AddValue("definition", m_definition, GetType(T))

    End Sub

End Class

