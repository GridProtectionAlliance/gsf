'*******************************************************************************************************
'  ChannelFrameParsingStateBase.vb - Channel data frame parsing state base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation the parsing state used by any frame of data that can be sent or received from a PMU.
    Public MustInherit Class ChannelFrameParsingStateBase

        Inherits ChannelParsingStateBase
        Implements IChannelFrameParsingState

        Private m_cells As IChannelCellCollection
        Private m_cellCount As Integer
        Private m_cellType As Type

        Public Sub New(ByVal cells As IChannelCellCollection, ByVal cellCount As Integer, ByVal cellType As Type)

            m_cells = cells
            m_cellCount = cellCount
            m_cellType = cellType

        End Sub

        Public ReadOnly Property Cells() As IChannelCellCollection Implements IChannelFrameParsingState.Cells
            Get
                Return m_cells
            End Get
        End Property

        Public ReadOnly Property CellCount() As Integer Implements IChannelFrameParsingState.CellCount
            Get
                Return m_cellCount
            End Get
        End Property

        Public ReadOnly Property CellType() As Type Implements IChannelFrameParsingState.CellType
            Get
                Return m_cellType
            End Get
        End Property

    End Class

End Namespace