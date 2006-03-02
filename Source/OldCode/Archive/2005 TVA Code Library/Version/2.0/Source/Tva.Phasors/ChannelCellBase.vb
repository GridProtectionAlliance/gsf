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
Public MustInherit Class ChannelCellBase(Of T As IChannelCell)

    Inherits ChannelBase
    Implements IChannelCell

    Private m_parent As IChannelFrame(Of T)
    Private m_alignOnDWordBoundry As Boolean

    Protected Sub New(ByVal parent As IChannelFrame(Of T), ByVal alignOnDWordBoundry As Boolean)

        MyBase.New()

        m_parent = parent
        m_alignOnDWordBoundry = alignOnDWordBoundry

    End Sub

    ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame(Of T), ByVal state As IChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

    ' Derived classes are expected to expose a Protected Sub New(ByVal channelCell As IChannelCell)
    Protected Sub New(ByVal channelCell As T)

        MyClass.New(channelCell.Parent, channelCell.AlignOnDWordBoundry)

    End Sub

    Public ReadOnly Property Parent() As IChannelFrame(Of T) Implements IChannelCell.Parent
        Get
            Return m_parent
        End Get
    End Property

    Public ReadOnly Property AlignOnDWordBoundry() As Boolean Implements IChannelCell.AlignOnDWordBoundry
        Get
            Return m_alignOnDWordBoundry
        End Get
    End Property

    Public Overrides ReadOnly Property BinaryLength() As Int16
        Get
            Dim length As Int16 = MyBase.BinaryLength

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

