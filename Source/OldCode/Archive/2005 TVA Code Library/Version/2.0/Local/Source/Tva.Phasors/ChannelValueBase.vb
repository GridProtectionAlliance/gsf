'*******************************************************************************************************
'  ChannelValueBase.vb - Channel data value base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  3/7/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Measurements

' This class represents the common implementation of the protocol independent representation of any kind of data value.
<CLSCompliant(False)> _
Public MustInherit Class ChannelValueBase(Of T As IChannelDefinition)

    Inherits ChannelBase
    Implements IChannelValue(Of T)

    Private m_parent As IDataCell
    Private m_definition As T

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

    Public MustOverride ReadOnly Property Values() As Single() Implements IChannelValue(Of T).Values

    Public Overridable Function GetMeasurement(ByVal valueIndex As Integer, ByVal assignedID As Integer) As Measurements.IMeasurement Implements IChannelValue(Of T).GetMeasurement

        Return New ChannelValueMeasurement(Of T)(Me, valueIndex, assignedID)

    End Function

End Class

