'*******************************************************************************************************
'  ChannelCellBase.vb - Channel frame cell base class
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent representation of any kind of data cell.
    Public MustInherit Class ChannelCellBase

        Inherits ChannelBase
        Implements IChannelCell

        Private m_parent As IChannelFrame
        Private m_alignOnDWordBoundry As Boolean

        Protected Sub New(ByVal parent As IChannelFrame, ByVal alignOnDWordBoundry As Boolean)

            MyBase.New()

            m_parent = parent
            m_alignOnDWordBoundry = alignOnDWordBoundry

        End Sub

        Protected Sub New(ByVal parent As IChannelFrame, ByVal alignOnDWordBoundry As Boolean, ByVal state As Object, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            Me.New(parent, alignOnDWordBoundry)

            ParseHeader(state, binaryImage, startIndex)
            startIndex += HeaderLength
            ParseBody(state, binaryImage, startIndex)

        End Sub

        ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As ChannelFrameParsingState, ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        ' Derived classes are expected to expose a Protected Sub New(ByVal channelCell As IChannelCell)
        Protected Sub New(ByVal channelCell As IChannelCell)

            Me.New(channelCell.Parent, channelCell.AlignOnDWordBoundry)

        End Sub

        Protected MustOverride Sub ParseHeader(ByVal state As Object, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        Protected MustOverride Sub ParseBody(ByVal state As Object, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        Public ReadOnly Property Parent() As IChannelFrame Implements IChannelCell.Parent
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

End Namespace
