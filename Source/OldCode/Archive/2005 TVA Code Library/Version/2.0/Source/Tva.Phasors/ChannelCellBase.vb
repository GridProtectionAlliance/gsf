'*******************************************************************************************************
'  ChannelCellBase.vb - Channel frame cell base class
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

' This class represents the common implementation of the protocol independent representation of any kind of data cell.
<CLSCompliant(False)> _
Public MustInherit Class ChannelCellBase

    Inherits ChannelBase
    Implements IChannelCell

    Private m_parent As IChannelFrame
    Private m_idCode As UInt16
    Private m_alignOnDWordBoundry As Boolean

    Protected Sub New(ByVal parent As IChannelFrame, ByVal alignOnDWordBoundry As Boolean)

        m_parent = parent
        m_alignOnDWordBoundry = alignOnDWordBoundry

    End Sub

    Protected Sub New(ByVal parent As IChannelFrame, ByVal alignOnDWordBoundry As Boolean, ByVal idCode As UInt16)

        MyClass.New(parent, alignOnDWordBoundry)
        m_idCode = idCode

    End Sub

    ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

    ' Derived classes are expected to expose a Protected Sub New(ByVal channelCell As IChannelCell)
    Protected Sub New(ByVal channelCell As IChannelCell)

        MyClass.New(channelCell.Parent, channelCell.AlignOnDWordBoundry, channelCell.IDCode)

    End Sub

    Public Overridable ReadOnly Property Parent() As IChannelFrame Implements IChannelCell.Parent
        Get
            Return m_parent
        End Get
    End Property

    Public Overridable Property IDCode() As UInt16 Implements IChannelCell.IDCode
        Get
            Return m_idCode
        End Get
        Set(ByVal value As UInt16)
            m_idCode = value
        End Set
    End Property

    Public Overridable ReadOnly Property AlignOnDWordBoundry() As Boolean Implements IChannelCell.AlignOnDWordBoundry
        Get
            Return m_alignOnDWordBoundry
        End Get
    End Property

    Public Overrides ReadOnly Property BinaryLength() As UInt16
        Get
            Dim length As UInt16 = MyBase.BinaryLength

            If m_alignOnDWordBoundry Then
                ' If requested, we align frame cells on 32-bit word boundries
                Do Until length Mod 4 = 0
                    length += 1
                Loop
            End If

            Return length

        End Get
    End Property

End Class

