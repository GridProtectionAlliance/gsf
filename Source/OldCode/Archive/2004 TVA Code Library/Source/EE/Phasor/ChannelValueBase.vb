'***********************************************************************
'  ChannelValueBase.vb - Channel data base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  3/7/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of any kind of data.
    Public MustInherit Class ChannelValueBase

        Implements IChannelValue

        Private m_dataFormat As DataFormat

        Protected Sub New()

            m_dataFormat = DataFormat.FixedInteger

        End Sub

        Protected Sub New(ByVal dataFormat As DataFormat)

            m_dataFormat = dataFormat

        End Sub

        ' Dervied classes are expected to expose a Protected Sub New(ByVal channelValue As IChannelValue)
        Protected Sub New(ByVal channelValue As IChannelValue)

            Me.New(channelValue.DataFormat)

        End Sub

        Public MustOverride ReadOnly Property InheritedType() As System.Type Implements IChannelValue.InheritedType

        Public Overridable ReadOnly Property This() As IChannel Implements IChannelValue.This
            Get
                Return Me
            End Get
        End Property

        Public Overridable Property DataFormat() As DataFormat Implements IChannelValue.DataFormat
            Get
                Return m_dataFormat
            End Get
            Set(ByVal Value As DataFormat)
                m_dataFormat = Value
            End Set
        End Property

        Public MustOverride ReadOnly Property IsEmpty() As Boolean Implements IChannelValue.IsEmpty

        Public MustOverride ReadOnly Property Values() As Double() Implements IChannelValue.Values

        Public MustOverride ReadOnly Property BinaryLength() As Int16 Implements IChannelValue.BinaryLength

        Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IChannelValue.BinaryImage

    End Class

End Namespace
