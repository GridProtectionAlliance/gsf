'***********************************************************************
'  ChannelValueBase.vb - Channel data base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
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

Imports System.Text

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of any kind of data.
    Public MustInherit Class ChannelValueBase

        Implements IChannelValue

        Protected m_dataFormat As DataFormat

        Protected Sub New()

            m_dataFormat = DataFormat.FloatingPoint

        End Sub

        Protected Sub New(ByVal dataFormat As DataFormat)

            m_dataFormat = dataFormat

        End Sub

        Public MustOverride ReadOnly Property InheritedType() As System.Type Implements IChannelValue.InheritedType

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

        Public MustOverride ReadOnly Property BinaryLength() As Integer Implements IChannelValue.BinaryLength

        Public MustOverride ReadOnly Property BinaryImage() As Byte() Implements IChannelValue.BinaryImage

    End Class

End Namespace
